using System;
using System.Globalization;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Explicitly simulated purchases. Uses no purchasing SDK, receipt or network service.</summary>
    public sealed class RedesignShopScreen : MonoBehaviour
    {
        public TMP_Text balance, status, pendingText, simulationStatus;
        public TMP_Text[] packNames, packAmounts, packPrices;
        public Button[] packs;
        public Button delivered, cancelled, failed, leavePending, back, testStates, ready, loading, unavailable, retry;
        public GameObject pendingPanel, testPanel;
        public ScrollRect scroll;
        RedesignUiContext context;
        UnityAction[] packActions;
        Func<bool> retrySaving;
        bool disposableGuest;
        string shownOperation;
        float elapsed;
        public enum TestAvailability { Ready, Loading, Unavailable }
        public TestAvailability Availability { get; private set; }
        public string PendingOperation => shownOperation;
        bool Allowed
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return disposableGuest && context != null && !context.Engine.State.HasPremium;
#else
                return false;
#endif
            }
        }
        void Awake()
        {
            if (!balance || !status || !pendingText || !simulationStatus || packs.Length != 4 || packNames.Length != 4 || packAmounts.Length != 4 || packPrices.Length != 4 ||
                !delivered || !cancelled || !failed || !leavePending || !back || !testStates || !ready || !loading || !unavailable || !retry || !pendingPanel || !testPanel || !scroll)
                throw new InvalidOperationException("Incomplete saved test shop");
            packActions = new UnityAction[4];
            for (int i = 0; i < 4; i++) { int index = i; packActions[i] = () => Begin(index); packs[i].onClick.AddListener(packActions[i]); }
            delivered.onClick.AddListener(Deliver); cancelled.onClick.AddListener(Cancel); failed.onClick.AddListener(Fail);
            leavePending.onClick.AddListener(Back); back.onClick.AddListener(Back); testStates.onClick.AddListener(ToggleTests);
            ready.onClick.AddListener(Ready); loading.onClick.AddListener(Loading); unavailable.onClick.AddListener(Unavailable); retry.onClick.AddListener(Retry);
        }
        void OnDestroy()
        {
            if (packActions != null) for (int i = 0; i < 4; i++) if (packs[i]) packs[i].onClick.RemoveListener(packActions[i]);
            if (delivered) delivered.onClick.RemoveListener(Deliver); if (cancelled) cancelled.onClick.RemoveListener(Cancel); if (failed) failed.onClick.RemoveListener(Fail);
            if (leavePending) leavePending.onClick.RemoveListener(Back); if (back) back.onClick.RemoveListener(Back); if (testStates) testStates.onClick.RemoveListener(ToggleTests);
            if (ready) ready.onClick.RemoveListener(Ready); if (loading) loading.onClick.RemoveListener(Loading); if (unavailable) unavailable.onClick.RemoveListener(Unavailable); if (retry) retry.onClick.RemoveListener(Retry);
        }
        public void Bind(RedesignUiContext value, bool isDisposableGuest, Func<bool> retrySave)
        {
            if (context?.Engine != value.Engine) { Availability = TestAvailability.Ready; shownOperation = null; }
            context = value; disposableGuest = isDisposableGuest; retrySaving = retrySave;
            for (int i = 0; i < 4; i++)
            {
                var pack = context.Engine.Data.Packs[i]; packNames[i].text = pack.name;
                packAmounts[i].text = pack.kajuKatli.ToString("N0", CultureInfo.InvariantCulture) + " KK · " + (pack.kajuKatli / 100) + " draws";
                packPrices[i].text = "Example price ₹" + pack.designInr.ToString("N0", CultureInfo.InvariantCulture) + " · not charged";
            }
            Refresh();
        }
        public void Open() { gameObject.SetActive(true); Refresh(); if (pendingPanel.activeSelf) scroll.verticalNormalizedPosition = 1; }
        void Back() => context?.Navigate("talent", "");
        void ToggleTests() { testPanel.SetActive(!testPanel.activeSelf); if (testPanel.activeSelf) scroll.verticalNormalizedPosition = 1; }
        void Ready() { Availability = TestAvailability.Ready; Refresh(); }
        void Loading() { Availability = TestAvailability.Loading; Refresh(); }
        void Unavailable() { Availability = TestAvailability.Unavailable; Refresh(); }
        void Retry() { if (retrySaving != null && !retrySaving()) context.Notice("Saving still needs attention. The pending purchase has not changed."); Refresh(); }
        void Begin(int index)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!Allowed || !context.CanMutate() || Availability != TestAvailability.Ready || context.Engine.State.developmentShop?.status == "pending") return;
            bool started = context.Execute(e => e.EnableDevelopmentShop(DevelopmentShopState.Namespace)) &&
                context.Execute(e => e.BeginDevelopmentPurchase(e.Data.Packs[index].id) != null);
            if (!started) context.Notice("Test purchase was not started. Recover saving before retrying.");
            Refresh(); scroll.verticalNormalizedPosition = 1;
#endif
        }
        void Deliver() => Resolve(0);
        void Cancel() => Resolve(1);
        void Fail() => Resolve(2);
        void Resolve(int outcome)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!Allowed || !context.CanMutate() || string.IsNullOrEmpty(shownOperation) || outcome == 0 && Availability != TestAvailability.Ready) return;
            string operation = shownOperation;
            bool completed = context.Execute(e => e.ResolveDevelopmentPurchase(operation,
                outcome == 0 ? DevelopmentPurchaseOutcome.Delivered : outcome == 1 ? DevelopmentPurchaseOutcome.Cancelled : DevelopmentPurchaseOutcome.Failed));
            if (!completed) context.Notice("Test outcome was not saved. Recover saving, then retry this pending purchase.");
            Refresh();
#endif
        }
        void Update() { elapsed += Time.unscaledDeltaTime; if (elapsed >= .2f) { elapsed = 0; Refresh(); } }
        public void Refresh()
        {
            if (context == null) return;
            var state = context.Engine.State; var purchase = state.developmentShop;
            bool pending = state.HasDevelopmentShop && purchase.status == "pending";
            bool mutable = context.CanMutate(); shownOperation = pending ? purchase.operationId : null;
            balance.text = RedesignCardCopy.Count(state.kajuKatli) + " KK · One draw costs 100 KK";
            status.text = !Allowed ? "Test purchases are unavailable for this profile or build." : !mutable ? "Saving or ownership needs attention. No new test outcome can be applied." :
                pending ? "A test purchase is pending. No KK has been delivered." : purchase?.status == "delivered" ? "Test KK delivered and saved. Nothing was spent automatically." :
                purchase?.status == "cancelled" ? "Test purchase cancelled. No KK delivered." : purchase?.status == "failed" ? "Test purchase failed. No KK delivered. You can try a pack again." :
                "Choose a pack to try a simulated purchase. No money will be charged.";
            simulationStatus.text = Availability == TestAvailability.Ready ? "Simulation: ready" : Availability == TestAvailability.Loading ? "Simulation: loading · pack actions are disabled" : "Simulation: unavailable · pack actions are disabled";
            for (int i = 0; i < packs.Length; i++) packs[i].interactable = Allowed && mutable && !pending && Availability == TestAvailability.Ready;
            pendingPanel.SetActive(pending);
            if (pending)
            {
                var pack = context.Engine.Data.Packs[0]; foreach (var candidate in context.Engine.Data.Packs) if (candidate.id == purchase.packId) pack = candidate;
                pendingText.text = pack.name + " · " + pack.kajuKatli.ToString("N0", CultureInfo.InvariantCulture) + " KK\nChoose a test outcome. Leaving or restarting keeps this same pending purchase; it grants nothing until success is saved.";
            }
            delivered.interactable = pending && Allowed && mutable && Availability == TestAvailability.Ready;
            cancelled.interactable = failed.interactable = pending && Allowed && mutable;
            retry.gameObject.SetActive(!mutable); retry.interactable = retrySaving != null;
            testStates.interactable = ready.interactable = loading.interactable = unavailable.interactable = Allowed;
        }
    }
}
