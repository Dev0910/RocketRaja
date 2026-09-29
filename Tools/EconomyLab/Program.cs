using RocketRaja.Core;
using System.Text.Json;
using System.Diagnostics;

var options = new JsonSerializerOptions { IncludeFields = true, IgnoreReadOnlyProperties = true };
int catalogArgument = Array.IndexOf(args, "--catalog");
Catalog authored = catalogArgument >= 0 ? JsonSerializer.Deserialize<Catalog>(File.ReadAllText(args[catalogArgument + 1]), options) : Catalog.Create();
GameEngine Fresh() => new GameEngine(authored, new GameState());
void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); }
bool Near(Amount a, Amount b) => Math.Abs(Amount.Ratio(a, b) - 1) < 1e-8 || a.mantissa == 0 && b.mantissa == 0;
GameState Clone(GameState s) => JsonSerializer.Deserialize<GameState>(JsonSerializer.Serialize(s, options), options);

if (!args.Contains("--simulate"))
{
    var g = Fresh();
    Check(g.Data.products.Length == 8 && g.Data.upgrades.Length == 96 && g.Data.upgrades.Select(x => x.id).Distinct().Count() == 96, "Catalog has 8 products and 96 unique unlockables");
    Check(g.State.legacy.Count == 12 && g.State.Valid(), "12 Legacy perks and valid initial state");
    Check(new Amount(1, 450) + new Amount(2, 449) > new Amount(1, 450), "Large numbers exceed double range");
    Check(new Amount(double.Epsilon).Valid && (new Amount(1, -450) - Amount.Zero).Valid, "Tiny values normalize and subtract zero safely");
    Check((new Amount(1, 450) * 100).exponent == 452, "Large-number multiply");
    g.State.cash = 1e100;
    Amount total = Amount.Zero;
    for (int i = 0; i < 100; i++) { total += g.ProductCost(0, 1); g.BuyProduct(0, 1); }
    var h = Fresh(); Check(Near(total, h.ProductCost(0, 100)), "Bulk cost equals sequential single purchases");
    h.State.cash = h.ProductCost(0, 100) * 1.000000001;
    Check(h.ProductQuote(0, int.MaxValue).quantity == 100, "Buy Max boundary");
    Check(!h.BuyProduct(1, 1), "Products unlock in sequence");
    Check(h.BuyProduct(0, 100) && h.State.cash >= Amount.Zero, "Affordable bulk buy preserves non-negative cash");
    foreach (var n in new[] { 0, 1, 25, 50, 100 }) { h.State.products[0].level = n; Check(h.VisualTier(0) == Array.IndexOf(new[] { 0, 1, 25, 50, 100 }, n), "Visual tier at " + n); }
    Check(ProductMilestones.Progress(75) == .5f && ProductMilestones.Progress(100) == 0, "Interval progress resets at milestone");
    Check(ProductMilestones.Next(0) == 1 && ProductMilestones.Progress(1) == 0 && ProductMilestones.Next(Catalog.MaxLevel) == 0 && ProductMilestones.Progress(Catalog.MaxLevel) == 1, "Stocking and maximum endpoints");
    for (int product = 0; product < 8; product++)
    {
        var m = Fresh();
        foreach (int level in new[] { 0, 1, 9, 10, 24, 25, 49, 50, 75, 99, 100, 174, 175, 199, 200, 249, 250, 299, 300, Catalog.MaxLevel })
        {
            m.State.products[product].level = level;
            int boosts = Enumerable.Range(1, 1000).Count(x => x * 100 <= level) + (level >= 25 ? 1 : 0) + (level >= 50 ? 1 : 0);
            Amount expected = new Amount(m.Data.products[product].income) * level * Amount.Pow10(boosts * Math.Log10(2));
            Check(m.ProductIncome(product).Valid && Near(m.ProductIncome(product), expected), "Product " + product + " profit schedule at " + level);
        }
        m.State.cash = Amount.Pow10(6000);
        m.State.products[product].level = 1;
        if (product > 0) m.State.products[product - 1].level = 1;
        Check(m.BuyProduct(product, 299) && ProductMilestones.BoostCount(m.State.Level(product)) == 5, "Bulk crosses every milestone once " + product);
        m.State.upgrades.Add("tap-0");
        var restored = new GameEngine(m.Data, Clone(m.State));
        Check(restored.State.Level(product) == 300 && restored.State.upgrades.Contains("tap-0") && Near(restored.ProductIncome(product), m.ProductIncome(product)), "State roundtrip retains progress " + product);
    }
    g = Fresh(); g.State.cash = 1e100; for (int i = 0; i < 8; i++) g.BuyProduct(i, 300); g.State.tapLevel = 100; g.BuyAll();
    Check(g.State.upgrades.Count == 96, "All 96 unlockables reachable and ordered");
    g.ActivateSkill(0); g.ActivateSkill(1); g.RewardAd("one", "sponsor");
    h = new GameEngine(g.Data, Clone(g.State));
    g.Advance(400); for (int i = 0; i < 4000; i++) h.Advance(.1);
    Check(Near(g.State.runEarnings, h.State.runEarnings), "Large and small timesteps agree across buff boundaries");
    Check(g.State.runEarnings > g.State.eligibleEarnings, "Ad bonus excluded from prestige earnings");
    var before = g.State.eligibleEarnings; g.ActivateSkill(2);
    Check(Near(g.State.eligibleEarnings - before, g.NormalIncome * 300), "Bulk Order uses normal income");
    g.State.eligibleEarnings = Catalog.PrestigeThreshold * 4; var p = g.PreviewPrestige();
    Check(p.reward == 10, "Prestige square-root reward");
    g.Tap(); Check(!g.Prestige(p), "Stale prestige quote rejected");
    p = g.PreviewPrestige(); Check(g.Prestige(p), "Prestige commits");
    Check(!g.Prestige(p) && g.State.OwnedProducts == 0 && g.StallTier == 0 && g.State.upgrades.Count == 0 && g.State.tapLevel == 0 && g.State.skills.All(x => x.readyAt == 0) && g.State.sponsorUntil == 0, "Prestige resets entire run exactly once");
    Check(g.BuyLegacy(9) && g.State.Legacy(9) == 1 && g.State.lifetimeReputation == 10, "Legacy spends balance without reducing lifetime reputation");
    g.State.products[0].level = 10; g.State.now = g.State.savedAt = 1000;
    Amount normal = g.NormalIncome; var reward = g.ResolveOffline(1000 + 12 * 3600);
    Check(Near(reward, normal * .5 * 8 * 3600), "Eight-hour offline cap");
    var balance = g.State.cash; g.ResolveOffline(g.State.savedAt); Check(Near(balance, g.State.cash), "Repeated resume has no duplicate payout");
    string offline = g.State.pendingOfflineId;
    Check(g.RewardAd("two", "offline", offline) && !g.RewardAd("two", "offline", offline) && !g.RewardAd("three", "offline", offline), "Offline reward callback is single-use");
    balance = g.State.cash; double saved = g.State.savedAt; g.ResolveOffline(saved - 1000); g.ResolveOffline(saved); Check(Near(balance, g.State.cash), "Clock rollback cannot replay offline time");
    var root = Path.Combine(Path.GetTempPath(), "DiwaliTests-" + Guid.NewGuid());
    var store = new SaveStore(Path.Combine(root, "save"), s => JsonSerializer.Serialize(s, options), s => JsonSerializer.Deserialize<GameState>(s, options));
    store.Save(g.State); var loaded = store.Load(); Check(loaded.Valid() && Near(loaded.cash, g.State.cash) && loaded.Legacy(9) == 1, "Save roundtrip retains values and permanent perks");
    store.Save(g.State); File.WriteAllText(Path.Combine(root, "save"), "corrupt"); loaded = store.Load(); Check(loaded != null && store.Warning != null, "Corrupt save recovers backup");
    Check(!g.RewardAd("invalid", "unknown"), "Unknown reward placement cannot mutate economy");
    var fully = Fresh(); fully.State.reputation = fully.State.lifetimeReputation = 10000;
    for (int i = 0; i < 12; i++) { for (int rank = 0; rank < 5; rank++) Check(fully.BuyLegacy(i), "Legacy " + i + " rank " + (rank + 1)); Check(!fully.BuyLegacy(i), "Legacy rank cap " + i); }
    Check(fully.OfflineCap == 18 * 3600, "Maximum offline cap is eighteen hours");
    fully.State.cash = 1e100; for (int i = 0; i < 8; i++) fully.BuyProduct(i, 300); fully.State.tapLevel = 100; fully.BuyAll();
    Check(fully.ActivateSkill(0) && fully.State.skills[0].readyAt - fully.State.now == 225, "Cooldown perk reduces cooldown by 25 percent");
    double ready = fully.State.skills[0].readyAt; fully.ResolveOffline(20);
    Check(fully.State.skills[0].readyAt == ready && !fully.ActivateSkill(0), "Short closure preserves absolute skill cooldown");
    fully.ResolveOffline(300); Check(fully.ActivateSkill(0), "Cooldown finishes while closed");
    var eligible = fully.State.eligibleEarnings; fully.BuyProduct(0, 1); Check(Near(eligible, fully.State.eligibleEarnings), "Spending never reduces prestige eligibility");
    store.Save(fully.State); Check(store.Load().legacy.All(x => x.level == 5), "All Legacy ranks survive serialization");
    File.WriteAllText(Path.Combine(root, "save"), "bad main"); File.WriteAllText(Path.Combine(root, "save.bak"), "bad backup");
    Check(store.Load() == null && store.Warning != null && File.ReadAllText(Path.Combine(root, "save")) == "bad main", "Both corrupt saves preserved for explicit recovery");
    Console.WriteLine("All core checks passed. Test artifacts: " + root);
}
else
{
    var results = new List<object>();
    foreach (string policy in new[] { "active", "occasional", "offline" })
    {
        var g = Fresh(); double clock = 0, previousCycle = double.PositiveInfinity;
        for (int run = 0; run < 3; run++)
        {
            double start = clock, firstProduct = -1, firstTier = -1, allProducts = -1;
            while (clock - start < 86400 * 3)
            {
                bool active = policy == "active" || clock % 1800 < 120;
                if (policy == "offline" && !active) { g.State.savedAt = g.State.now; g.ResolveOffline(g.State.now + 1680); clock += 1680; }
                else { g.Advance(1); clock++; }
                if (active)
                {
                    for (int tap = 0; tap < 3; tap++) g.Tap();
                    // Purchase unlocked bonuses and pursue the best marginal income per rupee.
                    g.BuyAll();
                    for (int purchase = 0; purchase < 20; purchase++)
                    {
                        int best = -2; double payoff = double.PositiveInfinity;
                        if (g.State.cash >= g.TapCost && g.State.tapLevel < 60) { payoff = Amount.Ratio(g.TapCost, g.TapIncome * (1.4 / (1 + g.State.tapLevel * 1.4)) * 3); best = -1; }
                        for (int i = 0; i < 8; i++)
                        {
                            var q = g.ProductQuote(i, 1); if (!q.affordable) continue;
                            double p = Amount.Ratio(q.cost, g.ProductIncome(i, 1) - g.ProductIncome(i));
                            if (p < payoff) { payoff = p; best = i; }
                        }
                        if (best == -2) break;
                        if (best == -1) g.BuyTap(); else g.BuyProduct(best, 1);
                    }
                    for (int i = 0; i < 3; i++) g.ActivateSkill(i);
                }
                if (firstProduct < 0 && g.State.OwnedProducts > 0) firstProduct = clock - start;
                if (firstTier < 0 && g.State.products.Any(x => x.level >= 25)) firstTier = clock - start;
                if (allProducts < 0 && g.State.OwnedProducts == 8) allProducts = clock - start;
                var pQuote = g.PreviewPrestige();
                if (pQuote.reward > 0 && active)
                {
                    if (policy == "active")
                    {
                        double elapsed = clock - start;
                        if (run == 0 && (firstProduct < 30 || firstProduct > 60 || firstTier > 300 || allProducts >= elapsed || elapsed < 1200 || elapsed > 1800)) throw new Exception("First-run pacing target failed");
                        if (run == 1 && (elapsed < 300 || elapsed > 600)) throw new Exception("First rebuild pacing target failed");
                        if (run == 2 && elapsed >= previousCycle) throw new Exception("Third cycle must improve on second cycle");
                        previousCycle = elapsed;
                    }
                    results.Add(new { policy, run = run + 1, seconds = clock - start, firstProduct, firstTier, allProducts, reputation = pQuote.reward, earnings = g.State.eligibleEarnings.ToString() });
                    g.Prestige(pQuote); while (g.BuyLegacy(9)) {} while (g.BuyLegacy(8)) {} for (int i = 7; i >= 0; i--) while (g.BuyLegacy(i)) {}
                    break;
                }
            }
        }
    }
    Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
}
