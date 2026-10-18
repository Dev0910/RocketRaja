using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    /// <summary>Exercises the saved UI against a disposable guest, without touching player saves.</summary>
    [InitializeOnLoad]
    public static class ClassicBugRegression
    {
        const string Key = "Classic.BugRegression";
        const string Report = "Temp/classic-bug-regression.txt";
        static readonly List<string> checks = new List<string>();
        static ClassicBugRegression() { EditorApplication.playModeStateChanged += Mode; }
        public static void Start(bool productBars = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Save scene edits first");
            SessionState.SetString(Key + ".scene", SceneManager.GetActiveScene().path);
            SessionState.SetBool(Key + ".bars", productBars);
            var test = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var main = EditorSceneManager.OpenScene(BuildClassicUI.ScenePath, OpenSceneMode.Additive);
            var copy = UnityEngine.Object.Instantiate(ClassicImportQA.UI.gameObject);
            SceneManager.MoveGameObjectToScene(copy, test);
            var ui = copy.GetComponent<ClassicUI>();
            UnityEngine.Object.DestroyImmediate(ui.application); ui.application = null; ui.isolatedPreview = true;
            EditorSceneManager.CloseScene(main, true); SceneManager.SetActiveScene(test);
            new GameObject("Regression EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SessionState.SetBool(Key, true); EditorSceneManager.playModeStartScene = null;
            EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (mode == PlayModeStateChange.EnteredPlayMode) EditorApplication.delayCall += Run;
            if (mode == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Key);
                EditorSceneManager.OpenScene(SessionState.GetString(Key + ".scene", ClassicProductionAuthoring.Bootstrap));
            }
        }
        static void Check(bool condition, string label)
        {
            checks.Add((condition ? "PASS " : "FAIL ") + label);
            if (!condition) throw new InvalidOperationException(label);
        }
        static async void Run()
        {
            checks.Clear();
            try
            {
                var ui = UnityEngine.Object.FindFirstObjectByType<ClassicUI>();
                // Keep physical Editor clicks from changing the disposable review mid-capture.
                EventSystem.current.GetComponent<StandaloneInputModule>().enabled = false;
                Check(ui.driver.InitializeGuest(Path.GetFullPath("Temp/classic-regression-" + Guid.NewGuid().ToString("N")), true), "Disposable guest initializes");
                ui.Bind(); ui.driver.enabled = false;
                await System.Threading.Tasks.Task.Delay(500);
                if (SessionState.GetBool(Key + ".bars", false))
                {
                    await ProductBars(ui);
                    File.WriteAllLines(Report, checks); EditorApplication.ExitPlaymode(); return;
                }
                var toolbar = ui.transform.Find("SafeArea/Panels/Player/Scroll/Viewport/Content/Toolbar");
                foreach (string name in new[] { "Stats", "Skills" })
                {
                    var icon = toolbar.Find(name + "/Icon").GetComponent<Image>();
                    Check(icon.sprite == AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/Icon " + name + ".png") && !icon.raycastTarget, name + " uses HTML atlas sprite without intercepting input");
                }
                foreach (string tab in new[] { "Home", "Player", "Products", "Legacy", "Shop" })
                {
                    ui.SetTab(tab); ui.Engine.Advance(1); Canvas.ForceUpdateCanvases();
                    var target = (RectTransform)ui.tapTarget.transform;
                    foreach (float x in new[] { -.25f, .25f })
                    {
                        ui.Engine.Advance(1);
                        foreach (var particle in ui.motion.particles) particle.gameObject.SetActive(false);
                        var world = target.TransformPoint(new Vector3(target.rect.width * x, 0, 0));
                        var pointer = new PointerEventData(EventSystem.current) { pointerId = 10, position = RectTransformUtility.WorldToScreenPoint(null, world), button = PointerEventData.InputButton.Left };
                        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
                        Check(hits.Count > 0 && hits[0].gameObject == target.gameObject, "Stall receives raycast on " + tab + " at " + x + "; hits=" + string.Join(",", hits.Select(h => h.gameObject.name)) + "; screen=" + pointer.position + "/"+Screen.width+"x"+Screen.height+" depth="+target.GetComponent<Image>().depth);
                        var before = ui.Engine.State.manualTaps;
                        ui.tapTarget.OnPointerDown(pointer); ui.tapTarget.OnPointerUp(pointer);
                        Check(ui.Engine.State.manualTaps.Value == before.Value + 1, "Stall pays once on " + tab + " at " + x);
                        var coins = ui.motion.particles.Where(p => p.gameObject.activeSelf && p.sprite == ui.motion.coinSprite).ToArray();
                        var local = (Vector2)ui.motion.effectRoot.InverseTransformPoint(world);
                        Check(coins.Length == 3 && coins.All(p => Vector2.Distance(p.rectTransform.anchoredPosition, local) < .1f), "Coins originate at pointer on " + tab + " at " + x);
                        ui.Engine.Advance(1); before = ui.Engine.State.manualTaps;
                        ui.tapTarget.OnPointerDown(pointer); ui.tapTarget.OnBeginDrag(pointer); ui.tapTarget.OnPointerUp(pointer);
                        Check(ui.Engine.State.manualTaps.Value == before.Value, "Dragging stall gives no tap on " + tab);
                    }
                }
                ui.SetTab("Player"); ui.Refresh();
                Check(ui.nextUpgrades.All(r => r.root.activeSelf), "Fresh Player displays ten upgrades");
                ui.Engine.State.cash = 0; ui.Refresh();
                Check(ui.nextUpgrades.All(r => !r.buy.interactable), "Unaffordable upgrades remain disabled");
                for (int i = 0; i < 4; i++) ui.Handle("Quantity", "tap");
                var tapText = ui.textBindings.Single(t => t.key == "tap-row-cost").text.text;
                Check(tapText.StartsWith("₹40") && tapText.Contains("×1"), "Unaffordable Max tap displays the next price and one level");
                var beforeDialog = ui.Engine.State.manualTaps;
                ui.Open("Statistics"); ui.tapTarget.Tap();
                Check(ui.Engine.State.manualTaps.Value == beforeDialog.Value, "Modal controls do not trigger background earning");
                ui.motion.reduced = true; ui.Close(); ui.Engine.Advance(1); ui.tapTarget.Tap();
                Check(ui.CurrentTab == "Player" && ui.Engine.State.manualTaps.Value == beforeDialog.Value + 1, "Closing dialog restores stall earning on Player");
                ui.Engine.State.cash = 30;
                Check(ui.driver.Command(e => e.BuyLevels(0, 1)), "Buy initial product for Max price regression");
                ui.SetTab("Products"); for (int i = 0; i < 4; i++) ui.Handle("Quantity", "products");
                Check(ui.products[0].cost.text.StartsWith("₹" + ui.Engine.LevelQuote(0, 1).cost) && ui.products[0].cost.text.Contains("×1") && !ui.products[0].buy.interactable, "Unaffordable product Max displays next level price with purchase disabled");
                ui.SetTab("Player"); Canvas.ForceUpdateCanvases();
                await System.Threading.Tasks.Task.Delay(700);
                Canvas.ForceUpdateCanvases();
                Check(ui.panels.Single(p => p.route == "Player").group.alpha > .99f, "Player panel is visible after its entrance animation");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/classic-bugfix-player.png"));
                var balanceState = ui.Engine.State;
                balanceState.fame = balanceState.completedPrestiges = 1;
                balanceState.target = RocketRaja.Core.ProgressionNumbers.RunTarget(1);
                balanceState.eligibleEarnings = balanceState.target;
                ui.InvalidateQuotes(); ui.SetTab("Player"); ui.Open("Prestige");
                await System.Threading.Tasks.Task.Delay(700); Canvas.ForceUpdateCanvases();
                var fameLabel = ui.textBindings.Single(b => b.key == "fame").text;
                var prestigeLabel = ui.textBindings.Single(b => b.key == "prestige-copy").text;
                Check(fameLabel.text.Contains("products ×") && fameLabel.text.Contains("taps ×"), "Player shows separate product and tap Fame bonuses");
                Check(prestigeLabel.text.Contains("Products ×") && prestigeLabel.text.Contains("taps ×"), "Prestige review shows both resulting Fame bonuses");
                fameLabel.ForceMeshUpdate(); prestigeLabel.ForceMeshUpdate();
                Check(!fameLabel.isTextOverflowing && !prestigeLabel.isTextOverflowing, "Fame and prestige bonus labels fit the saved UI");
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/tap-balance-prestige.png"));
                File.WriteAllLines(Report, checks);
                SessionState.SetFloat(Key + ".exit", (float)EditorApplication.timeSinceStartup + 1);
                EditorApplication.update += ExitAfterCapture;
            }
            catch (Exception ex)
            {
                checks.Add(ex.ToString()); File.WriteAllLines(Report, checks); EditorApplication.ExitPlaymode();
            }
        }
        static void ExitAfterCapture()
        {
            if (EditorApplication.timeSinceStartup < SessionState.GetFloat(Key + ".exit", 0)) return;
            EditorApplication.update -= ExitAfterCapture; EditorApplication.ExitPlaymode();
        }
        static async System.Threading.Tasks.Task ProductBars(ClassicUI ui)
        {
            Check(ui.Engine.State.products.All(n => n == 0) && ui.Engine.BuyLevels(0, 1) && Math.Abs(RocketRaja.Core.Amount.Ratio(ui.Engine.NormalIncome(), new RocketRaja.Core.Amount(.85)) - 1) < 1e-10, "Fresh opening buys the first product for the seeded cash and earns 0.85 per second");
            Check(ui.textBindings.Single(b => b.key == "quantity").text.text == "Buy Max" && ui.textBindings.Single(b => b.key == "tap-quantity").text.text == "Buy Max", "Product and tap quantity controls start on Max");
            ui.Engine.State.products = new[] { 0, 12, 24, 25, 37, 50, 75, 150 };
            ui.Engine.State.cash = 0;
            ui.InvalidateQuotes(); ui.SetTab("Products");
            await System.Threading.Tasks.Task.Delay(900);
            Canvas.ForceUpdateCanvases();
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/classic-product-bars.png"));
            await System.Threading.Tasks.Task.Delay(200);
            ui.Open("ProductDetail:1");
            await System.Threading.Tasks.Task.Delay(900);
            Canvas.ForceUpdateCanvases();
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/classic-product-popup-bars.png"));
            await System.Threading.Tasks.Task.Delay(200);
            Check(Mathf.Abs(ui.products[1].progress.fillAmount - 2f/15) < .001f, "Level 12 tile progress is 13.33 percent between the 10 and 25 milestones");
            Check(Mathf.Abs(ui.imageBindings.Single(b => b.key == "product-progress").image.fillAmount - 2f/15) < .001f, "Level 12 popup matches tile progress");
            var bars = ui.products.Select(p => p.progress).Concat(ui.imageBindings.Where(b => b.key == "product-progress" || b.key == "synergy-progress").Select(b => b.image)).ToArray();
            Check(bars.Length == 10 && bars.All(b => AssetDatabase.GetAssetPath(b.sprite) == "Assets/Game/ClassicUI/Solid Fill.png" && b.fillMethod == Image.FillMethod.Horizontal && b.fillOrigin == 0), "All ten product bars use solid left-to-right fills");
            var pixels = new Texture2D(2, 2);
            ImageConversion.LoadImage(pixels, File.ReadAllBytes("Assets/Game/ClassicUI/Solid Fill.png"));
            Check(pixels.GetPixels().All(p => p == Color.white), "Fill texture has no transparent corners, gradient or shadow");
            UnityEngine.Object.Destroy(pixels);
            ui.Engine.State.reducedMotion = true;
            var levels = new[] { 0, 1, 12, 24, 25, 37, 49, 50, 75, 99, 100, 150, 99999, 100000 };
            var expected = new[] { 0f, .1f, 2f/15, 14f/15, 0f, .48f, .96f, 0f, .5f, .98f, 0f, .5f, .99f, 1f };
            for (int i = 0; i < levels.Length; i++)
            {
                ui.Engine.State.products[1] = levels[i]; ui.Refresh();
                Check(Mathf.Abs(ui.products[1].progress.fillAmount - expected[i]) < .001f && Mathf.Abs(ui.imageBindings.Single(b => b.key == "product-progress").image.fillAmount - expected[i]) < .001f, "Tile and popup agree at level " + levels[i]);
            }
            ui.Engine.State.products = Enumerable.Repeat(50, 8).ToArray(); ui.Engine.State.products[7] = 49; ui.Refresh();
            Check(ui.textBindings.Single(b => b.key == "synergy-progress-label").text.text == "7/8 products at Lvl 50" && ui.textBindings.Single(b => b.key == "synergy-copy").text.text.Contains("Celebration boxes (49)"), "Synergy identifies the final product below its shared threshold");
            ui.Engine.State.products[7] = 50; ui.Refresh();
            Check(ui.textBindings.Single(b => b.key == "synergy-progress-label").text.text == "0/8 products at Lvl 100" && ui.textBindings.Single(b => b.key == "synergy-reward").text.text.Contains("×2"), "Synergy displays the earned multiplier and advances to the next threshold");
            ui.Engine.State.products = new int[8]; ui.Engine.State.cash = new RocketRaja.Core.Amount(1, 12); ui.Handle("BuyLevels", "0");
            Check(ui.Engine.State.products[0] == 1, "Stock shelf buys one initial level even with Max selected");
            ui.Handle("BuyLevels", "0"); Check(ui.Engine.State.products[0] > 1, "The next purchase upgrades with Max");
        }
    }
}
