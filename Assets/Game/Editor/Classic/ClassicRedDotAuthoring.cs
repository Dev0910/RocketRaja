using System;
using System.Collections.Generic;
using System.Linq;
using RocketRaja.Classic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static class ClassicRedDotAuthoring
    {
        [MenuItem("Rocket Raja/Classic/Author notification dots")]
        public static void Author()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(BuildClassicUI.ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(BuildClassicUI.ScenePath, OpenSceneMode.Additive);
            var ui = scene.GetRootGameObjects().Single(r => r.name == BuildClassicUI.RootName).GetComponent<ClassicUI>();
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Author notification dots");
            var notifications = ui.GetComponent<ClassicRedDots>();
            if (!notifications) notifications = Undo.AddComponent<ClassicRedDots>(ui.gameObject);
            notifications.ui = ui;
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (!sprite) throw new InvalidOperationException("Missing circular notification sprite");
            var bindings = new List<ClassicRedDotBinding>();
            foreach (var control in ui.GetComponentsInChildren<ClassicControl>(true))
            {
                bool destination = (control.action == "Navigate" || control.action == "Tab") &&
                    (new[] { "Achievements", "Skills", "Player", "Legacy", "Prestige", "Upgrades", "Catalogue" }.Contains(control.parameter) || control.parameter.StartsWith("SkillDetail:"));
                if (!destination && control.action != "ClaimAchievement" && control.action != "Draw" && control.action != "Prestige" && control.action != "ActivateSkill" && control.action != "BuyUpgrade" && control.action != "BuyAll" && control.action != "ReviewBatch" && control.action != "ConfirmBatch") continue;
                var dot = control.transform.Find("Notification dot") as RectTransform;
                if (!dot)
                {
                    var go = new GameObject("Notification dot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                    Undo.RegisterCreatedObjectUndo(go, "Author notification dot");
                    dot = (RectTransform)go.transform; dot.SetParent(control.transform, false);
                }
                dot.anchorMin = dot.anchorMax = dot.pivot = Vector2.one;
                dot.anchoredPosition = new Vector2(-2, -2); dot.sizeDelta = new Vector2(10, 10);
                dot.GetComponent<LayoutElement>().ignoreLayout = true;
                var image = dot.GetComponent<Image>(); image.sprite = sprite; image.color = new Color32(220, 43, 54, 255); image.raycastTarget = false;
                dot.SetAsLastSibling(); dot.gameObject.SetActive(false);
                bindings.Add(new ClassicRedDotBinding { control = control, dot = dot.gameObject });
            }
            notifications.bindings = bindings.ToArray(); EditorUtility.SetDirty(notifications);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            if (opened) EditorSceneManager.CloseScene(scene, true);
            Debug.Log("Authored " + bindings.Count + " notification dots for rewards, skills, prestige, upgrades and Talent draws.");
        }
    }
}
