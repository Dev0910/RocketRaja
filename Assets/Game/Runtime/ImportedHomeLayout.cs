using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Fits the whole Home dashboard in its fixed viewport, including short phones.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ImportedHomeLayout : MonoBehaviour
    {
        void LateUpdate()
        {
            var content = (RectTransform)transform;
            var viewport = content.parent as RectTransform;
            if (viewport == null || viewport.rect.height <= 0) return;
            float height = LayoutUtility.GetPreferredHeight(content);
            if (height <= 0) return;
            float scale = Mathf.Min(1, viewport.rect.height / height);
            content.localScale = Vector3.one * scale;
            content.anchoredPosition = Vector2.zero;
            // Compensate width so scaled cards still fill the viewport.
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewport.rect.width / scale);
        }
    }
}
