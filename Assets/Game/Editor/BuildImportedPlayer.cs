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
    /// <summary>Editor authoring for the HTML Player panel and ten reusable unlockable rows.</summary>
    public static class BuildImportedPlayer
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Player Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Player UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Player.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "PlayerImporterCanvas")
                    throw new System.InvalidOperationException("PlayerImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            var statsIcon = ImportIcon("Icon Stats");
            var skillsIcon = ImportIcon("Icon Skills");
            var tapIcon = ImportIcon("Icon Tap");
            var prestigeIcon = ImportIcon("Icon Prestige");
            if (statsIcon == null || skillsIcon == null || tapIcon == null || prestigeIcon == null)
                throw new System.InvalidOperationException("Player icon artwork has not been imported as sprites.");

            var root = CanvasRoot("PlayerImporterCanvas", 11);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var panel = Stretch(safe, "Player Panel", 0, 0, 145, 1400);
            Image(panel, IvoryColor, true);
            var fit = root.AddComponent<ImportedPanelFit>();
            var fitData = new SerializedObject(fit);
            fitData.FindProperty("safeArea").objectReferenceValue = safe;
            fitData.FindProperty("lowerPanel").objectReferenceValue = panel;
            fitData.ApplyModifiedPropertiesWithoutUndo();

            var header = Top(panel, "Header", 144);
            Image(header, IvoryColor);
            Text(Stretch(header, "Title", 28, 170, 15, 10), "Player", 58,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var collapse = InvisibleButton(Stretch(header, "collapse-Player", 905, 20, 15, 12));
            Text(Fill(collapse.transform, "Collapse Icon"), "v", 63, Ink, TextAlignmentOptions.Center, true);
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
            layout.padding = new RectOffset(28, 28, 14, 25);
            layout.spacing = 16;
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

            var toolbar = New("Player Toolbar", content).GetComponent<RectTransform>();
            toolbar.gameObject.AddComponent<LayoutElement>().preferredHeight = 180;
            var stats = InvisibleButton(Region(toolbar, "open-stats", .01f, .02f, .21f, .98f));
            Icon(Stretch(stats.transform, "Stats Icon", 45, 45, 80, 12), statsIcon);
            Text(Stretch(stats.transform, "Stats Label", 8, 8, 4, 112), "Stats", 32,
                Ink, TextAlignmentOptions.Center, true);
            var skills = InvisibleButton(Region(toolbar, "open-skills", .22f, .02f, .42f, .98f));
            Icon(Stretch(skills.transform, "Skills Icon", 45, 45, 80, 12), skillsIcon);
            Text(Stretch(skills.transform, "Skills Label", 8, 8, 4, 112), "Skills", 32,
                Ink, TextAlignmentOptions.Center, true);
            var quantity = Button(Region(toolbar, "buy-quantity", .70f, .10f, .99f, .92f), Blue);
            var quantityText = Text(Fill(quantity.transform, "Quantity Label"), "Buy ×1", 41,
                Color.white, TextAlignmentOptions.Center, true);

            var tap = FramedCard(content, "Salesmanship", 175);
            Icon(Stretch(tap, "Tap Icon", 35, 855, 28, 28), tapIcon);
            Text(Stretch(tap, "Name", 170, 385, 88, 22), "Salesmanship", 41,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var level = Text(Stretch(tap, "tap-level", 520, 350, 85, 25), "Level 12", 28,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var income = Text(Stretch(tap, "tap-income", 170, 350, 29, 96), "₹18 per tap", 31,
                Muted, TextAlignmentOptions.MidlineLeft);
            var buy = Button(Stretch(tap, "buy-tap", 700, 24, 19, 18), Blue);
            var buyText = Text(Fill(buy.transform, "Buy Label"), "₹263\nUpgrade ×1", 34,
                Color.white, TextAlignmentOptions.Center, true);

            var prestige = FramedCard(content, "Prestige", 180);
            Icon(Stretch(prestige, "Prestige Icon", 35, 855, 28, 28), prestigeIcon);
            Text(Stretch(prestige, "Heading", 170, 385, 88, 22), "A brighter beginning", 41,
                Hex("A82149"), TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(prestige, "Description", 170, 350, 29, 96),
                "Earn reputation for permanent perks.", 29, Muted, TextAlignmentOptions.MidlineLeft);
            var prestigeButton = Button(Stretch(prestige, "open-prestige", 700, 24, 19, 18), Red);
            var prestigeText = Text(Fill(prestigeButton.transform, "Prestige Label"), "View prestige", 33,
                Color.white, TextAlignmentOptions.Center, true);

            var heading = New("Unlockables Heading", content).GetComponent<RectTransform>();
            heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 155;
            Text(Stretch(heading, "Title", 12, 400, 75, 10), "Unlockables", 41,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var count = Text(Stretch(heading, "unlockable-count", 12, 350, 10, 87), "0 / 96 purchased", 29,
                Muted, TextAlignmentOptions.MidlineLeft);

            var buyAll = Button(Stretch(heading, "buy-all", 700, 22, 5, 5), Blue);
            Text(Fill(buyAll.transform, "Label"), "Buy All", 33, Color.white, TextAlignmentOptions.Center, true);

            var rows = new GameObject[10];
            var buttons = new Button[10];
            var names = new TMP_Text[10];
            var effects = new TMP_Text[10];
            var prices = new TMP_Text[10];
            for (int i = 0; i < 10; i++)
            {
                var row = FramedCard(content, "Unlockable Row " + i, 176);
                rows[i] = row.gameObject;
                names[i] = Text(Stretch(row, "Name", 42, 365, 93, 20), "Unlockable", 37,
                    Ink, TextAlignmentOptions.MidlineLeft, true);
                effects[i] = Text(Stretch(row, "Effect", 42, 365, 25, 91), "A helpful boost", 29,
                    Muted, TextAlignmentOptions.MidlineLeft);
                buttons[i] = Button(Stretch(row, "Buy", 700, 22, 19, 17), Blue);
                prices[i] = Text(Fill(buttons[i].transform, "Price"), "₹250\nBuy upgrade", 31,
                    Color.white, TextAlignmentOptions.Center, true);
            }

            var menu = New("Quantity Menu", panel).GetComponent<RectTransform>();
            menu.anchorMin = menu.anchorMax = new Vector2(1, 1);
            menu.pivot = new Vector2(1, 1);
            menu.anchoredPosition = new Vector2(-35, -150);
            menu.sizeDelta = new Vector2(305, 470);
            Image(menu, IvoryColor, true);
            var choices = new Button[4];
            string[] labels = { "Buy ×1", "Buy ×10", "Buy ×100", "Buy MAX" };
            for (int i = 0; i < 4; i++)
            {
                var rect = New("Quantity " + labels[i], menu).GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(.5f, 1);
                rect.offsetMin = new Vector2(12, -(20 + (i + 1) * 108));
                rect.offsetMax = new Vector2(-12, -(20 + i * 108));
                choices[i] = Button(rect, Blue);
                Text(Fill(rect, "Label"), labels[i], 32, Color.white, TextAlignmentOptions.Center, true);
            }
            menu.gameObject.SetActive(false);

            var presenter = root.AddComponent<ImportedPlayerScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("collapseButton").objectReferenceValue = collapse;
            serialized.FindProperty("scroll").objectReferenceValue = scroll;
            serialized.FindProperty("statsButton").objectReferenceValue = stats;
            serialized.FindProperty("skillsButton").objectReferenceValue = skills;
            serialized.FindProperty("quantityButton").objectReferenceValue = quantity;
            Assign(serialized, "quantityChoices", choices);
            serialized.FindProperty("quantityMenu").objectReferenceValue = menu.gameObject;
            serialized.FindProperty("tapBuyButton").objectReferenceValue = buy;
            serialized.FindProperty("prestigeButton").objectReferenceValue = prestigeButton;
            serialized.FindProperty("buyAllButton").objectReferenceValue = buyAll;
            serialized.FindProperty("quantityLabel").objectReferenceValue = quantityText;
            serialized.FindProperty("tapLevelLabel").objectReferenceValue = level;
            serialized.FindProperty("tapIncomeLabel").objectReferenceValue = income;
            serialized.FindProperty("tapBuyLabel").objectReferenceValue = buyText;
            serialized.FindProperty("prestigeLabel").objectReferenceValue = prestigeText;
            serialized.FindProperty("unlockableCountLabel").objectReferenceValue = count;
            Assign(serialized, "upgradeRows", rows);
            Assign(serialized, "upgradeButtons", buttons);
            Assign(serialized, "upgradeNames", names);
            Assign(serialized, "upgradeEffects", effects);
            Assign(serialized, "upgradePrices", prices);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Player panel with ten preplaced unlockable rows in " + ImporterPath);
        }

        static void Assign<T>(SerializedObject serialized, string name, T[] items) where T : Object
        {
            var property = serialized.FindProperty(name);
            property.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        static Sprite ImportIcon(string name)
        {
            string path = "Assets/Game/ImportedUI/Art/" + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Missing Player icon: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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

        static void Icon(RectTransform rect, Sprite sprite)
        {
            var image = Image(rect, Color.white, false, sprite);
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = true;
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Player integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Player integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "PlayerImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedPlayerScreen>() == null)
                throw new System.InvalidOperationException("Player canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedPlayerCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Player canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Player prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedPlayerCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Player screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("playerScreen").objectReferenceValue = instance.GetComponent<ImportedPlayerScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Player panel in Main and bound GameController.");
        }

    }
}
