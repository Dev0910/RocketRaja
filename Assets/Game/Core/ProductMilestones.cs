using System;

namespace RocketRaja.Core
{
    /// <summary>Stocking is an interval boundary; only profit milestones award multipliers.</summary>
    public static class ProductMilestones
    {
        public static int Previous(int level) => level < 1 ? 0 : level < 25 ? 1 :
            level < 50 ? 25 : level < 100 ? 50 : Math.Min(level, Catalog.MaxLevel) / 100 * 100;
        public static int Next(int level) => level >= Catalog.MaxLevel ? 0 : level < 1 ? 1 :
            level < 25 ? 25 : level < 50 ? 50 : level < 100 ? 100 : (level / 100 + 1) * 100;
        public static int BoostCount(int level) => (level >= 25 ? 1 : 0) + (level >= 50 ? 1 : 0) +
            Math.Max(0, Math.Min(level, Catalog.MaxLevel) / 100);
        public static Amount Multiplier(int level) => Amount.Pow10(BoostCount(level) * Math.Log10(2));
        public static float Progress(int level)
        {
            int next = Next(level), previous = Previous(level);
            return next == 0 ? 1 : Math.Max(0, (level - previous) / (float)(next - previous));
        }
    }
}
