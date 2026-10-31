using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace RocketRaja
{
    /// <summary>Unity IAP 5 adapter. Nothing initializes it until an authenticated authority is configured.</summary>
    public sealed class UnityKajuPurchasing : IDisposable
    {
        readonly RedesignCatalog catalog;
        readonly IPremiumFulfillmentClient authority;
        readonly Func<PremiumIdentity> identity;
        readonly Dictionary<string, Product> products = new Dictionary<string, Product>(StringComparer.Ordinal);
        readonly PremiumFulfillmentCoordinator fulfillment;
        StoreController store;
        bool disposed, connected;
        public bool Busy { get; private set; }
        public string Status { get; private set; } = "Purchases are unavailable in this build.";
        public event Action Changed;
        public UnityKajuPurchasing(RedesignCatalog catalog, IPremiumFulfillmentClient authority, Func<PremiumIdentity> identity)
        { this.catalog = catalog; this.authority = authority; this.identity = identity; fulfillment = new PremiumFulfillmentCoordinator(catalog, authority, identity, () => !disposed); }
        void SetStatus(string value) { if (disposed) return; Status = value; Changed?.Invoke(); }
        bool HasOwner => !string.IsNullOrEmpty(identity()?.Uid) && !string.IsNullOrEmpty(identity()?.SessionKey);
        public string LocalizedPrice(string id) => products.TryGetValue(id, out var product) && product.availableToPurchase ? product.metadata.localizedPriceString : null;
        public bool CanPurchase(string id) => !disposed && connected && !Busy && authority?.Configured == true && HasOwner && LocalizedPrice(id) != null;
        public async Task InitializeAsync()
        {
            if (disposed) throw new ObjectDisposedException(nameof(UnityKajuPurchasing));
            if (Application.platform != RuntimePlatform.Android || authority?.Configured != true) { SetStatus("Purchases are unavailable in this build."); return; }
            if (!HasOwner) { SetStatus("Store purchases are not available yet."); return; }
            if (store == null)
            {
                store = UnityIAPServices.StoreController(GooglePlay.Name);
                store.OnStoreConnected += Connected;
                store.OnStoreDisconnected += Disconnected;
                store.OnProductsFetched += ProductsFetched;
                store.OnProductsFetchFailed += ProductsFailed;
                store.OnPurchasePending += PurchasePending;
                store.OnPurchaseConfirmed += PurchaseConfirmed;
                store.OnPurchaseFailed += PurchaseFailed;
                store.OnPurchaseDeferred += PurchaseDeferred;
                store.OnPurchasesFetched += PurchasesFetched;
                store.OnPurchasesFetchFailed += PurchasesFailed;
                store.ProcessPendingOrdersOnPurchasesFetched(false);
            }
            SetStatus("Connecting to Google Play…");
            try { await store.Connect(); }
            catch (Exception) { SetStatus("Google Play is unavailable. Reconnect and retry."); }
        }
        void Connected()
        {
            if (disposed) return;
            connected = true;
            store.FetchProducts(catalog.Packs.Select(p => new ProductDefinition(p.id, ProductType.Consumable)).ToList());
        }
        void Disconnected(StoreConnectionFailureDescription _) { connected = false; Busy = false; SetStatus("Google Play disconnected. Reconnect to finish pending purchases."); }
        void ProductsFetched(List<Product> fetched)
        {
            if (disposed) return;
            products.Clear();
            foreach (var product in fetched) if (catalog.Packs.Any(p => p.id == product.uSku) && product.type == ProductType.Consumable) products[product.uSku] = product;
            SetStatus(products.Count == catalog.Packs.Count ? "Ready" : "Some packs are unavailable from Google Play.");
            store.FetchPurchases();
        }
        void ProductsFailed(ProductFetchFailed _) => SetStatus("Pack prices could not be loaded. Reconnect and retry.");
        public bool Purchase(string id)
        {
            if (!CanPurchase(id)) return false;
            var owner = identity();
            if (store.GooglePlayStoreExtendedService == null) { SetStatus("Google Play purchase service is unavailable."); return false; }
            store.GooglePlayStoreExtendedService.SetObfuscatedAccountId(owner.ObfuscatedAccountId);
            Busy = true; SetStatus("Waiting for Google Play…");
            try { store.PurchaseProduct(products[id]); return true; }
            catch (Exception) { Busy = false; SetStatus("The purchase could not start. No reward was granted."); return false; }
        }
        async void PurchasePending(PendingOrder order)
        {
            if (disposed) return;
            var owner = identity();
            string token = order?.Info?.Google?.PurchaseToken;
            var items = order?.CartOrdered?.Items();
            if (owner == null || items == null || items.Count != 1 || items[0].Quantity != 1 || string.IsNullOrEmpty(token))
            { Busy = false; SetStatus("This purchase needs account recovery before it can be delivered."); return; }
            string sku = items[0].Product.uSku;
            if (!catalog.Packs.Any(p => p.id == sku) || order.Info.Google.ObfuscatedAccountId != owner.ObfuscatedAccountId)
            { Busy = false; SetStatus("Sign in to the game account that started this purchase to finish delivery."); return; }
            Busy = true; SetStatus("Verifying your purchase…");
            try
            {
                // The server already owns a durable, deduplicated grant. No client-side currency increment.
                if (await fulfillment.FulfillAsync(owner, sku, token, () => store.ConfirmPurchase(order)))
                    SetStatus("Kaju Katli delivered. Finishing the Google Play confirmation…");
            }
            catch (Exception) { SetStatus("Delivery is pending. Reconnect and retry; this purchase will not be granted twice."); }
            finally { Busy = fulfillment.IsBusy; if (!disposed) Changed?.Invoke(); }
        }
        void PurchaseConfirmed(Order order)
        {
            Busy = false;
            if (order is ConfirmedOrder) SetStatus("Purchase complete.");
            else SetStatus("Your currency is delivered. Google Play confirmation will retry after reconnecting.");
        }
        void PurchaseFailed(FailedOrder order)
        {
            Busy = false;
            SetStatus(order.FailureReason == PurchaseFailureReason.UserCancelled ? "Purchase canceled." : "The purchase did not complete. Retry from the shop.");
        }
        void PurchaseDeferred(DeferredOrder _) { Busy = false; SetStatus("Payment is pending with Google Play. Currency arrives after payment is verified."); }
        void PurchasesFetched(Orders orders)
        {
            if (disposed) return;
            foreach (var pending in orders.PendingOrders) PurchasePending(pending);
            // Confirmed consumables are restored from the authority, never granted from this callback.
        }
        void PurchasesFailed(PurchasesFetchFailureDescription _) => SetStatus("Pending purchases could not be checked. Reconnect and retry.");
        public async Task RestoreAsync()
        {
            var owner = identity();
            if (disposed || !connected || owner == null || authority?.Configured != true) return;
            try { await authority.ReconcileWalletAsync(owner); if (!disposed && owner.Same(identity())) store.FetchPurchases(); }
            catch (Exception) { SetStatus("Purchases could not be restored. Reconnect using the original game account."); }
        }
        public void Dispose()
        {
            if (disposed) return; disposed = true;
            if (store == null) return;
            store.OnStoreConnected -= Connected; store.OnStoreDisconnected -= Disconnected;
            store.OnProductsFetched -= ProductsFetched; store.OnProductsFetchFailed -= ProductsFailed;
            store.OnPurchasePending -= PurchasePending; store.OnPurchaseConfirmed -= PurchaseConfirmed;
            store.OnPurchaseFailed -= PurchaseFailed; store.OnPurchaseDeferred -= PurchaseDeferred;
            store.OnPurchasesFetched -= PurchasesFetched; store.OnPurchasesFetchFailed -= PurchasesFailed;
        }
    }
}
