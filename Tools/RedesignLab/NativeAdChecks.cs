using System.Text.Json;
using RocketRaja.Core;

static partial class Program
{
    static async Task NativeAdChecks(RedesignCatalog catalog)
    {
        var s = RedesignState.Fresh(1000); s.products[0] = 1;
        var e = new RedesignEngine(catalog, s, _ => {}); e.Save(); e.ResolveOffline(1100);
        var receipt = Clone(e.State.offline); var balance = e.State.cash;
        e.Save(); e.ResolveOffline(1160, preservePendingReceipt: true);
        Check(e.State.offline.id == receipt.id && Near(e.State.offline.amount, receipt.amount) && e.State.offline.pending && e.State.cash > balance,
            "Native ad absence credits normal earnings while preserving the frozen pending reward");
        Check(e.RewardOfflineAd(receipt.id) && !e.RewardOfflineAd(receipt.id), "Native ad reward remains exactly once after presentation resume");
        balance = e.State.cash; e.ResolveOffline(1200, preservePendingReceipt: true);
        Check(e.State.offline.adRewarded && Near(e.State.offline.amount, receipt.amount) && e.State.cash > balance, "Resuming another native form cannot reset an already-paid receipt");

        string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);
        T Decode<T>(string value) => JsonSerializer.Deserialize<T>(value, Json);
        var cloud = new FakeCloud { Time = 1200 };
        const string uid = "native-ad-fixture", owner = "ad-device";
        await cloud.CreateAsync(new AccountProfile { uid = uid }, SavePayload.Create(Encode(e.State)));
        var root = Path.Combine(Path.GetTempPath(), "NativeAds-" + Guid.NewGuid().ToString("N"));
        var local = CheckpointStore.ForRedesign(root, uid, Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>);
        using var account = new RedesignAccountSession(cloud, local, uid, owner, catalog, Encode, Decode<RedesignState>, () => cloud.Time);
        await account.InitializeAsync(await cloud.AcquireAsync(uid, owner), reconcileOffline: false);
        var bound = account.BindEngine(); var key = bound.SessionKey; var id = bound.State.offline.id;
        cloud.Time += 60;
        Check(!account.CanPlay && !bound.Tap(), "Long native presentation does not bypass an expired account lease");
        await account.RenewAfterNativePresentationAsync();
        Check(account.CanPlay && bound.SessionKey == key && bound.State.offline.id == id && bound.Tap(), "Owned heartbeat renews the same engine binding and original reward receipt after an ad");
        cloud.Time += 60; await cloud.AcquireAsync(uid, "other-device");
        bool rejected = false;
        try { await account.RenewAfterNativePresentationAsync(); } catch (CloudConflictException) { rejected = true; }
        Check(rejected && !account.CanPlay && !bound.Tap(), "Native ad completion cannot take a lease back from another device");
    }
}
