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
            collapseButton.onClick.AddListener(() => view?.Switch("Home"));
            quantityButton.onClick.AddListener(() => UiMotion.Toggle(quantityMenu, controller));
            int[] modes = { 1, 10, 100, int.MaxValue };
            for (int i = 0; i < modes.Length; i++)
            {
                int selected = modes[i];
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

        public void Bind(GameController owner, GameView ownerView)
        {
            controller = owner;
            view = ownerView;
            UiFeedback.Wire(gameObject, owner);
            Refresh();
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
            var engine = controller.Engine;
            var state = engine.State;
            quantityLabel.text = buyMode == int.MaxValue ? "Buy MAX" : "Buy ×" + buyMode;
            profitLabel.text = "Profit · ₹" + engine.NormalIncome + " / sec";
            for (int i = 0; i < 8; i++)
            {
                int level = state.Level(i);
                names[i].text = engine.Data.products[i].name;
                levels[i].text = level == 0 ? "Not owned" : "Level " + level;
                incomes[i].text = "₹" + engine.ProductIncome(i) + " / sec";
                int next = 0;
                foreach (int mark in Catalog.VisualLevels)
                    if (mark > level) { next = mark; break; }
                displayProgress[i].Set(next == 0 ? 1 : level / (float)next, controller);
                milestones[i].text = next == 0 ? "Premium display · Auto sales on" :
                    level == 0 ? "Next display · Lv.1" : "Next display · Lv." + next;
                var quote = engine.ProductQuote(i, buyMode);
                bool blocked = i > 0 && state.Level(i - 1) == 0;
                if (blocked) purchaseLabels[i].text = "Own " + engine.Data.products[i - 1].name;
                else if (level >= Catalog.MaxLevel) purchaseLabels[i].text = "Max level";
                else if (quote.quantity == 0) purchaseLabels[i].text = "₹" + engine.ProductCost(i, 1) + "\n" +
                    (level == 0 ? "Stock shelf" : "Upgrade") + " ×1";
                else purchaseLabels[i].text = "₹" + quote.cost + "\n" + (level == 0 ? "Stock shelf" : "Upgrade") + " ×" + quote.quantity;
                purchaseButtons[i].interactable = quote.affordable;
                productImages[i].color = level == 0 ? new Color(1, 1, 1, .82f) : Color.white;
            }
        }

        void Purchase(int index)
        {
            if (controller == null || controller.Engine == null) return;
            int tier = controller.Engine.VisualTier(index);
            if (controller.Engine.BuyProduct(index, buyMode))
            {
                controller.Save();
                if (controller.Engine.VisualTier(index) > tier)
                    view?.Notice(controller.Engine.Data.products[index].name + " display upgraded!");
                else view?.Notice(controller.Engine.Data.products[index].name + " stocked.");
                view?.RefreshNow();
                UiFeedback.Pulse(productImages[index].transform, controller, .15f, .45f);
                UiFeedback.Pulse(levels[index].transform, controller, .09f);
                UiFeedback.Pulse(purchaseButtons[index].transform, controller, .06f);
            }
            Refresh();
        }
    }
}
