using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    /// <summary>Scoped Editor migration of the reviewed HTML product overview. No player factories.</summary>
    public static class ClassicProductOverviewAuthoring
    {
        const string Art = "Assets/Game/ClassicUI/ProductOverview/";
        const float S = .75f; // Reviewed 480px app -> unchanged production 360-unit canvas.
        static ClassicUI ui;
        static TMP_Text serifTemplate, bodyTemplate, boldTemplate;
        static Sprite cardSprite, barSprite;
        static readonly Color Ink = new Color32(22,75,128,255);
        static readonly Color Muted = new Color32(85,111,128,255);

        [MenuItem("Rocket Raja/Classic/Match reviewed product overview")]
        public static string Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath("Assets/Scenes/Main.unity");
            if (!scene.isLoaded || scene.isDirty) throw new InvalidOperationException("Open clean Main; preserve existing dirty work first");
            ui = scene.GetRootGameObjects().Single(g=>g.name=="ClassicProductionCanvas").GetComponent<ClassicUI>();
            var view = ui.dialogs.Single(v=>v.route=="ProductDetail");
            if (view.root.transform.parent.Find("ProductDetail__LegacyDisabled_20261031")) throw new InvalidOperationException("Already migrated; refine the saved replacement");
            ImportArt();
            var old = view.root;
            serifTemplate = old.GetComponentsInChildren<TMP_Text>(true).First(t=>t.name=="Title");
            bodyTemplate = old.GetComponentsInChildren<TMP_Text>(true).First(t=>t.name=="Body");
            boldTemplate = old.GetComponentsInChildren<TMP_Text>(true).First(t=>t.name=="Level");
            old.SetActive(false); old.name="ProductDetail__LegacyDisabled_20261031";
            var replacement = Object.Instantiate(old,old.transform.parent); replacement.name="ProductDetail";
            Undo.RegisterCreatedObjectUndo(replacement,"Reviewed product overview");
            view.root=replacement; view.group=replacement.GetComponent<CanvasGroup>();
            view.window=(RectTransform)replacement.transform.Find("Safe dialog bounds/Window");
            view.scroll=view.window.Find("Body").GetComponent<ScrollRect>();
            view.preferredHeight=646.3f*S;
            var bounds=(RectTransform)view.window.parent;
            bounds.offsetMin=new Vector2(12,14)*S; bounds.offsetMax=new Vector2(-12,-14)*S;
            var header=(RectTransform)view.window.Find("Header"); header.sizeDelta=new Vector2(0,96*S);
            header.Find("Red title ribbon").gameObject.SetActive(false);
            header.Find("Rule").gameObject.SetActive(false);
            var title=header.Find("Title").GetComponent<TMP_Text>();
            Configure(title,serifTemplate.font,21,Ink,TextAlignmentOptions.Center);
            Box(title.rectTransform,Vector2.zero,Vector2.one,new Vector2(74,8)*S,new Vector2(-74,-8)*S);
            TextBinding("product-title",title);
            var close=(RectTransform)header.Find("Close");
            Box(close,Vector2.one,Vector2.one,new Vector2(-70,-70)*S,new Vector2(-22,-22)*S);
            close.GetComponentInChildren<TMP_Text>(true).fontSize=25*S;
            var closeLabel=close.GetComponentInChildren<TMP_Text>(true);Stretch(closeLabel.rectTransform,2);closeLabel.overflowMode=TextOverflowModes.Overflow;
            var info=(RectTransform)header.Find("Information");
            Box(info,new Vector2(0,1),new Vector2(0,1),new Vector2(22,-70)*S,new Vector2(70,-22)*S);
            var infoCircle=(RectTransform)info.Find("Selected information circle");infoCircle.sizeDelta=Vector2.one*23*S;
            info.GetComponentInChildren<TMP_Text>(true).fontSize=16*S;
            var flower=New("Header flower",header);flower.anchorMin=flower.anchorMax=new Vector2(.5f,0);flower.sizeDelta=new Vector2(14,12)*S;
            Image(flower,AssetDatabase.LoadAssetAtPath<Sprite>(Art+"flower.png"),Color.white);
            var paper=New("Gouache paper",view.window);Stretch(paper,0);paper.SetAsFirstSibling();
            var paperImage=Image(paper,AssetDatabase.LoadAssetAtPath<Sprite>(Art+"paper.png"),Color.white);paperImage.type=UnityEngine.UI.Image.Type.Tiled;
            view.window.GetComponent<Image>().color=new Color32(255,243,216,255);
            var body=(RectTransform)view.scroll.transform;
            Box(body,Vector2.zero,Vector2.one,new Vector2(8,8)*S,new Vector2(-8,-96)*S);
            var content=view.scroll.content;
            foreach(Transform child in content.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
            var group=content.GetComponent<VerticalLayoutGroup>();
            group.padding=new RectOffset(9,9,11,18);group.spacing=0;group.childControlWidth=group.childControlHeight=true;group.childForceExpandHeight=false;group.childForceExpandWidth=true;
            content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var identity=Row("Product identity",content,110*S);
            var disc=Fixed("Gold portrait disc",identity,0,0,110,110);
            Image(disc,AssetDatabase.LoadAssetAtPath<Sprite>(Art+"portrait.png"),Color.white);
            var artwork=New("Artwork",disc);Stretch(artwork,16*S);
            var image=Image(artwork,ui.productArt[0],Color.white);image.preserveAspect=true;ImageBinding("product-detail-art",image);
            var name=New("Product name",identity);Box(name,new Vector2(0,0),Vector2.one,new Vector2(128*S,68.6f*S),new Vector2(0,-15*S));
            Text("product-detail-name",name,"Phuljhari",24*S,serifTemplate,Ink);
            var level=New("Level",identity);Box(level,Vector2.zero,Vector2.one,new Vector2(128*S,37.7f*S),new Vector2(0,-49.5f*S));
            Text("product-detail-level",level,"Lvl : 75",19*S,boldTemplate,new Color32(91,104,113,255));
            var subtitle=New("Signature",identity);Box(subtitle,Vector2.zero,Vector2.one,new Vector2(128*S,15*S),new Vector2(0,-79*S));
            Text("product-subtitle",subtitle,"Signature I",12*S,boldTemplate,new Color32(99,120,138,255));
            Gap(content,16*S);
            var earnings=Row("Earnings",content,21*S);
            Text("product-earnings",earnings,"Earns ₹0 / second",14*S,serifTemplate,new Color32(72,99,58,255),TextAlignmentOptions.Center);
            Gap(content,14*S);
            Card(content,"Level milestone","product-milestone","Lvl 100 · profit ×4","product-progress","product-progress-label","Lvl 100","product-milestone-reward","Milestone reward","product-milestone-copy","Milestones apply automatically. Buy cash upgrades for boosts, then use Max to level up.");
            Gap(content,12*S);
            Card(content,"Product synergy","synergy-title","Next synergy bonus","synergy-progress","synergy-progress-label","2/8 products at Lvl 50","synergy-reward","All products: ×1 → another ×2","synergy-copy","Chakri (29), Rockets (12), Small crackers (5), Cracker strings (3), Aerial shots (1), Celebration boxes (1)");
            ui.GetComponent<ClassicLayout>().infoRows=ui.GetComponent<ClassicLayout>().infoRows.Where(r=>r.layout&&!r.layout.transform.IsChildOf(old.transform)).ToArray();
            ui.guidance.rows=ui.guidance.rows.Where(r=>r.route!="ProductDetail").ToArray();
            // Shared touch padding expands these reference-sized controls to at least 48 logical units.
            var layout=ui.GetComponent<ClassicLayout>();layout.touchGraphics=layout.touchGraphics.Concat(new[]{close.GetComponent<Graphic>(),info.GetComponent<Graphic>()}).Where(g=>g).Distinct().ToArray();
            replacement.SetActive(true);Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(content);layout.Apply();Canvas.ForceUpdateCanvases();
            replacement.SetActive(false);view.group.alpha=1;view.group.interactable=true;view.group.blocksRaycasts=true;
            EditorUtility.SetDirty(ui);EditorUtility.SetDirty(layout);EditorUtility.SetDirty(ui.guidance);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Saved reviewed ProductDetail with complete inactive legacy root, live bindings, native cards and unchanged production canvas";
        }

        static void Card(Transform parent,string name,string headingKey,string heading,string fillKey,string labelKey,string progress,string rewardKey,string reward,string copyKey,string copy)
        {
            var card=New(name,parent);Image(card,cardSprite,Color.white);
            var g=card.gameObject.AddComponent<VerticalLayoutGroup>();g.padding=new RectOffset(11,11,11,11);g.spacing=0;g.childControlWidth=g.childControlHeight=true;g.childForceExpandWidth=true;g.childForceExpandHeight=false;
            var h=Row("Heading",card,23*S);Text(headingKey,h,heading,19*S,serifTemplate,Ink,TextAlignmentOptions.Center);Gap(card,12*S);
            var bar=Row("Progress",card,26*S);Image(bar,barSprite,Color.white);
            var track=New("Track",bar);Stretch(track,2*S);Image(track,null,new Color32(55,94,103,255));
            var fill=New("Fill",track);Stretch(fill,0);var f=Image(fill,ui.imageBindings.First(b=>b.key==fillKey).image.sprite,new Color32(69,183,173,255));f.type=UnityEngine.UI.Image.Type.Filled;f.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;f.fillOrigin=0;f.fillAmount=.5f;ImageBinding(fillKey,f);
            var label=New("Progress label",bar);Box(label,Vector2.zero,Vector2.one,new Vector2(7*S,0),new Vector2(-7*S,0));var t=Text(labelKey,label,progress,12*S,boldTemplate,new Color32(255,251,233,255),TextAlignmentOptions.Right);t.enableAutoSizing=true;t.fontSizeMin=8*S;t.fontSizeMax=12*S;
            Gap(card,10*S);var r=Row("Reward",card,18.2f*S);Text(rewardKey,r,reward,14*S,boldTemplate,Muted,TextAlignmentOptions.Center);Gap(card,8*S);
            var paragraph=New("Explanation",card);var p=Text(copyKey,paragraph,copy,12*S,bodyTemplate,Ink,TextAlignmentOptions.TopLeft);p.lineSpacing=3*S;p.overflowMode=TextOverflowModes.Overflow;
            p.rectTransform.SetParent(card,false);p.name="Explanation";Object.DestroyImmediate(paragraph.gameObject);
            var minimum=p.gameObject.GetComponent<LayoutElement>()??p.gameObject.AddComponent<LayoutElement>();minimum.minHeight=36*S;
        }
        static TMP_Text Text(string key,RectTransform rect,string value,float size,TMP_Text template,Color color,TextAlignmentOptions alignment=TextAlignmentOptions.Left)
        {
            // Clone the authored text including its saved fallback submeshes, never runtime TMP factories.
            var text=Object.Instantiate(template,rect,false);text.name="Text";Stretch(text.rectTransform,0);
            if(rect.GetComponent<LayoutElement>()) { var e=text.gameObject.AddComponent<LayoutElement>();e.ignoreLayout=true; }
            Configure(text,template.font,size,color,alignment);text.text=value;
            TextBinding(key,text);return text;
        }
        static void Configure(TMP_Text t,TMP_FontAsset font,float size,Color color,TextAlignmentOptions align)
        {t.font=font;t.fontSize=size;t.enableAutoSizing=false;t.fontStyle=font.name=="Card Serif SDF"?FontStyles.Bold:FontStyles.Normal;t.color=color;t.alignment=align;t.margin=Vector4.zero;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Overflow;t.raycastTarget=false;t.richText=false;}
        static void TextBinding(string key,TMP_Text t){var b=ui.textBindings.FirstOrDefault(x=>x.key==key);if(b!=null)b.text=t;else ui.textBindings=ui.textBindings.Concat(new[]{new ClassicTextBinding{key=key,text=t}}).ToArray();}
        static void ImageBinding(string key,Image i){ui.imageBindings.First(x=>x.key==key).image=i;}
        static RectTransform New(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        static RectTransform Row(string name,Transform parent,float height){var r=New(name,parent);var e=r.gameObject.AddComponent<LayoutElement>();e.minHeight=e.preferredHeight=height;return r;}
        static void Gap(Transform parent,float height)=>Row("Spacing",parent,height);
        static RectTransform Fixed(string name,Transform parent,float x,float y,float w,float h){var r=New(name,parent);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y)*S;r.sizeDelta=new Vector2(w,h)*S;return r;}
        static void Stretch(RectTransform r,float inset)=>Box(r,Vector2.zero,Vector2.one,Vector2.one*inset,-Vector2.one*inset);
        static void Box(RectTransform r,Vector2 min,Vector2 max,Vector2 low,Vector2 high){r.anchorMin=min;r.anchorMax=max;r.pivot=new Vector2(.5f,.5f);r.offsetMin=low;r.offsetMax=high;}
        static Image Image(RectTransform r,Sprite sprite,Color color){var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.color=color;i.raycastTarget=false;if(sprite&&sprite.border.sqrMagnitude>0){i.type=UnityEngine.UI.Image.Type.Sliced;i.pixelsPerUnitMultiplier=2;}return i;}

        static void ImportArt()
        {
            Directory.CreateDirectory(Art);
            Bake("portrait",256,256,(x,y)=>{float d=Vector2.Distance(new Vector2(x,y),new Vector2(127.5f,127.5f));if(d>127.5f)return Color.clear;if(d>120.5f)return new Color32(245,220,157,255);if(d>114)return new Color32(196,154,73,255);return Color.Lerp(new Color32(255,249,223,255),new Color32(237,200,115,255),Mathf.Clamp01(d/150));});
            Bake("card",96,96,(x,y)=>Rounded(x,y,96,12,2.5f,new Color32(200,167,100,255),new Color32(255,248,225,105)));
            Bake("bar",64,64,(x,y)=>Rounded(x,y,64,7,3,new Color32(169,139,71,255),new Color32(55,94,103,255)));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach(string name in new[]{"paper","flower","portrait","card","bar"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(Art+name+".png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=name=="paper"?133.3333f:100;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=name=="paper"?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
                importer.spriteBorder=name=="card"?Vector4.one*16:name=="bar"?Vector4.one*10:Vector4.zero;importer.SaveAndReimport();
            }
            cardSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"card.png");barSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"bar.png");
        }
        static Color Rounded(float x,float y,int size,float radius,float border,Color rim,Color fill)
        {float qx=Mathf.Abs(x-(size-1)*.5f)-(size*.5f-radius),qy=Mathf.Abs(y-(size-1)*.5f)-(size*.5f-radius);float distance=new Vector2(Mathf.Max(qx,0),Mathf.Max(qy,0)).magnitude+Mathf.Min(Mathf.Max(qx,qy),0)-radius;var c=distance>-border?rim:fill;c.a*=Mathf.Clamp01(-distance);return c;}
        static void Bake(string name,int w,int h,Func<int,int,Color> pixel){var t=new Texture2D(w,h,TextureFormat.RGBA32,false);var colors=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)colors[y*w+x]=pixel(x,y);t.SetPixels(colors);t.Apply();File.WriteAllBytes(Art+name+".png",t.EncodeToPNG());Object.DestroyImmediate(t);}

        public static string RefineTypography()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var scene=SceneManager.GetSceneByPath("Assets/Scenes/Main.unity");
            if(!scene.isLoaded||scene.isDirty)throw new InvalidOperationException("Open clean Main first");
            var owner=scene.GetRootGameObjects().Single(g=>g.name=="ClassicProductionCanvas").GetComponent<ClassicUI>();
            var v=owner.dialogs.Single(d=>d.route=="ProductDetail");ImportArt();
            foreach(var t in v.root.GetComponentsInChildren<TMP_Text>(true)){t.overflowMode=TextOverflowModes.Overflow;if(t.font.name=="Card Serif SDF"&&t.name!="i")t.fontStyle=FontStyles.Bold;}
            Stretch(v.window.Find("Header/Close/Label").GetComponent<RectTransform>(),2);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Saved non-clipping font boxes and calibrated rounded card outlines";
        }
    }
}
