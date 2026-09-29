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
    /// <summary>Editor authoring for the HTML Offline offer and its states.</summary>
    public static class BuildImportedOffline
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Offline Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly Color RedInk = Hex("A82149");

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Offline UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Offline.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "OfflineImporterCanvas")
                    throw new System.InvalidOperationException("OfflineImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();
            var coins = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Coins.png");
            var sprig = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Ivory Sprig Gold.png");
            if (coins == null || sprig == null)
                throw new System.InvalidOperationException("Exact local Offline artwork is missing.");

            var root = CanvasRoot("OfflineImporterCanvas", 95);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var dialog = Dialog(safe, "OfflineDialog", 1570);
            var fit = dialog.gameObject.AddComponent<ImportedDialogFit>();

            var header = Top(dialog, "Header", 183);
            Image(header, IvoryColor);
            Text(Stretch(header, "Offline Title", 30, 190, 20, 20), "Welcome back", 55, Ink,
                TextAlignmentOptions.MidlineLeft, true);
            var closeRect = New("close-Offline", header).GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-13, -13);
            closeRect.sizeDelta = new Vector2(158, 158);
            var close = Button(closeRect, Blue);
            Text(Fill(closeRect, "Close Icon"), "×", 90, Color.white, TextAlignmentOptions.Center, true);
            Image(Bottom(header, "Double Rule", 5), Hex("255883"));

            var footer = Bottom(dialog, "Footer", 360);
            Image(footer, IvoryColor);
            Image(Top(footer, "Footer Rule", 5), Hex("255883"));
            var video = Button(Stretch(footer, "offline-video", 31, 31, 192, 27), Blue);
            var videoFace = video.GetComponent<Image>();
            Text(Fill(video.transform, "Video Label"), "Optional video · Double reward", 39,
                Color.white, TextAlignmentOptions.Center, true);
            var next = Button(Stretch(footer, "back-Offline", 31, 31, 28, 192), Ivory);
            Text(Fill(next.transform, "Continue Label"), "Continue without video", 39,
                Ink, TextAlignmentOptions.Center, true);

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

            var lead = New("Offline Lead", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 65;
            Text(Fill(lead.transform, "Lead Text"), "Your shop kept the festival going.", 35,
                Muted, TextAlignmentOptions.Center);

            var hero = FramedCard(content, "Offline Reward", 480);
            var coinRect = New("Coins", hero).GetComponent<RectTransform>();
            coinRect.anchorMin = coinRect.anchorMax = new Vector2(.5f, 1);
            coinRect.pivot = new Vector2(.5f, 1);
            coinRect.sizeDelta = new Vector2(190, 190);
            coinRect.anchoredPosition = new Vector2(0, -55);
            var coinGraphic = Image(coinRect, Color.white, false, coins);
            coinGraphic.type = UnityEngine.UI.Image.Type.Simple;
            coinGraphic.preserveAspect = true;
            var sprigRect = New("Gold Sprig", hero).GetComponent<RectTransform>();
            sprigRect.anchorMin = sprigRect.anchorMax = new Vector2(1, .5f);
            sprigRect.pivot = new Vector2(1, .5f);
            sprigRect.sizeDelta = new Vector2(84, 180);
            sprigRect.anchoredPosition = new Vector2(-32, 0);
            var sprigGraphic = Image(sprigRect, Color.white, false, sprig);
            sprigGraphic.type = UnityEngine.UI.Image.Type.Simple;
            sprigGraphic.preserveAspect = true;
            var reward = Text(Stretch(hero, "offline-reward", 35, 35, 112, 260), "₹24.8K", 88,
                RedInk, TextAlignmentOptions.Center, true);
            reward.enableAutoSizing = true;
            reward.fontSizeMin = 50;
            reward.fontSizeMax = 88;
            Text(Stretch(hero, "Credited Text", 35, 35, 37, 390), "Already added to your wallet.", 35,
                Muted, TextAlignmentOptions.Center);

            var sales = FramedCard(content, "After-hours Sales", 270);
            Text(Stretch(sales, "Heading", 35, 35, 181, 27), "After-hours sales", 43,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(sales, "Description", 35, 35, 111, 90), "50% of normal product sales.", 35,
                Muted, TextAlignmentOptions.MidlineLeft);
            var cap = Text(Stretch(sales, "offline-cap", 35, 35, 38, 158),
                "Up to 8 hours while you are away.", 35, Muted, TextAlignmentOptions.MidlineLeft);

            var offerRow = New("Offer Row", content);
            offerRow.AddComponent<LayoutElement>().preferredHeight = 70;
            var offer = Text(Fill(offerRow.transform, "offline-offer"),
                "Watch an optional video to add another ₹24.8K.", 34, Ink, TextAlignmentOptions.Center);
            var messageRow = New("offline-message", content);
            messageRow.AddComponent<LayoutElement>().preferredHeight = 105;
            var messageBackground = Image(Fill(messageRow.transform, "Message Background"), Hex("F9E5DC"));
            messageBackground.color = new Color(249 / 255f, 229 / 255f, 220 / 255f, .5f);
            var message = Text(Stretch(messageRow.transform, "Message Text", 28, 22, 12, 12), "", 31,
                Hex("71364B"), TextAlignmentOptions.MidlineLeft);
            messageRow.SetActive(false);

            var presenter = root.AddComponent<ImportedOfflineScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = close;
            serialized.FindProperty("videoButton").objectReferenceValue = video;
            serialized.FindProperty("continueButton").objectReferenceValue = next;
            serialized.FindProperty("videoFace").objectReferenceValue = videoFace;
            serialized.FindProperty("rewardText").objectReferenceValue = reward;
            serialized.FindProperty("capText").objectReferenceValue = cap;
            serialized.FindProperty("offerText").objectReferenceValue = offer;
            serialized.FindProperty("messageRow").objectReferenceValue = messageRow;
            serialized.FindProperty("messageText").objectReferenceValue = message;
            serialized.FindProperty("enabledFace").objectReferenceValue = Blue;
            serialized.FindProperty("disabledFace").objectReferenceValue = Sand;
            serialized.FindProperty("dialogFit").objectReferenceValue = fit;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Offline offer in " + ImporterPath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Offline integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Offline integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "OfflineImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedOfflineScreen>() == null)
                throw new System.InvalidOperationException("Offline canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedOfflineCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Offline canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Offline prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedOfflineCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Offline screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("offlineScreen").objectReferenceValue = instance.GetComponent<ImportedOfflineScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Offline screen in Main and bound GameController.");
        }

    }
}
