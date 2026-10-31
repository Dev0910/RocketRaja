using System;
using System.Linq;
using RocketRaja.Classic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static partial class ClassicBazaarAuthoring
    {
        static HorizontalLayoutGroup RowFlow(RectTransform root,float minimum)
        {
            var old=root.GetComponent<VerticalLayoutGroup>();if(old)Object.DestroyImmediate(old);
            var g=Ensure<HorizontalLayoutGroup>(root.gameObject);g.padding=new RectOffset(12,12,12,12);g.spacing=8;g.childControlWidth=g.childControlHeight=true;g.childForceExpandWidth=false;g.childForceExpandHeight=false;g.childAlignment=TextAnchor.MiddleLeft;
            var e=Ensure<LayoutElement>(root.gameObject);e.minHeight=minimum;e.preferredHeight=-1;e.flexibleHeight=0;Surface(root,theme.section);return g;
        }
        static void Width(RectTransform r,float width,float height=-1)
        {
            var e=Ensure<LayoutElement>(r.gameObject);e.ignoreLayout=false;e.minWidth=e.preferredWidth=width;e.flexibleWidth=0;if(height>0){e.minHeight=e.preferredHeight=height;e.flexibleHeight=0;}
        }
        static RectTransform CopyColumn(RectTransform root)
        {
            var copy=New("Bazaar copy",root);Flow(copy,0,0,4);var e=Ensure<LayoutElement>(copy.gameObject);e.ignoreLayout=false;e.minWidth=e.preferredWidth=0;e.flexibleWidth=1;return copy;
        }
        static void CopyText(TMP_Text text,RectTransform parent,float size,bool bold=false)
        {
            text.transform.SetParent(parent,false);Text(text,size,bold,color:bold?theme.ink:theme.muted);
            var l=Ensure<LayoutElement>(text.gameObject);l.ignoreLayout=false;l.minHeight=0;l.preferredHeight=-1;l.flexibleHeight=0;
        }
        static RectTransform Common(Transform a,Transform b)
        {
            for(var t=a;t;t=t.parent)if(b.IsChildOf(t)||b==t)return R(t);
            throw new InvalidOperationException("Unrelated row bindings");
        }
        static void ProductRows()
        {
            foreach(var row in ui.products)
            {
                var root=Common(row.name.transform,row.buy.transform);RowFlow(root,104);
                var detail=row.name.GetComponentInParent<Button>(true);if(!detail||detail==row.buy)detail=root.GetComponentsInChildren<Button>(true).First(b=>b!=row.buy);
                var details=R(detail.transform);var previous=details.GetComponent<VerticalLayoutGroup>();if(previous)Object.DestroyImmediate(previous);
                var g=Ensure<HorizontalLayoutGroup>(details.gameObject);g.padding=new RectOffset();g.spacing=8;g.childControlWidth=g.childControlHeight=true;g.childForceExpandWidth=false;g.childForceExpandHeight=false;g.childAlignment=TextAnchor.MiddleLeft;
                var de=Ensure<LayoutElement>(details.gameObject);de.ignoreLayout=false;de.minWidth=0;de.preferredWidth=0;de.flexibleWidth=1;de.minHeight=72;
                var art=row.artFrame?row.artFrame:details.Find("Product frame") as RectTransform;
                if(art){art.SetParent(details,false);Width(art,52,68);var bg=art.GetComponent<Image>();if(bg){bg.sprite=null;bg.color=Color.clear;}}
                var copy=CopyColumn(details);CopyText(row.name,copy,14,true);CopyText(row.income,copy,12);CopyText(row.level,copy,12);
                row.buy.transform.SetParent(root,false);Button(row.buy);Width(R(row.buy.transform),92,52);Text(row.cost,13,true,color:theme.paper,align:TextAlignmentOptions.Center);
                Meter(row.progress);
            }
            var title=Bound("tap-row-title");var cost=Bound("tap-row-cost");if(title&&cost)BasicRow(Common(title.transform,cost.transform),title,Bound("tap-row-income"),cost.GetComponentInParent<Button>(true),88);
            var fame=Bound("fame");if(fame)
            {
                var root=R(fame.transform.parent);var button=root.GetComponentsInChildren<Button>(true).FirstOrDefault();var name=root.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t=>t!=fame&&t.transform.parent!=button?.transform);if(button&&name)BasicRow(root,name,fame,button,88);
            }
        }
        static void BasicRow(RectTransform root,TMP_Text title,TMP_Text copy,Button buy,float minimum)
        {
            RowFlow(root,minimum);
            var art=root.Cast<Transform>().Select(t=>t.GetComponent<Image>()).FirstOrDefault(i=>i&&i.transform!=buy.transform&&i.transform.name!="Bazaar surface");
            if(art){Width(art.rectTransform,44,48);art.preserveAspect=true;}
            var column=CopyColumn(root);CopyText(title,column,14,true);if(copy)CopyText(copy,column,12);
            if(art)art.transform.SetSiblingIndex(1);column.SetAsLastSibling();buy.transform.SetParent(root,false);buy.transform.SetAsLastSibling();Button(buy);Width(R(buy.transform),100,56);
        }
        static void UpgradeRows()
        {
            foreach(var row in ui.nextUpgrades.Concat(ui.catalogueRows))
            {
                var root=R(row.root.transform);RowFlow(root,88);
                if(row.artwork){row.artwork.transform.SetParent(root,false);Width(row.artwork.rectTransform,44,48);row.artwork.preserveAspect=true;}
                var column=CopyColumn(root);CopyText(row.title,column,14,true);CopyText(row.copy,column,12);
                if(row.artwork)row.artwork.transform.SetSiblingIndex(1);column.SetAsLastSibling();row.buy.transform.SetParent(root,false);row.buy.transform.SetAsLastSibling();Button(row.buy);Width(R(row.buy.transform),100,56);Text(row.cost,13,true,color:theme.paper,align:TextAlignmentOptions.Center);
            }
        }
        static void Legacy()
        {
            ui.miniFrames=theme.rarity;ui.outerFrames=theme.rarity;ui.innerFrames=new[]{theme.shadeSurface,theme.shadeSurface,theme.shadeSurface};
            var groups=ui.cards.GroupBy(c=>c.button.transform.parent).ToArray();
            foreach(var group in groups)
            {
                var grid=R(group.Key);int tier=Array.IndexOf(groups,group),rows=(group.Count()+2)/3;
                foreach(var old in grid.GetComponents<LayoutGroup>())Object.DestroyImmediate(old);
                var layout=Ensure<GridLayoutGroup>(grid.gameObject);layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=3;layout.spacing=new Vector2(8,8);layout.cellSize=new Vector2((336-16)/3f,132);layout.padding=new RectOffset();Height(grid,rows*132+(rows-1)*8);
                foreach(var card in group)
                {
                    var root=R(card.button.transform);card.button.transition=Selectable.Transition.None;card.frame.sprite=theme.rarity[Mathf.Clamp(tier,0,2)];card.frame.type=Image.Type.Sliced;card.frame.pixelsPerUnitMultiplier=1;card.frame.color=Color.white;Stretch(card.frame.rectTransform);
                    var vertical=Flow(root,4,8,4);vertical.childAlignment=TextAnchor.UpperCenter;vertical.childForceExpandWidth=true;
                    Ignore(card.frame.rectTransform);
                    card.art.transform.SetParent(root,false);Width(card.art.rectTransform,48,48);card.art.preserveAspect=true;
                    card.name.transform.SetParent(root,false);Text(card.name,12,true,align:TextAlignmentOptions.Center);var ne=Ensure<LayoutElement>(card.name.gameObject);ne.minHeight=36;ne.preferredHeight=36;
                    var badge=R(card.level.transform.parent);badge.SetParent(root,false);Height(badge,20);Surface(badge,theme.shadeSurface,false);Text(card.level,12,true,align:TextAlignmentOptions.Center);Stretch(card.level.rectTransform);
                    card.group.alpha=1;
                }
            }
        }
        static void Meter(Image fill)
        {
            if(!fill)return;var parent=fill.rectTransform.parent as RectTransform;
            var track=Ensure<Image>(parent.gameObject);track.sprite=theme.barTrack;track.color=Color.white;track.type=Image.Type.Sliced;track.pixelsPerUnitMultiplier=1;Ensure<Mask>(parent.gameObject).showMaskGraphic=true;
            fill.sprite=theme.barFill;fill.color=Color.white;fill.type=Image.Type.Sliced;fill.pixelsPerUnitMultiplier=1;
            Ensure<ClassicBazaarMeter>(fill.gameObject).fill=fill;
            Ensure<RocketRaja.LoadingBarCorners>(parent.gameObject).Refresh();Ensure<RocketRaja.LoadingBarCorners>(fill.gameObject).Refresh();
        }
        static void Shop()
        {
            var content=ui.panels.Single(v=>v.route=="Shop").scroll.content;
            var balance=R(content.Find("Balance"));Height(balance,84);Surface(balance,theme.section);
            var kicker=balance.Find("Kicker").GetComponent<TMP_Text>();Text(kicker,12,true,color:theme.muted);Top(kicker.rectTransform,12,8,12,20);
            var wallet=balance.Find("KK").GetComponent<TMP_Text>();Text(wallet,24,true);Top(wallet.rectTransform,12,32,12,38);
            foreach(var row in content.Cast<Transform>().ToArray())
            {
                if(row==balance)continue;
                var title=(row.Find("Title")??row.Find("Name"))?.GetComponent<TMP_Text>();
                var copy=(row.Find("Reward")??row.Find("Duration")??row.Find("Contents"))?.GetComponent<TMP_Text>();
                var button=row.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.transform.parent==row);
                if(title&&copy&&button)BasicRow(R(row),title,copy,button,108);
            }
        }
    }
}
