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
    public static class RedesignShopPlayValidation
    {
        const string Key = "RocketRaja.RedesignShopQA", Report = "Documentation/Implementation/Unity-Shop-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        [Serializable] sealed class SavedChecks { public string[] lines; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>(), tooling = new List<string>();
        static RedesignScreenRouter router;
        static Transform[] instances;
        static int step, boundFrame;
        static double deadline, frameDeadline;
        static RedesignShopPlayValidation()
        {
            EditorApplication.playModeStateChanged += Mode;
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (!SessionState.GetBool(Key + ".running", false) || type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
                const string advisory = "The project path contains spaces, which may cause issues during usage of AI Game Developer. Please consider the move the project to a folder without spaces.";
                if (message == advisory && trace.Contains("Startup")) tooling.Add(message); else errors.Add(message);
            };
        }
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Preserve dirty scenes before QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = setup.Select(s => new Record { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            string profile = Path.GetFullPath("Temp/RedesignShopQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(profile); SessionState.SetString(Key + ".profile", profile);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc); state.completedPrestiges = state.fame = 1; state.target = ProgressionNumbers.RunTarget(state.completedPrestiges);
            Store().Save(state);
            SessionState.SetString(Key + ".inventory", RedesignCataloguePlayValidation.Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            SessionState.SetInt(Key + ".phase", 0); SessionState.SetString(Key + ".checks", ""); SessionState.SetBool(Key + ".restart", false); SessionState.SetBool(Key + ".running", true);
            File.WriteAllText(Report, "RUNNING: native test shop, two Play Mode sessions, disposable profile.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static RedesignSaveStore Store() => new RedesignSaveStore(Path.Combine(SessionState.GetString(Key + ".profile", ""), "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key + ".running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); var saved = JsonUtility.FromJson<SavedChecks>(SessionState.GetString(Key + ".checks", "{}")); if (saved?.lines != null) checks.AddRange(saved.lines);
                step = -1; deadline = EditorApplication.timeSinceStartup + .5; EditorApplication.update -= Tick; EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick;
                if (SessionState.GetBool(Key + ".restart", false))
                {
                    SessionState.SetBool(Key + ".restart", false); SessionState.SetInt(Key + ".phase", 1); EditorApplication.delayCall += () => EditorApplication.EnterPlaymode();
                }
                else
                {
                    SessionState.SetBool(Key + ".running", false); var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
                    EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray()); Debug.Log("REDESIGN_SHOP_QA_RESTORED");
                }
            }
        }
        static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button button)
        {
            Check(button && button.IsActive() && button.IsInteractable(), "Active control: " + (button ? button.name : "missing"));
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        static void Touch(Button button)
        {
            Canvas.ForceUpdateCanvases(); var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Check(pointer.position.x >= 0 && pointer.position.x <= Screen.width && pointer.position.y >= 0 && pointer.position.y <= Screen.height && hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
                "Physical shop touch reaches topmost pack control; screen=" + Screen.width + "x" + Screen.height + "; point=" + pointer.position);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                if (step == -1)
                {
                    router = UnityEngine.Object.FindAnyObjectByType<RedesignScreenRouter>();
                    Check(router && !router.initializeGuestOnStart && SceneManager.GetActiveScene().path == BuildRedesignUI.PreviewPath, "Only the saved preview and explicit disposable profile are used");
                    Check(RedesignCataloguePlayValidation.Inventory(router.gameObject) == SessionState.GetString(Key + ".inventory", ""), "Startup preserves every saved UI object and component");
                    bool loaded = router.driver.InitializeGuest(SessionState.GetString(Key + ".profile", ""));
                    Check(loaded && router.driver.IsDisposableGuest, "Actual driver loads the isolated test guest; load=" + router.driver.LoadResult.status + "; save=" + router.driver.Session?.LastSaveException);
                    router.Bind(router.driver.Session); router.Bind(router.driver.Session); router.Bind(router.driver.Session); instances = router.GetComponentsInChildren<Transform>(true);
                    Click(router.talentTab); Click(router.talent.shop);
                    Check(router.CurrentRoute == "kajushop" && !router.driver.TapGameplaySurface(), "Talent opens the test shop with gameplay taps blocked");
                    boundFrame = Time.frameCount; frameDeadline = EditorApplication.timeSinceStartup + 15; deadline = EditorApplication.timeSinceStartup + .1; step = 0; return;
                }
                if (Time.frameCount <= boundFrame + 1)
                {
                    if (EditorApplication.timeSinceStartup > frameDeadline) throw new InvalidOperationException("Player layout did not render before touch test");
                    return;
                }
                var ui = router.shop; var e = router.Session.Engine;
                if (SessionState.GetInt(Key + ".phase", 0) == 0)
                {
                    Check(!e.State.HasDevelopmentShop && e.State.kajuKatli.IsZero, "Opening the shop never grants or marks a purchase");
                    Touch(ui.packs[0]); string operation = ui.PendingOperation; SessionState.SetString(Key + ".operation", operation);
                    Check(!string.IsNullOrEmpty(operation) && ui.pendingPanel.activeSelf && ui.packs.All(p => !p.interactable) && e.State.kajuKatli.IsZero, "Native purchase persists pending without delivering KK");
                    ui.packs[0].onClick.Invoke(); Check(ui.PendingOperation == operation, "Repeated native callback retains one pending operation");
                    Click(ui.leavePending); Click(router.talent.shop);
                    Check(ui.PendingOperation == operation && e.State.kajuKatli.IsZero, "Leaving and reopening keeps the same pending purchase");
                    var saved = Store().Load().state; Check(saved.developmentShop.operationId == operation && saved.developmentShop.status == "pending" && saved.kajuKatli.IsZero, "On-disk purchase is pending before application interruption");
                    Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Purchase state changes create no UI objects");
                    Check(errors.Count == 0, "No gameplay/runtime errors in first session: " + string.Join("; ", errors.Take(3))); checks.Add("NOTE Known MCP path advisory count: " + tooling.Count);
                    SessionState.SetString(Key + ".checks", JsonUtility.ToJson(new SavedChecks { lines = checks.ToArray() })); SessionState.SetBool(Key + ".restart", true); Finish();
                }
                else
                {
                    Check(ui.PendingOperation == SessionState.GetString(Key + ".operation", "") && e.State.kajuKatli.IsZero && ui.pendingPanel.activeSelf, "Second Play Mode session recovers the same undelivered purchase from disk");
                    ulong total = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (i > 0) Click(ui.packs[i]); Click(ui.delivered); total += (ulong)e.Data.Packs[i].kajuKatli;
                        Check(e.State.kajuKatli.Value == total && e.State.developmentShop.status == "delivered", "Native success delivers exact pack: " + e.Data.Packs[i].id);
                        ui.delivered.onClick.Invoke(); Check(e.State.kajuKatli.Value == total, "Duplicate success callback cannot redeliver pack " + i);
                    }
                    Check(total == 48000 && e.State.cardLevels.All(n => n.IsZero) && !e.State.reveal.pending, "All four packs total 48000 KK and never auto-draw");
                    Click(ui.packs[0]); Click(ui.cancelled); Check(e.State.developmentShop.status == "cancelled" && e.State.kajuKatli.Value == total, "Native cancellation grants nothing");
                    Click(ui.packs[1]); Click(ui.failed); Check(e.State.developmentShop.status == "failed" && e.State.kajuKatli.Value == total, "Native failure grants nothing");
                    Click(ui.testStates); Click(ui.loading); Check(ui.packs.All(p => !p.interactable), "Native loading simulation disables pack controls");
                    Click(ui.unavailable); Check(ui.packs.All(p => !p.interactable), "Native unavailable simulation disables pack controls"); Click(ui.ready);
                    Click(ui.back); Click(router.talent.paidDraw);
                    Check(router.CurrentRoute == "drawreveal" && e.State.kajuKatli.Value == total - 100 && e.State.cardLevels.Sum(n => (int)n.Value) == 1, "Explicit Talent action spends exactly 100 simulated KK for one actual card");
                    router.reveal.Settle(); Click(router.reveal.done);
                    Check(Store().Load().state.kajuKatli.Value == total - 100 && !Store().Load().state.reveal.pending, "Delivered packs and later spend remain consistent on disk");
                    Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Shop, outcomes and draw navigation retain actual saved UI identities");
                    Check(errors.Count == 0, "No gameplay/runtime errors in recovered session: " + string.Join("; ", errors.Take(3))); checks.Add("NOTE Known MCP path advisory count: " + tooling.Count);
                    File.WriteAllLines(Report, new[] { "Scope: native saved preview, two Play Mode sessions and one disposable profile. Simulated shop only; no Google Play, network or charge. Main/device acceptance pending." }.Concat(checks));
                    Debug.Log("REDESIGN_SHOP_PLAY_OK " + checks.Count); Finish();
                }
            }
            catch (Exception error) { File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + error })); SessionState.SetBool(Key + ".restart", false); Debug.LogError("REDESIGN_SHOP_PLAY_FAIL " + error); Finish(); }
        }
        static void Finish() { EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); }
    }
}
