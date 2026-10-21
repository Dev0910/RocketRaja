using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignCardDetailsScreen : MonoBehaviour
    {
        public TMP_Text itemName, status, effect, applies, odds, exactLabel;
        public Button exact, back;
        public Image artwork;
        public Sprite[] cardSprites;
        public ScrollRect scroll;
        RedesignUiContext context; int selected; bool full;
        public string SelectedId => context?.Engine.Data.Cards[selected].id;
        void Awake()
        {
            if (!itemName || !status || !effect || !applies || !odds || !exactLabel || !exact || !back || !artwork || cardSprites.Length != 28 || !scroll) throw new InvalidOperationException("Incomplete saved card details");
            exact.onClick.AddListener(Exact); back.onClick.AddListener(Back);
        }
        void OnDestroy() { if (exact) exact.onClick.RemoveListener(Exact); if (back) back.onClick.RemoveListener(Back); }
        public void Bind(RedesignUiContext value) { context = value; }
        public void Open(string id)
        {
            if (!context.Engine.Data.TryCard(id, out selected)) throw new ArgumentException("Unknown card");
            full = false; gameObject.SetActive(true); scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; Refresh();
        }
        void Exact() { full = !full; Refresh(); }
        void Back() => context?.Navigate("talent", "");
        public void Refresh()
        {
            if (context == null) return; var e = context.Engine; var card = e.Data.Cards[selected]; var level = e.State.cardLevels[selected];
            itemName.text = card.name; artwork.sprite = cardSprites[selected];
            status.text = card.tier + " · Level " + RedesignCardCopy.Count(level, full) + "\nAll copies remain after prestige.";
            effect.text = "Now: " + RedesignCardCopy.Effect(card, level, full) + "\nOne more copy: " + RedesignCardCopy.Effect(card, level + (Natural)1, full) + "\nLevel " + RedesignCardCopy.Count(level, full) + " → " + RedesignCardCopy.Count(level + (Natural)1, full) +
                (full ? "\nExact benefit: " + ProgressionNumbers.DuplicateBenefit(card.effect, level) : "");
            applies.text = RedesignCardCopy.Applies(e, card) + "\nEvery duplicate adds one level. No maximum card level.";
            odds.text = RedesignCardCopy.Odds(e.Data, card); exactLabel.text = full ? "Show shorter numbers" : "Show exact effect";
        }
    }
}
