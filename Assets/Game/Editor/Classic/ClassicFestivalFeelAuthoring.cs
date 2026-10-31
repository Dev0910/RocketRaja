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
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static class ClassicFestivalFeelAuthoring
    {
        const string Folder="Assets/Game/ClassicUI/FestivalFeel/";
        [MenuItem("Rocket Raja/Classic/Apply reviewed festival animation and audio")]
        public static string Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Idle Edit Mode required");
            var scene=SceneManager.GetSceneByPath("Assets/Scenes/Main.unity");
            if(!scene.isLoaded||scene.isDirty)throw new InvalidOperationException("Open saved Main first; preserve unsaved edits");
            var ui=scene.GetRootGameObjects().Where(g=>g.activeSelf).Select(g=>g.GetComponent<ClassicUI>()).Single(c=>c);
            if(ui.festivalFeel)return "Reviewed festival pool already authored";
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            var ring=Art("ring",64,(x,y)=>{float r=Vector2.Distance(new Vector2(x,y),new Vector2(.5f,.5f));return new Color(1,1,1,Mathf.Clamp01((.025f-Mathf.Abs(r-.43f))*64));});
            var glow=Art("warm-glow",128,(x,y)=>new Color(1,.65f,.16f,Mathf.Pow(Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(.5f,.5f))*1.6f),1.5f)));
            var bloom=Art("prestige-bloom",256,(x,y)=>{float dx=x-.5f,dy=y-.5f,r=Mathf.Sqrt(dx*dx+dy*dy),a=Mathf.Atan2(dy,dx),ray=Mathf.Pow(Mathf.Max(0,Mathf.Cos(a*12)),24);float mask=Mathf.Clamp01((r-.08f)*35)*Mathf.Clamp01((.48f-r)*12);return new Color(1,.86f,.48f,ray*mask);});
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject,"Author reviewed festival feel");
            var feel=Undo.AddComponent<ClassicFestivalFeel>(ui.gameObject);feel.ui=ui;ui.festivalFeel=feel;
            var world=ui.GetComponent<ClassicLayout>().world;
            feel.scene=Fill("Festival skill and prestige presentation",world);
            var coach=world.Find("Ivory and gold dialogue");if(coach)feel.scene.SetSiblingIndex(coach.GetSiblingIndex());
            feel.warmLight=Picture(Fill("Festival Rush warmth",feel.scene),glow);feel.warmLight.gameObject.SetActive(false);
            feel.hands=new Image[2];
            for(int i=0;i<2;i++){var r=Box("Working hands "+i,feel.scene,new Vector2(i==0?.32f:.58f,.19f),new Vector2(34,34));feel.hands[i]=Picture(r,ui.skillArt[1]);feel.hands[i].gameObject.SetActive(false);}
            var parcel=Box("Bulk Order parcel",feel.scene,new Vector2(0,.17f),new Vector2(44,44));parcel.pivot=new Vector2(.5f,.5f);
            feel.parcel=Picture(parcel,ui.skillArt[2]);feel.parcel.gameObject.SetActive(false);
            feel.prestigeVeil=Picture(Fill("Prestige lantern dim",feel.scene),null);feel.prestigeVeil.color=new Color(.082f,.075f,.17f,0);
            feel.prestigeBloom=Picture(Box("Prestige firework bloom",feel.prestigeVeil.transform,new Vector2(.5f,.6f),Vector2.one*160),bloom);feel.prestigeVeil.gameObject.SetActive(false);
            feel.fireworkTrail=Picture(Box("Idle firework trail",feel.scene,new Vector2(.5f,.5f),new Vector2(2,17)),null);feel.fireworkTrail.gameObject.SetActive(false);
            feel.fireworkSparks=new Image[20];
            for(int i=0;i<20;i++){feel.fireworkSparks[i]=Picture(Box("Idle firework spark "+i,feel.scene,new Vector2(.5f,.5f),Vector2.one*3),ui.motion.starSprite);feel.fireworkSparks[i].gameObject.SetActive(false);}
            feel.overlay=Fill("Festival feedback - saved pool",ui.transform);Overlay(feel.overlay.gameObject,124);
            // Existing 64 sparks/coins must remain visible over every dialog, with no input interception.
            Overlay(ui.motion.effectRoot.gameObject,125);
            foreach(var p in ui.motion.particles)p.raycastTarget=false;
            feel.rings=new Image[6];for(int i=0;i<6;i++){feel.rings[i]=Picture(Box("Reward ring "+i,feel.overlay,new Vector2(.5f,.5f),Vector2.one*52),ring);feel.rings[i].gameObject.SetActive(false);}
            feel.tokens=new Image[3];for(int i=0;i<3;i++){feel.tokens[i]=Picture(Box("Kaju Katli flight "+i,feel.overlay,new Vector2(.5f,.5f),Vector2.one*24),ui.legacyCurrencyIcon.sprite);feel.tokens[i].gameObject.SetActive(false);}
            feel.payouts=new TMP_Text[4];
            for(int i=0;i<4;i++)
            {
                var r=Box("Tap payout "+i,feel.overlay,new Vector2(.5f,.5f),new Vector2(180,36));
                feel.payouts[i]=Label(ui,r,"+₹1",19,new Color32(255,242,191,255));
                var shadow=feel.payouts[i].gameObject.AddComponent<Shadow>();shadow.effectColor=new Color32(75,37,25,220);shadow.effectDistance=new Vector2(0,-1);
                feel.payouts[i].gameObject.SetActive(false);
            }
            var ribbon=Box("Milestone receipt",feel.overlay,new Vector2(.5f,.65f),new Vector2(270,48));
            var surface=Picture(ribbon,ui.ivoryButton);surface.type=Image.Type.Sliced;surface.pixelsPerUnitMultiplier=4;
            feel.milestoneGroup=ribbon.gameObject.AddComponent<CanvasGroup>();feel.milestoneGroup.interactable=feel.milestoneGroup.blocksRaycasts=false;
            var label=Fill("Milestone label",ribbon);label.offsetMin=new Vector2(10,6);label.offsetMax=new Vector2(-10,-6);
            feel.milestone=Label(ui,label,"Phuljhari · Level 10!",15,new Color32(22,75,128,255));ribbon.gameObject.SetActive(false);
            ui.audioFeedback.music.volume=.1f;ui.audioFeedback.effects.volume=.25f;ui.audioFeedback.explicitFeedback=true;
            EditorUtility.SetDirty(ui);EditorUtility.SetDirty(feel);EditorUtility.SetDirty(ui.audioFeedback.music);EditorUtility.SetDirty(ui.motion);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            return "Saved festival presentation: 64 existing particles, 20 idle sparks, 6 rings, 4 payout labels, 3 reward tokens and reviewed scene effects";
        }
        static void Overlay(GameObject go,int order)
        {
            var canvas=go.GetComponent<Canvas>();if(!canvas)canvas=Undo.AddComponent<Canvas>(go);
            canvas.overrideSorting=true;canvas.sortingOrder=order;
        }
        static RectTransform Fill(string name,Transform parent)
        {
            var r=Box(name,parent,Vector2.one*.5f,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        static RectTransform Box(string name,Transform parent,Vector2 anchor,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));Undo.RegisterCreatedObjectUndo(go,"Author festival effect");
            var r=(RectTransform)go.transform;r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=Vector2.one*.5f;r.sizeDelta=size;return r;
        }
        static Image Picture(RectTransform r,Sprite sprite)
        {
            var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;image.preserveAspect=sprite;return image;
        }
        static TMP_Text Label(ClassicUI ui,RectTransform parent,string text,int size,Color color)
        {
            // Clone the current authored font/material slots so no runtime TMP children are created.
            var go=Object.Instantiate(ui.tutorial.objective.gameObject,parent,false);Undo.RegisterCreatedObjectUndo(go,"Author effect text");go.name="Editable text";
            var r=(RectTransform)go.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            var t=go.GetComponent<TMP_Text>();t.text=text;t.fontSize=t.fontSizeMax=size;t.enableAutoSizing=false;t.color=color;t.raycastTarget=false;t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.NoWrap;
            return t;
        }
        static Sprite Art(string name,int size,Func<float,float,Color> pixel)
        {
            string path=Folder+name+".png";
            if(!File.Exists(path))
            {
                var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);var colors=new Color[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)colors[y*size+x]=pixel((x+.5f)/size,(y+.5f)/size);
                texture.SetPixels(colors);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
