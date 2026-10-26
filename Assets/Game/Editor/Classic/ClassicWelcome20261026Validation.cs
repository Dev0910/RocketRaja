using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicWelcome20261026Validation
    {
        const string Key="Classic.Welcome20261026.Validation", Output="Documentation/Implementation/Welcome20261026/";
        static readonly List<string> checks=new List<string>();
        static readonly List<string> errors=new List<string>();
        static ClassicWelcome20261026Validation(){EditorApplication.playModeStateChanged+=Mode;}
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            if(EditorSceneManager.GetSceneManagerSetup().Any(s=>s.isLoaded&&SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Save edits first");
            SessionState.SetBool(Key,true);EditorSceneManager.playModeStartScene=null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange mode)
        {
            if(!SessionState.GetBool(Key,false))return;
            if(mode==PlayModeStateChange.EnteredPlayMode)Run();
            if(mode==PlayModeStateChange.EnteredEditMode){SessionState.EraseBool(Key);EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");EditorSceneManager.OpenScene("Assets/Scenes/Main.unity",OpenSceneMode.Additive);}
        }
        static void Check(bool value,string message){checks.Add((value?"PASS ":"FAIL ")+message);if(!value)throw new Exception(message);}
        static async Task Until(Func<bool> test){double end=EditorApplication.timeSinceStartup+25;while(!test()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException();await Task.Delay(60);}await Task.Delay(150);}
        static T Field<T>(BootstrapController owner,string name)=>(T)typeof(BootstrapController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(owner);
        static int[] Identity(Component root)=>root.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).ToArray();
        static void Log(string text,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        static async void Run()
        {
            checks.Clear();errors.Clear();Application.logMessageReceived+=Log;
            string profile=Path.Combine(Path.GetTempPath(),"Welcome20261026-"+Guid.NewGuid().ToString("N"));
            try
            {
                var coordinator=new RedesignAccountCoordinator(new ClassicProductionValidation.FixtureAuth(),new RedesignAccountPlayValidation.FixtureCloud(),RedesignRuntimeContent.Load(),profile,"welcome-review",JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>,JsonUtility.ToJson,JsonUtility.FromJson<AccountCheckpoint>,()=>Time.realtimeSinceStartupAsDouble,()=>RedesignRuntimeDriver.Utc);
                var host=RedesignSessionHost.Ensure();host.ConfigureFixture(coordinator,profile);
                await SceneManager.LoadSceneAsync("Bootstrap");await Until(()=>UnityEngine.Object.FindFirstObjectByType<BootstrapController>());
                var owner=UnityEngine.Object.FindFirstObjectByType<BootstrapController>();var identities=Identity(owner);
                Check(owner.transform.Find("Immersive welcome revision 20261026")!=null,"Saved immersive hierarchy loaded from Bootstrap");
                Check(!owner.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("YOUR FIREWORKS STORY")),"Removed tagline stays absent");
                for(int i=0;i<3;i++){owner.DismissPopup();owner.ReopenPopup();}
                Check(identities.SequenceEqual(Identity(owner)),"Welcome dismiss/reopen retains every UI component identity");
                foreach(var size in new[]{new Vector2Int(360,540),new Vector2Int(360,640),new Vector2Int(390,844)})
                {
                    ClassicImportQA.Viewport(size.x,size.y);await Task.Delay(450);Canvas.ForceUpdateCanvases();
                    foreach(string field in new[]{"primaryButton","secondaryButton"})
                    {var r=((RectTransform)Field<Button>(owner,field).transform).rect;Check(r.height>=143.9f,"48px logical button height "+field+" at "+size);}
                    var footer=Field<GameObject>(owner,"footer");var note=footer.GetComponentInChildren<TMP_Text>();
                    Check(note.text.Contains("Google progress follows you between devices."),"Both footer statements retained at "+size);
                    ClassicImportQA.Capture(Output+"Welcome-"+size.x+"x"+size.y+".png");
                }
                Field<Button>(owner,"secondaryButton").onClick.Invoke();
                await Until(()=>UnityEngine.Object.FindFirstObjectByType<ClassicApplication>()?.Ready==true);
                var ui=UnityEngine.Object.FindFirstObjectByType<ClassicApplication>().ui;
                if(ui.CurrentDialog=="Offline"){ui.Handle("Offline","");await Task.Delay(250);}
                Check(ui.CurrentTab=="Home","Guest choice reaches production Main Home");
                var mainIdentity=Identity(ui);Check(ui.motion.tapHint.gameObject.activeInHierarchy,"Painted banner visible in production Home");
                foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(390,844)})
                {ClassicImportQA.Viewport(size.x,size.y);await Task.Delay(450);Canvas.ForceUpdateCanvases();ClassicImportQA.Capture(Output+"Home-"+size.x+"x"+size.y+".png");}
                var target=(RectTransform)ui.tapTarget.transform;var corners=new Vector3[4];ui.motion.tapHint.GetWorldCorners(corners);
                var point=(corners[0]+corners[2])*.5f;point.y-=5;
                var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,point)};
                Check(RectTransformUtility.RectangleContainsScreenPoint(target,pointer.position),"Lower half of banner lies inside existing tap target");
                var taps=ui.Engine.State.manualTaps;ui.tapTarget.OnPointerDown(pointer);ui.tapTarget.OnPointerUp(pointer);
                Check(ui.Engine.State.manualTaps.Value==taps.Value+1,"Banner tap earns exactly once through authoritative owner");
                Check(ui.progression.summary.text=="TAP TO EARN","Progress refresh retains approved tap wording");
                taps=ui.Engine.State.manualTaps;ui.tapTarget.OnPointerDown(pointer);ui.tapTarget.OnBeginDrag(pointer);ui.tapTarget.OnPointerUp(pointer);
                Check(ui.Engine.State.manualTaps.Value==taps.Value,"Dragging does not earn a tap");
                ui.Open("Settings");taps=ui.Engine.State.manualTaps;ui.tapTarget.Tap();Check(ui.Engine.State.manualTaps.Value==taps.Value,"Open popup blocks stall tapping");ui.Close();
                await Task.Delay(650);Check(ui.motion.tapHint.localScale==Vector3.one,"Native tap pulse returns to saved pose");
                Check(mainIdentity.SequenceEqual(Identity(ui)),"Tap, popup and refresh create no UI objects or components");
                Check(errors.Count==0,"No runtime errors in welcome/banner checks");
            }
            catch(Exception ex){checks.Add("FAIL "+ex);}
            finally{Application.logMessageReceived-=Log;checks.AddRange(errors.Select(e=>"ERROR "+e));checks.Add("Isolated fixture profile: "+profile);Directory.CreateDirectory(Output);File.WriteAllLines(Output+"Validation.txt",checks);EditorApplication.ExitPlaymode();}
        }
    }
}
