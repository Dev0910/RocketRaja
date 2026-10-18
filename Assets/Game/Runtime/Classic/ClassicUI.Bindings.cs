using System;
using System.Linq;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Classic
{
    public sealed partial class ClassicUI
    {
        public void Refresh()
        {
            if(Engine==null)return;var e=Engine;var s=e.State;bool can=driver.Session.CanMutate;
            motion.reduced=s.reducedMotion;
            if(progression)progression.Refresh();
            if(guidance)guidance.Refresh();
            Text("cash",Cash(s.cash));Text("shop-name",string.IsNullOrWhiteSpace(s.storeName)?"DIWALI DUKAAN":s.storeName);
            Text("home-income",Cash(s.activeUntil[0]>s.now?e.NormalIncome()*e.PreviewSkill(0).power:e.NormalIncome()));Text("home-tap",Cash(e.TapPayout()));
            Text("home-goal",s.products[0]==0?"Stock Phuljhari · "+Cash(e.LevelQuote(0,1).cost):"Phuljhari · discover your next signature");
            Text("product-total","Automatic sales · "+Cash(e.NormalIncome())+"/s");Text("quantity",quantity==int.MaxValue?"Buy Max":"Buy ×"+quantity);Text("tap-quantity",tapQuantity==int.MaxValue?"Buy Max":"Buy ×"+tapQuantity);
            var tapQuote=e.LevelQuote(-1,LevelQuantity(-1));
            Text("tap-row-title","Salesmanship  ·  Level "+s.tapLevel);Text("tap-row-income",Cash(e.TapPayout())+" per tap");Text("tap-row-cost",s.tapLevel>=Catalog.MaxLevel?"Max level":Cash(tapQuote.cost)+"\nBuy ×"+Math.Max(1,tapQuote.quantity));Enable("BuyLevels","-1",can&&tapQuote.affordable);
            Text("fame",Count(s.fame)+" Fame · products ×"+ProgressionNumbers.FameMultiplier(s.fame)+" · taps ×"+ProgressionNumbers.TapFameMultiplier(s.fame));
            for(int i=0;i<8;i++)
            {
                var row=products[i];var q=e.LevelQuote(i,LevelQuantity(i));int n=s.products[i];row.name.text=e.Data.Products[i].name;row.level.text="Lvl : "+n;row.income.text=Cash(e.ProductIncome(i))+"/s";
                row.cost.text=n>=Catalog.MaxLevel?"Max level":Cash(q.cost)+"\n"+(!e.ProductAvailable(i)?"Own "+e.Data.Products[i-1].name:n==0?"Stock shelf":"Upgrade ×"+Math.Max(1,q.quantity));row.buy.interactable=can&&q.affordable;
                ((UnityEngine.UI.Image)row.buy.targetGraphic).sprite=row.buy.interactable?blueButton:disabledButton;
                motion.Fill(row.progress,MilestoneProgress(n));shelfProducts[i].SetActive(n>0);
            }
            for(int i=0;i<3;i++)
            {
                string state=SkillState(i);Text("home-skill-"+i,state=="READY"?"READY":state=="LOCKED"?"VIEW UNLOCK":state=="ACTIVE"?"ACTIVE · "+TimeLabel(s.activeUntil[i]-s.now):"WAIT · "+TimeLabel(s.readyAt[i]-s.now));
                homeSkillImages[i].sprite=e.SkillOwned(i)?skillArt[i]:lockArt;motion.skillGlows[i].gameObject.SetActive(state=="READY"||state=="ACTIVE");motion.SkillState(i,state);Text("skills-state-"+i,state);
            }
            Text("legacy-kk",Count(s.kajuKatli)+" KK");Text("shop-kk",Count(s.kajuKatli)+" KK");
            int discovered=s.cardLevels.Count(n=>!n.IsZero);for(int i=0;i<3;i++)Text("discovered-"+i,discovered+" / 28 discovered");
            Text("draw-one","1× pull\n"+(!s.freeCredits.IsZero?"1 free pull":"100 KK"));Text("draw-five","5× pull\n"+(s.freeCredits.Value>=4?"4 free pulls":"400 KK"));
            Enable("Draw","1",can&&e.CanDraw(!s.freeCredits.IsZero));Enable("Draw","5",can&&e.CanDrawFive(s.freeCredits.Value>=4));
            Text("legacy-state",s.requiresDrawReconciliation?"Backup recovered · Draws need reward recovery.":s.completedPrestiges.IsZero?"Complete your first prestige to unlock Talent.":"");
            for(int i=0;i<cards.Length;i++){bool owned=!s.cardLevels[i].IsZero;cards[i].level.text="Lvl "+Count(s.cardLevels[i]);cards[i].button.interactable=owned;cards[i].group.alpha=owned?1:.48f;cards[i].art.color=Color.white;cards[i].art.material=cards[i].frame.material=owned?null:undiscoveredMaterial;}
            Text("shop-status",s.HasDevelopmentShop?(s.developmentShop.status=="pending"?"Purchase pending — return to its pack to check the outcome.":s.developmentShop.status=="failed"?"Purchase could not be verified. No KK was granted.":s.developmentShop.status=="cancelled"?"Purchase cancelled. Nothing was charged.":s.developmentShop.status=="delivered"?"Development purchase delivered.":""):"");
            if(!isolatedPreview)Text("shop-status","Store purchases are not available yet.");
            if(lastRevision!=s.commandRevision.digits)
            {
                lastRevision=s.commandRevision.digits;var eligible=e.Data.Upgrades.Where(u=>e.Eligible(u.id)).OrderBy(u=>e.UpgradeCost(u.id)).ThenBy(u=>u.id,StringComparer.Ordinal).Take(10).ToArray();
                BindUpgrades(nextUpgrades,eligible);Text("upgrade-count",s.upgrades.Count+" / "+e.Data.Upgrades.Count);
                Text("upgrade-empty",eligible.Length==0?"All upgrades purchased this run.":"");RefreshCatalogue();
            }
            foreach(var row in nextUpgrades.Concat(catalogueRows))if(row.root.activeSelf&&e.Data.TryUpgrade(row.control.parameter,out _))
            {
                row.buy.interactable=can&&e.Eligible(row.control.parameter)&&e.UpgradeCost(row.control.parameter)<=s.cash;
                ((UnityEngine.UI.Image)row.buy.targetGraphic).sprite=row.buy.interactable?blueButton:disabledButton;
            }
            Enable("BuyAll","",can&&e.Data.Upgrades.Any(u=>e.Eligible(u.id)&&e.UpgradeCost(u.id)<=s.cash));RefreshDialog();
        }
        static string TimeLabel(double value)=>TimeSpan.FromSeconds(Math.Max(0,value)).ToString(@"mm\:ss");
        string SkillState(int i)=>!Engine.SkillOwned(i)?"LOCKED":Engine.State.activeUntil[i]>Engine.State.now?"ACTIVE":Engine.State.readyAt[i]>Engine.State.now?"COOLDOWN":"READY";
        static int NextMilestone(int level)=>EconomyBalance.NextMilestone(level);
        static float MilestoneProgress(int level){if(level>=100000)return 1;int previous=EconomyBalance.PreviousMilestone(level);return (float)(level-previous)/(NextMilestone(level)-previous);}
        void BindUpgrades(ClassicUpgradeRow[] rows,CashUpgrade[] upgrades)
        {
            for(int i=0;i<rows.Length;i++)
            {
                var row=rows[i];row.root.SetActive(i<upgrades.Length);if(i>=upgrades.Length)continue;var u=upgrades[i];row.control.parameter=u.id;row.title.text=u.name;bool owned=Engine.State.upgrades.Contains(u.id);
                row.copy.text=RedesignUpgradeRow.Effect(u,Engine.Data)+"\n"+(owned?"Owned this run":Engine.Eligible(u.id)?"Available":RedesignUpgradeRow.Requirements(u,Engine.Data));row.cost.text=owned?"Purchased":Cash(Engine.UpgradeCost(u.id))+"\nBuy upgrade";
                row.buy.interactable=!owned&&driver.Session.CanMutate&&Engine.Eligible(u.id)&&Engine.UpgradeCost(u.id)<=Engine.State.cash;((UnityEngine.UI.Image)row.buy.targetGraphic).sprite=row.buy.interactable?blueButton:disabledButton;
            }
        }
        void RefreshCatalogue()
        {
            if(Engine==null)return;var list=Engine.Data.Upgrades.Where(u=>
                (category=="All"||UpgradeScope.MatchesCategory(u,(UpgradeCategory)Enum.Parse(typeof(UpgradeCategory),category)))&&
                (string.IsNullOrEmpty(search)||(u.id+" "+u.name+" "+u.effectText).IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0)&&
                (view=="Full"||view=="Owned"&&Engine.State.upgrades.Contains(u.id)||view=="Available"&&Engine.Eligible(u.id)||view=="Upcoming"&&!Engine.State.upgrades.Contains(u.id)&&!Engine.Eligible(u.id))).ToArray();
            int pages=Math.Max(1,(list.Length+19)/20);page=Math.Min(page,pages-1);BindUpgrades(catalogueRows,list.Skip(page*20).Take(20).ToArray());
            Text("catalogue-count",list.Length+" matching · "+category+" / "+view);Text("catalogue-page",(page+1)+" / "+pages);Text("catalogue-view",view);Text("catalogue-category",category);
            Text("catalogue-empty",list.Length==0?"No upgrades match. Try a different filter or search.":"");Enable("Page","previous",page>0);Enable("Page","next",page+1<pages);
        }
        void RefreshDialog()
        {
            if(stack.Count==0||Engine==null)return;var e=Engine;var s=e.State;
            if(CurrentDialog=="CardDetail")
            {
                int tier=(int)e.Data.Cards[selectedCard].tier;Color ink=tier==2?new Color32(154,39,57,255):tier==1?new Color32(116,39,70,255):new Color32(22,75,112,255);
                texts["full-name"].color=texts["full-rarity"].color=texts["full-flavor"].color=ink;texts["full-level"].transform.parent.GetComponent<UnityEngine.UI.Image>().color=ink;
            }
            switch(CurrentDialog)
            {
                case "ProductDetail":
                    var p=e.Data.Products[product];int level=s.products[product],next=NextMilestone(level);Text("product-title",p.name);Text("product-detail-name",p.name);Text("product-detail-level","Lvl : "+level);Text("product-subtitle",RedesignProductArt.Label(e.ProductAppearance(product)));Picture("product-detail-art",progression?progression.ProductSprite(product):productArt[product]);
                    Text("product-earnings","Earns "+Cash(e.ProductIncome(product))+" / second");Text("product-milestone",level>=100000?"All milestones reached":"Lvl "+next+" · profit ×"+EconomyBalance.MilestoneReward(next));Text("product-progress-label","Lvl "+(next==0?Catalog.MaxLevel:next));motion.Fill(images["product-progress"],MilestoneProgress(level));
                    int minimum=s.products.Min(), synergy=EconomyBalance.NextSynergy(minimum);
                    Text("synergy-title",synergy==0?"Synergy complete":"Next synergy bonus");
                    Text("synergy-progress-label",synergy==0?"Complete":s.products.Count(n=>n>=synergy)+"/8 products at Lvl "+synergy);
                    motion.Fill(images["synergy-progress"],synergy==0?1:(float)s.products.Count(n=>n>=synergy)/8);
                    Text("synergy-reward","All products: ×"+EconomyBalance.SynergyMultiplier(minimum)+(synergy==0?"":" → another ×2"));
                    Text("synergy-copy",synergy==0?"All synergy milestones reached.":string.Join(", ",Enumerable.Range(0,8).Where(i=>s.products[i]<synergy).Select(i=>e.Data.Products[i].name+" ("+s.products[i]+")")));
                    break;
                case "TapTraining":var q=e.PreviewLevels(-1,LevelQuantity(-1));Text("tap-detail","Level "+q.beforeLevel+" → "+q.afterLevel+"\nPer tap "+Cash(q.tapBefore)+" → "+Cash(q.tapAfter)+"\n"+(q.nextMilestone==0?"All milestones reached":"Next: Lvl "+q.nextMilestone+" · ×"+EconomyBalance.MilestoneReward(q.nextMilestone)));Text("tap-buy",s.tapLevel>=Catalog.MaxLevel?"Max level":Cash(q.quote.cost)+" · Train ×"+Math.Max(1,q.quote.quantity));Enable("BuyLevels","-1",driver.Session.CanMutate&&q.quote.affordable);break;
                case "SkillDetail":var sk=e.PreviewSkill(skill);Text("skill-title",SkillNames[skill]);Picture("skill-art",skillArt[skill]);Text("skill-state",SkillState(skill));Text("skill-copy",skill==0?"×"+sk.power+" automatic sales for "+sk.duration+"s. Manual and Helping Hands taps receive ×1.25.":skill==1?sk.power+" automatic taps each second for "+sk.duration+"s.":"Pays "+Cash(sk.orderPayout)+" immediately ("+sk.power+" seconds of normal sales).");Text("skill-timing","Cooldown · "+Math.Round(sk.cooldown)+" seconds\nNormal sales are the base for tap links and Bulk Order.");Text("skill-activate",SkillState(skill)=="READY"?"Activate skill":SkillState(skill));Enable("ActivateSkill","",driver.Session.CanMutate&&SkillState(skill)=="READY");break;
                case "CardDetail":var c=e.Data.Cards[selectedCard];int tier=(int)c.tier;Picture("full-outer",outerFrames[tier]);Picture("full-inner",innerFrames[tier]);Picture("full-art",cardArt[selectedCard]);Text("full-name",c.name);Text("full-level","Lvl "+Count(s.cardLevels[selectedCard]));Text("full-rarity","· "+c.tier.ToString().ToUpperInvariant()+" ·");Text("full-benefit",RedesignCardCopy.Effect(c,s.cardLevels[selectedCard]));Text("full-flavor",c.effect==CardEffect.ProductProfit?"A brighter display. A busier shop.":c.effect==CardEffect.ProductCost?"Stock up wisely. Grow your shop.":"A little talent. A brighter festival.");break;
                case "DrawReveal":var r=s.reveal;if(!e.Data.TryCard(r.cardId,out int ci))break;var card=e.Data.Cards[ci];Text("reveal-progress",r.batch!=null&&r.batch.Length>0?"Card "+(r.batchIndex+1)+" / "+r.batch.Length:"Your card awaits");Text("reveal-name",card.name);Text("reveal-level","Lvl "+Count(r.after));Text("reveal-rarity",card.tier.ToString());Picture("reveal-frame",miniFrames[(int)card.tier]);Picture("reveal-art",cardArt[ci]);Text("reveal-result",r.before.IsZero?"New card discovered!":"Level "+Count(r.before)+" → "+Count(r.after));Text("reveal-effect",RedesignCardCopy.Effect(card,r.after));Text("reveal-continue",r.batch!=null&&r.batchIndex+1<r.batch.Length?"Next card":"Continue");break;
                case "Prestige":var f=prestige??e.PreviewPrestige();Text("prestige-reward",f.Eligible?"+"+Count(f.Fame)+" Fame":Cash(s.eligibleEarnings)+" earned");Text("prestige-copy","Run target · "+Cash(f.CurrentTarget)+"\nProducts ×"+f.ResultingIncomeMultiplier+" · taps ×"+f.ResultingTapIncomeMultiplier+"\n"+Count(f.Credits)+" free Talent draws\nNext run target · "+Cash(f.NextTarget));Enable("Prestige","",driver.Session.CanMutate&&e.CanCommitPrestige(f));break;
                case "Settings":Text("setting-music",s.music?"ON":"OFF");Text("setting-sound",s.sound?"ON":"OFF");Text("setting-motion",s.reducedMotion?"ON":"OFF");break;
                case "Offline":Text("offline-amount",Cash(s.offline.amount));Text("offline-copy","Already added to your cash\nAway for "+TimeSpan.FromSeconds(s.offline.absence).TotalHours.ToString("0.0")+" hours\nCredited "+TimeSpan.FromSeconds(s.offline.creditedSeconds).TotalHours.ToString("0.0")+" hours at "+(s.offline.efficiency*100).ToString("0")+"% efficiency.");break;
                case "Statistics":Text("stats-run","Earned this run: "+Cash(s.eligibleEarnings)+"\nNormal sales: "+Cash(e.NormalIncome())+" /s\nManual taps: "+Count(s.manualTaps)+"\nTap payout: "+Cash(e.TapPayout())+"\nOwned upgrades: "+s.upgrades.Count+" / 1,000");Text("stats-lifetime","Prestiges: "+Count(s.completedPrestiges)+"\nFame: "+Count(s.fame)+"\nDiscovered upgrades: "+s.UniqueCount+" / 1,000\nTalent cards: "+s.cardLevels.Count(n=>!n.IsZero)+" / 28\nLifetime earnings: "+Cash(s.lifetimeEarnings));break;
                case "Purchase":var packData=e.Data.Packs[pack];Text("purchase-name",packData.name);Text("purchase-amount",packData.kajuKatli+" Kaju Katli");Text("purchase-price","₹"+packData.designInr+" · example price");break;
                case "BuyAll":Text("batch-copy",batch==null?"No quote available":batch.Ids.Count+" upgrades · "+Cash(batch.Cost)+"\n"+string.Join("\n",batch.Ids.Select(id=>e.Data.Upgrades.First(u=>u.id==id).name)));Enable("ConfirmBatch","",batch!=null&&e.CanCommitBuyAll(batch));break;
            }
        }
    }
}
