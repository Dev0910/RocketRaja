using System;
using System.IO;
using System.Threading.Tasks;

namespace RocketRaja.Core
{
    /// <summary>Single-writer account session. Called on the Unity thread; adapters marshal listener callbacks there.</summary>
    public sealed class AccountSession : IPlayerSession, IDisposable
    {
        readonly ICloudSaveStore cloud;
        readonly CheckpointStore local;
        readonly Func<GameState, string> encode;
        readonly Func<string, GameState> decode;
        readonly Func<double> monotonic;
        readonly Catalog catalog;
        IDisposable listener;
        CloudRecord remote;
        AccountCheckpoint pending;
        Task operation;
        bool active, disposed, handoff, dirty, urgent;
        double lastAck, lastWrite, operationStarted, serverAnchor, localAnchor;
        public event Action Changed;
        public bool IsAccount => true;
        public bool CanPlay => active && !disposed && monotonic() - lastAck < 12;
        public string Status { get; private set; } = "Syncing progress…";
        public GameState State { get; private set; }
        public string Uid { get; }
        public string Owner { get; }
        public long Generation => remote?.generation ?? 0;
        public double ServerNow => serverAnchor + Math.Max(0, monotonic() - localAnchor);
        public bool Busy => operation != null && !operation.IsCompleted;
        public AccountSession(ICloudSaveStore cloud, CheckpointStore local, string uid, string owner,
            Catalog catalog, Func<GameState, string> encode, Func<string, GameState> decode, Func<double> monotonic)
        { this.cloud = cloud; this.local = local; Uid = uid; Owner = owner; this.catalog = catalog; this.encode = encode; this.decode = decode; this.monotonic = monotonic; }
        GameState ReadState(SavePayload payload)
        {
            if (payload == null || !payload.Valid()) throw new InvalidDataException("Cloud save checksum failed.");
            var state = decode(payload.json);
            if (state == null || !state.Valid() || state.economy != Catalog.EconomyVersion)
                throw new InvalidDataException("This save needs a compatible game version.");
            return state;
        }
        void Accept(CloudRecord value)
        {
            if (value == null || value.profile.uid != Uid || !value.OwnedBy(Owner, remote.generation))
                throw new CloudConflictException("Your game moved to another device.");
            remote = value; lastAck = localAnchor = monotonic(); serverAnchor = value.heartbeatAt;
        }
        public async Task InitializeAsync(CloudRecord owned, GameState guestReplacement = null, bool reconcileOffline = true)
        {
            active = false; handoff = false;
            listener?.Dispose(); listener = null;
            remote = owned;
            if (owned == null || owned.owner != Owner) throw new CloudConflictException("This device does not own the account session.");
            Accept(owned);
            var previous = local.Load(Uid);
            if (guestReplacement == null && previous != null && previous.pending)
            {
                if (previous.CanRetry(remote) || previous.IsAcknowledged(remote))
                {
                    if (previous.CanRetry(remote)) Accept(await cloud.CommitAsync(previous));
                    if (!previous.IsAcknowledged(remote)) throw new CloudConflictException("Pending save changed while recovering.");
                    if (previous.followup != null && remote.OwnedBy(previous.owner, previous.generation))
                    {
                        var next = NewCheckpoint(previous.followup);
                        local.Save(next); Accept(await cloud.CommitAsync(next)); previous = next;
                    }
                    else if (previous.followup != null) local.Archive("superseded-followup");
                    previous.pending = false; previous.followup = null; local.Save(previous);
                }
                else local.Archive("superseded");
            }
            if (guestReplacement != null) local.Archive("before-choice");
            State = guestReplacement == null ? ReadState(remote.save) : decode(encode(guestReplacement));
            if (!State.Valid()) throw new InvalidDataException("The selected progress is invalid.");
            if (guestReplacement != null)
            {
                // Local guest time is not an account clock. Preserve progress, reset time-based anchors on migration.
                double old = State.now;
                foreach (var s in State.skills) { s.activeUntil = ServerNow + Math.Max(0, s.activeUntil - old); s.readyAt = ServerNow + Math.Max(0, s.readyAt - old); }
                State.sponsorUntil = ServerNow + Math.Max(0, State.sponsorUntil - old);
                State.now = State.savedAt = ServerNow;
            }
            else if (reconcileOffline) new GameEngine(catalog, State).ResolveOffline(ServerNow);
            else State.now = State.savedAt = Math.Max(State.now, ServerNow);
            pending = NewCheckpoint(SavePayload.Create(encode(State))); local.Save(pending);
            Accept(await cloud.CommitAsync(pending));
            if (!pending.IsAcknowledged(remote)) throw new CloudConflictException("The initial save was not confirmed.");
            pending.pending = false; local.Save(pending); pending = null;
            active = true; lastWrite = monotonic(); Status = "Saved to Google";
            listener = cloud.Listen(Uid, OnRemote, ex => Block("Connection to your save was interrupted. Retry to continue."));
            Changed?.Invoke();
        }
        AccountCheckpoint NewCheckpoint(SavePayload payload) => new AccountCheckpoint
        {
            uid = Uid, owner = Owner, generation = remote.generation, baseRevision = remote.revision,
            save = payload, commitId = Guid.NewGuid().ToString("N"), pending = true
        };
        void OnRemote(CloudRecord value)
        {
            if (disposed) return;
            if (value == null || !value.OwnedBy(Owner, Generation)) { Block("Your game is active on another device."); return; }
            // Listener events do not acknowledge our pending transaction or re-enable a paused session.
            if (!string.IsNullOrEmpty(value.handoff) && value.handoff != Owner)
            { handoff = true; active = false; Status = "Saving before switching devices…"; Changed?.Invoke(); }
        }
        public void Save(bool immediate = false)
        {
            if (disposed || State == null) return;
            if (!State.Valid()) { Block("Your save needs attention."); return; }
            dirty = true; urgent |= immediate;
            State.savedAt = Math.Max(State.savedAt, State.now);
            var snapshot = SavePayload.Create(encode(State));
            // Keep the exact in-flight receipt plus newer local changes in ONE atomic envelope.
            if (pending != null && pending.pending) pending.followup = snapshot;
            else pending = NewCheckpoint(snapshot);
            local.Save(pending);
        }
        public void Tick(bool connected)
        {
            if (disposed) return;
            if (!connected) Block("Waiting for a connection. Your Google game is paused.");
            if (Busy)
            {
                if (monotonic() - operationStarted >= 10) Block("Waiting for save confirmation. Retry to continue.");
                return;
            }
            if (handoff) { operation = Run(Transfer); return; }
            if (!active) return;
            if (!CanPlay) { Block("Your connection needs checking. Retry to continue."); return; }
            if (urgent || monotonic() - lastWrite >= 5) operation = Run(Synchronize);
        }
        async Task Run(Func<Task> work)
        {
            operationStarted = monotonic();
            try { await work(); }
            catch (Exception ex)
            {
                handoff = false;
                Block(ex is CloudConflictException ? ex.Message : "Could not confirm your save. Reconnect and retry.");
            }
        }
        async Task Synchronize()
        {
            if (State == null) return;
            // Capture passive earnings too; GameEngine.Advance deliberately does not request immediate writes.
            if (pending == null) Save();
            var sent = pending;
            pending = sent; dirty = urgent = false;
            Status = "Saving…"; Changed?.Invoke();
            var result = await cloud.CommitAsync(sent);
            if (disposed) return;
            Accept(result);
            if (!sent.IsAcknowledged(result)) throw new CloudConflictException("Cloud progress changed during saving.");
            var newer = sent.followup;
            sent.pending = false; sent.followup = null; local.Save(sent);
            pending = newer == null ? null : NewCheckpoint(newer);
            urgent |= pending != null;
            if (pending != null) local.Save(pending);
            lastWrite = monotonic();
            if (active) Status = "Saved to Google";
            Changed?.Invoke();
        }
        async Task Transfer()
        {
            active = false;
            // The queued snapshot can predate passive earnings. Capture the exact paused state
            // as a follow-up before draining the queue and releasing the lease.
            Save(true);
            await Synchronize();
            if (pending != null) await Synchronize();
            await cloud.ReleaseAsync(Uid, Owner, Generation);
            handoff = false;
            Block("Your game moved to another device.");
        }
        public async Task FlushAndReleaseAsync()
        {
            active = false;
            if (Busy) await operation;
            if (disposed) return;
            await Transfer();
        }
        public void Block(string message)
        {
            if (disposed) return;
            bool changed = active || Status != message;
            active = false; Status = message;
            if (changed) Changed?.Invoke();
        }
        public async Task WaitForPendingAsync() { if (Busy) await operation; }
        public void Dispose() { disposed = true; active = false; listener?.Dispose(); listener = null; }
    }
}
