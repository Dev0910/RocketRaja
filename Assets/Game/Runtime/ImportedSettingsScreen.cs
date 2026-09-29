using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RocketRaja
{
    /// <summary>Bindings for the saved Settings canvas. All visual objects are authored in the Editor.</summary>
    public sealed class ImportedSettingsScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button backButton;
        [SerializeField] Button privacyButton;
        [SerializeField] Button consentButton;
        [SerializeField] Button[] settingButtons;
        [SerializeField] Image[] settingFaces;
        [SerializeField] TMP_Text[] settingStates;
        [SerializeField] Image[] settingDots;
        [SerializeField] GameObject consentRow;
        [SerializeField] Sprite enabledFace;
        [SerializeField] Sprite disabledFace;
        [SerializeField] Button accountButton;
        [SerializeField] TMP_Text accountStatus;

        GameController controller;
        GameView view;

        void Awake()
        {
            if (closeButton == null || backButton == null || privacyButton == null || consentButton == null ||
                consentRow == null || enabledFace == null || disabledFace == null ||
                settingButtons == null || settingButtons.Length != 3 ||
                settingFaces == null || settingFaces.Length != 3 ||
                settingStates == null || settingStates.Length != 3 ||
                settingDots == null || settingDots.Length != 3)
            {
                Debug.LogError("Settings canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < 3; i++)
                if (settingButtons[i] == null || settingFaces[i] == null || settingStates[i] == null || settingDots[i] == null)
                {
                    Debug.LogError("Settings toggle " + i + " has a missing editor-authored reference.", this);
                    enabled = false;
                    return;
                }

            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
            privacyButton.onClick.AddListener(OpenPrivacy);
            consentButton.onClick.AddListener(OpenConsent);
            if (accountButton != null) accountButton.onClick.AddListener(OpenAccount);
            settingButtons[0].onClick.AddListener(ToggleMusic);
            settingButtons[1].onClick.AddListener(ToggleSound);
            settingButtons[2].onClick.AddListener(ToggleMotion);
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
            if (privacyButton != null) privacyButton.onClick.RemoveListener(OpenPrivacy);
            if (consentButton != null) consentButton.onClick.RemoveListener(OpenConsent);
            if (accountButton != null) accountButton.onClick.RemoveListener(OpenAccount);
            if (settingButtons == null || settingButtons.Length != 3) return;
            if (settingButtons[0] != null) settingButtons[0].onClick.RemoveListener(ToggleMusic);
            if (settingButtons[1] != null) settingButtons[1].onClick.RemoveListener(ToggleSound);
            if (settingButtons[2] != null) settingButtons[2].onClick.RemoveListener(ToggleMotion);
        }

        public void Bind(GameController owner, GameView ownerView)
        {
            controller = owner;
            view = ownerView;
            Refresh();
        }

        public void Open()
        {
            if (controller == null || view == null)
            {
                Debug.LogError("Settings canvas was opened before binding its game owner.", this);
                return;
            }
            UiMotion.Show(gameObject, controller, true);
            Refresh();
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller);
        }

        public void CloseForChild() => UiMotion.Hide(gameObject, controller);

        public void Refresh()
        {
            if (controller == null || controller.Engine == null) return;
            var state = controller.Engine.State;
            var values = new[] { state.music, state.sound, state.reducedMotion };
            for (int i = 0; i < 3; i++)
            {
                if (settingStates[i] != null)
                {
                    settingStates[i].text = values[i] ? "ON" : "OFF";
                    settingStates[i].color = values[i] ? Color.white : new Color32(22, 75, 128, 255);
                    settingStates[i].rectTransform.anchoredPosition = values[i] ? new Vector2(-35, 0) : new Vector2(35, 0);
                }
                if (settingFaces[i] != null) settingFaces[i].sprite = values[i] ? enabledFace : disabledFace;
                if (settingDots[i] != null) settingDots[i].rectTransform.anchoredPosition = values[i] ? new Vector2(54, 0) : new Vector2(-54, 0);
            }
            if (consentRow != null)
                consentRow.SetActive(controller.Ads != null && controller.Ads.PrivacyOptionsRequired);
            if (accountStatus != null)
            {
                var host = Cloud.PlayerSessionHost.Instance;
                accountStatus.text = host?.Session?.IsAccount == true ? (host.Profile?.displayName ?? "Google account") + "\n" + host.Session.Status : "Guest · saved on this device";
            }
        }

        void ToggleMusic() => Toggle("music");
        void ToggleSound() => Toggle("sound");
        void ToggleMotion() => Toggle("motion");

        void Toggle(string setting)
        {
            if (controller == null) return;
            var positions = new Vector2[settingDots.Length];
            for (int i = 0; i < positions.Length; i++) positions[i] = settingDots[i].rectTransform.anchoredPosition;
            controller.SetSetting(setting);
            Refresh();
            StopAllCoroutines();
            if (!controller.Engine.State.reducedMotion) StartCoroutine(SlideToggles(positions));
        }

        System.Collections.IEnumerator SlideToggles(Vector2[] from)
        {
            var to = new Vector2[settingDots.Length];
            for (int i = 0; i < to.Length; i++) to[i] = settingDots[i].rectTransform.anchoredPosition;
            for (float time = 0; time < .18f; time += Time.unscaledDeltaTime)
            {
                if (controller.Engine.State.reducedMotion) break;
                float t = 1 - Mathf.Pow(1 - Mathf.Clamp01(time / .18f), 3);
                for (int i = 0; i < to.Length; i++) settingDots[i].rectTransform.anchoredPosition = Vector2.Lerp(from[i], to[i], t);
                yield return null;
            }
            for (int i = 0; i < to.Length; i++) settingDots[i].rectTransform.anchoredPosition = to[i];
        }

        void OpenPrivacy() => view?.ShowPrivacy();
        void OpenAccount() { controller?.Save(); Cloud.PlayerSessionHost.Ensure().OpenAccountMenu(); }

        void OpenConsent()
        {
            if (controller != null && controller.Ads != null && controller.Ads.PrivacyOptionsRequired)
                controller.Ads.PrivacyOptions();
        }
    }
}
