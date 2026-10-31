using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RocketRaja.Classic;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicTutorialPlayValidation
    {
        const string Key="RocketRaja.TutorialQA.", ProfileKey="RocketRaja.LocalOnlyQA.Profile";
        public const string Folder="Documentation/Implementation/Tutorial";
        static readonly List<string> checks=new List<string>();
        static ClassicUI ui;
        static HashSet<string> identities;
        static ClassicTutorialPlayValidation(){EditorApplication.playModeStateChanged+=Mode;}
        public static string Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)throw new InvalidOperationException("Ready Edit Mode required");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Preserve unsaved scene first");
            if(SessionState.GetBool(Key+"Running",false))return "Already running";
            Directory.CreateDirectory(Folder+"/Renders");
            string root=Path.Combine(Path.GetTempPath(),"ClassicTutorialPlay-"+Guid.NewGuid().ToString("N"));
            SessionState.SetString(Key+"OldProfile",SessionState.GetString(ProfileKey,""));
            SessionState.SetString(ProfileKey,root);SessionState.SetBool(Key+"Running",true);
            File.WriteAllText(Folder+"/PlayChecks.txt","RUNNING production Bootstrap → Main with disposable profile "+root+"\n");
            EditorSceneManager.playModeStartScene=null;EditorSceneManager.OpenScene(ClassicProductionAuthoring.Bootstrap);
            ClassicImportQA.Viewport(360,640);EditorApplication.EnterPlaymode();return "Started isolated production tutorial checks: "+root;
        }
        static void Mode(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Key+"Running",false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Run();
            if(state==PlayModeStateChange.EnteredEditMode){
                SessionState.SetString(ProfileKey,SessionState.GetString(Key+"OldProfile",""));SessionState.SetBool(Key+"Running",false);
                EditorApplication.delayCall+=()=>{EditorSceneManager.OpenScene(ClassicProductionAuthoring.Bootstrap);EditorSceneManager.OpenScene(ClassicProductionAuthoring.Main,OpenSceneMode.Additive);};
            }
        }
        static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("FAIL "+label);checks.Add("PASS "+label);File.AppendAllText(Folder+"/PlayChecks.txt","PASS "+label+"\n");}
        static async Task Until(Func<bool> condition)
        {
            double end=EditorApplication.timeSinceStartup+25;
            while(!condition()) {if(EditorApplication.timeSinceStartup>end)throw new TimeoutException("Tutorial state did not settle");await Task.Delay(50);}
        }
        static async Task Recover()
        {
            for(int i=0;i<5&&!ui.application.Ready;i++){ui.application.Retry();await Task.Delay(250);}
            await Until(()=>ui.application.Ready);
        }
        static async Task Click(string action,string arg="")
        {
            var c=ui.GetComponentsInChildren<ClassicControl>(true).First(x=>x.action==action&&x.parameter==arg&&x.gameObject.activeInHierarchy);
            Check(c.GetComponent<Button>().IsInteractable(),"Authored control enabled: "+action+":"+arg);
            c.GetComponent<Button>().onClick.Invoke();await Task.Delay(200);
            if(!ui.application.Ready)await Recover();
        }
        static async Task Capture(string name){await Task.Delay(350);ClassicImportQA.Capture(Folder+"/Renders/"+name+".png");if(ui.tutorial.bubble.gameObject.activeInHierarchy)Check(!ui.tutorial.title.isTextOverflowing&&!ui.tutorial.copy.isTextOverflowing,"Live dialogue text fits: "+name);}
        static async Task Note(TutorialNote note,string tab)
        {
            await Until(()=>ui.Engine.State.tutorial.note==note&&ui.CurrentTab==tab&&ui.tutorial.bubble.gameObject.activeInHierarchy);
        }
        static async Task Dismiss(){ui.tutorial.dismiss.onClick.Invoke();await Task.Delay(200);if(!ui.application.Ready){await Recover();ui.tutorial.dismiss.onClick.Invoke();await Task.Delay(200);}}
        static void PointerCenter()
        {
            var t=ui.tutorial;Check(t.hand.gameObject.activeInHierarchy&&t.Target,"Hand appears for the available CTA");
            float error=Vector3.Distance(t.hand.position,t.Target.TransformPoint(t.Target.rect.center));
            Check(error<.1f,"Opaque fingertip pivot is at CTA center (error "+error.ToString("F4")+")");
            Check(!t.hand.GetComponent<Image>().raycastTarget,"Decorative hand passes all input through");
        }
        static void Tap()
        {
            var p=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,ui.tutorial.tapBanner.position),pointerId=-1};
            ui.tapTarget.OnPointerDown(p);ui.tapTarget.OnPointerUp(p);
        }
        static async void Run()
        {
            checks.Clear();
            try {
                await Until(()=>UnityEngine.Object.FindAnyObjectByType<ClassicApplication>());
                ui=UnityEngine.Object.FindAnyObjectByType<ClassicApplication>().ui;await Recover();
                identities=new HashSet<string>(ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetEntityId().ToString()),StringComparer.Ordinal);
                await Note(TutorialNote.Welcome,"Home");PointerCenter();await Capture("01-welcome-360x640");
                Tap();await Until(()=>ui.Engine.State.tutorial.note==TutorialNote.None);
                Check(ui.Engine.State.manualTaps.Value==1,"Opening tap collapses dialogue without requiring dismissal");
                for(int i=0;i<9;i++)Tap();await Note(TutorialNote.Training,"Player");PointerCenter();await Capture("02-training-360x640");
                ui.SetTab("Products");await Task.Delay(250);Check(!ui.tutorial.bubble.gameObject.activeSelf,"Free navigation hides a note belonging to a different tab");
                ui.SetTab("Player");await Task.Delay(250);Check(ui.tutorial.bubble.gameObject.activeSelf,"Returning to the target tab retains the pending note");
                var old=ui.Engine.State.cash;await Dismiss();Check(ui.Engine.State.tapLevel==0&&ui.Engine.State.cash.CompareTo(old)==0,"Dialogue tap does not execute training");
                ui.SetTab("Home");Tap();await Task.Delay(200);Check(ui.CurrentTab=="Home","Later taps do not repeat Player introduction");
                ui.tutorial.GoToGoal();await Task.Delay(300);await Click("BuyLevels","-1");await Note(TutorialNote.TrainingResult,"Home");await Capture("03-training-result");await Dismiss();
                while(!ui.Engine.LevelQuote(0,1).affordable)Tap();await Note(TutorialNote.Product,"Products");PointerCenter();await Capture("04-product-360x640");
                ClassicImportQA.Viewport(390,844);await Capture("04-product-390x844");PointerCenter();
                ClassicImportQA.Viewport(360,540);await Capture("04-product-360x540");PointerCenter();
                ClassicImportQA.Viewport(360,640);await Task.Delay(200);
                await Click("BuyLevels","0");await Note(TutorialNote.ProductResult,"Home");await Dismiss();
                // Accelerated elapsed time uses the authoritative engine rate; no cash is seeded.
                ui.Engine.Advance(Amount.Ratio(ui.Engine.UpgradeCost(RedesignEngine.TutorialUpgrade),ui.Engine.NormalIncome())+.1);
                await Note(TutorialNote.Upgrade,"Player");PointerCenter();await Capture("05-upgrade");
                await Click("BuyUpgrade",RedesignEngine.TutorialUpgrade);await Note(TutorialNote.UpgradeResult,"Home");await Dismiss();
                int taps=0;while(RedesignAchievements.ReadyCount(ui.Engine.State)==0&&taps++<600)Tap();
                await Note(TutorialNote.Achievement,"Player");PointerCenter();await Capture("06-achievement");
                await Click("Navigate","Achievements");Check(!ui.tutorial.hand.gameObject.activeSelf&&!ui.tutorial.bubble.gameObject.activeSelf,"Claim modal suspends tutorial layers");
                var claim=RedesignAchievements.All.First(a=>RedesignAchievements.StageReady(ui.Engine.State,a));
                await Click("ClaimAchievement",claim.Id);ui.Close();await Note(TutorialNote.AchievementResult,"Home");await Dismiss();
                int steps=0;
                while(!ui.Engine.PreviewPrestige().Eligible) {
                    if(++steps>6000)throw new Exception("Earned journey stalled");
                    ClassicJourneyValidation.Step(ui.Engine,(p,q)=>ui.driver.Command(e=>e.BuyLevels(p,q)),()=>ui.driver.Command(e=>e.CommitBuyAll(e.QuoteBuyAll())));
                    if(!ui.application.Ready)await Recover();
                    if(steps%10==0)await Task.Delay(1);
                }
                await Note(TutorialNote.Prestige,"Player");PointerCenter();await Capture("07-prestige");
                await Click("Navigate","Prestige");await Capture("07-prestige-review");
                await Click("Close");await Task.Delay(400);Check(ui.Engine.State.completedPrestiges.IsZero,"Prestige review can be cancelled without resetting");
                ui.Open("Prestige");await Task.Delay(400);await Click("Prestige");await Note(TutorialNote.Talent,"Legacy");PointerCenter();await Capture("08-free-talent");
                var credits=ui.Engine.State.freeCredits;await Click("Draw","1");await Until(()=>ui.revealContinue.interactable);await Capture("09-committed-reveal");
                Check(!ui.tutorial.hand.gameObject.activeSelf,"Reveal suppresses tutorial pointer");
                await Click("ContinueReveal");await Note(TutorialNote.CardBenefit,"Home");await Capture("10-card-benefit");
                Check(ui.Engine.State.freeCredits.Value==credits.Value-1,"Tutorial result uses exactly one committed draw");await Dismiss();
                Check(!ui.tutorial.Active,"Card-benefit acknowledgement completes course");
                var after=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetEntityId().ToString());
                Check(identities.SetEquals(after),"All UI component identities retained through full journey; no runtime construction");
                ui.Open("Settings");await Task.Delay(400);await Click("Navigate","TutorialHelp");await Capture("11-help");
                await Click("TutorialReplay");await Note(TutorialNote.Welcome,"Home");
                Check(ui.Engine.State.completedPrestiges.Value==1&&ui.Engine.State.freeCredits.Value==credits.Value-1,"Replay preserves earned progress and rewards");
                int serial=ui.Engine.State.tutorial.serial;
                using(var locked=new FileStream(Path.Combine(ui.driver.GuestProfileRoot,"rocket-raja-v2.save"),FileMode.Open,FileAccess.Read,FileShare.Read)) {
                    ui.tutorial.dismiss.onClick.Invoke();await Task.Delay(250);
                    Check(!ui.driver.Session.CanMutate&&ui.CurrentDialog=="SaveRecovery","Failed tutorial save uses visible production recovery");
                    Check(ui.Engine.State.tutorial.serial==serial&&ui.Engine.State.tutorial.note==TutorialNote.Welcome,"Failed acknowledgement retains pending note");
                    Check(!ui.tutorial.hand.gameObject.activeSelf&&!ui.tutorial.bubble.gameObject.activeSelf,"Recovery hides both tutorial layers");
                }
                await Recover();await Note(TutorialNote.Welcome,"Home");Check(ui.driver.Session.CanMutate,"Retry restores pending guidance after storage recovers");
                ui.Open("TutorialHelp");await Task.Delay(400);await Click("TutorialSkip");Check(!ui.tutorial.Active,"Explicit skip completes without a restart");
                File.AppendAllText(Folder+"/PlayChecks.txt","ALL "+checks.Count+" PLAY CHECKS PASSED\n");
            } catch(Exception ex){File.AppendAllText(Folder+"/PlayChecks.txt","FAILED "+ex+"\n");Debug.LogException(ex);}
            finally {EditorApplication.ExitPlaymode();}
        }
    }
}
