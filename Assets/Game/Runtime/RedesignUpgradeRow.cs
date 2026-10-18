using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>One Editor-preplaced row, rebound by stable catalogue ID.</summary>
    public sealed class RedesignUpgradeRow : MonoBehaviour
    {
        public TMP_Text title, effect, requirement, price;
        public Button details, buy;
        public string ItemId { get; private set; }
        Action<string> inspect, purchase;
        void Awake()
        {
            if (!title || !effect || !requirement || !price || !details || !buy)
                throw new InvalidOperationException("Incomplete serialized upgrade row: " + name);
            details.onClick.AddListener(Inspect); buy.onClick.AddListener(Purchase);
        }
        void OnDestroy()
        {
            if (details) details.onClick.RemoveListener(Inspect);
            if (buy) buy.onClick.RemoveListener(Purchase);
        }
        void Inspect() { if (ItemId != null) inspect?.Invoke(ItemId); }
        void Purchase() { if (ItemId != null && buy.interactable) purchase?.Invoke(ItemId); }
        public void Bind(CashUpgrade item, RedesignEngine engine, RedesignUpgradeBrowser browser, bool allowed,
            Action<string> onInspect, Action<string> onPurchase)
        {
            ItemId = item.id; inspect = onInspect; purchase = onPurchase;
            title.text = item.name;
            effect.text = browser.PurchaseKind(item.id) + " · " + Effect(item, engine.Data);
            bool owned = browser.Owned(item.id), eligible = engine.Eligible(item.id);
            requirement.text = owned ? "Purchased this run" : eligible ? "Requirements met" : Requirements(item, engine.Data);
            Amount cost = engine.UpgradeCost(item.id);
            price.text = owned ? "Owned" : "₹" + cost;
            buy.interactable = allowed && !owned && eligible && engine.State.cash >= cost;
            details.interactable = true;
        }
        public static string Requirements(CashUpgrade item, RedesignCatalog catalog)
        {
            return "Purchase with cash. No other requirements.";
        }
        public static string Effect(CashUpgrade item, RedesignCatalog catalog)
        {
            string factor = item.factor.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            switch (item.effect)
            {
                case CashEffect.ProductProfit: return catalog.Products[item.target].name + " income ×" + factor;
                case CashEffect.ProductCost: return catalog.Products[item.target].name + " level prices ×" + factor + " of run base";
                case CashEffect.TapDirect: return "Tap profit ×" + factor;
                case CashEffect.TapLinked: return "Add more normal shop income to each tap";
                case CashEffect.TapCost: return "Tap level prices ×" + factor + " of run base";
                case CashEffect.GlobalProfit: return "Global profit ×" + factor;
                case CashEffect.UpgradeCost: return "Upgrade Price Discount: pay ×" + factor + " of base";
                case CashEffect.SkillPower:
                    double power = RedesignCatalog.SkillPower[item.target][item.rank - 1];
                    return item.target == 0 ? "Rush product income ×" + power : item.target == 1 ? "Helping Hands: " + power + " taps/s" : "Bulk Order: " + power + " seconds of normal sales";
                case CashEffect.SkillDuration: return "Increase skill duration";
                case CashEffect.SkillCooldown: return "Shorten skill cooldown";
                case CashEffect.Link: return catalog.Products[item.beneficiary].name + " comeback income ×" + factor;
                case CashEffect.Group: return "Linked product group income ×" + factor;
                default: return item.name;
            }
        }
    }
}
