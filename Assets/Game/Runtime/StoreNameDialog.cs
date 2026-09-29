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
        [SerializeField] TMP_Text title, message, cancelLabel;
        [SerializeField] Button saveButton, cancelButton;
        GameController owner;
        bool firstVisit;
        Action completed;

        void Awake()
        {
            input.onValueChanged.AddListener(_ => Validate());
            saveButton.onClick.AddListener(Save);
            cancelButton.onClick.AddListener(Dismiss);
        }
        public void Open(GameController game, bool first, Action done)
        {
            owner = game; firstVisit = first; completed = done;
            title.text = first ? "Name your store" : "Rename your store";
            cancelLabel.text = first ? "Skip for now" : "Cancel";
            input.SetTextWithoutNotify(game.Engine.State.StoreName);
            UiMotion.Show(gameObject, game, true);
            Validate();
        }
        void Update() => Validate();
        void Validate()
        {
            bool valid = GameState.TryStoreName(input.text, out _);
            saveButton.interactable = owner != null && owner.Engine.CanMutate && valid;
            message.text = valid ? "1–24 characters. You can rename it in Settings." : "Enter 1–24 characters on a single line.";
        }
        void Save()
        {
            if (owner == null || !owner.Engine.SetStoreName(input.text)) return;
            owner.Save(); owner.View.RefreshNow(); Close();
        }
        public void Dismiss()
        {
            if (owner == null) return;
            if (firstVisit)
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
