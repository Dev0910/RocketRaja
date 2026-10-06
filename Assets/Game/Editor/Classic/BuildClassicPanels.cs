using System.Collections.Generic;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        static void BuildShopWorld(RectTransform world)
        {
            Picture("",Fill("Painted Stall",world),art["stall"],false);
            var gear=Button(Region("Settings",world,.02f,.845f,.155f,.985f),"Navigate","Settings","","",blue);Picture("",Fill("Gear",gear.transform,8),Sprite("Icon Settings"));
            Label("cash",Region("Cash",world,.2f,.865f,.8f,.995f),"₹1.24M",36,true,new Color32(244,203,119,255),TextAlignmentOptions.Center);
            Label("",Region("Cash caption",world,.2f,.827f,.8f,.88f),"CASH BALANCE",10,true,Cream,TextAlignmentOptions.Center);
            Label("shop-name",Region("Shop sign",world,.24f,.675f,.76f,.76f),"DIWALI DUKAAN",16,true,Cream,TextAlignmentOptions.Center,heading);
            ui.shelfProducts=new GameObject[8];var products=new[]{"sparklers","anar","chakri","rockets","small-crackers","cracker-strings","aerial","gift-boxes"};
            for(int i=0;i<8;i++)
            {
                int col=i%4,line=i/4;float x=.087f+col*.2107f,top=.63f-line*.174f;
                var slot=Region("Shelf "+data.Products[i].name,world,x,top-.156f,x+.1907f,top);var hit=Button(slot,"Navigate","ProductDetail:"+i,"","",null);hit.targetGraphic.color=new Color(1,1,1,.001f);
                var merchandise=Region("Merchandise",slot,.06f,.23f,.94f,.98f);Picture("",merchandise,art["product-"+products[i]]);ui.shelfProducts[i]=merchandise.gameObject;
                Label("",Region("Nameplate",slot,0,.01f,1,.23f),data.Products[i].name,i==7?6:8,false,new Color32(57,41,28,255),TextAlignmentOptions.Center);
            }
            ui.motion.keeper=Region("Shopkeeper",world,.81f,.09f,.99f,.27f);Picture("",ui.motion.keeper,art["shopkeeper"]);
            Label("",Region("Counter message",world,.18f,.128f,.78f,.192f),"PATAAKHEIN, ROSHAN DIL",7,true,new Color32(246,213,144,255),TextAlignmentOptions.Center);
            var hint=Region("Tap invitation",world,.18f,.035f,.82f,.089f);Face(hint,new Color32(244,223,173,255),rounded);Label("",Fill("Label",hint),"TAP THE STALL TO EARN",9,true,Ink,TextAlignmentOptions.Center);
            var tap=Region("Counter tap surface",world,0,0,1,.24f);Face(tap,new Color(1,1,1,.001f),null,true);ui.tapTarget=tap.gameObject.AddComponent<RedesignTapTarget>();
        }
        static ClassicView BuildPanel(string tab,RectTransform parent)
        {
            var root=Fill(tab,parent);Face(root,Cream);var group=root.gameObject.AddComponent<CanvasGroup>();
            var result=new ClassicView{route=tab,root=root.gameObject,window=root,group=group};
            if(tab=="Home"){BuildHome(root);return result;}
            var head=Top("Header",root,48);Label("",Region("Title",head,.035f,0,.60f,1),tab,24,true,Ink,TextAlignmentOptions.Left,heading);
            var close=Button(Region("Collapse",head,.86f,0,1,1),"Tab",tab,"","⌄",ivory,23);
            Face(Bottom("Blue divider",head,2),Ink);
            if(tab=="Legacy"){Label("legacy-kk",Region("KK balance",head,.43f,0,.735f,1),"400 KK",17,true,Ink,TextAlignmentOptions.Right);Button(Region("Information",head,.73f,.08f,.86f,.92f),"Navigate","Odds","","i",ivory,17);}
            var region=Fill("Scroll",root);region.offsetMax=new Vector2(0,-49);result.scroll=Scroll(region,out var content);
            if(tab=="Player")BuildPlayer(content);if(tab=="Products")BuildProducts(content);if(tab=="Legacy")BuildLegacy(content);if(tab=="Shop")BuildStore(content);
            return result;
        }
        static void BuildHome(RectTransform root)
        {
            var next=Top("Next display",root,48,7);next.offsetMin=new Vector2(10,next.offsetMin.y);next.offsetMax=new Vector2(-10,next.offsetMax.y);Face(next,Cream,row);
            var button=next.gameObject.AddComponent<Button>();button.targetGraphic=next.GetComponent<Image>();button.navigation=new Navigation{mode=Navigation.Mode.None};var control=next.gameObject.AddComponent<ClassicControl>();control.owner=ui;control.action="Navigate";control.parameter="Products";
            Picture("",Region("Spark",next,.025f,.1f,.13f,.9f),Sprite("Burst"));Label("",Region("Kicker",next,.15f,.60f,.95f,.95f),"NEXT DISPLAY",8,true,Muted);Label("home-goal",Region("Goal",next,.15f,.12f,.95f,.61f),"Phuljhari · discover your next signature",12,true);
            var income=Bottom("Income",root,65,78);income.offsetMin=new Vector2(10,income.offsetMin.y);income.offsetMax=new Vector2(-10,income.offsetMax.y);Face(income,Cream,row);
            Picture("",Region("Coins",income,.02f,.15f,.105f,.85f),Sprite("Coins"));Label("",Region("Automatic sales",income,.13f,.7f,.60f,.95f),"AUTOMATIC SALES",8,true);Label("home-income",Region("Normal income",income,.13f,.36f,.6f,.73f),"₹1.7K",22,true);Label("",Region("Unit",income,.13f,.19f,.6f,.4f),"/ SECOND",8,true);Label("",Region("Caption",income,.13f,.025f,.6f,.22f),"Normal sales",8,false,Muted);
            Face(Region("Divider",income,.63f,.1f,.633f,.90f),new Color32(207,198,173,255));Label("",Region("Per tap",income,.67f,.65f,.97f,.95f),"PER TAP",8,true);Label("home-tap",Region("Tap income",income,.67f,.30f,.97f,.73f),"₹105.91",18,true);Label("",Region("Tap caption",income,.67f,.07f,.97f,.3f),"Counter taps",8,false,Muted);
            var skills=Bottom("Skills",root,74);ui.homeSkills=new Button[3];ui.homeSkillImages=new Image[3];ui.motion.skillGlows=new CanvasGroup[3];ui.skillArt=new[]{Sprite("Burst"),Sprite("Helping Hands"),Sprite("Bulk Order")};ui.lockArt=Sprite("Icon Lock");
            var names=new[]{"Festival Rush","Helping Hands","Bulk Order"};for(int i=0;i<3;i++)
            {
                var r=Region(names[i],skills,i/3f,0,(i+1)/3f,1);var target=Face(r,new Color(1,1,1,.001f),null,true);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=target;b.navigation=new Navigation{mode=Navigation.Mode.None};var c=r.gameObject.AddComponent<ClassicControl>();c.owner=ui;c.action="Navigate";c.parameter="SkillDetail:"+i;ui.homeSkills[i]=b;
                var glow=Fixed("Ready glow",r,33,0,54,54);Face(glow,new Color(1,.73f,.25f,.5f),rounded);ui.motion.skillGlows[i]=glow.gameObject.AddComponent<CanvasGroup>();
                var disc=Fixed("Skill disc",r,37,4,46,46);Face(disc,new Color32(23,76,123,255),rounded);ui.homeSkillImages[i]=Picture("",Fill("Artwork",disc,4),ui.skillArt[i]);
                Label("",Bottom("Name",r,17,9),names[i],10,true,Ink,TextAlignmentOptions.Center);Label("home-skill-"+i,Bottom("Status",r,12),"READY",8,true,Muted,TextAlignmentOptions.Center);
            }
        }
        static void BuildPlayer(RectTransform content)
        {
            var toolbar=New("Toolbar",content);Height(toolbar,48);Button(Region("Stats",toolbar,0,0,.23f,1),"Navigate","Statistics","","Stats",ivory);Button(Region("Skills",toolbar,.24f,0,.48f,1),"Navigate","Skills","","Skills",ivory);Button(Region("Quantity",toolbar,.66f,0,1,1),"Quantity","tap","tap-quantity","Buy ×1");
            var tapping=Row("Salesmanship",content);Picture("",Fixed("Icon",tapping,8,10,36,46),Sprite("Icon Player"));Label("tap-row-title",Region("Name",tapping,.15f,.5f,.69f,.9f),"Salesmanship · Level 12",12,true);Label("tap-row-income",Region("Income",tapping,.15f,.13f,.68f,.48f),"₹105.91 per tap",11);Button(Region("Train",tapping,.70f,.10f,.98f,.90f),"Navigate","TapTraining","tap-row-cost","₹300\nTrain tapping");
            var prestige=Row("A brighter beginning",content,76);Picture("",Fixed("Icon",prestige,8,12,36,42),Sprite("Icon Prestige"));Label("",Region("Title",prestige,.15f,.55f,.68f,.95f),"A brighter beginning",13,true);Label("fame",Region("Fame",prestige,.15f,.08f,.68f,.57f),"1 Fame · ×6 permanent income",10);Button(Region("View",prestige,.70f,.17f,.98f,.85f),"Navigate","Prestige","","View prestige");
            var header=New("Upgrades heading",content);Height(header,28);Label("",Region("Title",header,0,0,.6f,1),"Upgrades",19,true,Ink,TextAlignmentOptions.Left,heading);Label("upgrade-count",Region("Count",header,.6f,0,1,1),"4 / 1000",11,false,Muted,TextAlignmentOptions.Right);
            var buy=New("Buy all upgrades",content);Height(buy,48);Button(buy,"BuyAll","","","Buy all Upgrades");ui.nextUpgrades=Enumerable.Range(0,10).Select(i=>Upgrade(content,i,"Next upgrade")).ToArray();
            var empty=New("Empty",content);Height(empty,28);Label("upgrade-empty",Fill("Label",empty),"",11,false,Muted);
        }
        static ClassicUpgradeRow Upgrade(RectTransform content,int i,string prefix)
        {
            var r=Row(prefix+" "+(i+1),content,70);var title=Label("",Region("Title",r,.025f,.57f,.70f,.95f),"Next available upgrade",12,true);var copy=Label("",Region("Benefit",r,.025f,.08f,.70f,.57f),"Income improvement",10);var button=Button(Region("Buy",r,.72f,.12f,.98f,.89f),"BuyUpgrade","","","Buy upgrade");
            return new ClassicUpgradeRow{root=r.gameObject,title=title,copy=copy,buy=button,cost=button.GetComponentInChildren<TMP_Text>(),control=button.GetComponent<ClassicControl>()};
        }
        static void BuildProducts(RectTransform content)
        {
            var toolbar=New("Toolbar",content);Height(toolbar,48);Label("product-total",Region("Total income",toolbar,0,0,.68f,1),"Automatic sales · ₹1.7K/s",12,true);Button(Region("Quantity",toolbar,.7f,0,1,1),"Quantity","products","quantity","Buy ×1");
            ui.products=new ClassicProductRow[8];var keys=new[]{"sparklers","anar","chakri","rockets","small-crackers","cracker-strings","aerial","gift-boxes"};for(int i=0;i<8;i++)
            {
                var r=Row(data.Products[i].name,content,80);var detail=Region("Details",r,.02f,.06f,.59f,.94f);Face(detail,new Color(1,1,1,.001f),null,true);var b=detail.gameObject.AddComponent<Button>();b.targetGraphic=detail.GetComponent<Image>();b.navigation=new Navigation{mode=Navigation.Mode.None};var c=detail.gameObject.AddComponent<ClassicControl>();c.owner=ui;c.action="Navigate";c.parameter="ProductDetail:"+i;
                var frame=Fixed("Product frame",detail,0,3,54,62);Face(frame,new Color32(229,212,176,255),rounded);Picture("",Fill("Product",frame,4),art["product-"+keys[i]]);var bar=Bottom("Milestone track",frame,7);Face(bar,new Color32(216,203,176,255));var fill=Face(Fill("Fill",bar),new Color32(50,127,120,255));fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillAmount=i==0?0:.5f;
                var name=Label("",Region("Name",detail,.32f,.50f,1,.92f),data.Products[i].name,12,true);var income=Label("",Region("Income",detail,.32f,.14f,1,.50f),"₹0/s",11,true);
                var level=Label("",Region("Level",r,.58f,.35f,.70f,.7f),"Lvl : 25",9,true,Ink,TextAlignmentOptions.Right);var buy=Button(Region("Upgrade",r,.71f,.13f,.98f,.87f),"BuyLevels",i.ToString(),"","₹300\nUpgrade ×1");
                ui.products[i]=new ClassicProductRow{name=name,income=income,level=level,buy=buy,cost=buy.GetComponentInChildren<TMP_Text>(),progress=fill};
            }
        }
        static void BuildLegacy(RectTransform content)
        {
            var pulls=New("Draw buttons",content);Height(pulls,56);Button(Region("One pull",pulls,0,0,.49f,1),"Draw","1","draw-one","1× pull\n1 free pull");Button(Region("Five pulls",pulls,.51f,0,1,1),"Draw","5","draw-five","5× pull\n4 free pulls");
            var state=New("Availability",content);Height(state,14);Label("legacy-state",Fill("Message",state),"",10,false,Muted,TextAlignmentOptions.Center);
            ui.cards=new ClassicCardRow[28];for(int tier=0;tier<3;tier++)
            {
                var header=New("Rarity "+tier,content);Height(header,28);Label("",Region("Title",header,0,0,.43f,1),new[]{"Common","Rare","Legendary"}[tier],19,true,Ink,TextAlignmentOptions.Left,heading);Label("discovered-"+tier,Region("Count",header,.43f,0,1,1),"4 / 28 discovered",10,false,Muted,TextAlignmentOptions.Right);
                var indices=data.Cards.Select((c,i)=>new{c,i}).Where(x=>(int)x.c.tier==tier).Select(x=>x.i).ToArray();int rows=(indices.Length+3)/4;
                var grid=New("Card grid",content);Height(grid,rows*120+(rows-1)*7);for(int j=0;j<indices.Length;j++)
                {
                    int i=indices[j];var r=Region(data.Cards[i].name,grid,(j%4)*.25f,1f-(j/4+1f)/rows,(j%4+1)*.25f,1f-(j/4f-(j%4)/4f)/rows);r.offsetMin=new Vector2(2,3);r.offsetMax=new Vector2(-2,-3);
                    var frame=Picture("",Fill("Folk frame",r),art[new[]{"folk-common","folk-rare","folk-legendary"}[tier]],false);frame.raycastTarget=true;var button=r.gameObject.AddComponent<Button>();button.targetGraphic=frame;button.transition=Selectable.Transition.None;button.navigation=new Navigation{mode=Navigation.Mode.None};var control=r.gameObject.AddComponent<ClassicControl>();control.owner=ui;control.action="Card";control.parameter=i.ToString();
                    var image=Picture("",Region("Artwork",r,.13f,.40f,.87f,.87f),art[data.Cards[i].id]);var badge=Region("Level badge",r,.28f,.28f,.72f,.40f);Face(badge,Ink,rounded);var level=Label("",Fill("Level",badge),"Lvl 1",9,true,Cream,TextAlignmentOptions.Center);var name=Label("",Region("Name",r,.12f,.08f,.88f,.26f),data.Cards[i].name,9,true,Ink,TextAlignmentOptions.Center,serif);var cg=r.gameObject.AddComponent<CanvasGroup>();
                    ui.cards[i]=new ClassicCardRow{button=button,frame=frame,art=image,level=level,name=name,group=cg};
                }
            }
        }
        static void BuildStore(RectTransform content)
        {
            var balance=New("Balance",content);Height(balance,58);Label("",Region("Kicker",balance,0,.62f,.5f,1),"KAJU KATLI",9,true);Label("shop-kk",Region("KK",balance,0,0,.5f,.68f),"400 KK",24,true);Button(Region("Collection",balance,.56f,.10f,1,.93f),"Navigate","Legacy","","Your collection",ivory);
            var status=New("Store status",content);Height(status,32);Label("shop-status",Fill("Message",status),"",10,false,Muted);
            for(int i=0;i<data.Packs.Count;i++){var p=data.Packs[i];var r=Row(p.name,content,84);Picture("",Fixed("Pack",r,7,12,46,58),art["product-gift-boxes"]);Label("",Region("Name",r,.18f,.56f,.68f,.95f),p.name,14,true);Label("",Region("Contents",r,.18f,.12f,.68f,.58f),p.kajuKatli+" KK\n"+(p.kajuKatli/100)+" Talent draws",12,true);Button(Region("Price",r,.72f,.15f,.98f,.86f),"Purchase",i.ToString(),"","₹"+p.designInr+"\nView pack");}
            Info(content,"Card draws","","One draw costs 100 KK.\nCommon 75% · Rare 22% · Legendary 3%.",85);
            var odds=New("Odds",content);Height(odds,48);Button(odds,"Navigate","Odds","","See card odds",ivory);
            var note=New("Store note",content);Height(note,43);Label("",Fill("Label",note),"Isolated Editor preview · no real payment\nFinal prices come from your app store.",10,false,Muted,TextAlignmentOptions.Center);
        }
        static void BuildNavigation(RectTransform navigation)
        {
            ui.navigation=new Button[4];var tabs=new[]{"Player","Products","Legacy","Shop"};var icons=new[]{"Icon Player","Icon Products","Icon Legacy","Coins"};
            for(int i=0;i<4;i++){var rect=Region(tabs[i],navigation,i*.25f,0,(i+1)*.25f,1);rect.offsetMin=new Vector2(4,3);rect.offsetMax=new Vector2(-3,-3);ui.navigation[i]=Button(rect,"Tab",tabs[i],"","",blue);Picture("",Region("Icon",rect,.25f,.32f,.75f,.95f),Sprite(icons[i]));Label("",Region("Label",rect,0,.02f,1,.32f),tabs[i],10,true,Cream,TextAlignmentOptions.Center);}
        }
    }
}
