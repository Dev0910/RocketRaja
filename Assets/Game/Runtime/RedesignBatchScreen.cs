using System;
using System.Text;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    [Serializable] public sealed class RedesignBatchRow
    {
        public RectTransform root;
        public TMP_Text itemName, price;
    }
    public sealed class RedesignBatchScreen : MonoBehaviour
    {
        public TMP_Text summary, status, confirmLabel, exactLabel;
        public Button confirm, cancel, refreshReview, exact;
        public ScrollRect scroll;
        public RectTransform content;
        public RedesignBatchRow[] rows;
        public float rowHeight;
        RedesignUiContext context;
        CashBatchQuote quote;
        UpgradeScope scope;
        bool fullPrecision, drawing;
        float elapsed;
        public CashBatchQuote ReviewedQuote => quote;
        public void Bind(RedesignUiContext value) { context = value; Refresh(); }
        public void Open(CashBatchQuote value)
        {
            quote = value ?? throw new ArgumentNullException(nameof(value)); scope = value.Selection; fullPrecision = false;
            content.anchoredPosition = Vector2.zero; scroll.StopMovement(); gameObject.SetActive(true); Refresh();
        }
        void Awake()
        {
            if (!summary || !status || !confirmLabel || !exactLabel || !confirm || !cancel || !refreshReview || !exact || !scroll || !content || rows == null || rows.Length < 8 || rowHeight <= 0)
                throw new InvalidOperationException("Incomplete serialized Buy All screen");
            foreach (var row in rows) if (row == null || !row.root || !row.itemName || !row.price) throw new InvalidOperationException("Incomplete Buy All row");
            confirm.onClick.AddListener(Confirm); cancel.onClick.AddListener(Cancel); refreshReview.onClick.AddListener(ReviewAgain);
            exact.onClick.AddListener(Exact); scroll.onValueChanged.AddListener(Scrolled);
        }
        void OnDestroy()
        {
            if (confirm) confirm.onClick.RemoveListener(Confirm); if (cancel) cancel.onClick.RemoveListener(Cancel);
            if (refreshReview) refreshReview.onClick.RemoveListener(ReviewAgain); if (exact) exact.onClick.RemoveListener(Exact);
            if (scroll) scroll.onValueChanged.RemoveListener(Scrolled);
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        void Scrolled(Vector2 unused) { if (!drawing) Refresh(); }
        void Cancel() { quote = null; context?.Navigate("back", ""); }
        void Exact() { fullPrecision = !fullPrecision; Refresh(); }
        void ReviewAgain() { if (context != null && scope != null && context.CanMutate()) Open(context.Engine.QuoteBuyAll(scope)); }
        void Confirm()
        {
            if (context == null || quote == null) return;
            var reviewed = quote;
            var feedback = new RedesignPurchaseFeedback(context.Engine);
            if (context.Execute(e => e.CommitBuyAll(reviewed)))
            {
                quote = null; confirm.interactable = false;
                context.Notice(feedback.Complete(context.Engine));
                context.Navigate("upgrades", "");
            }
            else { context.Notice("This review could not be completed. Check saving or review the current choices again."); Refresh(); }
        }
        public void Refresh()
        {
            if (context == null || quote == null || drawing) return;
            drawing = true;
            try
            {
                var e = context.Engine; bool current = e.CanCommitBuyAll(quote), allowed = context.CanMutate();
                var description = new StringBuilder("Scope: ").Append(scope.category);
                if (scope.product >= 0) description.Append(" / ").Append(e.Data.Products[scope.product].name);
                if (scope.skill >= 0) description.Append(" / ").Append(new[] { "Festival Rush", "Helping Hands", "Bulk Order" }[scope.skill]);
                if (!string.IsNullOrEmpty(scope.search)) description.Append(" / Search: ").Append(scope.search);
                description.Append("\nBudget at review: ").Append(RedesignUiContext.Cash(quote.Budget, fullPrecision));
                description.Append("\n").Append(quote.Ids.Count).Append(" upgrades · ").Append(quote.FirstPurchases).Append(" first purchases");
                description.Append("\nTotal: ").Append(RedesignUiContext.Cash(quote.Cost, fullPrecision)); summary.text = description.ToString();
                status.text = !allowed ? "Saving or recovery needs attention. No purchase has been applied." : quote.Ids.Count == 0 ? "No affordable eligible upgrades in this scope." : !current ? "This review changed. Review again before buying." : "Only this list will be bought. Normal sales continue; new earnings do not add entries.";
                confirm.interactable = current && allowed; confirmLabel.text = "Buy " + quote.Ids.Count + " · " + RedesignUiContext.Cash(quote.Cost);
                refreshReview.interactable = allowed; exactLabel.text = fullPrecision ? "Shorter numbers" : "Full precision";
                float height = quote.Ids.Count * rowHeight;
                if (!Mathf.Approximately(content.sizeDelta.y, height)) content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                float offset = Mathf.Clamp(content.anchoredPosition.y, 0, Mathf.Max(0, height - scroll.viewport.rect.height));
                int first = Mathf.Clamp(Mathf.FloorToInt(offset / rowHeight), 0, Math.Max(0, quote.Ids.Count - 1));
                for (int slot = 0; slot < rows.Length; slot++)
                {
                    int index = first + slot; var row = rows[slot]; row.root.gameObject.SetActive(index < quote.Ids.Count);
                    if (index >= quote.Ids.Count) continue;
                    row.root.anchoredPosition = new Vector2(0, -index * rowHeight);
                    e.Data.TryUpgrade(quote.Ids[index], out int dataIndex);
                    row.itemName.text = (index + 1) + ". " + e.Data.Upgrades[dataIndex].name;
                    row.price.text = RedesignUiContext.Cash(quote.Prices[index], fullPrecision);
                }
            }
            finally { drawing = false; }
        }
    }
}
