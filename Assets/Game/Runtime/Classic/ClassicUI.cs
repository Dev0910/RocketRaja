using System;
using System.Collections.Generic;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    [Serializable] public sealed class ClassicTextBinding { public string key; public TMP_Text text; }
    [Serializable] public sealed class ClassicImageBinding { public string key; public Image image; }
    [Serializable] public sealed class ClassicView { public string route; public GameObject root; public RectTransform window; public CanvasGroup group; public ScrollRect scroll; }
    [Serializable] public sealed class ClassicProductRow { public TMP_Text name,level,income,cost; public Button buy; public Image progress; public RectTransform artFrame; }
    [Serializable] public sealed class ClassicCardRow { public TMP_Text name,level; public Button button; public Image frame,art; public CanvasGroup group; }
    [Serializable] public sealed class ClassicUpgradeRow { public GameObject root; public TMP_Text title,copy,cost; public Button buy; public ClassicControl control; }

    /// <summary>Bindings and navigation for saved native Classic screens. Owns no UI factories.</summary>
    public sealed partial class ClassicUI : MonoBehaviour
    {
        public RedesignRuntimeDriver driver;
        public ClassicMotion motion;
        public ClassicView[] panels,dialogs;
        public ClassicTextBinding[] textBindings;
        public ClassicImageBinding[] imageBindings;
        public ClassicProductRow[] products;
        public ClassicCardRow[] cards;
        public ClassicUpgradeRow[] nextUpgrades,catalogueRows;
        public Sprite[] productArt,cardArt,miniFrames,outerFrames,innerFrames;
        public Sprite blueButton,ivoryButton,disabledButton,redButton;
        public Button[] navigation,homeSkills;
        public GameObject[] shelfProducts;
        public Image[] homeSkillImages;
        public Sprite[] skillArt;
        public Sprite lockArt;
        public Material undiscoveredMaterial;
        public TMP_InputField nameInput,searchInput;
        public GameObject toast,revealFront,revealBack,revealResult;
        public RectTransform revealCard;
        public Button revealContinue;
        public RedesignTapTarget tapTarget;
        public bool isolatedPreview=true;
        public ClassicApplication application;
        public TMP_Text recoveryMessage;
        public ClassicGuidance guidance;
        public ClassicProgression progression;
        public RedesignAudio audioFeedback;
        public string CurrentTab { get; private set; }="Home";
        public string CurrentDialog => stack.Count==0?"":stack[stack.Count-1].route;
        public RedesignEngine Engine => driver.Session?.Engine;
        readonly Dictionary<string,TMP_Text> texts=new Dictionary<string,TMP_Text>();
        readonly Dictionary<string,Image> images=new Dictionary<string,Image>();
        readonly List<ClassicView> stack=new List<ClassicView>();
        readonly List<ClassicControl> controls=new List<ClassicControl>();
        int quantity=int.MaxValue,tapQuantity=int.MaxValue,product,skill,selectedCard,pack,page;
        string search="",view="Available",category="All",lastRevision="";
        float refreshAt;bool closing,initialized;
        CashBatchQuote batch;FameQuote prestige;ClassicDrawQuote five;TalentDrawQuote one;
        static readonly string[] SkillNames={"Festival Rush","Helping Hands","Bulk Order"};
        static readonly int[] Quantities={1,10,100,int.MaxValue};
        int LevelQuantity(int index) => index < -1 || index >= 8 ? 1 : (index < 0 ? Engine.State.tapLevel : Engine.State.products[index]) == 0 ? 1 : index < 0 ? tapQuantity : quantity;
        void Awake()
        {
            if(!driver||!motion||products.Length!=8||cards.Length!=28||nextUpgrades.Length!=10||catalogueRows.Length!=20)throw new InvalidOperationException("Classic UI is missing its Editor-authored bindings");
            foreach(var b in textBindings)texts.Add(b.key,b.text);foreach(var b in imageBindings)images.Add(b.key,b.image);
            controls.AddRange(GetComponentsInChildren<ClassicControl>(true));
            tapTarget.Tap=Tap;searchInput.onValueChanged.AddListener(Search);
        }
        void Start(){if(driver.Initialized)Bind();}
        public void Bind()
        {
            if(initialized)return;if(!driver.Initialized)throw new InvalidOperationException("Bind Classic after its session initializes");initialized=true;driver.Changed+=OnChanged;Refresh();
            if(!application){if(Engine.State.reveal.pending)Open("DrawReveal");else if(Engine.State.offline.pending)Open("Offline");}
        }
        void OnDestroy(){if(driver)driver.Changed-=OnChanged;if(searchInput)searchInput.onValueChanged.RemoveListener(Search);if(tapTarget)tapTarget.Tap=null;}
        void OnChanged(){if(initialized)Refresh();}
        void Update()
        {
            if(!initialized)return;if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+.2f;Refresh();}
            if(Input.GetKeyDown(KeyCode.Escape)){if(stack.Count>0)Close();else SetTab("Home");}
        }
        void Text(string key,string value){if(texts.TryGetValue(key,out var t)&&t.text!=value)t.text=value;}
        void Picture(string key,Sprite value){if(images.TryGetValue(key,out var image))image.sprite=value;}
        static string Cash(Amount value)=>"₹"+value;
        static string Count(Natural n)=>RedesignCardCopy.Count(n);
        void Enable(string action,string parameter,bool enabled)
        {
            foreach(var control in controls)if(control.action==action&&control.parameter==parameter){var b=control.GetComponent<Button>();b.interactable=enabled;if(b.targetGraphic is Image img&&control.action!="Card")img.sprite=enabled?(action=="Draw"&&parameter=="5"?redButton:blueButton):disabledButton;}
        }
        // Transient banners are disabled; retain this entry point for existing callers.
        public void Notice(string message){if(toast)toast.SetActive(false);}
        bool Command(Func<RedesignEngine,bool> action){if(!driver.Command(action)){Notice(driver.RecoveryNotice??"This action is unavailable. Your progress has not changed.");return false;}return true;}
        public void SetTab(string tab)
        {
            if (!driver.Initialized || !driver.Session.CanMutate) return;
            if(!panels.Any(v=>v.route==tab))return;CloseAll();CurrentTab=tab;
            foreach(var v in panels){motion.ResetView(v.root.transform);v.root.SetActive(v.route==tab);}
            for(int i=0;i<navigation.Length;i++)((Image)navigation[i].targetGraphic).sprite=blueButton;
            tapTarget.gameObject.SetActive(true);driver.Session.GameplayInputEnabled=true;motion.dialogOpen=false;
            var active=panels.First(v=>v.route==tab);Refresh();motion.Enter((RectTransform)active.root.transform,0,14);
            motion.Content(active.scroll?(RectTransform)active.scroll.transform:(RectTransform)active.root.transform);
            for(int i=0;i<navigation.Length;i++)if(panels[i+1].route==tab)motion.Pulse((RectTransform)navigation[i].transform.Find("Icon"));
        }
        public void Open(string route)
        {
            if (application && IsAccountRoute(route)) { application.OpenAccount(); return; }
            if (!driver.Initialized || !driver.Session.CanMutate) return;
            var parts=route.Split(':');route=parts[0];if(parts.Length>1&&int.TryParse(parts[1],out int value)){if(route=="ProductDetail")product=value;if(route=="SkillDetail")skill=value;if(route=="CardDetail")selectedCard=value;}
            if(panels.Any(v=>v.route==route)){SetTab(route);return;}
            var next=dialogs.FirstOrDefault(v=>v.route==route);if(next==null){Notice("This view is unavailable.");return;}
            if(stack.Contains(next)){while(stack.Count>0&&stack[stack.Count-1]!=next){stack[stack.Count-1].root.SetActive(false);stack.RemoveAt(stack.Count-1);}next.root.SetActive(true);RefreshDialog();return;}
            if(stack.Count>0){motion.ResetView(stack[stack.Count-1].root.transform);stack[stack.Count-1].root.SetActive(false);}stack.Add(next);next.root.SetActive(true);closing=false;
            next.group.interactable=next.group.blocksRaycasts=true;motion.dialogOpen=true;driver.Session.GameplayInputEnabled=false;
            if(route=="Prestige")prestige=Engine.PreviewPrestige();if(route=="Naming")nameInput.SetTextWithoutNotify(string.IsNullOrEmpty(Engine.State.storeName)?"DIWALI DUKAAN":Engine.State.storeName);
            RefreshDialog();motion.Enter(next.window,0,20,.33f,.96f);motion.Backdrop(next.root.GetComponent<Image>());
            if(next.scroll)motion.Content((RectTransform)next.scroll.transform);
            if(route=="CardDetail")motion.Enter(images["full-art"].rectTransform,0,0,.45f,.96f,.3f);
            if(route=="DrawReveal")StartReveal();
            if(guidance)guidance.Refresh();
        }
        void CloseAll(){closing=false;motion.SettleFlip();foreach(var v in stack){motion.ResetView(v.root.transform);v.root.SetActive(false);v.group.alpha=1;}stack.Clear();motion.dialogOpen=false;}
        static bool IsAccountRoute(string route) => route == "Account" || route == "Welcome" || route == "AccountCompare" || route == "Handoff" || route == "SignOut" || route == "Promote" || route == "RecoveryConfirm";
        public void InvalidateQuotes() { lastRevision = ""; batch = null; prestige = null; five = null; one = null; Refresh(); }
        public void ShowRecovery(string message)
        {
            if (recoveryMessage) recoveryMessage.text = message;
            if (CurrentDialog != "SaveRecovery")
            {
                CloseAll();
                var view = dialogs.First(v => v.route == "SaveRecovery");
                stack.Add(view); view.root.SetActive(true); view.group.alpha = 1;
                view.group.interactable = view.group.blocksRaycasts = true;
            }
            motion.dialogOpen = true;
            if (driver.Session != null) driver.Session.GameplayInputEnabled = false;
        }
        public void ReturnFromRecovery() { CloseAll(); SetTab("Home"); }
        public void Close()
        {
            if (!driver.Initialized || !driver.Session.CanMutate) return;
            if(closing||stack.Count==0)return;
            if(CurrentDialog=="SaveRecovery"||CurrentDialog=="RecoveryExit")return;
            if(CurrentDialog=="DrawReveal")
            {
                if(!Command(e=>e.DismissReveal(e.State.reveal.operationId)))return;motion.SettleFlip();CloseAll();SetTab("Legacy");return;
            }
            var top=stack[stack.Count-1];closing=true;top.group.interactable=false;
            motion.Backdrop(top.root.GetComponent<Image>(),true);
            motion.Exit(top.window,()=>{if(!this)return;top.root.SetActive(false);stack.Remove(top);closing=false;if(stack.Count>0){stack[stack.Count-1].root.SetActive(true);RefreshDialog();}motion.dialogOpen=stack.Count>0;driver.Session.GameplayInputEnabled=stack.Count==0;});
        }
        void Search(string value){search=value;page=0;RefreshCatalogue();}
        void Tap()
        {
            if (!initialized || !driver.Initialized || !driver.Session.CanMutate) return;
            if(stack.Count>0)return;if(!driver.TapGameplaySurface())return;
            if(audioFeedback)audioFeedback.Feedback("tap");
            var origin=tapTarget.LastTapWorldPosition??tapTarget.transform.position;
            motion.Burst(origin,0,7);motion.Coins(origin,texts["cash"].rectTransform);motion.Pulse(texts["cash"].rectTransform);motion.Pulse(motion.tapHint);
        }
        public void Handle(string action,string arg)
        {
            if (application)
            {
                if (action == "Retry") { application.Retry(); return; }
                if (action == "Account" || action == "Navigate" && IsAccountRoute(arg)) { application.OpenAccount(); return; }
                if (action == "RecoveryExit") { GameView.BackgroundTask(); return; }
            }
            if(!initialized||closing)return;
            if (!driver.Session.CanMutate) return;
            string revision=Engine.State.commandRevision.digits;
            switch(action)
            {
                case "Tip":if(guidance)guidance.Open();break;
                case "LearnTip":if(guidance)guidance.Acknowledge();break;
                case "Tab":SetTab(CurrentTab==arg?"Home":arg);break;
                case "Navigate":Open(arg);break;
                case "Close":Close();break;
                case "Card":if(int.TryParse(arg,out selectedCard)&&!Engine.State.cardLevels[selectedCard].IsZero)Open("CardDetail");break;
                case "Quantity":if(arg=="tap")tapQuantity=Quantities[(Array.IndexOf(Quantities,tapQuantity)+1)%4];else quantity=Quantities[(Array.IndexOf(Quantities,quantity)+1)%4];Refresh();break;
                case "BuyLevels":{int i=int.Parse(arg);bool fresh=Engine.State.products[0]==0;if(Command(e=>e.BuyLevels(i,LevelQuantity(i)))){if(i>=0){motion.Success(products[i].artFrame);motion.Pulse((RectTransform)shelfProducts[i].transform);}else motion.Success(texts["tap-row-cost"].rectTransform);Notice("Purchase complete");if(fresh&&i==0)SetTab("Home");}}break;
                case "BuyUpgrade":if(Command(e=>e.BuyUpgrade(arg))){Notice("Upgrade purchased");motion.Success(texts["upgrade-count"].rectTransform);}break;
                case "BuyAll":batch=Engine.QuoteBuyAll();if(batch.Ids.Count>0&&Command(e=>e.CommitBuyAll(batch))){Notice(batch.Ids.Count+" upgrades purchased");motion.Success(texts["upgrade-count"].rectTransform,0,22);}break;
                case "ReviewBatch":batch=Engine.QuoteBuyAll(new UpgradeScope{search=search});Open("BuyAll");break;
                case "ConfirmBatch":if(batch!=null&&Command(e=>e.CommitBuyAll(batch))){Close();Notice("Reviewed upgrades purchased");motion.Success(texts["upgrade-count"].rectTransform,0,22);}break;
                case "ActivateSkill":if(Command(e=>e.ActivateSkill(skill))){Notice(SkillNames[skill]+" activated");motion.Success(texts["skill-title"].rectTransform,1,22);}break;
                case "Draw":Draw(int.Parse(arg));break;
                case "ContinueReveal":ContinueReveal();break;
                case "Prestige":if(prestige!=null&&Command(e=>e.Prestige(prestige))){CloseAll();SetTab("Home");motion.Success(texts["shop-name"].rectTransform,2,22);Notice("A brighter beginning!");}break;
                case "Setting":Command(e=>e.ToggleSetting(arg));break;
                case "Rename":if(Command(e=>e.SetStoreName(nameInput.text))){Close();Notice("Shop name updated");}break;
                case "Offline":if(Command(e=>e.DismissOffline())){Close();if(arg=="goal")SetTab("Products");}break;
                case "Page":page=Math.Max(0,page+(arg=="next"?1:-1));RefreshCatalogue();break;
                case "Filter":if(arg=="view")view=view=="Available"?"Upcoming":view=="Upcoming"?"Owned":view=="Owned"?"Full":"Available";else category=category=="All"?"Products":category=="Products"?"Tapping":category=="Tapping"?"Skills":category=="Skills"?"Shop":"All";page=0;RefreshCatalogue();break;
                case "Purchase":if(!isolatedPreview){Notice("Store purchases are not available yet. No payment was started.");break;}pack=int.Parse(arg);Open("Purchase");break;
                case "PurchaseResult":PurchaseOutcome(arg);break;
                case "Retry":if(driver.RetrySaving()){CloseAll();SetTab("Home");Notice("Saving recovered");}else Notice(driver.RecoveryNotice??"Still unavailable. Your saved progress is retained.");break;
                case "Account":AccountAction(arg);break;
                case "RecoveryExit":CloseAll();Open("RecoveryExit");break;
            }
            if(audioFeedback && revision!=Engine.State.commandRevision.digits)
            {
                string cue=action=="Prestige"?"prestige":action=="ActivateSkill"?"skill":action=="Draw"||action=="ContinueReveal"&&CurrentDialog=="DrawReveal"?"reveal":action=="BuyLevels"||action=="BuyUpgrade"||action=="BuyAll"||action=="ConfirmBatch"?"purchase":null;
                if(cue!=null)audioFeedback.Feedback(cue);
            }
            Refresh();
        }
        void Draw(int count)
        {
            bool success;
            if(count==5){if(five==null||!Engine.CanCommitDrawFive(five))five=Engine.QuoteDrawFive();success=five!=null&&Command(e=>e.DrawFive(five));if(success)five=null;}
            else{bool credit=!Engine.State.freeCredits.IsZero;if(one==null||one.UseCredit!=credit||!Engine.CanCommitDraw(one))one=Engine.QuoteDraw(credit);success=one!=null&&Command(e=>e.Draw(one));if(success)one=null;}
            if(success)Open("DrawReveal");else Notice("Not enough resources, or a saved card result is still pending.");
        }
        void StartReveal()
        {
            revealContinue.interactable=false;motion.ResetView(revealResult.transform);revealResult.SetActive(true);var resultGroup=revealResult.GetComponent<CanvasGroup>();resultGroup.alpha=0;resultGroup.interactable=resultGroup.blocksRaycasts=false;RefreshDialog();
            if(motion.revealGlowImage&&Engine.Data.TryCard(Engine.State.reveal.cardId,out int cardIndex))motion.revealGlowImage.color=(int)Engine.Data.Cards[cardIndex].tier==2?new Color32(237,186,75,255):(int)Engine.Data.Cards[cardIndex].tier==1?new Color32(201,121,173,255):new Color32(98,158,180,255);
            motion.Reveal(revealCard,revealFront,revealBack,()=>{revealContinue.interactable=true;resultGroup.alpha=1;motion.Enter((RectTransform)revealResult.transform,0,7,.25f);if(Engine.Data.TryCard(Engine.State.reveal.cardId,out int i))motion.Burst(revealCard,(int)Engine.Data.Cards[i].tier,22);});
        }
        void ContinueReveal()
        {
            if(!revealContinue.interactable)return;var r=Engine.State.reveal;
            if(r.batch!=null&&r.batchIndex+1<r.batch.Length){if(Command(e=>e.AdvanceClassicReveal(r.operationId)))StartReveal();}else Close();
        }
        void PurchaseOutcome(string result)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(!isolatedPreview||!driver.IsDisposableGuest){Notice("Store purchases are not available yet.");return;}
            if(!Engine.State.HasDevelopmentShop&&!Command(e=>e.EnableDevelopmentShop(DevelopmentShopState.Namespace)))return;
            string id=Engine.State.developmentShop.status=="pending"?Engine.State.developmentShop.operationId:null;
            if(id==null&&!Command(e=>{id=e.BeginDevelopmentPurchase(e.Data.Packs[pack].id);return id!=null;}))return;
            if(result=="pending"){CloseAll();SetTab("Shop");Notice("Development purchase pending. No currency granted.");return;}
            var outcome=result=="delivered"?DevelopmentPurchaseOutcome.Delivered:result=="cancelled"?DevelopmentPurchaseOutcome.Cancelled:DevelopmentPurchaseOutcome.Failed;
            if(Command(e=>e.ResolveDevelopmentPurchase(id,outcome))){CloseAll();SetTab("Shop");Notice("Development purchase "+result+". No real payment.");if(result=="delivered")motion.Success((RectTransform)texts["shop-kk"].transform.parent,0,22);}
#else
            Notice("Store verification is unavailable in this importer scene.");
#endif
        }
        void AccountAction(string action)
        {
            if(action=="guest"){CloseAll();SetTab("Home");return;}
            if(action=="signin")Open("AccountCompare");else if(action=="signout")Open("SignOut");else if(action=="confirm-signout"){CloseAll();SetTab("Home");}
            else Notice("Account services are unavailable in the isolated importer. Your real account and saves are unchanged.");
        }
    }
}
