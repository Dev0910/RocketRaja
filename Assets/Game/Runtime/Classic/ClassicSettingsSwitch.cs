using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RocketRaja.Classic
{
    /// <summary>Animates the Editor-placed settings thumb; the existing Setting command owns the value.</summary>
    public sealed class ClassicSettingsSwitch : MonoBehaviour
    {
        public ClassicUI ui;
        public string key;
        public RectTransform thumb, track;
        public TMP_Text onLabel, offLabel;
        public Image surface;
        bool initialized, last;
        float started, from;
        void Update()
        {
            if (!ui || ui.Engine == null) return;
            var s = ui.Engine.State;
            bool value = key == "music" ? s.music : key == "sound" ? s.sound : s.reducedMotion;
            float edge = ui.bazaarTheme ? 15 : 16;
            float target = value ? track.rect.width - edge : edge;
            if (!initialized) { thumb.anchoredPosition = new Vector2(target, 0); initialized = true; last = value; }
            if (value != last) { from = thumb.anchoredPosition.x; started = Time.unscaledTime; last = value; }
            float t = s.reducedMotion ? 1 : Mathf.Clamp01((Time.unscaledTime - started) / .18f);
            if (t >= 1) thumb.anchoredPosition = new Vector2(target, 0);
            else thumb.anchoredPosition = new Vector2(Mathf.Lerp(from, target, t * t * (3 - 2 * t)), 0);
            onLabel.enabled = value; offLabel.enabled = !value;
            surface.color = ui.bazaarTheme ? Color.white : value ? new Color32(24, 83, 130, 255) : new Color32(204, 193, 162, 255);
        }
    }
}
