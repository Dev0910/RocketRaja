#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        static readonly BindingFlags QualityPrivate = BindingFlags.NonPublic | BindingFlags.Instance;

        IEnumerator QualityChecks()
        {
            var engine = game.Engine;
            var state = engine.State;
            state.reducedMotion = true;
            state.sound = state.music = false;
            state.storeNamePromptHandled = true;
            state.storeName = GameState.DefaultStoreName;
            state.cash = 1e100;
            state.reputation = 1000000;
            state.lifetimeReputation = state.reputation;
            state.onboarding = 4;
            foreach (var product in state.products) product.level = 25;
            state.upgrades.Clear();
            foreach (var skill in engine.Data.upgrades.Where(x => x.effect == Effect.Skill && x.tier == 0))
                state.upgrades.Add(skill.id);
            Check(state.Valid(), "Quality gameplay fixture satisfies all saved-state invariants before any saved operation");
            QualityCloseModals();
            game.View.Switch("Home");

            game.View.ShowSkills();
            foreach (int rank in new[] { 0, 1, 5 })
            {
                state.legacy[11].level = rank;
                game.SkillsScreen.Refresh();
                for (int i = 0; i < 3; i++)
                {
                    var label = game.SkillsScreen.GetComponentsInChildren<TMP_Text>(true)
                        .Single(x => x.name == "guide-cooldown-" + i);
                    double expected = engine.Data.skills[i].cooldown *
                        (1 - engine.Data.legacy[11].valuePerRank * rank);
                    Check(Math.Abs(engine.SkillCooldown(i) - expected) < .00001 &&
                          label.text == "Cooldown " + GameView.Duration(expected),
                        "Skills guide reports effective cooldown for skill " + i + " at Legacy rank " + rank);
                    state.skills[i].activeUntil = state.skills[i].readyAt = 0;
                    double activatedAt = state.now;
                    Check(engine.ActivateSkill(i) && Math.Abs(state.skills[i].readyAt - activatedAt - expected) < .00001,
                        "Skill " + i + " activation uses the same cooldown at Legacy rank " + rank);
                    state.skills[i].activeUntil = state.skills[i].readyAt = 0;
                }
            }
            state.legacy[11].level = 0;
            game.SkillsScreen.Close();

            var authorizationField = typeof(GameEngine).GetField("<MutationAllowed>k__BackingField", QualityPrivate);
            var authorization = (Func<bool>)authorizationField.GetValue(engine);
            engine.MutationAllowed = () => false;
            foreach (string tab in new[] { "Products", "Player", "Legacy" })
            {
                game.View.Switch(tab);
                var root = QualityTab(tab);
                var buys = QualityPurchaseButtons(root, tab);
                Check(buys.Length > 0 && buys.All(x => !x.interactable),
                    tab + " purchase controls show disabled state when mutation is blocked despite ample currency");
            }
            // Preserve the controller's session and suspension checks exactly.
            engine.MutationAllowed = authorization;
            foreach (string tab in new[] { "Products", "Player", "Legacy" })
            {
                game.View.Switch(tab);
                var buys = QualityPurchaseButtons(QualityTab(tab), tab);
                Check(buys.Any(x => x.interactable), tab + " purchase controls recover when mutation resumes");
            }
            game.View.Switch("Home");
            yield return QualityRewardRetryChecks();
            yield return QualityPendingRewardChecks();

            game.View.ShowStoreName();
            var input = game.StoreNameScreen.GetComponentInChildren<TMP_InputField>(true);
            var surface = input.transform.Find("Input Surface")?.GetComponent<Image>();
            var border = input.GetComponent<Image>();
            var focus = input.transform.Find("Input Focus Underline");
            var placeholder = input.placeholder as TMP_Text;
            var ivory = game.StoreNameScreen.GetComponentsInChildren<Image>(true)
                .FirstOrDefault(x => x.name == "Ivory Surface");
            Check(surface != null && border != null && ivory != null &&
                  TapContrast(border.color, surface.color) >= 3 && TapContrast(border.color, ivory.color) >= 3,
                "Store name field border contrasts at least 3:1 with its inner and outer surfaces");
            Check(placeholder != null && surface != null && TapContrast(placeholder.color, surface.color) >= 4.5f,
                "Store name placeholder contrasts at least 4.5:1 with the input surface");
            input.onSelect.Invoke(input.text);
            Check(focus != null && focus.gameObject.activeSelf, "Store name selection reveals the saved focus underline");
            input.onDeselect.Invoke(input.text);
            Check(focus != null && !focus.gameObject.activeSelf, "Store name deselection clears the saved focus underline");
            game.StoreNameScreen.Dismiss();
            QualitySettingsRefreshCheck();
            yield return QualitySettingsAppearanceChecks();
            QualityBootstrapPreferenceCheck();
            state.cash = 738950000;
            state.reputation = 35;
            // Preserve the normal toast dwell; baseline captures begin after it ends.
            yield return new WaitForSecondsRealtime(3.1f);
            Check(!game.HomeScreen.HasNotice, "Transient reward notices expire before baseline matrix captures");

            foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(360, 640),
                         new Vector2Int(390, 844), new Vector2Int(450, 916), new Vector2Int(540, 1200) })
            {
                ResizeForReview(size.x, size.y);
                QualityCloseModals();
                game.View.Switch("Home");
                // Resize, safe area, panel fit and layout settle on separate frames.
                yield return null; yield return null; yield return null;
                Canvas.ForceUpdateCanvases();
                Check(Screen.width == size.x && Screen.height == size.y, "Quality viewport activated: " + size.x + "x" + size.y);
                var prefix = "quality-" + size.x + "x" + size.y + "-";
                QualityGeometry(game.HomeScreen.gameObject, "Home", size);
                yield return Capture(prefix + "home", 0);
                foreach (string tab in new[] { "Products", "Player", "Legacy" })
                {
                    game.View.Switch(tab);
                    yield return QualityReviewScreen(QualityTab(tab), tab, prefix, size);
                }
                game.View.Switch("Home");

                if (size == new Vector2Int(360, 540) || size == new Vector2Int(450, 916))
                    yield return QualityQuantityChecks(prefix, size);

                game.View.ShowSettings();
                yield return QualityReviewScreen(game.SettingsScreen.gameObject, "Settings", prefix, size);
                QualityCloseModals();
                game.PrivacyScreen.Bind(game, null); game.PrivacyScreen.Open();
                yield return QualityReviewScreen(game.PrivacyScreen.gameObject, "Privacy", prefix, size);
                QualityCloseModals();
                game.View.ShowStatistics();
                yield return QualityReviewScreen(game.StatisticsScreen.gameObject, "Statistics", prefix, size);
                QualityCloseModals();
                game.View.ShowSkills();
                yield return QualityReviewScreen(game.SkillsScreen.gameObject, "Skills", prefix, size);
                if (size == new Vector2Int(360, 540) || size == new Vector2Int(450, 916))
                    yield return QualitySkillsStates(prefix, size);
                QualityCloseModals();
                game.View.ShowProductDetail(0);
                yield return QualityReviewScreen(game.ProductDetailScreen.gameObject, "Product-detail", prefix, size);
                QualityCloseModals();
                game.View.ShowPrestige();
                yield return QualityReviewScreen(game.PrestigeScreen.gameObject, "Prestige", prefix, size);
                QualityCloseModals();
                state.pendingOfflineId = "quality-viewport-" + size.x + "-" + size.y;
                state.pendingOfflineReward = new Amount(12345);
                game.View.ShowOffline();
                yield return QualityReviewScreen(game.OfflineScreen.gameObject, "Offline", prefix, size);
                QualityCloseModals();
                game.SaveRecoveryScreen.Bind(game); game.SaveRecoveryScreen.Open();
                yield return QualityReviewScreen(game.SaveRecoveryScreen.gameObject, "Save-recovery", prefix, size);
                QualityCloseModals();
                game.View.ShowStoreName();
                yield return QualityReviewScreen(game.StoreNameScreen.gameObject, "Store-name", prefix, size);
                if (size == new Vector2Int(360, 540) || size == new Vector2Int(450, 916))
                    yield return QualityStoreNameFocus(prefix, size);
                QualityCloseModals();
                game.View.ShowStoreNameDetail();
                yield return QualityReviewScreen(game.StoreNameScreen.gameObject, "Store-name-detail", prefix, size);
                QualityCloseModals();
                if (size == new Vector2Int(360, 540) || size == new Vector2Int(450, 916))
                    yield return QualitySkillUpgradeCopy(prefix, size);
            }
            state.pendingOfflineId = null;
            state.pendingOfflineReward = Amount.Zero;
            QualityCloseModals();
            game.View.Switch("Home");
            checks.Add("LIMIT Geometry and pixels are desktop Game View evidence, not Android dp/sp or device accessibility validation.");
            checks.Add("LIMIT Native captures cover the remaining Main screen presenters plus store-name detail; Bootstrap scene and real cloud are not loaded.");
            checks.Add("LIMIT Contrast checks cover authored solid input colors; sprite gradients and global text contrast require visual review.");
        }

        GameObject QualityTab(string tab) => tab == "Products" ? game.ProductsScreen.gameObject :
            tab == "Player" ? game.PlayerScreen.gameObject : game.LegacyScreen.gameObject;

        static Button[] QualityPurchaseButtons(GameObject root, string tab) => root.GetComponentsInChildren<Button>(true)
            .Where(x => x.gameObject.activeInHierarchy && (tab == "Products" ? x.name.StartsWith("purchase-", StringComparison.Ordinal) :
                tab == "Legacy" ? x.name.StartsWith("buy-perk-", StringComparison.Ordinal) :
                x.name == "buy-tap" || x.name.StartsWith("buy-upgrade-", StringComparison.Ordinal) || x.name.Contains("buy-all") ||
                x.name == "Buy" && x.transform.parent.name.StartsWith("Unlockable Row", StringComparison.Ordinal))).ToArray();

        void QualityCloseModals()
        {
            foreach (var root in new[] { game.SettingsScreen.gameObject, game.PrivacyScreen.gameObject,
                         game.StatisticsScreen.gameObject, game.SkillsScreen.gameObject, game.ProductDetailScreen.gameObject,
                         game.PrestigeScreen.gameObject, game.OfflineScreen.gameObject,
                         game.SaveRecoveryScreen.gameObject, game.StoreNameScreen.gameObject })
                root.SetActive(false);
            game.ModalPaused = false;
        }

        IEnumerator QualityRewardRetryChecks()
        {
            var state = game.Engine.State;
            state.pendingOfflineId = "quality-retired-ad";
            state.pendingOfflineReward = new Amount(500);
            var cash = state.cash;
            int receipts = state.claimedRewards.Count;
            game.View.ShowOffline();
            game.OfflineScreen.RequestVideo(); game.OfflineScreen.RequestVideo();
            Check(state.cash.CompareTo(cash) == 0 && state.claimedRewards.Count == receipts && state.pendingOfflineId == "quality-retired-ad",
                "Retired offline events cannot add a bonus or consume the return summary");
            var close = QualityField<Button>(game.OfflineScreen, "closeButton");
            var exit = QualityField<Button>(game.OfflineScreen, "continueButton");
            Check(close.interactable && exit.interactable, "Return summary never waits for an ad callback");
            game.OfflineScreen.Dismiss();
            Check(state.cash.CompareTo(cash) == 0 && string.IsNullOrEmpty(state.pendingOfflineId), "Closing keeps all settled offline cash");
            game.View.ShowSponsor();
            Check(game.SponsorScreen == null || !game.SponsorScreen.gameObject.activeSelf, "Retired Sponsor cannot open");
            yield return null;
        }

        static T QualityField<T>(object owner, string name) =>
            (T)owner.GetType().GetField(name, QualityPrivate).GetValue(owner);

        IEnumerator QualityStoreNameFocus(string prefix, Vector2Int size)
        {
            var input = game.StoreNameScreen.GetComponentInChildren<TMP_InputField>(true);
            var focus = input.transform.Find("Input Focus Underline");
            string previous = input.text;
            input.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
            input.SetTextWithoutNotify("");
            yield return null; yield return null;
            Check(input.placeholder.gameObject.activeInHierarchy && focus != null && !focus.gameObject.activeSelf,
                "Empty store-name field shows its placeholder before focus at " + size);
            QualityGeometry(game.StoreNameScreen.gameObject, "Store-name empty", size);
            yield return Capture(prefix + "store-name-empty", 0);
            EventSystem.current.SetSelectedGameObject(input.gameObject);
            input.ActivateInputField();
            yield return null; yield return null;
            Check(input.isFocused && focus != null && focus.gameObject.activeInHierarchy,
                "Native store-name input selection visibly activates its focus underline at " + size);
            yield return Capture(prefix + "store-name-focused", 0);
            input.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
            input.SetTextWithoutNotify(previous);
            yield return null;
            Check(focus != null && !focus.gameObject.activeSelf, "Native store-name deselection removes its focus underline at " + size);
        }

        IEnumerator QualitySkillUpgradeCopy(string prefix, Vector2Int size)
        {
            var engine = game.Engine;
            var state = engine.State;
            var previous = state.upgrades.ToArray();
            int legacy = state.Legacy(11);
            game.View.Switch("Player");
            var ids = QualityField<string[]>(game.PlayerScreen, "visibleIds");
            var effects = QualityField<TMP_Text[]>(game.PlayerScreen, "upgradeEffects");
            game.PlayerScreen.Refresh();
            for (int i = 0; i < ids.Length; i++)
            {
                var upgrade = engine.Data.upgrades.FirstOrDefault(x => x.id == ids[i]);
                if (upgrade != null && upgrade.effect != Effect.Skill)
                    Check(effects[i].text == upgrade.Description(engine.Data),
                        "Player retains the existing non-skill upgrade description for " + upgrade.id);
            }
            try
            {
                // Own ordinary upgrades and first skill ranks, exposing all nine later
                // skill upgrades in the existing ten-row pool without a purchase/save.
                state.upgrades.Clear();
                state.upgrades.AddRange(engine.Data.upgrades.Where(x => x.effect != Effect.Skill || x.tier == 0).Select(x => x.id));
                foreach (int rank in new[] { 1, 5 })
                {
                    state.legacy[11].level = rank;
                    Check(state.Valid(), "Later skill-upgrade fixture remains a valid isolated state at Legacy rank " + rank);
                    game.PlayerScreen.Refresh();
                    Canvas.ForceUpdateCanvases();
                    yield return null; yield return null;
                    int reviewed = 0;
                    for (int i = 0; i < ids.Length; i++)
                    {
                        var upgrade = engine.Data.upgrades.FirstOrDefault(x => x.id == ids[i]);
                        if (upgrade == null) continue;
                        reviewed++;
                        string expected = "\nCooldown " + GameView.Duration(engine.SkillCooldown(upgrade.target));
                        effects[i].ForceMeshUpdate();
                        Check(upgrade.effect == Effect.Skill && upgrade.tier > 0 && effects[i].text.EndsWith(expected, StringComparison.Ordinal),
                            "Player " + upgrade.id + " reports effective cooldown at Legacy rank " + rank + " and " + size);
                        Check(!effects[i].isTextTruncated && !effects[i].isTextOverflowing,
                            "Player " + upgrade.id + " complete skill effect/cooldown fits its pooled text box at " + size);
                    }
                    Check(reviewed == 9, "All nine later skill upgrades are covered at Legacy rank " + rank + " and " + size);
                    yield return QualityReviewScreen(game.PlayerScreen.gameObject, "Player-skill-upgrades-rank-" + rank, prefix, size);
                }
            }
            finally
            {
                state.upgrades.Clear(); state.upgrades.AddRange(previous);
                state.legacy[11].level = legacy;
                game.PlayerScreen.Refresh();
                game.View.Switch("Home");
            }
        }

        IEnumerator QualityQuantityChecks(string prefix, Vector2Int size)
        {
            var priorCash = game.Engine.State.cash;
            game.Engine.State.cash = 1e100;
            foreach (string tab in new[] { "Products", "Player" })
            {
                game.View.Switch(tab);
                var root = QualityTab(tab);
                var opener = root.GetComponentsInChildren<Button>(true).Single(x => x.name == "buy-quantity");
                var menu = root.GetComponentsInChildren<UiQuantityDismiss>(true).Single();
                var blocker = root.GetComponentsInChildren<Button>(true).Single(x => x.name == "Quantity Outside Blocker");
                var label = root.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Quantity Label");
                var choices = menu.GetComponentsInChildren<Button>(true);
                Check(choices.Length == 4, tab + " quantity menu retains all four authored choices at " + size);
                opener.onClick.Invoke();
                yield return null;
                Check(menu.gameObject.activeInHierarchy && blocker.gameObject.activeInHierarchy,
                    tab + " quantity opener enables its menu and outside blocker at " + size);
                var full = new Rect(0, 0, Screen.width, Screen.height);
                Check(choices.Length == 4 && choices.All(x => QualityContains(full, TapScreenRect((RectTransform)x.transform))),
                    tab + " entire quantity menu fits all four choices on screen at " + size);
                yield return Capture(prefix + tab.ToLowerInvariant() + "-quantity", 0);
                foreach (var choice in choices)
                {
                    if (!menu.gameObject.activeInHierarchy) opener.onClick.Invoke();
                    yield return null;
                    var mark = choice.transform.Find("Selected Mark");
                    var pointer = new PointerEventData(EventSystem.current) { position = TapScreenRect((RectTransform)choice.transform).center };
                    var hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, hits);
                    var hit = hits.FirstOrDefault().gameObject;
                    bool front = hit != null && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit) == choice.gameObject;
                    Check(front, tab + " " + choice.name + " receives the frontmost raycast at " + size);
                    if (front) ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerClickHandler);
                    string expected = choice.name.Contains("MAX") ? "Qty Max" : "Qty " + choice.name.Substring(choice.name.IndexOf('×'));
                    Check(label.text == expected && mark != null && mark.gameObject.activeSelf && !menu.gameObject.activeSelf && !blocker.gameObject.activeSelf,
                        tab + " " + choice.name + " updates quantity and selection, then closes menu/blocker at " + size);
                }

                opener.onClick.Invoke();
                yield return null;
                Button outsidePurchase = null;
                var scroll = root.GetComponentInChildren<ScrollRect>(true);
                foreach (float position in new[] { 1f, .5f, 0f })
                {
                    if (scroll != null) { scroll.StopMovement(); scroll.verticalNormalizedPosition = position; }
                    Canvas.ForceUpdateCanvases(); yield return null;
                    var menuRect = TapScreenRect((RectTransform)menu.transform);
                    outsidePurchase = QualityPurchaseButtons(root, tab).FirstOrDefault(x => x.IsInteractable() &&
                        QualityContains(QualityVisible((RectTransform)x.transform), TapScreenRect((RectTransform)x.transform)) &&
                        !menuRect.Contains(TapScreenRect((RectTransform)x.transform).center));
                    if (outsidePurchase != null) break;
                }
                Check(outsidePurchase != null, tab + " has a visible purchase center outside its quantity menu at " + size);
                if (outsidePurchase != null)
                {
                    var state = game.Engine.State;
                    var cash = state.cash;
                    var levels = state.products.Select(x => x.level).ToArray();
                    int tapLevel = state.tapLevel, upgrades = state.upgrades.Count;
                    var pointer = new PointerEventData(EventSystem.current) { position = TapScreenRect((RectTransform)outsidePurchase.transform).center };
                    var hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, hits);
                    var hit = hits.FirstOrDefault().gameObject;
                    Check(hit != null && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit) == blocker.gameObject,
                        tab + " outside blocker intercepts an underlying purchase hit at " + size);
                    if (hit != null) ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerClickHandler);
                    Check(!menu.gameObject.activeSelf && !blocker.gameObject.activeSelf && state.cash.CompareTo(cash) == 0 &&
                          state.products.Select(x => x.level).SequenceEqual(levels) && state.tapLevel == tapLevel && state.upgrades.Count == upgrades,
                        tab + " outside tap closes the menu without purchasing or changing currency at " + size);
                }
                menu.gameObject.SetActive(false);
                if (scroll != null) scroll.verticalNormalizedPosition = 1;
            }
            game.Engine.State.cash = priorCash;
            game.View.Switch("Home");
        }

        IEnumerator QualitySkillsStates(string prefix, Vector2Int size)
        {
            var state = game.Engine.State;
            var definitions = game.Engine.Data.upgrades.Where(x => x.effect == Effect.Skill).Select(x => x.id).ToArray();
            var owned = state.upgrades.Where(definitions.Contains).ToArray();
            foreach (string fixture in new[] { "locked", "active", "waiting" })
            {
                state.upgrades.RemoveAll(definitions.Contains);
                if (fixture != "locked") state.upgrades.AddRange(owned);
                for (int i = 0; i < 3; i++)
                {
                    // Instant-sale skill has no sustained active phase; it goes straight to WAIT.
                    state.skills[i].activeUntil = fixture == "active" && i < 2 ? state.now + game.Engine.Data.skills[i].duration : 0;
                    state.skills[i].readyAt = fixture == "locked" ? 0 : state.now + game.Engine.SkillCooldown(i);
                }
                game.SkillsScreen.Refresh();
                yield return null;
                for (int i = 0; i < 3; i++)
                {
                    var label = game.SkillsScreen.GetComponentsInChildren<TMP_Text>(true)
                        .Single(x => x.name == "guide-status-" + i);
                    string expected = fixture == "locked" ? "LOCKED" : fixture == "active" && i < 2 ? "ACTIVE" : "WAIT";
                    Check(label.text.StartsWith(expected, StringComparison.Ordinal),
                        "Skills " + fixture + " fixture shows " + expected + " for skill " + i + " at " + size);
                }
                QualityGeometry(game.SkillsScreen.gameObject, "Skills " + fixture, size);
                yield return Capture(prefix + "skills-" + fixture, 0);
            }
            state.upgrades.RemoveAll(definitions.Contains);
            state.upgrades.AddRange(owned);
            foreach (var skill in state.skills) skill.activeUntil = skill.readyAt = 0;
            game.SkillsScreen.Refresh();
        }

        void QualitySettingsRefreshCheck()
        {
            var host = Cloud.PlayerSessionHost.Instance;
            Check(host != null && host.Session != null && !host.Session.IsAccount,
                "Settings status regression runs in an isolated guest smoke session");
            if (host == null || host.Session == null || host.Session.IsAccount) return;
            game.View.ShowSettings();
            var label = QualityField<TMP_Text>(game.SettingsScreen, "accountStatus");
            var eventField = typeof(Cloud.PlayerSessionHost).GetField("Changed", QualityPrivate);
            var callback = eventField?.GetValue(host) as Action;
            Check(callback != null && callback.GetInvocationList().Any(x => x.Target == game.SettingsScreen),
                "Visible Settings subscribes to session status changes");
            label.text = "Stale status fixture";
            callback?.Invoke();
            Check(label.text == "Guest · saved on this device", "Session change refreshes visible Settings account status immediately");
            game.SettingsScreen.Close();
            callback = eventField?.GetValue(host) as Action;
            Check(callback == null || callback.GetInvocationList().All(x => x.Target != game.SettingsScreen),
                "Hidden Settings releases its session status subscription");
        }

        IEnumerator QualitySettingsAppearanceChecks()
        {
            game.View.ShowSettings();
            var states = QualityField<TMP_Text[]>(game.SettingsScreen, "settingStates");
            var faces = QualityField<Image[]>(game.SettingsScreen, "settingFaces");
            var buttons = QualityField<Button[]>(game.SettingsScreen, "settingButtons");
            var state = game.Engine.State;
            for (int i = 0; i < buttons.Length; i++)
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    buttons[i].onClick.Invoke();
                    yield return null; yield return null;
                    bool on = i == 0 ? state.music : i == 1 ? state.sound : state.reducedMotion;
                    Check(states[i].text == (on ? "ON" : "OFF") &&
                          faces[i].overrideSprite.name == (on ? "Folk Blue" : "Folk Ivory") &&
                          Vector4.Distance(states[i].color, on ? Color.white : (Color)new Color32(22, 75, 128, 255)) < .01f,
                        "Settings toggle " + i + " keeps matching high-contrast " + (on ? "ON" : "OFF") + " artwork after feedback updates");
                }
            }
            state.reducedMotion = true; state.music = state.sound = false;
            game.SettingsScreen.Refresh();
            game.SettingsScreen.Close();
        }

        void QualityBootstrapPreferenceCheck()
        {
            var host = Cloud.PlayerSessionHost.Instance;
            if (host == null || host.GuestState == null || host.Session == null || host.Session.IsAccount)
            { Check(false, "Bootstrap feedback fixture requires the isolated guest smoke session"); return; }
            var fixture = new GameObject("Quality bootstrap feedback fixture", typeof(RectTransform), typeof(Button));
            fixture.SetActive(false);
            var rect = fixture.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(8, 12);
            var feedback = fixture.AddComponent<Cloud.BootstrapButtonFeedback>();
            typeof(Cloud.BootstrapButtonFeedback).GetField("label", QualityPrivate).SetValue(feedback, rect);
            fixture.SetActive(true);
            var sessionField = typeof(Cloud.PlayerSessionHost).GetField("<Session>k__BackingField", QualityPrivate);
            var session = host.Session;
            bool reduced = host.GuestState.reducedMotion;
            try
            {
                // No yield while the temporary fallback fixture clears the host session.
                sessionField.SetValue(host, null);
                host.GuestState.reducedMotion = true;
                var pointer = new PointerEventData(EventSystem.current) { pointerId = -1 };
                feedback.OnPointerDown(pointer);
                Check(rect.anchoredPosition == new Vector2(8, 12), "Bootstrap button honors GuestState Reduced Motion before a session is selected");
                host.GuestState.reducedMotion = false;
                feedback.OnPointerDown(pointer);
                Check(rect.anchoredPosition == new Vector2(8, 11), "Bootstrap button retains press feedback with motion enabled");
                host.GuestState.reducedMotion = true;
                typeof(Cloud.BootstrapButtonFeedback).GetMethod("Update", QualityPrivate).Invoke(feedback, null);
                Check(rect.anchoredPosition == new Vector2(8, 12), "Enabling Reduced Motion resets a currently pressed bootstrap label");
            }
            finally
            {
                sessionField.SetValue(host, session);
                host.GuestState.reducedMotion = reduced;
                Destroy(fixture);
            }
        }

        IEnumerator QualityReviewScreen(GameObject root, string name, string prefix, Vector2Int size)
        {
            var scrolls = root.GetComponentsInChildren<ScrollRect>(false);
            foreach (var scroll in scrolls) { scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; }
            Canvas.ForceUpdateCanvases();
            yield return null; yield return null;
            QualityGeometry(root, name + " top", size);
            yield return Capture(prefix + name.ToLowerInvariant(), 0);
            foreach (var scroll in scrolls)
            {
                if (scroll.content == null || scroll.viewport == null || scroll.content.rect.height <= scroll.viewport.rect.height + 1) continue;
                scroll.verticalNormalizedPosition = 0;
                Canvas.ForceUpdateCanvases();
                yield return null;
                QualityGeometry(root, name + " bottom", size);
                yield return Capture(prefix + name.ToLowerInvariant() + "-bottom", 0);
                scroll.verticalNormalizedPosition = 1;
            }
        }

        void QualityGeometry(GameObject root, string context, Vector2Int size)
        {
            Canvas.ForceUpdateCanvases();
            var full = new Rect(0, 0, Screen.width, Screen.height);
            var fixedOutside = new List<string>();
            var unreachableScroll = new List<string>();
            var missed = new List<string>();
            var hits = new List<RaycastResult>();
            foreach (var button in root.GetComponentsInChildren<Button>(false))
            {
                var rect = (RectTransform)button.transform;
                var bounds = TapScreenRect(rect);
                var scroll = button.GetComponentInParent<ScrollRect>();
                bool inScroll = scroll != null && scroll.content != null && button.transform.IsChildOf(scroll.content);
                if (!inScroll)
                {
                    if (!QualityContains(full, bounds)) fixedOutside.Add(button.name);
                }
                else
                {
                    var content = TapScreenRect(scroll.content);
                    var viewport = TapScreenRect(scroll.viewport);
                    if (!QualityContains(content, bounds) || bounds.width > viewport.width + 2 || bounds.height > viewport.height + 2)
                        unreachableScroll.Add(button.name);
                }
                var visible = QualityVisible(rect);
                if (!button.IsInteractable() || !QualityContains(visible, bounds)) continue;
                hits.Clear();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = bounds.center }, hits);
                var hit = hits.FirstOrDefault().gameObject;
                if (hit == null || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit) != button.gameObject) missed.Add(button.name);
            }
            Check(fixedOutside.Count == 0, context + " fixed buttons stay within " + size + QualityNames(fixedOutside));
            Check(unreachableScroll.Count == 0, context + " scrolling controls fit within reachable content" + QualityNames(unreachableScroll));
            Check(missed.Count == 0, context + " fully visible enabled buttons receive the frontmost pointer hit" + QualityNames(missed));
            var clipped = new List<string>();
            var smallCopy = new List<string>();
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(false))
            {
                if (text.color.a <= .01f || string.IsNullOrWhiteSpace(text.text) || text.GetComponentInParent<TMP_InputField>() != null) continue;
                var visible = QualityVisible(text.rectTransform);
                if (visible.width < 1 || visible.height < 1) continue;
                text.ForceMeshUpdate();
                if (text.isTextTruncated || text.isTextOverflowing) clipped.Add(text.name + " (" + text.text.Replace('\n', ' ') + ")");
                if (context.StartsWith("Products", StringComparison.Ordinal) ||
                    context.StartsWith("Player", StringComparison.Ordinal) ||
                    context.StartsWith("Legacy", StringComparison.Ordinal) ||
                    context.StartsWith("Store-name", StringComparison.Ordinal) && text.name == "Validation" ||
                    text.name.StartsWith("guide-effect-", StringComparison.Ordinal) ||
                    text.name.StartsWith("guide-cooldown-", StringComparison.Ordinal) ||
                    text.name.StartsWith("guide-status-", StringComparison.Ordinal))
                {
                    float pointSize = text.textInfo.characterCount > 0 ? text.textInfo.characterInfo.Take(text.textInfo.characterCount).Where(x => x.isVisible).Select(x => x.pointSize).DefaultIfEmpty(text.fontSize).Min() : text.fontSize;
                    float scale = text.canvas.rootCanvas.scaleFactor;
                    for (var parent = text.transform; parent != null && parent != text.canvas.rootCanvas.transform; parent = parent.parent)
                        scale *= Mathf.Abs(parent.localScale.y);
                    if (pointSize * scale < 12 - .01f) smallCopy.Add(text.name + " " + (pointSize * scale).ToString("0.0") + "px");
                }
            }
            Check(clipped.Count == 0, context + " visible essential text fits without TMP truncation or overflow" + QualityNames(clipped));
            if (new[] { "Skills", "Products", "Player", "Legacy", "Store-name" }.Any(x => context.StartsWith(x, StringComparison.Ordinal)))
                Check(smallCopy.Count == 0, context + " reviewed body/status copy renders at least 12 screen pixels" + QualityNames(smallCopy));
        }

        static Rect QualityVisible(RectTransform rect)
        {
            var visible = QualityIntersect(TapScreenRect(rect), new Rect(0, 0, Screen.width, Screen.height));
            foreach (var group in rect.GetComponentsInParent<CanvasGroup>())
                if (group.alpha <= .01f) return Rect.zero;
            for (var parent = rect.parent; parent != null; parent = parent.parent)
                if (parent.TryGetComponent<RectMask2D>(out var mask) && mask.isActiveAndEnabled ||
                    parent.TryGetComponent<Mask>(out var stencil) && stencil.isActiveAndEnabled)
                    visible = QualityIntersect(visible, TapScreenRect((RectTransform)parent));
            return visible;
        }

        static Rect QualityIntersect(Rect a, Rect b) => Rect.MinMaxRect(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin),
            Mathf.Max(Mathf.Max(a.xMin, b.xMin), Mathf.Min(a.xMax, b.xMax)),
            Mathf.Max(Mathf.Max(a.yMin, b.yMin), Mathf.Min(a.yMax, b.yMax)));
        static bool QualityContains(Rect parent, Rect child) => child.width > 0 && child.height > 0 &&
            child.xMin >= parent.xMin - 1 && child.xMax <= parent.xMax + 1 && child.yMin >= parent.yMin - 1 && child.yMax <= parent.yMax + 1;
        static string QualityNames(List<string> names) => names.Count == 0 ? "" : ": " + string.Join("; ", names);
    }
}
#endif
