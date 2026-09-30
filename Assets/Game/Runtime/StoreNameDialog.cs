using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    public sealed class StoreNameDialog : MonoBehaviour
    {
        [SerializeField] TMP_InputField input;
        [SerializeField] TMP_Text title, description, message, cancelLabel, saveLabel, detailName;
        [SerializeField] GameObject inputRow, detailPanel;
        [SerializeField] Button saveButton, cancelButton;
        GameController owner;
        bool firstVisit, detailMode;
        Action completed;

        void Awake()
        {
            input.onValueChanged.AddListener(_ => Validate());
            saveButton.onClick.AddListener(Save);
            cancelButton.onClick.AddListener(Dismiss);
        }
        public void Open(GameController game, bool first, Action done)
        {
            owner = game; firstVisit = first; detailMode = false; completed = done;
            bool replacingSavedName = game.Engine.State.NeedsStoreNameUpdate;
            inputRow.SetActive(true);
            detailPanel.SetActive(false);
            message.gameObject.SetActive(true);
            saveButton.gameObject.SetActive(true);
            SetWideCancel(false);
            title.text = first ? "Name your store" : replacingSavedName ? "Choose a new name" : "Rename your store";
            description.text = replacingSavedName ? "Old name stays until you save." :
                "A name for your fireworks shop";
            description.fontSize = replacingSavedName ? 36 : 33;
            message.fontSize = 36;
            cancelLabel.text = first ? "Skip for now" : replacingSavedName ? "Keep name" : "Cancel";
            saveLabel.text = "Save name";
            input.SetTextWithoutNotify(replacingSavedName ? "" : game.Engine.State.StoreName);
            if (input.placeholder is TMP_Text placeholder)
                placeholder.text = replacingSavedName ? "Enter new store name" : "My Fireworks Store";
            UiMotion.Show(gameObject, game, true);
            Validate();
        }

        public void OpenDetail(GameController game, string returnLabel)
        {
            owner = game; firstVisit = false; detailMode = true; completed = null;
            bool replacingSavedName = game.Engine.State.NeedsStoreNameUpdate;
            title.text = replacingSavedName ? GameState.StoreNameUpdateRequired : "Your store";
            description.text = replacingSavedName ?
                "Some characters can't show here. Your name is still saved." : "Full store name";
            description.fontSize = replacingSavedName ? 36 : 33;
            detailName.text = game.Engine.State.StoreName;
            detailName.richText = false;
            detailPanel.SetActive(true);
            inputRow.SetActive(false);
            message.gameObject.SetActive(false);
            cancelLabel.text = replacingSavedName ? "Keep name" : returnLabel;
            saveLabel.text = replacingSavedName ? "Choose new name" : "Rename";
            bool canRename = game.Engine.CanMutate;
            saveButton.gameObject.SetActive(canRename);
            saveButton.interactable = canRename;
            SetWideCancel(!canRename);
            UiMotion.Show(gameObject, game, true);
        }

        void SetWideCancel(bool wide)
        {
            var rect = (RectTransform)cancelButton.transform;
            // UiFeedback caches the authored left-hand position and writes it each frame.
            // Suspend that decoration while the sole read-only action is centered.
            var feedback = cancelButton.GetComponent<UiFeedback>();
            if (wide && feedback != null) feedback.enabled = false;
            rect.anchorMin = new Vector2(.5f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(.5f, rect.anchorMax.y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, wide ? 732 : 427);
            rect.anchoredPosition = new Vector2(wide ? 0 : -237.5f, rect.anchoredPosition.y);
            if (!wide && feedback != null) feedback.enabled = true;
        }

        void Update() { if (!detailMode) Validate(); }
        void Validate()
        {
            if (detailMode) return;
            bool valid = GameState.TryStoreName(input.text, out _);
            saveButton.interactable = owner != null && owner.Engine.CanMutate && valid;
            bool replacingSavedName = owner != null && owner.Engine.State.NeedsStoreNameUpdate;
            message.text = valid ? "1–24 Latin letters, numbers or punctuation." :
                replacingSavedName && string.IsNullOrEmpty(input.text) ? "Enter a Latin name to replace it." :
                "Use supported Latin letters or numbers (punctuation allowed).";
        }
        void Save()
        {
            if (detailMode)
            {
                if (owner == null || !owner.Engine.CanMutate) return;
                Open(owner, false, null);
                return;
            }
            if (owner == null || !owner.Engine.SetStoreName(input.text)) return;
            owner.Save(); owner.View.RefreshNow(); Close();
        }
        public void Dismiss()
        {
            if (owner == null) return;
            if (!detailMode && firstVisit && string.IsNullOrEmpty(owner.Engine.State.storeName))
            {
                if (!owner.Engine.SetStoreName(GameState.DefaultStoreName)) return;
                owner.Save(); owner.View.RefreshNow();
            }
            Close();
        }
        void Close()
        {
            var done = completed; completed = null;
            UiMotion.Hide(gameObject, owner, done);
        }
    }
}
