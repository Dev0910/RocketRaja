using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Native catalogue view. Owns filters/scroll and reuses a saved pool.</summary>
    public sealed class RedesignUpgradesScreen : MonoBehaviour
    {
        public ScrollRect scroll;
        public RectTransform content;
        public RedesignUpgradeRow[] rows;
        public Button[] categories, views;
        public Button targetFilter, reviewButton;
        public TMP_Text targetLabel, reviewLabel, emptyLabel, countLabel;
        public TMP_InputField search;
        public float rowHeight = 560;
        RedesignEngine engine;
        RedesignUpgradeBrowser browser;
        Func<bool> canMutate;
        Func<string, bool> purchase;
        Action<string> inspect, notice;
        Action<CashBatchQuote> review;
        UnityAction[] categoryActions, viewActions;
        float refreshTime;
        bool drawing;
        public RedesignUpgradeBrowser Browser => browser;
        public int FirstVisibleIndex { get; private set; }
        public void Bind(RedesignEngine value, Func<bool> mutationAllowed, Func<string, bool> onPurchase,
            Action<string> onInspect, Action<CashBatchQuote> onReview, Action<string> onNotice)
        {
            if (engine != value) { engine = value ?? throw new ArgumentNullException(nameof(value)); browser = new RedesignUpgradeBrowser(engine); }
            canMutate = mutationAllowed; purchase = onPurchase; inspect = onInspect; review = onReview; notice = onNotice;
            Refresh();
        }
        void Awake()
        {
            if (!scroll || !content || rows == null || rows.Length < 8 || categories == null || categories.Length != 5 ||
                views == null || views.Length != 4 || !targetFilter || !reviewButton || !targetLabel || !reviewLabel || !emptyLabel || !countLabel || !search)
                throw new InvalidOperationException("Incomplete serialized Redesign Upgrades screen");
            categoryActions = new UnityAction[categories.Length]; viewActions = new UnityAction[views.Length];
            for (int i = 0; i < categories.Length; i++) { int index = i; categoryActions[i] = () => SetCategory((UpgradeCategory)index); categories[i].onClick.AddListener(categoryActions[i]); }
            for (int i = 0; i < views.Length; i++) { int index = i; viewActions[i] = () => SetView((UpgradeView)index); views[i].onClick.AddListener(viewActions[i]); }
            targetFilter.onClick.AddListener(CycleTarget); reviewButton.onClick.AddListener(Review);
            search.onValueChanged.AddListener(Search); scroll.onValueChanged.AddListener(Scrolled);
        }
        void OnDestroy()
        {
            if (categoryActions != null) for (int i = 0; i < categories.Length; i++) categories[i].onClick.RemoveListener(categoryActions[i]);
            if (viewActions != null) for (int i = 0; i < views.Length; i++) views[i].onClick.RemoveListener(viewActions[i]);
            if (targetFilter) targetFilter.onClick.RemoveListener(CycleTarget);
            if (reviewButton) reviewButton.onClick.RemoveListener(Review);
            if (search) search.onValueChanged.RemoveListener(Search);
            if (scroll) scroll.onValueChanged.RemoveListener(Scrolled);
        }
        void OnEnable() { if (browser != null) Refresh(); }
        void Update() { refreshTime += Time.unscaledDeltaTime; if (refreshTime >= .2f) { refreshTime = 0; Refresh(); } }
        public void SetCategory(UpgradeCategory category)
        {
            if (browser == null) return;
            browser.Scope.category = category; browser.Scope.product = browser.Scope.skill = -1; ResetScroll();
        }
        public void SetView(UpgradeView view) { if (browser == null) return; browser.View = view; ResetScroll(); }
        public void FilterProduct(int product) { SetCategory(UpgradeCategory.Products); browser.Scope.product = product; ResetScroll(); }
        public void FilterSkill(int skill) { SetCategory(UpgradeCategory.Skills); browser.Scope.skill = skill; ResetScroll(); }
        void Search(string text) { if (browser == null) return; browser.Scope.search = text.Trim(); ResetScroll(); }
        void CycleTarget()
        {
            if (browser == null) return;
            if (browser.Scope.category == UpgradeCategory.Skills) browser.Scope.skill = (browser.Scope.skill + 2) % 4 - 1;
            else browser.Scope.product = (browser.Scope.product + 2) % 9 - 1;
            ResetScroll();
        }
        void ResetScroll() { scroll.StopMovement(); content.anchoredPosition = Vector2.zero; Refresh(); }
        void Scrolled(Vector2 unused) { if (!drawing) Refresh(); }
        void Review()
        {
            if (browser == null || canMutate?.Invoke() != true) return;
            var quote = browser.ReviewBatch();
            if (quote.Ids.Count == 0) notice?.Invoke("No affordable eligible upgrades in this scope.");
            else review?.Invoke(quote);
        }
        void Buy(string id)
        {
            if (canMutate?.Invoke() != true) return;
            var feedback = new RedesignPurchaseFeedback(engine);
            bool bought = purchase?.Invoke(id) == true;
            notice?.Invoke(bought ? feedback.Complete(engine) : "Purchase unavailable. Check cash, requirements or saving.");
            Refresh();
        }
        public void Refresh()
        {
            if (browser == null || drawing) return;
            drawing = true;
            try
            {
                browser.Refresh();
                float height = browser.Count * rowHeight;
                if (!Mathf.Approximately(content.sizeDelta.y, height)) content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                float offset = Mathf.Clamp(content.anchoredPosition.y, 0, Mathf.Max(0, height - scroll.viewport.rect.height));
                FirstVisibleIndex = Mathf.Clamp(Mathf.FloorToInt(offset / rowHeight), 0, Math.Max(0, browser.Count - 1));
                bool allowed = canMutate?.Invoke() == true;
                for (int slot = 0; slot < rows.Length; slot++)
                {
                    int index = FirstVisibleIndex + slot;
                    var row = rows[slot]; row.gameObject.SetActive(index < browser.Count);
                    if (index >= browser.Count) continue;
                    ((RectTransform)row.transform).anchoredPosition = new Vector2(0, -index * rowHeight);
                    row.Bind(browser[index], engine, browser, allowed, inspect, Buy);
                }
                emptyLabel.gameObject.SetActive(browser.Count == 0);
                countLabel.text = browser.Count + " matching upgrades · " + engine.State.UniqueCount + " / 1,000 first purchases";
                targetFilter.interactable = browser.Scope.category == UpgradeCategory.All || browser.Scope.category == UpgradeCategory.Products || browser.Scope.category == UpgradeCategory.Skills;
                targetLabel.text = browser.Scope.category == UpgradeCategory.Skills ? "Skill: " + (browser.Scope.skill < 0 ? "All" : new[] { "Rush", "Hands", "Order" }[browser.Scope.skill]) : "Product: " + (browser.Scope.product < 0 ? "All" : engine.Data.Products[browser.Scope.product].name);
                reviewLabel.text = "Review Buy All · " + browser.Scope.category;
                reviewButton.interactable = allowed && browser.View != UpgradeView.Owned;
                for (int i = 0; i < categories.Length; i++) Tint(categories[i], i == (int)browser.Scope.category);
                for (int i = 0; i < views.Length; i++) Tint(views[i], i == (int)browser.View);
            }
            finally { drawing = false; }
        }
        static void Tint(Button button, bool selected)
        {
            button.targetGraphic.color = selected ? new Color32(117, 31, 53, 255) : new Color32(255, 243, 213, 255);
            var text = button.GetComponentInChildren<TMP_Text>();
            text.color = selected ? new Color32(255, 240, 204, 255) : new Color32(56, 33, 49, 255);
        }
    }
}
