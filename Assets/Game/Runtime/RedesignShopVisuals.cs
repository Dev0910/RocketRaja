using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;

namespace RocketRaja
{
    public sealed class RedesignShopVisuals : MonoBehaviour
    {
        public GameObject[] layers;
        public TMP_Text progress;
        public int DisplayedStage { get; private set; } = -1;
        public static readonly string[] Names = { "Opening stall", "Marigold welcome", "Lantern glow", "Embroidered counter", "Festival bunting", "Raja crest" };
        public void Refresh(RedesignEngine engine)
        {
            if (layers == null || layers.Length != 5 || !progress) throw new InvalidOperationException("Incomplete saved shop progression");
            int stage = engine.ShopAppearance;
            for (int i = 0; i < layers.Length; i++) if (layers[i].activeSelf != (i < stage)) layers[i].SetActive(i < stage);
            DisplayedStage = stage;
            progress.text = "Shop " + (stage + 1) + "/6 · " + Names[stage] + "\n" +
                (stage == 5 ? "All shop decorations earned." : engine.State.UniqueCount + " unique purchases · Next: " + Names[stage + 1] + " at " + RedesignShopProgress.Threshold(stage + 1)) +
                "\nDecorations stay after Prestige. Product displays rebuild.";
        }
    }
}
