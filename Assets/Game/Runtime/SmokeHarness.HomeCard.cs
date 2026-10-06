#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
            var home = game.HomeScreen;
            var hint = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Hint Text");
            var nextKicker = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Next Action Kicker");
            var nextGoal = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Next Goal");
            var nextProgress = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Next Progress");
            var nextProducts = home.GetComponentsInChildren<Button>(true).Single(x => x.name == "Next Products Button");
            var nextActionText = nextProducts.GetComponentInChildren<TMP_Text>(true);
            var sponsor = home.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == "Sponsor")?.gameObject;

            home.Refresh();
            Check(sponsor == null || !sponsor.activeSelf, "Home has no active Sponsor offer");
            Check(e.ProductCost(0, 1).CompareTo(new Amount(30)) == 0, "First Phuljhari costs the introductory ₹30");
            var introBulk = new Amount(30) + new Amount(120) * Catalog.Growth;
            Check(System.Math.Abs(Amount.Ratio(e.ProductCost(0, 2), introBulk) - 1) < 1e-8,
                "Introductory discount applies only to the first unit in a bulk quote");
            Check(!dialog.gameObject.activeSelf && !s.storeNamePromptHandled && s.onboarding == 0 &&
                  hint.text == "Tap to earn" && nextActionText.text == "Tap to earn",
                "Fresh Home starts with a tap prompt and no naming dialog");
            long taps = s.manualTaps; game.Tap(); home.Refresh();
            Check(s.manualTaps == taps + 1 && s.onboarding == 1 && hint.text.StartsWith("Earn ₹") &&
                  nextGoal.text == "Stock Phuljhari" && nextProducts.interactable,
                "First tap earns and shows the introductory shelf goal");
            yield return new WaitForSecondsRealtime(.3f);
            Check(!dialog.gameObject.activeSelf, "Naming waits while the first shelf is unowned");
            taps = s.manualTaps; nextProducts.onClick.Invoke();
            Check(s.manualTaps == taps + 1 && s.onboarding == 1, "Goal action earns while the shelf is unaffordable");
            game.View.Switch("Products");
            Check(s.onboarding == 2, "Products visit is recorded after the first tap");
            var firstPurchase = game.ProductsScreen.GetComponentsInChildren<Button>(true).Single(x => x.name == "purchase-0");
            firstPurchase.onClick.Invoke();
            Check(s.Level(0) == 0 && s.onboarding == 2, "Unaffordable purchase does not finish onboarding");
            s.cash = e.ProductCost(0, 1);
            game.View.Switch("Home"); home.Refresh();
            Check(hint.text == "Shelf ready" && nextActionText.text == "View shelf",
                "Affordable shelf action describes the Product Detail destination");
            var detail = game.ProductDetailScreen;
            var detailName = detail.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "detail-name");
            var detailBack = detail.GetComponentsInChildren<Button>(true).Single(x => x.name == "back-ProductDetail");
            var detailBackText = detailBack.GetComponentInChildren<TMP_Text>(true);
            var detailPurchase = detail.GetComponentsInChildren<Button>(true).Single(x => x.name == "purchase-ProductDetail");
            nextProducts.onClick.Invoke();
            Check(detail.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                  detailName.text == e.Data.products[0].name && detailBackText.text == "Return to Home",
                "Affordable starter goal opens Phuljhari Detail with Home as its origin");
            ResizeForReview(450, 1000);
            yield return Capture("home-goal-first-shelf-450", .5f);
            detailBack.onClick.Invoke(); yield return null;
            Check(!detail.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                  !game.ProductsScreen.gameObject.activeSelf,
                "Goal Detail return restores Home without switching tabs");
            nextProducts.onClick.Invoke();
            Check(detailPurchase.interactable, "First shelf is purchasable from its Detail action");
            detailPurchase.onClick.Invoke();
            Check(s.Level(0) == 1 && s.onboarding == 3 && game.View.CurrentTab == "Home",
                "First successful Detail purchase completes core onboarding on Home");
            Check(System.Math.Abs(Amount.Ratio(e.ProductCost(0, 1), new Amount(120) * Catalog.Growth) - 1) < 1e-8,
                "Second Phuljhari uses its regular upgrade price");
            home.Refresh();
            Check(nextGoal.text == "Upgrade Phuljhari" && nextActionText.text == "View upgrade" &&
                  hint.text == "Tap for ₹" + e.TapIncome,
                "After first purchase Home shows the next upgrade and the exact tap reward");
            int completedOnboarding = s.onboarding;
            s.onboarding = 2; home.Refresh();
            Check(hint.text == "Tap for ₹" + e.TapIncome,
                "An owned shelf shows the exact tap reward even if onboarding is incomplete");
            s.onboarding = completedOnboarding; home.Refresh();
            yield return null;
            Amount tapReward = e.TapIncome;
            Amount cashBeforeTap = s.cash;
            taps = s.manualTaps;
            game.Tap(); home.Refresh();
            Check(s.manualTaps == taps + 1 && s.cash.CompareTo(cashBeforeTap + tapReward) == 0 &&
                  hint.text == "Tap for ₹" + tapReward,
                "Established Home tap awards exactly the value shown in its hint");
            Amount savedCash = s.cash; s.cash = e.ProductCost(0, 1); home.Refresh();
            Check(nextKicker.text == "NEXT UPGRADE" && nextProgress.text.StartsWith("Ready") &&
                  nextActionText.text == "View upgrade",
                "Nearby meaningful upgrade is labeled as the next action");
            nextProducts.onClick.Invoke();
            Check(detail.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                  detailName.text == e.Data.products[0].name && detailBackText.text == "Return to Home" &&
                  detailPurchase.GetComponentInChildren<TMP_Text>(true).text.Contains("Upgrade"),
                "Upgrade goal opens the named Product Detail while keeping Home as origin");
            detailBack.onClick.Invoke(); yield return null;
            Check(!detail.gameObject.activeSelf && game.View.CurrentTab == "Home",
                "Upgrade Detail return restores Home");
            s.cash = savedCash; home.Refresh();
            yield return new WaitForSecondsRealtime(3.4f);
            Check(dialog.gameObject.activeSelf && game.ModalPaused, "Store naming opens after the first purchase and Home return");
            taps = s.manualTaps; game.Tap(); Check(s.manualTaps == taps, "Naming blocks background taps");
            dialog.Dismiss(); yield return null;
            Check(!dialog.gameObject.activeSelf && s.storeNamePromptHandled && s.StoreName == GameState.DefaultStoreName, "Skip saves default and handles prompt");
            yield return new WaitForSecondsRealtime(.3f); Check(!dialog.gameObject.activeSelf, "Skipped prompt does not repeat");
            int originalLevel = s.products[0].level;
            Amount originalCash = s.cash;
            s.products[0].level = 4; s.cash = Amount.Zero; home.Refresh();
            Check(nextGoal.text == "Stock Anar", "Near-term Anar unlock becomes the next goal as income grows");
            game.View.Switch("Products");
            var tenMode = game.ProductsScreen.GetComponentsInChildren<Button>(true).Single(x => x.name == "Quantity Buy ×10");
            tenMode.onClick.Invoke();
            game.View.Switch("Home"); home.Refresh(); nextProducts.onClick.Invoke();
            var quantity = game.ProductsScreen.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Quantity Label");
            Check(detail.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                  detailName.text == e.Data.products[1].name && quantity.text == "Qty ×10",
                "Recommended Anar shelf opens its Detail without changing Products quantity");
            detailBack.onClick.Invoke(); yield return null;
            Check(!detail.gameObject.activeSelf && game.View.CurrentTab == "Home",
                "Recommended shelf returns to Home");
            s.products[0].level = originalLevel; s.cash = originalCash; home.Refresh();
            var originalLevels = s.products.Select(x => x.level).ToArray();
            for (int i = 0; i < s.products.Count; i++) s.products[i].level = 1;
            s.cash = Amount.Zero; home.Refresh();
            Check(nextGoal.text == "Begin again brighter" ||
                  nextGoal.text.StartsWith("Upgrade ") && nextGoal.text != "Upgrade Phuljhari",
                "All-stocked Home avoids recommending a negligible first-shelf level");
            for (int i = 0; i < s.products.Count; i++) s.products[i].level = originalLevels[i];
            s.cash = originalCash; home.Refresh();
            int originalOnboarding = s.onboarding;
            Amount originalEligible = s.eligibleEarnings;
            var originalUpgrades = s.upgrades.ToArray();
            var originalLegacy = s.legacy.Select(x => x.level).ToArray();
            try
            {
                int[] lateLevels = { 1, 170, 151, 133, 114, 96, 78, 59 };
                for (int i = 0; i < lateLevels.Length; i++) s.products[i].level = lateLevels[i];
                for (int i = 0; i < s.legacy.Count; i++) s.legacy[i].level = 0;
                s.upgrades.Clear(); s.onboarding = 4;
                s.cash = s.eligibleEarnings = Amount.Zero;
                home.Refresh();
                Amount giftCost = e.ProductCost(7, 1);
                Check(e.PreviewPrestige().reward == 0 && nextKicker.text == "SAVE FOR UPGRADE" &&
                      nextGoal.text == "Upgrade Gift boxes" && nextProgress.text == "Earn ₹" + giftCost &&
                      nextActionText.text == "View upgrade" &&
                      hint.text == "Tap for ₹" + e.TapIncome && e.LiveIncome > e.TapIncome,
                    "Late-game Home shows the actual tap reward beside a stronger passive income and upgrade goal");
                foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(450, 1000) })
                {
                    ResizeForReview(size.x, size.y);
                    yield return Capture("home-late-save-for-upgrade-" + size.x + "x" + size.y, .5f);
                    hint.ForceMeshUpdate();
                    float hintPixels = 0;
                    for (int character = 0; character < hint.textInfo.characterCount; character++)
                    {
                        var glyph = hint.textInfo.characterInfo[character];
                        if (!glyph.isVisible) continue;
                        float top = RectTransformUtility.WorldToScreenPoint(null, hint.transform.TransformPoint(glyph.topLeft)).y;
                        float bottom = RectTransformUtility.WorldToScreenPoint(null, hint.transform.TransformPoint(glyph.bottomLeft)).y;
                        hintPixels = Mathf.Abs(top - bottom);
                        break;
                    }
                    Check(!hint.isTextOverflowing && hintPixels >= 11.5f,
                        "Actual tap reward hint remains legible at " + size +
                        " (" + hintPixels.ToString("0.0") + "px)");
                }
                nextProducts.onClick.Invoke();
                Check(detail.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                      detailName.text == e.Data.products[7].name && detailBackText.text == "Return to Home",
                    "Late-game saving goal opens Gift boxes Detail from Home");
                detailBack.onClick.Invoke(); yield return null;
                Check(!detail.gameObject.activeSelf && game.View.CurrentTab == "Home",
                    "Late-game Detail returns to Home");

                s.cash = giftCost; home.Refresh();
                string[] compactNames = { "Phuljhari", "Anar", "Chakri", "Rockets", "Crackers", "Strings", "Aerial", "Gift boxes" };
                int nearIndex = System.Array.FindIndex(compactNames, name => nextGoal.text == "Upgrade " + name);
                Amount strongestGain = Amount.Zero;
                for (int i = 0; i < compactNames.Length; i++)
                {
                    Amount gain = e.ProductIncome(i, 1) - e.ProductIncome(i);
                    if (gain > strongestGain) strongestGain = gain;
                }
                Check(nearIndex >= 0 && nextKicker.text == "NEXT UPGRADE" &&
                      nextProgress.text.StartsWith("Ready") &&
                      s.cash >= e.ProductCost(nearIndex, 1) &&
                      e.ProductIncome(nearIndex, 1) - e.ProductIncome(nearIndex) >= strongestGain * .1,
                    "Funded late-game action remains nearby and clears the meaningful-gain floor");
            }
            finally
            {
                for (int i = 0; i < originalLevels.Length; i++) s.products[i].level = originalLevels[i];
                for (int i = 0; i < originalLegacy.Length; i++) s.legacy[i].level = originalLegacy[i];
                s.upgrades.Clear(); s.upgrades.AddRange(originalUpgrades);
                s.onboarding = originalOnboarding; s.eligibleEarnings = originalEligible;
                s.cash = originalCash; home.Refresh();
            }
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
            Check(restored.StoreName == s.StoreName && restored.storeNamePromptHandled && restored.onboarding == 3,
                "Unity guest serialization retains store name and completed onboarding");
            var payload = SavePayload.Create(JsonUtility.ToJson(s));
            Check(payload.Valid() && JsonUtility.FromJson<GameState>(payload.json).StoreName == s.StoreName, "Account payload preserves store name");
            game.SettingsScreen.Close();
            string savedNameBeforeOrdering = s.storeName;
            s.storeName = null; s.storeNamePromptHandled = false;
            s.pendingOfflineId = "naming-order-test"; s.pendingOfflineReward = 10;
            game.View.ShowOffline(); yield return new WaitForSecondsRealtime(.3f);
            Check(!dialog.gameObject.activeSelf, "Naming waits behind offline dialog");
            game.OfflineScreen.Dismiss(); yield return new WaitForSecondsRealtime(.3f);
            Check(dialog.gameObject.activeSelf, "Naming appears after offline dismissal"); dialog.Dismiss();
            s.storeName = null; s.storeNamePromptHandled = false;
            game.View.ShowSaveRecovery(); yield return new WaitForSecondsRealtime(.3f);
            Check(!dialog.gameObject.activeSelf, "Naming waits behind recovery dialog");
            UiMotion.Hide(game.SaveRecoveryScreen.gameObject, game); yield return new WaitForSecondsRealtime(.3f); dialog.Dismiss();
            e.SetStoreName(savedNameBeforeOrdering);
            Check(!home.GetComponentsInChildren<Transform>(true).Any(x => x.name == "Next Display" || x.name == "home-normal" || x.name == "home-run" || x.name == "Counter Message"), "Removed Home sections are absent");
            Check(!home.GetComponentInChildren<ShopAmbient>(true).VisitorsVisible, "No decorative visitors");
            s.sponsorUntil = s.now + 300; home.Refresh();
            Check(sponsor == null || !sponsor.activeSelf, "Legacy sponsor timestamp never reopens a retired offer");
            s.sponsorUntil = 0;
            var skill = home.GetComponentsInChildren<Button>(true).Single(x => x.name == "home-skill-0");
            var status = skill.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Status");
            var notice = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Notice Text");
            s.upgrades.Clear(); home.Refresh(); Check(status.text == "LOCKED" && skill.interactable,
                "Locked skill can explain its unlock path");
            skill.onClick.Invoke();
            Check(home.HasNotice && notice.text == "Unlock this skill in Player.",
                "Locked skill points to its upgrade screen");
            s.products[0].level = 1; s.upgrades.Add("skill-0-0"); home.Refresh(); Check(status.text == "READY" && skill.interactable, "Ready skill state");
            e.ActivateSkill(0); home.Refresh(); Check(status.text.StartsWith("ACTIVE") && !skill.interactable, "Active skill state");
            s.skills[0].activeUntil = 0; home.Refresh(); Check(status.text.StartsWith("COOLDOWN") && !skill.interactable, "Cooldown skill state");
            var tapUpgrade = e.Data.upgrades.Single(x => x.id == "tap-0"); double originalMultiplier = tapUpgrade.multiplier;
            bool originalReducedMotion = s.reducedMotion;
            s.upgrades.Add("tap-0"); s.tapLevel = 0;
            try
            {
                foreach (var size in new[] { new Vector2Int(360, 640), new Vector2Int(450, 1000) })
                {
                    ResizeForReview(size.x, size.y);
                    yield return new WaitForSecondsRealtime(.5f);
                    e.SetStoreName("ABCDEFGHIJKLMNOPQRSTUVWX");
                    foreach (double value in new[] { 1e3, 1e6, 1e9, 1e12 })
                    {
                        tapUpgrade.multiplier = value; home.Refresh(); home.TapFeedback();
                        yield return Capture("home-" + size.x + "-" + value.ToString("0"), 0);
                        var perTap = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "home-tap");
                        perTap.ForceMeshUpdate();
                        hint.ForceMeshUpdate();
                        Check(perTap.text == "₹" + new Amount(value) && !perTap.isTextOverflowing &&
                              hint.text == "Tap for ₹" + e.TapIncome && !hint.isTextOverflowing,
                            "Per Tap and action hint suffixes fit " + size + "/" + value);
                        var rewards = home.GetComponentsInChildren<TMP_Text>(true).Where(x => x.name == "Popup Text" && x.gameObject.activeInHierarchy).ToArray();
                        Check(rewards.Any(x => x.text == "+₹" + new Amount(value)), "Floating reward keeps suffix " + value);
                        Check(TapPopupClearOfShelves(home, out string popupGeometry),
                            "Floating reward stays on the counter at " + size + "/" + value + " (" + popupGeometry + ")");
                    }
                    sign.ForceMeshUpdate();
                    float signPixels = sign.textInfo.characterCount > 0 ?
                        sign.textInfo.characterInfo[0].pointSize * sign.canvas.rootCanvas.scaleFactor : 0;
                    Check(signPixels >= 11.5f,
                        "Long store name keeps readable lettering on Home sign at " + size +
                        " (" + signPixels.ToString("0.0") + "px)");
                    game.View.ShowStoreName(); yield return Capture("store-name-" + size.x); dialog.Dismiss(); game.SettingsScreen.Close();
                    game.View.ShowSettings(); yield return Capture("home-settings-" + size.x); game.SettingsScreen.Close();
                }
                var inspectSign = home.GetComponentsInChildren<Button>(true).Single(x => x.name == "inspect-store-name");
                var fullName = dialog.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Full Name");
                var signClose = dialog.GetComponentsInChildren<Button>(true).Single(x => x.name == "cancel-store-name");
                var shelfTargets = home.GetComponentsInChildren<Button>(true).Where(x => x.name.StartsWith("open-")).ToArray();
                var gear = home.GetComponentsInChildren<Button>(true).Single(x => x.name == "settings-button");
                foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(450, 1000) })
                {
                    ResizeForReview(size.x, size.y);
                    game.View.Switch("Home");
                    e.SetStoreName(new string('W', 24)); home.Refresh();
                    yield return new WaitForSecondsRealtime(.5f);
                    sign.ForceMeshUpdate();
                    float signSize = sign.textInfo.characterCount > 0 ?
                        sign.textInfo.characterInfo[0].pointSize * sign.canvas.rootCanvas.scaleFactor : 0;
                    Check(sign.text == new string('W', 24) && sign.isTextTruncated && signSize >= 12f,
                        "Compact sign ellipsizes long names at readable type size at " + size +
                        " (" + signSize.ToString("0.0") + "px)");
                    Rect hitArea = TapScreenRect((RectTransform)inspectSign.transform);
                    bool avoidsControls = !hitArea.Overlaps(TapScreenRect((RectTransform)gear.transform)) &&
                        shelfTargets.All(x => !hitArea.Overlaps(TapScreenRect((RectTransform)x.transform)));
                    Check(hitArea.width >= 48 && hitArea.height >= 48 && avoidsControls,
                        "Sign has a 48-pixel touch area clear of gear and shelves at " + size +
                        " (" + hitArea.width.ToString("0.0") + "×" + hitArea.height.ToString("0.0") + ")");
                    var pointer = new PointerEventData(EventSystem.current) { position = hitArea.center };
                    var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
                    bool frontmost = hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == inspectSign;
                    Check(frontmost, "Sign receives the frontmost raycast at " + size);
                    Amount beforeCash = s.cash; long beforeTaps = s.manualTaps;
                    if (frontmost) ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
                    yield return null;
                    Check(dialog.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                          s.cash.CompareTo(beforeCash) == 0 && s.manualTaps == beforeTaps,
                        "Inspecting the sign opens its detail without earning cash at " + size);
                    fullName.ForceMeshUpdate();
                    float fullSize = fullName.textInfo.characterCount > 0 ?
                        fullName.textInfo.characterInfo[0].pointSize * fullName.canvas.rootCanvas.scaleFactor : 0;
                    Check(fullName.text == new string('W', 24) && !fullName.richText &&
                          !fullName.isTextTruncated && !fullName.isTextOverflowing && fullSize >= 14f,
                        "Full 24-character name wraps visibly at " + size +
                        " (" + fullSize.ToString("0.0") + "px)");
                    Check(save.gameObject.activeSelf && save.interactable && !input.gameObject.activeInHierarchy,
                        "Mutable sign detail offers Rename without opening the keyboard at " + size);
                    Check(!TapScreenRect((RectTransform)signClose.transform)
                            .Overlaps(TapScreenRect((RectTransform)save.transform)),
                        "Sign detail return and Rename actions remain separate at " + size);
                    Check(signClose.GetComponentInChildren<TMP_Text>(true).text == "Back to shop",
                        "Home-origin sign detail names its shop return at " + size);
                    yield return Capture("store-name-detail-" + size.x + "x" + size.y, 0);
                    save.onClick.Invoke();
                    var namingHelp = dialog.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Validation");
                    namingHelp.ForceMeshUpdate();
                    Check(input.gameObject.activeInHierarchy && input.text == new string('W', 24),
                        "Sign Rename opens the editable current name at " + size);
                    Check(namingHelp.text == "1–24 Latin letters, numbers or punctuation." &&
                          !namingHelp.isTextTruncated && !namingHelp.isTextOverflowing,
                        "Naming guidance states supported characters without clipping at " + size);
                    yield return Capture("store-name-edit-" + size.x + "x" + size.y, 0);
                    input.text = ""; yield return null;
                    Check(!save.interactable, "Invalid edit disables Save before sign detail is reopened at " + size);
                    dialog.Dismiss(); yield return null;
                    Check(!dialog.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                          !game.SettingsScreen.gameObject.activeSelf,
                        "Sign-origin close returns Home at " + size);
                    e.MutationAllowed = () => false;
                    inspectSign.onClick.Invoke(); yield return null;
                    Check(dialog.gameObject.activeSelf && fullName.text == new string('W', 24) &&
                          !save.gameObject.activeSelf && s.cash.CompareTo(beforeCash) == 0,
                        "Read-only player can inspect the full name without Rename at " + size);
                    Rect readonlyClose = TapScreenRect((RectTransform)signClose.transform);
                    Check(readonlyClose.width >= 48 && Mathf.Abs(readonlyClose.center.x - size.x * .5f) < 4,
                        "Read-only detail centers its sole return action at " + size);
                    yield return Capture("store-name-readonly-" + size.x + "x" + size.y, 0);
                    signClose.onClick.Invoke(); yield return null;
                    e.MutationAllowed = null;
                    Check(!dialog.gameObject.activeSelf && game.View.CurrentTab == "Home",
                        "Read-only sign detail closes to Home at " + size);
                    inspectSign.onClick.Invoke(); yield return null;
                    Check(save.gameObject.activeSelf && save.interactable &&
                          !TapScreenRect((RectTransform)signClose.transform)
                            .Overlaps(TapScreenRect((RectTransform)save.transform)),
                        "Rename recovers without button overlap after invalid edit and read-only view at " + size);
                    signClose.onClick.Invoke(); yield return null;
                    game.View.ShowSettings();
                    game.SettingsScreen.GetComponentsInChildren<Button>(true)
                        .Single(x => x.name == "settings-rename-store").onClick.Invoke();
                    Check(dialog.gameObject.activeSelf && input.gameObject.activeInHierarchy,
                        "Settings Rename still opens the editable name at " + size);
                    dialog.Dismiss(); yield return null;
                    Check(game.SettingsScreen.gameObject.activeSelf,
                        "Settings-origin Rename returns to Settings at " + size);
                    game.SettingsScreen.Close();
                    game.View.Switch("Products");
                    yield return new WaitForSecondsRealtime(.35f);
                    var productsScroll = game.ProductsScreen.GetComponentInChildren<ScrollRect>(true);
                    productsScroll.verticalNormalizedPosition = .4f;
                    Canvas.ForceUpdateCanvases();
                    float browseScroll = productsScroll.verticalNormalizedPosition;
                    hits.Clear(); EventSystem.current.RaycastAll(pointer, hits);
                    bool browseReachable = hits.Count > 0 &&
                        hits[0].gameObject.GetComponentInParent<Button>() == inspectSign;
                    Check(browseReachable, "Shared sign remains reachable while browsing Products at " + size);
                    beforeCash = s.cash; beforeTaps = s.manualTaps;
                    if (browseReachable) ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
                    yield return null;
                    Check(dialog.gameObject.activeSelf && game.View.CurrentTab == "Products" &&
                          s.cash.CompareTo(beforeCash) == 0 && s.manualTaps == beforeTaps &&
                          signClose.GetComponentInChildren<TMP_Text>(true).text == "Back to Products",
                        "Browse-origin sign inspection preserves Products without awarding cash at " + size);
                    var browseCloseLabel = signClose.GetComponentInChildren<TMP_Text>(true);
                    browseCloseLabel.ForceMeshUpdate();
                    Check(!TapScreenRect((RectTransform)signClose.transform)
                            .Overlaps(TapScreenRect((RectTransform)save.transform)) &&
                          !browseCloseLabel.isTextTruncated && !browseCloseLabel.isTextOverflowing,
                        "Products return and Rename remain separate and legible at " + size);
                    yield return Capture("store-name-detail-products-" + size.x + "x" + size.y, 0);
                    signClose.onClick.Invoke(); yield return null;
                    Check(!dialog.gameObject.activeSelf && game.View.CurrentTab == "Products" &&
                          game.ProductsScreen.gameObject.activeSelf &&
                          Mathf.Abs(productsScroll.verticalNormalizedPosition - browseScroll) < .001f,
                        "Browse-origin close restores Products and its scroll position at " + size);
                    game.View.Switch("Home");
                }
                tapUpgrade.multiplier = 1e12;
                s.reducedMotion = false;
                var popupPool = home.GetComponentsInChildren<RectTransform>(true)
                    .Where(x => x.name.StartsWith("Cash Popup ")).ToArray();
                foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(360, 640), new Vector2Int(450, 1000) })
                {
                    ResizeForReview(size.x, size.y);
                    game.View.Switch("Home");
                    yield return new WaitForSecondsRealtime(.5f);
                    foreach (var popup in popupPool) popup.gameObject.SetActive(false);
                    home.Refresh();
                    for (int tap = 0; tap < 8; tap++) home.TapFeedback();
                    yield return null;
                    var activePopup = popupPool.Single(x => x.gameObject.activeSelf);
                    var burstLabel = activePopup.GetComponentInChildren<TMP_Text>(true);
                    var rewardMaterial = burstLabel.fontSharedMaterial;
                    Color outline = rewardMaterial != null && rewardMaterial.HasProperty("_OutlineColor")
                        ? rewardMaterial.GetColor("_OutlineColor") : Color.clear;
                    float outlineWidth = rewardMaterial != null && rewardMaterial.HasProperty("_OutlineWidth")
                        ? rewardMaterial.GetFloat("_OutlineWidth") : 0;
                    Check(rewardMaterial != null && rewardMaterial.name.StartsWith("Tap Reward Material") &&
                          outline.a >= .99f && outlineWidth >= .2f &&
                          TapContrast(Color.white, outline) >= 4.5f,
                        "Burst popup uses its saved, high-contrast outline at " + size);
                    bool burstClear = TapPopupClearOfShelves(home, out string burstGeometry);
                    Check(burstLabel.text == "+₹" + e.TapIncome * 8 && burstLabel.color.a >= .99f && burstClear,
                        "Eight-tap reward aggregates without covering shelves at " + size + " (" + burstGeometry + ")");
                    yield return Capture("home-tap-burst-" + size.x + "x" + size.y, .12f);
                    bool movingClear = TapPopupClearOfShelves(home, out string movingGeometry);
                    Check(burstLabel.color.a >= .99f && movingClear,
                        "Moving burst stays fully opaque between hint and shelves at " + size + " (" + movingGeometry + ")");
                    yield return new WaitForSecondsRealtime(.2f);
                    bool middleClear = TapPopupClearOfShelves(home, out string middleGeometry);
                    Check(burstLabel.color.a >= .99f && middleClear,
                        "Mid-animation burst stays fully opaque between hint and shelves at " + size + " (" + middleGeometry + ")");
                    yield return Capture("home-tap-mid-" + size.x + "x" + size.y, 0);
                    yield return new WaitForSecondsRealtime(.45f);
                    Check(!activePopup.gameObject.activeSelf,
                        "Burst popup clears after its readable dwell and brief exit at " + size);
                }
                s.reducedMotion = true;
                ResizeForReview(360, 540);
                yield return new WaitForSecondsRealtime(.5f);
                home.TapFeedback();
                var staticPopup = popupPool.Single(x => x.gameObject.activeSelf);
                var staticLabel = staticPopup.GetComponentInChildren<TMP_Text>(true);
                Vector2 staticPosition = staticPopup.anchoredPosition;
                yield return new WaitForSecondsRealtime(.3f);
                Check(staticPopup.gameObject.activeSelf && staticLabel.color.a >= .99f &&
                      (staticPopup.anchoredPosition - staticPosition).sqrMagnitude < .01f,
                    "Reduced-motion tap reward remains static and fully opaque during its dwell");
                yield return new WaitForSecondsRealtime(.4f);
                Check(!staticPopup.gameObject.activeSelf,
                    "Reduced-motion tap reward clears without a fade");
            }
            finally
            {
                tapUpgrade.multiplier = originalMultiplier;
                s.reducedMotion = originalReducedMotion;
                foreach (var popup in home.GetComponentsInChildren<RectTransform>(true)
                    .Where(x => x.name.StartsWith("Cash Popup "))) popup.gameObject.SetActive(false);
            }
            yield return LegacyNameChecks();
            yield return HtmlAmbientChecks();
            yield return HtmlTapChecks();
        }

        IEnumerator LegacyNameChecks()
        {
            var engine = game.Engine;
            var state = engine.State;
            string previousRaw = state.storeName;
            bool previousHandled = state.storeNamePromptHandled;
            bool previousMotion = state.reducedMotion;
            const string legacyRaw = "दीपावली दुकान";
            const string detailCopy = "Some characters can't show here. Your name is still saved.";
            var home = game.HomeScreen;
            var sign = home.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Shop Sign");
            var inspect = home.GetComponentsInChildren<Button>(true).Single(x => x.name == "inspect-store-name");
            var dialog = game.StoreNameScreen;
            var input = dialog.GetComponentInChildren<TMP_InputField>(true);
            var placeholder = input.placeholder as TMP_Text;
            var title = dialog.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Title");
            var description = dialog.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Description");
            var fullName = dialog.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Full Name");
            var help = dialog.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Validation");
            var choose = dialog.GetComponentsInChildren<Button>(true).Single(x => x.name == "save-store-name");
            var keep = dialog.GetComponentsInChildren<Button>(true).Single(x => x.name == "cancel-store-name");
            var chooseLabel = choose.GetComponentInChildren<TMP_Text>(true);
            var keepLabel = keep.GetComponentInChildren<TMP_Text>(true);
            var settings = game.SettingsScreen;
            var settingsChoose = settings.GetComponentsInChildren<Button>(true).Single(x => x.name == "settings-rename-store");
            var settingsStatus = settingsChoose.transform.parent.Find("Description").GetComponent<TMP_Text>();
            var settingsAction = settingsChoose.GetComponentInChildren<TMP_Text>(true);
            try
            {
                state.reducedMotion = true;
                foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(450, 1000) })
                {
                    ResizeForReview(size.x, size.y);
                    game.View.Switch("Home");
                    state.storeName = legacyRaw;
                    state.storeNamePromptHandled = false;
                    home.Refresh();
                    yield return new WaitForSecondsRealtime(.3f);
                    game.View.ShowStoreName(true);
                    Check(!dialog.gameObject.activeSelf && state.storeName == legacyRaw &&
                          state.NeedsStoreNameUpdate && sign.text == GameState.StoreNameUpdateRequired,
                        "Existing unsupported name shows sign status without first-visit overwrite at " + size);
                    Amount beforeCash = state.cash; long beforeTaps = state.manualTaps;
                    inspect.onClick.Invoke(); yield return null;
                    description.ForceMeshUpdate(); fullName.ForceMeshUpdate();
                    float detailPixels = description.textInfo.characterCount > 0 ?
                        description.textInfo.characterInfo[0].pointSize * description.canvas.rootCanvas.scaleFactor : 0;
                    Check(dialog.gameObject.activeSelf && title.text == GameState.StoreNameUpdateRequired &&
                          description.text == detailCopy && !description.isTextTruncated &&
                          !description.isTextOverflowing && detailPixels >= 11.5f &&
                          fullName.text == GameState.StoreNameUpdateRequired &&
                          chooseLabel.text == "Choose new name" && keepLabel.text == "Keep name" &&
                          state.storeName == legacyRaw && state.cash.CompareTo(beforeCash) == 0 &&
                          state.manualTaps == beforeTaps,
                        "Legacy sign detail explains preserved raw name and offers replacement at " + size +
                        " (copy " + detailPixels.ToString("0.0") + "px)");
                    yield return Capture("store-name-legacy-detail-" + size.x + "x" + size.y, 0);
                    choose.onClick.Invoke(); yield return null;
                    help.ForceMeshUpdate();
                    float helpPixels = help.textInfo.characterCount > 0 ?
                        help.textInfo.characterInfo[0].pointSize * help.canvas.rootCanvas.scaleFactor : 0;
                    Check(input.gameObject.activeInHierarchy && input.text == "" && !choose.interactable &&
                          title.text == "Choose a new name" && keepLabel.text == "Keep name" &&
                          description.text == "Old name stays until you save." &&
                          placeholder != null && placeholder.text == "Enter new store name" &&
                          help.text == "Enter a Latin name to replace it." &&
                          !help.isTextTruncated && !help.isTextOverflowing && helpPixels >= 11.5f &&
                          state.storeName == legacyRaw,
                        "Legacy replacement starts blank and keeps old raw until Save at " + size +
                        " (help " + helpPixels.ToString("0.0") + "px)");
                    yield return Capture("store-name-legacy-edit-" + size.x + "x" + size.y, 0);
                    input.text = legacyRaw; yield return null;
                    Check(!choose.interactable, "Unsupported legacy characters cannot be resaved at " + size);
                    keep.onClick.Invoke(); yield return null;
                    Check(!dialog.gameObject.activeSelf && state.storeName == legacyRaw &&
                          game.View.CurrentTab == "Home", "Keep name preserves raw value from Home at " + size);

                    engine.MutationAllowed = () => false;
                    inspect.onClick.Invoke(); yield return null;
                    Check(dialog.gameObject.activeSelf && description.text == detailCopy &&
                          !choose.gameObject.activeSelf && keepLabel.text == "Keep name",
                        "Read-only legacy player can inspect and keep saved name at " + size);
                    yield return Capture("store-name-legacy-readonly-" + size.x + "x" + size.y, 0);
                    keep.onClick.Invoke(); yield return null;
                    engine.MutationAllowed = null;
                    Check(state.storeName == legacyRaw && !dialog.gameObject.activeSelf,
                        "Read-only inspection leaves raw name untouched at " + size);

                    game.View.ShowSettings(); settings.Refresh();
                    settingsAction.ForceMeshUpdate();
                    float actionPixels = settingsAction.textInfo.characterCount > 0 ?
                        settingsAction.textInfo.characterInfo[0].pointSize * settingsAction.canvas.rootCanvas.scaleFactor : 0;
                    Check(settingsStatus.text == GameState.StoreNameUpdateRequired &&
                          settingsAction.text == "Fix name" && settingsChoose.interactable &&
                          !settingsAction.isTextTruncated && !settingsAction.isTextOverflowing && actionPixels >= 11.5f,
                        "Settings names the problem and readable replacement action at " + size +
                        " (action " + actionPixels.ToString("0.0") + "px)");
                    yield return Capture("store-name-legacy-settings-" + size.x + "x" + size.y, 0);
                    settingsChoose.onClick.Invoke(); yield return null;
                    Check(dialog.gameObject.activeSelf && input.text == "" && !choose.interactable,
                        "Settings replacement starts blank at " + size);
                    keep.onClick.Invoke(); yield return null;
                    Check(settings.gameObject.activeSelf && state.storeName == legacyRaw,
                        "Settings Keep returns to Settings without replacing raw at " + size);
                    settings.Close();

                    game.View.Switch("Products");
                    yield return new WaitForSecondsRealtime(.3f);
                    var productsScroll = game.ProductsScreen.GetComponentInChildren<ScrollRect>(true);
                    productsScroll.verticalNormalizedPosition = .4f;
                    Canvas.ForceUpdateCanvases();
                    float browseScroll = productsScroll.verticalNormalizedPosition;
                    inspect.onClick.Invoke(); yield return null;
                    Check(dialog.gameObject.activeSelf && game.View.CurrentTab == "Products" &&
                          description.text == detailCopy && state.storeName == legacyRaw,
                        "Browse sign exposes legacy status without changing Products at " + size);
                    keep.onClick.Invoke(); yield return null;
                    Check(game.View.CurrentTab == "Products" && game.ProductsScreen.gameObject.activeSelf &&
                          Mathf.Abs(productsScroll.verticalNormalizedPosition - browseScroll) < .001f &&
                          state.storeName == legacyRaw,
                        "Browse Keep preserves Products scroll and raw name at " + size);
                    game.View.Switch("Home");

                    game.View.ShowSettings(); settingsChoose.onClick.Invoke();
                    input.text = "José Fiesta"; yield return null;
                    Check(choose.interactable, "Supported accented replacement enables Save at " + size);
                    choose.onClick.Invoke(); yield return null;
                    Check(state.storeName == "José Fiesta" && state.StoreName == "José Fiesta" &&
                          !state.NeedsStoreNameUpdate && state.storeNamePromptHandled &&
                          settings.gameObject.activeSelf,
                        "Valid replacement clears legacy status and returns Settings at " + size);
                    settings.Close(); home.Refresh();
                }
            }
            finally
            {
                engine.MutationAllowed = null;
                state.storeName = previousRaw;
                state.storeNamePromptHandled = previousHandled;
                state.reducedMotion = previousMotion;
                home.Refresh();
            }
        }

        static float TapContrast(Color light, Color dark)
        {
            float Luminance(Color color) => .2126f * Linear(color.r) +
                .7152f * Linear(color.g) + .0722f * Linear(color.b);
            float Linear(float channel) => channel <= .04045f ? channel / 12.92f :
                Mathf.Pow((channel + .055f) / 1.055f, 2.4f);
            float a = Luminance(light), b = Luminance(dark);
            return (Mathf.Max(a, b) + .05f) / (Mathf.Min(a, b) + .05f);
        }

        static Rect TapScreenRect(RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float left = float.PositiveInfinity, bottom = float.PositiveInfinity;
            float right = float.NegativeInfinity, top = float.NegativeInfinity;
            foreach (var corner in corners)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(null, corner);
                left = Mathf.Min(left, point.x); right = Mathf.Max(right, point.x);
                bottom = Mathf.Min(bottom, point.y); top = Mathf.Max(top, point.y);
            }
            return Rect.MinMaxRect(left, bottom, right, top);
        }

        static Rect TapVisualRect(TMP_Text label)
        {
            label.ForceMeshUpdate();
            float left = float.PositiveInfinity, bottom = float.PositiveInfinity;
            float right = float.NegativeInfinity, top = float.NegativeInfinity;
            for (int i = 0; i < label.textInfo.characterCount; i++)
            {
                var character = label.textInfo.characterInfo[i];
                if (!character.isVisible) continue;
                foreach (var corner in new[] { character.bottomLeft, character.topLeft,
                             character.topRight, character.bottomRight })
                {
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(null, label.transform.TransformPoint(corner));
                    left = Mathf.Min(left, point.x); right = Mathf.Max(right, point.x);
                    bottom = Mathf.Min(bottom, point.y); top = Mathf.Max(top, point.y);
                }
            }
            return float.IsInfinity(left) ? Rect.zero : Rect.MinMaxRect(left, bottom, right, top);
        }

        static bool TapPopupClearOfShelves(ImportedHomeScreen home, out string geometry)
        {
            var active = home.GetComponentsInChildren<RectTransform>(true)
                .Where(x => x.name.StartsWith("Cash Popup ") && x.gameObject.activeInHierarchy).ToArray();
            if (active.Length != 1) { geometry = "active popups=" + active.Length; return false; }
            var label = active[0].GetComponentInChildren<TMP_Text>(true);
            Rect host = TapScreenRect(active[0]);
            Rect visual = TapVisualRect(label);
            var texts = home.GetComponentsInChildren<TMP_Text>(true);
            Rect hint = TapScreenRect((RectTransform)texts.Single(x => x.name == "Hint Text").transform.parent);
            Rect keeper = TapScreenRect(home.GetComponentsInChildren<RectTransform>(true)
                .Single(x => x.name == "Shopkeeper"));
            var shelfNames = texts.Where(x => x.name.StartsWith("shelf-name-")).ToArray();
            float shelfBottom = shelfNames.Min(x => TapScreenRect(x.rectTransform).yMin);
            bool clear = visual.width > 0 && visual.height > 0 &&
                visual.xMin >= host.xMin - 1 && visual.xMax <= host.xMax + 1 &&
                visual.yMin >= host.yMin - 1 && visual.yMax <= host.yMax + 1 &&
                visual.xMin >= hint.xMin - 1 && visual.xMax <= Mathf.Min(hint.xMax, keeper.xMin) - 1 &&
                visual.yMin >= hint.yMax + 1 && visual.yMax <= shelfBottom - 1 &&
                shelfNames.All(x => !visual.Overlaps(TapScreenRect(x.rectTransform))) &&
                texts.Where(x => x.name == "Empty Mark" && x.gameObject.activeInHierarchy)
                    .All(x => !visual.Overlaps(TapScreenRect(x.rectTransform))) &&
                !label.isTextTruncated && !label.isTextOverflowing && !label.raycastTarget;
            geometry = "visual=" + visual + ", hintTop=" + hint.yMax.ToString("0.0") +
                ", shelfBottom=" + shelfBottom.ToString("0.0") + ", keeperLeft=" + keeper.xMin.ToString("0.0");
            return clear;
        }
    }
}
#endif
