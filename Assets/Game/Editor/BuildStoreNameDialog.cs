using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RocketRaja.EditorTools.ImportedUiKit;

namespace RocketRaja.EditorTools
{
    public static class BuildStoreNameDialog
    {
        // Applies the full-name view to previously saved prefabs without replacing their
        // existing object IDs, which are referenced by the Main scene prefab instance.
        public static void Apply(GameObject root)
        {
            Load();
            var dialog = root.GetComponentsInChildren<RectTransform>(true).Single(x => x.name == "StoreNameDialog");
            var detail = dialog.Find("Full Name Panel") as RectTransform;
            if (detail == null)
            {
                detail = Stretch(dialog, "Full Name Panel", 65, 65, 235, 285);
                Image(detail, Color.white);
                var name = Text(Stretch(detail, "Full Name", 24, 24, 18, 18),
                    "My Fireworks Store", 42, Hex("164B80"), TextAlignmentOptions.Center, true);
                name.richText = false;
                name.enableAutoSizing = false;
                name.textWrappingMode = TextWrappingModes.Normal;
            }
            detail.gameObject.SetActive(false);
            var data = new SerializedObject(root.GetComponent<StoreNameDialog>());
            data.FindProperty("description").objectReferenceValue = dialog.Find("Description").GetComponent<TMP_Text>();
            data.FindProperty("detailName").objectReferenceValue = detail.Find("Full Name").GetComponent<TMP_Text>();
            data.FindProperty("detailPanel").objectReferenceValue = detail.gameObject;
            data.FindProperty("inputRow").objectReferenceValue = dialog.Find("Store Name Input").gameObject;
            data.FindProperty("saveLabel").objectReferenceValue = dialog.Find("save-store-name/Label").GetComponent<TMP_Text>();
            data.ApplyModifiedPropertiesWithoutUndo();
            StyleInput(dialog.Find("Store Name Input").GetComponent<TMP_InputField>());
            StyleValidation(dialog.Find("Validation").GetComponent<TMP_Text>());
        }

        static void StyleValidation(TMP_Text message)
        {
            // Two Nunito lines at 36 reference units fit in this 105-unit slot.
            // Keep the essential input rules at 12 pixels on a 360-wide viewport.
            var box = message.rectTransform;
            box.anchorMin = new Vector2(0, 1);
            box.anchorMax = Vector2.one;
            box.offsetMin = new Vector2(50, -635);
            box.offsetMax = new Vector2(-50, -530);
            message.enableAutoSizing = false;
            message.fontSize = message.fontSizeMin = message.fontSizeMax = 36;
            message.textWrappingMode = TextWrappingModes.Normal;
            EditorUtility.SetDirty(message);
        }

        // Keep the field's saved references intact while making its boundary and focus
        // visible against the ivory dialog. Focus adds a line, so it does not rely on hue.
        static void StyleInput(TMP_InputField input)
        {
            var entry = (RectTransform)input.transform;
            var ink = Hex("164B80");
            var face = entry.GetComponent<Image>();
            face.color = ink;
            face.raycastTarget = true;
            input.targetGraphic = face;
            input.transition = Selectable.Transition.ColorTint;
            var colors = input.colors;
            colors.normalColor = colors.highlightedColor = colors.selectedColor = colors.pressedColor = Color.white;
            input.colors = colors;
            input.customCaretColor = true;
            input.caretColor = ink;

            var surface = entry.Find("Input Surface") as RectTransform;
            if (surface == null) surface = Stretch(entry, "Input Surface", 6, 6, 6, 6);
            surface.anchorMin = Vector2.zero;
            surface.anchorMax = Vector2.one;
            surface.offsetMin = new Vector2(6, 6);
            surface.offsetMax = new Vector2(-6, -6);
            surface.SetAsFirstSibling();
            var surfaceImage = surface.GetComponent<Image>();
            if (surfaceImage == null) surfaceImage = Image(surface, Color.white);
            surfaceImage.color = Color.white;
            surfaceImage.raycastTarget = false;

            var focus = entry.Find("Input Focus Underline") as RectTransform;
            if (focus == null) focus = New("Input Focus Underline", entry).GetComponent<RectTransform>();
            focus.anchorMin = Vector2.zero;
            focus.anchorMax = new Vector2(1, 0);
            focus.offsetMin = new Vector2(6, 6);
            focus.offsetMax = new Vector2(-6, 16);
            focus.SetAsLastSibling();
            var focusImage = focus.GetComponent<Image>();
            if (focusImage == null) focusImage = Image(focus, ink);
            focusImage.color = ink;
            focusImage.raycastTarget = false;
            focus.gameObject.SetActive(false);
            SetFocusListener(input.onSelect, focus.gameObject, true);
            SetFocusListener(input.onDeselect, focus.gameObject, false);
            SetFocusListener(input.onEndEdit, focus.gameObject, false);

            if (input.placeholder is TMP_Text placeholder)
            {
                placeholder.color = Hex("375A77");
                placeholder.richText = false;
            }
            EditorUtility.SetDirty(input);
        }

        static void SetFocusListener(UnityEventBase callback, GameObject indicator, bool visible)
        {
            for (int i = 0; i < callback.GetPersistentEventCount(); i++)
            {
                if (callback.GetPersistentTarget(i) != indicator ||
                    callback.GetPersistentMethodName(i) != nameof(GameObject.SetActive)) continue;
                UnityEventTools.RegisterBoolPersistentListener(callback, i, indicator.SetActive, visible);
                callback.SetPersistentListenerState(i, UnityEventCallState.RuntimeOnly);
                return;
            }
            UnityEventTools.AddBoolPersistentListener(callback, indicator.SetActive, visible);
        }

        public static GameObject Create()
        {
            Load();
            var root = CanvasRoot("StoreNameImporterCanvas", 110);
            Image(Fill(root.transform, "DimBackdrop"), new Color(.035f, .07f, .13f, .76f), true);
            var safe = Fill(root.transform, "SafeArea"); safe.gameObject.AddComponent<ImportedSafeArea>();
            var host = Fill(safe, "Dialog Motion Host");
            var dialog = Dialog(host, "StoreNameDialog", 850);
            dialog.gameObject.AddComponent<ImportedDialogFit>().SetPreferredHeight(850);
            var ink = Hex("164B80");
            var title = Text(Stretch(dialog, "Title", 45, 45, 650, 35), "Name your store", 52, ink, TextAlignmentOptions.Center, true);
            var description = Text(Stretch(dialog, "Description", 50, 50, 540, 180), "A name for your fireworks shop", 33, ink, TextAlignmentOptions.Center);
            var entry = Stretch(dialog, "Store Name Input", 50, 50, 335, 330);
            var face = Image(entry, Color.white, true);
            var viewport = Stretch(entry, "Text Area", 22, 22, 12, 12); viewport.gameObject.AddComponent<RectMask2D>();
            var value = Text(Fill(viewport, "Value"), "", 42, ink, TextAlignmentOptions.MidlineLeft);
            value.richText = false;
            var placeholder = Text(Fill(viewport, "Placeholder"), "My Fireworks Store", 42, Hex("748899"), TextAlignmentOptions.MidlineLeft);
            var input = entry.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = face; input.textViewport = viewport; input.textComponent = value; input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine; input.richText = false;
            StyleInput(input);
            var detail = Stretch(dialog, "Full Name Panel", 65, 65, 235, 285);
            Image(detail, Color.white);
            var detailName = Text(Stretch(detail, "Full Name", 24, 24, 18, 18), "My Fireworks Store", 42,
                ink, TextAlignmentOptions.Center, true);
            detailName.richText = false;
            detailName.enableAutoSizing = false;
            detailName.textWrappingMode = TextWrappingModes.Normal;
            detail.gameObject.SetActive(false);
            var message = Text(Stretch(dialog, "Validation", 50, 50, 215, 530), "1–24 characters", 36, ink, TextAlignmentOptions.Center);
            StyleValidation(message);
            var cancel = Button(Stretch(dialog, "cancel-store-name", 45, 520, 45, 655), Ivory);
            var cancelText = Text(Fill(cancel.transform, "Label"), "Skip for now", 35, ink, TextAlignmentOptions.Center, true);
            var save = Button(Stretch(dialog, "save-store-name", 520, 45, 45, 655), Blue);
            var saveText = Text(Fill(save.transform, "Label"), "Save name", 35, Color.white, TextAlignmentOptions.Center, true);
            var presenter = root.AddComponent<StoreNameDialog>(); var data = new SerializedObject(presenter);
            data.FindProperty("input").objectReferenceValue = input; data.FindProperty("title").objectReferenceValue = title;
            data.FindProperty("description").objectReferenceValue = description;
            data.FindProperty("message").objectReferenceValue = message; data.FindProperty("cancelLabel").objectReferenceValue = cancelText;
            data.FindProperty("saveLabel").objectReferenceValue = saveText; data.FindProperty("detailName").objectReferenceValue = detailName;
            data.FindProperty("inputRow").objectReferenceValue = entry.gameObject; data.FindProperty("detailPanel").objectReferenceValue = detail.gameObject;
            data.FindProperty("saveButton").objectReferenceValue = save; data.FindProperty("cancelButton").objectReferenceValue = cancel;
            data.ApplyModifiedPropertiesWithoutUndo();
            var group = root.AddComponent<CanvasGroup>(); var motion = root.AddComponent<UiMotion>(); var m = new SerializedObject(motion);
            m.FindProperty("kind").enumValueIndex = (int)UiMotion.Kind.Modal; m.FindProperty("group").objectReferenceValue = group;
            m.FindProperty("surface").objectReferenceValue = host; m.ApplyModifiedPropertiesWithoutUndo();
            foreach (var button in new[] { cancel, save })
            {
                var feedback = button.gameObject.AddComponent<UiFeedback>(); var f = new SerializedObject(feedback);
                f.FindProperty("target").objectReferenceValue = button.transform; f.FindProperty("face").objectReferenceValue = button.targetGraphic;
                f.FindProperty("disabledFace").objectReferenceValue = Sand; f.ApplyModifiedPropertiesWithoutUndo();
            }
            root.SetActive(false);
            return root;
        }
    }
}
