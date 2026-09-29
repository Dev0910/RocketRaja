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
    /// <summary>Editor authoring for locked, ready and stale Prestige states.</summary>
    public static class BuildImportedPrestige
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Prestige Screen.prefab";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color IvoryColor = Hex("FFF4DB");
        static readonly Color RedInk = Hex("A82149");

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Prestige UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Prestige.");
            foreach (var existing in scene.GetRootGameObjects())
                if (existing.name == "PrestigeImporterCanvas")
                    throw new System.InvalidOperationException("PrestigeImporterCanvas already exists.");
            EditorSceneManager.SetActiveScene(scene);
            Load();

            var root = CanvasRoot("PrestigeImporterCanvas", 97);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();
            var dialog = Dialog(safe, "PrestigeDialog", 1480);
            var fit = dialog.gameObject.AddComponent<ImportedDialogFit>();
            fit.SetPreferredHeight(1480);

            var header = Top(dialog, "Header", 183);
            Image(header, IvoryColor);
            Text(Stretch(header, "Prestige Title", 30, 190, 20, 20), "Begin again, brighter", 55,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var closeRect = New("close-Prestige", header).GetComponent<RectTransform>();
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
            var confirm = Button(Stretch(footer, "confirm-prestige", 31, 31, 192, 27), Sand);
            confirm.interactable = false;
            var confirmFace = confirm.GetComponent<Image>();
            var confirmText = Text(Fill(confirm.transform, "Confirm Label"), "Prestige locked", 39,
                Hex("514B3C"), TextAlignmentOptions.Center, true);
            var back = Button(Stretch(footer, "back-Prestige", 31, 31, 28, 192), Ivory);
            Text(Fill(back.transform, "Back Label"), "Keep growing", 39, Ink,
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

            var lead = New("Prestige Lead", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 65;
            var leadText = Text(Fill(lead.transform, "prestige-reward"),
                "Grow your shop to unlock prestige", 35, Muted, TextAlignmentOptions.Center);

            var locked = FramedCard(content, "prestige-locked", 385);
            Text(Stretch(locked, "Heading", 35, 35, 295, 25), "A little more to grow", 42,
                Ink, TextAlignmentOptions.MidlineLeft, true);
            var requirement = Text(Stretch(locked, "Requirement", 35, 35, 160, 100),
                "Own all eight product families and earn ₹170Q prestige-eligible income this run.",
                34, Muted, TextAlignmentOptions.TopLeft);
            var eligible = Text(Stretch(locked, "prestige-eligible", 35, 35, 95, 236),
                "Current eligible income · ₹0", 34, Muted, TextAlignmentOptions.MidlineLeft);
            Text(Stretch(locked, "Advertising Note", 35, 35, 20, 305),
                "Advertising bonuses do not count.", 34, Muted, TextAlignmentOptions.MidlineLeft);

            var reset = FramedCard(content, "prestige-reset", 255);
            reset.GetComponent<Image>().color = Hex("FAE8DF");
            Text(Stretch(reset, "Heading", 35, 35, 180, 20), "RESET", 43, RedInk,
                TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(reset, "Description", 35, 35, 25, 82),
                "Cash, shelves, product and tap levels, run upgrades, skills, stall appearance, sponsor and offline offer.",
                34, Muted, TextAlignmentOptions.TopLeft);
            reset.gameObject.SetActive(false);

            var keep = FramedCard(content, "Prestige Keep", 255);
            keep.GetComponent<Image>().color = Hex("EFF2DF");
            Text(Stretch(keep, "Heading", 35, 35, 180, 20), "KEEP", 43, Hex("35644C"),
                TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(keep, "Description", 35, 35, 25, 82),
                "Reputation, Legacy perks, lifetime statistics and settings.", 34, Muted,
                TextAlignmentOptions.TopLeft);

            var note = New("Decision Note", content);
            note.AddComponent<LayoutElement>().preferredHeight = 65;
            Text(Fill(note.transform, "Note Text"), "Take your time. Your shop pauses while you decide.",
                34, Ink, TextAlignmentOptions.Center);
            var failure = New("prestige-failure", content);
            failure.AddComponent<LayoutElement>().preferredHeight = 110;
            Image(Fill(failure.transform, "Message Background"), Hex("F9E5DC"));
            Text(Stretch(failure.transform, "Message Text", 26, 20, 14, 14),
                "Your rewards changed. Please review prestige again.", 31,
                Hex("71364B"), TextAlignmentOptions.MidlineLeft);
            failure.SetActive(false);

            var presenter = root.AddComponent<ImportedPrestigeScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = close;
            serialized.FindProperty("confirmButton").objectReferenceValue = confirm;
            serialized.FindProperty("backButton").objectReferenceValue = back;
            serialized.FindProperty("confirmFace").objectReferenceValue = confirmFace;
            serialized.FindProperty("leadText").objectReferenceValue = leadText;
            serialized.FindProperty("confirmText").objectReferenceValue = confirmText;
            serialized.FindProperty("requirementText").objectReferenceValue = requirement;
            serialized.FindProperty("eligibleText").objectReferenceValue = eligible;
            serialized.FindProperty("lockedCard").objectReferenceValue = locked.gameObject;
            serialized.FindProperty("resetCard").objectReferenceValue = reset.gameObject;
            serialized.FindProperty("failureRow").objectReferenceValue = failure;
            serialized.FindProperty("readyFace").objectReferenceValue = Red;
            serialized.FindProperty("disabledFace").objectReferenceValue = Sand;
            serialized.FindProperty("dialogFit").objectReferenceValue = fit;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Prestige states in " + ImporterPath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Prestige integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before Prestige integration.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "PrestigeImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedPrestigeScreen>() == null)
                throw new System.InvalidOperationException("Prestige canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedPrestigeCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Prestige canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Prestige prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedPrestigeCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Prestige screen in Main");
            GameController owner = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") owner = root.GetComponent<GameController>();
            if (owner == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(owner);
            serialized.FindProperty("prestigeScreen").objectReferenceValue = instance.GetComponent<ImportedPrestigeScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Prestige screen in Main and bound GameController.");
        }

    }
}
