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
    public static class ClassicBazaarFeedbackValidation
    {
        const string Key="RocketRaja.BazaarFeedbackQA.",Profile="RocketRaja.LocalOnlyQA.Profile";
        const string Folder="Documentation/Implementation/UiUnification/native/feedback";
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static ClassicBazaarFeedbackValidation(){EditorApplication.playModeStateChanged+=Mode;}
        public static string Result=>SessionState.GetString(Key+"Result","");
        public static string Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Idle Edit Mode required");
            foreach(var setup in EditorSceneManager.GetSceneManagerSetup())
                if(setup.isLoaded&&SceneManager.GetSceneByPath(setup.path).isDirty)throw new InvalidOperationException("Save current scene changes before validation");
            Directory.CreateDirectory(Folder);
            SessionState.SetString(Key+"OldProfile",SessionState.GetString(Profile,""));
            SessionState.SetString(Profile,Path.Combine(Path.GetTempPath(),"FestivalFeelQA-"+Guid.NewGuid().ToString("N")));
            SessionState.SetBool(Key+"Running",true);SessionState.SetString(Key+"Result","RUNNING");
            EditorSceneManager.playModeStartScene=null;EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
            ClassicImportQA.Viewport(390,844);EditorApplication.EnterPlaymode();return Result;
        }
        static void Mode(PlayModeStateChange mode)
        {
            if(!SessionState.GetBool(Key+"Running",false))return;
            if(mode==PlayModeStateChange.EnteredPlayMode)Run();
            if(mode==PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetString(Profile,SessionState.GetString(Key+"OldProfile",""));SessionState.SetBool(Key+"Running",false);
                EditorApplication.delayCall+=()=>{EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");EditorSceneManager.OpenScene("Assets/Scenes/Main.unity",OpenSceneMode.Additive);};
            }
        }
        static void Check(bool ok,string text){if(!ok)throw new Exception("FAIL "+text);checks.Add("PASS "+text);File.WriteAllLines(Folder+"/checks.txt",checks);}
        static async Task Until(Func<bool> condition){double end=EditorApplication.timeSinceStartup+30;while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException("Production startup did not settle");await Task.Delay(50);}}
        static async Task Capture(string name){await Task.Delay(150);ClassicImportQA.Capture(Folder+"/"+name+".png");}
        static void Log(string text,string trace,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)errors.Add(text);}
        static void Tap(ClassicUI ui)
        {
            var p=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=-1,position=RectTransformUtility.WorldToScreenPoint(null,ui.motion.tapHint.position)};
            ui.tapTarget.OnPointerDown(p);ui.tapTarget.OnPointerUp(p);
        }
        static async void Run()
        {
            checks.Clear();errors.Clear();Application.logMessageReceived+=Log;
            try
            {
                await Until(()=>UnityEngine.Object.FindFirstObjectByType<ClassicApplication>()&&UnityEngine.Object.FindFirstObjectByType<ClassicApplication>().Ready);
                var ui=UnityEngine.Object.FindFirstObjectByType<ClassicApplication>().ui;var f=ui.festivalFeel;var audio=ui.audioFeedback;
                Check(ui.driver.GuestProfileRoot==SessionState.GetString(Profile,"")&&ui.driver.GuestProfileRoot.StartsWith(Path.GetTempPath(),StringComparison.OrdinalIgnoreCase),"Validation uses only the disposable guest profile");
                Check(f&&f.rings.Length==6&&f.payouts.Length==4&&f.tokens.Length==3&&f.fireworkSparks.Length==20&&ui.motion.particles.Length==64,"Saved fixed pools and original 64-particle pool are present");
                Check(f.overlay.GetComponentsInChildren<Graphic>(true).Concat(f.scene.GetComponentsInChildren<Graphic>(true)).All(g=>!g.raycastTarget),"New feedback graphics cannot intercept input");
                Check(ui.motion.effectRoot.GetComponent<Canvas>().overrideSorting&&ui.motion.effectRoot.GetComponent<Canvas>().sortingOrder>ui.GetComponent<Canvas>().sortingOrder,"Existing sparks render above dialogs");
                ui.tutorial.enabled=false;Check(ui.driver.Command(e=>e.SkipTutorial()),"Temporary tutorial can be skipped without changing the production profile");
                ui.SetTab("Home");await Task.Delay(300);
                var identities=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).OrderBy(n=>n).ToArray();
                int cues=audio.CueCount;Tap(ui);await Task.Delay(100);
                Check(ui.Engine.State.manualTaps.Value==1&&audio.CueCount==cues+1,"One tap gives one existing reward and one sound cue");
                Check(f.payouts.Any(t=>t.gameObject.activeInHierarchy&&t.text.StartsWith("+₹"))&&ui.motion.particles.Count(p=>p.gameObject.activeSelf&&p.sprite==ui.motion.coinSprite)==3,"Native payout label and exactly three coins are visible");await Capture("tap-390x844");
                for(int i=1;i<30;i++)Tap(ui);
                Check(ui.Engine.State.manualTaps.Value==30,"Rapid effects do not drop or duplicate gameplay taps");
                await Task.Delay(1100);Check(f.payouts.All(t=>!t.gameObject.activeSelf)&&f.rings.All(t=>!t.gameObject.activeSelf)&&ui.motion.particles.All(p=>!p.gameObject.activeSelf),"Tap pools recycle completely");
                ui.SetTab("Products");ui.Handle("BuyLevels","0");
                Check(ui.Engine.State.products[0]==1&&f.StockCount==1&&f.StockAnimating,"Actual first purchase pops the Editor-authored shelf product");await Capture("stock-390x844");await Task.Delay(800);
                Check(!f.StockAnimating,"Stock pose settles");
                Check(audio.music.clip.name.Contains("reviewed")&&Mathf.Abs(audio.music.clip.length-11.52f)<.02f,"Reviewed quiet 32-beat music loop is bound to the existing source");
                // Rich fixture is intentionally seeded only in this disposable profile. Purchases/claims/skills/prestige below still use normal commands.
                Check(ui.driver.Command(e=>{e.State.cash=1e12;e.State.eligibleEarnings=e.State.target*1.25;e.State.lifetimeEarnings=e.State.eligibleEarnings;e.State.products=new[]{9,1,1,1,1,1,1,1};for(int i=0;i<3;i++){string id=e.PreviewSkill(i).unlockId;if(!e.State.upgrades.Contains(id))e.State.upgrades.Add(id);if(!e.State.everPurchased.Contains(id))e.State.everPurchased.Add(id);}return e.Save();}),"Disposable advanced fixture is saved through the normal codec");
                ui.InvalidateQuotes();ui.Handle("Quantity","products");ui.SetTab("Products");ui.Handle("BuyLevels","0");
                Check(ui.Engine.State.products[0]==10&&f.MilestoneCount==1&&f.milestone.text.Contains("Level 10"),"Real level 9 to 10 purchase triggers its milestone once");await Capture("milestone-390x844");await Task.Delay(2300);
                Check(!f.milestoneGroup.gameObject.activeSelf,"Milestone receipt recycles");
                ui.SetTab("Home");ui.Handle("ActivateSkill","0");ui.Handle("ActivateSkill","1");ui.Handle("ActivateSkill","2");await Task.Delay(100);
                Check(f.warmLight.gameObject.activeSelf&&f.hands.All(h=>h.gameObject.activeSelf)&&f.parcel.gameObject.activeSelf,"Three successful skill activations show warmth, hands and a parcel");await Capture("skills-390x844");
                ui.Handle("ActivateSkill","2");Check(ui.Engine.State.readyAt[2]>ui.Engine.State.now,"Bulk Order remains guarded by its cooldown");await Task.Delay(1400);Check(!f.parcel.gameObject.activeSelf,"Parcel recycles");
                Check(ui.driver.Command(e=>{e.State.activeUntil[0]=e.State.activeUntil[1]=0;return e.Save();}),"Advanced fixture expires active intervals");await Task.Delay(100);
                Check(!f.warmLight.gameObject.activeSelf&&f.hands.All(h=>!h.gameObject.activeSelf),"Skill appearance ends with active time");
                ui.Open("Achievements");await Task.Delay(400);var kk=ui.Engine.State.kajuKatli;ui.Handle("ClaimAchievement","products-1");
                Check(ui.Engine.State.kajuKatli.Value==kk.Value+5&&f.RewardCount==1&&f.tokens.All(t=>t.gameObject.activeSelf),"Achievement grants exactly five KK and flies three saved tokens");await Capture("reward-390x844");
                ui.Handle("ClaimAchievement","products-1");Check(ui.Engine.State.kajuKatli.Value==kk.Value+5&&f.RewardCount==1,"Duplicate claim cannot grant or replay reward flight");await Task.Delay(1200);Check(f.tokens.All(t=>!t.gameObject.activeSelf),"Reward tokens recycle");
                ui.SetTab("Home");ui.Open("Settings");await Task.Delay(400);ui.Open("Naming");await Task.Delay(400);
                Check(ui.dialogs.Single(v=>v.route=="Settings").root.activeInHierarchy&&!ui.dialogs.Single(v=>v.route=="Settings").group.interactable,"Settings remains visible behind an animated child");ui.Close();await Task.Delay(250);Check(ui.CurrentDialog=="Settings","Back reveals the retained Settings layer");
                ui.Handle("Setting","music");await Task.Delay(100);Check(!audio.music.isPlaying&&ui.Engine.State.sound,"Music can stop independently of sound");
                ui.Handle("Setting","music");ui.Handle("Setting","sound");await Task.Delay(100);cues=audio.CueCount;audio.Feedback("tap");Check(audio.music.isPlaying&&audio.CueCount==cues&&!audio.effects.isPlaying,"Sound mute stops effects without stopping music");ui.Handle("Setting","sound");
                ui.Handle("Setting","motion");await Task.Delay(100);ui.SetTab("Home");Tap(ui);await Task.Delay(100);
                Check(f.payouts.All(t=>!t.gameObject.activeSelf)&&f.rings.All(t=>!t.gameObject.activeSelf)&&ui.motion.particles.All(t=>!t.gameObject.activeSelf),"Reduced motion suppresses all travel, rings and particles");
                f.Levels(0,24,25);await Task.Delay(100);Check(f.milestoneGroup.gameObject.activeSelf&&f.milestoneGroup.alpha==1,"Reduced motion retains a static milestone receipt");ui.Handle("Setting","motion");
                foreach(var size in new[]{new Vector2Int(360,540),new Vector2Int(450,916)}){ClassicImportQA.Viewport(size.x,size.y);await Task.Delay(400);Tap(ui);await Capture("tap-"+size.x+"x"+size.y);Check(f.payouts.Any(t=>t.gameObject.activeInHierarchy),"Payout works at "+size);}
                ui.Open("Prestige");await Task.Delay(400);ui.Close();await Until(()=>ui.CurrentDialog=="");Check(ui.Engine.State.completedPrestiges.IsZero&&!f.prestigeVeil.gameObject.activeSelf,"Cancelling prestige does not start its scene transition");
                ui.Open("Prestige");await Task.Delay(400);Check(ui.CurrentDialog=="Prestige"&&ui.Engine.PreviewPrestige().Eligible,"Prestige review is open and economically eligible");ui.Handle("Prestige","");Check(ui.Engine.State.completedPrestiges.Value==1&&f.PrestigeCount==1&&f.prestigeVeil.gameObject.activeSelf,"Explicit prestige commits once before the non-blocking bloom; completed="+ui.Engine.State.completedPrestiges+" count="+f.PrestigeCount+" active="+f.prestigeVeil.gameObject.activeSelf+" reduced="+ui.motion.reduced+" ready="+ui.driver.Session.CanMutate);await Capture("prestige-450x916");await Task.Delay(1500);
                Check(!f.prestigeVeil.gameObject.activeSelf&&ui.Engine.State.completedPrestiges.Value==1,"Prestige veil recycles without another reset");
                ui.SetTab("Legacy");ui.Handle("Draw","1");await Until(()=>ui.revealContinue.interactable);
                Check(f.rings.Any(r=>r.gameObject.activeSelf)&&ui.Engine.State.reveal.pending,"Card reveal settles with its pooled rarity ring");
                string card=ui.Engine.State.reveal.cardId;ui.Engine.Data.TryCard(card,out int ci);var level=ui.Engine.State.cardLevels[ci];ui.Handle("ContinueReveal","");await Until(()=>ui.CurrentDialog=="");
                Check(!ui.Engine.State.reveal.pending&&ui.Engine.State.cardLevels[ci].CompareTo(level)==0,"Reveal feedback cannot grant a duplicate card");
                Check(identities.SequenceEqual(ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).OrderBy(n=>n)),"Runtime component identities stay unchanged: no UI objects or effect sources are created");
                Check(ui.GetComponentsInChildren<AudioSource>(true).Length==2,"Exactly two authored audio sources remain");
                Check(errors.Count==0,"No runtime errors: "+string.Join("; ",errors));
                SessionState.SetString(Key+"Result","PASS "+checks.Count+" native festival checks; disposable profile "+ui.driver.GuestProfileRoot);
            }
            catch(Exception e){SessionState.SetString(Key+"Result",e.ToString());Debug.LogException(e);}
            finally{checks.Add(Result);File.WriteAllLines(Folder+"/checks.txt",checks);Application.logMessageReceived-=Log;EditorApplication.isPlaying=false;}
        }
    }
}
