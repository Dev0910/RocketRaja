using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace RocketRaja.Classic
{
    /// <summary>Formats currency in pre-authored TMP slots. It never creates UI or changes balances.</summary>
    public sealed class ClassicCurrencyPresentation : MonoBehaviour
    {
        public TMP_Text[] labels = Array.Empty<TMP_Text>();
        public TMP_SpriteAsset token;
        static readonly Regex Abbreviation = new Regex(@"\bKK\b");
        string[] rendered;
        void LateUpdate() => Apply();
        public void Apply()
        {
            if (rendered == null || rendered.Length != labels.Length) rendered = new string[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var text = labels[i];
                if (!text || text.text == rendered[i]) continue;
                string value = text.text;
                if (value != null && value.Contains("KK"))
                {
                    float size = Mathf.Clamp(text.fontSize * 1.25f, 18, 28);
                    string icon = "<size=" + size.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "><sprite=0></size>";
                    value = Abbreviation.Replace(value, icon);
                    text.text = value;
                }
                rendered[i] = value;
            }
        }
    }
}
