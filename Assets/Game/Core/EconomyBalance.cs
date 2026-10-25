using System;

namespace RocketRaja.Core
{
    /// <summary>Versioned economy constants shared by authoring, gameplay and the balance lab.</summary>
    public static class EconomyBalance
    {
        public const string Revision = "tap-start-2026-10-25";
        public const double LevelGrowth = 1.12, ProductCostGrowth = 14, ProductIncomeGrowth = 2.5;
        public const double UpgradeBase = 20, UpgradeSlope = .36, UpgradeCurvature = .00005;
        public static double TapBase { get; private set; } = 1;
        // Offline calibration only. These remain fixed for every player, independent
        // of taps, cards, purchases and elapsed play time. Every upgrade still applies
        // its full authored multiplier on top of these Fame bonuses.
        internal static readonly double[] TapFameScales = { 1, 5.3, 2, 2, 5, 2, 5, 5, 3.2, 4.4, .75, 2, 1.5, 1, 6.5, 1.95, 3, 4.7, 2, 3, 2, 3, 1.5, .3, .03, .95, .6, 3, 1.5, 2, .6, .6, 2, .55, .55 };
        internal static readonly double[] TargetOffsetLogs = { 0, -.29, -.67, .13, 0, .53, -.08, 1.26, 2.2, .4, .43, -.37, .13, -.47, -.01, -1.13, .21, .99, 1.2, .41, .37, .04, -.54, -.03, -1.64, .75, -.05, .42, .27, .03, .02, -.45, 1.89, -1.48, -1.48 };
        static double StageValue(double[] values, Natural fame, double fallback) => values.Length == 0 ? fallback : values[fame.Value >= values.Length ? values.Length - 1 : (int)fame.Value];
        public static double TapFameScale(Natural fame)
        {
#if REDESIGN_LAB
            if (!double.IsNaN(TapFameScaleOverride)) return TapFameScaleOverride;
#endif
            return StageValue(TapFameScales, fame, 1);
        }
        public static double TargetOffset(Natural fame) => StageValue(TargetOffsetLogs, fame, 0);
        // Offline fit, relaxed active policy, no cards. Values are log10(total Fame income multiplier).
        internal static readonly double[] FameIncomeLogs = { 0, 0.7781512503836436, 1.29823, 1.58926, 3.0512, 3.86303, 4.40702, 5.81866, 6.93064, 7.29544, 9.02875, 10.24088, 10.60533, 12.30848, 13.48456, 14.18667, 15.32748, 17.19432, 17.94086, 18.87212, 20.77355, 21.98677, 22.56694, 23.94351, 25.7434, 26.36133, 28.00599, 29.95205, 31.30884, 32.08603, 33.75571, 35.67975, 36.71929, 38.59057, 42.57681 };
        public static double FameStage(Natural fame)
        {
            int last = FameIncomeLogs.Length - 1;
            if (fame.Value <= last) return (int)fame.Value;
            return Math.Min(90000, last * (1 + (fame.ToAmount().Log10 - Math.Log10(last)) * Math.Log(10)));
        }
        public static Amount FameIncome(Natural fame)
        {
            double stage = FameStage(fame); int last = FameIncomeLogs.Length - 1;
            if (stage <= last) return fame.Value == 1 ? new Amount(6) : Amount.Pow10(FameIncomeLogs[(int)stage]);
            return Amount.Pow10(FameIncomeLogs[last] + (stage - last) * (FameIncomeLogs[last] - FameIncomeLogs[last - 1]));
        }
        public static double TargetGrowth { get; private set; } = 100000;
        public static Amount SecondTarget { get; private set; } = new Amount(3, 23);
        public static Amount FirstTarget { get; private set; } = new Amount(6, 21);
#if REDESIGN_LAB
        internal static string CardCurve = "original";
        internal static double HandsPayoutScale = 1;
        internal static double CardGlobalGainOverride = double.NaN;
        internal static double IncomeLog = double.NaN;
        internal static double TapFameScaleOverride = double.NaN;
        internal static void Calibrate(double tap, double target, double first, double second)
        { TapBase = tap; TargetGrowth = target; FirstTarget = Amount.Pow10(first); SecondTarget = Amount.Pow10(second); }
#endif
        public static double ProductCost(int product) => 30 * Math.Pow(ProductCostGrowth, product);
        public static double ProductIncome(int product) => .85 * Math.Pow(ProductIncomeGrowth, product);
        public static Amount UpgradePrice(int index) => Amount.Pow10(Math.Log10(UpgradeBase) + (UpgradeSlope * index + UpgradeCurvature * index * index) / Math.Log(10));
        public static int NextMilestone(int level) => level >= Catalog.MaxLevel ? 0 : level < 10 ? 10 : level < 25 ? 25 : level < 50 ? 50 : (level / 100 + 1) * 100;
        public static int PreviousMilestone(int level) => level < 10 ? 0 : level < 25 ? 10 : level < 50 ? 25 : level < 100 ? 50 : level / 100 * 100;
        public static int MilestoneReward(int level) => level <= 25 ? 2 : level == 50 ? 3 : level == 100 ? 4 : 5;
        public static Amount MilestoneMultiplier(int level)
        {
            double log = (level >= 10 ? Math.Log10(2) : 0) + (level >= 25 ? Math.Log10(2) : 0)
                + (level >= 50 ? Math.Log10(3) : 0) + (level >= 100 ? Math.Log10(4) : 0)
                + Math.Max(0, level / 100 - 1) * Math.Log10(5);
            return Amount.Pow10(log);
        }
        public static int SynergyCount(int minimumLevel) => (minimumLevel >= 50 ? 1 : 0) + minimumLevel / 100;
        public static int NextSynergy(int minimumLevel) => minimumLevel >= Catalog.MaxLevel ? 0 : minimumLevel < 50 ? 50 : (minimumLevel / 100 + 1) * 100;
        public static Amount SynergyMultiplier(int minimumLevel) => Amount.Pow10(SynergyCount(minimumLevel) * Math.Log10(2));
    }
}
