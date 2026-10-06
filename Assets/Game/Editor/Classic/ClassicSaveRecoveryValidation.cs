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
    public static class ClassicSaveRecoveryValidation
    {
        const string Key="Classic.SaveRecoveryValidation", Folder="Documentation/Implementation/ClassicRecovery";
        static readonly List<string> report=new List<string>(), errors=new List<string>();
        static ClassicApplication App=>UnityEngine.Object.FindFirstObjectByType<ClassicApplication>();
        static BootstrapController Bootstrap=>UnityEngine.Object.FindFirstObjectByType<BootstrapController>();
        static ClassicSaveRecoveryValidation(){EditorApplication.playModeStateChanged+=Mode;}
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            if(EditorSceneManager.GetSceneManagerSetup().Any(s=>s.isLoaded&&SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Save scene edits first");
            Directory.CreateDirectory(Folder);SessionState.SetBool(Key,true);EditorSceneManager.playModeStartScene=null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange mode)
        {
            if(!SessionState.GetBool(Key,false))return;
            if(mode==PlayModeStateChange.EnteredPlayMode)Run();
            if(mode==PlayModeStateChange.EnteredEditMode){SessionState.EraseBool(Key);EditorSceneManager.OpenScene(ClassicProductionAuthoring.Bootstrap);}
        }
        static void Check(bool ok,string label){report.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(Folder+"/Native.txt",report);if(!ok)throw new InvalidOperationException(label);}
        static void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        static async Task Until(Func<bool> condition){double end=EditorApplication.timeSinceStartup+30;while(!condition()){if(EditorApplication.timeSinceStartup>end)throw new TimeoutException("Recovery flow did not settle");await Task.Delay(50);}await Task.Delay(180);}
        static void ChooseGuest()=>((Button)typeof(BootstrapController).GetField("secondaryButton",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Bootstrap)).onClick.Invoke();
        static async Task Menu(){App.OpenAccount();await Until(()=>Bootstrap);}
        static async Task Enter(){ChooseGuest();await Until(()=>App);}
        static async Task Ready()
        {
            for(int i=0;i<4&&App&&!App.Ready;i++){await Task.Delay(300);App.Retry();}
            await Until(()=>App&&App.Ready);
        }
        static async Task Capture(string name){await Task.Delay(300);ClassicImportQA.Capture(Folder+"/"+name+".png");}
        static async void Run()
        {
            report.Clear();errors.Clear();Application.logMessageReceived+=Log;FileStream locked=null;
            string root=Path.Combine(Path.GetTempPath(),"ClassicRecovery-"+Guid.NewGuid().ToString("N"));
            string file=Path.Combine(root,"rocket-raja-v2.save");
            try
            {
                var catalog=RedesignRuntimeContent.Load();
                var store=new RedesignSaveStore(file,catalog,JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>);
                var host=RedesignSessionHost.Ensure();var cloud=new RedesignAccountPlayValidation.FixtureCloud();
                var coordinator=new RedesignAccountCoordinator(new ClassicProductionValidation.FixtureAuth(),cloud,catalog,root,"classic-recovery-device",JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>,JsonUtility.ToJson,JsonUtility.FromJson<AccountCheckpoint>,()=>Time.realtimeSinceStartupAsDouble,()=>RedesignRuntimeDriver.Utc);
                host.ConfigureFixture(coordinator,root);await host.ReturnToGuestAsync(false);host.EnterMain();await Until(()=>App&&App.Ready);
                ClassicImportQA.Viewport(360,640);var ui=App.ui;
                ui.SetTab("Products");ui.Handle("BuyLevels","0");
                Check(ui.Engine.State.products[0]==1,"Fresh Classic manual purchase is persisted");
                ui.driver.Session.Tick(10.1);var saved=store.Load().state;
                Check(saved.products[0]==1&&saved.eligibleEarnings>Amount.Zero,"Classic autosave persists passive earnings");
                var identities=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID()).ToArray();
                locked=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read);
                var before=ui.Engine.State.Copy();
                Check(!ui.driver.Command(e=>e.SetStoreName("Must not commit")),"Real atomic-replace failure rejects a Classic command");
                await Until(()=>ui.CurrentDialog=="SaveRecovery");
                Check(!App.Ready&&ui.Engine.State.storeName==before.storeName&&!ui.driver.TapGameplaySurface(),"Save recovery rolls back command and blocks gameplay");
                await Capture("Save-Failure");App.Retry();await Task.Delay(100);Check(!App.Ready,"Retry while storage is locked stays blocked");
                locked.Dispose();locked=null;await Ready();
                Check(identities.SequenceEqual(ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetInstanceID())),"Storage retry preserves the saved UI hierarchy");

                bool diskFull=true;
                var fault=new RedesignLifecycle(new RedesignEngine(catalog,ui.Engine.State.Copy(),s=>{if(diskFull)throw new IOException("Synthetic ENOSPC for isolated Editor acceptance");store.Save(s);}));
                typeof(RedesignRuntimeDriver).GetProperty("Session").SetValue(ui.driver,fault);await Task.Delay(200);
                before=ui.Engine.State.Copy();ui.driver.Command(e=>e.SetStoreName("Disk full"));
                await Until(()=>ui.CurrentDialog=="SaveRecovery");
                Check(!App.Ready&&ui.recoveryMessage.text.Contains("storage")&&ui.Engine.State.storeName==before.storeName,"ENOSPC injection reaches Classic storage recovery without changing progress");
                await Capture("Low-Storage");diskFull=false;await Ready();
                Check(store.Load().state.storeName==before.storeName,"Storage retry commits the retained state before reopening Classic");

                ui.driver.Session.Suspend();double returnTime=ui.Engine.State.now+36000;
                Check(ui.driver.Session.Resume(returnTime),"Classic resumes after a ten-hour simulated absence");
                await Until(()=>ui.CurrentDialog=="Offline");
                string receipt=ui.Engine.State.offline.id;var settled=ui.Engine.State.cash;
                Check(ui.Engine.State.offline.creditedSeconds==28800&&store.Load().state.offline.id==receipt,"Eight-hour cap and receipt are durable before showing the return dialog");
                Check(ui.driver.Session.Resume(returnTime)&&!ui.Engine.ResolveOffline(returnTime-60)&&ui.Engine.State.offline.id==receipt,"Duplicate resume and backward clock cannot replay the displayed receipt");
                await Capture("Offline-Cap");ui.Handle("Offline","");
                Check(!store.Load().state.offline.pending&&ui.Engine.State.cash>=settled,"Continue acknowledges offline credit without charging or replaying it");

                // Earn the first prestige through normal production, using an accelerated clock only.
                ui.driver.Command(e=>{e.Advance(Amount.Ratio(e.State.target-e.State.eligibleEarnings,e.NormalIncome())+1);return e.Prestige(e.PreviewPrestige());});
                ui.SetTab("Legacy");ui.Handle("Draw","1");
                Check(ui.Engine.State.reveal.pending&&ui.Engine.State.freeCredits.Value==2,"Earned free draw persists debit and reward before reveal");
                string operation=ui.Engine.State.reveal.operationId, card=ui.Engine.State.reveal.cardId;
                var copies=ui.Engine.State.cardLevels.Select(n=>n.digits).ToArray();
                await Menu();await Enter();await Ready();ui=App.ui;
                await Until(()=>ui.CurrentDialog=="DrawReveal");
                Check(ui.Engine.State.reveal.operationId==operation&&ui.Engine.State.reveal.cardId==card&&ui.Engine.State.freeCredits.Value==2&&ui.Engine.State.cardLevels.Select(n=>n.digits).SequenceEqual(copies),"Reload restores the exact pending reveal without another charge or grant");
                await Capture("Restored-Reveal");
                await Until(()=>ui.revealContinue.interactable);ui.Handle("ContinueReveal","");await Task.Delay(350);
                Check(!ui.Engine.State.reveal.pending&&ui.Engine.State.cardLevels.Select(n=>n.digits).SequenceEqual(copies),"Continue dismisses the stored reveal without rewards");
                await Menu();await Enter();await Ready();ui=App.ui;
                Check(!ui.Engine.State.reveal.pending&&ui.Engine.State.freeCredits.Value==2,"Acknowledged reveal stays dismissed after another reload");

                await Menu();var bytes=File.ReadAllBytes(file);var backup=File.ReadAllBytes(file+".bak");
                var future=store.Load().state;future.schema=99;string json=JsonUtility.ToJson(future);File.WriteAllText(file,SavePayload.Digest(json)+"\n"+json);var futureBytes=File.ReadAllBytes(file);
                await Enter();await Until(()=>App.ui.CurrentDialog=="SaveRecovery");ui=App.ui;
                Check(!App.Ready&&ui.driver.Session==null&&!ui.driver.TapGameplaySurface(),"Future save reaches blocked Classic recovery, never a fresh game");
                App.Retry();await Task.Delay(200);
                Check(futureBytes.SequenceEqual(File.ReadAllBytes(file))&&backup.SequenceEqual(File.ReadAllBytes(file+".bak")),"Future-version retry preserves main and backup bytes");await Capture("Future-Version");
                File.WriteAllBytes(file,bytes);await Ready();
                Check(App.ui.Engine.State.cardLevels.Select(n=>n.digits).SequenceEqual(copies),"Compatible file restored externally retries the same selected profile");

                await Menu();File.WriteAllText(file,"damaged acceptance fixture");await Enter();await Ready();ui=App.ui;
                Check(ui.driver.LoadResult.status==RedesignLoadStatus.RecoveredBackup&&ui.Engine.State.requiresDrawReconciliation,"Classic backup recovery durably fences further card draws");
                ui.SetTab("Legacy");
                Check(ui.GetComponentsInChildren<ClassicControl>(true).Where(c=>c.action=="Draw").All(c=>!c.GetComponent<Button>().interactable),"Recovered backup cannot spend rolled-back credits or reroll cards");
                Check(ui.textBindings.Single(b=>b.key=="legacy-state").text.text.Contains("Backup recovered"),"Talent explains why backup recovery has paused drawing");
                Check(Directory.GetFiles(root,"*.damaged-*").Any(p=>File.ReadAllText(p)=="damaged acceptance fixture"),"Classic recovery preserves damaged source bytes");await Capture("Backup-Recovered");

                await Menu();bytes=File.ReadAllBytes(file);backup=File.ReadAllBytes(file+".bak");File.Delete(file);File.Delete(file+".bak");File.WriteAllText(file+".tmp","interrupted initial write");
                await Enter();await Until(()=>App.ui.CurrentDialog=="SaveRecovery");
                Check(!App.Ready&&App.ui.driver.Session==null&&File.ReadAllText(file+".tmp")=="interrupted initial write"&&!File.Exists(file),"Orphan first write stays in visible recovery with no fresh replacement");
                await Capture("Interrupted-First-Write");File.WriteAllBytes(file,bytes);File.WriteAllBytes(file+".bak",backup);await Ready();
                Check(App.Ready,"Restoring committed files enables normal retry after an interrupted first write");
                Check(errors.Count==0,"No runtime errors during Classic durability acceptance");
            }
            catch(Exception error){report.Add("FAIL "+error);}
            finally
            {
                locked?.Dispose();Application.logMessageReceived-=Log;report.AddRange(errors.Select(e=>"ERROR "+e));
                report.Add("Disposable Editor profile: "+root);report.Add("Low storage is an ENOSPC injection; no physical device disk was filled. No player files or live Firebase used.");
                File.WriteAllLines(Folder+"/Native.txt",report);EditorApplication.ExitPlaymode();
            }
        }
    }
}
