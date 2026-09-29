using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RocketRaja
{
    /// <summary>One tap on the stall, excluding drags and release outside the touch range.</summary>
    public sealed class ShopTapTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public Action onTap;
        public Vector2? TapPosition { get; private set; }
        public Camera TapCamera { get; private set; }
        readonly Dictionary<int, Vector2> pointers = new Dictionary<int, Vector2>();
        readonly HashSet<int> dragged = new HashSet<int>();

        public void OnPointerDown(PointerEventData data)
        {
            pointers[data.pointerId] = data.position;
            dragged.Remove(data.pointerId);
        }

        public void OnDrag(PointerEventData data)
        {
            if (pointers.TryGetValue(data.pointerId, out var start) &&
                Vector2.Distance(start, data.position) > 15) dragged.Add(data.pointerId);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (pointers.TryGetValue(data.pointerId, out var start) &&
                !dragged.Contains(data.pointerId) && Vector2.Distance(start, data.position) < 15)
            {
                TapPosition = data.position;
                TapCamera = data.pressEventCamera;
                try { onTap?.Invoke(); }
                finally { TapPosition = null; TapCamera = null; }
            }
            pointers.Remove(data.pointerId);
            dragged.Remove(data.pointerId);
        }

        void OnDisable() { pointers.Clear(); dragged.Clear(); TapPosition = null; TapCamera = null; }
    }
}
