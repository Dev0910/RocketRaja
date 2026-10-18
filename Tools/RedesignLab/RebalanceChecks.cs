using RocketRaja.Core;
using RocketRaja.Editor;
using System.Text.Json;

static partial class Program
{
    static void RebalanceChecks(RedesignCatalog data)
    {
        Check(Enumerable.Range(1, 35).All(f => ProgressionNumbers.TapFameMultiplier(f) >= ProgressionNumbers.TapFameMultiplier(f - 1)) &&
              Enumerable.Range(1, 35).All(f => ProgressionNumbers.RunTarget(f) > ProgressionNumbers.RunTarget(f - 1)),
              "Calibrated tap Fame and prestige targets increase through and beyond authored stages");
        var strongestTap = data.Upgrades.First(u => u.effect == CashEffect.TapDirect && u.factor == 32);
        foreach (int fame in new[] { 1, 8, 15, 25, 33 })
        {
            var s = RedesignState.Fresh(); s.fame = s.completedPrestiges = fame; s.secondPrestigeGift = fame >= 2;
            s.target = ProgressionNumbers.RunTarget(fame); s.products = Enumerable.Repeat(200, 8).ToArray(); s.tapLevel = 200;
            s.cardLevels[16] = s.cardLevels[19] = s.cardLevels[22] = 100; s.cash = Amount.Pow10(250);
            var e = new RedesignEngine(data, s, _ => { }); var before = e.TapPayout(); var projection = e.PreviewUpgrade(strongestTap.id);
            e.BuyUpgrade(strongestTap.id);
            Check(Near(e.TapPayout(), before * 32) && Near(e.TapPayout(true), before * 32) && Near(projection.manualTapAfter, e.TapPayout()),
                "Tap upgrades retain full x32 manual/Helping Hands payout with tap-heavy cards at Fame " + fame);
            var quote = e.PreviewPrestige();
            Check(Near(quote.ResultingTapIncomeMultiplier, ProgressionNumbers.TapFameMultiplier(e.State.fame + quote.Fame)), "Prestige preview uses the same tap Fame curve at " + fame);
        }
        foreach (var pair in new[] { (9, 2), (24, 2), (49, 3), (99, 4), (199, 5), (299, 5) })
        {
            var s = RedesignState.Fresh(); s.products[0] = s.tapLevel = pair.Item1; s.cash = Amount.Pow10(250);
            var e = new RedesignEngine(data, s, _ => { }); var before = e.ProductIncome(0); var tap = e.TapPayout();
            var projection = e.PreviewLevels(0, 1); e.BuyLevels(0, 1);
            Check(Near(e.ProductIncome(0), before * (pair.Item1 + 1d) / pair.Item1 * pair.Item2) && Near(e.NormalIncome(), projection.normalAfter), "Product milestone and preview agree at " + (pair.Item1 + 1));
            e.BuyLevels(-1, 1);
            Check(Near(e.TapPayout(), tap * (1 + 1.4 * (pair.Item1 + 1)) / (1 + 1.4 * pair.Item1) * pair.Item2), "Tap milestone applies exactly once at " + (pair.Item1 + 1));
        }
        foreach (int threshold in new[] { 50, 100, 200, 300 })
        {
            var s = RedesignState.Fresh(); s.products = Enumerable.Repeat(threshold, 8).ToArray(); s.products[7]--;
            s.cash = Amount.Pow10(250); var e = new RedesignEngine(data, s, _ => { });
            var before = e.ProductIncome(0); var tap = e.TapPayout(); var projected = e.PreviewLevels(7, 1);
            e.BuyLevels(7, 1);
            Check(Near(e.ProductIncome(0), before * 2) && Near(e.NormalIncome(), projected.normalAfter) && Near(e.TapPayout(), tap), "All-eight synergy doubles products, excludes taps, and appears in bulk preview at " + threshold);
            e.BuyLevels(7, 1); Check(Near(e.ProductIncome(0), before * 2), "Synergy does not repeat above threshold " + threshold);
            e.State.eligibleEarnings = e.State.target; e.Prestige(e.PreviewPrestige());
            Check(e.State.products.All(n => n == 0) && Near(EconomyBalance.SynergyMultiplier(e.State.products.Min()), 1), "Synergy resets with prestige " + threshold);
        }
        var rich = RedesignState.Fresh(); rich.cash = Amount.Pow10(250); rich.products[0] = 1; rich.cardLevels[8] = rich.cardLevels[17] = rich.cardLevels[27] = 1;
        // Cards require a completed prestige in a valid profile.
        rich.fame = rich.completedPrestiges = 1; rich.target = ProgressionNumbers.RunTarget(1);
        var discounts = new RedesignEngine(data, rich, _ => { }); var productCost = discounts.LevelQuote(0, 1).cost; var tapCost = discounts.LevelQuote(-1, 1).cost;
        discounts.BuyUpgrade("PC-PHU-16"); discounts.BuyUpgrade("PC-PHU-01");
        discounts.BuyUpgrade("TC-40"); discounts.BuyUpgrade("TC-01");
        var upgradeCost = discounts.UpgradeCost("GP-01"); discounts.BuyUpgrade("UC-32"); discounts.BuyUpgrade("UC-01");
        Check(Near(discounts.LevelQuote(0, 1).cost, productCost * .01) && Near(discounts.LevelQuote(-1, 1).cost, tapCost * .01) && Near(discounts.UpgradeCost("GP-01"), upgradeCost * .5), "Strongest discounts survive out-of-order purchases and stack with cards");
        Check(new Amount(1, 3).ToString() == "1K" && new Amount(1, 12).ToString() == "1T" && new Amount(1, 15).ToString() == "1aa" && new Amount(1, 90).ToString() == "1az" && new Amount(1, 93).ToString() == "1ba" && new Amount(1, 2040).ToString() == "1zz" && new Amount(1, 2043).ToString() == "1aaa", "Currency suffixes carry through az/ba and zz/aaa without scientific notation");
        Check(EconomyBalance.MilestoneMultiplier(Catalog.MaxLevel).Valid && EconomyBalance.SynergyMultiplier(Catalog.MaxLevel).Valid, "Cumulative milestones stay in Amount beyond double range");
        string source = File.ReadAllText("Game Design/1000 Unlockables Catalogue.md"), roster = File.ReadAllText("Game Design/Talent and Prestige Draft.md");
        var authored = RedesignContentImporter.Import(source.Replace("PP(PHU,1): product profit x2; factor=2", "PP(PHU,1): product profit x3; factor=3"), roster);
        Check(authored.upgrades.Single(u => u.id == "PP-PHU-01").factor == 3, "Importer preserves authored profit instead of substituting signature defaults");
        string directory = Path.Combine(Path.GetTempPath(), "FasterStartReset-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "game.save");
        var old = RedesignState.Fresh(); old.economy = "2.0.0"; old.contentVersion = "cash-only-v1"; old.cash = 9999;
        void Raw(string file, RedesignState state) { string json = JsonSerializer.Serialize(state, Json); File.WriteAllText(file, SavePayload.Digest(json) + "\n" + json); }
        Raw(path, old); Raw(path + ".bak", old);
        var store = new RedesignSaveStore(path, data, s => JsonSerializer.Serialize(s, Json), s => JsonSerializer.Deserialize<RedesignState>(s, Json), s => JsonSerializer.Deserialize<SaveVersionHeader>(s, Json));
        Check(store.Load().CanCreateFresh, "Known previous local economy allows its one-time reset");
        var current = RedesignState.Fresh(); store.Save(current); current.cash = 75; current.revision += (Natural)1; store.Save(current);
        Check(store.Load().state.cash.CompareTo(75) == 0, "Subsequent launches keep new-economy progress");
        var existing = RedesignState.Fresh(); existing.completedPrestiges = existing.fame = 1;
        existing.revision = current.revision + (Natural)1;
        existing.target = ProgressionNumbers.LegacyFasterStartTarget(1); existing.cash = 12345; existing.cardLevels[16] = 3;
        store.Save(existing); var resumed = store.Load().state;
        Check(resumed != null && resumed.cash.CompareTo(12345) == 0 && resumed.cardLevels[16].Value == 3 && Near(resumed.target, existing.target),
            "Tap calibration preserves existing 3.0.0 cash, cards and in-progress prestige target without a reset");
        var nextRun = new RedesignEngine(data, resumed, store.Save); nextRun.State.eligibleEarnings = nextRun.State.target;
        nextRun.Prestige(nextRun.PreviewPrestige());
        Check(Near(store.Load().state.target, ProgressionNumbers.RunTarget(2)), "The next prestige adopts the calibrated target and persists normally");
        old.schema = 3; Raw(path, old);
        Check(store.Load().status == RedesignLoadStatus.UnsupportedVersion, "Local reset never silently discards a previous authoritative wallet");
    }
}
