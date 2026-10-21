using RocketRaja.Core;
using System.Text.Json;

static partial class Program
{
    static void PremiumWalletChecks(RedesignCatalog data)
    {
        const string uid = "wallet-fixture";
        RedesignState disk = null; bool fail = false;
        RedesignEngine Make(RedesignState s) => new(data, s, next => { if (fail) throw new IOException("cache unavailable"); disk = Clone(next); }, new SequenceRandom(0, 0));
        RedesignState Local()
        {
            var state = RedesignState.Fresh(1000); state.completedPrestiges = state.fame = 2; state.target = ProgressionNumbers.RunTarget(2);
            state.secondPrestigeGift = true; state.kajuKatli = 100; state.freeCredits = 3; return state;
        }
        PremiumWalletSnapshot Snapshot(RedesignState s, string revision = "1", string balance = "100") => new() {
            uid = uid, profileId = s.guestWallet.profileId, revision = revision, balance = balance, deficit = "0", cards = Enumerable.Repeat("0", 28).ToArray(), giftRegistered = true, importedGiftCardId = "", importedGiftOperationId = ""
        };
        var local = Local(); var e = Make(local); var gift = Snapshot(local);
        Check(e.ReconcilePremiumWallet(uid, gift) && e.State.schema == 3 && e.State.kajuKatli.Value == 100 && disk.premium.giftRegistered, "Authority activation upgrades the schema and replaces the gift wallet without doubling it");
        var revision = e.State.revision; Check(e.ReconcilePremiumWallet(uid, gift) && e.State.revision.CompareTo(revision) == 0, "Identical wallet refresh is idempotent without invalidating cash quotes");
        Check(e.QuoteDraw(false) == null, "Authoritative currency never uses the local random/debit path");
        var wrongOwner = Clone(gift); wrongOwner.uid = "other";
        Check(!e.ReconcilePremiumWallet(uid, wrongOwner) && e.State.kajuKatli.Value == 100, "Wrong-owner wallet response is rejected");
        var stale = Clone(gift); stale.revision = "0"; stale.balance = "10000";
        Check(!e.ReconcilePremiumWallet(uid, stale), "Stale wallet cannot restore an already-spent balance");
        var conflicting = Clone(gift); conflicting.balance = "999";
        Check(!e.ReconcilePremiumWallet(uid, conflicting), "Conflicting data at the same wallet revision is rejected");
        var request = e.BeginCurrencyDraw();
        Check(request != null && disk.premium.pendingOperationId == request.OperationId && e.State.kajuKatli.Value == 100, "Paid operation identity is durable before contacting the authority or debiting currency");
        Check(e.BeginCurrencyDraw().OperationId == request.OperationId && e.QuoteDraw(true) == null, "Retry reuses the operation while another reveal cannot overlap it");
        var result = new CurrencyDrawOutcome { operationId = request.OperationId, cardId = "C01", before = "0", after = "1", price = "100", revision = "2", wallet = Snapshot(local, "2", "0") };
        result.wallet.cards[0] = "1";
        fail = true; bool failed = false;
        try { e.ApplyCurrencyDraw(uid, result); } catch (IOException) { failed = true; }
        Check(failed && e.State.premium.pendingOperationId == request.OperationId && e.State.cardLevels[0].IsZero && !e.State.reveal.pending, "Cache failure preserves the pending paid operation and never reveals an unpersisted result");
        fail = false; e = Make(disk);
        Check(e.BeginCurrencyDraw().OperationId == request.OperationId && e.ApplyCurrencyDraw(uid, result) && e.State.kajuKatli.IsZero && e.State.cardLevels[0].Value == 1 && disk.reveal.pending, "Reload retries the same paid operation and persists result before revealing");
        e.DismissReveal(result.operationId);
        Check(!e.ApplyCurrencyDraw(uid, result) && e.State.cardLevels[0].Value == 1 && !e.State.reveal.pending, "Repeated paid response cannot add a copy or replay a dismissed reveal");
        Check(e.Draw(e.QuoteDraw(true)) && e.State.cardLevels[0].Value == 2 && e.State.premium.currencyCards[0].Value == 1, "A free-credit duplicate remains separate from the server-owned copy component");
        e.DismissReveal(e.State.reveal.operationId);
        var newer = Snapshot(local, "3", "1000"); newer.cards[0] = "1";
        Check(e.ReconcilePremiumWallet(uid, newer) && e.State.cardLevels[0].Value == 2 && e.State.kajuKatli.Value == 1000, "Pack wallet refresh preserves free copies and replaces balance rather than adding it");
        var next = e.BeginCurrencyDraw(); var nextResult = Clone(result); nextResult.operationId = next.OperationId; nextResult.before = "1"; nextResult.after = "2"; nextResult.revision = "4";
        nextResult.wallet = Snapshot(local, "4", "900"); nextResult.wallet.cards[0] = "2";
        var ahead = Clone(nextResult.wallet); ahead.revision = "5"; ahead.balance = "1900";
        Check(e.ReconcilePremiumWallet(uid, ahead) && e.State.cardLevels[0].Value == 3 && e.ApplyCurrencyDraw(uid, nextResult) && e.State.kajuKatli.Value == 1900 && e.State.cardLevels[0].Value == 3, "Late draw result can finish its reveal without rolling back a newer wallet");
        Check(e.State.reveal.before.Value == 2 && e.State.reveal.after.Value == 3, "Paid reveal shows total free-plus-currency level change");
        e.DismissReveal(next.OperationId); var rejected = e.BeginCurrencyDraw();
        Check(!e.ResolveRejectedCurrencyDraw(uid, "wrong") && e.ResolveRejectedCurrencyDraw(uid, rejected.OperationId), "Only the matching authoritatively rejected request may clear pending spending");
        var refund = Clone(ahead); refund.revision = "6"; refund.balance = "0"; refund.deficit = "100";
        Check(e.ReconcilePremiumWallet(uid, refund) && e.BeginCurrencyDraw() == null && e.State.cardLevels[0].Value == 3, "Refund deficit blocks paid spending while retaining drawn cards");
        var downCards = Clone(refund); downCards.revision = "7"; downCards.cards[0] = "0";
        Check(!e.ReconcilePremiumWallet(uid, downCards), "Wallet reconciliation cannot erase already-granted currency card copies");
        e = Make(Local()); Check(e.Draw(e.QuoteDraw(false)), "Guest may spend the one-time gift offline");
        string giftOperation = e.State.guestWallet.giftDrawOperationId;
        e.DismissReveal(e.State.reveal.operationId); var spentGuest = e.State.Copy();
        var spentSnapshot = Snapshot(spentGuest, "1", "0"); spentSnapshot.cards[0] = "1"; spentSnapshot.importedGiftCardId = "C01"; spentSnapshot.importedGiftOperationId = giftOperation;
        Check(e.ReconcilePremiumWallet(uid, spentSnapshot) && e.State.cardLevels[0].Value == 1 && e.State.premium.currencyCards[0].Value == 1 && e.State.kajuKatli.IsZero, "Promoting a spent guest gift imports its original result without granting another copy or 100 KK");
        var first = RedesignState.Fresh(1000); first.completedPrestiges = first.fame = 1; first.target = ProgressionNumbers.RunTarget(1);
        var beforeGift = Snapshot(first, "1", "0"); beforeGift.giftRegistered = false;
        e = Make(first); e.ReconcilePremiumWallet(uid, beforeGift);
        var atSecond = e.State.Copy(); atSecond.eligibleEarnings = atSecond.target; e = Make(atSecond);
        Check(e.Prestige(e.PreviewPrestige()) && e.State.secondPrestigeGift && e.State.kajuKatli.IsZero && !e.State.premium.giftRegistered, "An account's second prestige records gift eligibility without locally duplicating the server grant");
        var delivered = Snapshot(e.State, "2", "100");
        Check(e.ReconcilePremiumWallet(uid, delivered) && e.State.kajuKatli.Value == 100 && e.State.premium.giftRegistered, "Authoritative second-prestige grant settles once after cloud progress is confirmed");
        var old = Local(); old.guestWallet = null; old.kajuKatli = 0;
        e = Make(old); Check(!e.PreparePremiumProfile(), "Ambiguous older development gift provenance cannot create a replacement card or grant");
        old.kajuKatli = 100; e = Make(old); Check(e.PreparePremiumProfile() && e.State.guestWallet.Valid(data), "Unspent old-development gift can acquire durable promotion provenance");
        var directory = Path.Combine(Path.GetTempPath(), "PremiumSchema-" + Guid.NewGuid().ToString("N"));
        var store = new RedesignSaveStore(Path.Combine(directory, "wallet.save"), data, s => JsonSerializer.Serialize(s, Json), s => JsonSerializer.Deserialize<RedesignState>(s, Json), s => JsonSerializer.Deserialize<SaveVersionHeader>(s, Json));
        e.ReconcilePremiumWallet(uid, Snapshot(e.State)); store.Save(e.State);
        Check(store.Load().state.schema == 3 && store.Load().state.premium.uid == uid, "Authoritative schema-3 wallet survives checksummed roundtrip");
        var downgrade = Local(); downgrade.revision = e.State.revision + (Natural)100;
        Reject(() => store.Save(downgrade), "A local schema-2 save cannot overwrite an authoritative schema-3 wallet");
        var malformed = e.State.Copy(); malformed.premium = null;
        Check(!malformed.Valid(data), "Schema 3 rejects missing authority state rather than treating it as a local wallet");
    }
}
