using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ValidateUiMotion
    {
        static ValidateUiMotion()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                    SessionState.SetBool("RocketRaja.EditorSmoke", false);
            };
        }

        [MenuItem("Rocket Raja/Validate UI Component Creation")]
        public static void Components()
        {
            var root = new GameObject("Temporary UI component regression", typeof(RectTransform));
            root.hideFlags = HideFlags.HideAndDontSave;
            root.SetActive(false);
            var checks = new List<string>();
            void Check(bool ok, string message)
            {
                if (!ok) throw new InvalidOperationException("FAIL " + message);
                checks.Add("PASS " + message);
            }
            try
            {
                var group = root.AddComponent<CanvasGroup>();
                var motion = root.AddComponent<UiMotion>();
                var authored = new SerializedObject(motion);
                authored.FindProperty("group").objectReferenceValue = group;
                authored.FindProperty("surface").objectReferenceValue = root.transform;
                authored.ApplyModifiedPropertiesWithoutUndo();
                UiMotion.Show(root, null);
                Check(group.interactable, "Show uses the Editor-authored CanvasGroup");
                UiMotion.Hide(root, null);
                Check(!root.activeSelf, "A newly created surface closes");
                UiMotion.Show(root, null);
                Check(root.GetComponent<CanvasGroup>() == group && root.GetComponents<UiMotion>().Length == 1,
                    "Reopening reuses existing components");
                root.AddComponent<Image>();
                root.AddComponent<Button>();
                root.AddComponent<UiFeedback>();
                UiFeedback.Wire(root, null);
                UiFeedback.Wire(root, null);
                Check(root.GetComponents<UiFeedback>().Length == 1, "Button feedback reuses its authored component");
                Directory.CreateDirectory("Documentation/AnimationValidation/Editor");
                File.WriteAllLines("Documentation/AnimationValidation/Editor/ComponentResults.txt", checks);
                Debug.Log("DIWALI_UI_COMPONENT_TESTS_OK: " + checks.Count);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [MenuItem("Rocket Raja/Run UI Smoke Checks in Editor")]
        public static void PlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new InvalidOperationException("Stop Play Mode and finish compilation before running smoke checks.");
            if (SceneManager.GetActiveScene().path != "Assets/Scenes/Main.unity")
                throw new InvalidOperationException("Open Main.unity before running UI smoke checks.");
            Components();
            string output = Path.GetFullPath("Documentation/AnimationValidation/Editor");
            SessionState.SetString("RocketRaja.EditorSmokeDirectory", output);
            // Read before GameController.Awake so testing never loads or writes the player's save.
            SessionState.SetBool("RocketRaja.EditorSmoke", true);
            SessionState.SetString("RocketRaja.HtmlMotionStage", "");
            EditorApplication.isPlaying = true;
        }

        public static void Stage(string name)
        { PlayMode(); SessionState.SetString("RocketRaja.HtmlMotionStage", name); }
    }
}
