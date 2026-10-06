using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class BuildClassicProductionAndroid
    {
        const string Request="Classic.ConnectedBuild.Request";
        static BuildClassicProductionAndroid(){EditorApplication.update+=Tick;}
        public const string Apk="Builds/Android/RocketRaja-classic-connected.apk";
        public const string Report="Documentation/Implementation/Classic-Connected-Android-Build.txt";
        public static string Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed||BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Compiled idle Edit Mode required");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new InvalidOperationException("Android target required");
            ClassicProductionAuthoring.Integrate();
            File.WriteAllText(Report,"QUEUED Bootstrap → Main (saved Classic production binding)\n");
            SessionState.SetBool(Request,true);
            return "Queued connected Classic development APK using the configured application identity";
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Request,false)||EditorApplication.isCompiling||EditorApplication.isUpdating||BuildPipeline.isBuildingPlayer)return;
            SessionState.EraseBool(Request);Build();
        }
        static void Build()
        {
            bool bundle=EditorUserBuildSettings.buildAppBundle;
            try
            {
                EditorUserBuildSettings.buildAppBundle=false;
                Directory.CreateDirectory(Path.GetDirectoryName(Apk));
                File.WriteAllText(Report,"RUNNING\nPackage: "+Application.identifier+"\nScenes: Bootstrap, Main\nNo preview defines or boosted profile\n");
                AssetDatabase.ReleaseCachedFileHandles();
                if(!GooglePlayServices.PlayServicesResolver.ResolveSync(false))throw new BuildFailedException("Android dependency resolution failed");
                var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ClassicProductionAuthoring.Bootstrap,ClassicProductionAuthoring.Main},target=BuildTarget.Android,locationPathName=Apk,options=BuildOptions.Development});
                File.AppendAllText(Report,result.summary.result+"\nErrors: "+result.summary.totalErrors+"\nWarnings: "+result.summary.totalWarnings+"\nDuration: "+result.summary.totalTime+"\nBytes: "+result.summary.totalSize+"\n");
                File.AppendAllLines(Report,result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception||m.type==LogType.Assert||m.type==LogType.Warning).Select(m=>m.type+": "+m.content).Distinct());
                if(result.summary.result!=BuildResult.Succeeded||result.summary.totalErrors!=0)throw new BuildFailedException("Connected Classic build must succeed with zero errors");
            }
            catch(Exception ex){File.AppendAllText(Report,"FAIL "+ex+"\n");Debug.LogException(ex);}
            finally{EditorUserBuildSettings.buildAppBundle=bundle;}
        }
    }
}
