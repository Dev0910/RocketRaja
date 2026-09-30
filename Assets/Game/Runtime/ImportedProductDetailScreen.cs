using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved product guide bound to one authoritative catalog entry.</summary>
    public sealed class ImportedProductDetailScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button backButton;
        [SerializeField] Button purchaseButton;
        [SerializeField] TMP_Text purchaseLabel;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text subtitleLabel;
        [SerializeField] TMP_Text levelLabel;
        [SerializeField] TMP_Text incomeLabel;
        [SerializeField] TMP_Text nextLabel;
        [SerializeField] TMP_Text progressLabel;
        [SerializeField] Image productImage;
        [SerializeField] RectTransform progressFill;
        [SerializeField] UiProgress progressMotion;
        [SerializeField] Sprite[] productArt;

        GameController controller;
        TMP_Text backLabel;
        int productIndex;
        float refreshClock;
        float progressTarget;

        void Awake()
        {
            backLabel = backButton == null ? null : backButton.GetComponentInChildren<TMP_Text>(true);
            if (closeButton == null || backButton == null || purchaseButton == null || purchaseLabel == null ||
                nameLabel == null || subtitleLabel == null ||
                levelLabel == null || incomeLabel == null || nextLabel == null || progressLabel == null ||
                backLabel == null || productImage == null || progressFill == null || progressMotion == null || productArt == null || productArt.Length != 8 ||
                Array.Exists(productArt, x => x == null))
            {
                Debug.LogError("Product Detail canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
            purchaseButton.onClick.AddListener(Purchase);
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
            if (purchaseButton != null) purchaseButton.onClick.RemoveListener(Purchase);
        }

        public void Bind(GameController owner) => controller = owner;

        public void Open(int index)
        {
            // Inactive scene roots receive Awake only when shown, so resolve the label here too.
            if (backLabel == null && backButton != null)
                backLabel = backButton.GetComponentInChildren<TMP_Text>(true);
            if (controller == null || controller.Engine == null || controller.View == null || backLabel == null || index < 0 || index >= 8)
            {
                Debug.LogError("Product Detail was opened without a valid game owner or product.", this);
                return;
            }
            productIndex = index;
            backLabel.text = "Return to " + controller.View.CurrentTab;
            productImage.sprite = productArt[index];
            Refresh();
            UiMotion.Show(gameObject, controller, true);
            progressMotion.Set(progressTarget, controller, !controller.Engine.State.reducedMotion);
            UiFeedback.Pulse(productImage.transform, controller, .08f, .55f);
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            refreshClock += Time.unscaledDeltaTime;
            if (refreshClock < .2f) return;
            refreshClock = 0;
            Refresh();
        }

        public void Refresh()
        {
            if (controller == null || controller.Engine == null) return;
            var engine = controller.Engine;
            int level = engine.State.Level(productIndex);
            var definition = engine.Data.products[productIndex];
            nameLabel.text = definition.name;
            subtitleLabel.text = definition.subtitle;
            levelLabel.text = level > 0 ? "Level " + level : "Not stocked";
            incomeLabel.text = "₹" + engine.ProductIncome(productIndex) + " / sec";
            int next = ProductMilestones.Next(level);
            if (next > 0)
            {
                nextLabel.text = level == 0 ? "Stock at Level 1" : "Profit ×2 at Level " + next;
                progressLabel.text = level == 0 ? "Profit ×2 at Level " + ProductMilestones.Next(1) : level + " / " + next + " levels";
                progressTarget = ProductMilestones.Progress(level);
            }
            else
            {
                nextLabel.text = "Max level";
                progressLabel.text = "All milestones reached";
                progressTarget = 1;
            }
            progressMotion.SetInterval(progressTarget, ProductMilestones.Previous(level), controller);

            // A shelf opened from Home should let the player complete the purchase here.
            // Quote again on each refresh so the introductory first-unit price is accurate.
            var quote = engine.ProductQuote(productIndex, 1);
            var cost = engine.ProductCost(productIndex, 1);
            bool predecessorLocked = productIndex > 0 && engine.State.Level(productIndex - 1) == 0;
            if (predecessorLocked)
                purchaseLabel.text = "Own " + engine.Data.products[productIndex - 1].name + " first";
            else if (level >= Catalog.MaxLevel)
                purchaseLabel.text = "Max level";
            else if (!controller.CanPlay || !engine.CanMutate)
                purchaseLabel.text = "Purchases paused";
            else if (quote.affordable)
                purchaseLabel.text = (level == 0 ? "Stock shelf" : "Upgrade ×1") + "\n₹" + cost;
            else
                purchaseLabel.text = "₹" + cost + "\nNeed ₹" + (cost - engine.State.cash) + " more";
            purchaseButton.interactable = !predecessorLocked && level < Catalog.MaxLevel &&
                controller.CanPlay && engine.CanMutate && quote.affordable;
        }

        void Purchase()
        {
            // Button.onClick.Invoke can bypass Button.interactable; keep the command guarded.
            if (controller == null || purchaseButton == null || !purchaseButton.interactable) return;
            if (!controller.PurchaseProduct(productIndex, 1)) { Refresh(); return; }
            purchaseButton.interactable = false;
            Close();
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller);
        }
    }
}
