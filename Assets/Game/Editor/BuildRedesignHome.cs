using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        static RectTransform Anchored(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = New(name, parent); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        static void BuildHome(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignHomeScreen>(); var content = FlowScroll(root, out ui.scroll);
            var stall = New("Saved fireworks stall", content); Height(stall, 380);
            var background = Image(Fill("Painted stall", stall), Color.white); background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Stall Background.png");
            ui.storeName = Text(Anchored("Store name", stall, new Vector2(.23f, .665f), new Vector2(.77f, .742f)), "Your fireworks stall", 16, true); ui.storeName.color = Cream; ui.storeName.alignment = TextAlignmentOptions.Center;
            ui.products = new Button[8]; ui.productNames = new TMP_Text[8]; ui.productStatus = new TMP_Text[8]; ui.productImages = new Image[8]; ui.productSprites = new Sprite[8];
            ui.lockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Icon Lock.png");
            string[] ids = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };
            for (int i = 0; i < 8; i++)
            {
                float x = .085f + (i % 4) * .209f, y = i < 4 ? .369f : .545f;
                var shelf = Anchored("home-product-" + i, stall, new Vector2(x, 1 - y - .175f), new Vector2(x + .196f, 1 - y));
                var hit = Image(Fill("Shelf touch area", shelf), new Color(1, 1, 1, .001f), true);
                var button = shelf.gameObject.AddComponent<Button>(); button.targetGraphic = hit; button.navigation = new Navigation { mode = Navigation.Mode.None }; ui.products[i] = button;
                ui.productSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Product " + ids[i] + ".png");
                ui.productImages[i] = Image(Top("Product artwork", shelf, 4, 38, 8, 8), Color.white); ui.productImages[i].sprite = ui.productSprites[i]; ui.productImages[i].preserveAspect = true;
                ui.productNames[i] = Text(Top("Product name", shelf, 44, 20), "Product", 11); ui.productNames[i].fontStyle = FontStyles.Bold; ui.productNames[i].alignment = TextAlignmentOptions.Center;
                ui.productStatus[i] = Text(Top("Product level", shelf, 0, 18), "Empty", 10); ui.productStatus[i].color = Cream; ui.productStatus[i].alignment = TextAlignmentOptions.TopRight;
            }
            var counter = Anchored("Manual tap counter", stall, new Vector2(.06f, .105f), new Vector2(.94f, .25f));
            Image(counter, Teal, true, true); ui.counter = counter.gameObject.AddComponent<RedesignTapTarget>(); ui.counter.scroll = ui.scroll;
            ui.counterLabel = Text(Fill("Counter hint", counter, 8, 8, 8, 8), "Tap the counter to earn", 17, true); ui.counterLabel.color = Cream; ui.counterLabel.alignment = TextAlignmentOptions.Center;
            ui.tapPopups = new TMP_Text[6]; ui.tapGroups = new CanvasGroup[6];
            for (int i = 0; i < 6; i++)
            {
                var popup = Anchored("Saved tap popup " + i, counter, new Vector2(.15f, .55f), new Vector2(.85f, 1.15f));
                ui.tapPopups[i] = Text(popup, "+₹0", 19, true); ui.tapPopups[i].alignment = TextAlignmentOptions.Center; ui.tapPopups[i].color = Cream;
                ui.tapGroups[i] = popup.gameObject.AddComponent<CanvasGroup>(); ui.tapGroups[i].alpha = 0; ui.tapGroups[i].blocksRaycasts = false; ui.tapGroups[i].interactable = false;
            }
            var goal = Panel(content, "Next step", ""); ui.goalTitle = FlowText(goal, "home-goal-title", "Start your shop", 20, true); ui.goalCopy = FlowText(goal, "home-goal-copy", "Buy your first Phuljhari manually.", 14);
            ui.firstBuy = FlowButton(content, "home-first-buy", "Buy Phuljhari · ₹30", true); ui.firstBuyLabel = ui.firstBuy.GetComponentInChildren<TMP_Text>();
            ui.goal = FlowButton(content, "home-goal", "Browse cash upgrades", true); ui.goalLabel = ui.goal.GetComponentInChildren<TMP_Text>();
            ui.training = FlowButton(content, "home-training", "₹0.85 per tap · Train taps"); ui.trainingLabel = ui.training.GetComponentInChildren<TMP_Text>();
            var skills = New("Home skills", content); Height(skills, 66); ui.skills = Segment(skills, new[] { "Rush\nView unlock", "Hands\nView unlock", "Order\nView unlock" });
            ui.skillLabels = new TMP_Text[3]; for (int i = 0; i < 3; i++) ui.skillLabels[i] = ui.skills[i].GetComponentInChildren<TMP_Text>();
            ui.wallet = FlowButton(content, "home-wallet", "0 Kaju Katli · Test shop"); ui.walletLabel = ui.wallet.GetComponentInChildren<TMP_Text>();
            root.gameObject.SetActive(false);
        }
        public static void AddHome()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var pages = root.transform.Find("Safe Area/Screen States"); if (pages.Find("screen-home")) throw new InvalidOperationException("Home already authored; use a scoped revision");
                var home = Fill("screen-home", pages); BuildHome(home); var router = root.GetComponent<RedesignScreenRouter>(); router.home = home.GetComponent<RedesignHomeScreen>();
                router.homeTab = root.transform.Find("Safe Area/Navigation/Tabs/Home").GetComponent<Button>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            SerializeTextSlots();
        }
        public static void ReviseHomeLabels()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var home = root.GetComponent<RedesignScreenRouter>().home;
                for (int i = 0; i < 8; i++)
                {
                    home.productNames[i].rectTransform.offsetMin = new Vector2(0, -64) * S; home.productNames[i].rectTransform.offsetMax = new Vector2(0, -44) * S;
                    home.productStatus[i].rectTransform.offsetMin = new Vector2(0, -18) * S; home.productStatus[i].rectTransform.offsetMax = Vector2.zero;
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
