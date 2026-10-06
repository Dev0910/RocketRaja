using System;
using System.Globalization;
using RocketRaja.Core;

namespace RocketRaja
{
    /// <summary>Small per-command snapshot. Never runs per frame, writes a save, or owns rewards.</summary>
    public sealed class RedesignPurchaseFeedback
    {
        readonly Amount[] income = new Amount[8];
        readonly Amount normal;
        readonly int highestOwned, ownedCount, unique, shop;
        readonly string run;
        public RedesignPurchaseFeedback(RedesignEngine engine)
        {
            highestOwned = -1;
            for (int p = 0; p < 8; p++) { income[p] = engine.ProductIncome(p); if (engine.State.products[p] > 0) highestOwned = p; }
            normal = engine.NormalIncome(); ownedCount = engine.State.upgrades.Count; unique = engine.State.UniqueCount; shop = engine.ShopAppearance; run = engine.State.runId;
        }
        public string Complete(RedesignEngine engine)
        {
            int count = engine.State.upgrades.Count - ownedCount, first = engine.State.UniqueCount - unique;
            if (engine.State.runId != run || count <= 0 || first < 0 || first > count) throw new InvalidOperationException("Feedback requires a committed purchase in the captured run");
            string N(int n) => n.ToString("N0", CultureInfo.InvariantCulture);
            string message = count == 1 ? first == 1 ? "New cash upgrade purchased · +1 unique" : "Cash upgrade rebought · unique total unchanged" :
                "Bought " + N(count) + " upgrades · " + N(first) + " new · " + N(count - first) + " rebought";
            int comeback = -1; double bestGain = .005; var afterNormal = engine.NormalIncome();
            if (normal > Amount.Zero && afterNormal > Amount.Zero)
                for (int p = 0; p < highestOwned; p++)
                {
                    var after = engine.ProductIncome(p); if (income[p] <= Amount.Zero || after < income[p] * 2) continue;
                    double gain = Amount.Ratio(after, afterNormal) - Amount.Ratio(income[p], normal);
                    if (gain > bestGain) { bestGain = gain; comeback = p; }
                }
            if (comeback >= 0) message += "\n" + engine.Data.Products[comeback].name + " comeback: " + RedesignUiContext.Cash(income[comeback]) + " → " + RedesignUiContext.Cash(engine.ProductIncome(comeback)) + "/s";
            if (engine.ShopAppearance > shop) message += "\nShop " + (engine.ShopAppearance + 1) + "/6 unlocked · " + RedesignShopVisuals.Names[engine.ShopAppearance];
            return message;
        }
        public static string Skill(RedesignEngine engine, int target)
        {
            if (target == 2) return "Bulk Order paid from normal sales.\nRush does not multiply this payout.";
            string message = RedesignSkillScreen.Names[target] + " activated";
            if (engine.State.activeUntil[0] > engine.State.now && engine.State.activeUntil[1] > engine.State.now)
                message += "\nRush + Hands overlap: Hands taps earn ×1.25 while both run.";
            return message;
        }
    }
}
