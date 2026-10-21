using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RocketRaja.Core
{
    /// <summary>Identity/selection boundary for redesigned saves. Never reads legacy paths or merges accounts.</summary>
    public sealed class RedesignAccountCoordinator : IDisposable
    {
        readonly IAccountAuth auth;
        readonly ICloudSaveStore cloud;
        readonly RedesignCatalog catalog;
        readonly RedesignSaveStore guest;
        readonly string root, checkpointRoot, owner;
        readonly Func<RedesignState, string> encode;
        readonly Func<string, RedesignState> decode;
        readonly Func<string, SaveVersionHeader> version;
        readonly Func<AccountCheckpoint, string> encodeCheckpoint;
        readonly Func<string, AccountCheckpoint> decodeCheckpoint;
        readonly Func<double> clock, utc;
        readonly Func<Exception, bool> acquireBusy;
        bool disposed, entering, selecting;
        string preparedUid;
        public RedesignAccountSession Account { get; private set; }
        public AccountProfile Profile => auth.Current;
        public CloudRecord Remote { get; private set; }
        public RedesignLoadResult GuestLoad { get; private set; }
        public RedesignState GuestState => GuestLoad?.state;
        public RedesignState CloudState { get; private set; }
        public bool OtherDevice => Remote != null && Remote.owner != "" && Remote.owner != owner;
        public bool CanPromoteGuest => GuestState != null && !GuestState.developmentFixture && string.IsNullOrEmpty(GuestState.migrationSourceDigest) && !GuestState.HasPremium && !GuestState.HasDevelopmentShop && !GuestState.requiresDrawReconciliation;
        public static bool HasProgress(RedesignState state) => state != null &&
            (!state.manualTaps.IsZero || !state.completedPrestiges.IsZero || !state.fame.IsZero || !state.freeCredits.IsZero ||
             !state.kajuKatli.IsZero || state.upgrades.Count != 0 || state.everPurchased.Count != 0 ||
             state.products.Any(level => level > 0) || state.cardLevels.Any(level => !level.IsZero));
        public RedesignAccountCoordinator(IAccountAuth auth, ICloudSaveStore cloud, RedesignCatalog catalog,
            string root, string owner, Func<RedesignState, string> encode, Func<string, RedesignState> decode,
            Func<string, SaveVersionHeader> version, Func<AccountCheckpoint, string> encodeCheckpoint,
            Func<string, AccountCheckpoint> decodeCheckpoint, Func<double> clock, Func<double> utc, Func<Exception, bool> acquireBusy = null, string accountRoot = null)
        {
            this.auth = auth ?? throw new ArgumentNullException(nameof(auth)); this.cloud = cloud ?? throw new ArgumentNullException(nameof(cloud));
            this.catalog = catalog; this.root = root; this.owner = owner; this.encode = encode; this.decode = decode;
            checkpointRoot = accountRoot ?? root;
            this.version = version;
            this.encodeCheckpoint = encodeCheckpoint; this.decodeCheckpoint = decodeCheckpoint; this.clock = clock; this.utc = utc;
            this.acquireBusy = acquireBusy ?? (_ => false);
            guest = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), catalog, encode, decode, version);
            auth.Changed += AuthChanged;
        }
        void Alive() { if (disposed) throw new ObjectDisposedException(nameof(RedesignAccountCoordinator)); }
        void Identity(string uid)
        {
            Alive();
            if (Profile == null || Profile.uid != uid) throw new CloudConflictException("The signed-in account changed. Choose your account again.");
        }
        void AuthChanged()
        {
            if (Account != null && (Profile == null || Profile.uid != Account.Uid)) Account.Block("Please sign in again to continue your Google game.");
        }
        public RedesignLoadResult InspectGuest()
        {
            Alive(); GuestLoad = guest.Load();
            // A missing guest may be offered as a fresh account seed, without creating a device save.
            if (GuestLoad.CanCreateFresh) GuestLoad.state = RedesignState.Fresh(utc());
            return GuestLoad;
        }
        RedesignState Read(CloudRecord remote, string uid)
        {
            if (remote == null) return null;
            if (remote.profile?.uid != uid || remote.save == null || !remote.save.Valid()) throw new InvalidDataException("Your Google save needs recovery. Existing progress is preserved.");
            var state = decode(remote.save.json);
            if (state == null || !state.Valid(catalog) || state.developmentFixture || !string.IsNullOrEmpty(state.migrationSourceDigest) || state.HasDevelopmentShop || state.HasPremium && state.premium.uid != uid)
                throw new InvalidDataException("This Google save needs a compatible game version. Existing progress is preserved.");
            return state;
        }
        public async Task PrepareGoogleAsync(bool interactive)
        {
            Alive();
            if (entering || selecting) throw new InvalidOperationException("An account operation is already in progress.");
            if (Account != null) throw new InvalidOperationException("Return to guest or reconnect the current account before selecting another account.");
            selecting = true; preparedUid = null;
            try
            {
                Remote = null; CloudState = null; InspectGuest();
                await auth.InitializeAsync(); Alive();
                if (interactive || Profile == null) await auth.SignInAsync();
                Alive(); string uid = Profile?.uid;
                if (string.IsNullOrEmpty(uid)) throw new InvalidOperationException("Google sign-in did not finish.");
                var value = await cloud.ReadAsync(uid); Identity(uid);
                CloudState = Read(value, uid); Remote = value; preparedUid = uid;
            }
            finally { selecting = false; }
        }
        public async Task EnterAccountAsync(bool promoteGuest, bool requestHandoff)
        {
            Alive();
            if (entering || selecting || Account != null) throw new InvalidOperationException("An account operation is already in progress.");
            string uid = Profile?.uid; if (string.IsNullOrEmpty(uid)) throw new InvalidOperationException("Sign in before choosing progress.");
            if (preparedUid != uid) throw new CloudConflictException("The signed-in account changed. Compare its progress before continuing.");
            if (promoteGuest && !CanPromoteGuest)
                throw new CloudConflictException("This device save needs recovery or contains test/purchase data that cannot transfer to Google. Keep playing as guest or use Google progress.");
            if (!string.Equals(Path.GetFullPath(root), Path.GetFullPath(checkpointRoot), StringComparison.OrdinalIgnoreCase) &&
                CheckpointStore.ForRedesign(root, uid, encodeCheckpoint, decodeCheckpoint, version).Load(uid) != null)
                throw new InvalidDataException("An earlier account checkpoint is preserved beside your guest profile. Recover it before changing account save locations.");
            entering = true;
            RedesignAccountSession candidate = null;
            try
            {
                bool creating = Remote == null;
                if (creating)
                {
                    // Do not seed cloud timestamps from the device clock. Initialization rebases
                    // the selected fresh/guest state after acquisition supplies server time.
                    var created = await cloud.CreateAsync(Profile, SavePayload.Create(encode(RedesignState.Fresh(0))));
                    Identity(uid); CloudState = Read(created, uid); Remote = created;
                }
                if (OtherDevice && !requestHandoff) throw new CloudConflictException("Your game is active on another device. Choose Play here to move it.");
                if (requestHandoff && OtherDevice) { await cloud.RequestHandoffAsync(uid, owner); Identity(uid); }
                var wait = System.Diagnostics.Stopwatch.StartNew(); CloudRecord owned;
                while (true)
                {
                    Identity(uid);
                    try { owned = await cloud.AcquireAsync(uid, owner); break; }
                    catch (Exception error) when (requestHandoff && acquireBusy(error) && wait.Elapsed.TotalSeconds < 45) { await Task.Delay(2000); }
                }
                Identity(uid); Read(owned, uid);
                if (!owned.OwnedBy(owner, owned.generation)) throw new CloudConflictException("Could not confirm ownership of this game.");
                var local = CheckpointStore.ForRedesign(checkpointRoot, uid, encodeCheckpoint, decodeCheckpoint, version);
                candidate = new RedesignAccountSession(cloud, local, uid, owner, catalog, encode, decode, clock);
                var selected = promoteGuest ? decode(encode(GuestState)) : creating ? RedesignState.Fresh(owned.heartbeatAt) : null;
                // The guest remains in its original independent save; account checkpoints use economy-2.
                await candidate.InitializeAsync(owned, selected, replaceExistingProgress: promoteGuest); Identity(uid);
                Account = candidate; candidate = null; Remote = owned; CloudState = Account.State;
            }
            finally { candidate?.Dispose(); entering = false; }
        }
        public async Task ReconnectAsync()
        {
            Alive(); var current = Account ?? throw new InvalidOperationException("No account is selected.");
            Identity(current.Uid); await current.ReconnectOwnedAsync(); Identity(current.Uid);
        }
        public async Task ReturnToGuestAsync(bool flush)
        {
            Alive();
            if (entering || selecting) throw new InvalidOperationException("Wait for the current account operation.");
            if (Account != null)
            {
                if (flush) await Account.FlushAndReleaseAsync(); else await Account.WaitForPendingAsync();
                Alive(); Account.Dispose(); Account = null;
            }
            await auth.SignOutAsync(); Alive(); preparedUid = null; Remote = null; CloudState = null; InspectGuest();
        }
        public void Dispose()
        {
            if (disposed) return; disposed = true;
            auth.Changed -= AuthChanged; Account?.Dispose(); cloud.Dispose(); auth.Dispose();
        }
    }
}
