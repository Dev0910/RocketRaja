using System;
using System.Linq;
using System.Reflection;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        [MenuItem("Rocket Raja/Classic/Configure rewarded ads")]
        public static string AuthorRewardedAds()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var config = RewardedAdsSettings.Load();
            if (!config)
            {
                config = ScriptableObject.CreateInstance<RewardedAdsSettings>();
                config.androidAppId = "ca-app-pub-3595621361914402~9466422249";
                config.androidRewardedUnitId = "ca-app-pub-3595621361914402/1364570948";
                config.useTestAds = false; config.enabledInGame = true;
                AssetDatabase.CreateAsset(config, "Assets/Resources/RewardedAdsSettings.asset");
            }
            SetGoogleAdsAppIds(config);
            var scene = SceneManager.GetSceneByPath(ScenePath); bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                ui = scene.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
                Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Author ad privacy options");
                var settings = ui.dialogs.Single(v => v.route == "Settings");
                var content = settings.root.GetComponentsInChildren<ClassicControl>(true).First(c => c.action == "Navigate" && c.parameter == "Privacy").transform.parent;
                while (content && !content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>()) content = content.parent;
                if (!content) throw new InvalidOperationException("Missing Settings scroll layout");
                if (!ui.adPrivacyRow)
                {
                    body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
                    bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
                    blue = ui.blueButton; ivory = ui.ivoryButton;
                    var r = New("Ad privacy choices", content); Height(r, 48);
                    Button(r, "AdPrivacyOptions", "", "", "Ad privacy choices", ivory);
                    ui.adPrivacyRow = r.gameObject; r.gameObject.SetActive(false);
                }
                ui.adPrivacyRow.transform.SetParent(content, false);
                // Replace outdated ad-free/privacy claims within the existing popup.
                var privacy = ui.dialogs.Single(v => v.route == "Privacy");
                foreach (var label in privacy.root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (label.text.Contains("This importer does not contact"))
                        label.text = "Optional rewarded ads use Google AdMob. Complete a video to double one offline reward. Manage available ad privacy choices in Settings.";
                }
                EditorUtility.SetDirty(ui); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                return "Configured Android AdMob IDs, automatic provider startup, and Settings privacy choices. Development builds always use test rewarded units.";
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        internal static void SetGoogleAdsAppIds(RewardedAdsSettings config)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings")).First(t => t != null);
            var sdk = (UnityEngine.Object)type.GetMethod("LoadInstance", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            var serialized = new SerializedObject(sdk);
            // The project already owns templates/min API and resolves before BuildPlayer.
            // GMA's optional hook force-resolves again during scene serialization, which
            // reproduces EDM4U's Windows mapped-Firebase-POM failure (error 1224).
            serialized.FindProperty("enableGradleBuildPreProcessor").boolValue = false;
            serialized.FindProperty("adMobAndroidAppId").stringValue = string.IsNullOrWhiteSpace(config.androidAppId) ? RewardedAdsSettings.TestAndroidApp : config.androidAppId;
            serialized.FindProperty("adMobIOSAppId").stringValue = string.IsNullOrWhiteSpace(config.iosAppId) ? RewardedAdsSettings.TestIosApp : config.iosAppId;
            serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(sdk); AssetDatabase.SaveAssetIfDirty(sdk);
        }
    }

    public sealed class RewardedAdsBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1200;
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android && report.summary.platform != BuildTarget.iOS) return;
            var settings = RewardedAdsSettings.Load();
            if (!settings || !settings.enabledInGame) return;
            bool android = report.summary.platform == BuildTarget.Android;
            bool development = (report.summary.options & BuildOptions.Development) != 0;
            if (!development && !settings.useTestAds)
            {
                var app = android ? settings.androidAppId : settings.iosAppId;
                var unit = android ? settings.androidRewardedUnitId : settings.iosRewardedUnitId;
                if (!System.Text.RegularExpressions.Regex.IsMatch(app ?? "", @"^ca-app-pub-\d{16}~\d{10}$") ||
                    !System.Text.RegularExpressions.Regex.IsMatch(unit ?? "", @"^ca-app-pub-\d{16}/\d{10}$"))
                    throw new BuildFailedException("Configure this platform's AdMob App ID and rewarded Ad Unit ID before building live ads.");
            }
            BuildClassicUI.SetGoogleAdsAppIds(settings);
        }
    }
}
