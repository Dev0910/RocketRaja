using System;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;

namespace RocketRaja.Classic
{
    [Serializable] public sealed class ClassicTipRow { public string route; public GameObject root; public TMP_Text title, brief; }

    /// <summary>Saved contextual hints using the session's durable learned-topic command.</summary>
    public sealed class ClassicGuidance : MonoBehaviour
    {
        public ClassicUI ui;
        public ClassicTipRow[] rows;
        public RedesignGuidanceText[] topics;
        public TMP_Text heading, body;
        public string CurrentTopic { get; private set; }
        public void Refresh()
        {
            if (ui.CurrentDialog == "Guidance") { FitReader(); return; }
            string route = ui.CurrentDialog == "" ? ui.CurrentTab : ui.CurrentDialog;
            string next = ui.driver.Initialized && ui.driver.Session.CanMutate ? Next(route) : null;
            CurrentTopic = next;
            bool banner = next != null && rows.Any(r => r.route == route);
            foreach (var row in rows)
            {
                bool show = next != null && row.route == route;
                row.root.SetActive(show);
                if (show) { var copy = topics.Single(t => t.id == next); row.title.text = copy.title; row.brief.text = copy.brief; }
            }
            foreach (var panel in ui.panels)
                if (panel.scroll) { var r = (RectTransform)panel.scroll.transform; r.offsetMax = new Vector2(r.offsetMax.x, -(banner && panel.route == route ? 105 : 49)); }
            var goal = ui.panels[0].root.transform.Find("Next display");
            if (goal) goal.gameObject.SetActive(!(route == "Home" && next != null));
        }
        string Next(string route)
        {
            if (ui.Engine.TutorialActive) return null;
            var e = ui.Engine; var s = e.State;
            string Unread(string id) => s.learnedTopics.Contains(id) ? null : id;
            switch (route)
            {
                case "Home":
                    if (s.products[0] == 0) return Unread(s.completedPrestiges.IsZero ? "buy-first" : "rebuild");
                    if (s.manualTaps.IsZero) return Unread("tap-counter");
                    if (s.eligibleEarnings >= s.target) return Unread("prestige");
                    return null;
                case "Products": return Unread("product-levels");
                case "ProductDetail": return Unread("comebacks") ?? Unread("product-levels");
                case "Player": return Unread("buy-all") ?? Unread("catalogue-filters");
                case "Upgrades": return Unread("catalogue-filters");
                case "Skills": case "SkillDetail": return null;
                case "Offline": return Unread("return-play");
                case "Legacy": return RedesignGuidance.Next(e, "talent", "");
                case "DrawReveal": return RedesignGuidance.Next(e, "drawreveal", "");
                case "Prestige": return Unread("prestige");
                case "Shop": return RedesignGuidance.Next(e, "kajushop", "");
                case "BuyAll": return Unread("buy-all");
                case "TapTraining": return Unread("product-levels");
                default: return null;
            }
        }
        public void Open()
        {
            if (CurrentTopic == null || !ui.driver.Session.CanMutate) return;
            var text = topics.Single(t => t.id == CurrentTopic);
            heading.text = text.title; body.text = text.body; ui.Open("Guidance");
            FitReader();
        }
        public void OpenTopic(string id)
        {
            if (!ui.driver.Session.CanMutate) return;
            var text = topics.Single(t => t.id == id);
            CurrentTopic = id;
            heading.text = text.title; body.text = text.body;
            ui.Open("Guidance"); FitReader();
        }
        void FitReader()
        {
            body.alignment = TextAlignmentOptions.TopLeft;
            var row = body.transform.parent.GetComponent<UnityEngine.UI.LayoutElement>();
            if (row && body.rectTransform.rect.width > 20)
                row.minHeight = row.preferredHeight = body.GetPreferredValues(body.text, body.rectTransform.rect.width, 0).y + 24;
        }
        public void Acknowledge()
        {
            if (ui.CurrentDialog != "Guidance" || CurrentTopic == null) return;
            if (ui.driver.Command(e => e.LearnTopic(CurrentTopic))) ui.Close();
            else ui.Notice("The tip could not be saved. Retry after saving recovers.");
        }
    }
}
