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
    /// <summary>Editor authoring for the HTML Statistics modal.</summary>
    public static class BuildImportedStatistics
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Statistics Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Statistics UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Statistics.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "StatisticsImporterCanvas")
                    throw new System.InvalidOperationException("StatisticsImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();

            var root = CanvasRoot("StatisticsImporterCanvas", 98);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var dialog = Dialog(safe, "StatisticsDialog", 1405);

            var header = Top(dialog, "Header", 183);
            Image(header, IvoryColor);
            Text(Stretch(header, "Statistics Title", 30, 190, 20, 20), "Your shop in numbers", 55,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var closeRect = New("close-Statistics", header).GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-13, -13);
            closeRect.sizeDelta = new Vector2(158, 158);
            var close = Button(closeRect, Blue);
            Text(Fill(closeRect, "Close Icon"), "×", 90, Color.white,
                TextAlignmentOptions.Center, true);
            Image(Bottom(header, "Double Rule", 5), Hex("255883"));

            var footer = Bottom(dialog, "Footer", 202);
            Image(footer, IvoryColor);
            Image(Top(footer, "Footer Rule", 5), Hex("255883"));
            var back = Button(Stretch(footer, "back-Statistics", 34, 34, 29, 34), Ivory);
            Text(Fill(back.transform, "Back Label"), "Return to Player", 41,
                Ink, TextAlignmentOptions.Center, true);

            var body = Stretch(dialog, "Scroll Body", 5, 5, 202, 183);
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
            layout.padding = new RectOffset(39, 39, 43, 35);
            layout.spacing = 0;
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

            var lead = New("Statistics Lead", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 65;
            Text(Fill(lead.transform, "Lead Text"), "Every little spark adds up", 36,
                Muted, TextAlignmentOptions.Center);
            var firstGap = New("Lead Gap", content);
            firstGap.AddComponent<LayoutElement>().preferredHeight = 40;
            Section(content, "THIS RUN");
            var values = new TMP_Text[6];
            values[0] = Stat(content, "Total earned", "₹3.8M", "run-total");
            values[1] = Stat(content, "Prestige-eligible", "₹3.6M", "run-eligible");
            var gap = New("Section Gap", content);
            gap.AddComponent<LayoutElement>().preferredHeight = 77;
            Section(content, "LIFETIME");
            values[2] = Stat(content, "Total earned", "₹24.6M", "lifetime-total");
            values[3] = Stat(content, "Manual taps", "12,480", "manual-taps");
            values[4] = Stat(content, "Prestiges", "3", "prestiges");
            values[5] = Stat(content, "Reputation earned", "24", "lifetime-reputation");

            var presenter = root.AddComponent<ImportedStatisticsScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = close;
            serialized.FindProperty("backButton").objectReferenceValue = back;
            var valuesProperty = serialized.FindProperty("values");
            valuesProperty.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                valuesProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Statistics modal in " + ImporterPath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Statistics integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Statistics integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "StatisticsImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedStatisticsScreen>() == null)
                throw new System.InvalidOperationException("Statistics canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedStatisticsCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Statistics canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Statistics prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedStatisticsCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Statistics screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("statisticsScreen").objectReferenceValue = instance.GetComponent<ImportedStatisticsScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Statistics screen in Main and bound GameController.");
        }

        static void Section(Transform content, string title)
        {
            var row = New(title + " Heading", content).GetComponent<RectTransform>();
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 65;
            Text(Fill(row, "Label"), title, 39,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            Image(Bottom(row, "Rule", 3), Hex("65859F"));
        }

        static TMP_Text Stat(Transform content, string label, string value, string name)
        {
            var row = New(label + " Row", content).GetComponent<RectTransform>();
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 104;
            Text(Stretch(row, "Label", 10, 290, 13, 12), label, 36,
                Muted, TextAlignmentOptions.MidlineLeft);
            var output = Text(Stretch(row, name, 550, 10, 13, 12), value, 39,
                Ink, TextAlignmentOptions.MidlineRight, true);
            Image(Bottom(row, "Separator", 2), Hex("D8D0B9"));
            return output;
        }

    }
}
