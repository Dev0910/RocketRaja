using System;
using System.Linq;

namespace RocketRaja.Core
{
    public sealed partial class RedesignEngine
    {
        public bool PreparePremiumProfile()
        {
            if (State.HasDevelopmentShop) return false;
            if (State.HasGuestProvenance) return true;
            // Compatibility for older disposable schema-2 saves: a spent gift needs an
            // identifiable stored result. Never invent a card or reissue an ambiguous gift.
            bool spent = State.secondPrestigeGift && State.kajuKatli.IsZero;
            if (spent && (State.reveal.payment != "kk" || !PremiumWalletProtocol.OperationId(State.reveal.operationId))) return false;
            return Commit(s => {
                s.guestWallet = GuestWalletProvenance.Fresh();
                if (spent) { s.guestWallet.giftDrawCardId = s.reveal.cardId; s.guestWallet.giftDrawOperationId = s.reveal.operationId; }
                return true;
            });
        }
        bool ApplyWallet(RedesignState state, PremiumWalletSnapshot snapshot)
        {
            if (state.HasDevelopmentShop) return false;
            if (!PremiumWalletProtocol.Valid(snapshot, Data) || !state.HasGuestProvenance || snapshot.profileId != state.guestWallet.profileId ||
                state.requiresDrawReconciliation || snapshot.giftRegistered && !state.secondPrestigeGift) return false;
            var revision = new Natural { digits = snapshot.revision };
            var incoming = snapshot.cards.Select(x => new Natural { digits = x }).ToArray();
            var previous = state.HasPremium ? state.premium : null;
            Natural[] applied;
            if (previous != null)
            {
                if (snapshot.uid != previous.uid || revision.CompareTo(previous.revision) < 0 || previous.giftRegistered && !snapshot.giftRegistered) return false;
                applied = previous.currencyCards;
                if (revision.CompareTo(previous.revision) == 0)
                    return state.kajuKatli.digits == snapshot.balance && previous.deficit.digits == snapshot.deficit && previous.giftRegistered == snapshot.giftRegistered &&
                        incoming.Select(x => x.digits).SequenceEqual(applied.Select(x => x.digits));
            }
            else
            {
                if (state.secondPrestigeGift && !snapshot.giftRegistered) return false;
                applied = Enumerable.Repeat(Natural.Zero, 28).ToArray();
                if (state.guestWallet.giftDrawCardId != "")
                {
                    if (snapshot.importedGiftCardId != state.guestWallet.giftDrawCardId || snapshot.importedGiftOperationId != state.guestWallet.giftDrawOperationId) return false;
                    Data.TryCard(snapshot.importedGiftCardId, out int imported); applied[imported] = 1;
                }
                else if (snapshot.importedGiftCardId != "") return false;
            }
            for (int i = 0; i < 28; i++)
            {
                if (incoming[i].CompareTo(applied[i]) < 0 || state.cardLevels[i].CompareTo(applied[i]) < 0) return false;
                state.cardLevels[i] = state.cardLevels[i] - applied[i] + incoming[i];
            }
            state.kajuKatli = new Natural { digits = snapshot.balance };
            state.premium = new PremiumWalletCache {
                uid = snapshot.uid, profileId = snapshot.profileId, revision = revision, deficit = new Natural { digits = snapshot.deficit }, currencyCards = incoming,
                giftRegistered = snapshot.giftRegistered, pendingOperationId = previous?.pendingOperationId ?? "", pendingExpectedRevision = previous?.pendingExpectedRevision ?? "", lastAppliedOperationId = previous?.lastAppliedOperationId ?? ""
            };
            state.schema = 3;
            return true;
        }
        public bool ReconcilePremiumWallet(string uid, PremiumWalletSnapshot snapshot)
        {
            if (snapshot == null || snapshot.uid != uid || string.IsNullOrEmpty(uid)) return false;
            // Unchanged reconciliation is read-only and must not invalidate an unrelated cash quote.
            if (State.HasPremium && snapshot.revision == State.premium.revision.digits)
                return MutationAllowed() && ApplyWallet(State.Copy(), snapshot);
            return Commit(s => ApplyWallet(s, snapshot));
        }
        public CurrencyDrawRequest BeginCurrencyDraw()
        {
            var p = State.premium;
            if (!State.HasPremium || !MutationAllowed() || State.requiresDrawReconciliation || State.completedPrestiges.IsZero || State.reveal.pending || !PremiumSpendAllowed()) return null;
            if (p.pendingOperationId == "")
            {
                if (State.kajuKatli.Value < 100 || !p.deficit.IsZero) return null;
                string operation = Guid.NewGuid().ToString("N");
                if (!Commit(s => { s.premium.pendingOperationId = operation; s.premium.pendingExpectedRevision = s.premium.revision.digits; return true; })) return null;
            }
            return new CurrencyDrawRequest { OperationId = State.premium.pendingOperationId, ExpectedRevision = State.premium.pendingExpectedRevision, Uid = State.premium.uid };
        }
        public bool ApplyCurrencyDraw(string uid, CurrencyDrawOutcome result)
        {
            if (result == null || !State.HasPremium || State.premium.uid != uid || State.premium.pendingOperationId != result.operationId || State.reveal.pending ||
                !PremiumWalletProtocol.OperationId(result.operationId) || !Data.TryCard(result.cardId, out int card) || !PremiumWalletProtocol.Valid(result.wallet, Data) ||
                result.wallet.uid != uid || result.revision != result.wallet.revision || result.price != "100") return false;
            var before = new Natural { digits = result.before }; var after = new Natural { digits = result.after };
            if (!before.Valid || !after.Valid || after.Value != before.Value + 1 || result.wallet.cards[card] != result.after) return false;
            return Commit(s => {
                var receivedRevision = new Natural { digits = result.revision };
                if (receivedRevision.CompareTo(s.premium.revision) >= 0)
                { if (!ApplyWallet(s, result.wallet)) return false; }
                else if (s.premium.currencyCards[card].CompareTo(after) < 0) return false;
                Natural freeCopies = s.cardLevels[card] - s.premium.currencyCards[card];
                s.reveal = new DrawReveal { operationId = result.operationId, cardId = result.cardId, payment = "kk", before = freeCopies + before, after = freeCopies + after, pending = true };
                s.premium.pendingOperationId = ""; s.premium.pendingExpectedRevision = ""; s.premium.lastAppliedOperationId = result.operationId;
                return true;
            });
        }
        // Only call after an authoritative stale-wallet/insufficient-currency response.
        // The server checked for an already-committed operation before returning that rejection.
        public bool ResolveRejectedCurrencyDraw(string uid, string operationId) => State.HasPremium && State.premium.uid == uid && State.premium.pendingOperationId == operationId &&
            Commit(s => { s.premium.pendingOperationId = ""; s.premium.pendingExpectedRevision = ""; return true; });
    }
}
