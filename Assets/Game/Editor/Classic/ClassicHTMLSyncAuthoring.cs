using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        const string SyncArt="Assets/Game/ClassicUI/HTMLSync/";
        static Sprite popupFrame, ringSprite;
        static readonly Color Gold=new Color32(198,152,67,255);
        static readonly List<ClassicSkillClock> clocks=new List<ClassicSkillClock>();

        // Scoped migration, never invoked by runtime startup. Original hierarchy remains saved.
        public static string SyncReviewedHTML()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var scene=SceneManager.GetSceneByPath(ScenePath);
            if(!scene.isLoaded)throw new InvalidOperationException("Open Main additively first");
            if(scene.isDirty)throw new InvalidOperationException("Preserve dirty Main before migration");
            SceneManager.SetActiveScene(scene);
            var current=scene.GetRootGameObjects().Single(g=>g.name==RootName);
            if(current.transform.Find("HTML sync revision"))throw new InvalidOperationException("Migration already saved; use a scoped refinement");
            var legacy=Object.Instantiate(current);legacy.name=RootName+"__LegacyDisabled_20261019";legacy.SetActive(false);
            Undo.RegisterCreatedObjectUndo(legacy,"Preserve complete pre-sync Classic UI");
            ui=current.GetComponent<ClassicUI>();
            texts.Clear();texts.AddRange(ui.textBindings);images.Clear();images.AddRange(ui.imageBindings);views.Clear();views.AddRange(ui.dialogs);clocks.Clear();
            body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Body SDF.asset");bold=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Bold SDF.asset");
            heading=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Heading SDF.asset");serif=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Card Serif SDF.asset");
            blue=ui.blueButton;ivory=ui.ivoryButton;sand=ui.disabledButton;row=Sprite("Ivory Row Frame");rounded=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            art.Clear();foreach(var a in JsonUtility.FromJson<ArtManifest>(File.ReadAllText(Local+"art-manifest.json")).assets)art[a.key]=AssetDatabase.LoadAssetAtPath<Sprite>(a.path);
            ImportSyncArt();
            SyncHome();SyncPlayer();SyncSkills();SyncHero();SyncSettings();
            var exact=Dialog(ui.transform.Find("Local dialogs"),"Exact","Your amounts",out var exactBody);
            Info(exactBody,"Current game values","exact-values","",160);Footer(exact,0,"Close","","","Back",true);
            foreach(var v in views)RefreshPopup(v);
            AddIllustrations();SyncAppearance();
            var batch=views.Single(v=>v.route=="BuyAll");var refresh=New("Refresh review",batch.scroll.content);Height(refresh,48);Button(refresh,"RefreshBatch","","","Refresh review",ivory);refresh.SetAsFirstSibling();
            var card=views.Single(v=>v.route=="CardDetail");var close=Region("Close",card.window,.84f,.92f,.98f,1);Button(close,"Close","","","×",blue,22);
            var cash=ui.textBindings.Single(t=>t.key=="cash").text;
            if(!cash.GetComponent<Button>()){cash.raycastTarget=true;var b=cash.gameObject.AddComponent<Button>();b.targetGraphic=cash;b.transition=Selectable.Transition.None;var c=cash.gameObject.AddComponent<ClassicControl>();c.owner=ui;c.action="Navigate";c.parameter="Exact";}
            // Privacy describes the actual production owner, never a browser/importer fixture.
            var privacy=views.Single(v=>v.route=="Privacy").scroll.content;
            privacy.Find("Purchases & information/Body").GetComponent<TMP_Text>().text="Store purchases require verification before currency is delivered. Google sign-in and cloud progress use the existing account service. Store purchases are currently unavailable.";
            ui.textBindings=texts.ToArray();ui.imageBindings=images.ToArray();ui.dialogs=views.ToArray();ui.skillClocks=clocks.ToArray();
            ui.guidance.rows=ui.guidance.rows.Where(r=>r.root).ToArray();
            var layout=ui.GetComponent<ClassicLayout>();
            layout.infoRows=ui.GetComponentsInChildren<LayoutElement>(true).Where(e=>e.transform.Find("Heading")&&e.transform.Find("Body")&&e.transform.Find("Body").GetComponent<TMP_Text>()).Select(e=>new ClassicInfoFit{layout=e,copy=e.transform.Find("Body").GetComponent<TMP_Text>()}).ToArray();
            layout.touchGraphics=ui.GetComponentsInChildren<Button>(true).Select(b=>b.targetGraphic).Where(g=>g).ToArray();
            foreach(var v in views)v.root.SetActive(false);
            var marker=New("HTML sync revision",ui.transform);marker.gameObject.SetActive(false);
            ClassicPanelControlsAuthoring.ApplyToUI(ui);
            foreach(var v in views)ClassicPopupClearanceAuthoring.ApplyView(v);
            Canvas.ForceUpdateCanvases();layout.Apply();
            EditorUtility.SetDirty(ui);EditorUtility.SetDirty(layout);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Saved "+views.Count+" dialogs, "+clocks.Count+" authoritative skill clock views; original hierarchy preserved inactive";
        }
        static void ImportSyncArt()
        {
            Directory.CreateDirectory(SyncArt);
            foreach(var name in new[]{"frame","music","sound","motion","shop","cloud","shield","book"})
            {
                string path=SyncArt+name+".png";File.Copy("UI Builder/HTML/Classic/assets/popup-refresh/"+name+".png",path,true);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
                importer.spriteBorder=name=="frame"?Vector4.one*144:Vector4.zero;importer.SaveAndReimport();
            }
            popupFrame=AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt+"frame.png");
            // A transparent annulus is a single editable radial Filled Image, not a timer animation.
            var texture=new Texture2D(256,256,TextureFormat.RGBA32,false);
            for(int y=0;y<256;y++)for(int x=0;x<256;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(128,128));float alpha=Mathf.Clamp01(128-d)*Mathf.Clamp01(d-115);texture.SetPixel(x,y,new Color(1,1,1,alpha));}
            texture.Apply();File.WriteAllBytes(SyncArt+"ring.png",texture.EncodeToPNG());Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SyncArt+"ring.png",ImportAssetOptions.ForceSynchronousImport);var ri=(TextureImporter)AssetImporter.GetAtPath(SyncArt+"ring.png");ri.textureType=TextureImporterType.Sprite;ri.spriteImportMode=SpriteImportMode.Single;ri.mipmapEnabled=false;ri.alphaIsTransparency=true;ri.textureCompression=TextureImporterCompression.Uncompressed;ri.SaveAndReimport();ringSprite=AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt+"ring.png");
        }
        static void Put(RectTransform rect,float x0,float y0,float x1,float y1){rect.anchorMin=new Vector2(x0,y0);rect.anchorMax=new Vector2(x1,y1);rect.offsetMin=rect.offsetMax=Vector2.zero;}
        static void SyncHome()
        {
            var shop=ui.transform.Find("SafeArea/Shop");shop.Find("Tap invitation").gameObject.SetActive(false);
            var sign=shop.Find("Shop sign").GetComponent<TMP_Text>();Put(sign.rectTransform,.24f,.675f,.76f,.76f);sign.alignment=TextAlignmentOptions.Center;sign.margin=Vector4.zero;
            var home=ui.panels.Single(v=>v.route=="Home").root.transform;
            var income=home.Find("Income");
            foreach(var n in new[]{"Coins","Unit","Caption","Tap caption"})income.Find(n).gameObject.SetActive(false);
            Put((RectTransform)income.Find("Automatic sales"),.035f,.73f,.53f,.94f);Put((RectTransform)income.Find("Normal income"),.035f,.51f,.53f,.76f);
            Put((RectTransform)income.Find("Per tap"),.035f,.32f,.53f,.53f);Put((RectTransform)income.Find("Tap income"),.035f,.07f,.53f,.34f);
            income.Find("Normal income").GetComponent<TMP_Text>().fontSize=15;income.Find("Tap income").GetComponent<TMP_Text>().fontSize=15;
            Put((RectTransform)income.Find("Divider"),.54f,.08f,.542f,.92f);
            var chart=Region("Rising earnings chart",income,.58f,.57f,.65f,.85f);
            for(int i=0;i<3;i++)Face(Region("Bar "+i,chart,i*.31f,0,i*.31f+.23f,.3f+i*.25f),new Color32(230,165,37,255));
            Label("",Region("Rising arrow",chart,0,.5f,1,1.1f),"↗",15,true,Gold,TextAlignmentOptions.Center);
            Label("",Region("Earning now",income,.67f,.57f,.98f,.87f),"EARNING NOW",7,true);
            Label("home-total",Region("Total earnings",income,.58f,.12f,.98f,.58f),"₹0/sec",21,true);
            for(int i=0;i<3;i++)
            {
                var b=ui.homeSkills[i];var c=b.GetComponent<ClassicControl>();c.action="ActivateSkill";c.parameter=i.ToString();
                var disc=(RectTransform)ui.homeSkillImages[i].transform.parent;
                // Center the circular artwork using anchors so changing viewport widths stays symmetric.
                disc.anchorMin=disc.anchorMax=new Vector2(.5f,1);disc.pivot=new Vector2(.5f,1);disc.anchoredPosition=new Vector2(0,-4);
                clocks.Add(Clock(i,disc,ui.homeSkillImages[i],ui.textBindings.Single(t=>t.key=="home-skill-"+i).text,b));
            }
        }
        static ClassicSkillClock Clock(int index,RectTransform disc,Image artwork,TMP_Text status,Button activate)
        {
            var ring=Picture("",Fill("Duration and cooldown ring",disc,-3),ringSprite);ring.type=Image.Type.Filled;ring.fillMethod=Image.FillMethod.Radial360;ring.fillOrigin=(int)Image.Origin360.Top;ring.fillClockwise=true;ring.color=new Color32(255,208,97,255);
            return new ClassicSkillClock{index=index,ring=ring,artwork=artwork,status=status,activate=activate};
        }
        static void SyncPlayer()
        {
            var player=ui.panels.Single(v=>v.route=="Player");
            foreach(var name in new[]{"Stats","Skills"}){var b=player.scroll.content.Find("Toolbar/"+name).GetComponent<Button>();var image=(Image)b.targetGraphic;image.sprite=null;image.color=Color.clear;b.transition=Selectable.Transition.None;}
            foreach(var r in ui.nextUpgrades.Concat(ui.catalogueRows))
            {
                var rect=(RectTransform)r.buy.transform;rect.anchorMax=new Vector2(1,rect.anchorMax.y);rect.offsetMax=new Vector2(-10,rect.offsetMax.y);
                r.title.rectTransform.anchorMax=new Vector2(.69f,r.title.rectTransform.anchorMax.y);r.copy.rectTransform.anchorMax=new Vector2(.69f,r.copy.rectTransform.anchorMax.y);
            }
        }
        static void ClearChildren(Transform parent)
        {
            var removed=parent.GetComponentsInChildren<TMP_Text>(true);texts.RemoveAll(b=>removed.Contains(b.text));
            var pictures=parent.GetComponentsInChildren<Image>(true);images.RemoveAll(b=>pictures.Contains(b.image));
            for(int i=parent.childCount-1;i>=0;i--)Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
        static void NoFooter(ClassicView view)
        {
            foreach(var c in view.window.GetComponentsInChildren<ClassicControl>(true).Where(c=>c.transform.parent==view.window).ToArray())
            {
                var labels=c.GetComponentsInChildren<TMP_Text>(true);texts.RemoveAll(b=>labels.Contains(b.text));Object.DestroyImmediate(c.gameObject);
            }
            var scroll=(RectTransform)view.scroll.transform;scroll.offsetMin=new Vector2(6,10);
        }
        static void SyncSkills()
        {
            var v=views.Single(d=>d.route=="Skills");NoFooter(v);ClearChildren(v.scroll.content);
            var subtitle=New("Subtitle",v.scroll.content);Height(subtitle,24);Label("",Fill("Copy",subtitle),"Your skills for this run",11,false,Muted,TextAlignmentOptions.Center);
            for(int i=0;i<3;i++)
            {
                var r=Row("Skill "+i,v.scroll.content,113);var disc=Fixed("Portrait",r,9,24,49,49);Face(disc,Ink,AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));var picture=Picture("",Fill("Artwork",disc,6),ui.skillArt[i]);
                Label("",Region("Name",r,.21f,.76f,.72f,.96f),new[]{"Festival Rush","Helping Hands","Bulk Order"}[i],14,true);
                var status=Label("skills-state-"+i,Region("State",r,.21f,.59f,.72f,.77f),"READY",8,true,Gold);
                var effect=Label("",Region("Effect",r,.21f,.25f,.72f,.60f),"",10);var timing=Label("",Region("Timing",r,.21f,.04f,.72f,.27f),"",8,false,Muted);
                var activate=Button(Region("Activate",r,.74f,.53f,.98f,.96f),"ActivateSkill",i.ToString(),"","Activate",blue,10);
                var details=Button(Region("Details",r,.74f,.04f,.98f,.47f),"Navigate","SkillDetail:"+i,"","Details",ivory,10);
                var clock=Clock(i,disc,picture,status,activate);clock.effect=effect;clock.timing=timing;clocks.Add(clock);
            }
            var note=New("Prestige note",v.scroll.content);Height(note,36);Label("",Fill("Copy",note,3),"Skills reset on prestige. Permanent Talent bonuses return when you buy the base skill again.",9,false,Muted,TextAlignmentOptions.Center);
        }
        static void SyncHero()
        {
            var v=views.Single(d=>d.route=="SkillDetail");NoFooter(v);ClearChildren(v.scroll.content);
            var identity=New("Hero artwork",v.scroll.content);Height(identity,164);
            var disc=New("Portrait",identity);disc.anchorMin=disc.anchorMax=new Vector2(.5f,.6f);disc.sizeDelta=new Vector2(122,122);Face(disc,Ink,AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));
            var picture=Picture("skill-art",Fill("Artwork",disc,10),ui.skillArt[0]);var status=Label("skill-state",Bottom("Status",identity,25),"READY",12,true,Gold,TextAlignmentOptions.Center);clocks.Add(Clock(-1,disc,picture,status,null));
            var name=New("Name",v.scroll.content);Height(name,34);Label("skill-title",Fill("Title",name),"Festival Rush",25,true,Ink,TextAlignmentOptions.Center,serif);
            var tiles=New("Effect panels",v.scroll.content);Height(tiles,106);
            for(int i=0;i<2;i++){var tile=Region("Effect "+i,tiles,i*.51f,0,i*.51f+.49f,1);Face(tile,Cream,row);Picture("",Region("Icon",tile,.39f,.65f,.61f,.94f),i==0?Sprite("Coins"):Sprite("Icon Player"));Label("skill-effect-"+(i==0?"a":"b"),Region("Value",tile,.04f,.31f,.96f,.68f),"",22,true,Ink,TextAlignmentOptions.Center);Label("skill-effect-"+(i==0?"a-copy":"b-copy"),Region("Description",tile,.04f,.04f,.96f,.34f),"",10,false,Muted,TextAlignmentOptions.Center);}
            var timing=Row("Timing",v.scroll.content,50);
            for(int i=0;i<2;i++){Label("",Region("Heading "+i,timing,i*.5f,.59f,(i+1)*.5f,.96f),i==0?"DURATION":"COOLDOWN",8,true,Muted,TextAlignmentOptions.Center);Label(i==0?"skill-duration":"skill-cooldown",Region("Value "+i,timing,i*.5f,.06f,(i+1)*.5f,.61f),"",20,true,Ink,TextAlignmentOptions.Center);}
            var note=New("Timing policy",v.scroll.content);Height(note,34);Label("",Fill("Copy",note,2),"Normal sales are the base for tap links and Bulk Order. Cooldown starts at activation.",9,false,Muted,TextAlignmentOptions.Center);
            Footer(v,0,"ActivateSkill","","skill-activate","Activate skill");((RectTransform)v.scroll.transform).offsetMin=new Vector2(6,68);
        }
        static void SyncSettings()
        {
            var v=views.Single(d=>d.route=="Settings");NoFooter(v);
            for(int i=0;i<3;i++)
            {
                string key=new[]{"music","sound","motion"}[i];var r=(RectTransform)v.scroll.content.Find(key);
                var im=r.GetComponent<Image>();im.sprite=null;im.color=Color.clear;
                Picture("",Region("Painted icon",r,0,.15f,.14f,.85f),AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt+key+".png"));
                Put((RectTransform)r.Find("Title"),.18f,.45f,.71f,.95f);Put((RectTransform)r.Find("Copy"),.18f,.10f,.71f,.45f);r.Find("Title").GetComponent<TMP_Text>().font=serif;r.Find("Title").GetComponent<TMP_Text>().fontSize=17;
                Face(Bottom("Rule",r,1),new Color(198/255f,152/255f,67/255f,.3f));
            }
            for(int i=0;i<4;i++)
            {
                var r=(RectTransform)v.scroll.content.Find(new[]{"Naming","Account","Privacy","Help"}[i]);r.GetComponent<LayoutElement>().preferredHeight=r.GetComponent<LayoutElement>().minHeight=60;
                Picture("",Region("Painted icon",r,.02f,.15f,.16f,.85f),AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt+new[]{"shop","cloud","shield","book"}[i]+".png"));
                var label=r.GetComponentInChildren<TMP_Text>();Put(label.rectTransform,.20f,.1f,.90f,.90f);label.alignment=TextAlignmentOptions.Left;
                Label("",Region("Chevron",r,.90f,0,.98f,1),"›",20,true,Gold,TextAlignmentOptions.Center);
            }
        }
        static void RefreshPopup(ClassicView v)
        {
            var outside=v.root.transform.Find("Outside dismissal").GetComponent<Button>();outside.interactable=v.route!="SaveRecovery"&&v.route!="RecoveryExit";
            if(v.route=="CardDetail")return;
            var surface=v.window.GetComponent<Image>();surface.color=Cream;surface.sprite=rounded;
            var frame=Picture("",Fill("Painted gold and cobalt frame",v.window,-2),popupFrame,false);frame.type=Image.Type.Sliced;frame.fillCenter=false;frame.pixelsPerUnitMultiplier=7.2f;frame.transform.SetAsLastSibling();
            var header=(RectTransform)v.window.Find("Header");header.sizeDelta=new Vector2(0,64);
            var ribbon=header.Find("Red title ribbon");if(ribbon)ribbon.gameObject.SetActive(false);
            var title=header.Find("Title").GetComponent<TMP_Text>();Put(title.rectTransform,.06f,.08f,.80f,.92f);title.font=serif;title.fontSize=22;title.fontSizeMax=22;title.fontSizeMin=15;title.enableAutoSizing=true;title.alignment=TextAlignmentOptions.Center;title.color=Ink;
            var rule=header.Find("Rule");if(rule){rule.gameObject.SetActive(true);rule.GetComponent<Image>().color=new Color(198/255f,152/255f,67/255f,.4f);}
            var close=header.Find("Close") as RectTransform;if(close){close.anchorMin=close.anchorMax=new Vector2(1,1);close.pivot=new Vector2(1,1);close.sizeDelta=new Vector2(48,48);close.anchoredPosition=new Vector2(-6,-6);var image=close.GetComponent<Image>();image.sprite=blue;image.type=Image.Type.Sliced;image.color=Color.white;}
            var scroll=(RectTransform)v.scroll.transform;scroll.offsetMax=new Vector2(-6,-66);scroll.offsetMin=new Vector2(6,scroll.offsetMin.y);
            foreach(var element in v.scroll.content.GetComponentsInChildren<LayoutElement>(true))
            {
                var h=element.transform.Find("Heading");var copy=element.transform.Find("Body");if(!h||!copy)continue;
                var headingText=h.GetComponent<TMP_Text>();headingText.font=serif;headingText.fontSize=16;headingText.alignment=TextAlignmentOptions.Left;((RectTransform)h).offsetMin=new Vector2(12,-30);((RectTransform)h).offsetMax=new Vector2(-12,0);
                copy.GetComponent<TMP_Text>().fontSize=12;copy.GetComponent<TMP_Text>().overflowMode=TextOverflowModes.Overflow;
            }
        }
        static void AddIllustrations()
        {
            foreach(var pair in new[]{("Privacy","cloud"),("Help","book"),("Guidance","book"),("TapTraining","shop"),("AccountCompare","cloud"),("Handoff","shop"),("SaveRecovery","shield"),("Naming","shop"),("Promote","shop"),("SignOut","shop"),("RecoveryConfirm","shield"),("RecoveryExit","shop"),("Welcome","shop"),("Account","cloud")})
            {
                var v=views.Single(x=>x.route==pair.Item1);var r=New("Painted emblem",v.scroll.content);Height(r,68);var icon=New("Artwork",r);icon.anchorMin=icon.anchorMax=new Vector2(.5f,.5f);icon.sizeDelta=new Vector2(62,62);Picture("",icon,AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt+pair.Item2+".png"));r.SetAsFirstSibling();
            }
            foreach(var route in new[]{"Prestige","Offline"})
            {
                var v=views.Single(x=>x.route==route);var r=New("Reward artwork",v.scroll.content);Height(r,68);Picture("",Region("Artwork",r,.38f,0,.62f,1),Sprite(route=="Prestige"?"Icon Legacy":"Coins"));r.SetAsFirstSibling();
            }
            var stats=views.Single(x=>x.route=="Statistics");int i=0;foreach(var r in stats.scroll.content.GetComponentsInChildren<LayoutElement>().Where(r=>r.transform.Find("Heading")))
            {
                var headingText=r.transform.Find("Heading").GetComponent<TMP_Text>();headingText.rectTransform.offsetMax=new Vector2(-48,0);Picture("",Fixed("Ledger artwork",r.transform,260,8,35,35),Sprite(i++==0?"Coins":"Icon Legacy"));
            }
        }
        static void SyncAppearance()
        {
            var v=views.Single(x=>x.route=="Appearance");
            // Product variants and decoration sprites come from the same saved progression owner.
            var looks=ui.progression.appearances;
            for(int i=0;i<4;i++)
            {
                var r=Row("Phuljhari appearance "+i,v.scroll.content,72);Picture("",Fixed("Artwork",r,10,5,54,62),i==0?ui.productArt[0]:looks.At(0,i));Label("",Region("Milestone",r,.25f,.15f,.96f,.9f),i==0?"Original display":"Permanent appearance · stage "+i,12,true);
            }
        }
        public static string SyncBootstrapHTML()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var scene=SceneManager.GetSceneByPath("Assets/Scenes/Bootstrap.unity");if(!scene.isLoaded||scene.isDirty)throw new InvalidOperationException("Bootstrap must be loaded and clean");
            var root=scene.GetRootGameObjects().Single(g=>g.name=="Bootstrap Canvas");
            if(root.transform.Find("HTML sync revision"))throw new InvalidOperationException("Bootstrap already migrated");
            var legacy=Object.Instantiate(root);legacy.name="Bootstrap Canvas__LegacyDisabled_20261019";legacy.SetActive(false);Undo.RegisterCreatedObjectUndo(legacy,"Preserve original startup UI");
            SceneManager.MoveGameObjectToScene(legacy,scene);
            var controller=root.GetComponent<RocketRaja.Cloud.BootstrapController>();
            var safe=root.transform.Find("Safe Area");safe.Find("Branding").gameObject.SetActive(false);safe.Find("Flexible Scene Space").gameObject.SetActive(false);
            var stack=safe.GetComponent<VerticalLayoutGroup>();stack.childAlignment=TextAnchor.MiddleCenter;stack.padding=new RectOffset(36,36,54,54);stack.childControlHeight=stack.childControlWidth=true;stack.childForceExpandHeight=false;
            var panel=(RectTransform)safe.Find("Ivory Status Panel");panel.GetComponent<VerticalLayoutGroup>().enabled=false;
            var panelSize=panel.GetComponent<LayoutElement>();if(!panelSize)panelSize=panel.gameObject.AddComponent<LayoutElement>();panelSize.minHeight=0;panelSize.preferredHeight=1700;panelSize.flexibleHeight=0;
            panel.GetComponent<Image>().color=Cream;panel.GetComponent<Image>().sprite=rounded;
            var outside=Fill("Outside dismissal",safe);outside.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;outside.SetAsFirstSibling();Face(outside,new Color(0,0,0,.3f),null,true);outside.gameObject.AddComponent<RocketRaja.Cloud.BootstrapPopupDismiss>().owner=controller;
            var old=Enumerable.Range(0,panel.childCount).Select(i=>panel.GetChild(i)).ToArray();
            var scrollRect=Fill("Popup scroll",panel,36);scrollRect.offsetMax=new Vector2(-36,-48);var scroll=Scroll(scrollRect,out var content);var flow=content.GetComponent<VerticalLayoutGroup>();flow.padding=new RectOffset(24,24,24,24);flow.spacing=24;
            foreach(var child in old)
            {
                if(child.name=="Paper Surface"){child.gameObject.SetActive(false);continue;}
                child.SetParent(content,false);
                if(child.GetComponent<TMP_Text>())
                {
                    var text=child.GetComponent<TMP_Text>();text.color=Ink;text.alignment=TextAlignmentOptions.Center;text.overflowMode=TextOverflowModes.Overflow;
                    var el=child.GetComponent<LayoutElement>();if(el){el.minHeight=0;el.preferredHeight=-1;}
                    if(child.name=="Heading"){text.font=serif;text.fontSize=66;}
                    else{text.font=body;text.fontSize=child.name=="Description"?39:31;}
                }
            }
            foreach(var button in content.GetComponentsInChildren<Button>(true))
            {
                var image=button.targetGraphic as Image;if(image){image.sprite=blue;image.color=Color.white;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=1;}
                foreach(var text in button.GetComponentsInChildren<TMP_Text>(true)){text.color=Cream;text.font=bold;text.fontSize=39;text.enableAutoSizing=true;text.fontSizeMax=39;text.fontSizeMin=30;}
            }
            var frame=Picture("",Fill("Painted gold and cobalt frame",panel,-6),popupFrame,false);frame.type=Image.Type.Sliced;frame.fillCenter=false;frame.pixelsPerUnitMultiplier=2.4f;
            var close=New("Close popup",panel);close.anchorMin=close.anchorMax=new Vector2(1,1);close.pivot=Vector2.one;close.sizeDelta=new Vector2(144,144);close.anchoredPosition=new Vector2(-18,-18);var face=Face(close,Color.white,blue,true);var closeButton=close.gameObject.AddComponent<Button>();closeButton.targetGraphic=face;close.gameObject.AddComponent<RocketRaja.Cloud.BootstrapPopupDismiss>().owner=controller;Label("",Fill("Label",close),"×",66,true,Cream,TextAlignmentOptions.Center);
            // Reserve title space for the independently placed close control.
            var headingRect=(RectTransform)content.Find("Heading");headingRect.GetComponent<TMP_Text>().margin=new Vector4(100,0,100,0);
            var serialized=new SerializedObject(controller);serialized.FindProperty("popupClose").objectReferenceValue=closeButton;serialized.ApplyModifiedPropertiesWithoutUndo();
            New("HTML sync revision",root.transform).gameObject.SetActive(false);
            ClassicPopupClearanceAuthoring.ApplyBootstrap(panel);
            Canvas.ForceUpdateCanvases();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Saved production Bootstrap popup presentation with existing account callbacks and guarded outside dismissal";
        }
        static Sprite BoxSprite(string name,Color fill,int goldWidth,int blueWidth=0)
        {
            const int size=128;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            bool Inside(float x,float y,float inset)
            {
                float radius=Mathf.Max(0,15-inset),low=inset+radius,high=size-inset-radius;
                return Vector2.Distance(new Vector2(x,y),new Vector2(Mathf.Clamp(x,low,high),Mathf.Clamp(y,low,high)))<=radius&&x>=inset&&y>=inset&&x<=size-inset&&y<=size-inset;
            }
            var colors=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++)colors[y*size+x]=!Inside(x+.5f,y+.5f,0)?Color.clear:!Inside(x+.5f,y+.5f,goldWidth)?Gold:blueWidth>0&&!Inside(x+.5f,y+.5f,goldWidth+blueWidth)?Ink:fill;
            texture.SetPixels(colors);texture.Apply();string path=SyncArt+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=Vector4.one*18;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        public static string RefineHTMLPresentation()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var main=SceneManager.GetSceneByPath(ScenePath);bool mainWasLoaded=main.isLoaded;if(!main.isLoaded)main=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
            body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Body SDF.asset");bold=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Bold SDF.asset");serif=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Card Serif SDF.asset");
            var primary=BoxSprite("popup-primary",Ink,2);var secondary=BoxSprite("popup-secondary",Cream,1);var disabled=BoxSprite("popup-disabled",new Color32(233,222,196,255),1);var panel=BoxSprite("skill-panel",Cream,2,3);var note=BoxSprite("note",Cream,1);
            var importer=(TextureImporter)AssetImporter.GetAtPath(SyncArt+"ring.png");importer.spriteImportMode=SpriteImportMode.Single;importer.SaveAndReimport();var ring=AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt+"ring.png");if(!ring)throw new InvalidOperationException("Ring sprite import failed");
            foreach(var scene in new[]{main,SceneManager.GetSceneByPath("Assets/Scenes/HTML Importer.unity")}.Where(s=>s.isLoaded))
            {
                var native=scene.GetRootGameObjects().Where(g=>g.activeSelf).Select(g=>g.GetComponent<ClassicUI>()).Single(c=>c);native.popupPrimary=primary;native.popupSecondary=secondary;native.popupDisabled=disabled;
                ui=native;
                var stats=native.dialogs.Single(v=>v.route=="Statistics");
                var extraBindings=new List<ClassicTextBinding>(native.textBindings);
                var extraShop=native.textBindings.FirstOrDefault(b=>b.key=="stats-shop");if(extraShop!=null&&extraShop.text){Object.DestroyImmediate(extraShop.text.transform.parent.gameObject);extraBindings.RemoveAll(b=>b.key=="stats-shop"||b.key=="stats-shop-values");}
                foreach(var key in new[]{"stats-run","stats-lifetime"})
                {
                    var copy=native.textBindings.Single(b=>b.key==key).text;var rect=copy.rectTransform;
                    rect.anchorMax=new Vector2(.57f,rect.anchorMax.y);rect.offsetMax=new Vector2(-4,rect.offsetMax.y);
                    if(!extraBindings.Any(b=>b.key==key+"-values"))
                    {
                        var value=Region("Values",rect.parent,.58f,0,1,1);value.offsetMin=new Vector2(0,12);value.offsetMax=new Vector2(-12,-32);
                        var label=Label("",value,"",12,true,Ink,TextAlignmentOptions.TopRight);label.overflowMode=TextOverflowModes.Overflow;extraBindings.Add(new ClassicTextBinding{key=key+"-values",text=label});
                    }
                }
                native.textBindings=extraBindings.ToArray();
                var naming=native.dialogs.Single(d=>d.route=="Naming");
                if(!naming.scroll.content.Find("Name validation"))
                {
                    var hint=New("Name help",naming.scroll.content);Height(hint,24);Label("",Fill("Copy",hint,2),"Up to 24 characters.",10,false,Muted);
                    var validation=New("Name validation",naming.scroll.content);Height(validation,30);var label=Label("",Fill("Error",validation,2),"",10,true,new Color32(154,39,57,255));
                    native.textBindings=native.textBindings.Concat(new[]{new ClassicTextBinding{key="naming-error",text=label}}).ToArray();validation.gameObject.SetActive(false);
                }
                foreach(var route in new[]{"TapTraining","ProductDetail"})
                {
                    var v=native.dialogs.Single(d=>d.route==route);if(!v.scroll.content.Find("Exact price")){var price=New("Exact price",v.scroll.content);Height(price,48);var b=Button(price,"Exact",route=="TapTraining"?"tap":"product","","See exact price",secondary);b.GetComponent<ClassicControl>().htmlStyle=2;}
                }
                var appearance=native.dialogs.Single(d=>d.route=="Appearance");
                for(int i=1;i<4;i++)appearance.scroll.content.Find("Phuljhari appearance "+i+"/Milestone").GetComponent<TMP_Text>().text=new[]{"","Signature I","Signature IV","Signature X"}[i]+" · cash upgrade this run";
                if(!appearance.scroll.content.Find("Permanent shop decorations"))
                {
                    Info(appearance.scroll.content,"Permanent shop decorations","","Earned from unique lifetime upgrade purchases. Decorations stay after prestige; product displays rebuild.",95);
                    for(int i=0;i<6;i++){var r=Row("Decoration stage "+i,appearance.scroll.content,68);var sprite=i==0?native.productArt[0]:native.progression.shop.layers[i-1].GetComponentInChildren<Image>(true).sprite;Picture("",Fixed("Artwork",r,8,5,56,58),sprite);Label("",Region("Requirement",r,.24f,.10f,.97f,.90f),RedesignShopVisuals.Names[i]+"\n"+RedesignShopProgress.Threshold(i)+" unique purchases",12,true);}
                }
                foreach(var c in native.skillClocks)
                {
                    c.ring.sprite=ring;c.disc=c.artwork.transform.parent.GetComponent<Image>();
                    if(!c.track){c.track=Picture("",Fill("Clock track",c.ring.transform.parent,-3),ring);c.track.transform.SetSiblingIndex(c.ring.transform.GetSiblingIndex());}
                }
                foreach(var v in native.dialogs)
                {
                    var popupHeader=v.window.Find("Header");
                    if(popupHeader&&v.route!="Skills"&&v.route!="SkillDetail"){var title=popupHeader.Find("Title").GetComponent<TMP_Text>();Put(title.rectTransform,.06f,.08f,.80f,.92f);title.fontSizeMin=15;title.fontSizeMax=22;}
                    if(popupHeader&&(v.route=="SaveRecovery"||v.route=="RecoveryExit")){var title=popupHeader.Find("Title").GetComponent<TMP_Text>();Put(title.rectTransform,.07f,.08f,.93f,.92f);title.fontSizeMin=16;title.fontSizeMax=22;}
                    foreach(var r in v.window.GetComponentsInChildren<LayoutElement>(true))
                    {
                        var h=r.transform.Find("Heading") as RectTransform;
                        if(h&&h.GetComponent<TMP_Text>()){h.anchorMin=new Vector2(0,1);h.anchorMax=Vector2.one;h.pivot=new Vector2(.5f,1);h.sizeDelta=new Vector2(-24,30);h.anchoredPosition=Vector2.zero;}
                    }
                    foreach(var image in v.window.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Ledger artwork")){image.rectTransform.sizeDelta=new Vector2(24,24);image.rectTransform.anchoredPosition=new Vector2(260,-3);}
                    foreach(var c in v.window.GetComponentsInChildren<ClassicControl>(true))
                    {
                        if(!(c.GetComponent<Button>()?.targetGraphic is Image im))continue;
                        bool plain=v.route=="Skills"&&c.action=="Navigate"&&c.parameter.StartsWith("SkillDetail:");bool second=c.action=="Tip"||im.sprite==native.ivoryButton||im.sprite==secondary;
                        c.htmlStyle=plain?3:second?2:1;im.sprite=plain?null:second?secondary:primary;im.type=Image.Type.Sliced;im.pixelsPerUnitMultiplier=1;im.color=plain?Color.clear:Color.white;
                        foreach(var t in c.GetComponentsInChildren<TMP_Text>(true)){t.color=second||plain?Ink:Cream;if(plain)t.fontStyle=FontStyles.Underline;}
                    }
                    foreach(var r in v.window.GetComponentsInChildren<LayoutElement>(true))if(r.TryGetComponent<Image>(out var image)&&!r.GetComponent<Button>()&&r.transform.Find("Heading")){image.sprite=note;image.color=Color.white;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=1;}
                    foreach(var image in v.window.GetComponentsInChildren<Image>(true))if(image.sprite&&image.sprite.name=="Ivory Row Frame"){image.sprite=note;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=1;}
                    foreach(var text in v.window.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if(!text.enableAutoSizing){text.fontSizeMax=text.fontSize;text.fontSizeMin=text.fontSize*.72f;text.enableAutoSizing=true;}
                    }
                    if(v.route=="Skills"||v.route=="SkillDetail")
                    {
                        var frame=v.window.Find("Painted gold and cobalt frame").GetComponent<Image>();frame.sprite=panel;frame.fillCenter=false;frame.pixelsPerUnitMultiplier=1;
                        if(v.route=="Skills")
                        {
                            var header=v.window.Find("Header");var subtitle=v.scroll.content.Find("Subtitle")??header.Find("Subtitle");subtitle.Find("Copy").GetComponent<TMP_Text>().text="Three ways to boost your shop";
                            subtitle.SetParent(header,false);var sub=(RectTransform)subtitle;sub.anchorMin=sub.anchorMax=new Vector2(.5f,0);sub.pivot=new Vector2(.5f,0);sub.sizeDelta=new Vector2(260,18);sub.anchoredPosition=new Vector2(0,3);
                            var title=header.Find("Title").GetComponent<TMP_Text>();Put(title.rectTransform,.13f,.4f,.80f,.95f);title.fontSizeMax=25;title.fontSizeMin=18;
                            foreach(var r in v.scroll.content.GetComponentsInChildren<LayoutElement>(true).Where(r=>r.name.StartsWith("Skill "))){var im=r.GetComponent<Image>();im.sprite=note;im.type=Image.Type.Sliced;im.color=Color.white;im.pixelsPerUnitMultiplier=1;}
                            var reset=v.scroll.content.Find("Prestige note").GetComponent<LayoutElement>();reset.minHeight=reset.preferredHeight=26;
                        }
                        else
                        {
                            var headerTitle=v.window.Find("Header/Title").GetComponent<TMP_Text>();headerTitle.fontSizeMax=25;headerTitle.fontSizeMin=18;
                            var name=v.scroll.content.Find("Name");if(name)Object.DestroyImmediate(name.gameObject);
                            var binding=native.textBindings.Single(b=>b.key=="skill-title");binding.text=headerTitle;
                            v.scroll.content.Find("Effect panels/Effect 0/Icon").GetComponent<Image>().sprite=Sprite("Icon Stats");
                        }
                    }
                }
                foreach(var key in new[]{"home-income","home-tap"}){var t=native.textBindings.Single(b=>b.key==key).text;t.fontSizeMin=11;t.fontSizeMax=15;t.enableAutoSizing=true;t.overflowMode=TextOverflowModes.Overflow;}
                foreach(var key in new[]{"home-income","home-tap","home-total"}){var t=native.textBindings.Single(b=>b.key==key).text;t.textWrappingMode=TextWrappingModes.NoWrap;t.enableAutoSizing=true;if(key=="home-total"){t.fontSizeMin=12;t.fontSizeMax=21;}}
                var settings=native.dialogs.Single(v=>v.route=="Settings");
                foreach(var old in settings.scroll.content.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Painted icon").ToArray())
                {
                    if(old.transform.Find("Gold disc"))continue;
                    var disc=Fill("Gold disc",old.rectTransform,0);var circle=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");Face(disc,Gold,circle);var inner=Fill("Cobalt disc",disc,2);Face(inner,Ink,circle);Picture("",Fill("Artwork",inner,6),old.sprite);old.color=Color.clear;
                    // Fixed square at the row's left edge; artwork keeps its aspect ratio.
                    old.rectTransform.anchorMin=old.rectTransform.anchorMax=new Vector2(0,.5f);old.rectTransform.pivot=new Vector2(0,.5f);old.rectTransform.sizeDelta=new Vector2(42,42);old.rectTransform.anchoredPosition=new Vector2(5,0);
                }
                native.guidance.rows=native.guidance.rows.Where(r=>r.root).ToArray();
                native.GetComponent<ClassicLayout>().infoRows=native.GetComponentsInChildren<LayoutElement>(true).Where(e=>e.transform.Find("Heading")&&e.transform.Find("Body")&&e.transform.Find("Body").GetComponent<TMP_Text>()).Select(e=>new ClassicInfoFit{layout=e,copy=e.transform.Find("Body").GetComponent<TMP_Text>(),values=e.transform.Find("Values")?e.transform.Find("Values").GetComponent<TMP_Text>():null}).ToArray();
                foreach(var v in native.dialogs)ClassicPopupClearanceAuthoring.ApplyView(v);
                ClassicPanelControlsAuthoring.ApplyToUI(native);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            // Bootstrap uses the same styles at its existing 3x logical reference scale.
            var bootstrap=SceneManager.GetSceneByPath("Assets/Scenes/Bootstrap.unity");bool bootstrapWasLoaded=bootstrap.isLoaded;if(!bootstrap.isLoaded)bootstrap=EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity",OpenSceneMode.Additive);
            var boot=bootstrap.GetRootGameObjects().Single(g=>g.name=="Bootstrap Canvas");foreach(var button in boot.GetComponentsInChildren<Button>(true))if(button.targetGraphic is Image im){im.sprite=primary;im.type=Image.Type.Sliced;im.pixelsPerUnitMultiplier=1/3f;}
            foreach(var button in boot.GetComponentsInChildren<Button>(true)){var face=button.transform.Find("Face");if(face)face.gameObject.SetActive(false);foreach(var label in button.GetComponentsInChildren<TMP_Text>(true))label.color=Cream;}
            var owner=boot.GetComponent<RocketRaja.Cloud.BootstrapController>();var safe=boot.transform.Find("Safe Area");var popup=safe.Find("Ivory Status Panel");var backdrop=safe.Find("Outside dismissal");var opener=safe.Find("Open your stall") as RectTransform;
            var flow=(RectTransform)popup.Find("Popup scroll/Viewport/Content");
            var sizer=popup.GetComponent<RocketRaja.Cloud.BootstrapPopupSize>();if(!sizer)sizer=popup.gameObject.AddComponent<RocketRaja.Cloud.BootstrapPopupSize>();sizer.content=flow;sizer.padding=96;
            var brand=safe.Find("Branding");if(brand)
            {
                brand.SetParent(flow,false);brand.SetAsFirstSibling();brand.gameObject.SetActive(true);var el=brand.GetComponent<LayoutElement>();el.minHeight=el.preferredHeight=180;
                brand.Find("Ornament").gameObject.SetActive(false);var title=brand.Find("Rocket Raja").GetComponent<TMP_Text>();Put(title.rectTransform,.05f,.37f,.95f,.97f);title.font=serif;title.fontSizeMin=60;title.fontSizeMax=72;title.fontSize=72;title.color=Ink;
                var tag=brand.Find("Tagline").GetComponent<TMP_Text>();Put(tag.rectTransform,.02f,.02f,.98f,.32f);tag.fontSize=24;tag.fontSizeMin=18;tag.fontSizeMax=24;tag.enableAutoSizing=true;tag.color=Muted;
            }
            if(!flow.Find("Painted shop artwork"))
            {
                var artRow=New("Painted shop artwork",flow);Height(artRow,300);artRow.SetSiblingIndex(1);
                var mainUI=main.GetRootGameObjects().Single(g=>g.name==RootName).GetComponent<ClassicUI>();Picture("",Fill("Stall",artRow,4),mainUI.transform.Find("SafeArea/Shop/Painted Stall").GetComponent<Image>().sprite);
            }
            if(!opener)
            {
                opener=Bottom("Open your stall",safe,144,96);opener.offsetMin=new Vector2(90,opener.offsetMin.y);opener.offsetMax=new Vector2(-90,opener.offsetMax.y);opener.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;
                var face=Face(opener,Color.white,primary,true);face.pixelsPerUnitMultiplier=1/3f;var button=opener.gameObject.AddComponent<Button>();button.targetGraphic=face;Label("",Fill("Label",opener,12),"Open your stall",42,true,Cream,TextAlignmentOptions.Center);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick,owner.ReopenPopup);opener.gameObject.SetActive(false);
            }
            var so=new SerializedObject(owner);so.FindProperty("popupPanel").objectReferenceValue=popup.gameObject;so.FindProperty("popupBackdrop").objectReferenceValue=backdrop.gameObject;so.FindProperty("popupOpener").objectReferenceValue=opener.gameObject;so.ApplyModifiedPropertiesWithoutUndo();
            SaveBootstrapTextSlots(boot);
            ClassicPopupClearanceAuthoring.ApplyBootstrap((RectTransform)popup);
            EditorSceneManager.MarkSceneDirty(bootstrap);EditorSceneManager.SaveScene(bootstrap);
            if(!bootstrapWasLoaded)EditorSceneManager.CloseScene(bootstrap,true);if(!mainWasLoaded)EditorSceneManager.CloseScene(main,true);
            return "Saved ring sprite, compact label fit, shared rounded actions, circular settings artwork and dynamic hero heading";
        }
        static void SaveBootstrapTextSlots(GameObject root)
        {
            // TMP's usual fallback path creates submesh objects. Supply the same two saved
            // material slots used by Main, and repoint every serialized presenter reference.
            foreach(var old in root.GetComponentsInChildren<TextMeshProUGUI>(true).Where(t=>!(t is RedesignSavedText)).ToArray())
            {
                var links=new List<Tuple<Component,string>>();
                foreach(var component in root.GetComponentsInChildren<Component>(true).Where(c=>c&&c!=old))
                {
                    var serialized=new SerializedObject(component);var property=serialized.GetIterator();
                    while(property.Next(true))if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue==old)links.Add(Tuple.Create(component,property.propertyPath));
                }
                var rect=old.rectTransform;string copy=old.text;var font=old.font;float size=old.fontSize,min=old.fontSizeMin,max=old.fontSizeMax;bool auto=old.enableAutoSizing;var color=old.color;var alignment=old.alignment;var wrap=old.textWrappingMode;var overflow=old.overflowMode;var style=old.fontStyle;var margin=old.margin;float line=old.lineSpacing,character=old.characterSpacing;bool rich=old.richText;
                Object.DestroyImmediate(old);
                var replacement=Label("",rect,copy,size,false,color,alignment,font);replacement.fontSizeMin=min;replacement.fontSizeMax=max;replacement.enableAutoSizing=auto;replacement.textWrappingMode=wrap;replacement.overflowMode=overflow;replacement.fontStyle=style;replacement.margin=margin;replacement.lineSpacing=line;replacement.characterSpacing=character;replacement.richText=rich;
                foreach(var link in links)if(link.Item1){var serialized=new SerializedObject(link.Item1);serialized.FindProperty(link.Item2).objectReferenceValue=replacement;serialized.ApplyModifiedPropertiesWithoutUndo();}
            }
        }
    }
}
