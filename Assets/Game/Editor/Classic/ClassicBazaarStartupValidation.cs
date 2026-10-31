using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicBazaarStartupValidation
    {
        const string Key = "RocketRaja.BazaarStartupQA.", Report = "Documentation/Implementation/UiUnification/native/startup/";
        static ClassicBazaarStartupValidation()
        {
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
            Application.logMessageReceived += (message, stack, type) =>
            {
                if (SessionState.GetBool(Key + "Running", false) && EditorApplication.isPlaying && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                    SessionState.SetString(Key + "RuntimeError", message);
            };
        }
        static void Check(bool ok, string message, List<string> checks)
        {
            if (!ok) throw new InvalidOperationException("FAIL " + message);
            checks.Add("PASS " + message);
        }
        public static void RunEditChecks()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode required");
            Directory.CreateDirectory(Report);
            var checks = new List<string>();
            string root = Path.GetFullPath("Temp/LocalOnlyEdit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var catalog = RedesignRuntimeContent.Load();
            RedesignSaveStore Store(string folder) => new RedesignSaveStore(Path.Combine(folder, "rocket-raja-v2.save"), catalog, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            var store = Store(root); var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc);
            Check(store.Load().CanCreateFresh, "Missing local profile permits a fresh game", checks);
            state.storeName = "Local test shop"; state.products[0] = 1; store.Save(state);
            Check(store.Load().state.storeName == state.storeName && store.Load().state.products[0] == 1, "Existing device progress round-trips without an account", checks);
            state.cash += new Amount(100); store.Save(state);
            File.WriteAllText(Path.Combine(root, "rocket-raja-v2.save"), "damaged main");
            Check(Store(root).Load().status == RedesignLoadStatus.RecoveredBackup, "Valid backup remains recoverable", checks);
            string corrupt = Path.Combine(root, "corrupt"); Directory.CreateDirectory(corrupt); string corruptFile = Path.Combine(corrupt, "rocket-raja-v2.save"); File.WriteAllText(corruptFile, "unreadable source");
            Check(Store(corrupt).Load().status == RedesignLoadStatus.Corrupt && File.ReadAllText(corruptFile) == "unreadable source", "Corrupt local files stay blocked and untouched", checks);
            string future = Path.Combine(root, "future"); Directory.CreateDirectory(future); string raw = "{\"schema\":99,\"economy\":\"future\",\"contentVersion\":\"future\"}"; string futureFile = Path.Combine(future, "rocket-raja-v2.save"); File.WriteAllText(futureFile, SavePayload.Digest(raw) + "\n" + raw);
            Check(Store(future).Load().status == RedesignLoadStatus.UnsupportedVersion, "Future-version local saves cannot become a fresh game", checks);
            var main = SceneManager.GetSceneByPath(ClassicProductionAuthoring.Main);
            var ui = main.GetRootGameObjects().Where(g => g.activeSelf).Select(g => g.GetComponent<ClassicUI>()).Single(g => g);
            Check(ui.dialogs.All(v => !LocalOnlyPresentationAuthoring.RemovedRoute(v.route)), "Saved Main has no account/login dialogs", checks);
            Check(!ui.GetComponentsInChildren<ClassicControl>(true).Any(c => c.action == "Account" || c.action == "Navigate" && LocalOnlyPresentationAuthoring.RemovedRoute(c.parameter)), "Settings and recovery have no account navigation", checks);
            Check(ui.GetComponentsInChildren<ClassicControl>(true).Where(c => c.action == "Purchase").All(c => !c.GetComponent<Button>().interactable), "Real purchase actions remain unavailable", checks);
            Check(!File.Exists("Assets/Game/Runtime/Cloud/FirebaseAccountAuth.cs") && !File.Exists("Assets/Plugins/Android/GoogleSignInBridge.java"), "Live Google adapters and native bridge are removed", checks);
            string gradle = File.ReadAllText("Assets/Plugins/Android/mainTemplate.gradle");
            Check(!gradle.Contains("androidx.credentials:") && !gradle.Contains("identity.googleid:"), "Resolved Android declarations contain no login-only libraries", checks);
            Check(File.Exists("Assets/Firebase/Plugins/Firebase.Auth.dll") && File.Exists("Packages/com.google.ads.mobile/package.json"), "Firebase modules and rewarded-ad package remain installed", checks);
            File.WriteAllLines(Report + "EditChecks.txt", checks);
            Debug.Log("LOCAL_ONLY_EDIT_CHECKS_OK " + checks.Count);
        }

        public static void CaptureAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode required");
            Directory.CreateDirectory(Report + "Renders");
            var scene = SceneManager.GetSceneByPath(ClassicProductionAuthoring.Bootstrap);
            var owner = scene.GetRootGameObjects().Where(g => g.activeSelf).Select(g => g.GetComponent<BootstrapController>()).Single(c => c);
            var canvas = owner.GetComponent<Canvas>(); var scaler = owner.GetComponent<CanvasScaler>();
            var other = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(c => c != canvas && c.enabled).ToArray();
            foreach (var c in other) c.enabled = false;
            var camera = scene.GetRootGameObjects().Select(g => g.GetComponent<Camera>()).First(c => c);
            var mode = canvas.renderMode; var oldCamera = canvas.worldCamera; bool oldScaler = scaler.enabled; var previous = RenderTexture.active;
            var checks = new List<string>();
            try
            {
                foreach (var stateName in new[] { "loading", "recovery", "legacy" })
                foreach (var size in new[] { new Vector2Int(1080, 2200), new Vector2Int(390, 844), new Vector2Int(360, 640), new Vector2Int(360, 540) })
                {
                    owner.PreviewState(stateName);
                    var target = RenderTexture.GetTemporary(size.x, size.y, 24);
                    try
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10; scaler.enabled = false;
                        canvas.scaleFactor = size.x / 1080f; camera.targetTexture = target;
                        Canvas.ForceUpdateCanvases(); foreach (var text in canvas.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate(); Canvas.ForceUpdateCanvases();
                        foreach(var layout in canvas.GetComponentsInChildren<ClassicBazaarStartupLayout>()) layout.Apply(); Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                        var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); texture.Apply();
                        File.WriteAllBytes(Report + "Renders/" + stateName + "-" + size.x + "x" + size.y + ".png", texture.EncodeToPNG()); Object.DestroyImmediate(texture);
                        Check(!canvas.GetComponentsInChildren<TMP_Text>().Any(t => t.isTextOverflowing), stateName + " text fits " + size.x + "x" + size.y, checks);
                    }
                    finally { camera.targetTexture = null; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); }
                }
            }
            finally
            {
                canvas.renderMode = mode; canvas.worldCamera = oldCamera; scaler.enabled = oldScaler;
                foreach (var c in other) if (c) c.enabled = true;
                owner.PreviewState("loading"); Canvas.ForceUpdateCanvases(); EditorSceneManager.SaveScene(scene);
                File.WriteAllLines(Report + "VisualChecks.txt", checks);
            }
        }

        public static void StartPlayChecks()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            if (SessionState.GetBool(Key + "Running", false)) return;
            Directory.CreateDirectory(Report);
            SessionState.SetString(Key+"OldProfile",SessionState.GetString("RocketRaja.LocalOnlyQA.Profile",""));
            SessionState.SetBool(Key + "HadMode", PlayerPrefs.HasKey(RedesignEnvironment.PreferenceKey("RocketRaja.IdentityMode")));
            SessionState.SetString(Key + "OldMode", LocalProfiles.Mode);
            SessionState.SetString(Key + "Root", Path.GetFullPath("Temp/LocalOnlyPlay-" + Guid.NewGuid().ToString("N")));
            SessionState.SetInt(Key + "Case", 0); SessionState.SetBool(Key + "Failed", false); SessionState.SetBool(Key + "Running", true);
            File.WriteAllText(Report + "PlayChecks.txt", "RUNNING isolated Editor startup checks; live saves/ads excluded\n");
            PrepareCase();
        }
        static readonly string[] Cases = { "fresh", "existing", "old-account-mode", "corrupt", "future", "backup", "restart" };
        static void PrepareCase()
        {
            int index = SessionState.GetInt(Key + "Case", 0);
            string folder = Path.Combine(SessionState.GetString(Key + "Root", ""), index == 6 ? "existing" : Cases[index]); Directory.CreateDirectory(folder);
            SessionState.SetString("RocketRaja.LocalOnlyQA.Profile", folder);
            SessionState.EraseString(Key + "RuntimeError");
            string file = Path.Combine(folder, "rocket-raja-v2.save");
            var catalog = RedesignRuntimeContent.Load();
            if (index == 1 || index == 5)
            {
                var s = RedesignState.Fresh(RedesignRuntimeDriver.Utc - 3600); s.storeName = "Local test shop"; s.products[0] = 1;
                var store = new RedesignSaveStore(file, catalog, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
                store.Save(s); if (index == 5) { store.Save(s); File.WriteAllText(file, "damaged main"); }
            }
            if (index == 2) { Directory.CreateDirectory(Path.Combine(folder, "accounts")); File.WriteAllText(Path.Combine(folder, "accounts/untouched-checkpoint"), "cloud progress stays separate"); }
            if (index == 3) File.WriteAllText(file, "unreadable source");
            if (index == 4) { string raw = "{\"schema\":99,\"economy\":\"future\",\"contentVersion\":\"future\"}"; File.WriteAllText(file, SavePayload.Digest(raw) + "\n" + raw); }
            LocalProfiles.SetMode("account");
            SessionState.SetBool("RocketRaja.BootstrapPreview", false); SessionState.SetBool(Key + "Observed", false);
            SessionState.SetString(Key + "Started", DateTime.UtcNow.ToString("O"));
            EditorSceneManager.OpenScene(ClassicProductionAuthoring.Bootstrap);
            EditorApplication.isPlaying = true;
        }
        static void Tick()
        {
            if(SessionState.GetBool(Key+"Running",false)&&SessionState.GetBool(Key+"PrepareNext",false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating)
            {SessionState.SetBool(Key+"PrepareNext",false);PrepareCase();return;}
            if (!SessionState.GetBool(Key + "Running", false) || !EditorApplication.isPlaying || EditorApplication.isCompiling || SessionState.GetBool(Key + "Observed", false)) return;
            int index = SessionState.GetInt(Key + "Case", 0);
            string runtimeError = SessionState.GetString(Key + "RuntimeError", "");
            if (!string.IsNullOrEmpty(runtimeError)) { Fail(runtimeError); return; }
            var application = Object.FindAnyObjectByType<ClassicApplication>();
            if (!application || !application.StartupAttempted)
            {
                if (DateTime.TryParse(SessionState.GetString(Key + "Started", ""), out var at) && (DateTime.UtcNow - at.ToUniversalTime()).TotalSeconds > 90) Fail("Startup timed out: " + Cases[index]);
                return;
            }
            try
            {
                var host = RedesignSessionHost.Instance;
                if (!host || host.Account != null || host.Coordinator != null || host.GoogleSupported || host.Mode != "guest") throw new InvalidOperationException("Live identity was reached");
                bool recovery = index == 3 || index == 4;
                if (recovery ? application.Ready || application.ui.CurrentDialog != "SaveRecovery" : !application.Ready) throw new InvalidOperationException("Unexpected readiness/recovery state");
                string root = SessionState.GetString("RocketRaja.LocalOnlyQA.Profile", "");
                if (host.ProfileRoot != root || application.ui.driver.IsAccount) throw new InvalidOperationException("Wrong local profile");
                if ((index == 1 || index == 6) && application.ui.Engine.State.storeName != "Local test shop") throw new InvalidOperationException("Local progress lost");
                if (index == 2 && File.ReadAllText(Path.Combine(root, "accounts/untouched-checkpoint")) != "cloud progress stays separate") throw new InvalidOperationException("Account checkpoint changed");
                if (index == 3 && File.ReadAllText(Path.Combine(root, "rocket-raja-v2.save")) != "unreadable source") throw new InvalidOperationException("Corrupt source changed");
                if (index == 4 && application.ui.driver.LoadResult.status != RedesignLoadStatus.UnsupportedVersion) throw new InvalidOperationException("Future version bypassed");
                File.AppendAllText(Report + "PlayChecks.txt", "PASS " + Cases[index] + ": automatic local startup; old account preference ignored; " + (recovery ? "protected recovery" : "playable local session") + "\n");
                SessionState.SetBool(Key + "Observed", true); EditorApplication.isPlaying = false;
            }
            catch (Exception error) { Fail(Cases[index] + ": " + error.Message); }
        }
        static void Fail(string error)
        {
            File.AppendAllText(Report + "PlayChecks.txt", "FAIL " + error + "\n");
            SessionState.SetBool(Key + "Failed", true); SessionState.SetBool(Key + "Observed", true); EditorApplication.isPlaying = false;
        }
        static void ModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Key + "Running", false)) return;
            int next = SessionState.GetInt(Key + "Case", 0) + 1;
            if (next < Cases.Length && !SessionState.GetBool(Key + "Failed", false))
            { SessionState.SetInt(Key + "Case", next); SessionState.SetBool(Key+"PrepareNext",true); return; }
            Finish();
        }
        static void Finish()
        {
            string modeKey = RedesignEnvironment.PreferenceKey("RocketRaja.IdentityMode");
            if (SessionState.GetBool(Key + "HadMode", false)) PlayerPrefs.SetString(modeKey, SessionState.GetString(Key + "OldMode", "")); else PlayerPrefs.DeleteKey(modeKey);
            PlayerPrefs.Save(); SessionState.SetString("RocketRaja.LocalOnlyQA.Profile",SessionState.GetString(Key+"OldProfile","")); SessionState.SetBool(Key + "Running", false);
            File.AppendAllText(Report + "PlayChecks.txt", SessionState.GetBool(Key + "Failed", false) ? "FAILED\n" : "ALL 7 STARTUP CASES PASSED\n");
            EditorApplication.delayCall += () => { EditorSceneManager.OpenScene(ClassicProductionAuthoring.Bootstrap); EditorSceneManager.OpenScene(ClassicProductionAuthoring.Main, OpenSceneMode.Additive); };
        }
    }
}
