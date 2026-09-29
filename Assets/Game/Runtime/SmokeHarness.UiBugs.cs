#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
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
                var layout = game.HomeScreen.GetComponentInChildren<ImportedHomeLayout>();
                var content = (RectTransform)layout.transform;
                var viewport = (RectTransform)content.parent;
                Check(game.HomeScreen.GetComponentInChildren<ScrollRect>(true) == null,
                    "Home has no scroll input at " + size);
                Check(content.rect.height * content.localScale.y <= viewport.rect.height + 1 && content.anchoredPosition == Vector2.zero,
                    "Entire fixed Home dashboard fits at " + size);
                var tap = game.HomeScreen.GetComponentInChildren<ShopTapTarget>();
                var popup = game.HomeScreen.GetComponentsInChildren<RectTransform>(true).Where(x => x.name.StartsWith("Cash Popup ")).ToArray();
                foreach (var item in popup) item.gameObject.SetActive(false);
                var point = RectTransformUtility.WorldToScreenPoint(null, tap.transform.TransformPoint(((RectTransform)tap.transform).rect.center));
                point += new Vector2(23, 17);
                var pointer = new PointerEventData(EventSystem.current) { pointerId = -1, position = point };
                tap.OnPointerDown(pointer); tap.OnPointerUp(pointer);
                var active = popup.Single(x => x.gameObject.activeSelf);
                Check(Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null, active.position), point) < 1,
                    "Cash popup starts at the actual pointer at " + size);
                Check(active.GetComponentInChildren<TMP_Text>().color == (Color)new Color32(22, 75, 128, 255), "Cash popup is blue");
                yield return Capture("ui-fixed-home-" + size.x, 0);
            }
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
        }
    }
}
#endif
