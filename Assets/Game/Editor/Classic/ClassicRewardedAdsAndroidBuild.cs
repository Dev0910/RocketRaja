using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicRewardedAdsAndroidBuild
    {
        const string Key = "Classic.RewardedAdsAndroidBuild";
        public const string Apk = "Builds/Android/RocketRaja-classic-rewarded-test.apk";
        const string Report = "Documentation/Implementation/RewardedAds/AndroidBuild.txt";
        static ClassicRewardedAdsAndroidBuild() { EditorApplication.update += Tick; }
        public static string Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed || BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Idle compiled Edit Mode required");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Android target required");
            Directory.CreateDirectory(Path.GetDirectoryName(Report));
            File.WriteAllText(Report, "Queued Android development APK with Google test rewarded ads.\n");
            SessionState.SetBool(Key, true); return "Queued " + Apk;
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer) return;
            SessionState.EraseBool(Key);
            bool bundle = EditorUserBuildSettings.buildAppBundle;
            bool customSigning = PlayerSettings.Android.useCustomKeystore;
            try
            {
                EditorUserBuildSettings.buildAppBundle = false;
                Directory.CreateDirectory(Path.GetDirectoryName(Apk));
                File.WriteAllText(Report, "RUNNING\nApplication: " + Application.identifier + "\nDevelopment build; Google test rewarded unit.\n");
                if (customSigning && (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) || string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass)))
                {
                    PlayerSettings.Android.useCustomKeystore = false;
                    File.AppendAllText(Report, "Debug signing fallback: configured release-key passwords are not loaded. This APK cannot update an existing release-signed installation.\n");
                }
                AssetDatabase.ReleaseCachedFileHandles();
                if (!GooglePlayServices.PlayServicesResolver.ResolveSync(false)) throw new BuildFailedException("Android dependency resolution failed");
                // Reuse the project's bounded generated-Firebase repair/verification.
                var repair = typeof(BuildRedesignDeviceGame).GetMethod("RepairGeneratedPomPackaging", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                repair.Invoke(null, null);
                typeof(BuildRedesignDeviceGame).GetMethod("VerifyGeneratedMaven", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
                AssetDatabase.ReleaseCachedFileHandles();
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Main.unity" },
                    target = BuildTarget.Android, locationPathName = Apk, options = BuildOptions.Development
                });
                File.AppendAllText(Report, result.summary.result + "\nErrors: " + result.summary.totalErrors + "\nWarnings: " + result.summary.totalWarnings + "\nDuration: " + result.summary.totalTime + "\nBytes: " + result.summary.totalSize + "\n");
                File.AppendAllLines(Report, result.steps.SelectMany(s => s.messages).Where(m => m.type == LogType.Error || m.type == LogType.Exception || m.type == LogType.Warning).Select(m => m.type + ": " + m.content).Distinct());
                if (result.summary.result != BuildResult.Succeeded || result.summary.totalErrors != 0) throw new BuildFailedException("Rewarded ads APK build must succeed with zero errors");
            }
            catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); Debug.LogException(e); }
            finally { EditorUserBuildSettings.buildAppBundle = bundle; PlayerSettings.Android.useCustomKeystore = customSigning; }
        }
    }
}
