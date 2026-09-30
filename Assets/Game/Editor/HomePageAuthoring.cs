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
            Place(sign.transform, .29f, .660f, .71f, .756f);
            sign.text = GameState.DefaultStoreName; sign.richText = false; sign.enableAutoSizing = true;
            sign.fontSizeMin = 36; sign.fontSizeMax = 48; sign.textWrappingMode = TextWrappingModes.NoWrap;
            sign.overflowMode = TextOverflowModes.Ellipsis;
            data.FindProperty("counterLabel").objectReferenceValue = sign;
            var inspect = Find(root, "inspect-store-name") as RectTransform;
            if (inspect == null)
            {
                inspect = New("inspect-store-name", sign.transform.parent).GetComponent<RectTransform>();
                var face = Image(inspect, new Color(1, 1, 1, .001f), true);
                var button = inspect.gameObject.AddComponent<Button>();
                button.targetGraphic = face;
                button.transition = Selectable.Transition.None;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
            }
            Place(inspect, .22f, .658f, .78f, .870f);
            data.FindProperty("inspectSignButton").objectReferenceValue = inspect.GetComponent<Button>();
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
            ApplyFeedback(root);
        }
        public static void ApplyFeedback(GameObject root)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name != "inspect-store-name" && button.name != "Next Products Button") continue;
                var feedback = button.GetComponent<UiFeedback>();
                if (feedback == null) feedback = button.gameObject.AddComponent<UiFeedback>();
                var data = new SerializedObject(feedback);
                data.FindProperty("kind").enumValueIndex = (int)UiFeedback.Kind.Button;
                data.FindProperty("target").objectReferenceValue = button.transform;
                data.FindProperty("face").objectReferenceValue = button.targetGraphic;
                var face = button.targetGraphic as Image;
                data.FindProperty("disabledFace").objectReferenceValue = face != null && face.sprite != null && face.sprite.name.StartsWith("Folk ")
                    ? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Folk Sand.png") : null;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public static void ApplySettings(GameObject root)
        {
            Load();
            var lead = Find(root, "Lead Text")?.GetComponent<TMP_Text>();
            if (lead != null) lead.text = "Scroll to account & privacy ↓";

            var scrollBody = Find(root, "Scroll Body");
            var scroll = scrollBody.GetComponent<ScrollRect>();
            var scrollTrack = Find(root, "Settings Scrollbar");
            if (scrollTrack == null)
            {
                var track = New("Settings Scrollbar", scrollBody).GetComponent<RectTransform>();
                track.anchorMin = new Vector2(1, 0);
                track.anchorMax = new Vector2(1, 1);
                track.pivot = new Vector2(1, .5f);
                track.offsetMin = new Vector2(-27, 20);
                track.offsetMax = new Vector2(-9, -20);
                Image(track, new Color32(32, 86, 133, 42));
                var sliding = Stretch(track, "Sliding Area", 2, 2, 3, 3);
                var handle = Fill(sliding, "Handle");
                var handleImage = Image(handle, Hex("205685"));
                var scrollbar = track.gameObject.AddComponent<Scrollbar>();
                scrollbar.direction = Scrollbar.Direction.BottomToTop;
                scrollbar.handleRect = handle;
                scrollbar.targetGraphic = handleImage;
                scrollbar.transition = Selectable.Transition.None;
                scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
                scrollbar.interactable = false;
                scrollTrack = track;
            }
            scroll.verticalScrollbar = scrollTrack.GetComponent<Scrollbar>();
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            var accountStatus = new SerializedObject(root.GetComponent<ImportedSettingsScreen>())
                .FindProperty("accountStatus").objectReferenceValue as TMP_Text;
            if (accountStatus != null)
            {
                var rect = accountStatus.rectTransform;
                if (rect.parent.name == "Google account row")
                {
                    var heading = rect.parent.Find("Account heading") as RectTransform;
                    if (heading != null)
                    {
                        heading.anchoredPosition = new Vector2(heading.anchoredPosition.x, -12);
                        heading.sizeDelta = new Vector2(heading.sizeDelta.x, 70);
                    }
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -82);
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, 128);
                }
                else
                {
                    rect.offsetMin = new Vector2(rect.offsetMin.x, 18);
                    rect.offsetMax = new Vector2(rect.offsetMax.x, -92);
                    var heading = rect.parent.Find("Heading") as RectTransform;
                    if (heading != null)
                    {
                        heading.offsetMin = new Vector2(heading.offsetMin.x, 103);
                        heading.offsetMax = new Vector2(heading.offsetMax.x, -24);
                    }
                }
                accountStatus.enableAutoSizing = true;
                accountStatus.fontSizeMin = 29;
                accountStatus.fontSizeMax = 31;
                accountStatus.richText = false;
            }
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
            var storeRow = button.transform.parent;
            storeRow.GetComponent<LayoutElement>().preferredHeight = 220;
            var storeName = storeRow.Find("Description").GetComponent<TMP_Text>();
            storeName.text = GameState.DefaultStoreName;
            storeName.fontSize = 36;
            storeName.enableAutoSizing = false;
            storeName.textWrappingMode = TextWrappingModes.Normal;
            storeName.overflowMode = TextOverflowModes.Ellipsis;
            storeName.richText = false;
            var storeNameRect = storeName.rectTransform;
            storeNameRect.offsetMin = new Vector2(storeNameRect.offsetMin.x, 14);
            storeNameRect.offsetMax = new Vector2(storeNameRect.offsetMax.x, -85);
            var storeTitle = storeRow.Find("Title") as RectTransform;
            if (storeTitle != null)
            {
                storeTitle.offsetMin = new Vector2(storeTitle.offsetMin.x, 130);
                storeTitle.offsetMax = new Vector2(storeTitle.offsetMax.x, -10);
            }
            var data = new SerializedObject(root.GetComponent<ImportedSettingsScreen>());
            data.FindProperty("renameStoreButton").objectReferenceValue = button;
            data.FindProperty("storeNameLabel").objectReferenceValue = storeName;
            data.ApplyModifiedPropertiesWithoutUndo();
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
