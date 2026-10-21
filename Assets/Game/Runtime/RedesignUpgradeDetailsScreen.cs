using System;
using System.Text;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignUpgradeDetailsScreen : MonoBehaviour
    {
        public TMP_Text itemName, effect, projection, requirements, price, status;
        public Button buy, pin, nextRequirement, alternatives, exact, back;
        public TMP_Text buyLabel, pinLabel, exactLabel, requirementLabel;
        public ScrollRect scroll;
        public RedesignGoalsScreen goals;
        RedesignUiContext context;
        string selectedId, projectedRevision;
        UpgradeProjection preview;
        bool fullPrecision;
        float elapsed;
        public string SelectedId => selectedId;
        public void Bind(RedesignUiContext value) { context = value; if (goals) { goals.Bind(value); goals.Close = () => goals.gameObject.SetActive(false); } Refresh(); }
        public void Open(string id)
        {
            if (context == null || !context.Engine.Data.TryUpgrade(id, out _)) throw new ArgumentException("Unknown upgrade");
            selectedId = id; projectedRevision = null; fullPrecision = false;
            if (goals) goals.gameObject.SetActive(false);
            if (scroll) { scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; }
            gameObject.SetActive(true); Refresh();
        }
        void Awake()
        {
            if (!itemName || !effect || !projection || !requirements || !price || !status || !buy || !pin || !nextRequirement || !alternatives || !exact || !back || !buyLabel || !pinLabel || !exactLabel || !requirementLabel)
                throw new InvalidOperationException("Incomplete serialized upgrade details");
            buy.onClick.AddListener(Buy); pin.onClick.AddListener(Pin); nextRequirement.onClick.AddListener(Requirement);
            alternatives.onClick.AddListener(Alternatives); exact.onClick.AddListener(Exact); back.onClick.AddListener(Back);
        }
        void OnDestroy()
        {
            if (buy) buy.onClick.RemoveListener(Buy); if (pin) pin.onClick.RemoveListener(Pin); if (nextRequirement) nextRequirement.onClick.RemoveListener(Requirement);
            if (alternatives) alternatives.onClick.RemoveListener(Alternatives); if (exact) exact.onClick.RemoveListener(Exact); if (back) back.onClick.RemoveListener(Back);
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        void Buy()
        {
            if (context == null || selectedId == null) return;
            var feedback = new RedesignPurchaseFeedback(context.Engine);
            bool done = context.Execute(e => e.BuyUpgrade(selectedId));
            context.Notice(done ? feedback.Complete(context.Engine) : "Purchase unavailable. Review cash, requirements or saving."); Refresh();
        }
        void Pin() { if (context == null || selectedId == null) return; context.Execute(e => e.PinUpgrade(e.State.pinnedUpgradeId == selectedId ? "" : selectedId)); Refresh(); }
        void Requirement() { if (context != null) context.OpenGoal(context.Engine.NextRequirement(selectedId)); }
        void Alternatives()
        {
            if (!goals || !scroll) return;
            goals.gameObject.SetActive(true); goals.Refresh(); Canvas.ForceUpdateCanvases(); scroll.verticalNormalizedPosition = 0;
        }
        void Exact() { fullPrecision = !fullPrecision; Refresh(); }
        void Back() => context?.Navigate("back", "");
        public void Refresh()
        {
            if (context == null || selectedId == null || !context.Engine.Data.TryUpgrade(selectedId, out int index)) return;
            var engine = context.Engine; var data = engine.Data.Upgrades[index]; bool owned = engine.State.upgrades.Contains(selectedId);
            itemName.text = data.name; effect.text = RedesignUpgradeRow.Effect(data, engine.Data);
            requirements.text = RedesignUpgradeRow.Requirements(data, engine.Data);
            price.text = "Cash price: " + RedesignUiContext.Cash(engine.UpgradeCost(selectedId), fullPrecision);
            if (projectedRevision != engine.State.commandRevision.digits) { preview = engine.PreviewUpgrade(selectedId); projectedRevision = engine.State.commandRevision.digits; }
            projection.text = preview == null ? "This cash upgrade is already applied for the current run." : Describe(engine, data, preview, fullPrecision);
            bool eligible = engine.Eligible(selectedId), allowed = context.CanMutate();
            buy.interactable = allowed && !owned && eligible && engine.State.cash >= engine.UpgradeCost(selectedId);
            buyLabel.text = owned ? "Owned this run" : buy.interactable ? "Buy upgrade" : !allowed ? "Saving / recovery required" : !eligible ? "Requirements not met" : "Not enough cash";
            pin.interactable = allowed && !owned; pinLabel.text = engine.State.pinnedUpgradeId == selectedId ? "Unpin goal" : "Pin as goal";
            status.text = owned ? "Owned · repurchase after prestige" : engine.State.everPurchased.Contains(selectedId) ? "Rebuy · lifetime count stays the same" : "First purchase · adds 1 to lifetime count";
            nextRequirement.interactable = !owned && engine.NextRequirement(selectedId) != null;
            requirementLabel.text = nextRequirement.interactable ? "View next missing requirement" : "Requirements met";
            exactLabel.text = fullPrecision ? "Show shorter numbers" : "Show full precision";
        }
        public static string Describe(RedesignEngine engine, CashUpgrade item, UpgradeProjection p, bool full)
        {
            string Cash(Amount n) => RedesignUiContext.Cash(n, full);
            var text = new StringBuilder(); text.Append("Normal shop income: ").Append(Cash(p.normalBefore)).Append(" → ").Append(Cash(p.normalAfter)).Append("/s");
            for (int i = 0; i < 8; i++)
            {
                if (p.productsBefore[i].CompareTo(p.productsAfter[i]) == 0) continue;
                text.Append("\n").Append(engine.Data.Products[i].name).Append(": ").Append(Cash(p.productsBefore[i])).Append(" → ").Append(Cash(p.productsAfter[i])).Append("/s");
                double before = p.normalBefore.mantissa == 0 ? 0 : Amount.Ratio(p.productsBefore[i], p.normalBefore) * 100;
                double after = p.normalAfter.mantissa == 0 ? 0 : Amount.Ratio(p.productsAfter[i], p.normalAfter) * 100;
                text.Append("\nShop share: ").Append(UpgradeEffectCopy.Whole(before)).Append("% → ").Append(UpgradeEffectCopy.Whole(after)).Append('%');
            }
            if (p.manualTapBefore.CompareTo(p.manualTapAfter) != 0) text.Append("\nNormal manual tap: ").Append(Cash(p.manualTapBefore)).Append(" → ").Append(Cash(p.manualTapAfter));
            if (p.costCategory != AffectedCost.None)
                text.Append("\n").Append(p.costCategory == AffectedCost.ProductLevels ? "Next product level" : p.costCategory == AffectedCost.TapLevels ? "Next tap level" : "Cash upgrades per ₹100 base").Append(": ").Append(Cash(p.costBefore)).Append(" → ").Append(Cash(p.costAfter));
            if (item.effect >= CashEffect.SkillPower && item.effect <= CashEffect.SkillCooldown)
            {
                string unit = item.target == 0 ? "income" : item.target == 1 ? "taps/s" : "seconds of normal sales";
                text.Append("\nPower: ").Append(item.target == 0 ? UpgradeEffectCopy.Multiplier(p.skillPowerBefore) : UpgradeEffectCopy.Whole(p.skillPowerBefore)).Append(" → ").Append(item.target == 0 ? UpgradeEffectCopy.Multiplier(p.skillPowerAfter) : UpgradeEffectCopy.Whole(p.skillPowerAfter)).Append(' ').Append(unit);
                if (item.target != 2) text.Append("\nDuration: ").Append(UpgradeEffectCopy.Whole(p.durationBefore)).Append(" → ").Append(UpgradeEffectCopy.Whole(p.durationAfter)).Append('s');
                text.Append("\nCooldown: ").Append(UpgradeEffectCopy.Whole(p.cooldownBefore)).Append(" → ").Append(UpgradeEffectCopy.Whole(p.cooldownAfter)).Append('s');
                if (p.skillPowerBefore == 0) text.Append("\nThe skill is locked before purchase.");
            }
            return text.ToString();
        }
    }
}
