using UnityEngine;
using TMPro;

namespace RocketRaja
{
    /// <summary>Keeps the saved shop and lower panels readable at short phone heights.</summary>
    [ExecuteAlways]
    public sealed class ImportedPanelFit : MonoBehaviour
    {
        [SerializeField] RectTransform safeArea;
        [SerializeField] RectTransform world;
        [SerializeField] RectTransform lowerPanel;
        [SerializeField] TMP_Text cashLabel;
        [SerializeField] TMP_Text cashCaption;
        [SerializeField] TMP_Text shopSign;
        [SerializeField] TMP_Text counterMessage;
        [SerializeField] TMP_Text tapHint;
        float lastHeight = -1;
        public RectTransform MotionTarget => lowerPanel;

        public static float WorldHeight(float safeHeight) => Mathf.Clamp(safeHeight - 930, 690, 1400);

        void OnEnable() => Apply();

        void LateUpdate()
        {
            if (safeArea != null && !Mathf.Approximately(lastHeight, safeArea.rect.height)) Apply();
        }

        void Apply()
        {
            if (safeArea == null || lowerPanel == null) return;
            lastHeight = safeArea.rect.height;
            float height = WorldHeight(lastHeight);
            if (world != null) world.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            lowerPanel.offsetMax = new Vector2(lowerPanel.offsetMax.x, -height);
            if (world == null) return;
            float t = Mathf.InverseLerp(690, 1400, height);
            if (cashLabel != null)
            {
                cashLabel.fontSize = Mathf.Lerp(66, 115, t);
                var rect = cashLabel.rectTransform;
                rect.anchorMin = new Vector2(rect.anchorMin.x, Mathf.Lerp(.84f, .87f, t));
            }
            if (cashCaption != null)
            {
                cashCaption.fontSize = Mathf.Lerp(30, 40, t);
                var rect = cashCaption.rectTransform;
                rect.anchorMin = new Vector2(rect.anchorMin.x, Mathf.Lerp(.77f, .833f, t));
                rect.anchorMax = new Vector2(rect.anchorMax.x, Mathf.Lerp(.85f, .885f, t));
            }
            if (shopSign != null) shopSign.fontSize = Mathf.Lerp(35, 48, t);
            if (counterMessage != null) counterMessage.fontSize = Mathf.Lerp(27, 37, t);
            if (tapHint != null) tapHint.fontSize = Mathf.Lerp(20, 39, t);
        }
    }
}
