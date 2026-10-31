using System;
using System.Collections;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Cloud
{
    public sealed partial class BootstrapController
    {
        RedesignSessionHost redesignHost;
        void StartRedesign() => StartCoroutine(LoadLocal());
        IEnumerator LoadLocal()
        {
            if (busy) yield break;
            LoadingFailed = false; LoadingProgress = 0;
            Show("Getting your stall ready", "");
            busy = true;
            if (loadingPresentation) { loadingPresentation.SetActive(true); if (popupPanel) popupPanel.SetActive(false); if (popupBackdrop) popupBackdrop.SetActive(false); }
            if (loadingGroup) loadingGroup.SetActive(true);
            RefreshPopupClose();
            SetLoading(.05f, "Preparing your device save…");
            yield return null; // Render the loading presentation before local I/O.
            var result = redesignHost.InspectGuest();
            if (result.status == RedesignLoadStatus.LegacyTransitionRequired)
            {
                Show("Older progress found", "Your older game is preserved. Classic uses a separate profile and does not convert or combine that progress.",
                    "Start a separate Classic game…", ConfirmSeparateClassicGuest, "Keep files and leave", GameView.BackgroundTask);
                yield break;
            }
            // Corrupt, future-version and unreadable saves go to Main's existing protected recovery flow.
            var preparation = redesignHost.ReturnToGuestAsync(false);
            while (!preparation.IsCompleted) yield return null;
            if (preparation.IsFaulted) { StartupError(); yield break; }
            SetLoading(.2f, "Opening your stall…");
            var canvas = GetComponent<Canvas>();
            if (canvas) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32000; }
            DontDestroyOnLoad(gameObject);
            var operation = redesignHost.EnterMain();
            if (operation == null) { StartupError(); yield break; }
            while (!operation.isDone)
            {
                SetLoading(.2f + .7f * Mathf.Clamp01(operation.progress / .9f), "Opening your stall…");
                yield return null;
            }
            var application = FindFirstObjectByType<Classic.ClassicApplication>();
            while (application && !application.StartupAttempted) yield return null;
            if (application && application.Ready)
            {
                SetLoading(1, "Ready");
                yield return null;
            }
            // On failure reveal the existing Main recovery dialog without claiming loading completed.
            gameObject.SetActive(false);
        }
        void ConfirmSeparateClassicGuest() => Show("Start a separate Classic game?",
            "Your older progress stays untouched in its original files. This starts a separate Classic profile with normal starting progress.",
            "Start Classic game", SelectSeparateClassicGuest, "Keep existing progress", StartRedesign);
        async void SelectSeparateClassicGuest()
        {
            if (busy) return;
            busy = true;
            try { await redesignHost.SelectSeparateGuestAsync(); if (this) { busy = false; StartRedesign(); } }
            catch (Exception error) when (error is System.IO.IOException || error is UnauthorizedAccessException || error is InvalidOperationException)
            { if (this) StartupError(); }
        }
        void StartupError()
        {
            LoadingFailed = true;
            Show("Save needs attention", "Your existing files are preserved. Retry before continuing.", "Retry", StartRedesign, "Keep files and leave", GameView.BackgroundTask);
        }
    }
}
