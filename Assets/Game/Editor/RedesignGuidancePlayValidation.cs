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
    public static class RedesignGuidancePlayValidation
    {
        const string Key = "RocketRaja.RedesignGuidanceQA", Report = "Documentation/Implementation/Unity-Guidance-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>();
        static RedesignScreenRouter router; static Transform[] instances; static int step, boundFrame; static double deadline;
        static CashBatchQuote reviewed;
        static RedesignGuidancePlayValidation()
        {
            EditorApplication.playModeStateChanged += Mode;
            Application.logMessageReceived += (m, trace, type) =>
            {
                if (!SessionState.GetBool(Key, false) || type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
                const string advisory = "The project path contains spaces, which may cause issues during usage of AI Game Developer. Please consider the move the project to a folder without spaces.";
                if (m == advisory && trace.Contains("Startup")) return; errors.Add(m);
            };
        }
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var scenes = EditorSceneManager.GetSceneManagerSetup(); if (scenes.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Preserve dirty scene before QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = scenes.Select(s => new Record { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            string folder = Path.GetFullPath("Temp/RedesignGuidanceQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder); SessionState.SetString(Key + ".profile", folder);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc); state.cash = 10000;
            new RedesignSaveStore(Path.Combine(folder, "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>).Save(state);
            SessionState.SetString(Key + ".inventory", RedesignCataloguePlayValidation.Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            SessionState.SetInt(Key + ".phase", 0); SessionState.SetBool(Key + ".restart", false); SessionState.SetBool(Key, true);
            File.WriteAllText(Report, "Scope: two actual preview Play Mode sessions, disposable profile, topmost native raycasts and durable acknowledgements. No device or human comprehension claim.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) { router = null; step = 0; checks.Clear(); errors.Clear(); checks.AddRange(File.ReadAllLines(Report)); deadline = EditorApplication.timeSinceStartup + 30; EditorApplication.update -= Tick; EditorApplication.update += Tick; }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick;
                if (SessionState.GetBool(Key + ".restart", false)) { SessionState.SetBool(Key + ".restart", false); SessionState.SetInt(Key + ".phase", 1); EditorApplication.delayCall += EditorApplication.EnterPlaymode; return; }
                SessionState.SetBool(Key, false); var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}")); EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray()); Debug.Log("REDESIGN_GUIDANCE_QA_RESTORED");
            }
        }
        static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button button)
        {
            Canvas.ForceUpdateCanvases(); Check(button && button.IsActive() && button.IsInteractable(), "Active native control: " + button.name);
            var rect = (RectTransform)button.transform; var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button, "Topmost physical control: " + button.name + "; hits=" + string.Join(",", hits.Take(3).Select(h => h.gameObject.name)));
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            try
            {
                if (Time.frameCount < 3 || router && Time.frameCount <= boundFrame + 1) { if (EditorApplication.timeSinceStartup > deadline) throw new InvalidOperationException("No new rendered frame"); return; }
                if (!router)
                {
                    router = UnityEngine.Object.FindAnyObjectByType<RedesignScreenRouter>(); Check(router && !router.initializeGuestOnStart, "Saved preview starts without accessing a normal profile");
                    Check(RedesignCataloguePlayValidation.Inventory(router.gameObject) == SessionState.GetString(Key + ".inventory", ""), "Startup has exactly the saved UI inventory");
                    Check(router.driver.InitializeGuest(SessionState.GetString(Key + ".profile", "")), "Actual driver loads the disposable profile"); router.Bind(router.driver.Session, true);
                    instances = router.GetComponentsInChildren<Transform>(true); boundFrame = Time.frameCount; return;
                }
                var e = router.Session.Engine; var guide = router.guidance;
                if (SessionState.GetInt(Key + ".phase", 0) == 1)
                {
                    Check(new[] { "buy-first", "tap-counter", "buy-all" }.All(e.State.learnedTopics.Contains), "Second actual session restores acknowledged and skipped topics");
                    Check(!guide.ReaderOpen && !guide.strip.activeSelf && e.State.manualTaps.IsZero, "Restart neither reopens learned help nor manufactures counter taps");
                    router.Navigate("productdetails", "0"); Check(guide.CurrentTopic == "product-levels" && !guide.ReaderOpen, "An unacknowledged contextual topic remains optional after restart"); Finish(false); return;
                }
                switch (step++)
                {
                    case 0:
                        Check(guide.CurrentTopic == "buy-first" && !guide.ReaderOpen && e.State.products[0] == 0, "Opening suggests a manual first purchase without executing it"); Click(guide.read); break;
                    case 1:
                        Check(guide.ReaderOpen, "Explicit Read tip opens the reader"); Click(guide.skip);
                        Check(e.State.learnedTopics.Contains("buy-first") && !guide.ReaderOpen && !guide.strip.activeSelf && e.State.products[0] == 0 && e.State.manualTaps.IsZero, "Physical Skip persists but never buys or taps"); break;
                    case 2: Click(router.home.firstBuy); break;
                    case 3:
                        guide.Refresh("home", ""); Check(e.State.products[0] == 1 && guide.CurrentTopic == "tap-counter" && !guide.ReaderOpen, "Actual manual purchase makes tap guidance relevant without forcing it open"); Click(guide.read); break;
                    case 4: Click(guide.gotIt); Check(e.State.learnedTopics.Contains("tap-counter") && e.State.manualTaps.IsZero, "Got it persists without a leaked counter tap"); router.Navigate("productdetails", "0"); break;
                    case 5: Click(guide.read); break;
                    case 6:
                        router.Navigate("back", ""); Check(!guide.ReaderOpen && router.CurrentRoute == "productdetails" && !e.State.learnedTopics.Contains("product-levels"), "Back closes help before leaving the product page or learning its topic"); Click(guide.read); break;
                    case 7:
                        router.SetOverlay(true); Check(!guide.ReaderOpen && !guide.strip.activeSelf, "Utility overlay closes help and removes the strip"); router.SetOverlay(false); Click(router.upgradesTab); break;
                    case 8: Click(router.upgrades.reviewButton); reviewed = router.batch.ReviewedQuote; Check(reviewed.Ids.Count > 0 && guide.CurrentTopic == "buy-all", "Native frozen review offers relevant optional Buy All guidance"); break;
                    case 9: Click(guide.read); break;
                    case 10:
                        Click(guide.gotIt); Check(e.CanCommitBuyAll(reviewed) && e.State.learnedTopics.Contains("buy-all"), "Physical acknowledgement keeps the same reviewed batch valid"); break;
                    case 11:
                        Click(router.batch.confirm); Check(reviewed.Ids.All(e.State.upgrades.Contains) && e.State.manualTaps.IsZero, "The reviewed batch commits after help with no unintended counter taps"); Finish(true); return;
                }
                boundFrame = Time.frameCount; deadline = EditorApplication.timeSinceStartup + 30;
            }
            catch (Exception ex) { File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + ex })); Debug.LogError("REDESIGN_GUIDANCE_PLAY_FAIL " + ex); SessionState.SetBool(Key + ".restart", false); EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); }
        }
        static void Finish(bool restart)
        {
            Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Guidance and gameplay preserve all authored UI object identities"); Check(errors.Count == 0, "No gameplay/runtime errors: " + string.Join("; ", errors.Take(3)));
            File.WriteAllLines(Report, checks); SessionState.SetBool(Key + ".restart", restart); EditorApplication.update -= Tick; EditorApplication.ExitPlaymode();
        }
    }
}
