using RocketRaja.Core;

static partial class Program
{
    static void ShopVisualChecks(RedesignCatalog data)
    {
        foreach (int count in new[] { 0, 99, 100, 249, 250, 499, 500, 749, 750, 999, 1000 })
        {
            var s = RedesignState.Fresh(); s.everPurchased = data.Upgrades.Take(count).Select(u => u.id).ToList();
            var e = new RedesignEngine(data, s, _ => throw new Exception("Visual loading must not save"));
            int expected = new[] { 100, 250, 500, 750, 1000 }.Count(t => count >= t);
            Check(e.ShopAppearance == expected && s.decorationStage == 0 && s.upgrades.Count == 0, "Lifetime shop boundary derives without purchases, saves or run displays " + count);
        }
        var retained = RedesignState.Fresh(); retained.decorationStage = 5;
        Check(new RedesignEngine(data, retained, _ => { }).ShopAppearance == 5, "Durable decoration stage is a retained presentation floor");
        var rich = RedesignState.Fresh(); rich.cash = Amount.Pow10(250); rich.products = Enumerable.Repeat(300, 8).ToArray(); rich.eligibleEarnings = rich.target;
        RedesignState disk = null; var engine = new RedesignEngine(data, rich, s => disk = Clone(s)); var review = engine.QuoteBuyAll();
        Check(engine.CommitBuyAll(review) && engine.ShopAppearance == 5 && Enumerable.Range(0, 8).All(p => engine.ProductAppearance(p) == 3), "One real batch crosses every shop and product milestone to the final state");
        engine = new RedesignEngine(data, disk, s => disk = Clone(s));
        Check(engine.ShopAppearance == 5 && engine.ProductAppearance(7) == 3, "Both types of appearance survive durable reload");
        Check(engine.Prestige(engine.PreviewPrestige()) && engine.ShopAppearance == 5 && Enumerable.Range(0, 8).All(p => engine.ProductAppearance(p) == -1), "Prestige keeps the final shop and empties every product display");
        Check(engine.BuyLevels(0, 1) && engine.ShopAppearance == 5 && engine.ProductAppearance(0) == 0, "Manual rebuilding restores only the basic product inside the retained shop");
        engine.Advance(30);
        Check(engine.BuyUpgrade(data.AppearanceUpgrade(0, 1)) && engine.ShopAppearance == 5 && engine.State.UniqueCount == 1000 && engine.ProductAppearance(0) == 1, "Rebuy restores run art without replaying or recounting permanent milestones");
        Reject(() => RedesignShopProgress.StageForUnique(1001), "Impossible lifetime visual count rejected");
    }
}
