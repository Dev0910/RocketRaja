using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>The counter alone accepts manual taps; dragging it still scrolls Home.</summary>
    public sealed class RedesignTapTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ScrollRect scroll;
        public Action Tap;
        public Vector3? LastTapWorldPosition { get; private set; }
        readonly Dictionary<int, Vector2> starts = new Dictionary<int, Vector2>();
        readonly HashSet<int> dragged = new HashSet<int>();
        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left) return;
            starts[e.pointerId] = e.position; dragged.Remove(e.pointerId);
        }
        public void OnBeginDrag(PointerEventData e) { dragged.Add(e.pointerId); if (scroll) scroll.OnBeginDrag(e); }
        public void OnDrag(PointerEventData e) { dragged.Add(e.pointerId); if (scroll) scroll.OnDrag(e); }
        public void OnEndDrag(PointerEventData e) { if (scroll) scroll.OnEndDrag(e); }
        public void OnPointerUp(PointerEventData e)
        {
            bool accepted = e.button == PointerEventData.InputButton.Left && starts.TryGetValue(e.pointerId, out var start) && !dragged.Contains(e.pointerId) &&
                Vector2.Distance(start, e.position) < 15 && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, e.position, e.pressEventCamera);
            starts.Remove(e.pointerId); dragged.Remove(e.pointerId);
            if (accepted)
            {
                LastTapWorldPosition = RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out var point) ? point : (Vector3?)null;
                Tap?.Invoke();
            }
        }
        void OnDisable() { starts.Clear(); dragged.Clear(); }
        void OnApplicationPause(bool paused) { if (paused) { starts.Clear(); dragged.Clear(); } }
        void OnApplicationFocus(bool focused) { if (!focused) { starts.Clear(); dragged.Clear(); } }
    }
}
