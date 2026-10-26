using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Projects committed progress onto existing Classic images and five saved decoration layers.</summary>
    public sealed class ClassicProgression : MonoBehaviour
    {
        public ClassicUI ui;
        public RedesignProductArt appearances;
        public Image[] shelves, rows;
        public RedesignShopVisuals shop;
        public TMP_Text summary;
        public GameObject counterSlogan;
        int stage = -1;
        int[] looks = new int[8];
        public Sprite ProductSprite(int product) => ui.Engine.ProductAppearance(product) <= 0 ? ui.productArt[product] : appearances.At(product, ui.Engine.ProductAppearance(product));
        public void Refresh()
        {
            if (!ui.driver.Initialized) return;
            bool first = stage < 0;
            for (int i = 0; i < 8; i++)
            {
                int look = ui.Engine.ProductAppearance(i);
                shelves[i].sprite = rows[i].sprite = ProductSprite(i);
                if (!first && look > looks[i]) ui.motion.Pulse(shelves[i].rectTransform);
                looks[i] = look;
            }
            shop.Refresh(ui.Engine);
            stage = shop.DisplayedStage;
            if(counterSlogan)counterSlogan.SetActive(stage<3);
            summary.text = "TAP TO EARN";
        }
    }
}
