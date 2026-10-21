using System;
using System.Linq;
using System.Threading.Tasks;
using RocketRaja.Core;
using TMPro;
using UnityEngine;

namespace RocketRaja.Cloud
{
    public sealed partial class BootstrapController
    {
        bool redesignMode, promoteRedesignGuest;
        RedesignSessionHost redesignHost;
        void StartRedesign()
        {
            popupReturnToShop=redesignHost.AccountMenuRequested;
            if (redesignHost.AccountMenuRequested)
            {
                redesignHost.AccountMenuRequested = false;
                if (redesignHost.Account != null) RedesignAccountMenu(); else RedesignWelcome();
            }
            else if (redesignHost.Account != null) RedesignAccountMenu();
            else if (redesignHost.Mode == "account") Run(() => RedesignGoogle(false));
            else if (redesignHost.Mode == "guest") Run(() => RedesignGuest(false));
            else RedesignWelcome();
        }
        void RedesignWelcome()
        {
            bool legacy = redesignHost.InspectGuest().status == RedesignLoadStatus.LegacyTransitionRequired;
            Show("Welcome to your stall", "Continue your game on this device or choose your Google progress.",
                redesignHost.GoogleSupported ? "Sign in with Google" : "Google sign-in unavailable",
                redesignHost.GoogleSupported ? () => Run(() => RedesignGoogle(true)) : (Action)null,
                "Play as guest", () => Run(() => RedesignGuest(false)),
                legacy ? "Start a separate Classic game…" : null, legacy ? ConfirmSeparateClassicGuest : (Action)null);
            hint.text = "Guest and Google progress stay separate.";
            googleMark.SetActive(redesignHost.GoogleSupported);
            primaryLabel.margin = redesignHost.GoogleSupported ? new Vector4(85, 0, 0, 0) : Vector4.zero;
        }
        void ConfirmSeparateClassicGuest() => Show("Start a separate Classic game?",
            "Your older progress will stay untouched in its original files. This opens a separate Classic profile with normal starting progress. Older progress is not converted or combined.",
            "Start Classic game", () => Run(async () => { await redesignHost.SelectSeparateGuestAsync(); if (this != null) redesignHost.EnterMain(); }),
            "Keep existing progress", RedesignWelcome);
        async Task RedesignGoogle(bool interactive)
        {
            var coordinator = await redesignHost.ConnectAsync(interactive);
            if (this == null) return;
            promoteRedesignGuest = false;
            if (coordinator.Remote != null)
            {
                Show("Choose your progress", "Keep device progress replaces this account's Google save with your device game. Choosing Google keeps your current Google progress.",
                    "Use Google progress", () => Run(ContinueRedesignAccount),
                    "Keep device progress", () => { promoteRedesignGuest = true; Run(ContinueRedesignAccount); },
                    "Stay as guest", () => Run(() => RedesignGuest(false)));
                RedesignSummary(coordinator.GuestState, deviceCash, deviceDetails, deviceTime);
                RedesignSummary(coordinator.CloudState, cloudCash, cloudDetails, cloudTime); comparisonCards.SetActive(true);
                hint.text = "Your chosen progress will be saved to Google. You'll stay signed in.";
            }
            else if (coordinator.CanPromoteGuest && RedesignAccountCoordinator.HasProgress(coordinator.GuestState))
            {
                Show("Save this game with Google?", "Copy your device progress to this new Google game. Your device save stays available separately.",
                    "Use device progress", () => { promoteRedesignGuest = true; Run(ContinueRedesignAccount); },
                    "Start a new Google game", () => Run(ContinueRedesignAccount), "Stay as guest", () => Run(() => RedesignGuest(false)));
            }
            else
            {
                Show("Start your Google game", coordinator.GuestState?.HasDevelopmentShop == true
                    ? "Your test-shop progress stays on this device. Simulated purchases cannot transfer to Google. Start a separate Google game?"
                    : "This account has no redesigned save yet. Start a new Google game while keeping your device files?",
                    "Start Google game", () => Run(ContinueRedesignAccount), "Stay as guest", () => Run(() => RedesignGuest(false)));
            }
        }
        static void RedesignSummary(RedesignState state, TMP_Text cash, TMP_Text detail, TMP_Text time)
        {
            cash.text = state == null ? "Needs recovery" : "₹" + state.cash;
            int products = state?.products.Count(n => n > 0) ?? 0;
            detail.text = state == null ? "Original files kept" : products + (products == 1 ? " product · " : " products · ") + state.completedPrestiges + (state.completedPrestiges.Value == 1 ? " prestige" : " prestiges");
            time.text = state == null ? "" : "Saved " + DateTimeOffset.FromUnixTimeSeconds((long)Math.Max(0, Math.Min(state.savedAt, 253402300799))).ToLocalTime().ToString("g");
        }
        async Task ContinueRedesignAccount()
        {
            if (redesignHost.Coordinator.OtherDevice)
            {
                Show("Playing on another device", "Move your redesigned game here. The other device will finish saving and pause.",
                    "Play here", () => Run(() => EnterRedesignAccount(true)), "Return to guest", () => Run(() => RedesignGuest(false)));
                return;
            }
            await EnterRedesignAccount(false);
        }
        async Task EnterRedesignAccount(bool handoff)
        {
            if (handoff)
            {
                heading.text = "Moving your game here"; description.text = "Waiting for the other device to save or its session to expire.";
                progress.text = "Moving your saved progress…";
            }
            await redesignHost.EnterAccountAsync(promoteRedesignGuest, handoff);
            if (this != null) redesignHost.EnterMain();
        }
        void RedesignAccountMenu()
        {
            Show("Your Google account", (redesignHost.Coordinator.Profile?.displayName ?? "Google player") + "\n" + redesignHost.Account.Status,
                "Sign out", () => Run(() => RedesignGuest(true)), "Back to my shop", () => Run(RedesignReconnect),
                "Return to guest", () => Run(() => RedesignGuest(false)));
            hint.text = "Your local guest and Google progress remain separate.";
        }
        async Task RedesignReconnect() { await redesignHost.ReconnectAsync(); if (this != null) redesignHost.EnterMain(); }
        async Task RedesignGuest(bool flush) { await redesignHost.ReturnToGuestAsync(flush); if (this != null) redesignHost.EnterMain(); }
        void RedesignError(string message) => Show("Your Google game is paused", message,
            "Retry connection", () => Run(redesignHost.Account != null ? RedesignReconnect : () => RedesignGoogle(false)),
            "Return to guest", () => Run(() => RedesignGuest(false)));
    }
}
