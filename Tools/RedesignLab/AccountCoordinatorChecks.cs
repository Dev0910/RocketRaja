using System.Text.Json;
using RocketRaja.Core;

static partial class Program
{
    sealed class CoordinatorAuth : IAccountAuth
    {
        public AccountProfile Current { get; private set; }
        public string NextUid = "coordinator-player";
        public bool Cancel;
        public TaskCompletionSource<bool> InitializeGate;
        public event Action Changed;
        public Task InitializeAsync() => InitializeGate?.Task ?? Task.CompletedTask;
        public Task<AccountProfile> SignInAsync()
        {
            if (Cancel) throw new OperationCanceledException("Fixture cancellation");
            Set(NextUid); return Task.FromResult(Current);
        }
        public void Set(string uid) { Current = uid == null ? null : new AccountProfile { uid = uid, displayName = "Fixture", email = "", photoUrl = "" }; Changed?.Invoke(); }
        public Task SignOutAsync() { Set(null); return Task.CompletedTask; }
        public void Dispose() { }
    }
    static async Task AccountCoordinatorChecks(RedesignCatalog data)
    {
        string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);
        T Decode<T>(string value) => JsonSerializer.Deserialize<T>(value, Json);
        async Task Rejected(Func<Task> operation, string label)
        { bool rejected = false; try { await operation(); } catch { rejected = true; } Check(rejected, label); }
        string Root() => Path.Combine(Path.GetTempPath(), "RedesignIdentity-" + Guid.NewGuid().ToString("N"));
        RedesignSaveStore Store(string root) => new(Path.Combine(root, "rocket-raja-v2.save"), data, Encode, Decode<RedesignState>, Decode<SaveVersionHeader>);
        RedesignAccountCoordinator Coordinator(string root, CoordinatorAuth auth, FakeCloud cloud) =>
            new(auth, cloud, data, root, "fixture-device", Encode, Decode<RedesignState>, Decode<SaveVersionHeader>, Encode, Decode<AccountCheckpoint>, () => cloud.Time, () => 20000);
        string root = Root(); var auth = new CoordinatorAuth(); var cloud = new FakeCloud();
        var source = RedesignState.Fresh(20000); source.products[0] = 1; source.storeName = "Device fixture"; source.manualTaps = 3;
        Store(root).Save(source); var bytes = File.ReadAllBytes(Path.Combine(root, "rocket-raja-v2.save"));
        using var host = Coordinator(root, auth, cloud);
        auth.Cancel = true;
        await Rejected(() => host.PrepareGoogleAsync(true), "Cancelled identity selection creates no cloud account");
        Check(cloud.Record == null && bytes.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "rocket-raja-v2.save"))), "Cancelled login preserves every guest-save byte");
        auth.Cancel = false; auth.InitializeGate = new TaskCompletionSource<bool>();
        var preparing = host.PrepareGoogleAsync(true);
        await Rejected(() => host.PrepareGoogleAsync(true), "Concurrent identity preparation cannot race account selection");
        auth.InitializeGate.SetResult(true); await preparing; auth.InitializeGate = null;
        Check(host.Remote == null && host.CanPromoteGuest && cloud.Record == null, "Preparing a new identity reads progress without creating an account before the choice");
        await host.EnterAccountAsync(true, false);
        Check(host.Account.CanPlay && host.Account.State.products[0] == 1 && host.Account.State.manualTaps.Value == 3 && host.Account.State.storeName == "Device fixture",
            "Confirmed new-account choice imports the guest once through the actual account initialization boundary");
        Check(host.Account.State.now == cloud.Time && bytes.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "rocket-raja-v2.save"))),
            "Promotion rebases server time while retaining the independent guest bytes");
        Check(Directory.Exists(Path.Combine(root, "economy-2")) && !Directory.Exists(Path.Combine(root, "accounts")), "Coordinator checkpoints use only the redesigned economy namespace");
        var old = host.Account.BindEngine(); auth.Set("changed-player");
        Check(!host.Account.CanPlay && !old.Tap(), "Authentication change immediately blocks the bound account and stale gameplay");
        await Rejected(host.ReconnectAsync, "Recovery cannot renew the old UID under a different authenticated identity");
        auth.Set("coordinator-player"); await host.ReconnectAsync();
        await host.ReturnToGuestAsync(true);
        Check(host.Account == null && auth.Current == null && cloud.Record.owner == "" && host.GuestState.manualTaps.Value == 3,
            "Sign out confirms pending progress, releases the lease and restores the independent guest");
        await host.PrepareGoogleAsync(true); long revision = cloud.Record.revision;
        await Rejected(() => host.EnterAccountAsync(true, false), "Guest selection cannot overwrite an existing Google game");
        Check(cloud.Record.revision == revision, "Rejected replacement leaves confirmed cloud progress unchanged");
        await host.EnterAccountAsync(false, false);
        Check(host.Account.CanPlay && host.Account.State.manualTaps.Value == 3, "Existing Google progress opens without reseeding it from the guest");
        await host.ReturnToGuestAsync(true);

        string testRoot = Root(); var testCloud = new FakeCloud(); var testAuth = new CoordinatorAuth { NextUid = "test-grant-player" };
        var engine = new RedesignEngine(data, RedesignState.Fresh(20000), Store(testRoot).Save);
        Check(engine.EnableDevelopmentShop(DevelopmentShopState.Namespace), "Fixture creates an explicitly marked local test-shop profile");
        using var simulator = Coordinator(testRoot, testAuth, testCloud);
        await simulator.PrepareGoogleAsync(true);
        Check(!simulator.CanPromoteGuest, "Test-shop history is ineligible for live account promotion even without a pending pack");
        await Rejected(() => simulator.EnterAccountAsync(true, false), "Promotion refuses the simulated-purchase namespace");
        Check(testCloud.Record == null, "Rejected test-grant transfer creates no cloud document");
        await simulator.EnterAccountAsync(false, false);
        Check(!simulator.Account.State.HasDevelopmentShop && simulator.Account.State.products.All(n => n == 0) && Store(testRoot).Load().state.HasDevelopmentShop,
            "A separate fresh Google choice preserves the local simulator without transferring its grants");

        string switchedRoot = Root(); var switchedAuth = new CoordinatorAuth(); var switchedCloud = new FakeCloud();
        foreach (bool converted in new[] { false, true })
        {
            string fixtureRoot = Root(); var fixtureCloud = new FakeCloud();
            var fixture = RedesignState.Fresh(20000);
            fixture.developmentFixture = !converted;
            fixture.migrationSourceDigest = converted ? new string('a', 64) : null;
            Store(fixtureRoot).Save(fixture);
            using var fixtureHost = Coordinator(fixtureRoot, new CoordinatorAuth(), fixtureCloud);
            await fixtureHost.PrepareGoogleAsync(true);
            Check(!fixtureHost.CanPromoteGuest, "Boosted/converted fixture provenance cannot promote to a real account");
            await Rejected(() => fixtureHost.EnterAccountAsync(true, false), "Fixture promotion is rejected before creating remote progress");
            Check(fixtureCloud.Record == null, "Fixture rejection leaves the cloud empty");
        }
        using var switched = Coordinator(switchedRoot, switchedAuth, switchedCloud);
        await switched.PrepareGoogleAsync(true); switchedAuth.Set("other-player");
        await Rejected(() => switched.EnterAccountAsync(false, false), "Identity changed after comparison requires a new selection");
        Check(switchedCloud.Record == null, "Late identity change cannot create an account from a stale choice");
        var future = RedesignState.Fresh(); future.schema = 99;
        await switchedCloud.CreateAsync(new AccountProfile { uid = "other-player" }, SavePayload.Create(Encode(future)));
        await Rejected(() => switched.PrepareGoogleAsync(false), "Future account schema is rejected before acquisition or checkpoint writes");
        Check(switchedCloud.Record.owner == "" && switchedCloud.Record.revision == 1, "Unsupported cloud data remains untouched by account selection");

        string stableRoot = Root(), separateGuest = Path.Combine(stableRoot, "classic-v2");
        var stableCloud = new FakeCloud(); var stableAuth = new CoordinatorAuth { NextUid = "canonical-account" };
        using var stable = new RedesignAccountCoordinator(stableAuth, stableCloud, data, separateGuest, "stable-device", Encode, Decode<RedesignState>, Decode<SaveVersionHeader>, Encode, Decode<AccountCheckpoint>, () => stableCloud.Time, () => 20000, accountRoot: stableRoot);
        await stable.PrepareGoogleAsync(true); await stable.EnterAccountAsync(false, false);
        Check(Directory.Exists(Path.Combine(stableRoot, "economy-2", "accounts")) && !Directory.Exists(Path.Combine(separateGuest, "economy-2")), "Live account checkpoint location is independent of the selected guest profile");
        await stable.ReturnToGuestAsync(true);
        var retainedCheckpoint = CheckpointStore.ForRedesign(stableRoot, stableAuth.NextUid, Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>).Load(stableAuth.NextUid);
        CheckpointStore.ForRedesign(separateGuest, stableAuth.NextUid, Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>).Save(retainedCheckpoint);
        await stable.PrepareGoogleAsync(true); long untouchedRevision = stableCloud.Record.revision, untouchedGeneration = stableCloud.Record.generation;
        await Rejected(() => stable.EnterAccountAsync(false, false), "Earlier guest-relative checkpoint requires recovery before selecting the canonical account path");
        Check(stableCloud.Record.revision == untouchedRevision && stableCloud.Record.generation == untouchedGeneration, "Checkpoint-location conflict is detected before cloud ownership or save changes");
    }
}
