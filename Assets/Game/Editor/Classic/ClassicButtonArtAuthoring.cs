using System;
using System.Linq;
using RocketRaja.Classic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Applies the shared imported button artwork to saved production UI.</summary>
    public static class ClassicButtonArtAuthoring
    {
        const string Art = "Assets/Game/ImportedUI/Art/";

        [MenuItem("Rocket Raja/Classic/Use imported button artwork everywhere")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            foreach (var path in new[] { "Assets/Scenes/Main.unity", "Assets/Scenes/Bootstrap.unity" })
            {
                var scene = SceneManager.GetSceneByPath(path);
                if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                if (scene.isDirty) throw new InvalidOperationException("Preserve unsaved scene edits first: " + path);
                foreach (var root in scene.GetRootGameObjects().Where(g => g.activeSelf))
                {
                    var ui = root.GetComponent<ClassicUI>();
                    if (ui) ApplyTo(ui);
                    else if (path.EndsWith("Bootstrap.unity"))
                    {
                        // Only the current recovery UI; historical welcome references stay untouched.
                        var recovery = root.transform.Find("Local Recovery Safe Area");
                        if (recovery) ApplyButtons(recovery);
                    }
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }

        public static void ApplyTo(ClassicUI ui)
        {
            Undo.RecordObject(ui, "Use shared imported button artwork");
            ui.blueButton = ui.popupPrimary = Load("Folk Blue.png");
            ui.ivoryButton = ui.popupSecondary = Load("Folk Ivory.png");
            ui.disabledButton = ui.popupDisabled = Load("Folk Sand.png");
            ui.redButton = Load("Folk Red.png");
            ui.achievementClaimReady = ui.blueButton;
            ui.achievementClaimProgress = ui.disabledButton;
            ui.achievementClaimed = ui.ivoryButton;
            // Limit changes to current views, excluding retained historical dialog hierarchies.
            foreach (var view in ui.panels.Concat(ui.dialogs)) ApplyButtons(view.root.transform);
            EditorUtility.SetDirty(ui);
        }

        static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name)
            ?? throw new InvalidOperationException("Missing imported button sprite: " + name);

        static void ApplyButtons(Transform root)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                var control = button.GetComponent<ClassicControl>();
                // Preserve switches, transparent hit areas, icon controls and collectible card frames.
                if (control && (control.action == "Setting" || control.action == "Card" || control.htmlStyle == 3)) continue;
                if (!(button.targetGraphic is Image image) || !image.sprite) continue;
                string path = AssetDatabase.GetAssetPath(image.sprite);
                string name = image.sprite.name;
                Sprite sprite;
                if (path.StartsWith(Art) && name.StartsWith("Folk ")) sprite = image.sprite;
                else if (name == "popup-primary" || name == "claim-ready") sprite = Load("Folk Blue.png");
                else if (name == "popup-disabled" || name == "claim-progress") sprite = Load("Folk Sand.png");
                else if (name == "popup-secondary" || name == "skills-list-row" || name == "claim-claimed" || name == "Ivory Row Frame") sprite = Load("Folk Ivory.png");
                else continue;
                Undo.RecordObject(image, "Use imported button face");
                image.sprite = sprite;image.type = Image.Type.Sliced;image.color = Color.white;
                // Imported sprites use 400 pixels per unit; Main uses 360 logical pixels.
                // Account for Bootstrap's 1080 reference so borders have the same physical weight.
                var canvas = image.canvas;
                var scaler = canvas ? canvas.rootCanvas.GetComponent<CanvasScaler>() : null;
                float reference = scaler ? scaler.referenceResolution.x : 360;
                image.pixelsPerUnitMultiplier = 4 * 360 / reference;
                EditorUtility.SetDirty(image);
            }
        }
    }
}
