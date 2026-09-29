using UnityEngine;
using UnityEngine.UI;
namespace RocketRaja
{
    /// <summary>Native equivalent of the clockwise-clearing conic cooldown overlay.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiCooldown : MaskableGraphic
    {
        float remaining;
        public float Remaining => remaining;
        public void Set(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Abs(value - remaining) < .00001f) return;
            remaining = value; SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (remaining <= 0) return;
            var rect = rectTransform.rect; float radius = Mathf.Min(rect.width, rect.height) * .5f;
            int steps = Mathf.Max(1, Mathf.CeilToInt(remaining * 96));
            vh.AddVert(rect.center, color, Vector2.zero);
            for (int i = 0; i <= steps; i++)
            {
                float angle = (1 - remaining + remaining * i / steps) * Mathf.PI * 2;
                vh.AddVert(rect.center + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius, color, Vector2.zero);
                if (i > 0) vh.AddTriangle(0, i, i + 1);
            }
        }
    }
}
