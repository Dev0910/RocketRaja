using System;
using System.IO;
using System.Linq;
using RocketRaja.Cloud;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace RocketRaja.EditorTools
{
    // Explicit, one-time Editor authoring only. No UI factories are used in the player.
    public static class BuildBootstrap
    {
        public const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        const string Root = "Assets/Game/Bootstrap/";
        static TMP_FontAsset headingFont, bodyFont, boldFont;
        static Sprite rounded;
        static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
        static readonly Color Ink = C("173A45"), Maroon = C("7D1025"), Gold = C("C88A2C"), Muted = C("4E646A");
        [MenuItem("Rocket Raja/Author Bootstrap Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (File.Exists(ScenePath)) { Debug.Log("Bootstrap already exists; preserved. Use the scene to edit it."); return; }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ImportSprite(Root + "Art/Festival Stall.png"); ImportSprite(Root + "Art/Google Mark.png");
            CreateSpinner();
            headingFont = MakeFont("Fraunces ExtraBold", Root + "Fonts/Fraunces ExtraBold.ttf");
            bodyFont = MakeFont("Nunito Medium", "Assets/Resources/Fonts/Nunito-Medium.ttf");
            boldFont = MakeFont("Nunito ExtraBold", "Assets/Resources/Fonts/Nunito-ExtraBold.ttf");
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var camera = new GameObject("Bootstrap Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera"; camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = C("23333B"); camera.transform.position = new Vector3(0,0,-10);
            var root = Rect("Bootstrap Canvas", null);
            var canvas = root.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080,2200); scaler.matchWidthOrHeight = 0;
            root.gameObject.AddComponent<GraphicRaycaster>();
            var art = Fill(root,"Festival Illustration"); var picture = Image(art,Color.white);
            picture.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Festival Stall.png");
            var aspect = art.gameObject.AddComponent<AspectRatioFitter>(); aspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            aspect.aspectRatio = picture.sprite.rect.width / picture.sprite.rect.height;
            var safe = Fill(root,"Safe Area"); safe.gameObject.AddComponent<ImportedSafeArea>();
            var stack = safe.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(49,49,42,67); stack.childControlWidth = stack.childControlHeight = true;
            stack.childForceExpandHeight = false; stack.childForceExpandWidth = true;
            var brand = Row(safe,"Branding",280);
            Text(At(brand,"Ornament",0,0,0,52),"◆",40,Gold,bodyFont);
            Text(At(brand,"Rocket Raja",0,0,52,152),"ROCKET RAJA",124,Maroon,headingFont);
            Text(At(brand,"Tagline",0,0,223,47),"YOUR FIREWORKS STORY STARTS HERE",30,Ink,boldFont);
            var spacer = Row(safe,"Flexible Scene Space",0); spacer.GetComponent<LayoutElement>().flexibleHeight = 1;
            var panel = Rect("Ivory Status Panel",safe);
            Image(panel,Gold,rounded);
            var surface = Inset(panel,"Paper Surface",4); Image(surface,C("FFF3DB"),rounded);
            surface.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(65,65,36,30); layout.spacing = 18;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false;
            var title = Text(Row(panel,"Heading",86),"Welcome to your stall",66,Maroon,headingFont);
            var rule = Row(panel,"Heading Ornament",22); Divider(rule);
            var description = Text(Row(panel,"Description",112),"Save your progress with Google and continue on another device.",39,Ink,bodyFont);
            var compare = Row(panel,"Save Comparison",228);
            var columns = compare.gameObject.AddComponent<HorizontalLayoutGroup>(); columns.spacing = 22;
            columns.childControlHeight = columns.childControlWidth = true; columns.childForceExpandWidth = true;
            var device = Card(compare,"This device",false); var cloud = Card(compare,"Google save",true);
            compare.gameObject.SetActive(false);
            var loading = Row(panel,"Loading",200);
            var spinRect = Rect("Spinner",loading); spinRect.anchorMin = spinRect.anchorMax = new Vector2(.5f,1); spinRect.pivot = new Vector2(.5f,1); spinRect.sizeDelta = new Vector2(76,76);
            Image(spinRect,Color.white).sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/Spinner.png");
            var progress = Text(At(loading,"Progress",0,0,108,84),"Syncing your progress…",35,Muted,bodyFont);
            loading.gameObject.SetActive(false);
            var primary = Button(panel,"Primary Button","Sign in with Google",Color.white,out var primaryLabel);
            var google = Rect("Google Mark",primary.transform); google.anchorMin=google.anchorMax=new Vector2(.18f,.5f); google.sizeDelta=new Vector2(62,62);
            Image(google,Color.white).sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/Google Mark.png");
            // Centered label reserves space for the icon without baking either into the button.
            primaryLabel.margin = new Vector4(85,0,0,0);
            var secondary = Button(panel,"Secondary Button","Play as guest",C("FFF0CF"),out var secondaryLabel);
            var tertiary = Button(panel,"Tertiary Button","Return to guest",C("FFF3DB"),out var tertiaryLabel);
            tertiaryLabel.fontSize=36; tertiaryLabel.fontStyle=FontStyles.Underline; tertiary.gameObject.SetActive(false);
            var hint = Text(Row(panel,"Hint",72),"You can connect Google later in Settings.",31,Muted,bodyFont);
            var footer = Row(panel,"Footer",110); Divider(At(footer,"Footer Ornament",0,0,0,20));
            Text(At(footer,"Local and cloud note",0,0,34,76),"Guest progress stays on this device.\nGoogle progress follows you between devices.",27,Muted,bodyFont);
            var compatibility = Text(Rect("Comparison compatibility",panel),"",30,Ink,bodyFont); compatibility.gameObject.SetActive(false);
            var presenter = root.gameObject.AddComponent<BootstrapController>(); var so = new SerializedObject(presenter);
            void Bind(string name,UnityEngine.Object value) { so.FindProperty(name).objectReferenceValue=value; }
            Bind("heading",title);Bind("description",description);Bind("progress",progress);Bind("comparison",compatibility);
            Bind("primaryButton",primary);Bind("secondaryButton",secondary);Bind("tertiaryButton",tertiary);
            Bind("primaryLabel",primaryLabel);Bind("secondaryLabel",secondaryLabel);Bind("tertiaryLabel",tertiaryLabel);
            Bind("hint",hint);Bind("comparisonCards",compare.gameObject);Bind("loadingGroup",loading.gameObject);Bind("spinner",spinRect);Bind("googleMark",google.gameObject);Bind("footer",footer.gameObject);
            Bind("deviceCash",device[0]);Bind("deviceDetails",device[1]);Bind("deviceTime",device[2]);Bind("cloudCash",cloud[0]);Bind("cloudDetails",cloud[1]);Bind("cloudTime",cloud[2]);
            so.ApplyModifiedPropertiesWithoutUndo();
            new GameObject("Bootstrap Event System",typeof(EventSystem),typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(root.gameObject,"Author Bootstrap");
            Canvas.ForceUpdateCanvases();
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes = new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            Refine();
            AssetDatabase.SaveAssets();
            Debug.Log("BOOTSTRAP_AUTHORED " + ScenePath);
        }
        static TMP_FontAsset MakeFont(string name,string source)
        {
            string path=Root+"Fonts/"+name+" SDF.asset";var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if(font==null){font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(source),90,9,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic);font.name=name;AssetDatabase.CreateAsset(font,path);}
            font.atlasPopulationMode=AtlasPopulationMode.Dynamic;
            string chars=new string(Enumerable.Range(32,95).Select(i=>(char)i).ToArray())+"₹…·’–—◆";
            font.TryAddCharacters(chars,out string missing);
            if(!string.IsNullOrEmpty(missing)) font.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>{AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/ImportedUI/Noto Sans Rupee SDF.asset")};
            if(!EditorUtility.IsPersistent(font.material))AssetDatabase.AddObjectToAsset(font.material,font);foreach(var t in font.atlasTextures)if(t!=null&&!EditorUtility.IsPersistent(t))AssetDatabase.AddObjectToAsset(t,font);
            font.atlasPopulationMode=AtlasPopulationMode.Static;EditorUtility.SetDirty(font);AssetDatabase.SaveAssetIfDirty(font);return font;
        }
        static void ImportSprite(string path)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        static void CreateSpinner()
        {
            string path=Root+"Art/Spinner.png";if(File.Exists(path)){ImportSprite(path);return;}
            var t=new Texture2D(128,128,TextureFormat.RGBA32,false);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++){float dx=x-63.5f,dy=y-63.5f,r=Mathf.Sqrt(dx*dx+dy*dy);var color=(Mathf.Atan2(dy,dx)>.5f&&Mathf.Atan2(dy,dx)<2)?C("245B61"):C("DAB878");color.a=Mathf.Clamp01(Mathf.Min(r-49,59-r));t.SetPixel(x,y,color);}
            t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);ImportSprite(path);
        }
        static TMP_Text[] Card(RectTransform parent,string label,bool cloud)
        {
            var r=Rect(label,parent);Image(r,C(cloud?"E7EEE4":"F7E7C7"),rounded);var l=r.gameObject.AddComponent<VerticalLayoutGroup>();l.padding=new RectOffset(24,24,18,14);l.spacing=6;l.childControlHeight=l.childControlWidth=true;l.childForceExpandHeight=false;
            Text(Row(r,"Profile",36),label,32,C("245B61"),boldFont,TextAlignmentOptions.Left);
            var cash=Text(Row(r,"Cash",58),cloud?"₹8.60M":"₹1.24M",54,Maroon,headingFont,TextAlignmentOptions.Left);
            var detail=Text(Row(r,"Products and prestige",46),"0 products · 0 prestiges",29,Muted,bodyFont,TextAlignmentOptions.Left);
            var time=Text(Row(r,"Saved time",40),"Saved just now",26,Muted,bodyFont,TextAlignmentOptions.Left);return new[]{cash,detail,time};
        }
        static Button Button(RectTransform parent,string name,string label,Color color,out TMP_Text text)
        {
            var r=Row(parent,name,144);var edge=Image(r,C("866333"),rounded);Image(Inset(r,"Face",3),color,rounded);
            edge.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=edge;b.navigation=new Navigation{mode=Navigation.Mode.None};
            var colors=b.colors;colors.highlightedColor=new Color(.97f,.97f,.97f);colors.pressedColor=new Color(.85f,.85f,.85f);colors.disabledColor=new Color(1,1,1,.55f);b.colors=colors;
            text=Text(Inset(r,"Label",12),label,44,Ink,boldFont);return b;
        }
        static void Divider(RectTransform r)
        {
            var line=Rect("Gold rule",r);line.anchorMin=new Vector2(.22f,.5f);line.anchorMax=new Vector2(.78f,.5f);line.sizeDelta=new Vector2(0,2);Image(line,Gold);
            var jewel=Rect("Diamond",r);jewel.anchorMin=jewel.anchorMax=new Vector2(.5f,.5f);jewel.sizeDelta=new Vector2(18,18);jewel.localRotation=Quaternion.Euler(0,0,45);Image(jewel,Gold);
        }
        static RectTransform Rect(string name,Transform parent)
        {var go=new GameObject(name,typeof(RectTransform));if(parent!=null)go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();}
        static RectTransform Fill(Transform parent,string name){var r=Rect(name,parent);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static RectTransform Inset(Transform parent,string name,float inset){var r=Fill(parent,name);r.offsetMin=new Vector2(inset,inset);r.offsetMax=new Vector2(-inset,-inset);return r;}
        static RectTransform Row(Transform parent,string name,float height){var r=Rect(name,parent);var l=r.gameObject.AddComponent<LayoutElement>();l.minHeight=l.preferredHeight=height;return r;}
        static RectTransform At(Transform parent,string name,float left,float right,float top,float height){var r=Rect(name,parent);r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.offsetMin=new Vector2(left,-top-height);r.offsetMax=new Vector2(-right,-top);return r;}
        static Image Image(RectTransform r,Color c,Sprite sprite=null){var i=r.gameObject.AddComponent<Image>();i.color=c;i.raycastTarget=false;if(sprite!=null){i.sprite=sprite;i.type=UnityEngine.UI.Image.Type.Sliced;i.pixelsPerUnitMultiplier=.25f;}return i;}
        static TMP_Text Text(RectTransform r,string value,float size,Color c,TMP_FontAsset f,TextAlignmentOptions align=TextAlignmentOptions.Center){var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=f;t.text=value;t.fontSize=size;t.color=c;t.alignment=align;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Ellipsis;return t;}
        public static void Refine()
        {
            var scene=SceneManager.GetSceneByPath(ScenePath);if(!scene.IsValid())throw new InvalidOperationException("Open Bootstrap first.");
            string path=Root+"Art/Rounded Surface.png";
            if(!File.Exists(path))
            {
                var texture=new Texture2D(128,128,TextureFormat.RGBA32,false);
                for(int y=0;y<128;y++)for(int x=0;x<128;x++)
                {float dx=Mathf.Max(Mathf.Abs(x-63.5f)-31.5f,0),dy=Mathf.Max(Mathf.Abs(y-63.5f)-31.5f,0);texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(32-Mathf.Sqrt(dx*dx+dy*dy))));}
                texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            }
            ImportSprite(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.spriteBorder=new Vector4(32,32,32,32);importer.SaveAndReimport();var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var root=scene.GetRootGameObjects().Single(g=>g.GetComponent<BootstrapController>()!=null);
            foreach(var image in root.GetComponentsInChildren<Image>(true))if(image.type==UnityEngine.UI.Image.Type.Sliced){image.sprite=sprite;image.pixelsPerUnitMultiplier=1.4f;}
            foreach(var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if(t.name=="Rocket Raja") {t.enableAutoSizing=true;t.fontSizeMin=90;t.fontSizeMax=124;t.textWrappingMode=TextWrappingModes.NoWrap;var r=t.rectTransform;r.offsetMin=new Vector2(0,-227);r.offsetMax=new Vector2(0,-42);}
                if(t.name=="Heading"){t.enableAutoSizing=true;t.fontSizeMin=48;t.fontSizeMax=66;t.textWrappingMode=TextWrappingModes.NoWrap;}
                if(t.name=="Description"){t.enableAutoSizing=true;t.fontSizeMin=32;t.fontSizeMax=39;}
                if(t.name=="Ornament")
                {
                    t.text="";if(t.transform.Find("Diamond")==null){var d=Rect("Diamond",t.transform);d.anchorMin=d.anchorMax=new Vector2(.5f,.5f);d.sizeDelta=new Vector2(24,24);d.localRotation=Quaternion.Euler(0,0,45);Image(d,Gold);}
                }
                if(t.name=="Profile"){t.GetComponent<LayoutElement>().minHeight=t.GetComponent<LayoutElement>().preferredHeight=52;}
                if(t.name=="Cash"){t.GetComponent<LayoutElement>().minHeight=t.GetComponent<LayoutElement>().preferredHeight=80;t.fontSize=50;}
                if(t.name=="Products and prestige"){t.fontSize=27;t.GetComponent<LayoutElement>().minHeight=t.GetComponent<LayoutElement>().preferredHeight=38;}
                if(t.name=="Saved time"){t.fontSize=24;t.enableAutoSizing=true;t.fontSizeMin=20;t.fontSizeMax=24;}
            }
            var comparison=root.transform.Find("Safe Area/Ivory Status Panel/Save Comparison").GetComponent<LayoutElement>();comparison.minHeight=comparison.preferredHeight=260;
            foreach(var b in root.GetComponentsInChildren<Button>(true))
            {
                var feedback=b.GetComponent<BootstrapButtonFeedback>();if(feedback==null)feedback=b.gameObject.AddComponent<BootstrapButtonFeedback>();var so=new SerializedObject(feedback);so.FindProperty("label").objectReferenceValue=b.transform.Find("Label");so.ApplyModifiedPropertiesWithoutUndo();
                if(b.name=="Tertiary Button"){b.GetComponent<Image>().color=new Color(1,1,1,0);b.transform.Find("Face").GetComponent<Image>().color=new Color(1,1,1,0);}
            }
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
