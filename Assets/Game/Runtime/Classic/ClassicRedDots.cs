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

        public static bool SupportsIndicator(ClassicControl control)
        {
            if (!control || control.name == "Collapse" || control.action == "Close" || control.action == "ActivateSkill") return false;
            return !((control.action == "Navigate" || control.action == "Tab") &&
                (control.parameter == "Skills" || (control.parameter ?? "").StartsWith("SkillDetail:")));
        }

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
            foreach (var binding in bindings)
            {
                if (!binding.control || !binding.dot) continue;
                var c = binding.control;
                bool show = false;
                if (can && SupportsIndicator(c))
                {
                    if (c.action == "ClaimAchievement") show = engine.CanClaimAchievement(c.parameter);
                    else if (c.action == "Draw") show = draw && (c.parameter == "1" || c.parameter == "5" && engine.CanDrawFive(engine.State.freeCredits.Value >= 4));
                    else if (c.action == "BuyUpgrade") show = engine.Eligible(c.parameter) && engine.UpgradeCost(c.parameter) <= engine.State.cash;
                    else if (c.action == "BuyAll" || c.action == "ReviewBatch") show = upgrades;
                    else if (c.action == "ConfirmBatch") show = ui.NotificationBatchReady;
                    else if (c.action == "Prestige") show = prestige;
                    else if (c.action == "Navigate" || c.action == "Tab")
                    {
                        switch (c.parameter)
                        {
                            case "Achievements": show = achievements; break;
                            case "Player": show = achievements || prestige || upgrades; break;
                            case "Legacy": show = draw; break;
                            case "Upgrades":
                            case "Catalogue": show = upgrades; break;
                            case "Prestige": show = prestige; break;
                        }
                    }
                }
                if (binding.dot.activeSelf != show) binding.dot.SetActive(show);
            }
        }

    }
}
