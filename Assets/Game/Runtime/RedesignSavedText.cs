using System;
using TMPro;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>Supplies TMP's material slots from the saved prefab, including inactive pages.</summary>
    public sealed class RedesignSavedText : TextMeshProUGUI
    {
        public TMP_SubMeshUI[] savedMaterialSlots = Array.Empty<TMP_SubMeshUI>();

        public void BindSavedSlots()
        {
            if (savedMaterialSlots.Length == 0) return; // Editor authoring before slots are assigned.
            m_subTextObjects = new TMP_SubMeshUI[savedMaterialSlots.Length + 1];
            for (int i = 0; i < savedMaterialSlots.Length; i++) m_subTextObjects[i + 1] = savedMaterialSlots[i];
        }

        protected override void Awake()
        {
            if (Application.isPlaying && savedMaterialSlots.Length == 0)
                throw new InvalidOperationException("Text material slots were not authored: " + name);
            BindSavedSlots(); base.Awake();
        }

        protected override void OnEnable() { BindSavedSlots(); base.OnEnable(); }
    }
}
