using UnityEngine;
namespace RocketRaja
{
    /// <summary>Retargetable 360ms CSS width transition on an authored fill.</summary>
    public sealed class UiProgress : MonoBehaviour
    {
        [SerializeField] RectTransform fill;
        GameController owner;
        float from, target, elapsed = 1;
        bool bound;
        int interval = -1;
        public float Value => fill.anchorMax.x;
        public void SetInterval(float value, int previousThreshold, GameController game)
        {
            bool changed = interval != previousThreshold;
            interval = previousThreshold;
            Set(value, game);
            if (changed) Finish();
        }
        public void Set(float value, GameController game, bool reveal = false)
        {
            owner = game; value = Mathf.Clamp01(value);
            if (reveal) fill.anchorMax = new Vector2(0, 1);
            if (bound && !reveal && Mathf.Approximately(value, target)) return;
            bound = true;
            from = fill.anchorMax.x; target = value; elapsed = 0;
            if (!gameObject.activeInHierarchy || Reduced) Finish();
        }
        bool Reduced => owner == null || owner.Engine == null || owner.Engine.State.reducedMotion;
        void Update()
        {
            if (elapsed >= .36f) return;
            if (Reduced) { Finish(); return; }
            elapsed += Time.unscaledDeltaTime;
            fill.anchorMax = new Vector2(Mathf.Lerp(from, target, UiEasing.Standard(elapsed / .36f)), 1);
        }
        void Finish() { if (fill != null) fill.anchorMax = new Vector2(target, 1); elapsed = .36f; }
        void OnDisable() => Finish();
    }
}
