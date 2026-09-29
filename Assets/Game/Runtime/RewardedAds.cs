using System;
using System.Collections.Concurrent;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace RocketRaja
{
    public interface IRewardedAds : IDisposable
    {
        bool Ready { get; }
        bool PrivacyOptionsRequired { get; }
        void Initialize();
        void Tick();
        void Show(Action<bool, string> completed);
        void PrivacyOptions();
    }
    public sealed class AdMobRewardedAds : IRewardedAds
    {
        readonly GameController owner;
        readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
        RewardedAd ad;
        bool initialized, disposed, loading;
        double nextAttempt, loadedAt;
        Action<bool, string> completion;
        bool earned;
        string token;
        public AdMobRewardedAds(GameController controller) { owner = controller; }
        public bool Ready => !disposed && ad != null && ad.CanShowAd() && GameController.Utc - loadedAt < 3500;
        public bool PrivacyOptionsRequired => ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
        void Queue(Action action) { if (!disposed) callbacks.Enqueue(action); }
        public void Initialize()
        {
            ConsentInformation.Update(new ConsentRequestParameters { TagForUnderAgeOfConsent = false }, error => Queue(() =>
            {
                if (error != null) { TryInitialize(); return; }
                ConsentForm.LoadAndShowConsentFormIfRequired(formError => Queue(TryInitialize));
            }));
        }
        void TryInitialize()
        {
            if (disposed || initialized || !ConsentInformation.CanRequestAds()) return;
            initialized = true;
            MobileAds.SetRequestConfiguration(new RequestConfiguration { MaxAdContentRating = MaxAdContentRating.T });
            MobileAds.Initialize(_ => Queue(Load));
        }
        void Load()
        {
            if (disposed || loading || !ConsentInformation.CanRequestAds()) return;
            loading = true; ad?.Destroy(); ad = null;
            var config = Resources.Load<ProductionConfig>("ProductionConfig");
            string unit = "ca-app-pub-3940256099942544/5224354917";
            if (!Debug.isDebugBuild && config != null && config.productionAds && !string.IsNullOrEmpty(config.rewardedAdUnitId)) unit = config.rewardedAdUnitId;
            RewardedAd.Load(unit, new AdRequest(), (loaded, error) => Queue(() =>
            {
                loading = false; nextAttempt = GameController.Utc + 60;
                if (error != null || loaded == null) { loaded?.Destroy(); return; }
                ad = loaded; loadedAt = GameController.Utc;
                ad.OnAdFullScreenContentClosed += () => Queue(Finish);
                ad.OnAdFullScreenContentFailed += _ => Queue(Finish);
            }));
        }
        public void Tick()
        {
            while (callbacks.TryDequeue(out var action)) action();
            if (initialized && completion == null && !loading && !Ready && GameController.Utc >= nextAttempt) Load();
        }
        public void Show(Action<bool, string> completed)
        {
            if (!Ready || completion != null) { completed(false, null); return; }
            completion = completed; earned = false; token = Guid.NewGuid().ToString("N");
            ad.Show(_ => Queue(() => earned = true));
        }
        void Finish()
        {
            var done = completion; completion = null;
            done?.Invoke(earned, token); ad?.Destroy(); ad = null; nextAttempt = GameController.Utc + 2;
        }
        public void PrivacyOptions()
        {
            ConsentForm.ShowPrivacyOptionsForm(_ => Queue(() => { ad?.Destroy(); ad = null; if (ConsentInformation.CanRequestAds()) { TryInitialize(); nextAttempt = 0; } }));
        }
        public void Dispose() { disposed = true; ad?.Destroy(); ad = null; }
    }
    // Deterministic test adapter; never used for production earnings automatically.
    public sealed class TestRewardedAds : IRewardedAds
    {
        public bool Ready { get; set; } = true;
        public bool Success { get; set; } = true;
        public bool PrivacyOptionsRequired => false;
        public void Initialize() {} public void Tick() {} public void Dispose() {} public void PrivacyOptions() {}
        public void Show(Action<bool, string> completed) { completed(Ready && Success, Guid.NewGuid().ToString("N")); }
    }
}
