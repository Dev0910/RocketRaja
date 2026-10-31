using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Editor-authored presentation values, exported from the qualified HTML tokens.</summary>
    public sealed class ClassicBazaarTheme : ScriptableObject
    {
        public string revision, tokenHash;
        public TMP_FontAsset heading, body, bold;
        public Color paper, raised, shade, ink, muted, gold, goldLight, line, track, progress, success, danger, disabledInk;
        public Sprite window, section, shadeSurface, barTrack, barFill, switchTrack, switchThumb, information, paperTexture;
        public Sprite[] circles, rarity;
        public Sprite primary, secondary, unavailable, destructive;
        public Sprite Circle(float size)
        {
            int i=size<40?0:size<46?1:size<60?2:size<80?3:4;
            return circles[i];
        }
    }

    [Serializable] public sealed class ClassicBazaarButton
    {
        public Button button;
        public ClassicControl control;
        public TMP_Text[] labels;
    }
}
