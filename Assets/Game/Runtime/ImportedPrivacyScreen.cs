using System;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Controls the editor-authored Privacy dialog without creating UI at runtime.</summary>
    public sealed class ImportedPrivacyScreen : MonoBehaviour
    {
        [SerializeField] GameObject consentRow;
        [SerializeField] Button consentButton;
        [SerializeField] Button closeButton;
        [SerializeField] Button backButton;

        GameController controller;
        Action returnToSettings;

        void Awake()
        {
            if (closeButton == null || backButton == null || consentButton == null || consentRow == null)
            {
                Debug.LogError("Privacy dialog has missing editor-authored references.", this);
                return;
            }

            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
            consentButton.onClick.AddListener(OpenPrivacyOptions);
            Refresh();
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
            if (consentButton != null) consentButton.onClick.RemoveListener(OpenPrivacyOptions);
        }

        public void Bind(GameController owner, Action onReturnToSettings)
        {
            controller = owner;
            returnToSettings = onReturnToSettings;
            Refresh();
        }

        public void Open()
        {
            UiMotion.Show(gameObject, controller, true);
            Refresh();
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller, () => returnToSettings?.Invoke());
        }

        public void Refresh()
        {
            if (consentRow != null)
                consentRow.SetActive(controller != null && controller.Ads != null && controller.Ads.PrivacyOptionsRequired);
        }

        void OpenPrivacyOptions()
        {
            if (controller != null && controller.Ads != null && controller.Ads.PrivacyOptionsRequired)
                controller.Ads.PrivacyOptions();
        }
    }
}
