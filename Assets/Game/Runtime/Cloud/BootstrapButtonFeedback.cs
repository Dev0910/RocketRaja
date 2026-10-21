using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Cloud
{
    /// <summary>Press feedback on an already-authored label; never changes layout or creates UI.</summary>
    public sealed class BootstrapButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] RectTransform label;
        Vector2 baseline;
        bool initialized, pressed;
        bool Reduced
        {
            get
            {
                var redesign=RedesignSessionHost.Instance;
                if(redesign)return redesign.Account?.State?.reducedMotion??redesign.Coordinator?.GuestState?.reducedMotion??false;
                var host = PlayerSessionHost.Instance;
                return host?.Session?.State?.reducedMotion ?? host?.GuestState?.reducedMotion ?? false;
            }
        }
        void OnEnable() { if (label != null) { baseline = label.anchoredPosition; initialized = true; } }
        public void OnPointerDown(PointerEventData e)
        {
            if (!initialized || !GetComponent<Button>().interactable || Reduced) return;
            pressed = true;
            label.anchoredPosition = baseline + Vector2.down;
        }
        void Update() { if (pressed && Reduced) ResetPose(); }
        public void OnPointerUp(PointerEventData e) => ResetPose();
        public void OnPointerExit(PointerEventData e) => ResetPose();
        void OnDisable() => ResetPose();
        void ResetPose() { pressed = false; if (initialized && label != null) label.anchoredPosition = baseline; }
    }
}
