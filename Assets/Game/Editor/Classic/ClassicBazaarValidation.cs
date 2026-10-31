using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RocketRaja.Classic;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicBazaarValidation
    {
        const string Key="RocketRaja.BazaarQA.",Profile="RocketRaja.LocalOnlyQA.Profile";
        const string Folder="Documentation/Implementation/UiUnification/native/";
        static ClassicUI ui;
        static readonly List<object> records=new List<object>();
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static ClassicBazaarValidation(){EditorApplication.playModeStateChanged+=Mode;}
        public static string Start(string pass="iteration-1",bool complete=false)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new Exception("Compiled Edit Mode required");
            foreach(var setup in EditorSceneManager.GetSceneManagerSetup())if(setup.isLoaded&&SceneManager.GetSceneByPath(setup.path).isDirty)throw new Exception("Preserve unsaved scene "+setup.path);
            SessionState.SetString(Key+"OldProfile",SessionState.GetString(Profile,""));SessionState.SetString(Profile,Path.Combine(Path.GetTempPath(),"BazaarQA-"+Guid.NewGuid().ToString("N")));
            SessionState.SetString(Key+"Pass",pass);SessionState.SetBool(Key+"Complete",complete);SessionState.SetBool(Key+"Running",true);SessionState.SetString(Key+"Result","RUNNING");
            Directory.CreateDirectory(Folder+pass);EditorSceneManager.playModeStartScene=null;EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");ClassicImportQA.Viewport(1080,2200);EditorApplication.EnterPlaymode();return "Started "+pass;
        }
        static void Mode(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Key+"Running",false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Run();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetString(Profile,SessionState.GetString(Key+"OldProfile",""));SessionState.SetBool(Key+"Running",false);}
        }
        static void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        static async Task Until(Func<bool> condition){double end=EditorApplication.timeSinceStartup+35;while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new Exception("Startup timeout");await Task.Delay(100);}}
        static void Check(bool ok,string label){checks.Add((ok?"PASS ":"FAIL ")+label);if(!ok)throw new Exception(label);}
        public static string Result=>SessionState.GetString(Key+"Result","");
        static async void Run()
        {
            checks.Clear();records.Clear();errors.Clear();Application.logMessageReceived+=Log;string pass=SessionState.GetString(Key+"Pass","");
            try
            {
                await Until(()=>SceneManager.GetSceneByPath("Assets/Scenes/Main.unity").isLoaded);
                ui=SceneManager.GetSceneByPath("Assets/Scenes/Main.unity").GetRootGameObjects().Single(g=>g.name=="ClassicProductionCanvas").GetComponent<ClassicUI>();
                await Until(()=>ui.driver.Initialized&&ui.application.Ready);await Task.Delay(600);
                Check(ui.bazaarTheme,"Production theme binding");Check(ui.Engine.State.cash.CompareTo(0)==0,"Disposable fresh profile");
                await Capture(pass,"Fresh",1080,2200);
                ui.driver.enabled=false;
                Check(ui.driver.Command(e=>{e.State.cash=1240000;e.State.products=new[]{75,51,29,12,5,3,1,1};e.State.manualTaps=500;e.State.tapLevel=12;e.State.kajuKatli=250;e.State.freeCredits=4;e.State.reducedMotion=true;e.State.tutorial.skipped=true;for(int i=0;i<e.State.cardLevels.Length;i++)e.State.cardLevels[i]=i%4==0?2:0;for(int i=0;i<3;i++){string id=e.PreviewSkill(i).unlockId;if(!e.State.upgrades.Contains(id))e.State.upgrades.Add(id);if(!e.State.everPurchased.Contains(id))e.State.everPurchased.Add(id);}return e.Save();}),"Progressed fixture through existing codec");
                ui.Refresh();await Task.Delay(300);ui.driver.enabled=false;
                var before=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).OrderBy(n=>n).ToArray();
                var routes=SessionState.GetBool(Key+"Complete",false)?new[]{"Home","Player","Products","Legacy","Shop","ProductDetail:0","TapTraining","Skills","SkillDetail:0","SkillDetail:1","SkillDetail:2","CardDetail:0","CardDetail:16","CardDetail:26","DrawReveal","Upgrades","Prestige","Settings","Naming","Statistics","Odds","Purchase:0","BuyAll","Help","Privacy","SaveRecovery","RecoveryExit","Appearance","Guidance","Exact","Achievements","DeveloperReset","Offline","TutorialHelp"}:new[]{"Home","Products","Settings","Skills","ProductDetail:0","CardDetail:0","Statistics","Achievements"};
                var sizes=SessionState.GetBool(Key+"Complete",false)?new[]{new Vector2Int(1080,2200),new Vector2Int(390,844),new Vector2Int(360,640),new Vector2Int(360,540)}:new[]{new Vector2Int(1080,2200),new Vector2Int(360,540)};
                foreach(var size in sizes)
                {
                    ClassicImportQA.Viewport(size.x,size.y);await Task.Delay(350);
                    foreach(var route in routes)
                    {
                        ui.SetTab(route=="Home"||ui.panels.Any(v=>v.route==route)?route:"Products");
                        if(route=="SaveRecovery"){ui.application.enabled=false;ui.ShowRecovery("Your progress is safe on this device. Saving is currently unavailable. Retry before continuing.");}
                        else if(route=="Guidance")ui.guidance.OpenTopic("product-levels");
                        else if(!ui.panels.Any(v=>v.route==route))ui.Open(route);
                        ui.motion.reduced=true;ui.Refresh();
                        var view=ui.CurrentDialog!=""?ui.dialogs.Single(v=>v.route==ui.CurrentDialog):ui.panels.Single(v=>v.route==ui.CurrentTab);
                        if(view.scroll)view.scroll.verticalNormalizedPosition=1;
                        await Task.Delay(160);await Capture(pass,route,size.x,size.y);
                        if(view.scroll&&view.scroll.content.rect.height>view.scroll.viewport.rect.height+2){view.scroll.verticalNormalizedPosition=0;await Task.Delay(120);await Capture(pass,route+"-bottom",size.x,size.y);view.scroll.verticalNormalizedPosition=1;}
                        if(route=="SaveRecovery"){ui.ReturnFromRecovery();ui.application.enabled=true;}
                    }
                }
                ui.SetTab("Home");ui.Open("Settings");await Task.Delay(100);var settings=ui.dialogs.Single(v=>v.route=="Settings");settings.scroll.verticalNormalizedPosition=.2f;
                foreach(string child in new[]{"Naming","Privacy","Help","Statistics","DeveloperReset"})
                {ui.Open(child);Check(settings.root.activeInHierarchy&&!settings.group.interactable,"Retained settings behind "+child);ui.Close();Check(ui.CurrentDialog=="Settings","Returns to retained Settings from "+child);}
                if(SessionState.GetBool(Key+"Complete",false))
                {
                    ClassicBazaarLayersValidation.Start();await Until(()=>ClassicBazaarLayersValidation.Result!="RUNNING");
                    Check(ClassicBazaarLayersValidation.Result.StartsWith("PASS"),"Nested layers, focus, scrolling and shared button roles: "+ClassicBazaarLayersValidation.Result);
                    await Edges(pass);
                }
                Check(before.SequenceEqual(ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).OrderBy(n=>n)),"No runtime UI construction");
                Check(errors.Count==0,"No runtime errors: "+string.Join("; ",errors));
                SessionState.SetString(Key+"Result","PASS "+checks.Count+" functional checks; "+records.Count+" rendered states");
            }
            catch(Exception e){SessionState.SetString(Key+"Result","FAIL "+e);Debug.LogException(e);}
            finally
            {
                Application.logMessageReceived-=Log;File.WriteAllText(Folder+pass+"/measurements.json",JsonConvert.SerializeObject(records,Formatting.Indented));File.WriteAllLines(Folder+pass+"/checks.txt",checks.Concat(new[]{Result}));EditorApplication.isPlaying=false;
            }
        }
        static async Task Capture(string pass,string route,int width,int height)
        {
            await Task.Delay(120);Canvas.ForceUpdateCanvases();
            string file=route.Replace(':','-')+"-"+width+"x"+height+".png";ClassicImportQA.Capture(Folder+pass+"/"+file);
            var root=ui.CurrentDialog!=""?ui.dialogs.Single(v=>v.route==ui.CurrentDialog).window:ui.panels.Single(v=>v.route==ui.CurrentTab).window;
            var texts=root.GetComponentsInChildren<TMP_Text>().Where(t=>t.isActiveAndEnabled&&t.color.a>0&&t.canvasRenderer.GetInheritedAlpha()>.01f).Select(t=>new{path=PathOf(t.transform),text=t.text,size=t.fontSize,width=t.rectTransform.rect.width,height=t.rectTransform.rect.height,preferred=t.GetPreferredValues(t.text,t.rectTransform.rect.width,0).y,font=t.font.name,inkOverflow=InkOverflow(t)});
            var buttons=root.GetComponentsInChildren<Button>().Where(b=>b.isActiveAndEnabled).Select(b=>new{name=b.name,width=R(b.transform).rect.width,height=R(b.transform).rect.height});
            records.Add(new{route,width,height,dialog=ui.CurrentDialog,file,texts=texts.ToArray(),buttons=buttons.ToArray()});
        }
        static RectTransform R(Transform t)=>t as RectTransform;
        static async Task Edges(string pass)
        {
            foreach(var size in new[]{new Vector2Int(1080,2200),new Vector2Int(390,844),new Vector2Int(360,640),new Vector2Int(360,540)})
            {
                ClassicImportQA.Viewport(size.x,size.y);await Task.Delay(250);
                var safe=ui.GetComponent<ClassicLayout>().safe;var owner=safe.GetComponent<RocketRaja.ImportedSafeArea>();if(owner)owner.enabled=false;
                safe.anchorMin=new Vector2(0,34f/size.y);safe.anchorMax=new Vector2(1,1-44f/size.y);safe.offsetMin=safe.offsetMax=Vector2.zero;
                ui.SetTab("Home");ui.Refresh();await Capture(pass,"Safe-Home",size.x,size.y);
                ui.Open("Settings");await Capture(pass,"Safe-Settings",size.x,size.y);
                var window=ui.dialogs.Single(v=>v.route=="Settings").window;var corners=new Vector3[4];window.GetWorldCorners(corners);
                Check(corners.Min(c=>c.y)>=34&&corners.Max(c=>c.y)<=size.y-44+.1f,"Safe-area window clearance "+size);
                if(owner)owner.enabled=true;else{safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;}
            }
            ui.SetTab("Home");var state=ui.Engine.State;var cash=state.cash;string name=state.storeName;
            Check(ui.driver.Command(e=>{e.State.cash=Amount.Pow10(99999);e.State.storeName=new string('W',24);return e.Save();}),"Extreme balance and long-name fixture");ui.Refresh();await Task.Delay(180);await Capture(pass,"Long-Home",360,540);
            var money=ui.textBindings.Single(b=>b.key=="cash").text;var sign=ui.textBindings.Single(b=>b.key=="shop-name").text;
            Check(InkOverflow(money)==0&&InkOverflow(sign)==0,"Large balance and 24-character name fit");
            ui.driver.Command(e=>{e.State.cash=cash;e.State.storeName=name;return e.Save();});ui.Refresh();
            var taps=state.manualTaps;var point=RectTransformUtility.WorldToScreenPoint(null,ui.tapTarget.transform.TransformPoint(R(ui.tapTarget.transform).rect.center));
            var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=-1,position=point};
            ui.tapTarget.OnPointerDown(pointer);ui.tapTarget.OnBeginDrag(pointer);pointer.position+=new Vector2(0,40);ui.tapTarget.OnDrag(pointer);ui.tapTarget.OnEndDrag(pointer);ui.tapTarget.OnPointerUp(pointer);
            Check(state.manualTaps.Equals(taps),"Counter drag never earns cash");
            ui.panels.Single(v=>v.route=="Home").scroll.OnScroll(new PointerEventData(EventSystem.current){scrollDelta=new Vector2(0,-2)});
            Check(state.manualTaps.Equals(taps),"Home scrolling never earns cash");
            ui.Open("Settings");pointer.position=point;ui.tapTarget.OnPointerDown(pointer);ui.tapTarget.OnPointerUp(pointer);
            Check(state.manualTaps.Equals(taps),"Modal interaction blocks underlying cash input");ui.SetTab("Home");
        }
        static int InkOverflow(TMP_Text t)
        {
            var rect=t.rectTransform.rect;int count=0;
            foreach(var c in t.textInfo.characterInfo.Take(t.textInfo.characterCount).Where(c=>c.isVisible))if(c.bottomLeft.x<rect.xMin-1||c.topRight.x>rect.xMax+1||c.bottomLeft.y<rect.yMin-1||c.topRight.y>rect.yMax+1)count++;
            return count;
        }
        static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
    }
}
