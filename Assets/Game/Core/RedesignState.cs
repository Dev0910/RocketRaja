using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketRaja.Core
{
    [Serializable] public sealed class ClassicDrawResult
    {
        public string cardId;
        public Natural before, after;
        public ClassicDrawResult Copy() => (ClassicDrawResult)MemberwiseClone();
    }
    [Serializable] public sealed class DrawReveal
    {
        public string operationId, cardId, payment;
        public Natural before, after;
        public bool pending;
        // Optional persisted bundle; old single-card saves remain valid.
        public ClassicDrawResult[] batch;
        public int batchIndex;
        public DrawReveal Copy() { var copy = (DrawReveal)MemberwiseClone(); copy.batch = batch?.Select(x => x?.Copy()).ToArray(); return copy; }
        public static DrawReveal Empty() => new DrawReveal { operationId = "", cardId = "", payment = "", before = 0, after = 0 };
    }
    [Serializable] public sealed class OfflineReceipt
    {
        public string id;
        public double absence, creditedSeconds, efficiency;
        public Amount amount;
        public bool pending;
        public OfflineReceipt Copy() => (OfflineReceipt)MemberwiseClone();
        public static OfflineReceipt Empty() => new OfflineReceipt { id = "", efficiency = .5 };
    }
    [Serializable] public sealed class RedesignState
    {
        // No version/counter/collection initializers: missing mandatory fields must fail validation.
        public int schema;
        public string economy, contentVersion, runId;
        // Optional provenance for explicitly identified disposable-fixture conversion only.
        public string migrationSourceDigest;
        public bool developmentFixture;
        public Natural revision, commandRevision, fame, completedPrestiges, kajuKatli, freeCredits, manualTaps;
        public Natural[] cardLevels;
        public List<string> everPurchased, upgrades, learnedTopics;
        public bool secondPrestigeGift, music, sound, reducedMotion, storeNamePromptHandled;
        public bool requiresDrawReconciliation;
        // Schema 2 is local-only. Enabling an authoritative wallet upgrades to schema 3,
        // so older clients cannot interpret server currency as spendable local currency.
        public GuestWalletProvenance guestWallet;
        public PremiumWalletCache premium;
        public DevelopmentShopState developmentShop;
        public string storeName, pinnedUpgradeId;
        public int decorationStage, tapLevel;
        public int[] products;
        public Amount cash, eligibleEarnings, lifetimeEarnings, target, departureRate;
        public double now, savedAt, runSeconds, nextManualTapAt;
        public double[] activeUntil, readyAt, nextHandsAt;
        public DrawReveal reveal;
        public OfflineReceipt offline;
        public int UniqueCount => everPurchased.Count;
        public bool HasPremium => schema == 3;
        public bool HasDevelopmentShop => developmentShop != null && !developmentShop.IsEmpty;
        public bool HasGuestProvenance => guestWallet != null && !string.IsNullOrEmpty(guestWallet.profileId);
        public static RedesignState Fresh(double now = 0)
        {
            var state = new RedesignState {
                schema = 2, economy = RedesignCatalog.EconomyVersion, contentVersion = RedesignCatalog.ContentVersion,
                runId = Guid.NewGuid().ToString("N"), revision = 0, commandRevision = 0, fame = 0, completedPrestiges = 0, kajuKatli = 0, freeCredits = 0, manualTaps = 0,
                cardLevels = Enumerable.Range(0, 28).Select(_ => Natural.Zero).ToArray(),
                everPurchased = new List<string>(), upgrades = new List<string>(), learnedTopics = new List<string>(),
                music = true, sound = true, storeName = "", pinnedUpgradeId = "", products = new int[8], cash = 30,
                target = new Amount(1, 10), now = now, savedAt = now, activeUntil = new double[3], readyAt = new double[3], nextHandsAt = new double[3],
                reveal = DrawReveal.Empty(), offline = OfflineReceipt.Empty(), guestWallet = GuestWalletProvenance.Fresh()
            };
            return state;
        }
        public RedesignState Copy()
        {
            var s = (RedesignState)MemberwiseClone();
            s.products = (int[])products.Clone(); s.cardLevels = (Natural[])cardLevels.Clone();
            s.everPurchased = new List<string>(everPurchased); s.upgrades = new List<string>(upgrades); s.learnedTopics = new List<string>(learnedTopics);
            s.activeUntil = (double[])activeUntil.Clone(); s.readyAt = (double[])readyAt.Clone(); s.nextHandsAt = (double[])nextHandsAt.Clone();
            s.reveal = reveal.Copy(); s.offline = offline.Copy();
            s.guestWallet = guestWallet?.Copy(); s.premium = premium?.Copy();
            s.developmentShop = developmentShop?.Copy();
            return s;
        }
        public bool Valid(RedesignCatalog data)
        {
            bool Time(double v) => !double.IsNaN(v) && !double.IsInfinity(v) && v >= 0 && v < 1e12;
            bool Times(double[] values) => values != null && values.Length == 3 && values.All(Time);
            bool Ids(List<string> ids) => ids != null && ids.Count <= 1000 && ids.Distinct(StringComparer.Ordinal).Count() == ids.Count && ids.All(id => data.TryUpgrade(id, out _));
            if (schema != 2 && schema != 3 || economy != RedesignCatalog.EconomyVersion || contentVersion != RedesignCatalog.ContentVersion || string.IsNullOrEmpty(runId) || runId.Length > 64 ||
                !revision.Valid || !commandRevision.Valid || commandRevision.CompareTo(revision) > 0 || !fame.Valid || !completedPrestiges.Valid || !kajuKatli.Valid || !freeCredits.Valid || !manualTaps.Valid ||
                cardLevels == null || cardLevels.Length != 28 || !cardLevels.All(x => x.Valid) ||
                !Ids(everPurchased) || !Ids(upgrades) || !new HashSet<string>(everPurchased, StringComparer.Ordinal).IsSupersetOf(upgrades) ||
                products == null || products.Length != 8 || products.Any(n => n < 0 || n > Catalog.MaxLevel) || tapLevel < 0 || tapLevel > Catalog.MaxLevel ||
                !cash.Valid || !eligibleEarnings.Valid || !lifetimeEarnings.Valid || !target.Valid || target.mantissa == 0 || !departureRate.Valid ||
                !Time(now) || !Time(savedAt) || !Time(runSeconds) || !Time(nextManualTapAt) || !Times(activeUntil) || !Times(readyAt) || !Times(nextHandsAt) ||
                learnedTopics == null || learnedTopics.Count > 256 || learnedTopics.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > 80) || learnedTopics.Distinct().Count() != learnedTopics.Count ||
                storeName == null || storeName.Length > 128 || pinnedUpgradeId == null || pinnedUpgradeId != "" && !data.TryUpgrade(pinnedUpgradeId, out _) ||
                decorationStage < 0 || decorationStage > 5 || reveal == null || offline == null) return false;
            if (!string.IsNullOrEmpty(migrationSourceDigest) && (migrationSourceDigest.Length != 64 || !migrationSourceDigest.All(Uri.IsHexDigit))) return false;
            if (HasDevelopmentShop && (HasPremium || !developmentShop.Valid(data))) return false;
            if (guestWallet != null && (HasGuestProvenance || !string.IsNullOrEmpty(guestWallet.giftDrawCardId) || !string.IsNullOrEmpty(guestWallet.giftDrawOperationId)) &&
                (!guestWallet.Valid(data) || guestWallet.giftDrawCardId != "" && (!secondPrestigeGift || !data.TryCard(guestWallet.giftDrawCardId, out int giftCard) || cardLevels[giftCard].IsZero))) return false;
            // JsonUtility expands null inline classes into empty objects. Schema, rather than
            // object nullness, is the authority boundary; an unbound local object contains no data.
            if (schema == 2 && premium != null && !premium.IsUnbound || schema == 3 && (premium == null || !premium.Valid(this))) return false;
            if (fame.CompareTo(completedPrestiges) < 0 || completedPrestiges.IsZero && (!fame.IsZero || !freeCredits.IsZero || cardLevels.Any(x => !x.IsZero)) ||
                secondPrestigeGift != (completedPrestiges.Value >= 2) || Math.Abs(Amount.Ratio(target, ProgressionNumbers.RunTarget(completedPrestiges)) - 1) > 1e-12) return false;
            if (!reveal.before.Valid || !reveal.after.Valid || reveal.operationId == null || reveal.cardId == null || reveal.payment == null) return false;
            if (reveal.operationId != "" && (!data.TryCard(reveal.cardId, out int ci) || reveal.after.Value != reveal.before.Value + 1 || cardLevels[ci].CompareTo(reveal.after) < 0 || reveal.payment != "credit" && reveal.payment != "kk")) return false;
            if (reveal.pending && reveal.operationId == "") return false;
            if (reveal.batch != null && reveal.batch.Length > 0)
            {
                if (reveal.batch.Length != 5 || reveal.batchIndex < 0 || reveal.batchIndex >= 5 || reveal.operationId == "") return false;
                foreach (var result in reveal.batch)
                    if (result == null || !result.before.Valid || !result.after.Valid || !data.TryCard(result.cardId, out int index) ||
                        result.after.Value != result.before.Value + 1 || cardLevels[index].CompareTo(result.after) < 0) return false;
                var current = reveal.batch[reveal.batchIndex];
                if (reveal.cardId != current.cardId || reveal.before.Value != current.before.Value || reveal.after.Value != current.after.Value) return false;
                foreach (var group in reveal.batch.GroupBy(x => x.cardId))
                {
                    Natural? previous = null;
                    foreach (var result in group) { if (previous.HasValue && previous.Value.Value != result.before.Value) return false; previous = result.after; }
                }
            }
            else if (reveal.batchIndex != 0) return false;
            if (offline.id == null || !Time(offline.absence) || !Time(offline.creditedSeconds) || offline.creditedSeconds > offline.absence || !offline.amount.Valid || double.IsNaN(offline.efficiency) || offline.efficiency < .5 || offline.efficiency > 1 || offline.pending && offline.id == "") return false;
            return true;
        }
    }
}
