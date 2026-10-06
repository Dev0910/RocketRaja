using System;
using System.Globalization;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Shared presenter on separate saved product and tap-training page instances.</summary>
    public sealed class RedesignLevelsScreen : MonoBehaviour
    {
        public TMP_Text itemName, status, projection, price, buyLabel, exactLabel, upgradesLabel;
        public Button[] quantities;
        public Button buy, exact, upgrades, requirement, back;
        public TMP_Text requirementLabel;
        public ScrollRect scroll;
        public Image productImage;
        public Sprite[] productSprites;
        public RedesignProductArt productArt;
        public TMP_Text appearance;
        RedesignUiContext context;
        UnityAction[] listeners;
        static readonly int[] Counts = { 1, 10, 100, int.MaxValue };
        int target, selectedQuantity;
        bool fullPrecision;
        string revision;
        LevelProjection preview;
        float elapsed;
        public int Target => target;
        public int SelectedQuantity => selectedQuantity;
        public LevelProjection DisplayedProjection => preview;
        public void Bind(RedesignUiContext value) { context = value; revision = null; }
        public void Open(int product)
        {
            if (product < -1 || product >= 8) throw new ArgumentOutOfRangeException(nameof(product));
            target = product; preview = null; fullPrecision = false;
            scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; gameObject.SetActive(true); Refresh();
        }
        void Awake()
        {
            if (!itemName || !status || !projection || !price || !buyLabel || !exactLabel || !upgradesLabel ||
                !buy || !exact || !upgrades || !requirement || !requirementLabel || !back || !scroll || quantities == null || quantities.Length != 4 || !productImage || productSprites == null || productSprites.Length != 8)
                throw new InvalidOperationException("Incomplete serialized level screen");
            listeners = new UnityAction[4];
            for (int i = 0; i < 4; i++) { int slot = i; listeners[i] = () => SelectQuantity(slot); quantities[i].onClick.AddListener(listeners[i]); }
            buy.onClick.AddListener(Buy); exact.onClick.AddListener(Exact); upgrades.onClick.AddListener(Upgrades);
            requirement.onClick.AddListener(Requirement); back.onClick.AddListener(Back);
        }
        void OnDestroy()
        {
            if (listeners != null) for (int i = 0; i < 4; i++) quantities[i].onClick.RemoveListener(listeners[i]);
            if (buy) buy.onClick.RemoveListener(Buy); if (exact) exact.onClick.RemoveListener(Exact);
            if (upgrades) upgrades.onClick.RemoveListener(Upgrades); if (requirement) requirement.onClick.RemoveListener(Requirement); if (back) back.onClick.RemoveListener(Back);
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        public void SelectQuantity(int value) { if (value < 0 || value > 3) throw new ArgumentOutOfRangeException(nameof(value)); selectedQuantity = value; Refresh(); }
        void Exact() { fullPrecision = !fullPrecision; Refresh(); }
        void Back() => context?.Navigate("back", "");
        void Requirement() { if (target > 0) context?.Navigate("productdetails", (target - 1).ToString(CultureInfo.InvariantCulture)); }
        void Upgrades() => context?.Navigate(target < 0 ? "tappingupgrades" : "productupgrades", target.ToString(CultureInfo.InvariantCulture));
        void Buy()
        {
            if (context == null || preview == null || preview.quote.quantity <= 0) return;
            // Buy exactly the displayed count. Passive cash must not silently grow a Max click.
            int count = preview.quote.quantity, level = preview.beforeLevel; var cost = preview.quote.cost;
            bool done = context.Execute(e => (target < 0 ? e.State.tapLevel : e.State.products[target]) == level &&
                e.LevelQuote(target, count).cost.CompareTo(cost) == 0 && e.BuyLevels(target, count));
            context.Notice(done ? "Bought " + count + (target < 0 ? " tap " : " product ") + (count == 1 ? "level" : "levels") : "Purchase unavailable. Review cash, requirements or saving.");
            Refresh();
        }
        public void Refresh()
        {
            if (context == null) return;
            var e = context.Engine; var quote = e.LevelQuote(target, Counts[selectedQuantity]);
            if (preview == null || revision != e.State.commandRevision.digits || preview.quote.quantity != quote.quantity)
            { preview = e.PreviewLevels(target, Counts[selectedQuantity]); revision = e.State.commandRevision.digits; }
            string Cash(Amount n) => RedesignUiContext.Cash(n, fullPrecision);
            bool tap = target < 0; itemName.text = tap ? "A more valuable tap" : e.Data.Products[target].name;
            productImage.gameObject.SetActive(!tap);
            if (!tap)
            {
                int tier = e.ProductAppearance(target);
                productImage.sprite = productArt ? productArt.At(target, Math.Max(0, tier)) : productSprites[target];
                productImage.color = new Color(1, 1, 1, preview.beforeLevel > 0 ? 1 : .35f);
                if (appearance) appearance.text = (tier < 0 ? "Preview · Basic display after ownership" : "Current display: " + RedesignProductArt.Label(tier)) +
                    (tier < 0 ? "\nBuy your first level manually." : tier < 3 ? "\nNext look: " + RedesignProductArt.Label(tier + 1) + " · " + e.Data.AppearanceUpgrade(target, tier + 1) : "\nFinal look for this run.") +
                    "\nProduct displays rebuild after Prestige. Shop decorations stay.";
            }
            status.text = !preview.available ? "Locked · Buy " + e.Data.Products[target - 1].name + " first" :
                (tap ? "Tap level " : preview.beforeLevel == 0 ? "Available · Level " : "Owned · Level ") + preview.beforeLevel;
            if (tap)
                projection.text = "Normal payout: " + Cash(preview.tapBefore) + " → " + Cash(preview.tapAfter) + " per tap\n" +
                    "Product-linked part: " + Cash(preview.linkedTapBefore) + " → " + Cash(preview.linkedTapAfter) + "\n" +
                    "Current payout: " + Cash(e.TapPayout()) + " per tap\nOnly taps on the open stall earn cash. Up to 3 full-value taps each second.";
            else
            {
                double share = preview.normalBefore.mantissa == 0 ? 0 : Amount.Ratio(preview.productBefore, preview.normalBefore) * 100;
                projection.text = "Normal income: " + Cash(preview.productBefore) + " → " + Cash(preview.productAfter) + "/s\n" +
                    "Shop contribution: " + share.ToString("0.##") + "%\n" +
                    (preview.nextMilestone == 0 ? "All level milestones reached" : "Next milestone: level " + preview.nextMilestone + " · ×2 income") + "\n" +
                    "Shop income after: " + Cash(preview.normalAfter) + "/s";
            }
            string levelWord = quote.quantity == 1 ? "level" : "levels";
            price.text = "Levels " + preview.beforeLevel + " → " + preview.afterLevel + "\n" + quote.quantity + " " + levelWord + " · " + Cash(quote.cost) + "\nRun discounts and Talent are included.";
            buy.interactable = context.CanMutate() && preview.available && quote.affordable;
            buyLabel.text = !context.CanMutate() ? "Saving / recovery required" : !preview.available ? "Requirements not met" : quote.quantity == 0 ?
                (preview.beforeLevel >= Catalog.MaxLevel ? "Maximum level reached" : "Not enough cash") :
                "Buy " + quote.quantity + (tap ? " tap " : " ") + levelWord + " · " + RedesignUiContext.Cash(quote.cost);
            requirement.gameObject.SetActive(!preview.available); requirementLabel.text = target > 0 ? "View " + e.Data.Products[target - 1].name : "";
            exactLabel.text = fullPrecision ? "Show shorter numbers" : "Show full precision";
            upgradesLabel.text = tap ? "Open Tapping upgrades" : "View " + e.Data.Products[target].name + " upgrades";
            for (int i = 0; i < 4; i++)
            {
                bool active = i == selectedQuantity;
                quantities[i].targetGraphic.color = active ? new Color32(117, 31, 53, 255) : new Color32(255, 243, 213, 255);
                quantities[i].GetComponentInChildren<TMP_Text>().color = active ? new Color32(255, 240, 204, 255) : new Color32(56, 33, 49, 255);
            }
        }
    }
}
