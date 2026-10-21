using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class RedesignGoalsScreen : MonoBehaviour
    {
        public TMP_Text pinnedLabel;
        public Button openPinned, clearPinned, back;
        public Button[] alternatives;
        public TMP_Text[] alternativeLabels;
        RedesignUiContext context;
        GoalTarget[] targets = Array.Empty<GoalTarget>();
        UnityAction[] listeners;
        string revision;
        float elapsed;
        public Action Close;
        public void Bind(RedesignUiContext value) { context = value; revision = null; Refresh(); }
        void Awake()
        {
            if (!pinnedLabel || !openPinned || !clearPinned || !back || alternatives == null || alternatives.Length != 3 || alternativeLabels == null || alternativeLabels.Length != 3)
                throw new InvalidOperationException("Incomplete serialized goals screen");
            listeners = new UnityAction[3];
            for (int i = 0; i < 3; i++) { int index = i; listeners[i] = () => OpenAlternative(index); alternatives[i].onClick.AddListener(listeners[i]); }
            openPinned.onClick.AddListener(OpenPinned); clearPinned.onClick.AddListener(Clear); back.onClick.AddListener(Back);
        }
        void OnDestroy()
        {
            if (listeners != null) for (int i = 0; i < 3; i++) alternatives[i].onClick.RemoveListener(listeners[i]);
            if (openPinned) openPinned.onClick.RemoveListener(OpenPinned); if (clearPinned) clearPinned.onClick.RemoveListener(Clear); if (back) back.onClick.RemoveListener(Back);
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        void OpenPinned() { if (context != null && context.Engine.State.pinnedUpgradeId != "") context.Navigate("upgradedetails", context.Engine.State.pinnedUpgradeId); }
        void Clear() { context?.Execute(e => e.PinUpgrade("")); Refresh(); }
        void Back() => Close?.Invoke();
        void OpenAlternative(int index) { if (context != null && index < targets.Length) context.OpenGoal(targets[index]); }
        public void Refresh()
        {
            if (context == null) return;
            var e = context.Engine;
            if (revision != e.State.commandRevision.digits + ":" + e.State.pinnedUpgradeId)
            {
                targets = e.GoalAlternatives(); revision = e.State.commandRevision.digits + ":" + e.State.pinnedUpgradeId;
            }
            bool pinned = e.Data.TryUpgrade(e.State.pinnedUpgradeId, out int item);
            pinnedLabel.text = pinned ? e.Data.Upgrades[item].name + "\n" + RedesignUiContext.Cash(e.UpgradeCost(e.State.pinnedUpgradeId)) + (e.Eligible(e.State.pinnedUpgradeId) ? e.State.cash >= e.UpgradeCost(e.State.pinnedUpgradeId) ? " · Affordable now" : " · Keep earning" : " · Requirements remain") : "No pinned goal. Pin a cash upgrade from its details.";
            openPinned.interactable = pinned; clearPinned.interactable = pinned && context.CanMutate();
            for (int i = 0; i < alternatives.Length; i++)
            {
                alternatives[i].gameObject.SetActive(i < targets.Length); if (i >= targets.Length) continue;
                var target = targets[i]; string label;
                if (target.kind == GoalTargetKind.Upgrade) { e.Data.TryUpgrade(target.upgradeId, out int index); label = e.Data.Upgrades[index].name; }
                else label = target.kind == GoalTargetKind.Product ? "Add a " + e.Data.Products[target.product].name + " level" : "Train your taps";
                alternativeLabels[i].text = label + "\n" + RedesignUiContext.Cash(target.cost) + (e.State.cash >= target.cost ? " · Affordable" : " · Save toward this");
            }
        }
    }
}
