using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Editor-only authoring. Every UI instance is serialized before the player starts.</summary>
    public static partial class BuildRedesignUI
    {
        public const string PrefabPath = "Assets/Game/RedesignUI/Redesign Canvas.prefab";
        public const string PreviewPath = "Assets/Scenes/Redesign UI Preview.unity";
        const float S = 1080f / 390f;
        static TMP_FontAsset body, heading;
        static Sprite rounded, paper;
        static readonly Color Ink = Hex("382131"), Cream = Hex("FFF3D5"), Gold = Hex("B37A33"), Maroon = Hex("751F35"), Teal = Hex("086F70");
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color result); return result; }
        static RectTransform New(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent) go.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(go, "Author redesign UI");
            return (RectTransform)go.transform;
        }
        static RectTransform Fill(string name, Transform parent, float left = 0, float right = 0, float bottom = 0, float top = 0)
        {
            var r = New(name, parent); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(left, bottom) * S; r.offsetMax = new Vector2(-right, -top) * S; return r;
        }
        static RectTransform Top(string name, Transform parent, float y, float height, float left = 0, float right = 0)
        {
            var r = New(name, parent); r.anchorMin = new Vector2(0, 1); r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f, 1);
            r.offsetMin = new Vector2(left, -y - height) * S; r.offsetMax = new Vector2(-right, -y) * S; return r;
        }
        static Image Image(RectTransform r, Color color, bool raycast = false, bool round = false)
        {
            var image = r.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = raycast;
            if (round) { image.sprite = rounded; image.type = UnityEngine.UI.Image.Type.Sliced; image.pixelsPerUnitMultiplier = .5f / S; }
            return image;
        }
        static void Texture(RectTransform parent)
        {
            var image = Image(Fill("Paper grain", parent), Color.white); image.sprite = paper;
            image.type = UnityEngine.UI.Image.Type.Tiled; image.pixelsPerUnitMultiplier = 1f / S;
        }
        static TMP_Text Text(RectTransform r, string value, float size, bool title = false)
        {
            var t = r.gameObject.AddComponent<RedesignSavedText>(); t.font = title ? heading : body; t.fontSharedMaterial = t.font.material;
            t.text = value; t.fontSize = size * S; t.color = Ink; t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal; t.overflowMode = TextOverflowModes.Ellipsis; t.richText = false;
            AuthorTextSlots(t); return t;
        }
        static Button Button(RectTransform rect, string label, bool primary = false)
        {
            Image(rect, primary ? Hex("064E56") : Gold, true, true);
            var face = Image(Fill("Face", rect, 1, 1, 1, 1), primary ? Teal : Cream, false, true);
            Texture(face.rectTransform);
            var b = rect.gameObject.AddComponent<Button>(); b.targetGraphic = face; b.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = b.colors; colors.fadeDuration = .08f; colors.pressedColor = new Color(.82f, .82f, .82f); colors.disabledColor = new Color(1, 1, 1, .52f); b.colors = colors;
            var t = Text(Fill("Label", rect, 5, 5, 4, 4), label, 13); t.alignment = TextAlignmentOptions.Center; t.fontStyle = FontStyles.Bold;
            t.color = primary ? Cream : Ink; return b;
        }
        static Button[] Segment(RectTransform root, string[] labels)
        {
            var result = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var r = New(labels[i], root); r.anchorMin = new Vector2((float)i / labels.Length, 0); r.anchorMax = new Vector2((float)(i + 1) / labels.Length, 1);
                r.offsetMin = new Vector2(i == 0 ? 0 : 2.5f * S, 0); r.offsetMax = new Vector2(i == labels.Length - 1 ? 0 : -2.5f * S, 0);
                result[i] = Button(r, labels[i]);
            }
            return result;
        }
        static void LoadAssets()
        {
            body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/ImportedUI/Nunito Medium SDF.asset");
            heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/Bootstrap/Fonts/Fraunces ExtraBold SDF.asset");
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            const string paperPath = "Assets/Game/RedesignUI/Art/Paper.png";
            AssetDatabase.ImportAsset(paperPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(paperPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.wrapMode = TextureWrapMode.Repeat;
            importer.SaveAndReimport(); paper = AssetDatabase.LoadAssetAtPath<Sprite>(paperPath);
            if (!body || !heading || !paper || !rounded) throw new InvalidOperationException("Redesign artwork/fonts are missing");
        }
        public static void Build()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Author outside Play/compilation");
            if (Application.dataPath.Replace('\\', '/') != "C:/Unity/Projects/Incremental game/Assets") throw new InvalidOperationException("Wrong project");
            if (File.Exists(PrefabPath)) throw new InvalidOperationException("Native prefab already exists; apply a scoped revision instead of overwriting it");
            LoadAssets();
            var originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                var root = New("Redesign Canvas", null);
                var canvas = root.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = root.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 2200); scaler.matchWidthOrHeight = 0;
                root.gameObject.AddComponent<GraphicRaycaster>(); Image(Fill("Background", root), Hex("F9E7BD")); Texture(root);
                var safe = Fill("Safe Area", root); safe.gameObject.AddComponent<ImportedSafeArea>();
                var header = Top("Header", safe, 0, 90); Image(header, Maroon); Texture(header);
                var title = Text(Top("screen-title", header, 14, 34, 16, 86), "Upgrades", 27, true); title.color = Cream;
                var cash = Text(Top("header-summary", header, 57, 20, 16, 86), "₹30 cash · Normal sales ₹0/s", 13); cash.color = Cream;
                var settingsRect = Top("settings", header, 20, 48); settingsRect.anchorMin = new Vector2(1, 1); settingsRect.anchorMax = Vector2.one;
                settingsRect.offsetMin = new Vector2(-81, -68) * S; settingsRect.offsetMax = new Vector2(-16, -20) * S; Button(settingsRect, "Settings");
                var footer = Fill("Navigation", safe); footer.anchorMax = new Vector2(1, 0); footer.pivot = new Vector2(.5f, 0); footer.offsetMin = Vector2.zero; footer.offsetMax = new Vector2(0, 68 * S);
                Image(footer, Maroon); Texture(footer); Segment(Fill("Tabs", footer, 8, 8, 7, 7), new[] { "Home", "Upgrades", "Talent", "Prestige" });
                var screens = Fill("Screen States", safe, 0, 0, 68, 90);
                CreateUpgrades(Fill("screen-upgrades", screens));
                var saved = PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject, PrefabPath, InteractionMode.AutomatedAction);
                if (!saved) throw new InvalidOperationException("Could not save native prefab");
                ReviseCatalogue();
                AddDetailsAndBatch(); BindCatalogueRouter(); AddLevelScreens(); AddTalentAndPrestige(); AddShop(); AddReturnAndSkills(); AddHome(); AddRetainedDialogs(); AddAudio(); AddProductAppearances(); AddShopLayers(); AddFeedbackDismissal(); AddGuidance();
                new GameObject("Redesign Event System", typeof(EventSystem), typeof(StandaloneInputModule));
                if (!EditorSceneManager.SaveScene(scene, PreviewPath)) throw new InvalidOperationException("Could not save native preview scene");
                Debug.Log("REDESIGN_NATIVE_AUTHOR_OK " + PrefabPath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (originalScene.IsValid()) UnityEngine.SceneManagement.SceneManager.SetActiveScene(originalScene);
            }
        }
        static void CreateUpgrades(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignUpgradesScreen>(); ui.rowHeight = 210 * S;
            var frame = Fill("Catalogue", root, 14, 14, 14, 14);
            ui.categories = Segment(Top("Categories", frame, 0, 48), new[] { "All", "Products", "Tapping", "Skills", "Shop" });
            var filters = Top("Target and Search", frame, 56, 48);
            var target = Fill("target-filter", filters); target.anchorMax = new Vector2(.5f, 1); target.offsetMax = new Vector2(-4 * S, 0);
            ui.targetFilter = Button(target, "Product: All"); ui.targetLabel = ui.targetFilter.GetComponentInChildren<TMP_Text>();
            var searchRect = Fill("upgrade-search", filters); searchRect.anchorMin = new Vector2(.5f, 0); searchRect.offsetMin = new Vector2(4 * S, 0);
            Image(searchRect, Cream, true, true); ui.search = searchRect.gameObject.AddComponent<RedesignSearchInput>();
            var textArea = Fill("Text Area", searchRect, 9, 9, 5, 5); textArea.gameObject.AddComponent<RectMask2D>();
            ui.search.textViewport = textArea; ui.search.textComponent = (TMP_Text)Text(Fill("Value", textArea), "", 14);
            var hint = Text(Fill("Placeholder", textArea), "Search upgrades", 14); hint.color = Hex("70554D");
            ui.search.placeholder = hint; ui.search.characterLimit = 100; ui.search.lineType = TMP_InputField.LineType.SingleLine; ui.search.navigation = new Navigation { mode = Navigation.Mode.None };
            AuthorCaret((RedesignSearchInput)ui.search);
            ui.views = Segment(Top("Views", frame, 112, 48), new[] { "Available", "Upcoming", "Owned", "Full" });
            ui.countLabel = Text(Top("Match Count", frame, 168, 22), "1,000 catalogue entries", 12);
            var scrollRoot = Fill("upgrade-list", frame, 0, 0, 58, 196); ui.scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            var viewport = Fill("Viewport", scrollRoot); Image(viewport, new Color(1, 1, 1, .001f), true); viewport.gameObject.AddComponent<RectMask2D>();
            ui.content = Top("Content", viewport, 0, 8 * 210); ui.scroll.content = ui.content; ui.scroll.viewport = viewport;
            ui.scroll.horizontal = false; ui.scroll.vertical = true; ui.scroll.movementType = ScrollRect.MovementType.Clamped;
            ui.scroll.inertia = true; ui.scroll.decelerationRate = .135f; ui.scroll.scrollSensitivity = 45 * S;
            ui.rows = new RedesignUpgradeRow[8];
            for (int i = 0; i < ui.rows.Length; i++)
            {
                var rect = Top("upgrade-row-" + i, ui.content, i * 210, 200); Image(rect, Hex("CC9B53"), false, true);
                var face = Fill("Face", rect, 1.5f, 1.5f, 1.5f, 1.5f); Image(face, Hex("FFF4D8"), false, true); Texture(face);
                var row = rect.gameObject.AddComponent<RedesignUpgradeRow>(); ui.rows[i] = row;
                row.title = Text(Top("Name", rect, 12, 46, 14, 14), "Phuljhari Signature Breakthrough I", 19, true);
                row.effect = Text(Top("Effect", rect, 60, 36, 14, 14), "First purchase · Multiply family normal profit by x2", 13);
                row.requirement = Text(Top("Requirement", rect, 98, 34, 14, 14), "Own Phuljhari", 12);
                row.price = Text(Top("Price", rect, 132, 20, 14, 14), "₹115.28", 15);
                var pair = Top("Actions", rect, 150, 48, 14, 14); var buttons = Segment(pair, new[] { "Details", "Buy" }); row.details = buttons[0]; row.buy = buttons[1];
                row.buy.targetGraphic.color = Teal; row.buy.GetComponentInChildren<TMP_Text>().color = Cream;
            }
            ui.emptyLabel = Text(Fill("list-empty", viewport, 12, 12, 12, 12), "No matching upgrades. Try another filter.", 16); ui.emptyLabel.gameObject.SetActive(false);
            var review = Fill("review-buyall", frame); review.anchorMax = new Vector2(1, 0); review.pivot = new Vector2(.5f, 0); review.offsetMin = Vector2.zero; review.offsetMax = new Vector2(0, 48 * S);
            ui.reviewButton = Button(review, "Review Buy All · All", true); ui.reviewLabel = ui.reviewButton.GetComponentInChildren<TMP_Text>();
        }
        public static void ReviseCatalogue()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var ui = root.GetComponentInChildren<RedesignUpgradesScreen>(true);
                void Place(RectTransform r, float y, float h)
                {
                    r.offsetMin = new Vector2(r.offsetMin.x, (-y - h) * S);
                    r.offsetMax = new Vector2(r.offsetMax.x, -y * S);
                }
                var frame = ui.transform.Find("Catalogue");
                Place((RectTransform)frame.Find("Categories"), 0, 52);
                Place((RectTransform)frame.Find("Target and Search"), 60, 52);
                Place((RectTransform)frame.Find("Views"), 120, 52);
                Place((RectTransform)frame.Find("Match Count"), 180, 22);
                var scroll = (RectTransform)ui.scroll.transform; scroll.offsetMin = new Vector2(0, 62 * S); scroll.offsetMax = new Vector2(0, -208 * S);
                var review = (RectTransform)ui.reviewButton.transform; review.offsetMax = new Vector2(0, 52 * S);
                Place((RectTransform)root.transform.Find("Safe Area/Header/settings"), 18, 52);
                ui.rowHeight = 224 * S; ui.content.sizeDelta = new Vector2(ui.content.sizeDelta.x, 8 * ui.rowHeight);
                for (int i = 0; i < ui.rows.Length; i++)
                {
                    var row = ui.rows[i]; Place((RectTransform)row.transform, i * 224, 214);
                    Place(row.requirement.rectTransform, 98, 23); Place(row.price.rectTransform, 123, 26);
                    Place((RectTransform)row.transform.Find("Actions"), 154, 52);
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
