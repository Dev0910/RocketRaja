using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Firebase.Firestore;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace RocketRaja.Cloud
{
    public sealed class PlayerSessionHost : MonoBehaviour
    {
        public static PlayerSessionHost Instance { get; private set; }
        public static PlayerSessionHost Ensure()
        {
            if (Instance == null) new GameObject("PlayerSessionHost").AddComponent<PlayerSessionHost>();
            return Instance;
        }
        readonly ConcurrentQueue<Action> mainQueue = new ConcurrentQueue<Action>();
        readonly Stopwatch clock = Stopwatch.StartNew();
        IAccountAuth auth;
        ICloudSaveStore cloud;
        GoogleSignInBridge google;
        GuestSession guest;
        bool changingScene, foreground = true;
        public bool Ready { get; private set; }
        public bool AccountMenuRequested { get; set; }
        public IPlayerSession Session { get; private set; }
        public AccountSession Account => Session as AccountSession;
        public AccountProfile Profile => auth?.Current;
        public CloudRecord Remote { get; private set; }
        public GameState GuestState => guest?.State;
        public bool GuestNeedsRecovery => guest?.NeedsRecovery ?? false;
        public bool GoogleSupported => !RedesignEnvironment.Isolated && google != null && google.Supported;
        public string StartupNotice { get; private set; }
        public bool AskGuestChoice { get; private set; }
        public Catalog Catalog { get; private set; }
        public event Action Changed;
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            Catalog = Resources.Load<GameContent>("GameContent")?.catalog ?? RocketRaja.Core.Catalog.Create();
            var bridge = new GameObject("GoogleSignInBridge"); bridge.transform.SetParent(transform);
            google = bridge.AddComponent<GoogleSignInBridge>();
            SceneManager.sceneLoaded += SceneLoaded;
        }
        void SceneLoaded(Scene scene, LoadSceneMode mode) { changingScene = false; }
        public void LoadGuest(bool smoke = false)
        {
            guest = new GuestSession(smoke ? System.IO.Path.Combine(Application.temporaryCachePath, "RocketRajaSmoke-" + Guid.NewGuid().ToString("N")) : LocalProfiles.Root);
            Session = guest; Ready = true;
            if (!smoke) LocalProfiles.SetMode("guest");
        }
        public void InspectGuest() { if (guest == null) guest = new GuestSession(LocalProfiles.Root); }
        async Task EnsureCloudAsync()
        {
            RedesignEnvironment.RequireLiveCloud();
            if (auth == null) { auth = new FirebaseAccountAuth(google); auth.Changed += AuthChanged; }
            await auth.InitializeAsync();
            if (cloud == null)
            {
                var db = FirebaseFirestore.DefaultInstance;
                db.Settings.PersistenceEnabled = false;
                cloud = new MainThreadCloudStore(new FirestoreSaveStore(db), a => mainQueue.Enqueue(a));
            }
        }
        void AuthChanged() => mainQueue.Enqueue(() =>
        {
            if (Account != null && (Profile == null || Profile.uid != Account.Uid)) Account.Block("Please sign in again to continue your Google game.");
        });
        public async Task PrepareGoogleAsync(bool interactive)
        {
            Ready = false; InspectGuest();
            if (Account != null) { await Account.WaitForPendingAsync(); Account.Dispose(); Session = null; }
            await EnsureCloudAsync();
            if (interactive || Profile == null) await auth.SignInAsync();
            if (Profile == null) throw new InvalidOperationException("Please sign in with Google.");
            Remote = await cloud.ReadAsync(Profile.uid);
            AskGuestChoice = interactive && !guest.NeedsRecovery && LocalProfiles.HasProgress(guest.State) && Remote != null;
            if (Remote == null)
            {
                var seed = guest.NeedsRecovery ? new GameState { now = GameController.Utc, savedAt = GameController.Utc } : guest.State;
                Remote = await cloud.CreateAsync(Profile, SavePayload.Create(JsonUtility.ToJson(seed)));
                NewAccount = true;
            }
            else NewAccount = false;
        }
        bool NewAccount;
        public bool OtherDevice => Remote != null && Remote.owner != "" && Remote.owner != LocalProfiles.DeviceId;
        public async Task EnterAccountAsync(bool useGuest, bool requestHandoff)
        {
            if (Remote == null || Profile == null) throw new InvalidOperationException("Sign in before loading account progress.");
            string uid = Profile.uid, owner = LocalProfiles.DeviceId;
            // Keep the compared cloud revision; do not silently overwrite a save which changed during the choice.
            long comparedRevision = Remote.revision;
            if (requestHandoff) await cloud.RequestHandoffAsync(uid, owner);
            CloudRecord owned = null;
            double started = clock.Elapsed.TotalSeconds;
            while (owned == null)
            {
                try { owned = await cloud.AcquireAsync(uid, owner); }
                catch (FirestoreException ex) when (requestHandoff && ex.ErrorCode == FirestoreError.PermissionDenied && clock.Elapsed.TotalSeconds - started < 45)
                { await Task.Delay(2000); }
            }
            if (useGuest && !NewAccount && owned.revision != comparedRevision)
            {
                await cloud.ReleaseAsync(uid, owner, owned.generation); Remote = owned; AskGuestChoice = true;
                throw new CloudConflictException("The Google save changed while switching devices. Compare the updated saves before replacing it.");
            }
            var selected = useGuest && !guest.NeedsRecovery ? guest.State : null;
            if (useGuest || NewAccount)
            {
                LocalProfiles.Backup(JsonUtility.FromJson<GameState>(owned.save.json), uid, "cloud-before-choice");
                if (!guest.NeedsRecovery) { LocalProfiles.Backup(guest.State, uid, "guest-before-choice"); selected = guest.State; }
            }
            var local = new CheckpointStore(LocalProfiles.Root, uid, JsonUtility.ToJson, JsonUtility.FromJson<AccountCheckpoint>);
            var account = new AccountSession(cloud, local, uid, owner, Catalog, JsonUtility.ToJson, JsonUtility.FromJson<GameState>, () => clock.Elapsed.TotalSeconds);
            Session = account; account.Changed += SessionChanged;
            await account.InitializeAsync(owned, selected);
            Remote = owned; LocalProfiles.SetMode("account"); Ready = true; AskGuestChoice = false;
        }
        public async Task ResumeAfterAdAsync(string uid, long generation)
        {
            var account = Account;
            if (account == null || account.Uid != uid || account.Generation != generation) throw new CloudConflictException("The account changed during the video.");
            await account.WaitForPendingAsync();
            var record = await cloud.ReadAsync(uid);
            if (record == null || !record.OwnedBy(account.Owner, generation) || record.handoff != "") throw new CloudConflictException("The account moved during the video.");
            record = await cloud.AcquireAsync(uid, account.Owner);
            await account.InitializeAsync(record, null, false);
        }
        void SessionChanged() { Changed?.Invoke(); }
        public async Task ReturnToGuestAsync(bool flush)
        {
            Ready = false;
            var account = Account;
            if (account != null)
            {
                if (flush) await account.FlushAndReleaseAsync();
                account.Dispose(); Session = null;
            }
            if (auth != null) await auth.SignOutAsync();
            guest = null; LoadGuest(GameController.SmokeMode); StartupNotice = null;
        }
        public void OpenStartup(string notice = null)
        {
            Ready = false; StartupNotice = notice;
            if (changingScene || SceneManager.GetActiveScene().name == "Bootstrap") return;
            changingScene = true; SceneManager.LoadSceneAsync("Bootstrap");
        }
        public void OpenAccountMenu()
        {
            AccountMenuRequested = true;
            if (changingScene) return;
            changingScene = true; SceneManager.LoadSceneAsync("Bootstrap");
        }
        public void EnterMain()
        {
            if (!Ready || Session == null || changingScene) return;
            changingScene = true; StartupNotice = null; SceneManager.LoadSceneAsync("Main");
        }
        public void StartFreshGuest() { InspectGuest(); guest.StartFresh(); Session = guest; Ready = true; LocalProfiles.SetMode("guest"); }
        void Update()
        {
            while (mainQueue.TryDequeue(out var action)) { try { action(); } catch (Exception ex) { Debug.LogException(ex); } }
            if (Account == null) return;
            if (foreground) Account.Tick(Application.internetReachability != NetworkReachability.NotReachable);
            if (Ready && !Account.CanPlay && !Account.Status.StartsWith("Saving before") && SceneManager.GetActiveScene().name == "Main")
            {
                var controller = FindFirstObjectByType<GameController>();
                if (controller == null || !controller.IsAdShowing) { controller?.Save(); OpenStartup(Account.Status); }
            }
        }
        void OnApplicationPause(bool paused) { foreground = !paused; if (paused) Account?.Block("Reconnect to resume your account."); }
        void OnApplicationQuit() { Account?.Save(true); }
        void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= SceneLoaded; Account?.Dispose(); auth?.Dispose(); cloud?.Dispose(); Instance = null;
        }
    }
    // SDK listener errors can run on worker threads; no callback touches Unity objects from there.
    sealed class MainThreadCloudStore : ICloudSaveStore
    {
        readonly ICloudSaveStore inner; readonly Action<Action> post;
        public MainThreadCloudStore(ICloudSaveStore inner, Action<Action> post) { this.inner = inner; this.post = post; }
        public Task<CloudRecord> ReadAsync(string uid) => inner.ReadAsync(uid);
        public Task<CloudRecord> CreateAsync(AccountProfile p, SavePayload s) => inner.CreateAsync(p, s);
        public Task<CloudRecord> AcquireAsync(string uid, string owner) => inner.AcquireAsync(uid, owner);
        public Task<CloudRecord> HeartbeatAsync(string uid, string owner, long g) => inner.HeartbeatAsync(uid, owner, g);
        public Task<CloudRecord> CommitAsync(AccountCheckpoint c, bool release = false) => inner.CommitAsync(c, release);
        public Task RequestHandoffAsync(string uid, string target) => inner.RequestHandoffAsync(uid, target);
        public Task ReleaseAsync(string uid, string owner, long g) => inner.ReleaseAsync(uid, owner, g);
        public IDisposable Listen(string uid, Action<CloudRecord> changed, Action<Exception> failed) => inner.Listen(uid, r => post(() => changed(r)), e => post(() => failed(e)));
        public void Dispose() => inner.Dispose();
    }
}
