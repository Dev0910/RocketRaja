using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved shared shop, Home dashboard and navigation from the HTML design.</summary>
    public sealed class ImportedHomeScreen : MonoBehaviour
    {
        [SerializeField] GameObject homePanel;
        [SerializeField] ShopTapTarget stallTap;
        [SerializeField] Button settingsButton;
        [SerializeField] Button[] navigationButtons;
        [SerializeField] Button[] shelfButtons;
        [SerializeField] Button[] skillButtons;
        [SerializeField] Button sponsorButton;
        [SerializeField] Image[] shelfArt;
        [SerializeField] TMP_Text[] emptyShelfMarks;
        [SerializeField] TMP_Text[] shelfNames;
        [SerializeField] TMP_Text[] skillStatus;
        [SerializeField] Image[] skillImages;
        [SerializeField] Sprite[] skillArtwork;
        [SerializeField] Sprite lockedSkillIcon;
        [SerializeField] TMP_Text cashLabel;
        [SerializeField] TMP_Text milestoneKicker;
        [SerializeField] TMP_Text milestoneLabel;
        [SerializeField] TMP_Text incomeLabel;
        [SerializeField] TMP_Text tapLabel;
        [SerializeField] TMP_Text normalLabel;
        [SerializeField] TMP_Text runLabel;
        [SerializeField] TMP_Text sponsorLabel;
        [SerializeField] TMP_Text tapHintLabel;
        [SerializeField] TMP_Text counterLabel;
        [SerializeField] TMP_Text noticeLabel;
        [SerializeField] GameObject noticeRoot;
        [SerializeField] RectTransform[] tapPopupRects;
        [SerializeField] TMP_Text[] tapPopupLabels;
        [SerializeField] Image celebrationImage;
        [SerializeField] UiCooldown[] skillCooldowns;
        [SerializeField] UiSkillRing[] skillRings;
        [SerializeField] FestivalSparkles sparkles;
        [SerializeField] ShopAmbient ambient;

        GameController controller;
        GameView view;
        float refreshClock, noticeClock;
        int popupIndex;
        readonly float[] popupAges = new float[12];
        readonly Vector2[] popupOrigins = new Vector2[12];
        readonly bool[] popupStatic = new bool[12];
        readonly int[] lastLevels = new int[8];
        readonly int[] lastTiers = new int[8];
        readonly bool[] skillReady = new bool[3];
        bool observedState, wasPrestigeReady;
        string selectedTab = "Home";
        RectTransform keeper;
        bool hadMotion = true;

        void Awake()
        {
            if (homePanel == null || stallTap == null || settingsButton == null ||
                !Valid(navigationButtons, 3) || !Valid(shelfButtons, 8) || !Valid(skillButtons, 3) ||
                sponsorButton == null || !Valid(shelfArt, 8) || !Valid(emptyShelfMarks, 8) || !Valid(shelfNames, 8) ||
                !Valid(skillStatus, 3) || !Valid(skillImages, 3) || !Valid(skillArtwork, 3) ||
                lockedSkillIcon == null || cashLabel == null || milestoneKicker == null || milestoneLabel == null ||
                incomeLabel == null || tapLabel == null || normalLabel == null || runLabel == null ||
                sponsorLabel == null || tapHintLabel == null || counterLabel == null ||
                noticeLabel == null || noticeRoot == null ||
                !Valid(tapPopupRects, 12) || !Valid(tapPopupLabels, 12) || !Valid(skillCooldowns, 3) || !Valid(skillRings, 3) || celebrationImage == null || sparkles == null || ambient == null)
            {
                Debug.LogError("Home canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            settingsButton.onClick.AddListener(OpenSettings);
            sponsorButton.onClick.AddListener(OpenSponsor);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                navigationButtons[i].onClick.AddListener(() => Switch(index == 0 ? "Player" : index == 1 ? "Products" : "Legacy"));
                skillButtons[i].onClick.AddListener(() => ActivateSkill(index));
            }
            for (int i = 0; i < 8; i++)
            {
                int index = i;
                shelfButtons[i].onClick.AddListener(() => view?.ShowProductDetail(index));
            }
            noticeRoot.SetActive(false);
            for (int i = 0; i < tapPopupRects.Length; i++) tapPopupRects[i].gameObject.SetActive(false);
            celebrationImage.gameObject.SetActive(false);
            keeper = stallTap.transform.Find("Shopkeeper") as RectTransform;
        }

        static bool Valid<T>(T[] array, int length) where T : UnityEngine.Object =>
            array != null && array.Length == length && !Array.Exists(array, x => x == null);

        void OnDestroy()
        {
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            if (sponsorButton != null) sponsorButton.onClick.RemoveListener(OpenSponsor);
        }

        public void Bind(GameController owner, GameView ownerView)
        {
            controller = owner;
            view = ownerView;
            UiFeedback.Wire(gameObject, owner);
            sparkles.Bind(owner);
            ambient.Bind(owner);
            stallTap.onTap = owner.Tap;
            Refresh();
        }

        public void SetHomeVisible(bool visible) => UiMotion.SetVisible(homePanel, visible, controller);
        public void SettlePanelExit() => UiMotion.SettleExit(homePanel);

        void OnDisable()
        {
            if (sparkles != null) sparkles.Clear();
            noticeClock = 0;
            if (noticeRoot != null) noticeRoot.SetActive(false);
            if (tapPopupRects == null) return;
            foreach (var popup in tapPopupRects)
                if (popup != null) { popup.gameObject.SetActive(false); popup.localScale = Vector3.one; popup.anchoredPosition = Vector2.zero; }
        }

        public void SelectNavigation(string tab)
        {
            if (selectedTab == tab) return;
            selectedTab = tab;
            int index = tab == "Player" ? 0 : tab == "Products" ? 1 : tab == "Legacy" ? 2 : -1;
            for (int i = 0; i < navigationButtons.Length; i++) UiFeedback.Select(navigationButtons[i].transform, controller, i == index);
        }

        void Switch(string tab) => view?.Switch(tab);
        void OpenSettings() => view?.ShowSettings();
        void OpenSponsor() => view?.ShowSponsor();

        void ActivateSkill(int index)
        {
            if (controller == null || controller.Engine == null) return;
            var engine = controller.Engine;
            if (engine.ActivateSkill(index))
            {
                view?.Notice(Catalog.SkillNames[index] + " activated!");
                Celebrate();
                UiFeedback.Pulse(skillButtons[index].transform, controller, .12f, .38f);
            }
            else view?.Notice(engine.SkillRank(index) == 0 ? "Unlock this skill in Player." : "This skill is cooling down.");
            view?.RefreshNow();
        }

        void Update()
        {
            if (controller == null || controller.Engine == null) return;
            for (int i = 0; i < skillCooldowns.Length; i++)
            {
                var engine = controller.Engine; var state = engine.State; var skill = state.skills[i];
                double cooldown = engine.Data.skills[i].cooldown * (1 - engine.Data.legacy[11].valuePerRank * state.Legacy(11));
                skillCooldowns[i].Set(engine.SkillRank(i) > 0 && skill.activeUntil <= state.now ? (float)((skill.readyAt - state.now) / cooldown) : 0);
            }
            refreshClock += Time.unscaledDeltaTime;
            if (refreshClock >= .2f) { refreshClock = 0; Refresh(); }
            if (noticeClock > 0 && (noticeClock -= Time.unscaledDeltaTime) <= 0) UiMotion.Hide(noticeRoot, controller);
            bool motion = !controller.Engine.State.reducedMotion;
            if (!motion)
            {
                if (hadMotion) sparkles.Clear();
                celebrationImage.gameObject.SetActive(false);
            }
            hadMotion = motion;
            for (int i = 0; i < popupAges.Length; i++)
            {
                if (!tapPopupRects[i].gameObject.activeSelf) continue;
                popupAges[i] += Time.unscaledDeltaTime;
                if ((!motion && !popupStatic[i]) || popupAges[i] >= (popupStatic[i] ? .65f : .9f))
                { tapPopupRects[i].gameObject.SetActive(false); continue; }
                float eased = popupStatic[i] ? 0 : UiEasing.EaseOut(popupAges[i] / .9f);
                tapPopupRects[i].anchoredPosition = popupOrigins[i] + Vector2.up * (38 * eased * UiEasing.Pixels(tapPopupRects[i]));
                tapPopupRects[i].localScale = Vector3.one;
                var color = tapPopupLabels[i].color;
                color.a = 1 - eased;
                tapPopupLabels[i].color = color;
            }
        }

        public void Refresh()
        {
            if (controller == null || controller.Engine == null) return;
            var engine = controller.Engine;
            var state = engine.State;
            cashLabel.text = "₹" + state.cash;
            int owned = state.OwnedProducts;
            if (owned == 0)
            {
                milestoneKicker.text = "FIRST PRODUCT";
                milestoneLabel.text = engine.Data.products[0].name + " · ₹" + engine.ProductCost(0, 1) + " to stock your first shelf";
            }
            else
            {
                int best = -1, remaining = int.MaxValue;
                for (int i = 0; i < 8; i++)
                {
                    int level = state.Level(i);
                    if (level == 0) continue;
                    foreach (int milestone in Catalog.VisualLevels)
                        if (milestone > level && milestone - level < remaining)
                        { best = i; remaining = milestone - level; break; }
                }
                if (best >= 0)
                {
                    milestoneKicker.text = "NEXT DISPLAY";
                    milestoneLabel.text = engine.Data.products[best].name + " · " + remaining + " levels to upgrade";
                }
                else if (owned < 8)
                {
                    milestoneKicker.text = "NEXT SHELF";
                    milestoneLabel.text = engine.Data.products[owned].name + " · ₹" + engine.ProductCost(owned, 1);
                }
                else
                {
                    milestoneKicker.text = "PRESTIGE";
                    milestoneLabel.text = engine.PreviewPrestige().reward + " reputation available";
                }
            }
            incomeLabel.text = "₹" + engine.LiveIncome;
            tapLabel.text = "₹" + engine.TapIncome;
            normalLabel.text = "Normal sales ₹" + engine.NormalIncome + " / sec" + (state.sponsorUntil > state.now ? " · Sponsor ×2" : "");
            runLabel.text = "This run · ₹" + state.runEarnings + " earned";
            for (int i = 0; i < 8; i++)
            {
                bool visible = state.Level(i) > 0;
                shelfArt[i].enabled = visible;
                emptyShelfMarks[i].gameObject.SetActive(!visible);
                shelfNames[i].text = engine.Data.products[i].name;
                shelfButtons[i].interactable = true;
                int tier = engine.VisualTier(i);
                if (observedState && state.Level(i) > lastLevels[i])
                {
                    UiFeedback.Pulse(shelfArt[i].transform, controller, tier > lastTiers[i] ? .24f : .12f, .55f);
                    UiFeedback.Pulse(incomeLabel.transform, controller, .07f);
                    if (tier > lastTiers[i]) Celebrate();
                }
                lastLevels[i] = state.Level(i);
                lastTiers[i] = tier;
            }
            for (int i = 0; i < 3; i++)
            {
                int rank = engine.SkillRank(i);
                var skill = state.skills[i];
                skillRings[i].SetState(skill.activeUntil > state.now, rank == 0, controller);
                skillStatus[i].text = rank == 0 ? "LOCKED" : skill.activeUntil > state.now ?
                    "ACTIVE · " + GameView.Duration(skill.activeUntil - state.now) :
                    skill.readyAt > state.now ? "WAIT · " + GameView.Duration(skill.readyAt - state.now) : "READY";
                skillButtons[i].interactable = rank > 0 && skill.readyAt <= state.now;
                bool ready = rank > 0 && skill.readyAt <= state.now;
                if (observedState && ready && !skillReady[i]) UiFeedback.Pulse(skillButtons[i].transform, controller, .08f, .38f);
                skillReady[i] = ready;
                skillImages[i].sprite = rank == 0 ? lockedSkillIcon : skillArtwork[i];
            }
            tapHintLabel.text = owned == 0 ? "TAP TO EARN · THEN OPEN PRODUCTS" : "TAP THE STALL TO EARN";
            tapHintLabel.fontSize = owned == 0 ? 30 : 39;
            counterLabel.text = owned == 0 ? "" : "PATAAKHEIN, ROSHAN DIL";
            sponsorLabel.text = state.sponsorUntil > state.now ?
                "SPONSOR ACTIVE · " + GameView.Duration(state.sponsorUntil - state.now) :
                "OPTIONAL SPONSOR · ×2 sales for five minutes";
            sponsorButton.interactable = state.sponsorUntil <= state.now;
            bool prestigeReady = engine.PreviewPrestige().reward > 0;
            if (observedState && prestigeReady && !wasPrestigeReady)
            {
                Notice("Prestige is ready. Begin again, brighter!");
                UiFeedback.Pulse(navigationButtons[0].transform, controller, .1f, .65f);
                Celebrate();
            }
            wasPrestigeReady = prestigeReady;
            observedState = true;
        }

        public void Notice(string message)
        {
            noticeLabel.text = message;
            UiMotion.Show(noticeRoot, controller);
            // Unity can reset sorting overrides when re-enabling a nested canvas.
            // Set it after activation so the toast stays above the lower panels.
            var noticeCanvas = noticeRoot.GetComponent<Canvas>();
            if (noticeCanvas != null) { noticeCanvas.overrideSorting = true; noticeCanvas.sortingOrder = 16; }
            var noticeGroup = noticeRoot.GetComponent<CanvasGroup>();
            noticeGroup.blocksRaycasts = false;
            noticeClock = 3;
        }

        public void TapFeedback()
        {
            if (controller == null) return;
            int index = popupIndex++ % 6;
            popupAges[index] = 0;
            popupStatic[index] = controller.Engine.State.reducedMotion;
            var parent = (RectTransform)tapPopupRects[index].parent;
            tapPopupRects[index].anchorMin = tapPopupRects[index].anchorMax = parent.pivot;
            var screenPosition = stallTap.TapPosition ?? RectTransformUtility.WorldToScreenPoint(
                stallTap.TapCamera, stallTap.transform.TransformPoint(((RectTransform)stallTap.transform).rect.center));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, stallTap.TapCamera, out popupOrigins[index]);
            tapPopupRects[index].anchoredPosition = popupOrigins[index];
            tapPopupRects[index].localScale = Vector3.one;
            tapPopupLabels[index].text = "+₹" + controller.Engine.TapIncome;
            tapPopupLabels[index].color = new Color32(22, 75, 128, 255);
            tapPopupRects[index].gameObject.SetActive(true);
            UiFeedback.Pulse(cashLabel.transform, controller, .035f, .18f);
            UiFeedback.Pulse(keeper, controller, .035f, .2f);
            controller.GetComponent<ShopAudio>()?.Tap();
        }

        public void Celebrate()
        {
            if (controller != null && controller.Engine.State.reducedMotion) return;
            var area = sparkles.rectTransform.rect;
            sparkles.Burst(new Vector2(area.center.x, area.yMin + area.height * .8f));
        }

        public void RewardFeedback(bool burst = true)
        {
            UiFeedback.Pulse(cashLabel.transform, controller, .09f, .45f);
            UiFeedback.Pulse(incomeLabel.transform, controller, .09f, .45f);
            if (burst) Celebrate();
        }

        public void CelebratePrestige()
        {
            RewardFeedback();
            if (controller == null || controller.Engine.State.reducedMotion) return;
            UiFeedback.Pulse(milestoneLabel.transform, controller, .08f, .8f);
        }
    }
}
