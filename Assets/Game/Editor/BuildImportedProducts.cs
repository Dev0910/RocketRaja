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
    /// <summary>Editor authoring for the HTML Products panel with eight saved rows.</summary>
    public static class BuildImportedProducts
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Products Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly string[] Ids = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };
        static readonly string[] Names = { "Phuljhari", "Anar", "Chakri", "Rockets", "Small crackers", "Cracker strings", "Aerial shots", "Celebration boxes" };

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Products UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Products.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "ProductsImporterCanvas")
                    throw new System.InvalidOperationException("ProductsImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            var sprites = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Product " + Ids[i] + ".png");
                if (sprites[i] == null) throw new System.InvalidOperationException("Product artwork is missing: " + Ids[i]);
            }

            var root = CanvasRoot("ProductsImporterCanvas", 11);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var panel = Stretch(safe, "Products Panel", 0, 0, 145, 1400);
            Image(panel, IvoryColor, true);
            var fit = root.AddComponent<ImportedPanelFit>();
            var fitData = new SerializedObject(fit);
            fitData.FindProperty("safeArea").objectReferenceValue = safe;
            fitData.FindProperty("lowerPanel").objectReferenceValue = panel;
            fitData.ApplyModifiedPropertiesWithoutUndo();

            var header = Top(panel, "Header", 144);
            Image(header, IvoryColor);
            Text(Stretch(header, "Title", 28, 300, 15, 10), "Products", 58,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var collapse = InvisibleButton(Stretch(header, "collapse-Products", 800, 20, 5, 5));
            Text(Fill(collapse.transform, "Collapse Icon"), "← Shop", 42, Ink, TextAlignmentOptions.Center, true);
            Image(Bottom(header, "Double Rule", 5), Hex("255883"));

            var body = Stretch(panel, "Scroll Body", 0, 0, 0, 144);
            Image(body, new Color(1, 1, 1, .001f), true);
            body.gameObject.AddComponent<RectMask2D>();
            var content = New("Content", body).GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 22, 26);
            layout.spacing = 18;
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

            var toolbar = New("Products Toolbar", content).GetComponent<RectTransform>();
            toolbar.gameObject.AddComponent<LayoutElement>().preferredHeight = 175;
            var profit = Text(Stretch(toolbar, "products-profit", 4, 385, 28, 28), "Profit · ₹13.42M / sec", 40,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var quantity = Button(Region(toolbar, "buy-quantity", .70f, .10f, .99f, .92f), Sand);
            var quantityText = Text(Stretch(quantity.transform, "Quantity Label", 8, 43, 4, 4), "Qty ×1", 40,
                Ink, TextAlignmentOptions.Center, true);
            Text(Stretch(quantity.transform, "Quantity Arrow", 247, 10, 4, 4), "v", 38,
                Ink, TextAlignmentOptions.Center, true);

            var detailButtons = new Button[8];
            var purchaseButtons = new Button[8];
            var images = new Image[8];
            var names = new TMP_Text[8];
            var levels = new TMP_Text[8];
            var incomes = new TMP_Text[8];
            var milestones = new TMP_Text[8];
            var priceLabels = new TMP_Text[8];
            for (int i = 0; i < 8; i++)
            {
                var row = FramedCard(content, "Product Row " + Ids[i], 220);
                var artFrame = Stretch(row, "Art Frame", 24, 815, 22, 22);
                Image(artFrame, Hex("E8EBE5"));
                images[i] = Image(Stretch(artFrame, "Product Art", 8, 8, 8, 8), Color.white, false, sprites[i]);
                images[i].type = UnityEngine.UI.Image.Type.Simple;
                images[i].preserveAspect = true;
                // Nunito's TMP line height is about 1.36 times font size; shorter rects ellipsize whole lines.
                names[i] = Text(Stretch(row, "product-name-" + i, 213, 350, 160, 5), Names[i], 38,
                    Ink, TextAlignmentOptions.MidlineLeft, true);
                levels[i] = Text(Stretch(row, "product-level-" + i, 213, 350, 113, 60), "Level 13", 33,
                    Muted, TextAlignmentOptions.MidlineLeft);
                incomes[i] = Text(Stretch(row, "product-income-" + i, 213, 350, 65, 107), "₹36.4 / sec", 32,
                    Ink, TextAlignmentOptions.MidlineLeft, true);
                milestones[i] = Text(Stretch(row, "product-next-" + i, 213, 350, 10, 155), "Milestones · Lv.25", 33,
                    Muted, TextAlignmentOptions.MidlineLeft);
                detailButtons[i] = InvisibleButton(Stretch(row, "detail-" + i, 10, 350, 10, 10));
                purchaseButtons[i] = Button(Stretch(row, "purchase-" + i, 700, 18, 18, 18), Blue);
                priceLabels[i] = Text(Fill(purchaseButtons[i].transform, "Price"), "₹524\nUpgrade ×1", 33,
                    Color.white, TextAlignmentOptions.Center, true);
            }

            var menu = New("Quantity Menu", panel).GetComponent<RectTransform>();
            menu.anchorMin = menu.anchorMax = new Vector2(1, 1);
            menu.pivot = new Vector2(1, 1);
            menu.anchoredPosition = new Vector2(-35, -150);
            menu.sizeDelta = new Vector2(365, 568);
            Image(menu, IvoryColor, true);
            var choices = new Button[4];
            string[] objectNames = { "Buy ×1", "Buy ×10", "Buy ×100", "Buy MAX" };
            string[] labels = { "×1", "×10", "×100", "Max affordable" };
            for (int i = 0; i < 4; i++)
            {
                var rect = New("Quantity " + objectNames[i], menu).GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(.5f, 1);
                rect.offsetMin = new Vector2(12, -(20 + (i + 1) * 132));
                rect.offsetMax = new Vector2(-12, -(20 + i * 132));
                choices[i] = Button(rect, Sand);
                var mark = Image(Stretch(rect, "Selected Mark", 17, 309, 36, 36), Ink);
                mark.gameObject.SetActive(i == 0);
                Text(Stretch(rect, "Label", 38, 12, 6, 6), labels[i] + (i == 0 ? "\nSelected" : ""), 33,
                    Ink, TextAlignmentOptions.Center, true);
            }
            menu.gameObject.SetActive(false);

            var presenter = root.AddComponent<ImportedProductsScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("collapseButton").objectReferenceValue = collapse;
            serialized.FindProperty("quantityButton").objectReferenceValue = quantity;
            Assign(serialized, "quantityChoices", choices);
            serialized.FindProperty("quantityMenu").objectReferenceValue = menu.gameObject;
            serialized.FindProperty("quantityLabel").objectReferenceValue = quantityText;
            serialized.FindProperty("profitLabel").objectReferenceValue = profit;
            Assign(serialized, "detailButtons", detailButtons);
            Assign(serialized, "purchaseButtons", purchaseButtons);
            Assign(serialized, "productImages", images);
            Assign(serialized, "names", names);
            Assign(serialized, "levels", levels);
            Assign(serialized, "incomes", incomes);
            Assign(serialized, "milestones", milestones);
            Assign(serialized, "purchaseLabels", priceLabels);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            ApplyDetailsAffordance(root);
            UiQuantityAuthoring.Apply(root);
            UiCompactTypography.Apply(root);
            AuthorHtmlMotion.Meters(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Products panel with eight preplaced catalog rows in " + ImporterPath);
        }

        static void Assign<T>(SerializedObject serialized, string name, T[] items) where T : Object
        {
            var property = serialized.FindProperty(name);
            property.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        static RectTransform Region(Transform parent, string name, float x0, float y0, float x1, float y1)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(x0, y0);
            rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        static Button InvisibleButton(RectTransform rect)
        {
            var graphic = Image(rect, new Color(1, 1, 1, .001f), true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }

        public static void ApplyDetailsAffordance(GameObject root)
        {
            foreach (var art in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (art.name != "Art Frame") continue;
                var picture = art.Find("Product Art") as RectTransform;
                if (picture == null) continue;
                picture.offsetMin = new Vector2(8, 60);
                var cue = art.Find("Details Cue") as RectTransform;
                if (cue == null) cue = Bottom(art, "Details Cue", 56);
                cue.offsetMin = new Vector2(4, 0);
                cue.offsetMax = new Vector2(-4, 56);
                var label = cue.GetComponent<TMP_Text>();
                if (label == null) label = Text(cue, "Details >", 36, Ink, TextAlignmentOptions.Center, true);
                label.text = "Details >";
                label.fontSize = 36;
                label.enableAutoSizing = false;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                EditorUtility.SetDirty(label);
            }
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Products integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Products integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "ProductsImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedProductsScreen>() == null)
                throw new System.InvalidOperationException("Products canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedProductsCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Products canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Products prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedProductsCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Products screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("productsScreen").objectReferenceValue = instance.GetComponent<ImportedProductsScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Products panel in Main and bound GameController.");
        }

    }
}
