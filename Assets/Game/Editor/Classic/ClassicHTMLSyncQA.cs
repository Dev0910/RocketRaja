using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Classic;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    /// <summary>Explicit disposable QA only. Not a player entry point or build scene.</summary>
    public static class ClassicHTMLSyncQA
    {
        const string QAPath="Assets/Scenes/HTML Importer.unity";
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static ClassicUI UI=>SceneManager.GetActiveScene().GetRootGameObjects().Where(g=>g.activeSelf).Select(g=>g.GetComponent<ClassicUI>()).Single(c=>c);
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        public static string Prepare()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play first");Require(!File.Exists(QAPath),"QA scene already exists; inspect before replacing");
            for(int i=0;i<SceneManager.sceneCount;i++){var scene=SceneManager.GetSceneAt(i);Require(scene.path==BuildClassicUI.ScenePath||scene.path=="Assets/Scenes/Bootstrap.unity","Unexpected open scene");if(scene.isDirty)EditorSceneManager.SaveScene(scene);}
            var main=SceneManager.GetSceneByPath(BuildClassicUI.ScenePath);EditorSceneManager.SaveScene(main,QAPath,true);
            var qa=EditorSceneManager.OpenScene(QAPath,OpenSceneMode.Single);var ui=UI;
            Object.DestroyImmediate(ui.application);ui.application=null;ui.isolatedPreview=true;
            var preview=ui.gameObject.AddComponent<ClassicPreviewSession>();preview.driver=ui.driver;preview.seedReviewProgress=true;
            EditorSceneManager.MarkSceneDirty(qa);EditorSceneManager.SaveScene(qa);ClassicImportQA.Viewport(390,844);
            return "Saved disposable HTML Importer QA copy. Production startup removed; no production profile is loaded";
        }
        public static string AuditSaved()
        {
            var ui=UI;var issues=new List<string>();
            if(ui.dialogs.Length!=30||ui.dialogs.Count(v=>v.route=="Achievements")!=1)issues.Add("Expected 30 dialogs including Achievements");
            if(ui.achievementRows.Length!=15||ui.achievementRows.Any(r=>!r.progress||!r.reward||!r.claim||!r.action||!r.fill))issues.Add("Missing achievement reward binding");
            if(ui.textBindings.GroupBy(b=>b.key).Any(g=>g.Count()>1))issues.Add("Duplicate text binding");
            if(ui.textBindings.Any(b=>!b.text)||ui.imageBindings.Any(b=>!b.image))issues.Add("Missing binding target");
            if(ui.GetComponentsInChildren<Transform>(true).Any(t=>t.GetComponents<Component>().Any(c=>!c)))issues.Add("Missing component script");
            if(ui.guidance.rows.Any(r=>!r.root||!r.title||!r.brief))issues.Add("Stale guidance reference");
            if(ui.skillClocks.Length!=7||ui.skillClocks.Any(c=>!c.ring||!c.ring.sprite||!c.track||!c.disc||c.ring.fillMethod!=Image.FillMethod.Radial360))issues.Add("Missing radial clock");
            if(ui.dialogs.Single(v=>v.route=="Settings").window.GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text=="Back"))issues.Add("Settings back footer remains");
            if(ui.dialogs.Single(v=>v.route=="SkillDetail").window.GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text=="View skill upgrades"))issues.Add("Skill upgrade footer remains");
            foreach(var v in ui.dialogs){var b=v.root.transform.Find("Outside dismissal").GetComponent<Button>();if(b.interactable==(v.route=="SaveRecovery"||v.route=="RecoveryExit"))issues.Add("Backdrop policy "+v.route);}
            foreach(var row in ui.nextUpgrades.Concat(ui.catalogueRows))if(((RectTransform)row.buy.transform).offsetMax.x!=-10)issues.Add("Upgrade gutter");
            Require(issues.Count==0,string.Join("; ",issues));
            return "PASS saved hierarchy: 30 dialogs, 15 achievements, 7 radial clocks, valid unique bindings, recovery guards, removed footers, 30 upgrade gutters; objects="+ui.GetComponentsInChildren<Transform>(true).Length;
        }
        static void Invoke(string method,params object[] args)=>typeof(ClassicUI).GetMethod(method,Flags).Invoke(UI,args);
        static string Bound(string key)=>UI.textBindings.Single(b=>b.key==key).text.text;
        static void Hide()=>Invoke("CloseAll");
        public static string Behaviors()
        {
            Require(EditorApplication.isPlaying,"Play disposable scene first");var ui=UI;Require(ui.driver.IsDisposableGuest,"QA must be isolated");
            ui.Bind();var e=ui.Engine;var s=e.State;var log=new List<string>();ui.motion.reduced=true;s.reducedMotion=true;
            Hide();ui.SetTab("Home");int count=ui.GetComponentsInChildren<RectTransform>(true).Length;
            var before=s.cash;string resting=Bound("home-total");Invoke("Tap");Require(s.cash>before,"Accepted tap missing");Require(Bound("home-total")!=resting,"Combined rate did not change");e.Advance(1.01);ui.Refresh();Require(Bound("home-total")==Bound("home-income"),"Tap sample did not expire");log.Add("accepted tap rate + one-second expiry");
            for(int i=0;i<3;i++)
            {
                s.activeUntil[i]=s.readyAt[i]=0;ui.Refresh();ui.Handle("ActivateSkill",i.ToString());s=e.State;Require(ui.CurrentDialog=="","Home activation opened popup");Require(s.readyAt[i]>s.now,"Wrong skill activated");var c=ui.skillClocks[i];Require(!c.activate.interactable,"Skill repeated while unavailable");
                if(i<2){Require(s.activeUntil[i]>s.now&&c.ring.fillAmount>.99f,"Active ring missing");e.Advance(e.PreviewSkill(i).duration+.01);ui.Refresh();Require(c.artwork.material==ui.undiscoveredMaterial,"Cooldown art not grey");}
                else Require(s.activeUntil[i]<=s.now&&c.ring.fillAmount<.01f,"Instant skill did not enter cooldown");
                e.Advance(s.readyAt[i]-s.now+.01);ui.Refresh();Require(c.activate.interactable&&c.ring.fillAmount==1,"Skill did not become ready");
            }
            log.Add("all three skill guards, active gold / cooldown grey / ready ring, instant Bulk Order");
            ui.SetTab("Player");var opener=ui.GetComponentsInChildren<ClassicControl>(true).First(c=>c.parameter=="Skills"&&c.action=="Navigate"&&c.gameObject.activeInHierarchy).gameObject;
            EventSystem.current.SetSelectedGameObject(opener);ui.Open("Skills");var detail=ui.dialogs.Single(v=>v.route=="Skills").root.GetComponentsInChildren<ClassicControl>().First(c=>c.parameter=="SkillDetail:1").gameObject;
            EventSystem.current.SetSelectedGameObject(detail);ui.Open("SkillDetail:1");ui.Close();Require(ui.CurrentDialog=="Skills"&&EventSystem.current.currentSelectedGameObject==detail,"Parent/focus not restored");ui.Close();Require(ui.CurrentDialog==""&&EventSystem.current.currentSelectedGameObject==opener,"Opener focus not restored");log.Add("nested popup and invoking focus restoration");
            ui.Open("Settings");before=s.cash;Invoke("Tap");Require(s.cash.CompareTo(before)==0,"Backdrop input reached stall");
            Canvas.ForceUpdateCanvases();
            var pointer=new PointerEventData(EventSystem.current){position=new Vector2(2,Screen.height*.5f),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            var outside=ui.dialogs.Single(v=>v.route=="Settings").root.transform.Find("Outside dismissal").gameObject;ExecuteEvents.Execute(outside,pointer,ExecuteEvents.pointerClickHandler);Require(ui.CurrentDialog=="","Outside click did not close");Require(s.cash.CompareTo(before)==0,"Outside click earned money");
            ui.ShowRecovery("QA recovery");ui.Close();Require(ui.CurrentDialog=="SaveRecovery","Mandatory recovery dismissed");Hide();log.Add("modal gameplay block and mandatory recovery close guard");
            var owned=s.upgrades.ToList();s.upgrades.Clear();typeof(RedesignEngine).GetField("cached",Flags).SetValue(e,null);ui.Refresh();Require(ui.homeSkills.All(b=>!b.interactable)&&ui.skillClocks.Take(3).All(c=>!c.ring.enabled),"Locked skills appear ready");s.upgrades=owned;typeof(RedesignEngine).GetField("cached",Flags).SetValue(e,null);ui.Refresh();log.Add("locked state disables activation and ready rings");
            ui.driver.Session.OwnsSession=()=>false;ui.Refresh();Require(ui.homeSkills.All(b=>!b.interactable),"Blocked session enabled skills");before=e.State.cash;ui.Handle("ActivateSkill","2");Require(e.State.cash.CompareTo(before)==0,"Blocked skill granted income");ui.driver.Session.OwnsSession=()=>true;ui.Refresh();log.Add("save/session block does not grant skill rewards");
            foreach(var route in ui.dialogs.Select(v=>v.route).Where(r=>r!="DrawReveal"&&r!="SaveRecovery"&&r!="RecoveryExit")){Hide();ui.Open(route);ui.Refresh();Require(ui.CurrentDialog==route,"Route failed: "+route);ui.Close();}
            Hide();Require(count==ui.GetComponentsInChildren<RectTransform>(true).Length,"Runtime UI structure changed");ui.SetTab("Home");log.Add("26 ordinary dialog routes, controls retain preplaced hierarchy");
            return "PASS: "+string.Join("; ",log);
        }
        public static string Route(string route)
        {
            var ui=UI;ui.Bind();Hide();ui.motion.reduced=true;ui.Engine.State.reducedMotion=true;
            if(route=="Home"||route=="Player"||route=="Products"||route=="Legacy"||route=="Shop")ui.SetTab(route);
            else if(route=="DrawReveal"){ui.SetTab("Legacy");if(ui.Engine.State.reveal.pending)ui.Open("DrawReveal");else ui.Handle("Draw","1");}
            else {ui.SetTab("Player");ui.Open(route);}
            Canvas.ForceUpdateCanvases();ui.GetComponent<ClassicLayout>().Apply();Canvas.ForceUpdateCanvases();
            return ui.CurrentTab+"/"+ui.CurrentDialog;
        }
        public static string Capture(string path)
        {
            var ui=UI;Canvas.ForceUpdateCanvases();ui.GetComponent<ClassicLayout>().Apply();Canvas.ForceUpdateCanvases();
            return ClassicImportQA.Capture(path);
        }
        public static string LayoutAudit()
        {
            var ui=UI;var issues=new List<string>();var v=ui.dialogs.FirstOrDefault(d=>d.route==ui.CurrentDialog);
            if(v!=null)
            {
                var corners=new Vector3[4];v.window.GetWorldCorners(corners);
                if(corners[0].x<-.5f||corners[0].y<-.5f||corners[2].x>Screen.width+.5f||corners[2].y>Screen.height+.5f)issues.Add("window outside screen");
                if(v.scroll&&v.scroll.content.rect.width>v.scroll.viewport.rect.width+1)issues.Add("horizontal content overflow");
                if(v.route!="SaveRecovery"&&v.route!="RecoveryExit")Require(v.root.transform.Find("Outside dismissal").GetComponent<Button>().interactable,"Missing dismiss target");
                var title=v.window.Find("Header/Title");if(title&&title.GetComponent<TMP_Text>().isTextOverflowing)issues.Add("title overflow");
            }
            Require(issues.Count==0,ui.CurrentDialog+": "+string.Join(",",issues));return "PASS "+Screen.width+"x"+Screen.height+" "+ui.CurrentTab+"/"+ui.CurrentDialog;
        }
        public static string Hierarchy(string path)
        {
            var root=UI.transform;
            string Name(Transform t)=>t==root?t.name:Name(t.parent)+"/"+t.name;
            var rows=root.GetComponentsInChildren<RectTransform>(true).Select(t=>Name(t)+"|"+string.Join(",",t.GetComponents<Component>().Where(c=>c).Select(c=>c.GetType().FullName).OrderBy(n=>n))).OrderBy(s=>s);
            File.WriteAllLines(path,rows);return "Saved UI hierarchy and component types: "+path;
        }
        public static string Restore()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play before restoring");
            for(int i=0;i<SceneManager.sceneCount;i++)Require(!SceneManager.GetSceneAt(i).isDirty,"QA scene dirty; inspect first");
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity",OpenSceneMode.Single);
            AssetDatabase.DeleteAsset(QAPath);
            return "Restored original Bootstrap scene; disposable QA scene removed and excluded from builds";
        }
    }
}
