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
    public static class RedesignHomeValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var router = root.GetComponent<RedesignScreenRouter>(); router.upgrades.gameObject.SetActive(false); var home = router.home;
                typeof(RedesignHomeScreen).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(home, null);
                var data = RedesignRuntimeContent.Load(); var engine = new RedesignEngine(data, RedesignState.Fresh(), _ => { }); var session = new RedesignLifecycle(engine);
                string route = null, argument = null; var context = new RedesignUiContext(engine, () => session.CanMutate, session.Run, (r, a) => { route = r; argument = a; }, _ => { });
                home.Bind(context, session.TapGameplaySurface); home.Bind(context, session.TapGameplaySurface); home.Open(); router.title.text = "Rocket Raja"; router.summary.text = "₹30 cash · Normal sales ₹0/s";
                var objects = root.GetComponentsInChildren<Transform>(true);
                Check(engine.State.products.All(p => p == 0) && home.firstBuy.interactable && home.firstBuyLabel.text.Contains("₹30"), "Fresh Home keeps the exact opening grant and requires a manual purchase");
                Check(home.productStatus[0].text == "Empty" && home.productStatus.Skip(1).All(t => t.text == "Locked"), "Fresh shelves distinguish empty first product from locked families");
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540), new Vector2Int(360, 640), new Vector2Int(450, 916) })
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "home-fresh", () => home.scroll.verticalNormalizedPosition = 1);
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "home-actions", () => home.scroll.verticalNormalizedPosition = 0);
                home.firstBuy.onClick.Invoke(); home.firstBuy.onClick.Invoke();
                Check(engine.State.products[0] == 1 && engine.State.cash.CompareTo(Amount.Zero) == 0 && !home.firstBuy.gameObject.activeSelf, "Native first purchase spends the grant once even after repeated binding/callback");
                Check(home.productStatus[0].text == "Lv 1" && home.productStatus[1].text == "Empty", "Manual buying populates the correct shelf and exposes the next family");
                home.counter.Tap(); home.counter.Tap();
                Check(engine.State.manualTaps.Value == 1, "Counter cannot bypass the engine's one-third-second ceiling");
                engine.Advance(3600); home.counter.Tap(); home.counter.Tap(); Check(engine.State.manualTaps.Value == 2, "A long idle gap does not bank extra counter taps");
                session.GameplayInputEnabled = false; home.counter.Tap(); Check(engine.State.manualTaps.Value == 2, "Session gameplay gate blocks a late counter callback"); session.GameplayInputEnabled = true;
                for (int i = 0; i < 8; i++) { home.products[i].onClick.Invoke(); Check(route == "productdetails" && argument == i.ToString(), "Saved shelf routes by exact product ID " + i); }
                for (int i = 0; i < 3; i++) { home.skills[i].onClick.Invoke(); Check(route == "skilldetails" && argument == i.ToString(), "Saved skill routes by exact skill ID " + i); }
                home.training.onClick.Invoke(); Check(route == "taptraining", "Home exposes real tap training"); home.wallet.onClick.Invoke(); Check(route == "kajushop" && !engine.State.HasDevelopmentShop, "Home wallet opens the test shop without granting or marking a purchase");
                engine.PinUpgrade("PP-PHU-01"); home.Refresh(); home.goal.onClick.Invoke(); Check(route == "upgradedetails" && argument == "PP-PHU-01", "Home preserves the actual pinned goal destination");
                var progressed = RedesignState.Fresh(); progressed.cash = Amount.Pow10(30); progressed.products = Enumerable.Repeat(25, 8).ToArray(); progressed.eligibleEarnings = progressed.target;
                progressed.storeName = "Raja's bright Diwali fireworks";
                engine = new RedesignEngine(data, progressed, _ => { }); session = new RedesignLifecycle(engine);
                home.Bind(new RedesignUiContext(engine, () => session.CanMutate, session.Run, (r, a) => { route = r; argument = a; }, _ => { }), session.TapGameplaySurface); home.Open();
                router.summary.text = "₹" + engine.State.cash + " cash · Normal sales ₹" + engine.NormalIncome() + "/s";
                Check(home.tapGroups.All(g => g.alpha == 0), "Rebinding a different profile clears previous tap feedback");
                Check(home.productStatus.All(t => t.text == "Lv 25") && home.goalLabel.text == "Review Prestige", "Progressed Home binds all eight levels and earned-target prestige readiness");
                RedesignNativeUiValidation.Render(root, scene, 390, 844, true, "home-progressed", () => home.scroll.verticalNormalizedPosition = 1);
                Check(new HashSet<Transform>(objects).SetEquals(root.GetComponentsInChildren<Transform>(true)), "Home and six tap feedback slots preserve every saved UI object");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
            }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
            File.WriteAllLines("Documentation/Implementation/Unity-Home-UI.txt", new[] { "Scope: isolated saved Home rendering/binding; actual pointer events and Main/device acceptance are separate." }.Concat(checks)); Debug.Log("REDESIGN_HOME_UI_OK " + checks.Count);
        }
    }
}
