using System;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static partial class ClassicBazaarAuthoring
    {
        static void Dialog(ClassicView view)
        {
            if(!view.window||!view.scroll)return;
            var window=view.window;var content=view.scroll.content;
            var spec=specs.TryGetValue(view.route,out var found)?found:null;
            float head=spec?.headerHeight??64,foot=spec?.footerHeight??72;
            var safe=R(window.parent);safe.offsetMin=new Vector2(12,16);safe.offsetMax=new Vector2(-12,-16);
            window.localScale=Vector3.one;Surface(window,theme.window,false);
            var background=window.GetComponent<Image>();background.pixelsPerUnitMultiplier=1;
            var header=R(window.Find("Header"));header.gameObject.SetActive(true);Top(header,1,1,1,head);
            foreach(Transform child in header)
                if(child.name.Contains("ribbon")||child.name=="Subtitle"||child.name=="Header flower"||child.name=="Rule")child.gameObject.SetActive(false);
            var title=header.Find("Title").GetComponent<TMP_Text>();Text(title,22,heading:true,align:TextAlignmentOptions.Center);
            Stretch(title.rectTransform);title.rectTransform.offsetMin=new Vector2(60,12);title.rectTransform.offsetMax=new Vector2(-60,-12);
            if(view.route=="CardDetail")title.text="Talent card";
            var backdrop=New("Bazaar header paper",header);Stretch(backdrop);backdrop.SetAsFirstSibling();Ignore(backdrop);var hp=Ensure<Image>(backdrop.gameObject);hp.sprite=null;hp.color=theme.raised;hp.raycastTarget=false;
            var rule=New("Bazaar header rule",header);Box(rule,new Vector2(0,0),new Vector2(1,0),Vector2.zero,new Vector2(0,1));var ri=Ensure<Image>(rule.gameObject);ri.sprite=null;ri.color=theme.line;ri.raycastTarget=false;
            foreach(var button in header.GetComponentsInChildren<Button>(true))
            {
                if(button.name.Contains("Close")){Box(R(button.transform),new Vector2(1,1),new Vector2(1,1),new Vector2(-56,-56),new Vector2(-8,-8));Button(button);foreach(var t in button.GetComponentsInChildren<TMP_Text>(true)){t.text="×";Text(t,26,true,color:theme.paper,align:TextAlignmentOptions.Center);Stretch(t.rectTransform);}}
                else if(button.name.Contains("Information")||button.name.Contains("Info"))
                {
                    var face=button.targetGraphic as Image;if(face){face.sprite=null;face.color=Color.clear;}var control=button.GetComponent<ClassicControl>();if(control){control.htmlStyle=0;control.faceOverride=null;}
                    Fixed(R(button.transform),8,8,48,48);var circle=button.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.gameObject!=button.gameObject);
                    if(circle){circle.sprite=theme.information;circle.type=Image.Type.Simple;circle.color=Color.white;Box(circle.rectTransform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-12,-12),new Vector2(12,12));}
                    foreach(var t in button.GetComponentsInChildren<TMP_Text>(true)){t.gameObject.SetActive(true);if(string.IsNullOrEmpty(t.text))t.text="i";Text(t,16,true,color:theme.ink,align:TextAlignmentOptions.Center);Stretch(t.rectTransform);}
                }
            }
            var footer=New("Bazaar actions",window);Box(footer,Vector2.zero,new Vector2(1,0),new Vector2(1,1),new Vector2(-1,Mathf.Max(0,foot)+1));
            var controls=window.GetComponentsInChildren<Button>(true).Where(b=>b.transform.parent==window).ToArray();
            foreach(var button in controls){button.transform.SetParent(footer,false);Button(button,button.GetComponent<ClassicControl>()?.htmlStyle==2);Height(R(button.transform),48);}
            var flow=Flow(footer,12,12,8);flow.childForceExpandHeight=false;
            footer.gameObject.SetActive(foot>0);
            if(foot>0){var fp=Ensure<Image>(footer.gameObject);fp.sprite=null;fp.color=theme.raised;fp.raycastTarget=false;}
            var footerRule=New("Bazaar footer rule",footer);Ignore(footerRule);Top(footerRule,0,0,0,1);var fr=Ensure<Image>(footerRule.gameObject);fr.color=theme.line;fr.raycastTarget=false;
            var body=R(view.scroll.transform);Box(body,Vector2.zero,Vector2.one,new Vector2(1,foot+1),new Vector2(-1,-head-1));
            Flow(content,12,16,12);
            var fitter=Ensure<ContentSizeFitter>(content.gameObject);fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            foreach(var image in new[]{body.GetComponent<Image>(),view.scroll.viewport.GetComponent<Image>(),content.GetComponent<Image>()})if(image){image.color=Color.clear;image.sprite=null;}
            view.scroll.horizontal=false;view.scroll.vertical=true;view.scroll.movementType=ScrollRect.MovementType.Clamped;
            view.preferredHeight=spec?.desiredHeight??Mathf.Max(280,view.preferredHeight);
            foreach(var fit in ui.GetComponent<ClassicLayout>().infoRows)
            {
                if(!fit.layout||!fit.layout.transform.IsChildOf(content))continue;
                Info(fit);
            }
            var oldPaper=window.Find("Gouache paper");if(oldPaper)oldPaper.gameObject.SetActive(false);
            var oldFlat=window.Find("Single-frame paper");if(oldFlat)oldFlat.gameObject.SetActive(false);
            var paper=New("Bazaar parchment",window);Stretch(paper,1);paper.SetAsFirstSibling();var pi=Ensure<Image>(paper.gameObject);pi.sprite=theme.paperTexture;pi.type=Image.Type.Tiled;pi.pixelsPerUnitMultiplier=theme.paperTexture?theme.paperTexture.rect.width/180:1;pi.raycastTarget=false;
            Ensure<Mask>(window.gameObject).showMaskGraphic=true;
        }
        static void Info(ClassicInfoFit fit)
        {
            var root=R(fit.layout.transform);Surface(root,theme.section);Flow(root,12,12,8);
            fit.layout.minHeight=0;fit.layout.preferredHeight=-1;fit.layout.flexibleHeight=0;
            var texts=root.GetComponentsInChildren<TMP_Text>(true).Where(t=>!Legacy(t.transform)&&!t.GetComponentInParent<Button>(true)&&!t.GetComponentInParent<TMP_InputField>(true)).ToArray();
            foreach(Transform child in root.Cast<Transform>().ToArray())
                if(child.GetComponent<Image>()&&!child.GetComponent<TMP_Text>())Ignore(R(child));
            foreach(var t in texts)
            {
                if(t==fit.values)continue;
                bool heading=t!=fit.copy;
                t.transform.SetParent(root,false);Text(t,heading?18:14,heading:heading,color:heading?theme.ink:theme.muted);
                var layout=Ensure<LayoutElement>(t.gameObject);layout.minHeight=0;layout.preferredHeight=-1;layout.flexibleHeight=0;
            }
            if(fit.values)
            {
                // Paired statistic columns keep their existing live bindings.
                Text(fit.values,14,true,align:TextAlignmentOptions.TopRight);
            }
        }
        static void Panel(ClassicView view)
        {
            if(!view.scroll)return;var root=R(view.root.transform);
            var header=root.Find("Header") as RectTransform;
            if(view.route!="Home"&&header)
            {
                Top(header,0,0,0,56);var image=header.GetComponent<Image>();if(image){image.sprite=null;image.color=theme.paper;}
                var title=header.Find("Title")?.GetComponent<TMP_Text>();if(title){Text(title,28,heading:true,align:TextAlignmentOptions.MidlineLeft);Stretch(title.rectTransform);title.rectTransform.offsetMin=new Vector2(12,0);title.rectTransform.offsetMax=new Vector2(-112,0);}
                foreach(var b in header.GetComponentsInChildren<Button>(true))
                {
                    float right=b.name.Contains("Collapse")?8:56;Box(R(b.transform),Vector2.one,Vector2.one,new Vector2(-right-48,-52),new Vector2(-right,-4));
                    foreach(var t in b.GetComponentsInChildren<TMP_Text>(true))Text(t,16,true,align:TextAlignmentOptions.Center);
                }
                Box(R(view.scroll.transform),Vector2.zero,Vector2.one,Vector2.zero,new Vector2(0,-56));
            }
            if(view.route!="Home")Flow(view.scroll.content,12,12,8);
            view.scroll.horizontal=false;view.scroll.vertical=true;view.scroll.movementType=ScrollRect.MovementType.Clamped;
            foreach(var toolbar in view.scroll.content.Cast<Transform>().Where(t=>t.name=="Toolbar"))
            {
                Height(R(toolbar),56);
                foreach(var t in toolbar.GetComponentsInChildren<TMP_Text>(true))Text(t,14,true);
            }
        }
        static void Navigation()
        {
            for(int i=0;i<ui.navigation.Length;i++)
            {
                var b=ui.navigation[i];Button(b,true);b.GetComponent<ClassicControl>().htmlStyle=2;
                var rect=R(b.transform);rect.offsetMin=new Vector2(2,4);rect.offsetMax=new Vector2(-2,-4);
                var icon=b.transform.Find("Icon") as RectTransform;if(icon)Box(icon,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-15,-34),new Vector2(15,-4));
                foreach(var t in b.GetComponentsInChildren<TMP_Text>(true)){Text(t,12,true,align:TextAlignmentOptions.Center);Box(t.rectTransform,Vector2.zero,new Vector2(1,0),new Vector2(0,4),new Vector2(0,18));}
            }
        }
        static void Home()
        {
            var view=ui.panels.Single(v=>v.route=="Home");
            if(!view.scroll)
            {
                var children=view.root.transform.Cast<Transform>().ToArray();
                var scroll=New("Bazaar scroll",view.root.transform);Stretch(scroll);
                var viewport=New("Viewport",scroll);Stretch(viewport);Ensure<RectMask2D>(viewport.gameObject);
                var backing=Ensure<Image>(viewport.gameObject);backing.color=Color.clear;backing.raycastTarget=true;
                var holder=New("Content",viewport);Stretch(holder);
                foreach(var child in children)child.SetParent(holder,false);
                view.scroll=Ensure<ScrollRect>(scroll.gameObject);view.scroll.viewport=viewport;view.scroll.content=holder;view.scroll.horizontal=false;view.scroll.movementType=ScrollRect.MovementType.Clamped;
            }
            var content=view.scroll.content;
            foreach(var layout in content.GetComponents<LayoutGroup>())Object.DestroyImmediate(layout);
            var fitter=content.GetComponent<ContentSizeFitter>();if(fitter)fitter.enabled=false;
            Box(content,new Vector2(0,1),Vector2.one,new Vector2(0,-Mathf.Max(256,view.scroll.viewport.rect.height)),Vector2.zero);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
            ui.bazaarHomeContent=content;ui.bazaarHomeViewport=view.scroll.viewport;
            ui.tapTarget.scroll=view.scroll;
            var goal=Bound("home-goal");var next=R(goal.transform.parent);Top(next,12,24,12,56);Surface(next,theme.section);
            foreach(var t in next.GetComponentsInChildren<TMP_Text>(true))
            {
                if(t==goal){Text(t,13,true);Top(t.rectTransform,52,20,52,36);}
                else if(t.text.Contains("NEXT")){Text(t,12,true,color:theme.muted);Top(t.rectTransform,52,6,52,16);}
            }
            var layoutOwner=ui.GetComponent<ClassicLayout>();var income=layoutOwner.income;
            Box(income,Vector2.zero,new Vector2(1,0),new Vector2(12,88),new Vector2(-12,172));Surface(income,theme.section);
            var sale=Bound("home-income");var tap=Bound("home-tap");var total=Bound("home-total");
            foreach(var t in income.GetComponentsInChildren<TMP_Text>(true))Text(t,12,true,color:theme.muted);
            Top(sale.rectTransform,12,22,174,20);Text(sale,16,true);
            Top(tap.rectTransform,12,58,174,20);Text(tap,16,true);
            Top(total.rectTransform,182,40,12,28);Text(total,22,true);
            foreach(var image in income.GetComponentsInChildren<Image>(true))if(image.name=="Chart")image.gameObject.SetActive(false);
            var arrow=income.Find("Rising arrow");if(arrow)arrow.gameObject.SetActive(false);
            for(int i=0;i<3;i++){var bar=New("Bazaar sales bar "+i,income);Fixed(bar,182+i*6,36-(12+i*6),5,12+i*6);var im=Ensure<Image>(bar.gameObject);im.sprite=null;im.color=theme.gold;im.raycastTarget=false;}
            foreach(var t in income.GetComponentsInChildren<TMP_Text>(true))
            {
                if(t.text=="AUTOMATIC SALES")Top(t.rectTransform,12,8,174,16);
                else if(t.text=="PER TAP")Top(t.rectTransform,12,44,174,16);
                else if(t.text=="EARNING NOW")Top(t.rectTransform,206,18,12,18);
            }
            Box(layoutOwner.skills,Vector2.zero,new Vector2(1,0),Vector2.zero,new Vector2(0,80));
            for(int i=0;i<3;i++)
            {
                var b=ui.homeSkills[i];var r=R(b.transform);Box(r,new Vector2(i/3f,0),new Vector2((i+1)/3f,1),Vector2.zero,Vector2.zero);
                var disc=r.Find("Skill disc") as RectTransform;if(disc){Fixed(disc,38,0,44,44);var im=disc.GetComponent<Image>();if(im){im.sprite=theme.Circle(44);im.color=Color.white;im.type=Image.Type.Simple;}}
                foreach(var t in b.GetComponentsInChildren<TMP_Text>(true))
                {
                    bool state=t==Bound("home-skill-"+i);Text(t,12,!state,color:state?theme.muted:theme.ink,align:TextAlignmentOptions.Center);Top(t.rectTransform,0,state?62:46,0,16);
                }
            }
            var shop=ui.GetComponent<ClassicLayout>().world;
            foreach(var t in shop.GetComponentsInChildren<TMP_Text>(true))
            {
                if(t.name=="Nameplate"){Text(t,t.text.Contains("Celebration")?6:8,color:new Color32(57,41,28,255),align:TextAlignmentOptions.Center);t.enableWordWrapping=false;}
                else if(t.text=="CASH BALANCE")Text(t,10,true,color:theme.paper,align:TextAlignmentOptions.Center);
                else if(t.text.Contains("ROSHAN"))Text(t,7,true,color:new Color32(246,213,144,255),align:TextAlignmentOptions.Center);
                else if(t.text=="TAP TO EARN")Text(t,13,heading:true,color:theme.ink,align:TextAlignmentOptions.Center);
            }
            // A caption shares its own raycast label; explicit roles above remain authoritative.
            foreach(var t in shop.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.name=="Nameplate")){Text(t,t.text.Contains("Celebration")?6:8,color:new Color32(57,41,28,255),align:TextAlignmentOptions.Center);t.enableWordWrapping=false;}
            Text(Bound("cash"),31,true,color:new Color32(244,203,119,255),align:TextAlignmentOptions.Center);
            var sign=Bound("shop-name");Text(sign,16,true,color:theme.paper,align:TextAlignmentOptions.Center);sign.enableAutoSizing=true;sign.fontSizeMin=12;sign.fontSizeMax=16;sign.enableWordWrapping=false;sign.overflowMode=TextOverflowModes.Ellipsis;
        }
    }
}
