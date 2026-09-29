using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static RocketRaja.EditorTools.ImportedUiKit;

namespace RocketRaja.EditorTools
{
    public static class BuildStoreNameDialog
    {
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
            Text(Stretch(dialog, "Description", 50, 50, 540, 180), "A name for your fireworks shop", 33, ink, TextAlignmentOptions.Center);
            var entry = Stretch(dialog, "Store Name Input", 50, 50, 335, 330);
            var face = Image(entry, Color.white, true);
            var viewport = Stretch(entry, "Text Area", 22, 22, 12, 12); viewport.gameObject.AddComponent<RectMask2D>();
            var value = Text(Fill(viewport, "Value"), "", 42, ink, TextAlignmentOptions.MidlineLeft);
            value.richText = false;
            var placeholder = Text(Fill(viewport, "Placeholder"), "My Fireworks Store", 42, Hex("748899"), TextAlignmentOptions.MidlineLeft);
            var input = entry.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = face; input.textViewport = viewport; input.textComponent = value; input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine; input.richText = false;
            var message = Text(Stretch(dialog, "Validation", 50, 50, 215, 530), "1–24 characters", 30, ink, TextAlignmentOptions.Center);
            var cancel = Button(Stretch(dialog, "cancel-store-name", 45, 520, 45, 655), Ivory);
            var cancelText = Text(Fill(cancel.transform, "Label"), "Skip for now", 35, ink, TextAlignmentOptions.Center, true);
            var save = Button(Stretch(dialog, "save-store-name", 520, 45, 45, 655), Blue);
            Text(Fill(save.transform, "Label"), "Save name", 35, Color.white, TextAlignmentOptions.Center, true);
            var presenter = root.AddComponent<StoreNameDialog>(); var data = new SerializedObject(presenter);
            data.FindProperty("input").objectReferenceValue = input; data.FindProperty("title").objectReferenceValue = title;
            data.FindProperty("message").objectReferenceValue = message; data.FindProperty("cancelLabel").objectReferenceValue = cancelText;
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
