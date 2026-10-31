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
    public static class LocalOnlyAndroidBuild
    {
        const string Key = "RocketRaja.LocalOnlyBuild.Queued";
        public const string Output = "Builds/Android/RocketRaja-local-only-validation.apk";
        public const string Report = "Documentation/Implementation/LocalOnly/AndroidBuild.txt";
        static LocalOnlyAndroidBuild() { EditorApplication.update += Tick; }
        public static string Start()
        {
            if (SessionState.GetBool(Key, false) || BuildPipeline.isBuildingPlayer) return "Local-only build already queued or running.";
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Compiled idle Edit Mode required");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Android target required");
            Directory.CreateDirectory(Path.GetDirectoryName(Report)); File.WriteAllText(Report, "QUEUED local-only debug-signed validation APK; existing package retained; project signing restored afterward\n");
            SessionState.SetBool(Key, true); return "Queued local-only Android APK.";
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer) return;
            SessionState.SetBool(Key, false);
            bool bundle = EditorUserBuildSettings.buildAppBundle;
            bool customSigning = PlayerSettings.Android.useCustomKeystore;
            try
            {
                EditorUserBuildSettings.buildAppBundle = false; PlayerSettings.Android.useCustomKeystore = false; Directory.CreateDirectory(Path.GetDirectoryName(Output));
                File.WriteAllText(Report, "RUNNING\nPackage: " + Application.identifier + "\nScenes: Bootstrap, Main\nDebug-signed development validation APK; local-only gameplay; no account migration\nProject signing settings restored after building\n");
                AssetDatabase.ReleaseCachedFileHandles();
                if (!GooglePlayServices.PlayServicesResolver.ResolveSync(false)) throw new BuildFailedException("Android dependency resolution failed");
                var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ClassicProductionAuthoring.Bootstrap, ClassicProductionAuthoring.Main }, target = BuildTarget.Android, locationPathName = Output, options = BuildOptions.Development });
                File.AppendAllText(Report, result.summary.result + "\nErrors: " + result.summary.totalErrors + "\nWarnings: " + result.summary.totalWarnings + "\nDuration: " + result.summary.totalTime + "\nBytes: " + result.summary.totalSize + "\n");
                File.AppendAllLines(Report, result.steps.SelectMany(s => s.messages).Where(m => m.type == LogType.Error || m.type == LogType.Exception).Select(m => m.content).Distinct());
                if (result.summary.result != BuildResult.Succeeded || result.summary.totalErrors != 0) throw new BuildFailedException("Local-only Android build failed");
            }
            catch (Exception error) { File.AppendAllText(Report, "FAIL " + error + "\n"); Debug.LogException(error); }
            finally { EditorUserBuildSettings.buildAppBundle = bundle; PlayerSettings.Android.useCustomKeystore = customSigning; }
        }
    }
}
