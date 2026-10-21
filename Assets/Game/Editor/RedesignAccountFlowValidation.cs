using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RocketRaja.Cloud;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class RedesignAccountFlowValidation
    {
        const string Key = "RocketRaja.RedesignAccountFlowQA", Report = "Documentation/Implementation/Unity-Account-Flow.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>();
        static RedesignAccountPlayValidation.FixtureCloud cloud;
        static FixtureAuth auth;
        static RedesignSessionHost host;
        static RedesignApplication app;
        static BootstrapController bootstrap;
        static RedesignEngine stale;
        static string root;
        static byte[] guestBytes;
        static int step;
        static bool menuCaptured;
        static double deadline, sceneDeadline;
        static RedesignAccountFlowValidation() => EditorApplication.playModeStateChanged += Mode;
        [MenuItem("Rocket Raja/Redesign/Test Account Scene Flow")]
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Preserve dirty scenes before QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = setup.Select(s => new Record { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            SessionState.SetString(Key + ".oldGuest", SessionState.GetString("RocketRaja.RedesignMainQA.profile", "")); SessionState.EraseString("RocketRaja.RedesignMainQA.profile");
            SessionState.SetBool(Key, true); File.WriteAllText(Report, "RUNNING: real saved Bootstrap/Main scenes; fixture identity/cloud; isolated guest/checkpoint files. No live authentication.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (mode == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); errors.Clear(); step = 0; menuCaptured = false; deadline = EditorApplication.timeSinceStartup + .5;
                Application.logMessageReceived += Log; EditorApplication.update += Tick;
            }
            else if (mode == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
                SessionState.SetBool(Key, false); SessionState.SetString("RocketRaja.RedesignMainQA.profile", SessionState.GetString(Key + ".oldGuest", ""));
                var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
                EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
                Debug.Log("REDESIGN_ACCOUNT_FLOW_RESTORED");
            }
        }
        static void Log(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (message.StartsWith("The project path contains spaces") && trace.Contains("Startup")) return;
            errors.Add(message);
        }
        static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static T Field<T>(string name) => (T)typeof(BootstrapController).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bootstrap);
        static string Heading => Field<TMP_Text>("heading").text;
        static void Click(Button button)
        {
            Check(button && button.IsActive() && button.IsInteractable(), "Active native control: " + (button ? button.name : "missing"));
            Canvas.ForceUpdateCanvases(); var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
                "Topmost native hit: " + button.name + "; point=" + pointer.position + "; hits=" + string.Join(",", hits.Take(3).Select(h => h.gameObject.name)));
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        static void Primary() => Click(Field<Button>("primaryButton"));
        static void Secondary() => Click(Field<Button>("secondaryButton"));
        static void Tertiary() => Click(Field<Button>("tertiaryButton"));
        static void Next(double delay = .35) { step++; deadline = EditorApplication.timeSinceStartup + delay; sceneDeadline = deadline + 30; }
        static bool Scene(string name)
        {
            if (SceneManager.GetActiveScene().name != name || name == "Main" && (!app || !app.Ready))
            {
                app = UnityEngine.Object.FindAnyObjectByType<RedesignApplication>();
                if (EditorApplication.timeSinceStartup > sceneDeadline) throw new InvalidOperationException("Timed out entering " + name + "; active=" + SceneManager.GetActiveScene().name + "; error=" + app?.router.driver.RecoveryNotice);
                return false;
            }
            if (name == "Bootstrap") bootstrap = UnityEngine.Object.FindAnyObjectByType<BootstrapController>();
            return true;
        }
        static void Settings()
        {
            if (app.router.CurrentRoute == "offline") Click(app.router.offline.close);
            Click(app.retained.settingsEntry);
            app.retained.settings.GetComponentInChildren<ScrollRect>(true).verticalNormalizedPosition = 0;
        }
        static void Capture(string name) => ScreenCapture.CaptureScreenshot(Path.GetFullPath("Documentation/Implementation/NativeUI/account-flow-" + name + ".png"));
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                switch (step)
                {
                    case 0:
                        root = Path.GetFullPath("Temp/RedesignAccountFlow-" + Guid.NewGuid().ToString("N"));
                        cloud = new RedesignAccountPlayValidation.FixtureCloud(); auth = new FixtureAuth(); var data = RedesignRuntimeContent.Load();
                        var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc); state.cash = 2000; state.products[0] = 1; state.manualTaps = 3; state.reducedMotion = true;
                        state.learnedTopics.AddRange(RedesignGuidance.Topics);
                        var store = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
                        store.Save(state); guestBytes = File.ReadAllBytes(Path.Combine(root, "rocket-raja-v2.save"));
                        var coordinator = new RedesignAccountCoordinator(auth, cloud, data, root, "device-a", JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>,
                            JsonUtility.FromJson<SaveVersionHeader>, JsonUtility.ToJson, JsonUtility.FromJson<AccountCheckpoint>, () => cloud.Now, () => RedesignRuntimeDriver.Utc);
                        host = RedesignSessionHost.Ensure(root); host.ConfigureFixture(coordinator, root);
                        SceneManager.LoadScene("Bootstrap"); Next(.7); break;
                    case 1:
                        if (!Scene("Bootstrap")) return;
                        Check(Heading == "Welcome to your stall" && PlayerSessionHost.Instance == null, "Saved Bootstrap uses the redesigned flow without constructing the legacy host");
                        auth.Cancel = true; Primary();
                        Check(Heading == "Welcome to your stall" && Field<TMP_Text>("description").text.Contains("did not finish"), "Cancelled sign-in returns to the native welcome state without losing the guest");
                        auth.Cancel = false; Next(); break;
                    case 2:
                        Primary();
                        Check(Heading == "Save this game with Google?" && cloud.Record == null, "New-account choice is explicit before any cloud document is created");
                        Capture("promote"); Next(); break;
                    case 3:
                        Primary(); Next(.7); break;
                    case 4:
                        if (!Scene("Main")) return;
                        Check(app.router.driver.IsAccount && app.router.Session.Engine.State.manualTaps.Value == 3 && app.router.Session.Engine.State.products[0] == 1,
                            "Native confirmation opens saved Main on the promoted account session");
                        Check(guestBytes.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "rocket-raja-v2.save"))), "Scene transition and promotion preserve the original guest bytes");
                        Check(!app.router.driver.IsDisposableGuest, "Account Main cannot use the local purchase simulator");
                        Settings(); Next(); break;
                    case 5:
                        Check(app.retained.accountHeading.text == "Google account" && app.retained.privacyDescription.text.Contains("Firebase") && !app.retained.privacyDescription.text.Contains("unavailable"),
                            "Configured account flow uses accurate Settings and Privacy copy on existing text objects");
                        Click(app.retained.account); Next(.7); break;
                    case 6:
                        if (!Scene("Bootstrap")) return;
                        if (!menuCaptured)
                        {
                            Check(Heading == "Your Google account" && !host.Account.CanPlay, "Settings opens the saved account menu and pauses its old gameplay binding");
                            Capture("menu"); menuCaptured = true; deadline = EditorApplication.timeSinceStartup + .35; return;
                        }
                        Primary(); Next(.7); break;
                    case 7:
                        if (!Scene("Main")) return;
                        Check(!app.router.driver.IsAccount && host.Account == null && app.router.Session.Engine.State.manualTaps.Value == 3,
                            "Native sign-out returns to the original guest with its progress intact");
                        Check(cloud.Record.owner == "", "Sign-out releases the confirmed account lease");
                        Settings(); Next(); break;
                    case 8:
                        Click(app.retained.account); Next(.7); break;
                    case 9:
                        if (!Scene("Bootstrap")) return;
                        Primary(); Check(Heading == "Choose your progress" && Field<TMP_Text>("secondaryLabel").text == "Keep device progress" && Field<Button>("tertiaryButton").gameObject.activeSelf,
                            "Existing Google comparison offers Google progress, explicit device replacement and staying as guest");
                        Capture("compare"); Next(); break;
                    case 10:
                        Primary(); Next(.7); break;
                    case 11:
                        if (!Scene("Main")) return;
                        Check(app.router.driver.IsAccount && app.router.Session.Engine.State.manualTaps.Value == 3, "Existing Google progress restores through Bootstrap into Main");
                        cloud.Online = false; host.Account.Tick(false); Next(); break;
                    case 12:
                        Check(app.retained.Page == "recovery" && app.retained.recoveryExit.GetComponentInChildren<TMP_Text>().text == "Account options",
                            "Disconnected account exposes an account-options route in native recovery");
                        Click(app.retained.recoveryExit); Next(.7); break;
                    case 13:
                        if (!Scene("Bootstrap")) return;
                        Check(Heading == "Your Google account", "Recovery reaches account options without clearing the pending account files");
                        Tertiary(); Next(.7); break;
                    case 14:
                        if (!Scene("Main")) return;
                        Check(!app.router.driver.IsAccount && app.router.Session.Engine.State.manualTaps.Value == 3, "Offline return-to-guest remains playable without pretending the account upload succeeded");
                        cloud.Online = true; auth.NextUid = "account-flow-b"; Settings(); Next(); break;
                    case 15:
                        Click(app.retained.account); Next(.7); break;
                    case 16:
                        if (!Scene("Bootstrap")) return;
                        Primary(); Check(Heading == "Save this game with Google?", "A different Google UID receives its own new-account choice"); Next(); break;
                    case 17:
                        Secondary(); Next(.7); break;
                    case 18:
                        if (!Scene("Main")) return;
                        Check(host.Account.Uid == "account-flow-b" && app.router.Session.Engine.State.products.All(n => n == 0) && app.router.Session.Engine.State.manualTaps.IsZero,
                            "Choosing a fresh second account does not import the guest or first account");
                        Check(cloud.ReadAsync("account-flow-a").GetAwaiter().GetResult().save != null, "First Google account remains present after switching UID");
                        stale = app.router.Session.Engine; cloud.Offset += 31; cloud.AcquireAsync("account-flow-b", "device-b").GetAwaiter().GetResult(); Next(); break;
                    case 19:
                        Check(!app.Ready && app.retained.Page == "recovery" && !stale.Tap(), "Other-device takeover blocks the second account's native Main");
                        Click(app.retained.recoveryExit); Next(.7); break;
                    case 20:
                        if (!Scene("Bootstrap")) return;
                        Tertiary(); Next(.7); break;
                    case 21:
                        if (!Scene("Main")) return;
                        Settings(); Next(); break;
                    case 22:
                        Click(app.retained.account); Next(.7); break;
                    case 23:
                        if (!Scene("Bootstrap")) return;
                        Primary(); Next(); break;
                    case 24:
                        Primary(); Check(Heading == "Playing on another device", "Existing remote owner requires an explicit Play here confirmation");
                        Capture("handoff"); cloud.Offset += 31; Next(); break;
                    case 25:
                        Primary(); Next(.7); break;
                    case 26:
                        if (!Scene("Main")) return;
                        Check(host.Account.Uid == "account-flow-b" && cloud.Record.owner == "device-a" && host.Account.CanPlay && !stale.Tap(),
                            "Confirmed handoff enters Main under the new generation while the old engine stays fenced");
                        Check(PlayerSessionHost.Instance == null && RedesignEnvironment.Isolated, "All scene routes retain the redesign namespace and legacy/live guard");
                        Check(errors.Count == 0, "No runtime errors during complete account/guest/UID/handoff scene routes");
                        Finish(null); break;
                }
            }
            catch (Exception error) { Finish(error); }
        }
        static void Finish(Exception error)
        {
            EditorApplication.update -= Tick;
            if (error != null) checks.Add("FAIL " + error);
            checks.AddRange(errors.Select(e => "ERROR " + e)); File.WriteAllLines(Report, checks);
            if (error == null) Debug.Log("REDESIGN_ACCOUNT_FLOW_OK " + checks.Count); else Debug.LogError(error);
            EditorApplication.ExitPlaymode();
        }
        sealed class FixtureAuth : IAccountAuth
        {
            public AccountProfile Current { get; private set; }
            public string NextUid = "account-flow-a";
            public bool Cancel;
            public event Action Changed;
            public Task InitializeAsync() => Task.CompletedTask;
            public Task<AccountProfile> SignInAsync()
            {
                if (Cancel) throw new OperationCanceledException("Fixture cancellation");
                Current = new AccountProfile { uid = NextUid, displayName = "Fixture player", email = "", photoUrl = "" }; Changed?.Invoke(); return Task.FromResult(Current);
            }
            public Task SignOutAsync() { Current = null; Changed?.Invoke(); return Task.CompletedTask; }
            public void Dispose() { }
        }
    }
}
