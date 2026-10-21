using System;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static class ClassicProductionAuthoring
    {
        public const string Main = "Assets/Scenes/Main.unity", Bootstrap = "Assets/Scenes/Bootstrap.unity";
        [MenuItem("Rocket Raja/Classic/Connect production startup")]
        public static void Integrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            for (int i=0;i<SceneManager.sceneCount;i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save open scene edits before integration");
            var scene = EditorSceneManager.OpenScene(Main);
            var ui = scene.GetRootGameObjects().Select(g=>g.GetComponent<ClassicUI>()).FirstOrDefault(c=>c);
            if (!ui)
                throw new InvalidOperationException("Main is missing its saved Classic UI. Author Classic in Main before integrating startup.");
            foreach (var root in scene.GetRootGameObjects())
                root.SetActive(root == ui.gameObject || root.GetComponent<Camera>() || root.GetComponent<EventSystem>());
            ui.name = "ClassicProductionCanvas";
            foreach (var preview in ui.GetComponentsInChildren<ClassicPreviewSession>(true)) UnityEngine.Object.DestroyImmediate(preview);
            foreach (var probe in ui.GetComponentsInChildren<ClassicDeviceProbe>(true)) UnityEngine.Object.DestroyImmediate(probe);
            ui.isolatedPreview = false;
            ui.application = ui.GetComponent<ClassicApplication>() ?? ui.gameObject.AddComponent<ClassicApplication>();
            ui.application.ui = ui;
            foreach (var panel in ui.panels) panel.root.SetActive(panel.route == "Home");
            foreach (var dialog in ui.dialogs) dialog.root.SetActive(false);
            ui.toast.SetActive(false);
            ui.textBindings.Single(b=>b.key=="cash").text.text="₹—";
            foreach(var key in new[]{"home-income","home-tap","home-goal","fame","legacy-kk","shop-kk"})
                ui.textBindings.Single(b=>b.key==key).text.text="—";
            for(int i=0;i<3;i++)
            {
                ui.textBindings.Single(b=>b.key=="home-skill-"+i).text.text="—";
                ui.homeSkillImages[i].sprite=ui.lockArt;
                ui.motion.skillGlows[i].gameObject.SetActive(false);
            }
            foreach(var shelf in ui.shelfProducts)shelf.SetActive(false);
            var recovery = ui.dialogs.Single(v=>v.route == "SaveRecovery");
            var labels = recovery.root.GetComponentsInChildren<TMP_Text>(true);
            ui.recoveryMessage = labels.Single(t=>t.text.StartsWith("Keep existing files while retrying."));
            ui.recoveryMessage.text = "Keep existing files while retrying. Gameplay stays paused until your progress can load and save safely.";
            labels.Single(t=>t.text == "We could not complete saving" || t.text == "Your game is paused").text = "Your game is paused";
            foreach(var control in recovery.root.GetComponentsInChildren<ClassicControl>(true))
            {
                if(control.action == "Retry") control.GetComponentInChildren<TMP_Text>().text = "Retry loading / saving";
                if(control.parameter == "RecoveryConfirm") control.gameObject.SetActive(false);
            }
            foreach(var control in ui.GetComponentsInChildren<ClassicControl>(true).Where(c=>c.action == "Purchase"))
            {
                control.GetComponent<Button>().interactable=false;
                control.GetComponentInChildren<TMP_Text>().text="Unavailable";
            }
            foreach(var label in ui.GetComponentsInChildren<TMP_Text>(true))
            {
                if(label.text.StartsWith("Isolated Editor preview")) label.text="Purchases will be available when the store is connected.";
                if(label.text.Contains("This importer does not contact")) label.text="Guest progress stays on this device. Optional Google saves use Firebase. Purchases are currently unavailable.";
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            EditorBuildSettings.scenes = new[]{new EditorBuildSettingsScene(Bootstrap,true),new EditorBuildSettingsScene(Main,true)};
            // Normal Play now uses the open scene. Main redirects to Bootstrap if no profile was selected.
            EditorSceneManager.playModeStartScene = null;
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>Protect the saved release hierarchy even when a build bypasses the project menu.</summary>
    public sealed class ClassicProductionSceneGuard : IProcessSceneWithReport
    {
        public int callbackOrder => 100;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if(report == null) return;
            if(scene.path == ClassicProductionAuthoring.Main)
            {
                var roots = scene.GetRootGameObjects();
                var active = roots.Where(g=>g.activeSelf).ToArray();
                var ui = active.Select(g=>g.GetComponent<ClassicUI>()).SingleOrDefault(c=>c);
                if(!ui || !ui.application || !ui.guidance || !ui.progression || !ui.audioFeedback || ui.isolatedPreview || active.Sum(g=>g.GetComponentsInChildren<RedesignRuntimeDriver>().Length)!=1 ||
                    roots.Any(g=>g.GetComponentsInChildren<GameController>(true).Length>0 || g.GetComponentsInChildren<RedesignApplication>(true).Length>0 || g.GetComponentsInChildren<ClassicPreviewSession>(true).Length>0))
                    throw new BuildFailedException("Main must contain one saved Classic production session without legacy or preview sessions.");
            }
            if((report.summary.options & BuildOptions.Development)==0 && scene.GetRootGameObjects().Any(g=>g.GetComponentsInChildren<ClassicPreviewSession>(true).Length>0 || g.GetComponentsInChildren<ClassicDeviceProbe>(true).Length>0))
                throw new BuildFailedException("Preview sessions and device probes cannot enter release scenes.");
        }
    }
}
