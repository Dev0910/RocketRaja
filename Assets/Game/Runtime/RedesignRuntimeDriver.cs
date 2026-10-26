using System;
using System.IO;
using System.Threading.Tasks;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>Scene-independent new-economy lifecycle. Reviewed screens bind to this driver.</summary>
    public sealed class RedesignRuntimeDriver : MonoBehaviour
    {
        public RedesignLifecycle Session { get; private set; }
        public RedesignLoadResult LoadResult { get; private set; }
        public event Action Changed;
        RedesignAccountSession account;
        string guestRoot;
        bool initialOfflinePending;
        Task<bool> recovery;
        bool destroyed;
        public bool NativeAdPresentation { get; private set; }
        bool presentationForeground = true;
        bool developmentProfile;
        public bool Initialized => Session != null && !initialOfflinePending;
        public bool IsAccount => account != null;
        public string GuestProfileRoot => guestRoot;
        public bool Recovering => recovery != null && !recovery.IsCompleted;
        public string RecoveryNotice => Session?.SaveFailure ?? account?.Status ?? LoadResult?.notice;
        public bool IsDisposableGuest => Session != null && account == null && developmentProfile;
        public bool CanResetDevelopmentGame
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return Initialized && !IsAccount && !Recovering && Session.CanMutate && !Session.Engine.State.HasPremium;
#else
                return false;
#endif
            }
        }

        public bool ResetDevelopmentGame()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!CanResetDevelopmentGame) return false;
            var catalog = RedesignRuntimeContent.Load();
            var store = new RedesignSaveStore(Path.Combine(guestRoot, "rocket-raja-v2.save"),
                catalog, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            var fresh = RedesignState.Fresh(Utc);
            // Keep the durable revision monotonic while clearing every gameplay field.
            fresh.revision = Session.Engine.State.revision;
            if (!Session.Run(e => { store.Save(fresh); return true; })) { Changed?.Invoke(); return false; }
            Session = new RedesignLifecycle(new RedesignEngine(catalog, fresh, store.Save));
            // Replace the recovery backup too, so recovery cannot resurrect pre-reset progress.
            bool saved = Session.Run(e => e.Save());
            Changed?.Invoke();
            return saved;
#else
            return false;
#endif
        }
        public void InitializeAccount(RedesignAccountSession account)
        {
            if (Session != null) throw new InvalidOperationException("A runtime session is already bound");
            if (account == null) throw new ArgumentNullException(nameof(account));
            this.account = account;
            // Account initialization already reconciles server time and durably settles its offline receipt.
            Session = new RedesignLifecycle(account.BindEngine());
            account.Changed += AccountChanged;
            Changed?.Invoke();
        }
        void AccountChanged() => Changed?.Invoke();
        public bool InitializeGuest(string root = null, bool allowDevelopmentProfile = false)
        {
            if (Session != null) throw new InvalidOperationException("A runtime session is already bound");
            guestRoot = root ?? RedesignEnvironment.ProfileRoot;
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && CLASSIC_DEVICE_PREVIEW)
            developmentProfile = allowDevelopmentProfile && root != null && guestRoot != RedesignEnvironment.ProfileRoot;
#endif
            var catalog = RedesignRuntimeContent.Load();
            var store = new RedesignSaveStore(Path.Combine(guestRoot, "rocket-raja-v2.save"),
                catalog, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            LoadResult = store.Load();
            if (LoadResult.state == null && !LoadResult.CanCreateFresh) return false;
            var state = LoadResult.state ?? RedesignState.Fresh(Utc);
            if (!developmentProfile && (state.developmentFixture || state.HasDevelopmentShop || !string.IsNullOrEmpty(state.migrationSourceDigest)))
            {
                LoadResult.notice = "This profile contains development progress. Its files are preserved; choose a normal player profile.";
                return false;
            }
            Session = new RedesignLifecycle(new RedesignEngine(catalog, state, store.Save));
            initialOfflinePending = true;
            // Persist backup-reconciliation flags before allowing interaction.
            if (!Session.Run(e => e.Save())) return false;
            Session.Run(e => e.ResolveOffline(Utc));
            if (Session.CanMutate) initialOfflinePending = false;
            Changed?.Invoke();
            return Session.CanMutate;
        }
        public static double Utc => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
        public bool Command(Func<RedesignEngine, bool> action)
        {
            bool done = Session != null && Session.Run(action);
            Changed?.Invoke();
            return done;
        }
        public bool TapGameplaySurface()
        {
            bool done = Session != null && Session.TapGameplaySurface();
            if (done) Changed?.Invoke();
            return done;
        }
        public bool RetrySaving()
        {
            bool recovered = Session != null && (Session.Suspended ? Session.Resume(account?.ServerNow ?? Utc) : Session.RetrySave());
            if (recovered && initialOfflinePending)
            {
                Session.Run(e => e.ResolveOffline(Utc));
                recovered = Session.CanMutate;
                if (recovered) initialOfflinePending = false;
            }
            Changed?.Invoke();
            return recovered;
        }
        public bool RetryLoading() => Session == null ? InitializeGuest(guestRoot, developmentProfile) : RetrySaving();
        public Task<bool> RetryLoadingAsync()
        {
            if (Recovering) return recovery;
            if (account == null) return Task.FromResult(RetryLoading());
            return recovery = RecoverAccount();
        }
        async Task<bool> RecoverAccount()
        {
            // A local disk failure must be retried before replacing the gameplay binding.
            if (Session.SaveFailure != null && account.CanPlay && !RetrySaving()) return false;
            try
            {
                await account.ReconnectOwnedAsync();
                if (destroyed) return false;
                Session = new RedesignLifecycle(account.BindEngine());
                Changed?.Invoke();
                return true;
            }
            catch (Exception)
            {
                // The account retains its blocked status and durable pending checkpoint.
                if (!destroyed) Changed?.Invoke();
                return false;
            }
        }
        void Update()
        {
            if (NativeAdPresentation)
            {
                if (presentationForeground) account?.Tick(Application.internetReachability != NetworkReachability.NotReachable);
                return;
            }
            account?.Tick(Application.internetReachability != NetworkReachability.NotReachable);
            Session?.Tick(Time.unscaledDeltaTime);
        }
        void OnApplicationPause(bool paused)
        {
            presentationForeground = !paused;
            if (NativeAdPresentation) return;
            if (Session == null) return;
            if (paused)
            {
                Session.Suspend();
                account?.Tick(Application.internetReachability != NetworkReachability.NotReachable);
                account?.Block("Reconnect to resume your account.");
            }
            else if (Session.Resume(account?.ServerNow ?? Utc)) initialOfflinePending = false;
            Changed?.Invoke();
        }
        public bool BeginNativeAdPresentation()
        {
            if (destroyed || NativeAdPresentation || Session == null || !Session.Run(e => e.Save())) return false;
            NativeAdPresentation = true;
            return true;
        }
        public async Task<bool> EndNativeAdPresentationAsync()
        {
            if (!NativeAdPresentation) return false;
            var session = Session;
            try
            {
                // The SDK's close callback may precede Android's foreground callback.
                double deadline = Time.realtimeSinceStartupAsDouble + 30;
                while (!presentationForeground && !destroyed && Time.realtimeSinceStartupAsDouble < deadline) await Task.Delay(50);
                if (destroyed || !presentationForeground || Session != session) return false;
                if (account != null) await account.RenewAfterNativePresentationAsync();
                if (destroyed || Session != session || !session.CanMutate) return false;
                double utc = account?.ServerNow ?? Utc;
                if (utc > Math.Max(session.Engine.State.now, session.Engine.State.savedAt))
                    return session.Run(e => e.ResolveOffline(utc, preservePendingReceipt: true));
                return true;
            }
            catch (Exception)
            {
                account?.Block("Reconnect to continue after the ad.");
                return false;
            }
            finally { NativeAdPresentation = false; if (!destroyed) Changed?.Invoke(); }
        }
        // Android keyboard/focus callbacks are not app suspension. OnApplicationPause owns that boundary.
        void OnApplicationQuit() => Session?.Suspend();
        void OnDestroy()
        {
            destroyed = true;
            Session?.Suspend();
            if (account != null) { account.Changed -= AccountChanged; account.Block("Reconnect to resume your account."); }
        }
    }
}
