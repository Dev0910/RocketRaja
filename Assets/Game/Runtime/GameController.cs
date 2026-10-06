using System;
using System.IO;
using System.Collections.Generic;
using RocketRaja.Core;
using UnityEngine;
using RocketRaja.Cloud;

namespace RocketRaja
{
    public sealed class GameController : MonoBehaviour
    {
        public RedesignApplication redesignApplication;
        [SerializeField] ImportedPrivacyScreen privacyScreen;
        public ImportedPrivacyScreen PrivacyScreen => privacyScreen;
        [SerializeField] ImportedSettingsScreen settingsScreen;
        public ImportedSettingsScreen SettingsScreen => settingsScreen;
        [SerializeField] ImportedSaveRecoveryScreen saveRecoveryScreen;
        public ImportedSaveRecoveryScreen SaveRecoveryScreen => saveRecoveryScreen;
        [SerializeField] ImportedOfflineScreen offlineScreen;
        public ImportedOfflineScreen OfflineScreen => offlineScreen;
        [SerializeField] ImportedSponsorScreen sponsorScreen;
        public ImportedSponsorScreen SponsorScreen => sponsorScreen;
        [SerializeField] ImportedPrestigeScreen prestigeScreen;
        public ImportedPrestigeScreen PrestigeScreen => prestigeScreen;
        [SerializeField] ImportedStatisticsScreen statisticsScreen;
        public ImportedStatisticsScreen StatisticsScreen => statisticsScreen;
        [SerializeField] ImportedSkillsScreen skillsScreen;
        public ImportedSkillsScreen SkillsScreen => skillsScreen;
        [SerializeField] ImportedProductDetailScreen productDetailScreen;
        public ImportedProductDetailScreen ProductDetailScreen => productDetailScreen;
        [SerializeField] StoreNameDialog storeNameScreen;
        public StoreNameDialog StoreNameScreen => storeNameScreen;
        public bool StartupReady { get; private set; }
        [SerializeField] ImportedHomeScreen homeScreen;
        public ImportedHomeScreen HomeScreen => homeScreen;
        [SerializeField] ImportedPlayerScreen playerScreen;
        public ImportedPlayerScreen PlayerScreen => playerScreen;
        [SerializeField] ImportedProductsScreen productsScreen;
        public ImportedProductsScreen ProductsScreen => productsScreen;
        [SerializeField] ImportedLegacyScreen legacyScreen;
        public ImportedLegacyScreen LegacyScreen => legacyScreen;
        public GameEngine Engine { get; private set; }
        public GameView View { get; private set; }
        public IRewardedAds Ads { get; } = new DisabledRewardedAds();
        public string SaveWarning { get; private set; }
        IPlayerSession session;
        public bool CanPlay => session != null && session.CanPlay && !saveBlocked;
        public bool IsAdShowing => false;
        public double SessionUtc => (session as AccountSession)?.ServerNow ?? Utc;
        double accumulator, saveTimer;
        bool suspended, modalPaused, saveBlocked;
        readonly HashSet<UnityEngine.Object> modalOwners = new HashSet<UnityEngine.Object>();
        public void HoldModal(UnityEngine.Object owner) { modalOwners.Add(owner); ModalPaused = true; }
        public void ReleaseModal(UnityEngine.Object owner) { modalOwners.Remove(owner); ModalPaused = modalOwners.Count > 0; }
        public static double Utc => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
        public static bool SmokeMode
        {
            get
            {
#if UNITY_EDITOR
                if (UnityEditor.SessionState.GetBool("RocketRaja.EditorSmoke", false)) return true;
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Debug.isDebugBuild && RedesignSmokeProfile.AndroidRequested) return true;
#endif
                return Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(), "--smoke") >= 0;
            }
        }
        public bool SaveNeedsRecovery => saveBlocked;
        public bool ModalPaused { get => modalPaused; set { if (modalPaused && !value && Engine != null && CanPlay) { Engine.State.now = Math.Max(Engine.State.now, SessionUtc); Engine.State.savedAt = Engine.State.now; accumulator = 0; } modalPaused = value; } }

        void Awake()
        {
            if (redesignApplication)
            {
                View = GetComponent<GameView>(); View.InitializeRedesign(redesignApplication.router);
                enabled = false; return;
            }
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            var host = PlayerSessionHost.Ensure();
            if (SmokeMode && !host.Ready) host.LoadGuest(true);
            // Direct Main play follows the same prepared-session boundary as player startup.
            if (!host.Ready)
            {
                enabled = false;
                host.OpenStartup();
                return;
            }
            session = host.Session;
            var guest = session as GuestSession;
            SaveWarning = guest?.Warning;
            saveBlocked = guest?.NeedsRecovery ?? false;
            BindEngine();
            string recoveryNotice = SaveWarning;
            if (!session.IsAccount) Engine.ResolveOffline(Utc);
            if (recoveryNotice != null) SaveWarning = recoveryNotice;
            View = GetComponent<GameView>();
            if (View == null)
            {
                Debug.LogError("Main Game object is missing its editor-authored GameView component.", this);
                enabled = false;
                return;
            }
            if (!View.Initialize(this))
            {
                enabled = false;
                return;
            }
            gameObject.AddComponent<ShopAudio>().Initialize(this);
        }
        void BindEngine()
        {
            if (Engine != null) Engine.CriticalSave -= Save;
            Engine = new GameEngine(PlayerSessionHost.Ensure().Catalog, session.State);
            Engine.MutationAllowed = () => CanPlay && !suspended;
            Engine.CriticalSave += Save;
        }
        void Start()
        {
            if (redesignApplication) return;
            if (saveBlocked) View.ShowSaveRecovery();
            else if (SaveWarning != null) View.Notice(SaveWarning);
            else if (!string.IsNullOrEmpty(Engine.State.pendingOfflineId)) View.ShowOffline();
            StartupReady = true;
        }
        void Update()
        {
            if (!CanPlay || suspended || modalPaused || saveBlocked) return;
            accumulator += Time.unscaledDeltaTime;
            if (accumulator >= .1) { Engine.Advance(accumulator); accumulator = 0; }
            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= 10) { Save(); saveTimer = 0; }
        }
        public void Tap()
        {
            if (!CanPlay || suspended || modalPaused || saveBlocked) return;
            Engine.Tap(); View.TapFeedback();
            if (Engine.State.onboarding == 0)
            {
                Engine.State.onboarding = 1;
                Save();
                View.RefreshNow();
            }
        }
        public void Save()
        {
            if (Engine == null || saveBlocked) return;
            try { session.Save(true); SaveWarning = null; }
            catch (Exception ex) { SaveWarning = "Saving failed. Please check device storage."; View?.Notice(SaveWarning); Debug.LogError("Save failed: " + ex.Message); }
        }
        /// <summary>Applies the same purchase, onboarding, save, and feedback from every product entry point.</summary>
        public bool PurchaseProduct(int index, int quantity)
        {
            if (!CanPlay || suspended || Engine == null || View == null ||
                index < 0 || index >= Engine.Data.products.Length || quantity <= 0)
                return false;

            int tier = Engine.VisualTier(index);
            if (!Engine.BuyProduct(index, quantity)) return false;
            if (Engine.State.onboarding < 3) Engine.State.onboarding = 3;
            Save();
            string productName = Engine.Data.products[index].name;
            View.Notice(Engine.VisualTier(index) > tier
                ? productName + " display upgraded!" : productName + " stocked.");
            View.RefreshNow();
            return true;
        }
        public void SetSetting(string setting)
        {
            if (!CanPlay) return;
            if (setting == "music") Engine.State.music = !Engine.State.music;
            if (setting == "sound") Engine.State.sound = !Engine.State.sound;
            if (setting == "motion") Engine.State.reducedMotion = !Engine.State.reducedMotion;
            Save();
        }
        void Suspend()
        {
            if (Engine == null || suspended) return;
            if (!modalPaused && accumulator > 0) { Engine.Advance(accumulator); accumulator = 0; }
            suspended = true;
            Save();
        }
        void Resume()
        {
            if (!suspended || Engine == null) return;
            suspended = false;
            if (session.IsAccount) { PlayerSessionHost.Ensure().OpenStartup("Syncing your account before resuming…"); return; }
            Engine.ResolveOffline(Utc);
            if (!string.IsNullOrEmpty(Engine.State.pendingOfflineId)) View.ShowOffline();
        }
        void OnApplicationPause(bool paused) { if (SmokeMode) return; if (paused) Suspend(); else Resume(); }
        void OnApplicationFocus(bool focused) { if (SmokeMode) return; if (!focused) Suspend(); else Resume(); }
        void OnApplicationQuit() { Save(); Ads?.Dispose(); }
        void OnDestroy() { Ads?.Dispose(); }
        // Compatibility entry point: even a stale event returns rejection synchronously.
        public void ShowAd(string placement, Action<bool> completed = null) => completed?.Invoke(false);
        public bool StartFreshAfterDamagedSave()
        {
            // This action is only offered with explicit in-game confirmation. Damaged files remain archived.
            if (!saveBlocked) return true;
            try { (session as GuestSession)?.StartFresh(); saveBlocked = false; }
            catch { return false; }
            Save();
            if (SaveWarning == null) return true;
            saveBlocked = true;
            return false;
        }
    }
}
