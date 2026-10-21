using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        // Base + bold face + symbol fallback covers current text. Extra saved capacity avoids lazy UI creation.
        const int SavedMaterialSlotCount = 4;

        static void AuthorTextSlots(RedesignSavedText text)
        {
            if (text.savedMaterialSlots.Length == SavedMaterialSlotCount && text.savedMaterialSlots.All(s => s))
            {
                for (int i = 0; i < SavedMaterialSlotCount; i++) text.savedMaterialSlots[i].name = "Saved material " + (i + 1);
                text.BindSavedSlots(); return;
            }
            foreach (var old in text.GetComponentsInChildren<TMP_SubMeshUI>(true)) UnityEngine.Object.DestroyImmediate(old.gameObject);
            text.savedMaterialSlots = new TMP_SubMeshUI[SavedMaterialSlotCount];
            for (int i = 0; i < SavedMaterialSlotCount; i++)
            {
                var rect = Fill("Saved material " + (i + 1), text.transform);
                rect.pivot = text.rectTransform.pivot; rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                var slot = rect.gameObject.AddComponent<RedesignSavedSubMesh>();
                var serialized = new SerializedObject(slot);
                serialized.FindProperty("m_TextComponent").objectReferenceValue = text;
                serialized.FindProperty("m_materialReferenceIndex").intValue = i + 1;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                slot.raycastTarget = false; slot.fontAsset = text.font; slot.sharedMaterial = text.fontSharedMaterial; slot.isDefaultMaterial = true;
                slot.name = "Saved material " + (i + 1);
                slot.hideFlags = HideFlags.None; slot.gameObject.hideFlags = HideFlags.None;
                text.savedMaterialSlots[i] = slot;
            }
            text.BindSavedSlots(); EditorUtility.SetDirty(text);
        }

        static void AuthorCaret(RedesignSearchInput input)
        {
            if (input.savedCaret) return;
            var rect = Fill("Saved caret", input.textComponent.transform.parent);
            rect.SetAsFirstSibling(); rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            rect.gameObject.AddComponent<TMP_SelectionCaret>();
            input.savedCaret = rect.GetComponent<CanvasRenderer>(); EditorUtility.SetDirty(input);
        }
        public static void SerializeTextSlots()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            // Change script through Unity serialization to preserve every existing component reference/file ID.
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                void ChangeScript(Component component, string path)
                {
                    var serialized = new SerializedObject(component);
                    serialized.FindProperty("m_Script").objectReferenceValue = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                    if (!(text is RedesignSavedText)) ChangeScript(text, "Assets/Game/Runtime/RedesignSavedText.cs");
                foreach (var input in root.GetComponentsInChildren<TMP_InputField>(true))
                    if (!(input is RedesignSearchInput)) ChangeScript(input, "Assets/Game/Runtime/RedesignSearchInput.cs");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (!(text is RedesignSavedText saved)) throw new InvalidOperationException("Text script migration failed");
                    AuthorTextSlots(saved);
                }
                foreach (var input in root.GetComponentsInChildren<RedesignSearchInput>(true)) AuthorCaret(input);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("REDESIGN_SAVED_TEXT_OK");
        }
    }
}
