using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignFeedbackValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            const string report = "Documentation/Implementation/Unity-Feedback-UI.txt"; File.WriteAllText(report, "RUNNING\n");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            var data = RedesignRuntimeContent.Load(); int saves = 0;
            RedesignState Earlier() { var s = RedesignState.Fresh(); s.products[0] = 25; s.products[1] = 1; s.cash = Amount.Pow10(10); return s; }
            var engine = new RedesignEngine(data, Earlier(), _ => saves++); var before = new RedesignPurchaseFeedback(engine);
            Check(engine.BuyUpgrade("PP-PHU-01"), "Real signature purchase succeeds for an earlier owned product"); string first = before.Complete(engine);
            Check(first.Contains("+1 unique") && first.Contains("Phuljhari comeback: ₹42.5 → ₹85/s") && saves == 1, "First-purchase/comeback copy reports the actual income change with no extra save");
            var re = Earlier(); re.everPurchased.Add("PP-PHU-01"); engine = new RedesignEngine(data, re, _ => saves++); before = new RedesignPurchaseFeedback(engine); engine.BuyUpgrade("PP-PHU-01"); string rebuy = before.Complete(engine);
            Check(rebuy.StartsWith("Cash upgrade rebought") && rebuy.Contains("unique total unchanged") && engine.State.UniqueCount == 1, "Rebuy copy distinguishes restored run power from lifetime first purchases");
            var all = Earlier(); all.cash = Amount.Pow10(100); all.products = Enumerable.Repeat(300, 8).ToArray(); all.everPurchased = data.Upgrades.Take(375).Select(u => u.id).ToList(); engine = new RedesignEngine(data, all, _ => saves++); before = new RedesignPurchaseFeedback(engine); engine.CommitBuyAll(engine.QuoteBuyAll()); string batch = before.Complete(engine);
            Check(batch.Contains("1,000 upgrades · 625 new · 375 rebought") && batch.Contains("Shop 6/6 unlocked") && batch.Split('\n').Length <= 3, "One actual batch produces bounded aggregate counts and only its final shop milestone");
            var fresh = new RedesignEngine(data, RedesignState.Fresh(), _ => { }); fresh.BuyLevels(0, 1); fresh.Advance(200); before = new RedesignPurchaseFeedback(fresh); fresh.BuyUpgrade("PP-PHU-01");
            Check(!before.Complete(fresh).Contains("comeback"), "An opening product is not mislabeled as an older-product comeback");
            engine.ActivateSkill(0); engine.ActivateSkill(1);
            string overlap = RedesignPurchaseFeedback.Skill(engine, 1);
            Check(overlap.Contains("Rush + Hands overlap") && overlap.Contains("×1.25"), "Concurrent real skill states explain the correct overlap bonus");
            string order = RedesignPurchaseFeedback.Skill(engine, 2);
            Check(order.Contains("normal sales") && order.Contains("does not multiply"), "Bulk Order message does not promise a Rush multiplier");
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var router = root.GetComponent<RedesignScreenRouter>(); var home = router.home;
                foreach (Transform page in home.transform.parent) page.gameObject.SetActive(false);
                typeof(RedesignHomeScreen).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(home, null);
                var session = new RedesignLifecycle(engine); var context = new RedesignUiContext(engine, () => session.CanMutate, session.Run, (_, __) => { }, router.Notice);
                home.Bind(context, session.TapGameplaySurface); home.Open(); router.title.text = "Rocket Raja"; router.summary.text = "Feedback review · committed results";
                var instances = root.GetComponentsInChildren<Transform>(true);
                Check(router.toastDismiss && router.toastRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget, "Saved dismiss button and notice face prevent covered-control taps");
                Check(router.toast is RedesignSavedText saved && saved.savedMaterialSlots.All(s => s), "Feedback text reuses saved material slots");
                var messages = new[] { first, rebuy, batch, overlap, order, "First Prestige complete · +1 Fame\n3 free draws ready. Shop decorations stay; products rebuild.", "Phuljhari Spark saved\nDuplicate · +1 level\nCommon · Level 4 → 5" };
                for (int i = 0; i < messages.Length; i++)
                {
                    router.Notice(messages[i]);
                    foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                    {
                        RedesignNativeUiValidation.Render(root, scene, size.x, size.y, size.x == 390, "feedback-" + i);
                        Check(router.toastRoot.activeSelf && !router.toast.isTextTruncated && router.toast.text == messages[i], "Full committed feedback readable at " + size + " / case " + i);
                    }
                }
                var unchanged = JsonUtility.ToJson(engine.State); router.DismissNotice(); router.DismissNotice();
                Check(!router.toastRoot.activeSelf && JsonUtility.ToJson(engine.State) == unchanged, "Repeated dismissal hides presentation without a save, spend or grant");
                var muted = engine.State.Copy(); muted.sound = false; muted.music = false; muted.reducedMotion = true;
                var muteEngine = new RedesignEngine(data, muted, _ => { }); var muteSession = new RedesignLifecycle(muteEngine); home.Bind(new RedesignUiContext(muteEngine, () => muteSession.CanMutate, muteSession.Run, (_, __) => { }, router.Notice), muteSession.TapGameplaySurface);
                router.Notice(batch); RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "feedback-muted-reduced");
                Check(router.toastRoot.activeSelf && !router.toast.isTextTruncated && router.toast.text == batch, "Muted/reduced-motion state retains the complete immediately visible batch result");
                home.gameObject.SetActive(false); var failed = new RedesignEngine(data, Earlier(), _ => throw new IOException("Injected feedback QA save failure")); var failedSession = new RedesignLifecycle(failed);
                typeof(RedesignUpgradeDetailsScreen).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(router.details, null);
                router.details.Bind(new RedesignUiContext(failed, () => failedSession.CanMutate, failedSession.Run, (_, __) => { }, router.Notice)); router.details.Open("PP-PHU-01"); router.details.buy.onClick.Invoke();
                Check(failedSession.SaveFailure != null && failed.State.UniqueCount == 0 && router.toast.text.StartsWith("Purchase unavailable") && !router.toast.text.Contains("comeback"), "A failed native save shows no purchase/milestone success and retains the old state");
                Check(new HashSet<Transform>(instances).SetEquals(root.GetComponentsInChildren<Transform>(true)), "Every feedback state, dismissal and failure preserves saved object identities");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds"); File.WriteAllLines(report, new[] { "Scope: actual command-derived copy, isolated saved native renders and save-failure binding. Physical interactions are tested separately in Play Mode." }.Concat(checks)); Debug.Log("REDESIGN_FEEDBACK_UI_OK " + checks.Count);
            }
            catch (Exception ex) { File.AppendAllText(report, string.Join("\n", checks) + "\nFAIL " + ex); throw; }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
