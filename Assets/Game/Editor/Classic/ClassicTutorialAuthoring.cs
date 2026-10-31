using System;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        const string TutorialArt="Assets/Game/ClassicUI/Tutorial/";
        [MenuItem("Rocket Raja/Classic/Author approved shopkeeper tutorial")]
        public static string AuthorTutorial()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var scene=SceneManager.GetSceneByPath(ScenePath);
            if(!scene.isLoaded || scene.isDirty)throw new InvalidOperationException("Open saved Main before scoped authoring");
            ui=scene.GetRootGameObjects().Single(g=>g.name==RootName).GetComponent<ClassicUI>();
            if(ui.tutorial)throw new InvalidOperationException("Tutorial exists; refine it without duplicating its hierarchy");
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject,"Author approved tutorial");
            body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Body SDF.asset");
            bold=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Bold SDF.asset");
            heading=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local+"Fonts/Heading SDF.asset");
            serif=TutorialFont();blue=ui.blueButton;ivory=ui.ivoryButton;
            rounded=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            foreach(string name in new[]{"tutorial-pointing-hand.png","ivory-gold-frame.png"}) {
                string path=TutorialArt+name;AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
                importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            var safe=(RectTransform)ui.transform.Find("SafeArea");var world=(RectTransform)safe.Find("Shop");
            var t=Undo.AddComponent<ClassicTutorial>(ui.gameObject);t.ui=ui;ui.tutorial=t;t.safeArea=safe;t.tapBanner=ui.motion.tapHint;
            var overlay=Fill("Shopkeeper tutorial",safe);overlay.SetAsLastSibling();
            // Relative to Shop, following its existing responsive height with authored anchors.
            var bubble=New("Ivory and gold dialogue",world);
            bubble.anchorMin=new Vector2(.19f,.26f);bubble.anchorMax=new Vector2(.93f,.26f);
            bubble.pivot=new Vector2(1,0);bubble.sizeDelta=new Vector2(0,92);bubble.anchoredPosition=Vector2.zero;
            var frame=Face(Fill("Decorative parchment frame",bubble),Color.white,AssetDatabase.LoadAssetAtPath<Sprite>(TutorialArt+"ivory-gold-frame.png"));
            var shadow=frame.gameObject.AddComponent<Shadow>();shadow.effectDistance=new Vector2(0,-2);shadow.effectColor=new Color(.30f,.17f,.07f,.25f);
            var buttonImage=Face(Fill("Tap dialogue to dismiss",bubble),Color.clear,null,true);
            t.dismiss=buttonImage.gameObject.AddComponent<Button>();t.dismiss.targetGraphic=buttonImage;t.dismiss.transition=Selectable.Transition.None;t.dismiss.navigation=new Navigation{mode=Navigation.Mode.None};
            t.title=Label("",Top("Live heading",bubble,24,9),"Let’s open your stall!",13.5f,true,new Color32(19,62,96,255),TextAlignmentOptions.TopLeft,serif);
            t.title.rectTransform.offsetMin=new Vector2(29,t.title.rectTransform.offsetMin.y);t.title.rectTransform.offsetMax=new Vector2(-17,t.title.rectTransform.offsetMax.y);
            t.title.fontStyle=FontStyles.Normal;t.title.enableAutoSizing=true;t.title.fontSizeMin=11;t.title.fontSizeMax=13.5f;
            var textRect=Fill("Live guidance",bubble);textRect.offsetMin=new Vector2(17,22);textRect.offsetMax=new Vector2(-20,-33);
            t.copy=Label("",textRect,"Tap the counter to earn rupees. We’ll put your first earnings back into the shop.",11);
            t.copy.enableAutoSizing=true;t.copy.fontSizeMin=9;t.copy.fontSizeMax=11;t.copy.alignment=TextAlignmentOptions.TopLeft;
            t.bubble=bubble;t.bubbleGroup=bubble.gameObject.AddComponent<CanvasGroup>();bubble.SetAsLastSibling();
            t.hand=New("Centered pointing hand",overlay);t.hand.anchorMin=t.hand.anchorMax=new Vector2(.5f,.5f);
            t.hand.pivot=new Vector2(.205630f,.951356f);t.hand.sizeDelta=new Vector2(48.6f,48.6f);
            Face(t.hand,Color.white,AssetDatabase.LoadAssetAtPath<Sprite>(TutorialArt+"tutorial-pointing-hand.png"));
            ClassicControl Control(string action,string arg)=>ui.GetComponentsInChildren<ClassicControl>(true).First(c=>c.action==action&&c.parameter==arg);
            t.training=Control("BuyLevels","-1").GetComponent<Button>();
            t.achievement=Control("Navigate","Achievements").GetComponent<Button>();
            t.prestige=Control("Navigate","Prestige").GetComponent<Button>();t.freePull=Control("Draw","1").GetComponent<Button>();
            var home=ui.panels.Single(v=>v.route=="Home");t.objectiveRoot=home.root.transform.Find("Next display").gameObject;
            t.objective=ui.textBindings.Single(b=>b.key=="home-goal").text;
            t.objectiveRoot.GetComponent<ClassicControl>().action="TutorialObjective";
            // Reuse the approved production popup shell for explicit tutorial settings.
            var source=ui.dialogs.Single(v=>v.route=="Guidance");
            var helpRoot=UnityEngine.Object.Instantiate(source.root,source.root.transform.parent,false);
            Undo.RegisterCreatedObjectUndo(helpRoot,"Author tutorial help");helpRoot.name="TutorialHelp";
            var window=helpRoot.GetComponentsInChildren<RectTransform>(true).Single(r=>r.name=="Window");
            var scroll=helpRoot.GetComponentInChildren<ScrollRect>(true);
            foreach(Transform child in scroll.content.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
            helpRoot.GetComponentsInChildren<TMP_Text>(true).First(l=>l.transform.parent.name=="Header").text="Tutorial guidance";
            var info=New("Tutorial status",scroll.content);Height(info,100);
            t.helpStatus=Label("",Fill("Copy",info,10),"Replay guidance at your current progress. Skipping keeps all progress.",12);
            foreach(var entry in new[]{("TutorialResume","Continue guidance"),("TutorialReplay","Replay guidance"),("TutorialSkip","Skip tutorial")}) {
                var r=New(entry.Item2,scroll.content);Height(r,48);var b=Button(r,entry.Item1,"","",entry.Item2,ivory);
                b.GetComponent<ClassicControl>().htmlStyle=2;((Image)b.targetGraphic).sprite=ui.popupSecondary;
            }
            foreach(var c in helpRoot.GetComponentsInChildren<ClassicControl>(true)) {c.owner=ui;if(c.action=="LearnTip"){c.action="Close";var label=c.GetComponentInChildren<TMP_Text>();if(label)label.text="Back";}}
            var help=new ClassicView{route="TutorialHelp",root=helpRoot,window=window,group=helpRoot.GetComponent<CanvasGroup>(),scroll=scroll,preferredHeight=430};
            ui.dialogs=ui.dialogs.Concat(new[]{help}).ToArray();
            var settingsView=ui.dialogs.Single(v=>v.route=="Settings");
            var setting=New("Tutorial guidance",settingsView.scroll.content);Height(setting,60);
            var settingButton=Button(setting,"Navigate","TutorialHelp","","Tutorial guidance",ivory);settingButton.GetComponent<ClassicControl>().htmlStyle=2;((Image)settingButton.targetGraphic).sprite=ui.popupSecondary;
            // Zero means content-sized. Preserve it so adding a row cannot collapse the dialog.
            if(settingsView.preferredHeight>0)
                settingsView.preferredHeight+=60+settingsView.scroll.content.GetComponent<VerticalLayoutGroup>().spacing;
            bubble.gameObject.SetActive(false);t.hand.gameObject.SetActive(false);helpRoot.SetActive(false);
            EditorUtility.SetDirty(ui);EditorUtility.SetDirty(t);
            ui.GetComponent<ClassicLayout>().Apply();Canvas.ForceUpdateCanvases();
            foreach(var text in bubble.GetComponentsInChildren<RedesignSavedText>(true))text.ForceMeshUpdate(true,true);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Saved approved tutorial in Main; all dialogue, hand, help controls and material slots preauthored.";
        }
        static TMP_FontAsset TutorialFont()
        {
            string path=TutorialArt+"Fraunces 700 SDF.asset";var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(font)return font;
            var source=AssetDatabase.LoadAssetAtPath<Font>(TutorialArt+"fraunces.ttf");
            font=TMP_FontAsset.CreateFontAsset(source,64,7,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic);
            font.name="Tutorial Fraunces SDF";font.TryAddCharacters(string.Concat(Enumerable.Range(32,224).Select(c=>(char)c))+"₹×→’!",out _);
            AssetDatabase.CreateAsset(font,path);AssetDatabase.AddObjectToAsset(font.material,font);
            foreach(var texture in font.atlasTextures)AssetDatabase.AddObjectToAsset(texture,font);
            font.atlasPopulationMode=AtlasPopulationMode.Static;EditorUtility.SetDirty(font);AssetDatabase.SaveAssetIfDirty(font);return font;
        }
        public static string RefineTutorialHelp()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
            var scene=SceneManager.GetSceneByPath(ScenePath);
            ui=scene.GetRootGameObjects().Single(g=>g.name==RootName).GetComponent<ClassicUI>();
            var t=ui.tutorial;var parent=t.objectiveRoot.transform;
            var headerFont=TutorialFont();t.title.font=headerFont;t.title.fontStyle=FontStyles.Normal;
            foreach(var slot in ((RedesignSavedText)t.title).savedMaterialSlots){slot.fontAsset=headerFont;slot.sharedMaterial=headerFont.material;EditorUtility.SetDirty(slot);}
            t.title.ForceMeshUpdate(true,true);EditorUtility.SetDirty(t.title);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            if(parent.Find("Replay tutorial tip"))return "Tutorial tip already authored";
            Undo.RegisterFullObjectHierarchyUndo(t.objectiveRoot,"Author tutorial replay tip");
            var source=ui.panels.Single(v=>v.route=="Player").root.transform.Find("Header/Information");
            var button=UnityEngine.Object.Instantiate(source.gameObject,parent,false);Undo.RegisterCreatedObjectUndo(button,"Author tutorial tip");button.name="Replay tutorial tip";
            var rect=(RectTransform)button.transform;rect.anchorMin=rect.anchorMax=new Vector2(1,.5f);rect.pivot=new Vector2(1,.5f);rect.sizeDelta=new Vector2(48,48);rect.anchoredPosition=Vector2.zero;
            var control=button.GetComponent<ClassicControl>();control.owner=ui;control.action="TutorialTip";control.parameter="";
            var label=button.GetComponentInChildren<TMP_Text>(true);label.text="?";label.fontSize=label.fontSizeMax=15;
            var goal=t.objective.rectTransform;goal.offsetMax=new Vector2(-50,goal.offsetMax.y);
            t.objective.enableAutoSizing=true;t.objective.fontSizeMin=9;t.objective.fontSizeMax=12;
            EditorUtility.SetDirty(t.objective);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Saved Home objective replay-tip control using production information ring";
        }
    }
}
