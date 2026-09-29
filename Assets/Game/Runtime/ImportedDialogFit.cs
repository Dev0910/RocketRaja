using UnityEngine;

namespace RocketRaja
{
    /// <summary>Fits a saved modal RectTransform within the existing safe-area rect.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ImportedDialogFit : MonoBehaviour
    {
        [SerializeField] float preferredHeight = 1570;
        [SerializeField] float minimumVerticalMargin = 90;
        RectTransform rect;
        float lastParentHeight = -1, lastPreferred = -1;

        public void SetPreferredHeight(float value)
        {
            preferredHeight = value;
            Apply();
        }

        void OnEnable()
        {
            rect = GetComponent<RectTransform>();
            Apply();
        }

        void LateUpdate()
        {
            if (rect == null) rect = GetComponent<RectTransform>();
            if (!(rect.parent is RectTransform parent)) return;
            if (!Mathf.Approximately(parent.rect.height, lastParentHeight) ||
                !Mathf.Approximately(preferredHeight, lastPreferred)) Apply();
        }

        void Apply()
        {
            if (rect == null) rect = GetComponent<RectTransform>();
            if (!(rect.parent is RectTransform parent)) return;
            lastParentHeight = parent.rect.height;
            lastPreferred = preferredHeight;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Min(preferredHeight, Mathf.Max(1, parent.rect.height - minimumVerticalMargin)));
        }
    }
}
