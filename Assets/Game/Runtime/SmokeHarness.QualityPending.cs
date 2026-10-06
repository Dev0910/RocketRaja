#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        IEnumerator AdRetirementChecks()
        {
            Check(game.enabled && game.View.enabled && game.StartupReady, "Existing Main boots without a required Sponsor presenter");
            Check(Cloud.PlayerSessionHost.Instance.Session is Cloud.GuestSession && RedesignEnvironment.Isolated,
                "Ad retirement checks use a disposable guest with live cloud disabled");
            var state = game.Engine.State;
            state.reducedMotion = true;
            state.music = state.sound = false;
            state.storeNamePromptHandled = true;
            state.storeName = GameState.DefaultStoreName;
            game.View.Switch("Home");
            Check(!game.HomeScreen.GetComponentsInChildren<Transform>(true).Any(x => x.name == "Sponsor"), "Saved Home has no Sponsor controls");
            Check(game.SponsorScreen == null, "Saved Main no longer references a Sponsor canvas");
            game.View.ShowSettings();
            Check(game.SettingsScreen.enabled && !game.SettingsScreen.GetComponentsInChildren<Transform>(true).Any(x => x.name == "settings-consent-row"),
                "Settings keeps account/preferences without an advertising consent row");
            game.SettingsScreen.Close();
            game.PrivacyScreen.Bind(game, null); game.PrivacyScreen.Open();
            Check(!game.PrivacyScreen.GetComponentsInChildren<TMP_Text>(true).Any(x => x.text.Contains("advertising") || x.text.Contains("advertiser configuration")),
                "Saved privacy copy no longer claims use of advertising services");
            game.PrivacyScreen.Close();
            state.products[0].level = 10;
            state.sponsorUntil = state.now + 300;
            var normal = game.Engine.NormalIncome;
            Check(game.Engine.LiveIncome.CompareTo(normal) == 0, "Saved sponsor timestamp does not affect runtime income");
            var before = state.cash;
            var earned = game.Engine.ResolveOffline(state.now + 3600);
            Check(earned.CompareTo(normal * 1800) == 0 && state.cash.CompareTo(before + earned) == 0,
                "Offline settlement credits ordinary production once without the saved sponsor boost");
            game.View.ShowOffline();
            Check(game.OfflineScreen.enabled && game.OfflineScreen.gameObject.activeSelf &&
                !game.OfflineScreen.GetComponentsInChildren<Button>(true).Any(x => x.name == "offline-video"),
                "Saved Offline opens with Continue and no reward button");
            yield return Capture("ad-free-offline", .2f);
            yield return QualityPendingRewardChecks();
            before = state.cash;
            game.OfflineScreen.Dismiss(); game.OfflineScreen.Dismiss();
            Check(state.cash.CompareTo(before) == 0 && string.IsNullOrEmpty(state.pendingOfflineId), "Repeated dismissal preserves credited cash");
            yield return QualityRewardRetryChecks();
            game.View.Switch("Home");
            yield return Capture("ad-free-home", .2f);
            Check(!game.ModalPaused && !game.IsAdShowing, "Retired routes leave no modal or advertising input block");
        }

        IEnumerator QualityPendingRewardChecks()
        {
            var state = game.Engine.State;
            string before = JsonUtility.ToJson(state);
            int callbacks = 0;
            bool accepted = false;
            foreach (string placement in new[] { "offline", "sponsor", "unknown" })
                game.ShowAd(placement, result => { callbacks++; accepted |= result; });
            var late = typeof(ImportedOfflineScreen).GetMethod("OnAdResult", QualityPrivate);
            Check(late != null, "Legacy offline completion remains an inert compatibility callback");
            late?.Invoke(game.OfflineScreen, new object[] { true });
            late?.Invoke(game.OfflineScreen, new object[] { true });
            Check(callbacks == 3 && !accepted && !game.IsAdShowing && JsonUtility.ToJson(state) == before,
                "Repeated retired requests and late successful callbacks cannot mutate progress or hold an ad guard");
            Check(game.Ads is DisabledRewardedAds && !game.Ads.Ready && !game.Ads.PrivacyOptionsRequired,
                "Smoke mode uses the same disabled provider as the game");
            yield return null;
        }
    }
}
#endif
