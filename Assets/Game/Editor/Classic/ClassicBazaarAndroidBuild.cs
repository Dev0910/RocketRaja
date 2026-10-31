using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicBazaarAndroidBuild
    {
        const string Key="RocketRaja.BazaarBuild";
        public const string Report="Documentation/Implementation/UiUnification/native/android-build.txt";
        public const string Apk="Builds/Android/RocketRaja-bazaar-development.apk";
        static ClassicBazaarAndroidBuild(){EditorApplication.update+=Tick;}
        public static string Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||BuildPipeline.isBuildingPlayer)throw new Exception("Idle compiled Edit Mode required");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new Exception("Android target required");
            EditorSceneManager.SaveOpenScenes();
            File.WriteAllText(Report,"QUEUED native development APK; existing authored Bootstrap and Main; no UI regeneration.\n");SessionState.SetBool(Key,true);return "Queued development APK";
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Key,false)||EditorApplication.isCompiling||EditorApplication.isUpdating||BuildPipeline.isBuildingPlayer)return;
            SessionState.EraseBool(Key);bool bundle=EditorUserBuildSettings.buildAppBundle,custom=PlayerSettings.Android.useCustomKeystore;
            var profile=BuildProfile.GetActiveBuildProfile();string profileSnapshot=profile?EditorJsonUtility.ToJson(profile):null;
            try
            {
                EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;Directory.CreateDirectory(Path.GetDirectoryName(Apk));
                File.WriteAllText(Report,"RUNNING\nApplication: "+Application.identifier+"\nScenes: Bootstrap, Main\nDevelopment build; debug signing; no boosted profile or QA defines.\n");
                var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Bootstrap.unity","Assets/Scenes/Main.unity"},target=BuildTarget.Android,locationPathName=Apk,options=BuildOptions.Development});
                File.AppendAllText(Report,result.summary.result+"\nErrors: "+result.summary.totalErrors+"\nWarnings: "+result.summary.totalWarnings+"\nDuration: "+result.summary.totalTime+"\nBytes: "+result.summary.totalSize+"\n");
                File.AppendAllLines(Report,result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning||m.type==LogType.Exception).Select(m=>m.type+": "+m.content).Distinct());
                if(result.summary.result!=BuildResult.Succeeded||result.summary.totalErrors>0)throw new BuildFailedException("Native Bazaar development build failed");
            }
            catch(Exception e){File.AppendAllText(Report,"FAIL "+e+"\n");Debug.LogException(e);}
            finally
            {
                EditorUserBuildSettings.buildAppBundle=bundle;PlayerSettings.Android.useCustomKeystore=custom;
                if(profile){EditorJsonUtility.FromJsonOverwrite(profileSnapshot,profile);EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();}
            }
        }
    }
}
