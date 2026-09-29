using RocketRaja;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static RocketRaja.EditorTools.ImportedUiKit;

namespace RocketRaja.EditorTools
{
    /// <summary>Editor authoring for the HTML Product Detail guide.</summary>
    public static class BuildImportedProductDetail
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Product Detail Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly string[] Ids = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Product Detail UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Product Detail.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "ProductDetailImporterCanvas")
                    throw new System.InvalidOperationException("ProductDetailImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            var sprites = new Sprite[Ids.Length];
            for (int i = 0; i < Ids.Length; i++)
            {
                string path = "Assets/Game/ImportedUI/Art/Product " + Ids[i] + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new System.InvalidOperationException("Missing Product Detail art: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] == null) throw new System.InvalidOperationException("Product Detail art import failed: " + path);
            }

            var root = CanvasRoot("ProductDetailImporterCanvas", 100);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var dialog = Dialog(safe, "ProductDetailDialog", 1250);
            var fit = dialog.gameObject.AddComponent<ImportedDialogFit>();
            fit.SetPreferredHeight(1250);

            var header = Top(dialog, "Header", 183);
            Image(header, IvoryColor);
            var title = Text(Stretch(header, "detail-name", 30, 190, 20, 20), "Phuljhari", 55,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var closeRect = New("close-ProductDetail", header).GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-13, -13);
            closeRect.sizeDelta = new Vector2(158, 158);
            var close = Button(closeRect, Blue);
            Text(Fill(closeRect, "Close Icon"), "×", 90, Color.white, TextAlignmentOptions.Center, true);
            Image(Bottom(header, "Double Rule", 5), Hex("255883"));

            var footer = Bottom(dialog, "Footer", 202);
            Image(footer, IvoryColor);
            Image(Top(footer, "Footer Rule", 5), Hex("255883"));
            var back = Button(Stretch(footer, "back-ProductDetail", 34, 34, 29, 34), Ivory);
            Text(Fill(back.transform, "Back Label"), "Return to Products", 41,
                Ink, TextAlignmentOptions.Center, true);

            var body = Stretch(dialog, "Scroll Body", 5, 5, 202, 183);
            var image = Image(body, IvoryColor, true);
            image.color = new Color(1, 1, 1, .001f);
            body.gameObject.AddComponent<RectMask2D>();
            var content = New("Content", body).GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(35, 35, 38, 30);
            layout.spacing = 22;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = body.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = body;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var lead = New("Product Subtitle", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 62;
            var subtitle = Text(Fill(lead.transform, "detail-subtitle"), "A little spark of celebration", 34,
                Muted, TextAlignmentOptions.Center);

            var hero = New("Detail Hero", content).GetComponent<RectTransform>();
            hero.gameObject.AddComponent<LayoutElement>().preferredHeight = 320;
            var productImage = Image(Stretch(hero, "detail-art", 35, 650, 7, 7), Color.white, false, sprites[0]);
            productImage.type = UnityEngine.UI.Image.Type.Simple;
            productImage.preserveAspect = true;
            var level = Text(Stretch(hero, "detail-level", 340, 20, 223, 21), "Level 13", 45,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var income = Text(Stretch(hero, "detail-income", 340, 20, 151, 90), "₹36.4 / sec", 36,
                Ink, TextAlignmentOptions.MidlineLeft);
            Text(Stretch(hero, "Income Caption", 340, 20, 83, 158), "Normal product income", 33,
                Muted, TextAlignmentOptions.MidlineLeft);

            var progress = FramedCard(content, "Milestones", 280);
            var next = Text(Stretch(progress, "detail-next", 36, 30, 174, 27), "Profit ×2 at Level 25", 40,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var track = Stretch(progress, "Progress Track", 38, 38, 88, 145);
            Image(track, Hex("E8E0CE"));
            var fill = New("detail-progress", track).GetComponent<RectTransform>();
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(.52f, 1);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            Image(fill, Hex("255883"));
            var progressLabel = Text(Stretch(progress, "detail-progress-label", 36, 30, 14, 205),
                "13 / 25 levels", 32, Muted, TextAlignmentOptions.MidlineLeft);

            var presenter = root.AddComponent<ImportedProductDetailScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = close;
            serialized.FindProperty("backButton").objectReferenceValue = back;
            serialized.FindProperty("nameLabel").objectReferenceValue = title;
            serialized.FindProperty("subtitleLabel").objectReferenceValue = subtitle;
            serialized.FindProperty("levelLabel").objectReferenceValue = level;
            serialized.FindProperty("incomeLabel").objectReferenceValue = income;
            serialized.FindProperty("nextLabel").objectReferenceValue = next;
            serialized.FindProperty("progressLabel").objectReferenceValue = progressLabel;
            serialized.FindProperty("productImage").objectReferenceValue = productImage;
            serialized.FindProperty("progressFill").objectReferenceValue = fill;
            var art = serialized.FindProperty("productArt");
            art.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                art.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AuthorHtmlMotion.Meters(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Product Detail guide in " + ImporterPath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Product Detail integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Product Detail integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "ProductDetailImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedProductDetailScreen>() == null)
                throw new System.InvalidOperationException("Product Detail canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedProductDetailCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Product Detail canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Product Detail prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedProductDetailCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Product Detail guide in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("productDetailScreen").objectReferenceValue = instance.GetComponent<ImportedProductDetailScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Product Detail in Main and bound GameController.");
        }

    }
}
