#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Exercises the shipped, editor-authored UI in a development player.</summary>
    public sealed partial class SmokeHarness : MonoBehaviour
    {
        GameController game;
        string directory;
        readonly List<string> checks = new List<string>();
        int errors;
        float startedAt;
        bool finished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!GameController.SmokeMode) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
                new GameObject("Development smoke checks").AddComponent<SmokeHarness>();
            else UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSmokeScene;
        }
        static void OnSmokeScene(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name != "Main") return;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSmokeScene;
            new GameObject("Development smoke checks").AddComponent<SmokeHarness>();
        }

        void OnEnable() { startedAt = Time.realtimeSinceStartup; Application.logMessageReceived += Log; }
        void OnDisable()
        {
            Application.logMessageReceived -= Log;
#if UNITY_EDITOR
            RestoreReviewViewport();
#endif
        }
        void Update()
        {
            if (!finished && Time.realtimeSinceStartup - startedAt > 90)
            {
                Check(false, "Smoke run timed out; see the recorded error above");
                Finish();
            }
        }
        void Log(string message, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            errors++;
            checks.Add("ERROR " + message);
        }

        void Check(bool ok, string message)
        {
            checks.Add((ok ? "PASS " : "FAIL ") + message);
            if (!ok) errors++;
        }

        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "--smoke");
            directory = at >= 0 && at + 1 < args.Length ? args[at + 1] : Application.temporaryCachePath;
#if UNITY_EDITOR
            if (UnityEditor.SessionState.GetBool("RocketRaja.EditorSmoke", false))
                directory = UnityEditor.SessionState.GetString("RocketRaja.EditorSmokeDirectory", directory);
#endif
            Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(2);
            game = FindFirstObjectByType<GameController>();
            Check(game != null, "Main scene booted");
            if (game == null) { Finish(); yield break; }
#if UNITY_EDITOR
            string motionStage = UnityEditor.SessionState.GetString("RocketRaja.HtmlMotionStage", "");
            if (motionStage == "taps") { yield return HtmlTapChecks(); Finish(); yield break; }
            if (motionStage == "celebration") { yield return HtmlCelebrationChecks(); Finish(); yield break; }
            if (motionStage == "ambient") { yield return HtmlAmbientChecks(); Finish(); yield break; }
            if (motionStage == "ui-bugs") { yield return UiBugChecks(); Finish(); yield break; }
            if (motionStage == "max-purchase") { yield return MaxPurchaseChecks(); Finish(); yield break; }
            if (motionStage == "milestones") { yield return MilestoneChecks(); Finish(); yield break; }
            if (motionStage == "unlockables") { yield return UnlockableChecks(); Finish(); yield break; }
#endif
            var e = game.Engine;
            e.State.reducedMotion = true;
            e.State.music = false;
            Check(Resources.Load<GameContent>("GameContent")?.catalog.upgrades.Length == 96,
                "Authored catalog loaded");
            Check(game.HomeScreen != null && game.PlayerScreen != null && game.ProductsScreen != null &&
                  game.LegacyScreen != null && game.PrivacyScreen != null && game.SettingsScreen != null &&
                  game.SaveRecoveryScreen != null && game.OfflineScreen != null && game.SponsorScreen != null &&
                  game.PrestigeScreen != null && game.StatisticsScreen != null && game.SkillsScreen != null &&
                  game.ProductDetailScreen != null, "All thirteen HTML screens bound in Main");
            Check(game.GetComponent<GameView>() == game.View && EventSystem.current != null &&
                  GameObject.Find("Diwali UI") == null, "Saved UI boots without generated canvas");
            Check(!FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(x => x.name.EndsWith("__LegacyDisabled", StringComparison.Ordinal)),
                "No obsolete UI copies remain in the shipped scene");
            var cashLabel = game.HomeScreen.GetComponentsInChildren<TMP_Text>(true)
                .FirstOrDefault(x => x.name == "cash-value");
            Check(cashLabel != null && cashLabel.font.HasCharacter('₹', true, true),
                "Home wallet supports rupee glyph");
            if (cashLabel == null) { Finish(); yield break; }
            yield return Capture("01-home");

            var tapTarget = game.HomeScreen.GetComponentInChildren<ShopTapTarget>(true);
            Check(tapTarget != null, "Saved stall tap target present");
            if (tapTarget == null) { Finish(); yield break; }
            var pointer = new PointerEventData(EventSystem.current)
                { pointerId = -1, position = new Vector2(200, 600) };
            long taps = e.State.manualTaps;
            tapTarget.OnPointerDown(pointer); tapTarget.OnPointerUp(pointer);
            Check(e.State.manualTaps == taps + 1, "Stall tap earns exactly once");
            taps = e.State.manualTaps;
            tapTarget.OnPointerDown(pointer); pointer.position += new Vector2(60, 0);
            tapTarget.OnDrag(pointer); tapTarget.OnPointerUp(pointer);
            Check(e.State.manualTaps == taps, "Stall drag earns no tap income");

            e.State.cash = 1e25;
            game.View.Switch("Products");
            Check(game.ProductsScreen.gameObject.activeSelf && !game.PlayerScreen.gameObject.activeSelf,
                "Products opens saved panel");
            for (int i = 0; i < 8; i++) Check(e.BuyProduct(i, 1), "Product purchase " + i);
            game.View.RefreshNow();
            yield return Capture("02-products");
            for (int i = 0; i < 8; i++) Check(e.BuyProduct(i, 249), "Product reaches unlockable milestone " + i);
            game.View.RefreshNow();
            Check(e.State.OwnedProducts == 8, "All product shelves owned");

            game.View.Switch("Player");
            e.State.tapLevel = 100;
            e.BuyAll();
            game.View.RefreshNow();
            Check(e.State.upgrades.Count == 96, "All run unlockables purchased");
            Check(game.PlayerScreen.gameObject.activeSelf && !game.ProductsScreen.gameObject.activeSelf,
                "Player opens saved panel");
            yield return Capture("03-player");
            var scroll = game.PlayerScreen.GetComponentInChildren<ScrollRect>(true);
            if (scroll != null)
            {
                scroll.verticalNormalizedPosition = .5f;
                yield return null;
                float position = scroll.verticalNormalizedPosition;
                game.View.Switch("Products"); game.View.Switch("Player");
                yield return null;
                Check(Math.Abs(scroll.verticalNormalizedPosition - position) < .001f &&
                      e.State.manualTaps == taps, "Player scroll survives navigation without taps");
            }
            else Check(false, "Player scroll exists");

            e.State.eligibleEarnings = Catalog.PrestigeThreshold;
            game.View.ShowPrestige();
            Check(game.PrestigeScreen.gameObject.activeSelf, "Prestige dialog opens");
            yield return Capture("04-prestige");
            var confirm = game.PrestigeScreen.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(x => x.name == "confirm-prestige");
            Check(confirm != null && confirm.interactable, "Prestige confirmation ready");
            if (confirm != null && confirm.interactable) confirm.onClick.Invoke();
            Check(e.State.prestiges == 1 && e.State.OwnedProducts == 0 && e.State.reputation == 5,
                "Prestige reset and reward applied");
            Check(game.LegacyScreen.gameObject.activeSelf, "Prestige routes to saved Legacy panel");
            var perk = game.LegacyScreen.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(x => x.name == "buy-perk-9");
            Check(perk != null && perk.interactable, "Legacy perk affordable");
            if (perk != null && perk.interactable) perk.onClick.Invoke();
            Check(e.State.Legacy(9) == 1 && e.State.reputation == 2,
                "Legacy purchase deducts reputation and persists rank");
            yield return Capture("05-legacy");

            game.View.Switch("Home");
            e.State.products[0].level = 25;
            e.ResolveOffline(e.State.now + 3600);
            game.View.ShowOffline();
            Check(game.OfflineScreen.gameObject.activeSelf, "Offline dialog opens");
            yield return Capture("06-offline");
            var cash = e.State.cash;
            var bonus = e.State.pendingOfflineReward;
            game.OfflineScreen.RequestVideo();
            Check(e.State.cash >= cash + bonus && string.IsNullOrEmpty(e.State.pendingOfflineId),
                "Test rewarded callback grants offline bonus once");
            var adapter = game.Ads as TestRewardedAds;
            Check(adapter != null, "Test rewarded ads adapter active");
            if (adapter != null)
            {
                adapter.Success = false; game.ShowAd("sponsor");
                Check(e.State.sponsorUntil == 0, "Cancelled sponsor grants no bonus");
                adapter.Success = true; game.ShowAd("sponsor");
                Check(e.State.sponsorUntil > e.State.now, "Completed sponsor activates boost");
            }
            game.View.ShowStatistics(); Check(game.StatisticsScreen.gameObject.activeSelf, "Statistics opens"); game.StatisticsScreen.Close();
            game.View.ShowSkills(); Check(game.SkillsScreen.gameObject.activeSelf, "Skills opens"); game.SkillsScreen.Close();
            game.View.ShowProductDetail(0); Check(game.ProductDetailScreen.gameObject.activeSelf, "Product detail opens"); game.ProductDetailScreen.Close();
            game.View.ShowSettings(); Check(game.SettingsScreen.gameObject.activeSelf, "Settings opens"); game.SettingsScreen.Close();
            game.View.ShowSponsor(); Check(game.SponsorScreen.gameObject.activeSelf, "Sponsor opens"); game.SponsorScreen.Close();
            game.PrivacyScreen.Bind(game, null); game.PrivacyScreen.Open();
            Check(game.PrivacyScreen.gameObject.activeSelf, "Privacy opens"); game.PrivacyScreen.Close();
            game.SaveRecoveryScreen.Bind(game); game.SaveRecoveryScreen.Open();
            Check(game.SaveRecoveryScreen.gameObject.activeSelf && game.ModalPaused, "Save recovery opens as blocking screen");
            game.SaveRecoveryScreen.gameObject.SetActive(false); game.ModalPaused = false;
            foreach (int height in new[] { 900, 1200 })
            {
                ResizeForReview(540, height);
                yield return new WaitForSecondsRealtime(.5f);
                Check(Screen.width == 540 && Screen.height == height, "Requested review viewport is actually active: 540x" + height);
                yield return Capture("home-540x" + height);
            }
            Check(GameObject.Find("Diwali UI") == null, "No runtime canvas created during play");
            game.OfflineScreen.Dismiss();
            yield return MotionChecks();
            yield return HtmlControlChecks();
            game.Save();
            Finish();
        }

        IEnumerator MotionChecks()
        {
            var state = game.Engine.State;
            state.reducedMotion = false;
            game.View.Switch("Home");
            yield return new WaitForSecondsRealtime(.4f);
            yield return PopupCloseChecks();
            Time.timeScale = 0;
            game.View.ShowSettings();
            var settingsGroup = game.SettingsScreen.GetComponent<CanvasGroup>();
            Check(game.ModalPaused && !settingsGroup.interactable && settingsGroup.blocksRaycasts,
                "Opening modal blocks input and pauses economy");
            long beforeTaps = state.manualTaps;
            game.Tap();
            Check(state.manualTaps == beforeTaps, "Opening modal prevents shop income");
            yield return new WaitForSecondsRealtime(.4f);
            Check(settingsGroup.interactable && settingsGroup.alpha == 1, "Modal opens while time scale is zero");
            game.SettingsScreen.Close();
            Check(game.SettingsScreen.gameObject.activeSelf && game.ModalPaused && !settingsGroup.interactable,
                "Modal stays visible and blocks interaction through closing animation");
            yield return new WaitForSecondsRealtime(.3f);
            Check(!game.SettingsScreen.gameObject.activeSelf && !game.ModalPaused,
                "Closing animation releases modal pause");
            Time.timeScale = 1;

            game.View.ShowSettings();
            yield return new WaitForSecondsRealtime(.08f);
            game.SettingsScreen.Close();
            game.View.ShowSettings();
            yield return new WaitForSecondsRealtime(.4f);
            Check(game.SettingsScreen.gameObject.activeSelf && settingsGroup.interactable && game.ModalPaused,
                "Reopening a closing modal cancels stale close completion");
            game.View.ShowPrivacy();
            yield return new WaitForSecondsRealtime(.4f);
            Check(!game.SettingsScreen.gameObject.activeSelf && game.PrivacyScreen.gameObject.activeSelf && game.ModalPaused,
                "Settings to Privacy handoff retains the modal pause");
            game.PrivacyScreen.Close();
            yield return new WaitForSecondsRealtime(.6f);
            Check(game.SettingsScreen.gameObject.activeSelf && game.ModalPaused,
                "Privacy returns to Settings after closing");
            game.SettingsScreen.Close();
            yield return new WaitForSecondsRealtime(.3f);
            Check(!game.ModalPaused, "Nested modal handoff leaves no stale pause");

            game.View.Switch("Products");
            game.View.Switch("Player");
            game.View.Switch("Legacy");
            game.View.Switch("Products");
            yield return new WaitForSecondsRealtime(.5f);
            Check(game.ProductsScreen.gameObject.activeSelf && !game.PlayerScreen.gameObject.activeSelf &&
                !game.LegacyScreen.gameObject.activeSelf && game.ProductsScreen.GetComponent<CanvasGroup>().interactable,
                "Rapid navigation settles on only the selected panel");

            state.cash = new Amount(1, 30);
            game.View.RefreshNow();
            int level = state.Level(0);
            var buy = game.ProductsScreen.GetComponentsInChildren<Button>(true).First(x => x.name == "purchase-0");
            buy.onClick.Invoke();
            Check(state.Level(0) > level, "Animated product purchase commits immediately");
            for (int i = 0; i < 15; i++) UiFeedback.Pulse(buy.transform, game, .1f);
            yield return new WaitForSecondsRealtime(.65f);
            Check(Vector3.Distance(buy.transform.localScale, Vector3.one) < .001f,
                "Repeated purchase feedback returns to its authored scale");

            var quantity = game.ProductsScreen.GetComponentsInChildren<Button>(true).First(x => x.name == "buy-quantity");
            quantity.onClick.Invoke();
            game.View.Switch("Player");
            yield return new WaitForSecondsRealtime(.35f);
            game.View.Switch("Products");
            yield return new WaitForSecondsRealtime(.35f);
            quantity.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.35f);
            var menuMotion = game.ProductsScreen.GetComponentsInChildren<UiMotion>().FirstOrDefault(x => x != game.ProductsScreen.GetComponent<UiMotion>());
            Check(menuMotion != null && menuMotion.GetComponent<CanvasGroup>().interactable,
                "Quantity menu remains usable after leaving during its opening animation");
            quantity.onClick.Invoke();

            var productPanel = game.ProductsScreen.GetComponent<ImportedPanelFit>().MotionTarget;
            game.View.Switch("Home");
            yield return new WaitForSecondsRealtime(.3f);
            game.View.Switch("Products");
            ResizeForReview(540, 900);
            yield return new WaitForSecondsRealtime(.5f);
            float fittedTop = -ImportedPanelFit.WorldHeight(((RectTransform)productPanel.parent).rect.height);
            Check(Mathf.Abs(productPanel.offsetMax.y - fittedTop) < .1f,
                "Resize during panel animation preserves the fitted layout position");

            game.View.ShowSkills();
            state.reducedMotion = true;
            yield return null;
            yield return null;
            Check(game.SkillsScreen.GetComponent<CanvasGroup>().interactable,
                "Enabling Reduced Motion mid-transition completes the modal");
            game.SkillsScreen.Close();
            Check(!game.SkillsScreen.gameObject.activeSelf && !game.ModalPaused,
                "Reduced Motion closes immediately and releases input");

            state.reducedMotion = false;
            for (int i = 0; i < 8; i++) state.products[i].level = 25;
            state.eligibleEarnings = Catalog.PrestigeThreshold;
            game.View.ShowPrestige();
            yield return new WaitForSecondsRealtime(.4f);
            int prestiges = state.prestiges;
            var confirm = game.PrestigeScreen.GetComponentsInChildren<Button>(true).First(x => x.name == "confirm-prestige");
            confirm.onClick.Invoke();
            confirm.onClick.Invoke();
            Check(state.prestiges == prestiges + 1, "Repeated prestige confirm awards exactly once during exit");
            yield return new WaitForSecondsRealtime(.55f);
            Check(game.LegacyScreen.gameObject.activeSelf && !game.PrestigeScreen.gameObject.activeSelf && !game.ModalPaused,
                "Animated prestige completes on Legacy without a modal lock");
            var noticeCanvas = game.HomeScreen.GetComponentsInChildren<Canvas>(true).First(x => x.name == "Notice");
            Check(noticeCanvas.overrideSorting && noticeCanvas.sortingOrder > game.LegacyScreen.GetComponent<Canvas>().sortingOrder,
                "Reward toast renders above the active lower panel");
            yield return Capture("07-prestige-celebration", 0);
            game.View.Switch("Home");
            yield return new WaitForSecondsRealtime(2);
        }

        IEnumerator PopupCloseChecks()
        {
            Action[] open =
            {
                game.View.ShowSettings, game.View.ShowStatistics, game.View.ShowSkills,
                () => game.View.ShowProductDetail(0), game.View.ShowPrestige, game.View.ShowSponsor,
                () => { game.Engine.ResolveOffline(game.Engine.State.now + 3600); game.View.ShowOffline(); },
                () => { game.PrivacyScreen.Bind(game, null); game.PrivacyScreen.Open(); },
                () => { game.SaveRecoveryScreen.Bind(game); game.SaveRecoveryScreen.Open(); }
            };
            MonoBehaviour[] screens =
            {
                game.SettingsScreen, game.StatisticsScreen, game.SkillsScreen, game.ProductDetailScreen,
                game.PrestigeScreen, game.SponsorScreen, game.OfflineScreen, game.PrivacyScreen, game.SaveRecoveryScreen
            };
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                open[i]();
                yield return new WaitForSecondsRealtime(.4f);
                var group = screen.GetComponent<CanvasGroup>();
                Check(screen.gameObject.activeInHierarchy && group != null && group.interactable,
                    screen.name + " opens with a real, interactive CanvasGroup");
                var close = (Button)screen.GetType().GetField(i == 8 ? "startButton" : "closeButton",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(screen);
                var rect = (RectTransform)close.transform;
                var pointer = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
                };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                bool reachable = hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == close;
                Check(reachable, screen.name + " close control receives pointer input");
                if (reachable) ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
                yield return new WaitForSecondsRealtime(.3f);
                Check(!screen.gameObject.activeSelf && !game.ModalPaused,
                    screen.name + " closes through its button and releases the game");
            }
        }

        IEnumerator Capture(string name, float settle = .3f)
        {
            game.View.RefreshNow();
            Canvas.ForceUpdateCanvases();
            yield return new WaitForSecondsRealtime(settle);
            var camera = Camera.main;
            if (camera == null) { Check(false, "Camera for capture " + name); yield break; }
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Where(x => x.isRootCanvas && x.gameObject.activeInHierarchy).ToArray();
            var modes = canvases.Select(x => x.renderMode).ToArray();
            var cameras = canvases.Select(x => x.worldCamera).ToArray();
            var distances = canvases.Select(x => x.planeDistance).ToArray();
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = camera;
                canvases[i].planeDistance = 10;
            }
            camera.targetTexture = target;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
            Destroy(image);
            checks.Add("CAPTURE " + name + " " + target.width + "x" + target.height);
            RenderTexture.active = oldActive;
            camera.targetTexture = oldTarget;
            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].renderMode = modes[i];
                canvases[i].worldCamera = cameras[i];
                canvases[i].planeDistance = distances[i];
            }
            RenderTexture.ReleaseTemporary(target);
            yield return null;
        }

        void Finish()
        {
            if (finished) return;
            finished = true;
            checks.Add(errors == 0 ? "SMOKE SUCCEEDED" : "SMOKE FAILED: " + errors);
            File.WriteAllLines(Path.Combine(directory, "SmokeResults.txt"), checks);
#if UNITY_EDITOR
            RestoreReviewViewport();
            UnityEditor.SessionState.SetBool("RocketRaja.EditorSmoke", false);
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(errors == 0 ? 0 : 1);
#endif
        }
    }
}
#endif
