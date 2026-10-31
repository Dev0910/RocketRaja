using System;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static partial class ClassicBazaarAuthoring
    {
        static ClassicView View(string route)=>ui.dialogs.Single(v=>v.route==route);
        static void Preferred(RectTransform r,float minimum=0)
        {var e=Ensure<LayoutElement>(r.gameObject);e.ignoreLayout=false;e.minHeight=minimum;e.preferredHeight=-1;e.flexibleHeight=0;}
        static void Paragraphs(RectTransform root,float size=14)
        {
            foreach(var t in root.GetComponentsInChildren<TMP_Text>(true).Where(t=>!t.GetComponentInParent<Button>(true)&&!t.GetComponentInParent<TMP_InputField>(true)))
            {
                var parent=t.transform.parent as RectTransform;
                if(parent==root){Preferred(t.rectTransform);continue;}
                if(parent.GetComponentsInChildren<TMP_Text>(true).Length==1&&!parent.GetComponent<LayoutGroup>()&&!parent.GetComponent<Button>())
                {t.transform.SetParent(root,false);Preferred(t.rectTransform);}
            }
        }
        static void SkillViews()
        {
            var v=View("Skills");
            for(int i=0;i<3;i++)
            {
                var r=R(v.scroll.content.Find("Skill "+i));Height(r,168);Surface(r,theme.section);
                var portrait=R(r.Find("Portrait"));Fixed(portrait,12,12,44,44);var disc=portrait.GetComponent<Image>();disc.sprite=theme.Circle(44);disc.color=Color.white;disc.type=Image.Type.Simple;
                var name=r.Find("Name").GetComponent<TMP_Text>();Text(name,16,true);Top(name.rectTransform,64,12,12,24);
                var clock=ui.skillClocks.Single(c=>c.index==i&&c.effect);
                var badge=R(clock.status.transform.parent);foreach(var group in badge.GetComponents<LayoutGroup>())Object.DestroyImmediate(group);var fit=badge.GetComponent<ContentSizeFitter>();if(fit)fit.enabled=false;
                Fixed(badge,64,36,88,20);Surface(badge,theme.shadeSurface,false);Stretch(clock.status.rectTransform,2);Text(clock.status,12,true,align:TextAlignmentOptions.Center);clock.statusBadge=badge.GetComponent<Image>();
                foreach(Transform child in badge)if(child.GetComponent<Image>())child.gameObject.SetActive(false);
                Text(clock.effect,14,color:theme.muted);Top(clock.effect.rectTransform,64,58,12,24);
                Text(clock.timing,12,color:theme.muted);Top(clock.timing.rectTransform,64,82,12,24);
                var activate=r.Find("Activate").GetComponent<Button>();Button(activate);Box(R(activate.transform),Vector2.zero,new Vector2(.5f,0),new Vector2(12,12),new Vector2(-4,60));
                var details=r.Find("Details").GetComponent<Button>();Button(details,true);Box(R(details.transform),new Vector2(.5f,0),new Vector2(1,0),new Vector2(4,12),new Vector2(-12,60));
            }
            var note=R(v.scroll.content.Find("Prestige note"));Height(note,56);var nt=note.GetComponentInChildren<TMP_Text>(true);Text(nt,12,color:theme.muted,align:TextAlignmentOptions.Center);Stretch(nt.rectTransform,4);
            var hero=View("SkillDetail");var identity=R(hero.scroll.content.Find("Hero artwork"));Height(identity,120);
            var heroDisc=R(identity.Find("Portrait"));Box(heroDisc,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-44,-88),new Vector2(44,0));var im=heroDisc.GetComponent<Image>();im.sprite=theme.Circle(88);im.color=Color.white;im.type=Image.Type.Simple;
            var status=Bound("skill-state");var heroBadge=New("Bazaar state badge",identity);Box(heroBadge,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-44,-120),new Vector2(44,-100));Surface(heroBadge,theme.shadeSurface,false);status.transform.SetParent(heroBadge,false);Text(status,12,true,align:TextAlignmentOptions.Center);Stretch(status.rectTransform);ui.skillClocks.Single(c=>c.index<0).statusBadge=heroBadge.GetComponent<Image>();
            var title=Bound("skill-title");Text(title,22,heading:true,align:TextAlignmentOptions.Center);
            var effects=R(hero.scroll.content.Find("Effect panels"));Height(effects,120);
            for(int i=0;i<2;i++)
            {var tile=R(effects.Find("Effect "+i));Surface(tile,theme.section);var value=tile.Find("Value").GetComponent<TMP_Text>();Text(value,22,true,align:TextAlignmentOptions.Center);Top(value.rectTransform,8,42,8,32);var copy=tile.Find("Description").GetComponent<TMP_Text>();Text(copy,12,align:TextAlignmentOptions.Center);Top(copy.rectTransform,8,78,8,34);}
            var timing=R(hero.scroll.content.Find("Timing"));Height(timing,72);Surface(timing,theme.section);
            foreach(var t in timing.GetComponentsInChildren<TMP_Text>(true))Text(t,t.name.StartsWith("Heading")?12:20,true,align:TextAlignmentOptions.Center);
            var policy=R(hero.scroll.content.Find("Timing policy"));Flow(policy,4,4,0);Paragraphs(policy);Preferred(policy);foreach(var t in policy.GetComponentsInChildren<TMP_Text>(true))Text(t,12,color:theme.muted);
        }
        static void ProductOverview()
        {
            var v=View("ProductDetail");var content=v.scroll.content;
            foreach(Transform child in content)if(child.name.StartsWith("Spacing"))child.gameObject.SetActive(false);
            var identity=R(content.Find("Product identity"));Height(identity,72);
            var disc=identity.GetComponentsInChildren<Image>(true).First(i=>i.name.Contains("portrait"));disc.sprite=theme.Circle(72);disc.type=Image.Type.Simple;disc.color=Color.white;Fixed(disc.rectTransform,0,0,72,72);
            var name=Bound("product-detail-name");if(name){Text(name,22,heading:true);Top(R(name.transform.parent),84,0,0,28);Stretch(name.rectTransform);}
            var level=Bound("product-detail-level");if(level){Text(level,16,true);Top(R(level.transform.parent),84,30,0,22);Stretch(level.rectTransform);}
            var signature=Bound("product-subtitle");if(signature){Text(signature,12,color:theme.muted);Top(R(signature.transform.parent),84,54,0,18);Stretch(signature.rectTransform);}
            var earnings=Bound("product-earnings");Text(earnings,16,true,color:theme.success,align:TextAlignmentOptions.Center);Height(R(earnings.transform.parent),36);Stretch(earnings.rectTransform);
            foreach(var pair in new[]{new[]{"Level milestone","product-milestone","product-progress","product-progress-label","product-milestone-reward","product-milestone-copy"},new[]{"Product synergy","synergy-title","synergy-progress","synergy-progress-label","synergy-reward","synergy-copy"}})
            {
                var card=R(content.Find(pair[0]));Surface(card,theme.section);Flow(card,12,12,12);Preferred(card);
                foreach(Transform child in card)if(child.name.StartsWith("Spacing"))child.gameObject.SetActive(false);
                var heading=Bound(pair[1]);Text(heading,18,heading:true,align:TextAlignmentOptions.Center);Height(R(heading.transform.parent),32);Stretch(heading.rectTransform);
                var fill=ui.imageBindings.First(b=>b.key==pair[2]).image;var bar=R(fill.transform.parent.parent);Height(bar,24);Surface(bar,theme.barTrack,false);var inner=R(fill.transform.parent);Stretch(inner,1);inner.GetComponent<Image>().color=Color.clear;Meter(fill);
                var label=Bound(pair[3]);Text(label,12,true,color:theme.paper,align:TextAlignmentOptions.MidlineRight);Stretch(R(label.transform.parent));R(label.transform.parent).offsetMin=new Vector2(6,0);R(label.transform.parent).offsetMax=new Vector2(-6,0);Stretch(label.rectTransform);
                var reward=Bound(pair[4]);Text(reward,14,true,color:theme.muted,align:TextAlignmentOptions.Center);Height(R(reward.transform.parent),40);Stretch(reward.rectTransform);
                var copy=Bound(pair[5]);Text(copy,12);copy.transform.SetParent(card,false);Preferred(copy.rectTransform);
            }
        }
        static void PrepareCard()
        {
            var v=View("CardDetail");if(v.scroll)return;
            var aspect=v.window.GetComponent<AspectRatioFitter>();if(aspect)aspect.enabled=false;
            // The previous large-card root is retained in the inactive checkpoint scene.
            var old=v.window.Cast<Transform>().ToArray();
            if(!v.window.Find("Header")){var header=Object.Instantiate(View("Skills").window.Find("Header"),v.window,false);header.name="Header";header.Find("Title").GetComponent<TMP_Text>().text="Talent card";}
            var body=New("Body",v.window);Stretch(body);var viewport=New("Viewport",body);Stretch(viewport);Ensure<RectMask2D>(viewport.gameObject);var im=Ensure<Image>(viewport.gameObject);im.color=Color.clear;
            var content=New("Content",viewport);Box(content,new Vector2(0,1),Vector2.one,Vector2.zero,Vector2.zero);content.pivot=new Vector2(.5f,1);
            var scroll=Ensure<ScrollRect>(body.gameObject);scroll.viewport=viewport;scroll.content=content;v.scroll=scroll;
            foreach(var child in old)if(child.name!="Header")child.SetParent(content,false);
        }
        static void Settings()
        {
            var v=View("Settings");
            foreach(var setting in v.root.GetComponentsInChildren<ClassicSettingsSwitch>(true))
            {
                var row=R(setting.transform.parent);Height(row,80);var rowImage=row.GetComponent<Image>();if(rowImage)rowImage.enabled=false;var surface=row.Find("Bazaar surface");if(surface)surface.gameObject.SetActive(false);
                var title=row.Find("Title")?.GetComponent<TMP_Text>();var copy=row.Find("Copy")?.GetComponent<TMP_Text>();
                if(title){Text(title,16,true);Top(title.rectTransform,56,12,88,26);}if(copy){Text(copy,12,color:theme.muted);Top(copy.rectTransform,56,40,88,36);}
                var painted=row.Find("Painted icon") as RectTransform;if(painted){painted.gameObject.SetActive(true);Fixed(painted,12,20,36,36);RepaintIcon(painted,36);}
                var button=setting.GetComponent<Button>();var c=button.GetComponent<ClassicControl>();if(c)c.htmlStyle=0;
                Box(R(button.transform),new Vector2(1,.5f),new Vector2(1,.5f),new Vector2(-88,-24),new Vector2(-8,24));
                setting.track.anchorMin=setting.track.anchorMax=new Vector2(.5f,.5f);setting.track.sizeDelta=new Vector2(68,32);setting.track.anchoredPosition=Vector2.zero;
                setting.surface.sprite=theme.switchTrack;setting.surface.type=Image.Type.Simple;setting.surface.color=Color.white;
                var outer=setting.track.GetComponent<Image>();outer.sprite=theme.switchTrack;outer.color=Color.white;outer.type=Image.Type.Simple;Stretch(setting.surface.rectTransform);
                setting.thumb.sizeDelta=Vector2.one*24;var thumb=setting.thumb.GetComponent<Image>();thumb.sprite=theme.switchThumb;thumb.color=Color.white;
                foreach(Transform child in setting.thumb)if(child.GetComponent<Image>())child.gameObject.SetActive(false);
                foreach(var t in new[]{setting.onLabel,setting.offLabel})Text(t,12,true,color:theme.paper,align:TextAlignmentOptions.Center);
            }
            foreach(var b in v.scroll.content.GetComponentsInChildren<Button>(true).Where(b=>b.transform.parent==v.scroll.content))
            {Height(R(b.transform),56);Button(b,true);var painted=b.transform.Find("Painted icon") as RectTransform;if(painted){Fixed(painted,12,10,36,36);RepaintIcon(painted,36);}
                foreach(var t in b.GetComponentsInChildren<TMP_Text>(true)){if(t.name=="Chevron"){t.gameObject.SetActive(true);Text(t,22,true,align:TextAlignmentOptions.Center);Box(t.rectTransform,new Vector2(1,0),Vector2.one,new Vector2(-36,0),Vector2.zero);}else if(painted){Text(t,14,true,align:TextAlignmentOptions.MidlineLeft);Stretch(t.rectTransform);t.rectTransform.offsetMin=new Vector2(60,8);t.rectTransform.offsetMax=new Vector2(-40,-8);}}}
        }
        static void RepaintIcon(RectTransform root,float size)
        {
            var gold=root.Find("Gold disc");if(gold){var im=gold.GetComponent<Image>();im.sprite=theme.Circle(size);im.type=Image.Type.Simple;im.color=Color.white;Stretch(im.rectTransform);}
            var cobalt=root.Find("Cobalt disc");if(cobalt)cobalt.gameObject.SetActive(false);
            var art=root.Find("Artwork") as RectTransform;if(art)Stretch(art,6);
        }
    }
}
