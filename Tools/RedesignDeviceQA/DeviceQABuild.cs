using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DeviceQABuild
{
    public static void Android()
    {
        if (!File.Exists("DEVICE_QA_ONLY")) throw new BuildFailedException("This builder runs only in the isolated diagnostic project");
        PlayerSettings.companyName = "Diwali Diagnostics"; PlayerSettings.productName = "Rocket Raja Core QA";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.diwali.rocketrajaredesign.qa");
        PlayerSettings.bundleVersion = "0.0.1"; PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 0;
        EditorUserBuildSettings.buildAppBundle = false;
        Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Build");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Diagnostic camera", typeof(Camera)); camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        camera.GetComponent<Camera>().backgroundColor = Color.black;
        new GameObject("Isolated core diagnostic", typeof(DeviceCoreProbe));
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/DeviceQA.unity");
        AssetDatabase.SaveAssets();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/Scenes/DeviceQA.unity" }, target = BuildTarget.Android,
            locationPathName = "Build/RocketRaja-core-qa.apk", options = BuildOptions.Development });
        File.WriteAllText("Build/result.txt", report.summary.result + "\nerrors=" + report.summary.totalErrors + "\nbytes=" + report.summary.totalSize + "\nduration=" + report.summary.totalTime);
        if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Diagnostic APK failed: " + report.summary.result);
        Debug.Log("REDESIGN_DEVICE_QA_BUILD_OK");
    }
}
