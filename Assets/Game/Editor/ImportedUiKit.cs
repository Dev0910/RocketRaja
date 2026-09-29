using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace RocketRaja.EditorTools
{
    /// <summary>Shared Editor-only primitives for saved Ivory Bazaar uGUI screens.</summary>
    internal static class ImportedUiKit
    {
        internal static TMP_FontAsset Font;
        internal static Sprite Blue, Ivory, Red, Sand, RowFrame, Rounded;

        internal static void Load()
        {
            const string basePath = "Assets/Game/ImportedUI/Art/";
            Font = GameTypography.Body;
            Blue = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Folk Blue.png");
            Ivory = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Folk Ivory.png");
            Red = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Folk Red.png");
            Sand = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Folk Sand.png");
            RowFrame = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Ivory Row Frame.png");
            Rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (Font == null || Blue == null || Ivory == null || Red == null || Sand == null || RowFrame == null)
                throw new System.InvalidOperationException("Imported UI font or ticket artwork is missing.");
            EnsureRupeeFallback();
        }

        internal static void EnsureRupeeFallback()
        {
            const string fallbackPath = "Assets/Game/ImportedUI/Noto Sans Rupee SDF.asset";
            var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fallbackPath);
            if (fallback == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/NotoSans.ttf");
                if (source == null) throw new System.InvalidOperationException("NotoSans.ttf is missing.");
                fallback = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA,
                    1024, 1024, AtlasPopulationMode.Dynamic);
                fallback.name = "Noto Sans Rupee SDF";
                AssetDatabase.CreateAsset(fallback, fallbackPath);
                if (!fallback.TryAddCharacters("₹", out var missing))
                    throw new System.InvalidOperationException("Noto Sans could not author the rupee glyph: " + missing);
                if (fallback.material != null && !EditorUtility.IsPersistent(fallback.material))
                    AssetDatabase.AddObjectToAsset(fallback.material, fallback);
                foreach (var texture in fallback.atlasTextures)
                    if (texture != null && !EditorUtility.IsPersistent(texture))
                        AssetDatabase.AddObjectToAsset(texture, fallback);
                fallback.atlasPopulationMode = AtlasPopulationMode.Static;
                EditorUtility.SetDirty(fallback);
            }
            if (!Font.HasCharacter('₹', true, true))
            {
                var fonts = Font.fallbackFontAssetTable;
                if (fonts == null) fonts = new System.Collections.Generic.List<TMP_FontAsset>();
                if (!fonts.Contains(fallback)) fonts.Add(fallback);
                Font.fallbackFontAssetTable = fonts;
                EditorUtility.SetDirty(Font);
            }
            AssetDatabase.SaveAssets();
            if (!Font.HasCharacter('₹', true, true))
                throw new System.InvalidOperationException("Saved imported font still lacks the rupee glyph.");
        }

        internal static GameObject New(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            if (parent != null) obj.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(obj, "Author imported UI");
            return obj;
        }

        internal static RectTransform Fill(Transform parent, string name)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        internal static RectTransform Stretch(Transform parent, string name, float left, float right, float bottom, float top)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        internal static RectTransform Top(Transform parent, string name, float height)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.offsetMin = new Vector2(0, -height);
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        internal static RectTransform Bottom(Transform parent, string name, float height)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(.5f, 0);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0, height);
            return rect;
        }

        internal static Image Image(RectTransform rect, Color color, bool raycast = false, Sprite sprite = null)
        {
            var graphic = rect.gameObject.AddComponent<Image>();
            graphic.sprite = sprite != null ? sprite : Rounded;
            graphic.type = UnityEngine.UI.Image.Type.Sliced;
            graphic.color = color;
            graphic.raycastTarget = raycast;
            return graphic;
        }

        internal static Button Button(RectTransform rect, Sprite sprite)
        {
            var graphic = Image(rect, Color.white, true, sprite);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.highlightedColor = Hex("E9E0C9");
            colors.pressedColor = Hex("C8D4DD");
            colors.disabledColor = new Color(.7f, .7f, .7f, .6f);
            button.colors = colors;
            return button;
        }

        internal static TMP_Text Text(RectTransform rect, string value, float size, Color color,
            TextAlignmentOptions alignment, bool bold = false)
        {
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.fontSize = size;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.text = value;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            GameTypography.Configure(label);
            return label;
        }

        internal static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out var color);
            return color;
        }

        internal static GameObject CanvasRoot(string name, int order)
        {
            var root = New(name, null);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2200);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0;
            root.AddComponent<GraphicRaycaster>();
            return root;
        }

        internal static RectTransform Dialog(Transform safeArea, string name, float height, float side = 44)
        {
            var rect = New(name, safeArea).GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, .5f);
            rect.anchorMax = new Vector2(1, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = new Vector2(side, -height / 2);
            rect.offsetMax = new Vector2(-side, height / 2);
            Image(rect, Hex("255883"), true);
            Image(Stretch(rect, "Ivory Surface", 5, 5, 5, 5), Hex("FFF4DB"));
            return rect;
        }

        internal static RectTransform FramedCard(Transform parent, string name, float preferredHeight, bool fillParent = false)
        {
            var rect = fillParent ? Fill(parent, name) : New(name, parent).GetComponent<RectTransform>();
            if (!fillParent) rect.gameObject.AddComponent<LayoutElement>().preferredHeight = preferredHeight;
            Image(rect, Hex("FFF5DE"));
            Image(Fill(rect, "Ticket Border"), Color.white, false, RowFrame);
            return rect;
        }
    }
}
