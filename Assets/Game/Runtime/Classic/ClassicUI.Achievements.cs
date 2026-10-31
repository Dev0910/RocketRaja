using System;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    [Serializable] public sealed class ClassicAchievementRow
    {
        public string id;
        public TMP_Text progress, reward, action;
        public TMP_Text title, description;
        public Button claim;
        public Image fill;
        public GameObject readyBadge;
    }

    public sealed partial class ClassicUI
    {
        public ClassicAchievementRow[] achievementRows = Array.Empty<ClassicAchievementRow>();
        public TMP_Text achievementSummary, achievementTabLabel;
        public TMP_Text achievementClaimedCount, achievementReadyCount, achievementWallet;
        public Sprite achievementClaimReady, achievementClaimProgress, achievementClaimed;
        public int achievementRowOffset = 1;

        void RefreshAchievements()
        {
            var state = Engine.State;
            int ready = RedesignAchievements.ReadyCount(state);
            if (achievementTabLabel) achievementTabLabel.text = "Achievements";
            if (CurrentDialog != "Achievements") return;
            foreach (var row in achievementRows)
            {
                if (!row.title || !RedesignAchievements.TryGet(row.id, out var original)) continue;
                var stage = RedesignAchievements.CurrentStage(state, original.Metric);
                row.id = stage.Id;
                row.claim.GetComponent<ClassicControl>().parameter = stage.Id;
                row.title.text = stage.Title;
                row.description.text = "Stage " + RedesignAchievements.StageNumber(stage) + " / " + RedesignAchievements.StageCount(stage.Metric) + " · " + stage.Description;
            }
            var ordered = achievementRows.OrderBy(r => RedesignAchievements.Claimed(state, r.id) ? 2
                : RedesignAchievements.TryGet(r.id, out var a) && a.Complete(state) ? 0 : 1).ToArray();
            for (int i = 0; i < ordered.Length; i++)
                if (ordered[i].claim.transform.parent.GetSiblingIndex() != i + achievementRowOffset)
                    ordered[i].claim.transform.parent.SetSiblingIndex(i + achievementRowOffset);
            if (achievementClaimedCount)
            {
                achievementClaimedCount.text = (state.claimedAchievements?.Count ?? 0) + " / " + RedesignAchievements.All.Count;
                achievementReadyCount.text = ready.ToString();
                achievementWallet.text = Count(state.kajuKatli) + " KK";
            }
            else if (achievementSummary)
                achievementSummary.text = (state.claimedAchievements?.Count ?? 0) + " / " + RedesignAchievements.All.Count
                    + " claimed   ·   " + Count(state.kajuKatli) + " Kaju Katli\n"
                    + (ready > 0 ? ready + " reward" + (ready == 1 ? "" : "s") + " ready to collect" : "Keep growing your stall. Every milestone counts.");
            foreach (var row in achievementRows)
            {
                if (!RedesignAchievements.TryGet(row.id, out var definition)) continue;
                bool claimed = RedesignAchievements.Claimed(state, row.id), complete = definition.Complete(state);
                row.progress.text = definition.Progress(state);
                row.reward.text = "+" + definition.Reward + (row.readyBadge ? "" : " Kaju Katli");
                row.action.text = claimed ? "Claimed" : complete ? "Claim" : "In progress";
                row.claim.interactable = driver.Session.CanMutate && Engine.CanClaimAchievement(row.id);
                if (row.readyBadge)
                {
                    row.readyBadge.SetActive(complete && !claimed);
                    var face = (Image)row.claim.targetGraphic;
                    var surface = claimed ? achievementClaimed : complete ? achievementClaimReady : achievementClaimProgress;
                    face.sprite = surface ? surface : complete && !claimed ? (popupPrimary ? popupPrimary : blueButton) : (popupSecondary ? popupSecondary : ivoryButton);
                    face.color = surface || complete && !claimed ? Color.white : claimed ? new Color32(205, 198, 179, 255) : new Color32(192, 216, 232, 255);
                    row.action.fontSize = row.action.fontSizeMax = complete && !claimed ? 14 : 11;
                    row.action.color = complete && !claimed ? new Color32(255, 244, 219, 255)
                        : claimed ? new Color32(83, 98, 103, 255) : new Color32(25, 72, 113, 255);
                }
                else SetButtonSkin(row.claim, row.claim.interactable);
                row.fill.fillAmount = complete ? 1 : definition.Metric == AchievementMetric.Earnings
                    ? Mathf.Clamp01((float)Amount.Ratio(state.lifetimeEarnings, definition.EarningsTarget))
                    : Mathf.Clamp01((float)((double)definition.Current(state) / definition.Target));
                row.fill.color = claimed ? new Color32(72, 145, 129, 255) : new Color32(203, 151, 58, 255);
            }
        }
    }
}
