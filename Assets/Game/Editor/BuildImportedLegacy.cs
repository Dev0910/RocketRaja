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
    /// <summary>Author the HTML Legacy panel and its twelve saved perk rows.</summary>
    public static class BuildImportedLegacy
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Legacy Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly string[] ProductIds = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Legacy UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Legacy.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "LegacyImporterCanvas")
                    throw new System.InvalidOperationException("LegacyImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            var icons = new Sprite[12];
            for (int i = 0; i < 12; i++)
            {
                string file = i < 8 ? "Product " + ProductIds[i] :
                    i == 8 ? "Icon Tap" : i == 9 ? "Icon Prestige" :
                    i == 10 ? "Icon Stats" : "Icon Skills";
                icons[i] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/" + file + ".png");
                if (icons[i] == null) throw new System.InvalidOperationException("Missing Legacy row art: " + file);
            }

            var root = CanvasRoot("LegacyImporterCanvas", 11);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var panel = Stretch(safe, "Legacy Panel", 0, 0, 145, 1400);
            Image(panel, IvoryColor, true);
            var fit = root.AddComponent<ImportedPanelFit>();
            var fitData = new SerializedObject(fit);
            fitData.FindProperty("safeArea").objectReferenceValue = safe;
            fitData.FindProperty("lowerPanel").objectReferenceValue = panel;
            fitData.ApplyModifiedPropertiesWithoutUndo();

            var header = Top(panel, "Header", 144);
            Image(header, IvoryColor);
            Text(Stretch(header, "Title", 28, 300, 15, 10), "Legacy", 58,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var collapse = InvisibleButton(Stretch(header, "collapse-Legacy", 800, 20, 5, 5));
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

            var reputation = FramedCard(content, "Reputation Summary", 246);
            var reputationLabel = Text(Stretch(reputation, "reputation-value", 300, 300, 100, 22), "12", 80,
                Hex("A02750"), TextAlignmentOptions.Center, true);
            Text(Stretch(reputation, "Reputation caption", 170, 170, 62, 127), "reputation available", 38,
                Ink, TextAlignmentOptions.Center, true);
            Text(Stretch(reputation, "Explanation", 60, 60, 16, 183),
                "Permanent perks. A brighter beginning after every prestige.", 28,
                Muted, TextAlignmentOptions.Center);

            var buttons = new Button[12];
            var names = new TMP_Text[12];
            var ranks = new TMP_Text[12];
            var effects = new TMP_Text[12];
            var labels = new TMP_Text[12];
            for (int i = 0; i < 12; i++)
            {
                var row = FramedCard(content, "Legacy Row " + i, 220);
                var art = Image(Stretch(row, "Perk Art", 10, 880, 30, 30), Color.white, false, icons[i]);
                art.type = UnityEngine.UI.Image.Type.Simple;
                art.preserveAspect = true;
                names[i] = Text(Stretch(row, "perk-name-" + i, 150, 365, 142, 17),
                    "Perk", 36, Ink, TextAlignmentOptions.MidlineLeft, true);
                ranks[i] = Text(Stretch(row, "perk-rank-" + i, 150, 365, 78, 85),
                    "Rank 1 / 5", 32, Ink, TextAlignmentOptions.MidlineLeft);
                effects[i] = Text(Stretch(row, "perk-effect-" + i, 150, 365, 25, 140),
                    "×2 product income per rank", 29, Muted, TextAlignmentOptions.MidlineLeft);
                buttons[i] = Button(Stretch(row, "buy-perk-" + i, 700, 18, 18, 18), Blue);
                labels[i] = Text(Fill(buttons[i].transform, "Price"), "2 reputation\nImprove perk", 32,
                    Color.white, TextAlignmentOptions.Center, true);
            }

            var presenter = root.AddComponent<ImportedLegacyScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("collapseButton").objectReferenceValue = collapse;
            serialized.FindProperty("reputationLabel").objectReferenceValue = reputationLabel;
            Assign(serialized, "purchaseButtons", buttons);
            Assign(serialized, "names", names);
            Assign(serialized, "ranks", ranks);
            Assign(serialized, "effects", effects);
            Assign(serialized, "purchaseLabels", labels);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            UiCompactTypography.Apply(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Legacy panel with twelve preplaced perk rows in " + ImporterPath);
        }

        static void Assign<T>(SerializedObject serialized, string name, T[] items) where T : Object
        {
            var property = serialized.FindProperty(name);
            property.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
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

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Legacy integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Legacy integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "LegacyImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedLegacyScreen>() == null)
                throw new System.InvalidOperationException("Legacy canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedLegacyCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Legacy canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Legacy prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedLegacyCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Legacy screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("legacyScreen").objectReferenceValue = instance.GetComponent<ImportedLegacyScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Legacy panel in Main and bound GameController.");
        }

    }
}
