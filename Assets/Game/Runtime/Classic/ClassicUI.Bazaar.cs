using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    public sealed partial class ClassicUI
    {
        public ClassicBazaarTheme bazaarTheme;
        public ClassicBazaarButton[] bazaarButtons = System.Array.Empty<ClassicBazaarButton>();
        public RectTransform bazaarHomeContent, bazaarHomeViewport;

        // State styling only. All graphics, labels and containers are authored before Play Mode.
        void RefreshBazaarPresentation()
        {
            var theme=bazaarTheme;if(!theme)return;
            foreach(var binding in bazaarButtons)
            {
                var button=binding.button;
                if(!button||!button.gameObject.activeInHierarchy||binding.control&&binding.control.action=="Setting")continue;
                var image=button.targetGraphic as Image;
                if(!image||!image.sprite||!image.sprite.name.StartsWith("Folk"))continue;
                if(!button.interactable)image.sprite=theme.unavailable;
                var color=!button.interactable?theme.disabledInk:image.sprite==theme.secondary?theme.ink:theme.paper;
                foreach(var text in binding.labels)if(text&&text.name!="Nameplate")text.color=color;
            }
            for(int i=0;i<navigation.Length;i++)
            {
                var button=navigation[i];bool selected=panels[i+1].route==CurrentTab;
                ((Image)button.targetGraphic).sprite=selected?theme.primary:theme.secondary;
                foreach(var binding in bazaarButtons)if(binding.button==button)
                    foreach(var label in binding.labels)if(label)label.color=selected?theme.paper:theme.ink;
            }
            foreach(var clock in skillClocks)
            {
                if(clock.disc)clock.disc.color=Color.white;
                if(clock.ring)clock.ring.color=theme.gold;
                if(clock.track)clock.track.color=theme.line;
                if(clock.statusBadge)clock.statusBadge.color=Color.white;
                if(clock.status)clock.status.color=theme.ink;
            }
            foreach(var card in cards)
            {
                if(!card.button)continue;
                card.group.alpha=1;
                card.name.color=theme.ink;card.level.color=theme.ink;
                card.art.color=card.button.interactable?Color.white:new Color(1,1,1,.55f);
            }
            if(CurrentDialog=="CardDetail")
            {var level=texts["full-level"];level.color=theme.ink;level.transform.parent.GetComponent<Image>().color=Color.white;}
            foreach(var row in achievementRows)
            {if(row.action)row.action.fontSize=12;if(row.fill)row.fill.color=Color.white;}
            for(int i=0;i<homeSkills.Length;i++)
                foreach(var label in homeSkills[i].GetComponentsInChildren<TMP_Text>(true))label.color=label==texts["home-skill-"+i]?theme.muted:theme.ink;
        }
    }
}
