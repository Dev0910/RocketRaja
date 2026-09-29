using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class CloudBuildValidation
    {
        public static void RunOnce(string requestId)
        {
            if (string.IsNullOrEmpty(requestId)) throw new ArgumentException("A unique build request ID is required.", nameof(requestId));
            const string key = "RocketRaja.CloudBuildValidation.Request";
            if (SessionState.GetString(key, "") == requestId) return;
            SessionState.SetString(key, requestId);
            Run();
        }

        [MenuItem("Rocket Raja/Build Cloud Backend Check APK")]
        public static void Run() => Schedule(false);

        public static void RunSignedOnce(string requestId)
        {
            if (string.IsNullOrEmpty(requestId)) throw new ArgumentException("A unique build request ID is required.");
            const string key = "RocketRaja.CloudBuildValidation.SignedRequest";
            if (SessionState.GetString(key, "") == requestId) return;
            if (!PlayerSettings.Android.useCustomKeystore || string.IsNullOrEmpty(PlayerSettings.Android.keystoreName))
                throw new InvalidOperationException("Configure the installed APK's signing key before building an update.");
            SessionState.SetString(key, requestId);
            Schedule(true);
        }

        static void Schedule(bool useConfiguredSigning)
        {
            // A long blocking MCP call may be retried by its transport. Schedule once and
            // return immediately so retries cannot enqueue identical native builds.
            const string key = "RocketRaja.CloudBuildValidation.Scheduled";
            if (SessionState.GetBool(key, false) || BuildPipeline.isBuildingPlayer) return;
            if (EditorApplication.isCompiling || EditorApplication.isPlaying) throw new InvalidOperationException("Finish compilation and stop play mode first.");
            SessionState.SetBool(key, true);
            EditorApplication.delayCall += () =>
            {
                try { Build(useConfiguredSigning); }
                finally { SessionState.SetBool(key, false); }
            };
        }
        static void Build(bool useConfiguredSigning)
        {
            string reportFile = useConfiguredSigning ? "Documentation/CloudSaveValidation/AckFixAndroidBuild.txt" : "Documentation/CloudSaveValidation/AndroidBuild.txt";
            Directory.CreateDirectory("Documentation/CloudSaveValidation"); Directory.CreateDirectory("Builds/Android");
            bool wasBundle = EditorUserBuildSettings.buildAppBundle, wasCustom = PlayerSettings.Android.useCustomKeystore;
            File.WriteAllText(reportFile, "Running Android native integration build…");
            try
            {
                EditorUserBuildSettings.buildAppBundle = false;
                if (!useConfiguredSigning) PlayerSettings.Android.useCustomKeystore = false;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = File.Exists("Assets/Scenes/Bootstrap.unity") ? new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Main.unity" } : new[] { "Assets/Scenes/Main.unity" },
                    locationPathName = useConfiguredSigning ? "Builds/Android/RocketRaja-cloud-save-fix.apk" : "Builds/Android/RocketRaja-cloud-backend-check.apk",
                    target = BuildTarget.Android, options = useConfiguredSigning ? BuildOptions.None : BuildOptions.Development
                });
                File.WriteAllText(reportFile, report.summary.result + "\nErrors=" + report.summary.totalErrors + "\nWarnings=" + report.summary.totalWarnings + "\nDuration=" + report.summary.totalTime);
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Android cloud integration build failed. See the build report and Editor log.");
                Debug.Log("CLOUD_ANDROID_BUILD_OK");
            }
            catch (Exception ex) { File.AppendAllText(reportFile, "\n" + ex); throw; }
            finally { EditorUserBuildSettings.buildAppBundle = wasBundle; PlayerSettings.Android.useCustomKeystore = wasCustom; }
        }
    }
}
