using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Removes retired controls from existing approved assets; creates no new screen.</summary>
    public static class RedesignAdRetirement
    {
        [MenuItem("Rocket Raja/Redesign/Retire Existing Ad Controls")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play Mode before editing saved screens.");
            foreach (string path in new[] { "Assets/Scenes/Main.unity", "Assets/Scenes/HTML Importer.unity" })
            {
                var scene = SceneManager.GetSceneByPath(path);
                if (scene.IsValid() && scene.isDirty) throw new InvalidOperationException("Save or review existing scene changes first: " + path);
            }
            foreach (string name in new[] { "Home", "Offline", "Settings", "Privacy", "Prestige" })
            {
                string path = "Assets/Game/ImportedUI/" + name + " Screen.prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try { Apply(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            var active = SceneManager.GetActiveScene();
            foreach (string path in new[] { "Assets/Scenes/Main.unity", "Assets/Scenes/HTML Importer.unity" })
            {
                var scene = SceneManager.GetSceneByPath(path);
                bool opened = !scene.IsValid() || !scene.isLoaded;
                if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        if (root.GetComponent<ImportedSponsorScreen>() != null)
                        { UnityEngine.Object.DestroyImmediate(root); continue; }
                        Apply(root);
                        foreach (var owner in root.GetComponentsInChildren<GameController>(true))
                        {
                            var data = new SerializedObject(owner);
                            data.FindProperty("sponsorScreen").objectReferenceValue = null;
                            data.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + path);
                }
                finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
            }
            if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
            DeleteOwnedAsset("Assets/Game/ImportedUI/Sponsor Screen.prefab");
            DeleteOwnedAsset("Assets/GoogleMobileAds");
            Debug.Log("REDESIGN_AD_CONTROLS_RETIRED");
        }

        static void DeleteOwnedAsset(string path)
        {
            string root = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
            if (!Path.GetFullPath(path).StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Unexpected asset cleanup path.");
            if ((File.Exists(path) || Directory.Exists(path)) && !AssetDatabase.DeleteAsset(path)) throw new IOException("Could not remove retired asset " + path);
        }

        static Transform Find(GameObject root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == name);
        static void Remove(GameObject root, string name)
        {
            var found = Find(root, name);
            if (found != null) UnityEngine.Object.DestroyImmediate(found.gameObject);
        }
        static void Clear(UnityEngine.Object target, params string[] fields)
        {
            var data = new SerializedObject(target);
            foreach (string field in fields) data.FindProperty(field).objectReferenceValue = null;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        static void Apply(GameObject root)
        {
            foreach (var prestige in root.GetComponentsInChildren<ImportedPrestigeScreen>(true))
                foreach (var label in prestige.GetComponentsInChildren<TMP_Text>(true))
                    if (label.text.Contains("sponsor and offline offer."))
                    {
                        label.text = label.text.Replace("sponsor and offline offer.", "offline summary.");
                        EditorUtility.SetDirty(label);
                    }
            foreach (var privacy in root.GetComponentsInChildren<ImportedPrivacyScreen>(true))
            {
                Clear(privacy, "consentButton", "consentRow");
                Remove(privacy.gameObject, "Consent Required");
                Remove(privacy.gameObject, "Optional videos Card");
                foreach (var label in privacy.GetComponentsInChildren<TMP_Text>(true))
                    if (label.text.Contains("advertiser configuration"))
                    {
                        label.text = "This is a test build. Production privacy information must be supplied before release.";
                        EditorUtility.SetDirty(label);
                    }
            }
            foreach (var home in root.GetComponentsInChildren<ImportedHomeScreen>(true))
            {
                Clear(home, "sponsorButton", "sponsorLabel", "sponsorLayout");
                Remove(home.gameObject, "Sponsor");
            }
            foreach (var settings in root.GetComponentsInChildren<ImportedSettingsScreen>(true))
            {
                Clear(settings, "consentButton", "consentRow");
                Remove(settings.gameObject, "settings-consent-row");
            }
            foreach (var offline in root.GetComponentsInChildren<ImportedOfflineScreen>(true))
            {
                Remove(offline.gameObject, "offline-video");
                Remove(offline.gameObject, "Offer Row");
                Remove(offline.gameObject, "offline-message");
                var label = Find(offline.gameObject, "Continue Label")?.GetComponent<TMP_Text>();
                if (label != null) { label.text = "Continue"; EditorUtility.SetDirty(label); }
                var button = Find(offline.gameObject, "back-Offline") as RectTransform;
                if (button != null) button.offsetMax = new Vector2(button.offsetMax.x, -27);
                var footer = Find(offline.gameObject, "Footer") as RectTransform;
                if (footer != null) footer.sizeDelta = new Vector2(footer.sizeDelta.x, 180);
                var body = Find(offline.gameObject, "Scroll Body") as RectTransform;
                if (body != null) body.offsetMin = new Vector2(body.offsetMin.x, 180);
                var fit = offline.GetComponentInChildren<ImportedDialogFit>(true);
                if (fit != null) fit.SetPreferredHeight(1270);
            }
        }
    }
}
