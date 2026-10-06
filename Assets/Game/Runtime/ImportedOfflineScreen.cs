using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace RocketRaja
{
    /// <summary>Displays income already settled by the engine; closing preserves that credit.</summary>
    public sealed class ImportedOfflineScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button continueButton;
        [SerializeField] TMP_Text rewardText;
        [SerializeField] TMP_Text capText;
        [SerializeField] ImportedDialogFit dialogFit;
        GameController controller;
        void Awake()
        {
            if (closeButton == null || continueButton == null || rewardText == null || capText == null || dialogFit == null)
            {
                Debug.LogError("Offline canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            closeButton.onClick.AddListener(Dismiss);
            continueButton.onClick.AddListener(Dismiss);
        }
        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Dismiss);
            if (continueButton != null) continueButton.onClick.RemoveListener(Dismiss);
        }
        public void Bind(GameController owner) => controller = owner;
        public void Open()
        {
            if (controller == null || controller.Engine == null)
            {
                Debug.LogError("Offline canvas was opened before binding its game owner.", this);
                return;
            }
            var engine = controller.Engine;
            if (string.IsNullOrEmpty(engine.State.pendingOfflineId)) return;
            rewardText.text = "₹" + engine.State.pendingOfflineReward;
            capText.text = "Up to " + (engine.OfflineCap / 3600).ToString("0") + " hours while you are away.";
            closeButton.interactable = continueButton.interactable = true;
            UiMotion.Show(gameObject, controller, true);
            UiFeedback.Pulse(rewardText.transform, controller, .1f, .65f);
        }
        // Old saved UnityEvents and delayed callbacks are deliberately inert.
        public void RequestVideo() { }
        void OnAdResult(bool rewarded) { }
        public void Dismiss()
        {
            if (controller != null && controller.Engine != null && !string.IsNullOrEmpty(controller.Engine.State.pendingOfflineId))
                controller.Engine.DismissOffline();
            UiMotion.Hide(gameObject, controller, () => controller?.HomeScreen?.RewardFeedback());
        }
    }
}
