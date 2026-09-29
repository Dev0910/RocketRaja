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
        bool initialized;
        void OnEnable() { if (label != null) { baseline = label.anchoredPosition; initialized = true; } }
        public void OnPointerDown(PointerEventData e)
        {
            if (!initialized || !GetComponent<Button>().interactable || (PlayerSessionHost.Instance?.Session?.State?.reducedMotion ?? false)) return;
            label.anchoredPosition = baseline + Vector2.down;
        }
        public void OnPointerUp(PointerEventData e) => ResetPose();
        public void OnPointerExit(PointerEventData e) => ResetPose();
        void OnDisable() => ResetPose();
        void ResetPose() { if (initialized && label != null) label.anchoredPosition = baseline; }
    }
}
