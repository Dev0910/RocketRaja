using System.Globalization;
using RocketRaja.Core;

namespace RocketRaja
{
    public static class RedesignCardCopy
    {
        public static string Count(Natural n, bool full = false) => full || n.digits.Length < 10 ? n.digits : n.ToAmount().ToString();
        public static string Odds(RedesignCatalog data, TalentCard card)
        {
            int percent = card.tier == CardTier.Common ? 75 : card.tier == CardTier.Rare ? 22 : 3;
            int count = data.TierCount(card.tier);
            return card.tier + " " + percent + "% ÷ " + count + " cards = " + ((double)percent / count).ToString("0.####", CultureInfo.InvariantCulture) + "% per draw";
        }
        public static string Effect(TalentCard card, Natural level, bool full = false)
        {
            string factor = card.effect == CardEffect.ProductProfit ? "+" + (level.Value * 20).ToString(CultureInfo.InvariantCulture) + "%"
                : card.effect == CardEffect.GlobalProfit ? "+" + (level.Value * 25).ToString(CultureInfo.InvariantCulture) + "%"
                : UpgradeEffectCopy.Multiplier(ProgressionNumbers.CardFactor(card.effect, level));
            switch (card.effect)
            {
                case CardEffect.ProductProfit: return "Product income " + factor;
                case CardEffect.GlobalProfit: return "Normal product and direct tap income " + factor;
                case CardEffect.ProductCost: return "Product level cost ×" + UpgradeEffectCopy.Cost(ProgressionNumbers.CardFactor(card.effect, level));
                case CardEffect.TapCost: return "Tap level cost ×" + UpgradeEffectCopy.Cost(ProgressionNumbers.CardFactor(card.effect, level));
                case CardEffect.UpgradeCost: return "Cash upgrade prices ×" + UpgradeEffectCopy.Cost(ProgressionNumbers.CardFactor(card.effect, level));
                case CardEffect.TapProfit: return "Tap and Helping Hands payout " + factor;
                case CardEffect.SkillCooldown: return "Skill cooldown −" + UpgradeEffectCopy.Reduction(ProgressionNumbers.CardFactor(card.effect, level));
                case CardEffect.SkillDuration: return "Skill duration " + factor;
                case CardEffect.OrderPayout: return "Bulk Order payout " + factor;
                case CardEffect.OfflineHours: return "Offline storage " + UpgradeEffectCopy.Whole(ProgressionNumbers.CardFactor(card.effect, level)) + " hours";
                case CardEffect.OfflineEfficiency: return "Offline income efficiency " + UpgradeEffectCopy.Whole(ProgressionNumbers.CardFactor(card.effect, level) * 100) + "%";
                default: return "";
            }
        }
        public static string Applies(RedesignEngine engine, TalentCard card)
        {
            if (card.effect == CardEffect.ProductProfit) return "Applies when you own " + engine.Data.Products[card.target].name + " this run.";
            if (card.effect == CardEffect.ProductCost) return "Reduces " + engine.Data.Products[card.target].name + " level prices, including its first purchase.";
            if (card.effect == CardEffect.SkillCooldown || card.effect == CardEffect.SkillDuration || card.effect == CardEffect.OrderPayout)
                return "Buy " + new[] { "Festival Rush", "Helping Hands", "Bulk Order" }[card.target] + " in this run to use this bonus.";
            if (card.effect == CardEffect.OfflineEfficiency || card.effect == CardEffect.OfflineHours) return "Applies to normal product income while away; temporary skills are excluded.";
            return "Applies automatically when relevant. No equipment slots or merge cost.";
        }
    }
}
