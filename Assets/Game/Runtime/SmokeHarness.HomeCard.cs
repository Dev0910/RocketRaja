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
        IEnumerator HomeCardChecks()
        {
            var e = game.Engine; var s = e.State;
            s.sound = s.music = false; s.reducedMotion = true;
            var dialog = game.StoreNameScreen;
            var input = dialog.GetComponentInChildren<TMP_InputField>(true);
            var save = dialog.GetComponentsInChildren<Button>(true).Single(x => x.name == "save-store-name");
            Check(dialog.gameObject.activeSelf && game.ModalPaused, "First eligible Home visit opens store naming");
            long taps = s.manualTaps; game.Tap(); Check(s.manualTaps == taps, "Naming blocks background taps");
            dialog.Dismiss(); yield return null;
            Check(!dialog.gameObject.activeSelf && s.storeNamePromptHandled && s.StoreName == GameState.DefaultStoreName, "Skip saves default and handles prompt");
            yield return new WaitForSecondsRealtime(.3f); Check(!dialog.gameObject.activeSelf, "Skipped prompt does not repeat");
            game.View.ShowSettings();
            game.SettingsScreen.GetComponentsInChildren<Button>(true).Single(x => x.name == "settings-rename-store").onClick.Invoke();
            input.text = "Temporary"; dialog.Dismiss();
            Check(s.StoreName == GameState.DefaultStoreName && game.SettingsScreen.gameObject.activeSelf, "Rename cancellation preserves name and returns to Settings");
            game.SettingsScreen.Close(); game.View.ShowStoreName();
            foreach (string invalid in new[] { "", "   ", new string('x',25) })
            {
                input.text = invalid; yield return null;
                Check(!save.interactable, "Invalid name disables save: length " + invalid.Length);
            }
            input.text = "<b>My Store</b>"; yield return null; save.onClick.Invoke();
            var sign = game.HomeScreen.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Shop Sign");
            Check(!sign.richText && sign.text == "<b>My Store</b>", "Store markup renders literally");
            var restored = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(s));
            Check(restored.StoreName == s.StoreName && restored.storeNamePromptHandled, "Unity guest serialization retains store name");
            var payload = SavePayload.Create(JsonUtility.ToJson(s));
            Check(payload.Valid() && JsonUtility.FromJson<GameState>(payload.json).StoreName == s.StoreName, "Account payload preserves store name");
            game.SettingsScreen.Close();
            s.storeNamePromptHandled = false; s.pendingOfflineId = "naming-order-test"; s.pendingOfflineReward = 10;
            game.View.ShowOffline(); yield return new WaitForSecondsRealtime(.3f);
            Check(!dialog.gameObject.activeSelf, "Naming waits behind offline dialog");
            game.OfflineScreen.Dismiss(); yield return new WaitForSecondsRealtime(.3f);
            Check(dialog.gameObject.activeSelf, "Naming appears after offline dismissal"); dialog.Dismiss();
            s.storeNamePromptHandled = false; game.View.ShowSaveRecovery(); yield return new WaitForSecondsRealtime(.3f);
            Check(!dialog.gameObject.activeSelf, "Naming waits behind recovery dialog");
            UiMotion.Hide(game.SaveRecoveryScreen.gameObject, game); yield return new WaitForSecondsRealtime(.3f); dialog.Dismiss();
            var home = game.HomeScreen;
            Check(!home.GetComponentsInChildren<Transform>(true).Any(x => x.name == "Next Display" || x.name == "home-normal" || x.name == "home-run" || x.name == "Counter Message"), "Removed Home sections are absent");
            Check(!home.GetComponentInChildren<ShopAmbient>(true).VisitorsVisible, "No decorative visitors");
            var sponsor = home.GetComponentsInChildren<Transform>(true).Single(x => x.name == "Sponsor").gameObject;
            var sponsorButton = sponsor.GetComponentInChildren<Button>(true);
            var ads = (TestRewardedAds)game.Ads;
            ads.Ready = false; home.Refresh(); Check(!sponsor.activeSelf, "Unavailable Sponsor collapses");
            ads.Ready = true; home.Refresh(); Check(sponsor.activeSelf && sponsorButton.interactable, "Ready Sponsor offer is enabled");
            e.MutationAllowed = () => false; home.Refresh(); Check(!sponsorButton.interactable, "Read-only session disables Sponsor"); e.MutationAllowed = null;
            s.sponsorUntil = s.now + 300; home.Refresh();
            Check(sponsor.activeSelf && !sponsorButton.interactable && sponsor.GetComponent<LayoutElement>().preferredHeight == 95, "Active Sponsor uses compact timer");
            s.sponsorUntil = s.now; ads.Ready = false; home.Refresh(); Check(!sponsor.activeSelf, "Expired unavailable Sponsor collapses");
            var skill = home.GetComponentsInChildren<Button>(true).Single(x => x.name == "home-skill-0");
            var status = skill.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Status");
            s.upgrades.Clear(); home.Refresh(); Check(status.text == "LOCKED" && !skill.interactable, "Locked skill state");
            s.products[0].level = 1; s.upgrades.Add("skill-0-0"); home.Refresh(); Check(status.text == "READY" && skill.interactable, "Ready skill state");
            e.ActivateSkill(0); home.Refresh(); Check(status.text.StartsWith("ACTIVE") && !skill.interactable, "Active skill state");
            s.skills[0].activeUntil = 0; home.Refresh(); Check(status.text.StartsWith("COOLDOWN") && !skill.interactable, "Cooldown skill state");
            var tapUpgrade = e.Data.upgrades.Single(x => x.id == "tap-0"); double originalMultiplier = tapUpgrade.multiplier;
            s.upgrades.Add("tap-0"); s.tapLevel = 0;
            try
            {
                foreach (var size in new[] { new Vector2Int(360, 640), new Vector2Int(450, 1000) })
                {
                    ResizeForReview(size.x, size.y); e.SetStoreName("ABCDEFGHIJKLMNOPQRSTUVWX");
                    foreach (double value in new[] { 1e3, 1e6, 1e9, 1e12 })
                    {
                        tapUpgrade.multiplier = value; home.Refresh(); home.TapFeedback();
                        yield return Capture("home-" + size.x + "-" + value.ToString("0"), 0);
                        var perTap = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "home-tap");
                        perTap.ForceMeshUpdate();
                        Check(perTap.text == "₹" + new Amount(value) && !perTap.isTextOverflowing, "Per Tap suffix fits " + size + "/" + value);
                        var rewards = home.GetComponentsInChildren<TMP_Text>(true).Where(x => x.name == "Popup Text" && x.gameObject.activeInHierarchy).ToArray();
                        Check(rewards.Any(x => x.text == "+₹" + new Amount(value)), "Floating reward keeps suffix " + value);
                    }
                    sign.ForceMeshUpdate(); Check(!sign.isTextOverflowing, "Longest store name fits sign " + size);
                    game.View.ShowStoreName(); yield return Capture("store-name-" + size.x); dialog.Dismiss(); game.SettingsScreen.Close();
                    game.View.ShowSettings(); yield return Capture("home-settings-" + size.x); game.SettingsScreen.Close();
                }
            }
            finally { tapUpgrade.multiplier = originalMultiplier; }
            yield return HtmlAmbientChecks();
            yield return HtmlTapChecks();
        }
    }
}
#endif
