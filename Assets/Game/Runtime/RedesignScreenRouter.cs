using System;
using System.Collections.Generic;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Navigation and session ports for the saved redesign screens.</summary>
    public sealed partial class RedesignScreenRouter : MonoBehaviour
    {
        public RedesignRuntimeDriver driver;
        public RedesignUpgradesScreen upgrades;
        public RedesignUpgradeDetailsScreen details;
        public RedesignBatchScreen batch;
        public RedesignLevelsScreen productDetails, tapTraining;
        public RedesignTalentScreen talent;
        public RedesignCardDetailsScreen cardDetails;
        public RedesignRevealScreen reveal;
        public RedesignPrestigeScreen prestige;
        public RedesignShopScreen shop;
        public RedesignSkillScreen skillDetails;
        public RedesignOfflineScreen offline;
        public RedesignHomeScreen home;
        public RedesignGuidanceView guidance;
        public TMP_Text title, summary, toast;
        public GameObject toastRoot;
        public Button toastDismiss;
        public Button homeTab, upgradesTab, talentTab, prestigeTab;
        public bool initializeGuestOnStart;
        public Action<string, string> ExternalNavigate;
        public Func<bool> BackRequested;
        bool overlayOpen;
        RedesignLifecycle session;
        RedesignUiContext context;
        readonly Stack<KeyValuePair<string, string>> history = new Stack<KeyValuePair<string, string>>();
        string route = "upgrades", parameter = "";
        float refreshTime, toastUntil;
        public string CurrentRoute => route;
        public RedesignLifecycle Session => session;
        void Awake()
        {
            if (!driver || !upgrades || !details || !batch || !productDetails || !tapTraining || !talent || !cardDetails || !reveal || !prestige || !shop || !skillDetails || !offline || !home || !title || !summary || !toast || !toastRoot || !homeTab || !upgradesTab || !talentTab || !prestigeTab)
                throw new InvalidOperationException("Incomplete serialized redesign router");
            upgradesTab.onClick.AddListener(OpenUpgrades);
            talentTab.onClick.AddListener(OpenTalent); prestigeTab.onClick.AddListener(OpenPrestige);
            homeTab.onClick.AddListener(OpenHome);
            if (toastDismiss) toastDismiss.onClick.AddListener(DismissNotice);
        }
        void Start()
        {
            if (!initializeGuestOnStart) return;
            if (driver.InitializeGuest()) Bind(driver.Session);
            else Notice("Your save needs recovery before you can continue.");
        }
        void OnDestroy() { if (toastDismiss) toastDismiss.onClick.RemoveListener(DismissNotice); if (homeTab) homeTab.onClick.RemoveListener(OpenHome); if (upgradesTab) upgradesTab.onClick.RemoveListener(OpenUpgrades); if (talentTab) talentTab.onClick.RemoveListener(OpenTalent); if (prestigeTab) prestigeTab.onClick.RemoveListener(OpenPrestige); }
        public void Bind(RedesignLifecycle value, bool showHome = false)
        {
            session = value ?? throw new ArgumentNullException(nameof(value));
            bool Current() => session == value && value.CanMutate && !overlayOpen;
            context = new RedesignUiContext(value.Engine, Current, command => Current() && value.Run(command), Navigate, Notice);
            upgrades.Bind(value.Engine, Current, id => Current() && value.Run(e => e.BuyUpgrade(id)), id => Navigate("upgradedetails", id), OpenBatch, Notice);
            details.Bind(context); batch.Bind(context); productDetails.Bind(context); tapTraining.Bind(context);
            talent.Bind(context); cardDetails.Bind(context); reveal.Bind(context); prestige.Bind(context);
            shop.Bind(context, driver.IsDisposableGuest, driver.RetrySaving);
            skillDetails.Bind(context); offline.Bind(context);
            home.Bind(context, driver.TapGameplaySurface);
            if (guidance) guidance.Bind(context);
            history.Clear(); Show(session.Engine.State.reveal.pending ? "drawreveal" : showHome ? "home" : "upgrades", "");
        }
        void OpenHome() => Navigate("home", "");
        void OpenUpgrades() => Navigate("upgrades", "");
        void OpenTalent() => Navigate("talent", "");
        void OpenPrestige() => Navigate("prestige", "");
        public void Navigate(string destination, string argument)
        {
            if (session == null) return;
            if (destination == "back" && BackRequested?.Invoke() == true) return;
            if (overlayOpen) return;
            if (destination == "back" && CloseGuidanceReader()) return;
            if (destination == "back") { if (history.Count > 0) { var previous = history.Pop(); Show(previous.Key, previous.Value); } else Show("home", ""); return; }
            if (destination == "productupgrades" || destination == "tappingupgrades" || destination == "skillupgrades")
            {
                Show("upgrades", ""); history.Clear();
                if (destination == "productupgrades") upgrades.FilterProduct(int.Parse(argument)); else if (destination == "skillupgrades") upgrades.FilterSkill(int.Parse(argument)); else upgrades.SetCategory(UpgradeCategory.Tapping);
                return;
            }
            if (destination != "home" && destination != "upgrades" && destination != "upgradedetails" && destination != "productdetails" && destination != "taptraining" && destination != "talent" && destination != "carddetails" && destination != "drawreveal" && destination != "prestige" && destination != "kajushop" && destination != "skilldetails" && destination != "offline")
            {
                if (ExternalNavigate != null) ExternalNavigate(destination, argument);
                else Notice("This destination is unavailable in the current preview.");
                return;
            }
            if (destination == "home" || destination == "upgrades" || destination == "talent" || destination == "prestige") history.Clear();
            else if (route != destination || parameter != argument) history.Push(new KeyValuePair<string, string>(route, parameter));
            Show(destination, argument);
        }
        public void OpenBatch(CashBatchQuote quote)
        {
            if (overlayOpen) return;
            history.Push(new KeyValuePair<string, string>(route, parameter));
            route = "buyall"; parameter = ""; SetVisible(); batch.Open(quote); if (guidance) guidance.Refresh(route, parameter, true); UpdateHeader();
        }
        void Show(string destination, string argument)
        {
            if (destination == "talent" && session.Engine.State.reveal.pending) destination = "drawreveal";
            route = destination; parameter = argument; SetVisible();
            if (route == "upgradedetails") details.Open(argument);
            else if (route == "productdetails") productDetails.Open(int.Parse(argument));
            else if (route == "taptraining") tapTraining.Open(-1);
            else if (route == "upgrades") upgrades.Refresh();
            else if (route == "talent") talent.Open();
            else if (route == "carddetails") cardDetails.Open(argument);
            else if (route == "drawreveal") reveal.Open();
            else if (route == "prestige") prestige.Open();
            else if (route == "kajushop") shop.Open();
            else if (route == "skilldetails") skillDetails.Open(int.Parse(argument));
            else if (route == "offline") offline.Open();
            else if (route == "home") home.Open();
            if (guidance) guidance.Refresh(route, parameter, true);
            UpdateHeader();
        }
        void SetVisible()
        {
            upgrades.gameObject.SetActive(route == "upgrades"); details.gameObject.SetActive(route == "upgradedetails"); batch.gameObject.SetActive(route == "buyall");
            productDetails.gameObject.SetActive(route == "productdetails"); tapTraining.gameObject.SetActive(route == "taptraining");
            talent.gameObject.SetActive(route == "talent"); cardDetails.gameObject.SetActive(route == "carddetails"); reveal.gameObject.SetActive(route == "drawreveal"); prestige.gameObject.SetActive(route == "prestige");
            shop.gameObject.SetActive(route == "kajushop");
            skillDetails.gameObject.SetActive(route == "skilldetails"); offline.gameObject.SetActive(route == "offline");
            home.gameObject.SetActive(route == "home");
            session.GameplayInputEnabled = route == "home" && !overlayOpen;
        }
        public void SetOverlay(bool open) { overlayOpen = open; if (session != null) session.GameplayInputEnabled = route == "home" && !open; if (guidance) guidance.Refresh(route, parameter); }
        public bool CloseGuidanceReader() { if (!guidance || !guidance.ReaderOpen) return false; guidance.CloseReader(); return true; }
        void Update()
        {
            if (session == null) return;
            refreshTime += Time.unscaledDeltaTime;
            if (refreshTime >= .2f) { refreshTime = 0; UpdateHeader(); }
            if (toastRoot.activeSelf && Time.unscaledTime >= toastUntil) toastRoot.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Escape)) Navigate("back", "");
        }
        void UpdateHeader()
        {
            if (session == null) return;
            if (guidance) guidance.Refresh(route, parameter);
            title.text = route == "upgradedetails" ? "Upgrade details" : route == "buyall" ? "Review Buy All" : route == "productdetails" ? "Product details" : route == "taptraining" ? "Tap training" : route == "talent" ? "Talent" : route == "carddetails" ? "Card details" : route == "drawreveal" ? "Your Talent card" : route == "prestige" ? "Prestige" : "Upgrades";
            summary.text = "₹" + session.Engine.State.cash + " cash · Normal sales ₹" + session.Engine.NormalIncome() + "/s";
            if (route == "kajushop") title.text = "Kaju Katli shop";
            else if (route == "skilldetails") title.text = "Skill details";
            else if (route == "offline") title.text = "Welcome back";
            else if (route == "home") title.text = "Rocket Raja";
        }
        public void Notice(string text) { if (guidance) guidance.CloseReader(); toast.text = text; toastRoot.SetActive(true); toastUntil = Time.unscaledTime + Mathf.Clamp(4 + text.Length * .04f, 5, 12); }
        public void DismissNotice() => toastRoot.SetActive(false);
    }
}
