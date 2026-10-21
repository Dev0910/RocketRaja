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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicProductionValidation
    {
        const string Key="Classic.ProductionValidation", Report="Documentation/Implementation/Classic-Production-Validation.txt";
        static readonly List<string> checks=new List<string>();
        static readonly List<string> errors=new List<string>();
        static ClassicProductionValidation() { EditorApplication.playModeStateChanged+=Mode; }
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            if(EditorSceneManager.GetSceneManagerSetup().Any(s=>s.isLoaded&&SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Save scene edits first");
            SessionState.SetBool(Key,true);
            EditorSceneManager.playModeStartScene=null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange mode)
        {
            if(!SessionState.GetBool(Key,false))return;
            if(mode==PlayModeStateChange.EnteredPlayMode)Run();
            if(mode==PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Key);
                EditorSceneManager.OpenScene(ClassicProductionAuthoring.Bootstrap);
            }
        }
        static void Check(bool ok,string label){checks.Add((ok?"PASS ":"FAIL ")+label);if(!ok)throw new InvalidOperationException(label);}
        static void Log(string text,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        static async Task Until(Func<bool> condition)
        {
            double end=EditorApplication.timeSinceStartup+20;
            while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException("Scene/operation did not settle");await Task.Delay(50);}
            await Task.Delay(120);
        }
        static ClassicApplication App => UnityEngine.Object.FindFirstObjectByType<ClassicApplication>();
        static BootstrapController Bootstrap => UnityEngine.Object.FindFirstObjectByType<BootstrapController>();
        static void Choose(string field)
        {
            var button=(Button)typeof(BootstrapController).GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Bootstrap);
            Check(button.IsInteractable(),"Bootstrap choice enabled: "+button.GetComponentInChildren<TMPro.TMP_Text>().text);
            button.onClick.Invoke();
        }
        static void Click(ClassicUI ui,string action,string arg="")
        {
            var c=ui.GetComponentsInChildren<ClassicControl>(true).First(c=>c.action==action&&c.parameter==arg&&c.gameObject.activeInHierarchy);
            Check(c.GetComponent<Button>().IsInteractable(),"Saved Classic control enabled: "+action+":"+arg);
            c.GetComponent<Button>().onClick.Invoke();
        }
        static async Task Home()
        {
            // Windows scanners can briefly deny an atomic replacement after the deliberately
            // locked file is released. Exercise the visible Retry action, never bypass persistence.
            await Until(()=>App);
            for(int attempt=0;attempt<4&&!App.Ready;attempt++)
            {
                if(!(App.ui.driver.Session?.LastSaveException is IOException))break;
                await Task.Delay(300);App.Retry();
            }
            await Until(()=>App&&App.Ready);
            if(App.ui.CurrentDialog=="Offline"){App.ui.Handle("Offline","");await Task.Delay(250);}
            Check(App.ui.CurrentDialog==""&&App.ui.CurrentTab=="Home","Selected profile reaches Classic Home");
        }
        static async void Run()
        {
            checks.Clear();errors.Clear();Application.logMessageReceived+=Log;
            string root=Path.Combine(Path.GetTempPath(),"ClassicProduction-"+Guid.NewGuid().ToString("N"));
            string file=Path.Combine(root,"rocket-raja-v2.save");
            FileStream locked=null;
            try
            {
                var auth=new FixtureAuth();var cloud=new RedesignAccountPlayValidation.FixtureCloud();
                var coordinator=new RedesignAccountCoordinator(auth,cloud,RedesignRuntimeContent.Load(),root,"classic-device-a",JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>,JsonUtility.ToJson,JsonUtility.FromJson<AccountCheckpoint>,()=>Time.realtimeSinceStartupAsDouble,()=>RedesignRuntimeDriver.Utc);
                var host=RedesignSessionHost.Ensure();host.ConfigureFixture(coordinator,root);
                await SceneManager.LoadSceneAsync("Bootstrap");await Until(()=>Bootstrap);
                var popup=(GameObject)typeof(BootstrapController).GetField("popupPanel",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Bootstrap);
                var opener=(GameObject)typeof(BootstrapController).GetField("popupOpener",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Bootstrap);
                var bootstrapIdentity=Bootstrap.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).ToArray();
                Bootstrap.DismissPopup();
                Check(!popup.activeSelf&&opener.activeSelf&&host.Mode==""&&!File.Exists(file),"Outside Welcome dismissal does not select or create a save");
                Bootstrap.ReopenPopup();
                Check(popup.activeSelf&&!opener.activeSelf&&bootstrapIdentity.SequenceEqual(Bootstrap.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID())),"Welcome reopens using the same saved controls");
                string captures=SessionState.GetString("Classic.HTMLSync.CaptureDirectory","");
                if(!string.IsNullOrEmpty(captures))
                {
                    Directory.CreateDirectory(captures);
                    foreach(var size in new[]{new Vector2Int(360,540),new Vector2Int(390,844)}){ClassicImportQA.Viewport(size.x,size.y);await Task.Delay(500);ClassicImportQA.Capture(Path.Combine(captures,"BootstrapWelcome-"+size.x+"x"+size.y+".png"));}
                }
                Choose("secondaryButton");await Home();
                var ui=App.ui;var state=ui.Engine.State;
                Check(state.cash.CompareTo((Amount)30)==0&&state.products.All(n=>n==0)&&state.fame.IsZero&&state.freeCredits.IsZero&&state.kajuKatli.IsZero&&state.cardLevels.All(n=>n.IsZero),"Fresh profile has only the 30 cash manual first-purchase grant");
                Check(!ui.isolatedPreview&&!ui.GetComponent<ClassicPreviewSession>()&&UnityEngine.Object.FindObjectsByType<RedesignRuntimeDriver>(FindObjectsSortMode.None).Length==1&&PlayerSessionHost.Instance==null,"One live driver, no preview or legacy session");
                Check(ui.navigation.Length==4&&ui.cards.Length==28&&ui.nextUpgrades.Length==10&&ui.motion.particles.Length==64,"Approved navigation, Talent, next-ten and fixed effect pool retained");
                Check(!ui.dialogs.Any(v=>v.route=="UpgradeDetail"||v.route=="Collection"),"Removed upgrade-detail and collection popups stay absent");
                var identities=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).ToArray();
                ui.SetTab("Legacy");Check(ui.cards.All(c=>!c.button.interactable),"All unowned Talent cards disabled");
                ui.Handle("Card","0");Check(ui.CurrentDialog=="","Unowned card cannot open");
                ui.SetTab("Products");Click(ui,"BuyLevels","0");
                Check(ui.Engine.State.products[0]==1&&ui.CurrentTab=="Home","First Phuljhari purchase is manual and returns Home");
                ui.SetTab("Shop");Check(ui.GetComponentsInChildren<ClassicControl>(true).Where(c=>c.action=="Purchase").All(c=>!c.GetComponent<Button>().interactable),"Normal session cannot select simulated purchase outcomes");
                ui.Handle("PurchaseResult","delivered");Check(ui.Engine.State.kajuKatli.IsZero&&!ui.Engine.State.HasDevelopmentShop,"Direct simulated outcome cannot grant production currency");
                ui.SetTab("Home");
                locked=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read);
                Check(!ui.driver.Command(e=>e.SetStoreName("Save lock test")),"Real file lock blocks persistence");
                await Until(()=>ui.CurrentDialog=="SaveRecovery");
                var before=ui.Engine.State.manualTaps;ui.Handle("BuyLevels","0");
                Check(!App.Ready&&!ui.driver.TapGameplaySurface()&&ui.Engine.State.manualTaps.Value==before.Value,"Recovery blocks commands and tapping");
                Click(ui,"Retry");Check(!App.Ready,"Retry under a file lock stays blocked");
                locked.Dispose();locked=null;Click(ui,"Retry");await Home();
                Check(identities.SequenceEqual(ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID())),"Recovery retains every UI/component identity");
                ui.driver.SendMessage("OnApplicationPause",true);Check(ui.driver.Session.Suspended,"Background suspends the gameplay owner");
                ui.driver.SendMessage("OnApplicationPause",false);await Home();
                App.OpenAccount();await Until(()=>Bootstrap);Choose("secondaryButton");await Home();
                Check(App.ui.Engine.State.products[0]==1,"Guest survives account options and scene reload");

                // Corrupt this isolated test profile only, after the scene has durably departed.
                App.OpenAccount();
                if(App&&App.ui.driver.Session.SaveFailure!=null){await Home();App.OpenAccount();}
                await Until(()=>Bootstrap);
                byte[] original=File.ReadAllBytes(file);File.WriteAllText(file,"unreadable fixture");File.WriteAllText(file+".bak","unreadable backup fixture");
                Choose("secondaryButton");await Until(()=>App&&App.ui.CurrentDialog=="SaveRecovery");
                Check(!App.Ready&&App.ui.driver.Session==null&&File.ReadAllText(file)=="unreadable fixture","Unreadable existing progress reaches recovery without a fresh fallback or overwrite");
                App.ui.Handle("Retry","");Check(!App.Ready,"Corrupt-profile retry cannot create fresh progress");
                File.WriteAllBytes(file,original);App.ui.Handle("Retry","");await Home();
                Check(App.ui.Engine.State.products[0]==1,"Load retry restores the same selected profile");

                App.OpenAccount();await Until(()=>Bootstrap);Choose("primaryButton");
                await Until(()=>Bootstrap&&coordinator.Profile!=null);Choose("primaryButton");await Home();
                ui=App.ui;Check(ui.driver.IsAccount&&ui.Engine.SessionKey.StartsWith(coordinator.Account.SessionKey+":binding:",StringComparison.Ordinal),"Classic binds the Bootstrap-selected Google account engine");
                var old=ui.driver.Session;
                cloud.Online=false;coordinator.Account.Block("Fixture connection lost");await Until(()=>ui.CurrentDialog=="SaveRecovery");
                Click(ui,"Retry");await Task.Delay(150);Check(!App.Ready&&ReferenceEquals(old,ui.driver.Session),"Offline retry retains the blocked account binding");
                cloud.Online=true;cloud.Offset+=60;Click(ui,"Retry");await Home();
                Check(!ReferenceEquals(old,ui.driver.Session)&&!old.CanMutate,"Reconnect renews the selected account and fences stale commands");
                ui.driver.SendMessage("OnApplicationPause",true);cloud.Offset+=60;ui.driver.SendMessage("OnApplicationPause",false);
                await Until(()=>ui.CurrentDialog=="SaveRecovery");Check(!App.Ready,"Account resume waits for confirmed ownership");
                Click(ui,"Retry");await Home();
                App.OpenAccount();await Until(()=>Bootstrap);Choose("secondaryButton");await Home();
                Check(App.ui.driver.IsAccount,"Back to shop reconnects the existing account");
                App.OpenAccount();await Until(()=>Bootstrap);Choose("primaryButton");await Home();
                Check(!App.ui.driver.IsAccount&&auth.Current==null&&coordinator.Account==null&&App.ui.Engine.State.products[0]==1,"Sign-out releases account and restores retained guest progress");
                Check(App.ui.driver.Command(e=>e.SetStoreName("Keep this device game")),"Device game has a distinct durable replacement choice");
                App.OpenAccount();await Until(()=>Bootstrap);Choose("primaryButton");
                await Until(()=>Bootstrap&&coordinator.Remote!=null);
                Check(((TMPro.TMP_Text)typeof(BootstrapController).GetField("secondaryLabel",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Bootstrap)).text=="Keep device progress","Existing Google comparison exposes the device replacement choice");
                byte[] deviceBeforeReplacement=File.ReadAllBytes(file);
                Choose("secondaryButton");await Home();
                Check(App.ui.driver.IsAccount&&auth.Current!=null&&coordinator.Account.CanPlay&&App.ui.Engine.State.storeName=="Keep this device game"&&JsonUtility.FromJson<RedesignState>(cloud.Record.save.json).storeName=="Keep this device game","Keep device progress saves the chosen device game to Google and opens signed-in Classic gameplay");
                Check(deviceBeforeReplacement.SequenceEqual(File.ReadAllBytes(file)),"Native replacement leaves independent guest-save bytes intact");
                App.OpenAccount();await Until(()=>Bootstrap);Choose("primaryButton");await Home();
                ui=App.ui;
                Check(host.ProfileRoot==root&&root.StartsWith(Path.GetTempPath(),StringComparison.OrdinalIgnoreCase),"Advanced UI fixture is confined to this disposable Editor test profile");
                ui.driver.Command(e=>{
                    var s=e.State;s.cash=1e9;s.products=new[]{25,18,12,8,5,3,1,1};s.tapLevel=12;
                    s.completedPrestiges=1;s.fame=1;s.target=ProgressionNumbers.RunTarget(s.completedPrestiges);s.freeCredits=4;s.reducedMotion=true;
                    return e.Save();
                });
                ui.InvalidateQuotes();ui.SetTab("Products");
                Click(ui,"Quantity","products");
                Check(Math.Abs(ui.products[0].progress.fillAmount)<.001f,"Product milestone bar resets at level 25");
                Click(ui,"BuyLevels","0");Check(ui.Engine.State.products[0]==26&&Math.Abs(ui.products[0].progress.fillAmount-.04f)<.001f,"Direct product purchase updates the next milestone segment");
                ui.SetTab("Player");
                var next=ui.Engine.Data.Upgrades.Where(u=>ui.Engine.Eligible(u.id)).OrderBy(u=>ui.Engine.UpgradeCost(u.id)).ThenBy(u=>u.id,StringComparer.Ordinal).Take(10).Select(u=>u.id);
                Check(ui.nextUpgrades.Where(r=>r.root.activeSelf).Select(r=>r.control.parameter).SequenceEqual(next),"Player binds exactly the next ten eligible upgrades in price order");
                Click(ui,"Quantity","tap");int tap=ui.Engine.State.tapLevel;Click(ui,"BuyLevels","-1");Check(ui.Engine.State.tapLevel==tap+1&&ui.CurrentDialog=="","Salesmanship purchase stays inline");
                int upgrades=ui.Engine.State.upgrades.Count;Click(ui,"BuyAll");Check(ui.Engine.State.upgrades.Count>upgrades&&ui.CurrentDialog=="","Buy All commits directly without a detail popup");
                ui.SetTab("Legacy");
                Check(ui.cards.Take(4).Select((c,i)=>Math.Abs(((RectTransform)c.button.transform).anchorMin.x-i*.25f)<.001f).All(x=>x)&&((RectTransform)ui.cards[4].button.transform).anchorMin.x==0&&((RectTransform)ui.cards[4].button.transform).anchorMin.y<((RectTransform)ui.cards[0].button.transform).anchorMin.y,"Talent cards remain inline in four-column rows");
                var beforeDraw=ui.Engine.State.cardLevels.Sum(n=>(int)n.Value);Click(ui,"Draw","5");
                Check(ui.CurrentDialog=="DrawReveal"&&ui.Engine.State.freeCredits.IsZero&&ui.Engine.State.cardLevels.Sum(n=>(int)n.Value)==beforeDraw+5,"Five-card draw charges four credits and persists exactly five grants");
                for(int i=0;i<5;i++)Click(ui,"ContinueReveal");
                Check(!ui.Engine.State.reveal.pending&&ui.CurrentTab=="Legacy"&&ui.CurrentDialog==""&&ui.Engine.State.cardLevels.Sum(n=>(int)n.Value)==beforeDraw+5,"Continue advances saved results then returns to Legacy without extra grants");
                int owned=Array.FindIndex(ui.Engine.State.cardLevels,n=>!n.IsZero);Click(ui,"Card",owned.ToString());
                Check(ui.CurrentDialog=="CardDetail","Owned Talent opens its saved full card");Click(ui,"Close");await Task.Delay(200);
                Check(ui.CurrentDialog=="","Outside-card dismissal returns to the inline collection");
                App.OpenAccount();await Until(()=>Bootstrap);
                UnityEngine.Object.Destroy(host.gameObject);await Until(()=>!RedesignSessionHost.Instance);
                string legacyRoot=Path.Combine(Path.GetTempPath(),"ClassicLegacy-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(legacyRoot);
                string legacyFile=Path.Combine(legacyRoot,"diwali-v1.save");File.WriteAllText(legacyFile,"preserved original legacy progress");
                var legacyCoordinator=new RedesignAccountCoordinator(new FixtureAuth(),new RedesignAccountPlayValidation.FixtureCloud(),RedesignRuntimeContent.Load(),legacyRoot,"classic-legacy-device",JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>,JsonUtility.ToJson,JsonUtility.FromJson<AccountCheckpoint>,()=>Time.realtimeSinceStartupAsDouble,()=>RedesignRuntimeDriver.Utc);
                host=RedesignSessionHost.Ensure();host.ConfigureFixture(legacyCoordinator,legacyRoot);
                await SceneManager.LoadSceneAsync("Bootstrap");await Until(()=>Bootstrap);
                Check(host.InspectGuest().status==RedesignLoadStatus.Corrupt,"Unreadable legacy sibling stays in recovery under the current economy policy");
                Choose("secondaryButton");await Until(()=>App&&App.ui.CurrentDialog=="SaveRecovery");
                Check(!App.Ready&&App.ui.driver.Session==null&&File.ReadAllText(legacyFile)=="preserved original legacy progress"&&!File.Exists(Path.Combine(legacyRoot,"rocket-raja-v2.save")),"Unreadable legacy source remains unchanged without a fresh fallback");
                App.OpenAccount();await Until(()=>Bootstrap);UnityEngine.Object.Destroy(host.gameObject);await Until(()=>!RedesignSessionHost.Instance);
                string resetRoot=Path.Combine(Path.GetTempPath(),"ClassicKnownEconomy-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(resetRoot);
                string oldJson="{\"schema\":1,\"economy\":\"1.0.0\"}",oldBytes=SavePayload.Digest(oldJson)+"\n"+oldJson;
                string resetFile=Path.Combine(resetRoot,"diwali-v1.save");File.WriteAllText(resetFile,oldBytes);
                var resetCoordinator=new RedesignAccountCoordinator(new FixtureAuth(),new RedesignAccountPlayValidation.FixtureCloud(),RedesignRuntimeContent.Load(),resetRoot,"classic-reset-device",JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>,JsonUtility.ToJson,JsonUtility.FromJson<AccountCheckpoint>,()=>Time.realtimeSinceStartupAsDouble,()=>RedesignRuntimeDriver.Utc);
                host=RedesignSessionHost.Ensure();host.ConfigureFixture(resetCoordinator,resetRoot);
                await SceneManager.LoadSceneAsync("Bootstrap");await Until(()=>Bootstrap);
                Check(host.InspectGuest().CanCreateFresh,"Known older economy follows the existing faster-start reset policy");
                Choose("secondaryButton");await Home();
                Check(host.ProfileRoot==resetRoot&&App.ui.Engine.State.cash.CompareTo((Amount)30)==0&&App.ui.Engine.State.products.All(n=>n==0),"Explicit Play as guest starts the current normal profile");
                Check(File.ReadAllText(resetFile)==oldBytes&&File.Exists(Path.Combine(resetRoot,"rocket-raja-v2.save")),"Known legacy source is retained beside the new current-economy save");
                Check(errors.Count==0,"No runtime errors during connected startup/recovery/account flow");
            }
            catch(Exception ex){checks.Add("FAIL "+ex);}
            finally
            {
                locked?.Dispose();Application.logMessageReceived-=Log;
                checks.AddRange(errors.Select(e=>"ERROR "+e));
                checks.Add("Fixture profile: "+root+" (no player files or live Firebase used)");
                File.WriteAllLines(Report,checks);EditorApplication.ExitPlaymode();
            }
        }
        internal sealed class FixtureAuth:IAccountAuth
        {
            public AccountProfile Current{get;private set;}
            public event Action Changed;
            public Task InitializeAsync()=>Task.CompletedTask;
            public Task<AccountProfile> SignInAsync(){Current=new AccountProfile{uid="classic-flow-account",displayName="Classic fixture",email="",photoUrl=""};Changed?.Invoke();return Task.FromResult(Current);}
            public Task SignOutAsync(){Current=null;Changed?.Invoke();return Task.CompletedTask;}
            public void Dispose(){}
        }
    }
}
