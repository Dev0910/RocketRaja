using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RocketRaja.Core
{
    /// <summary>Single-writer account session. Called on the Unity thread; adapters marshal listener callbacks there.</summary>
    public sealed class RedesignAccountSession : IDisposable
    {
        readonly ICloudSaveStore cloud;
        readonly CheckpointStore local;
        readonly Func<RedesignState, string> encode;
        readonly Func<string, RedesignState> decode;
        readonly Func<double> monotonic;
        readonly RedesignCatalog catalog;
        IDisposable listener;
        CloudRecord remote;
        AccountCheckpoint pending;
        Task operation, reconnect;
        bool active, disposed, handoff, dirty, urgent;
        long bindingEpoch;
        double lastAck, lastWrite, operationStarted, serverAnchor, localAnchor;
        public event Action Changed;
        public bool IsAccount => true;
        public bool CanPlay => active && !disposed && monotonic() - lastAck < 12;
        public string Status { get; private set; } = "Syncing progress…";
        public RedesignState State { get; private set; }
        public string Uid { get; }
        public string Owner { get; }
        public long Generation => remote?.generation ?? 0;
        public string SessionKey => Uid + ":" + Owner + ":" + Generation;
        public double ServerNow => serverAnchor + Math.Max(0, monotonic() - localAnchor);
        public bool Busy => operation != null && !operation.IsCompleted;
        public RedesignAccountSession(ICloudSaveStore cloud, CheckpointStore local, string uid, string owner,
            RedesignCatalog catalog, Func<RedesignState, string> encode, Func<string, RedesignState> decode, Func<double> monotonic)
        { this.cloud = cloud; this.local = local; Uid = uid; Owner = owner; this.catalog = catalog; this.encode = encode; this.decode = decode; this.monotonic = monotonic; }
        RedesignState ReadState(SavePayload payload)
        {
            if (payload == null || !payload.Valid()) throw new InvalidDataException("Cloud save checksum failed.");
            var state = decode(payload.json);
            if (state == null || !state.Valid(catalog) || state.developmentFixture || state.HasDevelopmentShop || !string.IsNullOrEmpty(state.migrationSourceDigest) || state.economy != RedesignCatalog.EconomyVersion || state.HasPremium && state.premium.uid != Uid)
                throw new InvalidDataException("This save needs a compatible game version.");
            return state;
        }
        void Accept(CloudRecord value)
        {
            if (disposed) throw new ObjectDisposedException(nameof(RedesignAccountSession));
            if (value == null || value.profile.uid != Uid || !value.OwnedBy(Owner, remote.generation))
                throw new CloudConflictException("Your game moved to another device.");
            remote = value; lastAck = localAnchor = monotonic(); serverAnchor = value.heartbeatAt;
        }
        public async Task InitializeAsync(CloudRecord owned, RedesignState guestReplacement = null, bool reconcileOffline = true)
        {
            if (disposed) throw new ObjectDisposedException(nameof(RedesignAccountSession));
            bindingEpoch++;
            active = false; handoff = false;
            listener?.Dispose(); listener = null;
            remote = owned;
            if (owned == null || owned.owner != Owner) throw new CloudConflictException("This device does not own the account session.");
            Accept(owned);
            var original = ReadState(owned.save); // Validate before replaying or acknowledging any checkpoint.
            if (guestReplacement != null && (original.HasPremium || !original.completedPrestiges.IsZero || !original.kajuKatli.IsZero || !original.freeCredits.IsZero || original.upgrades.Count != 0 || original.everPurchased.Count != 0 || original.products.Any(n => n > 0) || !original.manualTaps.IsZero))
                throw new CloudConflictException("Existing redesigned account progress cannot be overwritten by a recovery/guest snapshot. Reconcile ownership and the premium ledger first.");
            var previous = local.Load(Uid);
            if (guestReplacement == null && previous != null && previous.pending)
            {
                ReadState(previous.save);
                if (previous.followup != null) ReadState(previous.followup);
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
            if (!State.Valid(catalog) || State.developmentFixture || State.HasDevelopmentShop || !string.IsNullOrEmpty(State.migrationSourceDigest) || State.HasPremium && State.premium.uid != Uid) throw new InvalidDataException("The selected progress is invalid.");
            if (guestReplacement != null)
            {
                // Local guest time is not an account clock. Preserve progress, reset time-based anchors on migration.
                double old = State.now;
                for (int i = 0; i < 3; i++)
                {
                    State.activeUntil[i] = State.activeUntil[i] > old ? ServerNow + State.activeUntil[i] - old : 0;
                    State.readyAt[i] = ServerNow + Math.Max(0, State.readyAt[i] - old);
                    State.nextHandsAt[i] = State.activeUntil[i] > ServerNow ? ServerNow + Math.Max(0, State.nextHandsAt[i] - old) : 0;
                }
                State.nextManualTapAt = ServerNow + Math.Max(0, State.nextManualTapAt - old);
                State.now = State.savedAt = ServerNow;
            }
            else if (reconcileOffline) new RedesignEngine(catalog, State, next => State = next).ResolveOffline(ServerNow);
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
            if (!State.Valid(catalog)) { Block("Your save needs attention."); return; }
            QueueSnapshot(State, immediate);
        }
        public void CommitState(RedesignState next)
        {
            if (!CanPlay) throw new CloudConflictException("This session no longer owns the account.");
            if (next == null || !next.Valid(catalog) || next.developmentFixture || next.HasDevelopmentShop || !string.IsNullOrEmpty(next.migrationSourceDigest) || next.HasPremium && next.premium.uid != Uid) throw new InvalidDataException("Invalid command state");
            if (State != null && next.revision.CompareTo(State.revision) < 0) throw new CloudConflictException("Stale command state");
            if (State?.HasPremium == true && (!next.HasPremium || next.premium.uid != Uid || next.premium.profileId != State.premium.profileId || next.premium.revision.CompareTo(State.premium.revision) < 0))
                throw new CloudConflictException("The authoritative wallet cannot be replaced by an older gameplay snapshot.");
            QueueSnapshot(next, true);
            State = next;
        }
        public RedesignEngine BindEngine(IDrawRandom random = null)
        {
            if (!CanPlay || State == null) throw new CloudConflictException("Load an owned account before binding gameplay.");
            long epoch = ++bindingEpoch; string key = SessionKey;
            bool Current() => CanPlay && bindingEpoch == epoch && SessionKey == key;
            return new RedesignEngine(catalog, State, next => {
                if (!Current()) throw new CloudConflictException("This gameplay binding belongs to an earlier account session.");
                CommitState(next);
            }, random) { MutationAllowed = Current, SessionKey = key + ":binding:" + epoch };
        }
        void QueueSnapshot(RedesignState value, bool immediate)
        {
            var snapshot = SavePayload.Create(encode(value));
            bool following = pending != null && pending.pending;
            var staged = following ? new AccountCheckpoint { uid = pending.uid, owner = pending.owner, generation = pending.generation, baseRevision = pending.baseRevision, commitId = pending.commitId, save = pending.save, followup = snapshot, pending = true } : NewCheckpoint(snapshot);
            local.Save(staged); // Only publish the follow-up after its atomic local write succeeds.
            if (following) pending.followup = snapshot; else pending = staged;
            dirty = true; urgent |= immediate;
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
        // Recovery renews only the exact existing lease. Taking over another device
        // belongs to the separate, explicit account-selection flow.
        public Task ReconnectOwnedAsync()
        {
            if (reconnect != null && !reconnect.IsCompleted) return reconnect;
            return reconnect = ReconnectOwned();
        }
        async Task ReconnectOwned()
        {
            if (disposed) throw new ObjectDisposedException(nameof(RedesignAccountSession));
            if (remote == null || State == null) throw new CloudConflictException("Load an account before retrying its connection.");
            long generation = Generation;
            Block("Reconnecting to your save…");
            try
            {
                await WaitForPendingAsync();
                if (disposed) throw new ObjectDisposedException(nameof(RedesignAccountSession));
                if (Generation != generation) throw new CloudConflictException("The account session changed while reconnecting.");
                var owned = await cloud.HeartbeatAsync(Uid, Owner, generation);
                if (disposed) throw new ObjectDisposedException(nameof(RedesignAccountSession));
                if (owned == null || !owned.OwnedBy(Owner, generation) || !string.IsNullOrEmpty(owned.handoff))
                    throw new CloudConflictException("Your game moved to another device.");
                await InitializeAsync(owned);
            }
            catch (Exception error)
            {
                Block(error is CloudConflictException ? error.Message : "Could not reconnect to your save. Check your connection and retry.");
                throw;
            }
        }
        // Premium activation and spending require the profile/operation to be recoverable
        // from another device before the server can grant or debit anything.
        public async Task<bool> ConfirmProgressAsync()
        {
            if (!CanPlay || State == null) return false;
            long epoch = bindingEpoch; string key = SessionKey; Natural required = State.revision;
            Save(true);
            for (int attempt = 0; attempt < 3; attempt++)
            {
                if (Busy) await operation;
                if (!CanPlay || bindingEpoch != epoch || SessionKey != key) return false;
                if (ReadState(remote.save).revision.CompareTo(required) >= 0) return true;
                operation = Run(Synchronize);
                await operation;
            }
            return CanPlay && bindingEpoch == epoch && SessionKey == key && ReadState(remote.save).revision.CompareTo(required) >= 0;
        }
        public void Dispose() { disposed = true; active = false; listener?.Dispose(); listener = null; }
    }
}
