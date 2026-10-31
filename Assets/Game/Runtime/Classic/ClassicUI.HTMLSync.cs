using System;
using System.Collections.Generic;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    [Serializable] public sealed class ClassicSkillClock
    {
        public int index;
        public Image ring, track, disc, artwork, statusBadge;
        public TMP_Text status, effect, timing, actionLabel;
        public Button activate;
    }
    public sealed partial class ClassicUI
    {
        public ClassicSkillClock[] skillClocks=Array.Empty<ClassicSkillClock>();
        public Sprite popupPrimary, popupSecondary, popupDisabled;
        readonly Queue<KeyValuePair<double,Amount>> acceptedTaps=new Queue<KeyValuePair<double,Amount>>();
        readonly Dictionary<ClassicView,GameObject> dialogFocus=new Dictionary<ClassicView,GameObject>();
        string exactSelection="overview";
        void RememberDialogFocus(ClassicView view)
        {
            dialogFocus[view]=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);
        }
        void RestoreDialogFocus(ClassicView view)
        {
            if(dialogFocus.TryGetValue(view,out var target)&&target&&target.activeInHierarchy&&EventSystem.current)EventSystem.current.SetSelectedGameObject(target);
            dialogFocus.Remove(view);
        }
        void RecordAcceptedTap(Amount amount)
        {
            acceptedTaps.Enqueue(new KeyValuePair<double,Amount>(Engine.State.now,amount));RefreshHTMLPresentation();
        }
        static string Expanded(Amount amount)=>"₹"+amount.mantissa.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+" × 10^"+amount.exponent;
        void SetButtonSkin(Button button,bool enabled)
        {
            var control=button.GetComponent<ClassicControl>();if(!(button.targetGraphic is Image image))return;
            if(control&&control.faceOverride){image.sprite=control.faceOverride;return;}
            if(control&&control.htmlStyle>0){if(control.htmlStyle==3){image.sprite=null;image.color=Color.clear;}else image.sprite=!enabled?popupDisabled:control.htmlStyle==2?popupSecondary:popupPrimary;}
            else image.sprite=enabled?blueButton:disabledButton;
        }
        string SkillEffect(int i)
        {
            var p=Engine.PreviewSkill(i);return i==0?UpgradeEffectCopy.Multiplier(p.power)+" automatic sales. Taps +25%.":i==1?UpgradeEffectCopy.Whole(p.power)+" automatic taps per second.":UpgradeEffectCopy.Whole(p.power)+" seconds of sales, instantly.";
        }
        static string SkillListTime(double seconds)
        {
            int whole=(int)Math.Ceiling(seconds);
            return whole<60?whole+"s":whole/60+"m"+(whole%60>0?" "+whole%60+"s":"");
        }
        void RefreshHTMLPresentation()
        {
            if(Engine==null)return;var e=Engine;var s=e.State;
            while(acceptedTaps.Count>0&&acceptedTaps.Peek().Key<=s.now-1)acceptedTaps.Dequeue();
            Amount automatic=s.activeUntil[0]>s.now?e.NormalIncome()*e.PreviewSkill(0).power:e.NormalIncome();
            Amount total=automatic;foreach(var tap in acceptedTaps)total+=tap.Value;
            Text("home-income",Cash(automatic)+"/sec");Text("home-tap",Cash(e.TapPayout())+"/tap");Text("home-total",Cash(total)+"/sec");
            Text("exact-values","Cash · "+Expanded(s.cash)+"\nAutomatic sales · "+Expanded(automatic)+"/sec\nPer tap · "+Expanded(e.TapPayout())+"/tap");
            Text("stats-shop","Products stocked · "+s.products.Count(n=>n>0)+" / 8\nStore name · "+s.storeName);
            if(texts.ContainsKey("stats-earned"))
            {
                Text("stats-earned",Cash(s.eligibleEarnings));Text("stats-sales",Cash(e.NormalIncome())+" /s");
                Text("stats-taps",Count(s.manualTaps));Text("stats-payout",Cash(e.TapPayout()));
                Text("stats-upgrades",s.upgrades.Count+" / 1,000");Text("stats-prestiges",Count(s.completedPrestiges));
                Text("stats-fame",Count(s.fame));Text("stats-discoveries",s.UniqueCount+" / 1,000");
                Text("stats-cards",s.cardLevels.Count(n=>!n.IsZero)+" / 28");Text("stats-earned-lifetime",Cash(s.lifetimeEarnings));
            }
            if(texts.ContainsKey("stats-run-values"))
            {
                Text("stats-run","Earned this run\nNormal sales\nManual taps\nTap payout\nOwned upgrades");
                Text("stats-run-values",Cash(s.eligibleEarnings)+"\n"+Cash(e.NormalIncome())+"/sec\n"+Count(s.manualTaps)+"\n"+Cash(e.TapPayout())+"\n"+s.upgrades.Count+" / 1,000");
                Text("stats-lifetime","Prestiges\nFame\nDiscovered upgrades\nTalent cards\nLifetime earnings");
                Text("stats-lifetime-values",Count(s.completedPrestiges)+"\n"+Count(s.fame)+"\n"+s.UniqueCount+" / 1,000\n"+s.cardLevels.Count(n=>!n.IsZero)+" / 28\n"+Cash(s.lifetimeEarnings));
                Text("stats-shop","Products stocked\nShop name");Text("stats-shop-values",s.products.Count(n=>n>0)+" / 8\n"+s.storeName);
            }
            if(exactSelection=="tap"||exactSelection=="product")
            {
                int target=exactSelection=="tap"?-1:product;
                Text("exact-values",(target<0?"Salesmanship":e.Data.Products[target].name)+"\nCurrent level quote · "+Expanded(e.LevelQuote(target,LevelQuantity(target)).cost)+"\nDisplay rounding does not change the quote.");
            }
            foreach(var clock in skillClocks)
            {
                int i=clock.index<0?skill:clock.index;var p=e.PreviewSkill(i);string state=SkillState(i);
                bool active=state=="ACTIVE",cooldown=state=="COOLDOWN",locked=state=="LOCKED";
                string label=active?"ACTIVE · "+TimeLabel(Math.Ceiling(s.activeUntil[i]-s.now)):cooldown?"WAIT · "+TimeLabel(Math.Ceiling(s.readyAt[i]-s.now)):state;
                if(clock.status)clock.status.text=label;
                if(clock.ring){clock.ring.enabled=!locked;clock.ring.color=cooldown?new Color32(197,202,206,255):new Color32(255,208,97,255);double start=i==2?s.readyAt[i]-p.cooldown:s.activeUntil[i];clock.ring.fillAmount=active?Mathf.Clamp01((float)((s.activeUntil[i]-s.now)/Math.Max(1,p.duration))):cooldown?Mathf.Clamp01((float)(1-(s.readyAt[i]-s.now)/Math.Max(1,s.readyAt[i]-start))):1;}
                if(clock.track){clock.track.enabled=!locked;clock.track.color=cooldown?new Color32(127,137,143,255):new Color32(142,107,51,255);}
                if(clock.disc)clock.disc.color=cooldown||locked?new Color32(102,115,128,255):clock.statusBadge&&i>0?new Color32(242,211,139,255):new Color32(23,76,123,255);
                if(clock.statusBadge){clock.statusBadge.color=cooldown||locked?new Color32(231,228,217,255):new Color32(251,227,170,255);clock.status.color=cooldown||locked?new Color32(89,102,109,255):new Color32(117,81,30,255);}
                if(clock.artwork){clock.artwork.sprite=locked?lockArt:skillArt[i];clock.artwork.material=cooldown||locked?undiscoveredMaterial:null;clock.artwork.color=cooldown?new Color(1,1,1,.65f):Color.white;}
                if(clock.activate)clock.activate.interactable=driver.Session.CanMutate&&state=="READY";
                if(clock.actionLabel)clock.actionLabel.text=active?"Active":cooldown?TimeLabel(Math.Ceiling(s.readyAt[i]-s.now)):locked?"Locked":"Activate";
                if(clock.effect)clock.effect.text=SkillEffect(i);
                if(clock.timing)clock.timing.text="Duration: "+(p.duration>0?SkillListTime(p.duration):"Instant")+"   Cooldown: "+SkillListTime(p.cooldown);
            }
            for(int i=0;i<3;i++)Enable("ActivateSkill",i.ToString(),driver.Session.CanMutate&&SkillState(i)=="READY");
            var selected=e.PreviewSkill(skill);
            Text("skill-effect-a",skill==0?UpgradeEffectCopy.Multiplier(selected.power):skill==1?UpgradeEffectCopy.Whole(selected.power)+"/sec":Cash(selected.orderPayout));
            Text("skill-effect-a-copy",skill==0?"Automatic sales":skill==1?"Automatic taps":"Instant payout");
            Text("skill-effect-b",skill==0?"+25%":skill==1?Cash(e.TapPayout()):UpgradeEffectCopy.Whole(selected.power)+" sec");
            Text("skill-effect-b-copy",skill==0?"Manual & Helping Hands taps":skill==1?"Payout per automatic tap":"Normal sales equivalent");
            Text("skill-duration",selected.duration>0?TimeLabel(selected.duration):"Instant");Text("skill-cooldown",TimeLabel(selected.cooldown));
            bool validName=GameState.TryStoreName(nameInput.text,out _);
            Enable("Rename","",driver.Session.CanMutate&&validName);
            if(texts.TryGetValue("naming-error",out var error)){error.transform.parent.gameObject.SetActive(CurrentDialog=="Naming"&&!validName);error.text="Enter a shop name (up to 24 characters).";}
            foreach(var c in controls.Where(c=>c.action=="Setting"&&c.htmlStyle>0))
                if(c.GetComponent<Button>().targetGraphic is Image image)
                {
                    bool on=c.parameter=="music"?s.music:c.parameter=="sound"?s.sound:s.reducedMotion;
                    image.sprite=on?popupPrimary:popupSecondary;
                    foreach(var label in c.GetComponentsInChildren<TMP_Text>(true))label.color=on?new Color32(255,244,219,255):new Color32(22,75,128,255);
                }
        }
    }
}
