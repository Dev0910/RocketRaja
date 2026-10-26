using System;

namespace RocketRaja.Classic
{
    public sealed partial class ClassicUI
    {
        void RefreshShopOffers()
        {
            var e = Engine;
            bool can = driver.Session.CanMutate;
            Text("shop-day-cash", Cash(e.ShopDayCash) + " instantly\nBased on automatic income");
            var remaining = TimeSpan.FromSeconds(Math.Ceiling(Math.Max(0, e.State.shopProfitUntil - e.State.now)));
            Text("shop-profit-detail", e.ShopProfitActive
                ? "2× profits active · " + ((int)remaining.TotalHours).ToString("00") + remaining.ToString(@"\:mm\:ss") + " left"
                : "2× product and tap earnings\nFor the next 24 hours");
            Text("shop-cash-cost", "100 KK\nBuy cash");
            Text("shop-profit-cost", e.ShopProfitActive ? "Active" : "150 KK\nBuy boost");
            Enable("BuyShopCash", "", can && e.CanBuyShopCash);
            Enable("BuyShopProfit", "", can && e.CanBuyShopProfit);
        }
    }
}
