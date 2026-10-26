using RocketRaja.Core;

static partial class Program
{
    static void ShopOfferChecks(RedesignCatalog catalog)
    {
        var state = RedesignState.Fresh(1000); state.products[0] = 10; state.kajuKatli = 500;
        RedesignState disk = null; bool fail = false;
        var e = new RedesignEngine(catalog, state, s => { if (fail) throw new IOException("Shop save failed"); disk = Clone(s); });
        var rate = e.NormalIncome(); var tap = e.TapPayout(); var reward = rate * 86400;
        Check(Near(e.ShopDayCash, reward) && e.CanBuyShopCash, "Shop cash preview is 24 hours of automatic income");
        fail = true;
        bool threw = false; try { e.BuyShopCash(); } catch (IOException) { threw = true; }
        Check(threw && e.State.kajuKatli.Value == 500 && Near(e.State.cash, Amount.Zero), "Failed shop save spends and grants nothing");
        fail = false;
        Check(e.BuyShopCash() && e.State.kajuKatli.Value == 400 && Near(e.State.cash, reward) && Near(disk.cash, reward), "100 KK atomically buys the displayed day of cash");
        fail = true; threw = false; try { e.BuyShopProfit(); } catch (IOException) { threw = true; }
        Check(threw && !e.ShopProfitActive && e.State.kajuKatli.Value == 400, "Failed boost save leaves currency and timer unchanged");
        fail = false;
        Check(e.BuyShopProfit() && e.State.kajuKatli.Value == 250 && e.State.shopProfitUntil == 87400, "150 KK starts a saved 24-hour profit timer");
        Check(Near(e.NormalIncome(), rate * 2) && Near(e.TapPayout(), tap * 2) && Near(e.ShopDayCash, reward), "Shop boost doubles income and taps without inflating the instant cash offer");
        Check(!e.BuyShopProfit() && e.State.kajuKatli.Value == 250, "Active boost cannot be repurchased or charged again");
        e = new RedesignEngine(catalog, Clone(disk), s => disk = Clone(s));
        Check(e.ShopProfitActive && Near(e.NormalIncome(), rate * 2), "Active shop boost survives save reload");
        var before = e.State.cash;
        e.Advance(86410);
        Check(!e.ShopProfitActive && Near(e.State.cash - before, rate * (86400 * 2 + 10)) && Near(e.TapPayout(), tap), "Foreground accrual splits exactly at boost expiry");
        Check(e.BuyShopProfit() && e.State.kajuKatli.Value == 100, "Expired boost can be purchased again at 150 KK");
        e.State.now = e.State.shopProfitUntil - 10; e.Save(); before = e.State.cash;
        e.ResolveOffline(e.State.now + 100);
        Check(Near(e.State.cash - before, rate * .5 * 110), "Offline payout doubles only seconds before boost expiry");
        Check(!e.CanBuyShopProfit && e.CanBuyShopCash, "Shop offers respect their different affordability thresholds");
        e.PremiumSpendAllowed = () => false;
        Check(!e.BuyShopCash(), "Shop respects the premium spending gate");
        e.PremiumSpendAllowed = () => true; e.MutationAllowed = () => false;
        Check(!e.BuyShopCash(), "Blocked session cannot buy shop rewards");
        var empty = RedesignState.Fresh(1000); empty.kajuKatli = 100;
        var zero = new RedesignEngine(catalog, empty, _ => {});
        Check(!zero.BuyShopCash() && zero.State.kajuKatli.Value == 100, "Zero-income cash offer does not consume currency");
        var invalid = state.Copy(); invalid.shopProfitUntil = double.NaN;
        Check(!invalid.Valid(catalog), "Invalid boost timestamps are rejected");
        var gift = RedesignState.Fresh(1000); gift.products[0] = 1; gift.completedPrestiges = 2; gift.fame = 2;
        gift.target = ProgressionNumbers.RunTarget(2, 2); gift.secondPrestigeGift = true; gift.kajuKatli = 100;
        var guest = new RedesignEngine(catalog, gift, _ => {});
        Check(guest.BuyShopCash() && guest.State.shopGiftSpent && guest.State.kajuKatli.IsZero && guest.State.Valid(catalog), "Guest shop spend preserves spent-gift provenance for wallet activation");
        guest.State.kajuKatli = 100;
        Check(guest.Draw(guest.QuoteDraw(false)) && guest.State.Valid(catalog) && guest.State.guestWallet.giftDrawCardId == "", "Later earned-KK draw does not reuse the gift already spent in the shop");
        var prestige = RedesignState.Fresh(1000); prestige.products[0] = 1; prestige.kajuKatli = 150; prestige.eligibleEarnings = prestige.target;
        var rebirth = new RedesignEngine(catalog, prestige, _ => {}); rebirth.BuyShopProfit(); double expiry = rebirth.State.shopProfitUntil;
        Check(rebirth.Prestige(rebirth.PreviewPrestige()) && rebirth.State.shopProfitUntil == expiry, "Prestige preserves the purchased boost and its original expiry");
    }
}
