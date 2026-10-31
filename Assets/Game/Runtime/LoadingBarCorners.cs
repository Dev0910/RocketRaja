using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Keeps the saved loading images' sliced corners circular as progress and aspect change.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Image))]
    public sealed class LoadingBarCorners : MonoBehaviour
    {
        Image target;

        void OnEnable() => Refresh();
        void OnRectTransformDimensionsChange() => Refresh();

#if UNITY_EDITOR
        void OnValidate() => Refresh();
#endif

        public void Refresh()
        {
            if (!target) target = GetComponent<Image>();
            if (!target || !target.sprite || target.type != Image.Type.Sliced) return;
            var size = target.rectTransform.rect.size;
            float radius = Mathf.Min(size.x, size.y) * .5f;
            if (radius <= 0) return;
            // One source pixel must have the same scale in both axes. A fixed, oversized
            // border was clamped only vertically by uGUI, stretching the caps into ellipses.
            float multiplier = target.sprite.border.x / (radius * target.pixelsPerUnit);
            if (multiplier > 0 && !Mathf.Approximately(target.pixelsPerUnitMultiplier, multiplier))
                target.pixelsPerUnitMultiplier = multiplier;
        }
    }
}
