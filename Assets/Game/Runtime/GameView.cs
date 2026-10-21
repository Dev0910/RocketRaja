using System;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>Routes navigation and dialogs through the saved screen presenters.</summary>
    public sealed class GameView : MonoBehaviour
    {
        public static Color Navy = Hex("141E36");
        GameController game;
        GameEngine E => game.Engine;
        float refreshClock;
        string currentTab = "Home";
        RedesignScreenRouter redesign;
        public string CurrentTab => redesign ? redesign.CurrentRoute : currentTab;
        public void InitializeRedesign(RedesignScreenRouter router) { redesign = router; game = null; enabled = false; }

        public bool Initialize(GameController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (controller.HomeScreen == null || controller.PlayerScreen == null ||
                controller.ProductsScreen == null || controller.LegacyScreen == null ||
                controller.PrivacyScreen == null || controller.SettingsScreen == null ||
                controller.SaveRecoveryScreen == null || controller.OfflineScreen == null ||
                controller.PrestigeScreen == null ||
                controller.StatisticsScreen == null || controller.SkillsScreen == null ||
                controller.ProductDetailScreen == null || controller.StoreNameScreen == null)
            {
                Debug.LogError("Main GameController has missing editor-authored screen references.", this);
                enabled = false;
                return false;
            }

            game = controller;
#if UNITY_ANDROID && !UNITY_EDITOR
            Input.backButtonLeavesApp = false;
#endif
            game.HomeScreen.gameObject.SetActive(true);
            game.HomeScreen.Bind(game, this);
            game.PlayerScreen.Bind(game, this);
            game.ProductsScreen.Bind(game, this);
            game.LegacyScreen.Bind(game, this);
            Switch("Home");
            return true;
        }

        public void Switch(string tab)
        {
            if (redesign) { redesign.Navigate(tab.ToLowerInvariant(), ""); return; }
            game.HomeScreen.gameObject.SetActive(true);
            int direction = currentTab == "Home" || tab == "Home" ? 0 : Math.Sign(Array.IndexOf(new[] { "Player", "Products", "Legacy" }, tab) - Array.IndexOf(new[] { "Player", "Products", "Legacy" }, currentTab));
            currentTab = tab;
            // HTML settles an older exit before a new navigation occupies the same region.
            UiMotion.SettleExit(game.PlayerScreen.gameObject);
            UiMotion.SettleExit(game.ProductsScreen.gameObject);
            UiMotion.SettleExit(game.LegacyScreen.gameObject);
            game.HomeScreen.SettlePanelExit();
            bool browsing = tab != "Home";
            game.HomeScreen.GetComponent<ImportedPanelFit>().SetBrowsing(browsing);
            game.PlayerScreen.GetComponent<ImportedPanelFit>().SetBrowsing(browsing);
            game.ProductsScreen.GetComponent<ImportedPanelFit>().SetBrowsing(browsing);
            game.LegacyScreen.GetComponent<ImportedPanelFit>().SetBrowsing(browsing);
            game.HomeScreen.SetHomeVisible(tab == "Home");
            UiMotion.SetVisible(game.PlayerScreen.gameObject, tab == "Player", game, direction);
            UiMotion.SetVisible(game.ProductsScreen.gameObject, tab == "Products", game, direction);
            UiMotion.SetVisible(game.LegacyScreen.gameObject, tab == "Legacy", game, direction);
            game.HomeScreen.SelectNavigation(tab);
            if (game.CanPlay && tab == "Products" && E.State.onboarding < 2)
            {
                E.State.onboarding = 2;
                game.Save();
            }
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (game == null || E == null) return;
            game.HomeScreen.Refresh();
            if (game.PlayerScreen.gameObject.activeSelf) game.PlayerScreen.Refresh();
            if (game.ProductsScreen.gameObject.activeSelf) game.ProductsScreen.Refresh();
            if (game.LegacyScreen.gameObject.activeSelf) game.LegacyScreen.Refresh();
        }

        void Update()
        {
            if (game == null) return;
            refreshClock += Time.unscaledDeltaTime;
            if (refreshClock > .2f) { refreshClock = 0; RefreshNow(); }
            if (game.StartupReady && currentTab == "Home" && game.CanPlay && E.CanMutate &&
                (E.State.onboarding >= 3 || E.State.OwnedProducts > 0) &&
                !E.State.storeNamePromptHandled && string.IsNullOrEmpty(E.State.storeName) &&
                !game.ModalPaused && !game.IsAdShowing &&
                string.IsNullOrEmpty(E.State.pendingOfflineId) && !game.HomeScreen.HasNotice)
                ShowStoreName(true);
            if (Input.GetKeyDown(KeyCode.Escape) && !TryNavigateBack()) BackgroundTask();
        }

        // Return false only at the root, where Android should background the task.
        internal bool TryNavigateBack()
        {
            if (game == null) return true;
            if (game.SaveNeedsRecovery) Application.Quit();
            else if (game.StoreNameScreen.gameObject.activeInHierarchy) game.StoreNameScreen.Dismiss();
            else if (game.PrivacyScreen.gameObject.activeInHierarchy) game.PrivacyScreen.Close();
            else if (game.SettingsScreen.gameObject.activeInHierarchy) game.SettingsScreen.Close();
            else if (game.OfflineScreen.gameObject.activeInHierarchy) game.OfflineScreen.Dismiss();
            else if (game.PrestigeScreen.gameObject.activeInHierarchy) game.PrestigeScreen.Close();
            else if (game.StatisticsScreen.gameObject.activeInHierarchy) game.StatisticsScreen.Close();
            else if (game.SkillsScreen.gameObject.activeInHierarchy) game.SkillsScreen.Close();
            else if (game.ProductDetailScreen.gameObject.activeInHierarchy) game.ProductDetailScreen.Close();
            else if (currentTab == "Products" && game.ProductsScreen.DismissQuantityMenu()) return true;
            else if (currentTab == "Player" && game.PlayerScreen.DismissQuantityMenu()) return true;
            else if (currentTab != "Home") Switch("Home");
            else return false;
            return true;
        }

        internal static void BackgroundTask()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (activity == null || !activity.Call<bool>("moveTaskToBack", true))
                        Debug.LogWarning("Android could not move the game task to the background.");
                }
            }
            catch (AndroidJavaException exception)
            {
                Debug.LogWarning("Android could not move the game task to the background: " + exception.Message);
            }
#endif
        }

        public void TapFeedback() => game.HomeScreen.TapFeedback();
        public void Notice(string message) => game?.HomeScreen.Notice(message);

        void Celebrate()
        {
            GetComponent<ShopAudio>()?.Celebrate();
            game.HomeScreen.Celebrate();
        }

        public void ShowPrestige()
        {
            var prestige = game.PrestigeScreen;
            if (prestige == null)
            {
                Debug.LogError("The editor-authored Prestige screen is missing from Main.", this);
                return;
            }
            prestige.Bind(game, () =>
            {
                Switch("Legacy");
                GetComponent<ShopAudio>()?.Celebrate();
                game.HomeScreen.CelebratePrestige();
                game.LegacyScreen.CelebratePrestige();
                Notice("A brighter beginning! Spend your reputation on a permanent perk.");
            });
            prestige.Open();
        }
        public void ShowOffline()
        {
            if (string.IsNullOrEmpty(E.State.pendingOfflineId)) return;
            var offline = game.OfflineScreen;
            if (offline == null)
            {
                Debug.LogError("The editor-authored Offline screen is missing from Main.", this);
                return;
            }
            offline.Bind(game);
            offline.Open();
        }
        public void ShowSponsor() { } // Retired route retained for old callers.
        public void ShowStatistics()
        {
            var statistics = game.StatisticsScreen;
            if (statistics == null)
            {
                Debug.LogError("The editor-authored Statistics screen is missing from Main.", this);
                return;
            }
            statistics.Bind(game);
            statistics.Open();
        }
        public void ShowSkills()
        {
            var skills = game.SkillsScreen;
            if (skills == null)
            {
                Debug.LogError("The editor-authored Skills screen is missing from Main.", this);
                return;
            }
            skills.Bind(game);
            skills.Open();
        }
        public void ShowProductDetail(int productIndex)
        {
            var detail = game.ProductDetailScreen;
            if (detail == null)
            {
                Debug.LogError("The editor-authored Product Detail screen is missing from Main.", this);
                return;
            }
            detail.Bind(game);
            detail.Open(productIndex);
        }
        public void ShowSettings()
        {
            var settings = game.SettingsScreen;
            if (settings == null)
            {
                Debug.LogError("The editor-authored Settings screen is missing from Main.", this);
                return;
            }
            settings.Bind(game, this);
            settings.Open();
        }
        public void ShowStoreName(bool firstVisit = false)
        {
            if (!game.CanPlay || !E.CanMutate) return;
            if (firstVisit && !string.IsNullOrEmpty(E.State.storeName)) return;
            if (!firstVisit) game.SettingsScreen.CloseForChild();
            game.StoreNameScreen.Open(game, firstVisit, firstVisit ? (Action)null : ShowSettings);
        }
        public void ShowStoreNameDetail()
        {
            if (game == null || E == null || !game.StartupReady || game.ModalPaused || game.IsAdShowing) return;
            game.StoreNameScreen.OpenDetail(game, currentTab == "Home" ? "Back to shop" : "Back to " + currentTab);
        }
        public void ShowPrivacy()
        {
            var config = Resources.Load<ProductionConfig>("ProductionConfig");
            if (config != null && !string.IsNullOrWhiteSpace(config.privacyUrl))
            {
                Application.OpenURL(config.privacyUrl);
                return;
            }

            var privacy = game.PrivacyScreen;
            if (privacy == null)
            {
                Debug.LogError("The editor-authored Privacy screen is missing from Main.", this);
                return;
            }

            if (game.SettingsScreen != null && game.SettingsScreen.gameObject.activeInHierarchy)
                game.SettingsScreen.CloseForChild();
            privacy.Bind(game, ShowSettings);
            privacy.Open();
            game.ModalPaused = true;
        }
        public void ShowSaveRecovery()
        {
            var recovery = game.SaveRecoveryScreen;
            if (recovery == null)
            {
                Debug.LogError("The editor-authored Save Recovery screen is missing from Main.", this);
                return;
            }
            recovery.Bind(game);
            recovery.Open();
        }
        public static string Duration(double seconds)
        {
            int value = Math.Max(0, (int)Math.Ceiling(seconds));
            return (value / 60).ToString("00") + ":" + (value % 60).ToString("00");
        }

        public static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }
    }
}
