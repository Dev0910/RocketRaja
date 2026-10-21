using RocketRaja.Core;

static partial class Program
{
    static void UpgradeBrowserChecks(RedesignCatalog data)
    {
        var engine = new RedesignEngine(data, RedesignState.Fresh(), _ => { });
        var browser = new RedesignUpgradeBrowser(engine) { View = UpgradeView.Full };
        browser.Refresh();
        Check(browser.Count == 1000 && Enumerable.Range(0, browser.Count).Select(i => browser[i].id).Distinct().Count() == 1000, "Browser Full exposes every authored stable ID once");
        foreach (UpgradeCategory category in Enum.GetValues<UpgradeCategory>().Where(c => c != UpgradeCategory.All))
        {
            browser.Scope.category = category; browser.Refresh();
            Check(browser.Count > 0 && Enumerable.Range(0, browser.Count).All(i => UpgradeScope.MatchesCategory(browser[i], category)), "Browser category has complete isolated membership: " + category);
        }
        browser.Scope.category = UpgradeCategory.Products; browser.Scope.product = 0; browser.Refresh();
        Check(Enumerable.Range(0, browser.Count).All(i => browser[i].effect != CashEffect.Link && browser[i].effect != CashEffect.Group), "Product filter contains no retired links or collection groups");
        Check(Enumerable.Range(0, browser.Count).All(i => browser[i].effect < CashEffect.SkillPower || browser[i].effect > CashEffect.SkillCooldown), "Product zero never leaks skill-zero entries");
        browser.Scope.product = -1; browser.Scope.category = UpgradeCategory.Skills; browser.Scope.skill = 2; browser.Refresh();
        Check(browser.Count > 0 && Enumerable.Range(0, browser.Count).All(i => browser[i].target == 2), "Skill filter keeps only Bulk Order power and cooldown choices");
        browser.Scope.category = UpgradeCategory.All; browser.Scope.skill = -1; browser.Scope.search = "pp-phu-01"; browser.Refresh();
        Check(browser.Count == 1 && browser[0].id == "PP-PHU-01", "Browser searches stable IDs case-insensitively");
        browser.Scope.search = "does not exist"; browser.Refresh(); Check(browser.Count == 0, "Browser explicit empty state has no stale row");
        browser.Scope.search = ""; browser.View = UpgradeView.Available; browser.Refresh();
        Check(browser.Count == 1000, "Fresh browser lists all unowned upgrades regardless of products or prerequisites");
        Check(data.Upgrades.All(u => engine.Eligible(u.id)), "All upgrade categories ignore the former product, family and rank gates");
        var emptyWallet = RedesignState.Fresh(); emptyWallet.cash = 0;
        var poor = new RedesignEngine(data, emptyWallet, _ => throw new Exception("Unaffordable upgrade must not save"));
        Check(data.Upgrades.All(u => !poor.BuyUpgrade(u.id)) && poor.QuoteBuyAll().Ids.Count == 0, "Cash still blocks every unaffordable individual and batch upgrade");
        var noProducts = RedesignState.Fresh(); noProducts.cash = Amount.Pow10(250);
        var independent = new RedesignEngine(data, noProducts, _ => { });
        Check(independent.QuoteBuyAll().Ids.Count == 1000, "Buy All includes every affordable upgrade with no products owned");
        var highRank = data.Upgrades.Last(u => u.effect == CashEffect.SkillPower && u.target == 0);
        var baseRank = data.Upgrades.First(u => u.effect == CashEffect.SkillPower && u.target == 0);
        Check(independent.BuyUpgrade(highRank.id) && independent.SkillOwned(0) && independent.ActivateSkill(0), "Higher skill rank can be bought and activated before its former prerequisites");
        double ready = independent.State.readyAt[0], active = independent.State.activeUntil[0], power = independent.PreviewSkill(0).power;
        Check(independent.BuyUpgrade(baseRank.id) && independent.State.readyAt[0] == ready && independent.State.activeUntil[0] == active && independent.PreviewSkill(0).power == power,
            "Buying a lower skill rank preserves higher power, active time and cooldown");
        Check(!independent.BuyUpgrade(baseRank.id), "Already-owned upgrades cannot be purchased twice");
        Check(independent.CommitBuyAll(independent.QuoteBuyAll()) && independent.State.upgrades.Count == 1000 && independent.State.products.All(n => n == 0), "Cash-only batch commits all remaining upgrades without purchasing products");
        engine.BuyLevels(0, 1); browser.Refresh();
        Check(Enumerable.Range(0, browser.Count).Any(i => browser[i].id == "PP-PHU-01"), "Available includes eligible choices even before enough cash");
        int rebuilds = browser.RebuildCount; engine.Advance(1); browser.Refresh();
        Check(browser.RebuildCount == rebuilds, "Passive cash accrual does not rebuild the full catalogue");
        var rich = RedesignState.Fresh(); rich.cash = new Amount(1, 250); rich.products = Enumerable.Repeat(250, 8).ToArray();
        var buyer = new RedesignEngine(data, rich, _ => { }); var scoped = new RedesignUpgradeBrowser(buyer);
        scoped.Scope.category = UpgradeCategory.Tapping;
        var quote = scoped.ReviewBatch();
        Check(quote.Ids.Count > 0 && quote.Ids.All(id => data.TryUpgrade(id, out int i) && data.Upgrades[i].effect >= CashEffect.TapDirect && data.Upgrades[i].effect <= CashEffect.TapCost), "Buy All freezes only the chosen Tapping category");
        scoped.Scope.category = UpgradeCategory.Shop;
        Check(buyer.CommitBuyAll(quote) && buyer.State.upgrades.All(quote.Ids.Contains), "Changing browser filters cannot change an already-reviewed batch");
        scoped.Scope.category = UpgradeCategory.Tapping; scoped.View = UpgradeView.Owned; scoped.Refresh();
        Check(scoped.Count == quote.Ids.Count && scoped.PurchaseKind(quote.Ids[0]) == "Owned", "Owned view refreshes after successful batch commands");
    }
}
