using System.Numerics;
using System.Text.Json;
using RocketRaja.Core;

static partial class Program
{
    sealed class SequenceRandom : IDrawRandom
    {
        readonly int[] values; int index;
        public SequenceRandom(params int[] values) { this.values = values; }
        public int Next(int max) => values[index++ % values.Length] % max;
    }
    static bool Near(Amount a, Amount b) => a.mantissa == 0 && b.mantissa == 0 || Math.Abs(Amount.Ratio(a, b) - 1) < 1e-10;
    static void EngineChecks(RedesignCatalog catalog)
    {
        RedesignState fresh = RedesignState.Fresh(1000);
        RedesignState disk = null;
        RedesignEngine Make(RedesignState s = null, IDrawRandom random = null) => new(catalog, s ?? fresh, candidate => disk = Clone(candidate), random);
        var g = Make();
        Check(g.State.Valid(catalog) && g.State.cash.CompareTo(30) == 0 && g.State.eligibleEarnings.mantissa == 0 && g.State.products.All(x => x == 0), "Fresh run grants exact manual first cost without earned cash or products");
        Check(g.BuyLevels(0, 1) && g.State.cash.mantissa == 0 && disk.products[0] == 1, "Exact first-purchase affordability and immediate durability");
        Check(!g.BuyLevels(1, 1) && !g.BuyLevels(8, 1), "Insufficient cash and unknown product reject safely");
        Check(Near(g.NormalIncome(), .85), "First product rate is 0.85");
        var cashBeforeTaps = g.State.cash; var perTap = g.TapPayout();
        Check(Enumerable.Range(0, 100).All(_ => g.Tap()), "Every rapid manual tap succeeds without advancing time");
        Check(g.State.manualTaps.Value == 100 && Near(g.State.cash - cashBeforeTaps, perTap * 100), "Rapid taps each increment the count and award full earnings");
        g.Advance(100); Check(g.Tap() && g.Tap(), "Consecutive manual taps still succeed after idle time");
        var legacyCooldown = fresh.Copy(); legacyCooldown.nextManualTapAt = legacyCooldown.now + 1;
        Check(Make(legacyCooldown).Tap(), "A cooldown timestamp from an existing save does not block tapping");
        var projected = g.PreviewUpgrade("PP-PHU-01");
        Check(Near(projected.normalBefore, .85) && Near(projected.normalAfter, 1.7) && projected.requirementsMet && g.State.upgrades.Count == 0, "Upgrade projection shows correct per-family/shop change without a mutation");
        var wealth = RedesignState.Fresh(1000); wealth.cash = new Amount(1, 250); wealth.products = Enumerable.Repeat(300, 8).ToArray();
        g = Make(wealth);
        var all = g.QuoteBuyAll();
        Check(all.Ids.Count == 1000 && all.FirstPurchases == 1000, "All 1,000 upgrades reachable without any prestige requirement");
        Check(g.CommitBuyAll(all) && g.State.UniqueCount == 1000 && g.State.upgrades.Count == 1000 && g.State.decorationStage == 5, "One batch commits all purchases and lifetime decorations");
        Check(!g.CommitBuyAll(all) && !g.BuyUpgrade("PP-PHU-01") && !g.BuyUpgrade("missing"), "Stale batch, owned row and unknown row rejected");
        Check(g.State.kajuKatli.IsZero && g.State.freeCredits.IsZero && g.State.products[0] == 300 && g.State.tapLevel == 0, "Cash Buy All spends only on named cash entries");
        Check(g.SkillOwned(0) && g.SkillOwned(1) && g.SkillOwned(2) && g.SkillDuration(0) == 45 && g.SkillCooldown(0) == 225, "All skill branches apply cumulative ranks");
        Check(g.State.products.All(x => x == 300), "Batch does not buy product levels");
        var allRun = g.State.Copy(); allRun.eligibleEarnings = allRun.target;
        g = Make(allRun);
        var prestige = g.PreviewPrestige();
        Check(prestige.Eligible && prestige.Fame.Value == 1 && prestige.Credits.Value == 3, "First prestige offers x6-opening Fame and exactly three credits");
        Check(g.Prestige(prestige) && !g.Prestige(prestige), "Prestige is committed once and same quote cannot reset twice");
        Check(g.State.products.All(n => n == 0) && g.State.upgrades.Count == 0 && g.State.cash.CompareTo(30) == 0 && g.State.eligibleEarnings.mantissa == 0 && g.State.UniqueCount == 1000 && g.State.decorationStage == 5, "Reset clears the run and retains lifetime collection/decorations");
        Check(!g.SkillOwned(0) && g.State.activeUntil.All(n => n == 0), "Skills must be repurchased and no effects survive reset");
        var rebuy = g.State.Copy(); rebuy.cash = 10000; rebuy.products[0] = 1;
        g = Make(rebuy); Check(g.BuyUpgrade("PP-PHU-01") && g.State.UniqueCount == 1000, "Rebuy does not increment lifetime U");
        Check(Near(g.NormalIncome(), .85 * 2 * 6), "Signature replaces supporting effect and Fame applies exactly once");
        var second = g.State.Copy(); second.eligibleEarnings = second.target * 4; second.cardLevels[8] = 1; second.music = false; second.storeName = "Fixture Shop"; second.learnedTopics.Add("buy-first");
        g = Make(second);
        var secondQuote = g.PreviewPrestige(); Check(secondQuote.Credits.Value == 6 && secondQuote.Fame.Value == 2, "Later 4T plus U=1000 grants six credits and two Fame");
        Check(secondQuote.RunCredits.Value == 2 && secondQuote.CatalogueCredits == 4 && Near(secondQuote.NextFameEarnings, second.target * 9) && Near(secondQuote.NextDrawEarnings, second.target * 16), "Prestige preview separates credit components and next Fame/draw thresholds");
        Check(g.Prestige(secondQuote) && g.State.kajuKatli.Value == 100 && g.State.secondPrestigeGift && Near(g.State.cash, 27.6), "Second reset gifts 100 KK once and grants discounted manual opening cash");
        Check(!g.State.music && g.State.storeName == "Fixture Shop" && g.State.learnedTopics.Contains("buy-first") && g.State.cardLevels[8].Value == 1, "Reset retains preferences, name, taught topics and cards");
        var afterSecond = g.State.Copy();
        g = Make(afterSecond, new SequenceRandom(0, 0));
        var free = g.QuoteDraw(true); var creditsBefore = g.State.freeCredits.Value;
        Check(g.Draw(free) && g.State.cardLevels[0].Value == 1 && disk.reveal.pending && disk.freeCredits.Value == creditsBefore - 1, "Free draw persists selected card and debit before reveal");
        Check(!g.Draw(free) && g.QuoteDraw(true) == null, "Repeated draw click and a second action during reveal cannot spend again");
        var reopened = Make(disk, new SequenceRandom(99, 1));
        Check(reopened.State.reveal.cardId == "C01" && reopened.State.cardLevels[0].Value == 1, "Reload replays committed result without consulting new randomness");
        Check(reopened.DismissReveal(reopened.State.reveal.operationId), "Committed reveal can be skipped safely");
        g = Make(reopened.State, new SequenceRandom(0, 0));
        var paid = g.QuoteDraw(false);
        Check(g.Draw(paid) && g.State.kajuKatli.IsZero && g.State.cardLevels[0].Value == 2 && g.State.reveal.before.Value == 1 && g.State.reveal.after.Value == 2, "100-KK draw duplicates add exactly one level");
        g.DismissReveal(g.State.reveal.operationId); Check(g.QuoteDraw(false) == null, "Insufficient KK cannot draw");
        var lastCredit = afterSecond.Copy(); lastCredit.freeCredits = 1;
        g = Make(lastCredit, new SequenceRandom(75, 9));
        Check(g.Draw(g.QuoteDraw(true)) && g.State.reveal.cardId == "R10", "Tier lower boundary and uniform identity mapping reach last Rare");
        g.DismissReveal(g.State.reveal.operationId);
        Check(g.QuoteDraw(true) == null && g.State.kajuKatli.Value == 100, "Last free draw never changes into a paid action");
        g = Make(afterSecond, new SequenceRandom(97, 1));
        Check(g.Draw(g.QuoteDraw(true)) && g.State.reveal.cardId == "L02", "Legendary begins at 97 and reaches both identities");
        g = Make(); Check(g.QuoteDraw(true) == null && g.QuoteDraw(false) == null, "New players still need credits or Kaju Katli to draw");
        var earlyTalent = RedesignState.Fresh(1000); earlyTalent.kajuKatli = 500;
        g = Make(earlyTalent, new SequenceRandom(0, 0));
        Check(g.CanDraw(false) && g.Draw(g.QuoteDraw(false)) && g.State.completedPrestiges.IsZero && g.State.kajuKatli.Value == 400 && g.State.Valid(catalog), "Kaju Katli draws work and save before the first prestige");
        Check(Clone(g.State).Valid(catalog), "Pre-prestige Talent ownership survives save reload");
        earlyTalent = RedesignState.Fresh(1000); earlyTalent.freeCredits = 5;
        g = Make(earlyTalent, new SequenceRandom(0, 0));
        Check(g.CanDraw(true) && g.Draw(g.QuoteDraw(true)) && g.State.freeCredits.Value == 4 && g.State.Valid(catalog), "Free Talent draws work before the first prestige when credits are available");
        g.DismissReveal(g.State.reveal.operationId);
        Check(g.CanDrawFive(true) && g.DrawFive(g.QuoteDrawFive()) && g.State.Valid(catalog), "Five-card free draws work before the first prestige");
        var recoveryCopy = afterSecond.Copy(); recoveryCopy.requiresDrawReconciliation = true;
        g = Make(Clone(recoveryCopy));
        Check(g.QuoteDraw(true) == null && g.QuoteDraw(false) == null && g.State.Valid(catalog), "Restored backup cannot spend rolled-back currency or reroll credits before reconciliation");
        var stage = RedesignState.Fresh(1000); stage.products[0] = 1; stage.cash = 100000;
        g = Make(stage); var scoped = g.QuoteBuyAll(new UpgradeScope { effect = CashEffect.ProductProfit, product = 0 });
        Check(scoped.Ids.Count > 1 && scoped.Ids.All(id => id.StartsWith("PP-PHU-")), "Scoped batch includes newly eligible prerequisite successors only within the filter");
        g.Advance(100); Check(g.CommitBuyAll(scoped) && g.State.upgrades.SequenceEqual(scoped.Ids), "Passive accrual preserves frozen batch list without adding purchases");
        g = Make(stage); scoped = g.QuoteBuyAll(); g.SessionKey = "different-owner";
        Check(!g.CommitBuyAll(scoped), "Session change fences stale quote");
        g.MutationAllowed = () => false;
        Check(!g.BuyLevels(0, 1) && !g.BuyUpgrade("PP-PHU-01") && !g.Tap() && !g.Save(), "Read-only ownership blocks all mutation routes");
        var beforeTime = g.State.now; g.Advance(100); Check(g.State.now == beforeTime, "Read-only ownership also blocks accrual");
        bool fail = true;
        var atomic = new RedesignEngine(catalog, stage, s => { if (fail) throw new IOException("Injected disk full"); disk = Clone(s); });
        var atomicQuote = atomic.QuoteBuyAll();
        try { atomic.CommitBuyAll(atomicQuote); } catch (IOException) { }
        Check(atomic.State.upgrades.Count == 0 && atomic.State.cash.CompareTo(stage.cash) == 0, "Failed batch save rolls back every purchase and payment");
        fail = false; Check(atomic.CommitBuyAll(atomicQuote) && !atomic.CommitBuyAll(atomicQuote), "Retry after failed write commits once");
        fail = true;
        atomic = new RedesignEngine(catalog, afterSecond, s => { if (fail) throw new IOException("Injected draw failure"); disk = Clone(s); }, new SequenceRandom(0, 0, 99, 1));
        var retryDraw = atomic.QuoteDraw(true); var originalCredits = atomic.State.freeCredits.Value;
        try { atomic.Draw(retryDraw); } catch (IOException) { }
        Check(atomic.State.freeCredits.Value == originalCredits && !atomic.State.reveal.pending, "Failed draw persistence preserves payment and prior cards");
        fail = false;
        Check(atomic.Draw(retryDraw) && atomic.State.reveal.cardId == "C01", "Retry of failed draw preserves selected identity without reroll");
        fail = true;
        var thirdRun = afterSecond.Copy(); thirdRun.eligibleEarnings = thirdRun.target * 9;
        atomic = new RedesignEngine(catalog, thirdRun, s => { if (fail) throw new IOException("Injected reset failure"); disk = Clone(s); });
        var retryPrestige = atomic.PreviewPrestige();
        try { atomic.Prestige(retryPrestige); } catch (IOException) { }
        Check(atomic.State.runId == thirdRun.runId && atomic.State.fame.Value == thirdRun.fame.Value && atomic.State.completedPrestiges.Value == 2, "Failed prestige save retains prior run and all prior rewards");
        fail = false;
        Check(atomic.Prestige(retryPrestige) && atomic.State.completedPrestiges.Value == 3 && atomic.State.kajuKatli.Value == 100 && !atomic.Prestige(retryPrestige), "Retried prestige commits once; later prestiges never repeat the 100-KK gift");
        var uncoached = RedesignState.Fresh(1000); uncoached.eligibleEarnings = uncoached.target;
        g = Make(uncoached); var noFamilies = g.PreviewPrestige(); g.Advance(1);
        Check(g.Prestige(noFamilies), "Prestige has no compulsory all-eight ownership gate and survives passive accrual");
        // Product bulk quote compares with independent sequential single costs.
        g = Make(wealth); Amount singles = 0;
        for (int i = 0; i < 100; i++) { singles += g.LevelQuote(0, 1).cost; g.BuyLevels(0, 1); }
        var bulk = Make(wealth); Check(Near(singles, bulk.LevelQuote(0, 100).cost), "Bulk product costs equal sequential singles");
        var discounted = allRun.Copy(); discounted.cardLevels[8] = 1; discounted.fame = discounted.completedPrestiges = 1; discounted.target = ProgressionNumbers.RunTarget(1);
        var combined = Make(discounted);
        Check(Near(combined.LevelQuote(0, 1).cost, bulk.LevelQuote(0, 1).cost * .01 * .92), "Cumulative product discount replaces prior ranks and multiplies remaining card cost");
        var maxState = wealth.Copy(); maxState.cash = bulk.LevelQuote(0, 100).cost;
        g = Make(maxState); Check(g.LevelQuote(0, int.MaxValue).quantity == 100, "Max buys exact affordable bundle at quote boundary");
        var offlineState = RedesignState.Fresh(1000); offlineState.products[0] = 1;
        g = Make(offlineState); g.Save();
        Check(g.ResolveOffline(1000 + 40000) && Near(g.State.offline.amount, .85 * .5 * 28800) && g.State.offline.creditedSeconds == 28800, "Offline uses normal departure income at 50% capped to eight hours");
        Amount settled = g.State.cash;
        Check(!g.ResolveOffline(41000) && !g.ResolveOffline(500) && Near(settled, g.State.cash), "Duplicate resume and negative clock delta cannot pay twice");
        g = Make(disk); Check(!g.ResolveOffline(41000) && Near(g.State.cash, settled) && g.DismissOffline(), "Reload/summary close never credits offline a second time");
        var skillState = wealth.Copy(); skillState.cash = new Amount(1, 250);
        g = Make(skillState); g.CommitBuyAll(g.QuoteBuyAll()); var p = g.NormalIncome(); var normalTap = g.TapPayout();
        Check(g.ActivateSkill(0) && Near(g.TapPayout(), normalTap * 1.25), "Rush adds separate x1.25 taps without changing linked normal P");
        Amount beforeBulk = g.State.cash;
        // Huge wallet makes subtraction unsuitable for measuring payout; use eligible earnings.
        Amount beforeEarned = g.State.eligibleEarnings;
        Check(g.ActivateSkill(2) && Near(g.State.eligibleEarnings - beforeEarned, p * 300), "Bulk Order uses normal P during Rush");
        Check(!g.ActivateSkill(0) && g.State.readyAt[0] == 1225, "Cooldown starts at activation and prevents immediate reactivation");
        var overlap = g.State.Copy(); overlap.cash = 0; overlap.eligibleEarnings = 0;
        g = Make(overlap); g.ActivateSkill(1); g.Advance(46);
        Check(g.State.now == 1046 && g.State.activeUntil[0] <= g.State.now && g.State.activeUntil[1] <= g.State.now && g.State.eligibleEarnings.Valid, "Overlapping skills expire at boundaries without infinite loops");
        var deniedSkill = afterSecond.Copy(); deniedSkill.cardLevels[18] = 100; deniedSkill.cardLevels[21] = 100;
        g = Make(deniedSkill); Check(!g.SkillOwned(0) && !g.ActivateSkill(0) && g.SkillDuration(0) == 0, "Permanent skill cards never unlock a run-local skill");
        var handsBase = RedesignState.Fresh(1000); handsBase.products[0] = handsBase.products[1] = 1;
        handsBase.upgrades.Add("SK-HH-P-01"); handsBase.everPurchased.Add("SK-HH-P-01");
        g = Make(handsBase); g.ActivateSkill(1); g.Advance(30);
        Check(Near(g.State.eligibleEarnings, (EconomyBalance.ProductIncome(0) + EconomyBalance.ProductIncome(1)) * 30 + EconomyBalance.TapBase * 90), "Base Helping Hands delivers 90 independent cadence payouts over 30 seconds");
        var stepped = Make(handsBase); stepped.ActivateSkill(1); for (int i = 0; i < 300; i++) stepped.Advance(.1);
        Check(Near(stepped.State.eligibleEarnings, g.State.eligibleEarnings), "Helping Hands payout is independent of frame subdivision at expiry");
        SaveChecks(catalog);
    }
    static void SaveChecks(RedesignCatalog catalog)
    {
        string folder = Path.Combine(Path.GetTempPath(), "RocketRajaRedesign-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, "rocket-raja-v2.save");
        var store = new RedesignSaveStore(file, catalog, s => JsonSerializer.Serialize(s, Json), s => JsonSerializer.Deserialize<RedesignState>(s, Json), s => JsonSerializer.Deserialize<SaveVersionHeader>(s, Json));
        Check(store.Load().CanCreateFresh, "Missing save is distinct from malformed save");
        var state = RedesignState.Fresh(1000); store.Save(state);
        Check(store.Load().state.Valid(catalog), "New state roundtrips through checksummed file");
        state.revision += (Natural)1; state.cash = 50; store.Save(state);
        File.WriteAllText(file, "bad checksum");
        Check(store.Load().status == RedesignLoadStatus.RecoveredBackup && store.Load().state.cash.CompareTo(30) == 0, "Corrupt main recovers validated backup");
        state.requiresDrawReconciliation = true;
        store.Save(state); Check(Directory.GetFiles(folder, "*.damaged-*").Length == 1 && store.Load().state.cash.CompareTo(50) == 0, "Recovery preserves damaged source and valid backup");
        void Raw(object value) { string json = JsonSerializer.Serialize(value, value.GetType(), Json); File.WriteAllText(file, SavePayload.Digest(json) + "\n" + json); }
        state.schema = 4; Raw(state);
        Check(store.Load().status == RedesignLoadStatus.UnsupportedVersion, "Future main cannot silently fall back to old backup");
        Reject(() => store.Save(RedesignState.Fresh()), "Future save cannot be overwritten");
        File.Delete(file + ".bak"); Raw(new GameState()); Check(store.Load().CanCreateFresh, "Known old local save starts the new economy");
        store.Save(RedesignState.Fresh()); store.Save(store.Load().state);
        Check(store.Load().status == RedesignLoadStatus.Loaded, "Economy reset writes a durable current save and does not reset again");
        Raw(new RedesignState { schema = 2, economy = RedesignCatalog.EconomyVersion, contentVersion = RedesignCatalog.ContentVersion });
        Check(store.Load().status == RedesignLoadStatus.RecoveredBackup, "Missing mandatory new fields are malformed, not fresh defaults");
        string brokenJson = "{bad json"; File.WriteAllText(file, SavePayload.Digest(brokenJson) + "\n" + brokenJson);
        Check(store.Load().status == RedesignLoadStatus.RecoveredBackup, "Codec parse failures also recover without throwing away evidence");
        var source = new RedesignState(); Check(!source.Valid(catalog), "Empty default state is rejected");
        const string fixture = "Documentation/Implementation/Fixtures/Legacy/guest.save";
        string rawFixture = File.ReadAllText(fixture), target = Path.Combine(folder, "migrated", "rocket-raja-v2.save");
        RedesignState Migrate() => DisposableFixtureMigration.Convert(fixture, target, catalog, s => JsonSerializer.Deserialize<GameState>(s, Json), s => JsonSerializer.Serialize(s, Json), s => JsonSerializer.Deserialize<RedesignState>(s, Json), s => JsonSerializer.Deserialize<SaveVersionHeader>(s, Json), 20000);
        var converted = Migrate();
        Check(converted.Valid(catalog) && converted.fame.IsZero && converted.completedPrestiges.IsZero && converted.upgrades.Count == 0 && converted.everPurchased.Count == 0 && converted.kajuKatli.IsZero, "Identified disposable legacy fixture resets without inventing Fame/cards/KK/U");
        Check(!converted.offline.pending && converted.offline.amount.CompareTo(Amount.Zero) == 0 && converted.eligibleEarnings.CompareTo(Amount.Zero) == 0 &&
            converted.cash.CompareTo(new Amount(30)) == 0 && new RedesignEngine(catalog, converted, _ => { }).NormalIncome().CompareTo(Amount.Zero) == 0,
            "Fixture transition drops active sponsor, old receipts and pending offline offer without crediting an ad reward");
        Check(converted.storeName == "Fixture Shop" && !converted.music && converted.reducedMotion && File.ReadAllText(fixture) == rawFixture, "Fixture transition retains selected preferences/name and original bytes");
        Check(Migrate().runId == converted.runId && File.Exists(fixture + ".pre-v2-" + SavePayload.Digest(rawFixture)), "Migration retries restore same run and pre-migration archive");
        bool disallowed = false;
        try { DisposableFixtureMigration.Convert(file, target, catalog, s => null, s => "", s => null, s => null, 0); } catch (InvalidOperationException) { disallowed = true; }
        Check(disallowed, "Test-only migration rejects any path outside the synthetic-fixture directory");
        Console.WriteLine("Save fixtures: " + folder);
    }
}
