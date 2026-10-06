using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        static void BuildLevels(RectTransform root, bool tap)
        {
            var ui = root.gameObject.AddComponent<RedesignLevelsScreen>();
            var content = FlowScroll(root, out ui.scroll);
            var identity = Panel(content, "Identity", ""); ui.itemName = FlowText(identity, "level-name", tap ? "A more valuable tap" : "Phuljhari", 22, true);
            ui.status = FlowText(identity, "level-status", "Level 0", 13);
            var artwork = New("Product artwork", identity); Height(artwork, 100);
            ui.productImage = Image(artwork, Color.white); ui.productImage.preserveAspect = true;
            string[] ids = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };
            ui.productSprites = new Sprite[8];
            for (int i = 0; i < ids.Length; i++)
            {
                ui.productSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Product " + ids[i] + ".png");
                if (!ui.productSprites[i]) throw new InvalidOperationException("Missing product sprite: " + ids[i]);
            }
            ui.productImage.sprite = ui.productSprites[0]; artwork.gameObject.SetActive(!tap);
            ui.projection = FlowText(identity, "level-projection", "Normal income and level projection");
            ui.requirement = FlowButton(identity, "required-product", "View previous product"); ui.requirementLabel = ui.requirement.GetComponentInChildren<TMP_Text>(); ui.requirement.gameObject.SetActive(false);
            var cost = Panel(content, "Level price", tap ? "Train your taps" : "Buy product levels"); ui.price = FlowText(cost, "level-price", "Next level quote", 14);
            var quantity = New("Quantity", content); Height(quantity, 52); ui.quantities = Segment(quantity, new[] { "x1", "x10", "x100", "Max" });
            ui.buy = FlowButton(content, "buy-levels", "Buy 1 level", true); ui.buyLabel = ui.buy.GetComponentInChildren<TMP_Text>();
            ui.exact = FlowButton(content, "level-exact", "Show full precision"); ui.exactLabel = ui.exact.GetComponentInChildren<TMP_Text>();
            ui.upgrades = FlowButton(content, "level-upgrades", "View cash upgrades"); ui.upgradesLabel = ui.upgrades.GetComponentInChildren<TMP_Text>();
            ui.back = FlowButton(content, "level-back", "Back to previous screen"); root.gameObject.SetActive(false);
        }
        public static void AddLevelScreens()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var pages = root.transform.Find("Safe Area/Screen States");
                if (pages.Find("screen-productdetails") || pages.Find("screen-taptraining")) throw new InvalidOperationException("Level screens already exist; use a scoped revision");
                BuildLevels(Fill("screen-productdetails", pages), false); BuildLevels(Fill("screen-taptraining", pages), true);
                var router = root.GetComponent<RedesignScreenRouter>();
                router.productDetails = pages.Find("screen-productdetails").GetComponent<RedesignLevelsScreen>();
                router.tapTraining = pages.Find("screen-taptraining").GetComponent<RedesignLevelsScreen>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
