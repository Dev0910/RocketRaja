// Exports disposable, in-memory presentation examples through the existing core.
// Does not start Unity, access player files, or write game state.
using System.Text.Json;
using RocketRaja.Core;
var json = new JsonSerializerOptions { IncludeFields = true };
var data = JsonSerializer.Deserialize<RedesignContent>(File.ReadAllText("Documentation/RedesignContentCatalog.json"), json);
var catalog = new RedesignCatalog(data);
object Money(Amount a) => new { shortText = a.ToString(), value = double.IsFinite(a.mantissa * Math.Pow(10, a.exponent)) ? (double?)(a.mantissa * Math.Pow(10, a.exponent)) : null, exact = a.mantissa.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) + " × 10^" + a.exponent };
var fixtures = new Dictionary<string, object>();
foreach (var name in new[] { "new", "opened", "growing", "progressed", "prestige", "rebuilt" })
{
    var s = RedesignState.Fresh(100000);
    if (name != "new" && name != "opened")
    {
        s.products = new[] { 25, 18, 12, 8, 5, 3, 1, 1 }; s.tapLevel = 12;
        s.cash = 1240000; s.eligibleEarnings = 4800000; s.lifetimeEarnings = 24000000; s.manualTaps = 12480;
        if (name != "growing")
        {
            s.completedPrestiges = 1; s.fame = 1; s.freeCredits = 3; s.kajuKatli = 100;
            s.target = ProgressionNumbers.RunTarget(s.completedPrestiges, s.fame);
            s.cardLevels[0] = 1; s.cardLevels[2] = 2; s.cardLevels[16] = 1; s.cardLevels[18] = 1;
        }
        s.upgrades = data.upgrades.Where(u => u.effect == CashEffect.SkillPower && u.rank == 1).Select(u => u.id).ToList();
        s.upgrades.Add("PP-PHU-01"); s.everPurchased = new List<string>(s.upgrades);
    }
    if (name == "prestige" || name == "rebuilt") s.eligibleEarnings = s.target * 4;
    var e = new RedesignEngine(catalog, s, _ => { });
    if (name == "rebuilt") { if (!e.Prestige(e.PreviewPrestige())) throw new Exception("Cannot prepare rebuilt fixture"); s = e.State; }
    if (name == "opened")
    {
        // Earn the first display through the current tap command, starting from zero.
        for (int taps = 0; !e.PreviewLevels(0, 1).quote.affordable; taps++)
        {
            if (taps >= 1000 || !e.Tap()) throw new Exception("Cannot fund first-stock fixture");
        }
        if (!e.BuyLevels(0, 1)) throw new Exception("Cannot prepare first-stock fixture");
        s = e.State;
    }
    object Level(int product, int qty)
    {
        var p = e.PreviewLevels(product, qty);
        return new { p.beforeLevel, p.afterLevel, p.nextMilestone, p.available, count = p.quote.quantity, p.quote.affordable, cost = Money(p.quote.cost), normalBefore = Money(p.normalBefore), normalAfter = Money(p.normalAfter), productBefore = Money(p.productBefore), productAfter = Money(p.productAfter), tapBefore = Money(p.tapBefore), tapAfter = Money(p.tapAfter), linkedBefore = Money(p.linkedTapBefore) };
    }
    var upgrades = data.upgrades.ToDictionary(u => u.id, u => {
        var p = e.PreviewUpgrade(u.id);
        return (object)new { eligible = e.Eligible(u.id), owned = s.upgrades.Contains(u.id), ever = s.everPurchased.Contains(u.id), price = Money(e.UpgradeCost(u.id)), projection = p == null ? null : new { normalBefore = Money(p.normalBefore), normalAfter = Money(p.normalAfter), tapBefore = Money(p.manualTapBefore), tapAfter = Money(p.manualTapAfter), costBefore = Money(p.costBefore), costAfter = Money(p.costAfter), costCategory = p.costCategory.ToString(), p.skillPowerBefore, p.skillPowerAfter, p.durationBefore, p.durationAfter, p.cooldownBefore, p.cooldownAfter } };
    });
    var fame = e.PreviewPrestige();
    var batches = Enum.GetValues<UpgradeCategory>().ToDictionary(c => c.ToString(), c => {
        var b = e.QuoteBuyAll(new UpgradeScope { category = c });
        return (object)new { ids = b.Ids, prices = b.Prices.Select(Money), cost = Money(b.Cost), budget = Money(b.Budget), first = b.FirstPurchases };
    });
    foreach (var c in new[] { UpgradeCategory.Products, UpgradeCategory.Skills })
    for (int target = 0; target < (c == UpgradeCategory.Products ? 8 : 3); target++)
    {
        var scope = new UpgradeScope { category = c, product = c == UpgradeCategory.Products ? target : -1, skill = c == UpgradeCategory.Skills ? target : -1 };
        var b = e.QuoteBuyAll(scope);
        batches[c + ":" + target] = new { ids = b.Ids, prices = b.Prices.Select(Money), cost = Money(b.Cost), budget = Money(b.Budget), first = b.FirstPurchases };
    }
    fixtures[name] = new {
        name, cash = Money(s.cash), normal = Money(e.NormalIncome()), tap = Money(e.TapPayout()), boostedNormal = Money(e.NormalIncome() * e.PreviewSkill(0).power), boostedTap = Money(e.TapPayout() * 1.25), levels = s.products, s.tapLevel,
        fame = s.fame.ToString(), multiplier = Money(ProgressionNumbers.FameMultiplier(s.fame)), tapMultiplier = Money(ProgressionNumbers.TapFameMultiplier(s.fame)), prestiges = s.completedPrestiges.ToString(), kk = s.kajuKatli.ToString(), credits = s.freeCredits.ToString(), earned = Money(s.eligibleEarnings), lifetime = Money(s.lifetimeEarnings), taps = s.manualTaps.ToString(), owned = s.upgrades,
        levelQuotes = Enumerable.Range(-1, 9).ToDictionary(p => p.ToString(), p => new Dictionary<string, object> { ["1"] = Level(p, 1), ["10"] = Level(p, 10), ["100"] = Level(p, 100), ["Max"] = Level(p, int.MaxValue) }),
        skills = Enumerable.Range(0, 3).Select(i => { var p = e.PreviewSkill(i); return new { p.skill, p.unlockId, p.owned, p.power, p.duration, p.cooldown, payout = Money(p.orderPayout) }; }),
        cards = data.cards.Select((c, i) => new { level = s.cardLevels[i].ToString(), current = Money(ProgressionNumbers.CardMultiplier(c.effect, s.cardLevels[i])), next = Money(ProgressionNumbers.CardMultiplier(c.effect, s.cardLevels[i] + (Natural)1)), delta = ProgressionNumbers.DuplicateBenefit(c.effect, s.cardLevels[i]), examples = Enumerable.Range(0, 65).Select(n => Money(ProgressionNumbers.CardMultiplier(c.effect, (Natural)n))) }),
        prestige = new { fame.Eligible, fame = fame.Fame.ToString(), credits = fame.Credits.ToString(), target = Money(fame.CurrentTarget), nextTarget = Money(fame.NextTarget), multiplier = Money(fame.ResultingIncomeMultiplier), tapMultiplier = Money(fame.ResultingTapIncomeMultiplier), nextFame = Money(fame.NextFameEarnings), fame.CatalogueCredits, fame.NextUniqueMilestone }, upgrades, batches
    };
}
Directory.CreateDirectory("UI Builder/HTML/Classic");
File.WriteAllText("UI Builder/HTML/Classic/fixtures.js", "// Generated from existing C# core. Disposable examples, not a live game.\nwindow.classicFixtures=" + JsonSerializer.Serialize(fixtures, json) + ";\n");
File.WriteAllText("UI Builder/HTML/Classic/content.js", "// Authored current content; no store or network integration.\nwindow.classicContent=" + JsonSerializer.Serialize(data, json) + ";\n");
Console.WriteLine($"Exported {fixtures.Count} core-derived fixtures, {data.upgrades.Length} upgrades and {data.cards.Length} cards.");
