using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    public sealed partial class ClassicMotion
    {
        public RectTransform tapHint;
        public Image tapHintFace;
        public Image[] skillGlowImages,skillArtwork;
        public Material cooldownMaterial;
        public CanvasGroup revealGlow;
        public Image revealGlowImage;
        sealed class Pose
        {
            public RectTransform target;public Vector3 scale;public Quaternion rotation;
            public Vector2 original,written,offset;public float factor=1,angle,turn;public bool applied;
        }
        sealed class InputTrack
        {
            public ClassicControl control;public Pose pose;public float scale=1,y,angle,start,fromScale=1,fromY,fromAngle;public bool hover,pressed;
        }
        sealed class DimTrack { public Image image;public Color color;public float start;public bool exit; }
        readonly Dictionary<RectTransform,Pose> poses=new Dictionary<RectTransform,Pose>();
        readonly List<InputTrack> inputs=new List<InputTrack>();
        readonly List<DimTrack> dims=new List<DimTrack>();
        readonly string[] skillStates=new string[3];
        float ambientTime;bool focused=true;Color hintColor;
        static readonly Color[][] Palettes={new[]{Hex(0xf2c56d),Hex(0xfff0bd),Hex(0xe5a359),Hex(0x7cbab0)},new[]{Hex(0xdf95c4),Hex(0xffe8bd),Hex(0xa977c7)},new[]{Hex(0xffdc79),Hex(0xf49752),Hex(0xfff1bc)}};
        static Color Hex(int n)=>new Color32((byte)(n>>16),(byte)(n>>8),(byte)n,255);
        void InitializePresentation()
        {
            // Layout runs first; presentation offsets compose afterwards and are removed next Update.
            // This keeps animated rows/dialogs compatible with saved responsive layout components.
            _=CanvasUpdateRegistry.instance;Canvas.willRenderCanvases+=ApplyPoses;
            if(tapHintFace)hintColor=tapHintFace.color;
        }
        void OnDestroy()=>Canvas.willRenderCanvases-=ApplyPoses;
        Pose GetPose(RectTransform target)
        {
            if(!poses.TryGetValue(target,out var p)){p=new Pose{target=target,scale=target.localScale,rotation=target.localRotation};poses.Add(target,p);}return p;
        }
        static void RestorePose(Pose p)
        {
            if(!p.target||!p.applied)return;
            // A layout owner may already have replaced the animated position; retain its new result.
            if((p.target.anchoredPosition-p.written).sqrMagnitude<.0001f)p.target.anchoredPosition=p.original;
            p.target.localScale=p.scale;p.target.localRotation=p.rotation;p.applied=false;
        }
        void ResetPose(RectTransform target)
        {
            if(!poses.TryGetValue(target,out var p))return;RestorePose(p);p.factor=1;p.angle=p.turn=0;p.offset=Vector2.zero;
        }
        void RestorePresentation(){foreach(var p in poses.Values){RestorePose(p);p.factor=1;p.angle=p.turn=0;p.offset=Vector2.zero;}}
        void ApplyPoses()
        {
            if(!isActiveAndEnabled)return;
            foreach(var p in poses.Values)
            {
                RestorePose(p);if(!p.target||!p.target.gameObject.activeInHierarchy)continue;
                p.original=p.target.anchoredPosition;p.written=p.original+p.offset;p.target.anchoredPosition=p.written;
                p.target.localScale=p.scale*p.factor;p.target.localRotation=p.rotation*Quaternion.Euler(0,p.turn,p.angle);p.applied=true;
            }
        }
        public void Content(RectTransform root)
        {
            if(!root||reduced)return;Canvas.ForceUpdateCanvases();int index=0;var bounds=new Vector3[4];root.GetWorldCorners(bounds);
            foreach(var group in root.GetComponentsInChildren<CanvasGroup>())
            {
                var rect=(RectTransform)group.transform;if(rect==root||group.name=="Ready glow"||!rect.gameObject.activeInHierarchy||index==10)continue;
                var corners=new Vector3[4];rect.GetWorldCorners(corners);if(corners[2].y<bounds[0].y||corners[0].y>bounds[2].y)continue;
                Enter(rect,Mathf.Min(index++*.028f,.16f),9);
            }
            foreach(var image in root.GetComponentsInChildren<Image>())if(image.type==Image.Type.Filled)
            {
                var old=fills.Find(f=>f.image==image);float value=old!=null?old.to:image.fillAmount;if(old!=null)fills.Remove(old);
                fills.Add(new FillTrack{image=image,from=0,to=value,start=Time.unscaledTime,duration=.58f,delay=.08f});image.fillAmount=0;
            }
        }
        public void Backdrop(Image image,bool exit=false)
        {
            if(!image)return;for(int i=dims.Count-1;i>=0;i--)if(dims[i].image==image){image.color=dims[i].color;dims.RemoveAt(i);}
            if(reduced)return;dims.Add(new DimTrack{image=image,color=image.color,start=Time.unscaledTime,exit=exit});if(!exit){var c=image.color;c.a=0;image.color=c;}
        }
        public void ResetView(Transform root)
        {
            for(int i=tracks.Count-1;i>=0;i--)if(tracks[i].target.IsChildOf(root)){Restore(tracks[i]);tracks.RemoveAt(i);}
            for(int i=dims.Count-1;i>=0;i--)if(dims[i].image.transform.IsChildOf(root)){dims[i].image.color=dims[i].color;dims.RemoveAt(i);}
        }
        public void Input(ClassicControl control,bool hover,bool pressed)
        {
            var target=control.motionTarget?control.motionTarget:(RectTransform)control.transform;
            var t=inputs.Find(x=>x.control==control);if(t==null){t=new InputTrack{control=control,pose=GetPose(target)};inputs.Add(t);}
            t.fromScale=t.scale;t.fromY=t.y;t.fromAngle=t.angle;t.hover=hover;t.pressed=pressed;t.start=Time.unscaledTime;
        }
        public void ResetInput(ClassicControl control)
        {
            for(int i=inputs.Count-1;i>=0;i--)if(inputs[i].control==control){ResetPose(inputs[i].pose.target);inputs.RemoveAt(i);}
            if(control.motionShadow)control.motionShadow.enabled=false;
        }
        public void SkillState(int index,string state)
        {
            skillStates[index]=state;if(skillArtwork!=null&&index<skillArtwork.Length)skillArtwork[index].material=state=="COOLDOWN"?cooldownMaterial:null;
        }
        void ResetPresentation()
        {
            RestorePresentation();foreach(var t in inputs)if(t.control&&t.control.motionShadow)t.control.motionShadow.enabled=false;inputs.Clear();
            foreach(var d in dims)if(d.image)d.image.color=d.color;dims.Clear();if(revealGlow)revealGlow.alpha=0;
        }
        void OnApplicationFocus(bool value){focused=value;if(!value)StopEffects();}
        void OnApplicationPause(bool paused){focused=!paused;if(paused)StopEffects();}
        void AnimatePresentation(float time)
        {
            for(int i=dims.Count-1;i>=0;i--){var d=dims[i];float p=Mathf.Clamp01((time-d.start)/(d.exit?.16f:.22f));var c=d.color;c.a*=d.exit?1-Ease(p):Ease(p);d.image.color=c;if(p>=1){d.image.color=d.color;dims.RemoveAt(i);}}
            foreach(var t in inputs)
            {
                if(!t.control||!t.control.gameObject.activeInHierarchy)continue;bool enabled=t.control.GetComponent<Button>().IsInteractable()&&!reduced;
                bool hover=enabled&&t.hover,pressed=enabled&&t.pressed;int kind=t.control.motionKind;float q=Ease(Mathf.Clamp01((time-t.start)/(pressed?.12f:.18f)));
                t.scale=Mathf.Lerp(t.fromScale,pressed?.97f:1,q);t.y=Mathf.Lerp(t.fromY,pressed?-1:hover?(kind==1?3:1):0,q);t.angle=Mathf.Lerp(t.fromAngle,hover&&kind==1?.6f:0,q);
                t.pose.factor*=t.scale;t.pose.offset+=Vector2.up*t.y;t.pose.angle+=t.angle;
                if(hover&&kind==2){float p=(time-t.start)/.45f;t.pose.angle+=p<.35f?Mathf.Lerp(0,2,p/.35f):p<.7f?Mathf.Lerp(2,-1.5f,(p-.35f)/.35f):p<1?Mathf.Lerp(-1.5f,0,(p-.7f)/.3f):0;}
                if(t.control.motionShadow)t.control.motionShadow.enabled=hover&&!pressed;
            }
            bool ambient=!reduced&&!dialogOpen&&focused;if(ambient)ambientTime+=Time.unscaledDeltaTime;time=ambientTime;
            if(tapHintFace){float p=Mathf.Repeat(time/5.5f,1),strength=reduced?0:p<.75f?0:p<.87f?(p-.75f)/.12f:(1-p)/.13f;tapHintFace.color=new Color(Mathf.Min(1,hintColor.r*(1+.1f*strength)),Mathf.Min(1,hintColor.g*(1+.1f*strength)),Mathf.Min(1,hintColor.b*(1+.1f*strength)),hintColor.a);}
            for(int i=0;i<motes.Length;i++)
            {
                float p=Mathf.Repeat((time+i*.83f)/(5+i%4),1);var c=motes[i].color;
                c.a=reduced?0:p<.25f?Mathf.Lerp(0,.48f,p/.25f):p<.7f?Mathf.Lerp(.48f,.35f,(p-.25f)/.45f):p<.9f?Mathf.Lerp(.35f,0,(p-.7f)/.2f):0;
                motes[i].color=c;motes[i].rectTransform.anchoredPosition=moteOrigins[i]+new Vector2((i%2==0?-12:12)*p,Mathf.Lerp(-10,22,p));motes[i].rectTransform.localScale=Vector3.one*Mathf.Lerp(.5f,.8f,p);
            }
            for(int i=0;i<lanterns.Length;i++){float p=(1-Mathf.Cos((time+i*2.1f)*Mathf.PI/4.7f))*.5f;var c=lanterns[i].color;c.a=reduced?0:Mathf.Lerp(.35f,.85f,p);lanterns[i].color=c;lanterns[i].rectTransform.localScale=Vector3.one*Mathf.Lerp(.9f,1.12f,p);}
            if(ambient){float p=(1-Mathf.Cos(time*Mathf.PI*2/6.5f))*.5f;keeper.anchoredPosition=keeperOrigin+Vector2.up*p;keeper.localRotation=Quaternion.Euler(0,0,-.6f*p);}
            for(int i=0;i<skillGlows.Length;i++)
            {
                bool active=skillStates[i]=="ACTIVE";float p=Mathf.Repeat((time+i*1.3f)/3.8f,1),ring=p<.7f?0:p<.85f?(p-.7f)/.15f:(1-p)/.15f;
                skillGlows[i].alpha=reduced?0:active?Mathf.Lerp(.33f,.6f,(1-Mathf.Cos(time*Mathf.PI*2/2.2f))*.5f):ring*.4f;
                if(skillGlowImages!=null&&i<skillGlowImages.Length)skillGlowImages[i].color=active?Hex(0x55d9ec):Hex(0xdfae4b);
                skillGlows[i].transform.localScale=Vector3.one*(active?1.15f:1+ring*.075f);
            }
        }
        void AnimateParticle(int i,float p)
        {
            var rect=particles[i].rectTransform;int n=particleOrder[i];float q=coinFlight[i]?Ease(p):Bezier(p,.15f,.6f,.4f,1),alpha,scale;Vector2 offset;
            if(coinFlight[i])
            {
                var d=particleVelocity[i]-particleOrigin[i];var a=new Vector2(n*9-9,0);var b=new Vector2(d.x*.15f+n*12-12,d.y*.18f);var c=new Vector2(d.x*.85f,d.y*.8f);
                if(q<.2f){float t=q/.2f;offset=Vector2.Lerp(a,b,t);alpha=Mathf.Lerp(0,.95f,t);scale=Mathf.Lerp(.6f,1,t);}
                else if(q<.82f){float t=(q-.2f)/.62f;offset=Vector2.Lerp(b,c,t);alpha=Mathf.Lerp(.95f,.9f,t);scale=Mathf.Lerp(1,.75f,t);}
                else{float t=(q-.82f)/.18f;offset=Vector2.Lerp(c,d,t);alpha=Mathf.Lerp(.9f,0,t);scale=Mathf.Lerp(.75f,.35f,t);}
                particles[i].color=new Color(1,1,1,alpha);rect.localRotation=Quaternion.identity;
            }
            else
            {
                var mid=particleVelocity[i]*.6f+Vector2.up*10;var end=particleVelocity[i]+Vector2.down*25;
                if(q<.25f){float t=q/.25f;offset=Vector2.Lerp(Vector2.zero,mid,t);alpha=t;scale=Mathf.Lerp(.25f,1,t);}
                else{float t=(q-.25f)/.75f;offset=Vector2.Lerp(mid,end,t);alpha=1-t;scale=Mathf.Lerp(1,.3f,t);}
                var c=particleColors[i];c.a=alpha;particles[i].color=c;rect.localRotation=Quaternion.Euler(0,0,-n*43*Mathf.Clamp01((q-.25f)/.75f));
            }
            rect.anchoredPosition=particleOrigin[i]+offset;rect.localScale=Vector3.one*scale;
        }
        static float Ease(float p)=>Bezier(p,.2f,.75f,.25f,1);
        static float Bezier(float p,float x1,float y1,float x2,float y2)
        {
            if(p<=0)return 0;if(p>=1)return 1;float low=0,high=1,t=p;
            for(int i=0;i<10;i++){t=(low+high)*.5f;float x=3*(1-t)*(1-t)*t*x1+3*(1-t)*t*t*x2+t*t*t;if(x<p)low=t;else high=t;}
            return 3*(1-t)*(1-t)*t*y1+3*(1-t)*t*t*y2+t*t*t;
        }
    }
}
