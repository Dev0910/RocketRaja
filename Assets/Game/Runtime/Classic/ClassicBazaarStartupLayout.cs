using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Fits the existing startup recovery controls. Owns no session commands or UI factories.</summary>
    [ExecuteAlways]
    public sealed class ClassicBazaarStartupLayout : MonoBehaviour
    {
        public RectTransform safe,window,header,actions,body,content;
        public TMP_Text heading;
        public Button[] buttons;
        public GameObject illustration;
        void OnEnable()=>Apply();
        void LateUpdate()=>Apply();
        void OnDisable(){if(illustration)illustration.SetActive(false);}
        public void Apply()
        {
            if(!safe||!window||!heading||safe.rect.width<=0)return;
            if(illustration&&!illustration.activeSelf)illustration.SetActive(true);
            window.anchorMin=new Vector2(0,.5f);window.anchorMax=new Vector2(1,.5f);window.pivot=new Vector2(.5f,.5f);window.sizeDelta=new Vector2(-72,window.sizeDelta.y);window.anchoredPosition=Vector2.zero;
            int count=0;foreach(var button in buttons)if(button&&button.gameObject.activeSelf)count++;
            float head=Mathf.Max(192,Mathf.Ceil((heading.GetPreferredValues(heading.text,Mathf.Max(120,window.rect.width-72),0).y+72)/12)*12);
            float foot=count==0?0:count*144+Mathf.Max(0,count-1)*24+72;
            header.anchorMin=new Vector2(0,1);header.anchorMax=Vector2.one;header.pivot=new Vector2(.5f,1);header.sizeDelta=new Vector2(-6,head);header.anchoredPosition=new Vector2(0,-3);
            actions.anchorMin=Vector2.zero;actions.anchorMax=new Vector2(1,0);actions.pivot=new Vector2(.5f,0);actions.sizeDelta=new Vector2(-6,foot);actions.anchoredPosition=new Vector2(0,3);
            body.anchorMin=Vector2.zero;body.anchorMax=Vector2.one;body.offsetMin=new Vector2(3,foot+3);body.offsetMax=new Vector2(-3,-head-3);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float desired=head+foot+Mathf.Max(180,LayoutUtility.GetPreferredHeight(content));
            window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Mathf.Min(Mathf.Max(360,safe.rect.height-96),Mathf.Max(840,desired)));
        }
    }
}
