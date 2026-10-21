using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RocketRaja.EditorTools;

namespace RocketRaja.Editor
{
    /// <summary>Repeatable repairs and isolated native quality review of saved screens.</summary>
    public static class ReviewUiQuality
    {
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before applying saved UI repairs.");
            foreach (string name in new[] { "Store Name", "Skills", "Player", "Products", "Privacy", "Home", "Legacy" })
            {
                string path = "Assets/Game/ImportedUI/" + name + " Screen.prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try { ApplyRoot(root, name); PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            // Keep the editable authoring scene consistent without replacing object IDs.
            var importer = SceneManager.GetSceneByPath("Assets/Scenes/HTML Importer.unity");
            bool opened = !importer.IsValid() || !importer.isLoaded;
            if (opened) importer = EditorSceneManager.OpenScene("Assets/Scenes/HTML Importer.unity", OpenSceneMode.Additive);
            foreach (var root in importer.GetRootGameObjects())
            {
                if (root.GetComponent<StoreNameDialog>() != null) ApplyRoot(root, "Store Name");
                else if (root.GetComponent<ImportedSkillsScreen>() != null) ApplyRoot(root, "Skills");
                else if (root.GetComponent<ImportedPlayerScreen>() != null) ApplyRoot(root, "Player");
                else if (root.GetComponent<ImportedProductsScreen>() != null) ApplyRoot(root, "Products");
                else if (root.GetComponent<ImportedPrivacyScreen>() != null) ApplyRoot(root, "Privacy");
                else if (root.GetComponent<ImportedHomeScreen>() != null) ApplyRoot(root, "Home");
                else if (root.GetComponent<ImportedLegacyScreen>() != null) ApplyRoot(root, "Legacy");
            }
            EditorSceneManager.MarkSceneDirty(importer);
            EditorSceneManager.SaveScene(importer);
            if (opened) EditorSceneManager.CloseScene(importer, true);
            AssetDatabase.SaveAssets();
            Debug.Log("ROCKET_RAJA_UI_QUALITY_REPAIRS_APPLIED");
        }

        static void ApplyRoot(GameObject root, string screen)
        {
            if (screen == "Store Name") BuildStoreNameDialog.Apply(root);
            else if (screen == "Skills") BuildImportedSkills.ApplyReadability(root);
            else if (screen == "Privacy") BuildImportedPrivacy.ApplyReadability(root);
            else if (screen == "Home") HomePageAuthoring.ApplyFeedback(root);
            else if (screen == "Legacy") UiCompactTypography.Apply(root);
            else
            {
                if (screen == "Products") BuildImportedProducts.ApplyDetailsAffordance(root);
                UiCompactTypography.Apply(root);
                UiQuantityAuthoring.Apply(root);
            }
        }
        public static void Run() => Stage("quality", "Documentation/UIQualityValidation/Review");

        public static void Stage(string name, string output)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new InvalidOperationException("Stop Play Mode and finish compilation before reviewing UI.");
            if (SceneManager.GetActiveScene().path != "Assets/Scenes/Main.unity")
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            SessionState.SetString("RocketRaja.EditorSmokeDirectory", Path.GetFullPath(output));
            SessionState.SetBool("RocketRaja.EditorSmoke", true);
            SessionState.SetString("RocketRaja.HtmlMotionStage", name);
            EditorApplication.isPlaying = true;
        }
    }
}
