using UnityEngine;
using UnityEngine.EventSystems;

namespace RocketRaja.Cloud
{
    /// <summary>Editor-placed backdrop/close target; never forwards input to the stall.</summary>
    public sealed class BootstrapPopupDismiss : MonoBehaviour, IPointerClickHandler
    {
        public BootstrapController owner;
        public void OnPointerClick(PointerEventData data){if(data.button!=PointerEventData.InputButton.Left)return;data.Use();if(owner)owner.DismissPopup();}
    }
}
