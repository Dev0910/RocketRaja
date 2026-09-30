using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved Products panel with eight fixed catalog rows.</summary>
    public sealed class ImportedProductsScreen : MonoBehaviour
    {
        static readonly int[] QuantityModes = { 1, 10, 100, int.MaxValue };
        static readonly string[] QuantityOptions = { "×1", "×10", "×100", "Max affordable" };
        [SerializeField] Button collapseButton;
        [SerializeField] Button quantityButton;
        [SerializeField] Button[] quantityChoices;
        [SerializeField] GameObject quantityMenu;
        [SerializeField] TMP_Text quantityLabel;
        [SerializeField] TMP_Text profitLabel;
        [SerializeField] Button[] detailButtons;
        [SerializeField] Button[] purchaseButtons;
        [SerializeField] Image[] productImages;
        [SerializeField] TMP_Text[] names;
        [SerializeField] TMP_Text[] levels;
        [SerializeField] TMP_Text[] incomes;
        [SerializeField] TMP_Text[] milestones;
        [SerializeField] TMP_Text[] purchaseLabels;
        [SerializeField] UiProgress[] displayProgress;

        readonly TMP_Text[] quantityChoiceLabels = new TMP_Text[4];
        readonly GameObject[] quantitySelectionMarks = new GameObject[4];
        bool quantityChoicesCached;
        GameController controller;
        GameView view;
        int buyMode = 1;
        float refreshClock;

        void OnDisable() { if (quantityMenu != null) quantityMenu.SetActive(false); }

        void Awake()
        {
            if (collapseButton == null || quantityButton == null || !Valid(quantityChoices, 4) ||
                quantityMenu == null || quantityLabel == null || profitLabel == null ||
                !Valid(detailButtons, 8) || !Valid(purchaseButtons, 8) || !Valid(productImages, 8) ||
                !Valid(names, 8) || !Valid(levels, 8) || !Valid(incomes, 8) ||
                !Valid(milestones, 8) || !Valid(purchaseLabels, 8) || !Valid(displayProgress, 8))
            {
                Debug.LogError("Products canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            if (!CacheQuantityChoices()) { enabled = false; return; }
            collapseButton.onClick.AddListener(() => view?.Switch("Home"));
            quantityButton.onClick.AddListener(() => UiMotion.Toggle(quantityMenu, controller));
            for (int i = 0; i < QuantityModes.Length; i++)
            {
                int selected = QuantityModes[i];
                quantityChoices[i].onClick.AddListener(() => { buyMode = selected; UiMotion.Hide(quantityMenu, controller); Refresh(); });
            }
            for (int i = 0; i < 8; i++)
            {
                int index = i;
                detailButtons[i].onClick.AddListener(() => view?.ShowProductDetail(index));
                purchaseButtons[i].onClick.AddListener(() => Purchase(index));
            }
            quantityMenu.SetActive(false);
        }

        static bool Valid<T>(T[] array, int length) where T : UnityEngine.Object =>
            array != null && array.Length == length && !Array.Exists(array, x => x == null);

        bool CacheQuantityChoices()
        {
            if (quantityChoicesCached) return true;
            if (!Valid(quantityChoices, QuantityModes.Length)) return false;
            for (int i = 0; i < QuantityModes.Length; i++)
            {
                quantityChoiceLabels[i] = quantityChoices[i].GetComponentInChildren<TMP_Text>(true);
                quantitySelectionMarks[i] = quantityChoices[i].transform.Find("Selected Mark")?.gameObject;
                if (quantityChoiceLabels[i] != null) continue;
                Debug.LogError("Products quantity choice is missing its label.", quantityChoices[i]);
                return false;
            }
            quantityChoicesCached = true;
            return true;
        }

        public void Bind(GameController owner, GameView ownerView)
        {
            controller = owner;
            view = ownerView;
            UiFeedback.Wire(gameObject, owner);
            Refresh();
        }

        public bool DismissQuantityMenu()
        {
            if (quantityMenu == null || !quantityMenu.activeInHierarchy) return false;
            UiMotion.Hide(quantityMenu, controller);
            return true;
        }

        /// <summary>Opens the requested shelf with its purchase controls in view.</summary>
        public void FocusProduct(int index)
        {
            if (index < 0 || detailButtons == null || index >= detailButtons.Length ||
                detailButtons[index] == null || !gameObject.activeInHierarchy || controller == null)
                return;

            buyMode = 1;
            if (quantityMenu != null) quantityMenu.SetActive(false);
            Refresh();

            var scroll = GetComponentInChildren<ScrollRect>(true);
            var content = scroll != null ? scroll.content : null;
            var viewport = scroll != null ? scroll.viewport : null;
            var row = detailButtons[index].transform.parent as RectTransform;
            if (content == null || viewport == null || row == null) return;

            Canvas.ForceUpdateCanvases();
            float scrollableHeight = content.rect.height - viewport.rect.height;
            if (scrollableHeight <= 0f) return;

            // Row coordinates are relative to the layout-driven content, so this also works
            // when the browse panel changes height for a shorter device.
            float rowTop = RectTransformUtility.CalculateRelativeRectTransformBounds(content, row).max.y;
            float inset = Mathf.Min(18f, viewport.rect.height * .05f);
            float distanceFromTop = content.rect.yMax - rowTop - inset;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(distanceFromTop / scrollableHeight);
        }

        void Update()
        {
            if (controller == null || !gameObject.activeInHierarchy) return;
            refreshClock += Time.unscaledDeltaTime;
            if (refreshClock < .2f) return;
            refreshClock = 0;
            Refresh();
        }

        public void Refresh()
        {
            if (controller == null || controller.Engine == null) return;
            // Bind can refresh this inactive screen before Unity calls Awake.
            if (!CacheQuantityChoices()) return;
            var engine = controller.Engine;
            var state = engine.State;
            quantityLabel.text = buyMode == int.MaxValue ? "Qty Max" : "Qty ×" + buyMode;
            for (int i = 0; i < QuantityModes.Length; i++)
            {
                bool selected = buyMode == QuantityModes[i];
                quantityChoiceLabels[i].text = QuantityOptions[i] + (selected ? "\nSelected" : "");
                var mark = quantitySelectionMarks[i];
                if (mark != null && mark.activeSelf != selected) mark.SetActive(selected);
            }
            profitLabel.text = "Profit · ₹" + engine.NormalIncome + " / sec";
            for (int i = 0; i < 8; i++)
            {
                int level = state.Level(i);
                names[i].text = engine.Data.products[i].name;
                levels[i].text = level == 0 ? "Not owned" : "Level " + level;
                incomes[i].text = "₹" + engine.ProductIncome(i) + " / sec";
                int next = ProductMilestones.Next(level);
                displayProgress[i].SetInterval(ProductMilestones.Progress(level), ProductMilestones.Previous(level), controller);
                milestones[i].text = next == 0 ? "Max level" :
                    level == 0 ? "Stock at Level 1" : "Milestones · Lv." + next;
                var quote = engine.ProductQuote(i, buyMode);
                bool blocked = i > 0 && state.Level(i - 1) == 0;
                if (blocked) purchaseLabels[i].text = "Own " + engine.Data.products[i - 1].name;
                else if (level >= Catalog.MaxLevel) purchaseLabels[i].text = "Max level";
                else if (quote.quantity == 0) purchaseLabels[i].text = "₹" + engine.ProductCost(i, 1) + "\n" +
                    (level == 0 ? "Stock shelf" : "Upgrade") + " ×1";
                else purchaseLabels[i].text = "₹" + quote.cost + "\n" + (level == 0 ? "Stock shelf" : "Upgrade") + " ×" + quote.quantity;
                purchaseButtons[i].interactable = engine.CanMutate && quote.affordable;
                productImages[i].color = level == 0 ? new Color(1, 1, 1, .82f) : Color.white;
            }
        }

        void Purchase(int index)
        {
            if (controller == null || controller.Engine == null) return;
            if (controller.PurchaseProduct(index, buyMode))
            {
                UiFeedback.Pulse(productImages[index].transform, controller, .15f, .45f);
                UiFeedback.Pulse(levels[index].transform, controller, .09f);
                UiFeedback.Pulse(purchaseButtons[index].transform, controller, .06f);
            }
            Refresh();
        }
    }
}
