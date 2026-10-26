using System;

namespace RocketRaja.Core
{
    public sealed partial class RedesignEngine
    {
        public const int ShopCashCost = 100, ShopProfitCost = 150;
        public const double ShopDaySeconds = 86400;
        static double ShopProfitMultiplier(RedesignState state) => state.shopProfitUntil > state.now ? 2 : 1;
        public bool ShopProfitActive => State.shopProfitUntil > State.now;
        // A stable day of automatic income, excluding temporary Rush and shop boosts.
        public Amount ShopDayCash => NormalIncome() / ShopProfitMultiplier(State) * ShopDaySeconds;
        bool CanSpendInShop(int cost) => MutationAllowed() && PremiumSpendAllowed() && !State.HasPremium &&
            !State.requiresDrawReconciliation && !State.reveal.pending && State.kajuKatli.Value >= cost;
        public bool CanBuyShopCash => CanSpendInShop(ShopCashCost) && ShopDayCash > Amount.Zero;
        public bool CanBuyShopProfit => CanSpendInShop(ShopProfitCost) && !ShopProfitActive && State.now + ShopDaySeconds < 1e12;
        static void SpendInShop(RedesignState state, int cost)
        {
            state.kajuKatli -= (Natural)cost;
            if (state.secondPrestigeGift && state.HasGuestProvenance && !state.HasDevelopmentShop && state.guestWallet.giftDrawCardId == "")
                state.shopGiftSpent = true;
        }
        public bool BuyShopCash()
        {
            if (!CanBuyShopCash) return false;
            var reward = ShopDayCash;
            return Commit(s => { SpendInShop(s, ShopCashCost); Earn(s, reward); return true; });
        }
        public bool BuyShopProfit()
        {
            if (!CanBuyShopProfit) return false;
            return Commit(s => { SpendInShop(s, ShopProfitCost); s.shopProfitUntil = s.now + ShopDaySeconds; return true; });
        }
    }
}
