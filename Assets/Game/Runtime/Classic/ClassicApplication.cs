using System;
using RocketRaja.Cloud;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Classic
{
    /// <summary>Binds saved Classic views to the selected session. The driver alone saves and settles time.</summary>
    [DefaultExecutionOrder(-40)]
    public sealed class ClassicApplication : MonoBehaviour
    {
        public ClassicUI ui;
        RedesignLifecycle bound;
        bool started, leaving, foreground = true;
        string failure, shownReceipt;
        public bool Ready => started && !leaving && ui.driver.Initialized && ui.driver.Session.CanMutate;

        void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
#if UNITY_ANDROID && !UNITY_EDITOR
            Input.backButtonLeavesApp = false;
#endif
            var host = RedesignSessionHost.Instance;
            if (!host || !host.Ready)
            {
                leaving = true;
                SceneManager.LoadSceneAsync("Bootstrap");
                return;
            }
            started = true;
            ui.driver.Changed += Observe;
            try
            {
                if (host.Account != null) ui.driver.InitializeAccount(host.Account);
                else ui.driver.InitializeGuest(host.ProfileRoot);
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                failure = "Your progress could not be loaded. Existing files are preserved. Retry or open account options.";
                Debug.LogWarning("Classic startup needs recovery: " + ex.GetType().Name);
            }
            Observe();
        }

        void Update() { if (foreground) Observe(); }
        void OnApplicationPause(bool paused) { foreground = !paused; }
        void OnDestroy() { if (ui && ui.driver) ui.driver.Changed -= Observe; }

        public async void Retry()
        {
            if (!started || leaving || ui.driver.Recovering) return;
            failure = null;
            try { await ui.driver.RetryLoadingAsync(); }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
            { failure = "Recovery is still unavailable. Your existing files are preserved."; }
            if (this) Observe();
        }

        public void OpenAccount()
        {
            if (leaving || ui.driver.Recovering) return;
            var session = ui.driver.Session;
            // A blocked cloud owner can return to Bootstrap to reconnect, but a failed local
            // checkpoint must be saved before destroying its only in-memory copy.
            if (session != null && !session.Suspend() && (session.SaveFailure != null || !ui.driver.IsAccount))
            {
                ui.ShowRecovery(ui.driver.RecoveryNotice ?? "Retry saving before leaving your game.");
                return;
            }
            leaving = true;
            RedesignSessionHost.Ensure().OpenAccountMenu();
        }

        void Observe()
        {
            if (!started || leaving || !foreground || ui.driver.NativeAdPresentation) return;
            if (!Ready)
            {
                ui.ShowRecovery(failure ?? ui.driver.RecoveryNotice ?? "Your progress needs attention. Retry before continuing.");
                return;
            }
            if (bound != ui.driver.Session)
            {
                bound = ui.driver.Session;
                shownReceipt = null;
                ui.Bind();
                ui.InvalidateQuotes();
            }
            if (ui.CurrentDialog == "SaveRecovery") ui.ReturnFromRecovery();
            var state = bound.Engine.State;
            if (state.reveal.pending)
            {
                if (ui.CurrentDialog == "") ui.Open("DrawReveal");
                return;
            }
            if (state.offline.pending && state.offline.id != shownReceipt && ui.CurrentDialog == "")
            {
                shownReceipt = state.offline.id;
                ui.Open("Offline");
            }
        }
    }
}
