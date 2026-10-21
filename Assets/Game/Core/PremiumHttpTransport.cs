using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RocketRaja.Core
{
    public sealed class PremiumHttpResponse
    {
        public int Status;
        public string Json;
    }
    public interface IPremiumHttpConnection
    {
        bool Configured { get; }
        Task<PremiumHttpResponse> PostAsync(PremiumIdentity owner, string route, string json);
    }
    // Wire counts are decimal strings, unlike the embedded Natural struct used in local saves.
    [Serializable] public sealed class PremiumPackReply
    {
        public string uid, productId, revision, granted;
        public bool fulfilled, revoked;
        public VerifiedPackGrant ToGrant() => new VerifiedPackGrant { uid = uid, productId = productId, fulfilled = fulfilled, revoked = revoked, revision = new Natural { digits = revision }, granted = new Natural { digits = granted } };
    }
    public sealed class PremiumHttpTransport : IPremiumAuthorityTransport
    {
        [Serializable] public sealed class EmptyRequest { }
        [Serializable] public sealed class ActivateRequest { public string owner; public long generation; }
        [Serializable] public sealed class DrawRequest { public string operationId, expectedRevision, owner; public long generation; }
        [Serializable] public sealed class FulfillRequest { public string productId, purchaseToken; }
        [Serializable] public sealed class ErrorReply { public string error; }
        readonly IPremiumHttpConnection connection;
        readonly Func<object, string> encode;
        readonly Func<string, Type, object> decode;
        static readonly HashSet<string> rejections = new HashSet<string>(StringComparer.Ordinal) {
            "ownership-lost", "stale-wallet", "insufficient-currency", "talent-locked", "purchase-owner-mismatch", "payment-pending", "profile-owner-mismatch",
            "wallet-not-active", "progress-recovery-required", "gift-provenance-required", "operation-not-confirmed", "invalid-operation", "invalid-purchase", "purchase-invalid", "purchase-mismatch"
        };
        public bool Configured => connection.Configured;
        public PremiumHttpTransport(IPremiumHttpConnection connection, Func<object, string> encode, Func<string, Type, object> decode)
        { this.connection = connection; this.encode = encode; this.decode = decode; }
        async Task<T> Send<T>(PremiumIdentity owner, string route, object body) where T : class
        {
            if (!Configured) throw new PremiumServiceException("unconfigured");
            if (owner == null || string.IsNullOrEmpty(owner.Uid)) throw new PremiumServiceException("authentication-required");
            var response = await connection.PostAsync(owner, route, encode(body));
            if (response == null || response.Json == null || response.Json.Length > 262144) throw new PremiumServiceException("invalid-response");
            if (response.Status != 200)
            {
                if (response.Status == 401) throw new PremiumServiceException("authentication-required");
                if (response.Status == 429) throw new PremiumServiceException("retry-later");
                // A 5xx, redirect or network error can hide a committed transaction. Do not
                // reinterpret one as a definite no-debit rejection even if its body says so.
                if (response.Status == 400 || response.Status == 409)
                {
                    ErrorReply failure = null;
                    try { failure = decode(response.Json, typeof(ErrorReply)) as ErrorReply; } catch { }
                    if (failure?.error != null && rejections.Contains(failure.error)) throw new PremiumServiceException(failure.error);
                }
                throw new PremiumServiceException("service-unavailable");
            }
            try { return decode(response.Json, typeof(T)) as T ?? throw new FormatException(); }
            catch { throw new PremiumServiceException("invalid-response"); }
        }
        public Task<PremiumWalletSnapshot> ActivateAsync(PremiumSessionFence session) => Send<PremiumWalletSnapshot>(session.Identity, "activate", new ActivateRequest { owner = session.Owner, generation = session.Generation });
        public Task<PremiumWalletSnapshot> WalletAsync(PremiumIdentity identity) => Send<PremiumWalletSnapshot>(identity, "wallet", new EmptyRequest());
        public Task<CurrencyDrawOutcome> DrawAsync(PremiumSessionFence session, CurrencyDrawRequest request) => Send<CurrencyDrawOutcome>(session.Identity, "draw", new DrawRequest {
            owner = session.Owner, generation = session.Generation, operationId = request.OperationId, expectedRevision = request.ExpectedRevision
        });
        public async Task<VerifiedPackGrant> FulfillAsync(PremiumIdentity identity, string productId, string purchaseToken) =>
            (await Send<PremiumPackReply>(identity, "fulfill", new FulfillRequest { productId = productId, purchaseToken = purchaseToken })).ToGrant();
    }
}
