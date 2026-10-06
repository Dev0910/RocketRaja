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
    public static class RedesignReturnPlayValidation
    {
        const string Key = "RocketRaja.RedesignReturnQA", Report = "Documentation/Implementation/Unity-Return-Skills-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>(), tooling = new List<string>();
        static RedesignScreenRouter router;
        static Transform[] instances;
        static bool bound;
        static int frame;
        static double deadline;
        static RedesignReturnPlayValidation()
        {
            EditorApplication.playModeStateChanged += Mode;
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (!SessionState.GetBool(Key, false) || type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
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
            string profile = Path.GetFullPath("Temp/RedesignReturnQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(profile); SessionState.SetString(Key + ".profile", profile);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc - 36000); state.cash = 100000; state.products[0] = state.products[1] = state.products[2] = 1; state.pinnedUpgradeId = "PP-PHU-01";
            var e = new RedesignEngine(RedesignRuntimeContent.Load(), state, Store().Save); e.Save();
            SessionState.SetString(Key + ".inventory", RedesignCataloguePlayValidation.Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            SessionState.SetBool(Key, true); File.WriteAllText(Report, "RUNNING: native return/skill routes with a disposable saved profile.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static RedesignSaveStore Store() => new RedesignSaveStore(Path.Combine(SessionState.GetString(Key + ".profile", ""), "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) { bound = false; checks.Clear(); deadline = EditorApplication.timeSinceStartup + .5; EditorApplication.update -= Tick; EditorApplication.update += Tick; }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick; SessionState.SetBool(Key, false);
                var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
                EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray()); Debug.Log("REDESIGN_RETURN_QA_RESTORED");
            }
        }
        static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button button)
        {
            Check(button && button.IsActive() && button.IsInteractable(), "Active control: " + (button ? button.name : "missing"));
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                if (!bound)
                {
                    router = UnityEngine.Object.FindAnyObjectByType<RedesignScreenRouter>();
                    Check(router && !router.initializeGuestOnStart && SceneManager.GetActiveScene().path == BuildRedesignUI.PreviewPath, "Only the saved preview and explicit disposable profile are used");
                    Check(RedesignCataloguePlayValidation.Inventory(router.gameObject) == SessionState.GetString(Key + ".inventory", ""), "Startup preserves the whole authored UI inventory");
                    bool loaded = router.driver.InitializeGuest(SessionState.GetString(Key + ".profile", ""));
                    Check(loaded, "Driver loads and settles the real ten-hour absence; save error=" + router.driver.Session?.LastSaveException);
                    router.Bind(router.driver.Session); router.Bind(router.driver.Session); instances = router.GetComponentsInChildren<Transform>(true);
                    router.Navigate("offline", ""); router.offline.scroll.verticalNormalizedPosition = 0; frame = Time.frameCount; bound = true; deadline = EditorApplication.timeSinceStartup + .1; return;
                }
                if (Time.frameCount <= frame + 1) { if (EditorApplication.timeSinceStartup > deadline + 15) throw new InvalidOperationException("No player layout frames"); return; }
                var e = router.Session.Engine; var receipt = e.State.offline.Copy();
                Check(receipt.pending && receipt.creditedSeconds == 28800 && receipt.amount > Amount.Zero && e.State.cash >= new Amount(100000) + receipt.amount, "Return receipt was credited and capped before native interaction");
                Check(!router.driver.TapGameplaySurface(), "Return page blocks manual gameplay taps");
                Canvas.ForceUpdateCanvases(); var rect = (RectTransform)router.offline.next.transform;
                var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
                var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
                Check(pointer.position.y >= 0 && pointer.position.y <= Screen.height && hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == router.offline.next.gameObject, "Physical return action reaches the topmost saved control");
                var cash = e.State.cash; Click(router.offline.next);
                Check(router.CurrentRoute == "upgradedetails" && !e.State.offline.pending && e.State.cash.CompareTo(cash) == 0, "Next-step action opens the pinned upgrade without another claim");
                router.offline.next.onClick.Invoke(); Check(e.State.cash.CompareTo(cash) == 0, "Late duplicate return action cannot add income again");
                Check(!Store().Load().state.offline.pending, "Return acknowledgement survives a real save read");
                for (int i = 0; i < 3; i++)
                {
                    router.Navigate("skilldetails", i.ToString()); Check(!router.skillDetails.activate.interactable, "Unowned skill activation is disabled " + i);
                    Click(router.skillDetails.unlock); Check(router.CurrentRoute == "upgradedetails", "Skill base link opens native upgrade details " + i);
                    Click(router.details.buy); Click(router.details.back);
                    Check(router.CurrentRoute == "skilldetails" && e.SkillOwned(i) && router.skillDetails.activate.interactable, "Buying the base skill returns to a ready native skill " + i);
                    Click(router.skillDetails.activate); Check(e.State.readyAt[i] > e.State.now && !router.skillDetails.activate.interactable, "Native activation starts the real cooldown " + i);
                    if (i == 1) Check(router.toast.text.Contains("Rush + Hands overlap") && router.toast.text.Contains("×1.25"), "Native overlap feedback explains the actual Helping Hands bonus");
                    if (i == 2) Check(router.toast.text.Contains("normal sales") && router.toast.text.Contains("does not multiply"), "Bulk Order feedback keeps its payout basis separate from active Rush");
                    Click(router.skillDetails.upgrades); Check(router.CurrentRoute == "upgrades" && router.upgrades.Browser.Scope.skill == i, "Native skill upgrades link preserves scope " + i);
                }
                Check(e.State.activeUntil[0] > e.State.now && e.State.activeUntil[1] > e.State.now, "Rush and Hands can overlap while browsing menus");
                Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "All return/skill/catalogue navigation retains saved object identities");
                Check(errors.Count == 0, "No gameplay/runtime errors: " + string.Join("; ", errors.Take(3))); checks.Add("NOTE Known MCP path advisory count: " + tooling.Count);
                File.WriteAllLines(Report, new[] { "Scope: actual Play Mode, saved preview, real offline settlement and native skill purchases with a disposable profile. Main/device acceptance pending." }.Concat(checks));
                Debug.Log("REDESIGN_RETURN_PLAY_OK " + checks.Count); Finish();
            }
            catch (Exception error) { File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + error })); Debug.LogError("REDESIGN_RETURN_PLAY_FAIL " + error); Finish(); }
        }
        static void Finish() { EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); }
    }
}
