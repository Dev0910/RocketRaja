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
    public static class RedesignVisualPlayValidation
    {
        const string Key = "RocketRaja.RedesignVisualQA", Report = "Documentation/Implementation/Unity-Visual-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>();
        static RedesignScreenRouter router; static Transform[] instances; static int step, boundFrame; static double deadline;
        static RedesignVisualPlayValidation()
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
            string folder = Path.GetFullPath("Temp/RedesignVisualQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder); SessionState.SetString(Key + ".profile", folder);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc); state.cash = Amount.Pow10(100); state.products = Enumerable.Repeat(300, 8).ToArray(); state.eligibleEarnings = state.target;
            new RedesignSaveStore(Path.Combine(folder, "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>).Save(state);
            SessionState.SetString(Key + ".inventory", RedesignCataloguePlayValidation.Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            SessionState.SetInt(Key + ".phase", 0); SessionState.SetBool(Key + ".restart", false); SessionState.SetBool(Key, true);
            File.WriteAllText(Report, "Scope: two actual preview Play Mode sessions; saved native UI, disposable profile, real batch/reset/rebuy commands. No device acceptance claim.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) { router = null; step = 0; checks.Clear(); checks.AddRange(File.ReadAllLines(Report)); deadline = EditorApplication.timeSinceStartup + 30; EditorApplication.update -= Tick; EditorApplication.update += Tick; }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick;
                if (SessionState.GetBool(Key + ".restart", false)) { SessionState.SetBool(Key + ".restart", false); SessionState.SetInt(Key + ".phase", 1); EditorApplication.delayCall += EditorApplication.EnterPlaymode; return; }
                SessionState.SetBool(Key, false); var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}")); EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray()); Debug.Log("REDESIGN_VISUAL_QA_RESTORED");
            }
        }
        static void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button b)
        {
            Check(b && b.IsActive() && b.IsInteractable(), "Active native control: " + b.name);
            ExecuteEvents.Execute(b.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            try
            {
                if (Time.frameCount < 3) { if (EditorApplication.timeSinceStartup > deadline) throw new InvalidOperationException("No rendered frame"); return; }
                if (!router)
                {
                    router = UnityEngine.Object.FindAnyObjectByType<RedesignScreenRouter>(); Check(router && !router.initializeGuestOnStart, "Saved preview starts without normal profile access");
                    Check(RedesignCataloguePlayValidation.Inventory(router.gameObject) == SessionState.GetString(Key + ".inventory", ""), "Startup has the exact saved UI inventory");
                    Check(router.driver.InitializeGuest(SessionState.GetString(Key + ".profile", "")), "Actual driver loads the durable disposable state");
                    router.Bind(router.driver.Session, true); instances = router.GetComponentsInChildren<Transform>(true); boundFrame = Time.frameCount; return;
                }
                if (Time.frameCount <= boundFrame + 1) { if (EditorApplication.timeSinceStartup > deadline) throw new InvalidOperationException("Bound Home did not render"); return; }
                var e = router.Session.Engine; var home = router.home;
                if (SessionState.GetInt(Key + ".phase", 0) == 0)
                {
                    Check(home.shopVisuals.DisplayedStage == 0 && home.shopVisuals.layers.All(l => !l.activeSelf), "Unpurchased shop starts with no earned decoration layers");
                    Click(router.upgradesTab); router.upgrades.SetCategory(UpgradeCategory.All); Click(router.upgrades.reviewButton);
                    Check(router.batch.ReviewedQuote.Ids.Count == 1000, "Native batch review includes all 1,000 real first purchases"); Click(router.batch.confirm); Click(router.homeTab);
                    Check(home.shopVisuals.DisplayedStage == 5 && home.shopVisuals.layers.All(l => l.activeSelf) && home.productImages.Select((image, p) => image.sprite == home.productArt.At(p, 3)).All(ok => ok), "One confirmation settles all crossed shop and product tiers immediately");
                    Check(router.toast.text.Contains("1,000 upgrades · 1,000 new · 0 rebought") && router.toast.text.Contains("Shop 6/6 unlocked"), "One native Buy All notice aggregates the entire batch and only its final shop milestone");
                    Finish(true); return;
                }
                if (step++ == 0)
                {
                    Check(home.shopVisuals.DisplayedStage == 5 && e.State.UniqueCount == 1000 && home.productImages.Select((image, p) => image.sprite == home.productArt.At(p, 3)).All(ok => ok), "Second actual session restores final shop and product appearances without old celebrations");
                    // Layer images must not intercept the existing physical shelf hit regions.
                    Canvas.ForceUpdateCanvases();
                    foreach (var b in home.products)
                    {
                        var rect = (RectTransform)b.transform; var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)) }; var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
                        Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == b, "Decorated shelf remains the topmost physical target: " + b.name);
                    }
                    Click(router.prestigeTab); Click(router.prestige.confirm); Click(router.homeTab);
                    Check(e.State.products.All(p => p == 0) && home.shopVisuals.DisplayedStage == 5 && home.shopVisuals.layers.All(l => l.activeSelf), "Native prestige keeps earned shop decorations and clears every run display");
                    Click(home.firstBuy); Check(home.productImages[0].sprite == home.productArt.At(0, 0) && e.State.UniqueCount == 1000, "Manual rebuilding starts with Basic inside the retained decorated shop");
                    e.Advance(30); router.Navigate("upgradedetails", e.Data.AppearanceUpgrade(0, 1)); Click(router.details.buy); Click(router.homeTab);
                    Check(home.productImages[0].sprite == home.productArt.At(0, 1) && home.shopVisuals.DisplayedStage == 5 && e.State.UniqueCount == 1000, "Native signature rebuy restores only its run look and cannot recount lifetime milestones");
                    Finish(false);
                }
            }
            catch (Exception ex) { File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + ex })); Debug.LogError("REDESIGN_VISUAL_PLAY_FAIL " + ex); SessionState.SetBool(Key + ".restart", false); EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); }
        }
        static void Finish(bool restart)
        {
            Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Real progression navigation preserves all authored UI objects");
            Check(errors.Count == 0, "No gameplay/runtime errors: " + string.Join("; ", errors.Take(3)));
            File.WriteAllLines(Report, checks); SessionState.SetBool(Key + ".restart", restart); EditorApplication.update -= Tick; EditorApplication.ExitPlaymode();
        }
    }
}
