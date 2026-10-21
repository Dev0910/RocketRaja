using System;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>Bind the authored caret before TMP_InputField would create a transient one.</summary>
    public sealed class RedesignSearchInput : TMP_InputField
    {
        public CanvasRenderer savedCaret;
        static readonly FieldInfo RendererField = typeof(TMP_InputField).GetField("m_CachedInputRenderer", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo RectField = typeof(TMP_InputField).GetField("caretRectTrans", BindingFlags.Instance | BindingFlags.NonPublic);

        protected override void OnEnable()
        {
            if (Application.isPlaying)
            {
                if (!savedCaret || RendererField == null || RectField == null)
                    throw new InvalidOperationException("Saved search caret/TMP compatibility is missing: " + name);
                RendererField.SetValue(this, savedCaret);
                RectField.SetValue(this, (RectTransform)savedCaret.transform);
            }
            base.OnEnable();
        }
    }
}
