using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Operates an Editor-placed button. No UI construction or runtime listeners duplicated on reopen.</summary>
    public sealed class ClassicControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ClassicUI owner;
        public string action, parameter;
        public int htmlStyle; // 0 original; 1 filled popup action; 2 secondary; 3 text-only
        public RectTransform motionTarget;
        public Shadow motionShadow;
        public int motionKind; // 0 button, 1 collection card, 2 shelf merchandise
        bool hovered,pressed;
        Button button;
        void Awake() { button=GetComponent<Button>(); button.onClick.AddListener(Invoke); }
        void OnDestroy() { if(button)button.onClick.RemoveListener(Invoke); }
        public void Invoke() { if(button&&button.interactable&&owner)owner.Handle(action,parameter); }
        public void OnPointerDown(PointerEventData e) { if(button.IsInteractable()){pressed=true;owner.motion.Input(this,hovered,true);} }
        public void OnPointerUp(PointerEventData e) { pressed=false;owner.motion.Input(this,hovered,false); }
        public void OnPointerEnter(PointerEventData e) { if(button.IsInteractable()&&e.pointerId<0){hovered=true;owner.motion.Input(this,true,pressed);} }
        public void OnPointerExit(PointerEventData e) { hovered=pressed=false;owner.motion.Input(this,false,false); }
        void OnDisable() { hovered=pressed=false;if(owner&&owner.motion)owner.motion.ResetInput(this); }
    }
}
