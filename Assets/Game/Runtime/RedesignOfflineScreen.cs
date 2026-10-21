using System;
using System.Globalization;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignOfflineScreen : MonoBehaviour
    {
        public TMP_Text amount, calculation, nextStep, skills, status, nextLabel;
        public Button next, close, inspectSkills;
        public ScrollRect scroll;
        RedesignUiContext context;
        string receiptId, nextRoute, nextArgument;
        float elapsed;
        void Awake()
        {
            if (!amount || !calculation || !nextStep || !skills || !status || !nextLabel || !next || !close || !inspectSkills || !scroll) throw new InvalidOperationException("Incomplete saved return screen");
            next.onClick.AddListener(Next); close.onClick.AddListener(Close); inspectSkills.onClick.AddListener(Skills);
        }
        void OnDestroy() { if (next) next.onClick.RemoveListener(Next); if (close) close.onClick.RemoveListener(Close); if (inspectSkills) inspectSkills.onClick.RemoveListener(Skills); }
        public void Bind(RedesignUiContext value) { context = value; }
        public void Open()
        {
            receiptId = context.Engine.State.offline.id; gameObject.SetActive(true); scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; Refresh();
        }
        public static string Duration(double seconds) => seconds >= 3600 ? (seconds / 3600).ToString("0.##", CultureInfo.InvariantCulture) + " hours" : seconds >= 60 ? (seconds / 60).ToString("0.##", CultureInfo.InvariantCulture) + " minutes" : seconds.ToString("0.##", CultureInfo.InvariantCulture) + " seconds";
        void Next() => Acknowledge(nextRoute, nextArgument);
        void Close() => Acknowledge("home", "");
        void Skills() => Acknowledge("skilldetails", "0");
        void Acknowledge(string route, string argument)
        {
            if (context == null) return;
            var receipt = context.Engine.State.offline;
            if (receipt.id != receiptId) { context.Notice("A newer return summary is ready."); Open(); return; }
            if (!receipt.pending || context.Execute(e => e.State.offline.id == receiptId && e.DismissOffline())) context.Navigate(route, argument);
            else { context.Notice("The cash is already credited. Recover saving before closing this summary."); Refresh(); }
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        public void Refresh()
        {
            if (context == null) return; var e = context.Engine; var s = e.State; var receipt = s.offline;
            amount.text = "+" + RedesignUiContext.Cash(receipt.amount) + " cash\nAway for " + Duration(receipt.absence);
            // Recover the rate used by this receipt, never today's possibly changed production.
            if (receipt.creditedSeconds > 0 && receipt.amount > Amount.Zero) amount.text += "\nNormal sales used: " + RedesignUiContext.Cash(receipt.amount / receipt.creditedSeconds / receipt.efficiency) + "/s";
            calculation.text = "Credited time: " + Duration(receipt.creditedSeconds) + "\nEfficiency: " + (receipt.efficiency * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%\n" +
                (receipt.absence > receipt.creditedSeconds ? Duration(receipt.absence - receipt.creditedSeconds) + " exceeded your storage." : "All of this absence fit within your storage.") + "\nSkills and tapping are excluded.";
            bool pinned = e.Data.TryUpgrade(s.pinnedUpgradeId, out int index) && !s.upgrades.Contains(s.pinnedUpgradeId);
            bool rebuild = s.products.All(p => p == 0);
            nextRoute = pinned ? "upgradedetails" : rebuild ? "productdetails" : "upgrades"; nextArgument = pinned ? s.pinnedUpgradeId : rebuild ? "0" : "";
            nextLabel.text = pinned ? "View pinned upgrade" : rebuild ? "Buy your first Phuljhari" : "Browse cash upgrades";
            nextStep.text = pinned ? e.Data.Upgrades[index].name + (e.Eligible(s.pinnedUpgradeId) && e.UpgradeCost(s.pinnedUpgradeId) <= s.cash ? " is now affordable." : " is still your pinned goal. Inspect its next requirement or cash price.") :
                rebuild ? "Your opening grant is ready for a manual Phuljhari purchase. No product is bought automatically." : "Use your credited cash to review products or upgrades.";
            skills.text = string.Join("\n", Enumerable.Range(0, 3).Select(i => RedesignSkillScreen.Names[i] + ": " + (!e.SkillOwned(i) ? "buy again this run" : s.readyAt[i] <= s.now ? "ready" : "ready in " + Duration(s.readyAt[i] - s.now))));
            status.text = context.CanMutate() ? "Closing keeps all credited cash." : "Cash has already been credited. Saving or ownership needs attention.";
            next.interactable = close.interactable = inspectSkills.interactable = context.CanMutate();
        }
    }
}
