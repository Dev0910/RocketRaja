using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignPrestigeScreen : MonoBehaviour
    {
        public TMP_Text fame, credits, thresholds, rebuild, status, confirmLabel, exactLabel;
        public Button confirm, stay, review, exact, pendingCard;
        public ScrollRect scroll;
        RedesignUiContext context; FameQuote quote; bool full; float elapsed;
        public FameQuote ReviewedQuote => quote;
        void Awake()
        {
            if (!fame || !credits || !thresholds || !rebuild || !status || !confirmLabel || !exactLabel || !confirm || !stay || !review || !exact || !pendingCard || !scroll) throw new InvalidOperationException("Incomplete saved prestige screen");
            confirm.onClick.AddListener(Confirm); stay.onClick.AddListener(Stay); review.onClick.AddListener(Review); exact.onClick.AddListener(Exact); pendingCard.onClick.AddListener(PendingCard);
        }
        void OnDestroy()
        {
            if (confirm) confirm.onClick.RemoveListener(Confirm); if (stay) stay.onClick.RemoveListener(Stay); if (review) review.onClick.RemoveListener(Review); if (exact) exact.onClick.RemoveListener(Exact); if (pendingCard) pendingCard.onClick.RemoveListener(PendingCard);
        }
        public void Bind(RedesignUiContext value) { context = value; quote = null; }
        public void Open() { full = false; gameObject.SetActive(true); scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; Review(); }
        void Review() { if (context == null) return; quote = context.Engine.PreviewPrestige(); Refresh(); }
        void Stay() => context?.Navigate("back", "");
        void PendingCard() => context?.Navigate("drawreveal", "");
        void Exact() { full = !full; Refresh(); }
        void Confirm()
        {
            if (context == null || quote == null || context.Engine.State.reveal.pending) return;
            bool first = context.Engine.State.completedPrestiges.IsZero;
            bool gift = context.Engine.State.completedPrestiges.Value == 1;
            var reviewed = quote;
            if (context.Execute(e => e.Prestige(quote)))
            {
                quote = null; context.Notice(first ? "First Prestige complete · +" + RedesignCardCopy.Count(reviewed.Fame) + " Fame\n3 free draws ready. Shop decorations stay; products rebuild." :
                    "Prestige complete · permanent gains saved.\nManual first purchase is ready." + (gift ? "\nOne-time gift: +100 Kaju Katli." : ""));
                context.Navigate("talent", "");
            }
            else { context.Notice("This prestige review could not be completed. Check saving or review again."); Refresh(); }
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        public void Refresh()
        {
            if (context == null || quote == null) return; var e = context.Engine; var s = e.State;
            if (!quote.Eligible) { var current = e.PreviewPrestige(); if (current.Eligible) quote = current; }
            string Cash(Amount n) => RedesignUiContext.Cash(n, full);
            fame.text = "Fame " + RedesignCardCopy.Count(s.fame, full) + " → " + RedesignCardCopy.Count(s.fame + quote.Fame, full) + "\nGuaranteed gain: " + RedesignCardCopy.Count(quote.Fame, full) +
                "\nFame income multiplier ×" + RedesignUiContext.Number(ProgressionNumbers.FameMultiplier(s.fame), full) + " → ×" + RedesignUiContext.Number(quote.ResultingIncomeMultiplier, full);
            credits.text = RedesignCardCopy.Count(quote.Credits, full) + " free draw credits\n" + (s.completedPrestiges.IsZero ? "First prestige: exactly 3 credits once eligible." : "Run reward: " + RedesignCardCopy.Count(quote.RunCredits, full) + " · Catalogue bonus: " + quote.CatalogueCredits) +
                "\nCard results are random. Unused credits persist." + (s.completedPrestiges.Value == 1 ? "\nSecond prestige: one-time gift of 100 KK." : "");
            thresholds.text = "Earned this run: " + Cash(s.eligibleEarnings) + "\nCurrent target: " + Cash(quote.CurrentTarget) + "\nNext run target: " + Cash(quote.NextTarget) +
                "\nNext Fame gain at " + Cash(quote.NextFameEarnings) + (quote.NextDrawEarnings.mantissa == 0 ? "\nFirst-prestige credits stay at 3." : "\nNext run draw credit at " + Cash(quote.NextDrawEarnings)) +
                (quote.NextUniqueMilestone == 0 ? "\nAll 1,000 first purchases counted." : (s.completedPrestiges.IsZero ? "\nOn later prestiges, next catalogue bonus: " : "\nNext catalogue bonus: ") + quote.NextUniqueMilestone + " first purchases, with earnings ≥ " + Cash(s.target * 4));
            rebuild.text = "Start with " + Cash(new Amount(30) * ProgressionNumbers.CardFactor(CardEffect.ProductCost, s.cardLevels[8])) + " for a manual Phuljhari purchase. This grant is not run earnings.";
            bool valid = e.CanCommitPrestige(quote);
            confirm.interactable = context.CanMutate() && valid && !s.reveal.pending;
            confirmLabel.text = quote.Eligible ? "Confirm prestige" : "Reach the target first";
            pendingCard.gameObject.SetActive(s.reveal.pending);
            status.text = !context.CanMutate() ? "Saving or recovery needs attention." : s.reveal.pending ? "View your saved card result before resetting." : !quote.Eligible ? "Keep earning. There is no requirement to own all eight products." : !valid ? "The run changed. Review again before confirming." : "Reviewed rewards are a minimum. Further earnings can improve them before confirmation.";
            exactLabel.text = full ? "Show shorter numbers" : "Show full precision";
        }
    }
}
