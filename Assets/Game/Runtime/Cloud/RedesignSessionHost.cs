using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Cloud
{
    /// <summary>Persistent identity/session ownership; creates no UI and never selects legacy save paths.</summary>
    public sealed class RedesignSessionHost : MonoBehaviour
    {
        public static RedesignSessionHost Instance { get; private set; }
        // Account fixtures remain available in Editor tests; players never initialize live identity.
        public static bool AccountFlowAvailable => Instance?.fixture == true;
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
        bool changingScene, fixture, foreground = true;
        string fixtureMode = "";
        public RedesignAccountCoordinator Coordinator { get; private set; }
        public RedesignAccountSession Account => Coordinator?.Account;
        public string ProfileRoot { get; private set; }
        public bool AccountMenuRequested { get; set; }
        public bool Ready { get; private set; }
        public bool IsFixture => fixture;
        public bool GoogleSupported => fixture;
        public string Mode => fixture ? fixtureMode : "guest";
        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this; ProfileRoot = RedesignEnvironment.ProfileRoot; DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SceneLoaded;
        }
        void SceneLoaded(Scene scene, LoadSceneMode mode) => changingScene = false;
        void SetMode(string mode) { if (fixture) fixtureMode = mode; else LocalProfiles.SetMode(mode); }
        public async Task<RedesignAccountCoordinator> ConnectAsync(bool interactive)
        {
            if (!fixture || Coordinator == null) throw new NotSupportedException("Account services are unavailable. Progress is saved on this device.");
            await Coordinator.PrepareGoogleAsync(interactive); return Coordinator;
        }
        public async Task EnterAccountAsync(bool promote, bool handoff)
        {
            if (!fixture || Coordinator == null) throw new InvalidOperationException("Account sessions are available only to isolated Editor fixtures.");
            await Coordinator.EnterAccountAsync(promote, handoff); SetMode("account"); Ready = true;
        }
        public async Task ReconnectAsync()
        {
            if (!fixture || Coordinator == null) throw new InvalidOperationException("Account sessions are available only to isolated Editor fixtures.");
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
            if (!fixture) return;
            Ready = false; AccountMenuRequested = true;
            if (changingScene || SceneManager.GetActiveScene().name == "Bootstrap") return;
            changingScene = true; SceneManager.LoadSceneAsync("Bootstrap");
        }
        public AsyncOperation EnterMain()
        {
            if (!Ready || Account != null && !Account.CanPlay || changingScene) return null;
            changingScene = true; return SceneManager.LoadSceneAsync("Main");
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
        // Injection is Editor-only; an actual player can only select local progress.
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
