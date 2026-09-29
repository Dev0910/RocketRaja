using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>The shopkeeper's four-second idle, driven by a paused clock.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ShopAmbient : MaskableGraphic
    {
        [SerializeField] RectTransform keeper;
        GameController owner;
        Quaternion restRotation;
        float phase;
        bool visible, focused = true, cached;
        public float Phase => phase;
        public bool VisitorsVisible => false;
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
        protected override void OnPopulateMesh(VertexHelper mesh) => mesh.Clear();
    }
}
