#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        [Serializable] sealed class RedesignSamples
        {
            public string unity, device, processor, package, profile, contentVersion;
            public int width, height, saveBytes, activeRows;
            public float scrollMedianMs, scrollP95Ms, scrollMaxMs;
            public double quoteMedianMs, batchCommitMs, saveMs;
            public long managedBefore, managedAfter;
            public RedesignCycleSample[] cycles;
        }
        void RedesignClick(Button button)
        {
            bool ready = button && button.IsActive() && button.IsInteractable(); Check(ready, "Native control is active: " + (button ? button.name : "missing"));
            if (ready) ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        IEnumerator RedesignChecks()
        {
            var app = game.redesignApplication; var router = app.router;
            Check(app.Ready && !string.IsNullOrEmpty(RedesignSmokeProfile.Root), "Redesign smoke owns a newly created disposable profile");
            if (!app.Ready || string.IsNullOrEmpty(RedesignSmokeProfile.Root)) yield break;
            var engine = router.Session.Engine; var transforms = new HashSet<Transform>(router.GetComponentsInChildren<Transform>(true));
            Check(game.Engine == null && engine.Data.Upgrades.Count == 1000 && engine.Data.Cards.Count == 28, "Main selects 1,000 cash upgrades and 28 Talent cards, with no legacy engine");
            Check(EventSystem.current && router.summary.font.HasCharacter('₹'), "Native input system and rupee glyph are present");
            var samples = new RedesignSamples { unity = Application.unityVersion, device = SystemInfo.deviceModel, processor = SystemInfo.processorType, package = Application.identifier, profile = RedesignSmokeProfile.Root, contentVersion = RedesignCatalog.EconomyVersion + "/" + RedesignCatalog.ContentVersion, width = Screen.width, height = Screen.height, activeRows = router.upgrades.rows.Length, managedBefore = GC.GetTotalMemory(true) };
            if (router.CurrentRoute == "offline") { RedesignClick(router.offline.close); yield return null; }
            router.Navigate("home", "");
            Check(router.CurrentRoute == "home" && !engine.State.offline.pending, "Startup return receipt is acknowledged before capturing Home");
            yield return Capture("redesign-01-home");
            router.Navigate("upgrades", ""); router.upgrades.SetView(UpgradeView.Full);
            Check(router.upgrades.Browser.Count == 1000 && router.upgrades.rows.Length == 8, "Full native catalogue uses eight saved rows");
            for (int i = 0; i < 20; i++) yield return null;
            var frames = new List<float>();
            for (int i = 0; i < 180; i++)
            {
                router.upgrades.content.anchoredPosition = new Vector2(0, i / 179f * 999 * router.upgrades.rowHeight); router.upgrades.Refresh();
                yield return null; frames.Add(Time.unscaledDeltaTime * 1000);
            }
            Check(router.upgrades.rows.Any(r => r.gameObject.activeSelf && r.ItemId == router.upgrades.Browser[999].id), "Scrolling reaches the final catalogue item without constructing rows");
            frames.Sort(); samples.scrollMedianMs = frames[frames.Count / 2]; samples.scrollP95Ms = frames[(int)Math.Ceiling(frames.Count * .95) - 1]; samples.scrollMaxMs = frames[frames.Count - 1];
            var quotes = new List<double>(); CashBatchQuote quote = null;
            for (int i = 0; i < 7; i++) { var timer = Stopwatch.StartNew(); quote = engine.QuoteBuyAll(); timer.Stop(); quotes.Add(timer.Elapsed.TotalMilliseconds); }
            quotes.Sort(); samples.quoteMedianMs = quotes[quotes.Count / 2];
            Check(quote.Ids.Count == 1000, "Native game can review all 1,000 eligible first purchases");
            router.OpenBatch(quote); yield return null; var commit = Stopwatch.StartNew(); RedesignClick(router.batch.confirm); commit.Stop(); samples.batchCommitMs = commit.Elapsed.TotalMilliseconds;
            Check(engine.State.upgrades.Count == 1000 && engine.State.UniqueCount == 1000 && !engine.CanCommitBuyAll(quote), "Reviewed Buy All commits once and cannot replay");
            router.DismissNotice(); router.Navigate("home", "");
            Check(router.home.shopVisuals.DisplayedStage == 5 && Enumerable.Range(0, 8).All(p => router.home.productImages[p].sprite == router.home.productArt.At(p, 3)), "Full cash catalogue settles final permanent and run appearances");
            yield return Capture("redesign-02-complete-shop");
            router.Navigate("prestige", ""); yield return null; RedesignClick(router.prestige.confirm);
            Check(engine.State.completedPrestiges.CompareTo((Natural)1) == 0 && engine.State.freeCredits.CompareTo((Natural)3) == 0 && engine.State.products.All(p => p == 0) && engine.State.UniqueCount == 1000, "First Prestige grants three free credits, clears the run and retains unique purchases");
            router.DismissNotice(); router.Navigate("talent", ""); yield return null; RedesignClick(router.talent.freeDraw);
            Check(router.CurrentRoute == "drawreveal" && engine.State.reveal.pending && engine.State.freeCredits.CompareTo((Natural)2) == 0, "Native free draw persists its result and spends one credit");
            var store = new RedesignSaveStore(Path.Combine(RedesignSmokeProfile.Root, "rocket-raja-v2.save"), engine.Data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            var disk = store.Load(); Check(disk.state != null && disk.state.reveal.operationId == engine.State.reveal.operationId && disk.state.reveal.pending, "Pending native reveal is already durable on disk");
            yield return Capture("redesign-03-reveal"); RedesignClick(router.reveal.done);
            Check(!engine.State.reveal.pending && engine.State.cardLevels.Aggregate((Natural)0, (sum, n) => sum + n).CompareTo((Natural)1) == 0, "Acknowledging the reveal cannot grant a second card");
            router.DismissNotice(); router.Navigate("home", ""); yield return null; RedesignClick(router.home.firstBuy);
            Check(engine.State.products[0] == 1 && engine.State.manualTaps.IsZero, "Rebuild still requires a manual first purchase and menus never generate counter taps");
            var save = Stopwatch.StartNew(); Check(router.Session.Run(e => e.Save()), "Full redesigned state saves through the real driver store"); save.Stop(); samples.saveMs = save.Elapsed.TotalMilliseconds;
            samples.saveBytes = checked((int)new FileInfo(Path.Combine(RedesignSmokeProfile.Root, "rocket-raja-v2.save")).Length);
            Check(samples.saveBytes < 750000 && store.Load().state.Valid(engine.Data), "Saved full-history payload remains valid and below the cloud payload guard");
            yield return RedesignRepeatedCycles(router, samples);
            samples.managedAfter = GC.GetTotalMemory(true);
            Check(transforms.SetEquals(router.GetComponentsInChildren<Transform>(true)), "Scrolling, batch, Prestige and reveal preserve authored UI identities");
            File.WriteAllText(Path.Combine(directory, "RedesignSamples.json"), JsonUtility.ToJson(samples, true));
            checks.Add("SCOPE Performance fixtures explicitly stage run resources for eight additional native Prestige/reveal cycles. Timings describe this Editor/development runtime, not earned pacing or release-device acceptance.");
        }
    }
}
#endif
