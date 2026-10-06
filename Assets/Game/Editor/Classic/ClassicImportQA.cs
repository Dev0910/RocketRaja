using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Editor-only refinement and repeatable review of the saved import.</summary>
    public static class ClassicImportQA
    {
        public static ClassicUI UI => SceneManager.GetSceneByPath(BuildClassicUI.ScenePath).GetRootGameObjects().Single(g=>g.name==BuildClassicUI.RootName).GetComponent<ClassicUI>();
        static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        public static string Refine()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Refine in Edit Mode");
            var ui=UI;
            if(!ui.transform.Find("Viewport backing")){var backing=new GameObject("Viewport backing",typeof(RectTransform),typeof(Image));backing.transform.SetParent(ui.transform,false);backing.transform.SetAsFirstSibling();var rect=(RectTransform)backing.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;var face=backing.GetComponent<Image>();face.color=new Color32(16,36,55,255);face.raycastTarget=false;}
            foreach(var t in ui.GetComponentsInChildren<TMP_Text>(true))
            {
                if(!t.enableAutoSizing){t.fontSizeMax=t.fontSize;t.fontSizeMin=t.fontSize*.72f;t.enableAutoSizing=true;}
                if(t.text=="⌄")t.text="↓";
                t.text=t.text.Replace("✦","·");
            }
            var home=ui.transform.Find("SafeArea/Panels/Home");
            var income=(RectTransform)home.Find("Income");income.sizeDelta=new Vector2(income.sizeDelta.x,77);income.anchoredPosition=new Vector2(0,88);
            var skills=(RectTransform)home.Find("Skills");skills.sizeDelta=new Vector2(0,82);
            var circle=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            foreach(var image in skills.GetComponentsInChildren<Image>(true))if(image.name=="Skill disc"||image.name=="Ready glow"){image.sprite=circle;image.type=Image.Type.Simple;}
            var tap=ui.transform.Find("SafeArea/Panels/Player/Scroll/Viewport/Content/Salesmanship/Train").GetComponent<ClassicControl>();tap.action="BuyLevels";tap.parameter="-1";
            const string materialPath="Assets/Game/ClassicUI/Undiscovered.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);if(!material){material=new Material(Shader.Find("UI/Classic Grayscale"));AssetDatabase.CreateAsset(material,materialPath);}ui.undiscoveredMaterial=material;
            ui.motion.coinSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Coins.png");
            ui.redButton=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Folk Red.png");
            foreach(var image in ui.GetComponentsInChildren<Image>(true))if(image.type==Image.Type.Filled&&!image.sprite)image.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            foreach(var panel in ui.panels.Where(p=>p.route!="Home"))
            {
                var close=panel.root.transform.Find("Header/Collapse").GetComponent<Image>();close.sprite=null;close.color=Color.clear;
            }
            var info=ui.transform.Find("SafeArea/Panels/Legacy/Header/Information");info.GetComponent<Image>().sprite=null;info.GetComponent<Image>().color=Color.clear;
            if(!info.Find("Info circle"))
            {
                var ring=new GameObject("Info circle",typeof(RectTransform),typeof(Image));ring.transform.SetParent(info,false);ring.transform.SetAsFirstSibling();var rt=(RectTransform)ring.transform;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(23,23);var rim=ring.GetComponent<Image>();rim.sprite=circle;rim.color=new Color32(22,75,128,255);rim.raycastTarget=false;
                var center=new GameObject("Center",typeof(RectTransform),typeof(Image));center.transform.SetParent(ring.transform,false);var cr=(RectTransform)center.transform;cr.anchorMin=Vector2.zero;cr.anchorMax=Vector2.one;cr.offsetMin=Vector2.one*2;cr.offsetMax=-Vector2.one*2;var face=center.GetComponent<Image>();face.sprite=circle;face.color=new Color32(255,244,219,255);face.raycastTarget=false;
            }
            var full=ui.dialogs.Single(v=>v.route=="CardDetail").window;
            full.Find("Card name").GetComponent<TMP_Text>().fontStyle=FontStyles.Bold;
            var permanent=(RectTransform)full.Find("Permanent");permanent.anchorMin=new Vector2(.12f,.065f);permanent.anchorMax=new Vector2(.88f,.105f);
            var flavor=(RectTransform)full.Find("Flavor");flavor.anchorMin=new Vector2(.10f,.105f);flavor.anchorMax=new Vector2(.90f,.155f);
            var layout=ui.GetComponent<ClassicLayout>();if(!layout)layout=ui.gameObject.AddComponent<ClassicLayout>();layout.ui=ui;layout.safe=(RectTransform)ui.transform.Find("SafeArea");layout.world=(RectTransform)layout.safe.Find("Shop");layout.panels=(RectTransform)layout.safe.Find("Panels");layout.income=income;layout.skills=skills;
            layout.infoRows=ui.GetComponentsInChildren<LayoutElement>(true).Where(e=>e.transform.Find("Heading")&&e.transform.Find("Body")&&e.transform.Find("Body").GetComponent<TMP_Text>()).Select(e=>new ClassicInfoFit{layout=e,copy=e.transform.Find("Body").GetComponent<TMP_Text>()}).ToArray();
            foreach(var fit in layout.infoRows)fit.copy.alignment=TextAlignmentOptions.TopLeft;
            var product=ui.dialogs.Single(v=>v.route=="ProductDetail");var productBounds=(RectTransform)product.window.parent;productBounds.offsetMax=new Vector2(-12,-52);
            var header=(RectTransform)product.window.Find("Header");header.Find("Rule").gameObject.SetActive(false);
            var title=(RectTransform)header.Find("Title");title.anchorMin=new Vector2(.12f,1);title.anchorMax=new Vector2(.88f,1);title.pivot=new Vector2(.5f,.5f);title.sizeDelta=new Vector2(0,54);title.anchoredPosition=new Vector2(0,-2);var productTitle=title.GetComponent<TMP_Text>();productTitle.color=new Color32(255,244,219,255);productTitle.alignment=TextAlignmentOptions.Center;
            if(!header.Find("Red title ribbon")){var ribbon=new GameObject("Red title ribbon",typeof(RectTransform),typeof(Image));ribbon.transform.SetParent(header,false);ribbon.transform.SetAsFirstSibling();var rr=(RectTransform)ribbon.transform;rr.anchorMin=title.anchorMin;rr.anchorMax=title.anchorMax;rr.pivot=title.pivot;rr.sizeDelta=title.sizeDelta;rr.anchoredPosition=title.anchoredPosition;var image=ribbon.GetComponent<Image>();image.sprite=ui.redButton;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=3;image.raycastTarget=false;}
            var closeRect=(RectTransform)header.Find("Close");closeRect.anchorMin=closeRect.anchorMax=new Vector2(1,1);closeRect.pivot=new Vector2(.5f,.5f);closeRect.sizeDelta=new Vector2(48,48);closeRect.anchoredPosition=new Vector2(-16,-2);var closeImage=closeRect.GetComponent<Image>();closeImage.sprite=circle;closeImage.type=Image.Type.Simple;closeImage.color=new Color32(168,33,73,255);
            ((RectTransform)product.scroll.transform).offsetMax=new Vector2(0,-36);
            var identity=product.scroll.content.Find("Product identity").GetComponent<Image>();identity.sprite=null;identity.color=Color.clear;
            var sign=ui.transform.Find("SafeArea/Shop/Shop sign");if(!sign.GetComponent<Button>()){var text=sign.GetComponent<TMP_Text>();text.raycastTarget=true;var b=sign.gameObject.AddComponent<Button>();b.targetGraphic=text;var c=sign.gameObject.AddComponent<ClassicControl>();c.owner=ui;c.action="Navigate";c.parameter="Naming";}
            foreach(var button in ui.GetComponentsInChildren<Button>(true))button.navigation=new Navigation{mode=Navigation.Mode.None};
            Canvas.ForceUpdateCanvases();layout.Apply();Canvas.ForceUpdateCanvases();layout.Apply();EditorUtility.SetDirty(ui);EditorUtility.SetDirty(ui.motion);EditorUtility.SetDirty(layout);EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);EditorSceneManager.SaveScene(ui.gameObject.scene);
            return "Saved typography, circular skills, direct tapping purchases, grayscale material and coin trail bindings";
        }
        public static string Viewport(int width,int height)
        {
            var a=typeof(UnityEditor.Editor).Assembly;var t=a.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(t).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            var group=t.GetProperty("currentGroup").GetValue(singleton);var gt=group.GetType();int count=(int)gt.GetMethod("GetTotalCount").Invoke(group,null),index=-1;
            for(int i=0;i<count;i++){var size=gt.GetMethod("GetGameViewSize").Invoke(group,new object[]{i});var st=size.GetType();if((int)st.GetProperty("width").GetValue(size)==width&&(int)st.GetProperty("height").GetValue(size)==height){index=i;break;}}
            if(index<0){var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),Enum.Parse(a.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),width,height,"Classic "+width+"x"+height);gt.GetMethod("AddCustomSize").Invoke(group,new[]{size});index=count;}
            var vt=a.GetType("UnityEditor.GameView");var window=EditorWindow.GetWindow(vt);vt.GetProperty("selectedSizeIndex",Flags).SetValue(window,index);window.Show();window.Repaint();return width+"x"+height;
        }
        public static string Capture(string path)
        {
            var type=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(type);view.Repaint();
            var target=type.GetField("m_RenderTexture",Flags).GetValue(view) as RenderTexture;if(!target)throw new InvalidOperationException("Game View has not rendered");
            var previous=RenderTexture.active;Texture2D texture=null;
            try{RenderTexture.active=target;texture=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();if(SystemInfo.graphicsUVStartsAtTop){var pixels=texture.GetPixels();var row=new Color[target.width];for(int y=0;y<target.height/2;y++){int other=target.height-1-y;Array.Copy(pixels,y*target.width,row,0,target.width);Array.Copy(pixels,other*target.width,pixels,y*target.width,target.width);Array.Copy(row,0,pixels,other*target.width,target.width);}texture.SetPixels(pixels);texture.Apply();}File.WriteAllBytes(path,texture.EncodeToPNG());return path+" ("+target.width+"x"+target.height+")";}
            finally{RenderTexture.active=previous;if(texture)UnityEngine.Object.DestroyImmediate(texture);}
        }
        public static string Audit()
        {
            var ui=UI;var all=ui.GetComponentsInChildren<Transform>(true);
            var missing=all.Where(t=>t.GetComponents<Component>().Any(c=>!c)).Select(t=>t.name).ToArray();
            var text=ui.GetComponentsInChildren<RocketRaja.RedesignSavedText>(true);
            var report="objects="+all.Length+"; missingScripts="+missing.Length+"; text="+text.Length+"; invalidFonts="+text.Count(t=>!t.font)+"; missingTextSlots="+text.Count(t=>t.savedMaterialSlots.Length<2||t.savedMaterialSlots.Any(s=>!s))+"; dialogs="+ui.dialogs.Length+"; activePanels="+string.Join(",",ui.panels.Where(p=>p.root.activeSelf).Select(p=>p.route))+"; activeDialogs="+string.Join(",",ui.dialogs.Where(p=>p.root.activeSelf).Select(p=>p.route))+"; buttonsWithNavigation="+ui.GetComponentsInChildren<Button>(true).Count(b=>b.navigation.mode!=Navigation.Mode.None)+"; sceneDirty="+ui.gameObject.scene.isDirty;
            if(Application.isPlaying)report+="; initialized="+ui.driver.Initialized+"; current="+ui.CurrentTab+"/"+ui.CurrentDialog;
            return report;
        }
    }
}
