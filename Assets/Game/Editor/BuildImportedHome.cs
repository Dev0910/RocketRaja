using RocketRaja;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static RocketRaja.EditorTools.ImportedUiKit;

namespace RocketRaja.EditorTools
{
    /// <summary>Editor authoring for the HTML Home screen and shared shop shell.</summary>
    public static class BuildImportedHome
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Home Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly string[] ProductIds = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };
        // Shelf tags are narrow on phones; full product names remain in Products and the goal card.
        static readonly string[] ProductNames = { "Phuljhari", "Anar", "Chakri", "Rockets", "Crackers", "Strings", "Aerial", "Gift boxes" };

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Home UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Home.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "HomeImporterCanvas")
                    throw new System.InvalidOperationException("HomeImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            Sprite Import(string name)
            {
                string path = "Assets/Game/ImportedUI/Art/" + name + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new System.InvalidOperationException("Home artwork is missing: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) throw new System.InvalidOperationException("Home sprite import failed: " + path);
                return sprite;
            }
            var stall = Import("Stall Background");
            var keeper = Import("Shopkeeper");
            var navArt = new[] { Import("Icon Player"), Import("Icon Products"), Import("Icon Legacy") };
            var settingsArt = Import("Icon Settings");
            var skillArt = new[] { Import("Burst"), Import("Helping Hands"), Import("Bulk Order") };
            var lockedSkillIcon = Import("Icon Lock");
            var productArt = new Sprite[8];
            for (int i = 0; i < 8; i++) productArt[i] = Import("Product " + ProductIds[i]);

            var root = CanvasRoot("HomeImporterCanvas", 10);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            Image(safe, Hex("111B2B"));
            var world = New("World", safe).GetComponent<RectTransform>();
            world.anchorMin = new Vector2(0, 1);
            world.anchorMax = new Vector2(1, 1);
            world.pivot = new Vector2(.5f, 1);
            world.anchoredPosition = Vector2.zero;
            world.sizeDelta = new Vector2(0, 1400);
            var backdrop = Image(world, Color.white, true, stall);
            backdrop.type = UnityEngine.UI.Image.Type.Simple;
            var tap = world.gameObject.AddComponent<ShopTapTarget>();

            var settingsRect = Region(world, "settings-button", .022f, .883f, .155f, .985f);
            var settings = Button(settingsRect, Blue);
            var gear = Image(Stretch(settingsRect, "Settings Icon", 23, 23, 23, 23), Color.white, false, settingsArt);
            gear.type = UnityEngine.UI.Image.Type.Simple;
            gear.preserveAspect = true;
            var cash = Text(Region(world, "cash-value", .20f, .870f, .80f, .990f), "₹1.24M", 115,
                Hex("FFE3A6"), TextAlignmentOptions.Center, true);
            var cashCaption = Text(Region(world, "Cash Caption", .24f, .833f, .76f, .885f), "CASH BALANCE", 40,
                Color.white, TextAlignmentOptions.Center, true);
            var shopSign = Text(Region(world, "Shop Sign", .29f, .660f, .71f, .756f), RocketRaja.Core.GameState.DefaultStoreName, 48,
                IvoryColor, TextAlignmentOptions.Center, true);

            shopSign.richText = false; shopSign.enableAutoSizing = true; shopSign.fontSizeMin = 36; shopSign.fontSizeMax = 48;
            shopSign.textWrappingMode = TextWrappingModes.NoWrap; shopSign.overflowMode = TextOverflowModes.Ellipsis;
            // The painted sign is too short to be a touch target on compact phones.
            // Its transparent hit area fits between the top shelf row and cash controls.
            var inspectSign = InvisibleButton(Region(world, "inspect-store-name", .22f, .658f, .78f, .870f));
            var displays = new Image[8];
            var emptyMarks = new TMP_Text[8];
            var names = new TMP_Text[8];
            var shelfButtons = new Button[8];
            for (int i = 0; i < 8; i++)
            {
                int column = i % 4, row = i / 4;
                float center = .17f + column * .22f;
                float artBottom = row == 0 ? .500f : .320f;
                float artTop = row == 0 ? .625f : .445f;
                var art = Region(world, "display-" + ProductIds[i], center - .085f, artBottom, center + .085f, artTop);
                displays[i] = Image(art, Color.white, false, productArt[i]);
                displays[i].type = UnityEngine.UI.Image.Type.Simple;
                displays[i].preserveAspect = true;
                emptyMarks[i] = Text(Fill(art, "Empty Mark"), "+", 90,
                    Hex("D4AD70"), TextAlignmentOptions.Center);
                var label = Region(world, "shelf-name-" + ProductIds[i], center - .105f,
                    row == 0 ? .445f : .285f, center + .105f, row == 0 ? .525f : .365f);
                names[i] = Text(label, ProductNames[i], 38, Hex("422B20"), TextAlignmentOptions.Center, true);
                names[i].textWrappingMode = TextWrappingModes.NoWrap;
                names[i].overflowMode = TextOverflowModes.Ellipsis;
                var clickRect = Region(world, "open-" + ProductIds[i], center - .105f,
                    row == 0 ? .445f : .235f, center + .105f, row == 0 ? .655f : .445f);
                shelfButtons[i] = InvisibleButton(clickRect);
            }
            var keeperRect = Region(world, "Shopkeeper", .79f, .078f, 1, .239f);
            var keeperImage = Image(keeperRect, Color.white, false, keeper);
            keeperImage.type = UnityEngine.UI.Image.Type.Simple;
            keeperImage.preserveAspect = true;
            var hint = Region(world, "Tap Hint", .185f, .035f, .815f, .085f);
            Image(hint, Hex("FFF4DB"), false);
            var hintText = Text(Fill(hint, "Hint Text"), "Tap to earn", 39,
                Hex("4A291B"), TextAlignmentOptions.Center, true);

            var popupRects = new RectTransform[12];
            var popupLabels = new TMP_Text[12];
            for (int i = 0; i < 12; i++)
            {
                var rect = Region(world, "Cash Popup " + i, .40f, .36f, .65f, .45f);
                popupRects[i] = rect;
                popupLabels[i] = Text(Fill(rect, "Popup Text"), "+₹250", 44,
                    Ink, TextAlignmentOptions.Center, true);
                rect.gameObject.SetActive(false);
            }
            var burstRect = Region(world, "Celebration Burst", .32f, .62f, .68f, .86f);
            var celebration = Image(burstRect, Color.white, false, skillArt[0]);
            celebration.type = UnityEngine.UI.Image.Type.Simple;
            celebration.preserveAspect = true;
            celebration.gameObject.SetActive(false);

            var navRoot = Bottom(safe, "Navigation", 145);
            var navCanvas = navRoot.gameObject.AddComponent<Canvas>();
            navCanvas.overrideSorting = true;
            navCanvas.sortingOrder = 15;
            navRoot.gameObject.AddComponent<GraphicRaycaster>();
            var navButtons = new Button[3];
            var navLabels = new TMP_Text[3];
            var navIndicators = new Image[3];
            string[] navNames = { "Player", "Products", "Legacy" };
            for (int i = 0; i < 3; i++)
            {
                var buttonRect = Region(navRoot, "nav-" + navNames[i],
                    i / 3f + .018f, .005f, (i + 1) / 3f - .018f, .995f);
                navButtons[i] = Button(buttonRect, Blue);
                var icon = Image(Region(buttonRect, "Icon", .28f, .50f, .72f, .99f), Color.white, false, navArt[i]);
                icon.type = UnityEngine.UI.Image.Type.Simple;
                icon.preserveAspect = true;
                // Baloo's 1.6x line height needs more vertical room than its point size suggests.
                navLabels[i] = Text(Region(buttonRect, "Label", .05f, .06f, .95f, .50f), navNames[i], 35,
                    Color.white, TextAlignmentOptions.Center);
                navLabels[i].textWrappingMode = TextWrappingModes.NoWrap;
                navIndicators[i] = Image(Region(buttonRect, "Selected Underline", .31f, .005f, .69f, .045f),
                    Hex("FFE3A6"));
                navIndicators[i].enabled = false;
            }

            var panel = Stretch(safe, "Home Panel", 0, 0, 145, 1400);
            Image(panel, IvoryColor, true);
            var fit = root.AddComponent<ImportedPanelFit>();
            var fitData = new SerializedObject(fit);
            fitData.FindProperty("safeArea").objectReferenceValue = safe;
            fitData.FindProperty("world").objectReferenceValue = world;
            fitData.FindProperty("lowerPanel").objectReferenceValue = panel;
            fitData.FindProperty("cashLabel").objectReferenceValue = cash;
            fitData.FindProperty("cashCaption").objectReferenceValue = cashCaption;
            fitData.FindProperty("shopSign").objectReferenceValue = shopSign;
            fitData.FindProperty("tapHint").objectReferenceValue = hintText;
            fitData.ApplyModifiedPropertiesWithoutUndo();
            var viewport = Fill(panel, "Home Scroll");
            Image(viewport, new Color(1, 1, 1, .001f), true);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = New("Content", viewport).GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(25, 25, 14, 25);
            layout.spacing = 13;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            content.gameObject.AddComponent<ImportedHomeLayout>();

            var incomeCard = FramedCard(content, "Income", 230);
            Text(Stretch(incomeCard, "Income Kicker", 130, 295, 174, 15), "AUTOMATIC SALES", 29,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var coins = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Coins.png");
            var coinImage = Image(Stretch(incomeCard, "Coins", 20, 896, 80, 55), Color.white, false, coins);
            coinImage.type = UnityEngine.UI.Image.Type.Simple;
            coinImage.preserveAspect = true;
            var income = Text(Stretch(incomeCard, "home-income", 130, 325, 72, 44), "₹13.42M", 80,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(incomeCard, "Per Second", 550, 260, 75, 108), "/ sec", 34,
                Ink, TextAlignmentOptions.MidlineLeft);
            Text(Stretch(incomeCard, "Tap Kicker", 850, 20, 174, 15), "PER TAP", 29,
                Ink, TextAlignmentOptions.MidlineRight, true);
            var tapIcon = Image(Stretch(incomeCard, "Tap Icon", 739, 223, 76, 75), Color.white, false, Import("Icon Tap"));
            tapIcon.type = UnityEngine.UI.Image.Type.Simple;
            tapIcon.preserveAspect = true;
            var tapLabel = Text(Stretch(incomeCard, "home-tap", 835, 22, 68, 40), "₹250", 65,
                Ink, TextAlignmentOptions.MidlineRight, true);
            Image(Region(incomeCard, "Vertical Divider", .71f, .38f, .713f, .91f), Hex("D3C3A7"));

            var skillRow = New("Skills", content).GetComponent<RectTransform>();
            skillRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 238;
            var skillButtons = new Button[3];
            var skillStatus = new TMP_Text[3];
            var skillImages = new Image[3];
            string[] skillNames = { "Festival Rush", "Helping Hands", "Bulk Order" };
            for (int i = 0; i < 3; i++)
            {
                var box = Region(skillRow, "home-skill-" + i, i / 3f, 0, (i + 1) / 3f, 1);
                skillButtons[i] = InvisibleButton(box);
                var icon = Image(Stretch(box, "Icon", 80, 80, 93, 10), Color.white, false, skillArt[i]);
                icon.type = UnityEngine.UI.Image.Type.Simple;
                icon.preserveAspect = true;
                skillImages[i] = icon;
                var skillName = Text(Stretch(box, "Name", 4, 4, 50, 125), skillNames[i], 36,
                    Ink, TextAlignmentOptions.Center, true);
                skillName.textWrappingMode = TextWrappingModes.NoWrap;
                skillStatus[i] = Text(Stretch(box, "Status", 4, 4, 8, 172), "READY", 34,
                    Ink, TextAlignmentOptions.Center, true);
                skillStatus[i].textWrappingMode = TextWrappingModes.NoWrap;
            }

            var sponsor = FramedCard(content, "Sponsor", 136);
            var sponsorButton = InvisibleButton(Fill(sponsor, "sponsor-open"));
            var sponsorLabel = Text(Stretch(sponsor, "sponsor-status", 105, 20, 46, 15),
                "SPONSOR ACTIVE · 03:42", 38, Ink, TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(sponsor, "Sponsor Caption", 105, 20, 12, 80),
                "×2 sales · Available offer expiry", 30, Muted, TextAlignmentOptions.MidlineLeft);
            var sponsorIcon = Image(Stretch(sponsor, "Sponsor Icon", 15, 920, 20, 20), Color.white, false, skillArt[0]);
            sponsorIcon.type = UnityEngine.UI.Image.Type.Simple;
            sponsorIcon.preserveAspect = true;

            var nextAction = FramedCard(content, "Next Action", 140);
            Text(Region(nextAction, "Next Action Kicker", .035f, .65f, .65f, .95f), "STARTER OFFER", 32,
                Muted, TextAlignmentOptions.MidlineLeft, true);
            var nextGoal = Text(Region(nextAction, "Next Goal", .035f, .30f, .65f, .70f), "Stock Phuljhari", 38,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            nextGoal.textWrappingMode = TextWrappingModes.NoWrap;
            nextGoal.enableAutoSizing = true;
            nextGoal.fontSizeMin = 26;
            nextGoal.fontSizeMax = 38;
            var nextProgress = Text(Region(nextAction, "Next Progress", .035f, .07f, .65f, .37f), "Earn ₹30", 32,
                Muted, TextAlignmentOptions.MidlineLeft);
            nextProgress.textWrappingMode = TextWrappingModes.NoWrap;
            var nextProductsButton = Button(Region(nextAction, "Next Products Button", .69f, .12f, .975f, .88f), Blue);
            Text(Fill(nextProductsButton.transform, "Label"), "Tap to earn", 33,
                Color.white, TextAlignmentOptions.Center, true);

            var notice = Region(safe, "Notice", .07f, 0, .93f, 0);
            notice.offsetMin = new Vector2(0, 165);
            notice.offsetMax = new Vector2(0, 255);
            var noticeCanvas = notice.gameObject.AddComponent<Canvas>();
            noticeCanvas.overrideSorting = true;
            noticeCanvas.sortingOrder = 16;
            Image(notice, new Color(.06f, .12f, .22f, .96f));
            var noticeText = Text(Fill(notice, "Notice Text"), "", 34, IvoryColor,
                TextAlignmentOptions.Center, true);
            notice.gameObject.SetActive(false);

            var presenter = root.AddComponent<ImportedHomeScreen>();
            var data = new SerializedObject(presenter);
            data.FindProperty("homePanel").objectReferenceValue = panel.gameObject;
            data.FindProperty("stallTap").objectReferenceValue = tap;
            data.FindProperty("settingsButton").objectReferenceValue = settings;
            data.FindProperty("inspectSignButton").objectReferenceValue = inspectSign;
            Assign(data, "navigationButtons", navButtons);
            Assign(data, "navigationLabels", navLabels);
            Assign(data, "navigationIndicators", navIndicators);
            Assign(data, "shelfButtons", shelfButtons);
            Assign(data, "skillButtons", skillButtons);
            data.FindProperty("sponsorButton").objectReferenceValue = sponsorButton;
            Assign(data, "shelfArt", displays);
            Assign(data, "emptyShelfMarks", emptyMarks);
            Assign(data, "shelfNames", names);
            Assign(data, "skillStatus", skillStatus);
            Assign(data, "skillImages", skillImages);
            Assign(data, "skillArtwork", skillArt);
            data.FindProperty("lockedSkillIcon").objectReferenceValue = lockedSkillIcon;
            data.FindProperty("cashLabel").objectReferenceValue = cash;
            data.FindProperty("incomeLabel").objectReferenceValue = income;
            data.FindProperty("tapLabel").objectReferenceValue = tapLabel;
            data.FindProperty("sponsorLabel").objectReferenceValue = sponsorLabel;
            data.FindProperty("tapHintLabel").objectReferenceValue = hintText;
            data.FindProperty("nextActionLayout").objectReferenceValue = nextAction.GetComponent<LayoutElement>();
            data.FindProperty("nextGoalLabel").objectReferenceValue = nextGoal;
            data.FindProperty("nextProgressLabel").objectReferenceValue = nextProgress;
            data.FindProperty("nextProductsButton").objectReferenceValue = nextProductsButton;
            data.FindProperty("counterLabel").objectReferenceValue = shopSign;
            data.FindProperty("noticeLabel").objectReferenceValue = noticeText;
            data.FindProperty("noticeRoot").objectReferenceValue = notice.gameObject;
            Assign(data, "tapPopupRects", popupRects);
            Assign(data, "tapPopupLabels", popupLabels);
            data.FindProperty("celebrationImage").objectReferenceValue = celebration;
            data.ApplyModifiedPropertiesWithoutUndo();
            AuthorHtmlMotion.ApplyRoot(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Home and shared shop shell in " + ImporterPath);
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
            var image = Image(rect, new Color(1, 1, 1, .001f), true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Home integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Home integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "HomeImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedHomeScreen>() == null)
                throw new System.InvalidOperationException("Home canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedHomeCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Home canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Home prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedHomeCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Home screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("homeScreen").objectReferenceValue = instance.GetComponent<ImportedHomeScreen>();
            serialized.ApplyModifiedProperties();
            bool eventSystemExists = false;
            foreach (var root in main.GetRootGameObjects())
                if (root.GetComponent<EventSystem>() != null) eventSystemExists = true;
            if (!eventSystemExists)
            {
                var events = new GameObject("Imported Event System", typeof(EventSystem), typeof(StandaloneInputModule));
                SceneManager.MoveGameObjectToScene(events, main);
                Undo.RegisterCreatedObjectUndo(events, "Place UI Event System in Main");
            }
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Home in Main and bound GameController.");
        }

    }
}
