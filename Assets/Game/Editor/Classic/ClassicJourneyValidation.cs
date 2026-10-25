using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Editor
{
    /// <summary>Unboosted command replay. Advances the engine clock; never seeds cash, ownership or rewards.</summary>
    public static class ClassicJourneyValidation
    {
        public static void Step(RedesignEngine e, Action<int,int> buyLevels, Action buyAll)
        {
            if(e.QuoteBuyAll().Ids.Count>0){buyAll();return;}
            var upgrade=e.Data.Upgrades.Where(u=>e.Eligible(u.id)).OrderBy(u=>e.UpgradeCost(u.id)).FirstOrDefault();
            Amount price=upgrade==null?new Amount(1,300):e.UpgradeCost(upgrade.id);int product=-2,quantity=1;
            for(int p=0;p<8;p++)
            {
                if(!e.ProductAvailable(p))continue;
                int q=e.State.products[p]==0?1:10;var quote=e.LevelQuote(p,q);
                if(quote.quantity>0&&quote.cost<price){price=quote.cost;product=p;quantity=q;}
            }
            if(price>e.State.cash)
            {
                double seconds=Amount.Ratio(price-e.State.cash,e.NormalIncome())*1.0000001+.001;
                if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds>1e9)throw new InvalidOperationException("Journey stalled waiting for "+price);
                e.Advance(seconds);
            }
            if(product>=0)buyLevels(product,quantity);else buyAll();
        }
        public static string Run()
        {
            var log=new List<string>();void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);log.Add("PASS "+label);}
            try
            {
                RedesignAuthoring.Validate();var data=RedesignRuntimeContent.Load();
                Check(data.Upgrades.Count==1000&&data.Products.Count==8&&data.Cards.Count==28,"Exact Resources content validated against authored design and manifest: 1,000 upgrades / eight products / 28 cards");
                string root=Path.Combine(Path.GetTempPath(),"ClassicJourney-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
                var store=new RedesignSaveStore(Path.Combine(root,"rocket-raja-v2.save"),data,JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>);
                var e=new RedesignEngine(data,RedesignState.Fresh(1000),store.Save);
                Check(e.State.cash.CompareTo(Amount.Zero)==0&&e.State.products.All(n=>n==0)&&e.State.cardLevels.All(n=>n.IsZero),"Fresh unboosted starting state");
                for(int i=0;i<30;i++)Check(e.Tap(),"Earn opening cash with a manual tap");
                Check(!e.ProductAvailable(1)&&!e.BuyLevels(1,1)&&e.BuyLevels(0,1)&&e.ProductAvailable(1),"Manual first purchase and sequential product unlock");
                Check(e.State.cash.CompareTo(Amount.Zero)==0&&e.State.eligibleEarnings.CompareTo(Amount.Zero)==0,"Opening grant is spent, never counted as earnings");
                Check(e.LearnTopic("buy-first")&&e.ToggleSetting("motion")&&e.SetStoreName("Journey stall"),"Guidance, accessibility and name saved through commands");
                for(int run=0;run<2;run++)
                {
                    int steps=0;double began=e.State.now;
                    while(!e.PreviewPrestige().Eligible||e.State.products.Any(n=>n==0)||Enumerable.Range(0,3).Any(i=>!e.SkillOwned(i)))
                    {
                        if(++steps>6000)throw new InvalidOperationException("Journey exceeded action limit");
                        Step(e,(p,q)=>{if(!e.BuyLevels(p,q))throw new InvalidOperationException("Quoted level rejected");},()=>{var b=e.QuoteBuyAll();if(b.Ids.Count==0||!e.CommitBuyAll(b))throw new InvalidOperationException("Quoted batch rejected");});
                    }
                    Check(e.State.products.All(n=>n>0)&&Enumerable.Range(0,3).All(e.SkillOwned),"Run "+(run+1)+" unlocks all products and repurchases all base skills");
                    for(int i=0;i<3;i++)Check(e.ActivateSkill(i),"Run "+(run+1)+" activates skill "+i);
                    int unique=e.State.UniqueCount;var before=e.State.Copy();var quote=e.PreviewPrestige();
                    log.Add("Run "+(run+1)+": "+steps+" decisions, "+(e.State.now-began).ToString("F2")+" simulated seconds, "+unique+" lifetime discoveries, +"+quote.Fame+" Fame, +"+quote.Credits+" credits");
                    Check(e.Prestige(quote),"Reviewed prestige commits run "+(run+1));
                    Check(e.State.products.All(n=>n==0)&&e.State.upgrades.Count==0&&e.State.tapLevel==0&&e.State.activeUntil.All(n=>n==0)&&e.State.readyAt.All(n=>n==0)&&e.State.eligibleEarnings.CompareTo(Amount.Zero)==0,"Prestige clears run fields and skill timers");
                    Check(e.State.UniqueCount==unique&&e.State.learnedTopics.Contains("buy-first")&&e.State.reducedMotion&&e.State.storeName=="Journey stall"&&e.State.cardLevels.SequenceEqual(before.cardLevels)&&e.State.decorationStage==before.decorationStage,"Prestige retains lifetime discoveries, learned topics, preferences, cards and decorations");
                    Check(e.State.fame.CompareTo(before.fame+quote.Fame)==0&&e.State.freeCredits.CompareTo(before.freeCredits+quote.Credits)==0,"Fame and credits equal authoritative reviewed rewards");
                    Check(e.State.cash.CompareTo(e.LevelQuote(0,1).cost)==0&&e.State.products.All(n=>n==0),"Next opening cash exactly funds one manual Phuljhari");
                    if(run==0)
                    {
                        Check(e.State.freeCredits.Value==3&&e.State.kajuKatli.IsZero,"First prestige grants three free credits and no KK");
                        var draw=e.QuoteDraw(true);Check(e.Draw(draw)&&e.State.reveal.pending&&e.DismissReveal(draw.OperationId),"Earned free draw persists before dismissal");
                    }
                    else Check(e.State.secondPrestigeGift&&e.State.kajuKatli.Value==100,"Second prestige grants the one-time 100 KK gift");
                    Check(e.Save(),"Durable retained checkpoint");var loaded=store.Load();Check(loaded.state!=null,"Returning progress loads from normal save codec");
                    e=new RedesignEngine(data,loaded.state,store.Save);
                    Check(e.BuyLevels(0,1)&&e.ProductAppearance(0)==0&&e.State.UniqueCount==unique,"Rebuild starts with basic art and retains discovery count");
                    var first=data.AppearanceUpgrade(0,1);int oldUnique=e.State.UniqueCount;
                    e.Advance(Amount.Ratio(e.UpgradeCost(first),e.NormalIncome())+.01);
                    Check(e.BuyUpgrade(first)&&e.State.UniqueCount==oldUnique&&e.ProductAppearance(0)==1,"Signature repurchase changes run art without counting a new discovery");
                }
                File.WriteAllText(Path.Combine(root,"earned-after-two-prestiges.json"),JsonUtility.ToJson(e.State,true));
                log.Add("Earned checkpoint: "+root);
                foreach(string file in new[]{"Assets/Resources/GameContent.asset","Assets/Resources/RedesignContentManifest.json"})
                    using(var sha=SHA256.Create())log.Add(file+" SHA256="+BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file))).Replace("-",""));
                log.Add("No development grants, direct cash changes, ownership seeds, or real player files. Simulated time is not human-play balance evidence.");
                return "Classic journey passed: "+log.Count+" entries";
            }
            catch(Exception ex){log.Add("FAIL "+ex);throw;}
            finally{File.WriteAllLines("Documentation/Implementation/Classic-Journey-Validation.txt",log);}
        }
    }
}
