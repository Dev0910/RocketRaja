using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class RedesignMainPlayValidation
    {
        const string Key = "RocketRaja.RedesignMainQA", Report = "Documentation/Implementation/Unity-Main-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>(), tooling = new List<string>();
        static RedesignApplication app;
        static Transform[] instances;
        static int step;
        static double deadline, clock, startupDeadline;
        static Amount cash;
        static RedesignMainPlayValidation()
        {
            EditorApplication.playModeStateChanged += Mode;
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (!SessionState.GetBool(Key, false) || type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
                const string advisory = "The project path contains spaces, which may cause issues during usage of AI Game Developer. Please consider the move the project to a folder without spaces.";
                if (message == advisory && trace.Contains("Startup")) tooling.Add(message); else errors.Add(message);
            };
        }
        static RedesignSaveStore Store() => new RedesignSaveStore(Path.Combine(SessionState.GetString(Key + ".profile", ""), "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Preserve dirty scenes before QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = setup.Select(s => new Record { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            string profile = Path.GetFullPath("Temp/RedesignMainQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(profile); SessionState.SetString(Key + ".profile", profile);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc - 36000); state.cash = 100000; state.products[0] = 1; state.eligibleEarnings = state.target;
            new RedesignEngine(RedesignRuntimeContent.Load(), state, Store().Save).Save();
            SessionState.SetString(Key + ".inventory", RedesignCataloguePlayValidation.Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            SessionState.SetInt(Key + ".phase", 0); SessionState.SetBool(Key + ".restart", false); SessionState.SetBool(Key, true);
            File.WriteAllText(Report, "RUNNING: saved Bootstrap to Main with a disposable profile.\n");
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                app = null; step = 0; checks.Clear(); deadline = EditorApplication.timeSinceStartup + .7; startupDeadline = deadline + 30;
                if (SessionState.GetInt(Key + ".phase", 0) == 1) checks.AddRange(File.ReadAllLines(Report));
                EditorApplication.update -= Tick; EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick;
                if (SessionState.GetBool(Key + ".restart", false))
                {
                    SessionState.SetBool(Key + ".restart", false); SessionState.SetInt(Key + ".phase", 1);
                    EditorApplication.delayCall += () => { EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single); EditorApplication.EnterPlaymode(); }; return;
                }
                SessionState.SetBool(Key, false); SessionState.EraseString(Key + ".profile");
                var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
                EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
                Debug.Log("REDESIGN_MAIN_QA_RESTORED");
            }
        }
        static void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button button, bool physical = false)
        {
            Check(button && button.IsActive() && button.IsInteractable(), "Active control: " + button.name);
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            if (physical)
            {
                Canvas.ForceUpdateCanvases(); var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
                Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject, "Physical topmost hit: " + button.name);
            }
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                if (!app)
                {
                    var candidate = UnityEngine.Object.FindAnyObjectByType<RedesignApplication>();
                    if ((!candidate || !candidate.Ready || Time.frameCount < 3 || SceneManager.GetActiveScene().path != "Assets/Scenes/Main.unity") && EditorApplication.timeSinceStartup < startupDeadline) return;
                    app = candidate;
                    Check(app && app.Ready && SceneManager.GetActiveScene().path == "Assets/Scenes/Main.unity", "Saved Bootstrap opens the initialized redesign Main; scene=" + SceneManager.GetActiveScene().name + "; recovery=" + app?.router.driver.Session?.SaveFailure);
                    Check(Cloud.PlayerSessionHost.Instance == null, "Isolated startup never constructs the legacy/cloud session host");
                    var game = UnityEngine.Object.FindAnyObjectByType<GameController>();
                    Check(game && !game.enabled && game.Engine == null && game.redesignApplication == app, "GameController delegates without running old economy calculations");
                    Check(RedesignCataloguePlayValidation.Inventory(app.gameObject) == SessionState.GetString(Key + ".inventory", ""), "Main starts with the exact saved UI object/component inventory");
                    instances = app.GetComponentsInChildren<Transform>(true);
                    deadline = EditorApplication.timeSinceStartup + .15; return;
                }
                var r = app.router; var e = r.Session.Engine; var ui = app.retained;
                if (SessionState.GetInt(Key + ".phase", 0) == 1)
                {
                    Check(r.CurrentRoute == "drawreveal" && e.State.reveal.pending && r.reveal.OperationId == SessionState.GetString(Key + ".reveal", ""), "Main relaunch prioritizes the exact durable reveal without another draw");
                    Check(!e.State.music && !e.State.sound && e.State.reducedMotion && e.State.storeName == "<b>Raja & Co</b>", "Settings and literal store name survive actual Main relaunch");
                    Check(r.reveal.Settled && !r.reveal.skip.interactable, "Persisted reduced motion settles the restored reveal immediately");
                    Click(r.reveal.done); Click(r.homeTab);
                    Check(r.CurrentRoute == "home" && !e.State.reveal.pending && e.State.freeCredits.Value == 2, "Completing the restored reveal spends no additional credit");
                    Check(e.State.products.All(p => p == 0) && r.home.firstBuy.interactable, "Rebuild reaches Home with a manual first purchase still required");
                    Click(r.home.firstBuy); Check(e.State.products[0] == 1 && e.State.manualTaps.IsZero, "Rebuilt first purchase works through Main without an input leak");
                    Finish(false); return;
                }
                switch (step++)
                {
                    case 0:
                        var receipt = e.State.offline;
                        Check(r.CurrentRoute == "offline" && receipt.pending && receipt.creditedSeconds == 28800 && receipt.amount > Amount.Zero && !r.Session.GameplayInputEnabled, "Main settles the capped absence before showing the return page or enabling input");
                        Check(Store().Load().state.offline.id == receipt.id, "Main startup receipt is already persisted");
                        Click(r.offline.close); Check(r.CurrentRoute == "home" && !e.State.offline.pending, "Return close acknowledges once and reaches Home");
                        Click(ui.settingsEntry, true); cash = e.State.cash; clock = e.State.now;
                        Check(ui.Page == "settings" && !r.Session.Suspended && !r.driver.TapGameplaySurface(), "Settings blocks gameplay input without suspending simulation");
                        Click(r.talentTab); Check(r.CurrentRoute == "home", "Late background navigation cannot bypass a utility overlay");
                        deadline = EditorApplication.timeSinceStartup + .6; break;
                    case 1:
                        Check(e.State.now > clock && e.State.cash > cash, "Actual Main frames keep production running while browsing Settings");
                        Click(ui.toggles[0], true); Click(ui.toggles[1]); Click(ui.toggles[2]);
                        Check(!e.State.music && !e.State.sound && e.State.reducedMotion, "Retained toggles execute durable new-economy commands");
                        ui.settings.GetComponentInChildren<ScrollRect>(true).verticalNormalizedPosition = 0;
                        deadline = EditorApplication.timeSinceStartup + .15; break;
                    case 2:
                        Click(ui.privacyEntry, true); Check(ui.Page == "privacy", "Settings preserves the Privacy route");
                        r.Navigate("back", ""); Check(ui.Page == "settings", "Back from Privacy returns to Settings");
                        Click(ui.rename); Check(ui.Page == "naming", "Retained naming dialog opens from Settings");
                        ui.nameInput.text = "<b>Raja & Co</b>"; Click(ui.nameSave);
                        Check(e.State.storeName == "<b>Raja & Co</b>" && !r.home.storeName.richText && !ui.storeName.richText, "Allowed punctuation is literal text, never formatting markup");
                        using (var locked = new FileStream(Path.Combine(SessionState.GetString(Key + ".profile", ""), "rocket-raja-v2.save"), FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            Click(ui.toggles[0]); Check(ui.Page == "recovery" && !app.Ready && !e.State.music && !r.driver.TapGameplaySurface(), "Failed real-file setting write opens recovery and publishes no toggle");
                            Click(ui.recoveryRetry); Check(ui.Page == "recovery" && !app.Ready, "Retry keeps the block while the storage failure remains");
                        }
                        Click(ui.recoveryRetry); Check(app.Ready && !ui.IsOpen && !e.State.music, "Retry recovers the same state when storage becomes writable");
                        r.driver.SendMessage("OnApplicationPause", true); cash = e.State.cash; r.Session.Tick(30);
                        Check(r.Session.Suspended && e.State.cash.CompareTo(cash) == 0 && !r.driver.Command(x => x.ToggleSetting("music")), "True app suspension blocks foreground time and commands");
                        r.driver.SendMessage("OnApplicationPause", false); r.driver.SendMessage("OnApplicationPause", false);
                        Check(!r.Session.Suspended && app.Ready, "Repeated resume callbacks restore input without a second settlement");
                        Click(r.prestigeTab); Check(r.prestige.ReviewedQuote.Eligible && e.State.products.Count(p => p > 0) == 1, "Main prestige eligibility has no all-eight-products gate");
                        var quote = r.prestige.ReviewedQuote; r.Session.Tick(2);
                        Check(e.CanCommitPrestige(quote), "Menu production preserves the reviewed prestige reward");
                        Click(r.prestige.confirm); Check(e.State.completedPrestiges.Value == 1 && e.State.freeCredits.Value == 3 && ProgressionNumbers.FameMultiplier(e.State.fame).CompareTo(new Amount(6)) == 0, "Main prestige commits the first x6 Fame income and three free credits");
                        Check(e.State.products.All(p => p == 0) && e.State.target.CompareTo(new Amount(1.5e10)) == 0, "Main rebuild resets products and freezes the next target");
                        deadline = EditorApplication.timeSinceStartup + .15; break;
                    case 3:
                        Click(r.talent.freeDraw);
                        Check(e.State.reveal.pending && r.CurrentRoute == "drawreveal", "Main Talent opens a durable free draw reveal");
                        SessionState.SetString(Key + ".reveal", e.State.reveal.operationId); Finish(true); break;
                }
            }
            catch (Exception error)
            {
                File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + error })); Debug.LogError("REDESIGN_MAIN_PLAY_FAIL " + error);
                SessionState.SetBool(Key + ".restart", false); EditorApplication.update -= Tick; EditorApplication.ExitPlaymode();
            }
        }
        static void Finish(bool restart)
        {
            Check(new HashSet<Transform>(instances).SetEquals(app.GetComponentsInChildren<Transform>(true)), "All Main routes preserve every saved UI object, including input caret and text material slots");
            Check(errors.Count == 0, "No gameplay/runtime errors: " + string.Join("; ", errors.Take(3)));
            checks.Add("NOTE Known MCP path advisory count: " + tooling.Count); File.WriteAllLines(Report, checks);
            SessionState.SetBool(Key + ".restart", restart); EditorApplication.update -= Tick; EditorApplication.ExitPlaymode();
        }
    }
}
