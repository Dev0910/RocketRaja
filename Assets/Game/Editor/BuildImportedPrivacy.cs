using RocketRaja;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace RocketRaja.EditorTools
{
    /// <summary>One-time Editor authoring for the reviewed HTML Privacy dialog.</summary>
    public static class BuildImportedPrivacy
    {
        const string ScenePath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Privacy Screen.prefab";
        
        static readonly Color Ink = Hex("173B61");
        static readonly Color Ivory = Hex("FFF7E5");
        static readonly Color Paper = Hex("FFF9ED");
        static readonly Color Blue = Hex("255883");
        static TMP_FontAsset font;
        static Sprite rounded;

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Privacy UI must be authored outside Play Mode.");

            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer additively before building Privacy.");
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "PrivacyImporterCanvas")
                    throw new System.InvalidOperationException("PrivacyImporterCanvas already exists; inspect it before refining.");

            EditorSceneManager.SetActiveScene(scene);
            font = LoadOrCreateFont();
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            var canvasObject = New("PrivacyImporterCanvas", null);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2200);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0;
            canvasObject.AddComponent<GraphicRaycaster>();

            var backdrop = Fill(canvasObject.transform, "DimBackdrop");
            Image(backdrop, new Color(0.035f, 0.07f, 0.13f, 0.84f), true);
            var safeArea = Fill(canvasObject.transform, "SafeArea");
            safeArea.gameObject.AddComponent<ImportedSafeArea>();

            var dialog = New("PrivacyDialog", safeArea);
            var dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0, .5f);
            dialogRect.anchorMax = new Vector2(1, .5f);
            dialogRect.pivot = new Vector2(.5f, .5f);
            dialogRect.offsetMin = new Vector2(48, -700);
            dialogRect.offsetMax = new Vector2(-48, 700);
            Image(dialogRect, Blue, true);
            var surface = Stretch(dialogRect, "Ivory Surface", 5, 5, 5, 5);
            Image(surface, Ivory, false);

            var header = Edge(dialogRect, "Header", 0, 0, 0, 185, true);
            Image(header, Ivory, false);
            var title = Stretch(header, "Privacy Title", 30, 170, 25, 25);
            Text(title, "Privacy information", 55, Ink, TextAlignmentOptions.MidlineLeft, true);
            var close = Edge(header, "Close Button", 0, 12, 0, 150, true);
            close.anchorMin = close.anchorMax = new Vector2(1, 1);
            close.pivot = new Vector2(1, 1);
            close.anchoredPosition = new Vector2(-12, -12);
            close.sizeDelta = new Vector2(150, 150);
            var closeButton = Button(close, Paper, Blue);
            Text(Fill(close, "Close Icon"), "×", 95, Blue, TextAlignmentOptions.Center, true);
            var headerLine = Edge(header, "Double Rule", 0, 0, 0, 8, false);
            Image(headerLine, Blue, false);

            var footer = Edge(dialogRect, "Footer", 0, 0, 0, 200, false);
            Image(footer, Ivory, false);
            var footerLine = Edge(footer, "Footer Rule", 0, 0, 0, 5, true);
            Image(footerLine, Blue, false);
            var back = Stretch(footer, "Back to Settings", 40, 40, 28, 28);
            var backButton = Button(back, Paper, Blue);
            Text(Stretch(back, "Button Label", 14, 14, 12, 12), "Back to Settings", 41, Ink, TextAlignmentOptions.Center, true);

            var body = Stretch(dialogRect, "Scroll Body", 4, 4, 200, 185);
            var bodyImage = Image(body, Ivory, true);
            bodyImage.color = new Color(1, 1, 1, .001f);
            body.gameObject.AddComponent<RectMask2D>();
            var content = New("Content", body);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(36, 36, 42, 32);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = body.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = body;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 50;

            var lead = New("Lead", contentRect);
            lead.AddComponent<LayoutElement>().preferredHeight = 76;
            Text(Fill(lead.transform, "Lead Text"), "Guest saves stay local. Google sync is optional.", 38, Ink, TextAlignmentOptions.Center, false);
            Card(contentRect, "Progress and your account", "Guests stay on this device. Google players sync progress and basic profile data to Firebase. One device plays at a time. Analytics and crash collection are off by default.", 370);
            Card(contentRect, "Development build", "This is a test build. Production privacy information must be supplied before release.", 295);

            var presenter = canvasObject.AddComponent<ImportedPrivacyScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.FindProperty("backButton").objectReferenceValue = backButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            ApplyReadability(canvasObject);


            var eventSystem = new GameObject("Privacy EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create Privacy EventSystem");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Privacy dialog in " + ScenePath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Privacy integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ScenePath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before integrating Privacy.");

            GameObject importerCanvas = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "PrivacyImporterCanvas") importerCanvas = root;
            if (importerCanvas == null) throw new System.InvalidOperationException("The saved Privacy canvas is missing.");

            var oldPresenter = importerCanvas.GetComponentInChildren<ImportedPrivacyScreen>(true);
            if (oldPresenter == null) throw new System.InvalidOperationException("Privacy presenter is missing.");
            if (oldPresenter.gameObject != importerCanvas)
            {
                var current = new SerializedObject(oldPresenter);
                var consentRow = current.FindProperty("consentRow").objectReferenceValue;
                var consentButton = current.FindProperty("consentButton").objectReferenceValue;
                var closeButton = current.FindProperty("closeButton").objectReferenceValue;
                var backButton = current.FindProperty("backButton").objectReferenceValue;
                Undo.DestroyObjectImmediate(oldPresenter);
                var presenter = Undo.AddComponent<ImportedPrivacyScreen>(importerCanvas);
                var serialized = new SerializedObject(presenter);
                serialized.FindProperty("consentRow").objectReferenceValue = consentRow;
                serialized.FindProperty("consentButton").objectReferenceValue = consentButton;
                serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
                serialized.FindProperty("backButton").objectReferenceValue = backButton;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var safeArea = importerCanvas.transform.Find("SafeArea");
            if (safeArea.GetComponent<ImportedSafeArea>() == null)
                Undo.AddComponent<ImportedSafeArea>(safeArea.gameObject);
            var oldBackdrop = safeArea.Find("DimBackdrop");
            if (oldBackdrop != null)
            {
                Undo.SetTransformParent(oldBackdrop, importerCanvas.transform, "Move Privacy backdrop outside safe area");
                oldBackdrop.SetAsFirstSibling();
                var rect = oldBackdrop.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            EditorSceneManager.MarkSceneDirty(importer);
            EditorSceneManager.SaveScene(importer);

            var prefab = PrefabUtility.SaveAsPrefabAsset(importerCanvas, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Privacy prefab could not be saved.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedPrivacyCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Privacy canvas.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedPrivacyCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Privacy screen in Main");
            GameController game = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") game = root.GetComponent<GameController>();
            if (game == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var gameSerialized = new SerializedObject(game);
            gameSerialized.FindProperty("privacyScreen").objectReferenceValue = instance.GetComponent<ImportedPrivacyScreen>();
            gameSerialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Privacy screen in Main and bound GameController.");
        }

        public static void RefineSafeArea()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Safe-area authoring requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ScenePath);
            if (!importer.IsValid() || !importer.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before refining Privacy.");
            GameObject canvas = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "PrivacyImporterCanvas") canvas = root;
            if (canvas == null) throw new System.InvalidOperationException("PrivacyImporterCanvas is missing.");
            var safe = canvas.transform.Find("SafeArea");
            if (safe.GetComponent<ImportedSafeArea>() == null)
                Undo.AddComponent<ImportedSafeArea>(safe.gameObject);
            var backdrop = safe.Find("DimBackdrop");
            if (backdrop != null)
            {
                Undo.SetTransformParent(backdrop, canvas.transform, "Move Privacy backdrop outside safe area");
                backdrop.SetAsFirstSibling();
                var rect = backdrop.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            EditorSceneManager.MarkSceneDirty(importer);
            EditorSceneManager.SaveScene(importer);
            if (PrefabUtility.SaveAsPrefabAsset(canvas, PrefabPath) == null)
                throw new System.InvalidOperationException("Privacy prefab could not be updated.");
            AssetDatabase.SaveAssets();
        }


        /// <summary>Keeps the complete account explanation readable in saved privacy cards.</summary>
        public static void ApplyReadability(GameObject root)
        {
            foreach (var label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label.name != "Description") continue;
                var paper = label.transform.parent as RectTransform;
                var card = paper != null ? paper.parent : null;
                var layout = card != null ? card.GetComponent<LayoutElement>() : null;
                if (layout == null) continue;
                float height = card.name.StartsWith("Progress") ? 500 :
                    card.name.StartsWith("Optional") ? 260 : 340;
                layout.preferredHeight = height;
                var heading = paper.Find("Heading").GetComponent<TMP_Text>();
                PlaceCopy(heading, 26, 66, 40);
                PlaceCopy(label, 110, height - 136, 36);
                EditorUtility.SetDirty(layout);
            }
        }

        static void PlaceCopy(TMP_Text text, float top, float height, float size)
        {
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.offsetMin = new Vector2(32, -top - height);
            rect.offsetMax = new Vector2(-25, -top);
            text.fontSize = size;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            EditorUtility.SetDirty(text);
        }

        static TMP_FontAsset LoadOrCreateFont() => GameTypography.Body;

        static void Card(Transform parent, string heading, string copy, float height)
        {
            var card = New(heading + " Card", parent);
            card.AddComponent<LayoutElement>().preferredHeight = height;
            Image(card.GetComponent<RectTransform>(), Blue, false);
            var inset = Stretch(card.transform, "Paper", 3, 3, 3, 3);
            Image(inset, Paper, false);
            Text(Stretch(inset, "Heading", 32, 25, height - 115, 25), heading, 40, Ink, TextAlignmentOptions.MidlineLeft, true);
            Text(Stretch(inset, "Description", 32, 25, 20, 100), copy, 34, Ink, TextAlignmentOptions.TopLeft, false);
        }

        static GameObject New(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(go, "Author Privacy UI");
            return go;
        }

        static RectTransform Fill(Transform parent, string name)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        static RectTransform Stretch(Transform parent, string name, float left, float right, float bottom, float top)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        static RectTransform Edge(Transform parent, string name, float x, float y, float width, float height, bool top)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = top ? new Vector2(.5f, 1) : new Vector2(.5f, 0);
            rect.pivot = top ? new Vector2(.5f, 1) : new Vector2(.5f, 0);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            if (width == 0)
            {
                rect.anchorMin = new Vector2(0, top ? 1 : 0);
                rect.anchorMax = new Vector2(1, top ? 1 : 0);
                rect.offsetMin = new Vector2(0, top ? -height : 0);
                rect.offsetMax = new Vector2(0, top ? 0 : height);
            }
            return rect;
        }

        static Image Image(RectTransform rect, Color color, bool raycast)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = rounded;
            image.type = rounded == null ? UnityEngine.UI.Image.Type.Simple : UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        static Button Button(RectTransform rect, Color fill, Color border)
        {
            Image(rect, border, true);
            var inner = Stretch(rect, "Fill", 7, 7, 7, 7);
            Image(inner, fill, false);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.highlightedColor = Hex("E7D5B0");
            colors.pressedColor = Hex("C6DDDC");
            colors.disabledColor = new Color(.7f, .7f, .7f, .6f);
            button.colors = colors;
            return button;
        }

        static TextMeshProUGUI Text(RectTransform rect, string value, float size, Color color, TextAlignmentOptions alignment, bool bold)
        {
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.text = value;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            GameTypography.Configure(text);
            return text;
        }

        static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out var color);
            return color;
        }
    }
}
