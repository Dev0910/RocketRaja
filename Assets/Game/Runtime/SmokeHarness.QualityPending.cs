#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        IEnumerator QualityPendingRewardChecks()
        {
            var host = Cloud.PlayerSessionHost.Instance;
            var adapter = game.Ads as TestRewardedAds;
            bool isolated = host != null && host.Session != null && !host.Session.IsAccount &&
                            adapter != null && !game.IsAdShowing && !game.ModalPaused &&
                            !game.OfflineScreen.gameObject.activeSelf && !game.SponsorScreen.gameObject.activeSelf;
            Check(isolated, "Pending reward guard checks require an idle isolated guest test-ad session");
            if (!isolated) yield break;

            var showingField = typeof(GameController).GetField("adShowing", QualityPrivate);
            Check(showingField != null, "Pending reward fixture resolves the controller's in-flight guard");
            if (showingField == null) yield break;
            var state = game.Engine.State;
            var savedState = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(state));
            bool ready = adapter.Ready, success = adapter.Success;
            bool wasShowing = game.IsAdShowing;
            var offlineFields = CaptureQualityPendingFields(game.OfflineScreen, new[] {
                "resolved", "awaiting", "lastReady", "lastAdShowing", "lastCanWatch", "lastEligible",
                "lastAttemptFailed", "bonusAdded", "offeredOfflineId" });
            var sponsorFields = CaptureQualityPendingFields(game.SponsorScreen, new[] {
                "lastAttemptFailed", "awaiting", "lastReady", "lastAdShowing", "lastCanWatch", "lastRemaining" });
            try
            {
                adapter.Ready = adapter.Success = true;
                state.reducedMotion = true;
                state.pendingOfflineId = "quality-pending-guard";
                state.pendingOfflineReward = new Amount(500);
                state.sponsorUntil = 0;
                foreach (bool offline in new[] { true, false })
                {
                    var screen = offline ? (Component)game.OfflineScreen : game.SponsorScreen;
                    string name = offline ? "Offline" : "Sponsor";
                    var awaitingField = screen.GetType().GetField("awaiting", QualityPrivate);
                    var resultMethod = screen.GetType().GetMethod("OnAdResult", QualityPrivate);
                    Check(awaitingField != null && resultMethod != null,
                        name + " pending fixture resolves its request guard and completion callback");
                    if (awaitingField == null || resultMethod == null) continue;
                    if (offline) game.View.ShowOffline(); else game.View.ShowSponsor();
                    // Model an external video and its account-resync interval without calling Ads.Show.
                    awaitingField.SetValue(screen, true);
                    showingField.SetValue(game, true);
                    if (offline) game.OfflineScreen.Open(); else game.SponsorScreen.Open();
                    yield return null;
                    var close = QualityField<Button>(screen, "closeButton");
                    var exit = QualityField<Button>(screen, offline ? "continueButton" : "backButton");
                    var video = QualityField<Button>(screen, "videoButton");
                    Check((bool)awaitingField.GetValue(screen) && game.IsAdShowing && screen.gameObject.activeInHierarchy,
                        name + " reopening preserves its pending request while an external video/resync is in flight");
                    Check(!close.interactable && !exit.interactable && !video.interactable,
                        name + " pending video disables both exits and another video request");

                    string pendingState = JsonUtility.ToJson(state);
                    close.onClick.Invoke(); exit.onClick.Invoke();
                    if (offline) game.OfflineScreen.Dismiss(); else game.SponsorScreen.Close();
                    bool handledBack = game.View.TryNavigateBack();
                    Check(handledBack && screen.gameObject.activeInHierarchy && (bool)awaitingField.GetValue(screen),
                        name + " pending close/continue/Back cannot dismiss its protected surface");
                    if (offline)
                    {
                        game.OfflineScreen.RequestVideo(); game.OfflineScreen.RequestVideo();
                        game.OfflineScreen.Open();
                    }
                    else
                    {
                        game.SponsorScreen.RequestVideo(); game.SponsorScreen.RequestVideo();
                        game.SponsorScreen.Open();
                    }
                    yield return null;
                    Check(JsonUtility.ToJson(state) == pendingState && game.IsAdShowing &&
                          (bool)awaitingField.GetValue(screen) && !video.interactable,
                        name + " pending duplicate requests/reopen preserve the offer, cash, receipts and original in-flight guard");

                    showingField.SetValue(game, false);
                    resultMethod.Invoke(screen, new object[] { false });
                    yield return null;
                    var message = QualityField<TMP_Text>(screen, "messageText");
                    Check(!(bool)awaitingField.GetValue(screen) && !game.IsAdShowing &&
                          close.interactable && exit.interactable && video.interactable && message.text.Contains("try again"),
                        name + " completed unsuccessful pending callback restores exits and an explicit ready retry");
                    Check(JsonUtility.ToJson(state) == pendingState,
                        name + " unsuccessful pending callback grants no cash, receipt or boost and preserves eligibility");
                    if (offline) game.OfflineScreen.Dismiss(); else game.SponsorScreen.Close();
                    Check(!screen.gameObject.activeSelf, name + " can leave its modal after the pending callback completes");
                }
            }
            finally
            {
                // Close first: releasing modal ownership may update the clock. Restore the
                // original state afterward, retaining the session's existing GameState identity.
                showingField.SetValue(game, false);
                game.OfflineScreen.gameObject.SetActive(false);
                game.SponsorScreen.gameObject.SetActive(false);
                foreach (var field in typeof(GameState).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    field.SetValue(state, field.GetValue(savedState));
                foreach (var pair in offlineFields) pair.Key.SetValue(game.OfflineScreen, pair.Value);
                foreach (var pair in sponsorFields) pair.Key.SetValue(game.SponsorScreen, pair.Value);
                adapter.Ready = ready; adapter.Success = success;
                showingField.SetValue(game, wasShowing);
            }
            checks.Add("LIMIT Pending-video lifecycle uses reflected guards and a synthetic unsuccessful callback; no real external ad or cloud resync runs.");
        }

        static Dictionary<FieldInfo, object> CaptureQualityPendingFields(object screen, string[] names)
        {
            var fields = new Dictionary<FieldInfo, object>();
            foreach (string name in names)
            {
                var field = screen.GetType().GetField(name, QualityPrivate);
                if (field != null) fields.Add(field, field.GetValue(screen));
            }
            return fields;
        }
    }
}
#endif
