using System;
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
    public static partial class BuildClassicUI
    {
        /// <summary>Repairs only the saved Statistics popup; never called by player startup.</summary>
        [MenuItem("Rocket Raja/Classic/Refine Statistics popup")]
        public static void RefineStatisticsPopup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || scene.isDirty) throw new InvalidOperationException("Open saved Main before refining Statistics");
            ui = scene.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
            var view = ui.dialogs.Single(v => v.route == "Statistics");
            Undo.RegisterFullObjectHierarchyUndo(view.root, "Refine Statistics popup");
            Undo.RecordObject(ui, "Bind individual statistics cells");
            Undo.RecordObject(ui.progression.shop, "Bind decoration summary");
            var layout = ui.GetComponent<ClassicLayout>();
            Undo.RecordObject(layout, "Preserve Statistics row layout");
            body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
            bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
            serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Card Serif SDF.asset");
            var panel = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "skills-list-panel.png");
            var card = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "skills-list-row.png");
            if (!panel || !card) throw new InvalidOperationException("Missing reviewed native popup surfaces");
            var previous = view.window.Find("Previous statistics ledger (inactive)");
            if (!previous)
            {
                previous = New("Previous statistics ledger (inactive)", view.window);
                previous.gameObject.SetActive(false);
                foreach (Transform child in view.scroll.content.Cast<Transform>().ToArray()) child.SetParent(previous, false);
            }
            texts.Clear(); texts.AddRange(ui.textBindings);
            texts.RemoveAll(b => b.key == "stats-run" || b.key == "stats-lifetime" || b.key == "stats-run-values" || b.key == "stats-lifetime-values");

            StatisticsSkin(view.window.GetComponent<Image>(), panel);
            // Keep the existing painted corner flourishes over the smooth native surface.
            var frame = view.window.Find("Painted gold and cobalt frame").GetComponent<Image>();
            frame.fillCenter = false; frame.raycastTarget = false;
            var header = (RectTransform)view.window.Find("Header");
            header.sizeDelta = new Vector2(0, 84);
            var title = header.Find("Title").GetComponent<TMP_Text>();
            StatisticsBox(title.rectTransform, Vector2.zero, Vector2.one, new Vector2(72, 12), new Vector2(-72, -8));
            StatisticsText(title, 25, 22, TextAlignmentOptions.Center, Ink);
            title.font = serif; title.fontStyle = FontStyles.Bold; title.text = "Your shop in\nnumbers";
            var close = (RectTransform)header.Find("Close");
            StatisticsBox(close, Vector2.one, Vector2.one, new Vector2(-70, -70), new Vector2(-22, -22));
            var closeFace = close.GetComponent<Image>(); closeFace.sprite = ui.blueButton;
            closeFace.overrideSprite = null; closeFace.color = Color.white; closeFace.type = Image.Type.Sliced; closeFace.pixelsPerUnitMultiplier = 3;
            close.GetComponent<ClassicControl>().htmlStyle = 0;
            StatisticsText(close.GetComponentInChildren<TMP_Text>(true), 23, 22, TextAlignmentOptions.Center, Cream);
            var rule = (RectTransform)header.Find("Rule"); rule.sizeDelta = new Vector2(-30, .6f);
            rule.GetComponent<Image>().color = new Color(198 / 255f, 152 / 255f, 67 / 255f, .4f);

            var scroll = (RectTransform)view.scroll.transform;
            scroll.offsetMin = new Vector2(10, 68); scroll.offsetMax = new Vector2(-10, -88);
            foreach (var backing in new[] { view.scroll.viewport.GetComponent<Image>(), view.scroll.content.GetComponent<Image>() })
                if (backing) backing.color = Color.clear;
            var flow = view.scroll.content.GetComponent<VerticalLayoutGroup>();
            flow.padding = new RectOffset(6, 6, 4, 4); flow.spacing = 10;
            var run = StatisticsCard(view.scroll.content, "This run", card, Sprite("Coins"), 176);
            StatisticsCells(run, new[] { "Earned this run", "Normal sales", "Manual taps", "Tap payout", "Owned upgrades" },
                new[] { "earned", "sales", "taps", "payout", "upgrades" });
            var lifetime = StatisticsCard(view.scroll.content, "Your whole journey", card, Sprite("Icon Legacy"), 176);
            StatisticsCells(lifetime, new[] { "Prestiges", "Fame", "Discovered upgrades", "Talent cards", "Lifetime earnings" },
                new[] { "prestiges", "fame", "discoveries", "cards", "earned-lifetime" });
            var decoration = StatisticsCard(view.scroll.content, "Your decorations", card, Sprite("Icon Products"), 132);
            var summary = decoration.Find("Decoration summary")?.GetComponent<TMP_Text>();
            if (!summary) summary = Label("", Region("Decoration summary", decoration, 0, 0, 1, 1), "", 11, false, Muted, TextAlignmentOptions.MidlineLeft);
            summary.rectTransform.offsetMin = new Vector2(66, 10); summary.rectTransform.offsetMax = new Vector2(-12, -38);
            StatisticsText(summary, 11, 10, TextAlignmentOptions.MidlineLeft, Muted);
            ui.progression.shop.progress = summary;

            var back = (RectTransform)view.window.Find("Back");
            StatisticsBox(back, Vector2.zero, new Vector2(1, 0), new Vector2(16, 14), new Vector2(-16, 62));
            StatisticsSkin(back.GetComponent<Image>(), card);
            back.GetComponent<ClassicControl>().htmlStyle = 2;
            StatisticsText(back.GetComponentInChildren<TMP_Text>(true), 12, 12, TextAlignmentOptions.Center, Ink);
            layout.infoRows = layout.infoRows.Where(r => !r.layout.transform.IsChildOf(previous)).ToArray();
            ui.textBindings = texts.ToArray();
            ClassicPopupClearanceAuthoring.ApplyView(view);
            EditorUtility.SetDirty(ui); EditorUtility.SetDirty(ui.progression.shop); EditorUtility.SetDirty(layout);
            Canvas.ForceUpdateCanvases(); layout.Apply();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        static RectTransform StatisticsCard(Transform parent, string title, Sprite surface, Sprite icon, float height)
        {
            var card = parent.Find(title) as RectTransform;
            if (card) return card;
            card = New(title, parent); Height(card, height); StatisticsSkin(Face(card, Color.white), surface);
            var headingText = Label("", Region("Heading", card, 0, 1, 1, 1), title, 18, true, Ink, TextAlignmentOptions.Center, serif);
            headingText.rectTransform.offsetMin = new Vector2(10, -34); headingText.rectTransform.offsetMax = new Vector2(-10, -6);
            StatisticsText(headingText, 18, 17, TextAlignmentOptions.Center, Ink); headingText.fontStyle = FontStyles.Bold;
            var artRect = Region("Ledger artwork", card, 0, .5f, 0, .5f);
            artRect.offsetMin = new Vector2(12, -38); artRect.offsetMax = new Vector2(54, 14);
            Picture("", artRect, icon);
            return card;
        }

        static void StatisticsCells(RectTransform card, string[] labels, string[] keys)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                var metric = card.Find(labels[i]) as RectTransform;
                if (!metric) metric = Region(labels[i], card, 0, 1, 1, 1);
                metric.offsetMin = new Vector2(66, -63 - i * 25); metric.offsetMax = new Vector2(-12, -38 - i * 25);
                var caption = metric.Find("Label")?.GetComponent<TMP_Text>();
                if (!caption) caption = Label("", Region("Label", metric, 0, 0, .66f, 1), labels[i], 11);
                StatisticsText(caption, 11, 10.5f, TextAlignmentOptions.MidlineLeft, Ink);
                caption.textWrappingMode = TextWrappingModes.NoWrap;
                var value = metric.Find("Value")?.GetComponent<TMP_Text>();
                if (!value) value = Label("", Region("Value", metric, .66f, 0, 1, 1), "0", 11, true, Ink, TextAlignmentOptions.MidlineRight);
                StatisticsText(value, 11, 9, TextAlignmentOptions.MidlineRight, Ink);
                value.textWrappingMode = TextWrappingModes.NoWrap;
                if (!texts.Any(t => t.key == "stats-" + keys[i])) texts.Add(new ClassicTextBinding { key = "stats-" + keys[i], text = value });
                if (!metric.Find("Rule"))
                {
                    var line = Bottom("Rule", metric, .5f);
                    Face(line, new Color(198 / 255f, 152 / 255f, 67 / 255f, .3f));
                }
            }
        }

        static void StatisticsSkin(Image image, Sprite sprite)
        { image.sprite = sprite; image.overrideSprite = null; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 4; image.color = Color.white; }

        static void StatisticsBox(RectTransform rect, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = new Vector2(.5f, .5f); rect.offsetMin = low; rect.offsetMax = high; }

        static void StatisticsText(TMP_Text text, float size, float minimum, TextAlignmentOptions align, Color color)
        {
            text.fontSize = text.fontSizeMax = size; text.fontSizeMin = minimum; text.enableAutoSizing = true;
            text.alignment = align; text.color = color; text.margin = Vector4.zero; text.overflowMode = TextOverflowModes.Overflow;
        }
    }
}
