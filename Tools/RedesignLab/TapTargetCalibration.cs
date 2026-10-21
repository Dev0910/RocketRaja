using RocketRaja.Core;
using System.Text.Json;

static partial class Program
{
    static void FitTapTargets(RedesignCatalog catalog, string[] args)
    {
        int at = Array.IndexOf(args, "--fame"); int onlyFame = at < 0 ? -1 : int.Parse(args[at + 1]);
        int gapAt = Array.IndexOf(args, "--max-gap"); int maxGap = gapAt < 0 ? int.MaxValue : int.Parse(args[gapAt + 1]);
        int incomeAt = Array.IndexOf(args, "--income-offset"); double incomeOffset = incomeAt < 0 ? 0 : double.Parse(args[incomeAt + 1], System.Globalization.CultureInfo.InvariantCulture);
        double originalTap = EconomyBalance.TapBase;
        var starts = new Dictionary<int, List<RedesignState>>();
        int cacheAt = Array.IndexOf(args, "--cache-path");
        string cache = cacheAt < 0 ? "Temp/tap-calibration-starts.json" : args[cacheAt + 1];
        // Explicit synthetic fixtures test whether common fixed constants can serve
        // extreme collections too. They do not represent naturally earned careers.
        string[] stressProfiles = { "no-cards", "tap-heavy", "product-heavy", "all-cards" };
        if (args.Contains("--stress-only"))
        {
            foreach (int fame in onlyFame < 0 ? new[] { 1, 5, 10, 20, 33 } : new[] { onlyFame })
                starts[fame] = stressProfiles.Select(p => SyntheticCardStart(fame, p)).ToList();
        }
        else if (args.Contains("--reuse-starts"))
        {
            starts = JsonSerializer.Deserialize<Dictionary<int, List<RedesignState>>>(File.ReadAllText(cache), Json);
            if (onlyFame >= 0) starts = starts.Where(p => p.Key == onlyFame).ToDictionary(p => p.Key, p => p.Value);
        }
        else
        {
        foreach (int seed in new[] { 0, 1, 2, 7, 42 })
        {
            var options = new List<string> { "--quiet", "--runs", (onlyFame < 0 ? 34 : onlyFame + 1).ToString(), "--seed", seed.ToString() };
            if (seed == 0) options.Add("--no-cards");
            BalanceSimulation(catalog, options.ToArray(), onRunStart: s => {
                int fame = (int)s.fame.Value;
                if (fame >= 34 || onlyFame >= 0 && fame != onlyFame) return;
                if (!starts.ContainsKey(fame)) starts[fame] = new();
                if (fame != 0 || starts[fame].Count == 0) starts[fame].Add(s);
            });
        }
        if (args.Contains("--cache-starts"))
        { Directory.CreateDirectory("Temp"); File.WriteAllText(cache, JsonSerializer.Serialize(starts, Json)); return; }
        }
        int famesAt = Array.IndexOf(args, "--fames");
        if (famesAt >= 0)
        {
            var selected = args[famesAt + 1].Split(',').Select(int.Parse).ToHashSet();
            starts = starts.Where(p => selected.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value);
        }
        if (args.Contains("--include-stress") && !args.Contains("--stress-only"))
            foreach (int fame in new[] { 1, 5, 10, 20, 33 })
                if (starts.TryGetValue(fame, out var builds))
                    builds.AddRange(stressProfiles.Skip(1).Select(p => SyntheticCardStart(fame, p)));
        int scalesAt = Array.IndexOf(args, "--scales");
        double[] scales = scalesAt < 0 ? new[] { .03, .1, .3, .6, 1d, 1.5, 2, 3, 5, 10, 30 } : args[scalesAt + 1].Split(',').Select(s => double.Parse(s, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        BalanceSample At(List<BalanceSample> trace, double log)
        {
            int lo = 0, hi = trace.Count;
            while (lo < hi) { int mid = (lo + hi) / 2; if (trace[mid].EarnedLog < log) lo = mid + 1; else hi = mid; }
            return lo == trace.Count ? null : trace[lo];
        }
        var fits = new List<object>();
        double previousTapLog = double.NegativeInfinity;
        foreach (var stage in starts.OrderBy(p => p.Key))
        {
            double current = ProgressionNumbers.LegacyFasterStartTarget(stage.Value[0].completedPrestiges, stage.Value[0].fame).Log10, bestLoss = double.PositiveInfinity, bestLog = current, bestScale = 1;
            object best = null, closest = null; double closestError = double.PositiveInfinity;
            int candidateCount = 0, feasibleCount = 0, bestPassingBuilds = 0;
            foreach (double scale in args.Contains("--grid") && stage.Key > 0 ? scales : new[] { 1d })
            {
            if (EconomyBalance.FameIncome(stage.Key).Log10 + Math.Log10(scale) < previousTapLog) continue;
            var pairs = new List<(List<BalanceSample> active, List<BalanceSample> passive)>();
            foreach (var s in stage.Value)
            {
                // Cached targets are normalized to the loaded candidate for state
                // validation. Full traces still ignore prestige eligibility.
                s.target = ProgressionNumbers.RunTarget(s.completedPrestiges, s.fame);
                if (args.Contains("--verbose")) Console.Error.WriteLine($"Starting F{stage.Key}, scale {scale}, build {pairs.Count}");
                // Once the highest searched target has been crossed, later samples
                // cannot affect any comparison. Keep the first crossing sample.
                var options = new[] { "--quiet", "--runs", "1", "--no-cards", "--trace-full", "--trace-log-limit", (current + 6).ToString(System.Globalization.CultureInfo.InvariantCulture), "--limit", "2400", "--tap-fame-scale", "1", "--income-log", (EconomyBalance.FameIncome(stage.Key).Log10 + incomeOffset).ToString(System.Globalization.CultureInfo.InvariantCulture), "--tap-base", (originalTap * scale).ToString(System.Globalization.CultureInfo.InvariantCulture) };
                balanceTrace = new(); BalanceSimulation(catalog, options, s); var active = balanceTrace;
                if (args.Contains("--verbose")) Console.Error.WriteLine("Active complete");
                balanceTrace = new(); BalanceSimulation(catalog, options.Append("--no-tap").ToArray(), s); var passive = balanceTrace;
                balanceTrace = null; pairs.Add((active, passive));
            }
            for (int k = -300; k <= 600; k++)
            {
                double log = current + k * .01;
                var comparisons = pairs.Select(p => (a: At(p.active, log), b: At(p.passive, log))).ToArray();
                if (comparisons.Any(p => p.a == null || p.b == null)) continue;
                candidateCount++;
                int passingBuilds = comparisons.Count(p => {
                    double speed = 1 - (double)p.a.Seconds / p.b.Seconds;
                    return speed >= .1 && speed <= .2 && p.a.ProductShare > .5 && p.b.ProductShare > .5;
                });
                if (passingBuilds == comparisons.Length) feasibleCount++;
                bestPassingBuilds = Math.Max(bestPassingBuilds, passingBuilds);
                int primaryMinimum = stage.Key == 0 ? 1200 : stage.Key == 1 ? 600 : stage.Key == 33 ? 300 : 900;
                int primaryMaximum = stage.Key == 1 ? 900 : 1800;
                if (args.Contains("--require-primary-pace") && (comparisons[0].a.Seconds < primaryMinimum || comparisons[0].a.Seconds > primaryMaximum)) continue;
                if (args.Contains("--require-pace") && comparisons.Any(p => p.a.Seconds < (stage.Key == 1 ? 600 : 900) || p.a.Seconds > (stage.Key == 1 ? 900 : 1800))) continue;
                if (args.Contains("--require-gaps") && comparisons.Any(p => p.a.MaxMeaningfulGap > maxGap || p.b.MaxMeaningfulGap > maxGap)) continue;
                double ratioError = comparisons.Sum(p => {
                    double speed = 1 - (double)p.a.Seconds / p.b.Seconds;
                    return Math.Max(0, .1 - speed) + Math.Max(0, speed - .2) + Math.Max(0, .500001 - Math.Min(p.a.ProductShare, p.b.ProductShare));
                });
                if (ratioError < closestError)
                {
                    closestError = ratioError;
                    closest = new { tapScale = scale, targetLog = log, incomeOffset, ratioError, passingBuilds,
                        comparisons = comparisons.Select(p => new { activeSeconds = p.a.Seconds, passiveSeconds = p.b.Seconds,
                            speedup = 1 - (double)p.a.Seconds / p.b.Seconds, products = p.a.ProductShare, passiveProducts = p.b.ProductShare }).ToArray() };
                }
                if (args.Contains("--require-ratios") && comparisons.Any(p => {
                    double speed = 1 - (double)p.a.Seconds / p.b.Seconds;
                    return speed < .1 || speed > .2 || p.a.ProductShare <= .5 || p.b.ProductShare <= .5;
                })) continue;
                double loss = 0;
                for (int build = 0; build < comparisons.Length; build++)
                {
                    var p = comparisons[build];
                    double speed = 1 - (double)p.a.Seconds / p.b.Seconds;
                    loss += 100 * Math.Max(0, .1 - speed) + 100 * Math.Max(0, speed - .2);
                    loss += 100 * Math.Max(0, .55 - Math.Min(p.a.ProductShare, p.b.ProductShare));
                    bool synthetic = args.Contains("--stress-only") ? build > 0 : args.Contains("--include-stress") && new[] { 1, 5, 10, 20, 33 }.Contains(stage.Key) && build >= 5;
                    // Level-100 collections at low Fame are stress fixtures, not
                    // typical careers. Strong permanent cards may shorten them.
                    int min = synthetic ? 0 : primaryMinimum;
                    int max = stage.Key == 1 ? 900 : 1800;
                    loss += Math.Max(0, min - p.a.Seconds) / 300d + Math.Max(0, p.a.Seconds - max) / 300d;
                    loss += Math.Max(0, p.a.MaxMeaningfulGap - maxGap) / 60d;
                }
                loss += .001 * Math.Abs(k);
                if (loss < bestLoss)
                {
                    bestLoss = loss; bestLog = log; bestScale = scale;
                    best = comparisons.Select(p => new { activeSeconds = p.a.Seconds, passiveSeconds = p.b.Seconds, speedup = 1 - (double)p.a.Seconds / p.b.Seconds, products = p.a.ProductShare,
                        passiveProducts = p.b.ProductShare, activeMaxGap = p.a.MaxMeaningfulGap, passiveMaxGap = p.b.MaxMeaningfulGap }).ToArray();
                }
            }
            Console.Error.WriteLine($"F{stage.Key} scale {scale}: best loss {bestLoss:F3}");
            }
            if (best == null) { fits.Add(new { fame = stage.Key, found = false, candidateCount, feasibleCount, bestPassingBuilds, closest }); continue; }
            fits.Add(new { fame = stage.Key, current, targetLog = bestLog, tapScale = bestScale, incomeOffset, loss = bestLoss, candidateCount, feasibleCount, bestPassingBuilds, comparisons = best });
            previousTapLog = EconomyBalance.FameIncome(stage.Key).Log10 + Math.Log10(bestScale);
            Directory.CreateDirectory("Documentation/TapBalanceExperiments");
            string experimentPrefix = EconomyBalance.CardCurve == "original" && EconomyBalance.HandsPayoutScale == 1 ? "" : EconomyBalance.CardCurve + "-HH" + EconomyBalance.HandsPayoutScale.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-";
            if (!double.IsNaN(EconomyBalance.CardGlobalGainOverride)) experimentPrefix += "G" + EconomyBalance.CardGlobalGainOverride.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-";
            if (famesAt >= 0) experimentPrefix += "Fames" + args[famesAt + 1].Replace(',', '-') + "-";
            string progressName = experimentPrefix + (args.Contains("--stress-only") ? "Stress-" : "") + (onlyFame < 0 ? "TapGridProgress.json" : "TapGridStage" + onlyFame + "-income-" + incomeOffset.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".json");
            File.WriteAllText(Path.Combine("Documentation/TapBalanceExperiments", progressName), JsonSerializer.Serialize(fits, new JsonSerializerOptions { WriteIndented = true }));
            Console.Error.WriteLine($"F{stage.Key}: target {bestLog:F3}, delta {bestLog-current:F2}, tap {bestScale}, loss {bestLoss:F3}");
        }
        Console.WriteLine(JsonSerializer.Serialize(new { catalogSourceHash = catalog.SourceHash, balanceRevision = EconomyBalance.Revision,
            cardCurve = EconomyBalance.CardCurve, handsPayout = EconomyBalance.HandsPayoutScale,
            cardGlobalGain = double.IsNaN(EconomyBalance.CardGlobalGainOverride) ? (double?)null : EconomyBalance.CardGlobalGainOverride,
            profiles = args.Contains("--stress-only") ? stressProfiles : null,
            description = "Offline finite search, not proof of global feasibility or impossibility. No runtime constants or authored effects are changed.",
            scales, targetOffsetRange = new[] { -3d, 6d }, targetOffsetStep = .01, traceLimitSeconds = 2400, fits }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
