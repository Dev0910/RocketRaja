using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.EditorTools
{
    /// <summary>Authors a reusable outside-dismiss target without moving existing menu objects.</summary>
    public static class UiQuantityAuthoring
    {
        const string BlockerName = "Quantity Outside Blocker";

        static T Ensure<T>(GameObject root) where T : Component =>
            root.TryGetComponent<T>(out var component) ? component : root.AddComponent<T>();

        public static void Apply(GameObject root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Quantity targets must be authored outside Play Mode.");
            if (!(root.transform is RectTransform viewport) || root.GetComponent<Canvas>() == null)
                throw new InvalidOperationException("Quantity authoring requires the screen's root Canvas.");
            var menus = root.GetComponentsInChildren<RectTransform>(true)
                .Where(x => x.name == "Quantity Menu").ToArray();
            if (menus.Length != 1 || !(menus[0].parent is RectTransform parent))
                throw new InvalidOperationException("Expected one saved Quantity Menu with a RectTransform parent.");
            var menu = menus[0];
            var blockers = parent.Cast<Transform>().Where(x => x.name == BlockerName).ToArray();
            if (blockers.Length > 1)
                throw new InvalidOperationException("Quantity Menu has duplicate outside targets.");

            GameObject blockerObject;
            if (blockers.Length == 0)
            {
                blockerObject = new GameObject(BlockerName, typeof(RectTransform));
                blockerObject.transform.SetParent(parent, false);
            }
            else blockerObject = blockers[0].gameObject;
            if (!(blockerObject.transform is RectTransform))
                throw new InvalidOperationException("The saved quantity outside target requires a RectTransform.");
            blockerObject.layer = menu.gameObject.layer;
            blockerObject.SetActive(false);
            int menuIndex = menu.GetSiblingIndex();
            blockerObject.transform.SetSiblingIndex(blockerObject.transform.GetSiblingIndex() < menuIndex
                ? menuIndex - 1 : menuIndex);

            var blockerImage = Ensure<Image>(blockerObject);
            blockerImage.color = Color.clear;
            blockerImage.raycastTarget = true;
            blockerImage.maskable = false;
            // A fully transparent target still needs a live render depth for raycasting.
            blockerImage.canvasRenderer.cullTransparentMesh = false;
            var blockerButton = Ensure<Button>(blockerObject);
            blockerButton.targetGraphic = blockerImage;
            blockerButton.transition = Selectable.Transition.None;
            blockerButton.navigation = new Navigation { mode = Navigation.Mode.None };
            blockerButton.interactable = true;
            var blockerFeedback = Ensure<UiFeedback>(blockerObject);
            var feedbackData = new SerializedObject(blockerFeedback);
            feedbackData.FindProperty("kind").enumValueIndex = (int)UiFeedback.Kind.Button;
            feedbackData.FindProperty("target").objectReferenceValue = blockerObject.transform;
            feedbackData.FindProperty("face").objectReferenceValue = blockerImage;
            feedbackData.FindProperty("disabledFace").objectReferenceValue = null;
            feedbackData.FindProperty("navigationIcon").objectReferenceValue = null;
            feedbackData.ApplyModifiedPropertiesWithoutUndo();
            // This invisible hit surface participates in authored-reference validation,
            // but must keep its full viewport geometry while the pointer is pressed.
            blockerFeedback.enabled = false;
            var blockerGroup = Ensure<CanvasGroup>(blockerObject);
            blockerGroup.alpha = 1;
            blockerGroup.ignoreParentGroups = false;
            var blockerCanvas = Ensure<Canvas>(blockerObject);
            blockerCanvas.overrideSorting = true;
            blockerCanvas.sortingOrder = UiQuantityDismiss.BlockerOrder;
            var blockerRaycaster = Ensure<GraphicRaycaster>(blockerObject);
            blockerRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            var menuCanvas = Ensure<Canvas>(menu.gameObject);
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = UiQuantityDismiss.MenuOrder;
            var menuRaycaster = Ensure<GraphicRaycaster>(menu.gameObject);
            menuRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

            var dismiss = Ensure<UiQuantityDismiss>(menu.gameObject);
            var serialized = new SerializedObject(dismiss);
            serialized.FindProperty("viewport").objectReferenceValue = viewport;
            serialized.FindProperty("blocker").objectReferenceValue = blockerButton;
            serialized.FindProperty("menuCanvas").objectReferenceValue = menuCanvas;
            serialized.FindProperty("blockerCanvas").objectReferenceValue = blockerCanvas;
            serialized.FindProperty("screenGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            serialized.FindProperty("blockerGroup").objectReferenceValue = blockerGroup;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            dismiss.Refresh();
            menu.gameObject.SetActive(false);
            blockerObject.SetActive(false);
            EditorUtility.SetDirty(root);
        }
    }
}
