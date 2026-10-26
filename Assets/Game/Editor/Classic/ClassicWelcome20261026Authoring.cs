using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        const string WelcomeArt = "Assets/Game/ClassicUI/Welcome20261026/";

        // Deliberate Editor operation, never called from game initialization.
        public static string ApplyWelcomeOctober26()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var boot = SceneManager.GetSceneByPath("Assets/Scenes/Bootstrap.unity");
            var main = SceneManager.GetSceneByPath(ScenePath);
            if (!boot.isLoaded || !main.isLoaded || boot.isDirty || main.isDirty) throw new InvalidOperationException("Open clean Bootstrap and Main scenes");
            foreach (string name in new[] { "festival-salesman", "brand-sign", "tap-banner", "panel-frame", "floral-accent" }) ImportWelcomeSprite(name);
            body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
            bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
            serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Card Serif SDF.asset");
            heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Heading SDF.asset");
            if (!body || !bold || !serif || !heading) throw new InvalidOperationException("Missing existing font assets");
            var root = boot.GetRootGameObjects().Single(g => g.name == "Bootstrap Canvas");
            if (root.transform.Find("Immersive welcome revision 20261026")) throw new InvalidOperationException("Already applied; refine saved objects rather than duplicating");
            var legacy = Object.Instantiate(root); legacy.name = "Bootstrap Canvas__LegacyDisabled_20261026";
            legacy.SetActive(false); SceneManager.MoveGameObjectToScene(legacy, boot);
            Undo.RegisterCreatedObjectUndo(legacy, "Preserve previous welcome screen");
            Undo.RegisterFullObjectHierarchyUndo(root, "Apply immersive welcome");
            var owner = root.GetComponent<BootstrapController>();
            var artImage = root.transform.Find("Festival Illustration").GetComponent<Image>();
            var aspect = artImage.GetComponent<AspectRatioFitter>(); if (aspect) aspect.enabled = false;
            WelcomeRect(artImage.rectTransform, 0, 0, 1, 1); artImage.sprite = WelcomeSprite("festival-salesman");
            artImage.preserveAspect = false; artImage.color = Color.white; artImage.raycastTarget = false;
            var safe = root.transform.Find("Safe Area");
            var outer = safe.GetComponent<VerticalLayoutGroup>();
            outer.padding = new RectOffset(0, 0, 0, 42); outer.spacing = 0; outer.childAlignment = TextAnchor.LowerCenter;
            outer.childControlHeight = outer.childControlWidth = true; outer.childForceExpandHeight = false;
            safe.Find("Outside dismissal").GetComponent<Image>().color = Color.clear;
            var panel = (RectTransform)safe.Find("Ivory Status Panel");
            panel.GetComponent<VerticalLayoutGroup>().enabled = false;
            var sizer = panel.GetComponent<BootstrapPopupSize>(); sizer.viewportHeightFraction = .5f; sizer.minimumViewportHeight = 1020;
            var frame = panel.GetComponent<Image>(); frame.sprite = WelcomeSprite("panel-frame"); frame.type = Image.Type.Simple;
            frame.color = Color.white; frame.raycastTarget = true;
            foreach (string name in new[] { "Paper Surface", "Painted gold and cobalt frame" }) if (panel.Find(name)) panel.Find(name).gameObject.SetActive(false);
            var scroll = (RectTransform)panel.Find("Popup scroll");
            WelcomeRect(scroll, .09f, 0, .91f, 1); scroll.offsetMin = new Vector2(0, 48); scroll.offsetMax = new Vector2(0, -210);
            var flow = (RectTransform)scroll.Find("Viewport/Content");
            var layout = flow.GetComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(); layout.spacing = 18;
            layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false;
            var brand = (RectTransform)flow.Find("Branding"); brand.SetParent(safe, false);
            brand.GetComponent<LayoutElement>().ignoreLayout = true;
            WelcomeRect(brand, .03f, .94f, .97f, .94f); brand.pivot = new Vector2(.5f, 1);
            var brandFit = brand.gameObject.AddComponent<AspectRatioFitter>(); brandFit.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight; brandFit.aspectRatio = 2.4f;
            var sign = Picture("", Fill("Painted cobalt and gold sign", brand), WelcomeSprite("brand-sign"), false); sign.transform.SetAsFirstSibling();
            brand.Find("Ornament").gameObject.SetActive(false); brand.Find("Tagline").gameObject.SetActive(false);
            var title = brand.Find("Rocket Raja").GetComponent<TMP_Text>(); WelcomeRect(title.rectTransform, .09f, .27f, .91f, .56f);
            WelcomeText(title, serif, 113, new Color32(255, 209, 108, 255)); title.characterSpacing = -4; title.enableAutoSizing = true; title.fontSizeMin = 85; title.fontSizeMax = 113;
            flow.Find("Painted shop artwork").gameObject.SetActive(false);
            var floral = New("Vermilion floral crest", panel); floral.anchorMin = floral.anchorMax = new Vector2(.5f, 1); floral.pivot = new Vector2(.5f, 1);
            floral.sizeDelta = new Vector2(108, 74); floral.anchoredPosition = new Vector2(0, -80); Picture("", floral, WelcomeSprite("floral-accent"));
            WelcomeFlowText(flow, "Heading", 90, serif, 81, Ink);
            var h = flow.Find("Heading").GetComponent<TMP_Text>(); h.margin = Vector4.zero; h.enableAutoSizing = true; h.fontSizeMin = 60; h.fontSizeMax = 81;
            WelcomeHeight(flow.Find("Heading Ornament"), 24);
            WelcomeFlowText(flow, "Description", 90, body, 37, Ink);
            WelcomeFlowText(flow, "Hint", 45, body, 30, Ink);
            WelcomeHeight(flow.Find("Footer"), 100);
            foreach (var t in flow.Find("Footer").GetComponentsInChildren<TMP_Text>(true)) WelcomeText(t, body, 28, Muted);
            foreach (string name in new[] { "Primary Button", "Secondary Button", "Tertiary Button" })
            {
                var button = flow.Find(name).GetComponent<Button>(); WelcomeHeight(button.transform, 144);
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                var image = (Image)button.targetGraphic;
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + (name == "Primary Button" ? "popup-primary.png" : "popup-secondary.png"));
                image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1f / 3; image.color = Color.white;
                var text = button.GetComponentInChildren<TMP_Text>(true); WelcomeText(text, bold, 42, name == "Primary Button" ? Cream : Ink);
                text.enableAutoSizing = true; text.fontSizeMin = 33; text.fontSizeMax = 42;
            }
            var close = (RectTransform)panel.Find("Close popup"); close.SetParent(safe, false); close.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            close.anchorMin = close.anchorMax = close.pivot = Vector2.one; close.sizeDelta = new Vector2(144, 144); close.anchoredPosition = new Vector2(-42, -45);
            close.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "popup-primary.png");
            close.GetComponent<Button>().navigation = new Navigation { mode = Navigation.Mode.None };
            // Keep the same controller, serialized buttons, scroll and all comparison/error states.
            owner.ReopenPopup();
            var marker = New("Immersive welcome revision 20261026", root.transform); marker.gameObject.SetActive(false);
            EditorUtility.SetDirty(owner); EditorUtility.SetDirty(sizer);
            var mainRoot = main.GetRootGameObjects().Single(g => g.name == RootName); ui = mainRoot.GetComponent<ClassicUI>();
            Undo.RegisterFullObjectHierarchyUndo(mainRoot, "Apply painted tap banner");
            var shop = ui.transform.Find("SafeArea/Shop"); var prompt = ui.motion.tapHint;
            var oldPrompt = Object.Instantiate(prompt.gameObject, shop); oldPrompt.name = "Tap invitation__LegacyDisabled_20261026"; oldPrompt.SetActive(false);
            WelcomeRect(prompt, .23f, -.07f, .77f, .07f); prompt.gameObject.SetActive(true);
            prompt.GetComponent<Image>().sprite = WelcomeSprite("tap-banner"); prompt.GetComponent<Image>().color = Color.white; prompt.GetComponent<Image>().type = Image.Type.Simple;
            var promptLabel = prompt.GetComponentInChildren<TMP_Text>(true); WelcomeText(promptLabel, heading, 13, Ink); promptLabel.text = "TAP TO EARN";
            promptLabel.enableAutoSizing = false; WelcomeRect(promptLabel.rectTransform, .14f, .1f, .86f, .9f);
            WelcomeRect((RectTransform)ui.tapTarget.transform, 0, -.07f, 1, .24f);
            shop.SetAsLastSibling();
            var next = (RectTransform)ui.transform.Find("SafeArea/Panels/Home/Next display"); next.anchoredPosition = new Vector2(next.anchoredPosition.x, -28);
            foreach (var guide in ui.guidance.rows.Where(r => r.route == "Home")) { var r = (RectTransform)guide.root.transform; r.anchoredPosition = new Vector2(r.anchoredPosition.x, -28); }
            EditorUtility.SetDirty(ui); Canvas.ForceUpdateCanvases();
            EditorSceneManager.MarkSceneDirty(boot); EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(boot); EditorSceneManager.SaveScene(main); AssetDatabase.SaveAssets();
            return "Saved immersive Bootstrap welcome/account presentation and Main tap banner; complete previous welcome canvas retained inactive.";
        }
        static Sprite WelcomeSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(WelcomeArt + name + ".png");
        public static string RefineShortWelcomeSign()
        {
            var scene=SceneManager.GetSceneByPath("Assets/Scenes/Bootstrap.unity");
            var root=scene.GetRootGameObjects().Single(g=>g.name=="Bootstrap Canvas");
            var safe=root.transform.Find("Safe Area");
            if(safe.Find("Responsive sign space"))return "Short-screen sign constraint already authored";
            var brand=(RectTransform)safe.Find("Branding");
            var holder=Region("Responsive sign space",safe,.03f,.77f,.97f,.96f);
            holder.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;
            brand.SetParent(holder,false);WelcomeRect(brand,0,0,1,1);
            brand.pivot=new Vector2(.5f,.5f);
            var fit=brand.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=2.4f;
            brand.Find("Rocket Raja").GetComponent<TMP_Text>().fontSizeMin=65;
            holder.SetAsLastSibling();safe.Find("Close popup").SetAsLastSibling();
            Canvas.ForceUpdateCanvases();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Saved aspect-preserving title sign constrained by available upper viewport";
        }
        public static string RefineWelcomeOctober26()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var scene=SceneManager.GetSceneByPath("Assets/Scenes/Bootstrap.unity");
            var root=scene.GetRootGameObjects().Single(g=>g.name=="Bootstrap Canvas");
            Undo.RegisterFullObjectHierarchyUndo(root,"Refine responsive welcome");
            var safe=root.transform.Find("Safe Area");
            var panel=safe.Find("Ivory Status Panel");
            var flow=panel.Find("Popup scroll/Viewport/Content");
            var source=WelcomeSprite("festival-salesman");
            string spritePath=WelcomeArt+"festival-upper.asset";
            var upperSprite=AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if(!upperSprite){upperSprite=UnityEngine.Sprite.Create(source.texture,new Rect(0,source.texture.height*.4f,source.texture.width,source.texture.height*.6f),new Vector2(.5f,.5f),100);upperSprite.name="Festival upper composition";AssetDatabase.CreateAsset(upperSprite,spritePath);}
            var upper=safe.Find("Illustrated shop") as RectTransform;
            if(!upper){upper=New("Illustrated shop",safe);var space=upper.gameObject.AddComponent<LayoutElement>();space.minHeight=0;space.preferredHeight=0;space.flexibleHeight=1;var r=Fill("Shop and salesman",upper);r.offsetMin=new Vector2(0,-240);Picture("",r,upperSprite,false);}
            upper.SetSiblingIndex(1);
            var brand=safe.Find("Branding");
            var title=brand.Find("Rocket Raja").GetComponent<TMP_Text>();
            WelcomeRect(title.rectTransform,.09f,.24f,.91f,.58f);title.rectTransform.localScale=new Vector3(1,1.15f,1);title.fontSizeMin=105;title.fontSizeMax=title.fontSize=113;
            foreach(string name in new[]{"button-primary","button-secondary"})ImportWelcomeSprite(name);
            foreach(string name in new[]{"Primary Button","Secondary Button","Tertiary Button"})
            {var image=(Image)flow.Find(name).GetComponent<Button>().targetGraphic;image.sprite=WelcomeSprite(name=="Primary Button"?"button-primary":"button-secondary");image.type=Image.Type.Simple;}
            var footer=flow.Find("Footer");
            var note=footer.GetComponentsInChildren<TMP_Text>(true).First();
            WelcomeRect(note.rectTransform,.02f,0,.98f,.72f);note.enableAutoSizing=false;note.fontSize=28;note.overflowMode=TextOverflowModes.Overflow;
            note.text="Guest progress stays on this device.\nGoogle progress follows you between devices.";
            foreach(var rule in flow.Find("Heading Ornament").GetComponentsInChildren<Image>(true))rule.gameObject.SetActive(true);
            var line=flow.Find("Heading Ornament/Gold rule") as RectTransform;
            if(line){WelcomeRect(line,.25f,.45f,.75f,.55f);line.GetComponent<Image>().color=new Color32(200,138,44,255);}
            var headingText=flow.Find("Heading").GetComponent<TMP_Text>();headingText.fontSize=headingText.fontSizeMax=81;headingText.fontSizeMin=65;
            Canvas.ForceUpdateCanvases();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            return "Saved short-screen illustration region, full footer and matching button faces";
        }
        static void ImportWelcomeSprite(string name)
        {
            string path = WelcomeArt + name + ".png"; AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed; var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        static void WelcomeRect(RectTransform r, float x0, float y0, float x1, float y1)
        { r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1); r.offsetMin = r.offsetMax = Vector2.zero; r.localScale = Vector3.one; }
        static void WelcomeHeight(Transform t, float value)
        { var e = t.GetComponent<LayoutElement>(); e.minHeight = e.preferredHeight = value; e.flexibleHeight = 0; }
        static void WelcomeText(TMP_Text text, TMP_FontAsset font, float size, Color color)
        { text.font = font; text.fontSize = size; text.fontStyle = FontStyles.Normal; text.color = color; text.alignment = TextAlignmentOptions.Center; text.margin = Vector4.zero; }
        static void WelcomeFlowText(Transform flow, string name, float height, TMP_FontAsset font, float size, Color color)
        { WelcomeHeight(flow.Find(name), height); WelcomeText(flow.Find(name).GetComponent<TMP_Text>(), font, size, color); }
    }
}
