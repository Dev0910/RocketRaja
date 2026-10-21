using System;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Authors the reviewed popup clearance into saved UI. Never runs in a player.</summary>
    public static class ClassicPopupClearanceAuthoring
    {
        const string BootstrapPath = "Assets/Scenes/Bootstrap.unity";

        [MenuItem("Rocket Raja/Classic/Refine all popup close spacing")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var main = SceneManager.GetSceneByPath(BuildClassicUI.ScenePath);
            var bootstrap = SceneManager.GetSceneByPath(BootstrapPath);
            if (!main.isLoaded || !bootstrap.isLoaded || main.isDirty || bootstrap.isDirty)
                throw new InvalidOperationException("Open saved Main and Bootstrap before refining popup clearance");
            var ui = main.GetRootGameObjects().Single(g => g.name == BuildClassicUI.RootName).GetComponent<ClassicUI>();
            foreach (var view in ui.dialogs) ApplyView(view);
            var root = bootstrap.GetRootGameObjects().Single(g => g.name == "Bootstrap Canvas");
            ApplyBootstrap((RectTransform)root.transform.Find("Safe Area/Ivory Status Panel"));
            Canvas.ForceUpdateCanvases();
            ui.GetComponent<ClassicLayout>().Apply();
            EditorSceneManager.MarkSceneDirty(main); EditorSceneManager.SaveScene(main);
            EditorSceneManager.MarkSceneDirty(bootstrap); EditorSceneManager.SaveScene(bootstrap);
        }

        // Called last by scoped builders so a later art refinement retains the shared clearance.
        public static void ApplyView(ClassicView view)
        {
            if (view.route == "SaveRecovery" || view.route == "RecoveryExit") return;
            var header = view.window.Find("Header") as RectTransform;
            var close = (header ? header.Find("Close") : view.window.Find("Close")) as RectTransform;
            if (!close) throw new InvalidOperationException("Missing saved close: " + view.route);
            Undo.RecordObject(close, "Inset popup close");
            float inset = view.route == "CardDetail" ? 36 : 22;
            Box(close, Vector2.one, Vector2.one, new Vector2(-inset - 48, -inset - 48), new Vector2(-inset, -inset));
            if (view.route == "CardDetail")
            {
                // Reserve a fixed top band inside all three rarity frames. Keep the artwork bottom.
                ShiftCard((RectTransform)view.window.Find("Card name"), false);
                ShiftCard((RectTransform)view.window.Find("Level badge"), false);
                ShiftCard((RectTransform)view.window.Find("Rarity"), false);
                ShiftCard((RectTransform)view.window.Find("Illustration"), true);
                return;
            }
            bool skills = view.route == "Skills";
            float height = skills ? 100 : view.route == "Statistics" ? 96 : 88;
            Undo.RecordObject(header, "Reserve popup header");
            header.sizeDelta = new Vector2(header.sizeDelta.x, height);
            var title = header.Find("Title").GetComponent<TMP_Text>();
            Undo.RecordObject(title, "Reserve popup title gutter");
            Undo.RecordObject(title.rectTransform, "Reserve popup title gutter");
            Box(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(74, skills ? 36 : 12), new Vector2(-74, -12));
            title.margin = Vector4.zero; title.alignment = TextAlignmentOptions.Center;
            title.textWrappingMode = TextWrappingModes.Normal; title.enableAutoSizing = true;
            if (skills)
            {
                var subtitle = (RectTransform)header.Find("Subtitle");
                Undo.RecordObject(subtitle, "Reserve Skills subtitle");
                Box(subtitle, Vector2.zero, Vector2.one, new Vector2(12, 5), new Vector2(-12, -70));
            }
            var body = (RectTransform)view.scroll.transform;
            Undo.RecordObject(body, "Keep popup body below header");
            body.offsetMax = new Vector2(body.offsetMax.x, -height - (view.route == "Statistics" ? 4 : 2));
        }

        public static void ApplyBootstrap(RectTransform panel)
        {
            const float scale = 3; // Bootstrap's saved 1080 reference uses 3x Main's logical units.
            var close = (RectTransform)panel.Find("Close popup");
            Undo.RecordObject(close, "Inset startup popup close");
            Box(close, Vector2.one, Vector2.one, new Vector2(-70, -70) * scale, new Vector2(-22, -22) * scale);
            var scroll = (RectTransform)panel.Find("Popup scroll");
            Undo.RecordObject(scroll, "Reserve startup popup header");
            scroll.offsetMax = new Vector2(scroll.offsetMax.x, -88 * scale);
            var sizer = panel.GetComponent<BootstrapPopupSize>();
            if (!sizer) sizer = Undo.AddComponent<BootstrapPopupSize>(panel.gameObject);
            Undo.RecordObject(sizer, "Size startup popup including close clearance");
            sizer.content = (RectTransform)scroll.Find("Viewport/Content");
            sizer.padding = 312; // Existing 36 bottom inset + 264 top band + 12 layout slack.
        }

        static void ShiftCard(RectTransform rect, bool keepBottom)
        {
            Undo.RecordObject(rect, "Reserve talent card close band");
            // Set absolute offsets, making this operation repeatable after other card authoring.
            rect.offsetMin = new Vector2(rect.offsetMin.x, keepBottom ? 0 : -52);
            rect.offsetMax = new Vector2(rect.offsetMax.x, -52);
        }

        static void Box(RectTransform rect, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = low; rect.offsetMax = high;
        }
    }
}
