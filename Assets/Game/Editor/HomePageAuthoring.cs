using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static RocketRaja.EditorTools.ImportedUiKit;

namespace RocketRaja.EditorTools
{
    /// <summary>Shared authoring adjustments for saved Home and Settings and their builders.</summary>
    public static class HomePageAuthoring
    {
        static Transform Find(GameObject root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == name);
        static void Remove(GameObject root, string name) { var t = Find(root, name); if (t != null) Object.DestroyImmediate(t.gameObject); }
        static void Place(Transform t, float x0, float y0, float x1, float y1)
        {
            var r = (RectTransform)t; r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        public static void ApplyHome(GameObject root)
        {
            Load();
            Remove(root, "Next Display"); Remove(root, "home-normal"); Remove(root, "home-run"); Remove(root, "Counter Message");
            var presenter = root.GetComponent<ImportedHomeScreen>();
            var data = new SerializedObject(presenter);
            var sign = Find(root, "Shop Sign").GetComponent<TMP_Text>();
            Place(sign.transform, .32f, .672f, .68f, .744f);
            sign.text = GameState.DefaultStoreName; sign.richText = false; sign.enableAutoSizing = true;
            sign.fontSizeMin = 20; sign.fontSizeMax = 48; sign.textWrappingMode = TextWrappingModes.NoWrap;
            data.FindProperty("counterLabel").objectReferenceValue = sign;
            var fit = new SerializedObject(root.GetComponent<ImportedPanelFit>());
            fit.FindProperty("counterMessage").objectReferenceValue = null; fit.ApplyModifiedPropertiesWithoutUndo();
            var hint = Find(root, "Tap Hint"); Place(hint, .32f, .035f, .68f, .085f);
            var hintText = hint.GetComponentInChildren<TMP_Text>(); hintText.text = "Tap to earn"; hintText.fontSize = 32;
            var income = Find(root, "Income"); income.GetComponent<LayoutElement>().preferredHeight = 185;
            Place(Find(root, "Income Kicker"), .13f, .67f, .62f, .94f);
            Place(Find(root, "Coins"), .02f, .18f, .11f, .65f);
            Place(Find(root, "home-income"), .13f, .1f, .54f, .68f);
            Place(Find(root, "Per Second"), .54f, .12f, .63f, .57f);
            Place(Find(root, "Tap Kicker"), .68f, .67f, .97f, .94f);
            Place(Find(root, "Tap Icon"), .67f, .19f, .75f, .59f);
            Place(Find(root, "home-tap"), .76f, .1f, .98f, .67f);
            Place(Find(root, "Vertical Divider"), .65f, .12f, .653f, .91f);
            foreach (string name in new[] { "home-income", "home-tap" })
            {
                var text = Find(root, name).GetComponent<TMP_Text>(); text.enableAutoSizing = true;
                text.fontSizeMin = 22; text.fontSizeMax = name == "home-tap" ? 60 : 72;
                text.textWrappingMode = TextWrappingModes.NoWrap;
            }
            var sponsor = Find(root, "Sponsor");
            data.FindProperty("sponsorLayout").objectReferenceValue = sponsor.GetComponent<LayoutElement>();
            Remove(root, "Sponsor Caption"); Place(Find(root, "sponsor-status"), .12f, .1f, .98f, .9f);
            var sponsorText = Find(root, "sponsor-status").GetComponent<TMP_Text>(); sponsorText.enableAutoSizing = true;
            sponsorText.fontSizeMin = 26; sponsorText.fontSizeMax = 38;
            data.ApplyModifiedPropertiesWithoutUndo();
            // Keep pooled rewards legible against the bright illustration with a saved outline material.
            const string materialPath = "Assets/Game/ImportedUI/Tap Reward Material.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            var popupText = Find(root, "Cash Popup 0").GetComponentInChildren<TMP_Text>(true);
            if (material == null)
            {
                material = new Material(popupText.fontSharedMaterial) { name = "Tap Reward Material" };
                material.SetColor("_OutlineColor", new Color32(17, 32, 58, 255)); material.SetFloat("_OutlineWidth", .22f);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => x.name.StartsWith("Cash Popup ")))
            {
                var rect = (RectTransform)t; rect.sizeDelta = new Vector2(440, 100);
                rect.pivot = new Vector2(.5f, 1);
                var text = t.GetComponentInChildren<TMP_Text>(true); text.fontSharedMaterial = material; text.color = Color.white;
                text.alignment = TextAlignmentOptions.Top;
                text.enableAutoSizing = true; text.fontSizeMin = 30; text.fontSizeMax = 72;
                text.textWrappingMode = TextWrappingModes.NoWrap; text.margin = new Vector4(12, 8, 12, 8);
            }
        }
        public static void ApplySettings(GameObject root)
        {
            Load();
            var existing = Find(root, "settings-rename-store");
            Button button;
            if (existing == null)
            {
                var content = Find(root, "Content");
                var row = FramedCard(content, "Store name", 190);
                Text(Stretch(row, "Title", 28, 310, 88, 20), "Store name", 38, Hex("164B80"), TextAlignmentOptions.MidlineLeft, true);
                Text(Stretch(row, "Description", 28, 310, 20, 92), "Make your shop your own", 28, Hex("375A77"), TextAlignmentOptions.MidlineLeft);
                button = Button(Stretch(row, "settings-rename-store", 660, 20, 22, 22), Blue);
                Text(Fill(button.transform, "Label"), "Rename", 32, Color.white, TextAlignmentOptions.Center, true);
                row.SetSiblingIndex(4);
            }
            else button = existing.GetComponent<Button>();
            var data = new SerializedObject(root.GetComponent<ImportedSettingsScreen>());
            data.FindProperty("renameStoreButton").objectReferenceValue = button; data.ApplyModifiedPropertiesWithoutUndo();
            if (button.GetComponent<UiFeedback>() == null)
            {
                var feedback = button.gameObject.AddComponent<UiFeedback>(); var f = new SerializedObject(feedback);
                f.FindProperty("target").objectReferenceValue = button.transform;
                f.FindProperty("face").objectReferenceValue = button.targetGraphic;
                f.FindProperty("disabledFace").objectReferenceValue = Sand; f.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
