using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class BuildGame
    {
        const string Scene = "Assets/Scenes/Main.unity";
        const string Bootstrap = "Assets/Scenes/Bootstrap.unity";
        static string[] Scenes => File.Exists(Bootstrap) ? new[] { Bootstrap, Scene } : new[] { Scene };
        [MenuItem("Rocket Raja/Prepare Project")]
        public static void Prepare()
        {
            Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Assets/Resources");
            if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro/Resources"))
            {
                string package = Path.Combine(UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TMP_Text).Assembly).resolvedPath, "Package Resources/TMP Essential Resources.unitypackage");
                if (!File.Exists(package)) throw new BuildFailedException("TMP Essential Resources package not found");
                AssetDatabase.ImportPackage(package, false);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            var graphics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var shaders = graphics.FindProperty("m_AlwaysIncludedShaders");
            foreach (string name in new[] { "TextMeshPro/Mobile/Distance Field", "TextMeshPro/Distance Field", "UI/Default" })
            {
                var shader = Shader.Find(name); if (shader == null) continue;
                bool exists = false; for (int i = 0; i < shaders.arraySize; i++) if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader) exists = true;
                if (!exists) { shaders.InsertArrayElementAtIndex(shaders.arraySize); shaders.GetArrayElementAtIndex(shaders.arraySize - 1).objectReferenceValue = shader; }
            }
            graphics.ApplyModifiedPropertiesWithoutUndo();
            if (Resources.Load<TMPro.TMP_Settings>("TMP Settings") == null) throw new BuildFailedException("TMP resources are still importing. Let import finish, then rerun Prepare/Build.");
            var content = AssetDatabase.LoadAssetAtPath<GameContent>("Assets/Resources/GameContent.asset");
            if (content == null) { content = ScriptableObject.CreateInstance<GameContent>(); content.catalog = Catalog.Create(); AssetDatabase.CreateAsset(content, "Assets/Resources/GameContent.asset"); }
            if (content.catalog.skills == null || content.catalog.skills.Length != 3) { content.catalog.skills = Catalog.Create().skills; EditorUtility.SetDirty(content); }
            if (content.catalog.legacy == null || content.catalog.legacy.Length != 12) { content.catalog.legacy = Catalog.Create().legacy; EditorUtility.SetDirty(content); }
            PlayerSettings.companyName = "Diwali Games"; PlayerSettings.productName = "Rocket Raja";
            var appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Branding/AppIcon.png");
            if (appIcon != null) PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { appIcon });
            PlayerSettings.bundleVersion = "0.1.0"; PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.diwali.fireworkstycoon.dev");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = PlayerSettings.allowedAutorotateToLandscapeRight = PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.defaultScreenWidth = 540; PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true; PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard);
            var ps = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = ps.FindProperty("activeInputHandler"); if (input != null) input.intValue = 0; ps.ApplyModifiedPropertiesWithoutUndo();
            QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 0;
            foreach (string path in Directory.GetFiles("Assets/Resources/Art", "*.png"))
            {
                var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Default; importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false; importer.maxTextureSize = 2048; importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
            ConfigureTestAds();
            if (!File.Exists(Scene))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); camera.tag = "MainCamera";
                camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor; camera.GetComponent<Camera>().backgroundColor = GameView.Navy;
                new GameObject("Game", typeof(GameController)); EditorSceneManager.SaveScene(scene, Scene);
            }
            EditorBuildSettings.scenes = Scenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            File.WriteAllText("Documentation/ContentCatalog.json", JsonUtility.ToJson(content.catalog, true));
            AssetDatabase.SaveAssets(); ValidateGame.Run(); Debug.Log("DIWALI_PREPARE_OK");
        }
        static void ConfigureTestAds()
        {
            // SDK's settings class is internal; SerializedObject keeps this adapter editor-only.
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings")).FirstOrDefault(t => t != null);
            if (type == null) throw new BuildFailedException("Google Mobile Ads package missing");
            var settings = (UnityEngine.Object)type.GetMethod("LoadInstance", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            var data = new SerializedObject(settings);
            if (string.IsNullOrEmpty(data.FindProperty("adMobAndroidAppId").stringValue)) data.FindProperty("adMobAndroidAppId").stringValue = "ca-app-pub-3940256099942544~3347511713";
            data.FindProperty("adMobIOSAppId").stringValue = "ca-app-pub-3940256099942544~1458002511";
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(settings);
        }
        [MenuItem("Rocket Raja/Build Windows Preview")]
        public static void Windows() { Prepare(); Build("Builds/Windows/RocketRaja.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development); }
        [MenuItem("Rocket Raja/Build Android Test APK")]
        public static void Android()
        {
            Prepare(); EditorUserBuildSettings.buildAppBundle = false; PlayerSettings.Android.useCustomKeystore = false;
            Build("Builds/Android/RocketRaja-test.apk", BuildTarget.Android, BuildOptions.Development);
        }
        [MenuItem("Rocket Raja/Build Production Bundle")]
        public static void Production()
        {
            if (!File.Exists(Bootstrap)) throw new BuildFailedException("Complete the reviewed Bootstrap screen before building the cloud-save release.");
            var config = Resources.Load<ProductionConfig>("ProductionConfig");
            if (config == null || !config.productionAds || string.IsNullOrWhiteSpace(config.rewardedAdUnitId) || config.rewardedAdUnitId.Contains("3940256099942544") || !Uri.TryCreate(config.privacyUrl, UriKind.Absolute, out var privacy) || privacy.Scheme != "https" || config.applicationId.EndsWith(".dev")) throw new BuildFailedException("Supply verified production application ID, rewarded unit and public HTTPS privacy URL in ProductionConfig.");
            if (!PlayerSettings.Android.useCustomKeystore || string.IsNullOrEmpty(PlayerSettings.Android.keystoreName)) throw new BuildFailedException("Configure your owned upload key in Android Publishing Settings.");
            var ads = Resources.Load<ScriptableObject>("GoogleMobileAdsSettings");
            if (ads == null || new SerializedObject(ads).FindProperty("adMobAndroidAppId").stringValue.Contains("3940256099942544")) throw new BuildFailedException("Replace the sample AdMob application ID before production.");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, config.applicationId);
            EditorUserBuildSettings.buildAppBundle = true;
            Build("Builds/Android/RocketRaja-release.aab", BuildTarget.Android, BuildOptions.None);
        }
        static void Build(string path, BuildTarget target, BuildOptions options)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = Scenes, locationPathName = path, target = target, options = options });
            File.WriteAllText("Documentation/Build-" + target + ".txt", report.summary.result + "\n" + report.summary.totalSize + " bytes\n" + report.summary.totalTime + "\n" + report.summary.totalErrors + " errors\n");
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Build failed: " + report.summary.result);
            Debug.Log("DIWALI_BUILD_OK " + path);
        }
    }
}
