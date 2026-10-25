using System.Globalization;
using RocketRaja.Core;

static partial class Program
{
    static void AchievementChecks(RedesignCatalog catalog)
    {
        var fresh = RedesignState.Fresh();
        fresh.cash = 30; // Fixture funding for the ownership checks.
        var engine = new RedesignEngine(catalog, fresh, _ => { });
        Check(fresh.Valid(catalog) && RedesignAchievements.ReadyCount(fresh) == 0, "Fresh and pre-achievement saves load with no earned rewards");
        Check(!engine.ClaimAchievement("products-1") && !engine.ClaimAchievement("unknown") && !engine.ClaimAchievement(null), "Incomplete and unknown achievements cannot grant currency");
        Check(engine.BuyLevels(0, 1) && engine.State.mostProductsStocked == 1, "Product ownership records permanent achievement progress");
        Check(RedesignAchievements.ReadyCount(engine.State) == 1 && engine.State.kajuKatli.IsZero, "Completing a milestone exposes a claim without automatically granting currency");
        Check(engine.ClaimAchievement("products-1") && engine.State.kajuKatli.Value == 5, "Achievement pays its exact Kaju Katli reward");
        var claimed = Clone(engine.State);
        var reopened = new RedesignEngine(catalog, claimed, _ => { });
        Check(!reopened.ClaimAchievement("products-1") && reopened.State.kajuKatli.Value == 5, "Saved claims cannot pay twice after reload");
        var copy = engine.State.Copy(); copy.claimedAchievements.Clear();
        Check(engine.State.claimedAchievements.Count == 1, "Staged state copies never share achievement receipt lists");
        claimed.claimedAchievements.Add("products-1"); Check(!claimed.Valid(catalog), "Duplicate achievement receipts fail save validation");
        claimed = Clone(engine.State); claimed.claimedAchievements.Add("unknown"); Check(!claimed.Valid(catalog), "Unknown achievement receipts fail save validation");
        claimed = Clone(engine.State); claimed.mostProductsStocked = 9; Check(!claimed.Valid(catalog), "Impossible product achievement records fail validation");
        var before = RedesignState.Fresh(); before.products[0] = 1;
        bool fail = true; RedesignState persisted = null;
        var failing = new RedesignEngine(catalog, before, s => { if (fail) throw new IOException("disk unavailable"); persisted = Clone(s); });
        try { failing.ClaimAchievement("products-1"); throw new Exception("Save failure was not propagated"); } catch (IOException) { }
        Check(failing.State.kajuKatli.IsZero && failing.State.claimedAchievements == null, "Failed reward save publishes neither currency nor a claimed receipt");
        fail = false;
        Check(failing.ClaimAchievement("products-1") && persisted.kajuKatli.Value == 5 && persisted.claimedAchievements.Count == 1, "Retry saves currency and receipt in the same transaction");
        failing.MutationAllowed = () => false;
        Check(!failing.CanClaimAchievement("products-4") && !failing.ClaimAchievement("products-4"), "Read-only sessions cannot claim achievement rewards");
        var wealthy = RedesignState.Fresh(); wealthy.products = Enumerable.Repeat(1, 8).ToArray(); wealthy.cash = 1e30;
        wealthy.eligibleEarnings = wealthy.target * 4; wealthy.lifetimeEarnings = wealthy.eligibleEarnings;
        var prestige = new RedesignEngine(catalog, wealthy, _ => { });
        Check(prestige.Prestige(prestige.PreviewPrestige()) && prestige.State.products.All(x => x == 0) && prestige.State.mostProductsStocked == 8,
            "Prestige retains the best product collection, including achievements earned before this update");
        Check(!prestige.ClaimAchievement("products-8"), "Later stages cannot be claimed before earlier rewards");
        Check(prestige.ClaimAchievement("products-1") && prestige.ClaimAchievement("products-4") && prestige.ClaimAchievement("products-8") && prestige.ClaimAchievement("prestiges-1"), "Unclaimed product stages and prestige milestones remain claimable after reset");
        var lifetime = RedesignState.Fresh(); lifetime.manualTaps = 10000000; lifetime.lifetimeEarnings = 1e15;
        lifetime.everPurchased = catalog.Upgrades.Select(u => u.id).ToList(); lifetime.mostProductsStocked = 8;
        lifetime.completedPrestiges = 100; lifetime.fame = 100; lifetime.secondPrestigeGift = true;
        lifetime.target = ProgressionNumbers.RunTarget(lifetime.completedPrestiges, lifetime.fame);
        var completed = new RedesignEngine(catalog, lifetime, _ => { });
        Check(RedesignAchievements.ReadyCount(completed.State) == 5, "Only the current stage of each achievement category shows as ready");
        Check(!completed.CanClaimAchievement("taps-1000000"), "Completed future tap stages wait for earlier claims");
        foreach (var a in RedesignAchievements.All) Check(completed.ClaimAchievement(a.Id), "Claim milestone " + a.Id);
        Check(completed.State.kajuKatli.Value == RedesignAchievements.All.Sum(a => a.Reward) && RedesignAchievements.ReadyCount(completed.State) == 0,
            "All achievement payouts sum exactly and leave no duplicate claim");
        Check(RedesignAchievements.CurrentStage(completed.State, AchievementMetric.Taps).Id == "taps-10000000", "Completed achievement categories retain their final stage");
        var oldClaims = RedesignState.Fresh(); oldClaims.manualTaps = 10000;
        oldClaims.claimedAchievements = new List<string> { "taps-10000" };
        Check(oldClaims.Valid(catalog), "Existing saves with previously claimed later milestones remain valid");
        var migrated = new RedesignEngine(catalog, oldClaims, _ => { });
        Check(migrated.ClaimAchievement("taps-100") && migrated.ClaimAchievement("taps-1000") && !migrated.ClaimAchievement("taps-10000") && RedesignAchievements.CurrentStage(migrated.State, AchievementMetric.Taps).Id == "taps-100000", "Existing rewards are preserved and skipped when advancing stages");
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Check(UpgradeEffectCopy.Multiplier(1.25) == "+25%" && UpgradeEffectCopy.Multiplier(2) == "×2", "Bonuses use whole percentages or integer multipliers without changing effects");
            Check(UpgradeEffectCopy.Whole(0) == "0" && UpgradeEffectCopy.Whole(12.01) == "13" && UpgradeEffectCopy.Whole(25.000000000000004) == "25", "Effect values round up without floating point noise or negative zero");
            Check(UpgradeEffectCopy.Cost(.501) == "0.51" && UpgradeEffectCopy.Cost(.5) == "0.50" && UpgradeEffectCopy.Cost(.01) == "0.01", "Cost factors round up to hundredths with a stable decimal point");
            Check(catalog.Upgrades.Where(u => u.effect != CashEffect.ProductCost && u.effect != CashEffect.TapCost && u.effect != CashEffect.UpgradeCost)
                .All(u => !UpgradeEffectCopy.Multiplier(u.factor).Contains('.')), "Every non-cost cash upgrade formats without decimal effects");
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }
}
