using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace RocketRaja.Core
{
    public sealed class PremiumIdentity
    {
        public string Uid { get; }
        public string SessionKey { get; }
        public PremiumIdentity(string uid, string sessionKey) { Uid = uid; SessionKey = sessionKey; }
        public bool Same(PremiumIdentity other) => other != null && Uid == other.Uid && SessionKey == other.SessionKey;
        public string ObfuscatedAccountId => SavePayload.Digest("rocket-raja:" + Uid).ToLowerInvariant();
    }
    public sealed class VerifiedPackGrant
    {
        public string uid, productId;
        public bool fulfilled, revoked;
        public Natural revision, granted;
    }
    /// <summary>Authenticated server adapter. A local save or receipt decoder cannot implement trusted fulfillment.</summary>
    public interface IPremiumFulfillmentClient
    {
        bool Configured { get; }
        Task<VerifiedPackGrant> VerifyAndGrantAsync(PremiumIdentity identity, string productId, string purchaseToken);
        // Replaces the server-owned wallet/card component and durably caches it; never adds a snapshot balance.
        Task ReconcileWalletAsync(PremiumIdentity identity);
    }
    public sealed class PremiumFulfillmentCoordinator
    {
        readonly RedesignCatalog catalog;
        readonly IPremiumFulfillmentClient authority;
        readonly Func<PremiumIdentity> identity;
        readonly Func<bool> alive;
        readonly HashSet<string> processing = new HashSet<string>(StringComparer.Ordinal);
        public bool IsBusy => processing.Count != 0;
        public PremiumFulfillmentCoordinator(RedesignCatalog catalog, IPremiumFulfillmentClient authority, Func<PremiumIdentity> identity, Func<bool> alive)
        { this.catalog = catalog; this.authority = authority; this.identity = identity; this.alive = alive; }
        public async Task<bool> FulfillAsync(PremiumIdentity owner, string sku, string token, Action acknowledge)
        {
            var pack = catalog.Packs.FirstOrDefault(p => p.id == sku);
            if (!alive() || authority?.Configured != true || owner == null || !owner.Same(identity()) || pack == null || string.IsNullOrEmpty(token) || !processing.Add(token)) return false;
            try
            {
                var grant = await authority.VerifyAndGrantAsync(owner, sku, token);
                if (grant == null || !grant.fulfilled || grant.uid != owner.Uid || grant.productId != sku || !grant.revision.Valid || !grant.granted.Valid || grant.granted.Value != pack.kajuKatli)
                    throw new InvalidOperationException("Unverified premium fulfillment");
                if (!alive() || !owner.Same(identity())) return false;
                await authority.ReconcileWalletAsync(owner);
                if (!alive() || !owner.Same(identity())) return false;
                acknowledge();
                return true;
            }
            finally { processing.Remove(token); }
        }
    }
}
