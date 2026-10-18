using RocketRaja.Core;
using System.Text.Json;

static partial class Program
{
    static RedesignState SyntheticCardStart(int fame, string profile)
    {
        var state = RedesignState.Fresh(); state.fame = state.completedPrestiges = fame;
        state.secondPrestigeGift = fame >= 2; state.target = ProgressionNumbers.RunTarget(fame);
        for (int card = 0; card < 28; card++)
            if (profile == "all-cards" || profile == "product-heavy" && card < 16 || profile == "tap-heavy" && (card == 16 || card == 17 || card == 19 || card == 22)) state.cardLevels[card] = 100;
        state.cash = 30 * ProgressionNumbers.CardFactor(CardEffect.ProductCost, state.cardLevels[8]);
        return state;
    }
    // Replay the exact same saved start in each arm, including cards, Fame,
    // discoveries and target. Never compare equal run numbers from diverged careers.
    static void TapBalanceMatrix(RedesignCatalog catalog, string[] args)
    {
        int Option(string key, int fallback) { int at = Array.IndexOf(args, key); return at < 0 ? fallback : int.Parse(args[at + 1]); }
        int runs = Option("--runs", 34), decisions = Option("--decisions", 20);
        var rows = new List<object>(); int failures = 0;
        void Pair(string profile, RedesignState start)
        {
            var options = new[] { "--quiet", "--runs", "1", "--no-cards", "--decisions", decisions.ToString() }
                .Concat(args.Where(a => a == "--levels-first" || a == "--fixed-bulk")).ToArray();
            BalanceSimulation(catalog, options, start); var active = JsonSerializer.SerializeToElement(lastBalanceRow);
            BalanceSimulation(catalog, options.Append("--no-tap").ToArray(), start); var passive = JsonSerializer.SerializeToElement(lastBalanceRow);
            double reduction = 1 - (double)active.GetProperty("seconds").GetInt32() / passive.GetProperty("seconds").GetInt32();
            bool reached = active.GetProperty("reached").GetBoolean() && passive.GetProperty("reached").GetBoolean();
            bool passed = reached && reduction >= .1 && reduction <= .2 && active.GetProperty("productShare").GetDouble() > .5 && passive.GetProperty("productShare").GetDouble() > .5;
            if (!passed) failures++;
            rows.Add(new { profile, fame = start.fame.digits, cards = start.cardLevels.Select(n => n.digits).ToArray(), speedup = reduction, passed, active, withoutManualTaps = passive });
            Console.Error.WriteLine($"{profile} F{start.fame}: {reduction:P1}, products {active.GetProperty("productShare").GetDouble():P1}, pass={passed}");
        }
        if (!args.Contains("--stress-only"))
        {
            foreach (int seed in new[] { 0, 1, 2, 7, 42 })
            {
                var starts = new List<RedesignState>();
                var options = new List<string> { "--quiet", "--runs", runs.ToString(), "--seed", seed.ToString(), "--decisions", decisions.ToString() };
                options.AddRange(args.Where(a => a == "--levels-first" || a == "--fixed-bulk"));
                if (seed == 0) options.Add("--no-cards");
                BalanceSimulation(catalog, options.ToArray(), onRunStart: s => starts.Add(s));
                foreach (var state in starts) Pair(seed == 0 ? "no-cards" : "seed-" + seed, state);
            }
        }
        if (!args.Contains("--no-stress"))
            foreach (int fame in new[] { 1, 5, 10, 20, 33 })
                foreach (string profile in new[] { "tap-heavy", "product-heavy", "all-cards" })
                {
                    Pair(profile, SyntheticCardStart(fame, profile));
                }
        Console.WriteLine(JsonSerializer.Serialize(new { economy = RedesignCatalog.EconomyVersion, balanceRevision = EconomyBalance.Revision, tapFameScales = EconomyBalance.TapFameScales, targetOffsetLogs = EconomyBalance.TargetOffsetLogs, catalogSourceHash = catalog.SourceHash, decisions,
            cardCurve = EconomyBalance.CardCurve, handsPayout = EconomyBalance.HandsPayoutScale,
            cardGlobalGain = double.IsNaN(EconomyBalance.CardGlobalGainOverride) ? (double?)null : EconomyBalance.CardGlobalGainOverride,
            fameIncomeLogs = EconomyBalance.FameIncomeLogs,
            levelsFirst = args.Contains("--levels-first"), fixedBulk = args.Contains("--fixed-bulk"),
            description = "Matched saved starts; one manual tap/second on average; cards include seeded careers and synthetic level-100 stress builds", passed = rows.Count - failures, failed = failures, rows }, new JsonSerializerOptions { WriteIndented = true }));
        if (args.Contains("--verify") && failures > 0) Environment.ExitCode = 1;
    }
}
