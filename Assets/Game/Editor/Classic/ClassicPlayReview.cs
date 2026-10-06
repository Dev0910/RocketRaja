using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Runs only against the isolated import, using saved controls and actual core commands.</summary>
    public static class ClassicPlayReview
    {
        sealed class Step { public string name; public Action action; public float wait; public bool capture; }
        static readonly Queue<Step> steps=new Queue<Step>();static readonly List<string> results=new List<string>();
        static ClassicUI ui;static string folder;static double due;static Step current;static string[] identities;static int checks;static double levels;
        public static string Status { get; private set; }="Idle";
        static void Check(bool condition,string label){checks++;results.Add((condition?"PASS ":"FAIL ")+label);if(!condition)throw new InvalidOperationException(label);}
        static void Add(string name,Action action,float wait=.6f,bool capture=true)=>steps.Enqueue(new Step{name=name,action=action,wait=wait,capture=capture});
        static void Click(string action,string arg="")
        {
            var control=ui.GetComponentsInChildren<ClassicControl>(true).First(c=>c.action==action&&c.parameter==arg&&c.gameObject.activeInHierarchy);
            var button=control.GetComponent<Button>();Check(button.IsInteractable(),"Control enabled: "+action+":"+arg);button.onClick.Invoke();
        }
        static double Levels()=>ui.Engine.State.cardLevels.Sum(n=>(double)n.Value);
        public static string Begin(string output)
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Play Mode required");ui=ClassicImportQA.UI;if(!ui.isolatedPreview)throw new InvalidOperationException("Only the isolated importer may be tested");
            folder=output;Directory.CreateDirectory(folder);steps.Clear();results.Clear();checks=0;identities=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetEntityId().ToString()).OrderBy(i=>i).ToArray();
            if(ui.Engine.State.offline.pending)ui.Handle("Offline","");ui.SetTab("Home");
            Add("home",()=>{Check(ui.driver.Initialized,"Isolated session initialized");Check(ui.products.Length==8&&ui.cards.Length==28,"All products and Talent cards present");});
            Add("tap-flight",()=>{var before=ui.Engine.State.manualTaps;var p=new PointerEventData(EventSystem.current){pointerId=4,position=RectTransformUtility.WorldToScreenPoint(null,ui.tapTarget.transform.position),button=PointerEventData.InputButton.Left};ui.tapTarget.OnPointerDown(p);ui.tapTarget.OnPointerUp(p);Check(ui.Engine.State.manualTaps.Value==before.Value+1,"Counter tap grants once");ui.tapTarget.OnPointerDown(p);ui.tapTarget.OnBeginDrag(p);ui.tapTarget.OnPointerUp(p);Check(ui.Engine.State.manualTaps.Value==before.Value+1,"Drag does not tap");},.24f);
            Add("products",()=>{ui.SetTab("Products");int before=ui.Engine.State.products[0];Click("BuyLevels","0");Check(ui.Engine.State.products[0]==before+1,"Product purchase grants one level");});
            Add("player",()=>{ui.SetTab("Player");int before=ui.Engine.State.tapLevel;Click("BuyLevels","-1");Check(ui.Engine.State.tapLevel==before+1&&ui.CurrentDialog=="","Salesmanship buys directly");Click("BuyAll");Check(ui.Engine.State.upgrades.Count>3,"Buy all commits available upgrades");});
            Add("legacy",()=>{ui.SetTab("Legacy");Check(ui.cards.Where((c,i)=>ui.Engine.State.cardLevels[i].IsZero).All(c=>!c.button.interactable&&c.art.material==ui.undiscoveredMaterial),"Unowned cards grayscale and disabled");ui.Handle("Card","1");Check(ui.CurrentDialog=="","Unowned card cannot open");});
            Add("card-common",()=>Click("Card","0"));
            Add("outside-close",()=>{Click("Close");},.3f,false);
            Add("card-rare",()=>{Check(ui.CurrentDialog=="","Outside card click dismisses");ui.Handle("Card","16");});
            foreach(var route in new[]{"Shop","ProductDetail:0","TapTraining","Skills","SkillDetail:0","Prestige","Settings","Naming","Statistics","Offline","Odds","Purchase","Upgrades","BuyAll","Help","Privacy","Account","Welcome","AccountCompare","Handoff","SignOut","Promote","RecoveryConfirm","SaveRecovery","RecoveryExit","Appearance"})
            {var selected=route;Add("screen-"+route.Replace(':','-'),()=>{ui.SetTab("Home");ui.Open(selected);});}
            Add("draw-back",()=>{ui.SetTab("Legacy");levels=Levels();var kk=ui.Engine.State.kajuKatli;Click("Draw","5");Check(ui.Engine.State.freeCredits.IsZero&&ui.Engine.State.kajuKatli.Value==kk.Value,"Five cards cost four free credits");Check(Levels()==levels+5,"Five cards granted atomically once");Check(!ui.revealContinue.interactable,"Continue disabled during flip");},.15f);
            Add("draw-rotation",()=>{},.65f);
            Add("draw-front",()=>{},1.2f);
            Add("draw-closed",()=>{Check(ui.revealContinue.interactable,"Continue enabled when reveal settles");ui.Close();Check(ui.CurrentTab=="Legacy"&&ui.CurrentDialog==""&&!ui.Engine.State.reveal.pending,"Reveal close returns to collection");Check(Levels()==levels+5,"Closing does not grant again");});
            Add("paid-draw",()=>{levels=Levels();var kk=ui.Engine.State.kajuKatli;ui.Handle("Draw","5");Check(ui.Engine.State.kajuKatli.Value==kk.Value-400,"Paid five cards cost 400 KK");Check(Levels()==levels+5,"Paid batch grants five once");ui.Handle("Setting","motion");},.3f);
            Add("reduced-reveal",()=>{Check(ui.revealContinue.interactable,"Reduced motion settles flip");for(int i=0;i<5;i++)ui.Handle("ContinueReveal","");Check(!ui.Engine.State.reveal.pending&&ui.CurrentTab=="Legacy","Continue finishes all five results");Check(Levels()==levels+5,"Continuing never grants twice");});
            Add("short-home",()=>{ui.Handle("Setting","motion");ClassicImportQA.Viewport(360,540);ui.SetTab("Home");});
            Add("short-product",()=>ui.Open("ProductDetail:0"));
            Add("tall-legacy",()=>{ClassicImportQA.Viewport(390,844);ui.SetTab("Legacy");});
            Add("tall-card",()=>ui.Handle("Card","0"));
            Add("safe-home",()=>{ui.SetTab("Home");var safe=ui.transform.Find("SafeArea").GetComponent<RectTransform>();safe.GetComponent<ImportedSafeArea>().enabled=false;safe.anchorMin=new Vector2(0,34f/844);safe.anchorMax=new Vector2(1,1-44f/844);});
            Add("finish",()=>{ui.transform.Find("SafeArea").GetComponent<ImportedSafeArea>().enabled=true;ClassicImportQA.Viewport(360,640);ui.SetTab("Home");var after=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetEntityId().ToString()).OrderBy(i=>i).ToArray();Check(identities.SequenceEqual(after),"All UI/component identities stable across navigation, effects and reveals");Check(ui.revealCard.localRotation==Quaternion.identity,"Reveal returns to authored pose");results.Add(ClassicImportQA.Audit());},.5f,false);
            current=null;due=0;Status="Running";EditorApplication.update-=Tick;EditorApplication.update+=Tick;return "Native review started: "+steps.Count+" states";
        }
        static void Tick()
        {
            if(!Application.isPlaying){End("Interrupted");return;}if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(current!=null&&current.capture)ClassicImportQA.Capture(Path.Combine(folder,current.name+".png"));
                if(steps.Count==0){End("PASS");return;}
                current=steps.Dequeue();Status=current.name;current.action();due=EditorApplication.timeSinceStartup+current.wait;
            }
            catch(Exception ex){results.Add(ex.ToString());End("FAIL");}
        }
        static void End(string status){Status=status+" · "+checks+" checks";EditorApplication.update-=Tick;File.WriteAllLines(Path.Combine(folder,"native-review.txt"),results.Concat(new[]{Status}));Debug.Log("CLASSIC_REVIEW "+Status);}
    }
}
