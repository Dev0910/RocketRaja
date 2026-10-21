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
        public AchievementDefinition(string id, string title, string description, AchievementMetric metric, long target, int reward)
        { Id = id; Title = title; Description = description; Metric = metric; Target = target; Reward = reward; }

        public bool Complete(RedesignState state) => Metric == AchievementMetric.Earnings
            ? state.lifetimeEarnings >= new Amount(Target) : Current(state) >= Target;

        public System.Numerics.BigInteger Current(RedesignState state)
        {
            switch (Metric)
            {
                case AchievementMetric.Taps: return state.manualTaps.Value;
                case AchievementMetric.Upgrades: return state.UniqueCount;
                case AchievementMetric.Products: return Math.Max(state.mostProductsStocked, state.products.Count(n => n > 0));
                case AchievementMetric.Prestiges: return state.completedPrestiges.Value;
                default: return 0;
            }
        }
        public string Progress(RedesignState state)
        {
            if (Metric == AchievementMetric.Earnings)
                return "₹" + (Complete(state) ? new Amount(Target) : state.lifetimeEarnings) + " / ₹" + new Amount(Target);
            return System.Numerics.BigInteger.Min(Current(state), Target).ToString("N0", CultureInfo.InvariantCulture)
                + " / " + Target.ToString("N0", CultureInfo.InvariantCulture);
        }
    }

    public static class RedesignAchievements
    {
        public static readonly ReadOnlyCollection<AchievementDefinition> All = Array.AsReadOnly(new[] {
            new AchievementDefinition("taps-100", "First sparks", "Tap the stall 100 times.", AchievementMetric.Taps, 100, 10),
            new AchievementDefinition("taps-1000", "Busy counter", "Tap the stall 1,000 times.", AchievementMetric.Taps, 1000, 25),
            new AchievementDefinition("taps-10000", "Raja of the counter", "Tap the stall 10,000 times.", AchievementMetric.Taps, 10000, 50),
            new AchievementDefinition("taps-100000", "Festival crowds", "Tap the stall 100,000 times.", AchievementMetric.Taps, 100000, 75),
            new AchievementDefinition("taps-1000000", "Million sparks", "Tap the stall 1,000,000 times.", AchievementMetric.Taps, 1000000, 100),
            new AchievementDefinition("taps-10000000", "Endless festival", "Tap the stall 10,000,000 times.", AchievementMetric.Taps, 10000000, 150),
            new AchievementDefinition("earnings-1000", "A promising start", "Earn ₹1,000 across all runs.", AchievementMetric.Earnings, 1000, 10),
            new AchievementDefinition("earnings-1000000", "Festival favourite", "Earn ₹1 million across all runs.", AchievementMetric.Earnings, 1000000, 25),
            new AchievementDefinition("earnings-1000000000", "A billion bright lights", "Earn ₹1 billion across all runs.", AchievementMetric.Earnings, 1000000000, 50),
            new AchievementDefinition("earnings-1000000000000", "Festival empire", "Earn ₹1 trillion across all runs.", AchievementMetric.Earnings, 1000000000000, 100),
            new AchievementDefinition("earnings-1000000000000000", "Golden legacy", "Earn ₹1 quadrillion across all runs.", AchievementMetric.Earnings, 1000000000000000, 150),
            new AchievementDefinition("upgrades-10", "Better every day", "Discover 10 different upgrades.", AchievementMetric.Upgrades, 10, 10),
            new AchievementDefinition("upgrades-100", "An eye for upgrades", "Discover 100 different upgrades.", AchievementMetric.Upgrades, 100, 50),
            new AchievementDefinition("upgrades-1000", "The complete collection", "Discover all 1,000 upgrades.", AchievementMetric.Upgrades, 1000, 150),
            new AchievementDefinition("products-1", "Open for business", "Stock your first product.", AchievementMetric.Products, 1, 5),
            new AchievementDefinition("products-4", "Something for everyone", "Stock 4 different products together.", AchievementMetric.Products, 4, 15),
            new AchievementDefinition("products-8", "A fully lit stall", "Stock all 8 products together.", AchievementMetric.Products, 8, 25),
            new AchievementDefinition("prestiges-1", "A brighter beginning", "Prestige for the first time.", AchievementMetric.Prestiges, 1, 25),
            new AchievementDefinition("prestiges-5", "Back and brighter", "Complete 5 prestiges.", AchievementMetric.Prestiges, 5, 75),
            new AchievementDefinition("prestiges-10", "Rocket Raja", "Complete 10 prestiges.", AchievementMetric.Prestiges, 10, 150),
            new AchievementDefinition("prestiges-25", "Festival veteran", "Complete 25 prestiges.", AchievementMetric.Prestiges, 25, 200),
            new AchievementDefinition("prestiges-50", "Ever brighter", "Complete 50 prestiges.", AchievementMetric.Prestiges, 50, 250),
            new AchievementDefinition("prestiges-100", "Timeless Raja", "Complete 100 prestiges.", AchievementMetric.Prestiges, 100, 300)
        });
        static readonly Dictionary<string, AchievementDefinition> Index = All.ToDictionary(a => a.Id, StringComparer.Ordinal);
        public static bool TryGet(string id, out AchievementDefinition achievement)
        { achievement = null; return id != null && Index.TryGetValue(id, out achievement); }
        public static bool Claimed(RedesignState state, string id) => state.claimedAchievements != null && state.claimedAchievements.Contains(id);
        public static AchievementDefinition CurrentStage(RedesignState state, AchievementMetric metric) =>
            All.FirstOrDefault(a => a.Metric == metric && !Claimed(state, a.Id)) ?? All.Last(a => a.Metric == metric);
        public static bool StageReady(RedesignState state, AchievementDefinition a) =>
            a != null && !Claimed(state, a.Id) && CurrentStage(state, a.Metric).Id == a.Id && a.Complete(state);
        public static int StageNumber(AchievementDefinition a) => All.Count(x => x.Metric == a.Metric && x.Target <= a.Target);
        public static int StageCount(AchievementMetric metric) => All.Count(a => a.Metric == metric);
        public static int ReadyCount(RedesignState state) => All.Count(a => StageReady(state, a));
        public static bool ValidClaims(RedesignState state) => state.mostProductsStocked >= 0 && state.mostProductsStocked <= 8 &&
            (state.claimedAchievements == null || state.claimedAchievements.Count <= All.Count &&
             state.claimedAchievements.Distinct(StringComparer.Ordinal).Count() == state.claimedAchievements.Count &&
             state.claimedAchievements.All(id => TryGet(id, out var a) && a.Complete(state)));
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
