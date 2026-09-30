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
        bool browsing;
        public RectTransform MotionTarget => lowerPanel;

        public static float WorldHeight(float safeHeight) => Mathf.Clamp(safeHeight - 930, 690, 1400);

        // Leave both shelf rows unobstructed on short phones. Taller phones retain the
        // larger browse panel because enough of each shelf target remains visible.
        public static float BrowsePanelTop(float safeHeight)
        {
            float height = WorldHeight(safeHeight);
            return height * Mathf.Lerp(.765f, .72f, Mathf.InverseLerp(690, 1400, height));
        }

        public void SetBrowsing(bool value)
        {
            if (browsing == value) return;
            browsing = value;
            Apply();
        }

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
            lowerPanel.offsetMax = new Vector2(lowerPanel.offsetMax.x,
                -(browsing ? BrowsePanelTop(lastHeight) : height));
            if (world == null) return;
            // Keep the Settings hit area usable when the illustrated world compresses on short phones.
            var settings = world.Find("settings-button") as RectTransform;
            if (settings != null)
            {
                var rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
                float scale = rootCanvas == null ? 1f : Mathf.Max(.01f, rootCanvas.scaleFactor);
                float minimumHeight = 44f / scale;
                float bottom = Mathf.Min(.883f, .985f - minimumHeight / height);
                settings.anchorMin = new Vector2(settings.anchorMin.x, bottom);
            }
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
            if (shopSign != null) shopSign.fontSize = Mathf.Lerp(36, 48, t);
            if (counterMessage != null) counterMessage.fontSize = Mathf.Lerp(27, 37, t);
            if (tapHint != null)
            {
                // The short-phone world is only 230 screen pixels tall at 360px width.
                // Give the reward copy enough height and type size to remain legible there.
                tapHint.fontSize = Mathf.Lerp(48, 39, t);
                var hintRect = tapHint.rectTransform.parent as RectTransform;
                if (hintRect != null)
                {
                    hintRect.anchorMin = new Vector2(Mathf.Lerp(.265f, .185f, t), Mathf.Lerp(.025f, .035f, t));
                    hintRect.anchorMax = new Vector2(Mathf.Lerp(.735f, .815f, t), Mathf.Lerp(.12f, .085f, t));
                }
            }
        }
    }
}
