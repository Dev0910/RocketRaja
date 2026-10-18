using System;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        static ClassicView Dialog(Transform parent,string route,string title,out RectTransform content,int footerButtons=1,bool fullCard=false)
        {
            var layer=Fill(route,parent);var dim=Face(layer,new Color32(16,23,42,201),null,true);var group=layer.gameObject.AddComponent<CanvasGroup>();
            var outside=Fill("Outside dismissal",layer);var outsideButton=outside.gameObject.AddComponent<Button>();outsideButton.targetGraphic=Face(outside,new Color(0,0,0,.001f),null,true);outsideButton.transition=Selectable.Transition.None;outsideButton.navigation=new Navigation{mode=Navigation.Mode.None};var outsideControl=outside.gameObject.AddComponent<ClassicControl>();outsideControl.owner=ui;outsideControl.action="Close";outsideControl.parameter="";outsideButton.interactable=fullCard;
            var area=Fill("Safe dialog bounds",layer,fullCard?18:12);area.offsetMin=new Vector2(fullCard?18:12,fullCard?32:18);area.offsetMax=new Vector2(fullCard?-18:-12,fullCard?-32:-18);
            var window=Fill("Window",area);window.gameObject.AddComponent<CanvasGroup>();Face(window,Cream,rounded,true);
            if(fullCard){var fit=window.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=.65f;content=window;}
            else
            {
                var header=Top("Header",window,58);Face(Bottom("Rule",header,2),Ink);Label(route=="ProductDetail"?"product-title":"",Region("Title",header,.04f,.1f,.8f,.95f),title,23,true,Ink,TextAlignmentOptions.Left,heading);
                if(route!="SaveRecovery"&&route!="RecoveryExit")Button(Region("Close",header,.82f,.08f,.98f,.91f),"Close","","","×",blue,25);
                var bodyRect=Fill("Body",window);bodyRect.offsetMin=new Vector2(0,footerButtons==0?8:footerButtons*54+12);bodyRect.offsetMax=new Vector2(0,-59);
                var scroll=Scroll(bodyRect,out content);
            }
            var v=new ClassicView{route=route,root=layer.gameObject,window=window,group=group,scroll=window.GetComponentInChildren<ScrollRect>()};views.Add(v);return v;
        }
        static Button Footer(ClassicView view,int index,string action,string param,string key,string title,bool secondary=false)
        {
            var rect=Bottom(title,view.window,48,10+index*54);rect.offsetMin=new Vector2(12,rect.offsetMin.y);rect.offsetMax=new Vector2(-12,rect.offsetMax.y);return Button(rect,action,param,key,title,secondary?ivory:blue);
        }
        static Image Progress(Transform parent,string name,string labelKey,string imageKey)
        {
            var r=New(name,parent);Height(r,29);Face(r,new Color32(36,78,87,255));var inset=Fill("Track",r,3);Face(inset,new Color32(69,105,106,255));var fill=Face(Fill("Fill",inset),new Color32(69,183,173,255));ConfigureProgressFill(fill);fill.fillAmount=.45f;images.Add(new ClassicImageBinding{key=imageKey,image=fill});Label(labelKey,Fill("Progress label",r,6),"Level",12,true,Cream,TextAlignmentOptions.Right);return fill;
        }
        static void BuildDialogs(RectTransform root)
        {
            var host=Fill("Local dialogs",root);
            var product=Dialog(host,"ProductDetail","Phuljhari",out var body,0);var identity=Row("Product identity",body,116);Picture("product-detail-art",Fixed("Artwork",identity,8,9,86,96),art["product-sparklers"]);Label("product-detail-name",Region("Name",identity,.31f,.62f,.96f,.94f),"Phuljhari",23,true,Ink,TextAlignmentOptions.Left,heading);Label("product-detail-level",Region("Level",identity,.31f,.32f,.96f,.65f),"Lvl : 25",20,true);Label("product-subtitle",Region("Description",identity,.31f,.07f,.96f,.30f),"Sparklers",11,true,Muted);
            var earnings=New("Income",body);Height(earnings,45);Label("product-earnings",Fill("Label",earnings),"Earns ₹0 / second",19,true,new Color32(65,115,53,255),TextAlignmentOptions.Center,heading);
            var milestone=New("Milestone",body);Height(milestone,37);Label("product-milestone",Fill("Heading",milestone),"Next improvement at Lvl 10",20,true,Muted,TextAlignmentOptions.Center,heading);Progress(body,"Milestone progress","product-progress-label","product-progress");Info(body,"Milestone reward","","Milestones apply automatically. Buy cash upgrades for boosts, then use Max to level up.",86);
            var synergy=New("Synergy heading",body);Height(synergy,36);Label("synergy-title",Fill("Label",synergy),"Next synergy bonus",20,true,Muted,TextAlignmentOptions.Center,heading);Progress(body,"Synergy progress","synergy-progress-label","synergy-progress");var reward=New("Reward",body);Height(reward,30);Label("synergy-reward",Fill("Label",reward),"",14,true,Muted,TextAlignmentOptions.Center);Info(body,"Your next bonus","synergy-copy","",125);
            var tap=Dialog(host,"TapTraining","Salesmanship",out body,2);Info(body,"Train your tapping","tap-detail","Level 12 → 13\nPer tap ₹105.91 → ₹110",120);var qty=New("Buy quantity",body);Height(qty,48);Button(qty,"Quantity","tap","","Cycle quantity: 1 / 10 / 100 / Max",ivory);Info(body,"Growing your shop","","Training improves manual taps and Helping Hands. Tap milestones multiply profit at 10, 25, 50, 100 and every 100 levels.",112);Footer(tap,1,"BuyLevels","-1","tap-buy","Train tapping");Footer(tap,0,"Close","","","Back",true);
            var skills=Dialog(host,"Skills","Your skills",out body);for(int i=0;i<3;i++){var r=Row(new[]{"Festival Rush","Helping Hands","Bulk Order"}[i],body,80);Picture("",Fixed("Icon",r,7,12,45,48),ui.skillArt[i]);Label("",Region("Name",r,.18f,.5f,.65f,.95f),new[]{"Festival Rush","Helping Hands","Bulk Order"}[i],14,true);Label("skills-state-"+i,Region("State",r,.18f,.12f,.65f,.5f),"READY",10);Button(Region("Details",r,.69f,.15f,.98f,.85f),"Navigate","SkillDetail:"+i,"","View skill");}Info(body,"This run","","Skills reset on prestige. Permanent Talent bonuses apply after you buy the base skill again.",100);Footer(skills,0,"Close","","","Back",true);
            var skill=Dialog(host,"SkillDetail","Your skill",out body,2);var hero=Row("Skill identity",body,90);Picture("skill-art",Fixed("Artwork",hero,8,8,65,74),ui.skillArt[0]);Label("skill-title",Region("Name",hero,.28f,.46f,.98f,.95f),"Festival Rush",22,true,Ink,TextAlignmentOptions.Left,heading);Label("skill-state",Region("State",hero,.28f,.06f,.98f,.5f),"READY",12,true);Info(body,"What it does","skill-copy","",140);Info(body,"Timing & improvements","skill-timing","",125);Footer(skill,1,"ActivateSkill","","skill-activate","Activate skill");Footer(skill,0,"Navigate","Upgrades","","View skill upgrades",true);
            BuildFullCard(host);BuildReveal(host);BuildCatalogue(host);
            var prestige=Dialog(host,"Prestige","A brighter beginning",out body,2);var amount=New("Reward",body);Height(amount,60);Label("prestige-reward",Fill("Label",amount),"+1 Fame",32,true,new Color32(168,33,73,255),TextAlignmentOptions.Center,heading);Info(body,"Your next beginning","prestige-copy","",155);Info(body,"Resets","","Run cash · product and tap levels\nCash upgrades · skills and active effects",110);Info(body,"Stays","","Fame · Talent · KK · free draws\nLifetime discoveries · decorations",110);Footer(prestige,1,"Prestige","","","Begin again");Footer(prestige,0,"Close","","","Keep growing",true);
            var settings=Dialog(host,"Settings","Settings",out body);for(int i=0;i<3;i++){string key=new[]{"music","sound","motion"}[i];var r=Row(key,body,76);Label("",Region("Title",r,.03f,.45f,.65f,.95f),new[]{"Music","Sound effects","Reduced motion"}[i],18,true,Ink,TextAlignmentOptions.Left,heading);Label("",Region("Copy",r,.03f,.10f,.65f,.45f),new[]{"Festival atmosphere","Taps and celebrations","Settle effects instantly"}[i],11,false,Muted);Button(Region("Toggle",r,.69f,.17f,.97f,.83f),"Setting",key,"setting-"+key,"ON",ivory);}
            foreach(var n in new[]{"Naming","Account","Privacy","Help"}){var r=New(n,body);Height(r,48);Button(r,"Navigate",n,"",new[]{"Shop name","Account & cloud save","Privacy","How to play"}[Array.IndexOf(new[]{"Naming","Account","Privacy","Help"},n)],ivory);}Footer(settings,0,"Close","","","Back",true);
            var naming=Dialog(host,"Naming","Make it your own",out body,2);Info(body,"Name your shop","","A name for your little corner of the festival.",85);var field=New("Shop name",body);Height(field,52);ui.nameInput=Input(field,"Your shop name");ui.nameInput.characterLimit=24;Footer(naming,1,"Rename","","","Save shop name");Footer(naming,0,"Close","","","Back",true);
            var stats=Dialog(host,"Statistics","Your shop in numbers",out body);Info(body,"This run","stats-run","",205);Info(body,"Your whole journey","stats-lifetime","",205);Footer(stats,0,"Close","","","Back",true);
            var offline=Dialog(host,"Offline","Welcome back",out body,2);var money=New("Credited reward",body);Height(money,60);Label("offline-amount",Fill("Amount",money),"₹0",32,true,Ink,TextAlignmentOptions.Center,heading);Info(body,"While you were away","offline-copy","",180);Info(body,"Your next step","","Check product levels or purchase an upgrade. Closing keeps the credited earnings.",110);Footer(offline,1,"Offline","goal","","View your next step");Footer(offline,0,"Offline","close","","Continue to your shop",true);
            var odds=Dialog(host,"Odds","Every card has a chance",out body);Info(body,"Same pool for every draw","","Common · 75% / 16 = 4.6875% each\nRare · 22% / 10 = 2.2% each\nLegendary · 3% / 2 = 1.5% each",150);Info(body,"Every duplicate helps","","Cards apply automatically. A duplicate adds one level. Free and KK pulls use the same pool; no pack guarantees a rarity.",125);Footer(odds,0,"Close","","","Back",true);
            var purchase=Dialog(host,"Purchase","Kaju Katli pack",out body);var packHero=Row("Pack",body,95);Picture("",Fixed("Artwork",packHero,8,8,72,78),art["product-gift-boxes"]);Label("purchase-name",Region("Name",packHero,.30f,.5f,.97f,.95f),"Festival pack",22,true,Ink,TextAlignmentOptions.Left,heading);Label("purchase-amount",Region("Contents",packHero,.30f,.10f,.97f,.5f),"1,000 Kaju Katli",13,true);Info(body,"A treat for your Talent","purchase-price","₹99 · example price",80);Info(body,"Development preview","","These Editor-only outcomes exercise the saved purchase lifecycle. No real payment, receipt or server entitlement is created.",120);foreach(var outcome in new[]{"delivered","pending","cancelled","failed"}){var r=New(outcome,body);Height(r,48);Button(r,"PurchaseResult",outcome,"","Preview "+outcome+" purchase",outcome=="delivered"?blue:ivory);}Footer(purchase,0,"Close","","","Back",true);
            var buyAll=Dialog(host,"BuyAll","Review your purchases",out body,2);Info(body,"Only the reviewed upgrades","batch-copy","",600);Footer(buyAll,1,"ConfirmBatch","","","Buy reviewed upgrades");Footer(buyAll,0,"Close","","","Back",true);
            BuildUtilityDialogs(host);
        }
        static void BuildFullCard(Transform host)
        {
            var v=Dialog(host,"CardDetail","",out var card,0,true);Picture("full-outer",Fill("Painted outer border",card),art["full-outer-border"],false);
            Label("full-name",Region("Card name",card,.09f,.835f,.72f,.915f),"Phuljhari Showcase",21,true,Ink,TextAlignmentOptions.Left,serif);
            var level=Region("Level badge",card,.76f,.835f,.92f,.912f);Face(level,Ink,rounded);Label("full-level",Fill("Level",level,2),"Lvl 1",15,true,Cream,TextAlignmentOptions.Center);
            Label("full-rarity",Region("Rarity",card,.10f,.79f,.90f,.84f),"· COMMON ·",10,true,Ink,TextAlignmentOptions.Center);
            var illustration=Region("Illustration",card,.095f,.257f,.905f,.784f);Picture("full-art",Fill("Artwork",illustration,18),art["C01"]);Picture("full-inner",Fill("Painted inner border",illustration),art["full-inner-border"],false);
            Label("full-benefit",Region("Benefit",card,.11f,.17f,.89f,.265f),"Product income ×1.4",18,true,new Color32(48,99,46,255),TextAlignmentOptions.Center,serif);
            Face(Region("Divider",card,.15f,.151f,.85f,.153f),new Color32(163,43,73,255));Label("full-flavor",Region("Flavor",card,.10f,.075f,.90f,.15f),"A brighter display. A busier shop.",12,false,Ink,TextAlignmentOptions.Center,serif);Label("",Region("Permanent",card,.12f,.03f,.88f,.071f),"· Permanent bonus ·",9,true,new Color32(158,126,74,255),TextAlignmentOptions.Center);
        }
        static void BuildReveal(Transform host)
        {
            var v=Dialog(host,"DrawReveal","A little festival magic",out var body,1);
            var progress=New("Progress",body);Height(progress,24);Label("reveal-progress",Fill("Label",progress),"Your card awaits",12,true,Ink,TextAlignmentOptions.Center);
            var stage=New("Flip stage",body);Height(stage,250);var card=Region("Rotating card",stage,.2f,0,.8f,1);ui.revealCard=card;
            ui.revealBack=Fill("Shared back",card).gameObject;Picture("",(RectTransform)ui.revealBack.transform,art["shared-card-back"],false);
            var front=Fill("Card front",card);ui.revealFront=front.gameObject;Picture("reveal-frame",Fill("Folk frame",front),art["folk-common"],false);Picture("reveal-art",Region("Artwork",front,.15f,.31f,.85f,.82f),art["C01"]);Label("reveal-level",Region("Level",front,.53f,.77f,.88f,.90f),"Lvl 1",11,true,Ink,TextAlignmentOptions.Center);Label("reveal-name",Region("Name",front,.14f,.08f,.86f,.25f),"Phuljhari Showcase",15,true,Ink,TextAlignmentOptions.Center,serif);
            var result=New("Result",body);Height(result,105);ui.revealResult=result.gameObject;Label("reveal-rarity",Region("Rarity",result,0,.72f,1,1),"Common",10,true,Muted,TextAlignmentOptions.Center);Label("reveal-result",Region("Outcome",result,0,.35f,1,.74f),"New card discovered!",23,true,Ink,TextAlignmentOptions.Center,heading);Label("reveal-effect",Region("Benefit",result,0,0,1,.36f),"Product income ×1.4",12,false,Ink,TextAlignmentOptions.Center);ui.revealContinue=Footer(v,0,"ContinueReveal","","reveal-continue","Continue");
        }
        static void BuildCatalogue(Transform host)
        {
            var v=Dialog(host,"Upgrades","Cash upgrades",out var content,1);var search=New("Search",content);Height(search,48);ui.searchInput=Input(search,"Search all 1,000 upgrades…");
            var filters=New("Filters",content);Height(filters,48);Button(Region("Category",filters,0,0,.49f,1),"Filter","category","catalogue-category","All",ivory);Button(Region("View",filters,.51f,0,1,1),"Filter","view","catalogue-view","Available",ivory);
            var count=New("Count",content);Height(count,31);Label("catalogue-count",Fill("Label",count),"1,000 matching",10);ui.catalogueRows=Enumerable.Range(0,20).Select(i=>Upgrade(content,i,"Catalogue row")).ToArray();
            var empty=New("Empty",content);Height(empty,28);Label("catalogue-empty",Fill("Label",empty),"",11,false,Muted);
            var pages=New("Pagination",content);Height(pages,48);Button(Region("Previous",pages,0,0,.36f,1),"Page","previous","","Previous",ivory);Label("catalogue-page",Region("Page",pages,.37f,0,.63f,1),"1 / 50",11,true,Ink,TextAlignmentOptions.Center);Button(Region("Next",pages,.64f,0,1,1),"Page","next","","Next",ivory);Footer(v,0,"Close","","","Back",true);
        }
        static void BuildUtilityDialogs(Transform host)
        {
            var help=Dialog(host,"Help","A little guidance",out var body);Info(body,"Start with a spark","","Manually stock Phuljhari in Products. Products earn automatically; tap the counter on Home for extra cash.",135);Info(body,"Grow your way","","Player contains tap training and cash upgrades. Products grows the eight families. Legacy contains Talent. Shop contains Kaju Katli packs.",135);Info(body,"Begin again, stronger","","Prestige resets the run while keeping Fame, Talent, KK, free credits, discoveries and decorations.",120);Footer(help,0,"Close","","","Back",true);
            var privacy=Dialog(host,"Privacy","Your privacy",out body);Info(body,"Your progress","","Guest games stay on your device. Google sync is optional and uses a separate cloud game.",125);Info(body,"Purchases & information","","Store purchases must be verified before currency is delivered. This importer does not contact authentication, payment or analytics services.",145);Info(body,"Your experience","","Music, sound and reduced motion are available in Settings.",90);Footer(privacy,0,"Close","","","Back",true);
            var account=Dialog(host,"Account","Account & saves",out body);Info(body,"Playing as a guest","","This isolated importer uses temporary test progress. Production sign-in and real save selection require the existing account service.",145);foreach(var pair in new[]{("signin","Continue with Google"),("signout","Sign out to guest")}){var r=New(pair.Item1,body);Height(r,48);Button(r,"Account",pair.Item1,"",pair.Item2,ivory);}foreach(var route in new[]{"AccountCompare","Handoff","SaveRecovery"}){var r=New(route,body);Height(r,48);Button(r,"Navigate",route,"",route=="AccountCompare"?"Review saved games":route=="Handoff"?"Play on this device":"Save recovery options",ivory);}Footer(account,0,"Close","","","Back",true);
            var welcome=Dialog(host,"Welcome","Welcome to Rocket Raja",out body,2);Info(body,"Your festival starts here","","Build a little fireworks shop into a lasting legacy.",160);Footer(welcome,1,"Account","signin","","Continue with Google");Footer(welcome,0,"Account","guest","","Play as guest",true);
            var compare=Dialog(host,"AccountCompare","Choose your progress",out body,2);Info(body,"On this device","","The original guest save stays separate and safe.",100);Info(body,"Google game unavailable","","Connect the production account service to retrieve a real cloud save. The importer never invents a remote save or overwrites an account.",155);Footer(compare,1,"Account","cloud","","Use Google game");Footer(compare,0,"Account","guest","","Keep this guest",true);
            foreach(var route in new[]{"Handoff","SignOut","Promote","RecoveryConfirm"}){var v=Dialog(host,route,route=="Handoff"?"Play on this device?":route=="SignOut"?"Return to your guest game?":route=="Promote"?"Start your Google game":"Keep your existing progress",out body,2);Info(body,"Separate saved journeys","","Your existing files are retained. Real account and recovery transitions belong to the production save service; this scene uses temporary test progress.",190);Footer(v,1,"Account",route=="Handoff"?"handoff":route=="SignOut"?"confirm-signout":"fresh","",route=="SignOut"?"Return to guest":"Continue");Footer(v,0,"Close","","","Cancel",true);}
            var recovery=Dialog(host,"SaveRecovery","Your progress needs attention",out body,0);Info(body,"We could not complete saving","","Keep existing files while retrying. Purchases remain blocked until the session can save safely.",155);foreach(var x in new[]{("Retry","","Try saving again"),("Navigate","Account","Account options"),("Navigate","RecoveryConfirm","Review recovery"),("RecoveryExit","","Keep files and leave")}){var r=New(x.Item3,body);Height(r,48);Button(r,x.Item1,x.Item2,"",x.Item3,ivory);}
            var exit=Dialog(host,"RecoveryExit","Your files are safe",out body,1);Info(body,"Existing progress is retained","","The isolated preview remains open. Return to welcome to continue reviewing the UI.",145);Footer(exit,0,"Navigate","Welcome","","Return to welcome");
            var appearance=Dialog(host,"Appearance","Your shop’s journey",out body);Info(body,"Eight familiar shelves","","Your products occupy their painted shelf compartments. Talent, Fame and lifetime discoveries persist through prestige.",140);Footer(appearance,0,"Close","","","Back",true);
        }
        static void BuildEffects(RectTransform root,RectTransform world)
        {
            var environment=Fill("Festival atmosphere",world);ui.motion.motes=new Image[12];for(int i=0;i<12;i++){var r=New("Mote "+i,environment);r.anchorMin=r.anchorMax=new Vector2((5+i*37%90)/100f,(8+i*19%65)/100f);r.sizeDelta=new Vector2(3,3);ui.motion.motes[i]=Face(r,new Color(1,.9f,.63f,.3f),rounded);}
            ui.motion.lanterns=new Image[2];for(int i=0;i<2;i++)ui.motion.lanterns[i]=Face(Region("Lantern glow "+i,environment,i==0?0:.9f,.55f,i==0?.1f:1,.72f),new Color(1,.72f,.2f,.10f),rounded);
            environment.SetSiblingIndex(1);var pool=Fill("Effects - fixed pool",root);ui.motion.effectRoot=pool;ui.motion.particles=new Image[64];for(int i=0;i<64;i++){var r=New("Spark "+i,pool);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(i%3==0?8:4,i%3==0?8:6);ui.motion.particles[i]=Face(r,new Color(1,.84f,.47f),rounded);r.gameObject.SetActive(false);}
        }
        static void BuildToast(RectTransform safe)
        {
            var toast=Bottom("Toast",safe,42,78);toast.offsetMin=new Vector2(17,toast.offsetMin.y);toast.offsetMax=new Vector2(-17,toast.offsetMax.y);Face(toast,new Color32(23,59,71,255),rounded);toast.gameObject.AddComponent<CanvasGroup>();Label("toast",Fill("Message",toast,7),"",12,true,Cream,TextAlignmentOptions.Center);ui.toast=toast.gameObject;toast.SetAsLastSibling();
        }
    }
}
