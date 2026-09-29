using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Blocking presentation for an unreadable local save.</summary>
    public sealed class ImportedSaveRecoveryScreen : MonoBehaviour
    {
        [SerializeField] Button startButton;
        [SerializeField] Button exitButton;
        [SerializeField] GameObject errorRow;
        [SerializeField] TMP_Text errorText;

        GameController controller;

        void Awake()
        {
            if (startButton == null || exitButton == null || errorRow == null || errorText == null)
            {
                Debug.LogError("Save Recovery canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            startButton.onClick.AddListener(StartFresh);
            exitButton.onClick.AddListener(Exit);
        }

        void OnDestroy()
        {
            if (startButton != null) startButton.onClick.RemoveListener(StartFresh);
            if (exitButton != null) exitButton.onClick.RemoveListener(Exit);
        }

        public void Bind(GameController owner) => controller = owner;

        public void Open()
        {
            if (controller == null)
            {
                Debug.LogError("Save Recovery canvas was opened before binding its game owner.", this);
                return;
            }
            errorRow.SetActive(false);
            UiMotion.Show(gameObject, controller, true);
        }

        void StartFresh()
        {
            if (controller == null) return;
            if (controller.StartFreshAfterDamagedSave())
            {
                UiMotion.Hide(gameObject, controller);
                controller.View?.RefreshNow();
                return;
            }
            errorText.text = controller.SaveWarning ?? "Saving failed. Please check device storage.";
            errorRow.SetActive(true);
        }

        void Exit() => Application.Quit();
    }
}
