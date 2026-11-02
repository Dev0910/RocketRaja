using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    [Serializable] public sealed class ClassicInfoFit { public LayoutElement layout; public TMP_Text copy, values; [NonSerialized] public string lastText; [NonSerialized] public float lastWidth; }
    /// <summary>Responsive constraints for existing Editor-authored geometry; never assembles UI.</summary>
    [ExecuteAlways]
    public sealed class ClassicLayout : MonoBehaviour
    {
        public ClassicUI ui;
        public RectTransform safe,world,panels,income,skills;
        public ClassicInfoFit[] infoRows=Array.Empty<ClassicInfoFit>();
        public Graphic[] touchGraphics=Array.Empty<Graphic>();
        void LateUpdate()=>Apply();
        public void Apply()
        {
            if(!ui||!safe||safe.rect.height<=0)return;
            float height=safe.rect.height,worldHeight=Mathf.Clamp(height*(height<600?.5f:.51f),220,470),edge=1-worldHeight/height;
            world.anchorMin=new Vector2(0,edge);panels.anchorMax=new Vector2(1,edge);
            bool compact=panels.rect.height<230;income.sizeDelta=new Vector2(income.sizeDelta.x,compact?65:77);income.anchoredPosition=new Vector2(0,compact?78:88);skills.sizeDelta=new Vector2(0,compact?74:82);
            foreach(var graphic in touchGraphics)
            {
                if(!graphic||!graphic.gameObject.activeInHierarchy)continue;
                var rect=graphic.rectTransform.rect;
                float x=Mathf.Max(0,(48-rect.width)*.5f),y=Mathf.Max(0,(48-rect.height)*.5f);
                graphic.raycastPadding=new Vector4(-x,-y,-x,-y);
            }
            foreach(var row in infoRows)
            {
                if(!row.layout||!row.copy||row.copy.rectTransform.rect.width<20)continue;
                string signature=row.copy.text+"|"+(row.values?row.values.text:"");
                if(row.lastText==signature&&Mathf.Approximately(row.lastWidth,row.copy.rectTransform.rect.width))continue;row.lastText=signature;row.lastWidth=row.copy.rectTransform.rect.width;
                float preferred=Mathf.Max(70,row.copy.GetPreferredValues(row.copy.text,row.copy.rectTransform.rect.width,0).y+52);
                if(row.values)preferred=Mathf.Max(preferred,row.values.GetPreferredValues(row.values.text,row.values.rectTransform.rect.width,0).y+52);
                row.layout.minHeight=row.layout.preferredHeight=preferred;
            }
            foreach(var dialog in ui.dialogs)
            {
                var area=(RectTransform)dialog.window.parent;area.anchorMin=safe.anchorMin;area.anchorMax=safe.anchorMax;
                if(dialog.route=="CardDetail")continue;
                if(dialog.scroll&&dialog.scroll.content)
                {
                    var body=(RectTransform)dialog.scroll.transform;
                    // Authored fixed-content dialogs retain their correct size while inactive in Edit Mode.
                    // Unity's inactive LayoutGroup otherwise reports zero preferred height.
                    float desired=dialog.preferredHeight>0?dialog.preferredHeight:LayoutUtility.GetPreferredHeight(dialog.scroll.content)+body.offsetMin.y-body.offsetMax.y;
                    dialog.window.anchorMin=new Vector2(0,.5f);dialog.window.anchorMax=new Vector2(1,.5f);dialog.window.pivot=new Vector2(.5f,.5f);dialog.window.sizeDelta=new Vector2(0,Mathf.Min(area.rect.height,Mathf.Max(220,desired)));dialog.window.anchoredPosition=Vector2.zero;
                }
            }
        }
    }
}
