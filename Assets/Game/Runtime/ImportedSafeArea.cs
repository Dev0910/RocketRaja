using UnityEngine;

namespace RocketRaja
{
    /// <summary>Fits an existing, editor-authored RectTransform to the device safe area.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ImportedSafeArea : MonoBehaviour
    {
        RectTransform rect;
        Rect lastSafe;
        int lastWidth, lastHeight;

        void OnEnable()
        {
            rect = GetComponent<RectTransform>();
            Apply();
        }

        void Update()
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight || Screen.safeArea != lastSafe)
                Apply();
        }

        void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            lastSafe = Screen.safeArea;
            rect.anchorMin = new Vector2(lastSafe.xMin / lastWidth, lastSafe.yMin / lastHeight);
            rect.anchorMax = new Vector2(lastSafe.xMax / lastWidth, lastSafe.yMax / lastHeight);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
