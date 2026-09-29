using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved Statistics modal with values from the authoritative game state.</summary>
    public sealed class ImportedStatisticsScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text[] values;

        GameController controller;

        void Awake()
        {
            if (closeButton == null || backButton == null || values == null || values.Length != 6)
            {
                Debug.LogError("Statistics canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            foreach (var value in values)
                if (value == null)
                {
                    Debug.LogError("Statistics canvas has an unbound value label.", this);
                    enabled = false;
                    return;
                }
            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
        }

        public void Bind(GameController owner) => controller = owner;

        public void Open()
        {
            if (controller == null || controller.Engine == null)
            {
                Debug.LogError("Statistics canvas was opened before binding its game owner.", this);
                return;
            }
            Refresh();
            UiMotion.Show(gameObject, controller, true);
        }

        public void Refresh()
        {
            if (controller == null || controller.Engine == null) return;
            var state = controller.Engine.State;
            values[0].text = "₹" + state.runEarnings;
            values[1].text = "₹" + state.eligibleEarnings;
            values[2].text = "₹" + state.lifetimeEarnings;
            values[3].text = state.manualTaps.ToString("N0", CultureInfo.InvariantCulture);
            values[4].text = state.prestiges.ToString("N0", CultureInfo.InvariantCulture);
            values[5].text = state.lifetimeReputation.ToString("N0", CultureInfo.InvariantCulture);
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller);
        }
    }
}
