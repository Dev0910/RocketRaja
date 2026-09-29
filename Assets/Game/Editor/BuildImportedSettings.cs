using RocketRaja;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.EditorTools
{
    /// <summary>One-time Editor authoring for the reviewed HTML Settings modal.</summary>
    public static class BuildImportedSettings
    {
        const string ImporterPath = "Assets/Scenes/HTML Importer.unity";
        const string MainPath = "Assets/Scenes/Main.unity";
        const string PrefabPath = "Assets/Game/ImportedUI/Settings Screen.prefab";
        const string ArtPath = "Assets/Game/ImportedUI/Art/";
        static readonly Color Ink = Hex("164B80");
        static readonly Color Muted = Hex("375A77");
        static readonly Color Ivory = Hex("FFF4DB");
        static readonly Color Paper = Hex("FFF5DE");
        static readonly Color Blue = Hex("205685");
        static TMP_FontAsset font;
        static Sprite folkBlue, folkIvory, rowFrame, rounded;

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Settings UI must be authored outside Play Mode.");
            var scene = SceneManager.GetSceneByPath(ImporterPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer before building Settings.");
            foreach (var existingRoot in scene.GetRootGameObjects())
                if (existingRoot.name == "SettingsImporterCanvas")
                    throw new System.InvalidOperationException("SettingsImporterCanvas already exists.");

            EditorSceneManager.SetActiveScene(scene);
            font = GameTypography.Body;
            folkBlue = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "Folk Blue.png");
            folkIvory = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "Folk Ivory.png");
            rowFrame = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "Ivory Row Frame.png");
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (font == null || folkBlue == null || folkIvory == null || rowFrame == null)
                throw new System.InvalidOperationException("Settings font or exact local ticket artwork is missing.");

            var root = New("SettingsImporterCanvas", null);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2200);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0;
            root.AddComponent<GraphicRaycaster>();
            AddImage(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea");
            safe.gameObject.AddComponent<ImportedSafeArea>();

            var dialog = New("SettingsDialog", safe).GetComponent<RectTransform>();
            dialog.anchorMin = new Vector2(0, .5f);
            dialog.anchorMax = new Vector2(1, .5f);
            dialog.pivot = new Vector2(.5f, .5f);
            dialog.offsetMin = new Vector2(44, -714);
            dialog.offsetMax = new Vector2(-44, 714);
            AddImage(dialog, Blue, true);
            AddImage(Stretch(dialog, "Ivory Surface", 5, 5, 5, 5), Ivory, false);

            var header = Top(dialog, "Header", 183);
            AddImage(header, Ivory, false);
            AddText(Stretch(header, "Settings Title", 29, 190, 21, 21),
                "Make yourself comfortable", 54, Ink, TextAlignmentOptions.MidlineLeft, true);
            var closeRect = New("Close Settings", header).GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-13, -13);
            closeRect.sizeDelta = new Vector2(158, 158);
            var close = AddButton(closeRect, folkBlue);
            AddText(Fill(closeRect, "Close Icon"), "×", 90, Color.white, TextAlignmentOptions.Center, true);
            AddImage(Bottom(header, "Double Rule", 5), Blue, false);

            var footer = Bottom(dialog, "Footer", 202);
            AddImage(footer, Ivory, false);
            AddImage(Top(footer, "Footer Rule", 5), Blue, false);
            var back = AddButton(Stretch(footer, "Back to my shop", 34, 34, 29, 34), folkBlue);
            AddText(Fill(back.transform, "Button Label"), "Back to my shop", 41, Color.white, TextAlignmentOptions.Center, true);

            var body = Stretch(dialog, "Scroll Body", 5, 5, 202, 183);
            var bodyImage = AddImage(body, Ivory, true);
            bodyImage.color = new Color(1, 1, 1, .001f);
            body.gameObject.AddComponent<RectMask2D>();
            var content = New("Content", body).GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(31, 31, 42, 38);
            layout.spacing = 21;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = body.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = body;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 50;

            var lead = New("Settings Lead", content);
            lead.AddComponent<LayoutElement>().preferredHeight = 65;
            AddText(Fill(lead.transform, "Lead Text"), "Your festival, your way", 37, Muted,
                TextAlignmentOptions.Center, false);

            var buttons = new Button[3];
            var faces = new Image[3];
            var states = new TMP_Text[3];
            var dots = new Image[3];
            SettingRow(content, "Music", "Gentle festival melodies", "setting-music", 0, buttons, faces, states, dots);
            SettingRow(content, "Sound", "Taps and celebrations", "setting-sound", 1, buttons, faces, states, dots);
            SettingRow(content, "Reduced motion", "Quieter movement and effects", "setting-motion", 2, buttons, faces, states, dots);
            var account = ActionRow(content, "Google account", "Guest · saved on this device", "settings-account", "Open");
            var privacy = ActionRow(content, "Privacy information", "Local guests or optional Google sync.",
                "settings-privacy", "Open");
            var consentContainer = New("settings-consent-row", content);
            consentContainer.AddComponent<LayoutElement>().preferredHeight = 190;
            var consent = ActionRow(consentContainer.transform, "Advertising privacy choices", "Manage your preferences.",
                "settings-consent", "Open", true);
            consentContainer.SetActive(false);

            var presenter = root.AddComponent<ImportedSettingsScreen>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("closeButton").objectReferenceValue = close;
            serialized.FindProperty("backButton").objectReferenceValue = back;
            serialized.FindProperty("privacyButton").objectReferenceValue = privacy;
            serialized.FindProperty("accountButton").objectReferenceValue = account;
            serialized.FindProperty("accountStatus").objectReferenceValue = account.transform.parent.Find("Description").GetComponent<TMP_Text>();
            serialized.FindProperty("consentButton").objectReferenceValue = consent;
            serialized.FindProperty("consentRow").objectReferenceValue = consentContainer;
            serialized.FindProperty("enabledFace").objectReferenceValue = folkBlue;
            serialized.FindProperty("disabledFace").objectReferenceValue = folkIvory;
            SetArray(serialized.FindProperty("settingButtons"), buttons);
            SetArray(serialized.FindProperty("settingFaces"), faces);
            SetArray(serialized.FindProperty("settingStates"), states);
            SetArray(serialized.FindProperty("settingDots"), dots);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AuthorHtmlMotion.ApplyRoot(root);

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("Settings EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                SceneManager.MoveGameObjectToScene(eventSystem, scene);
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create Settings EventSystem");
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Authored Settings modal in " + ImporterPath);
        }

        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Settings integration requires Edit Mode.");
            var importer = SceneManager.GetSceneByPath(ImporterPath);
            var main = SceneManager.GetSceneByPath(MainPath);
            if (!importer.IsValid() || !importer.isLoaded || !main.IsValid() || !main.isLoaded)
                throw new System.InvalidOperationException("Open HTML Importer and Main before integrating Settings.");
            GameObject authored = null;
            foreach (var root in importer.GetRootGameObjects())
                if (root.name == "SettingsImporterCanvas") authored = root;
            if (authored == null || authored.GetComponent<ImportedSettingsScreen>() == null)
                throw new System.InvalidOperationException("The saved Settings canvas is missing.");
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "ImportedSettingsCanvas")
                    throw new System.InvalidOperationException("Main already owns an imported Settings canvas.");
            var prefab = PrefabUtility.SaveAsPrefabAsset(authored, PrefabPath);
            if (prefab == null) throw new System.InvalidOperationException("Settings prefab could not be saved.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, main);
            instance.name = "ImportedSettingsCanvas";
            instance.SetActive(false);
            Undo.RegisterCreatedObjectUndo(instance, "Place Settings screen in Main");
            GameController game = null;
            foreach (var root in main.GetRootGameObjects())
                if (root.name == "Game") game = root.GetComponent<GameController>();
            if (game == null) throw new System.InvalidOperationException("Main GameController is missing.");
            var serialized = new SerializedObject(game);
            serialized.FindProperty("settingsScreen").objectReferenceValue = instance.GetComponent<ImportedSettingsScreen>();
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
            AssetDatabase.SaveAssets();
            Debug.Log("Placed preauthored Settings screen in Main and bound GameController.");
        }

        static void SettingRow(Transform parent, string title, string description, string name, int index,
            Button[] buttons, Image[] faces, TMP_Text[] states, Image[] dots)
        {
            var row = Row(parent, name, 190);
            Copy(row, title, description);
            var control = Right(row, "Toggle " + title, 233, 141, 23);
            var button = AddButton(control, index == 2 ? folkIvory : folkBlue);
            var label = AddText(Stretch(control, "State", 20, 20, 0, 0), index == 2 ? "OFF" : "ON", 39,
                index == 2 ? Ink : Color.white,
                TextAlignmentOptions.Center, true);
            label.rectTransform.anchoredPosition = new Vector2(index == 2 ? 35 : -35, 0);
            var dot = New("State dot", control).GetComponent<RectTransform>();
            dot.anchorMin = dot.anchorMax = new Vector2(.5f, .5f);
            dot.pivot = new Vector2(.5f, .5f);
            dot.sizeDelta = new Vector2(51, 51);
            dot.anchoredPosition = new Vector2(index == 2 ? -54 : 54, 0);
            var dotImage = AddImage(dot, Hex("FFF3D8"), false);
            dotImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            dotImage.type = UnityEngine.UI.Image.Type.Simple;
            buttons[index] = button;
            faces[index] = control.GetComponent<Image>();
            states[index] = label;
            dots[index] = dot.GetComponent<Image>();
        }

        static Button ActionRow(Transform parent, string title, string description, string name, string action, bool fillParent = false)
        {
            var row = Row(parent, name + " Row", fillParent ? 0 : 190, fillParent);
            Copy(row, title, description);
            var button = AddButton(Right(row, name, 205, 138, 25), folkIvory);
            AddText(Fill(button.transform, "Action Label"), action, 38, Ink, TextAlignmentOptions.Center, true);
            return button;
        }

        static RectTransform Row(Transform parent, string name, float height, bool fill = false)
        {
            var row = fill ? Fill(parent, name) : New(name, parent).GetComponent<RectTransform>();
            if (!fill) row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            AddImage(row, Paper, false);
            var border = AddImage(Fill(row, "Ticket Border"), Color.white, false);
            border.sprite = rowFrame;
            border.type = UnityEngine.UI.Image.Type.Sliced;
            return row;
        }

        static void Copy(RectTransform row, string title, string description)
        {
            AddText(Stretch(row, "Heading", 35, 250, 91, 30), title, 39, Ink,
                TextAlignmentOptions.MidlineLeft, true);
            AddText(Stretch(row, "Description", 35, 250, 32, 100), description, 31, Muted,
                TextAlignmentOptions.MidlineLeft, false);
        }


        static void SetArray<T>(SerializedProperty property, T[] values) where T : Object
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static GameObject New(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            if (parent != null) obj.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(obj, "Author Settings UI");
            return obj;
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

        static RectTransform Top(Transform parent, string name, float height)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.offsetMin = new Vector2(0, -height);
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        static RectTransform Bottom(Transform parent, string name, float height)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(.5f, 0);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0, height);
            return rect;
        }

        static RectTransform Right(Transform parent, string name, float width, float height, float inset)
        {
            var rect = New(name, parent).GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1, .5f);
            rect.pivot = new Vector2(1, .5f);
            rect.anchoredPosition = new Vector2(-inset, 0);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        static Image AddImage(RectTransform rect, Color color, bool raycast)
        {
            var graphic = rect.gameObject.AddComponent<Image>();
            graphic.sprite = rounded;
            graphic.type = UnityEngine.UI.Image.Type.Sliced;
            graphic.color = color;
            graphic.raycastTarget = raycast;
            return graphic;
        }

        static Button AddButton(RectTransform rect, Sprite sprite)
        {
            var graphic = AddImage(rect, Color.white, true);
            graphic.sprite = sprite;
            graphic.type = UnityEngine.UI.Image.Type.Sliced;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.highlightedColor = Hex("E9E0C9");
            colors.pressedColor = Hex("C8D4DD");
            colors.disabledColor = new Color(.7f, .7f, .7f, .6f);
            button.colors = colors;
            return button;
        }

        static TMP_Text AddText(RectTransform rect, string value, float size, Color color,
            TextAlignmentOptions alignment, bool bold)
        {
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.text = value;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            GameTypography.Configure(label);
            return label;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
