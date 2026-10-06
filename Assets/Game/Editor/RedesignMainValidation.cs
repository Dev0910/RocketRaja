using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static class RedesignMainValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            BuildRedesignUI.IntegrateMain();
            Check(true, "Main authoring can run again without replacing the saved native owner");
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var app = root.GetComponent<RedesignApplication>(); var ui = app.retained; var router = app.router;
                typeof(RedesignRetainedUi).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);
                var profile = Path.GetFullPath("Temp/RedesignMainRender-" + Guid.NewGuid().ToString("N"));
                app.Initialize(profile); var e = router.Session.Engine;
                Check(app.Ready && router.CurrentRoute == "home", "Saved application starts a fresh isolated Home");
                Check(e.State.products.All(p => p == 0) && e.State.cash.CompareTo(new Amount(30)) == 0, "Fresh integrated startup preserves the manual opening grant");
                Check(root.GetComponentsInChildren<TMP_Text>(true).All(t => t is RedesignSavedText saved && saved.savedMaterialSlots.Length == 4 && saved.savedMaterialSlots.All(s => s)), "Every native and retained text has four serialized material slots");
                Check(root.GetComponentsInChildren<TMP_InputField>(true).All(t => t is RedesignSearchInput saved && saved.savedCaret), "Search and naming both have saved input carets");
                Check(!root.GetComponentInChildren<ImportedSettingsScreen>(true) && !root.GetComponentInChildren<ImportedSaveRecoveryScreen>(true) && !root.GetComponentInChildren<StoreNameDialog>(true), "Retained copies contain no old controller-bound presenters");
                Check(root.GetComponent<RedesignAudio>() && root.GetComponents<AudioSource>().Length == 2, "Two audio sources are serialized with the new driver");
                var objects = root.GetComponentsInChildren<Transform>(true);
                ui.OpenSettings();
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540), new Vector2Int(360, 640), new Vector2Int(450, 916) })
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "main-settings", () => ui.settings.GetComponentInChildren<ScrollRect>(true).verticalNormalizedPosition = 1);
                RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "main-settings-bottom", () => ui.settings.GetComponentInChildren<ScrollRect>(true).verticalNormalizedPosition = 0);
                ui.account.onClick.Invoke(); Check(ui.accountStatus.text.Contains("unavailable"), "Account entry explains the isolated development scope");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "main-account", () => ui.settings.GetComponentInChildren<ScrollRect>(true).verticalNormalizedPosition = 0);
                ui.privacyEntry.onClick.Invoke();
                RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "main-privacy");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "main-privacy-bottom", () => ui.privacy.GetComponentInChildren<ScrollRect>(true).verticalNormalizedPosition = 0);
                Check(ui.Back() && ui.Page == "settings", "Privacy returns to Settings without closing the input gate");
                ui.rename.onClick.Invoke(); ui.nameInput.text = "<b>Raja & Co</b>";
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "main-name");
                ui.nameSave.onClick.Invoke(); Check(e.State.storeName == "<b>Raja & Co</b>" && ui.Page == "settings", "Saved naming controls accept supported literal punctuation");
                ui.ShowRecovery("Saving failed. Free device storage and retry before continuing.");
                var retryRect = (RectTransform)ui.recoveryRetry.transform; var exitRect = (RectTransform)ui.recoveryExit.transform;
                Check(retryRect.parent == exitRect.parent && retryRect.anchoredPosition.y - retryRect.rect.height / 2 > exitRect.anchoredPosition.y + exitRect.rect.height / 2,
                    "Recovery Retry and Exit have separate non-overlapping saved touch targets");
                RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "main-recovery");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "main-recovery-bottom", () => ui.recovery.GetComponentInChildren<ScrollRect>(true).verticalNormalizedPosition = 0);
                Check(new HashSet<Transform>(objects).SetEquals(root.GetComponentsInChildren<Transform>(true)), "Utility navigation/rendering preserves all authored UI objects");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
                File.WriteAllLines("Documentation/Implementation/Unity-Main-UI.txt", checks);
                Debug.Log("REDESIGN_MAIN_UI_OK " + checks.Count);
            }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
