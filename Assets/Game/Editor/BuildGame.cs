using System;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
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
            PrepareContent();
            PlayerSettings.companyName = "Diwali Games"; PlayerSettings.productName = "Rocket Raja";
            var appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Branding/AppIcon.png");
            if (appIcon != null) PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { appIcon });
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
            if (!File.Exists(Scene))
                throw new BuildFailedException("The reviewed redesign scene is missing. Content is generated; finish the directed native screen integration before building the game.");
            ClassicProductionAuthoring.Integrate();
            EditorBuildSettings.scenes = Scenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            AssetDatabase.SaveAssets(); ValidateGame.Run(); Debug.Log("DIWALI_PREPARE_OK");
        }
        internal static GameContent PrepareContent()
        {
            // Isolation chooses save/network targets, never which economy to regenerate.
            RedesignAuthoring.Regenerate();
            return AssetDatabase.LoadAssetAtPath<GameContent>("Assets/Resources/GameContent.asset");
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
            if (RedesignEnvironment.Isolated) throw new BuildFailedException("The isolated redesign is not approved for production rollout. Complete the implementation release acceptance first.");
            if (!File.Exists(Bootstrap)) throw new BuildFailedException("Complete the reviewed Bootstrap screen before building the cloud-save release.");
            var config = Resources.Load<ProductionConfig>("ProductionConfig");
            if (!config || !config.releaseAcceptanceComplete) throw new BuildFailedException("Complete Classic release acceptance, account/billing configuration and store-installed validation before production rollout.");
            if (config == null || !Uri.TryCreate(config.privacyUrl, UriKind.Absolute, out var privacy) || privacy.Scheme != "https" || config.applicationId.EndsWith(".dev")) throw new BuildFailedException("Supply verified production application ID and public HTTPS privacy URL in ProductionConfig.");
            if (!PlayerSettings.Android.useCustomKeystore || string.IsNullOrEmpty(PlayerSettings.Android.keystoreName)) throw new BuildFailedException("Configure your owned upload key in Android Publishing Settings.");
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
