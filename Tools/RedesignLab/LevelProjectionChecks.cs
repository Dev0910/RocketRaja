using RocketRaja.Core;

static partial class Program
{
    static void LevelProjectionChecks(RedesignCatalog data)
    {
        int saved = 0; var state = RedesignState.Fresh(); state.products[0] = 24; state.cash = Amount.Pow10(100);
        var engine = new RedesignEngine(data, state, _ => saved++);
        var before = engine.State.Copy(); var p = engine.PreviewLevels(0, 1);
        Check(saved == 0 && engine.State.products[0] == 24 && engine.State.cash.CompareTo(before.cash) == 0 && engine.State.revision.CompareTo(before.revision) == 0, "Level projection has no persistence or state side effects");
        Check(p.beforeLevel == 24 && p.afterLevel == 25 && p.nextMilestone == 25 && p.productAfter > p.productBefore * 2, "Level preview includes the crossed level-25 boost");
        Check(engine.BuyLevels(0, 1) && engine.ProductIncome(0).CompareTo(p.productAfter) == 0 && engine.NormalIncome().CompareTo(p.normalAfter) == 0, "Projected product income equals the actual durable purchase");
        Check(engine.PreviewLevels(0, 10).nextMilestone == 50, "Next product milestone advances after purchase");
        var locked = engine.PreviewLevels(2, 10);
        Check(!locked.available && locked.quote.quantity == 0 && locked.afterLevel == 0, "Locked product projection never suggests purchasable levels");
        var starter = new RedesignEngine(data, RedesignState.Fresh(), _ => { });
        var opening = starter.PreviewLevels(0, int.MaxValue);
        Check(!opening.quote.affordable && opening.quote.quantity == 0 && opening.quote.cost.CompareTo(30) == 0, "Zero-cash opening previews the unaffordable first product");
        Check(starter.PreviewLevels(-1, int.MaxValue).quote.quantity == 0, "Unaffordable Max tap preview contains zero levels");
        var poor = RedesignState.Fresh(); poor.cash = 0;
        var poorEngine = new RedesignEngine(data, poor, _ => throw new Exception("Unaffordable purchase must not save"));
        foreach (int target in new[] { -1, 0 })
        {
            var max = poorEngine.LevelQuote(target, int.MaxValue);
            Check(max.quantity == 0 && !max.affordable && max.cost > Amount.Zero && max.cost.CompareTo(poorEngine.LevelQuote(target, 1).cost) == 0,
                "Unaffordable Max displays next single level price: " + target);
            Check(!poorEngine.BuyLevels(target, int.MaxValue), "Unaffordable Max never buys a level: " + target);
        }
        state = RedesignState.Fresh(); state.products[0] = 10; state.cash = 5000;
        state.upgrades.Add(data.Upgrades.First(u => u.effect == CashEffect.TapDirect).id);
        state.everPurchased.AddRange(state.upgrades);
        state.completedPrestiges = state.fame = (Natural)1; state.target = ProgressionNumbers.RunTarget(state.completedPrestiges);
        state.cardLevels[16] = (Natural)3; state.cardLevels[17] = (Natural)2;
        engine = new RedesignEngine(data, state, _ => { }); p = engine.PreviewLevels(-1, 10);
        Check(p.linkedTapBefore.mantissa == 0 && p.linkedTapAfter.mantissa == 0 && p.tapAfter > p.tapBefore, "Tap projection applies direct training and has no product-linked component");
        Check(engine.BuyLevels(-1, 10) && engine.TapPayout().CompareTo(p.tapAfter) == 0 && engine.NormalIncome().CompareTo(p.normalBefore) == 0, "Talent-adjusted projected tap payout matches actual training without changing normal sales");
        p = engine.PreviewLevels(0, int.MaxValue);
        Check(p.quote.quantity > 0 && p.quote.affordable && engine.LevelQuote(0, p.quote.quantity + 1).cost > engine.State.cash, "Projected Max stops at the actual affordability boundary");
        int displayed = p.quote.quantity; engine.Advance(500);
        Check(engine.BuyLevels(0, displayed) && engine.State.products[0] == p.afterLevel, "Buying the displayed Max count does not grow after passive earnings");
        state = RedesignState.Fresh(); state.products[0] = Catalog.MaxLevel; state.cash = Amount.Pow10(100);
        engine = new RedesignEngine(data, state, _ => { }); p = engine.PreviewLevels(0, 100);
        Check(p.quote.quantity == 0 && p.afterLevel == Catalog.MaxLevel && p.nextMilestone == 0, "Level cap projection offers neither extra levels nor an unreachable milestone");
        bool rejected = false; try { engine.PreviewLevels(8, 1); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Unknown product projection is rejected");
    }
}
