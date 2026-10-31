using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Cloud
{
    public sealed partial class BootstrapController
    {
        [SerializeField] Button popupClose;
        [SerializeField] GameObject popupPanel, popupBackdrop, popupOpener;
        public void DismissPopup() { } // Recovery and legacy choices cannot be dismissed into gameplay.
        public void ReopenPopup()
        {
            if (busy) return;
            if (popupPanel) popupPanel.SetActive(true);
            if (popupBackdrop) popupBackdrop.SetActive(true);
            if (popupOpener) popupOpener.SetActive(false);
        }
        void RefreshPopupClose() { if (popupClose) popupClose.gameObject.SetActive(false); }
    }
}
