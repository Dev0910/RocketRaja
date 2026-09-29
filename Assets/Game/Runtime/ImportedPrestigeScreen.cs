using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved Prestige decision screen using a captured, validated engine quote.</summary>
    public sealed class ImportedPrestigeScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button confirmButton;
        [SerializeField] Button backButton;
        [SerializeField] Image confirmFace;
        [SerializeField] TMP_Text leadText;
        [SerializeField] TMP_Text confirmText;
        [SerializeField] TMP_Text requirementText;
        [SerializeField] TMP_Text eligibleText;
        [SerializeField] GameObject lockedCard;
        [SerializeField] GameObject resetCard;
        [SerializeField] GameObject failureRow;
        [SerializeField] Sprite readyFace;
        [SerializeField] Sprite disabledFace;
        [SerializeField] ImportedDialogFit dialogFit;

        GameController controller;
        Action onSuccess;
        PrestigeQuote quote;
        bool committed;

        void Awake()
        {
            if (closeButton == null || confirmButton == null || backButton == null || confirmFace == null ||
                leadText == null || confirmText == null || requirementText == null || eligibleText == null ||
                lockedCard == null || resetCard == null || failureRow == null || readyFace == null ||
                disabledFace == null || dialogFit == null)
            {
                Debug.LogError("Prestige canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
            confirmButton.onClick.AddListener(Confirm);
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
            if (confirmButton != null) confirmButton.onClick.RemoveListener(Confirm);
        }

        public void Bind(GameController owner, Action successfulPrestige)
        {
            controller = owner;
            onSuccess = successfulPrestige;
        }

        public void Open()
        {
            if (controller == null || controller.Engine == null)
            {
                Debug.LogError("Prestige canvas was opened before binding its game owner.", this);
                return;
            }
            quote = controller.Engine.PreviewPrestige();
            committed = false;
            bool ready = quote.reward > 0;
            leadText.text = ready ? "Earn +" + quote.reward + " reputation" : "Grow your shop to unlock prestige";
            confirmText.text = ready ? "Prestige · +" + quote.reward + " reputation" : "Prestige locked";
            confirmText.color = ready ? Color.white : new Color32(81, 75, 60, 255);
            confirmFace.sprite = ready ? readyFace : disabledFace;
            confirmButton.interactable = ready;
            requirementText.text = "Own all eight product families and earn ₹" +
                (Catalog.PrestigeThreshold / 1e15).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) +
                "Q prestige-eligible income this run.";
            eligibleText.text = "Current eligible income · ₹" + controller.Engine.State.eligibleEarnings;
            lockedCard.SetActive(!ready);
            resetCard.SetActive(ready);
            failureRow.SetActive(false);
            dialogFit.SetPreferredHeight(ready ? 1380 : 1480);
            UiMotion.Show(gameObject, controller, true);
            UiFeedback.Pulse(leadText.transform, controller, .07f, .65f);
        }

        void Confirm()
        {
            if (controller == null || quote.reward <= 0 || committed) return;
            if (!controller.Engine.Prestige(quote))
            {
                UiMotion.Show(failureRow, controller);
                dialogFit.SetPreferredHeight(1500);
                return;
            }
            committed = true;
            confirmButton.interactable = false;
            UiMotion.Hide(gameObject, controller, () => onSuccess?.Invoke());
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller);
        }
    }
}
