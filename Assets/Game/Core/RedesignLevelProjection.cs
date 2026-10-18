using System;

namespace RocketRaja.Core
{
    public sealed class LevelProjection
    {
        public int product, beforeLevel, afterLevel, nextMilestone;
        public bool available;
        public Quote quote;
        public Amount normalBefore, normalAfter, productBefore, productAfter, tapBefore, tapAfter, linkedTapBefore, linkedTapAfter;
    }

    public sealed partial class RedesignEngine
    {
        // Reserved projection fields remain zero for older presentation consumers.
        Amount LinkedTapBase(bool hands, RedesignState s, RunEffects f) => Amount.Zero;
        public LevelProjection PreviewLevels(int product, int quantity)
        {
            if (product < -1 || product >= 8) throw new ArgumentOutOfRangeException(nameof(product));
            bool tap = product == -1;
            var quote = LevelQuote(product, quantity); var after = State.Copy();
            int before = tap ? State.tapLevel : State.products[product];
            if (tap) after.tapLevel += quote.quantity; else after.products[product] += quote.quantity;
            int milestone = EconomyBalance.NextMilestone(before);
            return new LevelProjection
            {
                product = product, beforeLevel = before, afterLevel = before + quote.quantity,
                nextMilestone = milestone <= Catalog.MaxLevel ? milestone : 0,
                available = tap || ProductAvailable(product), quote = quote,
                normalBefore = NormalIncome(), normalAfter = NormalIncome(after, Effects),
                productBefore = tap ? Amount.Zero : ProductIncome(product), productAfter = tap ? Amount.Zero : ProductIncome(product, after, Effects),
                tapBefore = BaseTapPayout(false, State, Effects), tapAfter = BaseTapPayout(false, after, Effects),
                linkedTapBefore = LinkedTapBase(false, State, Effects) * ProgressionNumbers.CardFactor(CardEffect.TapProfit, State.cardLevels[16]),
                linkedTapAfter = LinkedTapBase(false, after, Effects) * ProgressionNumbers.CardFactor(CardEffect.TapProfit, after.cardLevels[16])
            };
        }
    }
}
