using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RocketRaja.Classic;
using RocketRaja.Cloud;
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
    public static class ClassicPresentationValidation
    {
        const string Key="Classic.PresentationValidation",Folder="Documentation/Implementation/ClassicPresentation";
        static readonly List<string> report=new List<string>(),errors=new List<string>();
        static ClassicPresentationValidation(){EditorApplication.playModeStateChanged+=Mode;}
        static void Mode(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Run();
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.EraseBool(Key);EditorSceneManager.OpenScene(ClassicProductionAuthoring.Bootstrap);}
        }
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            Directory.CreateDirectory(Folder);SessionState.SetBool(Key,true);EditorSceneManager.playModeStartScene=null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Check(bool ok,string text){report.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Folder+"/Validation.txt",report);if(!ok)throw new InvalidOperationException(text);}
        static void Log(string message,string trace,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)errors.Add(message);}
        static ClassicApplication App=>UnityEngine.Object.FindFirstObjectByType<ClassicApplication>();
        static async Task Until(Func<bool> test){double end=EditorApplication.timeSinceStartup+30;while(!test()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException("Classic did not settle");await Task.Delay(50);}await Task.Delay(150);}
        static async Task Capture(string name){await Task.Delay(350);ClassicImportQA.Capture(Folder+"/"+name+".png");}
        static async Task Home()
        {
            await Until(()=>App&&App.Ready);var ui=App.ui;
            if(ui.CurrentDialog=="Offline"){ui.Handle("Offline","");await Task.Delay(250);}
        }
        static async Task ReturnGuest()
        {
            App.OpenAccount();await Until(()=>UnityEngine.Object.FindFirstObjectByType<BootstrapController>());
            var bootstrap=UnityEngine.Object.FindFirstObjectByType<BootstrapController>();
            ((Button)typeof(BootstrapController).GetField("secondaryButton",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(bootstrap)).onClick.Invoke();await Home();
        }
        static PointerEventData Pointer(ClassicUI ui)=>new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,ui.tapTarget.transform.position)};
        static async void Run()
        {
            report.Clear();errors.Clear();Application.logMessageReceived+=Log;
            try
            {
                string root=Path.Combine(Path.GetTempPath(),"ClassicPresentation-"+Guid.NewGuid().ToString("N"));
                var auth=new ClassicProductionValidation.FixtureAuth();var cloud=new RedesignAccountPlayValidation.FixtureCloud();
                var host=RedesignSessionHost.Ensure();
                var coordinator=new RedesignAccountCoordinator(auth,cloud,RedesignRuntimeContent.Load(),root,"presentation-test",JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>,JsonUtility.ToJson,JsonUtility.FromJson<AccountCheckpoint>,()=>Time.realtimeSinceStartupAsDouble,()=>RedesignRuntimeDriver.Utc);
                host.ConfigureFixture(coordinator,root);await host.ReturnToGuestAsync(false);host.EnterMain();await Home();
                var ui=App.ui;var audio=ui.audioFeedback;ClassicImportQA.Viewport(360,540);await Task.Delay(500);
                Check(ui.guidance&&ui.progression&&audio&&audio.music&&audio.effects,"Saved Classic guidance, artwork and audio bindings exist");
                Check(ui.Engine.State.cash.CompareTo((Amount)30)==0&&ui.Engine.State.products.All(n=>n==0),"Native journey starts unboosted");
                Check(ui.guidance.CurrentTopic=="buy-first","Contextual first purchase guidance");await Capture("Fresh-360x540");
                foreach(string route in new[]{"Home","Player","Products","Legacy","Shop"})
                {
                    ui.SetTab(route);await Task.Delay(100);Canvas.ForceUpdateCanvases();ui.GetComponent<ClassicLayout>().Apply();
                    Check(ui.GetComponentsInChildren<Button>().Where(b=>b.IsInteractable()).All(b=>{var g=b.targetGraphic;var r=g.rectTransform.rect;var p=g.raycastPadding;return r.width-p.x-p.z>=47.99f&&r.height-p.y-p.w>=47.99f;}),"48-unit minimum touch bounds on short-screen "+route);
                }
                ui.SetTab("Home");
                ui.Handle("Tip","");await Capture("Guidance-360x540");ui.Handle("LearnTip","");await Task.Delay(250);
                Check(ui.Engine.State.learnedTopics.Contains("buy-first")&&ui.guidance.CurrentTopic!="buy-first","Acknowledged topic is saved and not repeated");
                var identities=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).ToArray();
                ui.SetTab("Products");ui.Handle("BuyLevels","0");await Task.Delay(120);
                Check(ui.Engine.State.products[0]==1&&audio.LastCue=="purchase","Manual first purchase emits one purchase cue");
                Check(ui.guidance.CurrentTopic=="tap-counter","First ownership introduces counter taps");
                var n=ui.Engine.State.manualTaps;var pointer=Pointer(ui);ui.tapTarget.OnPointerDown(pointer);ui.tapTarget.OnBeginDrag(pointer);pointer.position+=Vector2.up*40;ui.tapTarget.OnDrag(pointer);ui.tapTarget.OnPointerUp(pointer);ui.tapTarget.OnEndDrag(pointer);
                Check(ui.Engine.State.manualTaps.CompareTo(n)==0,"Dragging the counter never earns cash");
                pointer=Pointer(ui);ui.tapTarget.OnPointerDown(pointer);ui.tapTarget.OnPointerUp(pointer);
                Check(ui.Engine.State.manualTaps.CompareTo(n+(Natural)1)==0&&audio.LastCue=="tap","Stationary counter tap earns once with one cue");
                ui.Open("Settings");pointer=Pointer(ui);ui.tapTarget.OnPointerDown(pointer);ui.tapTarget.OnPointerUp(pointer);
                Check(ui.Engine.State.manualTaps.CompareTo(n+(Natural)1)==0,"Modal blocks underlying gameplay tap");
                await Task.Delay(200);Canvas.ForceUpdateCanvases();
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Check(hits.Count>0&&hits[0].gameObject!=ui.tapTarget.gameObject,"Actual UI raycast lands on modal before the gameplay surface");
                ui.Close();ui.SetTab("Products");ui.Open("Settings");ui.Close();ui.SetTab("Legacy");ui.Open("Prestige");await Task.Delay(500);
                Check(ui.CurrentDialog=="Prestige"&&ui.dialogs.Single(d=>d.route=="Prestige").window.localScale==Vector3.one,"Interrupted close cannot dismiss reopened content");
                ui.SetTab("Legacy");Check(ui.cards.All(c=>!c.button.IsInteractable()),"Unowned Talent controls disabled");ui.SetTab("Home");
                for(int run=0;run<2;run++)
                {
                    int quantity=1,steps=0;
                    while(!ui.Engine.PreviewPrestige().Eligible||ui.Engine.State.products.Any(p=>p==0)||Enumerable.Range(0,3).Any(i=>!ui.Engine.SkillOwned(i)))
                    {
                        if(++steps>6000)throw new InvalidOperationException("Native journey exceeded decision limit");
                        ClassicJourneyValidation.Step(ui.Engine,(p,q)=>
                        {
                            ui.SetTab("Products");while(quantity!=q){ui.Handle("Quantity","products");quantity=quantity==1?10:quantity==10?100:quantity==100?int.MaxValue:1;}
                            var before=ui.Engine.State.products[p];var quote=ui.Engine.LevelQuote(p,q);var cash=ui.Engine.State.cash;
                            if(!ui.products[p].cost.text.StartsWith("₹"+quote.cost))throw new InvalidOperationException("Displayed product price differs from commit engine");
                            ui.Handle("BuyLevels",p.ToString());if(ui.Engine.State.products[p]!=before+quote.quantity||ui.Engine.State.cash.CompareTo(cash-quote.cost)!=0)throw new InvalidOperationException("Displayed engine level quote drift");
                        },()=>
                        {
                            ui.SetTab("Player");var quote=ui.Engine.QuoteBuyAll();var owned=ui.Engine.State.upgrades.Count;
                            ui.Handle("BuyAll","");if(ui.Engine.State.upgrades.Count!=owned+quote.Ids.Count)throw new InvalidOperationException("Buy All quote drift");
                        });
                        if(steps%25==0)await Task.Delay(1);
                    }
                    Check(ui.Engine.State.products.All(p=>p>0),"Native run "+(run+1)+" unlocks all eight products using committed quotes");
                    Check(ui.progression.shop.DisplayedStage>=1,"Earned lifetime discoveries activate saved decoration layers");
                    for(int p=0;p<8;p++)Check(ui.progression.shelves[p].sprite==ui.progression.ProductSprite(p)&&ui.progression.rows[p].sprite==ui.progression.ProductSprite(p),"Shelf and product row use engine appearance "+p+" run "+(run+1));
                    for(int i=0;i<3;i++){ui.Open("SkillDetail:"+i);ui.Handle("ActivateSkill","");Check(audio.LastCue=="skill","Skill "+i+" emits committed feedback");ui.SetTab("Home");}
                    ClassicImportQA.Viewport(390,844);ui.Open("Statistics");await Capture("Run"+(run+1)+"-Progress");ui.SetTab("Home");await Capture("Run"+(run+1)+"-Shop");
                    var before=ui.Engine.State.Copy();int stage=ui.progression.shop.DisplayedStage;ui.Open("Prestige");var quote=ui.Engine.PreviewPrestige();await Capture("Run"+(run+1)+"-Prestige");ui.Handle("Prestige","");
                    Check(ui.Engine.State.products.All(p=>p==0)&&ui.Engine.State.upgrades.Count==0&&ui.Engine.State.activeUntil.All(t=>t==0),"Native prestige clears run products, upgrades and skills");
                    Check(ui.Engine.State.UniqueCount==before.UniqueCount&&ui.progression.shop.DisplayedStage==stage&&ui.Engine.State.learnedTopics.Contains("buy-first"),"Native prestige retains discoveries, decorations and learned guidance");
                    Check(ui.Engine.State.fame.CompareTo(before.fame+quote.Fame)==0&&audio.LastCue=="prestige","Native prestige applies quoted Fame once and emits one cue");
                    Check(ui.Engine.State.cash.CompareTo(ui.Engine.LevelQuote(0,1).cost)==0,"Native rebuild cash buys exactly the first product");
                    await Capture("Run"+(run+1)+"-Rebuild");
                    if(run==0)
                    {
                        ui.SetTab("Legacy");ui.Handle("Draw","1");var levels=ui.Engine.State.cardLevels.Select(n=>n.digits).ToArray();var credits=ui.Engine.State.freeCredits;
                        Check(ui.Engine.State.reveal.pending&&!ui.revealContinue.interactable&&audio.LastCue=="reveal","Draw saves result and disables Continue during reveal");
                        ui.motion.StopEffects();await Task.Delay(250);
                        Check(ui.revealContinue.interactable&&levels.SequenceEqual(ui.Engine.State.cardLevels.Select(n=>n.digits))&&credits.CompareTo(ui.Engine.State.freeCredits)==0,"Settling animation changes no rewards or currency");int revealCues=audio.CueCount;ui.Handle("ContinueReveal","");Check(audio.CueCount==revealCues,"Final Continue closes without replaying the reveal cue");
                    }
                    else Check(ui.Engine.State.secondPrestigeGift&&ui.Engine.State.kajuKatli.Value==100,"Native second prestige grants 100 KK once");
                    await ReturnGuest();ui=App.ui;audio=ui.audioFeedback;
                    Check(UnityEngine.Object.FindObjectsByType<RedesignAudio>(FindObjectsSortMode.None).Length==1&&ui.GetComponentsInChildren<AudioSource>(true).Length==2,"Scene return has one audio owner and two saved sources");
                    Check(ui.Engine.State.completedPrestiges.Value==run+1&&ui.Engine.State.learnedTopics.Contains("buy-first"),"Earned prestige and learned guidance survive profile reload");
                    ui.SetTab("Products");ui.Handle("BuyLevels","0");
                }
                ui.Open("Settings");ui.Handle("Setting","music");ui.Handle("Setting","sound");ui.Handle("Setting","motion");await Task.Delay(180);
                Check(!audio.music.isPlaying&&audio.effects.mute&&ui.motion.reduced,"Persisted settings stop music, mute cues and reduce motion");
                int cues=audio.CueCount;audio.Feedback("purchase");Check(audio.CueCount==cues,"Sound off suppresses playback");
                await ReturnGuest();ui=App.ui;audio=ui.audioFeedback;
                Check(!ui.Engine.State.music&&!ui.Engine.State.sound&&ui.Engine.State.reducedMotion&&!audio.music.isPlaying,"All three preferences persist after scene reload");
                ui.SetTab("Legacy");ui.Handle("Draw","1");Check(ui.revealContinue.interactable&&ui.revealCard.localRotation==Quaternion.identity,"Reduced-motion reveal settles immediately");ui.Close();
                ui.SetTab("Home");ui.Handle("Setting","music");ui.Handle("Setting","sound");await Task.Delay(150);audio.SendMessage("OnApplicationPause",true);
                Check(!audio.music.isPlaying&&!audio.effects.isPlaying,"Background pauses music and stops effects");audio.SendMessage("OnApplicationPause",false);await Task.Delay(150);Check(audio.music.isPlaying,"Foreground resumes one music source");
                ui.Open("Settings");await Capture("Settings");ui.SetTab("Home");
                ui.driver.Command(e=>e.Save());ui.driver.Command(e=>e.ResolveOffline(e.State.now+120));await Task.Delay(200);
                Check(ui.CurrentDialog=="Offline"&&ui.guidance.CurrentTopic=="return-play","Settled offline receipt presents contextual return guidance");
                string receipt=ui.Engine.State.offline.id;ui.Handle("Tip","");await Capture("Return-guidance");ui.Handle("LearnTip","");await Task.Delay(250);ui.Handle("Offline","");await Task.Delay(250);
                Check(ui.Engine.State.learnedTopics.Contains("return-play")&&!ui.Engine.State.offline.pending&&ui.Engine.State.offline.id==receipt,"Return guidance acknowledgment and Continue retain the single settled receipt");
                ui.SetTab("Products");ui.Handle("Quantity","products");ui.Handle("Quantity","products");
                var hundred=ui.Engine.LevelQuote(0,100);ui.Engine.Advance(Amount.Ratio(hundred.cost,ui.Engine.NormalIncome())+.1);ui.Refresh();int ownedBefore=ui.Engine.State.products[0];ui.Handle("BuyLevels","0");
                Check(ui.Engine.State.products[0]==ownedBefore+100,"Native x100 quantity commits the displayed count");
                ui.Handle("Quantity","products");ui.Engine.Advance(1000);ui.Refresh();var max=ui.Engine.LevelQuote(0,int.MaxValue);ownedBefore=ui.Engine.State.products[0];ui.Handle("BuyLevels","0");
                Check(max.quantity>0&&ui.Engine.State.products[0]==ownedBefore+max.quantity,"Native Max quantity commits the exact affordable count");
                ui.SetTab("Player");var affordable=ui.nextUpgrades.Where(r=>r.root.activeSelf&&r.buy.interactable).ToArray();
                Check(affordable.All(r=>((Image)r.buy.targetGraphic).sprite==ui.blueButton),"Passive affordability refresh updates button appearance");
                // Read-only presentation matrix on an explicitly marked in-memory fixture, separate from earned journey state.
                var fixture=ui.Engine.State.Copy();fixture.developmentFixture=true;fixture.products=Enumerable.Repeat(1,8).ToArray();
                for(int tier=0;tier<4;tier++)
                {
                    fixture.upgrades.Clear();fixture.everPurchased.Clear();
                    if(tier>0)for(int p=0;p<8;p++){string id=ui.Engine.Data.AppearanceUpgrade(p,tier);fixture.upgrades.Add(id);fixture.everPurchased.Add(id);}
                    var engine=new RedesignEngine(ui.Engine.Data,fixture.Copy(),s=>throw new InvalidOperationException("Presentation matrix cannot save"));
                    Check(Enumerable.Range(0,8).All(p=>engine.ProductAppearance(p)==tier&&ui.progression.appearances.At(p,tier)),"All eight product mappings select appearance tier "+tier);
                    ui.SetTab("Home");ui.enabled=false;ui.toast.SetActive(false);
                    for(int p=0;p<8;p++){ui.shelfProducts[p].SetActive(true);ui.progression.shelves[p].sprite=tier==0?ui.productArt[p]:ui.progression.appearances.At(p,tier);}
                    ui.progression.summary.text="Product look "+(tier+1)+"/4 · art preview";
                    await Capture("Product-look-"+(tier+1)+"-matrix");ui.enabled=true;ui.Refresh();
                }
                fixture.upgrades.Clear();ui.SetTab("Home");
                for(int stage=0;stage<6;stage++)
                {
                    fixture.everPurchased=ui.Engine.Data.Upgrades.Take(RedesignShopProgress.Threshold(stage)).Select(u=>u.id).ToList();fixture.decorationStage=stage;
                    var engine=new RedesignEngine(ui.Engine.Data,fixture.Copy(),s=>throw new InvalidOperationException("Presentation matrix cannot save"));
                    ui.progression.shop.Refresh(engine);
                    Check(ui.progression.shop.layers.Select((l,i)=>l.activeSelf==(i<stage)).All(x=>x),"Saved decoration layers match stage "+stage);
                    if(stage==5){ui.enabled=false;ui.progression.counterSlogan.SetActive(false);ui.progression.summary.text="Shop 6/6 · Raja crest";await Capture("Decoration-stage6-matrix");ui.enabled=true;}
                }
                ui.Refresh();
                var finalIds=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetType().FullName).OrderBy(s=>s).ToArray();
                Check(finalIds.Length==identities.Length&&ui.motion.particles.Length==64,"Saved component count and fixed 64-particle pool remain constant across scene cycles");
                Check(errors.Count==0,"No runtime errors during native unboosted journey and presentation checks");
                report.Add("Temporary earned profile: "+root+". Clock advanced by test only; no cash, ownership or reward fields seeded.");
            }
            catch(Exception ex){report.Add("FAIL "+ex);report.AddRange(errors);}
            finally{Application.logMessageReceived-=Log;File.WriteAllLines(Folder+"/Validation.txt",report);EditorApplication.ExitPlaymode();}
        }
    }
}
