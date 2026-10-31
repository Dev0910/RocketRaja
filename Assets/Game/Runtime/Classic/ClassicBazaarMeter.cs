using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Projects the existing animated fill amount onto a preplaced rounded nine-slice.</summary>
    public sealed class ClassicBazaarMeter : MonoBehaviour
    {
        public Image fill;
        void LateUpdate()
        {
            if(!fill)return;
            float value=Mathf.Clamp01(fill.fillAmount);
            fill.enabled=value>0;
            var rect=fill.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=new Vector2(value,1);rect.offsetMin=Vector2.one;rect.offsetMax=-Vector2.one;
        }
    }
}
