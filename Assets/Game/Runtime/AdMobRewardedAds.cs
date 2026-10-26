using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>One cached rewarded ad. All SDK callbacks enter through the Unity-thread queue.</summary>
    public sealed class AdMobRewardedAds : IRewardedAds
    {
        readonly RewardedAdsSettings settings;
        readonly Func<bool> beginPresentation;
        readonly Func<Task<bool>> endPresentation;
        readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
        RewardedAd loaded, showing;
        Action<bool, string> completion;
        bool started, disposed, consentReady, sdkReady, loading, gatheringConsent, presenting;
        bool rewardEarned;
        double retryAt, loadedAt;
        int failures, loadVersion;
        string status = "Loading ad…";
        static bool sdkInitialized, sdkInitializing;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { sdkInitialized = sdkInitializing = false; }

        public AdMobRewardedAds(RewardedAdsSettings settings, Func<bool> beginPresentation, Func<Task<bool>> endPresentation)
        { this.settings = settings; this.beginPresentation = beginPresentation; this.endPresentation = endPresentation; }
        public bool Ready => !disposed && !presenting && !gatheringConsent && consentReady && ConsentInformation.CanRequestAds() && loaded != null && loaded.CanShowAd();
        public bool PrivacyOptionsRequired => !disposed && ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
        public string Status => status;
        double Now => Time.realtimeSinceStartupAsDouble;
        void Post(Action work) { if (!disposed) callbacks.Enqueue(work); }
        public void Initialize()
        {
            if (started || disposed) return;
            started = true;
            if (!settings || !settings.enabledInGame || string.IsNullOrWhiteSpace(settings.RewardedUnitId))
            { status = "Ads unavailable."; return; }
            UpdateConsent();
        }
        void UpdateConsent()
        {
            if (disposed || gatheringConsent) return;
            gatheringConsent = true; status = "Checking ad availability…";
            ConsentInformation.Update(new ConsentRequestParameters(), error => Post(() =>
            {
                if (error != null)
                {
                    gatheringConsent = false;
                    consentReady = ConsentInformation.CanRequestAds();
                    status = "Ads unavailable. Check your connection.";
                    retryAt = Now + 30;
                    return;
                }
                if (!beginPresentation()) { gatheringConsent = false; retryAt = Now + 5; return; }
                ConsentForm.LoadAndShowConsentFormIfRequired(formError => Post(async () =>
                {
                    bool resumed = await endPresentation();
                    if (disposed) return;
                    gatheringConsent = false;
                    consentReady = resumed && ConsentInformation.CanRequestAds();
                    status = consentReady ? "Loading ad…" : "Ads unavailable. Your normal reward is saved.";
                    retryAt = Now + (consentReady ? 0 : 30);
                }));
            }));
        }
        public void Tick()
        {
            if (disposed) return;
            while (callbacks.TryDequeue(out var work))
            {
                if (disposed) return;
                try { work(); } catch (Exception e) { Debug.LogWarning("Rewarded ad callback failed: " + e.GetType().Name); }
            }
            if (!started || !settings || !settings.enabledInGame || string.IsNullOrWhiteSpace(settings.RewardedUnitId) || gatheringConsent || presenting) return;
            if (!consentReady) { if (Now >= retryAt) UpdateConsent(); return; }
            if (!ConsentInformation.CanRequestAds())
            { consentReady = false; DestroyLoaded(); retryAt = Now + 30; return; }
            if (!sdkReady)
            {
                if (sdkInitialized) sdkReady = true;
                else if (!sdkInitializing)
                {
                    sdkInitializing = true;
                    MobileAds.Initialize(result =>
                    {
                        // These static flags are written/read only on Unity's main thread.
                        GoogleMobileAds.Common.MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        { sdkInitializing = false; sdkInitialized = result != null; });
                    });
                }
                return;
            }
            if (loaded != null && (Now - loadedAt >= 3300 || !loaded.CanShowAd())) DestroyLoaded();
            if (loaded == null && !loading && Now >= retryAt) Load();
        }
        void Load()
        {
            loading = true; status = "Loading ad…"; int version = ++loadVersion;
            RewardedAd.Load(settings.RewardedUnitId, new AdRequest(), (ad, error) =>
            {
                // Even after disposal, queue cleanup for a late native load result.
                GoogleMobileAds.Common.MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    if (disposed || version != loadVersion) { ad?.Destroy(); return; }
                    loading = false;
                    if (error != null || ad == null)
                    {
                        ad?.Destroy(); failures = Math.Min(failures + 1, 6);
                        retryAt = Now + Math.Min(120, 5 * Math.Pow(2, failures - 1));
                        status = "Ad unavailable right now. Retrying…";
                        return;
                    }
                    loaded = ad; loadedAt = Now; failures = 0; status = "Complete an ad to add one extra reward.";
                });
            });
        }
        public void Show(Action<bool, string> completed)
        {
            if (!Ready || !beginPresentation()) { completed?.Invoke(false, "Ad unavailable."); return; }
            presenting = true; rewardEarned = false; completion = completed;
            showing = loaded; loaded = null;
            var ad = showing;
            ad.OnAdFullScreenContentClosed += () => Post(() => Finish(ad, null));
            ad.OnAdFullScreenContentFailed += error => Post(() => Finish(ad, "Ad could not be shown."));
            try { ad.Show(reward => Post(() => { if (presenting && showing == ad) rewardEarned = true; })); }
            catch (Exception) { Finish(ad, "Ad could not be shown."); }
        }
        async void Finish(RewardedAd ad, string error)
        {
            if (disposed || showing != ad) return;
            showing = null; ad.Destroy();
            bool earned = rewardEarned; var callback = completion; completion = null;
            bool resumed = false;
            try { resumed = await endPresentation(); }
            catch (Exception e) { Debug.LogWarning("Could not resume rewarded ad session: " + e.GetType().Name); }
            if (disposed) return;
            presenting = false; retryAt = Now;
            callback?.Invoke(earned && error == null && resumed, resumed ? error : "Reconnect to continue.");
        }
        public void PrivacyOptions()
        {
            if (!PrivacyOptionsRequired || presenting || gatheringConsent || !beginPresentation()) return;
            gatheringConsent = true; DestroyLoaded(); ++loadVersion; loading = false;
            ConsentForm.ShowPrivacyOptionsForm(error => Post(async () =>
            {
                bool resumed = await endPresentation();
                if (disposed) return;
                gatheringConsent = false; consentReady = resumed && ConsentInformation.CanRequestAds(); retryAt = Now;
                status = consentReady ? "Loading ad…" : "Ads unavailable. Your normal reward is saved.";
            }));
        }
        void DestroyLoaded() { loaded?.Destroy(); loaded = null; }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; ++loadVersion; DestroyLoaded(); showing?.Destroy(); showing = null; completion = null;
            while (callbacks.TryDequeue(out _)) { }
        }
    }
}
