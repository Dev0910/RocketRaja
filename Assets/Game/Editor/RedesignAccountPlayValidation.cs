using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Saved native Main owner against an in-memory cloud and real disposable checkpoints.
    /// This fixture never creates Firebase clients, authenticates or touches deployed saves.</summary>
    [InitializeOnLoad]
    public static class RedesignAccountPlayValidation
    {
        const string Key = "RocketRaja.RedesignAccountQA", Report = "Documentation/Implementation/Unity-Account-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>();
        static RedesignApplication app;
        static RedesignAccountSession account;
        static FixtureCloud cloud;
        static RedesignLifecycle original;
        static RedesignUiContext oldContext;
        static Transform[] instances;
        static CheckpointStore checkpoint;
        static int step;
        static double deadline;
        static long taps;
        static long generation, revision;
        static string receipt;
        static RedesignAccountPlayValidation() => EditorApplication.playModeStateChanged += Mode;
        [MenuItem("Rocket Raja/Redesign/Test Account Recovery In Play Mode")]
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Require compiled Edit Mode");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Preserve dirty scenes before QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = setup.Select(s => new Record { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            SessionState.SetString(Key + ".profile", Path.GetFullPath("Temp/RedesignAccountQA-" + Guid.NewGuid().ToString("N")));
            SessionState.SetBool(Key, true);
            File.WriteAllText(Report, "RUNNING: saved native screens; fixture cloud; real isolated checkpoint files. No Google sign-in or live-device claim.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (mode == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); errors.Clear(); step = 0; deadline = EditorApplication.timeSinceStartup + .5;
                Application.logMessageReceived += Log; EditorApplication.update += Tick;
            }
            else if (mode == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
                account?.Dispose(); account = null; cloud = null; app = null;
                SessionState.SetBool(Key, false);
                var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
                EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
                Debug.Log("REDESIGN_ACCOUNT_QA_RESTORED");
            }
        }
        static void Log(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (message.StartsWith("The project path contains spaces") && trace.Contains("Startup")) return;
            errors.Add(message);
        }
        static void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button button)
        {
            Check(button && button.IsActive() && button.IsInteractable(), "Active native control: " + button.name);
            Canvas.ForceUpdateCanvases(); var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject, "Topmost native hit: " + button.name + "; point=" + pointer.position + "; hits=" + string.Join(",", hits.Take(3).Select(h => h.gameObject.name)));
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                switch (step++)
                {
                    case 0:
                        app = UnityEngine.Object.FindAnyObjectByType<RedesignApplication>();
                        Check(app && !app.runOnStart && app.router.driver.Session == null, "Saved preview has an unbound native Main owner");
                        cloud = new FixtureCloud(); var data = RedesignRuntimeContent.Load();
                        var state = RedesignState.Fresh(cloud.Now); state.cash = 2000; state.products[0] = 1; state.reducedMotion = true;
                        foreach (var topic in RedesignGuidance.Topics) state.learnedTopics.Add(topic);
                        cloud.CreateAsync(new AccountProfile { uid = "account-ui-fixture", displayName = "Fixture" }, SavePayload.Create(JsonUtility.ToJson(state))).GetAwaiter().GetResult();
                        var owned = cloud.AcquireAsync("account-ui-fixture", "device-a").GetAwaiter().GetResult();
                        checkpoint = CheckpointStore.ForRedesign(SessionState.GetString(Key + ".profile", ""), "account-ui-fixture", JsonUtility.ToJson, JsonUtility.FromJson<AccountCheckpoint>, JsonUtility.FromJson<SaveVersionHeader>);
                        account = new RedesignAccountSession(cloud, checkpoint, "account-ui-fixture", "device-a", data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, () => cloud.Now);
                        account.InitializeAsync(owned).GetAwaiter().GetResult(); app.InitializeAccount(account);
                        Check(app.Ready && app.router.Session == app.router.driver.Session && app.router.driver.IsAccount && !app.router.driver.IsDisposableGuest, "Main binds the owned account without enabling guest test grants");
                        Check(Cloud.PlayerSessionHost.Instance == null && RedesignEnvironment.Isolated, "Native fixture constructs no legacy/Firebase session host and preserves isolation");
                        instances = app.GetComponentsInChildren<Transform>(true); original = app.router.Session;
                        oldContext = (RedesignUiContext)typeof(RedesignBatchScreen).GetField("context", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app.router.batch);
                        deadline = EditorApplication.timeSinceStartup + .5; break;
                    case 1:
                        if (app.router.CurrentRoute == "offline") Click(app.router.offline.close);
                        Click(app.retained.settingsEntry);
                        deadline = EditorApplication.timeSinceStartup + .3; break;
                    case 2:
                        Check(!app.retained.accountStatus.text.Contains("guest") && !app.router.driver.TapGameplaySurface(), "Settings reports account status and blocks leaked gameplay taps");
                        Click(app.retained.settingsClose);
                        Check(app.router.driver.TapGameplaySurface(), "Owned account accepts a foreground gameplay tap");
                        taps = (long)account.State.manualTaps.Value;
                        cloud.Online = false; account.Tick(false);
                        Check(!app.Ready && app.retained.Page == "recovery" && app.retained.recoveryMessage.text.Contains("connection"), "Connection loss shows the actual account recovery reason");
                        Check(!oldContext.CanMutate() && !oldContext.Execute(e => e.Tap()), "Blocked account cannot mutate through an existing screen context");
                        deadline = EditorApplication.timeSinceStartup + .3; break;
                    case 3:
                        Click(app.retained.recoveryRetry);
                        Check(!app.Ready && app.router.Session == original && !app.router.driver.Recovering, "Offline Retry preserves the blocked binding and does not create a guest save");
                        cloud.Online = true; cloud.Offset += 60; cloud.HoldHeartbeat = new TaskCompletionSource<CloudRecord>();
                        Click(app.retained.recoveryRetry); Click(app.retained.recoveryRetry);
                        Check(app.router.driver.Recovering && cloud.HeartbeatCalls == 2 && !app.Ready, "Repeated Retry shares one in-flight renewal and keeps gameplay closed");
                        cloud.CompleteHeartbeat(); deadline = EditorApplication.timeSinceStartup + .5; break;
                    case 4:
                        Check(app.Ready && app.router.Session != original && app.router.Session == app.router.driver.Session, "Confirmed retry rebinds the saved screens to the recovered account engine");
                        Check(!original.CanMutate && !oldContext.CanMutate() && !oldContext.Execute(e => e.BuyLevels(0, 1)), "Old lifecycle and screen callbacks stay fenced after recovery");
                        Check(account.State.manualTaps.Value == taps && !checkpoint.Load(account.Uid).pending, "Pending account command survives reconnect exactly once and checkpoint acknowledgment is durable");
                        receipt = account.State.offline.id;
                        Check(app.router.CurrentRoute == "offline" && account.State.offline.pending && account.State.offline.creditedSeconds >= 59, "Recovered Main displays its already-saved offline receipt");
                        Click(app.router.offline.close);
                        Check(!account.State.offline.pending, "Native acknowledgment closes the recovered receipt once");
                        app.router.Navigate("kajushop", "");
                        Check(app.router.shop.packs.All(b => !b.interactable), "Account profile cannot use simulated guest pack grants");
                        app.router.Navigate("home", "");
                        app.router.driver.SendMessage("OnApplicationPause", true);
                        Check(!account.CanPlay && app.router.Session.Suspended, "Backgrounding saves and blocks account ownership until renewed");
                        cloud.Offset += 60; app.router.driver.SendMessage("OnApplicationPause", false);
                        Check(!app.Ready && app.retained.Page == "recovery", "Foreground return cannot reopen an unconfirmed account session");
                        deadline = EditorApplication.timeSinceStartup + .3; break;
                    case 5:
                        Click(app.retained.recoveryRetry); deadline = EditorApplication.timeSinceStartup + .5; break;
                    case 6:
                        Check(app.Ready && app.router.CurrentRoute == "offline" && account.State.offline.id != receipt, "Background retry binds the new server-timed absence receipt");
                        Click(app.router.offline.close); account.Save(true); account.Tick(true);
                        cloud.Offset += 31; cloud.AcquireAsync(account.Uid, "device-b").GetAwaiter().GetResult();
                        Check(!app.Ready && app.retained.Page == "recovery" && app.retained.recoveryMessage.text.Contains("another device"), "Device takeover reaches native recovery and stops foreground play");
                        generation = cloud.Record.generation; revision = cloud.Record.revision;
                        deadline = EditorApplication.timeSinceStartup + .3; break;
                    case 7:
                        cloud.Offset += 60; Click(app.retained.recoveryRetry);
                        Check(!app.Ready && cloud.Record.owner == "device-b" && cloud.Record.generation == generation && cloud.Record.revision == revision, "Native Retry cannot take over or rewrite the other device even after expiry");
                        Check(instances.SequenceEqual(app.GetComponentsInChildren<Transform>(true)), "Recovery and rebind retain every saved UI object identity");
                        Check(errors.Count == 0, "No runtime errors or exceptions during account recovery");
                        Finish(null); break;
                }
            }
            catch (Exception error) { Finish(error); }
        }
        static void Finish(Exception error)
        {
            EditorApplication.update -= Tick;
            if (error != null) checks.Add("FAIL " + error);
            checks.AddRange(errors.Select(e => "ERROR " + e));
            File.WriteAllLines(Report, checks);
            if (error == null) Debug.Log("REDESIGN_ACCOUNT_QA_OK " + checks.Count); else Debug.LogError(error);
            EditorApplication.ExitPlaymode();
        }
        internal sealed class FixtureCloud : ICloudSaveStore
        {
            public bool Online = true;
            public double Offset;
            public double Now => 1000 + Time.realtimeSinceStartupAsDouble + Offset;
            public CloudRecord Record;
            readonly Dictionary<string, CloudRecord> records = new Dictionary<string, CloudRecord>();
            public int HeartbeatCalls;
            public TaskCompletionSource<CloudRecord> HoldHeartbeat;
            event Action<CloudRecord> Changed;
            static T Copy<T>(T value) => JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
            void Connected() { if (!Online) throw new IOException("Fixture offline"); }
            void Notify() => Changed?.Invoke(Copy(Record));
            void Select(string uid) { records.TryGetValue(uid, out Record); }
            public Task<CloudRecord> ReadAsync(string uid) { Connected(); Select(uid); return Task.FromResult(Record == null ? null : Copy(Record)); }
            public Task<CloudRecord> CreateAsync(AccountProfile profile, SavePayload save)
            {
                Connected(); Select(profile.uid); if (Record != null) throw new CloudConflictException("Exists");
                Record = new CloudRecord { profile = profile, save = save, revision = 1, commitId = Guid.NewGuid().ToString("N"), createdAt = Now, updatedAt = Now, heartbeatAt = Now };
                records[profile.uid] = Record;
                return ReadAsync(profile.uid);
            }
            public Task<CloudRecord> AcquireAsync(string uid, string owner)
            {
                Connected(); Select(uid); if (Record.owner != owner && Record.owner != "" && Now - Record.heartbeatAt < 30) throw new CloudConflictException("Lease active");
                if (Record.owner != owner) { Record.owner = owner; Record.generation++; Record.handoff = ""; }
                Record.heartbeatAt = Now; Notify(); return ReadAsync(uid);
            }
            public Task<CloudRecord> HeartbeatAsync(string uid, string owner, long generation)
            {
                HeartbeatCalls++; Connected(); Select(uid);
                if (!Record.OwnedBy(owner, generation) || Record.handoff != "") throw new CloudConflictException("Your game is active on another device.");
                if (HoldHeartbeat != null) return HoldHeartbeat.Task;
                Record.heartbeatAt = Now; Notify(); return ReadAsync(uid);
            }
            public void CompleteHeartbeat()
            {
                var held = HoldHeartbeat; HoldHeartbeat = null; Record.heartbeatAt = Now; Notify(); held.SetResult(Copy(Record));
            }
            public Task<CloudRecord> CommitAsync(AccountCheckpoint value, bool release = false)
            {
                Connected(); Select(value.uid); if (value.IsAcknowledged(Record)) return ReadAsync(value.uid);
                if (!Record.OwnedBy(value.owner, value.generation) || value.baseRevision != Record.revision || Now - Record.heartbeatAt >= 30) throw new CloudConflictException("Stale checkpoint");
                Record.save = Copy(value.save); Record.revision++; Record.commitId = value.commitId; Record.updatedAt = Record.heartbeatAt = Now;
                if (release) Record.owner = ""; Notify(); return ReadAsync(value.uid);
            }
            public Task RequestHandoffAsync(string uid, string target) { Connected(); Select(uid); Record.handoff = target; Notify(); return Task.CompletedTask; }
            public Task ReleaseAsync(string uid, string owner, long generation)
            {
                Connected(); Select(uid); if (!Record.OwnedBy(owner, generation)) throw new CloudConflictException("Lost owner"); Record.owner = ""; Notify(); return Task.CompletedTask;
            }
            public IDisposable Listen(string uid, Action<CloudRecord> changed, Action<Exception> failed)
            { void Filter(CloudRecord value) { if (value.profile.uid == uid) changed(value); } Changed += Filter; return new Listener(() => Changed -= Filter); }
            sealed class Listener : IDisposable { readonly Action stop; public Listener(Action stop) { this.stop = stop; } public void Dispose() => stop(); }
            public void Dispose() { }
        }
    }
}
