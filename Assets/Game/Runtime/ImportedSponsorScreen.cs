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
        bool attempted, awaiting, lastReady;
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
            attempted = false;
            awaiting = false;
            UiMotion.Show(gameObject, controller, true);
            RefreshAvailability();
        }

        void Update()
        {
            if (controller == null || awaiting || attempted || controller.Ads == null) return;
            var remaining = controller.Engine.State.sponsorUntil - controller.Engine.State.now;
            if (controller.Ads.Ready != lastReady || Mathf.Abs((float)(remaining - lastRemaining)) > 1)
                RefreshAvailability();
        }

        void RefreshAvailability()
        {
            if (controller == null || controller.Engine == null) return;
            var remaining = controller.Engine.State.sponsorUntil - controller.Engine.State.now;
            lastRemaining = remaining;
            bool active = remaining > 0;
            bool ready = controller.Ads != null && controller.Ads.Ready;
            lastReady = ready;
            videoButton.interactable = !active && ready && !awaiting && !attempted;
            videoFace.sprite = videoButton.interactable ? enabledFace : disabledFace;
            if (active)
            {
                var seconds = Mathf.CeilToInt((float)remaining);
                messageText.text = "Sponsor active · " + (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") +
                    " remaining. Come back after it expires.";
            }
            else if (!ready) messageText.text = "No video is available. You can keep playing.";
            UiMotion.SetVisible(messageRow, active || !ready, controller);
            dialogFit.SetPreferredHeight(messageRow.activeSelf ? 1730 : 1570);
        }

        public void RequestVideo()
        {
            if (controller == null || controller.Ads == null || !controller.Ads.Ready)
            {
                RefreshAvailability();
                return;
            }
            if (awaiting || attempted || controller.Engine.State.sponsorUntil > controller.Engine.State.now) return;
            awaiting = true;
            videoButton.interactable = false;
            videoFace.sprite = disabledFace;
            controller.ShowAd("sponsor", OnAdResult);
        }

        void OnAdResult(bool rewarded)
        {
            awaiting = false;
            attempted = true;
            videoButton.interactable = false;
            videoFace.sprite = disabledFace;
            if (rewarded)
            {
                attempted = false;
                RefreshAvailability();
            }
            else
            {
                messageText.text = "Video closed without a reward. Your progress is safe.";
                UiMotion.Show(messageRow, controller);
                dialogFit.SetPreferredHeight(1730);
            }
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller);
        }
    }
}
