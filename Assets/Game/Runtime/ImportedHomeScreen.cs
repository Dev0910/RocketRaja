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
        [SerializeField] Button inspectSignButton;
        [SerializeField] Button[] navigationButtons;
        [SerializeField] TMP_Text[] navigationLabels;
        [SerializeField] Image[] navigationIndicators;
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
        [SerializeField] TMP_Text incomeLabel;
        [SerializeField] TMP_Text tapLabel;
        [SerializeField] TMP_Text sponsorLabel;
        [SerializeField] TMP_Text tapHintLabel;
        [SerializeField] LayoutElement nextActionLayout;
        [SerializeField] TMP_Text nextGoalLabel;
        [SerializeField] TMP_Text nextProgressLabel;
        [SerializeField] Button nextProductsButton;
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
        readonly float[] popupAges = new float[12];
        readonly Vector2[] popupOrigins = new Vector2[12];
        readonly bool[] popupStatic = new bool[12];
        readonly float[] popupRises = new float[12];
        Amount burstReward, lastTapIncome;
        float lastTapAt = float.NegativeInfinity;
        const float TapPopupDuration = .70f;
        const float TapPopupReadableDuration = .55f;
        const float TapBurstWindow = .4f;
        readonly int[] lastLevels = new int[8];
        readonly int[] lastTiers = new int[8];
        readonly bool[] skillReady = new bool[3];
        // Compact labels match the painted tags; Products retains the catalog's full names.
        static readonly string[] ShelfLabels = { "Phuljhari", "Anar", "Chakri", "Rockets", "Crackers", "Strings", "Aerial", "Gift boxes" };
        enum GoalKind { Stock, Upgrade, Prestige, Mastered }
        struct Goal
        {
            public GoalKind kind;
            public int product;
            public Amount cost;
            public long prestigeReward;
            public bool starter;
        }

        // A small share of the strongest available gain keeps low-impact cheap levels off the card.
        const double MinimumGainShare = .1;
        const double NearGoalSeconds = 90;
        readonly Amount[] upgradeCosts = new Amount[8];
        readonly Amount[] upgradeGains = new Amount[8];
        bool observedState, wasPrestigeReady;
        string selectedTab = "Home";
        RectTransform keeper;
        TMP_Text nextActionKicker, nextActionButtonLabel, sponsorCaption;
        bool hadMotion = true;
        public bool HasNotice => noticeClock > 0;
        [SerializeField] LayoutElement sponsorLayout;

        void Awake()
        {
            if (homePanel == null || stallTap == null || settingsButton == null || inspectSignButton == null ||
                !Valid(navigationButtons, 3) || !Valid(navigationLabels, 3) || !Valid(navigationIndicators, 3) ||
                !Valid(shelfButtons, 8) || !Valid(skillButtons, 3) ||
                !Valid(shelfArt, 8) || !Valid(emptyShelfMarks, 8) || !Valid(shelfNames, 8) ||
                !Valid(skillStatus, 3) || !Valid(skillImages, 3) || !Valid(skillArtwork, 3) ||
                lockedSkillIcon == null || cashLabel == null ||
                incomeLabel == null || tapLabel == null ||
                tapHintLabel == null ||
                nextActionLayout == null || nextGoalLabel == null || nextProgressLabel == null || nextProductsButton == null ||
                counterLabel == null ||
                noticeLabel == null || noticeRoot == null ||
                !Valid(tapPopupRects, 12) || !Valid(tapPopupLabels, 12) || !Valid(skillCooldowns, 3) || !Valid(skillRings, 3) || celebrationImage == null || sparkles == null || ambient == null)
            {
                Debug.LogError("Home canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            settingsButton.onClick.AddListener(OpenSettings);
            inspectSignButton.onClick.AddListener(OpenStoreNameDetail);
            if (sponsorLayout != null) sponsorLayout.gameObject.SetActive(false);
            nextProductsButton.onClick.AddListener(ActivateNextAction);
            if (!CacheNextActionText())
            {
                Debug.LogError("Home next-action card has missing editor-authored text.", this);
                enabled = false;
                return;
            }
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

        bool CacheNextActionText()
        {
            if (nextActionKicker == null)
                nextActionKicker = nextActionLayout?.transform.Find("Next Action Kicker")?.GetComponent<TMP_Text>();
            if (nextActionButtonLabel == null)
                nextActionButtonLabel = nextProductsButton?.GetComponentInChildren<TMP_Text>(true);
            if (sponsorCaption == null)
                sponsorCaption = sponsorLayout?.transform.Find("Sponsor Caption")?.GetComponent<TMP_Text>();
            return nextActionKicker != null && nextActionButtonLabel != null;
        }

        void OnDestroy()
        {
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            if (inspectSignButton != null) inspectSignButton.onClick.RemoveListener(OpenStoreNameDetail);
            if (sponsorButton != null) sponsorButton.onClick.RemoveListener(OpenSponsor);
            if (nextProductsButton != null) nextProductsButton.onClick.RemoveListener(ActivateNextAction);
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
            lastTapAt = float.NegativeInfinity;
            burstReward = lastTapIncome = Amount.Zero;
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
            for (int i = 0; i < navigationButtons.Length; i++)
            {
                bool selected = i == index;
                UiFeedback.Select(navigationButtons[i].transform, controller, selected);
                navigationLabels[i].color = selected ? new Color32(255, 227, 166, 255) : Color.white;
                navigationIndicators[i].enabled = selected;
            }
        }

        void Switch(string tab) => view?.Switch(tab);
        void OpenSettings() => view?.ShowSettings();
        void OpenStoreNameDetail() => view?.ShowStoreNameDetail();
        void OpenSponsor() { }
        void ActivateNextAction()
        {
            if (controller == null || controller.Engine == null) return;
            var engine = controller.Engine;
            Goal goal = RecommendGoal(engine);
            if (goal.kind == GoalKind.Prestige) { view?.ShowPrestige(); return; }
            if (goal.kind == GoalKind.Mastered) { view?.Switch("Player"); return; }
            if (goal.kind == GoalKind.Stock && goal.product == 0 && engine.State.cash < goal.cost &&
                (goal.starter || engine.NormalIncome <= Amount.Zero))
            {
                controller.Tap();
                return;
            }
            view?.ShowProductDetail(goal.product);
        }

        static bool NearOrAffordable(Amount cost, Amount cash, Amount normalIncome) =>
            cost <= cash || (normalIncome > Amount.Zero && cost <= cash + normalIncome * NearGoalSeconds);

        Goal RecommendGoal(GameEngine engine)
        {
            var state = engine.State;
            long prestigeReward = engine.PreviewPrestige().reward;
            if (prestigeReward > 0)
                return new Goal { kind = GoalKind.Prestige, prestigeReward = prestigeReward };

            int nextShelf = -1;
            for (int i = 0; i < ShelfLabels.Length; i++)
                if (state.Level(i) == 0) { nextShelf = i; break; }

            Amount normalIncome = engine.NormalIncome;
            if (nextShelf >= 0)
            {
                Amount shelfCost = engine.ProductCost(nextShelf, 1);
                // Keep the first tap-to-stock loop intact even without passive income.
                if ((nextShelf == 0 && state.onboarding < 3) ||
                    NearOrAffordable(shelfCost, state.cash, normalIncome))
                    return new Goal { kind = GoalKind.Stock, product = nextShelf, cost = shelfCost,
                        starter = nextShelf == 0 && state.onboarding < 3 };
            }

            int upgrade = BestUpgrade(engine, normalIncome, true);
            // After every shelf is stocked, keep showing a useful upgrade even when it takes longer to earn.
            if (upgrade < 0 && nextShelf < 0)
                upgrade = BestUpgrade(engine, normalIncome, false);
            if (upgrade >= 0)
                return new Goal { kind = GoalKind.Upgrade, product = upgrade, cost = upgradeCosts[upgrade] };

            if (nextShelf >= 0)
                return new Goal { kind = GoalKind.Stock, product = nextShelf,
                    cost = engine.ProductCost(nextShelf, 1), starter = nextShelf == 0 && state.onboarding < 3 };
            return new Goal { kind = GoalKind.Mastered };
        }

        int BestUpgrade(GameEngine engine, Amount normalIncome, bool nearOnly)
        {
            var state = engine.State;
            Amount strongestGain = Amount.Zero;
            for (int i = 0; i < ShelfLabels.Length; i++)
            {
                upgradeCosts[i] = upgradeGains[i] = Amount.Zero;
                int level = state.Level(i);
                if (level <= 0 || level >= Catalog.MaxLevel) continue;
                Amount cost = engine.ProductCost(i, 1);
                Amount gain = engine.ProductIncome(i, 1) - engine.ProductIncome(i);
                if (gain <= Amount.Zero) continue;
                upgradeCosts[i] = cost;
                upgradeGains[i] = gain;
                if (gain > strongestGain) strongestGain = gain;
            }
            if (strongestGain <= Amount.Zero) return -1;

            int best = -1;
            double bestEfficiency = double.NegativeInfinity;
            Amount bestGain = Amount.Zero;
            for (int i = 0; i < ShelfLabels.Length; i++)
            {
                Amount gain = upgradeGains[i];
                if (gain < strongestGain * MinimumGainShare ||
                    nearOnly && !NearOrAffordable(upgradeCosts[i], state.cash, normalIncome)) continue;
                double efficiency = gain.Log10 - upgradeCosts[i].Log10;
                // Within five percent of the best payback, favor the more visible income gain.
                if (best < 0 || efficiency > bestEfficiency + Math.Log10(1.05) ||
                    Math.Abs(efficiency - bestEfficiency) <= Math.Log10(1.05) && gain > bestGain)
                {
                    best = i;
                    bestEfficiency = Math.Max(bestEfficiency, efficiency);
                    bestGain = gain;
                }
            }
            return best;
        }

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
                double cooldown = engine.SkillCooldown(i);
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
                float duration = popupStatic[i] ? .65f : TapPopupDuration;
                if ((!motion && !popupStatic[i]) || popupAges[i] >= duration)
                { tapPopupRects[i].gameObject.SetActive(false); continue; }
                float eased = popupStatic[i] ? 0 : UiEasing.EaseOut(popupAges[i] / duration);
                tapPopupRects[i].anchoredPosition = popupOrigins[i] + Vector2.up * (popupRises[i] * eased);
                tapPopupRects[i].localScale = Vector3.one;
                var color = tapPopupLabels[i].color;
                // Keep the reward readable while it rises. Fade only after its dwell,
                // then remove it at the end of the brief exit animation.
                color.a = popupStatic[i] ? 1f : Mathf.Clamp01(
                    (TapPopupDuration - popupAges[i]) / (TapPopupDuration - TapPopupReadableDuration));
                tapPopupLabels[i].color = color;
            }
        }

        public void Refresh()
        {
            if (controller == null || controller.Engine == null) return;
            // Bind can refresh this screen before Unity invokes its Awake.
            if (!CacheNextActionText()) return;
            var engine = controller.Engine;
            var state = engine.State;
            cashLabel.text = "₹" + state.cash;
            incomeLabel.text = "₹" + engine.LiveIncome;
            tapLabel.text = "₹" + engine.TapIncome;
            for (int i = 0; i < 8; i++)
            {
                bool visible = state.Level(i) > 0;
                shelfArt[i].enabled = visible;
                emptyShelfMarks[i].gameObject.SetActive(!visible);
                shelfNames[i].text = ShelfLabels[i];
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
                skillRings[i].SetState(skill.activeUntil > state.now, rank == 0, controller, skill.readyAt > state.now);
                bool timed = rank > 0 && (skill.activeUntil > state.now || skill.readyAt > state.now);
                skillStatus[i].fontSize = timed ? 28 : 34;
                skillStatus[i].text = rank == 0 ? "LOCKED" : skill.activeUntil > state.now ?
                    "ACTIVE · " + GameView.Duration(skill.activeUntil - state.now) :
                    skill.readyAt > state.now ? "COOLDOWN · " + GameView.Duration(skill.readyAt - state.now) : "READY";
                skillButtons[i].interactable = engine.CanMutate &&
                    (rank == 0 || skill.readyAt <= state.now && skill.activeUntil <= state.now);
                bool ready = rank > 0 && skill.readyAt <= state.now;
                if (observedState && ready && !skillReady[i]) UiFeedback.Pulse(skillButtons[i].transform, controller, .08f, .38f);
                skillReady[i] = ready;
                skillImages[i].sprite = rank == 0 ? lockedSkillIcon : skillArtwork[i];
                skillImages[i].color = rank == 0 ? new Color(.55f, .55f, .55f, .7f) : skill.activeUntil > state.now ? new Color(1, .85f, .45f) : skill.readyAt > state.now ? new Color(.5f, .5f, .5f, .65f) : Color.white;
            }
            if (state.onboarding >= 3 || state.OwnedProducts > 0)
                tapHintLabel.text = "Tap for ₹" + engine.TapIncome;
            else if (state.onboarding == 0)
                tapHintLabel.text = "Tap to earn";
            else
            {
                Amount firstCost = engine.ProductCost(0, 1);
                if (state.cash >= firstCost)
                    tapHintLabel.text = "Shelf ready";
                else
                    tapHintLabel.text = "Earn ₹" + (firstCost - state.cash);
            }
            counterLabel.richText = false;
            counterLabel.text = state.StoreName;
            if (sponsorLayout != null) sponsorLayout.gameObject.SetActive(false);
            nextActionLayout.preferredHeight = 240;
            nextActionKicker.fontSize = nextProgressLabel.fontSize = 32;
            var nextButtonRect = (RectTransform)nextProductsButton.transform;
            nextButtonRect.anchorMin = new Vector2(nextButtonRect.anchorMin.x, .12f);
            nextButtonRect.anchorMax = new Vector2(nextButtonRect.anchorMax.x, .88f);
            Goal goal = RecommendGoal(engine);
            if (goal.kind == GoalKind.Prestige)
            {
                nextActionKicker.text = "PRESTIGE READY";
                nextGoalLabel.text = "Begin again brighter";
                nextProgressLabel.text = "+" + goal.prestigeReward + " reputation";
                nextActionButtonLabel.text = "Prestige";
            }
            else if (goal.kind == GoalKind.Mastered)
            {
                nextActionKicker.text = "ALL STOCKED";
                nextGoalLabel.text = "Shelves mastered";
                nextProgressLabel.text = "Keep earning prestige";
                nextActionButtonLabel.text = "View Player";
            }
            else
            {
                bool affordable = state.cash >= goal.cost;
                bool tapToEarn = goal.kind == GoalKind.Stock && goal.product == 0 && !affordable &&
                    (goal.starter || engine.NormalIncome <= Amount.Zero);
                bool near = NearOrAffordable(goal.cost, state.cash, engine.NormalIncome);
                nextActionKicker.text = goal.starter ? "STARTER OFFER" :
                    goal.kind == GoalKind.Upgrade ? (near ? "NEXT UPGRADE" : "SAVE FOR UPGRADE") :
                    near ? "NEXT SHELF" : "SAVE FOR SHELF";
                nextGoalLabel.text = (goal.kind == GoalKind.Stock ? "Stock " : "Upgrade ") + ShelfLabels[goal.product];
                nextProgressLabel.text = affordable ? "Ready · ₹" + goal.cost : "Earn ₹" + (goal.cost - state.cash);
                nextActionButtonLabel.text = tapToEarn ? "Tap to earn" :
                    goal.kind == GoalKind.Stock ? "View shelf" : "View upgrade";
            }
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
            if (controller == null || controller.Engine == null) return;
            var popup = tapPopupRects[0];
            var reward = controller.Engine.TapIncome;
            bool reduced = controller.Engine.State.reducedMotion;
            bool continuingBurst = popup.gameObject.activeSelf &&
                Time.unscaledTime - lastTapAt <= TapBurstWindow &&
                lastTapIncome.CompareTo(reward) == 0 && popupStatic[0] == reduced;
            burstReward = continuingBurst ? burstReward + reward : reward;
            lastTapIncome = reward;
            lastTapAt = Time.unscaledTime;
            // One refreshed label communicates a rapid burst without covering the shop shelves.
            for (int i = 1; i < tapPopupRects.Length; i++)
                if (tapPopupRects[i].gameObject.activeSelf) tapPopupRects[i].gameObject.SetActive(false);
            popupAges[0] = 0;
            popupStatic[0] = reduced;
            tapPopupLabels[0].text = "+₹" + burstReward;
            tapPopupLabels[0].textWrappingMode = TextWrappingModes.NoWrap;
            tapPopupLabels[0].raycastTarget = false;
            tapPopupLabels[0].color = Color.white;
            var screenPosition = stallTap.TapPosition ?? RectTransformUtility.WorldToScreenPoint(
                stallTap.TapCamera, stallTap.transform.TransformPoint(((RectTransform)stallTap.transform).rect.center));
            PlaceTapPopup(popup, screenPosition, stallTap.TapCamera);
            popup.anchoredPosition = popupOrigins[0];
            popup.localScale = Vector3.one;
            popup.gameObject.SetActive(true);
            UiFeedback.Pulse(cashLabel.transform, controller, .035f, .18f);
            UiFeedback.Pulse(keeper, controller, .035f, .2f);
            controller.GetComponent<ShopAudio>()?.Tap();
        }

        void PlaceTapPopup(RectTransform popup, Vector2 screenPosition, Camera tapCamera)
        {
            var world = (RectTransform)popup.parent;
            popup.anchorMin = popup.anchorMax = world.pivot;
            float pixel = UiEasing.Pixels(popup);
            float margin = 4 * pixel;
            var hint = (RectTransform)tapHintLabel.transform.parent;
            Bounds hintBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(world, hint);
            float shelfBottom = float.PositiveInfinity;
            foreach (var shelf in shelfNames)
                shelfBottom = Mathf.Min(shelfBottom,
                    RectTransformUtility.CalculateRelativeRectTransformBounds(world, shelf.rectTransform).min.y);
            float laneBottom = hintBounds.max.y + margin;
            float laneHeight = Mathf.Max(1, shelfBottom - margin - laneBottom);
            float popupHeight = Mathf.Min(80, laneHeight * .78f);
            float bottomSpace = 5 * pixel;
            popupRises[0] = Mathf.Min(12 * pixel,
                Mathf.Max(0, laneHeight - popupHeight - bottomSpace));

            float left = hintBounds.min.x + margin;
            float right = hintBounds.max.x - margin;
            if (keeper != null)
                right = Mathf.Min(right,
                    RectTransformUtility.CalculateRelativeRectTransformBounds(world, keeper).min.x - margin);
            float textWidth = tapPopupLabels[0].GetPreferredValues(tapPopupLabels[0].text).x;
            float popupWidth = Mathf.Max(1, Mathf.Min(Mathf.Max(220, textWidth + 24), right - left));
            popup.sizeDelta = new Vector2(popupWidth, popupHeight);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(world, screenPosition, tapCamera, out var tapLocal);
            float minCenter = left + popupWidth * .5f;
            float maxCenter = right - popupWidth * .5f;
            float x = minCenter <= maxCenter ? Mathf.Clamp(tapLocal.x, minCenter, maxCenter) : (left + right) * .5f;
            float freeSpace = Mathf.Max(0, laneHeight - popupHeight - popupRises[0]);
            float offset = Mathf.Min(freeSpace, Mathf.Max(freeSpace * .5f, bottomSpace));
            // The saved popup pivots at its top edge, not its centre.
            float y = laneBottom + popupHeight * popup.pivot.y + offset;
            popupOrigins[0] = new Vector2(x, y);
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
            UiFeedback.Pulse(counterLabel.transform, controller, .08f, .8f);
        }
    }
}
