using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Preplaces motion assets/references in the existing approved importer. Edit Mode only.</summary>
    public static class ClassicMotionAuthoring
    {
        static Sprite Texture(string name,bool star)
        {
            string path="Assets/Game/ClassicUI/"+name+".png";
            if(!File.Exists(path))
            {
                const int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);var pixels=new Color[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    float dx=Mathf.Abs((x+.5f)/size*2-1),dy=Mathf.Abs((y+.5f)/size*2-1);
                    float alpha=star?Mathf.Clamp01((1-Mathf.Sqrt(dx)-Mathf.Sqrt(dy))*20):Mathf.Pow(Mathf.Clamp01(1-Mathf.Sqrt(dx*dx+dy*dy)),2);
                    pixels[y*size+x]=new Color(1,1,1,alpha);
                }
                texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static void Group(GameObject target){if(!target.GetComponent<CanvasGroup>())target.AddComponent<CanvasGroup>();}
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Author motion in Edit Mode");var ui=ClassicImportQA.UI;var motion=ui.motion;
            var glow=Texture("Motion soft glow",false);motion.starSprite=Texture("Motion four-point spark",true);
            var world=ui.transform.Find("SafeArea/Shop");motion.tapHint=(RectTransform)world.Find("Tap invitation");motion.tapHintFace=motion.tapHint.GetComponent<Image>();
            motion.keeper.pivot=new Vector2(.8f,0);
            for(int i=0;i<motion.lanterns.Length;i++)
            {
                var image=motion.lanterns[i];image.sprite=glow;image.type=Image.Type.Simple;image.color=new Color32(255,192,72,120);
                var rect=image.rectTransform;rect.anchorMin=new Vector2(i==0?-.02f:.88f,.54f);rect.anchorMax=new Vector2(i==0?.12f:1.02f,.71f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            }
            motion.skillGlowImages=motion.skillGlows.Select(g=>g.GetComponent<Image>()).ToArray();motion.skillArtwork=ui.homeSkillImages;
            foreach(var image in motion.skillGlowImages){image.sprite=glow;image.type=Image.Type.Simple;}
            const string materialPath="Assets/Game/ClassicUI/Cooldown.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material){material=new Material(Shader.Find("UI/Classic Cooldown"));AssetDatabase.CreateAsset(material,materialPath);}motion.cooldownMaterial=material;
            foreach(var name in new[]{"Next display","Income","Skills"})Group(ui.transform.Find("SafeArea/Panels/Home/"+name).gameObject);
            Group(ui.imageBindings.Single(b=>b.key=="full-art").image.gameObject);Group(ui.revealResult);
            foreach(var product in ui.products)product.artFrame=(RectTransform)product.progress.transform.parent.parent;
            Group(ui.transform.Find("SafeArea/Panels/Shop/Scroll/Viewport/Content/Balance").gameObject);
            foreach(var control in ui.GetComponentsInChildren<ClassicControl>(true))
            {
                var colors=control.GetComponent<Button>().colors;colors.highlightedColor=new Color(1,.98f,.92f,1);colors.pressedColor=new Color(.94f,.90f,.83f,1);colors.fadeDuration=.16f;control.GetComponent<Button>().colors=colors;
                if(control.action=="Card")control.motionKind=1;
                if(control.transform.parent==world&&control.transform.Find("Merchandise")){control.motionKind=2;control.motionTarget=(RectTransform)control.transform.Find("Merchandise");}
                if(control.action=="Card"||control.transform.Find("Product frame"))
                {
                    var image=control.action=="Card"?control.GetComponent<Button>().targetGraphic as Image:control.transform.Find("Product frame").GetComponent<Image>();
                    if(!image)continue;if(control.action!="Card")control.motionTarget=image.rectTransform;
                    var shadow=image.GetComponent<Shadow>();if(!shadow)shadow=image.gameObject.AddComponent<Shadow>();shadow.effectDistance=new Vector2(0,-3);
                    string frameName=image.sprite?image.sprite.name.ToLowerInvariant():"";shadow.effectColor=frameName.Contains("legendary")?new Color(.84f,.59f,.15f,.3f):frameName.Contains("rare")?new Color(.47f,.13f,.32f,.26f):new Color(.1f,.25f,.35f,.22f);shadow.enabled=false;control.motionShadow=shadow;
                }
                EditorUtility.SetDirty(control);
            }
            var stage=ui.revealCard;var halo=stage.Find("Reveal rarity glow");if(!halo)halo=stage.parent.Find("Reveal rarity glow");
            if(!halo){var go=new GameObject("Reveal rarity glow",typeof(RectTransform),typeof(Image),typeof(CanvasGroup),typeof(LayoutElement));halo=go.transform;go.GetComponent<LayoutElement>().ignoreLayout=true;}
            halo.SetParent(stage,false);halo.SetAsFirstSibling();var hr=(RectTransform)halo;hr.anchorMin=Vector2.zero;hr.anchorMax=Vector2.one;hr.pivot=new Vector2(.5f,.5f);hr.offsetMin=new Vector2(-25,-25);hr.offsetMax=new Vector2(25,25);
            motion.revealGlowImage=halo.GetComponent<Image>();motion.revealGlowImage.sprite=glow;motion.revealGlowImage.color=new Color32(98,158,180,255);motion.revealGlowImage.raycastTarget=false;motion.revealGlow=halo.GetComponent<CanvasGroup>();motion.revealGlow.alpha=0;
            EditorUtility.SetDirty(ui);EditorUtility.SetDirty(motion);EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);EditorSceneManager.SaveScene(ui.gameObject.scene);
            return "Saved native motion references, glow/spark assets, skill states and hover feedback. "+ClassicImportQA.Audit();
        }
    }
}
