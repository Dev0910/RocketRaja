using System;
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
    public static partial class ClassicBazaarAuthoring
    {
        static void SpecialViews()
        {
            ui.blueButton=ui.popupPrimary=theme.primary;ui.ivoryButton=ui.popupSecondary=theme.secondary;ui.disabledButton=ui.popupDisabled=theme.unavailable;ui.redButton=theme.destructive;
            foreach(var view in ui.dialogs)
            {
                foreach(var frame in view.window.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Painted gold and cobalt frame"))frame.gameObject.SetActive(false);
                foreach(var b in view.root.GetComponentsInChildren<ClassicControl>(true).Where(c=>c.htmlStyle>0&&c.action!="Setting"))
                {var button=b.GetComponent<Button>();if(button&&button.transform.parent.name!="Header"&&b.action!="BuyUpgrade"&&b.GetComponentsInChildren<TMP_Text>(true).Length==1){Button(button,b.htmlStyle==2);if(b.action=="DeveloperReset"||b.action=="ResetGame")((Image)button.targetGraphic).sprite=theme.destructive;}}
            }
            CardDetail();Statistics();Achievements();
            foreach(var input in new[]{ui.nameInput,ui.searchInput}.Where(x=>x))
            {
                Height(R(input.transform),56);Surface(R(input.transform),theme.shadeSurface,false);
                Text(input.textComponent,16,color:theme.ink);input.textComponent.margin=new Vector4(12,8,12,8);
                if(input.placeholder is TMP_Text placeholder){Text(placeholder,14,color:theme.muted);placeholder.margin=new Vector4(12,8,12,8);}
            }
            foreach(var v in ui.dialogs.Where(v=>v.scroll))
            foreach(Transform child in v.scroll.content)
            {
                if(child.name=="Reward artwork"||child.name=="Painted emblem")Height(R(child),64);
                if(child.name.StartsWith("Phuljhari appearance"))
                {Height(R(child),88);Surface(R(child),theme.section);foreach(var t in child.GetComponentsInChildren<TMP_Text>(true)){Text(t,14,true);Top(t.rectTransform,72,12,12,64);}}
            }
            var offline=Bound("offline-amount");if(offline){Text(offline,32,true,color:theme.success,align:TextAlignmentOptions.Center);Height(R(offline.transform.parent),52);Stretch(offline.rectTransform);}
            var status=View("Offline").scroll.content.Find("Ad status");if(status){Flow(R(status),0,0,0);Paragraphs(R(status));Preferred(R(status));}
            var note=View("Achievements").window.Find("Permanent rewards note");if(note)note.gameObject.SetActive(false);
            var exit=View("RecoveryExit");if(!exit.scroll.content.Find("Bazaar recovery explanation"))
            {var t=Object.Instantiate(ui.guidance.body,exit.scroll.content,false);t.name="Bazaar recovery explanation";t.gameObject.SetActive(true);t.text="Your existing save files stay on this device. Retry when saving is available. Leaving preserves your files for another attempt.";Text(t,14);Preferred(t.rectTransform);}
            exit.preferredHeight=312;
            var reveal=View("DrawReveal");
            foreach(var key in new[]{"reveal-rarity","reveal-result","reveal-effect"})
            {var t=Bound(key);if(t)Text(t,key=="reveal-result"?22:14,key!="reveal-effect",heading:key=="reveal-result",align:TextAlignmentOptions.Center);}
            if(ui.revealFront){var frame=ui.imageBindings.FirstOrDefault(b=>b.key=="reveal-frame")?.image;if(frame){frame.sprite=theme.rarity[0];frame.type=Image.Type.Sliced;frame.pixelsPerUnitMultiplier=1;}}
        }
        static void CardDetail()
        {
            var v=View("CardDetail");var content=v.scroll.content;
            foreach(Transform child in content)
            {
                if(child.name=="Painted outer border"||child.name=="Divider") {child.gameObject.SetActive(false);continue;}
                if(child.name=="Illustration")
                {
                    Height(R(child),180);Surface(R(child),theme.section);
                    var art=ui.imageBindings.First(x=>x.key=="full-art").image;Stretch(art.rectTransform,16);art.preserveAspect=true;
                    var inner=ui.imageBindings.First(x=>x.key=="full-inner").image;inner.enabled=false;
                }
                else if(child.name=="Level badge")
                {Height(R(child),28);Surface(R(child),theme.shadeSurface,false);var t=child.GetComponentInChildren<TMP_Text>(true);Text(t,12,true,align:TextAlignmentOptions.Center);Stretch(t.rectTransform);}
                else if(child.GetComponent<TMP_Text>())
                {var t=child.GetComponent<TMP_Text>();bool title=child.name=="Card name";Text(t,title?22:child.name=="Benefit"?18:14,heading:title,color:child.name=="Benefit"?theme.success:theme.ink,align:TextAlignmentOptions.Center);Preferred(R(child));}
            }
        }
        static void Statistics()
        {
            var v=View("Statistics");
            foreach(Transform card in v.scroll.content)
            {
                if(Legacy(card))continue;var root=R(card);Surface(root,theme.section);Flow(root,12,12,8);Preferred(root);
                var art=card.Find("Ledger artwork");if(art)art.gameObject.SetActive(false);
                var head=card.Find("Heading")?.GetComponent<TMP_Text>();if(head){Text(head,18,heading:true);Preferred(head.rectTransform);}
                foreach(Transform metric in card.Cast<Transform>().ToArray())
                {
                    var label=metric.Find("Label")?.GetComponent<TMP_Text>();var value=metric.Find("Value")?.GetComponent<TMP_Text>();
                    if(label&&value)
                    {
                        var r=R(metric);var g=Ensure<HorizontalLayoutGroup>(metric.gameObject);g.padding=new RectOffset(0,0,4,4);g.spacing=8;g.childControlWidth=g.childControlHeight=true;g.childForceExpandWidth=false;g.childForceExpandHeight=false;g.childAlignment=TextAnchor.MiddleLeft;Preferred(r,36);
                        Text(label,14);var le=Ensure<LayoutElement>(label.gameObject);le.minWidth=le.preferredWidth=0;le.flexibleWidth=1;le.preferredHeight=-1;
                        Text(value,14,true,align:TextAlignmentOptions.MidlineRight);Width(value.rectTransform,100);Preferred(value.rectTransform);
                        var rule=metric.Find("Rule");if(rule)Ignore(R(rule));
                    }
                    else if(metric.GetComponent<TMP_Text>()){var t=metric.GetComponent<TMP_Text>();if(t!=head){Text(t,14,color:theme.muted);Preferred(R(metric));}}
                }
            }
        }
        static void Achievements()
        {
            var v=View("Achievements");ui.achievementClaimReady=theme.primary;ui.achievementClaimProgress=theme.unavailable;ui.achievementClaimed=theme.secondary;
            foreach(var binding in ui.achievementRows)
            {
                var row=binding.claim.transform.parent;var root=R(row);RowFlow(root,152);
                foreach(Transform child in row)if(child.GetComponent<Image>()&&child!=binding.claim.transform)Ignore(R(child));
                var art=row.Find("Achievement artwork") as RectTransform;if(art){Ensure<LayoutElement>(art.gameObject).ignoreLayout=false;Width(art,44,48);art.SetSiblingIndex(1);}
                var column=CopyColumn(root);column.SetAsLastSibling();CopyText(binding.title,column,14,true);CopyText(binding.description,column,12);CopyText(binding.progress,column,12);CopyText(binding.reward,column,12,true);
                var track=R(binding.fill.transform.parent);track.SetParent(column,false);Height(track,6);Ensure<LayoutElement>(track.gameObject).ignoreLayout=false;Meter(binding.fill);
                binding.claim.transform.SetAsLastSibling();Width(R(binding.claim.transform),92,48);Button(binding.claim);Text(binding.action,12,true,color:theme.paper,align:TextAlignmentOptions.Center);
                if(binding.readyBadge){var group=Ensure<CanvasGroup>(binding.readyBadge);group.alpha=0;group.blocksRaycasts=false;}
            }
        }
        static void Guidance()
        {
            foreach(var row in ui.guidance.rows)
            {
                var root=R(row.root.transform);var panel=ui.panels.FirstOrDefault(v=>v.route==row.route);
                if(panel!=null){root.SetParent(panel.scroll.content,false);root.SetAsFirstSibling();}
                Surface(root,theme.section);var flow=Flow(root,12,12,4);flow.padding.right=40;Preferred(root,56);
                row.title.transform.SetParent(root,false);row.brief.transform.SetParent(root,false);Text(row.title,14,true);Text(row.brief,12,color:theme.muted);Preferred(row.title.rectTransform);Preferred(row.brief.rectTransform);
                foreach(var child in root.Cast<Transform>().Where(t=>t!=row.title.transform&&t!=row.brief.transform)){Ignore(R(child));if(child.name!="Bazaar surface")child.gameObject.SetActive(false);}
                var control=root.GetComponent<ClassicControl>();if(control)control.htmlStyle=0;
                if(row.route=="Home"){foreach(var g in root.GetComponents<LayoutGroup>())Object.DestroyImmediate(g);Top(root,12,24,12,56);Top(row.title.rectTransform,12,8,40,22);Top(row.brief.rectTransform,12,32,40,20);}
            }
            var reader=View("Guidance");foreach(var text in new[]{ui.guidance.heading,ui.guidance.body}){text.transform.SetParent(reader.scroll.content,false);Text(text,text==ui.guidance.heading?18:14,heading:text==ui.guidance.heading);Preferred(text.rectTransform);}
            foreach(Transform child in reader.scroll.content)if(child.GetComponentsInChildren<TMP_Text>(true).Length==0)child.gameObject.SetActive(false);
        }
        static void Tutorial()
        {
            var t=ui.tutorial;if(!t)return;
            foreach(var im in t.bubble.GetComponentsInChildren<Image>(true).Where(i=>i.name.Contains("Decorative")))im.gameObject.SetActive(false);
            Surface(t.bubble,theme.section);Flow(t.bubble,12,12,8);
            foreach(Transform child in t.bubble)if(child!=t.title.transform&&child!=t.copy.transform)Ignore(R(child));
            Text(t.title,18,heading:true);Preferred(t.title.rectTransform);Text(t.copy,12);Preferred(t.copy.rectTransform);
            Ensure<ContentSizeFitter>(t.bubble.gameObject).verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        }
        static void Bootstrap()
        {
            var scene=SceneManager.GetSceneByPath("Assets/Scenes/Bootstrap.unity");if(!scene.isLoaded)scene=EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity",OpenSceneMode.Additive);
            var root=scene.GetRootGameObjects().Single(g=>g.activeSelf&&g.GetComponent<RocketRaja.Cloud.BootstrapController>());
            var layer=root.transform.Find("Local Loading Presentation");
            if(layer)
            {
                var title=layer.Find("Loading Safe Area/Game Title").GetComponent<TMP_Text>();Text(title,84,heading:true,color:theme.paper,align:TextAlignmentOptions.Center);
                var label=layer.Find("Loading Safe Area/Loading Status/Loading Label").GetComponent<TMP_Text>();Text(label,42,true,color:theme.paper,align:TextAlignmentOptions.Center);
                var track=R(layer.Find("Loading Safe Area/Loading Status/Loading Track"));Box(track,Vector2.zero,new Vector2(1,0),Vector2.zero,new Vector2(0,48));
                foreach(var im in layer.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Loading Track"||i.name=="Loading Fill"))
                {bool fill=im.name=="Loading Fill";im.sprite=fill?AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Bootstrap/Art/Rounded Surface.png"):theme.barTrack;im.color=fill?theme.goldLight:Color.white;im.type=Image.Type.Sliced;var corners=Ensure<RocketRaja.LoadingBarCorners>(im.gameObject);corners.Refresh();}
                var interior=layer.Find("Loading Safe Area/Loading Status/Loading Track/Track Interior")?.GetComponent<Image>();if(interior)interior.color=theme.track;
            }
            foreach(var t in root.GetComponentsInChildren<TMP_Text>(true).Where(t=>!Legacy(t.transform)&&!t.transform.IsChildOf(layer)))Text(t,t.name=="Heading"?66:42,t.name!="Description",heading:t.name=="Heading");
            StartupRecovery(root);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
