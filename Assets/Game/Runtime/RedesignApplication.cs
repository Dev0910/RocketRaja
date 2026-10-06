using System;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>Main startup owns settlement and recovery; utility overlays never suspend the simulation.</summary>
    public sealed class RedesignApplication : MonoBehaviour
    {
        public RedesignScreenRouter router;
        public RedesignRetainedUi retained;
        public bool runOnStart;
        bool started;
        RedesignLifecycle bound;
        string shownReceipt;
        public bool Ready => started && router.driver.Initialized && router.driver.Session.CanMutate;
        void Start()
        {
            if (!runOnStart) return;
            string root = null;
#if UNITY_EDITOR
            root = UnityEditor.SessionState.GetString("RocketRaja.RedesignMainQA.profile", "");
            if (string.IsNullOrEmpty(root)) root = null;
#endif
            var host = Cloud.RedesignSessionHost.Instance;
            if (!GameController.SmokeMode && host)
            {
                if (host.Account != null)
                {
                    if (!host.Ready || !host.Account.CanPlay) { host.OpenAccountMenu(); return; }
                    InitializeAccount(host.Account); return;
                }
                if (root == null) root = host.ProfileRoot;
            }
            Initialize(root);
        }
        public void Initialize(string root = null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (root == null && GameController.SmokeMode) root = RedesignSmokeProfile.Create();
#endif
            Prepare();
            router.driver.InitializeGuest(root); Observe();
        }
        public void InitializeAccount(RedesignAccountSession account)
        {
            Prepare();
            router.driver.InitializeAccount(account); Observe();
        }
        void Prepare()
        {
            if (started) throw new InvalidOperationException("Main session already initialized");
            if (!router || !retained) throw new InvalidOperationException("Main requires the saved router and utility dialogs");
            started = true; Application.targetFrameRate = 60; Screen.orientation = ScreenOrientation.Portrait;
#if UNITY_ANDROID && !UNITY_EDITOR
            Input.backButtonLeavesApp = false;
#endif
            retained.Bind(this); router.BackRequested = Back;
            router.ExternalNavigate = (route, _) => { if (route == "settings") retained.OpenSettings(); else router.Notice("This destination is unavailable in this development build."); };
            router.driver.Changed += Observe;
        }
        public async void Retry() { await router.driver.RetryLoadingAsync(); if (this) Observe(); }
        public void OpenAccount()
        {
            if (!Cloud.RedesignSessionHost.AccountFlowAvailable) return;
            var session = router.driver.Session;
            if (session != null)
            {
                bool saved = session.Suspend();
                if (!saved && (session.SaveFailure != null || !router.driver.IsAccount))
                { retained.ShowRecovery(router.driver.RecoveryNotice ?? "Saving needs attention before leaving your game."); return; }
            }
            Cloud.RedesignSessionHost.Ensure(router.driver.GuestProfileRoot).OpenAccountMenu();
        }
        bool Back()
        {
            if (retained.Back()) return true;
            if (router.CurrentRoute == "upgrades" && router.upgrades.search.isFocused) { router.upgrades.search.DeactivateInputField(); return true; }
            if (router.CloseGuidanceReader()) return true;
            if (router.CurrentRoute != "home") return false;
            GameView.BackgroundTask(); return true;
        }
        void Update() { if (started) Observe(); }
        void OnDestroy() { if (router && router.driver) router.driver.Changed -= Observe; }
        void Observe()
        {
            if (!started) return;
            var driver = router.driver; var session = driver.Session;
            if (session?.Suspended == true) return;
            if (!Ready)
            {
                retained.ShowRecovery(driver.RecoveryNotice ?? "Your save needs attention before you can continue."); return;
            }
            if (bound != session) { router.Bind(session, true); bound = session; shownReceipt = null; }
            if (retained.Page == "recovery") retained.Close();
            if (session.Engine.State.reveal.pending || retained.IsOpen) return;
            var receipt = session.Engine.State.offline;
            if (receipt.pending && receipt.id != shownReceipt) { shownReceipt = receipt.id; router.Navigate("offline", ""); }
        }
    }
}
