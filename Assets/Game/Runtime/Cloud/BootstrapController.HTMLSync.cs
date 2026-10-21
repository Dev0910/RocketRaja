using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Cloud
{
    public sealed partial class BootstrapController
    {
        [SerializeField] Button popupClose;
        [SerializeField] GameObject popupPanel, popupBackdrop, popupOpener;
        bool popupReturnToShop;
        public void DismissPopup()
        {
            // Dismissal never selects, replaces or promotes a save.
            if(busy||redesignHost==null)return;
            if(!popupReturnToShop)
            {
                if(heading.text!="Welcome to your stall"){RedesignWelcome();return;}
                if(popupPanel&&popupBackdrop&&popupOpener){popupPanel.SetActive(false);popupBackdrop.SetActive(false);popupOpener.SetActive(true);if(UnityEngine.EventSystems.EventSystem.current)UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(popupOpener);}
                return;
            }
            if(heading.text=="Your Google account"||heading.text=="Welcome to your stall")
            {
                if(redesignHost.Account!=null)Run(RedesignReconnect);
                else if(redesignHost.Mode=="guest")Run(()=>RedesignGuest(false));
            }
            else if(redesignHost.Account!=null)RedesignAccountMenu();else RedesignWelcome();
        }
        public void ReopenPopup(){if(busy)return;if(popupPanel)popupPanel.SetActive(true);if(popupBackdrop)popupBackdrop.SetActive(true);if(popupOpener)popupOpener.SetActive(false);RefreshPopupClose();}
        void RefreshPopupClose(){if(popupClose)popupClose.gameObject.SetActive(!busy);}
    }
}
