using System;
using System.Collections.Generic;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved Player panel with a fixed pool of ten unlockable rows.</summary>
    public sealed class ImportedPlayerScreen : MonoBehaviour
    {
        const int PoolSize = 10;
        [SerializeField] Button collapseButton;
        [SerializeField] ScrollRect scroll;
        [SerializeField] Button statsButton;
        [SerializeField] Button skillsButton;
        [SerializeField] Button quantityButton;
        [SerializeField] Button[] quantityChoices;
        [SerializeField] GameObject quantityMenu;
        [SerializeField] Button tapBuyButton;
        [SerializeField] Button prestigeButton;
        [SerializeField] Button buyAllButton;
        [SerializeField] TMP_Text quantityLabel;
        [SerializeField] TMP_Text tapLevelLabel;
        [SerializeField] TMP_Text tapIncomeLabel;
        [SerializeField] TMP_Text tapBuyLabel;
        [SerializeField] TMP_Text prestigeLabel;
        [SerializeField] TMP_Text unlockableCountLabel;
        [SerializeField] GameObject[] upgradeRows;
        [SerializeField] Button[] upgradeButtons;
        [SerializeField] TMP_Text[] upgradeNames;
        [SerializeField] TMP_Text[] upgradeEffects;
        [SerializeField] TMP_Text[] upgradePrices;

        readonly string[] visibleIds = new string[PoolSize];
        GameController controller;
        GameView view;
        int buyMode = 1;
        float refreshClock;
        float preservedScroll = 1f;
        bool restoreScroll;

        void OnDisable()
        {
            if (scroll != null) preservedScroll = scroll.verticalNormalizedPosition;
            if (quantityMenu != null) quantityMenu.SetActive(false);
        }

        void OnEnable() => restoreScroll = true;

        void LateUpdate()
        {
            if (!restoreScroll || scroll == null) return;
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = preservedScroll;
            restoreScroll = false;
        }

        void Awake()
        {
            if (collapseButton == null || scroll == null || statsButton == null || skillsButton == null ||
                quantityButton == null || !Valid(quantityChoices, 4) || quantityMenu == null ||
                tapBuyButton == null || prestigeButton == null || buyAllButton == null ||
                quantityLabel == null || tapLevelLabel == null ||
                tapIncomeLabel == null || tapBuyLabel == null || prestigeLabel == null ||
                unlockableCountLabel == null || !Valid(upgradeRows, PoolSize) ||
                !Valid(upgradeButtons, PoolSize) || !Valid(upgradeNames, PoolSize) ||
                !Valid(upgradeEffects, PoolSize) || !Valid(upgradePrices, PoolSize))
            {
                Debug.LogError("Player canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            collapseButton.onClick.AddListener(() => view?.Switch("Home"));
            statsButton.onClick.AddListener(() => view?.ShowStatistics());
            skillsButton.onClick.AddListener(() => view?.ShowSkills());
            quantityButton.onClick.AddListener(() => UiMotion.Toggle(quantityMenu, controller));
            int[] modes = { 1, 10, 100, int.MaxValue };
            for (int i = 0; i < modes.Length; i++)
            {
                int selected = modes[i];
                quantityChoices[i].onClick.AddListener(() => { buyMode = selected; UiMotion.Hide(quantityMenu, controller); Refresh(); });
            }
            tapBuyButton.onClick.AddListener(BuyTap);
            prestigeButton.onClick.AddListener(() => view?.ShowPrestige());
            buyAllButton.onClick.AddListener(BuyAll);
            for (int i = 0; i < PoolSize; i++)
            {
                int slot = i;
                upgradeButtons[i].onClick.AddListener(() => BuyUpgrade(slot));
            }
            quantityMenu.SetActive(false);
        }

        static bool Valid<T>(T[] items, int size) where T : UnityEngine.Object =>
            items != null && items.Length == size && !Array.Exists(items, x => x == null);

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
            tapLevelLabel.text = "Level " + state.tapLevel;
            tapIncomeLabel.text = "₹" + engine.TapIncome + " per tap";
            Quote tap = engine.TapQuote(buyMode);
            tapBuyLabel.text = state.tapLevel >= Catalog.MaxLevel ? "Max level" :
                tap.quantity > 0 ? "₹" + tap.cost + "\nUpgrade ×" + tap.quantity :
                "₹" + engine.TapCostFor(1) + "\nUpgrade ×1";
            tapBuyButton.interactable = tap.affordable;
            var prestige = engine.PreviewPrestige();
            prestigeLabel.text = prestige.reward > 0 ? "View prestige · +" + prestige.reward : "View prestige";
            var remaining = engine.Data.upgrades.Where(x => !state.upgrades.Contains(x.id)).OrderBy(x => x.price).ThenBy(x => x.id, StringComparer.Ordinal).ToList();
            unlockableCountLabel.text = remaining.Count == 0 ? "All upgrades purchased" : state.upgrades.Count + " / " + engine.Data.upgrades.Length + " purchased";
            buyAllButton.interactable = engine.CanMutate && remaining.Any(x => engine.Unlocked(x) && state.cash >= x.price);
            for (int slot = 0; slot < PoolSize; slot++)
            {
                int index = slot;
                bool present = index < remaining.Count;
                upgradeRows[slot].SetActive(present);
                visibleIds[slot] = present ? remaining[index].id : null;
                if (!present) continue;
                UpgradeDef upgrade = remaining[index];
                upgradeNames[slot].text = upgrade.name;
                upgradeEffects[slot].text = upgrade.Description(engine.Data);
                bool unlocked = engine.Unlocked(upgrade);
                upgradePrices[slot].text = unlocked ? "₹" + upgrade.price + "\nBuy upgrade" : Requirement(upgrade, engine);
                upgradeButtons[slot].interactable = engine.CanMutate && unlocked && state.cash >= upgrade.price;
            }
        }

        static string Requirement(UpgradeDef upgrade, GameEngine engine)
        {
            if (upgrade.effect == Effect.Skill)
                return upgrade.tier == 0 ? "Own " + (upgrade.target + 1) + " products" : "Previous skill rank";
            return "";
        }

        void AnimateRows()
        {
            for (int i = 0; i < upgradeRows.Length; i++)
                if (upgradeRows[i].activeSelf) UiFeedback.Pulse(upgradeRows[i].transform, controller, .025f, .26f + i * .025f);
        }

        void BuyTap()
        {
            if (controller == null) return;
            if (controller.Engine.BuyTap(buyMode))
            {
                controller.Save();
                view?.Notice("Salesmanship improved.");
                view?.RefreshNow();
                UiFeedback.Pulse(tapLevelLabel.transform, controller);
                UiFeedback.Pulse(tapBuyButton.transform, controller, .07f);
                controller.HomeScreen?.RewardFeedback(false);
            }
            Refresh();
        }

        void BuyAll()
        {
            if (controller == null) return;
            int count = controller.Engine.BuyAll();
            if (count > 0)
            {
                controller.Save();
                view?.Notice(count + " upgrades purchased.");
                view?.RefreshNow();
                AnimateRows();
                controller.HomeScreen?.RewardFeedback();
            }
            Refresh();
        }

        void BuyUpgrade(int slot)
        {
            if (controller == null || string.IsNullOrEmpty(visibleIds[slot])) return;
            if (controller.Engine.BuyUpgrade(visibleIds[slot]))
            {
                controller.Save();
                view?.Notice("Unlockable purchased.");
                view?.RefreshNow();
                UiFeedback.Pulse(unlockableCountLabel.transform, controller, .1f);
                AnimateRows();
                controller.HomeScreen?.RewardFeedback();
            }
            Refresh();
        }
    }
}
