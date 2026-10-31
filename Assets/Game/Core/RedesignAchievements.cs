using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace RocketRaja.Core
{
    public enum AchievementMetric { Taps, Earnings, Upgrades, Products, Prestiges }

    public sealed class AchievementDefinition
    {
        public readonly string Id, Title, Description;
        public readonly AchievementMetric Metric;
        public readonly long Target;
        public readonly int Reward;
        public readonly Amount EarningsTarget;
        public readonly int ProductLevel;
        public AchievementDefinition(string id, string title, string description, AchievementMetric metric, long target, int reward, int productLevel = 1)
        { Id = id; Title = title; Description = description; Metric = metric; Target = target; Reward = reward; EarningsTarget = new Amount(target); ProductLevel = productLevel; }
        public AchievementDefinition(string id, string title, Amount target, int reward)
        { Id = id; Title = title; Description = "Earn ₹" + target + " across all runs."; Metric = AchievementMetric.Earnings; EarningsTarget = target; Reward = reward; }

        public bool Complete(RedesignState state) => Metric == AchievementMetric.Earnings
            ? state.lifetimeEarnings >= EarningsTarget : Current(state) >= Target;

        public System.Numerics.BigInteger Current(RedesignState state)
        {
            switch (Metric)
            {
                case AchievementMetric.Taps: return state.manualTaps.Value;
                case AchievementMetric.Upgrades: return state.UniqueCount;
                case AchievementMetric.Products: return ProductLevel <= 1
                    ? Math.Max(state.mostProductsStocked, state.products.Count(n => n > 0))
                    : state.bestProductCollectionLevel >= ProductLevel ? 8 : state.products.Count(n => n >= ProductLevel);
                case AchievementMetric.Prestiges: return state.completedPrestiges.Value;
                default: return 0;
            }
        }
        public string Progress(RedesignState state)
        {
            if (Metric == AchievementMetric.Earnings)
                return "₹" + (Complete(state) ? EarningsTarget : state.lifetimeEarnings) + " / ₹" + EarningsTarget;
            return System.Numerics.BigInteger.Min(Current(state), Target).ToString("N0", CultureInfo.InvariantCulture)
                + " / " + Target.ToString("N0", CultureInfo.InvariantCulture);
        }
    }

    public static class RedesignAchievements
    {
        public static readonly ReadOnlyCollection<AchievementDefinition> All = Array.AsReadOnly(new[] {
            // IDs are permanent save receipts; their numeric suffixes predate this balance revision.
            new AchievementDefinition("taps-100", "First sparks", "Tap the stall 500 times.", AchievementMetric.Taps, 500, 10),
            new AchievementDefinition("taps-1000", "Busy counter", "Tap the stall 2,500 times.", AchievementMetric.Taps, 2500, 25),
            new AchievementDefinition("taps-7500", "Steady sparks", "Tap the stall 7,500 times.", AchievementMetric.Taps, 7500, 35),
            new AchievementDefinition("taps-10000", "Raja of the counter", "Tap the stall 25,000 times.", AchievementMetric.Taps, 25000, 50),
            new AchievementDefinition("taps-75000", "Nonstop celebrations", "Tap the stall 75,000 times.", AchievementMetric.Taps, 75000, 60),
            new AchievementDefinition("taps-100000", "Festival crowds", "Tap the stall 250,000 times.", AchievementMetric.Taps, 250000, 75),
            new AchievementDefinition("taps-1000000", "Million sparks", "Tap the stall 2,500,000 times.", AchievementMetric.Taps, 2500000, 100),
            new AchievementDefinition("taps-10000000", "Endless festival", "Tap the stall 25,000,000 times.", AchievementMetric.Taps, 25000000, 150),
            new AchievementDefinition("earnings-1000", "A promising start", new Amount(1, 9), 10),
            new AchievementDefinition("earnings-1000000", "Festival favourite", new Amount(1, 15), 25),
            new AchievementDefinition("earnings-1000000000", "A sky full of lights", new Amount(6, 21), 50),
            new AchievementDefinition("earnings-1000000000000", "Festival empire", new Amount(1, 30), 100),
            new AchievementDefinition("earnings-1000000000000000", "Golden legacy", new Amount(1, 45), 150),
            new AchievementDefinition("earnings-1e75", "Beyond the horizon", new Amount(1, 75), 200),
            new AchievementDefinition("earnings-1e120", "A world of celebrations", new Amount(1, 120), 250),
            new AchievementDefinition("earnings-1e180", "An everlasting legacy", new Amount(1, 180), 300),
            new AchievementDefinition("upgrades-10", "Better every day", "Discover 25 different upgrades.", AchievementMetric.Upgrades, 25, 10),
            new AchievementDefinition("upgrades-75", "Always improving", "Discover 75 different upgrades.", AchievementMetric.Upgrades, 75, 25),
            new AchievementDefinition("upgrades-100", "An eye for upgrades", "Discover 150 different upgrades.", AchievementMetric.Upgrades, 150, 50),
            new AchievementDefinition("upgrades-250", "A growing collection", "Discover 250 different upgrades.", AchievementMetric.Upgrades, 250, 65),
            new AchievementDefinition("upgrades-400", "Festival innovator", "Discover 400 different upgrades.", AchievementMetric.Upgrades, 400, 80),
            new AchievementDefinition("upgrades-600", "Master of improvements", "Discover 600 different upgrades.", AchievementMetric.Upgrades, 600, 100),
            new AchievementDefinition("upgrades-800", "Almost every trick", "Discover 800 different upgrades.", AchievementMetric.Upgrades, 800, 125),
            new AchievementDefinition("upgrades-1000", "The complete collection", "Discover all 1,000 upgrades.", AchievementMetric.Upgrades, 1000, 150),
            new AchievementDefinition("products-1", "Open for business", "Stock 4 different products together.", AchievementMetric.Products, 4, 5),
            new AchievementDefinition("products-4", "Something for everyone", "Stock 6 different products together.", AchievementMetric.Products, 6, 15),
            new AchievementDefinition("products-8", "A fully lit stall", "Stock all 8 products together.", AchievementMetric.Products, 8, 25),
            new AchievementDefinition("products-level-25", "Every shelf shining", "Reach level 25 on all 8 products together.", AchievementMetric.Products, 8, 40, 25),
            new AchievementDefinition("products-level-50", "A stronger stall", "Reach level 50 on all 8 products together.", AchievementMetric.Products, 8, 60, 50),
            new AchievementDefinition("products-level-100", "A hundred bright lights", "Reach level 100 on all 8 products together.", AchievementMetric.Products, 8, 80, 100),
            new AchievementDefinition("products-level-250", "Festival craftsmanship", "Reach level 250 on all 8 products together.", AchievementMetric.Products, 8, 110, 250),
            new AchievementDefinition("products-level-500", "A stall for the ages", "Reach level 500 on all 8 products together.", AchievementMetric.Products, 8, 150, 500),
            new AchievementDefinition("prestiges-1", "A brighter beginning", "Complete 3 prestiges.", AchievementMetric.Prestiges, 3, 25),
            new AchievementDefinition("prestiges-5", "Back and brighter", "Complete 10 prestiges.", AchievementMetric.Prestiges, 10, 75),
            new AchievementDefinition("prestiges-10", "Rocket Raja", "Complete 25 prestiges.", AchievementMetric.Prestiges, 25, 150),
            new AchievementDefinition("prestiges-25", "Festival veteran", "Complete 50 prestiges.", AchievementMetric.Prestiges, 50, 200),
            new AchievementDefinition("prestiges-50", "Ever brighter", "Complete 100 prestiges.", AchievementMetric.Prestiges, 100, 250),
            new AchievementDefinition("prestiges-100", "Timeless Raja", "Complete 250 prestiges.", AchievementMetric.Prestiges, 250, 300),
            new AchievementDefinition("prestiges-500", "Five hundred new dawns", "Complete 500 prestiges.", AchievementMetric.Prestiges, 500, 350),
            new AchievementDefinition("prestiges-1000", "The eternal festival", "Complete 1,000 prestiges.", AchievementMetric.Prestiges, 1000, 400)
        });
        static readonly Dictionary<string, AchievementDefinition> Index = All.ToDictionary(a => a.Id, StringComparer.Ordinal);
        public static bool TryGet(string id, out AchievementDefinition achievement)
        { achievement = null; return id != null && Index.TryGetValue(id, out achievement); }
        public static bool Claimed(RedesignState state, string id) => state.claimedAchievements != null && state.claimedAchievements.Contains(id);
        public static AchievementDefinition CurrentStage(RedesignState state, AchievementMetric metric) =>
            All.FirstOrDefault(a => a.Metric == metric && !Claimed(state, a.Id)) ?? All.Last(a => a.Metric == metric);
        public static bool StageReady(RedesignState state, AchievementDefinition a) =>
            a != null && !Claimed(state, a.Id) && CurrentStage(state, a.Metric).Id == a.Id && a.Complete(state);
        public static int StageNumber(AchievementDefinition a) => All.TakeWhile(x => x.Id != a.Id).Count(x => x.Metric == a.Metric) + 1;
        public static int StageCount(AchievementMetric metric) => All.Count(a => a.Metric == metric);
        public static int ReadyCount(RedesignState state) => All.Count(a => StageReady(state, a));
        static readonly HashSet<string> LegacyIds = new HashSet<string>(new[] {
            "taps-100", "taps-1000", "taps-10000", "taps-100000", "taps-1000000", "taps-10000000",
            "earnings-1000", "earnings-1000000", "earnings-1000000000", "earnings-1000000000000", "earnings-1000000000000000",
            "upgrades-10", "upgrades-100", "upgrades-1000", "products-1", "products-4", "products-8",
            "prestiges-1", "prestiges-5", "prestiges-10", "prestiges-25", "prestiges-50", "prestiges-100"
        }, StringComparer.Ordinal);
        // Only save validation accepts the original thresholds. New claims always use the harder goals.
        static bool LegacyComplete(RedesignState state, AchievementDefinition a)
        {
            if (!LegacyIds.Contains(a.Id)) return false;
            long originalTarget = long.Parse(a.Id.Substring(a.Id.IndexOf('-') + 1), CultureInfo.InvariantCulture);
            return a.Metric == AchievementMetric.Earnings ? state.lifetimeEarnings >= new Amount(originalTarget)
                : a.Current(state) >= originalTarget;
        }
        public static bool ValidClaims(RedesignState state) => state.mostProductsStocked >= 0 && state.mostProductsStocked <= 8 &&
            state.bestProductCollectionLevel >= 0 && state.bestProductCollectionLevel <= Catalog.MaxLevel &&
            (state.claimedAchievements == null || state.claimedAchievements.Count <= All.Count &&
             state.claimedAchievements.Distinct(StringComparer.Ordinal).Count() == state.claimedAchievements.Count &&
             state.claimedAchievements.All(id => TryGet(id, out var a) && (a.Complete(state) || LegacyComplete(state, a))));
    }

    public sealed partial class RedesignEngine
    {
        public bool CanClaimAchievement(string id) => !State.HasPremium && MutationAllowed() &&
            RedesignAchievements.TryGet(id, out var a) && RedesignAchievements.StageReady(State, a);

        public bool ClaimAchievement(string id)
        {
            if (!CanClaimAchievement(id) || !RedesignAchievements.TryGet(id, out var achievement)) return false;
            return Commit(s => {
                if (s.claimedAchievements == null) s.claimedAchievements = new List<string>();
                s.claimedAchievements.Add(id);
                s.kajuKatli += (Natural)achievement.Reward;
                return true;
            });
        }
    }
}
