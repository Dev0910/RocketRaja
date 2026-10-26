using UnityEngine;

namespace RocketRaja
{
    [CreateAssetMenu(menuName = "Rocket Raja/Rewarded Ads Settings")]
    public sealed class RewardedAdsSettings : ScriptableObject
    {
        public bool enabledInGame = true;
        public bool useTestAds = true;
        public string androidAppId = "", androidRewardedUnitId = "";
        public string iosAppId = "", iosRewardedUnitId = "";
        public const string TestAndroidApp = "ca-app-pub-3940256099942544~3347511713";
        public const string TestIosApp = "ca-app-pub-3940256099942544~1458002511";
        public const string TestAndroidRewarded = "ca-app-pub-3940256099942544/5224354917";
        public const string TestIosRewarded = "ca-app-pub-3940256099942544/1712485313";
        public bool TestMode => useTestAds || Debug.isDebugBuild || Application.isEditor;
        public string RewardedUnitId => TestMode
            ? (Application.platform == RuntimePlatform.IPhonePlayer ? TestIosRewarded : TestAndroidRewarded)
            : (Application.platform == RuntimePlatform.IPhonePlayer ? iosRewardedUnitId : androidRewardedUnitId);
        public static RewardedAdsSettings Load() => Resources.Load<RewardedAdsSettings>("RewardedAdsSettings");
    }
}
