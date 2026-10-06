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
        void CheckShelfTargets(Vector2Int size, string tab, Button[] buttons, float panelTop)
        {
            float narrowest = float.MaxValue;
            float shortestVisible = float.MaxValue;
            bool allReachable = true;
            var corners = new Vector3[4];
            var raycasts = new List<RaycastResult>();
            foreach (var button in buttons)
            {
                ((RectTransform)button.transform).GetWorldCorners(corners);
                float left = RectTransformUtility.WorldToScreenPoint(null, corners[0]).x;
                float right = RectTransformUtility.WorldToScreenPoint(null, corners[3]).x;
                float bottom = RectTransformUtility.WorldToScreenPoint(null, corners[0]).y;
                float top = RectTransformUtility.WorldToScreenPoint(null, corners[1]).y;
                narrowest = Mathf.Min(narrowest, right - left);
                float visibleBottom = Mathf.Max(bottom, panelTop);
                float visibleHeight = Mathf.Max(0, top - visibleBottom);
                shortestVisible = Mathf.Min(shortestVisible, visibleHeight);
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = new Vector2((left + right) * .5f, visibleBottom + visibleHeight * .5f)
                };
                raycasts.Clear();
                EventSystem.current.RaycastAll(pointer, raycasts);
                var hit = raycasts.FirstOrDefault().gameObject;
                allReachable &= hit != null && (hit == button.gameObject || hit.transform.IsChildOf(button.transform));
            }
            Check(buttons.Length == 8 && narrowest >= 48f && shortestVisible >= 48f,
                tab + " shelf touch targets stay at least 48 screen pixels wide and visible at " + size +
                " (min " + narrowest.ToString("0.0") + "×" + shortestVisible.ToString("0.0") + ")");
            Check(allReachable, tab + " shelf touch targets receive the frontmost raycast at " + size);
        }

        IEnumerator MaxPurchaseChecks()
        {
            var engine = game.Engine;
            engine.State.reducedMotion = true;
            engine.State.sound = engine.State.music = false;
            // Freeze passive income while checking exact affordability boundaries.
            game.ModalPaused = true;
            foreach (string tab in new[] { "Products", "Player" })
            {
                bool product = tab == "Products";
                game.View.Switch(tab);
                var root = product ? game.ProductsScreen.gameObject : game.PlayerScreen.gameObject;
                var buttons = root.GetComponentsInChildren<Button>(true);
                buttons.Single(x => x.name == "Quantity Buy MAX").onClick.Invoke();
                var buy = buttons.Single(x => x.name == (product ? "purchase-0" : "buy-tap"));
                var label = buy.GetComponentInChildren<TMP_Text>();
                foreach (int level in new[] { 0, 5 })
                {
                    if (product) engine.State.products[0].level = level;
                    else engine.State.tapLevel = level;
                    var cost = product ? engine.ProductCost(0, 1) : engine.TapCostFor(1);
                    string action = product && level == 0 ? "Stock shelf" : "Upgrade";
                    foreach (double fraction in new[] { 0d, .99d })
                    {
                        engine.State.cash = cost * fraction;
                        game.View.RefreshNow();
                        yield return new WaitForSecondsRealtime(.2f);
                        Check(!buy.interactable && label.text == "₹" + cost + "\n" + action + " ×1",
                            tab + " MAX at level " + level + " shows one-upgrade cost with " + fraction + " of required cash");
                        Check(buy.GetComponent<Image>().overrideSprite.name == "Folk Sand", tab + " unaffordable MAX stays greyed out");
                    }
                    engine.State.cash = cost;
                    game.View.RefreshNow();
                    Check(buy.interactable, tab + " MAX enables at the exact one-upgrade cost");
                    buy.onClick.Invoke();
                    var nextCost = product ? engine.ProductCost(0, 1) : engine.TapCostFor(1);
                    Check(!buy.interactable && label.text == "₹" + nextCost + "\nUpgrade ×1",
                        tab + " MAX immediately shows the next single cost after spending all cash");
                }
                engine.State.cash = (product ? engine.ProductCost(0, 1) : engine.TapCostFor(1)) * 10;
                game.View.RefreshNow();
                var quote = product ? engine.ProductQuote(0, int.MaxValue) : engine.TapQuote(int.MaxValue);
                Check(quote.quantity > 1 && buy.interactable && label.text == "₹" + quote.cost + "\nUpgrade ×" + quote.quantity,
                    tab + " MAX still quotes the affordable bulk quantity");
                if (product) engine.State.products[0].level = RocketRaja.Core.Catalog.MaxLevel;
                else engine.State.tapLevel = RocketRaja.Core.Catalog.MaxLevel;
                game.View.RefreshNow();
                Check(!buy.interactable && label.text == "Max level", tab + " actual level cap still displays Max level");
            }
            game.ModalPaused = false;
        }

        IEnumerator UiBugChecks()
        {
            game.Engine.State.reducedMotion = false;
            game.Engine.State.sound = game.Engine.State.music = false;
            foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(450, 916) })
            {
                ResizeForReview(size.x, size.y);
                game.View.Switch("Home");
                yield return new WaitForSecondsRealtime(.5f);
                // A fixed Game View size changes Screen, safe areas and layout in successive
                // frames. Inspect the settled layout, including in a throttled batch editor.
                yield return null;
                Canvas.ForceUpdateCanvases();
                yield return null;
                yield return null;
                var layout = game.HomeScreen.GetComponentInChildren<ImportedHomeLayout>();
                var content = (RectTransform)layout.transform;
                var viewport = (RectTransform)content.parent;
                Check(game.HomeScreen.GetComponentInChildren<ScrollRect>(true) == null,
                    "Home has no scroll input at " + size);
                Check(content.rect.height * content.localScale.y <= viewport.rect.height + 1 && content.anchoredPosition == Vector2.zero,
                    "Entire fixed Home dashboard fits at " + size);
                var homeText = game.HomeScreen.GetComponentsInChildren<TMP_Text>(true)
                    .Where(x => x.name == "Label" && x.transform.parent.name.StartsWith("nav-") ||
                                x.name.StartsWith("shelf-name-") || x.name == "Next Goal" || x.name == "Next Progress" ||
                                (x.name == "Name" || x.name == "Status") && x.transform.parent.name.StartsWith("home-skill-"))
                    .ToArray();
                foreach (var label in homeText) label.ForceMeshUpdate();
                Check(homeText.Length == 19 && homeText.All(x => !x.isTextTruncated),
                    "Navigation, shelf, skill and next-goal labels fit at " + size);
                var compactLabels = homeText.Where(x => x.name.StartsWith("shelf-name-") ||
                    (x.name == "Name" || x.name == "Status") && x.transform.parent.name.StartsWith("home-skill-")).ToArray();
                Check(compactLabels.Length == 14 && compactLabels.All(x =>
                    x.fontSize * x.canvas.rootCanvas.scaleFactor *
                    (x.transform.IsChildOf(content) ? content.localScale.x : 1f) >= 11f),
                    "Shelf and skill labels meet compact-screen type size at " + size +
                    " (dashboard scale " + content.localScale.x.ToString("0.00") + ")");
                var shelfLabels = homeText.Where(x => x.name.StartsWith("shelf-name-")).ToArray();
                Check(shelfLabels.Length == 8 && shelfLabels.All(x =>
                    !x.isTextTruncated && x.fontSize * x.canvas.rootCanvas.scaleFactor >= 12.5f),
                    "Every named shelf stays legible without truncation at " + size);
                var tapHint = game.HomeScreen.GetComponentsInChildren<TMP_Text>(true)
                    .Single(x => x.name == "Hint Text");
                string actualHint = tapHint.text;
                tapHint.ForceMeshUpdate();
                bool actualHintFits = !tapHint.isTextOverflowing && !tapHint.isTextTruncated &&
                    tapHint.textInfo.lineCount == 1;
                tapHint.text = "Tap for ₹" + new RocketRaja.Core.Amount(9.999e14);
                tapHint.ForceMeshUpdate();
                bool largeRewardFits = !tapHint.isTextOverflowing && !tapHint.isTextTruncated &&
                    tapHint.textInfo.lineCount == 1;
                tapHint.text = actualHint;
                tapHint.ForceMeshUpdate();
                Check(actualHintFits && largeRewardFits,
                    "Short-phone reward hint fits both current copy and a large suffix at " + size);
                var shelfButtons = game.HomeScreen.GetComponentsInChildren<Button>(true)
                    .Where(x => x.name.StartsWith("open-")).ToArray();
                CheckShelfTargets(size, "Home", shelfButtons, float.NegativeInfinity);
                var nextCopy = game.HomeScreen.GetComponentsInChildren<TMP_Text>(true)
                    .Where(x => x.name == "Next Action Kicker" || x.name == "Next Progress").ToArray();
                Check(nextCopy.Length == 2 && nextCopy.All(x =>
                    x.fontSize * x.canvas.rootCanvas.scaleFactor * content.localScale.x >= 10f),
                    "Starter offer and progress copy remain legible at " + size);
                var nextAction = game.HomeScreen.GetComponentsInChildren<Button>(true)
                    .Single(x => x.name == "Next Products Button");
                Check(nextAction.interactable && nextAction.gameObject.activeInHierarchy,
                    "Home next-goal action is visible and usable at " + size);
                var settings = game.HomeScreen.GetComponentsInChildren<Button>(true).Single(x => x.name == "settings-button");
                var targetCorners = new Vector3[4];
                ((RectTransform)settings.transform).GetWorldCorners(targetCorners);
                float settingsHeight = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null, targetCorners[0]),
                    RectTransformUtility.WorldToScreenPoint(null, targetCorners[1]));
                Check(settingsHeight >= 44f, "Home Settings hit area is at least 44 pixels tall at " + size);
                var tap = game.HomeScreen.GetComponentInChildren<ShopTapTarget>();
                var popup = game.HomeScreen.GetComponentsInChildren<RectTransform>(true).Where(x => x.name.StartsWith("Cash Popup ")).ToArray();
                foreach (var item in popup) item.gameObject.SetActive(false);
                var point = RectTransformUtility.WorldToScreenPoint(null, tap.transform.TransformPoint(((RectTransform)tap.transform).rect.center));
                point += new Vector2(23, 17);
                var pointer = new PointerEventData(EventSystem.current) { pointerId = -1, position = point };
                long tapsBeforeFeedback = game.Engine.State.manualTaps;
                Amount cashBeforeFeedback = game.Engine.State.cash;
                Amount tapReward = game.Engine.TapIncome;
                tap.OnPointerDown(pointer); tap.OnPointerUp(pointer);
                var active = popup.Single(x => x.gameObject.activeSelf);
                Check(game.Engine.State.manualTaps == tapsBeforeFeedback + 1 &&
                      game.Engine.State.cash.CompareTo(cashBeforeFeedback + tapReward) == 0 &&
                      active.GetComponentInChildren<TMP_Text>().text == "+₹" + tapReward,
                    "Pointer tap immediately awards and displays its exact cash reward at " + size);
                Check(TapPopupClearOfShelves(game.HomeScreen, out string pointerGeometry),
                    "Pointer reward stays on the counter at " + size + " (" + pointerGeometry + ")");
                Check(active.GetComponentInChildren<TMP_Text>().color == Color.white &&
                      !active.GetComponentInChildren<TMP_Text>().raycastTarget,
                    "Cash popup stays contrasting and does not block input");
                Rect worldScreen = TapScreenRect((RectTransform)tap.transform);
                Rect hintScreen = TapScreenRect((RectTransform)tapHint.transform.parent);
                float counterY = (hintScreen.yMax + shelfLabels.Min(x => TapScreenRect(x.rectTransform).yMin)) * .5f;
                foreach (float counterX in new[] { worldScreen.xMin + 12, worldScreen.xMax - 12 })
                {
                    pointer.position = new Vector2(counterX, counterY);
                    tap.OnPointerDown(pointer); tap.OnPointerUp(pointer);
                    Check(TapPopupClearOfShelves(game.HomeScreen, out string edgeGeometry),
                        "Edge tap reward stays clear of shelves at " + size + " (" + edgeGeometry + ")");
                }
                yield return Capture("ui-fixed-home-" + size.x, 0);
                int oldLevel = game.Engine.State.products[0].level;
                int oldOnboarding = game.Engine.State.onboarding;
                game.Engine.State.products[0].level = 1;
                game.Engine.State.onboarding = 3;
                game.View.RefreshNow();
                yield return new WaitForSecondsRealtime(.3f);
                ((RectTransform)nextAction.transform).GetWorldCorners(targetCorners);
                float offerHeight = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null, targetCorners[0]),
                    RectTransformUtility.WorldToScreenPoint(null, targetCorners[1]));
                var sponsor = game.HomeScreen.GetComponentsInChildren<LayoutElement>(true).FirstOrDefault(x => x.name == "Sponsor");
                Check((sponsor == null || !sponsor.gameObject.activeInHierarchy) && offerHeight >= 44f,
                    "Ad-free next-goal action remains readable and tappable at " + size);
                Check(nextCopy.All(x => x.fontSize * x.canvas.rootCanvas.scaleFactor * content.localScale.x >= 11.5f),
                    "Owned-product next-goal copy remains legible at " + size +
                    " (dashboard scale " + content.localScale.x.ToString("0.00") + ")");
                Check(content.rect.height * content.localScale.y <= viewport.rect.height + 1,
                    "Owned-product Home dashboard still fits at " + size);
                yield return Capture("ui-owned-home-" + size.x, 0);
                game.Engine.State.sponsorUntil = game.Engine.State.now + 300;
                game.View.RefreshNow();
                yield return new WaitForSecondsRealtime(.2f);
                Check(sponsor == null || !sponsor.gameObject.activeInHierarchy,
                    "Legacy sponsor timestamp cannot display a boost offer at " + size);
                game.Engine.State.sponsorUntil = game.Engine.State.now;
                game.Engine.State.products[0].level = oldLevel;
                game.Engine.State.onboarding = oldOnboarding;
                game.View.RefreshNow();
                game.View.Switch("Products");
                yield return new WaitForSecondsRealtime(.2f);
                var productText = game.ProductsScreen.GetComponentsInChildren<TMP_Text>(true)
                    .Where(x => x.name.StartsWith("product-name-") || x.name.StartsWith("product-level-") ||
                                x.name.StartsWith("product-income-") || x.name.StartsWith("product-next-"))
                    .ToArray();
                foreach (var label in productText) label.ForceMeshUpdate();
                Check(productText.Length == 32 && productText.All(x => !x.isTextTruncated),
                    "All product name, level, income and milestone rows fit at " + size);
                var productButtons = game.ProductsScreen.GetComponentsInChildren<Button>(true);
                productButtons.Single(x => x.name == "Quantity Buy ×10").onClick.Invoke();
                game.ProductsScreen.FocusProduct(7);
                yield return null;
                var productScroll = game.ProductsScreen.GetComponentInChildren<ScrollRect>(true);
                var lastRow = (RectTransform)productButtons.Single(x => x.name == "detail-7").transform.parent;
                var rowBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(productScroll.viewport, lastRow);
                var quantityLabel = game.ProductsScreen.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Quantity Label");
                Check(quantityLabel.text == "Qty ×1" && !game.ProductsScreen.GetComponentsInChildren<Transform>(true)
                    .Single(x => x.name == "Quantity Menu").gameObject.activeSelf,
                    "Recommended Products row resets quantity and closes its menu at " + size);
                Check(rowBounds.min.y >= productScroll.viewport.rect.yMin - 1f &&
                      rowBounds.max.y <= productScroll.viewport.rect.yMax + 1f,
                    "Recommended lower Products row scrolls fully into view at " + size);
                foreach (string browseTab in new[] { "Player", "Products", "Legacy" })
                {
                    game.View.Switch(browseTab);
                    yield return new WaitForSecondsRealtime(.35f);
                    var browseRoot = browseTab == "Player" ? game.PlayerScreen.gameObject :
                        browseTab == "Products" ? game.ProductsScreen.gameObject : game.LegacyScreen.gameObject;
                    var browsePanel = browseRoot.GetComponent<ImportedPanelFit>().MotionTarget;
                    browsePanel.GetWorldCorners(targetCorners);
                    float panelTop = RectTransformUtility.WorldToScreenPoint(null, targetCorners[1]).y;
                    CheckShelfTargets(size, browseTab, shelfButtons, panelTop);
                    yield return Capture("ui-browse-" + browseTab.ToLowerInvariant() + "-" + size.x, 0);
                }
            }
            ResizeForReview(360, 540);
            foreach (string tab in new[] { "Player", "Products", "Legacy" })
            {
                game.View.Switch(tab);
                yield return new WaitForSecondsRealtime(.2f);
                var root = tab == "Player" ? game.PlayerScreen.gameObject :
                    tab == "Products" ? game.ProductsScreen.gameObject : game.LegacyScreen.gameObject;
                var shop = root.GetComponentsInChildren<Button>(true).Single(x => x.name == "collapse-" + tab);
                var shopLabel = shop.GetComponentInChildren<TMP_Text>(true);
                shopLabel.ForceMeshUpdate();
                var rect = (RectTransform)shop.transform;
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                float width = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null, corners[0]),
                    RectTransformUtility.WorldToScreenPoint(null, corners[3]));
                float height = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null, corners[0]),
                    RectTransformUtility.WorldToScreenPoint(null, corners[1]));
                Check(shopLabel.text == "← Shop" && !shopLabel.isTextTruncated &&
                      width >= 44 && height >= 44,
                    tab + " has a readable Shop return control and 44-pixel hit area at 360x540");
                shop.onClick.Invoke();
                yield return new WaitForSecondsRealtime(.35f);
                Check(!root.activeSelf, tab + " Shop control returns Home");
            }
            Check(!game.View.TryNavigateBack() && game.View.CurrentTab == "Home",
                "Back at root Home requests task backgrounding without changing navigation");
            ResizeForReview(450, 916);
            foreach (string tab in new[] { "Products", "Player" })
            {
                game.View.Switch(tab);
                yield return new WaitForSecondsRealtime(.3f);
                var root = tab == "Products" ? game.ProductsScreen.gameObject : game.PlayerScreen.gameObject;
                var buttons = root.GetComponentsInChildren<Button>(true);
                buttons.Single(x => x.name == "buy-quantity").onClick.Invoke();
                yield return new WaitForSecondsRealtime(.2f);
                var choices = buttons.Where(x => x.name.StartsWith("Quantity Buy ")).OrderBy(x => x.transform.GetSiblingIndex()).ToArray();
                Check(choices.Length == 4 && choices.Zip(choices.Skip(1), (a,b) =>
                    Mathf.Abs(a.transform.position.x - b.transform.position.x) < 1 && a.transform.position.y > b.transform.position.y).All(x => x),
                    tab + " multiplier choices form one vertical column");
                yield return Capture("ui-quantity-" + tab, 0);
                var quantityMenu = root.GetComponentsInChildren<Transform>(true)
                    .Single(x => x.name == "Quantity Menu").gameObject;
                if (tab == "Products")
                {
                    game.View.ShowProductDetail(0);
                    Check(game.View.TryNavigateBack() && quantityMenu.activeSelf && game.View.CurrentTab == tab,
                        "Back closes Product Detail before the Products quantity menu");
                    yield return new WaitForSecondsRealtime(.25f);
                    Check(!game.ProductDetailScreen.gameObject.activeSelf && quantityMenu.activeSelf,
                        "Product Detail closes while its underlying quantity menu stays open");
                }
                Check(quantityMenu.activeSelf && game.View.TryNavigateBack() &&
                      !quantityMenu.activeSelf && game.View.CurrentTab == tab && root.activeSelf,
                    tab + " first Back dismisses its quantity menu and stays on the same tab");
                Check(game.View.TryNavigateBack() && game.View.CurrentTab == "Home",
                    tab + " second Back returns Home");
                yield return new WaitForSecondsRealtime(.25f);
                Check(!root.activeSelf, tab + " browse panel finishes closing after Back");
                game.View.Switch(tab);
                yield return new WaitForSecondsRealtime(.25f);
                buttons.Single(x => x.name == "buy-quantity").onClick.Invoke();
                choices[0].onClick.Invoke();
                game.Engine.State.cash = tab == "Products" ? game.Engine.ProductCost(0, 1) : game.Engine.TapQuote(1).cost;
                game.View.RefreshNow();
                var buy = buttons.Single(x => x.name == (tab == "Products" ? "purchase-0" : "buy-tap"));
                Check(buy.interactable, tab + " purchase initially affordable");
                buy.onClick.Invoke();
                Check(!buy.interactable, tab + " purchase disables immediately when cash is spent");
                yield return new WaitForSecondsRealtime(.3f);
                Check(Vector4.Distance(buy.targetGraphic.canvasRenderer.GetColor(), buy.colors.disabledColor * buy.colors.colorMultiplier) < .01f,
                    tab + " purchase animation preserves the disabled tint");
                Check(buy.GetComponent<Image>().overrideSprite.name == "Folk Sand", tab + " unaffordable purchase uses muted artwork");
                yield return Capture("ui-disabled-" + tab, 0);
                game.Engine.State.cash = 1e8;
                game.View.RefreshNow();
                yield return new WaitForSecondsRealtime(.2f);
                Check(buy.interactable && buy.targetGraphic.canvasRenderer.GetColor().a > .99f, tab + " purchase restores when affordable");
                Check(buy.GetComponent<Image>().overrideSprite.name == "Folk Blue", tab + " affordable purchase restores blue artwork");
            }
            game.View.ShowSettings();
            yield return new WaitForSecondsRealtime(.3f);
            var back = game.SettingsScreen.GetComponentsInChildren<Button>(true).Single(x => x.name == "Back to my shop");
            Check(back.GetComponent<Image>().sprite.name == "Folk Blue" && back.GetComponentInChildren<TMP_Text>().color == Color.white,
                "Settings return button uses blue artwork and white text");
            yield return Capture("ui-blue-settings", 0);
            game.SettingsScreen.Close();
            yield return new WaitForSecondsRealtime(.35f);

            var detail = game.ProductDetailScreen;
            var detailButtons = detail.GetComponentsInChildren<Button>(true);
            var detailBack = detailButtons.Single(x => x.name == "back-ProductDetail");
            var detailClose = detailButtons.Single(x => x.name == "close-ProductDetail");
            var returnLabel = detailBack.GetComponentInChildren<TMP_Text>(true);
            game.View.Switch("Home");
            game.View.ShowProductDetail(0);
            Check(detail.gameObject.activeSelf && returnLabel.text == "Return to Home",
                "Product guide opened from Home labels its actual return destination");
            detailBack.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.35f);
            Check(!detail.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                  !game.ProductsScreen.gameObject.activeSelf,
                "Product guide footer returns to Home without changing tabs");

            game.View.Switch("Products");
            game.ProductsScreen.FocusProduct(7);
            yield return null;
            var productsScroll = game.ProductsScreen.GetComponentInChildren<ScrollRect>(true);
            float previousScroll = productsScroll.verticalNormalizedPosition;
            game.View.ShowProductDetail(7);
            Check(detail.gameObject.activeSelf && returnLabel.text == "Return to Products",
                "Product guide opened from Products labels its actual return destination");
            detailClose.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.35f);
            Check(!detail.gameObject.activeSelf && game.View.CurrentTab == "Products" &&
                  game.ProductsScreen.gameObject.activeSelf &&
                  Mathf.Abs(productsScroll.verticalNormalizedPosition - previousScroll) < .001f,
                "Product guide close keeps Products and its scroll position");

            foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(450, 916) })
            {
                ResizeForReview(size.x, size.y);
                game.View.ShowSettings();
                yield return new WaitForSecondsRealtime(.35f);
                var settingsRoot = game.SettingsScreen;
                var settingsScroll = settingsRoot.GetComponentInChildren<ScrollRect>(true);
                var lead = settingsRoot.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Lead Text");
                var account = settingsRoot.GetComponentsInChildren<Button>(true).Single(x => x.name == "Account controls");
                var privacy = settingsRoot.GetComponentsInChildren<Button>(true).Single(x => x.name == "settings-privacy");
                var accountStatus = settingsRoot.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Account status");
                var storeValue = settingsRoot.GetComponentsInChildren<Button>(true)
                    .Single(x => x.name == "settings-rename-store").transform.parent.Find("Description").GetComponent<TMP_Text>();
                settingsScroll.verticalNormalizedPosition = 1;
                Canvas.ForceUpdateCanvases();
                lead.ForceMeshUpdate();
                Check(lead.text.Contains("account") && lead.text.Contains("privacy") && !lead.isTextTruncated,
                    "Settings names its offscreen account and privacy options at " + size);
                Check(settingsScroll.verticalScrollbar != null && settingsScroll.verticalScrollbar.gameObject.activeInHierarchy &&
                      settingsScroll.verticalScrollbar.size < .98f,
                    "Settings shows a proportional scrollbar when content overflows at " + size);
                yield return Capture("settings-top-" + size.x, 0);
                settingsScroll.verticalNormalizedPosition = 0;
                yield return new WaitForSecondsRealtime(.15f);
                var accountBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(settingsScroll.viewport,
                    (RectTransform)account.transform);
                var privacyBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(settingsScroll.viewport,
                    (RectTransform)privacy.transform);
                Check(accountBounds.min.y >= settingsScroll.viewport.rect.yMin - 1 &&
                      accountBounds.max.y <= settingsScroll.viewport.rect.yMax + 1 &&
                      privacyBounds.min.y >= settingsScroll.viewport.rect.yMin - 1 &&
                      privacyBounds.max.y <= settingsScroll.viewport.rect.yMax + 1,
                    "Account and Privacy actions are reachable together at the end of Settings at " + size);
                Check(settingsScroll.verticalScrollbar.value < .01f,
                    "Settings scrollbar thumb follows the scroll position at " + size);
                accountStatus.text = "Aarav Mahajan Festival Player\nSynced moments ago";
                accountStatus.ForceMeshUpdate();
                Check(!accountStatus.isTextTruncated && !accountStatus.isTextOverflowing,
                    "A connected account name and sync status fit at " + size);
                string priorStoreName = game.Engine.State.StoreName;
                foreach (string name in new[] { new string('W', 24), "<b>Literal Name</b>" })
                {
                    game.Engine.SetStoreName(name); settingsRoot.Refresh(); storeValue.ForceMeshUpdate();
                    float storePixels = storeValue.textInfo.characterCount > 0 ?
                        storeValue.textInfo.characterInfo[0].pointSize * storeValue.canvas.rootCanvas.scaleFactor : 0;
                    Check(storeValue.text == name && !storeValue.richText &&
                          !storeValue.isTextTruncated && !storeValue.isTextOverflowing && storePixels >= 12f,
                        "Settings shows the complete literal store name at " + size +
                        " at " + storePixels.ToString("0.0") + "px: " + name);
                    if (name[0] == 'W') yield return Capture("settings-name-long-" + size.x, 0);
                }
                game.Engine.SetStoreName(priorStoreName); settingsRoot.Refresh();
                yield return Capture("settings-bottom-" + size.x, 0);
                settingsRoot.Close();
                yield return new WaitForSecondsRealtime(.35f);
            }
        }
    }
}
#endif
