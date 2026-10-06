using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Firebase.Firestore;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Cloud
{
    /// <summary>Persistent identity/session ownership; creates no UI and never selects legacy save paths.</summary>
    public sealed class RedesignSessionHost : MonoBehaviour
    {
        public static RedesignSessionHost Instance { get; private set; }
        public static bool AccountFlowAvailable => Instance?.fixture == true || RedesignCloudSettings.Load()?.MatchesApplication == true;
        public static RedesignSessionHost Ensure(string root = null)
        {
            if (!Instance) new GameObject("RedesignSessionHost").AddComponent<RedesignSessionHost>();
            if (root != null)
            {
                if (Instance.Coordinator != null && Instance.ProfileRoot != root) throw new InvalidOperationException("A session cannot switch its guest save directory.");
                Instance.ProfileRoot = root;
            }
            return Instance;
        }
        readonly ConcurrentQueue<Action> mainQueue = new ConcurrentQueue<Action>();
        readonly Stopwatch clock = Stopwatch.StartNew();
        bool changingScene, fixture, foreground = true;
        string fixtureMode = "";
        GoogleSignInBridge google;
        public RedesignAccountCoordinator Coordinator { get; private set; }
        public RedesignAccountSession Account => Coordinator?.Account;
        public string ProfileRoot { get; private set; }
        public bool AccountMenuRequested { get; set; }
        public bool Ready { get; private set; }
        public bool GoogleSupported => fixture || AccountFlowAvailable && google && google.Supported;
        public string Mode => fixture ? fixtureMode : LocalProfiles.Mode;
        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this; ProfileRoot = RedesignEnvironment.ProfileRoot; DontDestroyOnLoad(gameObject);
            var bridge = new GameObject("RedesignGoogleSignInBridge"); bridge.transform.SetParent(transform); google = bridge.AddComponent<GoogleSignInBridge>();
            SceneManager.sceneLoaded += SceneLoaded;
        }
        void SceneLoaded(Scene scene, LoadSceneMode mode) => changingScene = false;
        void SetMode(string mode) { if (fixture) fixtureMode = mode; else LocalProfiles.SetMode(mode); }
        public async Task<RedesignAccountCoordinator> ConnectAsync(bool interactive)
        {
            if (!GoogleSupported) throw new NotSupportedException("Google saves are not configured for this build. Your guest progress stays on this device.");
            if (Coordinator == null)
            {
                var config = RedesignCloudSettings.Load();
                if (!config || !config.MatchesApplication) throw new NotSupportedException("The reviewed Google configuration is unavailable.");
                var auth = new FirebaseAccountAuth(google);
                try
                {
                    await auth.InitializeAsync();
                    if (this == null) { auth.Dispose(); throw new OperationCanceledException(); }
                    if (Firebase.FirebaseApp.DefaultInstance.Options.ProjectId != config.firebaseProjectId)
                        throw new NotSupportedException("Firebase project does not match the reviewed redesign configuration.");
                    var db = FirebaseFirestore.DefaultInstance; db.Settings.PersistenceEnabled = false;
                    var cloud = new MainThreadCloudStore(new FirestoreSaveStore(db, FirestoreSaveStore.RedesignCollection), action => mainQueue.Enqueue(action));
                    Coordinator = new RedesignAccountCoordinator(new MainThreadAuth(auth, action => mainQueue.Enqueue(action)), cloud,
                        RedesignRuntimeContent.Load(), ProfileRoot, LocalProfiles.DeviceId, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>,
                        JsonUtility.FromJson<SaveVersionHeader>, JsonUtility.ToJson, JsonUtility.FromJson<AccountCheckpoint>,
                        () => clock.Elapsed.TotalSeconds, () => RedesignRuntimeDriver.Utc,
                        error => error is FirestoreException firestore && firestore.ErrorCode == FirestoreError.PermissionDenied,
                        RedesignEnvironment.DefaultProfileRoot);
                }
                catch { if (Coordinator == null) auth.Dispose(); throw; }
            }
            await Coordinator.PrepareGoogleAsync(interactive); return Coordinator;
        }
        public async Task EnterAccountAsync(bool promote, bool handoff)
        {
            if (Coordinator == null) throw new InvalidOperationException("Choose an account first.");
            await Coordinator.EnterAccountAsync(promote, handoff); SetMode("account"); Ready = true;
        }
        public async Task ReconnectAsync()
        {
            if (Coordinator == null) throw new InvalidOperationException("Choose an account first.");
            await Coordinator.ReconnectAsync(); Ready = true;
        }
        public async Task ReturnToGuestAsync(bool flush)
        {
            Ready = false;
            if (Coordinator != null) await Coordinator.ReturnToGuestAsync(flush);
            SetMode("guest"); Ready = true;
        }
        public RedesignLoadResult InspectGuest()
        {
            try
            {
                return new RedesignSaveStore(System.IO.Path.Combine(ProfileRoot, "rocket-raja-v2.save"), RedesignRuntimeContent.Load(),
                    JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>).Load();
            }
            catch (Exception error) when (error is System.IO.IOException || error is UnauthorizedAccessException)
            { return new RedesignLoadResult { status = RedesignLoadStatus.Corrupt, notice = "Your saved files cannot be read. Retry before continuing." }; }
        }
        public async Task SelectSeparateGuestAsync()
        {
            if (SceneManager.GetActiveScene().name != "Bootstrap" || Account != null || InspectGuest().status != RedesignLoadStatus.LegacyTransitionRequired)
                throw new InvalidOperationException("A separate Classic profile requires the explicit legacy-save choice in Bootstrap.");
            Ready = false;
            if (Coordinator != null)
            {
                await Coordinator.ReturnToGuestAsync(false);
                Coordinator.Dispose(); Coordinator = null;
            }
#if UNITY_EDITOR
            if (fixture) ProfileRoot = System.IO.Path.Combine(ProfileRoot, "classic-v2");
            else
#endif
            {
                RedesignEnvironment.SelectSeparateGuest();
                ProfileRoot = RedesignEnvironment.ProfileRoot;
            }
            SetMode("guest"); Ready = true;
        }
        public void OpenAccountMenu()
        {
            Ready = false; AccountMenuRequested = true;
            if (changingScene || SceneManager.GetActiveScene().name == "Bootstrap") return;
            changingScene = true; SceneManager.LoadSceneAsync("Bootstrap");
        }
        public void EnterMain()
        {
            if (!Ready || Account != null && !Account.CanPlay || changingScene) return;
            changingScene = true; SceneManager.LoadSceneAsync("Main");
        }
        void Update()
        {
            while (mainQueue.TryDequeue(out var action)) action();
            if (foreground && SceneManager.GetActiveScene().name != "Main") Account?.Tick(Application.internetReachability != NetworkReachability.NotReachable);
        }
        void OnApplicationPause(bool paused)
        {
            foreground = !paused;
            // In Main the driver saves the departure checkpoint before blocking the account.
            if (paused && SceneManager.GetActiveScene().name != "Main") Account?.Block("Reconnect to resume your account.");
        }
        void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= SceneLoaded; Coordinator?.Dispose(); Instance = null;
        }
#if UNITY_EDITOR
        // Injection is Editor-only; an actual player can only construct the reviewed Firebase adapter.
        public void ConfigureFixture(RedesignAccountCoordinator coordinator, string root)
        {
            if (!Application.isPlaying || Coordinator != null) throw new InvalidOperationException("Configure one isolated Play Mode fixture before using the host.");
            fixture = true; ProfileRoot = root; Coordinator = coordinator;
        }
#endif
        sealed class MainThreadAuth : IAccountAuth
        {
            readonly IAccountAuth inner; readonly Action<Action> post; bool disposed;
            public MainThreadAuth(IAccountAuth inner, Action<Action> post) { this.inner = inner; this.post = post; inner.Changed += OnChanged; }
            public AccountProfile Current => inner.Current;
            public event Action Changed;
            void OnChanged() => post(() => { if (!disposed) Changed?.Invoke(); });
            public Task InitializeAsync() => inner.InitializeAsync();
            public Task<AccountProfile> SignInAsync() => inner.SignInAsync();
            public Task SignOutAsync() => inner.SignOutAsync();
            public void Dispose() { disposed = true; inner.Changed -= OnChanged; inner.Dispose(); }
        }
    }
}
