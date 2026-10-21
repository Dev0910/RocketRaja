using System;

namespace RocketRaja.Core
{
    // Cosmetic progression only. Prestige draw thresholds use their own economic rules.
    public static class RedesignShopProgress
    {
        static readonly int[] thresholds = { 0, 100, 250, 500, 750, 1000 };
        public static int Threshold(int stage)
        {
            if (stage < 0 || stage > 5) throw new ArgumentOutOfRangeException(nameof(stage));
            return thresholds[stage];
        }
        public static int StageForUnique(int unique)
        {
            if (unique < 0 || unique > 1000) throw new ArgumentOutOfRangeException(nameof(unique));
            for (int stage = 5; stage > 0; stage--) if (unique >= thresholds[stage]) return stage;
            return 0;
        }
    }
    public sealed partial class RedesignEngine
    {
        // Derive older saves' unlocked presentation without saving or replaying milestones.
        // The durable earned stage remains a floor if presentation thresholds are revised later.
        public int ShopAppearance => Math.Max(State.decorationStage, RedesignShopProgress.StageForUnique(State.UniqueCount));
    }
}
