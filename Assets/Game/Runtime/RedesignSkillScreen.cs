using System;
using System.Globalization;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignSkillScreen : MonoBehaviour
    {
        public TMP_Text itemName, effect, timing, status, requirement, activateLabel;
        public Image artwork;
        public Sprite[] sprites;
        public Button[] skills;
        public Button activate, unlock, upgrades, back;
        public ScrollRect scroll;
        public static readonly string[] Names = { "Festival Rush", "Helping Hands", "Bulk Order" };
        RedesignUiContext context;
        UnityAction[] actions;
        int target;
        float elapsed;
        string revision;
        SkillProjection projection;
        public int Target => target;
        void Awake()
        {
            if (!itemName || !effect || !timing || !status || !requirement || !activateLabel || !artwork || sprites.Length != 3 || skills.Length != 3 || !activate || !unlock || !upgrades || !back || !scroll)
                throw new InvalidOperationException("Incomplete saved skill screen");
            actions = new UnityAction[3]; for (int i = 0; i < 3; i++) { int selected = i; actions[i] = () => Open(selected); skills[i].onClick.AddListener(actions[i]); }
            activate.onClick.AddListener(Activate); unlock.onClick.AddListener(Unlock); upgrades.onClick.AddListener(Upgrades); back.onClick.AddListener(Back);
        }
        void OnDestroy()
        {
            if (actions != null) for (int i = 0; i < 3; i++) if (skills[i]) skills[i].onClick.RemoveListener(actions[i]);
            if (activate) activate.onClick.RemoveListener(Activate); if (unlock) unlock.onClick.RemoveListener(Unlock); if (upgrades) upgrades.onClick.RemoveListener(Upgrades); if (back) back.onClick.RemoveListener(Back);
        }
        public void Bind(RedesignUiContext value) { context = value; revision = null; }
        public void Open(int skill)
        {
            if (skill < 0 || skill >= 3) throw new ArgumentOutOfRangeException(nameof(skill));
            target = skill; revision = null; gameObject.SetActive(true); scroll.StopMovement(); scroll.verticalNormalizedPosition = 1; Refresh();
        }
        void Activate() { if (context != null) { context.Notice(context.Execute(e => e.ActivateSkill(target)) ? RedesignPurchaseFeedback.Skill(context.Engine, target) : "Skill unavailable. Check cooldown or saving."); Refresh(); } }
        void Unlock() => context?.Navigate("upgradedetails", projection.unlockId);
        void Upgrades() => context?.Navigate("skillupgrades", target.ToString(CultureInfo.InvariantCulture));
        void Back() => context?.Navigate("back", "");
        public static string Seconds(double value) => UpgradeEffectCopy.Whole(Math.Max(0, value)) + "s";
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        public void Refresh()
        {
            if (context == null) return; var e = context.Engine; var s = e.State;
            if (revision != s.commandRevision.digits)
            {
                revision = s.commandRevision.digits; projection = e.PreviewSkill(target); itemName.text = Names[target]; artwork.sprite = sprites[target];
                string prefix = projection.owned ? "" : "After buying the base skill:\n";
                effect.text = prefix + (target == 0 ? "Product income " + UpgradeEffectCopy.Multiplier(projection.power) + " for " + Seconds(projection.duration) + ". Manual and Helping Hands taps receive +25%.\nTap links and Bulk Order use normal sales." :
                    target == 1 ? UpgradeEffectCopy.Whole(projection.power) + " automatic taps/s for " + Seconds(projection.duration) + ". Each currently earns " + RedesignUiContext.Cash(e.TapPayout(true)) + ".\nHands uses the direct tap payout and its own tap timing." :
                    "Instantly earn " + RedesignUiContext.Cash(projection.orderPayout) + ".\nBase power: " + UpgradeEffectCopy.Whole(projection.power) + " seconds of normal sales, improved by your Bulk Order Talent card. Rush and other temporary effects do not boost this amount.");
                timing.text = "Cooldown: " + Seconds(projection.cooldown) + "\nStarts on activation. Repurchased base skills start ready.";
            }
            double active = Math.Max(0, s.activeUntil[target] - s.now), wait = Math.Max(0, s.readyAt[target] - s.now);
            if (target == 1) effect.text = (projection.owned ? "" : "After buying the base skill:\n") + UpgradeEffectCopy.Whole(projection.power) +
                " automatic taps/s for " + Seconds(projection.duration) + ". Each currently earns " + RedesignUiContext.Cash(e.TapPayout(true)) +
                ".\nHands uses the direct tap payout and its own tap timing.";
            status.text = !projection.owned ? "Locked this run · buy the base skill" : active > 0 ? "Active for " + Seconds(active) + " · ready again in " + Seconds(wait) : wait > 0 ? "Ready in " + Seconds(wait) : "Ready to activate";
            if (!context.CanMutate()) status.text += "\nSaving or ownership needs attention.";
            e.Data.TryUpgrade(projection.unlockId, out int i); var baseSkill = e.Data.Upgrades[i];
            requirement.text = projection.owned ? "Owned this run. Permanent cards improve this skill while owned; buy it again after prestige." : baseSkill.requirementText + "\n" + baseSkill.name + " · " + RedesignUiContext.Cash(e.UpgradeCost(baseSkill.id)) + "\nPermanent cards alone do not unlock it.";
            activate.interactable = projection.owned && wait <= 0 && context.CanMutate(); activateLabel.text = !projection.owned ? "Buy the base skill first" : wait > 0 ? "On cooldown" : "Activate " + Names[target];
        }
    }
}
