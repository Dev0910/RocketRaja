using System.Globalization;
using RocketRaja.Core;

static partial class Program
{
    static void AchievementChecks(RedesignCatalog catalog)
    {
        var fresh = RedesignState.Fresh();
        fresh.cash = 1e10; // Fixture funding for the ownership checks.
        var engine = new RedesignEngine(catalog, fresh, _ => { });
        Check(fresh.Valid(catalog) && RedesignAchievements.ReadyCount(fresh) == 0, "Fresh and pre-achievement saves load with no earned rewards");
        Check(!engine.ClaimAchievement("products-1") && !engine.ClaimAchievement("unknown") && !engine.ClaimAchievement(null), "Incomplete and unknown achievements cannot grant currency");
        Check(engine.BuyLevels(0, 1) && engine.State.mostProductsStocked == 1, "Product ownership records permanent achievement progress");
        Check(RedesignAchievements.ReadyCount(engine.State) == 0, "The first product no longer grants an immediate achievement");
        Check(engine.BuyLevels(1, 1) && engine.BuyLevels(2, 1) && engine.BuyLevels(3, 1), "First product reward now requires four different products");
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
        var before = RedesignState.Fresh(); before.products = new[] { 1, 1, 1, 1, 0, 0, 0, 0 };
        bool fail = true; RedesignState persisted = null;
        var failing = new RedesignEngine(catalog, before, s => { if (fail) throw new IOException("disk unavailable"); persisted = Clone(s); });
        try { failing.ClaimAchievement("products-1"); throw new Exception("Save failure was not propagated"); } catch (IOException) { }
        Check(failing.State.kajuKatli.IsZero && failing.State.claimedAchievements == null, "Failed reward save publishes neither currency nor a claimed receipt");
        fail = false;
        Check(failing.ClaimAchievement("products-1") && persisted.kajuKatli.Value == 5 && persisted.claimedAchievements.Count == 1, "Retry saves currency and receipt in the same transaction");
        failing.MutationAllowed = () => false;
        Check(!failing.CanClaimAchievement("products-4") && !failing.ClaimAchievement("products-4"), "Read-only sessions cannot claim achievement rewards");
        var wealthy = RedesignState.Fresh(); wealthy.products = Enumerable.Repeat(25, 8).ToArray(); wealthy.cash = 1e30;
        wealthy.eligibleEarnings = wealthy.target * 4; wealthy.lifetimeEarnings = wealthy.eligibleEarnings;
        var prestige = new RedesignEngine(catalog, wealthy, _ => { });
        Check(prestige.Prestige(prestige.PreviewPrestige()) && prestige.State.products.All(x => x == 0) && prestige.State.mostProductsStocked == 8 && prestige.State.bestProductCollectionLevel == 25,
            "Prestige retains the best product collection, including achievements earned before this update");
        Check(!prestige.ClaimAchievement("products-8"), "Later stages cannot be claimed before earlier rewards");
        Check(prestige.ClaimAchievement("products-1") && prestige.ClaimAchievement("products-4") && prestige.ClaimAchievement("products-8") && prestige.ClaimAchievement("products-level-25"), "Unclaimed collection and product level milestones remain claimable after reset");
        Check(!prestige.CanClaimAchievement("prestiges-1"), "One prestige no longer earns the three-prestige reward");
        var reloadedPrestige = new RedesignEngine(catalog, Clone(prestige.State), _ => { });
        Check(reloadedPrestige.State.bestProductCollectionLevel == 25 && !reloadedPrestige.ClaimAchievement("products-level-25"), "New product level receipts and records survive serialization without duplicate rewards");
        var lifetime = RedesignState.Fresh(); lifetime.manualTaps = 25000000; lifetime.lifetimeEarnings = new Amount(1, 180);
        lifetime.everPurchased = catalog.Upgrades.Select(u => u.id).ToList(); lifetime.mostProductsStocked = 8;
        lifetime.bestProductCollectionLevel = 500;
        lifetime.completedPrestiges = 1000; lifetime.fame = 1000; lifetime.secondPrestigeGift = true;
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
        Check(migrated.ClaimAchievement("taps-100") && migrated.ClaimAchievement("taps-1000") && migrated.ClaimAchievement("taps-7500") && !migrated.ClaimAchievement("taps-10000") && RedesignAchievements.CurrentStage(migrated.State, AchievementMetric.Taps).Id == "taps-75000", "Existing rewards are preserved and skipped when advancing stages");
        Check(RedesignAchievements.All.Count == 40 && Enum.GetValues<AchievementMetric>().All(m => RedesignAchievements.StageCount(m) == 8), "Forty milestones provide eight stages in each existing category");
        Check(RedesignAchievements.All.Select(a => a.Id).Distinct().Count() == 40, "All forty receipt IDs are unique");
        foreach (AchievementMetric metric in Enum.GetValues<AchievementMetric>())
        {
            var stages = RedesignAchievements.All.Where(a => a.Metric == metric).ToArray();
            Check(stages.Select(RedesignAchievements.StageNumber).SequenceEqual(Enumerable.Range(1, 8)), "Sequential stage labels for " + metric);
            Check(stages.Zip(stages.Skip(1), (a, b) => metric == AchievementMetric.Earnings ? b.EarningsTarget > a.EarningsTarget
                : metric == AchievementMetric.Products ? b.ProductLevel > a.ProductLevel || b.ProductLevel == a.ProductLevel && b.Target > a.Target
                : b.Target > a.Target).All(x => x), "Strictly harder successive stages for " + metric);
        }
        foreach (var a in RedesignAchievements.All)
        {
            var boundary = RedesignState.Fresh();
            void SetProgress(bool reached)
            {
                long target = a.Target - (reached ? 0 : 1);
                switch (a.Metric)
                {
                    case AchievementMetric.Taps: boundary.manualTaps = target; break;
                    case AchievementMetric.Earnings: boundary.lifetimeEarnings = reached ? a.EarningsTarget : a.EarningsTarget * .999; break;
                    case AchievementMetric.Upgrades: boundary.everPurchased = catalog.Upgrades.Take((int)target).Select(u => u.id).ToList(); break;
                    case AchievementMetric.Products:
                        boundary.products = Enumerable.Range(0, 8).Select(i => i < target ? a.ProductLevel : 0).ToArray(); break;
                    case AchievementMetric.Prestiges: boundary.completedPrestiges = target; break;
                }
            }
            SetProgress(false); Check(!a.Complete(boundary), "Below target stays incomplete: " + a.Id);
            SetProgress(true); Check(a.Complete(boundary), "Exact target completes: " + a.Id);
        }
        // Each of the 23 shipped receipts must still load at its original threshold.
        var legacyIds = new[] { "taps-100", "taps-1000", "taps-10000", "taps-100000", "taps-1000000", "taps-10000000",
            "earnings-1000", "earnings-1000000", "earnings-1000000000", "earnings-1000000000000", "earnings-1000000000000000",
            "upgrades-10", "upgrades-100", "upgrades-1000", "products-1", "products-4", "products-8",
            "prestiges-1", "prestiges-5", "prestiges-10", "prestiges-25", "prestiges-50", "prestiges-100" };
        foreach (string id in legacyIds)
        {
            RedesignAchievements.TryGet(id, out var a);
            long oldTarget = long.Parse(id.Substring(id.IndexOf('-') + 1), CultureInfo.InvariantCulture);
            var legacy = RedesignState.Fresh();
            switch (a.Metric)
            {
                case AchievementMetric.Taps: legacy.manualTaps = oldTarget; break;
                case AchievementMetric.Earnings: legacy.lifetimeEarnings = new Amount(oldTarget); break;
                case AchievementMetric.Upgrades: legacy.everPurchased = catalog.Upgrades.Take((int)oldTarget).Select(u => u.id).ToList(); break;
                case AchievementMetric.Products: legacy.mostProductsStocked = (int)oldTarget; break;
                case AchievementMetric.Prestiges:
                    legacy.completedPrestiges = legacy.fame = oldTarget; legacy.secondPrestigeGift = oldTarget >= 2;
                    legacy.target = ProgressionNumbers.RunTarget(legacy.completedPrestiges, legacy.fame); break;
            }
            legacy.claimedAchievements = new List<string> { id };
            Check(legacy.Valid(catalog) && Clone(legacy).Valid(catalog), "Previously earned receipt remains valid at original threshold: " + id);
            var oldEngine = new RedesignEngine(catalog, legacy, _ => { });
            Check(!oldEngine.ClaimAchievement(id), "Legacy receipt cannot pay twice: " + id);
            legacy.claimedAchievements = null;
            Check(a.Complete(legacy) || !new RedesignEngine(catalog, legacy, _ => { }).CanClaimAchievement(id), "Unclaimed old thresholds never bypass harder goals: " + id);
        }
        var invalidRecord = RedesignState.Fresh(); invalidRecord.bestProductCollectionLevel = Catalog.MaxLevel + 1;
        Check(!invalidRecord.Valid(catalog), "Impossible product level records fail validation");
        invalidRecord.bestProductCollectionLevel = -1;
        Check(!invalidRecord.Valid(catalog), "Negative product level records fail validation");
        var invalidClaim = RedesignState.Fresh(); invalidClaim.claimedAchievements = new List<string> { "products-level-25" };
        Check(!invalidClaim.Valid(catalog), "Unearned new product level receipts fail validation");
        var levelSave = RedesignState.Fresh(); levelSave.products = Enumerable.Repeat(25, 8).ToArray();
        levelSave.claimedAchievements = new List<string> { "products-1", "products-4", "products-8" };
        var failLevel = new RedesignEngine(catalog, levelSave, _ => throw new IOException("disk unavailable"));
        try { failLevel.ClaimAchievement("products-level-25"); throw new Exception("Save failure was not propagated"); } catch (IOException) { }
        Check(failLevel.State.bestProductCollectionLevel == 0 && failLevel.State.kajuKatli.IsZero && failLevel.State.claimedAchievements.Count == 3,
            "Failed product level reward save publishes neither historical progress nor currency nor receipt");
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
