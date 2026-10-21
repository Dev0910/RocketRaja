using RocketRaja.Core;

static partial class Program
{
    static void AppearanceChecks(RedesignCatalog data)
    {
        for (int product = 0; product < 8; product++)
        {
            var state = RedesignState.Fresh(); var engine = new RedesignEngine(data, state, _ => throw new Exception("Appearance must not save"));
            Check(engine.ProductAppearance(product) == -1, "Unowned family has no owned display " + product);
            state.products[product] = Catalog.MaxLevel; engine = new RedesignEngine(data, state, _ => throw new Exception("Appearance must not save"));
            Check(engine.ProductAppearance(product) == 0, "Product levels alone never replace signature requirements " + product);
            for (int tier = 1; tier <= 3; tier++)
            {
                string id = data.AppearanceUpgrade(product, tier); state.upgrades.Add(id); state.everPurchased.Add(id);
                engine = new RedesignEngine(data, state, _ => throw new Exception("Appearance must not save")); var before = engine.NormalIncome();
                Check(engine.ProductAppearance(product) == tier && engine.NormalIncome().CompareTo(before) == 0, "Current-run signature selects the exact display without an economic effect " + product + "/" + tier);
            }
            state.upgrades.Clear(); engine = new RedesignEngine(data, state, _ => { });
            Check(engine.ProductAppearance(product) == 0, "Lifetime signature history cannot restore a run display " + product);
        }
        var fresh = RedesignState.Fresh(); fresh.cash = 10000; fresh.eligibleEarnings = fresh.target;
        RedesignState disk = null; var e = new RedesignEngine(data, fresh, s => disk = Clone(s));
        Check(e.BuyLevels(0, 1) && e.BuyUpgrade(data.AppearanceUpgrade(0, 1)) && e.ProductAppearance(0) == 1, "Actual manual purchase and signature command change the display");
        Check(new RedesignEngine(data, disk, _ => { }).ProductAppearance(0) == 1, "Committed display requirements survive a real codec round trip");
        Check(e.Prestige(e.PreviewPrestige()) && e.ProductAppearance(0) == -1 && e.State.everPurchased.Contains(data.AppearanceUpgrade(0, 1)), "Prestige clears run displays while retaining first-purchase history");
        Check(e.BuyLevels(0, 1) && e.ProductAppearance(0) == 0, "First rebuild purchase restores only the basic display");
        e.Advance(30);
        Check(e.BuyUpgrade(data.AppearanceUpgrade(0, 1)) && e.ProductAppearance(0) == 1 && e.State.UniqueCount == 1, "Rebuy restores the signature look without another lifetime first purchase");
        Reject(() => e.ProductAppearance(8), "Unknown display family rejected");
    }
}
