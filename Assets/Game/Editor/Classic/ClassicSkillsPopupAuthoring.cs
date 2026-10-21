using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Scoped, repeatable Edit Mode repair of the saved production Skills popup.</summary>
    public static class ClassicSkillsPopupAuthoring
    {
        const string Art = "Assets/Game/ClassicUI/HTMLSync/";
        static readonly Color Ink = new Color32(22, 75, 128, 255);
        static readonly Color Cream = new Color32(255, 244, 219, 255);
        static readonly Color Muted = new Color32(82, 112, 139, 255);

        [MenuItem("Rocket Raja/Classic/Refine Skills popup")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(BuildClassicUI.ScenePath);
            if (!scene.isLoaded || scene.isDirty) throw new InvalidOperationException("Open saved Main before refining Skills");
            var ui = scene.GetRootGameObjects().Single(g => g.name == BuildClassicUI.RootName).GetComponent<ClassicUI>();
            var view = ui.dialogs.Single(v => v.route == "Skills");
            Undo.RegisterFullObjectHierarchyUndo(view.root, "Refine Skills popup");
            Undo.RecordObject(ui, "Bind Skills presentation");
            var panel = Surface("skills-list-panel", true);
            var row = Surface("skills-list-row", false);
            var serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/ClassicUI/Fonts/Card Serif SDF.asset");
            var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/ClassicUI/Fonts/Bold SDF.asset");

            var surface = view.window.GetComponent<Image>();
            Skin(surface, panel); surface.fillCenter = true;
            var oldFrame = view.window.Find("Painted gold and cobalt frame");
            if (oldFrame) oldFrame.gameObject.SetActive(false);
            var header = (RectTransform)view.window.Find("Header");
            header.sizeDelta = new Vector2(0, 72);
            header.Find("Rule").gameObject.SetActive(false);
            var title = header.Find("Title").GetComponent<TMP_Text>();
            Box(title.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-100, -42), new Vector2(100, -8));
            Text(title, 27, 22, TextAlignmentOptions.Center, Ink); title.font = serif; title.fontStyle = FontStyles.Bold;
            var subtitle = (RectTransform)header.Find("Subtitle");
            Box(subtitle, Vector2.zero, Vector2.one, new Vector2(12, 5), new Vector2(-12, -48));
            Text(subtitle.Find("Copy").GetComponent<TMP_Text>(), 11, 10, TextAlignmentOptions.Center, Muted);
            var close = (RectTransform)header.Find("Close");
            Box(close, Vector2.one, Vector2.one, new Vector2(-54, -54), new Vector2(-6, -6));
            DecorativeButton(close, ui, 22);

            var body = (RectTransform)view.scroll.transform;
            body.offsetMin = new Vector2(8, 10); body.offsetMax = new Vector2(-8, -74);
            // Opaque viewport backing made the original header/body look like separate blocks.
            foreach (var background in new[] { view.scroll.viewport.GetComponent<Image>(), view.scroll.content.GetComponent<Image>() })
                if (background) background.color = Color.clear;
            var flow = view.scroll.content.GetComponent<VerticalLayoutGroup>();
            flow.padding = new RectOffset(6, 6, 4, 4); flow.spacing = 10;
            for (int i = 0; i < 3; i++)
            {
                var r = (RectTransform)view.scroll.content.Find("Skill " + i);
                var height = r.GetComponent<LayoutElement>(); height.minHeight = height.preferredHeight = 113;
                Skin(r.GetComponent<Image>(), row);
                var portrait = (RectTransform)r.Find("Portrait");
                Box(portrait, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(8, -25.5f), new Vector2(59, 25.5f));
                var name = r.Find("Name").GetComponent<TMP_Text>();
                CopyBox(name.rectTransform, 66, 9, 90, 20);
                Text(name, 14, 11, TextAlignmentOptions.MidlineLeft, Ink); name.font = bold; name.fontStyle = FontStyles.Normal;
                var status = r.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "State");
                var badge = r.Find("State badge") as RectTransform;
                if (!badge)
                {
                    var go = new GameObject("State badge", typeof(RectTransform), typeof(Image));
                    Undo.RegisterCreatedObjectUndo(go, "Author Skills status badge");
                    badge = (RectTransform)go.transform; badge.SetParent(r, false); status.transform.SetParent(badge, false);
                }
                // The saved layout expands the badge with its live READY / ACTIVE / WAIT text.
                Box(badge, new Vector2(0, 1), new Vector2(0, 1), new Vector2(66, -47), new Vector2(102, -32));
                badge.pivot = new Vector2(0, 1); badge.anchoredPosition = new Vector2(66, -32);
                var badgeFlow = badge.GetComponent<HorizontalLayoutGroup>();
                if (!badgeFlow) badgeFlow = badge.gameObject.AddComponent<HorizontalLayoutGroup>();
                badgeFlow.padding = new RectOffset(4, 4, 1, 1); badgeFlow.childControlWidth = badgeFlow.childControlHeight = true;
                badgeFlow.childForceExpandWidth = badgeFlow.childForceExpandHeight = false;
                var badgeFit = badge.GetComponent<ContentSizeFitter>();
                if (!badgeFit) badgeFit = badge.gameObject.AddComponent<ContentSizeFitter>();
                badgeFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                // A sliced Image reports its border as a layout minimum. Keep it on an
                // ignored child so READY stays compact and countdowns can grow naturally.
                badge.GetComponent<Image>().enabled = false;
                var badgeBackground = badge.Find("Background") as RectTransform;
                if (!badgeBackground)
                {
                    var go = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                    Undo.RegisterCreatedObjectUndo(go, "Author Skills badge surface");
                    badgeBackground = (RectTransform)go.transform; badgeBackground.SetParent(badge, false); badgeBackground.SetAsFirstSibling();
                }
                badgeBackground.GetComponent<LayoutElement>().ignoreLayout = true;
                Box(badgeBackground, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                Skin(badgeBackground.GetComponent<Image>(), row); badgeBackground.GetComponent<Image>().raycastTarget = false;
                Box(status.rectTransform, Vector2.zero, Vector2.one, new Vector2(4, 1), new Vector2(-4, -1));
                Text(status, 8, 7, TextAlignmentOptions.MidlineLeft, Ink);
                status.enableAutoSizing = false;
                var effect = r.Find("Effect").GetComponent<TMP_Text>();
                CopyBox(effect.rectTransform, 66, 50, 90, 32); Text(effect, 10, 9, TextAlignmentOptions.TopLeft, Ink);
                var timing = r.Find("Timing").GetComponent<TMP_Text>();
                CopyBox(timing.rectTransform, 66, 86, 9, 17); Text(timing, 8, 7.5f, TextAlignmentOptions.TopLeft, Muted);
                var activate = (RectTransform)r.Find("Activate");
                Box(activate, Vector2.one, Vector2.one, new Vector2(-83, -56), new Vector2(-7, -8));
                DecorativeButton(activate, ui, 11);
                var details = (RectTransform)r.Find("Details");
                Box(details, Vector2.one, Vector2.one, new Vector2(-83, -104), new Vector2(-7, -56));
                var detailLabel = details.GetComponentInChildren<TMP_Text>(true);
                Text(detailLabel, 10, 10, TextAlignmentOptions.Center, Ink); detailLabel.fontStyle = FontStyles.Underline;
                details.GetComponent<ClassicControl>().htmlStyle = 3;
                details.GetComponent<Image>().color = Color.clear;
                var clock = ui.skillClocks.Single(c => c.index == i && c.effect);
                clock.statusBadge = badgeBackground.GetComponent<Image>();
                clock.actionLabel = activate.GetComponentInChildren<TMP_Text>(true);
            }
            var note = view.scroll.content.Find("Prestige note");
            note.GetComponent<LayoutElement>().minHeight = note.GetComponent<LayoutElement>().preferredHeight = 30;
            var copy = note.GetComponentInChildren<TMP_Text>(true);
            copy.text = "Skills reset on prestige. Talent bonuses apply after rebuying.";
            Text(copy, 9, 8.5f, TextAlignmentOptions.Center, Muted);
            // Preserve all live gameplay bindings, motion and dismissal owners.
            ClassicPopupClearanceAuthoring.ApplyView(view);
            EditorUtility.SetDirty(ui);
            Canvas.ForceUpdateCanvases(); ui.GetComponent<ClassicLayout>().Apply();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        static void CopyBox(RectTransform r, float left, float top, float right, float height) =>
            Box(r, new Vector2(0, 1), Vector2.one, new Vector2(left, -top - height), new Vector2(-right, -top));

        static void Box(RectTransform r, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        { r.anchorMin = min; r.anchorMax = max; r.pivot = new Vector2(.5f, .5f); r.offsetMin = low; r.offsetMax = high; }

        static void Text(TMP_Text t, float size, float min, TextAlignmentOptions align, Color color)
        {
            t.fontSize = t.fontSizeMax = size; t.fontSizeMin = min; t.enableAutoSizing = true;
            t.alignment = align; t.color = color; t.margin = Vector4.zero;
            t.overflowMode = TextOverflowModes.Overflow; t.characterSpacing = 0;
        }

        static void DecorativeButton(RectTransform r, ClassicUI ui, float size)
        {
            var face = r.GetComponent<Image>(); face.sprite = ui.blueButton; face.overrideSprite = null;
            face.color = Color.white; face.type = Image.Type.Sliced; face.pixelsPerUnitMultiplier = 3;
            r.GetComponent<ClassicControl>().htmlStyle = 0;
            var label = r.GetComponentInChildren<TMP_Text>(true);
            Text(label, size, size - 1, TextAlignmentOptions.Center, Cream); label.fontStyle = FontStyles.Bold;
        }

        static void Skin(Image image, Sprite sprite)
        { image.sprite = sprite; image.overrideSprite = null; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 4; image.color = Color.white; }

        // High resolution, antialiased nine-slice surfaces; text and controls remain editable.
        static Sprite Surface(string name, bool panel)
        {
            string path = Art + name + ".png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path); if (existing) return existing;
            const int size = 256; const float scale = 4;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float Edge(float x, float y, float inset, float radius)
            {
                var q = new Vector2(Mathf.Abs(x - 128), Mathf.Abs(y - 128)) - Vector2.one * (128 - inset - radius);
                return new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - radius;
            }
            Color gold = new Color32(198, 152, 67, 255), pale = new Color32(220, 193, 136, 255);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float px = x + .5f, py = y + .5f, radius = (panel ? 10 : 7) * scale;
                float d = Edge(px, py, 0, radius); Color c = gold;
                if (panel)
                {
                    if (Edge(px, py, 2 * scale, radius - 2 * scale) < 0) c = Ink;
                    if (Edge(px, py, 4 * scale, radius - 4 * scale) < 0) c = gold;
                    if (Edge(px, py, 5 * scale, radius - 5 * scale) < 0) c = Cream;
                }
                else
                {
                    if (Edge(px, py, scale, radius - scale) < 0) c = Cream;
                    if (Edge(px, py, 3 * scale, radius - 3 * scale) < 0) c = pale;
                    if (Edge(px, py, 3.5f * scale, radius - 3.5f * scale) < 0) c = Cream;
                }
                c.a = Mathf.Clamp01(.5f - d); pixels[y * size + x] = c;
            }
            texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = Vector4.one * 48; importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
