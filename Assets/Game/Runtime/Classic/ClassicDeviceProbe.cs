#if CLASSIC_DEVICE_PREVIEW && !UNITY_EDITOR && !DEVELOPMENT_BUILD
#error CLASSIC_DEVICE_PREVIEW cannot be included in a release player.
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Read-only telemetry for the dedicated Android development playtest; no command channel.</summary>
    public sealed class ClassicDeviceProbe : MonoBehaviour
    {
        public ClassicUI ui;
#if DEVELOPMENT_BUILD && CLASSIC_DEVICE_PREVIEW
        [Serializable] sealed class Control
        {
            public string name,action,parameter;public bool enabled;public float x,y,width,height;
        }
        [Serializable] sealed class Sample
        {
            public string utc,device,os,graphics,tab,dialog,cash,kk,credits,manualTaps;
            public int width,height,objects,components,initialObjects,initialComponents,tapLevel,upgrades,activeParticles,runtimeErrors;
            public int[] products;public string[] cardLevels;public bool reduced,pendingReveal,continueEnabled,suspended,saving;
            public float fpsAverage,frameMsP95,safeX,safeY,safeWidth,safeHeight,revealAngle,revealScale;
            public Control[] controls;public string[] errors;
        }
        float next,frameStart;int frames,initialObjects,initialComponents,lastFrame=-1;readonly List<float> frameTimes=new List<float>();readonly List<string> errors=new List<string>();
        void OnEnable()=>Application.logMessageReceived+=OnLog;
        void OnDisable(){Application.logMessageReceived-=OnLog;Canvas.willRenderCanvases-=SampleFrame;}
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void Start(){initialObjects=ui.GetComponentsInChildren<Transform>(true).Length;initialComponents=ui.GetComponentsInChildren<Component>(true).Length;frameStart=Time.realtimeSinceStartup;Canvas.willRenderCanvases+=SampleFrame;}
        void SampleFrame()
        {
            if(!ui||!ui.driver.Initialized||lastFrame==Time.frameCount)return;lastFrame=Time.frameCount;frames++;frameTimes.Add(Time.unscaledDeltaTime*1000);
            if(Time.realtimeSinceStartup<next)return;next=Time.realtimeSinceStartup+.75f;
            var s=ui.Engine.State;var safe=Screen.safeArea;var times=frameTimes.OrderBy(x=>x).ToArray();
            var controls=new List<Control>();foreach(var c in ui.GetComponentsInChildren<ClassicControl>())
            {
                var rect=(RectTransform)c.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);var low=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var high=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
                if(high.x<=0||high.y<=0||low.x>=Screen.width||low.y>=Screen.height)continue;
                var scroll=c.GetComponentInParent<ScrollRect>();if(scroll&&scroll.viewport){var clip=new Vector3[4];scroll.viewport.GetWorldCorners(clip);low=Vector2.Max(low,RectTransformUtility.WorldToScreenPoint(null,clip[0]));high=Vector2.Min(high,RectTransformUtility.WorldToScreenPoint(null,clip[2]));if(high.x<=low.x||high.y<=low.y)continue;}
                controls.Add(new Control{name=c.name,action=c.action,parameter=c.parameter,enabled=c.GetComponent<Button>().IsInteractable(),x=(low.x+high.x)/2,y=Screen.height-(low.y+high.y)/2,width=high.x-low.x,height=high.y-low.y});
            }
            var report=new Sample{utc=DateTime.UtcNow.ToString("O"),device=SystemInfo.deviceModel,os=SystemInfo.operatingSystem,graphics=SystemInfo.graphicsDeviceName,
                tab=ui.CurrentTab,dialog=ui.CurrentDialog,cash=s.cash.ToString(),kk=s.kajuKatli.ToString(),credits=s.freeCredits.ToString(),manualTaps=s.manualTaps.ToString(),
                width=Screen.width,height=Screen.height,objects=ui.GetComponentsInChildren<Transform>(true).Length,components=ui.GetComponentsInChildren<Component>(true).Length,initialObjects=initialObjects,initialComponents=initialComponents,
                products=s.products,tapLevel=s.tapLevel,upgrades=s.upgrades.Count,cardLevels=s.cardLevels.Select(n=>n.digits).ToArray(),reduced=s.reducedMotion,pendingReveal=s.reveal.pending,continueEnabled=ui.revealContinue.interactable,
                suspended=ui.driver.Session.Suspended,saving=ui.driver.Session.CanMutate,activeParticles=ui.motion.particles.Count(p=>p.gameObject.activeSelf),runtimeErrors=errors.Count,
                fpsAverage=frames/Mathf.Max(.001f,Time.realtimeSinceStartup-frameStart),frameMsP95=times.Length==0?0:times[Mathf.Min(times.Length-1,(int)(times.Length*.95f))],
                safeX=safe.x,safeY=safe.y,safeWidth=safe.width,safeHeight=safe.height,revealAngle=ui.revealCard.localEulerAngles.y,revealScale=ui.revealCard.localScale.x,controls=controls.ToArray(),errors=errors.ToArray()};
            string path=Path.Combine(Application.persistentDataPath,"classic-device-state.json");File.WriteAllText(path+".next",JsonUtility.ToJson(report,true));if(File.Exists(path))File.Delete(path);File.Move(path+".next",path);
            frames=0;frameTimes.Clear();frameStart=Time.realtimeSinceStartup;
        }
#endif
    }
}
