using RocketRaja.Core;

static partial class Program
{
    sealed class TestPremiumClient : IPremiumFulfillmentClient
    {
        public bool Configured { get; set; } = true;
        public Func<PremiumIdentity, string, string, Task<VerifiedPackGrant>> verify;
        public Func<PremiumIdentity, Task> reconcile = _ => Task.CompletedTask;
        public Task<VerifiedPackGrant> VerifyAndGrantAsync(PremiumIdentity owner, string sku, string token) => verify(owner, sku, token);
        public Task ReconcileWalletAsync(PremiumIdentity owner) => reconcile(owner);
    }
    static async Task PremiumClientChecks(RedesignCatalog data)
    {
        var original = new PremiumIdentity("synthetic-player-a", "owner-a:generation-1");
        PremiumIdentity current = original; bool alive = true; int ack = 0, cache = 0;
        var service = new TestPremiumClient();
        VerifiedPackGrant Grant(PremiumIdentity owner, string sku) => new() { uid = owner.Uid, productId = sku, fulfilled = true, revision = 1, granted = 1000 };
        service.verify = (owner, sku, token) => Task.FromResult(Grant(owner, sku));
        service.reconcile = _ => { cache++; return Task.CompletedTask; };
        var coordinator = new PremiumFulfillmentCoordinator(data, service, () => current, () => alive);
        string sku = data.Packs[0].id;
        Check(await coordinator.FulfillAsync(original, sku, "synthetic-token", () => { Check(cache == 1, "Paid wallet is durably reconciled before store acknowledgment"); ack++; }), "Verified fulfillment reaches acknowledgment without client currency increment");
        var pending = new TaskCompletionSource<VerifiedPackGrant>(); service.verify = (_, _, _) => pending.Task;
        var first = coordinator.FulfillAsync(original, sku, "pending-token", () => ack++);
        Check(!await coordinator.FulfillAsync(original, sku, "pending-token", () => ack++), "Concurrent pending callbacks share one fulfillment operation");
        current = new PremiumIdentity(original.Uid, "owner-b:generation-2"); pending.SetResult(Grant(original, sku));
        Check(!await first && ack == 1 && cache == 1, "Generation change fences wallet application and acknowledgment after verification");
        current = original;
        service.verify = (owner, id, _) => Task.FromResult(Grant(owner, id));
        service.reconcile = _ => throw new IOException("cache unavailable");
        bool failed = false; try { await coordinator.FulfillAsync(original, sku, "cache-token", () => ack++); } catch (IOException) { failed = true; }
        Check(failed && ack == 1 && !coordinator.IsBusy, "Local wallet failure leaves the store transaction pending and retriable");
        service.reconcile = _ => Task.CompletedTask;
        Check(await coordinator.FulfillAsync(original, sku, "cache-token", () => ack++) && ack == 2, "Uncertain delivery retries the same token before acknowledgment");
        service.verify = (_, id, _) => Task.FromResult(Grant(new PremiumIdentity("other", "other"), id));
        failed = false; try { await coordinator.FulfillAsync(original, sku, "wrong-owner", () => ack++); } catch (InvalidOperationException) { failed = true; }
        Check(failed && ack == 2, "Mismatched fulfillment owner cannot acknowledge or populate this wallet");
        service.verify = (owner, id, _) => Task.FromResult(Grant(owner, id));
        var cachePending = new TaskCompletionSource<bool>(); service.reconcile = _ => cachePending.Task;
        var disposed = coordinator.FulfillAsync(original, sku, "disposed-token", () => ack++); alive = false; cachePending.SetResult(true);
        Check(!await disposed && ack == 2, "Disposed store adapter rejects a late cache completion");
        Check(original.ObfuscatedAccountId == SavePayload.Digest("rocket-raja:synthetic-player-a").ToLowerInvariant(), "Client and authority use stable privacy-preserving account mapping");
    }
}
