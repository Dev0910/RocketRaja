using System;
using System.Threading.Tasks;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Cloud
{
    /// <summary>Legacy local-session host. Live identity is deliberately unavailable.</summary>
    public sealed class PlayerSessionHost : MonoBehaviour
    {
        public static PlayerSessionHost Instance { get; private set; }
        public static PlayerSessionHost Ensure()
        {
            if (!Instance) new GameObject("PlayerSessionHost").AddComponent<PlayerSessionHost>();
            return Instance;
        }
        GuestSession guest;
        bool changingScene;
        public bool Ready { get; private set; }
        public bool AccountMenuRequested { get; set; }
        public IPlayerSession Session { get; private set; }
        public AccountSession Account => null;
        public AccountProfile Profile => null;
        public CloudRecord Remote => null;
        public GameState GuestState => guest?.State;
        public bool GuestNeedsRecovery => guest?.NeedsRecovery ?? false;
        public bool GoogleSupported => false;
        public string StartupNotice { get; private set; }
        public bool AskGuestChoice => false;
        public bool OtherDevice => false;
        public Catalog Catalog { get; private set; }
        public event Action Changed;
        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            Catalog = Resources.Load<GameContent>("GameContent")?.catalog ?? RocketRaja.Core.Catalog.Create();
            SceneManager.sceneLoaded += SceneLoaded;
        }
        void SceneLoaded(Scene scene, LoadSceneMode mode) => changingScene = false;
        public void LoadGuest(bool smoke = false)
        {
            guest = new GuestSession(smoke ? System.IO.Path.Combine(Application.temporaryCachePath, "RocketRajaSmoke-" + Guid.NewGuid().ToString("N")) : LocalProfiles.Root);
            Session = guest; Ready = true;
            if (!smoke) LocalProfiles.SetMode("guest");
            Changed?.Invoke();
        }
        public void InspectGuest() { if (guest == null) guest = new GuestSession(LocalProfiles.Root); }
        public Task PrepareGoogleAsync(bool interactive) => Task.FromException(new NotSupportedException("Account services are unavailable in this local-only build."));
        public Task EnterAccountAsync(bool useGuest, bool requestHandoff) => Task.FromException(new NotSupportedException("Account services are unavailable in this local-only build."));
        public Task ResumeAfterAdAsync(string uid, long generation) => Task.FromException(new NotSupportedException("Account services are unavailable in this local-only build."));
        public Task ReturnToGuestAsync(bool flush)
        {
            guest = null; LoadGuest(GameController.SmokeMode); StartupNotice = null;
            return Task.CompletedTask;
        }
        public void OpenStartup(string notice = null)
        {
            Ready = false; StartupNotice = notice;
            if (changingScene || SceneManager.GetActiveScene().name == "Bootstrap") return;
            changingScene = true; SceneManager.LoadSceneAsync("Bootstrap");
        }
        public void OpenAccountMenu() { } // Preserve old serialized callbacks without opening account UI.
        public void EnterMain()
        {
            if (!Ready || Session == null || changingScene) return;
            changingScene = true; StartupNotice = null; SceneManager.LoadSceneAsync("Main");
        }
        public void StartFreshGuest() { InspectGuest(); guest.StartFresh(); Session = guest; Ready = true; LocalProfiles.SetMode("guest"); }
        void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= SceneLoaded; Instance = null;
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
