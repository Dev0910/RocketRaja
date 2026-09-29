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
        int productIndex;
        float refreshClock;
        float progressTarget;

        void Awake()
        {
            if (closeButton == null || backButton == null || nameLabel == null || subtitleLabel == null ||
                levelLabel == null || incomeLabel == null || nextLabel == null || progressLabel == null ||
                productImage == null || progressFill == null || progressMotion == null || productArt == null || productArt.Length != 8 ||
                Array.Exists(productArt, x => x == null))
            {
                Debug.LogError("Product Detail canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
        }

        public void Bind(GameController owner) => controller = owner;

        public void Open(int index)
        {
            if (controller == null || controller.Engine == null || index < 0 || index >= 8)
            {
                Debug.LogError("Product Detail was opened without a valid game owner or product.", this);
                return;
            }
            productIndex = index;
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
                nextLabel.text = level == 0 ? "Stock at Level 1" : "Milestones · Level " + next;
                progressLabel.text = level + " / " + next + " levels";
                progressTarget = ProductMilestones.Progress(level);
            }
            else
            {
                nextLabel.text = "Max level";
                progressLabel.text = "All milestones reached";
                progressTarget = 1;
            }
            progressMotion.SetInterval(progressTarget, ProductMilestones.Previous(level), controller);
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller, () => controller?.View?.Switch("Products"));
        }
    }
}
