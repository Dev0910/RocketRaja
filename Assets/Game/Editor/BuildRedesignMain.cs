using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        static T Reference<T>(Component source, string field) where T : UnityEngine.Object =>
            (T)new SerializedObject(source).FindProperty(field).objectReferenceValue;
        static T[] References<T>(Component source, string field) where T : UnityEngine.Object
        {
            var array = new SerializedObject(source).FindProperty(field);
            return Enumerable.Range(0, array.arraySize).Select(i => (T)array.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
        }
        static GameObject CopyUtility(string name, GameObject parent)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/ImportedUI/" + name + " Screen.prefab");
            if (!source) throw new InvalidOperationException("Missing original " + name);
            var copy = (GameObject)PrefabUtility.InstantiatePrefab(source, parent.scene);
            PrefabUtility.UnpackPrefabInstance(copy, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            copy.name = "Retained " + name; copy.transform.SetParent(parent.transform, false);
            FitUtility(copy);
            foreach (var motion in copy.GetComponentsInChildren<UiMotion>(true)) UnityEngine.Object.DestroyImmediate(motion);
            foreach (var feedback in copy.GetComponentsInChildren<UiFeedback>(true)) UnityEngine.Object.DestroyImmediate(feedback);
            foreach (var group in copy.GetComponentsInChildren<CanvasGroup>(true)) { group.alpha = 1; group.interactable = group.blocksRaycasts = true; }
            foreach (var button in copy.GetComponentsInChildren<Button>(true)) button.onClick = new Button.ButtonClickedEvent();
            foreach (var text in copy.GetComponentsInChildren<TMP_Text>(true)) text.richText = false;
            copy.SetActive(false); return copy;
        }
        static void FitUtility(GameObject copy)
        {
            // A nested former root Canvas can drive its RectTransform to zero while saving a prefab.
            // Utility siblings share the native root Canvas/raycaster and draw after the base screens.
            if (copy.TryGetComponent<CanvasScaler>(out var scaler)) UnityEngine.Object.DestroyImmediate(scaler);
            if (copy.TryGetComponent<GraphicRaycaster>(out var raycaster)) UnityEngine.Object.DestroyImmediate(raycaster);
            if (copy.TryGetComponent<Canvas>(out var canvas)) UnityEngine.Object.DestroyImmediate(canvas);
            var rect = (RectTransform)copy.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
            foreach (var button in copy.GetComponentsInChildren<Button>(true))
            {
                var hit = (RectTransform)button.transform;
                if (!Mathf.Approximately(hit.anchorMin.y, hit.anchorMax.y))
                {
                    float anchor = Mathf.Lerp(hit.anchorMin.y, hit.anchorMax.y, hit.pivot.y);
                    hit.anchorMin = new Vector2(hit.anchorMin.x, anchor); hit.anchorMax = new Vector2(hit.anchorMax.x, anchor);
                    hit.sizeDelta = new Vector2(hit.sizeDelta.x, 144);
                }
                else if (hit.rect.height > 0 && hit.rect.height < 144)
                    hit.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 144);
            }
        }
        public static void ReviseRecoveryTargets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) throw new InvalidOperationException("Compiled Edit Mode required");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var ui = root.GetComponent<RedesignRetainedUi>();
                // Preserve the existing HTML/source-prefab footer's two rows. The old
                // utility sizing pass had centered both stretched targets at y = 0.
                foreach (var button in new[] { ui.recoveryRetry, ui.recoveryExit })
                {
                    var rect = (RectTransform)button.transform;
                    rect.anchorMin = new Vector2(0, .5f); rect.anchorMax = new Vector2(1, .5f);
                    rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(-62, 144);
                    rect.anchoredPosition = new Vector2(0, button == ui.recoveryRetry ? 82.5f : -82f);
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        public static void ReviseUtilityGeometry()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var ui = root.GetComponent<RedesignRetainedUi>();
                foreach (var copy in new[] { ui.settings, ui.privacy, ui.naming, ui.recovery }) FitUtility(copy);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        static void ReplaceText(GameObject root, string from, string to)
        {
            var found = root.GetComponentsInChildren<TMP_Text>(true).Where(t => t.text == from).ToArray();
            if (found.Length != 1) throw new InvalidOperationException("Expected one retained label: " + from);
            found[0].text = to;
        }
        static void AssignAccountCopy(RedesignRetainedUi ui)
        {
            TMP_Text Find(GameObject owner, string text) => owner.GetComponentsInChildren<TMP_Text>(true).Single(t => t.text == text);
            ui.accountHeading = Find(ui.settings, "Development profile");
            ui.accountDescription = Find(ui.settings, "Local test save. No real payments.");
            ui.privacySummary = Find(ui.privacy, "Your test save stays on this device.");
            ui.privacyDescription = Find(ui.privacy, "This development game uses a separate local guest profile. Live Google saves and real payments are unavailable. Original progress stays separate. Analytics and crash collection are off.");
            ui.recoveryDescription = Find(ui.recovery, "Free device storage if needed, then retry. This test profile has no cloud recovery.");
        }
        public static void BindAccountCopy()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) throw new InvalidOperationException("Compiled Edit Mode required");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try { AssignAccountCopy(root.GetComponent<RedesignRetainedUi>()); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        public static void AddRetainedDialogs()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Compiled Edit Mode required");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (root.GetComponent<RedesignApplication>()) throw new InvalidOperationException("Utility dialogs already authored; use a scoped revision");
                var ui = root.AddComponent<RedesignRetainedUi>(); var app = root.AddComponent<RedesignApplication>();
                app.router = root.GetComponent<RedesignScreenRouter>(); app.retained = ui; app.runOnStart = false;
                ui.settingsEntry = root.transform.Find("Safe Area/Header/settings").GetComponent<Button>();
                ui.settings = CopyUtility("Settings", root); var settings = ui.settings.GetComponent<ImportedSettingsScreen>();
                ui.settingsClose = Reference<Button>(settings, "closeButton"); ui.settingsBack = Reference<Button>(settings, "backButton");
                ui.privacyEntry = Reference<Button>(settings, "privacyButton"); ui.account = Reference<Button>(settings, "accountButton"); ui.rename = Reference<Button>(settings, "renameStoreButton");
                ui.toggles = References<Button>(settings, "settingButtons"); ui.faces = References<Image>(settings, "settingFaces"); ui.dots = References<Image>(settings, "settingDots"); ui.toggleStates = References<TMP_Text>(settings, "settingStates");
                ui.enabledFace = Reference<Sprite>(settings, "enabledFace"); ui.disabledFace = Reference<Sprite>(settings, "disabledFace");
                ui.accountStatus = Reference<TMP_Text>(settings, "accountStatus"); ui.storeName = Reference<TMP_Text>(settings, "storeNameLabel");
                var consent = Reference<GameObject>(settings, "consentRow"); if (consent) consent.SetActive(false);
                ReplaceText(ui.settings, "Google account", "Development profile"); ReplaceText(ui.settings, "Local guests or optional Google sync.", "Local test save. No real payments.");
                UnityEngine.Object.DestroyImmediate(settings);
                ui.privacy = CopyUtility("Privacy", root); var privacy = ui.privacy.GetComponent<ImportedPrivacyScreen>();
                ui.privacyClose = Reference<Button>(privacy, "closeButton"); ui.privacyBack = Reference<Button>(privacy, "backButton");
                consent = Reference<GameObject>(privacy, "consentRow"); if (consent) consent.SetActive(false);
                ReplaceText(ui.privacy, "Guest saves stay local. Google sync is optional.", "Your test save stays on this device.");
                ReplaceText(ui.privacy, "Guests stay on this device. Google players sync progress and basic profile data to Firebase. One device plays at a time. Analytics and crash collection are off by default.", "This development game uses a separate local guest profile. Live Google saves and real payments are unavailable. Original progress stays separate. Analytics and crash collection are off.");
                UnityEngine.Object.DestroyImmediate(privacy);
                ui.naming = CopyUtility("Store Name", root); var naming = ui.naming.GetComponent<StoreNameDialog>();
                ui.nameInput = Reference<TMP_InputField>(naming, "input"); ui.nameTitle = Reference<TMP_Text>(naming, "title"); ui.nameDescription = Reference<TMP_Text>(naming, "description");
                ui.nameMessage = Reference<TMP_Text>(naming, "message"); ui.nameCancelLabel = Reference<TMP_Text>(naming, "cancelLabel"); ui.nameSaveLabel = Reference<TMP_Text>(naming, "saveLabel");
                ui.nameSave = Reference<Button>(naming, "saveButton"); ui.nameCancel = Reference<Button>(naming, "cancelButton");
                Reference<GameObject>(naming, "inputRow").SetActive(true); Reference<GameObject>(naming, "detailPanel").SetActive(false);
                ui.nameInput.characterLimit = 24; UnityEngine.Object.DestroyImmediate(naming);
                ui.recovery = CopyUtility("Save Recovery", root); var recovery = ui.recovery.GetComponent<ImportedSaveRecoveryScreen>();
                ui.recoveryRetry = Reference<Button>(recovery, "startButton"); ui.recoveryExit = Reference<Button>(recovery, "exitButton"); ui.recoveryError = Reference<GameObject>(recovery, "errorRow"); ui.recoveryMessage = Reference<TMP_Text>(recovery, "errorText");
                ui.recoveryRetry.GetComponentInChildren<TMP_Text>(true).text = "Retry";
                ReplaceText(ui.recovery, "Start a new local game", "Retry saving or loading");
                ReplaceText(ui.recovery, "Your saved files could not be read.", "Gameplay is paused until saving is ready.");
                ReplaceText(ui.recovery, "Main save and backup are unreadable", "Your progress stays protected");
                ReplaceText(ui.recovery, "Your damaged files are preserved.", "Existing save files are kept for recovery.");
                ReplaceText(ui.recovery, "Enable saving with a fresh game. This device has no cloud recovery.", "Free device storage if needed, then retry. This test profile has no cloud recovery.");
                UnityEngine.Object.DestroyImmediate(recovery);
                AssignAccountCopy(ui);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            SerializeTextSlots(); Debug.Log("REDESIGN_RETAINED_AUTHORED");
        }
        public static void IntegrateMain()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Compiled Edit Mode required");
            const string path = "Assets/Scenes/Main.unity";
            var scene = SceneManager.GetSceneByPath(path); bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Preserve dirty Main edits before integration");
                var game = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<GameController>(true)).Single();
                if (game.redesignApplication) { ValidateMain(scene); return; }
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                if (!prefab.GetComponent<RedesignApplication>()) throw new InvalidOperationException("Author utility dialogs first");
                foreach (var original in scene.GetRootGameObjects()) if (original.GetComponentInChildren<Canvas>(true)) original.SetActive(false);
                var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene); var app = root.GetComponent<RedesignApplication>();
                app.runOnStart = true; PrefabUtility.RecordPrefabInstancePropertyModifications(app);
                game.redesignApplication = app; EditorUtility.SetDirty(game);
                ValidateMain(scene); EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Main save failed");
                Debug.Log("REDESIGN_MAIN_SAVED");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
        public static void AddAudio()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (root.GetComponent<RedesignAudio>()) return;
                var audio = root.AddComponent<RedesignAudio>(); audio.driver = root.GetComponent<RedesignRuntimeDriver>();
                audio.music = root.AddComponent<AudioSource>(); audio.effects = root.AddComponent<AudioSource>();
                audio.music.playOnAwake = audio.effects.playOnAwake = false; audio.music.loop = true;
                audio.music.volume = .12f; audio.effects.volume = .25f;
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        static void ValidateMain(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var app = roots.SelectMany(r => r.GetComponentsInChildren<RedesignApplication>(true)).Single();
            if (!app.runOnStart || !app.gameObject.activeInHierarchy || app.router.initializeGuestOnStart || !app.retained) throw new InvalidOperationException("Invalid Main startup ownership");
            if (roots.SelectMany(r => r.GetComponentsInChildren<EventSystem>()).Count() != 1) throw new InvalidOperationException("Main needs one saved active EventSystem");
            if (roots.Where(r => r != app.gameObject && r.activeInHierarchy).Any(r => r.GetComponentInChildren<Canvas>() != null)) throw new InvalidOperationException("Old UI is still active in Main");
        }
    }
}
