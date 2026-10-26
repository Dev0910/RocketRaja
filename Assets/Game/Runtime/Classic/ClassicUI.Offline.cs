using System;
using RocketRaja.Core;

namespace RocketRaja.Classic
{
    public sealed partial class ClassicUI
    {
        public ClassicOfflineView offlineView;
        public UnityEngine.GameObject adPrivacyRow;
        IRewardedAds offlineAds = new DisabledRewardedAds();
        int offlineRequest;
        bool offlineWatching;
        string offlineAdMessage;
        bool adProviderOverridden;

        void InitializeOfflineAds()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
            if (adProviderOverridden || isolatedPreview || driver.IsDisposableGuest || Cloud.RedesignSessionHost.Instance?.IsFixture == true) return;
            var settings = RewardedAdsSettings.Load();
            if (settings && settings.enabledInGame && !string.IsNullOrWhiteSpace(settings.RewardedUnitId))
                SetOfflineAdProvider(new AdMobRewardedAds(settings, driver.BeginNativeAdPresentation, driver.EndNativeAdPresentationAsync));
#endif
        }

        public void SetOfflineAdProvider(IRewardedAds provider)
        {
            adProviderOverridden = true;
            CancelOfflineAd();
            offlineAds.Dispose();
            offlineAds = provider ?? new DisabledRewardedAds();
            offlineAds.Initialize();
            RefreshOffline();
        }
        void RefreshOffline()
        {
            if (!offlineView || Engine == null) return;
            offlineView.Refresh(this, offlineWatching, offlineAdMessage ??
                (offlineAds is AdMobRewardedAds admob ? admob.Status : offlineAds.Ready ? "Complete an ad to add one extra reward." : "Ads unavailable. Your normal reward is already saved."), offlineAds.Ready);
        }
        void WatchOfflineAd()
        {
            if (CurrentDialog != "Offline" || !Engine.State.offline.pending || Engine.State.offline.adRewarded || offlineWatching) return;
            if (!offlineAds.Ready) { offlineAdMessage = "Ads unavailable. Your normal reward is already saved."; RefreshOffline(); return; }
            int request = ++offlineRequest;
            var session = driver.Session;
            string id = Engine.State.offline.id;
            offlineWatching = true; offlineAdMessage = "Your normal reward stays saved if you close the ad.";
            RefreshOffline();
            try
            {
                offlineAds.Show((completed, error) =>
                {
                    if (!this || request != offlineRequest || driver.Session != session) return;
                    // A provider must report on Unity's main thread, as required by the existing interface.
                    offlineWatching = false; ++offlineRequest; // Consume this callback before a save/reentrant refresh.
                    bool open = stack.Exists(v => v.route == "Offline");
                    if (!open || Engine.State.offline.id != id || !Engine.State.offline.pending) return;
                    if (completed)
                    {
                        bool saved = Command(e => e.RewardOfflineAd(id));
                        offlineAdMessage = saved ? "Bonus saved." : "Bonus could not be saved. Your normal reward is safe.";
                    }
                    else offlineAdMessage = "Ad not completed. Your normal reward is already saved.";
                    RefreshOffline();
                });
            }
            catch (Exception)
            {
                if (request == offlineRequest) { ++offlineRequest; offlineWatching = false; offlineAdMessage = "Ad unavailable. Your normal reward is already saved."; RefreshOffline(); }
            }
        }
        void CancelOfflineAd() { ++offlineRequest; offlineWatching = false; offlineAdMessage = null; }
    }
}
