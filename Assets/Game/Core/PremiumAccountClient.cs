using System;
using System.Threading.Tasks;

namespace RocketRaja.Core
{
    public sealed class PremiumServiceException : Exception
    {
        public string Code { get; }
        public PremiumServiceException(string code) : base(code) { Code = code; }
    }
    public sealed class PremiumSessionFence
    {
        public PremiumIdentity Identity { get; }
        public string Owner { get; }
        public long Generation { get; }
        public PremiumSessionFence(PremiumIdentity identity, string owner, long generation)
        { Identity = identity; Owner = owner; Generation = generation; }
    }
    /// <summary>HTTPS adapter with verified Firebase ID and App Check tokens. No local grant fallback.</summary>
    public interface IPremiumAuthorityTransport
    {
        bool Configured { get; }
        Task<PremiumWalletSnapshot> ActivateAsync(PremiumSessionFence session);
        Task<PremiumWalletSnapshot> WalletAsync(PremiumIdentity identity);
        Task<CurrencyDrawOutcome> DrawAsync(PremiumSessionFence session, CurrencyDrawRequest request);
        Task<VerifiedPackGrant> FulfillAsync(PremiumIdentity identity, string productId, string purchaseToken);
    }
    /// <summary>One instance per bound account engine, used on its owning Unity thread.</summary>
    public sealed class PremiumAccountClient : IPremiumFulfillmentClient, IDisposable
    {
        readonly RedesignAccountSession account;
        readonly RedesignEngine engine;
        readonly IPremiumAuthorityTransport transport;
        readonly PremiumIdentity bound;
        bool disposed, drawing;
        public PremiumIdentity Identity => Current ? bound : null;
        bool Current => !disposed && account.CanPlay && engine.MutationAllowed() && engine.SessionKey == bound.SessionKey;
        public bool Configured => transport.Configured && Current && engine.State.HasPremium && engine.State.premium.uid == bound.Uid;
        public bool IsDrawing => drawing;
        public PremiumAccountClient(RedesignAccountSession account, RedesignEngine engine, IPremiumAuthorityTransport transport)
        {
            this.account = account ?? throw new ArgumentNullException(nameof(account));
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            bound = new PremiumIdentity(account.Uid, engine.SessionKey);
        }
        PremiumSessionFence Fence() => new PremiumSessionFence(bound, account.Owner, account.Generation);
        void RequireCurrent(PremiumIdentity identity)
        {
            if (!Current || !bound.Same(identity)) throw new PremiumServiceException("ownership-lost");
            if (!transport.Configured) throw new PremiumServiceException("unconfigured");
        }
        public async Task<bool> ActivateAsync()
        {
            RequireCurrent(bound);
            if (!engine.PreparePremiumProfile() || !await account.ConfirmProgressAsync()) return false;
            RequireCurrent(bound);
            var snapshot = await transport.ActivateAsync(Fence());
            RequireCurrent(bound);
            return engine.ReconcilePremiumWallet(bound.Uid, snapshot);
        }
        public async Task ReconcileWalletAsync(PremiumIdentity identity)
        {
            RequireCurrent(identity);
            var snapshot = await transport.WalletAsync(identity);
            RequireCurrent(identity);
            if (!engine.ReconcilePremiumWallet(identity.Uid, snapshot)) throw new PremiumServiceException("wallet-reconciliation-failed");
        }
        public async Task<VerifiedPackGrant> VerifyAndGrantAsync(PremiumIdentity identity, string productId, string purchaseToken)
        {
            RequireCurrent(identity);
            if (!Configured) throw new PremiumServiceException("wallet-not-active");
            var result = await transport.FulfillAsync(identity, productId, purchaseToken);
            RequireCurrent(identity);
            return result;
        }
        public async Task<bool> DrawAsync()
        {
            RequireCurrent(bound);
            if (!Configured || drawing) return false;
            drawing = true;
            try
            {
                var request = engine.BeginCurrencyDraw();
                if (request == null || !await account.ConfirmProgressAsync()) return false;
                RequireCurrent(bound);
                CurrencyDrawOutcome result;
                try { result = await transport.DrawAsync(Fence(), request); }
                catch (PremiumServiceException error) when (error.Code == "stale-wallet" || error.Code == "insufficient-currency")
                {
                    RequireCurrent(bound);
                    // These rejections are returned only after checking the durable operation ledger.
                    // Network failures and all other uncertain outcomes retain the request for replay.
                    if (!engine.ResolveRejectedCurrencyDraw(bound.Uid, request.OperationId)) return false;
                    await ReconcileWalletAsync(bound);
                    return false;
                }
                RequireCurrent(bound);
                return engine.ApplyCurrencyDraw(bound.Uid, result);
            }
            finally { drawing = false; }
        }
        public void Dispose() { disposed = true; }
    }
}
