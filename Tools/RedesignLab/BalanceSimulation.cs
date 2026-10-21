using RocketRaja.Core;
using System.Text.Json;

static partial class Program
{
    static int lastBalanceSeconds;
    static double lastBalanceShare;
    static double lastProductShare;
    static string[] lastBalanceUpgrades;
    static object lastBalanceRow;
    sealed record BalanceSample(int Seconds, double EarnedLog, double ProductShare, int MaxMeaningfulGap);
    static List<BalanceSample> balanceTrace;
    sealed class SeededRandom : IDrawRandom
    {
        readonly Random random;
        public SeededRandom(int seed) { random = new Random(seed); }
        public int Next(int maximum) => random.Next(maximum);
    }
    // All earnings, purchases, projections, skill events and resets use the production engine.
    static void BalanceSimulation(RedesignCatalog catalog, string[] args, RedesignState startingState = null, Action<RedesignState> onRunStart = null)
    {
        int Option(string key, int fallback) { int at = Array.IndexOf(args, key); return at < 0 ? fallback : int.Parse(args[at + 1]); }
        double Number(string key, double fallback) { int at = Array.IndexOf(args, key); return at < 0 ? fallback : double.Parse(args[at + 1], System.Globalization.CultureInfo.InvariantCulture); }
        EconomyBalance.Calibrate(Number("--tap-base", EconomyBalance.TapBase), Number("--target-growth", EconomyBalance.TargetGrowth), Number("--first-log", EconomyBalance.FirstTarget.Log10), Number("--second-log", EconomyBalance.SecondTarget.Log10));
        EconomyBalance.IncomeLog = Number("--income-log", double.NaN);
        EconomyBalance.TapFameScaleOverride = Number("--tap-fame-scale", double.NaN);
        int decisions = Option("--decisions", 20), runs = Option("--runs", 12), seed = Option("--seed", 1);
        bool tapping = !args.Contains("--no-tap"), cards = !args.Contains("--no-cards"), levelsFirst = args.Contains("--levels-first");
        int push = Option("--push", 1), limit = Option("--limit", 3600);
        double traceLogLimit = Number("--trace-log-limit", double.NaN);
        int start = Option("--start", 0);
        var initial = RedesignState.Fresh(); initial.fame = initial.completedPrestiges = start;
        initial.secondPrestigeGift = start >= 2; initial.target = ProgressionNumbers.RunTarget(start);
        if (startingState != null) initial = startingState.Copy();
        var engine = new RedesignEngine(catalog, initial, _ => { }, new SeededRandom(seed));
        var results = new List<object>();
        for (int run = 0; run < runs; run++)
        {
            onRunStart?.Invoke(engine.State.Copy());
            engine.BuyLevels(0, 1);
            int firstEight = -1, firstMilestone = -1, firstSkill = -1, lastUseful = 0, lastUpgrade = 0;
            var usefulGaps = new List<int>(); var cashGaps = new List<int>();
            Amount manual = 0, productEarnings = 0; int seconds;
            for (seconds = 0; seconds < limit; seconds++)
            {
                if (seconds > 0 && seconds % decisions == 0)
                {
                    var beforeIncome = engine.NormalIncome() + engine.TapPayout();
                    int bought = 0;
                    void Cash() { var q = engine.QuoteBuyAll(); if (q.Ids.Count > 0 && engine.CommitBuyAll(q)) bought = q.Ids.Count; }
                    void Levels()
                    {
                        double tapWeight = (tapping ? 1 : 0) + (engine.SkillOwned(1) ? EconomyBalance.HandsPayoutScale * engine.PreviewSkill(1).power * engine.SkillDuration(1) / engine.SkillCooldown(1) : 0);
                        for (int attempt = 0; attempt < 2; attempt++)
                        {
                            int selected = -2, count = 0; double best = double.NegativeInfinity;
                            for (int p = 0; p < 9; p++)
                            {
                                int product = p == 8 ? -1 : p;
                                if (p == 8 && tapWeight == 0 || p < 8 && !engine.ProductAvailable(p)) continue;
                                int level = p == 8 ? engine.State.tapLevel : engine.State.products[p];
                                int q = level == 0 ? 1 : engine.LevelQuote(product, 100).affordable ? 100 : engine.LevelQuote(product, 10).affordable ? 10 : 1;
                                if (!args.Contains("--fixed-bulk") && level > 0) q = Math.Max(1, engine.LevelQuote(product, int.MaxValue).quantity);
                                var preview = engine.PreviewLevels(product, q); if (!preview.quote.affordable) continue;
                                var delta = preview.normalAfter - preview.normalBefore;
                                delta += (preview.tapAfter - preview.tapBefore) * tapWeight;
                                double score = delta.Log10 - preview.quote.cost.Log10;
                                if (level == 0 && p < 8) score = 1e9;
                                if (score > best) { best = score; selected = product; count = q; }
                            }
                            if (selected == -2) break;
                            engine.BuyLevels(selected, count);
                        }
                    }
                    if (levelsFirst) { Levels(); Cash(); } else { Cash(); Levels(); }
                    for (int skill = 0; skill < 3; skill++)
                    {
                        var earned = engine.State.eligibleEarnings;
                        engine.ActivateSkill(skill);
                        if (skill == 2) productEarnings += engine.State.eligibleEarnings - earned;
                    }
                    if (firstEight < 0 && engine.State.products.All(n => n > 0)) firstEight = seconds;
                    if (firstMilestone < 0 && engine.State.products.Any(n => n >= 10)) firstMilestone = seconds;
                    if (firstSkill < 0 && engine.SkillOwned(0)) firstSkill = seconds;
                    if (bought > 0) { cashGaps.Add(seconds - lastUpgrade); lastUpgrade = seconds; }
                    // A meaningful action is at least 10% more baseline earning power, or a named upgrade.
                    if (bought > 0 || engine.NormalIncome() + engine.TapPayout() >= beforeIncome * 1.1)
                    { usefulGaps.Add(seconds - lastUseful); lastUseful = seconds; }
                }
                double rushSeconds = Math.Max(0, Math.Min(1, engine.State.activeUntil[0] - engine.State.now));
                double rushPower = rushSeconds > 0 ? engine.PreviewSkill(0).power : 1;
                productEarnings += engine.NormalIncome() * (1 + rushSeconds * (rushPower - 1));
                engine.Advance(1);
                if (tapping && seconds % decisions >= decisions / 2)
                    for (int tap = 0; tap < 2; tap++) { manual += engine.TapPayout(); engine.Tap(); }
                balanceTrace?.Add(new BalanceSample(seconds + 1, engine.State.eligibleEarnings.Log10, Amount.Ratio(productEarnings, engine.State.eligibleEarnings), Math.Max(seconds + 1 - lastUseful, usefulGaps.Count == 0 ? 0 : usefulGaps.Max())));
                if (!double.IsNaN(traceLogLimit) && engine.State.eligibleEarnings.Log10 >= traceLogLimit) { seconds++; break; }
                if (!args.Contains("--trace-full") && engine.State.eligibleEarnings >= engine.State.target * push) { seconds++; break; }
            }
            usefulGaps.Add(seconds - lastUseful); cashGaps.Add(seconds - lastUpgrade);
            var quote = engine.PreviewPrestige();
            var row = new { run = run + 1, seconds, reached = quote.Eligible, firstEight, firstMilestone, firstSkill,
                upgrades = engine.State.upgrades.Count, unique = engine.State.UniqueCount, fame = engine.State.fame.digits,
                rewardFame = quote.Fame.digits, credits = quote.Credits.digits, targetLog10 = engine.State.target.Log10,
                earnedLog10 = engine.State.eligibleEarnings.Log10, manualShare = Amount.Ratio(manual, engine.State.eligibleEarnings),
                productShare = Amount.Ratio(productEarnings, engine.State.eligibleEarnings),
                handsShare = Math.Max(0, 1 - Amount.Ratio(manual + productEarnings, engine.State.eligibleEarnings)),
                productLevels = engine.State.products.ToArray(), tapLevel = engine.State.tapLevel,
                tapUpgrades = catalog.Upgrades.Count(u => u.effect == CashEffect.TapDirect && engine.State.upgrades.Contains(u.id)),
                productUpgrades = catalog.Upgrades.Count(u => u.effect == CashEffect.ProductProfit && engine.State.upgrades.Contains(u.id)),
                handsAverageTapsPerSecond = engine.SkillOwned(1) ? engine.PreviewSkill(1).power * engine.SkillDuration(1) / engine.SkillCooldown(1) : 0,
                maxMeaningfulGap = usefulGaps.Max(), maxUpgradeGap = cashGaps.Max(),
                p90MeaningfulGap = usefulGaps.OrderBy(n => n).ElementAt(Math.Min(usefulGaps.Count - 1, (int)(usefulGaps.Count * .9))) };
            lastBalanceSeconds = seconds;
            lastBalanceRow = row;
            lastBalanceShare = Amount.Ratio(manual, engine.State.eligibleEarnings);
            lastProductShare = Amount.Ratio(productEarnings, engine.State.eligibleEarnings);
            lastBalanceUpgrades = engine.State.upgrades.ToArray();
            results.Add(row); if (!args.Contains("--quiet")) Console.Error.WriteLine(JsonSerializer.Serialize(row));
            if (!quote.Eligible || !engine.Prestige(quote)) break;
            if (cards) while (engine.CanDraw(true)) { engine.Draw(engine.QuoteDraw(true)); engine.DismissReveal(engine.State.reveal.operationId); }
        }
        if (!args.Contains("--quiet")) Console.WriteLine(JsonSerializer.Serialize(new { economy = RedesignCatalog.EconomyVersion, balanceRevision = EconomyBalance.Revision, catalogSourceHash = catalog.SourceHash, cardCurve = EconomyBalance.CardCurve, handsPayout = EconomyBalance.HandsPayoutScale, decisions, tapping, cards, levelsFirst, maxLevels = !args.Contains("--fixed-bulk"), seed, push, tapBase = EconomyBalance.TapBase, productCostGrowth = EconomyBalance.ProductCostGrowth, firstTargetLog10 = EconomyBalance.FirstTarget.Log10, secondTargetLog10 = EconomyBalance.SecondTarget.Log10, fameIncomeLogs = EconomyBalance.FameIncomeLogs, tapFameScales = EconomyBalance.TapFameScales, targetOffsetLogs = EconomyBalance.TargetOffsetLogs, results }, new JsonSerializerOptions { WriteIndented = true }));
    }
    static void FitPrestige(RedesignCatalog catalog)
    {
        var logs = new List<double> { 0, Math.Log10(6) };
        for (int stage = 2; stage <= 34; stage++)
        {
            // Search fixed authored income values, never change a player's live prices or rewards.
            double low = logs.Last(), high = stage * 5;
            for (int step = 0; step < 14; step++)
            {
                double mid = (low + high) / 2;
                BalanceSimulation(catalog, new[] { "--quiet", "--runs", "1", "--no-cards", "--start", stage.ToString(), "--income-log", mid.ToString(System.Globalization.CultureInfo.InvariantCulture), "--limit", "2100" });
                if (lastBalanceSeconds > 1350) low = mid; else high = mid;
            }
            logs.Add(Math.Round(high, 5));
            Console.WriteLine(stage + ": " + logs.Last().ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        File.WriteAllText("Documentation/FasterStartPrestigeFit.json", JsonSerializer.Serialize(logs));
    }
    static void PairedBalance(RedesignCatalog catalog)
    {
        var pairs = new List<object>();
        for (int stage = 0; stage < 34; stage++)
        {
            var arguments = new[] { "--quiet", "--runs", "1", "--no-cards", "--start", stage.ToString() };
            BalanceSimulation(catalog, arguments); var active = lastBalanceRow;
            BalanceSimulation(catalog, arguments.Concat(new[] { "--no-tap" }).ToArray());
            pairs.Add(new { fame = stage, active, withoutManualTaps = lastBalanceRow });
        }
        Console.WriteLine(JsonSerializer.Serialize(new { catalogSourceHash = catalog.SourceHash, description = "Matched starting Fame, zero cards, same decision policy and skills; only manual tapping differs", pairs }, new JsonSerializerOptions { WriteIndented = true }));
    }
    static void FitTaps(RedesignCatalog catalog)
    {
        int locked = 16;
        for (int stage = 2; stage <= 34; stage++)
        {
            int next = locked;
            double bestError = double.PositiveInfinity; double[] best = null;
            var taps = catalog.Upgrades.Where(u => u.effect == CashEffect.TapDirect).ToArray();
            for (int step = 0; step < 7; step++)
            {
                BalanceSimulation(catalog, new[] { "--quiet", "--runs", "1", "--no-cards", "--start", stage.ToString(), "--limit", "2400" });
                double error = Math.Abs(Math.Log(Math.Max(.000001, lastBalanceShare) / .06)) + 10 * Math.Max(0, .6 - lastProductShare);
                if (error < bestError) { bestError = error; best = taps.Select(u => u.factor).ToArray(); }
                var editable = catalog.Upgrades.Where(u => u.effect == CashEffect.TapDirect && u.rank > locked && lastBalanceUpgrades.Contains(u.id)).ToArray();
                if (editable.Length == 0) break;
                next = editable.Max(u => u.rank);
                if (lastBalanceShare >= .04 && lastBalanceShare <= .1 && lastProductShare >= .6) break;
                double adjustment = Math.Pow(.06 / Math.Max(.000001, lastBalanceShare), .5 / editable.Length);
                foreach (var u in editable) u.factor = Math.Round(Math.Max(2, Math.Min(32, u.factor * adjustment)), 8);
            }
            if (best != null) for (int i = 0; i < taps.Length; i++) taps[i].factor = best[i];
            locked = next;
            Console.WriteLine(stage + ": rank " + locked + ", manual share " + lastBalanceShare);
        }
        string path = "Game Design/1000 Unlockables Catalogue.md", source = File.ReadAllText(path);
        source = System.Text.RegularExpressions.Regex.Replace(source, @"^\| TP-\d+ .*?$", match => {
            string id = match.Value.Split('|')[1].Trim(); var u = catalog.Upgrades.Single(x => x.id == id);
            string factor = u.factor.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            return System.Text.RegularExpressions.Regex.Replace(match.Value, @"tap profit x[0-9.]+; factor=[0-9.]+", "tap profit x" + factor + "; factor=" + factor);
        }, System.Text.RegularExpressions.RegexOptions.Multiline);
        File.WriteAllText(path, source);
    }
}
