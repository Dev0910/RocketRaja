using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignTalentScreen : MonoBehaviour
    {
        public TMP_Text balances, status, freeLabel, oddsText;
        public GameObject lockedPanel, oddsPanel;
        public Button freeDraw, paidDraw, shop, odds, prestige, resume;
        public Button[] cards;
        public TMP_Text[] cardLabels;
        public ScrollRect scroll;
        RedesignUiContext context;
        UnityAction[] cardActions;
        TalentDrawQuote pending;
        string revision;
        float elapsed;
        void Awake()
        {
            if (!balances || !status || !freeLabel || !oddsText || !lockedPanel || !oddsPanel || !freeDraw || !paidDraw || !shop || !odds || !prestige || !resume || !scroll || cards.Length != 28 || cardLabels.Length != 28)
                throw new InvalidOperationException("Incomplete saved Talent screen");
            cardActions = new UnityAction[28];
            for (int i = 0; i < 28; i++) { int index = i; cardActions[i] = () => context?.Navigate("carddetails", context.Engine.Data.Cards[index].id); cards[i].onClick.AddListener(cardActions[i]); }
            freeDraw.onClick.AddListener(Free); paidDraw.onClick.AddListener(Paid); shop.onClick.AddListener(Shop);
            odds.onClick.AddListener(Odds); prestige.onClick.AddListener(Prestige); resume.onClick.AddListener(Resume);
        }
        void OnDestroy()
        {
            if (cardActions != null) for (int i = 0; i < 28; i++) cards[i].onClick.RemoveListener(cardActions[i]);
            if (freeDraw) freeDraw.onClick.RemoveListener(Free); if (paidDraw) paidDraw.onClick.RemoveListener(Paid); if (shop) shop.onClick.RemoveListener(Shop);
            if (odds) odds.onClick.RemoveListener(Odds); if (prestige) prestige.onClick.RemoveListener(Prestige); if (resume) resume.onClick.RemoveListener(Resume);
        }
        public void Bind(RedesignUiContext value) { if (context?.Engine != value.Engine) pending = null; context = value; revision = null; Refresh(); }
        public void Open() { gameObject.SetActive(true); Refresh(); }
        void Free() => Draw(true);
        void Paid() => Draw(false);
        void Shop() => context?.Navigate("kajushop", "");
        void Prestige() => context?.Navigate("prestige", "");
        void Resume() => context?.Navigate("drawreveal", "");
        void Odds() { oddsPanel.SetActive(!oddsPanel.activeSelf); }
        void Draw(bool credit)
        {
            if (context == null || !context.CanMutate()) return;
            if (pending == null || pending.UseCredit != credit || !context.Engine.CanCommitDraw(pending)) pending = context.Engine.QuoteDraw(credit);
            if (pending == null) { context.Notice("This draw is unavailable. Check your balance or pending result."); Refresh(); return; }
            if (context.Execute(e => e.Draw(pending))) { pending = null; context.Navigate("drawreveal", ""); }
            else context.Notice("The draw was not completed. Recover saving before retrying.");
            Refresh();
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        public void Refresh()
        {
            if (context == null) return; var e = context.Engine; var s = e.State;
            balances.text = RedesignCardCopy.Count(s.freeCredits) + " free draws · " + RedesignCardCopy.Count(s.kajuKatli) + " Kaju Katli";
            lockedPanel.SetActive(s.completedPrestiges.IsZero);
            status.text = !context.CanMutate() ? "Saving or recovery needs attention." : s.requiresDrawReconciliation ? "Restore the pending draw before drawing again." : s.reveal.pending ? "A saved card result is ready to view." : "Cards and unused draw balances remain after prestige.";
            resume.gameObject.SetActive(s.reveal.pending);
            freeDraw.interactable = context.CanMutate() && e.CanDraw(true); paidDraw.interactable = context.CanMutate() && e.CanDraw(false);
            freeLabel.text = "Use 1 free draw · " + RedesignCardCopy.Count(s.freeCredits) + " available";
            oddsText.text = "Common: 75% / 16 = 4.6875% each\nRare: 22% / 10 = 2.2% each\nLegendary: 3% / 2 = 1.5% each\nSame pool for free and KK draws. No pity or guaranteed rarity. Ownership does not change these odds.";
            if (revision == s.commandRevision.digits) return;
            revision = s.commandRevision.digits;
            for (int i = 0; i < 28; i++) cardLabels[i].text = e.Data.Cards[i].name + "\n" + e.Data.Cards[i].tier + " · " + (s.cardLevels[i].IsZero ? "Unowned" : "Lv " + RedesignCardCopy.Count(s.cardLevels[i]));
        }
    }
}
