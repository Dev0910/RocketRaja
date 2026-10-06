using System;
using System.Globalization;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignHomeScreen : MonoBehaviour
    {
        public Button[] products, skills;
        public TMP_Text[] productNames, productStatus, skillLabels;
        public Image[] productImages;
        public Sprite[] productSprites;
        public Sprite lockedSprite;
        public RedesignProductArt productArt;
        public RedesignShopVisuals shopVisuals;
        public TMP_Text storeName, goalTitle, goalCopy, firstBuyLabel, goalLabel, trainingLabel, walletLabel, counterLabel;
        public Button firstBuy, goal, training, wallet;
        public RedesignTapTarget counter;
        public ScrollRect scroll;
        public TMP_Text[] tapPopups;
        public CanvasGroup[] tapGroups;
        RedesignUiContext context;
        Func<bool> tap;
        UnityAction[] productActions, skillActions;
        string goalRoute, goalArgument;
        float elapsed;
        int popup;
        float[] ages;
        Vector2[] origins;
        static readonly string[] ShelfNames = { "Phuljhari", "Anar", "Chakri", "Rockets", "Crackers", "Strings", "Aerial", "Gift boxes" };
        void Awake()
        {
            if (products.Length != 8 || productNames.Length != 8 || productStatus.Length != 8 || productImages.Length != 8 || productSprites.Length != 8 || skills.Length != 3 || skillLabels.Length != 3 ||
                !lockedSprite || !storeName || !goalTitle || !goalCopy || !firstBuyLabel || !goalLabel || !trainingLabel || !walletLabel || !counterLabel || !firstBuy || !goal || !training || !wallet || !counter || !scroll || tapPopups.Length != 6 || tapGroups.Length != 6)
                throw new InvalidOperationException("Incomplete saved Home screen");
            productActions = new UnityAction[8]; skillActions = new UnityAction[3];
            for (int i = 0; i < 8; i++) { int product = i; productActions[i] = () => context?.Navigate("productdetails", product.ToString(CultureInfo.InvariantCulture)); products[i].onClick.AddListener(productActions[i]); }
            for (int i = 0; i < 3; i++) { int skill = i; skillActions[i] = () => context?.Navigate("skilldetails", skill.ToString(CultureInfo.InvariantCulture)); skills[i].onClick.AddListener(skillActions[i]); }
            firstBuy.onClick.AddListener(BuyFirst); goal.onClick.AddListener(Goal); training.onClick.AddListener(Training); wallet.onClick.AddListener(Wallet); counter.Tap = Tap;
            ages = new float[6]; origins = new Vector2[6];
            for (int i = 0; i < 6; i++) { ages[i] = 1; origins[i] = tapPopups[i].rectTransform.anchoredPosition; tapGroups[i].alpha = 0; }
        }
        void OnDestroy()
        {
            if (productActions != null) for (int i = 0; i < 8; i++) if (products[i]) products[i].onClick.RemoveListener(productActions[i]);
            if (skillActions != null) for (int i = 0; i < 3; i++) if (skills[i]) skills[i].onClick.RemoveListener(skillActions[i]);
            if (firstBuy) firstBuy.onClick.RemoveListener(BuyFirst); if (goal) goal.onClick.RemoveListener(Goal); if (training) training.onClick.RemoveListener(Training); if (wallet) wallet.onClick.RemoveListener(Wallet); if (counter) counter.Tap = null;
        }
        public void Bind(RedesignUiContext value, Func<bool> gameplayTap) { if (context?.Engine != value.Engine) ClearPopups(); context = value; tap = gameplayTap; Refresh(); }
        public void Open() { gameObject.SetActive(true); Refresh(); }
        void BuyFirst()
        {
            if (context == null || context.Engine.State.products[0] != 0) return;
            context.Notice(context.Execute(e => e.State.products[0] == 0 && e.BuyLevels(0, 1)) ? "Your first Phuljhari is on the shelf" : "Purchase unavailable. Check cash or saving."); Refresh();
        }
        void Goal() => context?.Navigate(goalRoute, goalArgument);
        void Training() => context?.Navigate("taptraining", "");
        void Wallet() => context?.Navigate("kajushop", "");
        void Tap()
        {
            if (context == null || tap == null || !context.CanMutate()) return;
            var amount = context.Engine.TapPayout(); if (!tap()) return;
            tapPopups[popup].text = "+" + RedesignUiContext.Cash(amount); tapGroups[popup].alpha = 1; ages[popup] = 0;
            tapPopups[popup].rectTransform.anchoredPosition = origins[popup]; popup = (popup + 1) % 6; Refresh();
        }
        void OnDisable() => ClearPopups();
        void ClearPopups() { if (ages != null) for (int i = 0; i < 6; i++) { ages[i] = 1; tapGroups[i].alpha = 0; } }
        void Update()
        {
            elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); }
            if (ages == null || context == null) return;
            for (int i = 0; i < 6; i++)
            {
                if (ages[i] >= .7f) continue; ages[i] += Time.unscaledDeltaTime;
                bool reduced = context.Engine.State.reducedMotion;
                tapGroups[i].alpha = ages[i] >= .7f ? 0 : reduced ? 1 : Mathf.Clamp01((.7f - ages[i]) / .3f);
                tapPopups[i].rectTransform.anchoredPosition = origins[i] + (reduced ? Vector2.zero : Vector2.up * (18 * 1080f / 390) * Mathf.Clamp01(ages[i] / .7f));
            }
        }
        public void Refresh()
        {
            if (context == null) return; var e = context.Engine; var s = e.State;
            if (shopVisuals) shopVisuals.Refresh(e);
            storeName.richText = false; storeName.text = RedesignRetainedUi.DisplayName(s);
            for (int i = 0; i < 8; i++)
            {
                bool owned = s.products[i] > 0, available = e.ProductAvailable(i); productNames[i].text = ShelfNames[i];
                productStatus[i].text = owned ? "Lv " + s.products[i] : available ? "Empty" : "Locked";
                productImages[i].sprite = available ? (productArt ? productArt.At(i, Math.Max(0, e.ProductAppearance(i))) : productSprites[i]) : lockedSprite; productImages[i].color = owned ? Color.white : new Color(1, 1, 1, .45f);
            }
            for (int i = 0; i < 3; i++)
            {
                double active = Math.Max(0, s.activeUntil[i] - s.now), wait = Math.Max(0, s.readyAt[i] - s.now);
                skillLabels[i].text = (i == 0 ? "Rush" : i == 1 ? "Hands" : "Order") + "\n" + (!e.SkillOwned(i) ? "View unlock" : active > 0 ? "Active " + Math.Ceiling(active) + "s" : wait > 0 ? Math.Ceiling(wait) + "s" : "Ready");
            }
            firstBuy.gameObject.SetActive(s.products[0] == 0); firstBuy.interactable = context.CanMutate() && e.LevelQuote(0, 1).affordable;
            firstBuyLabel.text = "Buy Phuljhari · " + RedesignUiContext.Cash(e.LevelQuote(0, 1).cost);
            bool pin = e.Data.TryUpgrade(s.pinnedUpgradeId, out int p) && !s.upgrades.Contains(s.pinnedUpgradeId);
            goal.gameObject.SetActive(s.products[0] > 0 || pin);
            if (pin) { goalTitle.text = "Your pinned goal"; goalCopy.text = e.Data.Upgrades[p].name; goalRoute = "upgradedetails"; goalArgument = s.pinnedUpgradeId; goalLabel.text = "View pinned upgrade"; }
            else if (s.products[0] == 0) { goalTitle.text = "Start your shop"; goalCopy.text = "Buy your first Phuljhari manually. Your " + RedesignUiContext.Cash(e.LevelQuote(0, 1).cost) + " opening grant is ready."; }
            else
            {
                bool prestige = s.eligibleEarnings >= s.target; goalTitle.text = prestige ? "Ready to grow your Fame" : "Grow this run";
                goalCopy.text = "Earned " + RedesignUiContext.Cash(s.eligibleEarnings) + " / " + RedesignUiContext.Cash(s.target) + " prestige target";
                goalRoute = prestige ? "prestige" : "upgrades"; goalArgument = ""; goalLabel.text = prestige ? "Review Prestige" : "Browse cash upgrades";
            }
            trainingLabel.text = RedesignUiContext.Cash(e.TapPayout()) + " per tap · Train taps";
            walletLabel.text = RedesignCardCopy.Count(s.kajuKatli) + " Kaju Katli · Test shop";
            counterLabel.text = context.CanMutate() ? "Tap the counter to earn" : "Saving needs attention";
        }
    }
}
