using System;
using System.Collections;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Cloud
{
    /// <summary>Automatic local-only startup. All visuals are authored and saved in the Editor.</summary>
    public sealed partial class BootstrapController : MonoBehaviour
    {
        // Existing serialized references stay compatible while the reviewed loading presentation is authored.
        [SerializeField] TMP_Text heading, description, progress, comparison;
        [SerializeField] Button primaryButton, secondaryButton, tertiaryButton;
        [SerializeField] TMP_Text primaryLabel, secondaryLabel, tertiaryLabel;
        [SerializeField] TMP_Text hint, deviceCash, deviceDetails, deviceTime, cloudCash, cloudDetails, cloudTime;
        [SerializeField] GameObject comparisonCards, loadingGroup, googleMark;
        [SerializeField] RectTransform spinner;
        [SerializeField] GameObject footer;
        [SerializeField] Image loadingFill;
        [SerializeField] GameObject loadingPresentation;
        Action primary, secondary, tertiary;
        bool busy;
        public float LoadingProgress { get; private set; }
        public bool LoadingFailed { get; private set; }
        void Awake()
        {
            redesignHost = RedesignSessionHost.Ensure();
            Application.targetFrameRate = 60; Screen.orientation = ScreenOrientation.Portrait;
            if (!heading || !description || !progress || !primaryButton || !secondaryButton || !tertiaryButton ||
                !primaryLabel || !secondaryLabel || !tertiaryLabel)
            { Debug.LogError("Bootstrap is missing its saved local-startup bindings.", this); enabled = false; return; }
            primaryButton.onClick.AddListener(Primary); secondaryButton.onClick.AddListener(Secondary); tertiaryButton.onClick.AddListener(Tertiary);
            if (googleMark) googleMark.SetActive(false);
            if (comparisonCards) comparisonCards.SetActive(false);
        }
        void Start()
        {
#if UNITY_EDITOR
            if (UnityEditor.SessionState.GetBool("RocketRaja.BootstrapPreview", false)) return;
#endif
            StartRedesign();
        }
        public static void CenterSpinnerPivot(RectTransform rect)
        {
            if (!rect) return;
            var center = new Vector2(.5f, .5f);
            rect.anchoredPosition += Vector2.Scale(center - rect.pivot, rect.rect.size); rect.pivot = center;
        }
        void Show(string title, string detail, string a = null, Action aa = null, string b = null, Action bb = null, string c = null, Action cc = null)
        {
            busy = false;
            if (loadingPresentation) loadingPresentation.SetActive(false);
            if (popupPanel) popupPanel.SetActive(true);
            if (popupBackdrop) popupBackdrop.SetActive(true);
            if (popupOpener) popupOpener.SetActive(false);
            heading.text = title; description.text = detail; progress.text = "";
            if (hint) hint.text = "Progress is saved on this device.";
            if (comparison) comparison.text = "";
            if (comparisonCards) comparisonCards.SetActive(false);
            if (loadingGroup) loadingGroup.SetActive(false);
            if (googleMark) googleMark.SetActive(false);
            if (footer) footer.SetActive(false);
            primary = aa; secondary = bb; tertiary = cc;
            primaryLabel.text = a ?? ""; secondaryLabel.text = b ?? ""; tertiaryLabel.text = c ?? "";
            primaryButton.gameObject.SetActive(a != null); secondaryButton.gameObject.SetActive(b != null); tertiaryButton.gameObject.SetActive(c != null);
            primaryButton.interactable = aa != null; secondaryButton.interactable = bb != null; tertiaryButton.interactable = cc != null;
            primaryLabel.margin = Vector4.zero;
            RefreshPopupClose();
        }
        void SetLoading(float value, string label)
        {
            LoadingProgress = Mathf.Max(LoadingProgress, Mathf.Clamp01(value));
            if (loadingFill)
            {
                if (loadingFill.type == Image.Type.Filled) loadingFill.fillAmount = LoadingProgress;
                else
                {
                    loadingFill.enabled = LoadingProgress > 0;
                    loadingFill.rectTransform.anchorMax = new Vector2(LoadingProgress, 1);
                }
            }
            progress.text = label;
        }
        void Primary() { if (!busy) primary?.Invoke(); }
        void Secondary() { if (!busy) secondary?.Invoke(); }
        void Tertiary() { if (!busy) tertiary?.Invoke(); }
        void OnDestroy()
        {
            if (primaryButton) primaryButton.onClick.RemoveListener(Primary);
            if (secondaryButton) secondaryButton.onClick.RemoveListener(Secondary);
            if (tertiaryButton) tertiaryButton.onClick.RemoveListener(Tertiary);
        }
#if UNITY_EDITOR
        public void PreviewState(string state)
        {
            if (state == "recovery" || state == "error")
                Show("Save needs attention", "Your original files are preserved. Retry before continuing.", "Retry", () => { }, "Keep files and leave", () => { });
            else if (state == "legacy")
                Show("Older progress found", "Keep the older files and start a separate Classic game?", "Start separate game…", () => { }, "Keep files and leave", () => { });
            else
            {
                Show("Getting your stall ready", "", null);
                if (loadingPresentation) { loadingPresentation.SetActive(true); if (popupPanel) popupPanel.SetActive(false); if (popupBackdrop) popupBackdrop.SetActive(false); }
                if (loadingGroup) loadingGroup.SetActive(true);
                LoadingProgress = 0; SetLoading(.58f, "Getting your stall ready…");
            }
        }
#endif
    }
}
