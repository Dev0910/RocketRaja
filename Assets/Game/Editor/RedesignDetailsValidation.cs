using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignDetailsValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var checks = new List<string>();
            void Check(bool ok, string text) { if (!ok) throw new InvalidOperationException(text); checks.Add("PASS " + text); }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var router = root.GetComponent<RedesignScreenRouter>(); var details = router.details; var batch = router.batch;
                foreach (var component in new MonoBehaviour[] { details, details.goals, batch })
                    component.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);
                var state = RedesignState.Fresh(); state.cash = 2000; state.products[0] = 1;
                var engine = new RedesignEngine(RedesignRuntimeContent.Load(), state, _ => { });
                router.summary.text = "₹" + state.cash + " cash · Normal sales ₹" + engine.NormalIncome() + "/s";
                string route = "", argument = "";
                var context = new RedesignUiContext(engine, () => true, action => action(engine), (r, a) => { route = r; argument = a; }, _ => { });
                details.Bind(context); batch.Bind(context); router.upgrades.gameObject.SetActive(false);
                Check(root.GetComponentsInChildren<TMP_Text>(true).All(t => t is RedesignSavedText saved && saved.savedMaterialSlots.Length == 4 && saved.savedMaterialSlots.All(s => s)), "All text has four saved material slots");
                Check(router.upgrades.search is RedesignSearchInput search && search.savedCaret, "Search has a saved caret");
                details.Open("PP-PHU-01"); router.title.text = "Upgrade details";
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                {
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "upgrade-details", () => details.scroll.verticalNormalizedPosition = 1);
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "upgrade-details-actions", () => details.scroll.verticalNormalizedPosition = 0);
                    Check(details.scroll.content.rect.height > details.scroll.viewport.rect.height, "Detail content scrolls at " + size);
                }
                details.pin.onClick.Invoke(); details.alternatives.onClick.Invoke();
                RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "upgrade-goals", () => details.scroll.verticalNormalizedPosition = 0);
                details.goals.openPinned.onClick.Invoke(); Check(route == "upgradedetails" && argument == "PP-PHU-01", "Pinned goal uses its stable ID");
                details.exact.onClick.Invoke();
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "upgrade-exact", () => details.scroll.verticalNormalizedPosition = 1);
                details.gameObject.SetActive(false); router.title.text = "Review Buy All";
                batch.Open(engine.QuoteBuyAll(new UpgradeScope { category = UpgradeCategory.Products, product = 0 }));
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                {
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "buyall");
                    Check(batch.scroll.viewport.rect.height > 0, "Buy All retains positive list viewport at " + size);
                }
                batch.exact.onClick.Invoke(); RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "buyall-exact");
                Check(batch.summary.text.Contains("10^"), "Batch precision expands the actual frozen quote");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
            }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
            File.WriteAllLines("Documentation/Implementation/Unity-Details-UI.txt", new[] { "Scope: saved asset and isolated Edit Mode rendering; Play Mode/Main/device acceptance tracked separately." }.Concat(checks));
            Debug.Log("REDESIGN_DETAILS_UI_OK " + checks.Count);
        }
    }
}
