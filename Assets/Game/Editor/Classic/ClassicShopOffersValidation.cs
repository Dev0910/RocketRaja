using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class ClassicShopOffersValidation
    {
        const string Key = "Classic.ShopOffersValidation";
        const string Folder = "Documentation/Implementation/ShopOffers";
        public static string Status { get; private set; } = "Not run";
        static readonly List<string> report = new List<string>();
        static ClassicShopOffersValidation() { EditorApplication.playModeStateChanged += Mode; }
        public static string Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorSceneManager.GetSceneManagerSetup().Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty))
                throw new InvalidOperationException("Clean Edit Mode required");
            SessionState.SetString(Key + ".StartScene", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetBool(Key, true); EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode(); return Status = "Running isolated shop validation";
        }
        static void Mode(PlayModeStateChange mode)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (mode == PlayModeStateChange.EnteredPlayMode) Run();
            if (mode == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Key);
                var start = SessionState.GetString(Key + ".StartScene", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(start) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(start);
                EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);
            }
        }
        static void Check(bool ok, string label) { if (!ok) throw new Exception(label); report.Add("PASS " + label); }
        static async void Run()
        {
            report.Clear(); Directory.CreateDirectory(Folder);
            try
            {
                string root = Path.Combine(Path.GetTempPath(), "ClassicShopOffers-" + Guid.NewGuid().ToString("N"));
                var catalog = RedesignRuntimeContent.Load(); var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc);
                state.products[0] = 10; state.kajuKatli = 400; state.storeNamePromptHandled = true;
                var store = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), catalog, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
                new RedesignEngine(catalog, state, store.Save).Save();
                var host = RedesignSessionHost.Ensure(); host.ConfigureFixture(null, root); await host.ReturnToGuestAsync(false);
                ClassicImportQA.Viewport(390, 844); host.EnterMain();
                double deadline = EditorApplication.timeSinceStartup + 30;
                while (UnityEngine.Object.FindFirstObjectByType<ClassicApplication>()?.Ready != true)
                { if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Main initialization"); await Task.Delay(50); }
                var ui = UnityEngine.Object.FindFirstObjectByType<ClassicApplication>().ui;
                ui.driver.enabled = false;
                if (ui.CurrentDialog == "Offline") { ui.Handle("Offline", "close"); await Task.Delay(450); }
                ui.application.enabled = false; ui.Engine.State.reducedMotion = true; ui.Refresh(); ui.SetTab("Shop");
                await Task.Delay(400);
                var identities = ui.GetComponentsInChildren<Component>(true).Select(c => c.GetInstanceID()).OrderBy(i => i).ToArray();
                var controls = ui.GetComponentsInChildren<ClassicControl>(true);
                var cash = controls.Single(c => c.action == "BuyShopCash"); var boost = controls.Single(c => c.action == "BuyShopProfit");
                var cashRow = cash.transform.parent; var boostRow = boost.transform.parent;
                var pack = controls.First(c => c.action == "Purchase").transform;
                while (pack.parent != cashRow.parent) pack = pack.parent;
                Check(cashRow.GetSiblingIndex() < boostRow.GetSiblingIndex() && boostRow.GetSiblingIndex() < pack.GetSiblingIndex(), "Both KK offers precede the IAPs");
                Check(cash.GetComponent<Button>().IsInteractable() && boost.GetComponent<Button>().IsInteractable(), "Affordable offers are enabled");
                var amount = ui.textBindings.Single(t => t.key == "shop-day-cash").text;
                Check(amount.text.Contains(ui.Engine.ShopDayCash.ToString()), "Cash row displays the actual reward");
                var formatter = ui.GetComponent<ClassicCurrencyPresentation>(); formatter.Apply();
                Check(ui.textBindings.Where(t => t.key == "shop-cash-cost" || t.key == "shop-profit-cost").All(t => t.text.text.Contains("<sprite=0>")), "Prices use the selected KK token");
                var scroll = ui.panels.Single(p => p.route == "Shop").root.GetComponentInChildren<ScrollRect>();
                Canvas.ForceUpdateCanvases();
                scroll.verticalNormalizedPosition = Mathf.Clamp01(1 - 145 / Mathf.Max(1, scroll.content.rect.height - scroll.viewport.rect.height));
                await Task.Delay(250);
                ClassicImportQA.Capture(Folder + "/Shop-390x844.png");
                var before = ui.Engine.State.cash; var reward = ui.Engine.ShopDayCash;
                cash.GetComponent<Button>().onClick.Invoke();
                Check(ui.Engine.State.kajuKatli.Value == 300 && ui.Engine.State.cash.CompareTo(before + reward) == 0, "Cash button charges 100 KK and grants the shown amount");
                boost.GetComponent<Button>().onClick.Invoke();
                Check(ui.Engine.ShopProfitActive && ui.Engine.State.kajuKatli.Value == 150 && !boost.GetComponent<Button>().interactable, "Boost button charges 150 KK and prevents active repurchase");
                Check(ui.textBindings.Single(t => t.key == "shop-profit-detail").text.text.Contains("24:00:00"), "Active boost displays a 24-hour countdown");
                Check(store.Load().state.shopProfitUntil == ui.Engine.State.shopProfitUntil, "Boost expiry is durably saved");
                await Task.Delay(250); ClassicImportQA.Capture(Folder + "/Shop-boost-active-390x844.png");
                ClassicImportQA.Viewport(360, 540); await Task.Delay(350); Canvas.ForceUpdateCanvases();
                foreach (var c in new[] { cash, boost })
                {
                    var button = (RectTransform)c.transform; var row = (RectTransform)c.transform.parent;
                    var corners = new Vector3[4]; button.GetWorldCorners(corners);
                    Check(corners.All(p => row.rect.Contains(row.InverseTransformPoint(p))), "Short-phone purchase button stays inside its row: " + c.action);
                }
                ClassicImportQA.Capture(Folder + "/Shop-boost-active-360x540.png");
                Check(identities.SequenceEqual(ui.GetComponentsInChildren<Component>(true).Select(c => c.GetInstanceID()).OrderBy(i => i)), "Purchases and token rendering reuse the saved UI objects");
                Status = "PASS: " + report.Count + " native shop checks";
                report.Add("Disposable profile only: " + root);
            }
            catch (Exception e) { Status = "FAIL: " + e.Message; report.Add(e.ToString()); }
            finally { report.Add(Status); File.WriteAllLines(Folder + "/Validation.txt", report); EditorApplication.ExitPlaymode(); }
        }
    }
}
