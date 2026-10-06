using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Animates a finite, Editor-authored hierarchy. Effects never own game rewards.</summary>
    public sealed partial class ClassicMotion : MonoBehaviour
    {
        public RectTransform effectRoot, keeper;
        public Image[] motes, lanterns, particles;
        public Sprite coinSprite,starSprite;
        public CanvasGroup[] skillGlows;
        public bool reduced, dialogOpen;
        sealed class Track { public RectTransform target; public CanvasGroup group; public Vector3 scale; public Vector2 position; public float start,duration,delay,distance,alpha=1,fromScale=1,fromAlpha; public bool pulse,exit; public Action done; }
        readonly List<Track> tracks=new List<Track>();
        Vector2[] moteOrigins,particleOrigin,particleVelocity;float[] particleTime;Color[] particleColors;
        bool[] coinFlight; Vector2[] particleSizes; Sprite sparkSprite;float[] particleDuration;int[] particleOrder;
        sealed class FillTrack { public Image image; public float from,to,start,duration=.4f,delay; }
        readonly List<FillTrack> fills=new List<FillTrack>();
        Vector2 keeperOrigin;
        RectTransform flip;GameObject front,back;Action revealed;float flipAt;bool wasReduced;
        void Awake()
        {
            if(!effectRoot||!keeper||motes.Length!=12||particles.Length<40)throw new InvalidOperationException("Classic motion needs its saved effect pool");
            moteOrigins=new Vector2[motes.Length];for(int i=0;i<motes.Length;i++)moteOrigins[i]=motes[i].rectTransform.anchoredPosition;
            keeperOrigin=keeper.anchoredPosition;particleOrigin=new Vector2[particles.Length];particleVelocity=new Vector2[particles.Length];particleTime=new float[particles.Length];particleColors=new Color[particles.Length];
            coinFlight=new bool[particles.Length];particleSizes=new Vector2[particles.Length];sparkSprite=particles[0].sprite;for(int i=0;i<particles.Length;i++)particleSizes[i]=particles[i].rectTransform.sizeDelta;
            particleDuration=new float[particles.Length];particleOrder=new int[particles.Length];
            for(int i=0;i<particles.Length;i++){particleTime[i]=-10;particles[i].gameObject.SetActive(false);}
            InitializePresentation();
        }
        void Cancel(RectTransform target)
        {
            for(int i=tracks.Count-1;i>=0;i--)if(tracks[i].target==target){Restore(tracks[i]);tracks.RemoveAt(i);}
        }
        void Restore(Track t) { if(!t.target)return;ResetPose(t.target);if(t.group)t.group.alpha=t.alpha; }
        public void Enter(RectTransform target,float delay=0,float distance=12,float duration=.32f,float fromScale=1,float fromAlpha=0)
        {
            if(!target)return;Cancel(target);if(reduced)return;var group=target.GetComponent<CanvasGroup>();
            GetPose(target);tracks.Add(new Track{target=target,group=group,alpha=group?group.alpha:1,scale=target.localScale,position=target.anchoredPosition,start=Time.unscaledTime,duration=duration,delay=delay,distance=distance,fromScale=fromScale,fromAlpha=fromAlpha});if(group)group.alpha=fromAlpha;
        }
        public void Exit(RectTransform target,Action done)
        {
            if(!target){done?.Invoke();return;}Cancel(target);if(reduced){done?.Invoke();return;}GetPose(target);var group=target.GetComponent<CanvasGroup>();tracks.Add(new Track{target=target,group=group,alpha=group?group.alpha:1,scale=target.localScale,position=target.anchoredPosition,start=Time.unscaledTime,duration=.16f,distance=7,exit=true,done=done});
        }
        public void Pulse(RectTransform target)
        {
            if(!target||reduced)return;Cancel(target);GetPose(target);tracks.Add(new Track{target=target,scale=target.localScale,position=target.anchoredPosition,start=Time.unscaledTime,duration=.38f,pulse=true});
        }
        public void Success(RectTransform target,int tier=0,int count=12){Pulse(target);Burst(target,tier,count);}
        public void Fill(Image image,float value)
        {
            var active=fills.Find(f=>f.image==image);if(active!=null&&Mathf.Approximately(active.to,value))return;
            if(active!=null)fills.Remove(active);if(reduced||!image.gameObject.activeInHierarchy){image.fillAmount=value;return;}
            if(Mathf.Approximately(image.fillAmount,value))return;fills.Add(new FillTrack{image=image,from=image.fillAmount,to=value,start=Time.unscaledTime});
        }
        public void Coins(RectTransform origin,RectTransform destination)
        {
            if(reduced||!coinSprite)return;int emitted=0;
            for(int i=0;i<particles.Length&&emitted<3;i++)if(!particles[i].gameObject.activeSelf)
            {
                coinFlight[i]=true;particleOrigin[i]=effectRoot.InverseTransformPoint(origin.position);particleVelocity[i]=effectRoot.InverseTransformPoint(destination.position);
                particleTime[i]=Time.unscaledTime+emitted*.055f;particleDuration[i]=.65f;particleOrder[i]=emitted;particles[i].sprite=coinSprite;particles[i].rectTransform.sizeDelta=new Vector2(11,11);particles[i].rectTransform.anchoredPosition=particleOrigin[i];particles[i].color=Color.clear;particles[i].gameObject.SetActive(true);emitted++;
            }
        }
        public void Burst(RectTransform target,int tier=0,int count=12)
        {
            if(!target||reduced)return;var center=(Vector2)effectRoot.InverseTransformPoint(target.position);int emitted=0;
            for(int i=0;i<particles.Length&&emitted<count;i++)if(!particles[i].gameObject.activeSelf)
            {
                float angle=emitted*6.283185f/count,range=(count==7?28:count==12?42:76)+emitted%4*5;particleOrigin[i]=center;particleVelocity[i]=new Vector2(Mathf.Cos(angle),-Mathf.Sin(angle))*range;
                particleTime[i]=Time.unscaledTime;particleDuration[i]=.65f+emitted%4*.065f;particleOrder[i]=emitted;var palette=Palettes[Mathf.Clamp(tier,0,2)];particleColors[i]=palette[emitted%palette.Length];
                coinFlight[i]=false;bool star=emitted%3==0;particles[i].sprite=star&&starSprite?starSprite:sparkSprite;particles[i].rectTransform.sizeDelta=star?new Vector2(10,10):new Vector2(5,7);
                particles[i].color=Color.clear;particles[i].gameObject.SetActive(true);emitted++;
            }
        }
        public void Reveal(RectTransform target,GameObject face,GameObject reverse,Action done)
        {
            SettleFlip();flip=target;GetPose(target);front=face;back=reverse;revealed=done;flipAt=Time.unscaledTime;front.SetActive(false);back.SetActive(true);if(revealGlow)revealGlow.alpha=0;if(reduced)SettleFlip();
        }
        public void SettleFlip()
        {
            if(!flip)return;ResetPose(flip);flip.localRotation=Quaternion.identity;front.SetActive(true);front.transform.localRotation=Quaternion.identity;back.SetActive(false);flip=null;if(revealGlow)revealGlow.alpha=reduced?0:.25f;var callback=revealed;revealed=null;callback?.Invoke();
        }
        public void StopEffects()
        {
            var ending=tracks.ToArray();tracks.Clear();foreach(var t in ending){Restore(t);if(t.exit)t.done?.Invoke();}SettleFlip();if(particles!=null)foreach(var p in particles)if(p)p.gameObject.SetActive(false);
            foreach(var f in fills)if(f.image)f.image.fillAmount=f.to;fills.Clear();
            if(keeper){keeper.anchoredPosition=keeperOrigin;keeper.localRotation=Quaternion.identity;}
            ResetPresentation();
        }
        void OnDisable(){StopEffects();}
        void Update()
        {
            RestorePresentation();
            if(reduced&&!wasReduced)StopEffects();wasReduced=reduced;
        }
        void LateUpdate()
        {
            float time=Time.unscaledTime;
            for(int i=fills.Count-1;i>=0;i--){var f=fills[i];float p=Mathf.Clamp01((time-f.start-f.delay)/f.duration);f.image.fillAmount=Mathf.Lerp(f.from,f.to,Ease(p));if(p>=1)fills.RemoveAt(i);}
            for(int i=tracks.Count-1;i>=0;i--)
            {
                var t=tracks[i];if(!t.target||!t.target.gameObject.activeInHierarchy){Restore(t);tracks.RemoveAt(i);continue;}
                float raw=(time-t.start-t.delay)/t.duration,p=Mathf.Clamp01(raw),ease=Ease(p);var pose=GetPose(t.target);
                pose.factor*=t.pulse?(ease<.35f?Mathf.Lerp(1,1.07f,ease/.35f):Mathf.Lerp(1.07f,1,(ease-.35f)/.65f)):t.exit?1-.02f*ease:Mathf.Lerp(t.fromScale,1,ease);
                if(!t.pulse)pose.offset+=Vector2.down*t.distance*(t.exit?ease:1-ease);
                if(t.group&&!t.pulse)t.group.alpha=t.alpha*(t.exit?1-ease:Mathf.Lerp(t.fromAlpha,1,ease));
                if(raw>=1){Restore(t);tracks.RemoveAt(i);t.done?.Invoke();}
            }
            if(flip)
            {
                float p=Mathf.Clamp01((time-flipAt)/1.65f),q,y,angle,scale;
                if(p<.22f){y=-8;angle=0;scale=.94f;}
                else if(p<.42f){q=Bezier((p-.22f)/.2f,.25f,.65f,.3f,1);y=Mathf.Lerp(-8,8,q);angle=Mathf.Lerp(0,-12,q);scale=Mathf.Lerp(.94f,1.02f,q);}
                else if(p<.78f){q=Bezier((p-.42f)/.36f,.25f,.65f,.3f,1);y=Mathf.Lerp(8,4,q);angle=Mathf.Lerp(-12,190,q);scale=Mathf.Lerp(1.02f,1.03f,q);}
                else{q=Bezier((p-.78f)/.22f,.25f,.65f,.3f,1);y=Mathf.Lerp(4,0,q);angle=Mathf.Lerp(190,180,q);scale=Mathf.Lerp(1.03f,1,q);}
                var pose=GetPose(flip);pose.offset=Vector2.up*y;pose.factor=scale;pose.turn=angle;bool facing=angle>=90;front.SetActive(facing);back.SetActive(!facing);front.transform.localRotation=Quaternion.Euler(0,180,0);
                if(p>=1)SettleFlip();
            }
            AnimatePresentation(time);
            for(int i=0;i<particles.Length;i++)if(particles[i].gameObject.activeSelf)
            {
                float p=(time-particleTime[i])/particleDuration[i];if(p>=1||reduced){particles[i].gameObject.SetActive(false);continue;}if(p<0)continue;
                AnimateParticle(i,p);
            }
        }
    }
}
