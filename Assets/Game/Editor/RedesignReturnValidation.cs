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
    public static class RedesignReturnValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var router = root.GetComponent<RedesignScreenRouter>(); router.upgrades.gameObject.SetActive(false); var ui = router.skillDetails; var offline = router.offline;
                foreach (var component in new MonoBehaviour[] { ui, offline }) component.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);
                var data = RedesignRuntimeContent.Load(); var state = RedesignState.Fresh(1000); state.cash = 100000; state.products[0] = state.products[1] = state.products[2] = 1;
                bool fail = false, owns = true; string route = null, parameter = null;
                var engine = new RedesignEngine(data, state, _ => { if (fail) throw new IOException("QA full disk"); }); var session = new RedesignLifecycle(engine); session.OwnsSession = () => owns;
                var context = new RedesignUiContext(engine, () => session.CanMutate, session.Run, (r, p) => { route = r; parameter = p; }, _ => { });
                ui.Bind(context); offline.Bind(context); ui.Open(0); router.title.text = "Skill details"; router.summary.text = "₹100K cash · Normal sales ₹4.39/s";
                var objects = root.GetComponentsInChildren<Transform>(true);
                Check(!ui.activate.interactable && ui.effect.text.Contains("After buying") && ui.requirement.text.Contains(RedesignUiContext.Cash(engine.UpgradeCost("SK-FR-P-01"))), "Locked skill shows the actual unlock price and an explicit after-purchase effect");
                RedesignNativeUiValidation.Render(root, scene, 390, 844, false, "skill-locked");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "skill-locked-actions", () => ui.scroll.verticalNormalizedPosition = 0);
                ui.unlock.onClick.Invoke(); Check(route == "upgradedetails" && parameter == "SK-FR-P-01", "Locked skill opens its exact catalogue entry");
                for (int i = 0; i < 3; i++)
                {
                    ui.skills[i].onClick.Invoke(); Check(ui.Target == i, "Saved selector opens exact skill " + i);
                    Check(engine.BuyUpgrade(engine.PreviewSkill(i).unlockId), "Fixture purchases actual base skill " + i); ui.Refresh();
                    Check(ui.activate.interactable && ui.status.text.Contains("Ready to activate"), "Newly purchased base skill is ready " + i);
                    var projected = engine.PreviewSkill(i); var cash = engine.State.cash; ui.activate.onClick.Invoke();
                    Check(!ui.activate.interactable && engine.State.readyAt[i] > engine.State.now, "Native activation starts cooldown and disables duplicates " + i);
                    if (i == 2) Check(Math.Abs(Amount.Ratio(engine.State.cash - cash, projected.orderPayout) - 1) < 1e-12, "Native Order awards precisely the engine's preview");
                    RedesignNativeUiValidation.Render(root, scene, 390, 844, i == 1, "skill-" + i + "-active", () => ui.scroll.verticalNormalizedPosition = 1);
                    ui.upgrades.onClick.Invoke(); Check(route == "skillupgrades" && parameter == i.ToString(), "Power/timing link keeps exact skill scope " + i);
                }
                ui.Open(1); string boosted = RedesignUiContext.Cash(engine.TapPayout(true)); engine.Advance(31); ui.Refresh();
                Check(ui.effect.text.Contains(RedesignUiContext.Cash(engine.TapPayout(true))) && boosted != RedesignUiContext.Cash(engine.TapPayout(true)), "Hands text refreshes the actual tap value when Rush expires without a command");
                owns = false; ui.Refresh(); Check(!ui.activate.interactable, "Skill activation respects lost session ownership"); owns = true;
                Check(engine.PinUpgrade("PP-PHU-01") && engine.Save() && engine.ResolveOffline(engine.State.now + 36000), "Offline fixture uses real saved departure production and settlement");
                var receipt = engine.State.offline.Copy(); var creditedCash = engine.State.cash;
                ui.gameObject.SetActive(false); offline.Open(); router.title.text = "Welcome back";
                router.summary.text = "₹" + engine.State.cash + " cash · Normal sales ₹" + engine.NormalIncome() + "/s";
                Check(receipt.creditedSeconds == 28800 && offline.calculation.text.Contains("8 hours") && offline.calculation.text.Contains("2 hours") && offline.calculation.text.Contains("50%"), "Return shows ten hours away, eight credited, two capped and 50 percent efficiency");
                Check(offline.nextLabel.text == "View pinned upgrade" && offline.nextStep.text.Contains("affordable"), "Return offers the actual now-affordable pinned goal");
                Check(offline.skills.text.Contains("Festival Rush: ready") && offline.skills.text.Contains("Helping Hands: ready") && offline.skills.text.Contains("Bulk Order: ready"), "Return reports actual skill readiness after the absence");
                foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                {
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "offline", () => offline.scroll.verticalNormalizedPosition = 1);
                    RedesignNativeUiValidation.Render(root, scene, size.x, size.y, false, "offline-actions", () => offline.scroll.verticalNormalizedPosition = 0);
                }
                fail = true; offline.next.onClick.Invoke(); Check(!session.CanMutate && engine.State.offline.pending && engine.State.cash.CompareTo(creditedCash) == 0, "Failed close retains already-credited cash and pending summary");
                fail = false; Check(session.RetrySave(), "Offline dismissal save can recover"); offline.Refresh(); offline.next.onClick.Invoke();
                Check(route == "upgradedetails" && parameter == "PP-PHU-01" && !engine.State.offline.pending && engine.State.cash.CompareTo(creditedCash) == 0, "Pinned-goal action acknowledges without claiming cash again");
                offline.close.onClick.Invoke(); Check(engine.State.cash.CompareTo(creditedCash) == 0, "Duplicate close neither loses nor duplicates income");
                var empty = new RedesignEngine(data, RedesignState.Fresh(), _ => { });
                offline.Bind(new RedesignUiContext(empty, () => true, f => f(empty), (r, p) => { route = r; parameter = p; }, _ => { })); offline.Open();
                router.summary.text = "₹30 cash · Normal sales ₹0/s";
                Check(offline.amount.text.Contains("+₹0") && offline.nextLabel.text == "Buy your first Phuljhari", "Zero-income return explains the manual first purchase without inventing earnings");
                RedesignNativeUiValidation.Render(root, scene, 360, 540, false, "offline-empty-actions", () => offline.scroll.verticalNormalizedPosition = 0);
                offline.next.onClick.Invoke(); Check(route == "productdetails" && parameter == "0" && empty.State.cash.CompareTo(new Amount(30)) == 0, "Zero-income next step reaches the first product without buying or claiming");
                Check(new HashSet<Transform>(objects).SetEquals(root.GetComponentsInChildren<Transform>(true)), "Skill/return interactions reuse every saved UI object");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
            }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
            File.WriteAllLines("Documentation/Implementation/Unity-Return-Skills-UI.txt", new[] { "Scope: saved native skill/return component binding and rendering with actual isolated engine fixtures. Main/device integration pending." }.Concat(checks));
            Debug.Log("REDESIGN_RETURN_UI_OK " + checks.Count);
        }
    }
}
