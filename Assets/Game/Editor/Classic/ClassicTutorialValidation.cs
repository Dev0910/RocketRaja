using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class ClassicTutorialValidation
    {
        public static string RunCore()
        {
            var checks=new List<string>();
            void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("FAIL "+label);checks.Add("PASS "+label);}
            string root=Path.Combine(Path.GetTempPath(),"ClassicTutorial-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
            var data=RedesignRuntimeContent.Load();
            var store=new RedesignSaveStore(Path.Combine(root,"rocket-raja-v2.save"),data,JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>);
            bool fail=false, accelerated=false;string memory=null;
            void Save(RedesignState s){
                if(fail)throw new IOException("Injected disposable save failure");
                if(accelerated){memory=JsonUtility.ToJson(s);return;}
                for(int attempt=0;;attempt++)try{store.Save(s);break;}catch(IOException){if(attempt>=3)throw;System.Threading.Thread.Sleep(150);}
            }
            var e=new RedesignEngine(data,RedesignState.Fresh(1000),Save);
            void Dismiss(){if(e.State.tutorial.note!=TutorialNote.None)Check(e.DismissTutorialNote(e.State.tutorial.serial),"Dismiss current note without economic action");}
            Check(e.TutorialActive && e.State.tutorial.note==TutorialNote.Welcome,"Fresh save explicitly enrolled");
            var old=e.State.Copy();old.tutorial=null;
            Check(!new RedesignEngine(data,JsonUtility.FromJson<RedesignState>(JsonUtility.ToJson(old)),Save).TutorialActive,"Existing save with absent tutorial stays unenrolled");
            Check(e.Save(),"Initial tutorial persisted");
            var loaded=store.Load().state;Check(loaded.tutorial.Active,"Save codec retains enrollment");
            Check(e.PresentTutorial(1)&&!e.PresentTutorial(1),"Introduction presentation receipt is idempotent");
            Dismiss();Check(e.State.cash.CompareTo(Amount.Zero)==0 && e.State.tapLevel==0,"Dismissal never buys or grants");
            for(int i=0;i<9;i++)Check(e.Tap(),"Earn training cash");
            Check(e.NextTutorialOffer()==TutorialNote.None,"Below training price does not offer");
            Check(e.Tap()&&e.NextTutorialOffer()==TutorialNote.Training,"Actual training affordability offers at 10");
            fail=true;try{e.OfferTutorial(TutorialNote.Training);throw new Exception("Expected IOException");}catch(IOException){}
            Check(!e.State.tutorial.Has(TutorialProgress.TrainingOffered),"Failed save cannot publish tutorial progress");fail=false;
            Check(e.OfferTutorial(TutorialNote.Training)&&e.PresentTutorial(e.State.tutorial.serial),"Training introduction saved once");
            int serial=e.State.tutorial.serial;e=new RedesignEngine(data,store.Load().state,Save);
            Check(e.State.tutorial.presented==serial&&!e.PresentTutorial(serial),"Reload never replays automatic navigation");Dismiss();
            Check(e.NextTutorialOffer()==TutorialNote.None,"Dismissed introduction does not repeat while waiting for purchase");
            Amount oldTap=e.TapPayout();Check(e.BuyLevels(-1,1),"Manual training commits");
            Check(e.State.tutorial.note==TutorialNote.TrainingResult&&e.State.tutorial.before.CompareTo(oldTap)==0&&e.State.tutorial.after.CompareTo(e.TapPayout())==0,"Actual before/after training result atomically saved");
            Check(!e.CanContinueTutorialEarning && e.NextTutorialOffer()==TutorialNote.None,"Training result waits for a new tap or affordable stock");
            var readyTraining=new RedesignEngine(data,e.State.Copy(),s=>{});
            while(!readyTraining.LevelQuote(0,1).affordable)readyTraining.Tap();
            Check(readyTraining.State.tutorial.note==TutorialNote.TrainingResult&&readyTraining.OfferTutorial(TutorialNote.Product),"Affordable stock replaces training result without dismissal");
            e=new RedesignEngine(data,store.Load().state,Save);
            Check(!e.CanContinueTutorialEarning,"Historical taps cannot dismiss a reloaded result");
            Check(e.Tap()&&e.CanContinueTutorialEarning&&e.BeginTutorialEarning()&&e.State.tutorial.note==TutorialNote.None,"New earning tap continues past training result");
            while(!e.LevelQuote(0,1).affordable)Check(e.Tap(),"Save for first stock");
            Check(e.NextTutorialOffer()==TutorialNote.Product&&e.OfferTutorial(TutorialNote.Product),"First product readiness uses spendable cash");Dismiss();
            Check(e.BuyLevels(0,1)&&e.State.tutorial.note==TutorialNote.ProductResult,"Manual first product result");
            var tappingProduct=new RedesignEngine(data,e.State.Copy(),s=>{});
            Check(tappingProduct.Tap()&&tappingProduct.BeginTutorialEarning()&&tappingProduct.State.tutorial.note==TutorialNote.None,"New earning tap continues past product result");
            Check(!e.CanContinueTutorialEarning && e.NextTutorialOffer()==TutorialNote.None,"Product result waits while upgrade is unaffordable");
            e.Advance(100);Check(e.NextTutorialOffer()==TutorialNote.Upgrade,"Passive earnings continue directly from product result to affordable upgrade");
            Check(e.NextTutorialOffer()==TutorialNote.Upgrade&&e.OfferTutorial(TutorialNote.Upgrade),"Passive cash triggers first useful upgrade");Dismiss();
            var projection=e.PreviewUpgrade(RedesignEngine.TutorialUpgrade);
            fail=true;try{e.BuyUpgrade(RedesignEngine.TutorialUpgrade);throw new Exception("Expected IOException");}catch(IOException){}
            Check(!e.State.tutorial.Has(TutorialProgress.UpgradeDone)&&!e.State.upgrades.Contains(RedesignEngine.TutorialUpgrade),"Failed upgrade save publishes neither ownership nor tutorial result");fail=false;
            Check(e.BuyUpgrade(RedesignEngine.TutorialUpgrade)&&e.State.tutorial.before.CompareTo(projection.normalBefore)==0&&e.State.tutorial.after.CompareTo(projection.normalAfter)==0,"Upgrade result matches authoritative projection");
            Check(!e.CanContinueTutorialEarning,"Upgrade result does not consume historical taps");
            Check(e.Tap()&&e.BeginTutorialEarning()&&e.State.tutorial.note==TutorialNote.None,"New earning tap continues past upgrade result");
            for(int i=0;RedesignAchievements.ReadyCount(e.State)==0&&i<600;i++)Check(e.Tap(),"Earn a real achievement");
            Check(e.NextTutorialOffer()==TutorialNote.Achievement&&e.OfferTutorial(TutorialNote.Achievement),"Real first achievement readiness, independent of prestige");Dismiss();
            string id=RedesignAchievements.All.First(a=>RedesignAchievements.StageReady(e.State,a)).Id;var reward=RedesignAchievements.All.First(a=>a.Id==id).Reward;var kk=e.State.kajuKatli;
            Check(e.ClaimAchievement(id)&&e.State.kajuKatli.Value==kk.Value+reward&&e.State.tutorial.note==TutorialNote.AchievementResult,"Existing achievement command grants once with tutorial receipt");
            Check(!e.ClaimAchievement(id),"Duplicate claim cannot grant again");Dismiss();
            accelerated=true;int steps=0;while(!e.PreviewPrestige().Eligible){if(++steps>6000)throw new Exception("Journey stalled");ClassicJourneyValidation.Step(e,(p,q)=>e.BuyLevels(p,q),()=>e.CommitBuyAll(e.QuoteBuyAll()));}
            accelerated=false;e=new RedesignEngine(data,JsonUtility.FromJson<RedesignState>(memory),Save);Check(e.Save(),"Accelerated earned journey checkpoint saved through normal disk codec");
            Check(e.NextTutorialOffer()==TutorialNote.Prestige&&e.OfferTutorial(TutorialNote.Prestige),"Earned first prestige offers review only");
            var cash=e.State.cash;Dismiss();Check(e.State.completedPrestiges.IsZero&&e.State.cash.CompareTo(cash)==0,"Deferring prestige changes no run state");
            Check(e.Prestige(e.PreviewPrestige())&&e.State.tutorial.Has(TutorialProgress.UpgradeDone)&&e.State.freeCredits.Value==3,"Explicit prestige preserves tutorial and grants authoritative credits");
            Check(e.NextTutorialOffer()==TutorialNote.Talent&&e.OfferTutorial(TutorialNote.Talent),"Free Talent introduction after completed prestige");Dismiss();
            var credits=e.State.freeCredits;Check(e.Draw(e.QuoteDraw(true)),"Manual free draw commits once");
            var result=e.State.reveal.Copy();Check(e.State.tutorial.note==TutorialNote.None&&e.State.freeCredits.Value==credits.Value-1,"Committed reveal has priority and one credit charge");
            e=new RedesignEngine(data,store.Load().state,Save);Check(e.State.reveal.pending,"Pending card result survives reload");
            Check(e.DismissReveal(result.operationId)&&e.State.tutorial.note==TutorialNote.CardBenefit,"Only acknowledged reveal introduces card benefit");
            Check(e.State.tutorial.cardId==result.cardId&&e.State.tutorial.cardBefore.CompareTo(result.before)==0&&e.State.tutorial.cardAfter.CompareTo(result.after)==0,"Actual random card and duplicate levels retained");
            var cards=e.State.cardLevels.ToArray();Dismiss();Check(!e.TutorialActive&&e.State.freeCredits.Value==credits.Value-1&&e.State.cardLevels.SequenceEqual(cards),"Card explanation completes without a second charge or grant");
            cash=e.State.cash;Check(e.ReplayTutorial()&&e.State.cash.CompareTo(cash)==0&&e.State.tutorial.Has(TutorialProgress.UpgradeDone),"Explicit replay respects existing earned milestones");
            Check(e.State.tutorial.note==TutorialNote.Welcome&&!e.BeginTutorialEarning(),"Replay welcome is not dismissed by historical taps");
            Check(e.Tap()&&e.BeginTutorialEarning(),"Only a new counter tap collapses replay welcome");
            Check(e.SkipTutorial()&&!e.TutorialActive&&e.NextTutorialOffer()==TutorialNote.None,"Skip persists without an automatic restart");
            e=new RedesignEngine(data,store.Load().state,Save);Check(!e.TutorialActive,"Skipped course remains skipped after reload");
            File.WriteAllLines(Path.Combine(root,"core-checks.txt"),checks);
            return "Tutorial core passed "+checks.Count+" checks; real actions and disposable save: "+root;
        }
    }
}
