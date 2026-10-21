using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Motion-specific Play Mode regression on the isolated saved importer.</summary>
    public static class ClassicMotionReview
    {
        sealed class Step { public string name;public Action action;public float wait; }
        static readonly Queue<Step> steps=new Queue<Step>();static readonly List<string> results=new List<string>();
        static ClassicUI ui;static ClassicView dialog;static ClassicControl control;static Vector2 origin,mote;static float before,revealHeight;static double levels;
        static string folder;static string[] identities;static Step current;static double due;static int checks;
        public static string Status { get; private set; }="Idle";
        static void Check(bool ok,string label){checks++;results.Add((ok?"PASS ":"FAIL ")+label);if(!ok)throw new InvalidOperationException(label);}
        static void Add(string name,Action action,float wait=.2f)=>steps.Enqueue(new Step{name=name,action=action,wait=wait});
        static PointerEventData Pointer()=>new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left};
        static double Levels()=>ui.Engine.State.cardLevels.Sum(n=>(double)n.Value);
        public static string Begin(string output)
        {
            ui=ClassicImportQA.UI;if(!Application.isPlaying||!ui.isolatedPreview)throw new InvalidOperationException("Isolated Play Mode required");
            folder=output;Directory.CreateDirectory(folder);steps.Clear();results.Clear();checks=0;current=null;
            identities=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetEntityId().ToString()).OrderBy(x=>x).ToArray();
            if(ui.Engine.State.offline.pending)ui.Handle("Offline","");if(ui.Engine.State.reducedMotion)ui.Handle("Setting","motion");ui.SetTab("Home");
            Add("home",()=>{},.7f);
            Add("ambient-start",()=>mote=ui.motion.motes[2].rectTransform.anchoredPosition,.35f);
            Add("ambient-moving",()=>{Check((mote-ui.motion.motes[2].rectTransform.anchoredPosition).sqrMagnitude>.001f,"Motes drift while Home is visible");dialog=ui.dialogs.Single(d=>d.route=="Settings");ui.Open("Settings");},.06f);
            Add("modal-start",()=>{Check(dialog.window.GetComponent<CanvasGroup>().alpha<1,"Dialog starts transparent");Check(dialog.window.anchoredPosition.y<0,"Dialog enters from below");},.12f);
            Add("modal-flight",()=>{},.45f);
            Add("modal-settled",()=>{Check(Mathf.Abs(dialog.window.anchoredPosition.y)<.01f&&dialog.window.localScale==Vector3.one,"Dialog settles at authored pose");Check(dialog.window.GetComponent<CanvasGroup>().alpha==1,"Dialog settles opaque");mote=ui.motion.motes[2].rectTransform.anchoredPosition;},.2f);
            Add("ambient-paused",()=>{Check(mote==ui.motion.motes[2].rectTransform.anchoredPosition,"Atmosphere pauses behind a modal");ui.Close();},.24f);
            Add("products-enter",()=>{Check(ui.CurrentDialog=="","Animated dismissal finishes");ui.SetTab("Products");},.08f);
            Add("products-flight",()=>Check(ui.panels.Single(p=>p.route=="Products").group.alpha<1,"Panel entrance is animated"),.8f);
            Add("products-settled",()=>{var panel=ui.panels.Single(p=>p.route=="Products");Check(panel.group.alpha==1&&panel.window.anchoredPosition==Vector2.zero,"Panel returns to authored pose");control=ui.products[0].buy.GetComponent<ClassicControl>();origin=((RectTransform)control.transform).anchoredPosition;control.OnPointerEnter(Pointer());},.12f);
            Add("hover",()=>{Check(((RectTransform)control.transform).anchoredPosition.y>origin.y,"Mouse hover lifts button");control.OnPointerDown(Pointer());},.13f);
            Add("pressed",()=>{Check(control.transform.localScale.x<.99f,"Press scales button down");control.OnPointerUp(Pointer());control.OnPointerExit(Pointer());},.28f);
            Add("released",()=>{Check(control.transform.localScale==Vector3.one&&((RectTransform)control.transform).anchoredPosition==origin,"Release restores button pose");ui.Handle("BuyLevels","0");},.09f);
            Add("product-success",()=>{Check(ui.products[0].artFrame.localScale.x>1,"Successful product purchase pulses its artwork frame");ui.SetTab("Home");},.6f);
            Add("tap",()=>{var p=Pointer();p.position=RectTransformUtility.WorldToScreenPoint(null,ui.tapTarget.transform.position);before=(float)ui.Engine.State.manualTaps.Value;ui.tapTarget.OnPointerDown(p);ui.tapTarget.OnPointerUp(p);Check(ui.motion.particles.Count(x=>x.gameObject.activeSelf&&x.sprite==ui.motion.coinSprite)==3,"Tap emits exactly three pooled coins");},.15f);
            Add("tap-flight",()=>{Check((float)ui.Engine.State.manualTaps.Value==before+1,"Effects do not duplicate tap reward");Check(ui.motion.particles.Any(x=>x.gameObject.activeSelf),"Tap particles visible in flight");},.95f);
            Add("tap-settled",()=>{Check(ui.motion.particles.All(x=>!x.gameObject.activeSelf),"Tap effects recycle all instances");Check(ui.motion.tapHint.localScale==Vector3.one,"Tap hint settles after pulse");ui.SetTab("Legacy");},.6f);
            Add("disabled-card",()=>{var locked=ui.cards.First(c=>!c.button.interactable);var cc=locked.button.GetComponent<ClassicControl>();cc.OnPointerEnter(Pointer());Check(!cc.motionShadow||!cc.motionShadow.enabled,"Undiscovered card has no hover response");ui.cards[0].button.GetComponent<ClassicControl>().OnPointerEnter(Pointer());},.12f);
            Add("owned-card-hover",()=>{var cc=ui.cards[0].button.GetComponent<ClassicControl>();Check(cc.motionShadow&&cc.motionShadow.enabled,"Owned-card hover shows rarity-tinted shadow");cc.OnPointerExit(Pointer());ui.Handle("Card","0");},.08f);
            Add("art-flight",()=>{Check(ui.imageBindings.Single(b=>b.key=="full-art").image.GetComponent<CanvasGroup>().alpha<1,"Full-card artwork fades in");},.6f);
            Add("art-settled",()=>{Check(ui.imageBindings.Single(b=>b.key=="full-art").image.GetComponent<CanvasGroup>().alpha==1,"Full-card artwork settles");ui.Close();},.05f);
            Add("interrupt",()=>{ui.SetTab("Products");ui.Open("Settings");ui.Close();ui.SetTab("Legacy");ui.Handle("Card","16");},.7f);
            Add("reopened",()=>{Check(ui.CurrentDialog=="CardDetail"&&ui.dialogs.Single(d=>d.route=="CardDetail").window.localScale==Vector3.one,"Interrupted dismissal cannot close a reopened card");ui.SetTab("Home");ui.Open("SkillDetail:0");ui.Handle("ActivateSkill","");},.4f);
            Add("skill-active",()=>{Check(ui.Engine.State.activeUntil[0]>ui.Engine.State.now,"Skill command activates");Check(ui.motion.skillGlowImages[0].color.b>.8f,"Active skill uses cyan glow");ui.Engine.State.activeUntil[0]=ui.Engine.State.now-1;ui.Engine.State.readyAt[0]=ui.Engine.State.now+10;ui.Refresh();},.25f);
            Add("skill-cooldown",()=>{Check(ui.homeSkillImages[0].material==ui.motion.cooldownMaterial,"Cooldown desaturates skill artwork");ui.SetTab("Legacy");levels=Levels();ui.Handle("Draw","5");},.18f);
            Add("reveal-back",()=>{Check(!ui.revealContinue.interactable&&ui.revealBack.activeSelf,"Reveal starts on shared back with Continue disabled");Check(ui.revealCard.localScale.x<1,"Reveal has the HTML wind-up scale");revealHeight=ui.dialogs.Single(d=>d.route=="DrawReveal").window.rect.height;Check(ui.revealResult.activeSelf&&ui.revealResult.GetComponent<CanvasGroup>().alpha==0,"Hidden result reserves layout space");},.65f);
            Add("reveal-turn",()=>{Check(Mathf.Abs(ui.revealCard.localRotation.eulerAngles.y)>1,"Card rotates during reveal");},.85f);
            Add("reveal-finish",()=>{},.35f);
            Add("reveal-settled",()=>{Check(ui.revealContinue.interactable&&ui.revealFront.activeSelf&&!ui.revealBack.activeSelf,"Reveal settles on front");Check(ui.revealCard.localRotation==Quaternion.identity&&ui.revealCard.localScale==Vector3.one,"Reveal restores settled transform");Check(ui.motion.revealGlow.alpha>0,"Settled card shows rarity glow");Check(Mathf.Abs(revealHeight-ui.dialogs.Single(d=>d.route=="DrawReveal").window.rect.height)<.01f,"Result appearance does not resize the reveal popup");ui.Handle("ContinueReveal","");},.12f);
            Add("reveal-early-close",()=>{ui.Close();Check(ui.CurrentTab=="Legacy"&&!ui.Engine.State.reveal.pending,"Early reveal close returns to collection");Check(Levels()==levels+5,"Five-card reveal grants exactly five once");ui.Open("Settings");ui.Handle("Setting","motion");},.2f);
            Add("reduced",()=>{Check(ui.motion.particles.All(x=>!x.gameObject.activeSelf),"Reduced motion clears effects");Check(ui.motion.motes.All(x=>x.color.a==0)&&ui.motion.lanterns.All(x=>x.color.a==0),"Reduced motion hides atmosphere");ui.Close();ui.Handle("Draw","5");},.15f);
            Add("reduced-reveal",()=>{Check(ui.revealContinue.interactable&&ui.revealCard.localRotation==Quaternion.identity,"Reduced reveal settles immediately");ui.Close();ui.Handle("Setting","motion");ui.SetTab("Home");},.6f);
            Add("focus-loss",()=>{ui.motion.Burst(ui.motion.tapHint,0,22);ui.motion.SendMessage("OnApplicationFocus",false);Check(ui.motion.particles.All(p=>!p.gameObject.activeSelf),"Losing focus cancels transient effects");mote=ui.motion.motes[2].rectTransform.anchoredPosition;},.2f);
            Add("focus-return",()=>{Check(mote==ui.motion.motes[2].rectTransform.anchoredPosition,"Losing focus pauses ambient time");ui.motion.SendMessage("OnApplicationFocus",true);},.2f);
            Add("final",()=>{var after=ui.GetComponentsInChildren<Component>(true).Select(c=>c.GetEntityId().ToString()).OrderBy(x=>x).ToArray();Check(identities.SequenceEqual(after),"Every UI/component identity survives transitions and effects");results.Add(ClassicImportQA.Audit());},.2f);
            due=0;Status="Running";EditorApplication.update-=Tick;EditorApplication.update+=Tick;return "Motion review started";
        }
        static void Tick()
        {
            if(!Application.isPlaying){End("Interrupted");return;}if(EditorApplication.timeSinceStartup<due)return;
            try
            {
                if(current!=null)ClassicImportQA.Capture(Path.Combine(folder,current.name+".png"));
                if(steps.Count==0){End("PASS");return;}current=steps.Dequeue();Status=current.name;current.action();due=EditorApplication.timeSinceStartup+current.wait;
            }
            catch(Exception ex){results.Add(ex.ToString());End("FAIL");}
        }
        static void End(string status){Status=status+" · "+checks+" checks";EditorApplication.update-=Tick;File.WriteAllLines(Path.Combine(folder,"motion-review.txt"),results.Concat(new[]{Status}));Debug.Log("CLASSIC_MOTION_REVIEW "+Status);}
    }
}
