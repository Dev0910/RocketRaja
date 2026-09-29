using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Editor-authored Offline offer; the engine remains the sole reward authority.</summary>
    public sealed class ImportedOfflineScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button videoButton;
        [SerializeField] Button continueButton;
        [SerializeField] Image videoFace;
        [SerializeField] TMP_Text rewardText;
        [SerializeField] TMP_Text capText;
        [SerializeField] TMP_Text offerText;
        [SerializeField] GameObject messageRow;
        [SerializeField] TMP_Text messageText;
        [SerializeField] Sprite enabledFace;
        [SerializeField] Sprite disabledFace;
        [SerializeField] ImportedDialogFit dialogFit;

        GameController controller;
        bool resolved, awaiting, lastReady;

        void Awake()
        {
            if (closeButton == null || videoButton == null || continueButton == null || videoFace == null ||
                rewardText == null || capText == null || offerText == null || messageRow == null ||
                messageText == null || enabledFace == null || disabledFace == null || dialogFit == null)
            {
                Debug.LogError("Offline canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            closeButton.onClick.AddListener(Dismiss);
            continueButton.onClick.AddListener(Dismiss);
            videoButton.onClick.AddListener(RequestVideo);
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Dismiss);
            if (continueButton != null) continueButton.onClick.RemoveListener(Dismiss);
            if (videoButton != null) videoButton.onClick.RemoveListener(RequestVideo);
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
            offerText.text = "Watch an optional video to add another ₹" + engine.State.pendingOfflineReward + ".";
            resolved = false;
            awaiting = false;
            UiMotion.Show(gameObject, controller, true);
            UiFeedback.Pulse(rewardText.transform, controller, .1f, .65f);
            RefreshAvailability();
        }

        void Update()
        {
            if (controller == null || resolved || awaiting || controller.Ads == null) return;
            if (controller.Ads.Ready != lastReady) RefreshAvailability();
        }

        void RefreshAvailability()
        {
            bool ready = controller != null && controller.Ads != null && controller.Ads.Ready &&
                         !string.IsNullOrEmpty(controller.Engine.State.pendingOfflineId);
            lastReady = ready;
            videoButton.interactable = ready && !awaiting && !resolved;
            videoFace.sprite = videoButton.interactable ? enabledFace : disabledFace;
            UiMotion.SetVisible(messageRow, !ready && !resolved, controller);
            if (!ready && !resolved) messageText.text = "No video is available. You can keep playing.";
            dialogFit.SetPreferredHeight(ready && !resolved ? 1570 : 1725);
        }

        public void RequestVideo()
        {
            if (controller == null || controller.Ads == null || !controller.Ads.Ready)
            {
                RefreshAvailability();
                return;
            }
            if (awaiting || resolved || string.IsNullOrEmpty(controller.Engine.State.pendingOfflineId)) return;
            awaiting = true;
            videoButton.interactable = false;
            videoFace.sprite = disabledFace;
            controller.ShowAd("offline", OnAdResult);
        }

        void OnAdResult(bool rewarded)
        {
            awaiting = false;
            resolved = true;
            videoButton.interactable = false;
            videoFace.sprite = disabledFace;
            offerText.text = rewarded ? "This offer has ended." : "Watch an optional video to add another ₹" + controller.Engine.State.pendingOfflineReward + ".";
            messageText.text = rewarded ? "This reward offer has already been used." :
                "Video closed without a reward. Your progress is safe.";
            UiMotion.Show(messageRow, controller);
            dialogFit.SetPreferredHeight(1725);
            if (rewarded) UiFeedback.Pulse(rewardText.transform, controller, .13f, .55f);
        }

        public void Dismiss()
        {
            if (controller != null && controller.Engine != null &&
                !string.IsNullOrEmpty(controller.Engine.State.pendingOfflineId))
                controller.Engine.DismissOffline();
            UiMotion.Hide(gameObject, controller, () => controller?.HomeScreen?.RewardFeedback());
        }
    }
}
