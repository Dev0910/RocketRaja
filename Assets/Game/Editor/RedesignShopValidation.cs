using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignShopValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var router = root.GetComponent<RedesignScreenRouter>(); router.upgrades.gameObject.SetActive(false); var ui = router.shop;
                typeof(RedesignShopScreen).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);
                var data = RedesignRuntimeContent.Load(); var original = RedesignState.Fresh(); original.completedPrestiges = original.fame = 1; original.target = ProgressionNumbers.RunTarget(original.completedPrestiges);
                bool owns = true, saveFails = false; string route = null;
                var engine = new RedesignEngine(data, original, _ => { if (saveFails) throw new IOException("QA disk full"); }); var session = new RedesignLifecycle(engine);
                session.OwnsSession = () => owns;
                var context = new RedesignUiContext(engine, () => session.CanMutate, session.Run, (r, _) => route = r, _ => { });
                ui.Bind(context, true, session.RetrySave); ui.Bind(context, true, session.RetrySave); ui.Open();
                router.title.text = "Kaju Katli shop"; router.summary.text = "₹30 cash · Normal sales ₹0/s";
                var instances = root.GetComponentsInChildren<Transform>(true);
                Check(!engine.State.HasDevelopmentShop && ui.packs.All(p => p.interactable), "Browsing does not mark or grant; disposable guest may begin a test purchase");
                int[] amounts = { 1000, 6000, 13000, 28000 }, prices = { 99, 499, 999, 1999 };
                for (int i = 0; i < 4; i++)
                {
                    Check(data.Packs[i].kajuKatli == amounts[i] && data.Packs[i].designInr == prices[i] && ui.packNames[i].text == data.Packs[i].name && ui.packPrices[i].text.Contains("Example price ₹") && ui.packPrices[i].text.Contains("not charged"), "Native pack identity, quantity and example price: " + data.Packs[i].id);
                }
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540), new Vector2Int(360, 640), new Vector2Int(450, 916) })
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "shop", () => ui.scroll.verticalNormalizedPosition = 1);
                RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "shop-bottom", () => ui.scroll.verticalNormalizedPosition = 0);
                ui.testStates.onClick.Invoke(); ui.loading.onClick.Invoke();
                Check(ui.Availability == RedesignShopScreen.TestAvailability.Loading && ui.packs.All(p => !p.interactable), "Loading simulation disables every pack");
                ui.packs[0].onClick.Invoke(); Check(!engine.State.HasDevelopmentShop, "Late loading-state click cannot begin a transaction");
                ui.unavailable.onClick.Invoke(); Check(ui.packs.All(p => !p.interactable), "Unavailable simulation disables every pack");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "shop-unavailable", () => ui.scroll.verticalNormalizedPosition = 1);
                ui.ready.onClick.Invoke(); ui.testStates.onClick.Invoke();
                ulong total = 0;
                for (int i = 0; i < 4; i++)
                {
                    ui.packs[i].onClick.Invoke(); string operation = ui.PendingOperation;
                    Check(engine.State.HasDevelopmentShop && engine.State.developmentShop.status == "pending" && engine.State.kajuKatli.Value == total && ui.pendingPanel.activeSelf && ui.packs.All(p => !p.interactable), "Pack begins one saved pending operation without grant: " + data.Packs[i].id);
                    ui.packs[i].onClick.Invoke(); Check(ui.PendingOperation == operation, "Repeated pending pack action retains operation: " + i);
                    if (i == 0)
                    {
                        RedesignNativeUiValidation.Render(root, scene, 390, 844, false, "shop-pending", () => ui.scroll.verticalNormalizedPosition = 1);
                        RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "shop-pending-short", () => ui.scroll.verticalNormalizedPosition = 1);
                        RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "shop-pending-actions", () => ui.scroll.content.anchoredPosition = new Vector2(0, 180 * 1080f / 390));
                        ui.leavePending.onClick.Invoke(); Check(route == "talent" && engine.State.developmentShop.operationId == operation, "Leave pending navigates without delivery or cancellation");
                        owns = false; ui.Refresh(); ui.delivered.onClick.Invoke();
                        Check(!ui.delivered.interactable && engine.State.kajuKatli.IsZero, "Lost ownership blocks a stale simulated success callback"); owns = true; ui.Refresh();
                        saveFails = true; ui.delivered.onClick.Invoke();
                        Check(!session.CanMutate && engine.State.kajuKatli.IsZero && engine.State.developmentShop.status == "pending" && ui.retry.gameObject.activeSelf, "Failed persistence publishes no grant and exposes saving recovery");
                        saveFails = false; ui.retry.onClick.Invoke(); Check(session.CanMutate && ui.PendingOperation == operation, "Retry saving retains the same pending operation");
                    }
                    ui.delivered.onClick.Invoke(); total += (ulong)amounts[i];
                    Check(engine.State.kajuKatli.Value == total && engine.State.developmentShop.status == "delivered" && !ui.pendingPanel.activeSelf, "Success delivers exactly the chosen pack: " + data.Packs[i].id);
                    ui.delivered.onClick.Invoke(); Check(engine.State.kajuKatli.Value == total, "Duplicate success cannot grant again: " + i);
                }
                Check(engine.State.cardLevels.All(n => n.IsZero) && engine.State.freeCredits.IsZero && engine.State.eligibleEarnings.CompareTo(Amount.Zero) == 0 && engine.State.fame.Value == 1, "Pack delivery never spends KK, draws cards or changes cash progression");
                ui.packs[0].onClick.Invoke(); ui.cancelled.onClick.Invoke(); Check(engine.State.kajuKatli.Value == total && engine.State.developmentShop.status == "cancelled", "Cancellation delivers nothing");
                ui.packs[1].onClick.Invoke(); ui.failed.onClick.Invoke(); Check(engine.State.kajuKatli.Value == total && engine.State.developmentShop.status == "failed", "Failure delivers nothing and permits a fresh pack");
                RedesignNativeUiValidation.Render(root, scene, 390, 844, false, "shop-failed", () => ui.scroll.verticalNormalizedPosition = 1);
                ui.Bind(context, false, session.RetrySave); ui.packs[0].onClick.Invoke();
                Check(ui.packs.All(p => !p.interactable) && engine.State.developmentShop.status == "failed", "Non-disposable or account binding cannot run test purchases");
                Check(new HashSet<Transform>(instances).SetEquals(root.GetComponentsInChildren<Transform>(true)), "All shop states reuse the saved UI objects");
                Check(!EditorUtility.scriptCompilationFailed, "Unity scripts compile");
            }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
            File.WriteAllLines("Documentation/Implementation/Unity-Shop-UI.txt", new[] { "Scope: isolated saved-native shop binding/rendering. Simulated purchases only; actual Play Mode, Main/device evidence is separate." }.Concat(checks));
            Debug.Log("REDESIGN_SHOP_UI_OK " + checks.Count);
        }
    }
}
