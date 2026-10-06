using RocketRaja.Core;

static partial class Program
{
    static void DevelopmentShopChecks(RedesignCatalog data)
    {
        RedesignState saved = null;
        bool failSave = false;
        var e = new RedesignEngine(data, RedesignState.Fresh(1000), s => {
            if (failSave) throw new IOException("Simulated development save failure");
            saved = Clone(s);
        });
        Check(!e.EnableDevelopmentShop("production") && e.BeginDevelopmentPurchase(data.Packs[0].id) == null, "Test shop requires the explicit disposable namespace before granting");
        Check(e.EnableDevelopmentShop(DevelopmentShopState.Namespace) && saved.HasDevelopmentShop && saved.Valid(data), "Test shop identity is durably marked and valid only in the development profile");
        Check(e.BeginDevelopmentPurchase("unknown") == null && e.State.developmentShop.status == "ready", "Unknown test packs do not start an operation");
        Natural total = 0;
        string previous = "";
        foreach (var pack in data.Packs)
        {
            string id = e.BeginDevelopmentPurchase(pack.id);
            Check(id != null && saved.developmentShop.status == "pending" && saved.kajuKatli.CompareTo(total) == 0, pack.id + " pending state persists before any simulated delivery");
            Check(e.BeginDevelopmentPurchase(pack.id) == id && e.BeginDevelopmentPurchase(data.Packs.First(p => p.id != pack.id).id) == null, pack.id + " duplicate taps reuse the pending operation and other packs wait");
            e = new RedesignEngine(data, Clone(saved), s => saved = Clone(s));
            Check(e.BeginDevelopmentPurchase(pack.id) == id, pack.id + " relaunch preserves the same pending operation");
            if (previous != "") Check(!e.ResolveDevelopmentPurchase(previous, DevelopmentPurchaseOutcome.Delivered), "Late test callback cannot complete a newer purchase");
            total += (Natural)pack.kajuKatli;
            Check(e.ResolveDevelopmentPurchase(id, DevelopmentPurchaseOutcome.Delivered) && saved.kajuKatli.CompareTo(total) == 0 && saved.developmentShop.status == "delivered", pack.id + " delivers the authored amount atomically");
            Check(!e.ResolveDevelopmentPurchase(id, DevelopmentPurchaseOutcome.Delivered) && e.State.kajuKatli.CompareTo(total) == 0, pack.id + " duplicate callback cannot grant twice");
            previous = id;
        }
        foreach (var outcome in new[] { DevelopmentPurchaseOutcome.Cancelled, DevelopmentPurchaseOutcome.Failed })
        {
            string id = e.BeginDevelopmentPurchase(data.Packs[0].id);
            Check(e.ResolveDevelopmentPurchase(id, outcome) && e.State.kajuKatli.CompareTo(total) == 0 && !e.ResolveDevelopmentPurchase(id, DevelopmentPurchaseOutcome.Delivered), "Test " + outcome + " preserves currency and rejects a late delivery");
        }
        e = new RedesignEngine(data, Clone(saved), s => { if (failSave) throw new IOException("test"); saved = Clone(s); });
        failSave = true;
        bool failed = false;
        try { e.BeginDevelopmentPurchase(data.Packs[0].id); } catch (IOException) { failed = true; }
        Check(failed && e.State.developmentShop.status == "failed", "Failed pending-save cannot publish a test purchase");
        failSave = false;
        string pending = e.BeginDevelopmentPurchase(data.Packs[0].id);
        failSave = true; failed = false;
        try { e.ResolveDevelopmentPurchase(pending, DevelopmentPurchaseOutcome.Delivered); } catch (IOException) { failed = true; }
        Check(failed && e.State.kajuKatli.CompareTo(total) == 0 && e.State.developmentShop.status == "pending", "Failed delivery-save leaves the exact test operation pending without a grant");
        failSave = false;
        e.MutationAllowed = () => false;
        Check(!e.ResolveDevelopmentPurchase(pending, DevelopmentPurchaseOutcome.Delivered) && !e.EnableDevelopmentShop(DevelopmentShopState.Namespace), "Session mutation gate also blocks simulated delivery");
        e.MutationAllowed = () => true;
        Check(e.ResolveDevelopmentPurchase(pending, DevelopmentPurchaseOutcome.Delivered), "Retry saves the previously failed test delivery once");
        Check(!e.PreparePremiumProfile(), "Test currency cannot prepare for live premium linking");
        var wallet = new PremiumWalletSnapshot { uid = "live-owner", profileId = e.State.guestWallet.profileId, revision = "1", balance = "1000", deficit = "0", cards = Enumerable.Repeat("0", 28).ToArray(), importedGiftCardId = "", importedGiftOperationId = "" };
        Check(!e.ReconcilePremiumWallet(wallet.uid, wallet) && !e.State.HasPremium, "Live wallet rejects a profile containing simulated purchases");
        var reset = e.State.Copy(); reset.eligibleEarnings = reset.target;
        e = new RedesignEngine(data, reset, s => saved = Clone(s));
        Check(e.Prestige(e.PreviewPrestige()) && e.State.HasDevelopmentShop, "Test balance and purchase marker survive first prestige");
        Natural balance = e.State.kajuKatli;
        Check(e.Draw(e.QuoteDraw(false)) && e.State.kajuKatli.Value == balance.Value - 100 && e.State.guestWallet.giftDrawCardId == "", "Test KK buys the real card-pool draw without fabricating gift provenance");
        string card = e.State.reveal.cardId, drawId = e.State.reveal.operationId;
        e = new RedesignEngine(data, Clone(saved), s => saved = Clone(s));
        Check(e.State.reveal.pending && e.State.reveal.cardId == card && e.DismissReveal(drawId) && !e.DismissReveal(drawId), "Test-funded card and skippable reveal survive reload exactly once");
        var bad = e.State.Copy(); bad.developmentShop.profileNamespace = "production";
        Check(!bad.Valid(data), "Malformed test marker is rejected rather than normalized into a real wallet");
    }
}
