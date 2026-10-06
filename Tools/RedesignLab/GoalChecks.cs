using RocketRaja.Core;

static partial class Program
{
    static void GoalChecks(RedesignCatalog data)
    {
        var state = RedesignState.Fresh(); state.cash = 2000; state.products[0] = 1;
        RedesignState saved = null;
        var engine = new RedesignEngine(data, state, value => saved = value.Copy());
        var scope = new UpgradeScope { category = UpgradeCategory.Products, product = 0 };
        var quote = engine.QuoteBuyAll(scope); var revision = engine.State.commandRevision;
        Check(engine.PinUpgrade("PP-PHU-03") && saved.pinnedUpgradeId == "PP-PHU-03", "Pin is durably saved before publication");
        Check(engine.State.commandRevision.CompareTo(revision) == 0 && engine.CanCommitBuyAll(quote), "Pin does not invalidate an economic quote");
        var requirements = engine.NextRequirement("PP-PHU-03");
        Check(requirements.kind == GoalTargetKind.Upgrade && requirements.upgradeId == "PP-PHU-02", "Goal points to the next missing prerequisite upgrade");
        var needed = engine.NextRequirement("PP-ANA-01");
        Check(needed.kind == GoalTargetKind.Product && needed.product == 1, "Unowned-product goal points to its product details");
        var alternatives = engine.GoalAlternatives();
        Check(alternatives.Length == 3 && alternatives.Any(x => x.kind == GoalTargetKind.Upgrade) && alternatives.Any(x => x.kind == GoalTargetKind.Product) && alternatives.Any(x => x.kind == GoalTargetKind.TapTraining), "Goal alternatives include cash upgrades, product levels and tap training");
        Check(alternatives.Single(x => x.kind == GoalTargetKind.Product).cost.CompareTo(engine.LevelQuote(0, 1).cost) == 0 && alternatives.Single(x => x.kind == GoalTargetKind.TapTraining).cost.CompareTo(engine.LevelQuote(-1, 1).cost) == 0, "Goal alternative prices come from engine quotes");
        Check(!engine.PinUpgrade("missing") && engine.State.pinnedUpgradeId == "PP-PHU-03", "Unknown goal cannot replace a valid pinned choice");
        engine.Advance(90); Check(engine.CanCommitBuyAll(quote) && quote.Ids.Count == 3, "Passive income preserves the reviewed list and eligibility");
        var snapshot = quote.Selection; snapshot.product = 7;
        Check(quote.Selection.product == 0, "Batch scope exposed to presentation is a defensive copy");
        Check(engine.CommitBuyAll(quote) && engine.State.pinnedUpgradeId == "", "Buying a pinned choice in a batch clears the completed goal");
        Check(!engine.CanCommitBuyAll(quote) && !engine.CommitBuyAll(quote), "Completed review disables confirmation and cannot spend twice");
        Check(!engine.PinUpgrade("PP-PHU-01"), "Already-owned cash upgrade is not offered as an unfinished goal");
        var next = engine.QuoteBuyAll(); engine.BuyLevels(-1, 1);
        Check(!engine.CanCommitBuyAll(next), "Another economic command invalidates a reviewed batch");
        var blocked = new RedesignEngine(data, state, _ => { throw new IOException("disk full"); });
        bool threw = false; try { blocked.PinUpgrade("PP-PHU-01"); } catch (IOException) { threw = true; }
        Check(threw && blocked.State.pinnedUpgradeId == "", "Failed goal save retains the previous state");
        engine.MutationAllowed = () => false;
        Check(!engine.PinUpgrade("PP-PHU-04"), "Lost session ownership prevents preference mutation");
    }
}
