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
    /// <summary>Editor authoring for the HTML Sponsor modal.</summary>
    public static class BuildImportedSponsor
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Sponsor Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly Color RedInk = Hex("A82149");

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Sponsor UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Sponsor.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "SponsorImporterCanvas")
                    throw new System.InvalidOperationException("SponsorImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            var burst = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Burst.png");
            var sprig = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Ivory Sprig Gold.png");
            if (burst == null || sprig == null)
                throw new System.InvalidOperationException("Exact local Sponsor artwork is missing.");

            var root = CanvasRoot("SponsorImporterCanvas", 96);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var dialog = Dialog(safe, "SponsorDialog", 1730);
            var fit = dialog.gameObject.AddComponent<ImportedDialogFit>();
            fit.SetPreferredHeight(1730);

            var header = Top(dialog, "Header", 183);
            Image(header, IvoryColor);
            Text(Stretch(header, "Sponsor Title", 30, 190, 20, 20), "Festival Sponsor", 55,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var closeRect = New("close-Sponsor", header).GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-13, -13);
            closeRect.sizeDelta = new Vector2(158, 158);
            var close = Button(closeRect, Blue);
            Text(Fill(closeRect, "Close Icon"), "×", 90, Color.white,
                TextAlignmentOptions.Center, true);
            Image(Bottom(header, "Double Rule", 5), Hex("255883"));

            var footer = Bottom(dialog, "Footer", 360);
            Image(footer, IvoryColor);
            Image(Top(footer, "Footer Rule", 5), Hex("255883"));
            var video = Button(Stretch(footer, "sponsor-video", 31, 31, 192, 27), Sand);
            var videoFace = video.GetComponent<Image>();
            Text(Fill(video.transform, "Video Label"), "Watch video", 39, Ink,
                TextAlignmentOptions.Center, true);
            var back = Button(Stretch(footer, "back-Sponsor", 31, 31, 28, 192), Ivory);
            Text(Fill(back.transform, "Back Label"), "Keep playing", 39, Ink,
                TextAlignmentOptions.Center, true);

            var body = Stretch(dialog, "Scroll Body", 5, 5, 360, 183);
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
            layout.padding = new RectOffset(32, 32, 42, 30);
            layout.spacing = 20;
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

            var lead = New("Sponsor Lead", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 65;
            Text(Fill(lead.transform, "Lead Text"), "A little help for your festival.", 35,
                Muted, TextAlignmentOptions.Center);
            var hero = FramedCard(content, "Sponsor Reward", 520);
            var burstRect = New("Burst", hero).GetComponent<RectTransform>();
            burstRect.anchorMin = burstRect.anchorMax = new Vector2(.5f, 1);
            burstRect.pivot = new Vector2(.5f, 1);
            burstRect.sizeDelta = new Vector2(190, 190);
            burstRect.anchoredPosition = new Vector2(0, -45);
            var burstGraphic = Image(burstRect, Color.white, false, burst);
            burstGraphic.type = UnityEngine.UI.Image.Type.Simple;
            burstGraphic.preserveAspect = true;
            var sprigRect = New("Gold Sprig", hero).GetComponent<RectTransform>();
            sprigRect.anchorMin = sprigRect.anchorMax = new Vector2(1, .5f);
            sprigRect.pivot = new Vector2(1, .5f);
            sprigRect.sizeDelta = new Vector2(84, 180);
            sprigRect.anchoredPosition = new Vector2(-32, 0);
            var sprigGraphic = Image(sprigRect, Color.white, false, sprig);
            sprigGraphic.type = UnityEngine.UI.Image.Type.Simple;
            sprigGraphic.preserveAspect = true;
            Text(Stretch(hero, "Boost", 35, 35, 145, 235), "×2", 88,
                RedInk, TextAlignmentOptions.Center, true);
            Text(Stretch(hero, "Automatic Sales", 35, 35, 103, 350), "automatic sales", 42,
                Ink, TextAlignmentOptions.Center, true);
            Text(Stretch(hero, "Duration", 35, 35, 40, 425), "For five minutes", 35,
                Muted, TextAlignmentOptions.Center);

            var details = FramedCard(content, "Optional Video Details", 270);
            Text(Stretch(details, "Heading", 35, 35, 181, 27), "Optional video", 43,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(details, "Description", 35, 35, 103, 90),
                "Complete the video to receive your bonus.", 35, Muted,
                TextAlignmentOptions.MidlineLeft);
            Text(Stretch(details, "Reputation Note", 35, 35, 30, 169),
                "Extra income does not earn reputation.", 35, Muted,
                TextAlignmentOptions.MidlineLeft);

            var messageRow = New("sponsor-message", content);
            messageRow.AddComponent<LayoutElement>().preferredHeight = 145;
            Image(Fill(messageRow.transform, "Message Background"), Hex("F9E5DC"));
            var message = Text(Stretch(messageRow.transform, "Message Text", 27, 20, 16, 16),
                "Sponsor active · 03:42 remaining. Come back after it expires.", 32,
                Hex("71364B"), TextAlignmentOptions.MidlineLeft);

            var presenter = root.AddComponent<ImportedSponsorScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = close;
            serialized.FindProperty("videoButton").objectReferenceValue = video;
            serialized.FindProperty("backButton").objectReferenceValue = back;
            serialized.FindProperty("videoFace").objectReferenceValue = videoFace;
            serialized.FindProperty("messageRow").objectReferenceValue = messageRow;
            serialized.FindProperty("messageText").objectReferenceValue = message;
            serialized.FindProperty("enabledFace").objectReferenceValue = Blue;
            serialized.FindProperty("disabledFace").objectReferenceValue = Sand;
            serialized.FindProperty("dialogFit").objectReferenceValue = fit;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Sponsor modal in " + ImporterPath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Sponsor integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Sponsor integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "SponsorImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedSponsorScreen>() == null)
                throw new System.InvalidOperationException("Sponsor canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedSponsorCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Sponsor canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Sponsor prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedSponsorCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Sponsor screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("sponsorScreen").objectReferenceValue = instance.GetComponent<ImportedSponsorScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Sponsor screen in Main and bound GameController.");
        }

    }
}
