using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        static Sprite[] CardSprites()
        {
            string[] ids = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };
            var sprites = new Sprite[28];
            for (int i = 0; i < 28; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/RedesignUI/Product Sprites/" + ids[i % 8] + "-0.asset");
                if (!sprites[i]) throw new InvalidOperationException("Missing card illustration");
            }
            return sprites;
        }
        static void ApplyTalentArt(GameObject root)
        {
            var router = root.GetComponent<RedesignScreenRouter>(); var sprites = CardSprites();
            for (int i = 0; i < sprites.Length; i++) router.talent.cards[i].transform.Find("Artwork").GetComponent<Image>().sprite = sprites[i];
            router.cardDetails.cardSprites = sprites; router.reveal.cardSprites = (Sprite[])sprites.Clone();
            router.cardDetails.artwork.sprite = sprites[0]; router.reveal.artwork.sprite = sprites[0];
        }
        public static void ReviseTalentArt()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try { ApplyTalentArt(root); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        static void BuildTalent(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignTalentScreen>();
            var upper = Fill("Collection area", root, 0, 0, 126, 0); var content = FlowScroll(upper, out ui.scroll);
            var balances = Panel(content, "Balances", "Permanent collection"); ui.balances = FlowText(balances, "talent-balances", "0 free draws · 0 Kaju Katli");
            ui.status = FlowText(balances, "talent-status", "Cards apply automatically when relevant.", 13);
            var locked = Panel(content, "Talent locked", "Grow your Fame first"); ui.lockedPanel = locked.gameObject;
            FlowText(locked, "Unlock instructions", "Talent unlocks after your first prestige. You will receive 3 free draws.", 14);
            ui.prestige = FlowButton(locked, "talent-to-prestige", "View Prestige");
            FlowText(content, "Tier odds", "Common 75% · Rare 22% · Legendary 3%", 13);
            ui.odds = FlowButton(content, "talent-odds", "View exact draw odds");
            var odds = Panel(content, "Exact draw odds", "Each card in its tier has equal odds"); ui.oddsPanel = odds.gameObject;
            ui.oddsText = FlowText(odds, "Odds", "Common 4.6875% · Rare 2.2% · Legendary 1.5%", 13); odds.gameObject.SetActive(false);
            ui.resume = FlowButton(content, "resume-draw", "View saved card result", true); ui.resume.gameObject.SetActive(false);
            var grid = New("talent-cards", content); var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(176, 164) * S; layout.spacing = new Vector2(10, 10) * S;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 2; layout.childAlignment = TextAnchor.UpperCenter;
            ui.cards = new Button[28]; ui.cardLabels = new TMP_Text[28]; var sprites = CardSprites();
            var data = RedesignRuntimeContent.Load();
            for (int i = 0; i < 28; i++)
            {
                var rect = New("card-" + data.Cards[i].id, grid); ui.cards[i] = Button(rect, "");
                rect.GetComponent<Image>().color = data.Cards[i].tier == Core.CardTier.Common ? Hex("647277") : data.Cards[i].tier == Core.CardTier.Rare ? Hex("247E9C") : Hex("BD842D");
                var artwork = Image(Top("Artwork", rect, 8, 70, 10, 10), Color.white); artwork.sprite = sprites[i]; artwork.preserveAspect = true;
                var label = ui.cards[i].GetComponentInChildren<TMP_Text>();
                label.rectTransform.anchorMin = new Vector2(0, 1); label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = new Vector2(8, -156) * S; label.rectTransform.offsetMax = new Vector2(-8, -82) * S;
                label.fontSize = 13 * S; label.text = data.Cards[i].name + "\n" + data.Cards[i].tier + " · Unowned"; ui.cardLabels[i] = label;
            }
            var actions = Fill("Draw actions", root, 14, 14, 14, 0); actions.anchorMax = new Vector2(1, 0); actions.offsetMax = new Vector2(-14 * S, 118 * S);
            ui.freeDraw = Button(Top("free-draw", actions, 0, 52), "Use 1 free draw · 0 available", true); ui.freeLabel = ui.freeDraw.GetComponentInChildren<TMP_Text>();
            var pair = Segment(Top("KK actions", actions, 60, 52), new[] { "Draw · 100 KK", "KK shop" }); ui.paidDraw = pair[0]; ui.shop = pair[1];
            root.gameObject.SetActive(false);
        }
        static void BuildCardDetails(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignCardDetailsScreen>(); var content = FlowScroll(root, out ui.scroll);
            var card = Panel(content, "Card identity", ""); ui.itemName = FlowText(card, "card-name", "Card name", 22, true);
            var art = New("Card artwork", card); Height(art, 100); ui.artwork = Image(art, Color.white); ui.artwork.preserveAspect = true; ui.cardSprites = CardSprites();
            ui.status = FlowText(card, "card-status", "Common · Unowned");
            var effect = Panel(content, "Card effect", "Every copy adds one level"); ui.effect = FlowText(effect, "card-effect", "Current → next effect");
            var applies = Panel(content, "Application", "How this card works"); ui.applies = FlowText(applies, "card-applies", "Permanent; applies automatically");
            var odds = Panel(content, "Card draw odds", "Draw odds"); ui.odds = FlowText(odds, "card-odds", "Exact chance per draw", 14);
            ui.exact = FlowButton(content, "card-exact", "Show exact effect"); ui.exactLabel = ui.exact.GetComponentInChildren<TMP_Text>();
            ui.back = FlowButton(content, "card-back", "Back to Talent"); root.gameObject.SetActive(false);
        }
        static void BuildReveal(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignRevealScreen>(); var content = FlowScroll(root, out _);
            var card = Panel(content, "Saved card result", ""); ui.cardFace = card; ui.cardGroup = card.gameObject.AddComponent<CanvasGroup>();
            ui.itemName = FlowText(card, "revealed-card", "Your saved card", 24, true);
            var art = New("Result artwork", card); Height(art, 145); ui.artwork = Image(art, Color.white); ui.artwork.preserveAspect = true; ui.cardSprites = CardSprites();
            ui.outcome = FlowText(card, "draw-outcome", "New card · Level 0 → 1", 18);
            ui.effect = FlowText(card, "draw-effect", "Before → after effect");
            ui.status = FlowText(content, "saved-result-status", "Saved to your collection", 14);
            ui.skip = FlowButton(content, "skip-reveal", "Skip animation");
            ui.details = FlowButton(content, "reveal-details", "View card details");
            ui.done = FlowButton(content, "reveal-done", "Continue to Talent", true); root.gameObject.SetActive(false);
        }
        static void BuildPrestige(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignPrestigeScreen>(); var content = FlowScroll(root, out ui.scroll);
            var fame = Panel(content, "Fame", "Guaranteed permanent income"); ui.fame = FlowText(fame, "prestige-fame", "Fame 0 → 1\nFame income multiplier ×1 → ×6");
            var credits = Panel(content, "Draw credits", "Free Talent draws"); ui.credits = FlowText(credits, "prestige-credits", "3 free draws");
            var reset = Panel(content, "Reset and keep", "Reset / keep");
            FlowText(reset, "Reset", "RESET: cash, product and tap levels, cash upgrades, skill ownership/effects and run earnings.", 14);
            FlowText(reset, "Keep", "KEEP: Fame, cards, KK, unused credits, lifetime first purchases, settings/name, learned topics and earned decorations.", 14);
            var rebuild = Panel(content, "Rebuild", "Rebuild with permanent gains"); ui.rebuild = FlowText(rebuild, "prestige-rebuild", "A manual first purchase starts the new run.", 14);
            var milestones = Panel(content, "Milestones", "Targets and next rewards"); ui.thresholds = FlowText(milestones, "prestige-next", "Current and next run targets", 14);
            ui.status = FlowText(content, "prestige-status", "Review your rewards before resetting.", 13);
            ui.pendingCard = FlowButton(content, "prestige-pending-card", "View saved card result"); ui.pendingCard.gameObject.SetActive(false);
            ui.review = FlowButton(content, "prestige-review", "Refresh reward review");
            ui.exact = FlowButton(content, "prestige-exact", "Show full precision"); ui.exactLabel = ui.exact.GetComponentInChildren<TMP_Text>();
            var pair = FlowPair(content, "Prestige actions", "Continue run", "Confirm prestige"); ui.stay = pair[0]; ui.confirm = pair[1]; ui.confirmLabel = ui.confirm.GetComponentInChildren<TMP_Text>();
            ui.confirm.targetGraphic.color = Teal; ui.confirmLabel.color = Cream; root.gameObject.SetActive(false);
        }
        public static void AddTalentAndPrestige()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var pages = root.transform.Find("Safe Area/Screen States");
                if (pages.Find("screen-talent") || pages.Find("screen-prestige")) throw new InvalidOperationException("Talent/Prestige already authored; use a scoped revision");
                BuildTalent(Fill("screen-talent", pages)); BuildCardDetails(Fill("screen-carddetails", pages)); BuildReveal(Fill("screen-drawreveal", pages)); BuildPrestige(Fill("screen-prestige", pages));
                var router = root.GetComponent<RedesignScreenRouter>();
                router.talent = pages.Find("screen-talent").GetComponent<RedesignTalentScreen>(); router.cardDetails = pages.Find("screen-carddetails").GetComponent<RedesignCardDetailsScreen>();
                router.reveal = pages.Find("screen-drawreveal").GetComponent<RedesignRevealScreen>(); router.prestige = pages.Find("screen-prestige").GetComponent<RedesignPrestigeScreen>();
                router.talentTab = root.transform.Find("Safe Area/Navigation/Tabs/Talent").GetComponent<Button>(); router.prestigeTab = root.transform.Find("Safe Area/Navigation/Tabs/Prestige").GetComponent<Button>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        public static void ReviseTalentBorders()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var ui = root.GetComponentInChildren<RedesignTalentScreen>(true); var data = RedesignRuntimeContent.Load();
                for (int i = 0; i < 28; i++) ui.cards[i].GetComponent<Image>().color = data.Cards[i].tier == Core.CardTier.Common ? Hex("647277") : data.Cards[i].tier == Core.CardTier.Rare ? Hex("247E9C") : Hex("BD842D");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
