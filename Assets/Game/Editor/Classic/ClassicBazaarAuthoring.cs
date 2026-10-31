using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    /// <summary>Reconciles the existing production owners with the qualified HTML. Editor only.</summary>
    public static partial class ClassicBazaarAuthoring
    {
        const string Folder="Assets/Game/ClassicUI/Bazaar/";
        const string Evidence="Documentation/Implementation/UiUnification/";
        static ClassicBazaarTheme theme;
        static ClassicUI ui;
        static Dictionary<string,ViewSpec> specs;
        static Sprite white;
        [Serializable] sealed class Gate {public bool htmlQualified;public float overall;public int consecutiveCompleteAudits;}
        [Serializable] sealed class LayoutSpec {public ViewSpec[] views;}
        [Serializable] sealed class ViewSpec {public string route;public float height,desiredHeight,headerHeight,footerHeight;public Part[] sections;}
        [Serializable] sealed class Part {public string className;public float x,y,w,h;}
        [Serializable] sealed class TokenRoot {public TokenColors color;}
        [Serializable] sealed class TokenColors {public string paper,paperRaised,paperShade,ink,muted,gold,goldLight,line,track,progress,success,danger,onDisabled;}

        [MenuItem("Rocket Raja/Classic/Apply qualified Gouache Bazaar UI")]
        public static string Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            ValidateGate();
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Preserve unsaved edits: "+SceneManager.GetSceneAt(i).path);
            Backup("Assets/Scenes/Main.unity","Main");Backup("Assets/Scenes/Bootstrap.unity","Bootstrap");
            ImportSkins();theme=LoadTheme();
            specs=JsonUtility.FromJson<LayoutSpec>(File.ReadAllText(Evidence+"layout-reference.json")).views.GroupBy(v=>v.route.Split(':')[0]).ToDictionary(g=>g.Key,g=>g.First());
            var scene=SceneManager.GetSceneByPath("Assets/Scenes/Main.unity");if(!scene.isLoaded)scene=EditorSceneManager.OpenScene("Assets/Scenes/Main.unity",OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            ui=scene.GetRootGameObjects().Single(g=>g.name=="ClassicProductionCanvas").GetComponent<ClassicUI>();
            ui.bazaarTheme=theme;
            var roots=ui.panels.Select(v=>v.root.transform).Concat(ui.dialogs.Select(v=>v.root.transform)).ToArray();
            foreach(var root in roots)ApplyTypography(root,1);
            ApplyTypography(ui.transform.Find("SafeArea/Shop"),1);
            PrepareCard();
            foreach(var view in ui.dialogs)Dialog(view);
            foreach(var view in ui.panels)Panel(view);
            Home();ProductRows();UpgradeRows();Legacy();Shop();SkillViews();ProductOverview();SpecialViews();Settings();Guidance();Tutorial();Navigation();
            CacheButtons();
            PublishComponents();
            foreach(var view in ui.panels.Concat(ui.dialogs))
            {
                bool active=view.root.activeSelf;view.root.SetActive(true);
                if(view.scroll)LayoutRebuilder.ForceRebuildLayoutImmediate(view.scroll.content);
                view.root.SetActive(active);
            }
            ui.GetComponent<ClassicLayout>().Apply();Canvas.ForceUpdateCanvases();EditorUtility.SetDirty(ui);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            Bootstrap();AssetDatabase.SaveAssets();
            File.WriteAllText(Evidence+"native/authoring.txt","Applied qualified Gouache Bazaar theme to Main and Bootstrap. Existing action/binding owners retained; component backgrounds are Editor-placed prefab instances.\n");
            return "Gouache Bazaar saved: "+ui.panels.Length+" panels, "+ui.dialogs.Length+" dialogs, "+ui.bazaarButtons.Length+" cached button bindings";
        }
        static void ValidateGate()
        {
            string raw=File.ReadAllText(Evidence+"quality-gate.json");var gate=JsonUtility.FromJson<Gate>(raw);
            if(!gate.htmlQualified||gate.overall<95||gate.consecutiveCompleteAudits<2)throw new InvalidOperationException("HTML quality gate has not passed");
            foreach(Match match in Regex.Matches(raw,"\"(UI Builder/[^\"]+)\"\\s*:\\s*\"([a-f0-9]{64})\""))
                using(var sha=SHA256.Create())if(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(match.Groups[1].Value))).Replace("-","").ToLowerInvariant()!=match.Groups[2].Value)throw new InvalidOperationException("Qualified HTML changed: "+match.Groups[1].Value);
        }
        static void Backup(string source,string name)
        {
            string path=Folder+"Legacy/"+name+"__LegacyDisabled_20261031.unity";
            if(File.Exists(path))return;
            Directory.CreateDirectory(Folder+"Legacy");AssetDatabase.Refresh();
            if(!AssetDatabase.CopyAsset(source,path))throw new InvalidOperationException("Could not preserve "+source);
            var copy=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            foreach(var root in copy.GetRootGameObjects())root.SetActive(false);
            EditorSceneManager.SaveScene(copy);EditorSceneManager.CloseScene(copy,true);
        }
        static void ImportSkins()
        {
            AssetDatabase.Refresh();
            foreach(string path in Directory.GetFiles(Folder+"Skins","*.png"))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=300;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
                string name=Path.GetFileNameWithoutExtension(path);float border=name=="track"||name=="fill"?36:name=="window"||name=="section"||name=="shade"||name=="common"||name=="rare"||name=="legendary"?24:0;
                importer.spriteBorder=Vector4.one*border;importer.SaveAndReimport();
            }
        }
        static ClassicBazaarTheme LoadTheme()
        {
            var value=AssetDatabase.LoadAssetAtPath<ClassicBazaarTheme>(Folder+"Gouache Bazaar.asset");if(!value){value=ScriptableObject.CreateInstance<ClassicBazaarTheme>();AssetDatabase.CreateAsset(value,Folder+"Gouache Bazaar.asset");}
            string raw=File.ReadAllText("UI Builder/HTML/Classic/system/tokens.json");raw=Regex.Replace(raw,"\"([a-z]+)-([a-z]+)\"\\s*:",m=>"\""+m.Groups[1].Value+char.ToUpperInvariant(m.Groups[2].Value[0])+m.Groups[2].Value.Substring(1)+"\":");
            var c=JsonUtility.FromJson<TokenRoot>(raw).color;
            value.revision="Gouache Bazaar 2026-10-31 / HTML 95.9";value.paper=C(c.paper);value.raised=C(c.paperRaised);value.shade=C(c.paperShade);value.ink=C(c.ink);value.muted=C(c.muted);value.gold=C(c.gold);value.goldLight=C(c.goldLight);value.line=C(c.line);value.track=C(c.track);value.progress=C(c.progress);value.success=C(c.success);value.danger=C(c.danger);value.disabledInk=C(c.onDisabled);
            using(var sha=SHA256.Create())value.tokenHash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes("UI Builder/HTML/Classic/system/tokens.json"))).Replace("-","").ToLowerInvariant();
            value.heading=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/ClassicUI/Fonts/Heading SDF.asset");value.body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/ClassicUI/Fonts/Body SDF.asset");value.bold=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/ClassicUI/Fonts/Bold SDF.asset");
            value.window=S("window");value.section=S("section");value.shadeSurface=S("shade");value.barTrack=S("track");value.barFill=S("fill");value.switchTrack=S("switch-track");value.switchThumb=S("switch-thumb");value.information=S("information");
            value.circles=new[]{36,44,48,72,88}.Select(n=>S("circle-"+n)).ToArray();value.rarity=new[]{S("common"),S("rare"),S("legendary")};
            value.paperTexture=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ClassicUI/ProductOverview/paper.png");
            value.primary=A("Folk Blue");value.secondary=A("Folk Ivory");value.unavailable=A("Folk Sand");value.destructive=A("Folk Red");EditorUtility.SetDirty(value);return value;
        }
        static Color C(string hex){if(!ColorUtility.TryParseHtmlString(hex,out var color))throw new InvalidOperationException("Invalid token color "+hex);return color;}
        static Sprite S(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"Skins/"+name+".png")??throw new InvalidOperationException("Missing skin "+name);
        static Sprite A(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/"+name+".png");
        static RectTransform R(Transform t)=>t as RectTransform;
        static RectTransform New(string name,Transform parent)
        {
            var existing=parent.Find(name);if(existing)return R(existing);
            var go=new GameObject(name,typeof(RectTransform));Undo.RegisterCreatedObjectUndo(go,"Gouache Bazaar authoring");go.layer=5;go.transform.SetParent(parent,false);return R(go.transform);
        }
        static T Ensure<T>(GameObject go)where T:Component{var component=go.GetComponent<T>();return component?component:go.AddComponent<T>();}
        static void Box(RectTransform r,Vector2 min,Vector2 max,Vector2 low,Vector2 high){r.anchorMin=min;r.anchorMax=max;r.pivot=new Vector2(.5f,.5f);r.offsetMin=low;r.offsetMax=high;r.localScale=Vector3.one;}
        static void Stretch(RectTransform r,float edge=0)=>Box(r,Vector2.zero,Vector2.one,Vector2.one*edge,-Vector2.one*edge);
        static void Top(RectTransform r,float left,float top,float right,float height)=>Box(r,new Vector2(0,1),Vector2.one,new Vector2(left,-top-height),new Vector2(-right,-top));
        static void Fixed(RectTransform r,float left,float top,float width,float height)=>Box(r,new Vector2(0,1),new Vector2(0,1),new Vector2(left,-top-height),new Vector2(left+width,-top));
        static LayoutElement Height(RectTransform r,float h){var e=Ensure<LayoutElement>(r.gameObject);e.minHeight=e.preferredHeight=h;e.flexibleHeight=0;return e;}
        static void Ignore(RectTransform r)=>Ensure<LayoutElement>(r.gameObject).ignoreLayout=true;
        static void Text(TMP_Text t,float size,bool bold=false,bool heading=false,Color? color=null,TextAlignmentOptions align=TextAlignmentOptions.TopLeft)
        {
            t.font=heading?theme.heading:bold?theme.bold:theme.body;t.fontSharedMaterial=t.font.material;t.fontStyle=FontStyles.Normal;t.fontSize=t.fontSizeMax=size;t.fontSizeMin=Mathf.Min(size,12);t.enableAutoSizing=false;
            t.color=color??theme.ink;t.alignment=align;t.characterSpacing=0;t.wordSpacing=0;t.paragraphSpacing=0;t.margin=Vector4.zero;t.overflowMode=TextOverflowModes.Overflow;t.enableWordWrapping=true;
            var face=t.font.faceInfo;float leading=heading?1.15f:size<=12?1.35f:1.45f;t.lineSpacing=leading*face.pointSize-face.lineHeight;
        }
        static void ApplyTypography(Transform root,float scale)
        {
            if(!root)return;
            foreach(var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if(Legacy(t.transform))continue;
                bool title=t.name=="Title"&&t.transform.parent&&t.transform.parent.name=="Header";
                bool strong=t.font&&t.font.name.Contains("Bold")||t.fontStyle.HasFlag(FontStyles.Bold);
                Text(t,(title?22:14)*scale,strong,title,color:strong?theme.ink:theme.muted);
            }
        }
        static bool Legacy(Transform t){for(;t;t=t.parent)if(t.name.Contains("LegacyDisabled")||t.name.Contains("__Legacy"))return true;return false;}
        static VerticalLayoutGroup Flow(RectTransform root,int left=12,int top=12,int gap=8)
        {
            var h=root.GetComponent<HorizontalLayoutGroup>();if(h)Object.DestroyImmediate(h);
            var g=Ensure<VerticalLayoutGroup>(root.gameObject);g.padding=new RectOffset(left,left,top,top);g.spacing=gap;g.childControlWidth=g.childControlHeight=true;g.childForceExpandWidth=true;g.childForceExpandHeight=false;g.childAlignment=TextAnchor.UpperLeft;return g;
        }
        static void Surface(RectTransform root,Sprite sprite,bool prefab=true)
        {
            var old=root.GetComponent<Image>();
            if(!prefab){var img=Ensure<Image>(root.gameObject);img.sprite=sprite;img.color=Color.white;img.type=Image.Type.Sliced;img.pixelsPerUnitMultiplier=1;return;}
            if(old)old.enabled=false;
            string name="Bazaar surface";var child=root.Find(name) as RectTransform;
            if(!child)
            {
                string asset=Folder+"Components/Surface.prefab";
                if(!File.Exists(asset))
                {
                    Directory.CreateDirectory(Folder+"Components");AssetDatabase.Refresh();var temp=new GameObject("Bazaar surface",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(LayoutElement));temp.layer=5;
                    temp.GetComponent<Image>().raycastTarget=false;temp.GetComponent<LayoutElement>().ignoreLayout=true;PrefabUtility.SaveAsPrefabAsset(temp,asset);Object.DestroyImmediate(temp);
                }
                child=(RectTransform)((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(asset),root)).transform;child.name=name;child.SetAsFirstSibling();
            }
            Stretch(child);Ignore(child);var image=child.GetComponent<Image>();image.sprite=sprite;image.color=Color.white;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=1;image.raycastTarget=false;
            var button=root.GetComponent<Button>();if(button&&button.targetGraphic==old){button.targetGraphic=image;image.raycastTarget=true;}
        }
        static void Button(Button button,bool secondary=false)
        {
            var face=button.targetGraphic as Image;if(face){face.sprite=secondary?theme.secondary:theme.primary;face.color=Color.white;face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=4;face.enabled=true;}
            var c=button.GetComponent<ClassicControl>();if(c){c.htmlStyle=secondary?2:1;c.faceOverride=null;}
            button.transition=Selectable.Transition.None;
            foreach(var t in button.GetComponentsInChildren<TMP_Text>(true)){Text(t,14,true,color:secondary?theme.ink:theme.paper,align:TextAlignmentOptions.Center);Stretch(t.rectTransform,8);}
        }
        static TMP_Text Bound(string key)=>ui.textBindings.FirstOrDefault(x=>x.key==key)?.text;
        static void CacheButtons()
        {
            ui.bazaarButtons=ui.GetComponentsInChildren<ClassicControl>(true).Where(c=>!Legacy(c.transform)&&c.GetComponent<Button>()).Select(c=>new ClassicBazaarButton{control=c,button=c.GetComponent<Button>(),labels=c.GetComponentsInChildren<TMP_Text>(true)}).ToArray();
        }
        static void PublishComponents()
        {
            // Templates use the same saved structures; instances retain their serialized gameplay owners.
            var samples=new Dictionary<string,GameObject>{
                {"Folk action",View("Skills").scroll.content.Find("Skill 0/Activate").gameObject},
                {"Settings switch",View("Settings").root.GetComponentsInChildren<ClassicSettingsSwitch>(true).First().gameObject},
                {"Information card",View("Privacy").scroll.content.Cast<Transform>().First(t=>t.Find("Heading")).gameObject},
                {"Product row",Common(ui.products[0].name.transform,ui.products[0].buy.transform).gameObject},
                {"Progress bar",ui.imageBindings.First(b=>b.key=="product-progress").image.transform.parent.parent.gameObject}
            };
            foreach(var pair in samples)
            {
                var copy=Object.Instantiate(pair.Value);copy.name=pair.Key;
                foreach(var control in copy.GetComponentsInChildren<ClassicControl>(true))control.owner=null;
                foreach(var toggle in copy.GetComponentsInChildren<ClassicSettingsSwitch>(true))toggle.ui=null;
                PrefabUtility.SaveAsPrefabAsset(copy,Folder+"Components/"+pair.Key+".prefab");Object.DestroyImmediate(copy);
            }
        }
    }
}
