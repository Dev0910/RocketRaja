using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class ClassicBazaarAuthoring
    {
        static void StartupRecovery(GameObject root)
        {
            var owner=root.GetComponent<BootstrapController>();var so=new SerializedObject(owner);
            var panel=((GameObject)so.FindProperty("popupPanel").objectReferenceValue).GetComponent<RectTransform>();
            var heading=(TMP_Text)so.FindProperty("heading").objectReferenceValue;var description=(TMP_Text)so.FindProperty("description").objectReferenceValue;var hint=(TMP_Text)so.FindProperty("hint").objectReferenceValue;
            var scroll=panel.GetComponentInChildren<ScrollRect>(true);var content=scroll.content;
            var rootLayout=panel.GetComponent<VerticalLayoutGroup>();if(rootLayout)rootLayout.enabled=false;var size=panel.GetComponent<BootstrapPopupSize>();if(size)size.enabled=false;
            foreach(var fitter in panel.GetComponents<ContentSizeFitter>())fitter.enabled=false;
            foreach(Transform child in panel)if(child!=scroll.transform&&child.name!="Bazaar header"&&child.name!="Bazaar actions"&&child.name!="Bazaar parchment")child.gameObject.SetActive(false);
            Surface(panel,theme.window,false);panel.GetComponent<Image>().pixelsPerUnitMultiplier=1/3f;Ensure<Mask>(panel.gameObject).showMaskGraphic=true;
            var header=New("Bazaar header",panel);var hp=Ensure<Image>(header.gameObject);hp.sprite=null;hp.color=theme.raised;hp.raycastTarget=false;
            heading.transform.SetParent(header,false);Text(heading,66,heading:true,align:TextAlignmentOptions.Center);Stretch(heading.rectTransform,36);Ignore(heading.rectTransform);
            var footer=New("Bazaar actions",panel);var fp=Ensure<Image>(footer.gameObject);fp.sprite=null;fp.color=theme.raised;fp.raycastTarget=false;Flow(footer,36,36,24);
            var buttons=new[]{"primaryButton","secondaryButton","tertiaryButton"}.Select(k=>(Button)so.FindProperty(k).objectReferenceValue).ToArray();
            for(int i=0;i<buttons.Length;i++)
            {
                var button=buttons[i];button.transform.SetParent(footer,false);Height(R(button.transform),144);button.transition=Selectable.Transition.None;
                var image=(Image)button.targetGraphic;image.sprite=i==0?theme.primary:theme.secondary;image.overrideSprite=null;image.color=Color.white;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=4/3f;
                foreach(var t in button.GetComponentsInChildren<TMP_Text>(true)){Text(t,42,true,color:i==0?theme.paper:theme.ink,align:TextAlignmentOptions.Center);Stretch(t.rectTransform,24);}
            }
            foreach(Transform child in content)child.gameObject.SetActive(false);
            var card=New("Bazaar recovery message",content);card.gameObject.SetActive(true);Surface(card,theme.section,false);card.GetComponent<Image>().pixelsPerUnitMultiplier=1/3f;Flow(card,36,36,24);Preferred(card);
            description.transform.SetParent(card,false);description.gameObject.SetActive(true);Text(description,42,color:theme.ink);Preferred(description.rectTransform);
            hint.transform.SetParent(card,false);hint.gameObject.SetActive(true);Text(hint,36,color:theme.muted);Preferred(hint.rectTransform);
            Flow(content,36,48,36);Ensure<ContentSizeFitter>(content.gameObject).verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            foreach(var image in new[]{scroll.GetComponent<Image>(),scroll.viewport.GetComponent<Image>(),content.GetComponent<Image>()})if(image){image.sprite=null;image.color=Color.clear;}
            foreach(var t in panel.GetComponentsInChildren<TMP_Text>(true))foreach(var shadow in t.GetComponents<Shadow>())shadow.enabled=false;
            var paper=New("Bazaar parchment",panel);Stretch(paper,3);paper.SetAsFirstSibling();var texture=Ensure<Image>(paper.gameObject);texture.sprite=theme.paperTexture;texture.type=Image.Type.Tiled;texture.pixelsPerUnitMultiplier=1/3f;texture.raycastTarget=false;
            var art=New("Bazaar recovery illustration",root.transform);art.SetAsFirstSibling();Stretch(art);
            var artwork=Ensure<Image>(art.gameObject);artwork.sprite=root.transform.Find("Local Loading Presentation/Festival Illustration").GetComponent<Image>().sprite;artwork.raycastTarget=false;
            var aspect=Ensure<AspectRatioFitter>(art.gameObject);aspect.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;aspect.aspectRatio=artwork.sprite.rect.width/artwork.sprite.rect.height;
            var layout=Ensure<ClassicBazaarStartupLayout>(panel.gameObject);layout.safe=R(panel.parent);layout.window=panel;layout.header=header;layout.actions=footer;layout.body=R(scroll.transform);layout.content=content;layout.heading=heading;layout.buttons=buttons;layout.illustration=art.gameObject;
            owner.PreviewState("loading");art.gameObject.SetActive(false);
        }
    }
}
