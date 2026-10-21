using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        static void BuildShop(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignShopScreen>();
            var banner = Top("Test shop banner", root, 0, 44); Image(banner, Teal);
            var bannerText = Text(Fill("No real payment", banner, 10, 10, 0, 0), "TEST SHOP · No real payment", 17, true); bannerText.color = Cream; bannerText.alignment = TextAlignmentOptions.Center;
            var body = Fill("Shop body", root, 0, 0, 0, 44); var content = FlowScroll(body, out ui.scroll);
            var balance = Panel(content, "Balance", "Kaju Katli"); ui.balance = FlowText(balance, "shop-balance", "0 KK · One draw costs 100 KK");
            ui.status = FlowText(balance, "shop-status", "Choose a simulated purchase", 13);
            ui.retry = FlowButton(content, "shop-retry-save", "Retry saving"); ui.retry.gameObject.SetActive(false);
            var pending = Panel(content, "Pending purchase", "Test purchase pending"); ui.pendingPanel = pending.gameObject;
            ui.pendingText = FlowText(pending, "pending-purchase-copy", "No KK delivered.", 14);
            ui.delivered = FlowButton(pending, "simulate-delivered", "Simulate success", true);
            var outcomes = FlowPair(pending, "Other test outcomes", "Cancel test", "Simulate failure"); ui.cancelled = outcomes[0]; ui.failed = outcomes[1];
            ui.leavePending = FlowButton(pending, "leave-pending", "Leave pending · Back to Talent"); pending.gameObject.SetActive(false);
            var tests = Panel(content, "Test availability", "Simulate shop availability"); ui.testPanel = tests.gameObject;
            FlowText(tests, "Availability help", "These controls simulate loading and an unavailable shop. No connection or payment is made.", 13);
            var modes = FlowPair(tests, "Test modes", "Loading", "Unavailable"); ui.loading = modes[0]; ui.unavailable = modes[1];
            ui.ready = FlowButton(tests, "test-ready", "Ready · Enable test purchases"); tests.gameObject.SetActive(false);
            ui.simulationStatus = FlowText(content, "shop-availability", "Simulation: ready", 13);
            ui.packNames = new TMP_Text[4]; ui.packAmounts = new TMP_Text[4]; ui.packPrices = new TMP_Text[4]; ui.packs = new Button[4];
            var data = RedesignRuntimeContent.Load(); var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Product gift-boxes.png");
            for (int i = 0; i < 4; i++)
            {
                var pack = Panel(content, "Pack " + data.Packs[i].id, ""); ui.packNames[i] = FlowText(pack, "pack-name-" + i, data.Packs[i].name, 22, true);
                var art = New("Pack illustration", pack); Height(art, 70); var image = Image(art, Color.white); image.sprite = sprite; image.preserveAspect = true;
                ui.packAmounts[i] = FlowText(pack, "pack-amount-" + i, "Exact KK amount and draws", 17);
                ui.packs[i] = FlowButton(pack, "pack-" + i, "Test purchase", true);
                ui.packPrices[i] = FlowText(pack, "pack-price-" + i, "Example price · not charged", 13);
            }
            var odds = Panel(content, "Shared card pool", "Same draw pool for everyone");
            FlowText(odds, "Tier odds", "Common 75% · Rare 22% · Legendary 3%", 14);
            FlowText(odds, "Individual odds", "Each Common: 4.6875% · Each Rare: 2.2% · Each Legendary: 1.5%\nNo purchase guarantees a rarity. Free and KK draws use the same pool.", 13);
            FlowText(odds, "No auto spend", "Buying a pack does not draw cards automatically. Talent is available from the start.", 13);
            ui.testStates = FlowButton(content, "test-shop-states", "Test purchase states");
            ui.back = FlowButton(content, "shop-back", "Back to Talent");
            root.gameObject.SetActive(false);
        }
        public static void AddShop()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var pages = root.transform.Find("Safe Area/Screen States");
                if (pages.Find("screen-kajushop")) throw new InvalidOperationException("Shop already authored; use a scoped revision");
                var page = Fill("screen-kajushop", pages); BuildShop(page); root.GetComponent<RedesignScreenRouter>().shop = page.GetComponent<RedesignShopScreen>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            SerializeTextSlots();
        }
    }
}
