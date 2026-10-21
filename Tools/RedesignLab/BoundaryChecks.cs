using RocketRaja.Core;

static partial class Program
{
    static void BoundaryChecks(RedesignCatalog data)
    {
        RedesignEngine Make(RedesignState s) => new(data, s, _ => { });
        var rich = RedesignState.Fresh(1000); rich.cash = new Amount(1, 250); rich.products = Enumerable.Repeat(300, 8).ToArray();
        var all = Make(rich); all.CommitBuyAll(all.QuoteBuyAll());
        var full = all.State.Copy(); full.completedPrestiges = full.fame = 1; full.target = ProgressionNumbers.RunTarget(1);
        var baseline = Make(full);
        foreach (int level in new[] { 1, 2, 100 })
            for (int index = 0; index < 28; index++)
            {
                var s = full.Copy(); s.cardLevels[index] = (Natural)level; var e = Make(s); var card = data.Cards[index];
                bool ok = true;
                for (int p = 0; p < 8; p++)
                {
                    double profit = index == p ? 1 + .2 * level : index == 26 ? 1 + .25 * level : 1;
                    double cost = index == p + 8 ? 1 - .4 * level / (level + 4d) : 1;
                    ok &= Near(e.ProductIncome(p), baseline.ProductIncome(p) * profit) && Near(e.LevelQuote(p, 1).cost, baseline.LevelQuote(p, 1).cost * cost);
                }
                ok &= Near(e.LevelQuote(-1, 10).cost, baseline.LevelQuote(-1, 10).cost * (index == 17 ? 1 - .4 * level / (level + 4d) : 1));
                ok &= Near(e.UpgradeCost("PP-PHU-01"), baseline.UpgradeCost("PP-PHU-01") * (index == 27 ? 1 - .5 * level / (level + 4d) : 1));
                for (int skill = 0; skill < 3; skill++)
                {
                    ok &= Math.Abs(e.SkillCooldown(skill) / baseline.SkillCooldown(skill) - (index == skill + 18 ? 1 - .3 * level / (level + 5d) : 1)) < 1e-12;
                    if (skill < 2) ok &= Math.Abs(e.SkillDuration(skill) / baseline.SkillDuration(skill) - (index == skill + 21 ? 1 + .75 * level / (level + 5d) : 1)) < 1e-12;
                }
                if (index == 16) ok &= Near(e.TapPayout(), baseline.TapPayout() * (1 + level / (level + 5d))) && Near(e.TapPayout(true), baseline.TapPayout(true) * (1 + level / (level + 5d)));
                if (index == 23)
                {
                    Amount before = e.State.eligibleEarnings; e.ActivateSkill(2);
                    ok &= Near(e.State.eligibleEarnings - before, e.NormalIncome() * 300 * (1 + level / (level + 5d)));
                }
                if (index == 24 || index == 25)
                {
                    e.Save(); e.ResolveOffline(101000);
                    double expectedSeconds = (index == 25 ? 8 + 12 * level / (level + 5d) : 8) * 3600;
                    double efficiency = index == 24 ? .5 + .5 * level / (level + 5d) : .5;
                    ok &= Math.Abs(e.State.offline.creditedSeconds - expectedSeconds) < 1e-7 && Near(e.State.offline.amount, e.NormalIncome() * expectedSeconds * efficiency);
                }
                Check(ok, $"{card.id} level {level} affects its specified targets/curve without changing unrelated products, costs or skills");
            }
        foreach (int n in new[] { 0, 1, 7, 19, 147, 148 })
            foreach (int ratio in new[] { 1, 4, 9, 16 })
            {
                var s = RedesignState.Fresh(); s.completedPrestiges = s.fame = (Natural)n; s.secondPrestigeGift = n >= 2; s.target = ProgressionNumbers.RunTarget((Natural)n);
                s.everPurchased = data.Upgrades.Take(500).Select(x => x.id).ToList();
                s.eligibleEarnings = s.target * ratio;
                var e = Make(s); var quote = e.PreviewPrestige();
                int expectedCredits = n == 0 ? 3 : 1 + (ratio >= 16 ? 2 : ratio >= 4 ? 1 : 0) + (ratio >= 4 ? 2 : 0);
                Check(quote.Fame.Value == (int)Math.Sqrt(ratio) && quote.Credits.Value == expectedCredits, $"Prestige threshold {ratio}T at completed count {n}");
                var adjacent = s.Copy(); adjacent.eligibleEarnings = new Amount(Math.BitDecrement(s.eligibleEarnings.mantissa), s.eligibleEarnings.exponent);
                Check(Make(adjacent).PreviewPrestige().Fame.Value == (int)Math.Sqrt(ratio) - 1, $"One representable step below {ratio}T at count {n} receives no early Fame");
                s.eligibleEarnings *= 1 - 1e-10;
                e = Make(s); quote = e.PreviewPrestige();
                Check(quote.Fame.Value == (int)Math.Sqrt(ratio) - 1, $"Immediately below Fame threshold {ratio}T at count {n} stays below");
            }
        var milestone = rich.Copy(); milestone.products[0] = 24; var one = Make(milestone); milestone.cash = one.LevelQuote(0, 1).cost;
        one = Make(milestone);
        Check(one.LevelQuote(0, int.MaxValue).quantity == 1 && one.BuyLevels(0, int.MaxValue) && one.State.products[0] == 25 && Near(one.ProductIncome(0), .85 * 25 * 4), "Max crosses the level-25 milestone using exactly affordable cash");
        var high = full.Copy(); high.products[0] = Catalog.MaxLevel - 1; var highest = Make(high); var price = highest.LevelQuote(0, 1).cost;
        Check(price.Valid && highest.LevelQuote(0, 100).quantity == 1 && highest.ProductIncome(0).Valid, "High levels retain finite large-number quotes and clamp bulk quantity at storage limit");
        var qState = rich.Copy(); qState.products[0] = 1; qState.cash = 500;
        var qEngine = Make(qState); var reviewed = qEngine.QuoteBuyAll(); qEngine.BuyLevels(0, 1);
        Check(!qEngine.CommitBuyAll(reviewed), "A level purchase invalidates an earlier frozen cash batch");
        var projection = Make(rich).PreviewUpgrade("PP-PHU-01");
        Check(projection != null && projection.productsBefore.Length == 8, "Per-family contribution projections are available to screen bindings");
    }
}
