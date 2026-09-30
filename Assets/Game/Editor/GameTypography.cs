using System;
using System.Linq;
using RocketRaja.Core;
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
        const string UiArrowCharacters = "\u2190\u2193";
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
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/" + sourceFile + ".ttf");
            if (source == null) throw new InvalidOperationException("Missing font source: " + sourceFile);
            if (font != null)
            {
                EnsureCharacters(font, source, characters);
                return font;
            }
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

        static void EnsureCharacters(TMP_FontAsset font, Font source, string characters)
        {
            string needed = new string(characters.Where(c => !font.HasCharacter(c)).Distinct().ToArray());
            if (needed.Length == 0) return;
            try
            {
                // The saved assets are static for builds. Temporarily restore the local
                // source face so TMP can add glyphs to their existing atlas/subassets.
                font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                var serialized = new SerializedObject(font);
                serialized.FindProperty("m_SourceFontFile").objectReferenceValue = source;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                if (!font.TryAddCharacters(needed, out string missing))
                    throw new InvalidOperationException(font.name + " could not add required glyphs: " + missing);
                string absent = new string(needed.Where(c => !font.HasCharacter(c)).ToArray());
                if (absent.Length != 0)
                    throw new InvalidOperationException(font.name + " still lacks required glyphs: " + absent);
                foreach (var texture in font.atlasTextures)
                {
                    if (texture == null) continue;
                    if (!EditorUtility.IsPersistent(texture)) AssetDatabase.AddObjectToAsset(texture, font);
                    EditorUtility.SetDirty(texture);
                }
            }
            finally
            {
                font.atlasPopulationMode = AtlasPopulationMode.Static;
                EditorUtility.SetDirty(font);
            }
        }

        static void EnsureArrowFallback(TMP_FontAsset font, TMP_FontAsset arrowFont)
        {
            var fallbacks = font.fallbackFontAssetTable;
            if (fallbacks == null) fallbacks = new System.Collections.Generic.List<TMP_FontAsset>();
            if (fallbacks.Contains(arrowFont)) return;
            fallbacks.Add(arrowFont);
            font.fallbackFontAssetTable = fallbacks;
            EditorUtility.SetDirty(font);
        }

        static string[] SavedScreenPaths() => AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/ImportedUI" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(" Screen.prefab", StringComparison.Ordinal)).OrderBy(p => p).ToArray();

        static string RequiredNonArrowCharacters(string[] paths)
        {
            string content = string.Concat(paths.SelectMany(p => AssetDatabase.LoadAssetAtPath<GameObject>(p)
                .GetComponentsInChildren<TMP_Text>(true)).Select(t => t.text));
            return new string((GameState.SupportedStoreNameCharacters + content)
                .Where(c => !char.IsControl(c) && UiArrowCharacters.IndexOf(c) < 0)
                .Distinct().OrderBy(c => c).ToArray());
        }

        [MenuItem("Tools/Rocket Raja/Ensure Game Glyphs")]
        public static void EnsureStoreNameGlyphs()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before updating font assets.");
            string[] paths = SavedScreenPaths();
            if (paths.Length != 14) throw new InvalidOperationException("Expected all 14 game screen prefabs.");
            string chars = RequiredNonArrowCharacters(paths);
            var fonts = new[]
            {
                Create("Nunito Medium SDF", "Nunito-Medium", BodyPath, chars),
                Create("Nunito ExtraBold SDF", "Nunito-ExtraBold", BoldPath, chars),
                Create("Baloo 2 Bold SDF", "Baloo2-Bold", HeadingPath, chars + UiArrowCharacters)
            };
            EnsureArrowFallback(fonts[0], fonts[2]);
            EnsureArrowFallback(fonts[1], fonts[2]);
            AssetDatabase.SaveAssets();
            foreach (var font in fonts)
            {
                string absent = new string(chars.Where(c => !font.HasCharacter(c)).ToArray());
                if (absent.Length != 0 || font.atlasPopulationMode != AtlasPopulationMode.Static)
                    throw new InvalidOperationException(font.name + " has incomplete saved store-name coverage: " + absent);
            }
            if (UiArrowCharacters.Any(c => !fonts[2].HasCharacter(c) ||
                    !fonts[0].HasCharacter(c, true, false) || !fonts[1].HasCharacter(c, true, false)))
                throw new InvalidOperationException("Saved game fonts cannot render the browse and scroll arrows.");
            bool Accepts(string input, string expected) =>
                GameState.TryStoreName(input, out string normalized) && normalized == expected;
            if (!Accepts("Jos\u00e9", "Jos\u00e9") || !Accepts("Jose\u0301", "Jos\u00e9") ||
                !Accepts("<b>Literal Name</b>", "<b>Literal Name</b>") ||
                GameState.TryStoreName("\u200b", out _) || GameState.TryStoreName("\u00a0", out _) ||
                GameState.TryStoreName("\u00ad", out _) || GameState.TryStoreName("\u0926\u0940\u0935\u093e\u0932\u0940", out _) ||
                GameState.TryStoreName("\ud83c\udf86", out _) || GameState.TryStoreName("\ud800", out _) ||
                GameState.TryStoreName("---", out _) || GameState.TryStoreName(new string('W', 25), out _))
                throw new InvalidOperationException("Store-name validation smoke failed.");
            Debug.Log($"Game glyphs saved: {chars.Length} store-name characters in {fonts.Length} static SDF assets; browse and scroll arrows render through Baloo; validation cases passed.");
        }

        [MenuItem("Tools/Rocket Raja/Apply Baloo 2 and Nunito")]
        public static void ApplyToSavedScreens()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            string[] paths = SavedScreenPaths();
            if (paths.Length != 14) throw new InvalidOperationException("Expected all 14 game screen prefabs.");
            // Include all authored UI text and player-entered name glyphs. Nunito's
            // sources lack arrows, which Baloo provides directly and as a fallback.
            string chars = RequiredNonArrowCharacters(paths);
            body = Create("Nunito Medium SDF", "Nunito-Medium", BodyPath, chars);
            var bold = Create("Nunito ExtraBold SDF", "Nunito-ExtraBold", BoldPath, chars);
            heading = Create("Baloo 2 Bold SDF", "Baloo2-Bold", HeadingPath, chars + UiArrowCharacters);
            EnsureArrowFallback(body, heading);
            EnsureArrowFallback(bold, heading);
            body.fontWeightTable[7].regularTypeface = bold;
            body.fontWeightTable[8].regularTypeface = bold;
            body.fontWeightTable[9].regularTypeface = bold;
            EditorUtility.SetDirty(body);
            AssetDatabase.SaveAssets();
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
