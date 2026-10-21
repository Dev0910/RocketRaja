using RocketRaja.Core;

static partial class Program
{
    sealed class SeededDrawRandom : IDrawRandom
    {
        readonly Random random;
        public SeededDrawRandom(int seed) { random = new Random(seed); }
        public int Next(int maximum) => random.Next(maximum);
    }
    static void DistributionChecks(RedesignCatalog data)
    {
        const int count = 250000, seed = 739103;
        var random = new SeededDrawRandom(seed);
        int[] counts = new int[28];
        for (int i = 0; i < count; i++) counts[TalentDrawPool.Select(data, random)]++;
        double[] tierOdds = { .75, .22, .03 };
        for (int i = 0; i < 28; i++)
        {
            var card = data.Cards[i]; double p = tierOdds[(int)card.tier] / data.TierCount(card.tier);
            Check(Math.Abs(counts[i] - count * p) <= 6 * Math.Sqrt(count * p * (1 - p)), $"Draw distribution {card.id} within six sigma of uniform tier odds ({counts[i]}/{count}, seed {seed})");
        }
        Check(Math.Abs(ProgressionNumbers.CardFactor(CardEffect.OfflineHours, 1) - 10) < 1e-12 && Math.Abs(ProgressionNumbers.CardFactor(CardEffect.OfflineEfficiency, 1) - 7d / 12) < 1e-12, "First offline cards independently provide 10h storage and 58.3333% efficiency");
        Check(Math.Abs(ProgressionNumbers.CardFactor(CardEffect.ProductCost, 1) - .92) < 1e-12 && Math.Abs(ProgressionNumbers.CardFactor(CardEffect.UpgradeCost, 1) - .9) < 1e-12, "First cost cards apply different category-specific discounts");
    }
}
