using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Cloud
{
    /// <summary>Preferred-size input to the saved safe-area layout group. Never writes geometry.</summary>
    [ExecuteAlways]
    public sealed class BootstrapPopupSize : UIBehaviour, ILayoutElement
    {
        public RectTransform content;
        public float padding=96;
        [Range(0,1)] public float viewportHeightFraction;
        public float minimumViewportHeight=1020;
        float last=-1;
        public float minWidth=>-1;
        public float preferredWidth=>-1;
        public float flexibleWidth=>-1;
        public float minHeight=>0;
        public float preferredHeight=>viewportHeightFraction>0 && transform.parent is RectTransform viewport
            ? Mathf.Min(viewport.rect.height,Mathf.Max(minimumViewportHeight,viewport.rect.height*viewportHeightFraction))
            : content?Mathf.Max(600,LayoutUtility.GetPreferredHeight(content)+padding):600;
        public float flexibleHeight=>0;
        public int layoutPriority=>2;
        public void CalculateLayoutInputHorizontal() { }
        public void CalculateLayoutInputVertical() { }
        protected override void OnEnable(){base.OnEnable();Canvas.willRenderCanvases+=InvalidatePreferredSize;last=-1;}
        protected override void OnDisable(){Canvas.willRenderCanvases-=InvalidatePreferredSize;base.OnDisable();}
        void InvalidatePreferredSize()
        {
            float value=preferredHeight;if(Mathf.Approximately(last,value))return;last=value;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }
    }
}
