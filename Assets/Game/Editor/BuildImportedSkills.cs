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
    /// <summary>Editor authoring for the HTML Skills guide and its saved states.</summary>
    public static class BuildImportedSkills
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Skills Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Skills UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Skills.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "SkillsImporterCanvas")
                    throw new System.InvalidOperationException("SkillsImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            ImportIcon("Helping Hands.png");
            ImportIcon("Bulk Order.png");
            var icons = new[] {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Burst.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Helping Hands.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Bulk Order.png")
            };
            foreach (var icon in icons)
                if (icon == null) throw new System.InvalidOperationException("Skills icon import failed.");

            var root = CanvasRoot("SkillsImporterCanvas", 99);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var dialog = Dialog(safe, "SkillsDialog", 1840);
            var fit = dialog.gameObject.AddComponent<ImportedDialogFit>();
            fit.SetPreferredHeight(1840);

            var header = Top(dialog, "Header", 183);
            Image(header, IvoryColor);
            Text(Stretch(header, "Skills Title", 30, 190, 20, 20), "Festival skills", 55,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var closeRect = New("close-Skills", header).GetComponent<RectTransform>();
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
            var back = Button(Stretch(footer, "back-Skills", 34, 34, 29, 34), Ivory);
            Text(Fill(back.transform, "Back Label"), "Return to Player", 41,
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
            layout.padding = new RectOffset(36, 36, 40, 28);
            layout.spacing = 17;
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

            var lead = New("Skills Lead", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 70;
            Text(Fill(lead.transform, "Lead Text"), "Activate your skills from Home", 34,
                Muted, TextAlignmentOptions.Center);
            var ranks = new TMP_Text[3];
            var effects = new TMP_Text[3];
            var cooldowns = new TMP_Text[3];
            var statuses = new TMP_Text[3];
            var faces = new Image[3];
            string[] names = { "Festival Rush", "Helping Hands", "Bulk Order" };
            string[] defaults = { "×2 automatic sales for 30s", "5 automatic taps/sec for 30s", "120 seconds of normal product sales" };
            string[] waits = { "Cooldown 5m", "Cooldown 5m", "Cooldown 10m" };
            for (int i = 0; i < 3; i++)
            {
                var card = FramedCard(content, names[i] + " Guide", 365);
                var iconRect = Stretch(card, "Icon", 35, 730, 65, 65);
                var icon = Image(iconRect, Color.white, false, icons[i]);
                icon.type = UnityEngine.UI.Image.Type.Simple;
                icon.preserveAspect = true;
                Text(Stretch(card, "Name", 206, 27, 250, 30), names[i], 42,
                    Ink, TextAlignmentOptions.MidlineLeft, true);
                ranks[i] = Text(Stretch(card, "guide-rank-" + i, 206, 27, 202, 105),
                    "Rank 1 / 4", 33, Ink, TextAlignmentOptions.MidlineLeft);
                effects[i] = Text(Stretch(card, "guide-effect-" + i, 206, 27, 140, 161),
                    defaults[i], 31, Muted, TextAlignmentOptions.MidlineLeft);
                cooldowns[i] = Text(Stretch(card, "guide-cooldown-" + i, 206, 27, 80, 223),
                    waits[i], 31, Muted, TextAlignmentOptions.MidlineLeft);
                var badge = Stretch(card, "guide-status-wrap-" + i, 206, 440, 25, 280);
                faces[i] = Image(badge, new Color32(231, 246, 232, 255));
                statuses[i] = Text(Stretch(badge, "guide-status-" + i, 16, 16, 2, 2), "READY", 29,
                    Ink, TextAlignmentOptions.Center, true);
            }
            var note = New("Skills Note", content);
            note.AddComponent<LayoutElement>().preferredHeight = 78;
            Text(Fill(note.transform, "Note Text"), "Buy skill ranks in Player unlockables.", 33,
                Ink, TextAlignmentOptions.Center);

            var presenter = root.AddComponent<ImportedSkillsScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = close;
            serialized.FindProperty("backButton").objectReferenceValue = back;
            Assign(serialized, "rankLabels", ranks);
            Assign(serialized, "effectLabels", effects);
            Assign(serialized, "cooldownLabels", cooldowns);
            Assign(serialized, "statusLabels", statuses);
            Assign(serialized, "statusFaces", faces);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Skills guide in " + ImporterPath);
        }

        static void Assign<T>(SerializedObject serialized, string name, T[] items) where T : Object
        {
            var property = serialized.FindProperty(name);
            property.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        static void ImportIcon(string name)
        {
            string path = "Assets/Game/ImportedUI/Art/" + name;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Cannot import " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Skills integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Skills integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "SkillsImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedSkillsScreen>() == null)
                throw new System.InvalidOperationException("Skills canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedSkillsCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Skills canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Skills prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedSkillsCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Skills guide in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("skillsScreen").objectReferenceValue = instance.GetComponent<ImportedSkillsScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Skills guide in Main and bound GameController.");
        }

    }
}
