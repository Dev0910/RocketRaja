using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RocketRaja;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicRevision20261025Validation
    {
        const string Key = "Classic.Revision20261025Validation";
        const string Folder = "Documentation/Implementation/ClassicHTMLSync/Unity20261025";
        static readonly List<string> report = new List<string>(), errors = new List<string>();
        public static string Status { get; private set; } = "Not run";
        static ClassicRevision20261025Validation() { EditorApplication.playModeStateChanged += Mode; }
        public static string Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var scene = SceneManager.GetSceneByPath("Assets/Scenes/Main.unity");
            if (!scene.isLoaded || scene.isDirty) throw new InvalidOperationException("Open clean Main first");
            var ui = scene.GetRootGameObjects().Single(g => g.name == "ClassicProductionCanvas").GetComponent<ClassicUI>();
            SessionState.SetInt(Key + ".Components", ui.GetComponentsInChildren<Component>(true).Length);
            SessionState.SetString(Key + ".StartScene", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            Directory.CreateDirectory(Folder); SessionState.SetBool(Key, true); EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); EditorApplication.EnterPlaymode();
            return Status = "Running production Classic revision validation";
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (mode == PlayModeStateChange.EnteredPlayMode) Run();
            if (mode == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Key); var path = SessionState.GetString(Key + ".StartScene", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);
            }
        }
        static void Check(bool ok, string text) { report.Add((ok ? "PASS " : "FAIL ") + text); if (!ok) throw new InvalidOperationException(text); }
        static async Task Until(Func<bool> done) { double end = EditorApplication.timeSinceStartup + 25; while (!done()) { if (EditorApplication.timeSinceStartup > end) throw new TimeoutException("Main startup timeout"); await Task.Delay(60); } }
        static void Log(string text, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(text); }
        static async Task Capture(string name) { await Task.Delay(360); ClassicImportQA.Capture(Folder + "/" + name + ".png"); }
        static Rect Bounds(Transform t) { var c = new Vector3[4]; ((RectTransform)t).GetWorldCorners(c); return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y); }
        static async void Run()
        {
            report.Clear(); errors.Clear(); Application.logMessageReceived += Log;
            string root = Path.Combine(Path.GetTempPath(), "ClassicRevision-" + Guid.NewGuid().ToString("N"));
            try
            {
                var catalog = RedesignRuntimeContent.Load(); var s = RedesignState.Fresh(RedesignRuntimeDriver.Utc);
                s.products = new[] { 25, 18, 12, 8, 5, 3, 1, 1 }; s.cash = 1240000; s.tapLevel = 12;
                s.completedPrestiges = 1; s.fame = 1; s.target = ProgressionNumbers.RunTarget(s.completedPrestiges);
                s.kajuKatli = 400; s.freeCredits = 0; s.cardLevels[0] = 1; s.cardLevels[16] = 1; s.cardLevels[26] = 1;
                s.storeName = "DIWALI DUKAAN"; s.storeNamePromptHandled = true;
                s.upgrades = catalog.Upgrades.Where(u => u.effect == CashEffect.SkillPower && u.rank == 1).Select(u => u.id).ToList(); s.everPurchased = s.upgrades.ToList();
                var store = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), catalog, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
                new RedesignEngine(catalog, s, store.Save).Save();
                var host = RedesignSessionHost.Ensure(); host.ConfigureFixture(null, root); await host.ReturnToGuestAsync(false);
                ClassicImportQA.Viewport(390, 844); host.EnterMain();
                await Until(() => UnityEngine.Object.FindFirstObjectByType<ClassicApplication>()?.Ready == true);
                var ui = UnityEngine.Object.FindFirstObjectByType<ClassicApplication>().ui;
                ui.driver.enabled = false;
                if (ui.CurrentDialog == "Offline") { ui.Handle("Offline", "close"); await Task.Delay(450); }
                ui.application.enabled = false; ui.motion.reduced = ui.Engine.State.reducedMotion = true;
                ui.SetTab("Home"); await Capture("01-home-square-settings");
                var identities = ui.GetComponentsInChildren<Component>(true).Select(c => c.GetInstanceID()).OrderBy(i => i).ToArray();
                Check(identities.Length == SessionState.GetInt(Key + ".Components", -1), "All UI components exist in the saved scene before startup");
                var gear = ui.GetComponentsInChildren<ClassicControl>().Single(c => c.action == "Navigate" && c.parameter == "Settings");
                var size = ((RectTransform)gear.transform).rect.size; Check(size == new Vector2(48, 48), "Settings uses a square 48-unit target");
                Check(ui.upgradeTypeIcons.Length == 12 && ui.upgradeTypeIcons.All(i => i), "All twelve effect icons have durable sprite references");
                Check(catalog.Upgrades.All(u => ui.upgradeTypeIcons[(int)u.effect]), "All 1,000 upgrades map to an authored effect icon");
                var formatter = ui.GetComponent<ClassicCurrencyPresentation>();
                Check(formatter && formatter.labels.All(t => ((RedesignSavedText)t).savedMaterialSlots.Length >= 4), "Inline currency uses Editor-preplaced material slots");
                foreach (var route in new[] { "Player", "Products", "Legacy", "Shop" })
                {
                    ui.SetTab(route); await Capture("02-" + route);
                    var panel = ui.panels.Single(v => v.route == route);
                    var guide = panel.root.transform.Find("Contextual guidance"); Check(!guide || !guide.gameObject.activeSelf, route + " has no large guidance banner");
                    if (route == "Player") Check(ui.achievementTabLabel.text == "Achievements", "Achievement shortcut never shows a count");
                }
                foreach (var route in new[] { "Player", "Products", "ProductDetail:0" })
                {
                    ui.SetTab("Home"); if (route.Contains(":")) ui.Open(route); else ui.SetTab(route);
                    string topic = route == "Player" ? "buy-all" : "product-levels";
                    var info = ui.GetComponentsInChildren<ClassicControl>().Single(c => c.action == "Info" && c.parameter == topic);
                    Check(((RectTransform)info.transform).rect.size == new Vector2(48, 48), route + " information target is 48 units");
                    ui.Handle("Info", topic); Check(ui.CurrentDialog == "Guidance" && ui.guidance.CurrentTopic == topic, route + " information opens the existing topic");
                    ui.guidance.Acknowledge(); await Task.Delay(100); ui.Handle("Info", topic);
                    Check(ui.CurrentDialog == "Guidance", route + " information remains available after learning"); ui.Close();
                }
                foreach (var rows in new[] { ui.nextUpgrades, ui.catalogueRows })
                {
                    ui.SetTab("Player"); if (rows == ui.catalogueRows) ui.Open("Upgrades"); Canvas.ForceUpdateCanvases();
                    foreach (var row in rows.Where(r => r.root.activeSelf))
                    {
                        var b = Bounds(row.buy.transform); var t = Bounds(row.title.transform); var art = Bounds(row.artwork.transform); var r = Bounds(row.root.transform);
                        Check(art.xMax <= t.xMin + .5f && t.xMax <= b.xMin + .5f && b.xMax < r.xMax && b.yMin >= r.yMin && b.yMax <= r.yMax, "Upgrade icon/copy/button columns stay separate: " + row.control.parameter);
                        catalog.TryUpgrade(row.control.parameter, out int index);
                        Check(row.artwork.sprite == ui.upgradeTypeIcons[(int)catalog.Upgrades[index].effect], "Upgrade artwork follows its actual effect: " + row.control.parameter);
                    }
                }
                ui.SetTab("Products"); Canvas.ForceUpdateCanvases();
                foreach (var p in ui.products)
                {
                    var buy = Bounds(p.buy.transform); var row = Bounds(p.buy.transform.parent); var details = Bounds(p.buy.transform.parent.Find("Details"));
                    Check(buy.xMax < row.xMax && buy.yMin >= row.yMin && buy.yMax <= row.yMax && details.xMax < buy.xMin, "Product stock/upgrade action stays within its own column");
                }
                ui.SetTab("Legacy"); formatter.Apply();
                var balance = ui.textBindings.Single(t => t.key == "legacy-kk").text;
                Check(!balance.text.Contains("KK") && ui.legacyCurrencyIcon && ui.legacyCurrencyIcon.sprite.texture == formatter.token.spriteSheet, "Legacy balance displays selected currency token beside the amount");
                Check(ui.GetComponentsInChildren<Image>(true).Where(i => i.name == "Selected information circle").All(i => i.sprite), "Every information circle has an imported sprite");
                var draw = ui.textBindings.Single(t => t.key == "draw-one").text;
                Check(draw.text.Contains("<sprite=0>"), "Paid draw cost uses selected token");
                ui.Open("Achievements"); await Capture("03-achievements-token");
                var claim = ui.achievementRows.FirstOrDefault(r => ui.Engine.CanClaimAchievement(r.id));
                Check(claim != null, "Seeded achievement reward is claimable"); var before = ui.Engine.State.kajuKatli;
                ui.Handle("ClaimAchievement", claim.id); formatter.Apply();
                Check(ui.Engine.State.kajuKatli.CompareTo(before) > 0 && ui.achievementWallet.text.Contains("<sprite=0>"), "Live claim updates wallet and retains currency token");
                foreach (var route in new[] { "Settings", "ProductDetail:0", "Skills", "SkillDetail:0", "CardDetail:0", "CardDetail:16", "CardDetail:26", "Odds", "Prestige", "Help", "Purchase", "Upgrades", "Appearance", "Statistics" })
                {
                    ui.SetTab("Home"); ui.Open(route); await Capture("04-" + route.Replace(':', '-'));
                    var view = ui.dialogs.Single(v => v.route == route.Split(':')[0]);
                    Check(view.window.GetComponent<RectMask2D>() && !view.window.GetComponents<Outline>().Any(o => o.enabled), route + " has a single clipped popup shell");
                    if (route == "ProductDetail:0") Check(view.window.Find("Header/Title").GetComponent<TMP_Text>().color == new Color32(22, 75, 128, 255), "Product title has cobalt contrast");
                }
                ui.SetTab("Home"); ui.Open("Settings"); ui.Engine.State.reducedMotion = false; ui.Refresh();
                var switches = ui.GetComponentsInChildren<ClassicSettingsSwitch>().ToArray(); Check(switches.Length == 3, "All three settings use saved switch controls");
                var music = switches.Single(t => t.key == "music"); ui.Handle("Setting", "music"); await Task.Delay(70);
                Check(music.thumb.anchoredPosition.x > 16 && music.thumb.anchoredPosition.x < 58, "Settings switch has an intermediate sliding pose"); await Task.Delay(170);
                Check(Math.Abs(music.thumb.anchoredPosition.x - 16) < .1 && !ui.Engine.State.music, "Music switch settles to saved OFF state");
                ui.Handle("Setting", "motion"); await Task.Delay(25);
                Check(switches.Single(t => t.key == "motion").thumb.anchoredPosition.x == 58, "Reduced motion settles switch immediately");
                Check(!store.Load().state.music && store.Load().state.reducedMotion, "Settings values survive save reload"); await Capture("05-settings-switches");
                ui.SetTab("Legacy"); ui.Handle("Draw", "1"); await Capture("06-draw-reveal");
                Check(ui.CurrentDialog == "DrawReveal" && !ui.guidance.rows.Any(r => r.route == "DrawReveal"), "Draw reveal has no duplicate-card banner"); ui.Close();
                foreach (var size2 in new[] { new Vector2Int(360, 540), new Vector2Int(360, 640), new Vector2Int(450, 916) })
                {
                    ClassicImportQA.Viewport(size2.x, size2.y);
                    foreach (var route in new[] { "Settings", "ProductDetail:0", "Skills", "SkillDetail:0", "Achievements" })
                    { ui.SetTab("Home"); ui.Open(route); await Capture("07-" + size2.x + "x" + size2.y + "-" + route.Replace(':', '-')); Check(ui.CurrentDialog == route.Split(':')[0], "Responsive popup opens at " + size2 + ": " + route); }
                }
                ClassicImportQA.Viewport(390, 844);
                ui.SetTab("Home"); ui.Open("Settings");
                var safe = (RectTransform)ui.transform.Find("SafeArea"); var adapter = safe.GetComponent<ImportedSafeArea>(); adapter.enabled = false;
                safe.anchorMin = new Vector2(0, 34f / 844); safe.anchorMax = new Vector2(1, 1 - 44f / 844);
                await Capture("08-notched-settings");
                var popupBounds = Bounds(ui.dialogs.Single(v => v.route == "Settings").window);
                Check(popupBounds.yMin >= 34 && popupBounds.yMax <= 800, "Shared popup stays within simulated notch and bottom insets"); adapter.enabled = true;
                ui.SetTab("Home"); ui.Open("DeveloperReset"); ui.Handle("DeveloperReset", ""); await Task.Delay(400);
                Check(ui.Engine.State.cash.CompareTo(Amount.Zero) == 0 && ui.Engine.State.products.All(n => n == 0), "Development reset retains the current fresh economy");
                ui.SetTab("Products"); await Capture("09-fresh-products");
                Check(ui.products[0].cost.text.Contains("Stock shelf") && !ui.products[0].buy.interactable, "Fresh stock-shelf CTA is in place and disabled without cash");
                for (int i = 0; i < 30; i++) ui.driver.Command(e => { e.Advance(.12); return e.Tap(); });
                Check(ui.products[0].buy.interactable, "Earned opening cash enables the existing stock-shelf action");
                ui.Handle("BuyLevels", "0");
                Check(ui.Engine.State.products[0] == 1 && ui.CurrentTab == "Home", "The first product purchase still commits once and returns Home");
                ui.SetTab("Home"); ui.GetComponent<ClassicCurrencyPresentation>().Apply();
                var after = ui.GetComponentsInChildren<Component>(true).Select(c => c.GetInstanceID()).OrderBy(i => i).ToArray();
                Check(identities.SequenceEqual(after), "All UI identities remain stable through navigation, currency rendering, draws and settings");
                Check(errors.Count == 0, "No runtime errors during revised production UI verification");
                report.Add("Disposable profile only: " + root); Status = "PASS: " + report.Count(r => r.StartsWith("PASS")) + " Classic revision checks";
            }
            catch (Exception e) { Status = "FAIL: " + e.Message; report.Add(e.ToString()); report.AddRange(errors); }
            finally { Application.logMessageReceived -= Log; report.Add(Status); File.WriteAllLines(Folder + "/Validation.txt", report); EditorApplication.ExitPlaymode(); }
        }
    }
}
