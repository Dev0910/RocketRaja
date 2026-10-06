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
    public static class RedesignHomePlayValidation
    {
        const string Key = "RocketRaja.RedesignHomeQA", Report = "Documentation/Implementation/Unity-Home-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>(), tooling = new List<string>();
        static RedesignScreenRouter router;
        static Transform[] instances;
        static Vector2[] popupPositions;
        static int step, frame;
        static double deadline, frameDeadline;
        static RedesignHomePlayValidation()
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
            string profile = Path.GetFullPath("Temp/RedesignHomeQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(profile); SessionState.SetString(Key + ".profile", profile);
            SessionState.SetString(Key + ".inventory", RedesignCataloguePlayValidation.Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            SessionState.SetBool(Key, true); File.WriteAllText(Report, "RUNNING: native Home input and navigation on a fresh disposable profile.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) { step = -1; checks.Clear(); deadline = EditorApplication.timeSinceStartup + .5; EditorApplication.update -= Tick; EditorApplication.update += Tick; }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick; SessionState.SetBool(Key, false); var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
                EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray()); Debug.Log("REDESIGN_HOME_QA_RESTORED");
            }
        }
        static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button b)
        {
            Check(b && b.IsActive() && b.IsInteractable(), "Active control: " + (b ? b.name : "missing"));
            ExecuteEvents.Execute(b.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        static PointerEventData Pointer(RectTransform rect, int id = 0)
        {
            Canvas.ForceUpdateCanvases(); var position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            return new PointerEventData(EventSystem.current) { position = position, pressPosition = position, pointerId = id, button = PointerEventData.InputButton.Left };
        }
        static void CounterTap(int id = 0)
        {
            var target = router.home.counter.gameObject; var data = Pointer((RectTransform)target.transform, id); var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data, hits);
            Check(data.position.x >= 0 && data.position.x <= Screen.width && data.position.y >= 0 && data.position.y <= Screen.height && hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject) == target, "Physical counter hit reaches only the saved tap surface");
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerDownHandler); ExecuteEvents.Execute(target, data, ExecuteEvents.pointerUpHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                if (step == -1)
                {
                    router = UnityEngine.Object.FindAnyObjectByType<RedesignScreenRouter>();
                    Check(router && !router.initializeGuestOnStart && SceneManager.GetActiveScene().path == BuildRedesignUI.PreviewPath, "Saved preview uses only its explicit fresh QA profile");
                    Check(RedesignCataloguePlayValidation.Inventory(router.gameObject) == SessionState.GetString(Key + ".inventory", ""), "Startup preserves every serialized UI object and component");
                    Check(router.driver.InitializeGuest(SessionState.GetString(Key + ".profile", "")), "Actual driver creates an isolated fresh profile");
                    router.Bind(router.driver.Session, true); router.Bind(router.driver.Session, true); instances = router.GetComponentsInChildren<Transform>(true);
                    frame = Time.frameCount; frameDeadline = EditorApplication.timeSinceStartup + 15; deadline = EditorApplication.timeSinceStartup + .1; step = 0; return;
                }
                if (step == 0 && Time.frameCount <= frame + 1) { if (EditorApplication.timeSinceStartup > frameDeadline) throw new InvalidOperationException("No player layout frames"); return; }
                var e = router.Session.Engine; var home = router.home;
                switch (step++)
                {
                    case 0:
                        Check(router.CurrentRoute == "home" && e.State.products.All(p => p == 0) && e.State.cash.CompareTo(new Amount(30)) == 0, "Fresh Home does not automatically purchase a product");
                        Click(home.firstBuy); Check(e.State.products[0] == 1 && e.State.cash.CompareTo(Amount.Zero) == 0 && e.State.manualTaps.IsZero, "First-purchase UI spends exactly the grant and leaks no tap");
                        CounterTap(); CounterTap(1); Check(e.State.manualTaps.Value == 1, "Two same-frame pointers cannot bypass the manual rate limit");
                        var target = home.counter.gameObject; var drag = Pointer((RectTransform)target.transform, 2);
                        ExecuteEvents.Execute(target, drag, ExecuteEvents.pointerDownHandler); ExecuteEvents.Execute(target, drag, ExecuteEvents.beginDragHandler); drag.position += Vector2.up * 70;
                        ExecuteEvents.Execute(target, drag, ExecuteEvents.dragHandler); ExecuteEvents.Execute(target, drag, ExecuteEvents.endDragHandler); ExecuteEvents.Execute(target, drag, ExecuteEvents.pointerUpHandler);
                        Check(e.State.manualTaps.Value == 1, "Counter scroll gesture never earns a tap");
                        var outside = Pointer((RectTransform)target.transform, 3); ExecuteEvents.Execute(target, outside, ExecuteEvents.pointerDownHandler); outside.position = new Vector2(-100, -100); ExecuteEvents.Execute(target, outside, ExecuteEvents.pointerUpHandler);
                        Check(e.State.manualTaps.Value == 1, "Releasing outside the counter earns nothing");
                        Click(home.products[3]); Check(router.CurrentRoute == "productdetails" && router.productDetails.Target == 3 && !router.driver.TapGameplaySurface(), "Shelf opens its exact product and closes the gameplay input gate");
                        home.counter.Tap(); Check(e.State.manualTaps.Value == 1, "Late hidden-counter callback is rejected by the session gate");
                        Click(router.homeTab); Click(home.training); Check(router.CurrentRoute == "taptraining", "Home training link reaches quantity controls");
                        Click(router.homeTab); Click(home.skills[1]); Check(router.CurrentRoute == "skilldetails" && router.skillDetails.Target == 1, "Home skill link keeps the intended identity");
                        Click(router.homeTab); Click(home.wallet); Check(router.CurrentRoute == "kajushop" && e.State.kajuKatli.IsZero && !e.State.HasDevelopmentShop, "Wallet opens the placeholder without granting anything");
                        Click(router.homeTab); Check(e.State.manualTaps.Value == 1, "All tested UI navigation remains separate from gameplay taps");
                        deadline = EditorApplication.timeSinceStartup + .45; break;
                    case 1:
                        e.State.reducedMotion = true; CounterTap(); Check(e.State.manualTaps.Value == 2, "A later physical tap earns exactly once after its cooldown");
                        popupPositions = home.tapPopups.Select(t => t.rectTransform.anchoredPosition).ToArray(); deadline = EditorApplication.timeSinceStartup + .15; break;
                    case 2:
                        Check(home.tapPopups.Select((t, i) => t.rectTransform.anchoredPosition == popupPositions[i]).All(x => x), "Reduced motion leaves saved tap feedback positions fixed");
                        Check(home.tapGroups.Any(g => g.alpha > 0), "Accepted tap still gives visible feedback with reduced motion");
                        Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Home, navigation and popups never construct UI at runtime");
                        Check(errors.Count == 0, "No gameplay/runtime errors: " + string.Join("; ", errors.Take(3))); checks.Add("NOTE Known MCP path advisory count: " + tooling.Count);
                        File.WriteAllLines(Report, new[] { "Scope: actual Play Mode saved Home, fresh disposable profile and Unity pointer events; Main/device integration pending." }.Concat(checks)); Debug.Log("REDESIGN_HOME_PLAY_OK " + checks.Count); Finish(); break;
                }
            }
            catch (Exception error) { File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + error })); Debug.LogError("REDESIGN_HOME_PLAY_FAIL " + error); Finish(); }
        }
        static void Finish() { EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); }
    }
}
