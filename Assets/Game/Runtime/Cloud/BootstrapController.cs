using System;
using System.Threading.Tasks;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Cloud
{
    /// <summary>Behavior for the separately reviewed, editor-authored startup canvas. Creates no UI at runtime.</summary>
    public sealed class BootstrapController : MonoBehaviour
    {
        [SerializeField] TMP_Text heading, description, progress, comparison;
        [SerializeField] Button primaryButton, secondaryButton, tertiaryButton;
        [SerializeField] TMP_Text primaryLabel, secondaryLabel, tertiaryLabel;
        [SerializeField] TMP_Text hint, deviceCash, deviceDetails, deviceTime, cloudCash, cloudDetails, cloudTime;
        [SerializeField] GameObject comparisonCards, loadingGroup, googleMark;
        [SerializeField] RectTransform spinner;
        [SerializeField] GameObject footer;
        PlayerSessionHost host;
        Action primary, secondary, tertiary;
        bool busy, useGuest, selectingProgress, googleInteractive;
        float busyStarted;
        void Awake()
        {
            host = PlayerSessionHost.Ensure();
            Application.targetFrameRate = 60; Screen.orientation = ScreenOrientation.Portrait;
            if (heading == null || description == null || progress == null || comparison == null ||
                primaryButton == null || secondaryButton == null || tertiaryButton == null || primaryLabel == null || secondaryLabel == null || tertiaryLabel == null ||
                hint == null || comparisonCards == null || loadingGroup == null || googleMark == null || spinner == null || footer == null ||
                deviceCash == null || deviceDetails == null || deviceTime == null || cloudCash == null || cloudDetails == null || cloudTime == null)
            { Debug.LogError("Bootstrap requires its reviewed, editor-authored canvas references.", this); enabled = false; return; }
            primaryButton.onClick.AddListener(Primary); secondaryButton.onClick.AddListener(Secondary); tertiaryButton.onClick.AddListener(Tertiary);
        }
        void Start()
        {
#if UNITY_EDITOR
            if (UnityEditor.SessionState.GetBool("RocketRaja.BootstrapPreview", false)) return;
            if (GameController.SmokeMode && UnityEditor.SessionState.GetBool("RocketRaja.BootstrapButtonSmoke", false)) { Welcome(); return; }
#endif
            if (GameController.SmokeMode) { host.LoadGuest(true); host.EnterMain(); return; }
            host.InspectGuest();
            if (host.AccountMenuRequested)
            {
                host.AccountMenuRequested = false;
                if (host.Account != null)
                    Show("Your Google account", (host.Profile?.displayName ?? "Google player") + "\n" + host.Account.Status,
                        "Sign out", () => Run(SignOut), host.Account.CanPlay ? "Back to my shop" : "Reconnect", () => { if (host.Account.CanPlay) host.EnterMain(); else Run(() => Google(false)); });
                else Welcome();
                return;
            }
            if (!string.IsNullOrEmpty(host.StartupNotice) && !host.StartupNotice.StartsWith("Syncing"))
            { Error(host.StartupNotice); return; }
            if (LocalProfiles.Mode == "account" && host.GoogleSupported) Run(() => Google(false));
            else if (LocalProfiles.Mode == "guest" || !host.GoogleSupported) Guest();
            else Welcome();
        }
        void Welcome()
        {
            Show("Welcome to your stall", "Save your progress with Google and continue on another device.",
                host.GoogleSupported ? "Sign in with Google" : "Google login requires Android", host.GoogleSupported ? () => Run(() => Google(true)) : (Action)null,
                "Play as guest", Guest);
            hint.text = "You can connect Google later in Settings.";
            googleMark.SetActive(host.GoogleSupported);
            primaryLabel.margin = host.GoogleSupported ? new Vector4(85, 0, 0, 0) : Vector4.zero;
        }
        void Show(string title, string detail, string a, Action aa, string b = null, Action bb = null, string c = null, Action cc = null)
        {
            if (this == null) return;
            heading.text = title; description.text = detail; progress.text = ""; comparison.text = "";
            hint.text = "Guest progress stays separate from your Google save.";
            comparisonCards.SetActive(false); loadingGroup.SetActive(false); googleMark.SetActive(false);
            primaryLabel.margin = Vector4.zero;
            footer.SetActive(c == null);
            primary = aa; secondary = bb; tertiary = cc;
            primaryLabel.text = a ?? ""; secondaryLabel.text = b ?? ""; tertiaryLabel.text = c ?? "";
            primaryButton.gameObject.SetActive(a != null); secondaryButton.gameObject.SetActive(b != null); tertiaryButton.gameObject.SetActive(c != null);
            primaryButton.interactable = aa != null && !busy; secondaryButton.interactable = bb != null && !busy; tertiaryButton.interactable = cc != null && !busy;
        }
        async void Run(Func<Task> work)
        {
            if (busy) return;
            busy = true; busyStarted = Time.realtimeSinceStartup;
            Show("Getting your stall ready", "Restoring your saved progress…", null, null);
            loadingGroup.SetActive(true); progress.text = "Loading your progress…";
            spinner.localRotation = Quaternion.identity;
            try { await work(); }
            catch (OperationCanceledException)
            {
                // Credential Manager can report cancellation for a rejected Android OAuth
                // configuration too. Do not silently make the button appear to do nothing.
                if (this != null)
                {
                    Welcome();
                    description.text = "Google sign-in did not finish. Please try again, or play as guest.";
                }
                Debug.LogWarning("Google sign-in did not finish. If account selection was completed, verify this APK's package name and signing SHA-1 in Firebase.");
            }
            catch (Exception ex)
            {
                if (this != null) Error(ex is CloudConflictException || ex is NotSupportedException || ex is System.IO.InvalidDataException
                    ? ex.Message : "We could not connect to your Google save. Check your connection and try again.");
                Debug.LogWarning("Cloud startup failed (" + ex.GetType().Name + "). Check the Firebase setup and connection.");
            }
            finally
            {
                busy = false;
                if (this != null) { primaryButton.interactable = primary != null; secondaryButton.interactable = secondary != null; tertiaryButton.interactable = tertiary != null; }
            }
        }
        async Task Google(bool interactive)
        {
            googleInteractive = interactive;
            description.text = "Restoring your Google save before opening your shop.";
            progress.text = "Syncing your progress…"; hint.text = "Please keep your connection on.";
            await host.PrepareGoogleAsync(interactive); useGuest = false;
            if (this == null) return;
            if (host.AskGuestChoice) { ChooseProgress(); return; }
            await ContinueAccount();
        }
        void ChooseProgress()
        {
            selectingProgress = true;
            Show("Choose your progress", "Keep your device progress or your Google save. These saves will not be combined.",
                "Use Google progress", () => { useGuest = false; Run(ContinueAccount); },
                "Use device progress…", ConfirmReplacement, "Return to guest", Guest);
            var cloudState = JsonUtility.FromJson<GameState>(host.Remote.save.json);
            SetSummary(host.GuestState, deviceCash, deviceDetails, deviceTime);
            SetSummary(cloudState, cloudCash, cloudDetails, cloudTime);
            comparisonCards.SetActive(true);
            hint.text = "A recovery copy is kept before a save is replaced.";
        }
        static void SetSummary(GameState s, TMP_Text cash, TMP_Text detail, TMP_Text time)
        {
            cash.text = s == null ? "Unavailable" : "₹" + s.cash;
            detail.text = s == null ? "" : s.OwnedProducts + " products · " + s.prestiges + " prestiges";
            time.text = s == null ? "" : "Saved " + DateTimeOffset.FromUnixTimeSeconds((long)Math.Max(0, Math.Min(s.savedAt, 253402300799))).ToLocalTime().ToString("g");
        }
        static string Summary(string label, GameState s) => s == null ? label + ": unavailable" :
            label + " · ₹" + s.cash + "\n" + s.OwnedProducts + " products · " + s.prestiges + " prestiges\nSaved " + DateTimeOffset.FromUnixTimeSeconds((long)Math.Min(s.savedAt, 253402300799)).ToLocalTime().ToString("g");
        void ConfirmReplacement() => Show("Replace Google progress?", "Your device progress will become this account's save. A recovery copy of each save stays on this device.",
            "Use device progress", () => { useGuest = true; Run(ContinueAccount); }, "Back", ChooseProgress);
        async Task ContinueAccount()
        {
            if (host.OtherDevice)
            {
                Show("Playing on another device", "Move your game here to continue. Your other device will pause after saving.",
                    "Play here", () => Run(() => Enter(true)), "Return to guest", Guest);
                return;
            }
            await Enter(false);
        }
        async Task Enter(bool transfer)
        {
            if (transfer)
            {
                heading.text = "Moving your game here";
                description.text = "Your other device is finishing its save. We’ll open the latest saved progress here.";
                progress.text = "Waiting for the other device…";
                hint.text = "If it cannot connect, we’ll wait for its session to expire.";
            }
            await host.EnterAccountAsync(useGuest, transfer);
            if (this != null) host.EnterMain();
        }
        void Guest()
        {
            if (busy) return;
            Run(async () =>
            {
                await host.ReturnToGuestAsync(false);
                if (this == null) return;
                if (host.GuestNeedsRecovery) Recovery(); else host.EnterMain();
            });
        }
        void Recovery() => Show("Save needs attention", "We could not read your device save or its backup. Your original files are preserved.",
            "Try again", Guest, "Sign in with Google", () => Run(() => Google(true)), "Start a new guest game…", () =>
                Show("Start a new guest game?", "Your unreadable files will be kept for recovery. This starts fresh progress on this device.",
                    "Start fresh", () => { host.StartFreshGuest(); host.EnterMain(); }, "Back", Recovery));
        void Error(string message) => Show("Your Google game is paused", message,
            "Retry connection", () => Run(() => Google(googleInteractive)), "Return to guest", Guest,
            selectingProgress && host.AskGuestChoice ? "Compare saves again" : null, ChooseProgress);
        async Task SignOut() { await host.ReturnToGuestAsync(true); if (this != null) { if (host.GuestNeedsRecovery) Recovery(); else host.EnterMain(); } }
        void Update()
        {
            if (busy && Time.realtimeSinceStartup - busyStarted > 15) progress.text = "Still connecting… Your saved progress is protected.";
            if (loadingGroup.activeSelf && !(host?.Session?.State?.reducedMotion ?? host?.GuestState?.reducedMotion ?? false))
                spinner.Rotate(0, 0, -300 * Time.unscaledDeltaTime);
        }
        void Primary() { if (!busy) primary?.Invoke(); }
        void Secondary() { if (!busy) secondary?.Invoke(); }
        void Tertiary() { if (!busy) tertiary?.Invoke(); }
        void OnDestroy()
        {
            if (primaryButton != null) primaryButton.onClick.RemoveListener(Primary);
            if (secondaryButton != null) secondaryButton.onClick.RemoveListener(Secondary);
            if (tertiaryButton != null) tertiaryButton.onClick.RemoveListener(Tertiary);
        }
#if UNITY_EDITOR
        // Presentation fixtures used only by the Editor validation tool. Never authenticates or saves.
        public void PreviewState(string state)
        {
            Action noop = () => { };
            Show("Welcome to your stall", "Save your progress with Google and continue on another device.", "Sign in with Google", noop, "Play as guest", noop);
            hint.text = "You can connect Google later in Settings."; googleMark.SetActive(true); primaryLabel.margin = new Vector4(85,0,0,0);
            if (state == "welcome") return;
            if (state == "loading" || state == "syncing" || state == "handoff")
            {
                Show(state == "handoff" ? "Moving your game here" : "Getting your stall ready", state == "loading" ? "Checking your device save and preparing your shop." : state == "handoff" ? "Your other device is finishing its save. We’ll open the latest saved progress here." : "Restoring your Google save before opening your shop.", null, null);
                loadingGroup.SetActive(true); progress.text = state == "handoff" ? "Waiting for the other device…" : "Syncing your progress…";
            }
            else if (state == "conflict")
            {
                Show("Choose your progress", "Keep your device progress or your Google save. These saves will not be combined.", "Use Google progress", noop, "Use device progress…", noop, "Return to guest", noop);
                comparisonCards.SetActive(true); deviceCash.text = "₹1.24M"; cloudCash.text = "₹8.60M";
                deviceDetails.text = "6 products · 1 prestige"; cloudDetails.text = "8 products · 3 prestiges";
                deviceTime.text = "Saved just now"; cloudTime.text = "Saved 2 hours ago";
                hint.text = "A recovery copy is kept before a save is replaced.";
            }
            else if (state == "replace") Show("Replace Google progress?", "Your device progress will become this account’s save. A recovery copy of each save stays on this device.", "Use device progress", noop, "Back", noop);
            else if (state == "device") Show("Playing on another device", "Move your game here to continue with the latest saved progress.", "Play here", noop, "Return to guest", noop);
            else if (state == "offline") Show("Waiting for a connection", "Your Google game is paused. Reconnect to save safely and continue.", "Retry connection", noop, "Return to guest", noop);
            else if (state == "error") Show("Could not sign in", "We could not connect to your Google save. Check your connection and try again.", "Try Google again", noop, "Play as guest", noop);
            else if (state == "recovery") Show("Save needs attention", "We could not read your device save or its backup. Your original files are preserved.", "Try again", noop, "Sign in with Google", noop, "Start a new guest game…", noop);
            else if (state == "fresh") Show("Start a new guest game?", "Your unreadable files will be kept for recovery. This starts fresh progress on this device.", "Start fresh", noop, "Back", noop);
            else if (state == "account") Show("Your Google account", "Raja · Saved", "Sign out", noop, "Back to my shop", noop);
        }
#endif
    }
}
