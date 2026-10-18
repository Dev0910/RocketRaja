using System.Numerics;
using System.Text.Json;
using RocketRaja.Core;
using RocketRaja.Editor;

static partial class Program
{
    static readonly JsonSerializerOptions Json = new() { IncludeFields = true, IgnoreReadOnlyProperties = true, WriteIndented = true };
    static int checks;
    static void Check(bool ok, string text) { if (!ok) throw new Exception("FAIL " + text); checks++; Console.WriteLine("PASS " + text); }
    static T Clone<T>(T input) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(input, Json), Json);
    static void Reject(Action action, string text) { bool rejected = false; try { action(); } catch (Exception e) when (e is InvalidDataException || e is FormatException || e is ArgumentException) { rejected = true; } Check(rejected, text); }
    static void Main(string[] args)
    {
        var data = RedesignContentImporter.Import(File.ReadAllText("Game Design/1000 Unlockables Catalogue.md"), File.ReadAllText("Game Design/Talent and Prestige Draft.md"));
        if (args.Contains("--export") || args.Contains("--export-only")) File.WriteAllText("Documentation/RedesignContentCatalog.json", JsonSerializer.Serialize(data, Json) + "\n");
        var catalog = new RedesignCatalog(data);
        int candidateAt = Array.IndexOf(args, "--candidate");
        if (candidateAt >= 0)
        {
            using var candidate = JsonDocument.Parse(File.ReadAllText(args[candidateAt + 1]));
            var root = candidate.RootElement;
            EconomyBalance.CardCurve = root.GetProperty("cardCurve").GetString();
            EconomyBalance.HandsPayoutScale = root.GetProperty("handsPayout").GetDouble();
            if (root.TryGetProperty("cardGlobalGain", out var globalGain)) EconomyBalance.CardGlobalGainOverride = globalGain.GetDouble();
            void Table(string name, double[] destination, bool positive)
            {
                var values = root.GetProperty(name).EnumerateArray().Select(v => v.GetDouble()).ToArray();
                if (values.Length != destination.Length || values.Any(v => !double.IsFinite(v) || positive && v <= 0))
                    throw new ArgumentException("Invalid candidate table: " + name);
                Array.Copy(values, destination, values.Length);
            }
            Table("tapFameScales", EconomyBalance.TapFameScales, true);
            Table("targetOffsetLogs", EconomyBalance.TargetOffsetLogs, false);
            if (root.TryGetProperty("fameIncomeLogs", out _)) Table("fameIncomeLogs", EconomyBalance.FameIncomeLogs, false);
        }
        int curveAt = Array.IndexOf(args, "--card-curve");
        if (curveAt >= 0) EconomyBalance.CardCurve = args[curveAt + 1];
        if (EconomyBalance.CardCurve != "original" && EconomyBalance.CardCurve != "balanced" && EconomyBalance.CardCurve != "light" && EconomyBalance.CardCurve != "moderate" && EconomyBalance.CardCurve != "strong")
            throw new ArgumentException("Unknown --card-curve");
        int handsAt = Array.IndexOf(args, "--hands-payout");
        if (handsAt >= 0) EconomyBalance.HandsPayoutScale = double.Parse(args[handsAt + 1], System.Globalization.CultureInfo.InvariantCulture);
        if (EconomyBalance.HandsPayoutScale <= 0 || EconomyBalance.HandsPayoutScale > 1) throw new ArgumentException("--hands-payout must be in (0, 1]");
        int globalAt = Array.IndexOf(args, "--card-global-gain");
        if (globalAt >= 0) EconomyBalance.CardGlobalGainOverride = double.Parse(args[globalAt + 1], System.Globalization.CultureInfo.InvariantCulture);
        if (!double.IsNaN(EconomyBalance.CardGlobalGainOverride) && (!double.IsFinite(EconomyBalance.CardGlobalGainOverride) || EconomyBalance.CardGlobalGainOverride <= 0)) throw new ArgumentException("Invalid card global gain");
        if (args.Contains("--export-only")) return;
        if (args.Contains("--card-experiment-checks")) { CardExperimentChecks(catalog); return; }
        if (args.Contains("--tap-matrix")) { TapBalanceMatrix(catalog, args); return; }
        if (args.Contains("--fit-tap-targets")) { FitTapTargets(catalog, args); return; }
        if (args.Contains("--paired-balance")) { PairedBalance(catalog); return; }
        if (args.Contains("--fit-taps")) { FitTaps(catalog); return; }
        if (args.Contains("--fit-prestige")) { FitPrestige(catalog); return; }
        if (args.Contains("--balance")) { BalanceSimulation(catalog, args); return; }
        Check(data.upgrades.Length == 1000 && data.cards.Length == 28, "Authoritative 1,000 cash entries and 28 cards import");
        Check(catalog.TryUpgrade("PP-PHU-01", out int first) && Near(data.upgrades[first].price, EconomyBalance.UpgradePrice(0)), "Stable-ID indexed first price");
        Check(Near(data.upgrades.Last().price, EconomyBalance.UpgradePrice(999)), "Continuous price curve spans all 1,000 entries");
        Check(data.upgrades.All(x => x.effect != CashEffect.Group && x.effect != CashEffect.Link && x.effect != CashEffect.TapLinked), "Retired collection and linked effects absent");
        var copy = Clone(data); copy.upgrades[1].id = copy.upgrades[0].id; Reject(() => new RedesignCatalog(copy), "Duplicate IDs rejected");
        copy = Clone(data); copy.upgrades[0].prerequisites = new[] { "missing" }; Reject(() => new RedesignCatalog(copy), "Unknown prerequisite rejected");
        copy = Clone(data); copy.upgrades[0].prerequisites = new[] { copy.upgrades[1].id }; copy.upgrades[1].prerequisites = new[] { copy.upgrades[0].id }; Reject(() => new RedesignCatalog(copy), "Prerequisite cycles rejected");
        copy = Clone(data); copy.upgrades[0].price = 0; Reject(() => new RedesignCatalog(copy), "Nonpositive price rejected");
        copy = Clone(data); copy.upgrades[0].target = 8; Reject(() => new RedesignCatalog(copy), "Invalid product target rejected");
        copy = Clone(data); copy.contentVersion = "future"; Reject(() => new RedesignCatalog(copy), "Unsupported future content rejected");
        copy = Clone(data); copy.upgrades[0].price *= 1.01; Reject(() => new RedesignCatalog(copy), "Price drift rejected");
        Reject(() => RedesignContentImporter.Import(File.ReadAllText("Game Design/1000 Unlockables Catalogue.md").Replace("Own Phuljhari |", "Own Phuljhari; Prestige 2 |"), File.ReadAllText("Game Design/Talent and Prestige Draft.md")), "Unrecognized authored gate fails import instead of being ignored");
        NumberChecks();
        RebalanceChecks(catalog);
        EngineChecks(catalog);
        CloudChecks(catalog).GetAwaiter().GetResult();
        DistributionChecks(catalog);
        LifecycleChecks(catalog);
        SaveDurabilityChecks(catalog);
        BoundaryChecks(catalog);
        PremiumClientChecks(catalog).GetAwaiter().GetResult();
        PremiumWalletChecks(catalog);
        DevelopmentShopChecks(catalog);
        UpgradeBrowserChecks(catalog);
        GoalChecks(catalog);
        LevelProjectionChecks(catalog);
        SkillProjectionChecks(catalog);
        PreferenceChecks(catalog);
        AppearanceChecks(catalog);
        ShopVisualChecks(catalog);
        GuidanceChecks(catalog);
        PremiumAccountChecks(catalog).GetAwaiter().GetResult();
        AccountCoordinatorChecks(catalog).GetAwaiter().GetResult();
        PremiumHttpChecks(catalog).GetAwaiter().GetResult();
        if (args.Contains("--parity-export")) ExportParity(catalog);
        if (args.Contains("--legacy-fixtures")) LegacyFixtures();
        Console.WriteLine($"All {checks} redesign checks passed. Content SHA256={data.sourceHash}");
    }
    static void NumberChecks()
    {
        Natural huge = new Natural(BigInteger.Pow(10, 100));
        Check((huge + (Natural)1).Value == BigInteger.Pow(10, 100) + 1, "Copies above double and long range increase by exactly one");
        Check(Clone(huge).Value == huge.Value, "Exact count roundtrips as decimal string");
        Check(!new Natural { digits = "01" }.Valid && !new Natural { digits = "-1" }.Valid && !new Natural { digits = "NaN" }.Valid, "Malformed counters rejected");
        foreach (int u in new[] { 249, 250, 499, 500, 749, 750, 999, 1000 })
            foreach (int r in new[] { 1, 4, 9, 16 })
            {
                ProgressionNumbers.PrestigeRewards(new Amount(r * 1e10), new Amount(1e10), 1, u, out var fame, out var credits);
                Check(fame.Value == (int)Math.Sqrt(r) && credits.Value == 1 + (r >= 16 ? 2 : r >= 4 ? 1 : 0) + (r >= 4 ? u / 250 : 0), $"Reward boundary E/T={r}, U={u}");
            }
        ProgressionNumbers.PrestigeRewards(new Amount(1e10 - 1), new Amount(1e10), 0, 1000, out var f, out var c);
        Check(f.IsZero && c.IsZero, "Below target grants nothing");
        ProgressionNumbers.PrestigeRewards(new Amount(1, 450), new Amount(1, 10), 0, 1000, out f, out c);
        Check(f.Value == BigInteger.Pow(10, 220) && c.Value == 3, "Extreme earnings yield exact uncapped Fame and three first credits");
        Check(ProgressionNumbers.FameMultiplier(1).CompareTo(6) == 0 && ProgressionNumbers.FameMultiplier(4) > ProgressionNumbers.FameMultiplier(3), "Fame follows the authored rebuild curve");
        Check(ProgressionNumbers.RunTarget(huge).Valid, "Target remains representable for unbounded prestige count");
        Check(ProgressionNumbers.CardMultiplier(CardEffect.ProductProfit, huge).Valid, "Unbounded card profit remains Amount");
        Check(ProgressionNumbers.DuplicateBenefit(CardEffect.TapCost, huge).Contains("/"), "Exact duplicate benefit survives rounded display saturation");
    }
    static void LegacyFixtures()
    {
        const string root = "Documentation/Implementation/Fixtures/Legacy";
        Directory.CreateDirectory(root);
        var state = new GameState { now = 10000, savedAt = 10000, cash = 5000, reputation = 12, lifetimeReputation = 20, prestiges = 3, storeName = "Fixture Shop", storeNamePromptHandled = true, music = false, reducedMotion = true, sponsorUntil = 12000, pendingOfflineId = "fixture-offline", pendingOfflineReward = 20, onboarding = 4, runId = "fixture-old-run", revision = 10 };
        state.products[0].level = 50; state.legacy[0].level = 2;
        state.upgrades.Add("profit-phuljhari-0"); state.claimedRewards.Add("fixture-ad-receipt");
        Check(state.Valid(), "Sanitized legacy fixture meets original validation");
        void Save(string name, object value)
        {
            string json = JsonSerializer.Serialize(value, value.GetType(), Json);
            File.WriteAllText(Path.Combine(root, name), SavePayload.Digest(json) + "\n" + json);
        }
        Save("guest.save", state);
        var older = Clone(state); older.revision = 9; older.cash = 4900; Save("guest.save.bak", older);
        var payload = SavePayload.Create(JsonSerializer.Serialize(state, Json));
        var checkpoint = new AccountCheckpoint { uid = "fixture-player", owner = "fixture-device", generation = 2, baseRevision = 10, commitId = "fixture-upload", save = payload, pending = false };
        Check(checkpoint.Valid("fixture-player"), "Sanitized confirmed account checkpoint validates");
        Save("confirmed.checkpoint", checkpoint);
        checkpoint.pending = true; checkpoint.followup = SavePayload.Create(JsonSerializer.Serialize(older, Json));
        Save("pending.checkpoint", checkpoint);
        File.WriteAllText(Path.Combine(root, "README.md"), "# Sanitized legacy fixtures\n\nGenerated by `dotnet run --project Tools/RedesignLab -- --legacy-fixtures`. All identifiers, balances and names are synthetic. Covers guest/main/backup, reputation/Legacy, old cash IDs, settings/name, sponsor/ad receipt, offline notice, confirmed and pending account checkpoint with follow-up. Do not use these as deployed player compensation evidence.\n");
    }
}
