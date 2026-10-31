using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static class ClassicTutorialSavedValidation
    {
        public static string Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Compiled Edit Mode required");
            var scene=SceneManager.GetSceneByPath(BuildClassicUI.ScenePath);
            if(!scene.isLoaded||scene.isDirty)throw new InvalidOperationException("Saved Main required");
            // Reload from disk; no presentation initialization or Play Mode is used for these checks.
            EditorSceneManager.CloseScene(scene,true);scene=EditorSceneManager.OpenScene(BuildClassicUI.ScenePath,OpenSceneMode.Additive);
            var ui=scene.GetRootGameObjects().Single(g=>g.name==BuildClassicUI.RootName).GetComponent<ClassicUI>();var t=ui.tutorial;
            var checks=new List<string>();void Check(bool ok,string name){if(!ok)throw new InvalidOperationException("FAIL "+name);checks.Add("PASS "+name);}
            Check(t&&t.ui==ui&&t.safeArea&&t.bubble&&t.hand&&t.title&&t.copy&&t.dismiss&&t.training&&t.achievement&&t.prestige&&t.freePull,"All tutorial references survive scene reload");
            Check(!t.bubble.gameObject.activeSelf&&!t.hand.gameObject.activeSelf&&!ui.dialogs.Single(v=>v.route=="TutorialHelp").root.activeSelf,"Transient tutorial layers are saved inactive");
            Check(scene.GetRootGameObjects().Single(g=>g.name=="ClassicProductionCanvas__LegacyDisabled_20261019").activeSelf==false,"Complete existing legacy presentation remains inactive");
            Check(ui.panels.Single(v=>v.route=="Home").root.activeSelf&&ui.panels.Where(v=>v.route!="Home").All(v=>!v.root.activeSelf),"Saved default panel is Home");
            Check(ui.GetComponentsInChildren<ClassicControl>(true).Where(c=>c.action.StartsWith("Tutorial",StringComparison.Ordinal)).All(c=>c.GetComponent<Button>().navigation.mode==Navigation.Mode.None),"Tutorial buttons retain authored navigation policy");
            Check(ui.GetComponentsInChildren<RedesignSavedText>(true).All(text=>text.savedMaterialSlots.Length>0&&text.savedMaterialSlots.All(s=>s)),"All existing and added TMP material slots are preplaced");
            Check(!t.hand.GetComponent<Image>().raycastTarget&&t.hand.pivot==new Vector2(.205630f,.951356f),"Approved hand import and opaque-fingertip pivot survive serialization");
            var canvas=ui.GetComponent<Canvas>();var scaler=ui.GetComponent<CanvasScaler>();var camera=scene.GetRootGameObjects().Select(g=>g.GetComponent<Camera>()).First(c=>c);
            Check(scaler.referenceResolution==new Vector2(360,640)&&scaler.matchWidthOrHeight==0,"Production CanvasScaler remains 360x640 width match");
            var others=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude).Where(c=>c!=canvas&&c.enabled).ToArray();
            var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;bool scaleEnabled=scaler.enabled;
            var min=t.safeArea.anchorMin;var max=t.safeArea.anchorMax;var oldTarget=camera.targetTexture;var previous=RenderTexture.active;
            var layout=ui.GetComponent<ClassicLayout>();var folder=ClassicTutorialPlayValidation.Folder;
            Directory.CreateDirectory(folder+"/Renders");
            try {
                foreach(var c in others)c.enabled=false;
                foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(390,844),new Vector2Int(360,540),new Vector2Int(440,884)})
                foreach(bool notch in new[]{false,true}) {
                    var target=RenderTexture.GetTemporary(size.x,size.y,24);
                    try {
                        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;scaler.enabled=false;canvas.scaleFactor=size.x/360f;camera.targetTexture=target;
                        t.safeArea.anchorMin=notch?new Vector2(.025f,.035f):Vector2.zero;t.safeArea.anchorMax=notch?new Vector2(.975f,.945f):Vector2.one;
                        t.bubble.gameObject.SetActive(true);Canvas.ForceUpdateCanvases();layout.Apply();Canvas.ForceUpdateCanvases();
                        t.title.ForceMeshUpdate(true,true);t.copy.ForceMeshUpdate(true,true);Canvas.ForceUpdateCanvases();
                        var corners=new Vector3[4];t.bubble.GetWorldCorners(corners);
                        Check(corners.All(c=>{var p=t.safeArea.InverseTransformPoint(c);return p.x>=t.safeArea.rect.xMin-.1f&&p.x<=t.safeArea.rect.xMax+.1f&&p.y>=t.safeArea.rect.yMin-.1f&&p.y<=t.safeArea.rect.yMax+.1f;}),"Authored dialogue fits "+size+(notch?" simulated notch":" full safe area"));
                        Check(!t.title.isTextOverflowing&&!t.copy.isTextOverflowing,"Edit Mode live copy fits "+size+(notch?" notch":""));
                        camera.Render();RenderTexture.active=target;var png=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
                        try{png.ReadPixels(new Rect(0,0,size.x,size.y),0,0);png.Apply();File.WriteAllBytes(folder+"/Renders/edit-welcome-"+size.x+"x"+size.y+(notch?"-notch":"")+".png",png.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(png);}
                    } finally {camera.targetTexture=oldTarget;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);}
                }
            } finally {
                t.bubble.gameObject.SetActive(false);canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;scaler.enabled=scaleEnabled;
                t.safeArea.anchorMin=min;t.safeArea.anchorMax=max;foreach(var c in others)if(c)c.enabled=true;
                Canvas.ForceUpdateCanvases();layout.Apply();
                // Discard only this method's temporary render poses; saved Main remains the authority.
                EditorSceneManager.CloseScene(scene,true);EditorSceneManager.OpenScene(BuildClassicUI.ScenePath,OpenSceneMode.Additive);
                File.WriteAllLines(folder+"/SavedChecks.txt",checks);
            }
            return "Saved tutorial passed "+checks.Count+" checks, including reload and eight Edit Mode viewport/safe-area captures.";
        }
    }
}
