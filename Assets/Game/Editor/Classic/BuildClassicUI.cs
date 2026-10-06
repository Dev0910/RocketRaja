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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    /// <summary>All object, component and geometry creation is Editor-only.</summary>
    public static partial class BuildClassicUI
    {
        public const string ScenePath="Assets/Scenes/Main.unity",RootName="ClassicProductionCanvas";
        const string Local="Assets/Game/ClassicUI/",Shared="Assets/Game/ImportedUI/Art/";
        [Serializable] class ArtEntry { public string key,path,source,sha256; public bool imported; }
        [Serializable] class ArtManifest { public ArtEntry[] assets; }
        static ClassicUI ui;static TMP_FontAsset body,bold,heading,serif;static Sprite blue,ivory,sand,row,rounded;
        static Color Ink=new Color32(22,75,128,255),Cream=new Color32(255,244,219,255),Muted=new Color32(85,111,128,255);
        static readonly Dictionary<string,Sprite> art=new Dictionary<string,Sprite>();
        static readonly List<ClassicTextBinding> texts=new List<ClassicTextBinding>();static readonly List<ClassicImageBinding> images=new List<ClassicImageBinding>();static readonly List<ClassicView> views=new List<ClassicView>();
        static RedesignCatalog data;
        [MenuItem("Rocket Raja/Classic/Build reviewed HTML UI")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Classic authoring requires Edit Mode");
            var scene=SceneManager.GetSceneByPath(ScenePath);if(!scene.isLoaded)throw new InvalidOperationException("Open Main before authoring Classic");
            if(scene.GetRootGameObjects().Any(g=>g.name==RootName))throw new InvalidOperationException("Classic root exists; inspect/refine it instead of rebuilding");
            SceneManager.SetActiveScene(scene);texts.Clear();images.Clear();views.Clear();art.Clear();LoadArt();
            data=new RedesignCatalog(AssetDatabase.LoadAssetAtPath<GameContent>("Assets/Resources/GameContent.asset").redesign);
            string characters=string.Concat(Enumerable.Range(32,224).Select(c=>(char)c))+"₹×→←↓↑✦◆…•–—“”’"+string.Join("",data.Upgrades.Select(u=>u.name+u.effectText+u.requirementText))+string.Join("",data.Cards.Select(c=>c.name));
            body=Font("Body","Assets/Resources/Fonts/Nunito-Medium.ttf",characters);bold=Font("Bold","Assets/Resources/Fonts/Nunito-ExtraBold.ttf",characters);heading=Font("Heading","Assets/Resources/Fonts/Baloo2-Bold.ttf",characters);serif=Font("Card Serif",Local+"Fonts/NotoSerif.ttf",characters);
            var fallback=Font("Symbols","Assets/Resources/Fonts/NotoSans.ttf",characters);foreach(var f in new[]{body,bold,heading,serif}){f.fallbackFontAssetTable=new List<TMP_FontAsset>{fallback};EditorUtility.SetDirty(f);AssetDatabase.SaveAssetIfDirty(f);}
            FinishAssets();
            var root=New(RootName,null);SceneManager.MoveGameObjectToScene(root.gameObject,scene);var canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(360,640);scaler.matchWidthOrHeight=0;
            root.gameObject.AddComponent<GraphicRaycaster>();ui=root.gameObject.AddComponent<ClassicUI>();ui.driver=root.gameObject.AddComponent<RedesignRuntimeDriver>();ui.motion=root.gameObject.AddComponent<ClassicMotion>();
            ui.application=root.gameObject.AddComponent<ClassicApplication>();ui.application.ui=ui;ui.isolatedPreview=false;ui.blueButton=blue;ui.ivoryButton=ivory;ui.disabledButton=sand;
            var safe=Fill("SafeArea",root);safe.gameObject.AddComponent<ImportedSafeArea>();Face(safe,Cream);
            var world=Region("Shop",safe,0,.49f,1,1);BuildShopWorld(world);
            var panelHost=Region("Panels",safe,0,0,1,.49f);panelHost.offsetMin=new Vector2(0,64);
            var panels=new List<ClassicView>();foreach(string tab in new[]{"Home","Player","Products","Legacy","Shop"})panels.Add(BuildPanel(tab,panelHost));ui.panels=panels.ToArray();
            BuildNavigation(Bottom("Navigation",safe,64));BuildDialogs(root);BuildEffects(root,world);BuildToast(safe);
            ui.productArt=new[]{"sparklers","anar","chakri","rockets","small-crackers","cracker-strings","aerial","gift-boxes"}.Select(n=>art["product-"+n]).ToArray();
            ui.cardArt=data.Cards.Select(c=>art[c.id]).ToArray();ui.miniFrames=new[]{art["folk-common"],art["folk-rare"],art["folk-legendary"]};
            ui.outerFrames=new[]{art["full-outer-border"],art["full-rare-outer"],art["full-legendary-outer"]};ui.innerFrames=new[]{art["full-inner-border"],art["full-rare-inner"],art["full-legendary-inner"]};
            ui.textBindings=texts.ToArray();ui.imageBindings=images.ToArray();ui.dialogs=views.ToArray();
            if(!scene.GetRootGameObjects().Any(g=>g.GetComponent<EventSystem>())){var es=new GameObject("Classic EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));Undo.RegisterCreatedObjectUndo(es,"Author Classic input");SceneManager.MoveGameObjectToScene(es,scene);}
            foreach(var panel in ui.panels)panel.root.SetActive(panel.route=="Home");foreach(var dialog in ui.dialogs)dialog.root.SetActive(false);ui.toast.SetActive(false);
            Canvas.ForceUpdateCanvases();foreach(var t in root.GetComponentsInChildren<RedesignSavedText>(true))t.ForceMeshUpdate(true,true);
            EditorUtility.SetDirty(ui);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            ClassicImportQA.Refine();
            ClassicMotionAuthoring.Apply();
            ConnectPresentation();
            ClassicProductionAuthoring.Integrate();
            var saved=SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().Single(g=>g.name==RootName);
            Selection.activeGameObject=saved;Debug.Log("CLASSIC_UI_SAVED objects="+saved.GetComponentsInChildren<Transform>(true).Length+" dialogs="+saved.GetComponent<ClassicUI>().dialogs.Length);
        }
        static void LoadArt()
        {
            foreach(var a in JsonUtility.FromJson<ArtManifest>(File.ReadAllText(Local+"art-manifest.json")).assets)
            {
                if(a.imported){AssetDatabase.ImportAsset(a.path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(a.path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=a.key.StartsWith("full-")?1024:512;importer.SaveAndReimport();}
                art[a.key]=AssetDatabase.LoadAssetAtPath<Sprite>(a.path)??throw new InvalidOperationException("Missing art "+a.path);
            }
            blue=Sprite("Folk Blue");ivory=Sprite("Folk Ivory");sand=Sprite("Folk Sand");row=Sprite("Ivory Row Frame");rounded=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }
        static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Shared+name+".png")??throw new InvalidOperationException("Missing shared art "+name);
        public static string FinishAssets()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Asset authoring requires Edit Mode");
            var arrows=Font("Directional symbols","Assets/TextMesh Pro/Fonts/LiberationSans.ttf","↑↓→←◆…");
            foreach(var guid in AssetDatabase.FindAssets("t:TMP_FontAsset",new[]{Local+"Fonts"}))
            {
                var f=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));if(f==arrows)continue;
                if(f.fallbackFontAssetTable==null)f.fallbackFontAssetTable=new List<TMP_FontAsset>();if(!f.fallbackFontAssetTable.Contains(arrows))f.fallbackFontAssetTable.Add(arrows);f.ReadFontAssetDefinition();EditorUtility.SetDirty(f);AssetDatabase.SaveAssetIfDirty(f);
            }
            int changed=0;AssetDatabase.StartAssetEditing();try
            {
                foreach(var a in JsonUtility.FromJson<ArtManifest>(File.ReadAllText(Local+"art-manifest.json")).assets.Where(a=>a.imported))
                {
                    var importer=(TextureImporter)AssetImporter.GetAtPath(a.path);int max=a.key.StartsWith("full-")?1024:512;if(importer.maxTextureSize==max)continue;importer.maxTextureSize=max;importer.SaveAndReimport();changed++;
                }
            }finally{AssetDatabase.StopAssetEditing();}
            return "Scoped symbol font saved; "+changed+" imported texture sizes optimized";
        }
        static TMP_FontAsset Font(string name,string source,string characters)
        {
            string path=Local+"Fonts/"+name+" SDF.asset";var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(font)return font;
            AssetDatabase.ImportAsset(source,ImportAssetOptions.ForceSynchronousImport);var sourceFont=AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(source);
            font=TMP_FontAsset.CreateFontAsset(sourceFont,64,7,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic);font.name=name+" SDF";font.TryAddCharacters(characters,out _);AssetDatabase.CreateAsset(font,path);
            AssetDatabase.AddObjectToAsset(font.material,font);foreach(var tex in font.atlasTextures)AssetDatabase.AddObjectToAsset(tex,font);font.atlasPopulationMode=AtlasPopulationMode.Static;EditorUtility.SetDirty(font);AssetDatabase.SaveAssetIfDirty(font);return font;
        }
        static RectTransform New(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform));if(parent)go.transform.SetParent(parent,false);Undo.RegisterCreatedObjectUndo(go,"Author Classic native UI");return (RectTransform)go.transform;
        }
        static RectTransform Fill(string name,Transform parent,float margin=0){var r=New(name,parent);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.one*margin;r.offsetMax=-Vector2.one*margin;return r;}
        static RectTransform Region(string name,Transform parent,float x0,float y0,float x1,float y1){var r=New(name,parent);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static RectTransform Top(string name,Transform parent,float height,float down=0){var r=New(name,parent);r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,height);r.anchoredPosition=new Vector2(0,-down);return r;}
        static RectTransform Bottom(string name,Transform parent,float height,float up=0){var r=New(name,parent);r.anchorMin=Vector2.zero;r.anchorMax=new Vector2(1,0);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(0,height);r.anchoredPosition=new Vector2(0,up);return r;}
        static RectTransform Fixed(string name,Transform parent,float x,float y,float w,float h){var r=New(name,parent);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
        static Image Face(RectTransform r,Color color,Sprite sprite=null,bool raycast=false){var img=r.gameObject.AddComponent<Image>();img.color=color;img.sprite=sprite;img.raycastTarget=raycast;if(sprite&&sprite.border.sqrMagnitude>0){img.type=Image.Type.Sliced;img.pixelsPerUnitMultiplier=3;}return img;}
        static Image Picture(string key,RectTransform rect,Sprite sprite,bool contain=true){var img=Face(rect,Color.white,sprite);img.type=Image.Type.Simple;img.preserveAspect=contain;if(key!="")images.Add(new ClassicImageBinding{key=key,image=img});return img;}
        static TMP_Text Label(string key,RectTransform rect,string value,float size=12,bool heavy=false,Color? color=null,TextAlignmentOptions align=TextAlignmentOptions.Left,TMP_FontAsset font=null)
        {
            var text=rect.gameObject.AddComponent<RedesignSavedText>();text.font=font?font:heavy?bold:body;text.fontSize=size;text.color=color??Ink;text.alignment=align;text.text=value;text.raycastTarget=false;text.richText=false;text.textWrappingMode=TextWrappingModes.Normal;text.overflowMode=TextOverflowModes.Ellipsis;
            text.savedMaterialSlots=new TMP_SubMeshUI[2];for(int i=0;i<2;i++){var child=Fill("Saved material "+(i+1),rect);child.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;var slot=child.gameObject.AddComponent<RedesignSavedSubMesh>();var so=new SerializedObject(slot);so.FindProperty("m_TextComponent").objectReferenceValue=text;so.FindProperty("m_materialReferenceIndex").intValue=i+1;so.ApplyModifiedPropertiesWithoutUndo();slot.raycastTarget=false;slot.fontAsset=text.font;slot.sharedMaterial=text.fontSharedMaterial;slot.isDefaultMaterial=true;slot.hideFlags=HideFlags.None;slot.gameObject.hideFlags=HideFlags.None;text.savedMaterialSlots[i]=slot;}text.BindSavedSlots();
            if(key!="")texts.Add(new ClassicTextBinding{key=key,text=text});return text;
        }
        static Button Button(RectTransform rect,string action,string parameter,string key,string title,Sprite face=null,float size=12)
        {
            var image=Face(rect,Color.white,face?face:blue,true);var b=rect.gameObject.AddComponent<Button>();b.targetGraphic=image;b.navigation=new Navigation{mode=Navigation.Mode.None};b.transition=Selectable.Transition.ColorTint;
            var colors=b.colors;colors.highlightedColor=new Color(1,.98f,.9f);colors.pressedColor=new Color(.8f,.85f,.9f);colors.disabledColor=new Color(.75f,.75f,.75f);b.colors=colors;
            var control=rect.gameObject.AddComponent<ClassicControl>();control.owner=ui;control.action=action;control.parameter=parameter;
            if(title!="")Label(key,Fill("Label",rect,8),title,size,true,face==ivory?Ink:Cream,TextAlignmentOptions.Center);
            return b;
        }
        static void Height(RectTransform r,float height){var el=r.gameObject.AddComponent<LayoutElement>();el.minHeight=height;el.preferredHeight=height;r.sizeDelta=new Vector2(r.sizeDelta.x,height);}
        static RectTransform Row(string name,Transform parent,float height=66){var r=New(name,parent);Height(r,height);Face(r,Color.white,row);r.gameObject.AddComponent<CanvasGroup>();return r;}
        static RectTransform List(Transform parent,string name){var r=New(name,parent);r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.sizeDelta=Vector2.zero;var l=r.gameObject.AddComponent<VerticalLayoutGroup>();l.padding=new RectOffset(9,9,7,12);l.spacing=7;l.childControlHeight=true;l.childControlWidth=true;l.childForceExpandHeight=false;l.childForceExpandWidth=true;r.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;return r;}
        static ScrollRect Scroll(RectTransform rect,out RectTransform content){var viewport=Fill("Viewport",rect);viewport.gameObject.AddComponent<RectMask2D>();Face(viewport,new Color(1,1,1,.001f),null,true);content=List(viewport,"Content");var scroll=rect.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;return scroll;}
        static RectTransform Info(Transform parent,string title,string key,string copy,float height=100){var r=Row(title,parent,height);Label("",Top("Heading",r,30),title,19,true,Ink,TextAlignmentOptions.Center,heading);var body=Fill("Body",r,12);body.offsetMax=new Vector2(-12,-32);Label(key,body,copy,12);return r;}
        static TMP_InputField Input(RectTransform rect,string placeholder){Face(rect,new Color32(255,249,236,255),rounded,true);var area=Fill("Text Area",rect,8);area.gameObject.AddComponent<RectMask2D>();var text=Label("",Fill("Text",area),"",14);var hint=Label("",Fill("Placeholder",area),placeholder,13,false,Muted);var input=rect.gameObject.AddComponent<RedesignSearchInput>();input.textViewport=area;input.textComponent=text;input.placeholder=hint;input.targetGraphic=rect.GetComponent<Image>();input.navigation=new Navigation{mode=Navigation.Mode.None};var caret=Fill("Saved caret",area);caret.SetAsFirstSibling();caret.gameObject.AddComponent<TMP_SelectionCaret>();input.savedCaret=caret.GetComponent<CanvasRenderer>();return input;}
    }
}
