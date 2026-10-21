using System;
using System.Linq;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Classic
{
    [Serializable]
    public sealed class ClassicRedDotBinding
    {
        public ClassicControl control;
        public GameObject dot;
    }

    /// <summary>Saved indicators reflect rewards and available gameplay actions; viewing never consumes them.</summary>
    public sealed class ClassicRedDots : MonoBehaviour
    {
        public ClassicUI ui;
        public ClassicRedDotBinding[] bindings = Array.Empty<ClassicRedDotBinding>();
        float nextRefresh;
        readonly bool[] skills = new bool[3];

        void LateUpdate()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .1f;
            Refresh();
        }

        public void Refresh()
        {
            var engine = ui ? ui.Engine : null;
            bool can = engine != null && ui.driver.Session.CanMutate;
            bool achievements = can && !engine.State.HasPremium && RedesignAchievements.ReadyCount(engine.State) > 0;
            bool draw = can && engine.CanDraw(!engine.State.freeCredits.IsZero);
            bool upgrades = can && engine.Data.Upgrades.Any(u => engine.Eligible(u.id) && engine.UpgradeCost(u.id) <= engine.State.cash);
            bool prestige = can && engine.CanCommitPrestige(engine.PreviewPrestige());
            for (int i = 0; i < skills.Length; i++)
                skills[i] = can && engine.SkillOwned(i) && engine.State.activeUntil[i] <= engine.State.now && engine.State.readyAt[i] <= engine.State.now;
            foreach (var binding in bindings)
            {
                if (!binding.control || !binding.dot) continue;
                var c = binding.control;
                bool show = false;
                if (can)
                {
                    if (c.action == "ClaimAchievement") show = engine.CanClaimAchievement(c.parameter);
                    else if (c.action == "Draw") show = draw && (c.parameter == "1" || c.parameter == "5" && engine.CanDrawFive(engine.State.freeCredits.Value >= 4));
                    else if (c.action == "BuyUpgrade") show = engine.Eligible(c.parameter) && engine.UpgradeCost(c.parameter) <= engine.State.cash;
                    else if (c.action == "BuyAll" || c.action == "ReviewBatch") show = upgrades;
                    else if (c.action == "ConfirmBatch") show = ui.NotificationBatchReady;
                    else if (c.action == "Prestige") show = prestige;
                    else if (c.action == "ActivateSkill")
                    {
                        int index;
                        if (string.IsNullOrEmpty(c.parameter)) show = ui.CurrentDialog == "SkillDetail" && SkillFromDialog(out index) && skills[index];
                        else show = int.TryParse(c.parameter, out index) && index >= 0 && index < 3 && skills[index];
                    }
                    else if (c.action == "Navigate" || c.action == "Tab")
                    {
                        switch (c.parameter)
                        {
                            case "Achievements": show = achievements; break;
                            case "Skills": show = skills[0] || skills[1] || skills[2]; break;
                            case "Player": show = achievements || prestige || upgrades || skills[0] || skills[1] || skills[2]; break;
                            case "Legacy": show = draw; break;
                            case "Upgrades":
                            case "Catalogue": show = upgrades; break;
                            case "Prestige": show = prestige; break;
                            default:
                                if (c.parameter.StartsWith("SkillDetail:") && int.TryParse(c.parameter.Substring(12), out int i) && i >= 0 && i < 3) show = skills[i];
                                break;
                        }
                    }
                }
                if (binding.dot.activeSelf != show) binding.dot.SetActive(show);
            }
        }

        bool SkillFromDialog(out int index) { index = ui.NotificationSkillIndex; return index >= 0 && index < 3; }
    }
}
