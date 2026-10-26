using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Explicit validation against production Main with a disposable local profile and fake ads.</summary>
    [InitializeOnLoad]
    public static class ClassicOfflineTimelineValidation
    {
        const string Key = "Classic.OfflineTimelineValidation";
        const string Folder = "Documentation/Implementation/OfflineCappedTimeline";
        static readonly List<string> report = new List<string>();
        static readonly List<string> errors = new List<string>();
        public static string Status { get; private set; } = "Not run";
        static ClassicOfflineTimelineValidation() { EditorApplication.playModeStateChanged += Mode; }
        public static string Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorSceneManager.GetSceneManagerSetup().Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty))
                throw new InvalidOperationException("Start from clean Edit Mode");
            var ui = SceneManager.GetSceneByPath("Assets/Scenes/Main.unity").GetRootGameObjects().Single(g => g.name == "ClassicProductionCanvas").GetComponent<ClassicUI>();
            SessionState.SetInt(Key + ".SavedComponents", ui.GetComponentsInChildren<Component>(true).Length);
            Directory.CreateDirectory(Folder);
            SessionState.SetBool(Key, true); EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode(); return Status = "Starting isolated production validation";
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (mode == PlayModeStateChange.EnteredPlayMode) Run();
            if (mode == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Key);
                EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);
            }
        }
        static void Check(bool ok, string label) { report.Add((ok ? "PASS " : "FAIL ") + label); if (!ok) throw new InvalidOperationException(label); }
        static async Task Until(Func<bool> done)
        {
            double end = EditorApplication.timeSinceStartup + 25;
            while (!done()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException("Main did not initialize"); await Task.Delay(50); }
        }
        static async Task Capture(string name) { await Task.Delay(500); ClassicImportQA.Capture(Folder + "/" + name + ".png"); }
        static void Log(string text, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(text); }
        internal sealed class TestAds : IRewardedAds
        {
            public bool Ready { get; set; } = true;
            public bool PrivacyOptionsRequired => false;
            public Action<bool, string> Callback;
            public void Initialize() { } public void Dispose() { } public void Tick() { } public void PrivacyOptions() { }
            public void Show(Action<bool, string> completed) { Callback = completed; }
        }
        static void Click(ClassicUI ui, string action, string arg = "")
        {
            var c = ui.GetComponentsInChildren<ClassicControl>(true).Single(c => c.action == action && c.parameter == arg && c.gameObject.activeInHierarchy);
            Check(c.GetComponent<Button>().IsInteractable(), "Control enabled: " + action);
            c.GetComponent<Button>().onClick.Invoke();
        }
        static async void Run()
        {
            report.Clear(); errors.Clear(); Application.logMessageReceived += Log;
            string root = Path.Combine(Path.GetTempPath(), "OfflineCappedTimeline-" + Guid.NewGuid().ToString("N"));
            try
            {
                var catalog = RedesignRuntimeContent.Load();
                var store = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), catalog, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
                var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc - 36000);
                state.products = new[] { 25, 18, 12, 8, 5, 3, 1, 1 }; state.cash = 1240000; state.tapLevel = 12;
                state.storeName = "DIWALI DUKAAN"; state.storeNamePromptHandled = true;
                new RedesignEngine(catalog, state, store.Save).Save();
                var coordinator = new RedesignAccountCoordinator(new ClassicProductionValidation.FixtureAuth(), new RedesignAccountPlayValidation.FixtureCloud(),
                    catalog, root, "offline-ui-test", JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>,
                    JsonUtility.ToJson, JsonUtility.FromJson<AccountCheckpoint>, () => Time.realtimeSinceStartupAsDouble, () => RedesignRuntimeDriver.Utc);
                var host = RedesignSessionHost.Ensure(); host.ConfigureFixture(coordinator, root);
                await host.ReturnToGuestAsync(false); ClassicImportQA.Viewport(390, 844); host.EnterMain();
                await Until(() => UnityEngine.Object.FindFirstObjectByType<ClassicApplication>()?.Ready == true);
                var ui = UnityEngine.Object.FindFirstObjectByType<ClassicApplication>().ui;
                await Until(() => ui.CurrentDialog == "Offline");
                var view = ui.offlineView; var receipt = ui.Engine.State.offline;
                var timeline = new OfflineTimeline(receipt, 0);
                Check(ui.gameObject.scene.path == "Assets/Scenes/Main.unity" && !ui.isolatedPreview, "Production Main automatically opens saved offline popup on return");
                Check(receipt.pending && timeline.Capped && timeline.Stored == 28800 && Math.Abs(timeline.Overflow - 7200) < 30, "Live return receipt shows actual cap and overflow");
                Check(ui.transform.Find("Local dialogs/Offline__LegacyDisabled_20261025")?.gameObject.activeSelf == false, "Previous complete popup retained inactive");
                var identities = ui.GetComponentsInChildren<Component>(true).Select(c => c.GetInstanceID()).OrderBy(i => i).ToArray();
                Check(identities.Length == SessionState.GetInt(Key + ".SavedComponents", -1), "Every production UI component already exists in the saved Edit Mode hierarchy");
                await Task.Delay(120); ClassicImportQA.Capture(Folder + "/01-opening.png");
                await Capture("02-capped-timeline-390x844");
                Check(Math.Abs(view.timeFill.fillAmount - timeline.Fill) < .001 && Math.Abs(view.efficiencyFill.fillAmount - .5) < .001, "Animated time and efficiency fills settle to independent values");
                Check(!view.watch.interactable && view.adStatus.text.Contains("unavailable"), "Shipping build gracefully disables ads without a provider");
                ui.Handle("OfflineAd", "watch"); Check(!receipt.adRewarded, "Unavailable ad cannot award cash");
                Click(ui, "OfflineInfo"); Check(ui.CurrentDialog == "Guidance", "Header info opens offline explanation");
                ui.Close(); await Task.Delay(400); Check(ui.CurrentDialog == "Offline", "Information closes back to the offline popup");
                var ads = new TestAds(); ui.SetOfflineAdProvider(ads);
                Click(ui, "OfflineAd", "watch"); Check(!view.watch.interactable && view.watchLabel.text.Contains("Watching"), "Pending ad disables repeat starts");
                ads.Callback(false, null); Check(!ui.Engine.State.offline.adRewarded && view.watch.interactable, "Cancelled test ad keeps base reward and permits retry");
                Click(ui, "OfflineAd", "watch"); var callback = ads.Callback;
                var before = ui.Engine.State.cash; ui.driver.Session.GameplayInputEnabled = false;
                callback(true, null); var after = ui.Engine.State.cash;
                Check(ui.Engine.State.offline.adRewarded && after.CompareTo(before + receipt.amount) == 0, "Completed test-provider callback saves one extra reward");
                callback(true, null); Check(ui.Engine.State.cash.CompareTo(after) == 0 && !view.watch.interactable, "Duplicate test callback cannot grant again");
                await Capture("03-ad-bonus-collected");
                string tab = ui.CurrentTab; Click(ui, "Offline", "close"); await Task.Delay(400);
                Check(ui.CurrentDialog == "" && ui.CurrentTab == tab && !ui.Engine.State.offline.pending, "Continue acknowledges the receipt and closes without navigation");
                Check(store.Load().state.offline.adRewarded, "Completed bonus survives a save reload");

                // Fresh receipts for cancellation and responsive presentation. Time advances only in this disposable profile.
                ui.driver.Command(e => e.ResolveOffline(e.State.now + 36000)); ui.Open("Offline");
                Click(ui, "OfflineAd", "watch"); callback = ads.Callback;
                ui.Close(); await Task.Delay(400); before = ui.Engine.State.cash; callback(true, null);
                Check(ui.Engine.State.cash.CompareTo(before) == 0 && !ui.Engine.State.offline.adRewarded, "Close invalidates an in-flight ad and ignores its late callback");
                ui.driver.Command(e => e.ResolveOffline(e.State.now + 36000)); ui.Open("Offline"); ui.SetOfflineAdProvider(null);
                foreach (var size in new[] { new Vector2Int(360, 640), new Vector2Int(360, 540), new Vector2Int(390, 844) })
                {
                    ClassicImportQA.Viewport(size.x, size.y); await Capture("04-capped-" + size.x + "x" + size.y);
                    var popup = ui.dialogs.Single(v => v.route == "Offline");
                    var proceed = popup.window.GetComponentsInChildren<ClassicControl>().Single(c => c.action == "Offline");
                    Check(((RectTransform)proceed.transform).rect.height >= 48 && popup.scroll.viewport.rect.height > 120, "Fixed actions and scrollable content at " + size);
                }
                var safe = (RectTransform)ui.transform.Find("SafeArea");
                var safeAdapter = safe.GetComponent<ImportedSafeArea>(); safeAdapter.enabled = false;
                safe.anchorMin = new Vector2(0, 34f / 844); safe.anchorMax = new Vector2(1, 1 - 44f / 844);
                await Capture("07-notched-safe-area");
                var window = ui.dialogs.Single(v => v.route == "Offline").window;
                var corners = new Vector3[4]; window.GetWorldCorners(corners);
                Check(corners[0].y >= 34 && corners[2].y <= 844 - 44, "Popup stays inside simulated top and bottom device insets");
                safeAdapter.enabled = true;
                ui.Close(); await Task.Delay(400);
                ui.driver.Command(e => e.ResolveOffline(e.State.now + 3600)); ui.Open("Offline"); await Capture("05-under-cap");
                Check(view.status.text == "ROOM TO EARN" && view.overflow.text.Contains("remaining"), "Under-cap state shows remaining storage with no overflow claim");
                ui.Close(); await Task.Delay(400);
                // Card levels only seeded in the disposable test profile, never in production files.
                ui.Engine.State.cardLevels[24] = 5; ui.Engine.State.cardLevels[25] = 5;
                ui.driver.Command(e => e.ResolveOffline(e.State.now + 72000)); ui.Open("Offline"); await Capture("06-card-improved-cap");
                Check(ui.Engine.State.offline.capSeconds == 50400 && view.efficiency.text == "75% efficiency", "Talent card levels update the native cap and efficiency from game rules");
                ui.Handle("Setting", "motion"); ui.Close(); await Task.Delay(100); ui.Open("Offline");
                Check(view.timeFill.fillAmount == (float)new OfflineTimeline(ui.Engine.State.offline, 5).Fill, "Reduced motion retains the settled fill");
                var final = ui.GetComponentsInChildren<Component>(true).Select(c => c.GetInstanceID()).OrderBy(i => i).ToArray();
                Check(identities.SequenceEqual(final), "No UI objects or components are created through repeated show, dismiss, ad and info cycles");
                Check(errors.Count == 0, "No runtime errors during production-route validation");
                report.Add("This regression suite explicitly uses fake providers and bypasses automatic SDK initialization. The Google SDK adapter is validated separately.");
                report.Add("Disposable seeded profile: " + root);
                Status = "PASS: " + report.Count(r => r.StartsWith("PASS")) + " native offline checks";
            }
            catch (Exception e) { Status = "FAIL: " + e.Message; report.Add(e.ToString()); report.AddRange(errors); }
            finally
            {
                Application.logMessageReceived -= Log; report.Add(Status); File.WriteAllLines(Folder + "/Validation.txt", report);
                EditorApplication.ExitPlaymode();
            }
        }
    }
}
