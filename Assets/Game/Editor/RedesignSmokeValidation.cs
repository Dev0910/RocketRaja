using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class RedesignSmokeValidation
    {
        const string Key = "RocketRaja.RedesignSmokeQA";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        static RedesignSmokeValidation() { EditorApplication.playModeStateChanged += Mode; }
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Preserve dirty scene before QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = setup.Select(s => new Record { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            SessionState.SetString(Key + ".previousProfile", SessionState.GetString("RocketRaja.RedesignMainQA.profile", "")); SessionState.EraseString("RocketRaja.RedesignMainQA.profile");
            string folder = Path.GetFullPath("Documentation/Implementation/NativeSmoke"); Directory.CreateDirectory(folder); File.WriteAllText(Path.Combine(folder, "SmokeResults.txt"), "RUNNING\n");
            SessionState.SetString("RocketRaja.EditorSmokeDirectory", folder); SessionState.SetBool("RocketRaja.EditorSmoke", true); SessionState.SetBool(Key, true);
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Key, false)) return;
            SessionState.SetBool(Key, false); SessionState.SetBool("RocketRaja.EditorSmoke", false);
            SessionState.SetString("RocketRaja.RedesignMainQA.profile", SessionState.GetString(Key + ".previousProfile", ""));
            var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
            EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
            Debug.Log("REDESIGN_SMOKE_QA_RESTORED");
        }
    }
}
