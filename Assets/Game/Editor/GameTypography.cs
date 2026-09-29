using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace RocketRaja.EditorTools
{
    /// <summary>Editor-authored typography matching the approved HTML font roles.</summary>
    public static class GameTypography
    {
        public const string BodyPath = "Assets/Game/ImportedUI/Nunito Medium SDF.asset";
        const string BoldPath = "Assets/Game/ImportedUI/Nunito ExtraBold SDF.asset";
        const string HeadingPath = "Assets/Game/ImportedUI/Baloo 2 Bold SDF.asset";
        static TMP_FontAsset body, heading;

        public static TMP_FontAsset Body => body != null ? body : body = Require(BodyPath);
        static TMP_FontAsset Heading => heading != null ? heading : heading = Require(HeadingPath);

        static TMP_FontAsset Require(string path) => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path)
            ?? throw new InvalidOperationException("Run GameTypography.ApplyToSavedScreens first: " + path);

        public static bool IsHeading(TMP_Text text)
        {
            var parent = text.transform.parent;
            return text.name == "Shop Sign"
                || (parent != null && parent.name == "Header" &&
                    (text.name == "Title" || text.name.EndsWith(" Title", StringComparison.Ordinal) || text.name == "detail-name"))
                || (parent != null && parent.name == "Unlockables Heading" && text.name == "Title")
                || (parent != null && (parent.name == "THIS RUN Heading" || parent.name == "LIFETIME Heading") && text.name == "Label")
                || (parent != null && parent.name.StartsWith("nav-", StringComparison.Ordinal));
        }

        public static void Configure(TMP_Text text)
        {
            bool display = IsHeading(text);
            text.font = display ? Heading : Body;
            text.fontSharedMaterial = text.font.material;
            // Baloo is already a true 700 face. Nunito uses the real 800 face via its weight table.
            if (display) text.fontStyle &= ~FontStyles.Bold;
            var parent = text.transform.parent;
            // Preserve the font sizes while accommodating the new faces' vertical metrics.
            if (parent != null && parent.name == "Unlockables Heading" && text.name == "Title")
            {
                var layout = parent.GetComponent<UnityEngine.UI.LayoutElement>();
                if (layout != null) layout.preferredHeight = 117;
            }
            if (parent != null && text.name == "Heading" &&
                (parent.name == "Prestige Keep" || parent.name == "prestige-reset"))
            {
                var rect = text.rectTransform;
                rect.offsetMin = new Vector2(rect.offsetMin.x, 176);
                rect.offsetMax = new Vector2(rect.offsetMax.x, -16);
            }
        }

        static TMP_FontAsset Create(string name, string sourceFile, string assetPath, string characters)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (font != null) return font;
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/" + sourceFile + ".ttf");
            if (source == null) throw new InvalidOperationException("Missing font source: " + sourceFile);
            font = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
            font.name = name;
            font.isMultiAtlasTexturesEnabled = false;
            AssetDatabase.CreateAsset(font, assetPath);
            if (!font.TryAddCharacters(characters, out string missing))
                throw new InvalidOperationException(name + " cannot render: " + missing);
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var texture in font.atlasTextures)
                if (texture != null && !EditorUtility.IsPersistent(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssetIfDirty(font);
            return font;
        }

        [MenuItem("Tools/Rocket Raja/Apply Baloo 2 and Nunito")]
        public static void ApplyToSavedScreens()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            string[] paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/ImportedUI" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p.EndsWith(" Screen.prefab", StringComparison.Ordinal)).OrderBy(p => p).ToArray();
            if (paths.Length != 13) throw new InvalidOperationException("Expected all 13 game screen prefabs.");
            string content = string.Concat(paths.SelectMany(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)
                .GetComponentsInChildren<TMP_Text>(true)).Select(t => t.text));
            // Include runtime figures, ordinary punctuation and typographic characters used by the UI.
            string chars = new string((string.Concat(Enumerable.Range(32, 95).Select(n => (char)n)) +
                "₹×·–—‘’“”…" + content).Where(c => !char.IsControl(c)).Distinct().OrderBy(c => c).ToArray());
            body = Create("Nunito Medium SDF", "Nunito-Medium", BodyPath, chars);
            var bold = Create("Nunito ExtraBold SDF", "Nunito-ExtraBold", BoldPath, chars);
            heading = Create("Baloo 2 Bold SDF", "Baloo2-Bold", HeadingPath, chars);
            body.fontWeightTable[7].regularTypeface = bold;
            body.fontWeightTable[8].regularTypeface = bold;
            body.fontWeightTable[9].regularTypeface = bold;
            EditorUtility.SetDirty(body);
            AssetDatabase.SaveAssetIfDirty(body);
            int total = 0, headings = 0;
            foreach (string path in paths)
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        Undo.RecordObject(text, "Apply approved game typography");
                        Configure(text);
                        total++;
                        if (IsHeading(text)) headings++;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            Debug.Log($"Typography saved: {paths.Length} prefabs, {total} labels, {headings} Baloo headings, {total - headings} Nunito labels. Open scenes were not saved.");
        }
    }
}
