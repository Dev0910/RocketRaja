using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Controls the saved, full-viewport outside target for one quantity menu.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    public sealed class UiQuantityDismiss : MonoBehaviour, ICanvasRaycastFilter
    {
        public const int BlockerOrder = 23;
        public const int MenuOrder = 24;

        [SerializeField] RectTransform viewport;
        [SerializeField] Button blocker;
        [SerializeField] Canvas menuCanvas;
        [SerializeField] Canvas blockerCanvas;
        [SerializeField] CanvasGroup screenGroup;
        [SerializeField] CanvasGroup blockerGroup;

        readonly Vector3[] corners = new Vector3[4];
        Button wiredBlocker;

        void OnEnable()
        {
            WireBlocker();
            if (blocker != null) blocker.gameObject.SetActive(true);
            Refresh();
        }

        void OnDisable()
        {
            if (blocker != null) blocker.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (wiredBlocker != null) wiredBlocker.onClick.RemoveListener(Dismiss);
        }

        void LateUpdate() => Refresh();

        void WireBlocker()
        {
            if (!Application.IsPlaying(gameObject) || wiredBlocker == blocker) return;
            if (wiredBlocker != null) wiredBlocker.onClick.RemoveListener(Dismiss);
            wiredBlocker = blocker;
            if (wiredBlocker != null) wiredBlocker.onClick.AddListener(Dismiss);
        }

        void Dismiss()
        {
            // Quantity exits are instantaneous. UiMotion.OnDisable settles its authored pose.
            if (isActiveAndEnabled) gameObject.SetActive(false);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) =>
            screenGroup == null || screenGroup.blocksRaycasts;

        /// <summary>Also used by Editor authoring to save the initial blocker geometry.</summary>
        public void Refresh()
        {
            WireBlocker();
            // Generic feedback authoring may enable every Button's feedback afterward.
            // This invisible surface must never shrink its hit area on press or hover.
            if (blocker != null && blocker.TryGetComponent<UiFeedback>(out var feedback) && feedback.enabled)
                feedback.enabled = false;
            // Unity may reset nested sorting overrides when their objects are enabled.
            if (menuCanvas != null)
            {
                menuCanvas.overrideSorting = true;
                menuCanvas.sortingOrder = MenuOrder;
            }
            if (blockerCanvas != null)
            {
                blockerCanvas.overrideSorting = true;
                blockerCanvas.sortingOrder = BlockerOrder;
            }
            // A sorting override stops Graphic.Raycast's traversal of parent groups.
            // Keep the outside target subject to the same screen transition gate.
            if (blockerGroup != null)
            {
                blockerGroup.interactable = screenGroup == null || screenGroup.interactable;
                blockerGroup.blocksRaycasts = screenGroup == null || screenGroup.blocksRaycasts;
            }

            if (viewport == null || blocker == null ||
                !(blocker.transform is RectTransform rect) || !(rect.parent is RectTransform parent)) return;
            viewport.GetWorldCorners(corners);
            Vector2 minimum = parent.InverseTransformPoint(corners[0]);
            Vector2 maximum = minimum;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 local = parent.InverseTransformPoint(corners[i]);
                minimum = Vector2.Min(minimum, local);
                maximum = Vector2.Max(maximum, local);
            }
            // Inverse conversion cancels the lower panel's fit, scale and animated travel.
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.sizeDelta = maximum - minimum;
            Vector2 center = (minimum + maximum) * .5f;
            rect.localPosition = new Vector3(center.x, center.y, 0);
        }
    }
}
