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
    public static class RedesignLevelsValidation
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
                var router = root.GetComponent<RedesignScreenRouter>(); router.upgrades.gameObject.SetActive(false);
                var state = RedesignState.Fresh(); state.cash = 2000; state.products[0] = 24;
                var engine = new RedesignEngine(RedesignRuntimeContent.Load(), state, _ => { }); bool allowed = true;
                var context = new RedesignUiContext(engine, () => allowed, action => action(engine), (_, __) => { }, _ => { });
                router.summary.text = "₹" + engine.State.cash + " cash · Normal sales ₹" + engine.NormalIncome() + "/s";
                foreach (var ui in new[] { router.productDetails, router.tapTraining })
                {
                    ui.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null); ui.Bind(context);
                    Check(ui.quantities.Length == 4 && ui.productSprites.All(s => s && EditorUtility.IsPersistent(s)), "Quantity controls and eight persistent product sprites reload on " + ui.name);
                }
                var product = router.productDetails; var tap = router.tapTraining;
                product.Open(0); router.title.text = "Product details";
                Check(product.projection.text.Contains("level 25") && product.DisplayedProjection.afterLevel == 25, "Product detail shows the next crossed income milestone");
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                {
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "product-details", () => product.scroll.verticalNormalizedPosition = 1);
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "product-actions", () => product.scroll.verticalNormalizedPosition = 0);
                }
                product.Open(7); RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "product-locked", () => product.scroll.verticalNormalizedPosition = 1);
                Check(!product.buy.interactable && product.requirement.gameObject.activeSelf, "Locked product offers navigation instead of purchase");
                product.gameObject.SetActive(false); tap.Open(-1); router.title.text = "Tap training";
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                {
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "tap-training", () => tap.scroll.verticalNormalizedPosition = 1);
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "tap-actions", () => tap.scroll.verticalNormalizedPosition = 0);
                }
                allowed = false; tap.Refresh(); Check(!tap.buy.interactable, "Blocked session disables level mutations"); allowed = true;
                tap.exact.onClick.Invoke(); RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "tap-exact", () => tap.scroll.verticalNormalizedPosition = 1);
                Check(tap.projection.text.Contains("10^"), "Tap expansion uses stored before/after magnitudes");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
            }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
            File.WriteAllLines("Documentation/Implementation/Unity-Levels-UI.txt", new[] { "Scope: isolated saved asset/Edit Mode render validation. Main/device acceptance remains open." }.Concat(checks));
            Debug.Log("REDESIGN_LEVELS_UI_OK " + checks.Count);
        }
    }
}
