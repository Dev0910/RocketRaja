using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Cloud;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.EditorTools
{
    [InitializeOnLoad]
    public static class ValidateBootstrap
    {
        static ValidateBootstrap() { EditorApplication.update += CheckGuestRoute; }
        static void CheckGuestRoute()
        {
            if(!EditorApplication.isPlaying || !SessionState.GetBool("RocketRaja.BootstrapButtonSmoke",false))return;
            var presenter=UnityEngine.Object.FindFirstObjectByType<BootstrapController>();if(presenter==null)return;
            var so=new SerializedObject(presenter);var button=(Button)so.FindProperty("secondaryButton").objectReferenceValue;
            if(!button.interactable || ((TMP_Text)so.FindProperty("secondaryLabel").objectReferenceValue).text!="Play as guest")return;
            SessionState.SetBool("RocketRaja.BootstrapButtonSmoke",false);
            var host=PlayerSessionHost.Instance;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            bool clean=typeof(PlayerSessionHost).GetField("auth",flags).GetValue(host)==null&&typeof(PlayerSessionHost).GetField("cloud",flags).GetValue(host)==null;
            Directory.CreateDirectory("Documentation/BootstrapValidation");
            File.WriteAllText("Documentation/BootstrapValidation/GuestRoute.txt",(clean?"PASS":"FAIL")+" No Firebase auth/cloud adapters initialized before guest choice\n");
            button.onClick.Invoke();
            File.AppendAllText("Documentation/BootstrapValidation/GuestRoute.txt",(host.Ready&&host.Session!=null&&!host.Session.IsAccount?"PASS":"FAIL")+" Real guest button initializes isolated local session\n");
        }
        public static readonly string[] States={"welcome","loading","syncing","conflict","replace","device","handoff","offline","error","recovery","fresh","account"};
        public static void CaptureAll()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode only.");
            var scene=SceneManager.GetSceneByPath(BuildBootstrap.ScenePath);
            if(!scene.IsValid())scene=EditorSceneManager.OpenScene(BuildBootstrap.ScenePath,OpenSceneMode.Additive);
            var root=scene.GetRootGameObjects().Single(g=>g.GetComponent<BootstrapController>()!=null);
            var presenter=root.GetComponent<BootstrapController>();var canvas=root.GetComponent<Canvas>();
            var scaler=root.GetComponent<CanvasScaler>();var camera=scene.GetRootGameObjects().Select(g=>g.GetComponent<Camera>()).First(c=>c!=null);
            var safe=(RectTransform)root.transform.Find("Safe Area");
            Directory.CreateDirectory("Documentation/BootstrapValidation/Renders");var checks=new List<string>();
            var otherCanvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c!=canvas&&c.enabled).ToArray();
            foreach(var c in otherCanvases)c.enabled=false;
            int nodes=root.GetComponentsInChildren<Transform>(true).Length;
            var so=new SerializedObject(presenter);var it=so.GetIterator();
            while(it.NextVisible(true))if(it.propertyType==SerializedPropertyType.ObjectReference&&it.name!="m_Script"&&it.objectReferenceValue==null)throw new InvalidOperationException("Missing binding: "+it.name);
            checks.Add("PASS All serialized Bootstrap references assigned");
            checks.Add("PASS Editor-authored hierarchy has "+nodes+" objects before Play Mode");
            try
            {
                foreach(var size in new[]{new Vector2Int(1080,2200),new Vector2Int(390,844),new Vector2Int(360,640),new Vector2Int(360,540)})
                foreach(string state in States)
                {
                    presenter.PreviewState(state);Capture(canvas,scaler,camera,size.x,size.y,"Documentation/BootstrapValidation/Renders/"+state+"-"+size.x+"x"+size.y+".png",checks);
                }
                presenter.PreviewState("conflict");safe.anchorMin=new Vector2(0,.04f);safe.anchorMax=new Vector2(1,.95f);
                Capture(canvas,scaler,camera,390,844,"Documentation/BootstrapValidation/Renders/conflict-safe-area.png",checks);
                safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;
                checks.Add((nodes==root.GetComponentsInChildren<Transform>(true).Length?"PASS":"FAIL")+" No UI hierarchy construction during state changes");
                checks.Add((EditorBuildSettings.scenes[0].path==BuildBootstrap.ScenePath?"PASS":"FAIL")+" Bootstrap is first build scene");
            }
            finally
            {
                presenter.PreviewState("welcome");safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;
                canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;scaler.enabled=true;camera.targetTexture=null;
                foreach(var c in otherCanvases)if(c!=null)c.enabled=true;
                Canvas.ForceUpdateCanvases();EditorSceneManager.SaveScene(scene);
                File.WriteAllLines("Documentation/BootstrapValidation/SceneChecks.txt",checks);
            }
            Debug.Log("BOOTSTRAP_SCENE_CHECKS "+checks.Count+"; failures="+checks.Count(x=>x.StartsWith("FAIL")));
        }
        static void Capture(Canvas canvas,CanvasScaler scaler,Camera camera,int width,int height,string path,List<string> checks)
        {
            var target=RenderTexture.GetTemporary(width,height,24);var previous=RenderTexture.active;
            try
            {
                scaler.enabled=false;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
                camera.targetTexture=target;canvas.scaleFactor=width/1080f;
                Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)canvas.transform.Find("Safe Area"));Canvas.ForceUpdateCanvases();
                foreach(var t in canvas.GetComponentsInChildren<TMP_Text>())t.ForceMeshUpdate();
                camera.Render();RenderTexture.active=target;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
                foreach(var button in canvas.GetComponentsInChildren<Button>())
                {
                    var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);var points=corners.Select(camera.WorldToScreenPoint).ToArray();
                    bool inside=points.All(p=>p.x>=-1&&p.x<=width+1&&p.y>=-1&&p.y<=height+1);
                    bool touch=points.Max(p=>p.y)-points.Min(p=>p.y)>=47.9f;
                    checks.Add((inside&&touch?"PASS":"FAIL")+" "+Path.GetFileNameWithoutExtension(path)+" "+button.name+" contained, minimum 48px");
                }
                foreach(var t in canvas.GetComponentsInChildren<TMP_Text>())if(t.isTextOverflowing)checks.Add("FAIL "+Path.GetFileNameWithoutExtension(path)+" overflowing text "+t.name+": "+t.text);
            }
            finally{camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);}
        }
        public static void StartSmoke()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Preserve dirty scene before smoke run.");
            EditorSceneManager.OpenScene(BuildBootstrap.ScenePath,OpenSceneMode.Single);
            SessionState.SetBool("RocketRaja.BootstrapPreview",false);
            SessionState.SetBool("RocketRaja.BootstrapButtonSmoke",true);
            SessionState.SetBool("RocketRaja.EditorSmoke",true);
            SessionState.SetString("RocketRaja.EditorSmokeDirectory",Path.GetFullPath("Documentation/BootstrapValidation/Smoke"));
            SessionState.SetString("RocketRaja.HtmlMotionStage","");
            EditorApplication.isPlaying=true;
        }
    }
}
