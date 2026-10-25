using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        [Serializable] sealed class ClassicGuideManifest { public RedesignGuidanceText[] topics; }
        [MenuItem("Rocket Raja/Classic/Connect presentation")]
        public static void ConnectPresentation()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var scene=EditorSceneManager.OpenScene(ClassicProductionAuthoring.Main);
            ui=scene.GetRootGameObjects().Select(g=>g.GetComponent<ClassicUI>()).Single(c=>c);
            body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Body SDF.asset");
            bold=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Bold SDF.asset");
            heading=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Heading SDF.asset");
            blue=ui.blueButton;ivory=ui.ivoryButton;row=Sprite("Ivory Row Frame");rounded=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if(!ui.guidance)AuthorClassicGuidance();
            if(!ui.progression)AuthorClassicProgression();
            // Use a word on this new reader's close action: it stays legible on the smallest screen.
            var guidanceDialog=ui.dialogs.Single(v=>v.route=="Guidance");
            var close=guidanceDialog.window.Find("Header/Close").GetComponentInChildren<TMP_Text>();close.text="Back";close.fontSize=12;
            ui.guidance.body.alignment=TextAlignmentOptions.TopLeft;
            ui.GetComponent<ClassicLayout>().touchGraphics=ui.GetComponentsInChildren<Button>(true).Select(b=>b.targetGraphic).Where(g=>g).Distinct().ToArray();
            ui.progression.counterSlogan=ui.transform.Find("SafeArea/Shop/Counter message").gameObject;
            ApplyStallPrompt(ui);
            if(!ui.audioFeedback)
            {
                var audio=new GameObject("Classic Audio",typeof(RedesignAudio));audio.transform.SetParent(ui.transform,false);
                var owner=audio.GetComponent<RedesignAudio>();owner.driver=ui.driver;owner.explicitFeedback=true;
                owner.music=audio.AddComponent<AudioSource>();owner.effects=audio.AddComponent<AudioSource>();
                owner.music.playOnAwake=owner.effects.playOnAwake=false;owner.music.loop=true;owner.music.volume=.12f;owner.effects.volume=.25f;
                owner.music.spatialBlend=owner.effects.spatialBlend=0;ui.audioFeedback=owner;
            }
            foreach(var v in ui.dialogs)v.root.SetActive(false);
            foreach(var p in ui.panels)p.root.SetActive(p.route=="Home");
            foreach(var r in ui.guidance.rows)r.root.SetActive(false);
            foreach(var layer in ui.progression.shop.layers)layer.SetActive(false);
            EditorUtility.SetDirty(ui);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        static void AuthorClassicGuidance()
        {
            var guide=ui.gameObject.AddComponent<ClassicGuidance>();ui.guidance=guide;guide.ui=ui;
            var topics=JsonUtility.FromJson<ClassicGuideManifest>(File.ReadAllText("UI Builder/HTML/Redesign/guidance-topics.json")).topics.ToList();
            var catalogue=topics.Single(t=>t.id=="catalogue-filters");catalogue.brief="The next ten are ready in Player.";
            catalogue.body="Player shows the next ten eligible cash upgrades. Each row states its effect and price. Buy directly, or open the catalogue to search and filter. These upgrades need only cash and their listed run requirements, never Fame or completed prestiges.";
            var batch=topics.Single(t=>t.id=="buy-all");batch.title="Buy affordable upgrades";batch.brief="Buy All spends cash on eligible upgrades.";
            batch.body="Player's Buy All immediately buys affordable eligible cash upgrades in price order. In the catalogue, Review Buy All shows the scoped list before purchase. Neither buys product levels or Talent, nor spends KK.";
            topics.Single(t=>t.id=="buy-first").brief="Tap to earn your first rupees, then stock Phuljhari.";
            topics.Single(t=>t.id=="buy-first").body="Start with ₹0 and tap the counter to earn ₹1 per tap. Your first Salesmanship upgrade costs ₹10. Save ₹30 to buy one Phuljhari from Products and start earning automatic sales.";
            topics.Single(t=>t.id=="comebacks").body="Signature cash upgrades improve earlier products and change their display at signatures I, IV and X. These appearances rebuild each run. Shop decorations earned from lifetime discoveries stay after Prestige.";
            topics.Single(t=>t.id=="talent-free").body="A 1× pull uses one free credit when available, otherwise 100 KK. A 5× pull uses four free credits when available, otherwise 400 KK. Payments are never mixed. Results are saved before the animation; Continue reveals the same saved cards.";
            topics.Add(new RedesignGuidanceText{id="return-play",title="Welcome back",brief="Your offline cash is already saved.",body="The amount shown was settled once from your departure sales rate, offline efficiency and time cap. Continue keeps it; reopening this screen does not grant it again. Resume product purchases or check Player for your next upgrades."});
            guide.topics=topics.ToArray();var rows=new List<ClassicTipRow>();
            foreach(var panel in ui.panels)
            {
                var tip=Top("Contextual guidance",panel.root.transform,48,panel.route=="Home"?7:51);tip.offsetMin=new Vector2(10,tip.offsetMin.y);tip.offsetMax=new Vector2(-10,tip.offsetMax.y);
                rows.Add(Tip(panel.route,tip));
            }
            foreach(var view in ui.dialogs.Where(v=>new[]{"ProductDetail","TapTraining","Skills","SkillDetail","Prestige","Offline","DrawReveal","BuyAll","Upgrades"}.Contains(v.route)))
            {
                if(!view.scroll)continue;
                var tip=New("Contextual guidance",view.scroll.content);Height(tip,56);tip.SetAsFirstSibling();rows.Add(Tip(view.route,tip));
            }
            guide.rows=rows.ToArray();
            var dialog=Dialog(ui.dialogs[0].root.transform.parent,"Guidance","A little guidance",out var content);
            var head=New("Topic",content);Height(head,60);guide.heading=Label("",Fill("Label",head,8),"",22,true);
            var copy=New("Explanation",content);Height(copy,230);guide.body=Label("",Fill("Label",copy,10),"",16);guide.body.overflowMode=TextOverflowModes.Overflow;
            Footer(dialog,0,"LearnTip","","","Got it");ui.dialogs=ui.dialogs.Concat(new[]{dialog}).ToArray();
        }
        static ClassicTipRow Tip(string route,RectTransform rect)
        {
            Face(rect,Cream,row,true);var control=rect.gameObject.AddComponent<ClassicControl>();control.owner=ui;control.action="Tip";control.parameter="";
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=rect.GetComponent<Image>();button.navigation=new Navigation{mode=Navigation.Mode.None};
            var title=Label("",Region("Topic",rect,.035f,.51f,.85f,.94f),"",13,true);
            var brief=Label("",Region("Hint",rect,.035f,.08f,.87f,.51f),"",11);
            Label("",Region("Read",rect,.88f,.1f,.98f,.9f),"i",20,true,Ink,TextAlignmentOptions.Center);
            return new ClassicTipRow{route=route,root=rect.gameObject,title=title,brief=brief};
        }
        static void AuthorClassicProgression()
        {
            var p=ui.gameObject.AddComponent<ClassicProgression>();ui.progression=p;p.ui=ui;
            p.appearances=AssetDatabase.LoadAssetAtPath<RedesignProductArt>(BuildRedesignUI.ProductArtPath);
            // ProductArtPath belongs to the existing art authoring module; never recrop at runtime.
            if(!p.appearances||p.appearances.appearances.Length!=32||p.appearances.appearances.Any(s=>!s))throw new InvalidOperationException("Missing four-look product art");
            p.shelves=ui.shelfProducts.Select(g=>g.GetComponent<Image>()).ToArray();
            p.rows=ui.products.Select(r=>r.artFrame.GetComponentsInChildren<Image>(true).First(i=>ui.productArt.Contains(i.sprite))).ToArray();
            var world=ui.transform.Find("SafeArea/Shop");var root=Fill("Earned Classic decorations",world);root.SetSiblingIndex(1);
            var shop=root.gameObject.AddComponent<RedesignShopVisuals>();p.shop=shop;shop.layers=new GameObject[5];
            for(int i=0;i<5;i++){var layer=Fill(RedesignShopVisuals.Names[i+1],root);shop.layers[i]=layer.gameObject;}
            Decoration("garland",shop.layers[0].transform,.10f,.63f,.90f,.69f);
            Decoration("lantern",shop.layers[1].transform,.006f,.35f,.079f,.61f);
            Decoration("lantern",shop.layers[1].transform,.925f,.35f,.998f,.61f);
            Decoration("apron",shop.layers[2].transform,.11f,.106f,.79f,.22f);
            Decoration("bunting",shop.layers[3].transform,.15f,.77f,.90f,.84f);
            Decoration("crest",shop.layers[4].transform,.46f,.205f,.58f,.28f);
            p.summary=world.Find("Tap invitation").GetComponentInChildren<TMP_Text>(true);
            var stats=ui.dialogs.Single(v=>v.route=="Statistics");var info=Info(stats.scroll.content,"Your decorations","","",150);shop.progress=info.Find("Body").GetComponent<TMP_Text>();
        }
        public static void ApplyStallPrompt(ClassicUI target)
        {
            var world=target.transform.Find("SafeArea/Shop");
            foreach(var oldName in new[]{"Shop stage","Shop stage background"})
            {
                var old=world.Find(oldName);if(old)old.gameObject.SetActive(false);
            }
            var invitation=(RectTransform)world.Find("Tap invitation");
            invitation.anchorMin=new Vector2(.18f,0);invitation.anchorMax=new Vector2(.82f,.052f);
            invitation.offsetMin=invitation.offsetMax=Vector2.zero;
            var face=invitation.GetComponent<Image>();face.sprite=null;face.color=new Color32(16,53,67,245);face.raycastTarget=false;
            var label=invitation.GetComponentInChildren<TMP_Text>(true);
            label.text="TAP THE STALL TO EARN";label.color=Cream;
            label.fontSize=label.fontSizeMax=9.5f;label.fontSizeMin=8;label.alignment=TextAlignmentOptions.Center;
            target.progression.summary=label;target.motion.tapHint=invitation;target.motion.tapHintFace=face;
            target.toast.SetActive(false);
        }
        static void Decoration(string key,Transform parent,float x0,float y0,float x1,float y1)
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/RedesignUI/Art/Decoration "+key+".asset");
            if(!sprite)throw new InvalidOperationException("Missing decoration "+key);
            Picture("",Region(key,parent,x0,y0,x1,y1),sprite,false);
        }
    }
}
