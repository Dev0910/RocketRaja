using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicRewardedAdsValidation
    {
        const string Key = "Classic.RewardedAdsValidation";
        const string Folder = "Documentation/Implementation/RewardedAds";
        public static string Status { get; private set; } = "Not run";
        static readonly List<string> report = new List<string>();
        static readonly List<string> errors = new List<string>();
        static ClassicRewardedAdsValidation() { EditorApplication.playModeStateChanged += Mode; }
        public static string Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorSceneManager.GetSceneManagerSetup().Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty))
                throw new InvalidOperationException("Clean Edit Mode required");
            SessionState.SetString(Key + ".StartScene", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetBool(Key, true); EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode(); return Status = "Running Google SDK Editor rewarded-ad validation";
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (mode == PlayModeStateChange.EnteredPlayMode) Run();
            if (mode == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Key);
                var path = SessionState.GetString(Key + ".StartScene", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);
            }
        }
        static void Check(bool ok, string message) { if (!ok) throw new Exception(message); report.Add("PASS " + message); }
        static async Task Until(Func<bool> predicate, string message)
        {
            double end = EditorApplication.timeSinceStartup + 30;
            while (!predicate()) { if (EditorApplication.timeSinceStartup >= end) throw new TimeoutException(message); await Task.Delay(50); }
        }
        static void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message); }
        static async void Run()
        {
            report.Clear(); errors.Clear(); Directory.CreateDirectory(Folder); Application.logMessageReceived += Log;
            try
            {
                string root = Path.Combine(Path.GetTempPath(), "ClassicRewardedAds-" + Guid.NewGuid().ToString("N"));
                var data = RedesignRuntimeContent.Load(); var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc - 36000);
                state.products[0] = 10; state.storeNamePromptHandled = true;
                var store = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
                new RedesignEngine(data, state, store.Save).Save();
                var host = RedesignSessionHost.Ensure(); host.ConfigureFixture(null, root); await host.ReturnToGuestAsync(false);
                ClassicImportQA.Viewport(390, 844); host.EnterMain();
                await Until(() => UnityEngine.Object.FindFirstObjectByType<ClassicApplication>()?.Ready == true, "Main startup");
                var ui = UnityEngine.Object.FindFirstObjectByType<ClassicApplication>().ui;
                ui.driver.enabled = false; ui.application.enabled = false; ui.Engine.State.reducedMotion = true;
                var settings = RewardedAdsSettings.Load();
                Check(settings.androidAppId == "ca-app-pub-3595621361914402~9466422249" && settings.androidRewardedUnitId == "ca-app-pub-3595621361914402/1364570948", "Supplied Android app and rewarded-unit IDs are saved");
                Check(settings.TestMode && settings.RewardedUnitId == RewardedAdsSettings.TestAndroidRewarded, "Editor uses Google test rewarded unit instead of live inventory");
                var provider = new AdMobRewardedAds(settings, ui.driver.BeginNativeAdPresentation, ui.driver.EndNativeAdPresentationAsync);
                ui.SetOfflineAdProvider(provider);
                await Until(() => provider.Ready, "Google SDK rewarded loading; status=" + provider.Status);
                Check(ui.CurrentDialog == "Offline" && ui.Engine.State.offline.pending, "Original offline reward remains open after consent and initialization");
                ui.Refresh(); Check(ui.offlineView.watch.interactable, "Loaded Google SDK ad enables the offline Watch button");
                var receipt = ui.Engine.State.offline.Copy(); var before = ui.Engine.State.cash;
                ui.Handle("OfflineAd", "watch");
                Check(ui.driver.NativeAdPresentation && !ui.offlineView.watch.interactable, "Showing an ad gates repeat watches and starts native presentation handling");
                ui.driver.SendMessage("OnApplicationPause", true);
                Check(!ui.driver.Session.Suspended && ui.Engine.State.offline.id == receipt.id, "Native Android ad pause preserves the engine and frozen receipt");
                ui.driver.SendMessage("OnApplicationPause", false);
                await Until(() => UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b => b.name == "Button" && b.GetComponentInChildren<UnityEngine.UI.Text>()?.text == "Close Ad"), "SDK Editor ad close button");
                var close = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == "Button" && b.GetComponentInChildren<UnityEngine.UI.Text>()?.text == "Close Ad");
                var adCanvas = close.GetComponentInParent<Canvas>(); if (adCanvas) adCanvas.sortingOrder = 32000;
                await Task.Delay(5500); ClassicImportQA.Capture(Folder + "/Google-SDK-editor-test-ad.png");
                close.onClick.Invoke();
                await Until(() => ui.Engine.State.offline.adRewarded && !ui.driver.NativeAdPresentation, "SDK completion reward");
                Check(ui.Engine.State.offline.id == receipt.id && ui.Engine.State.offline.amount.CompareTo(receipt.amount) == 0 && ui.Engine.State.cash >= before + receipt.amount, "SDK earned-reward callback durably doubles the original offline receipt");
                Check(store.Load().state.offline.adRewarded && !ui.offlineView.watch.interactable, "Saved completion prevents repeat rewards after reload");
                Check(!ui.Engine.RewardOfflineAd(receipt.id), "Duplicate provider delivery cannot grant again");
                await Task.Delay(250); ClassicImportQA.Capture(Folder + "/Offline-ad-reward-saved.png");
                ui.Handle("Offline", "close"); await Task.Delay(350); ui.Open("Settings"); ui.Refresh();
                Check(ui.adPrivacyRow && ui.adPrivacyRow.activeSelf == provider.PrivacyOptionsRequired, "Settings ad privacy entry follows UMP requirements");
                Check(ui.adPrivacyRow.transform.parent.GetComponent<VerticalLayoutGroup>(), "Ad privacy entry belongs to the Settings scroll layout");
                Check(errors.Count == 0, "No runtime errors during SDK Editor integration check");
                Status = "PASS: " + report.Count + " Google SDK integration checks (Editor simulation; no device/network delivery claim)";
                report.Add("Disposable local profile: " + root);
            }
            catch (Exception e) { Status = "FAIL: " + e.Message; report.Add(e.ToString()); report.AddRange(errors); }
            finally { Application.logMessageReceived -= Log; report.Add(Status); File.WriteAllLines(Folder + "/Validation.txt", report); EditorApplication.ExitPlaymode(); }
        }
    }
}
