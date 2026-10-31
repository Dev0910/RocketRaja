using System;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Projects durable tutorial receipts onto saved uGUI. Creates no UI or rewards.</summary>
    [DefaultExecutionOrder(80)]
    public sealed class ClassicTutorial : MonoBehaviour
    {
        public ClassicUI ui;
        public RectTransform safeArea, bubble, hand, tapBanner;
        public CanvasGroup bubbleGroup;
        public Button dismiss;
        public TMP_Text title, copy, objective;
        public GameObject objectiveRoot;
        public Button training, achievement, prestige, freePull;
        public TMP_Text helpStatus;
        public RectTransform Target { get; private set; }
        public bool Active => ui && ui.driver.Initialized && ui.Engine.TutorialActive;
        float observeAt;
        readonly Vector3[] corners=new Vector3[4];
        void Awake() { dismiss.onClick.AddListener(Dismiss); Hide(); }
        void OnDestroy() { if(dismiss)dismiss.onClick.RemoveListener(Dismiss); }
        void OnDisable() { Hide(); }
        void Hide() { if(bubble)bubble.gameObject.SetActive(false);if(hand)hand.gameObject.SetActive(false);Target=null; }
        bool Available => Active && ui.driver.Session.CanMutate && !ui.driver.NativeAdPresentation &&
            (ui.application==null || ui.application.Ready) && ui.CurrentDialog=="" && !ui.Engine.State.reveal.pending && !ui.Engine.State.offline.pending;
        void LateUpdate()
        {
            if(helpStatus && ui.Engine!=null)helpStatus.text=Active?"Guidance follows your real progress. You choose every purchase, claim and prestige. Skipping keeps all progress.":"Replay guidance at your current progress. Replaying never resets your stall or grants rewards.";
            if(!Available){Hide();return;}
            if(Time.unscaledTime>=observeAt) {
                observeAt=Time.unscaledTime+.1f;
                if(ui.Engine.CanContinueTutorialEarning &&
                    !ui.driver.Command(e=>e.BeginTutorialEarning())){Hide();return;}
                var next=ui.Engine.NextTutorialOffer();
                if(next!=TutorialNote.None && !ui.driver.Command(e=>e.OfferTutorial(next))){Hide();return;}
                var state=ui.Engine.State.tutorial;
                if(state.note!=TutorialNote.None && state.presented<state.serial) {
                    int serial=state.serial;
                    if(!ui.driver.Command(e=>e.PresentTutorial(serial))){Hide();return;}
                    Route(state.note);
                }
                RefreshCopy();
            }
            if(!Available){Hide();return;}
            var t=ui.Engine.State.tutorial;
            bool show=t.note!=TutorialNote.None && ui.CurrentTab==TabFor(t.note);
            bubble.gameObject.SetActive(show);
            // The approved coach appears without a separate modal transition.
            bubbleGroup.alpha=1;
            UpdatePointer();
            if(show && (Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape)))Dismiss();
        }
        void Dismiss()
        {
            if(!Available)return;int serial=ui.Engine.State.tutorial.serial;
            ui.driver.Command(e=>e.DismissTutorialNote(serial));
        }
        static string Cash(Amount amount)=>"₹"+amount;
        TutorialNote Goal()
        {
            var t=ui.Engine.State.tutorial;
            if(!t.Has(TutorialProgress.TrainingDone))return TutorialNote.Training;
            if(!t.Has(TutorialProgress.ProductDone))return TutorialNote.Product;
            if(!t.Has(TutorialProgress.UpgradeDone))return TutorialNote.Upgrade;
            if(t.Has(TutorialProgress.TalentOffered)&&!t.Has(TutorialProgress.TalentDone))return TutorialNote.Talent;
            if(t.Has(TutorialProgress.PrestigeOffered)&&ui.Engine.State.completedPrestiges.IsZero)return TutorialNote.Prestige;
            if(t.Has(TutorialProgress.AchievementOffered)&&!t.Has(TutorialProgress.AchievementDone))return TutorialNote.Achievement;
            return TutorialNote.None;
        }
        public void GoToGoal() {if(Available)Route(Goal());}
        public void RepeatTip()
        {
            if(!Available)return;
            var note=ui.Engine.State.tutorial.note;
            if(note!=TutorialNote.None){Route(note);return;}
            note=Goal();
            if(note==TutorialNote.None){ui.Open("TutorialHelp");return;}
            ui.driver.Command(e=>e.RepeatTutorialTip(note));
        }
        static string TabFor(TutorialNote note) => note==TutorialNote.Product?"Products":note==TutorialNote.Talent?"Legacy":
            note==TutorialNote.Training||note==TutorialNote.Upgrade||note==TutorialNote.Achievement||note==TutorialNote.Prestige?"Player":"Home";
        void Route(TutorialNote note)
        {
            string tab=TabFor(note);
            ui.SetTab(tab);
            var target=FindTarget(note);
            var panel=ui.panels.Single(v=>v.route==tab);
            if(target && panel.scroll) {
                Canvas.ForceUpdateCanvases();var scroll=panel.scroll;
                var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content,target);
                float overflow=scroll.content.rect.height-scroll.viewport.rect.height;
                if(overflow>0)scroll.verticalNormalizedPosition=1-Mathf.Clamp01((scroll.content.rect.yMax-bounds.center.y-scroll.viewport.rect.height*.5f)/overflow);
                scroll.StopMovement();
            }
        }
        RectTransform FindTarget(TutorialNote note)
        {
            switch(note) {
                case TutorialNote.Welcome:return ui.CurrentTab=="Home"?tapBanner:null;
                case TutorialNote.Training:return training.transform as RectTransform;
                case TutorialNote.Product:return ui.products[0].buy.transform as RectTransform;
                case TutorialNote.Upgrade:return ui.nextUpgrades.FirstOrDefault(r=>r.root.activeInHierarchy&&r.control.parameter==RedesignEngine.TutorialUpgrade)?.buy.transform as RectTransform;
                case TutorialNote.Achievement:return achievement.transform as RectTransform;
                case TutorialNote.Prestige:return prestige.transform as RectTransform;
                case TutorialNote.Talent:return !ui.Engine.State.freeCredits.IsZero && ui.Engine.CanDraw(true)?freePull.transform as RectTransform:null;
                default:return null;
            }
        }
        void RefreshCopy()
        {
            var e=ui.Engine;var s=e.State;var t=s.tutorial;var goal=Goal();
            if(ui.CurrentTab=="Home")objectiveRoot.SetActive(true);
            switch(goal) {
                case TutorialNote.Training:objective.text="Train Salesmanship · "+Cash(s.cash)+" / "+Cash(e.LevelQuote(-1,1).cost);break;
                case TutorialNote.Product:objective.text="Stock Phuljhari · "+Cash(s.cash)+" / "+Cash(e.LevelQuote(0,1).cost);break;
                case TutorialNote.Upgrade:objective.text="First Phuljhari upgrade · "+Cash(e.UpgradeCost(RedesignEngine.TutorialUpgrade));break;
                case TutorialNote.Achievement:objective.text="Your first achievement · view rewards";break;
                case TutorialNote.Prestige:objective.text="Your first prestige · review what stays";break;
                case TutorialNote.Talent:objective.text="Your free Talent pull · "+s.freeCredits+" credits";break;
                default:objective.text=t.Has(TutorialProgress.TalentDone)?"Your Talent stays through every prestige":"Grow your stall · check Player for your next goal";break;
            }
            switch(t.note) {
                case TutorialNote.Welcome:title.text="Let’s open your stall!";copy.text="Tap the counter to earn rupees. We’ll put your first earnings back into the shop.";break;
                case TutorialNote.Training:title.text=e.LevelQuote(-1,1).affordable?"You’ve earned "+Cash(e.LevelQuote(-1,1).cost)+"!":"Train your sales pitch";
                    copy.text="Train Salesmanship below for "+Cash(e.LevelQuote(-1,1).cost)+". Each tap will earn "+Cash(e.PreviewLevels(-1,1).tapAfter)+" instead of "+Cash(e.TapPayout())+".";break;
                case TutorialNote.TrainingResult:title.text="Your taps are stronger!";copy.text="Each tap now earns "+Cash(t.after)+" instead of "+Cash(t.before)+". Next, save for your first Phuljhari.";break;
                case TutorialNote.Product:title.text=e.LevelQuote(0,1).affordable?"You’ve earned "+Cash(e.LevelQuote(0,1).cost)+"!":"Your first Phuljhari";copy.text="Stock Phuljhari for "+Cash(e.LevelQuote(0,1).cost)+". It earns "+Cash(e.PreviewLevels(0,1).productAfter)+" every second.";break;
                case TutorialNote.ProductResult:title.text="Your first shelf is stocked!";copy.text="Phuljhari earns "+Cash(t.after)+" every second. Keep tapping while your stall earns automatically.";break;
                case TutorialNote.Upgrade:var p=e.PreviewUpgrade(RedesignEngine.TutorialUpgrade);title.text="A brighter Phuljhari!";copy.text=p==null?"Your first upgrade is already owned.":"Buy your first Phuljhari upgrade for "+Cash(p.price)+". Automatic sales improve from "+Cash(p.normalBefore)+" to "+Cash(p.normalAfter)+"/sec.";break;
                case TutorialNote.UpgradeResult:title.text="A stronger little stall!";copy.text="Automatic sales improved from "+Cash(t.before)+" to "+Cash(t.after)+"/sec. Your other tabs are always available.";break;
                case TutorialNote.Achievement:title.text="Your first achievement is ready!";copy.text="Open Achievements in Player to review and claim your Kaju Katli reward.";break;
                case TutorialNote.AchievementResult:title.text="Your reward is collected!";copy.text="You received "+(t.after-t.before)+" Kaju Katli. This reward stays with you through prestige.";break;
                case TutorialNote.Prestige:title.text="A brighter beginning awaits!";copy.text="View prestige in Player to review what resets and what stays. Begin again only when you choose.";break;
                case TutorialNote.Talent:title.text="Your legacy begins!";copy.text="You have "+s.freeCredits+" free Talent pulls. Try a 1× pull below. Your card stays through every prestige.";break;
                case TutorialNote.CardBenefit:
                    title.text=t.cardBefore.IsZero?"Your first Talent card!":"Every duplicate helps!";
                    if(e.Data.TryCard(t.cardId,out int i))copy.text=e.Data.Cards[i].name+" · "+(t.cardBefore.IsZero?RedesignCardCopy.Effect(e.Data.Cards[i],t.cardAfter):RedesignCardCopy.Effect(e.Data.Cards[i],t.cardBefore)+" → "+RedesignCardCopy.Effect(e.Data.Cards[i],t.cardAfter))+". This benefit stays through prestige.";break;
            }
        }
        void UpdatePointer()
        {
            var t=ui.Engine.State.tutorial;var goal=Goal();
            var note=t.note;
            if(note==TutorialNote.TrainingResult||note==TutorialNote.ProductResult||note==TutorialNote.UpgradeResult||note==TutorialNote.AchievementResult||note==TutorialNote.CardBenefit){hand.gameObject.SetActive(false);Target=null;return;}
            if(note==TutorialNote.None)note=goal;
            if(note==TutorialNote.Training && !ui.Engine.LevelQuote(-1,1).affordable || note==TutorialNote.Product && !ui.Engine.LevelQuote(0,1).affordable)note=ui.CurrentTab=="Home"?TutorialNote.Welcome:TutorialNote.None;
            Target=FindTarget(note);
            bool visible=Target && Target.gameObject.activeInHierarchy;
            var button=visible?Target.GetComponent<Button>():null;
            if(button && !button.IsInteractable())visible=false;
            if(visible) {
                foreach(var mask in Target.GetComponentsInParent<RectMask2D>()) {
                    Vector3 center=Target.TransformPoint(Target.rect.center);var point=mask.rectTransform.InverseTransformPoint(center);
                    if(!mask.rectTransform.rect.Contains(point))visible=false;
                }
            }
            hand.gameObject.SetActive(visible);if(!visible)return;
            var tip=((RectTransform)hand.parent).InverseTransformPoint(Target.TransformPoint(Target.rect.center));
            float size=Mathf.Clamp(safeArea.rect.width*.135f,44,64);
            Rect bounds=safeArea.rect;bounds.xMin+=4;bounds.xMax-=4;bounds.yMax-=4;
            var navigation=(RectTransform)ui.navigation[0].transform.parent;
            bounds.yMin=safeArea.InverseTransformPoint(navigation.TransformPoint(new Vector3(0,navigation.rect.yMax,0))).y+4;
            var safeTip=safeArea.InverseTransformPoint(Target.TransformPoint(Target.rect.center));
            if(!bounds.Contains(safeTip)){hand.gameObject.SetActive(false);return;}
            bool above=safeTip.y-bounds.yMin<size*.951356f;
            // The sprite's opaque fingertip is (0.205630, 0.048644) from its top-left.
            hand.localRotation=Quaternion.Euler(0,0,above?180:0);
            hand.localPosition=new Vector3(tip.x,tip.y,0);
            // Fit the largest pose, then pulse around the authored fingertip pivot.
            hand.localScale=Vector3.one;
            hand.sizeDelta=new Vector2(size,size);
            hand.GetWorldCorners(corners);float scale=1;
            foreach(var c in corners) {
                Vector3 local=safeArea.InverseTransformPoint(c);var delta=local-safeTip;
                if(delta.x>0)scale=Mathf.Min(scale,(bounds.xMax-safeTip.x)/delta.x);else if(delta.x<0)scale=Mathf.Min(scale,(bounds.xMin-safeTip.x)/delta.x);
                if(delta.y>0)scale=Mathf.Min(scale,(bounds.yMax-safeTip.y)/delta.y);else if(delta.y<0)scale=Mathf.Min(scale,(bounds.yMin-safeTip.y)/delta.y);
            }
            hand.sizeDelta*=Mathf.Clamp01(scale);
            float pulse=ui.Engine.State.reducedMotion?1:.94f+.06f*Mathf.Cos(Time.unscaledTime*Mathf.PI*2/1.2f);
            hand.localScale=Vector3.one*pulse;
        }
    }
}
