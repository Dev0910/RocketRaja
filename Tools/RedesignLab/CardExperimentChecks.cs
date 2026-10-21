using RocketRaja.Core;

static partial class Program
{
    // Semantic checks for the lab-only redesign. Existing production checks remain
    // unchanged and continue testing the currently shipped card/skill formulas.
    static void CardExperimentChecks(RedesignCatalog catalog)
    {
        foreach (CardEffect effect in Enum.GetValues<CardEffect>())
        {
            var levels = new[] { 0, 1, 5, 25, 100, 1000 };
            bool discount = effect == CardEffect.ProductCost || effect == CardEffect.TapCost || effect == CardEffect.UpgradeCost || effect == CardEffect.SkillCooldown;
            for (int i = 1; i < levels.Length; i++)
            {
                var before = ProgressionNumbers.CardMultiplier(effect, levels[i - 1]);
                var after = ProgressionNumbers.CardMultiplier(effect, levels[i]);
                Check(after.Valid && after > Amount.Zero && (discount ? after <= before : after >= before), $"{effect} retains positive, beneficial progression to level {levels[i]}");
            }
        }
        var upgrade = catalog.Upgrades.First(u => u.effect == CashEffect.TapDirect && u.factor == 32);
        foreach (int fame in new[] { 1, 5, 10, 20, 33 })
            foreach (string profile in new[] { "tap-heavy", "product-heavy", "all-cards" })
            {
                var state = SyntheticCardStart(fame, profile); state.products = Enumerable.Repeat(200, 8).ToArray(); state.tapLevel = 200; state.cash = Amount.Pow10(250);
                var engine = new RedesignEngine(catalog, state, _ => { });
                var manual = engine.TapPayout(); var hands = engine.TapPayout(true); var income = engine.NormalIncome();
                var preview = engine.PreviewUpgrade(upgrade.id);
                Check(engine.BuyUpgrade(upgrade.id) && Near(engine.TapPayout(), manual * 32) && Near(engine.TapPayout(true), hands * 32), $"Full x32 manual and Hands upgrade: {profile}, Fame {fame}");
                Check(Near(engine.NormalIncome(), income) && Near(engine.TapPayout(), preview.manualTapAfter), $"Tap upgrade does not change product income; preview agrees: {profile}, Fame {fame}");
                manual = engine.TapPayout(); engine.BuyLevels(0, 100);
                Check(Near(engine.TapPayout(), manual), $"Tap payout remains independent of product levels: {profile}, Fame {fame}");
            }
        var fresh = RedesignState.Fresh(); fresh.cash = Amount.Pow10(20); fresh.products[0] = 1;
        var oneStep = new RedesignEngine(catalog, fresh, _ => { });
        string skill = catalog.Upgrades.First(u => u.effect == CashEffect.SkillPower && u.target == 1 && u.rank == 1).id;
        oneStep.BuyUpgrade(skill); oneStep.ActivateSkill(1);
        var split = new RedesignEngine(catalog, oneStep.State.Copy(), _ => { });
        var earned = oneStep.State.eligibleEarnings;
        var expected = oneStep.NormalIncome() * 30 + oneStep.TapPayout(true) * 90;
        oneStep.Advance(30); for (int i = 0; i < 300; i++) split.Advance(.1);
        Check(Near(oneStep.State.eligibleEarnings - earned, expected), "Helping Hands uses its separate base for all 90 base-skill payouts");
        Check(Near(oneStep.State.eligibleEarnings, split.State.eligibleEarnings), "Separate Hands base preserves frame-independent event accrual");
        Console.WriteLine($"All {checks} experiment checks passed. Curve={EconomyBalance.CardCurve}; Hands base={EconomyBalance.HandsPayoutScale}.");
    }
}
