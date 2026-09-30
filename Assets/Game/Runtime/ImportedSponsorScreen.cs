using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved Sponsor modal bound to the existing rewarded-ad and boost systems.</summary>
    public sealed class ImportedSponsorScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button videoButton;
        [SerializeField] Button backButton;
        [SerializeField] Image videoFace;
        [SerializeField] GameObject messageRow;
        [SerializeField] TMP_Text messageText;
        [SerializeField] Sprite enabledFace;
        [SerializeField] Sprite disabledFace;
        [SerializeField] ImportedDialogFit dialogFit;

        GameController controller;
        bool lastAttemptFailed, awaiting, lastReady, lastAdShowing, lastCanWatch;
        double lastRemaining;

        void Awake()
        {
            if (closeButton == null || videoButton == null || backButton == null || videoFace == null ||
                messageRow == null || messageText == null || enabledFace == null || disabledFace == null ||
                dialogFit == null)
            {
                Debug.LogError("Sponsor canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
            videoButton.onClick.AddListener(RequestVideo);
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
            if (videoButton != null) videoButton.onClick.RemoveListener(RequestVideo);
        }

        public void Bind(GameController owner) => controller = owner;

        public void Open()
        {
            if (controller == null || controller.Engine == null)
            {
                Debug.LogError("Sponsor canvas was opened before binding its game owner.", this);
                return;
            }
            // Reopening the modal must not release an in-flight request's guard.
            awaiting = awaiting && controller.IsAdShowing;
            UiMotion.Show(gameObject, controller, true);
            RefreshAvailability();
        }

        void Update()
        {
            if (controller == null || controller.Engine == null) return;
            var remaining = controller.Engine.State.sponsorUntil - controller.Engine.State.now;
            if ((controller.Ads != null && controller.Ads.Ready) != lastReady ||
                controller.IsAdShowing != lastAdShowing ||
                (controller.CanPlay && controller.Engine.CanMutate) != lastCanWatch ||
                (remaining > 0) != (lastRemaining > 0) || Mathf.Abs((float)(remaining - lastRemaining)) > 1)
                RefreshAvailability();
        }

        void RefreshAvailability()
        {
            if (controller == null || controller.Engine == null) return;
            var remaining = controller.Engine.State.sponsorUntil - controller.Engine.State.now;
            lastRemaining = remaining;
            bool active = remaining > 0;
            bool ready = controller.Ads != null && controller.Ads.Ready;
            bool adShowing = controller.IsAdShowing;
            bool canWatch = controller.CanPlay && controller.Engine.CanMutate;
            lastReady = ready;
            lastAdShowing = adShowing;
            lastCanWatch = canWatch;
            closeButton.interactable = backButton.interactable = !awaiting && !adShowing;
            videoButton.interactable = !active && ready && canWatch && !adShowing && !awaiting;
            videoFace.sprite = videoButton.interactable ? enabledFace : disabledFace;
            if (active)
            {
                var seconds = Mathf.CeilToInt((float)remaining);
                messageText.text = "Sponsor active · " + (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") +
                    " remaining. Come back after it expires.";
            }
            else if (awaiting || adShowing) messageText.text = "Waiting for the video result.";
            else if (!canWatch) messageText.text = "Videos are paused right now.";
            else if (lastAttemptFailed) messageText.text = ready
                ? "Video ended without a reward. You can try again."
                : "Video ended without a reward. No video is available.";
            else if (!ready) messageText.text = "No video is available. You can keep playing.";
            bool showMessage = active || awaiting || adShowing || !canWatch || !ready || lastAttemptFailed;
            // An asynchronous ad result can arrive after this modal was hidden.
            if (gameObject.activeInHierarchy) UiMotion.SetVisible(messageRow, showMessage, controller);
            else messageRow.SetActive(showMessage);
            dialogFit.SetPreferredHeight(showMessage ? 1730 : 1570);
        }

        public void RequestVideo()
        {
            if (controller == null || controller.Engine == null) return;
            RefreshAvailability();
            if (!videoButton.interactable) return;
            awaiting = true;
            lastAttemptFailed = false;
            RefreshAvailability();
            controller.ShowAd("sponsor", OnAdResult);
        }

        void OnAdResult(bool rewarded)
        {
            if (this == null || controller == null || controller.Engine == null) return;
            awaiting = false;
            lastAttemptFailed = !rewarded;
            RefreshAvailability();
        }

        public void Close()
        {
            if (awaiting || (controller != null && controller.IsAdShowing)) return;
            UiMotion.Hide(gameObject, controller);
        }
    }
}
