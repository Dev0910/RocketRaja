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
        bool resolved, awaiting, lastReady, lastAdShowing, lastCanWatch, lastEligible;
        bool lastAttemptFailed, bonusAdded;
        string offeredOfflineId;

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
            if (offeredOfflineId != engine.State.pendingOfflineId)
                lastAttemptFailed = bonusAdded = false;
            offeredOfflineId = engine.State.pendingOfflineId;
            rewardText.text = "₹" + engine.State.pendingOfflineReward;
            capText.text = "Up to " + (engine.OfflineCap / 3600).ToString("0") + " hours while you are away.";
            offerText.text = "Watch an optional video to add another ₹" + engine.State.pendingOfflineReward + ".";
            resolved = false;
            // Reopening the modal must not release an in-flight request's guard.
            awaiting = awaiting && controller.IsAdShowing;
            UiMotion.Show(gameObject, controller, true);
            UiFeedback.Pulse(rewardText.transform, controller, .1f, .65f);
            RefreshAvailability();
        }

        void Update()
        {
            if (controller == null || controller.Engine == null) return;
            bool eligible = !string.IsNullOrEmpty(controller.Engine.State.pendingOfflineId) &&
                            controller.Engine.State.pendingOfflineId == offeredOfflineId;
            if ((controller.Ads != null && controller.Ads.Ready) != lastReady ||
                controller.IsAdShowing != lastAdShowing || eligible != lastEligible ||
                (controller.CanPlay && controller.Engine.CanMutate) != lastCanWatch)
                RefreshAvailability();
        }

        void RefreshAvailability()
        {
            if (controller == null || controller.Engine == null) return;
            bool eligible = !string.IsNullOrEmpty(controller.Engine.State.pendingOfflineId) &&
                            controller.Engine.State.pendingOfflineId == offeredOfflineId;
            bool ready = controller.Ads != null && controller.Ads.Ready;
            bool adShowing = controller.IsAdShowing;
            bool canWatch = controller.CanPlay && controller.Engine.CanMutate;
            resolved = !eligible;
            lastReady = ready;
            lastAdShowing = adShowing;
            lastCanWatch = canWatch;
            lastEligible = eligible;
            closeButton.interactable = continueButton.interactable = !awaiting && !adShowing;
            videoButton.interactable = ready && canWatch && !adShowing && !awaiting && !resolved;
            videoFace.sprite = videoButton.interactable ? enabledFace : disabledFace;
            bool showMessage = resolved || awaiting || adShowing || !canWatch || !ready || lastAttemptFailed;
            if (resolved)
            {
                offerText.text = bonusAdded ? "Your offline bonus has been added." : "This offer has ended.";
                messageText.text = bonusAdded ? "Your offline earnings doubled. Keep playing!" :
                    "This offer has ended. You can keep playing.";
            }
            else if (awaiting || adShowing) messageText.text = "Waiting for the video result.";
            else if (!canWatch) messageText.text = "Videos are paused right now.";
            else if (lastAttemptFailed) messageText.text = ready
                ? "Video ended without a reward. You can try again."
                : "Video ended without a reward. No video is available.";
            else if (!ready) messageText.text = "No video is available. You can keep playing.";
            // An asynchronous ad result can arrive after this modal was hidden.
            if (gameObject.activeInHierarchy) UiMotion.SetVisible(messageRow, showMessage, controller);
            else messageRow.SetActive(showMessage);
            dialogFit.SetPreferredHeight(showMessage ? 1725 : 1570);
        }

        public void RequestVideo()
        {
            if (controller == null || controller.Engine == null) return;
            RefreshAvailability();
            if (!videoButton.interactable) return;
            awaiting = true;
            lastAttemptFailed = false;
            RefreshAvailability();
            controller.ShowAd("offline", OnAdResult);
        }

        void OnAdResult(bool rewarded)
        {
            if (this == null || controller == null || controller.Engine == null) return;
            awaiting = false;
            lastAttemptFailed = !rewarded;
            bonusAdded = rewarded;
            RefreshAvailability();
            if (rewarded && gameObject.activeInHierarchy)
                UiFeedback.Pulse(rewardText.transform, controller, .13f, .55f);
        }

        public void Dismiss()
        {
            if (awaiting || (controller != null && controller.IsAdShowing)) return;
            if (controller != null && controller.Engine != null &&
                !string.IsNullOrEmpty(controller.Engine.State.pendingOfflineId))
                controller.Engine.DismissOffline();
            UiMotion.Hide(gameObject, controller, () => controller?.HomeScreen?.RewardFeedback());
        }
    }
}
