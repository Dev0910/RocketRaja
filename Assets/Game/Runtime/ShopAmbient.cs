using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Two clipped visitors and the shopkeeper's four-second idle, driven by one paused clock.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ShopAmbient : MaskableGraphic
    {
        [SerializeField] RectTransform keeper;
        GameController owner;
        Quaternion restRotation;
        float phase;
        bool visible, focused = true, cached;
        public float Phase => phase;
        public bool VisitorsVisible => visible;
        public RectTransform Keeper => keeper;
        public void Bind(GameController game)
        {
            owner = game;
            if (!cached) { restRotation = keeper.localRotation; cached = true; }
        }
        public static float KeeperAngle(float seconds)
        {
            float t = Mathf.Repeat(seconds, 4);
            return t < 2 ? Mathf.Lerp(-.7f, .7f, UiEasing.EaseInOut(t / 2)) : Mathf.Lerp(.7f, -.7f, UiEasing.EaseInOut((t - 2) / 2));
        }
        public static float VisitorX(float seconds, int index) => Mathf.Lerp(-.15f, 1.1f, Mathf.Repeat((seconds + (index == 1 ? 9 : 0)) / (index == 0 ? 16 : 21), 1));
        void Update()
        {
            if (owner == null || owner.Engine == null) return;
            if (owner.Engine.State.reducedMotion)
            {
                if (visible) { visible = false; phase = 0; keeper.localRotation = restRotation; SetVerticesDirty(); }
                return;
            }
            if (!visible) { visible = true; SetVerticesDirty(); }
            if (owner.ModalPaused || !focused) return;
            phase += Time.unscaledDeltaTime;
            keeper.localRotation = restRotation * Quaternion.Euler(0, 0, KeeperAngle(phase));
            SetVerticesDirty();
        }
        void OnApplicationFocus(bool focus) => focused = focus;
        protected override void OnDisable()
        {
            phase = 0; visible = false;
            if (cached && keeper != null) keeper.localRotation = restRotation;
            base.OnDisable();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); if (!visible) return;
            var rect = rectTransform.rect; float pixels = UiEasing.Pixels(transform);
            for (int i = 0; i < 2; i++)
            {
                Vector2 origin = new Vector2(rect.xMin + VisitorX(phase, i) * rect.width, rect.yMin - .12f * rect.height);
                DrawPerson(mesh, origin, pixels * .875f, color);
            }
        }
        static Vector2 Point(Vector2 origin, float scale, float x, float y) => origin + new Vector2(x, 64 - y) * scale;
        static void DrawPerson(VertexHelper mesh, Vector2 origin, float scale, Color tint)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(Point(origin, scale, 16, 9), tint, Vector2.zero);
            for (int i = 0; i < 20; i++)
            {
                float angle = i * Mathf.PI / 10;
                mesh.AddVert(Point(origin, scale, 16 + Mathf.Cos(angle) * 7, 9 + Mathf.Sin(angle) * 7), tint, Vector2.zero);
            }
            for (int i = 0; i < 20; i++) mesh.AddTriangle(first, first + 1 + i, first + 1 + (i + 1) % 20);
            Quad(mesh, origin, scale, tint, new Vector2(10, 18), new Vector2(22, 18), new Vector2(21, 45), new Vector2(10, 43));
            Quad(mesh, origin, scale, tint, new Vector2(10, 18), new Vector2(11, 28), new Vector2(7, 41), new Vector2(2, 39));
            Quad(mesh, origin, scale, tint, new Vector2(22, 18), new Vector2(29, 38), new Vector2(24, 40), new Vector2(20, 28));
            Quad(mesh, origin, scale, tint, new Vector2(10, 43), new Vector2(15, 43), new Vector2(13, 64), new Vector2(7, 64));
            Quad(mesh, origin, scale, tint, new Vector2(15, 43), new Vector2(21, 45), new Vector2(23, 64), new Vector2(17, 64));
        }
        static void Quad(VertexHelper mesh, Vector2 origin, float scale, Color tint, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            int n = mesh.currentVertCount;
            mesh.AddVert(Point(origin, scale, a.x, a.y), tint, Vector2.zero);
            mesh.AddVert(Point(origin, scale, b.x, b.y), tint, Vector2.zero);
            mesh.AddVert(Point(origin, scale, c.x, c.y), tint, Vector2.zero);
            mesh.AddVert(Point(origin, scale, d.x, d.y), tint, Vector2.zero);
            mesh.AddTriangle(n, n + 1, n + 2); mesh.AddTriangle(n, n + 2, n + 3);
        }
    }
}
