using UnityEngine;
using UnityEngine.UI;
namespace RocketRaja
{
    /// <summary>Native one-pixel skill state ring with the HTML 180ms shadow-color transition.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiSkillRing : MaskableGraphic
    {
        Color from, target;
        GameController owner;
        float elapsed = 1;
        bool bound;
        public void SetState(bool active, bool locked, GameController game, bool cooldown = false)
        {
            owner = game;
            Color next = locked ? new Color32(153, 138, 108, 255) : active ? new Color32(146, 35, 73, 255) : cooldown ? new Color32(112, 121, 132, 255) : new Color32(24, 68, 119, 255);
            if (bound && target == next) return;
            target = next; from = color; elapsed = 0;
            if (!bound || game.Engine.State.reducedMotion) { color = target; elapsed = .18f; }
            bound = true;
        }
        void Update()
        {
            if (elapsed >= .18f) return;
            elapsed += Time.unscaledDeltaTime;
            color = owner == null || owner.Engine.State.reducedMotion ? target : Color.Lerp(from, target, UiEasing.EaseOut(elapsed / .18f));
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var rect = rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * .5f, width = UiEasing.Pixels(transform);
            for (int i = 0; i <= 72; i++)
            {
                float angle = i * Mathf.PI * 2 / 72; var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(rect.center + direction * radius, color, Vector2.zero);
                vh.AddVert(rect.center + direction * (radius + width), color, Vector2.zero);
                if (i > 0) { int n = i * 2; vh.AddTriangle(n - 2, n - 1, n); vh.AddTriangle(n - 1, n + 1, n); }
            }
        }
    }
}
