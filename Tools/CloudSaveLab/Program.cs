using RocketRaja.Core;
using System.Text.Json;

static class Test
{
    static readonly JsonSerializerOptions Json = new() { IncludeFields = true, IgnoreReadOnlyProperties = true };
    static string Encode<T>(T v) => JsonSerializer.Serialize(v, Json);
    static T Decode<T>(string v) => JsonSerializer.Deserialize<T>(v, Json);
    static T Copy<T>(T v) => Decode<T>(Encode(v));
    static int checks;
    static void Check(bool condition, string message) { if (!condition) throw new Exception("FAIL: " + message); checks++; Console.WriteLine("PASS: " + message); }
    static async Task Rejected(Func<Task> work, string message) { bool failed = false; try { await work(); } catch { failed = true; } Check(failed, message); }
    static async Task Main()
    {
        var root = Path.Combine(Path.GetTempPath(), "RocketRajaCloudTests-" + Guid.NewGuid().ToString("N"));
        var data = Catalog.Create();
        var state = new GameState { now = 1000, savedAt = 1000, cash = new Amount(1.25, 450), reputation = 7, lifetimeReputation = 9, music = false, reducedMotion = true };
        state.products[0].level = 30; state.legacy[0].level = 1; state.claimedRewards.Add("receipt-1");
        state.pendingOfflineId = "offline-1"; state.pendingOfflineReward = 50;
        var payload = SavePayload.Create(Encode(state));
        Check(payload.Valid() && Decode<GameState>(payload.json).Valid(), "Complete game snapshot validates");
        var copy = Decode<GameState>(payload.json);
        Check(copy.cash.exponent == 450 && copy.legacy[0].level == 1 && copy.claimedRewards[0] == "receipt-1" && !copy.music && copy.reducedMotion && copy.pendingOfflineId == "offline-1", "Large amounts, preferences, permanent progress and receipts round-trip");
        var damaged = Copy(payload); damaged.json += " "; Check(!damaged.Valid(), "Modified cloud payload fails checksum");
        var cloud = new FakeCloud();
        await cloud.CreateAsync(new AccountProfile { uid = "player", displayName = "Test", email = "", photoUrl = "" }, payload);
        var owned = await cloud.AcquireAsync("player", "device-a");
        var local = new CheckpointStore(root, "player", Encode, Decode<AccountCheckpoint>);
        AccountSession Session() => new(cloud, local, "player", "device-a", data, Encode, Decode<GameState>, () => cloud.Time);
        using var session = Session();
        await session.InitializeAsync(owned);
        Check(session.CanPlay && cloud.Record.revision == 2, "Account enters gameplay only after initial confirmed commit");
        var engine = new GameEngine(data, session.State) { MutationAllowed = () => session.CanPlay };
        engine.Tap(); session.Save(true); session.Tick(true); await session.WaitForPendingAsync();
        Check(Decode<GameState>(cloud.Record.save.json).manualTaps == 1, "An immediate account save reaches the cloud");
        long before = cloud.Record.revision;
        var checkpoint = new AccountCheckpoint { uid = "player", owner = "device-a", generation = cloud.Record.generation, baseRevision = before, save = cloud.Record.save, commitId = Guid.NewGuid().ToString("N"), pending = true };
        await cloud.CommitAsync(checkpoint); await cloud.CommitAsync(checkpoint);
        Check(cloud.Record.revision == before + 1, "Retrying the same upload receipt cannot commit twice");
        await Rejected(() => cloud.CommitAsync(new AccountCheckpoint { uid = "player", owner = "device-a", generation = 1, baseRevision = before, save = payload, commitId = "different", pending = true }), "A stale revision cannot overwrite progress");
        local.Save(checkpoint);
        Check(local.Load("player").commitId == checkpoint.commitId && local.Load("different-player") == null, "Checkpoint metadata is atomic and account-bound");
        await session.InitializeAsync(await cloud.AcquireAsync("player", "device-a"));
        Check(!local.Load("player").pending, "A recovered acknowledged upload becomes a confirmed checkpoint");

        engine = new GameEngine(data, session.State) { MutationAllowed = () => session.CanPlay };
        cloud.Hold = new TaskCompletionSource<bool>();
        engine.Tap(); session.Save(true); session.Tick(true);
        engine.Tap(); session.Save(true); // A second action occurs while the first immutable snapshot is uploading.
        cloud.Hold.SetResult(true); await session.WaitForPendingAsync(); cloud.Hold = null;
        session.Tick(true); await session.WaitForPendingAsync();
        Check(Decode<GameState>(cloud.Record.save.json).manualTaps == 3, "Changes during an in-flight upload survive in a follow-up checkpoint");
        cloud.LoseAck = true; engine.Tap(); session.Save(true); session.Tick(true); await session.WaitForPendingAsync();
        Check(!session.CanPlay && Decode<GameState>(cloud.Record.save.json).manualTaps == 4, "Lost acknowledgment pauses play without repeating the accepted mutation");
        await session.InitializeAsync(await cloud.AcquireAsync("player", "device-a"));
        Check(session.State.manualTaps == 4 && session.CanPlay, "Reconnect recovers a server-accepted upload exactly once");

        engine = new GameEngine(data, session.State) { MutationAllowed = () => session.CanPlay };
        var snapshot = Encode(session.State); session.Tick(false);
        engine.Tap(); engine.Advance(100); engine.Grant(100); engine.BuyProduct(0, 1); engine.BuyTap(); engine.BuyUpgrade(data.upgrades[0].id);
        engine.ActivateSkill(0); engine.BuyLegacy(0); engine.Prestige(engine.PreviewPrestige()); engine.ResolveOffline(9000); engine.RewardAd("late-ad", "sponsor"); engine.DismissOffline();
        Check(Encode(session.State) == snapshot, "Offline session blocks every direct economy command and late reward callback");
        await session.InitializeAsync(await cloud.AcquireAsync("player", "device-a"));
        engine = new GameEngine(data, session.State) { MutationAllowed = () => session.CanPlay };
        session.Save(true);
        engine.Advance(2);
        var finalHandoffState = Encode(session.State);
        await cloud.RequestHandoffAsync("player", "device-b");
        session.Tick(true); await session.WaitForPendingAsync();
        Check(!session.CanPlay && cloud.Record.owner == "", "Device handoff pauses the old session and flushes before releasing ownership");
        var transferredState = Decode<GameState>(cloud.Record.save.json);
        var expectedHandoffState = Decode<GameState>(finalHandoffState);
        Check(transferredState.now == expectedHandoffState.now && transferredState.cash.CompareTo(expectedHandoffState.cash) == 0,
            "Handoff includes passive earnings accumulated after the last queued checkpoint");
        var b = await cloud.AcquireAsync("player", "device-b");
        Check(b.generation == 2 && Decode<GameState>(b.save.json).manualTaps == 4, "The second device receives the complete saved progress with a new ownership generation");
        await Rejected(() => cloud.CommitAsync(checkpoint), "The old device cannot save after ownership changes");
        await Rejected(() => cloud.AcquireAsync("player", "device-c"), "Two live devices cannot hold the same account lease");
        cloud.Time += 31; var c = await cloud.AcquireAsync("player", "device-c");
        Check(c.generation == 3, "A crashed device can be replaced after server lease expiry");
        Check(!checkpoint.CanRetry(c), "Superseded local checkpoint is not eligible for automatic replay");

        var secondRoot = Path.Combine(root, "offline-reward");
        var offlineCloud = new FakeCloud();
        var offlineState = new GameState { now = 1000, savedAt = 1000 }; offlineState.products[0].level = 1;
        await offlineCloud.CreateAsync(new AccountProfile { uid = "player" }, SavePayload.Create(Encode(offlineState)));
        offlineCloud.Time = 4600;
        var offlineStore = new CheckpointStore(secondRoot, "player", Encode, Decode<AccountCheckpoint>);
        using var offline = new AccountSession(offlineCloud, offlineStore, "player", "a", data, Encode, Decode<GameState>, () => offlineCloud.Time);
        await offline.InitializeAsync(await offlineCloud.AcquireAsync("player", "a"));
        var reward = offline.State.cash;
        await offline.InitializeAsync(await offlineCloud.AcquireAsync("player", "a"));
        Check(offline.State.cash.CompareTo(reward) == 0, "Repeated reconnect cannot double-award offline earnings");
        offlineCloud.Time += 120;
        await offline.InitializeAsync(await offlineCloud.AcquireAsync("player", "a"), null, false);
        Check(offline.State.cash.CompareTo(reward) == 0, "Account revalidation after an ad excludes ad time from offline earnings");

        var timeoutCloud = new FakeCloud();
        await timeoutCloud.CreateAsync(new AccountProfile { uid = "player" }, SavePayload.Create(Encode(new GameState { now = 1000, savedAt = 1000 })));
        var timeoutStore = new CheckpointStore(Path.Combine(root, "timeout"), "player", Encode, Decode<AccountCheckpoint>);
        using var timed = new AccountSession(timeoutCloud, timeoutStore, "player", "a", data, Encode, Decode<GameState>, () => timeoutCloud.Time);
        await timed.InitializeAsync(await timeoutCloud.AcquireAsync("player", "a"));
        var timedEngine = new GameEngine(data, timed.State) { MutationAllowed = () => timed.CanPlay };
        timedEngine.Tap(); timed.Save(true);
        timeoutCloud.Hold = new TaskCompletionSource<bool>(); timed.Tick(true);
        timeoutCloud.Time += 10; timed.Tick(true);
        Check(!timed.CanPlay && timeoutStore.Load("player").pending, "An unacknowledged upload timeout pauses play and retains its local checkpoint");
        timeoutCloud.Hold.SetResult(true); await timed.WaitForPendingAsync(); timeoutCloud.Hold = null;
        Check(!timed.CanPlay, "A late server acknowledgment cannot silently resume a paused session");
        await timed.InitializeAsync(await timeoutCloud.AcquireAsync("player", "a"));
        Check(timed.CanPlay && timed.State.manualTaps == 1, "Explicit reconnection restores the accepted timed-out upload once");

        timedEngine = new GameEngine(data, timed.State) { MutationAllowed = () => timed.CanPlay };
        timedEngine.Tap(); timed.Save(true); timed.Dispose(); // Process ends before the queued upload starts.
        using var restarted = new AccountSession(timeoutCloud, timeoutStore, "player", "a", data, Encode, Decode<GameState>, () => timeoutCloud.Time);
        await restarted.InitializeAsync(await timeoutCloud.AcquireAsync("player", "a"));
        Check(restarted.State.manualTaps == 2 && !timeoutStore.Load("player").pending,
            "Restart uploads a valid interrupted local checkpoint without losing or duplicating progress");
        foreach (bool acceptedBeforeRestart in new[] { false, true })
        {
            var recoveryCloud = new FakeCloud();
            var recoveryState = new GameState { now = 1000, savedAt = 1000, manualTaps = 1 };
            await recoveryCloud.CreateAsync(new AccountProfile { uid = "player" }, SavePayload.Create(Encode(new GameState { now = 1000, savedAt = 1000 })));
            var recoveryOwned = await recoveryCloud.AcquireAsync("player", "a");
            var recoveryStore = new CheckpointStore(Path.Combine(root, "empty-followup-" + acceptedBeforeRestart), "player", Encode, Decode<AccountCheckpoint>);
            var interrupted = new AccountCheckpoint { uid = "player", owner = "a", generation = recoveryOwned.generation,
                baseRevision = recoveryOwned.revision, commitId = "interrupted", pending = true, save = SavePayload.Create(Encode(recoveryState)),
                // Exact representation emitted by Unity JsonUtility for a null inline class.
                followup = new SavePayload { schema = 1, json = "", checksum = "" } };
            recoveryStore.Save(interrupted);
            if (acceptedBeforeRestart) await recoveryCloud.CommitAsync(interrupted);
            using var recovered = new AccountSession(recoveryCloud, recoveryStore, "player", "a", data, Encode, Decode<GameState>, () => recoveryCloud.Time);
            await recovered.InitializeAsync(await recoveryCloud.AcquireAsync("player", "a"));
            Check(recovered.CanPlay && recovered.State.manualTaps == 1 && !recoveryStore.Load("player").pending,
                "Restart with Unity empty follow-up recovers " + (acceptedBeforeRestart ? "already accepted" : "not yet uploaded") + " receipt exactly once");
        }
        Console.WriteLine($"All {checks} cloud protocol checks passed. Artifacts: {root}");
    }

    sealed class FakeCloud : ICloudSaveStore
    {
        public double Time = 1000;
        public CloudRecord Record;
        public TaskCompletionSource<bool> Hold;
        public bool LoseAck;
        event Action<CloudRecord> Changed;
        void Notify() => Changed?.Invoke(Copy(Record));
        public Task<CloudRecord> ReadAsync(string uid) => Task.FromResult(Copy(Record));
        public Task<CloudRecord> CreateAsync(AccountProfile p, SavePayload s)
        {
            if (Record != null) throw new CloudConflictException("exists");
            Record = new CloudRecord { profile = p, save = s, revision = 1, commitId = Guid.NewGuid().ToString("N"), createdAt = Time, updatedAt = Time, heartbeatAt = Time };
            return ReadAsync(p.uid);
        }
        public Task<CloudRecord> AcquireAsync(string uid, string owner)
        {
            if (Record.owner != owner && Record.owner != "" && Time - Record.heartbeatAt < 30) throw new CloudConflictException("lease active");
            if (Record.owner != owner) { Record.owner = owner; Record.generation++; Record.handoff = ""; }
            Record.heartbeatAt = Time; Notify(); return ReadAsync(uid);
        }
        public Task<CloudRecord> HeartbeatAsync(string uid, string owner, long generation) => AcquireAsync(uid, owner);
        public async Task<CloudRecord> CommitAsync(AccountCheckpoint c, bool release = false)
        {
            if (Hold != null) await Hold.Task;
            if (c.IsAcknowledged(Record)) return Copy(Record);
            if (!Record.OwnedBy(c.owner, c.generation) || c.baseRevision != Record.revision || Time - Record.heartbeatAt >= 30) throw new CloudConflictException("stale");
            Record.save = Copy(c.save); Record.revision++; Record.commitId = c.commitId; Record.updatedAt = Record.heartbeatAt = Time;
            if (release) Record.owner = "";
            Notify();
            if (LoseAck) { LoseAck = false; throw new IOException("response lost"); }
            return Copy(Record);
        }
        public Task RequestHandoffAsync(string uid, string target) { Record.handoff = target; Notify(); return Task.CompletedTask; }
        public Task ReleaseAsync(string uid, string owner, long generation)
        {
            if (!Record.OwnedBy(owner, generation)) throw new CloudConflictException("lost owner");
            Record.owner = ""; Notify(); return Task.CompletedTask;
        }
        public IDisposable Listen(string uid, Action<CloudRecord> changed, Action<Exception> failed)
        { Changed += changed; return new Subscription(() => Changed -= changed); }
        sealed class Subscription : IDisposable { readonly Action close; public Subscription(Action close) { this.close = close; } public void Dispose() => close(); }
        public void Dispose() { }
    }
}
