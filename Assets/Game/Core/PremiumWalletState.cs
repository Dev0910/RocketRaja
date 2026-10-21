using System;
using System.Linq;

namespace RocketRaja.Core
{
    [Serializable] public sealed class GuestWalletProvenance
    {
        public string profileId, giftDrawCardId, giftDrawOperationId;
        public static GuestWalletProvenance Fresh() => new GuestWalletProvenance { profileId = Guid.NewGuid().ToString("N"), giftDrawCardId = "", giftDrawOperationId = "" };
        public GuestWalletProvenance Copy() => (GuestWalletProvenance)MemberwiseClone();
        public bool Valid(RedesignCatalog catalog) => PremiumWalletProtocol.OperationId(profileId) && giftDrawCardId != null && giftDrawOperationId != null &&
            (giftDrawCardId == "" ? giftDrawOperationId == "" : catalog.TryCard(giftDrawCardId, out _) && PremiumWalletProtocol.OperationId(giftDrawOperationId));
    }
    [Serializable] public sealed class PremiumWalletCache
    {
        public string uid, profileId, pendingOperationId, pendingExpectedRevision, lastAppliedOperationId;
        public Natural revision, deficit;
        public Natural[] currencyCards;
        public bool giftRegistered;
        public PremiumWalletCache Copy()
        {
            var copy = (PremiumWalletCache)MemberwiseClone(); copy.currencyCards = currencyCards == null ? null : (Natural[])currencyCards.Clone(); return copy;
        }
        public bool IsUnbound => string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(profileId) && string.IsNullOrEmpty(pendingOperationId) && string.IsNullOrEmpty(pendingExpectedRevision) && string.IsNullOrEmpty(lastAppliedOperationId) &&
            !giftRegistered && ZeroOrMissing(revision) && ZeroOrMissing(deficit) && (currencyCards == null || currencyCards.All(ZeroOrMissing));
        static bool ZeroOrMissing(Natural value) => string.IsNullOrEmpty(value.digits) || value.digits == "0";
        public bool Valid(RedesignState state) => !string.IsNullOrEmpty(uid) && uid.Length <= 128 && PremiumWalletProtocol.OperationId(profileId) &&
            state.guestWallet != null && profileId == state.guestWallet.profileId && revision.Valid && deficit.Valid && (deficit.IsZero || state.kajuKatli.IsZero) &&
            currencyCards != null && currencyCards.Length == 28 && currencyCards.All(x => x.Valid) &&
            Enumerable.Range(0, 28).All(i => state.cardLevels[i].CompareTo(currencyCards[i]) >= 0) &&
            pendingOperationId != null && pendingExpectedRevision != null && lastAppliedOperationId != null &&
            (pendingOperationId == "" ? pendingExpectedRevision == "" : PremiumWalletProtocol.OperationId(pendingOperationId) && new Natural { digits = pendingExpectedRevision }.Valid && new Natural { digits = pendingExpectedRevision }.CompareTo(revision) <= 0) &&
            (lastAppliedOperationId == "" || PremiumWalletProtocol.OperationId(lastAppliedOperationId)) && (!giftRegistered || state.secondPrestigeGift);
    }
    [Serializable] public sealed class PremiumWalletSnapshot
    {
        public string uid, profileId, revision, balance, deficit, importedGiftCardId, importedGiftOperationId;
        public bool giftRegistered;
        public string[] cards;
    }
    [Serializable] public sealed class CurrencyDrawOutcome
    {
        public string operationId, cardId, before, after, revision, price;
        public PremiumWalletSnapshot wallet;
    }
    public sealed class CurrencyDrawRequest
    {
        public string OperationId { get; internal set; }
        public string ExpectedRevision { get; internal set; }
        public string Uid { get; internal set; }
    }
    public static class PremiumWalletProtocol
    {
        public static bool OperationId(string value) => value != null && value.Length == 32 && value.All(c => c >= '0' && c <= '9' || c >= 'a' && c <= 'f');
        public static bool Valid(PremiumWalletSnapshot s, RedesignCatalog catalog)
        {
            bool Count(string v) => new Natural { digits = v }.Valid;
            return s != null && !string.IsNullOrEmpty(s.uid) && s.uid.Length <= 128 && OperationId(s.profileId) && Count(s.revision) && Count(s.balance) && Count(s.deficit) &&
                (s.deficit == "0" || s.balance == "0") && s.cards != null && s.cards.Length == 28 && s.cards.All(Count) &&
                s.importedGiftCardId != null && s.importedGiftOperationId != null &&
                (s.importedGiftCardId == "" ? s.importedGiftOperationId == "" : s.giftRegistered && catalog.TryCard(s.importedGiftCardId, out _) && OperationId(s.importedGiftOperationId));
        }
    }
}
