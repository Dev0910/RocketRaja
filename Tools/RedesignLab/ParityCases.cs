using System.Text.Json;
using RocketRaja.Core;

static partial class Program
{
    static void ExportParity(RedesignCatalog data)
    {
        double Value(Amount value) => value.mantissa * Math.Pow(10, value.exponent);
        var cases = new List<object>();
        var names = new List<string> { "fresh", "milestones", "band1", "band3", "full", "cards", "rush" };
        foreach (int n in new[] { 0, 1, 19, 148 })
            foreach (int ratio in new[] { 0, 1, 4, 9, 16 })
                foreach (int u in new[] { 249, 250, 500, 1000 }) names.Add($"boundary_{n}_{ratio}_{u}");
        foreach (string name in names)
        {
            var state = RedesignState.Fresh(1000); state.cash = new Amount(1, 80);
            if (name != "fresh") { state.products = new[] { 25, 50, 100, 175, 200, 250, 299, 300 }; state.tapLevel = 50; }
            int band = name == "band1" ? 1 : name == "band3" ? 3 : name == "full" || name == "cards" || name == "rush" ? 20 : 0;
            state.upgrades = data.Upgrades.Where(u => u.band <= band).Select(u => u.id).ToList(); state.everPurchased = new List<string>(state.upgrades);
            if (name == "cards" || name == "rush")
            {
                state.completedPrestiges = 2; state.fame = 9; state.secondPrestigeGift = true; state.kajuKatli = 100;
                state.target = ProgressionNumbers.RunTarget(state.completedPrestiges);
                state.cardLevels = Enumerable.Range(0, 28).Select(i => (Natural)(i % 7 + 1)).ToArray();
            }
            state.eligibleEarnings = name == "fresh" ? Amount.Zero : state.target * 9;
            if (name.StartsWith("boundary_"))
            {
                var parameters = name.Split('_'); int n = int.Parse(parameters[1]), ratio = int.Parse(parameters[2]), u = int.Parse(parameters[3]);
                state.completedPrestiges = state.fame = (Natural)n; state.secondPrestigeGift = n >= 2;
                state.target = ProgressionNumbers.RunTarget(state.completedPrestiges);
                state.eligibleEarnings = state.target * (ratio == 0 ? 1 - 1e-10 : ratio);
                state.everPurchased = data.Upgrades.Take(u).Select(x => x.id).ToList();
            }
            if (name == "rush") state.activeUntil[0] = state.now + 30;
            var engine = new RedesignEngine(data, state, _ => { }); var reward = engine.PreviewPrestige();
            var resetEngine = new RedesignEngine(data, state, _ => { }); object reset = null;
            if (resetEngine.Prestige(resetEngine.PreviewPrestige()))
            {
                var s = resetEngine.State;
                reset = new { fame = s.fame.digits, prestiges = s.completedPrestiges.digits, cash = Value(s.cash), earned = Value(s.eligibleEarnings), target = Value(s.target), unique = s.UniqueCount, owned = s.upgrades.Count, levels = s.products, tapLevel = s.tapLevel, skills = Enumerable.Range(0, 3).Select(i => resetEngine.SkillOwned(i) ? 1 : 0), cards = s.cardLevels.Select(c => c.digits) };
            }
            string[] priceIds = { "PP-PHU-01", "UC-01", "SK-FR-P-01", "PF-MIX-04" };
            cases.Add(new {
                name, levels = state.products, tapLevel = state.tapLevel, owned = state.upgrades, cards = state.cardLevels.Select(x => x.digits),
                fame = state.fame.digits, prestiges = state.completedPrestiges.digits, earned = Value(state.eligibleEarnings), target = Value(state.target), unique = state.UniqueCount, rush = name == "rush", priceIds,
                expected = new {
                    computedTarget = Value(state.target),
                    production = Value(engine.NormalIncome()), products = Enumerable.Range(0, 8).Select(p => Value(engine.ProductIncome(p))),
                    manual = Value(engine.TapPayout()), hands = Value(engine.TapPayout(true)),
                    levelCosts = Enumerable.Range(0, 8).Select(p => new[] { 1, 10, 100 }.Select(q => Value(engine.LevelQuote(p, q).cost))),
                    tapCosts = new[] { 1, 10, 100 }.Select(q => Value(engine.LevelQuote(-1, q).cost)),
                    prices = priceIds.Select(id => Value(engine.UpgradeCost(id))),
                    durations = Enumerable.Range(0, 3).Select(engine.SkillDuration), cooldowns = Enumerable.Range(0, 3).Select(engine.SkillCooldown),
                    fame = reward.Fame.digits, credits = reward.Credits.digits, reset
                }
            });
        }
        File.WriteAllText("Documentation/Implementation/Parity-Inputs.json", JsonSerializer.Serialize(new { contentHash = data.SourceHash, cases }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Exported {cases.Count} runtime/model parity snapshots");
    }
}
