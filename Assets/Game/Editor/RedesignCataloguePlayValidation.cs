using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    [InitializeOnLoad]
    public static class RedesignCataloguePlayValidation
    {
        const string Key = "RocketRaja.RedesignCatalogueQA";
        const string Report = "Documentation/Implementation/Unity-Catalogue-Play.txt";
        [Serializable] sealed class SceneRecord { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public SceneRecord[] scenes; }
        static RedesignScreenRouter router;
        static readonly List<string> checks = new List<string>();
        static readonly List<string> errors = new List<string>();
        static Transform[] instances;
        static int step;
        static int boundFrame;
        static double warmupDeadline;
        static double deadline;
        static Amount cashBefore;
        static CashBatchQuote reviewed;
        static string[] frozenIds;
        // TMP renames a saved submesh when its material changes; compare its slot type/count at startup.
        // The later navigation check compares actual object references, not these normalized names.
        static string ObjectPath(Transform t) => t ? (t.parent ? ObjectPath(t.parent) + "/" : "") + (t.GetComponent<TMPro.TMP_SubMeshUI>() ? "Material slot" : t.name) : "destroyed";
        internal static string Inventory(GameObject root) => string.Join("\n", root.GetComponentsInChildren<Transform>(true)
            .SelectMany(t => t.GetComponents<Component>().Select(c => ObjectPath(t) + "|" + (c ? c.GetType().FullName : "MISSING"))).OrderBy(s => s));
        static RedesignCataloguePlayValidation() => EditorApplication.playModeStateChanged += Mode;
        [MenuItem("Rocket Raja/Redesign/Test Catalogue In Play Mode")]
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            foreach (var scene in setup) if (scene.isLoaded && SceneManager.GetSceneByPath(scene.path).isDirty) throw new InvalidOperationException("Preserve the dirty scene before running UI QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = setup.Select(s => new SceneRecord { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            string root = Path.GetFullPath("Temp/RedesignCatalogueQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc); state.cash = 2000; state.products[0] = 1;
            var store = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            store.Save(state); SessionState.SetString(Key + ".profile", root); SessionState.SetBool(Key + ".running", true);
            File.WriteAllText(Report, "RUNNING: isolated saved native preview, disposable profile.\n");
            SessionState.SetString(Key + ".inventory", Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key + ".running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); errors.Clear(); Application.logMessageReceived -= Log; Application.logMessageReceived += Log;
                step = -1; deadline = EditorApplication.timeSinceStartup + .5;
                EditorApplication.update -= Tick; EditorApplication.update += Tick;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick;
                var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", ""));
                if (setup != null) EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
                SessionState.SetBool(Key + ".running", false);
                Debug.Log("REDESIGN_CATALOGUE_QA_RESTORED");
            }
        }
        static void Check(bool ok, string text) { if (!ok) throw new InvalidOperationException(text); checks.Add("PASS " + text); }
        static void Log(string message, string trace, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message); }
        static void RaycastClick(Button button)
        {
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button, "Visible control receives the topmost UI raycast: " + button.name + "; screen=" + Screen.width + "x" + Screen.height + "; point=" + pointer.position + "; hits=" + string.Join(",", hits.Take(4).Select(h => ObjectPath(h.gameObject.transform))));
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        static void Click(Button button)
        {
            Check(button && button.IsActive() && button.IsInteractable(), "Active/interactable control: " + (button ? button.name : "missing"));
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                // Editor update callbacks can run before the player has rendered its first layout.
                if ((step == 0 || step == 4) && Time.frameCount <= boundFrame + 1)
                {
                    if (EditorApplication.timeSinceStartup > warmupDeadline) throw new InvalidOperationException("Player frames did not advance for raycast QA");
                    return;
                }
                switch (step++)
                {
                    case -1:
                        Check(SceneManager.GetActiveScene().path == BuildRedesignUI.PreviewPath, "Only the saved native preview is under test");
                        router = UnityEngine.Object.FindAnyObjectByType<RedesignScreenRouter>();
                        var beforeInventory = SessionState.GetString(Key + ".inventory", "").Split('\n');
                        var afterInventory = Inventory(router.gameObject).Split('\n');
                        Check(beforeInventory.SequenceEqual(afterInventory), "Play startup preserves every authored UI object and component, including text slots and caret; expected=" + beforeInventory.Length + "; actual=" + afterInventory.Length + "; added=" + string.Join(",", afterInventory.Except(beforeInventory).Take(10)) + "; removed=" + string.Join(",", beforeInventory.Except(afterInventory).Take(10)));
                        Check(router && !router.initializeGuestOnStart, "Preview does not open a normal guest profile automatically");
                        Check(router.driver.InitializeGuest(SessionState.GetString(Key + ".profile", "")), "Actual driver loads and settles a disposable saved profile");
                        router.Bind(router.driver.Session); router.Bind(router.driver.Session); router.Bind(router.driver.Session);
                        instances = router.GetComponentsInChildren<Transform>(true);
                        boundFrame = Time.frameCount; warmupDeadline = EditorApplication.timeSinceStartup + 15;
                        deadline = EditorApplication.timeSinceStartup + .1; break;
                    case 0:
                        RaycastClick(router.upgradesTab);
                        RaycastClick(router.upgrades.categories[1]);
                        Check(router.upgrades.Browser.Scope.category == UpgradeCategory.Products, "Raycast dispatch changes the actual category");
                        router.upgrades.search.ActivateInputField(); router.upgrades.search.text = "PP-PHU-01";
                        Check(router.upgrades.Browser.Count == 1, "Native input callback filters while using the saved caret");
                        router.upgrades.search.DeactivateInputField(); router.upgrades.search.text = "";
                        router.Navigate("upgradedetails", "PP-PHU-01");
                        Check(router.home.productImages[0].sprite == router.home.productArt.At(0, 0), "Owned family starts with its saved basic display before the first signature");
                        Check(router.details.projection.text.Contains("₹0.85 → ₹1.7/s"), "Live upgrade details show engine before/after income");
                        Click(router.details.pin); Check(router.Session.Engine.State.pinnedUpgradeId == "PP-PHU-01", "Repeated binding leaves one pin listener");
                        cashBefore = router.Session.Engine.State.cash; deadline = EditorApplication.timeSinceStartup + .8; break;
                    case 1:
                        Check(router.Session.Engine.State.cash > cashBefore, "Actual driver continues production while reading details");
                        Check(!router.driver.TapGameplaySurface(), "Details route blocks gameplay taps without suspending income");
                        Click(router.details.exact); Check(router.details.price.text.Contains("10^"), "Full-precision action exposes stored price magnitude");
                        Click(router.details.alternatives); Check(router.details.goals.gameObject.activeSelf && router.details.goals.alternatives.All(b => b.gameObject.activeSelf), "Goal alternatives reuse three saved controls");
                        Click(router.details.goals.back); Check(!router.details.goals.gameObject.activeSelf, "Hiding goal alternatives keeps the detail page");
                        Click(router.details.buy);
                        Check(router.Session.Engine.State.upgrades.Contains("PP-PHU-01") && router.Session.Engine.State.pinnedUpgradeId == "", "Live detail purchase applies once and clears completed goal");
                        router.home.Refresh();
                        Check(router.home.productImages[0].sprite == router.home.productArt.At(0, 1), "Live signature purchase immediately selects the saved Signature I shelf art");
                        Check(router.toastRoot.activeSelf && router.toast.text == "New cash upgrade purchased · +1 unique" && router.toastDismiss.IsActive(), "Purchase feedback comes from one listener and a dismissible saved toast");
                        boundFrame = Time.frameCount; warmupDeadline = EditorApplication.timeSinceStartup + 15;
                        step = 4; deadline = EditorApplication.timeSinceStartup + .1; break;
                    case 4:
                        var beforeDismiss = router.Session.Engine.State.commandRevision; var tapsBeforeDismiss = router.Session.Engine.State.manualTaps;
                        RaycastClick(router.toastDismiss);
                        Check(!router.toastRoot.activeSelf && router.Session.Engine.State.commandRevision.CompareTo(beforeDismiss) == 0 && router.Session.Engine.State.manualTaps.CompareTo(tapsBeforeDismiss) == 0, "Physical notice dismissal neither mutates the economy nor leaks a gameplay tap");
                        router.Navigate("upgradedetails", "PP-PHU-03"); Click(router.details.nextRequirement);
                        Check(router.details.SelectedId == "PP-PHU-02", "Missing prerequisite navigation opens the correct detail");
                        router.Navigate("back", ""); Check(router.details.SelectedId == "PP-PHU-03", "Back restores the previous detail identity");
                        router.Navigate("upgrades", ""); router.upgrades.FilterProduct(0);
                        Click(router.upgrades.reviewButton); reviewed = router.batch.ReviewedQuote; frozenIds = reviewed.Ids.ToArray();
                        Check(router.CurrentRoute == "buyall" && frozenIds.Length > 0, "Native catalogue opens actual frozen Buy All review");
                        cashBefore = router.Session.Engine.State.cash; step = 2; deadline = EditorApplication.timeSinceStartup + .8; break;
                    case 2:
                        Check(router.Session.Engine.State.cash > cashBefore && frozenIds.SequenceEqual(router.batch.ReviewedQuote.Ids), "Review earns passive cash without growing its quoted list");
                        Click(router.batch.confirm); Check(frozenIds.All(router.Session.Engine.State.upgrades.Contains) && router.CurrentRoute == "upgrades", "One live confirmation applies the reviewed entries and returns to catalogue; route=" + router.CurrentRoute + "; missing=" + string.Join(",", frozenIds.Where(id => !router.Session.Engine.State.upgrades.Contains(id))) + "; notice=" + router.toast.text + "; saving=" + router.Session.SaveFailure + "; error=" + router.Session.LastSaveException);
                        Check(!router.Session.Engine.CommitBuyAll(reviewed), "Late repeated confirmation cannot grant or charge twice");
                        router.upgrades.SetCategory(UpgradeCategory.All); Click(router.upgrades.reviewButton);
                        Check(router.batch.ReviewedQuote != null, "Second review can be opened after completion");
                        router.Session.Run(e => e.BuyLevels(-1, 1)); router.batch.Refresh();
                        Check(!router.batch.confirm.interactable && router.batch.status.text.Contains("Review again"), "Changed economy disables a stale native confirmation");
                        Click(router.batch.cancel); Check(router.CurrentRoute == "upgrades", "Cancel returns to the retained catalogue");
                        router.upgrades.SetView(UpgradeView.Full); router.upgrades.content.anchoredPosition = new Vector2(0, 2000);
                        router.upgrades.Refresh(); router.Navigate("upgradedetails", "PP-PHU-04"); router.Navigate("back", "");
                        Check(router.upgrades.Browser.View == UpgradeView.Full && Math.Abs(router.upgrades.content.anchoredPosition.y - 2000) < .1f, "Detail/back retains catalogue view and scroll offset");
                        var after = router.GetComponentsInChildren<Transform>(true);
                        var added = after.Except(instances).Select(ObjectPath); var removed = instances.Except(after).Select(ObjectPath);
                        Check(new HashSet<Transform>(instances).SetEquals(after), "Real Play Mode navigation and actions preserve all saved UI objects; added=" + string.Join(",", added) + "; removed=" + string.Join(",", removed));
                        router.driver.SendMessage("OnApplicationPause", true); cashBefore = router.Session.Engine.State.cash;
                        deadline = EditorApplication.timeSinceStartup + .6; break;
                    case 3:
                        Check(router.Session.Suspended && router.Session.Engine.State.cash.CompareTo(cashBefore) == 0, "Driver pause callback suspends accrual");
                        router.driver.SendMessage("OnApplicationPause", false);
                        Check(!router.Session.Suspended && router.Session.CanMutate, "Driver resume callback restores the session after settlement");
                        router.Navigate("taptraining", "");
                        int beforeTraining = router.Session.Engine.State.tapLevel; Click(router.tapTraining.buy);
                        Check(router.Session.Engine.State.tapLevel == beforeTraining + 1, "Native tap-training action durably buys one level");
                        router.Navigate("upgradedetails", "PP-ANA-01"); Click(router.details.nextRequirement);
                        Check(router.CurrentRoute == "productdetails" && router.productDetails.Target == 1, "Missing product requirement reaches its live product page");
                        router.Navigate("productdetails", "0"); Click(router.productDetails.quantities[1]);
                        Check(router.productDetails.productImage.sprite == router.home.productImages[0].sprite && router.productDetails.appearance.text.Contains(RedesignProductArt.Label(router.Session.Engine.ProductAppearance(0))), "Actual product route agrees with the current shelf appearance after native purchases");
                        Check(router.productDetails.SelectedQuantity == 1 && router.productDetails.DisplayedProjection.quote.quantity == 10, "Native x10 selection displays ten product levels");
                        Click(router.productDetails.quantities[3]);
                        var levelQuote = router.productDetails.DisplayedProjection;
                        Check(levelQuote.quote.affordable && levelQuote.quote.quantity > 0, "Native Max selection uses actual affordability");
                        Click(router.productDetails.buy);
                        Check(router.Session.Engine.State.products[0] == levelQuote.afterLevel, "Native level purchase buys exactly the displayed Max count");
                        router.Navigate("productdetails", "7");
                        Check(!router.productDetails.buy.interactable && router.productDetails.requirement.gameObject.activeSelf, "Locked product disables buying and exposes its predecessor");
                        Click(router.productDetails.requirement); Check(router.productDetails.Target == 6, "Locked-product navigation follows the correct predecessor");
                        router.Navigate("upgradedetails", "PP-PHU-04"); Click(router.details.alternatives); Click(router.details.goals.alternatives[2]);
                        Check(router.CurrentRoute == "taptraining" && router.tapTraining.Target == -1, "Goal alternative reaches actual tap training");
                        Click(router.tapTraining.quantities[2]); Check(router.tapTraining.DisplayedProjection.quote.quantity == 100, "Native x100 tap selection uses the actual quote");
                        Click(router.tapTraining.quantities[0]);
                        bool canBuyTap = router.tapTraining.buy.interactable;
                        if (canBuyTap)
                        {
                            int tapLevel = router.Session.Engine.State.tapLevel; Click(router.tapTraining.buy);
                            Check(router.Session.Engine.State.tapLevel == tapLevel + 1, "Native tap-training purchase applies one level");
                        }
                        else Check(!router.Session.Engine.LevelQuote(-1, 1).affordable, "Unaffordable tap training agrees with the actual engine quote");
                        Click(router.tapTraining.upgrades);
                        Check(router.CurrentRoute == "upgrades" && router.upgrades.Browser.Scope.category == UpgradeCategory.Tapping, "Tap training links to the matching upgrade category");
                        Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Level and goal routes preserve the same authored object instances");
                        Check(errors.Count == 0, "No Unity error, exception or assertion during the interaction run: " + string.Join("; ", errors.Take(5)));
                        File.WriteAllLines(Report, new[] { "Scope: isolated Play Mode saved native preview and disposable profile; not Main/device acceptance." }.Concat(checks));
                        Debug.Log("REDESIGN_CATALOGUE_PLAY_OK " + checks.Count); Finish(); break;
                }
            }
            catch (Exception error)
            {
                File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + error })); Debug.LogError("REDESIGN_CATALOGUE_PLAY_FAIL " + error); Finish();
            }
        }
        static void Finish() { Application.logMessageReceived -= Log; EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); }
    }
}
