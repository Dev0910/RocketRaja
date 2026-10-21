using System;
using System.Collections.Generic;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Editor
{
    public static class ClassicAchievementStagesAuthoring
    {
        [MenuItem("Rocket Raja/Classic/Author achievement stages")]
        public static void Author()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath(BuildClassicUI.ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(BuildClassicUI.ScenePath, OpenSceneMode.Additive);
            var ui = scene.GetRootGameObjects().Single(g => g.name == BuildClassicUI.RootName).GetComponent<ClassicUI>();
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Author achievement stages");
            var rows = new List<ClassicAchievementRow>();
            var seen = new HashSet<AchievementMetric>();
            foreach (var row in ui.achievementRows)
            {
                if (!RedesignAchievements.TryGet(row.id, out var definition)) continue;
                var root = row.claim.transform.parent;
                bool keep = seen.Add(definition.Metric);
                root.gameObject.SetActive(keep);
                if (!keep) continue;
                row.title = root.Find("Title").GetComponent<TMP_Text>();
                row.description = root.Find("Description").GetComponent<TMP_Text>();
                row.description.text = "Stage " + RedesignAchievements.StageNumber(definition) + " / " + RedesignAchievements.StageCount(definition.Metric) + " · " + definition.Description;
                rows.Add(row);
            }
            ui.achievementRows = rows.ToArray();
            if (ui.achievementClaimedCount) ui.achievementClaimedCount.text = "0 / " + RedesignAchievements.All.Count;
            EditorUtility.SetDirty(ui); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
