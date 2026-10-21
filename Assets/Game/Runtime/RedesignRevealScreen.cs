using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignRevealScreen : MonoBehaviour
    {
        public TMP_Text itemName, outcome, effect, status;
        public Image artwork;
        public Sprite[] cardSprites;
        public Button skip, done, details;
        public CanvasGroup cardGroup;
        public RectTransform cardFace;
        RedesignUiContext context; string operation, cardId; float started;
        public string OperationId => operation;
        public bool Settled { get; private set; }
        void Awake()
        {
            if (!itemName || !outcome || !effect || !status || !artwork || cardSprites.Length != 28 || !skip || !done || !details || !cardGroup || !cardFace) throw new InvalidOperationException("Incomplete saved draw reveal");
            skip.onClick.AddListener(Settle); done.onClick.AddListener(Done); details.onClick.AddListener(Details);
        }
        void OnDestroy() { if (skip) skip.onClick.RemoveListener(Settle); if (done) done.onClick.RemoveListener(Done); if (details) details.onClick.RemoveListener(Details); }
        public void Bind(RedesignUiContext value) { context = value; }
        public void Open()
        {
            var e = context.Engine; var reveal = e.State.reveal;
            if (!reveal.pending || !e.Data.TryCard(reveal.cardId, out int index)) { context.Navigate("talent", ""); return; }
            operation = reveal.operationId; cardId = reveal.cardId; var card = e.Data.Cards[index];
            itemName.text = card.name; artwork.sprite = cardSprites[index];
            outcome.text = (reveal.before.IsZero ? "New card" : "Duplicate · +1 level") + "\n" + card.tier + " · Level " + RedesignCardCopy.Count(reveal.before) + " → " + RedesignCardCopy.Count(reveal.after);
            effect.text = RedesignCardCopy.Effect(card, reveal.before) + " →\n" + RedesignCardCopy.Effect(card, reveal.after);
            status.text = "Saved to your collection. " + (reveal.payment == "credit" ? "1 free draw used." : "100 KK used.") +
                "\n" + RedesignCardCopy.Count(e.State.freeCredits) + " free draws · " + RedesignCardCopy.Count(e.State.kajuKatli) + " KK remain.";
            gameObject.SetActive(true); started = Time.unscaledTime; Settled = false; cardGroup.alpha = 0; cardFace.localScale = Vector3.one * .93f;
            if (e.State.reducedMotion) Settle(); RefreshControls();
        }
        void Update()
        {
            if (!Settled)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - started) / .4f); float eased = 1 - Mathf.Pow(1 - t, 3);
                cardGroup.alpha = eased; cardFace.localScale = Vector3.one * Mathf.Lerp(.93f, 1, eased); if (t >= 1 || context.Engine.State.reducedMotion) Settle();
            }
            RefreshControls();
        }
        public void Settle() { Settled = true; cardGroup.alpha = 1; cardFace.localScale = Vector3.one; RefreshControls(); }
        void OnDisable() { if (cardGroup && cardFace) { cardGroup.alpha = 1; cardFace.localScale = Vector3.one; } }
        void RefreshControls()
        {
            done.interactable = Settled && context != null && context.CanMutate(); details.interactable = Settled;
            skip.interactable = !Settled;
        }
        void Details() { Settle(); context?.Navigate("carddetails", cardId); }
        void Done()
        {
            if (context != null && context.Execute(e => e.DismissReveal(operation))) { context.Notice(itemName.text + " saved\n" + outcome.text); context.Navigate("talent", ""); }
            else context?.Notice("The card is saved. Recover saving before closing this result.");
        }
    }
}
