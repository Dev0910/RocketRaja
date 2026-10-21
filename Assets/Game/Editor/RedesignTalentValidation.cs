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
    public static class RedesignTalentValidation
    {
        sealed class FirstCard : IDrawRandom { public int calls; public int Next(int maximum) { calls++; return 0; } }
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var checks = new List<string>(); void Check(bool ok, string text) { if (!ok) throw new InvalidOperationException(text); checks.Add("PASS " + text); }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var r = root.GetComponent<RedesignScreenRouter>(); r.upgrades.gameObject.SetActive(false);
                var productArt = AssetDatabase.LoadAssetAtPath<RedesignProductArt>(BuildRedesignUI.ProductArtPath);
                Check(Enumerable.Range(0, 28).All(i => r.cardDetails.cardSprites[i] == productArt.At(i % 8, 0) && r.reveal.cardSprites[i] == productArt.At(i % 8, 0) && r.talent.cards[i].transform.Find("Artwork").GetComponent<UnityEngine.UI.Image>().sprite == productArt.At(i % 8, 0)), "All collection/detail/reveal illustrations reuse persistent clean product regions");
                foreach (var c in new MonoBehaviour[] { r.talent, r.cardDetails, r.reveal, r.prestige }) c.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(c, null);
                var data = RedesignRuntimeContent.Load(); var state = RedesignState.Fresh(); state.products[0] = 1; state.cash = 2000;
                var engine = new RedesignEngine(data, state, _ => { }, new FirstCard());
                var context = new RedesignUiContext(engine, () => true, f => f(engine), (_, __) => { }, _ => { });
                r.summary.text = "₹" + engine.State.cash + " cash · Normal sales ₹" + engine.NormalIncome() + "/s";
                r.talent.Bind(context); r.cardDetails.Bind(context); r.reveal.Bind(context); r.prestige.Bind(context);
                r.title.text = "Talent"; r.talent.Open();
                Check(r.talent.cards.Length == 28 && r.talent.lockedPanel.activeSelf && !r.talent.freeDraw.interactable && !r.talent.paidDraw.interactable, "All 28 cards are inspectable while first-prestige draw actions are locked");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "talent-locked");
                r.talent.gameObject.SetActive(false); r.prestige.Open(); r.title.text = "Prestige";
                Check(!r.prestige.confirm.interactable && r.prestige.status.text.Contains("no requirement to own all eight"), "Below-target prestige does not invent an all-product gate");
                RedesignNativeUiValidation.Render(root, scene, 390, 844, false, "prestige-locked");
                state = RedesignState.Fresh(); state.completedPrestiges = state.fame = (Natural)1; state.target = ProgressionNumbers.RunTarget(state.completedPrestiges);
                state.eligibleEarnings = state.target * 4; state.cash = 2000; state.products[0] = 1; state.freeCredits = 3; state.kajuKatli = 100;
                state.everPurchased.AddRange(data.Upgrades.Take(250).Select(u => u.id)); state.decorationStage = 2; state.cardLevels[0] = 1;
                engine = new RedesignEngine(data, state, _ => { }, new FirstCard());
                r.summary.text = "₹" + engine.State.cash + " cash · Normal sales ₹" + engine.NormalIncome() + "/s";
                context = new RedesignUiContext(engine, () => true, f => f(engine), (_, __) => { }, _ => { });
                r.talent.Bind(context); r.cardDetails.Bind(context); r.reveal.Bind(context); r.prestige.Bind(context); r.prestige.Open();
                Check(r.prestige.ReviewedQuote.Fame.Value == 2 && r.prestige.ReviewedQuote.Credits.Value == 3 && r.prestige.credits.text.Contains("Catalogue bonus: 1") && r.prestige.credits.text.Contains("one-time gift of 100 KK"), "Later prestige shows exact run/catalogue rewards and second-prestige gift");
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                {
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "prestige-ready", () => r.prestige.scroll.verticalNormalizedPosition = 1);
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "prestige-actions", () => r.prestige.scroll.verticalNormalizedPosition = 0);
                }
                r.prestige.gameObject.SetActive(false); r.talent.Open(); r.title.text = "Talent";
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "talent", () => r.talent.scroll.verticalNormalizedPosition = 1);
                RedesignNativeUiValidation.Render(root, scene, 390, 844, false, "talent-last-cards", () => r.talent.scroll.verticalNormalizedPosition = 0);
                r.talent.odds.onClick.Invoke(); RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "talent-odds", () => r.talent.scroll.verticalNormalizedPosition = 1);
                Check(r.talent.oddsText.text.Contains("4.6875%") && r.talent.oddsText.text.Contains("2.2%") && r.talent.oddsText.text.Contains("1.5%"), "Collection exposes exact per-card odds for all tiers");
                r.talent.gameObject.SetActive(false); r.title.text = "Card details";
                foreach (var card in data.Cards)
                {
                    r.cardDetails.Open(card.id);
                    Check(r.cardDetails.itemName.text == card.name && r.cardDetails.effect.text.Contains(RedesignCardCopy.Effect(card, engine.State.cardLevels[data.Cards.IndexOf(card)])), "Actual effect and stable identity: " + card.id);
                }
                r.cardDetails.Open(data.Cards[3].id);
                RedesignNativeUiValidation.Render(root, scene, 390, 844, false, "rocket-card-art", () => r.cardDetails.scroll.verticalNormalizedPosition = 1);
                r.cardDetails.Open(data.Cards[18].id); r.cardDetails.exact.onClick.Invoke();
                Check(r.cardDetails.applies.text.Contains("Buy Festival Rush") && r.cardDetails.effect.text.Contains("Exact benefit"), "Skill card explains its run prerequisite and exact duplicate benefit");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "card-detail-exact", () => r.cardDetails.scroll.verticalNormalizedPosition = 1);
                r.cardDetails.gameObject.SetActive(false); var quote = engine.QuoteDraw(true);
                Check(engine.Draw(quote), "Deterministic UI fixture commits one real draw before reveal");
                r.reveal.Open(); Check(r.reveal.outcome.text.Contains("Duplicate") && engine.State.cardLevels[0].Value == 2, "Duplicate reveal displays exactly one already-applied level");
                r.reveal.skip.onClick.Invoke(); Check(r.reveal.Settled && r.reveal.cardGroup.alpha == 1 && engine.State.freeCredits.Value == 2, "Skipping settles saved visuals without drawing or charging again");
                r.title.text = "Your Talent card";
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) }) RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "draw-reveal");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "draw-reveal-actions", () => r.reveal.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 0);
                engine.State.reducedMotion = true; r.reveal.Open(); Check(r.reveal.Settled && r.reveal.cardFace.localScale == Vector3.one, "Reduced motion opens at the final saved-result appearance");
                r.reveal.done.onClick.Invoke(); Check(!engine.State.reveal.pending && engine.State.cardLevels[0].Value == 2, "Closing acknowledges the result without changing the awarded card");
                bool saveFails = true; var random = new FirstCard();
                var failedEngine = new RedesignEngine(data, state, _ => { if (saveFails) throw new IOException("QA disk full"); }, random);
                var lifecycle = new RedesignLifecycle(failedEngine);
                var recoveryContext = new RedesignUiContext(failedEngine, () => lifecycle.CanMutate, lifecycle.Run, (_, __) => { }, _ => { });
                r.reveal.gameObject.SetActive(false); r.talent.Bind(recoveryContext); r.talent.Open(); r.talent.freeDraw.onClick.Invoke();
                var retryQuote = (TalentDrawQuote)typeof(RedesignTalentScreen).GetField("pending", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(r.talent);
                Check(!lifecycle.CanMutate && failedEngine.State.freeCredits.Value == 3 && !failedEngine.State.reveal.pending && random.calls == 2, "Failed draw persistence blocks UI and publishes neither debit nor reward");
                saveFails = false; Check(lifecycle.RetrySave(), "UI retry fixture recovers durable saving");
                r.talent.Refresh(); r.talent.freeDraw.onClick.Invoke();
                Check(failedEngine.State.reveal.operationId == retryQuote.OperationId && random.calls == 2 && failedEngine.State.freeCredits.Value == 2, "Native retry reuses the same selected result and operation without rerolling");
                lifecycle.OwnsSession = () => false; r.talent.Refresh();
                Check(!r.talent.freeDraw.interactable && !r.talent.paidDraw.interactable, "Lost session ownership disables both draw sources");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
            }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
            File.WriteAllLines("Documentation/Implementation/Unity-Talent-UI.txt", new[] { "Scope: isolated saved-native rendering/binding with explicit fixture states; Play Mode, Main and device evidence is separate." }.Concat(checks));
            Debug.Log("REDESIGN_TALENT_UI_OK " + checks.Count);
        }
    }
}
