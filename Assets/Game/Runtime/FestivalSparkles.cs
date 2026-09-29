using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Twelve preallocated native stars following the HTML celebration keyframes.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FestivalSparkles : MaskableGraphic
    {
        GameController owner;
        Vector2 origin;
        float age;
        bool alive;
        public int ActiveSparkCount => alive ? 12 : 0;
        public float Age => age;
        public void Bind(GameController game) => owner = game;
        public void Burst(Vector2 position, float intensity = 1)
        {
            if (owner != null && owner.Engine.State.reducedMotion) return;
            origin = position; age = 0; alive = true; SetVerticesDirty();
        }
        public void Clear() { alive = false; age = 0; SetVerticesDirty(); }
        public void Sample(float seconds) { age = seconds; alive = seconds < .85f; SetVerticesDirty(); }
        void Update()
        {
            if (!alive) return;
            if (owner != null && owner.Engine.State.reducedMotion) { Clear(); return; }
            Sample(age + Time.unscaledDeltaTime);
        }
        protected override void OnDisable() { Clear(); base.OnDisable(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); if (!alive) return;
            float t = UiEasing.EaseOut(age / .85f), pixels = UiEasing.Pixels(transform);
            float alpha = t <= .18f ? t / .18f : (1 - t) / .82f;
            float size = 22.5f * Mathf.Lerp(.4f, .6f, t);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6;
                var position = origin + new Vector2(Mathf.Cos(angle) * 45, -Mathf.Sin(angle) * 32) * (t * pixels);
                var tint = i % 3 == 2 ? new Color32(249, 167, 74, 255) : new Color32(255, 212, 101, 255);
                Star(mesh, position, size * 1.6f, new Color(1, .95f, .67f, alpha * .12f));
                Star(mesh, position, size * 1.25f, new Color(1, .95f, .67f, alpha * .2f));
                var core = (Color)tint; core.a = alpha; Star(mesh, position, size, core);
            }
        }
        static void Star(VertexHelper mesh, Vector2 position, float radius, Color tint)
        {
            int first = mesh.currentVertCount; mesh.AddVert(position, tint, Vector2.zero);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                mesh.AddVert(position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * (i % 2 == 0 ? 1 : .22f), tint, Vector2.zero);
            }
            for (int i = 0; i < 8; i++) mesh.AddTriangle(first, first + 1 + i, first + 1 + (i + 1) % 8);
        }
    }
}
