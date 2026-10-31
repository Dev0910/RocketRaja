using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Reviewed festival feedback on a fixed, Editor-authored pool. Owns no gameplay rewards.</summary>
    [DefaultExecutionOrder(110)]
    public sealed class ClassicFestivalFeel : MonoBehaviour
    {
        public ClassicUI ui;
        public RectTransform overlay, scene;
        public Image warmLight, parcel, prestigeVeil, prestigeBloom, fireworkTrail;
        public Image[] hands, rings, tokens, fireworkSparks;
        public TMP_Text[] payouts;
        public TMP_Text milestone;
        public CanvasGroup milestoneGroup;
        float[] ringAt, payoutAt;
        Vector2[] ringPosition, payoutPosition;
        float[] ringSize;
        Vector2 tokenFrom, tokenTo, fireworkCenter;
        float tokenAt=-10, parcelAt=-10, prestigeAt=-10, milestoneAt=-10, fireworkAt=-10, nextFirework, sceneTime;
        int fireworkCount;
        bool foreground=true, focused=true, paused, wasReduced;
        bool Ready => ui && ui.driver.Initialized && ui.driver.Session.CanMutate && !ui.driver.NativeAdPresentation && foreground;
        bool Moving => Ready && !ui.motion.reduced;
        public int StockCount { get; private set; }
        public int MilestoneCount { get; private set; }
        public int RewardCount { get; private set; }
        public int PrestigeCount { get; private set; }
        public bool StockAnimating => ui.motion.StockAnimating;

        void Awake()
        {
            ringAt=new float[rings.Length];ringPosition=new Vector2[rings.Length];ringSize=new float[rings.Length];
            payoutAt=new float[payouts.Length];payoutPosition=new Vector2[payouts.Length];
            Clear();nextFirework=Time.unscaledTime+6.8f;
        }
        public void Clear()
        {
            if(ringAt!=null)for(int i=0;i<ringAt.Length;i++){ringAt[i]=-10;rings[i].gameObject.SetActive(false);}
            if(payoutAt!=null)for(int i=0;i<payoutAt.Length;i++){payoutAt[i]=-10;payouts[i].gameObject.SetActive(false);}
            foreach(var token in tokens)token.gameObject.SetActive(false);
            foreach(var spark in fireworkSparks)spark.gameObject.SetActive(false);
            foreach(var hand in hands)hand.gameObject.SetActive(false);
            warmLight.gameObject.SetActive(false);parcel.gameObject.SetActive(false);prestigeVeil.gameObject.SetActive(false);
            fireworkTrail.gameObject.SetActive(false);milestoneGroup.gameObject.SetActive(false);
            tokenAt=parcelAt=prestigeAt=milestoneAt=fireworkAt=-10;
        }
        Vector2 Point(Vector3 world) => overlay.InverseTransformPoint(world);
        static float Smooth(float p)=>1-Mathf.Pow(1-Mathf.Clamp01(p),3);
        public void Ring(Vector3 origin,float size=52,int tier=0)
        {
            if(!Moving)return;
            for(int i=0;i<rings.Length;i++)if(!rings[i].gameObject.activeSelf)
            {
                ringAt[i]=Time.unscaledTime;ringPosition[i]=Point(origin);ringSize[i]=size;
                rings[i].color=tier==1?new Color32(221,164,219,255):new Color32(245,197,95,255);
                rings[i].gameObject.SetActive(true);return;
            }
        }
        public void Tap(Vector3 origin,string value)
        {
            if(!Moving)return;Ring(origin,26);
            for(int i=0;i<payouts.Length;i++)if(!payouts[i].gameObject.activeSelf)
            {
                payoutAt[i]=Time.unscaledTime;var p=Point(origin);
                p.x=Mathf.Clamp(p.x,overlay.rect.xMin+42,overlay.rect.xMax-42);
                p.y=Mathf.Clamp(p.y,overlay.rect.yMin+60,overlay.rect.yMax-60);
                payoutPosition[i]=p;payouts[i].text="+"+value;payouts[i].gameObject.SetActive(true);return;
            }
        }
        public void Levels(int product,int before,int after)
        {
            if(!Ready)return;
            if(product>=0&&before==0&&after>0)
            {
                StockCount++;
                if(Moving){var target=(RectTransform)ui.shelfProducts[product].transform;ui.motion.Stock(target);ui.motion.Burst(target,0,10,.6f);}
            }
            int crossed=0;foreach(int n in new[]{10,25,50,100})if(before<n&&after>=n)crossed=n;
            if(crossed==0)return;
            MilestoneCount++;milestoneAt=Time.unscaledTime;
            milestone.text=(product<0?"Salesmanship":ui.Engine.Data.Products[product].name)+" · Level "+crossed+"!";
            milestoneGroup.gameObject.SetActive(true);
            if(Moving){ui.motion.NodKeeper();ui.motion.Burst(product<0?ui.motion.tapHint:(RectTransform)ui.shelfProducts[product].transform,0,22,.7f);}
        }
        public void Skill(int index)
        {
            if(index>=0&&index<3)Ring(ui.homeSkillImages[index].rectTransform.position,52,1);
            if(index==2&&Moving){parcelAt=Time.unscaledTime;parcel.gameObject.SetActive(true);}
        }
        public void Reward(Vector3 origin)
        {
            if(!Ready)return;RewardCount++;
            if(!Moving)return;
            RectTransform destination=ui.achievementWallet&&Visible(ui.achievementWallet.rectTransform)?ui.achievementWallet.rectTransform:
                ui.legacyCurrencyIcon&&Visible(ui.legacyCurrencyIcon.rectTransform)?ui.legacyCurrencyIcon.rectTransform:(RectTransform)ui.navigation[3].transform;
            tokenFrom=Point(origin);tokenTo=Point(destination.position);tokenAt=Time.unscaledTime;
            foreach(var token in tokens)token.gameObject.SetActive(true);
        }
        static bool Visible(RectTransform target)
        {
            if(!target.gameObject.activeInHierarchy)return false;
            foreach(var mask in target.GetComponentsInParent<RectMask2D>())
                if(!mask.rectTransform.rect.Contains(mask.rectTransform.InverseTransformPoint(target.position)))return false;
            return true;
        }
        public void Prestige()
        {
            if(!Ready)return;PrestigeCount++;if(!Moving)return;
            prestigeAt=Time.unscaledTime;prestigeVeil.gameObject.SetActive(true);
        }
        void Firework()
        {
            fireworkAt=Time.unscaledTime;fireworkCount=14;
            fireworkCenter=new Vector2(Mathf.Lerp(scene.rect.xMin,scene.rect.xMax,UnityEngine.Random.Range(.27f,.73f)),Mathf.Lerp(scene.rect.yMin,scene.rect.yMax,.72f));
            fireworkTrail.gameObject.SetActive(true);
            foreach(var spark in fireworkSparks)spark.gameObject.SetActive(false);
        }
        void LateUpdate()
        {
            if(!Ready){Clear();return;}
            bool reduced=ui.motion.reduced;
            if(reduced&&!wasReduced)Clear();wasReduced=reduced;
            float now=Time.unscaledTime;
            if(!ui.motion.dialogOpen)sceneTime+=Time.unscaledDeltaTime;
            var s=ui.Engine.State;
            bool rush=s.activeUntil[0]>s.now,help=s.activeUntil[1]>s.now;
            warmLight.gameObject.SetActive(rush);
            warmLight.color=new Color(1,1,1,reduced?.2f:.2f+.035f*Mathf.Sin(sceneTime*Mathf.PI));
            for(int i=0;i<hands.Length;i++)
            {
                hands[i].gameObject.SetActive(help&&!reduced);
                float phase=(sceneTime+i*.65f)*Mathf.PI*2/1.3f;
                hands[i].rectTransform.anchoredPosition=Vector2.up*(1-Mathf.Cos(phase))*3.5f;
                hands[i].rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Cos(phase)*7);
            }
            if(milestoneGroup.gameObject.activeSelf)
            {
                float p=(now-milestoneAt)/2.2f;if(p>=1)milestoneGroup.gameObject.SetActive(false);
                else{milestoneGroup.alpha=reduced?1:Mathf.Clamp01(Mathf.Min(p/.2f,(1-p)/.2f));((RectTransform)milestoneGroup.transform).anchoredPosition=Vector2.up*(reduced?0:Mathf.Lerp(-8,0,Smooth(p/.2f)));}
            }
            if(reduced)return;
            for(int i=0;i<rings.Length;i++)if(rings[i].gameObject.activeSelf)
            {
                float p=(now-ringAt[i])/.52f;if(p>=1){rings[i].gameObject.SetActive(false);continue;}
                var r=rings[i].rectTransform;r.anchoredPosition=ringPosition[i];r.sizeDelta=Vector2.one*ringSize[i];r.localScale=Vector3.one*Mathf.Lerp(.2f,1.7f,Smooth(p));
                var c=rings[i].color;c.a=.7f*(1-p);rings[i].color=c;
            }
            for(int i=0;i<payouts.Length;i++)if(payouts[i].gameObject.activeSelf)
            {
                float p=(now-payoutAt[i])/.78f;if(p>=1){payouts[i].gameObject.SetActive(false);continue;}
                payouts[i].rectTransform.anchoredPosition=payoutPosition[i]+Vector2.up*54*Smooth(p);
                payouts[i].alpha=Mathf.Clamp01(Mathf.Min(p/.18f,(1-p)/.3f));
            }
            for(int i=0;i<tokens.Length;i++)if(tokens[i].gameObject.activeSelf)
            {
                float p=(now-tokenAt-i*.075f)/.85f;if(p>=1){tokens[i].gameObject.SetActive(false);continue;}
                p=Mathf.Clamp01(p);tokens[i].rectTransform.anchoredPosition=Vector2.Lerp(tokenFrom,tokenTo,Smooth(p))+Vector2.up*Mathf.Sin(p*Mathf.PI)*20;
                tokens[i].rectTransform.localScale=Vector3.one*Mathf.Lerp(.9f,.55f,p);tokens[i].color=new Color(1,1,1,Mathf.Clamp01(Mathf.Min(p/.15f,(1-p)/.15f)));
            }
            if(parcel.gameObject.activeSelf)
            {
                float p=(now-parcelAt)/1.35f;if(p>=1)parcel.gameObject.SetActive(false);
                else{parcel.rectTransform.anchoredPosition=new Vector2(Mathf.Lerp(-55,scene.rect.width+55,Smooth(p)),Mathf.Sin(p*Mathf.PI)*6);parcel.color=new Color(1,1,1,Mathf.Clamp01(Mathf.Min(p/.15f,(1-p)/.15f)));}
            }
            if(prestigeVeil.gameObject.activeSelf)
            {
                float p=(now-prestigeAt)/1.35f;if(p>=1||ui.motion.dialogOpen)prestigeVeil.gameObject.SetActive(false);
                else{prestigeVeil.color=new Color(.082f,.075f,.17f,.88f*Mathf.Sin(p*Mathf.PI));prestigeBloom.color=new Color(1,1,1,Mathf.Sin(p*Mathf.PI));prestigeBloom.rectTransform.localScale=Vector3.one*Mathf.Lerp(.1f,2.3f,Smooth(p));}
            }
            if(now>=nextFirework&&!ui.motion.dialogOpen&&ui.CurrentTab=="Home"){nextFirework=now+6.8f;Firework();}
            float age=now-fireworkAt;
            if(age>=0&&age<1.43f&&!ui.motion.dialogOpen)
            {
                fireworkTrail.gameObject.SetActive(age<.48f);
                fireworkTrail.rectTransform.anchoredPosition=fireworkCenter+Vector2.down*Mathf.Lerp(65,0,Smooth(age/.48f));
                fireworkTrail.color=new Color(1,.9f,.64f,Mathf.Sin(Mathf.Clamp01(age/.48f)*Mathf.PI));
                float p=(age-.38f)/.95f;
                for(int i=0;i<fireworkSparks.Length;i++)
                {
                    bool show=i<fireworkCount&&p>=0&&p<1;fireworkSparks[i].gameObject.SetActive(show);if(!show)continue;
                    float a=i*Mathf.PI*2/fireworkCount;fireworkSparks[i].rectTransform.anchoredPosition=fireworkCenter+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*30*Smooth(p)+Vector2.down*18*p;
                    fireworkSparks[i].color=new Color(1,.85f,.46f,Mathf.Sin(p*Mathf.PI));
                }
            }
            else{fireworkTrail.gameObject.SetActive(false);foreach(var spark in fireworkSparks)spark.gameObject.SetActive(false);}
        }
        void OnApplicationPause(bool value){paused=value;foreground=focused&&!paused;if(!foreground)Clear();}
        void OnApplicationFocus(bool value){focused=value;foreground=focused&&!paused;if(!foreground)Clear();}
        void OnDisable(){if(ringAt!=null)Clear();}
    }
}
