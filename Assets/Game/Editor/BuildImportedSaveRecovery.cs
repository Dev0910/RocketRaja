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
    /// <summary>Editor authoring for the blocking HTML Save Recovery screen.</summary>
    public static class BuildImportedSaveRecovery
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Save Recovery Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly Color RedInk = Hex("A82149");

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Save Recovery UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Save Recovery.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "SaveRecoveryImporterCanvas")
                    throw new System.InvalidOperationException("SaveRecoveryImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();

            var root = CanvasRoot("SaveRecoveryImporterCanvas", 110);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var dialog = Dialog(safe, "SaveRecoveryDialog", 1200);

            var header = Top(dialog, "Header", 163);
            Image(header, IvoryColor);
            Text(Stretch(header, "Recovery Title", 30, 30, 20, 20),
                "Your save needs attention", 56, RedInk, TextAlignmentOptions.MidlineLeft, true);
            Image(Bottom(header, "Double Rule", 5), Hex("255883"));

            var footer = Bottom(dialog, "Footer", 360);
            Image(footer, IvoryColor);
            Image(Top(footer, "Footer Rule", 5), Hex("255883"));
            var start = Button(Stretch(footer, "recovery-start", 31, 31, 192, 27), Red);
            Text(Fill(start.transform, "Start Label"), "Start a new local game", 40, Color.white,
                TextAlignmentOptions.Center, true);
            var exit = Button(Stretch(footer, "recovery-exit", 31, 31, 28, 192), Ivory);
            Text(Fill(exit.transform, "Exit Label"), "Keep files and exit", 39, Ink,
                TextAlignmentOptions.Center, true);

            var body = Stretch(dialog, "Scroll Body", 5, 5, 360, 163);
            var bodyImage = Image(body, IvoryColor, true);
            bodyImage.color = new Color(1, 1, 1, .001f);
            body.gameObject.AddComponent<RectMask2D>();
            var content = New("Content", body).GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 42, 35);
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

            var lead = New("Recovery Lead", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 58;
            Text(Fill(lead.transform, "Lead Text"), "Your saved files could not be read.", 35, Muted,
                TextAlignmentOptions.Center);
            Card(content, "Main save and backup are unreadable", "Your damaged files are preserved.", 200);
            Card(content, "Start a new local game", "Enable saving with a fresh game. This device has no cloud recovery.", 254);
            var errorRow = FramedCard(content, "recovery-message", 130);
            var errorText = Text(Stretch(errorRow, "Error Text", 30, 30, 15, 15), "", 32,
                Hex("71364B"), TextAlignmentOptions.Center, true);
            errorRow.gameObject.SetActive(false);

            var presenter = root.AddComponent<ImportedSaveRecoveryScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("startButton").objectReferenceValue = start;
            serialized.FindProperty("exitButton").objectReferenceValue = exit;
            serialized.FindProperty("errorRow").objectReferenceValue = errorRow.gameObject;
            serialized.FindProperty("errorText").objectReferenceValue = errorText;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored blocking Save Recovery modal in " + ImporterPath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Save Recovery integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "SaveRecoveryImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedSaveRecoveryScreen>() == null)
                throw new System.InvalidOperationException("Save Recovery canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedSaveRecoveryCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Save Recovery canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Save Recovery prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedSaveRecoveryCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Save Recovery screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("saveRecoveryScreen").objectReferenceValue = instance.GetComponent<ImportedSaveRecoveryScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Save Recovery screen in Main and bound GameController.");
        }

        static void Card(Transform parent, string title, string message, float height)
        {
            var card = FramedCard(parent, title + " Card", height);
            Text(Stretch(card, "Heading", 35, 35, height - 100, 25), title, 42, Ink,
                TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(card, "Description", 35, 35, 20, 90), message, 34, Muted,
                TextAlignmentOptions.TopLeft);
        }

    }
}
