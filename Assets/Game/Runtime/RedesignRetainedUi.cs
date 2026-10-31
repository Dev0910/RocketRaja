using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Adapts saved copies of the existing utility dialogs to the isolated economy.</summary>
    public sealed class RedesignRetainedUi : MonoBehaviour
    {
        public GameObject settings, privacy, naming, recovery;
        public Button settingsEntry, settingsClose, settingsBack, privacyEntry, privacyClose, privacyBack, account, rename;
        public Button[] toggles;
        public Image[] faces, dots;
        public TMP_Text[] toggleStates;
        public Sprite enabledFace, disabledFace;
        public TMP_Text accountStatus, storeName;
        public TMP_Text accountHeading, accountDescription, privacySummary, privacyDescription, recoveryDescription;
        public TMP_InputField nameInput;
        public TMP_Text nameTitle, nameDescription, nameMessage, nameCancelLabel, nameSaveLabel;
        public Button nameSave, nameCancel, recoveryRetry, recoveryExit;
        public TMP_Text recoveryMessage;
        public GameObject recoveryError;
        RedesignApplication application;
        public string Page { get; private set; }
        public bool IsOpen => !string.IsNullOrEmpty(Page);
        public static string DisplayName(RedesignState state) => GameState.TryStoreName(state.storeName, out string name) ? name :
            string.IsNullOrEmpty(state.storeName) ? GameState.DefaultStoreName : GameState.StoreNameUpdateRequired;
        void Awake()
        {
            if (!settings || !privacy || !naming || !recovery || !settingsEntry || !nameInput || !recoveryRetry || !accountHeading || !accountDescription || !privacySummary || !privacyDescription || !recoveryDescription || toggles.Length != 3 || faces.Length != 3 || dots.Length != 3 || toggleStates.Length != 3)
                throw new InvalidOperationException("Incomplete saved utility dialogs");
            settingsEntry.onClick.AddListener(OpenSettings); settingsClose.onClick.AddListener(Close); settingsBack.onClick.AddListener(Close);
            privacyEntry.onClick.AddListener(OpenPrivacy); privacyClose.onClick.AddListener(OpenSettings); privacyBack.onClick.AddListener(OpenSettings);
            account.onClick.AddListener(Account); rename.onClick.AddListener(OpenName);
            toggles[0].onClick.AddListener(ToggleMusic); toggles[1].onClick.AddListener(ToggleSound); toggles[2].onClick.AddListener(ToggleMotion);
            nameSave.onClick.AddListener(SaveName); nameCancel.onClick.AddListener(OpenSettings); nameInput.onValueChanged.AddListener(ValidateName);
            recoveryRetry.onClick.AddListener(Retry); recoveryExit.onClick.AddListener(Exit);
        }
        void OnDestroy()
        {
            if (!settingsEntry) return;
            settingsEntry.onClick.RemoveListener(OpenSettings); settingsClose.onClick.RemoveListener(Close); settingsBack.onClick.RemoveListener(Close);
            privacyEntry.onClick.RemoveListener(OpenPrivacy); privacyClose.onClick.RemoveListener(OpenSettings); privacyBack.onClick.RemoveListener(OpenSettings);
            account.onClick.RemoveListener(Account); rename.onClick.RemoveListener(OpenName);
            toggles[0].onClick.RemoveListener(ToggleMusic); toggles[1].onClick.RemoveListener(ToggleSound); toggles[2].onClick.RemoveListener(ToggleMotion);
            nameSave.onClick.RemoveListener(SaveName); nameCancel.onClick.RemoveListener(OpenSettings); nameInput.onValueChanged.RemoveListener(ValidateName);
            recoveryRetry.onClick.RemoveListener(Retry); recoveryExit.onClick.RemoveListener(Exit);
        }
        public void Bind(RedesignApplication owner)
        {
            application = owner;
            account.gameObject.SetActive(false);
            accountHeading.text = "Device save";
            accountDescription.text = "Progress is saved on this device.";
            privacySummary.text = "Your progress stays on this device.";
            privacyDescription.text = "Your game uses a local device save. Cloud saves and cross-device sync are unavailable. Store purchases are currently unavailable.";
            Close();
        }
        void Show(string page)
        {
            if (application == null) return;
            if (Page == "naming" && page != Page) nameInput.DeactivateInputField();
            Page = page; settings.SetActive(page == "settings"); privacy.SetActive(page == "privacy"); naming.SetActive(page == "naming"); recovery.SetActive(page == "recovery");
            application.router.SetOverlay(IsOpen);
        }
        public void Close() { if (Page == "recovery" && !application.Ready) return; Show(null); }
        public void OpenSettings()
        {
            if (application == null || !application.Ready) return;
            Show("settings"); Refresh();
        }
        void OpenPrivacy() { if (Page == "settings") Show("privacy"); }
        void Account() { }
        void ToggleMusic() => Toggle("music");
        void ToggleSound() => Toggle("sound");
        void ToggleMotion() => Toggle("motion");
        void Toggle(string key)
        {
            if (Page != "settings") return;
            application.router.driver.Command(e => e.ToggleSetting(key)); Refresh();
        }
        public void Refresh()
        {
            var session = application?.router.Session; if (session == null) return;
            var s = session.Engine.State; bool[] values = { s.music, s.sound, s.reducedMotion };
            for (int i = 0; i < 3; i++)
            {
                toggleStates[i].text = values[i] ? "ON" : "OFF"; toggleStates[i].color = values[i] ? Color.white : new Color32(22, 75, 128, 255);
                toggleStates[i].rectTransform.anchoredPosition = new Vector2(values[i] ? -35 : 35, 0);
                faces[i].sprite = values[i] ? enabledFace : disabledFace; dots[i].rectTransform.anchoredPosition = new Vector2(values[i] ? 54 : -54, 0);
                toggles[i].interactable = session.CanMutate;
            }
            storeName.richText = false; storeName.text = DisplayName(s); rename.interactable = session.CanMutate;
            accountStatus.text = application.router.driver.IsAccount ? application.router.driver.RecoveryNotice : "Saved on this device";
        }
        void OpenName()
        {
            if (application == null || !application.Ready) return;
            var s = application.router.Session.Engine.State; bool supported = GameState.TryStoreName(s.storeName, out string name);
            nameTitle.text = "Name your store"; nameDescription.text = !supported && !string.IsNullOrEmpty(s.storeName) ? "Old name stays until you save." : "A name for your fireworks shop";
            nameCancelLabel.text = "Cancel"; nameSaveLabel.text = "Save name"; nameInput.SetTextWithoutNotify(supported ? name : "");
            Show("naming"); ValidateName(nameInput.text);
        }
        void ValidateName(string value)
        {
            bool valid = GameState.TryStoreName(value, out _);
            nameSave.interactable = application != null && application.Ready && valid;
            nameMessage.text = valid ? "1–24 Latin letters, numbers or punctuation." : "Use supported Latin letters or numbers (punctuation allowed).";
        }
        void SaveName()
        {
            if (Page != "naming") return;
            if (application.router.driver.Command(e => e.SetStoreName(nameInput.text))) { application.router.home.Refresh(); OpenSettings(); }
        }
        public void ShowRecovery(string message)
        {
            Show("recovery"); recoveryError.SetActive(true); recoveryMessage.text = message;
            recoveryDescription.text = "Free device storage if needed, then retry. Your existing files are preserved.";
            recoveryExit.GetComponentInChildren<TMP_Text>(true).text = "Keep files and exit";
        }
        void Retry() { if (Page == "recovery") application.Retry(); }
        void Exit()
        {
            Application.Quit();
        }
        public bool Back()
        {
            if (!IsOpen) return false;
            if (Page == "recovery") return true;
            if (Page == "naming" && nameInput.isFocused) { nameInput.DeactivateInputField(); return true; }
            if (Page == "privacy" || Page == "naming") OpenSettings(); else Close();
            return true;
        }
    }
}
