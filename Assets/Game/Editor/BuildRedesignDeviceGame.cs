using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RocketRaja.Editor
{
    /// <summary>Full native game playtest; separate Android package and data from the user's installed game.</summary>
    [InitializeOnLoad]
    public static class BuildRedesignDeviceGame
    {
        public const string Package = "com.diwali.rocketrajaredesign.playtest";
        public const string Apk = "Builds/Android/RocketRaja-redesign-playtest.apk";
        const string Report = "Documentation/Implementation/DeviceGame/Build.txt";
        const string RequestKey = "RocketRaja.DeviceGameBuild.Request", CompletedKey = "RocketRaja.DeviceGameBuild.Completed";
        static BuildRedesignDeviceGame() { EditorApplication.update += RunQueued; }
        public static void Start(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("A stable build request ID is required");
            if (SessionState.GetString(RequestKey, "") == requestId || SessionState.GetString(CompletedKey, "") == requestId) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed || BuildPipeline.isBuildingPlayer) throw new InvalidOperationException("Require compiled, idle Edit Mode");
            Directory.CreateDirectory(Path.GetDirectoryName(Report)); File.WriteAllText(Report, "QUEUED " + DateTime.UtcNow.ToString("O") + "\n");
            SessionState.SetString(RequestKey, requestId);
        }
        static void RunQueued()
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(RequestKey, "")) && !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !EditorApplication.isUpdating && !BuildPipeline.isBuildingPlayer && !EditorUtility.scriptCompilationFailed) Build();
        }
        public static void Build()
        {
            string request = SessionState.GetString(RequestKey, "");
            if (string.IsNullOrEmpty(request)) return;
            SessionState.EraseString(RequestKey); SessionState.SetString(CompletedKey, request);
            var target = NamedBuildTarget.Android;
            string id = PlayerSettings.GetApplicationIdentifier(target), name = PlayerSettings.productName;
            bool bundle = EditorUserBuildSettings.buildAppBundle, customKey = PlayerSettings.Android.useCustomKeystore;
            var scenes = EditorSceneManager.GetSceneManagerSetup();
            Directory.CreateDirectory(Path.GetDirectoryName(Report));
            File.WriteAllText(Report, "RUNNING " + DateTime.UtcNow.ToString("O") + "\nFull native Bootstrap/Main; separate development package; no real billing or live cloud.\n");
            try
            {
                BuildGame.Prepare();
                PlayerSettings.SetApplicationIdentifier(target, Package); PlayerSettings.productName = "Rocket Raja Playtest";
                PlayerSettings.Android.useCustomKeystore = false; EditorUserBuildSettings.buildAppBundle = false;
                // Resolve before scene serialization so generated Maven labels are not first written inside the build callback.
                AssetDatabase.ReleaseCachedFileHandles();
                if (!GooglePlayServices.PlayServicesResolver.ResolveSync(false))
                {
                    File.AppendAllText(Report, "Dependency preflight failed; repairing existing generated POM packaging and retrying once.\n");
                    RepairGeneratedPomPackaging();
                    if (!GooglePlayServices.PlayServicesResolver.ResolveSync(false)) throw new BuildFailedException("Android dependency resolution must succeed before building the playtest");
                }
                VerifyGeneratedMaven();
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Directory.CreateDirectory(Path.GetDirectoryName(Apk));
                var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Main.unity" }, locationPathName = Apk,
                    target = BuildTarget.Android, options = BuildOptions.Development
                });
                File.AppendAllText(Report, result.summary.result + "\n" + result.summary.totalErrors + " errors\n" + result.summary.totalWarnings + " warnings\n" + result.summary.totalTime + "\nReported bytes: " + result.summary.totalSize + "\nPackage: " + Package + "\nAPK: " + Apk + "\n");
                if (result.summary.result != BuildResult.Succeeded || result.summary.totalErrors != 0) throw new BuildFailedException("Native playtest build requires success with zero errors: " + result.summary.result + ", errors=" + result.summary.totalErrors);
                Debug.Log("REDESIGN_DEVICE_GAME_BUILD_OK " + Apk);
            }
            catch (Exception ex) { File.AppendAllText(Report, "FAIL " + ex + "\n"); Debug.LogException(ex); }
            finally
            {
                PlayerSettings.SetApplicationIdentifier(target, id); PlayerSettings.productName = name;
                PlayerSettings.Android.useCustomKeystore = customKey; EditorUserBuildSettings.buildAppBundle = bundle;
                EditorSceneManager.RestoreSceneManagerSetup(scenes); AssetDatabase.SaveAssets();
            }
        }
        static void RepairGeneratedPomPackaging()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("GooglePlayServices.LocalMavenRepository")).FirstOrDefault(t => t != null);
            var patch = type?.GetMethod("PatchPomFile", BindingFlags.Static | BindingFlags.Public);
            if (patch == null) throw new BuildFailedException("Installed resolver POM recovery API is unavailable");
            const string repository = "Assets/GeneratedLocalRepo/Firebase/m2repository/com/google/firebase";
            if (!Directory.Exists(repository)) throw new BuildFailedException("Generated repository is unavailable for bounded recovery");
            foreach (string pom in Directory.GetFiles(repository, "*.pom", SearchOption.AllDirectories))
            {
                string aar = Path.ChangeExtension(pom, "aar"); if (!File.Exists(aar)) continue;
                AssetDatabase.ReleaseCachedFileHandles();
                if (!(bool)patch.Invoke(null, new object[] { aar, aar })) throw new BuildFailedException("Generated POM recovery failed: " + pom);
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
        static void VerifyGeneratedMaven()
        {
            const string repository = "Assets/GeneratedLocalRepo/Firebase/m2repository/com/google/firebase";
            const string source = "Assets/Firebase/m2repository/com/google/firebase";
            if (!Directory.Exists(repository)) throw new BuildFailedException("Firebase generated Maven repository is missing");
            foreach (string original in Directory.GetFiles(source, "*.srcaar", SearchOption.AllDirectories))
            {
                string aar = Path.ChangeExtension(repository + original.Substring(source.Length), "aar");
                string file = Path.ChangeExtension(aar, "pom");
                if (!File.Exists(file) || !File.Exists(aar)) throw new BuildFailedException("Missing generated Firebase Maven artifact: " + aar);
                var pom = new XmlDocument(); pom.Load(file); var nodes = pom.GetElementsByTagName("packaging");
                if (nodes.Count != 1 || nodes[0].InnerText != "aar")
                    throw new BuildFailedException("Incomplete generated Firebase Maven artifact: " + file);
                using (var hash = SHA256.Create())
                    if (!hash.ComputeHash(File.ReadAllBytes(original)).SequenceEqual(hash.ComputeHash(File.ReadAllBytes(aar)))) throw new BuildFailedException("Generated Firebase AAR differs from local SDK: " + aar);
            }
        }
    }
}
