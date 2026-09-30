#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RocketRaja.Cloud;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        IEnumerator DetailPurchaseChecks()
        {
            var engine = game.Engine;
            var state = engine.State;
            state.reducedMotion = true;
            state.sound = state.music = false;
            game.ModalPaused = true;
            Check(state.Level(0) == 0 && state.onboarding == 0,
                "Detail purchase starts from a fresh, unstocked shop");

            var homeButtons = game.HomeScreen.GetComponentsInChildren<Button>(true);
            var firstShelf = homeButtons.FirstOrDefault(x => x.name == "open-phuljhari");
            var secondShelf = homeButtons.FirstOrDefault(x => x.name == "open-anar");
            var detail = game.ProductDetailScreen;
            var detailButtons = detail.GetComponentsInChildren<Button>(true);
            var buy = detailButtons.FirstOrDefault(x => x.name == "purchase-ProductDetail");
            var back = detailButtons.FirstOrDefault(x => x.name == "back-ProductDetail");
            var close = detailButtons.FirstOrDefault(x => x.name == "close-ProductDetail");
            var buyLabel = buy == null ? null : buy.GetComponentInChildren<TMP_Text>(true);
            var backLabel = back == null ? null : back.GetComponentInChildren<TMP_Text>(true);
            Check(firstShelf != null && secondShelf != null && buy != null && back != null && close != null &&
                  buyLabel != null && backLabel != null,
                "Saved Home shelves and Product Detail purchase and return controls are bound");
            if (firstShelf == null || secondShelf == null || buy == null || back == null || close == null ||
                buyLabel == null || backLabel == null) yield break;

            game.View.Switch("Home");
            Amount firstCost = engine.ProductCost(0, 1);
            state.cash = firstCost;
            game.View.RefreshNow();
            firstShelf.onClick.Invoke();
            Check(detail.gameObject.activeSelf && game.View.CurrentTab == "Home" && game.ModalPaused,
                "Tapping the Home shelf opens its purchase guide without leaving Home");

            foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(450, 916) })
            {
                ResizeForReview(size.x, size.y);
                yield return new WaitForSecondsRealtime(.2f);
                detail.Refresh();
                Canvas.ForceUpdateCanvases();
                buyLabel.ForceMeshUpdate();
                backLabel.ForceMeshUpdate();
                Rect buyArea = ScreenRect(buy);
                Rect backArea = ScreenRect(back);
                var pointer = new PointerEventData(EventSystem.current) { position = buyArea.center };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                bool reachable = hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == buy;
                Check(Screen.width == size.x && Screen.height == size.y &&
                      buy.gameObject.activeInHierarchy && buy.interactable && reachable &&
                      buyArea.height >= 44 && backArea.height >= 44 && !buyArea.Overlaps(backArea),
                    "Primary purchase and secondary return are separate, reachable touch targets at " + size);
                Check(buyLabel.text.Contains("₹" + firstCost) &&
                      buyLabel.text.IndexOf("Stock", StringComparison.OrdinalIgnoreCase) >= 0 &&
                      !buyLabel.isTextTruncated && !buyLabel.isTextOverflowing &&
                      backLabel.text == "Return to Home" && !backLabel.isTextTruncated,
                    "Product Detail shows the first shelf action, exact price and Home return at " + size);
                yield return Capture("detail-home-affordable-" + size.x, 0);
            }

            int firstLevel = state.Level(0);
            Amount cashBefore = state.cash;
            buy.onClick.Invoke();
            Check(state.Level(0) == firstLevel + 1 && state.cash.CompareTo(cashBefore - firstCost) == 0 &&
                  state.onboarding == 3 && !detail.gameObject.activeSelf && game.View.CurrentTab == "Home",
                "Home shelf purchase spends once, stocks the shelf, completes onboarding and returns Home");
            var saved = LoadDetailPurchaseSave();
            Check(saved != null && saved.Level(0) == state.Level(0) && saved.onboarding == 3 &&
                  saved.cash.CompareTo(state.cash) == 0,
                "First Product Detail purchase persists the shelf, wallet and completed onboarding");
            game.ModalPaused = true;
            firstShelf.onClick.Invoke();
            detail.Refresh();
            int purchasedLevel = state.Level(0);
            buy.onClick.Invoke();
            Check(!buy.interactable && state.Level(0) == purchasedLevel &&
                  buyLabel.text.Contains("₹" + engine.ProductCost(0, 1)),
                "Spent wallet disables the next upgrade and repeated input cannot double-buy");
            yield return Capture("detail-home-after-purchase-450", 0);
            back.onClick.Invoke();
            yield return null;
            Check(!detail.gameObject.activeSelf && game.View.CurrentTab == "Home" &&
                  !game.ProductsScreen.gameObject.activeSelf,
                "Secondary Detail return restores Home without a purchase");

            game.ModalPaused = true;
            state.cash = Amount.Zero;
            firstShelf.onClick.Invoke();
            detail.Refresh();
            int beforeBlocked = state.Level(0);
            buy.onClick.Invoke();
            Check(!buy.interactable && state.Level(0) == beforeBlocked &&
                  buyLabel.text.Contains("₹" + engine.ProductCost(0, 1)),
                "Unaffordable upgrade explains its price and cannot spend");
            close.onClick.Invoke();

            game.ModalPaused = true;
            state.products[0].level = 0;
            state.cash = new Amount(1, 25);
            secondShelf.onClick.Invoke();
            detail.Refresh();
            buy.onClick.Invoke();
            Check(!buy.interactable && state.Level(1) == 0 &&
                  buyLabel.text.Contains(engine.Data.products[0].name),
                "Locked Anar points to the prerequisite shelf and cannot be bought");
            close.onClick.Invoke();

            game.ModalPaused = true;
            state.products[0].level = Catalog.MaxLevel;
            firstShelf.onClick.Invoke();
            detail.Refresh();
            buy.onClick.Invoke();
            Check(!buy.interactable && state.Level(0) == Catalog.MaxLevel &&
                  buyLabel.text.IndexOf("Max level", StringComparison.OrdinalIgnoreCase) >= 0,
                "Capped shelf has a disabled, explanatory purchase state");
            close.onClick.Invoke();

            game.ModalPaused = true;
            state.products[0].level = 1;
            state.cash = engine.ProductCost(0, 1);
            engine.MutationAllowed = () => false;
            try
            {
                firstShelf.onClick.Invoke();
                detail.Refresh();
                int readOnlyLevel = state.Level(0);
                Amount readOnlyCash = state.cash;
                buy.onClick.Invoke();
                Check(!buy.interactable && state.Level(0) == readOnlyLevel &&
                      state.cash.CompareTo(readOnlyCash) == 0,
                    "Read-only session disables Product Detail purchasing");
                close.onClick.Invoke();
            }
            finally { engine.MutationAllowed = null; }

            game.ModalPaused = true;
            for (int i = 0; i < 7; i++) state.products[i].level = 1;
            state.products[7].level = 0;
            Amount finalShelfCost = engine.ProductCost(7, 1);
            state.cash = finalShelfCost;
            ResizeForReview(360, 540);
            yield return new WaitForSecondsRealtime(.2f);
            game.View.Switch("Products");
            yield return null;
            Canvas.ForceUpdateCanvases();
            game.ProductsScreen.FocusProduct(7);
            yield return new WaitForSecondsRealtime(.2f);
            var productsScroll = game.ProductsScreen.GetComponentInChildren<ScrollRect>(true);
            float priorScroll = productsScroll.verticalNormalizedPosition;
            var finalShelf = game.ProductsScreen.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(x => x.name == "detail-7");
            Check(finalShelf != null && priorScroll < .99f,
                "Products can focus the last shelf before opening its guide");
            if (finalShelf == null) yield break;
            finalShelf.onClick.Invoke();
            detail.Refresh();
            Check(detail.gameObject.activeSelf && backLabel.text == "Return to Products" &&
                  buy.interactable && buyLabel.text.Contains("₹" + finalShelfCost),
                "Products guide retains a clear purchase and return path");
            yield return Capture("detail-products-affordable-360", 0);
            Check(Mathf.Abs(productsScroll.verticalNormalizedPosition - priorScroll) < .001f,
                "Product Detail stays at its Products scroll position while open (before " +
                priorScroll.ToString("0.0000") + ", after capture " +
                productsScroll.verticalNormalizedPosition.ToString("0.0000") + ")");
            back.onClick.Invoke();
            yield return null;
            Check(!detail.gameObject.activeSelf && game.ProductsScreen.gameObject.activeSelf &&
                  Mathf.Abs(productsScroll.verticalNormalizedPosition - priorScroll) < .001f,
                "Secondary Detail return restores Products at its previous scroll position (before " +
                priorScroll.ToString("0.0000") + ", after " +
                productsScroll.verticalNormalizedPosition.ToString("0.0000") + ")");
            game.ModalPaused = true;
            finalShelf.onClick.Invoke();
            detail.Refresh();
            buy.onClick.Invoke();
            Check(state.Level(7) == 1 && state.cash.CompareTo(Amount.Zero) == 0 &&
                  game.View.CurrentTab == "Products" && !detail.gameObject.activeSelf,
                "Products-origin purchase buys the selected shelf and returns to Products");
            yield return null;
            Check(!detail.gameObject.activeSelf && game.ProductsScreen.gameObject.activeSelf &&
                  Mathf.Abs(productsScroll.verticalNormalizedPosition - priorScroll) < .001f,
                "Products-origin purchase preserves its previous scroll position (before " +
                priorScroll.ToString("0.0000") + ", after " +
                productsScroll.verticalNormalizedPosition.ToString("0.0000") + ")");
            game.ModalPaused = false;
        }

        static Rect ScreenRect(Button button)
        {
            var corners = new Vector3[4];
            ((RectTransform)button.transform).GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            return Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
                Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
        }

        static GameState LoadDetailPurchaseSave()
        {
            // Reopen the isolated smoke guest's actual save; this catches a missing save
            // after the onboarding flag changes, even though BuyProduct itself saves.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var guest = PlayerSessionHost.Ensure().Session as GuestSession;
            if (guest == null) return null;
            var store = typeof(GuestSession).GetField("store", flags)?.GetValue(guest) as SaveStore;
            var path = store == null ? null : typeof(SaveStore).GetField("file", flags)?.GetValue(store) as string;
            if (string.IsNullOrEmpty(path)) return null;
            return new SaveStore(path, JsonUtility.ToJson, JsonUtility.FromJson<GameState>).Load();
        }
    }
}
#endif
