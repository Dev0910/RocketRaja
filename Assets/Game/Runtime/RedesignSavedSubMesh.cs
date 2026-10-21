using TMPro;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>TMP normally marks these transient; the redesign explicitly serializes its slots.</summary>
    public sealed class RedesignSavedSubMesh : TMP_SubMeshUI
    {
        protected override void OnEnable()
        {
            base.OnEnable(); hideFlags = HideFlags.None; gameObject.hideFlags = HideFlags.None;
        }
    }
}
