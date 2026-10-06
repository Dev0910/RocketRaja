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
    /// <summary>Two real Play Mode sessions share one disposable saved profile to test reveal recovery.</summary>
    [InitializeOnLoad]
    public static class RedesignTalentPlayValidation
    {
        const string Key = "RocketRaja.RedesignTalentQA", Report = "Documentation/Implementation/Unity-Talent-Play.txt";
        [Serializable] sealed class Record { public string path; public bool loaded, active; }
        [Serializable] sealed class Setup { public Record[] scenes; }
        [Serializable] sealed class SavedChecks { public string[] lines; }
        static readonly List<string> checks = new List<string>(), errors = new List<string>(), tooling = new List<string>();
        static RedesignScreenRouter router;
        static Transform[] instances;
        static int step;
        static double deadline;
        static FameQuote reviewed;
        static Amount beforeCash;
        static RedesignTalentPlayValidation()
        {
            EditorApplication.playModeStateChanged += Mode;
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (!SessionState.GetBool(Key + ".running", false) || type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
                // Verified in the installed MCP package Startup.cs; does not diagnose game code.
                const string pathAdvisory = "The project path contains spaces, which may cause issues during usage of AI Game Developer. Please consider the move the project to a folder without spaces.";
                if (message == pathAdvisory && trace.Contains("Startup")) tooling.Add(message);
                else errors.Add(message);
            };
        }
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Preserve dirty scenes before QA");
            SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Setup { scenes = setup.Select(s => new Record { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            string profile = Path.GetFullPath("Temp/RedesignTalentQA-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(profile);
            SessionState.SetString(Key + ".profile", profile);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc); state.cash = 2000; state.products[0] = 1; state.eligibleEarnings = state.target; state.lifetimeEarnings = state.target;
            Store().Save(state);
            SessionState.SetString(Key + ".inventory", RedesignCataloguePlayValidation.Inventory(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath)));
            SessionState.SetInt(Key + ".phase", 0); SessionState.SetString(Key + ".checks", ""); SessionState.SetBool(Key + ".restart", false); SessionState.SetBool(Key + ".running", true);
            File.WriteAllText(Report, "RUNNING: two Play Mode sessions, one disposable saved native profile.\n");
            EditorSceneManager.OpenScene(BuildRedesignUI.PreviewPath, OpenSceneMode.Single); EditorApplication.EnterPlaymode();
        }
        static RedesignSaveStore Store() => new RedesignSaveStore(Path.Combine(SessionState.GetString(Key + ".profile", ""), "rocket-raja-v2.save"), RedesignRuntimeContent.Load(), JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
        static void Mode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key + ".running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); var saved = JsonUtility.FromJson<SavedChecks>(SessionState.GetString(Key + ".checks", "{}"));
                if (saved?.lines != null) checks.AddRange(saved.lines);
                step = 0; deadline = EditorApplication.timeSinceStartup + .5;
                EditorApplication.update -= Tick; EditorApplication.update += Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick;
                if (SessionState.GetBool(Key + ".restart", false))
                {
                    SessionState.SetBool(Key + ".restart", false); SessionState.SetInt(Key + ".phase", 1);
                    EditorApplication.delayCall += () => EditorApplication.EnterPlaymode();
                }
                else
                {
                    SessionState.SetBool(Key + ".running", false);
                    var setup = JsonUtility.FromJson<Setup>(SessionState.GetString(Key + ".scenes", "{}"));
                    EditorSceneManager.RestoreSceneManagerSetup(setup.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
                    Debug.Log("REDESIGN_TALENT_QA_RESTORED");
                }
            }
        }
        static void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
        static void Click(Button button)
        {
            Check(button && button.IsActive() && button.IsInteractable(), "Active control: " + (button ? button.name : "missing"));
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        static void Bind()
        {
            router = UnityEngine.Object.FindAnyObjectByType<RedesignScreenRouter>();
            Check(router && !router.initializeGuestOnStart && SceneManager.GetActiveScene().path == BuildRedesignUI.PreviewPath, "Saved preview uses only the explicit QA profile");
            Check(RedesignCataloguePlayValidation.Inventory(router.gameObject) == SessionState.GetString(Key + ".inventory", ""), "Startup contains exactly the saved UI component inventory");
            Check(router.driver.InitializeGuest(SessionState.GetString(Key + ".profile", "")), "Actual driver loads the disposable profile");
            router.Bind(router.driver.Session); instances = router.GetComponentsInChildren<Transform>(true);
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < deadline) return;
            try
            {
                if (SessionState.GetInt(Key + ".phase", 0) == 0)
                {
                    switch (step++)
                    {
                        case 0:
                            Bind(); router.Bind(router.driver.Session); router.Bind(router.driver.Session);
                            Click(router.talentTab);
                            Check(router.talent.lockedPanel.activeSelf && !router.talent.freeDraw.interactable && !router.talent.paidDraw.interactable, "First-prestige lock disables both draw actions");
                            Click(router.talent.odds); Check(router.talent.oddsPanel.activeSelf && router.talent.oddsText.text.Contains("4.6875%"), "Exact odds action has one listener after repeated binding");
                            Click(router.talent.cards[27]); Check(router.cardDetails.SelectedId == router.Session.Engine.Data.Cards[27].id, "Last of all 28 saved card cells opens its own identity");
                            Click(router.cardDetails.back); Click(router.prestigeTab);
                            reviewed = router.prestige.ReviewedQuote;
                            Check(reviewed.Eligible && reviewed.Fame.Value == 1 && reviewed.Credits.Value == 3, "Native first-prestige preview shows one Fame and exactly three free credits");
                            router.Session.Run(e => e.BuyLevels(0, 1)); router.prestige.Refresh();
                            Check(!router.prestige.confirm.interactable, "An economic command invalidates the saved prestige review");
                            Click(router.prestige.review); reviewed = router.prestige.ReviewedQuote; beforeCash = router.Session.Engine.State.cash;
                            deadline = EditorApplication.timeSinceStartup + .8; break;
                        case 1:
                            Check(router.Session.Engine.State.cash > beforeCash && ReferenceEquals(reviewed, router.prestige.ReviewedQuote) && router.prestige.confirm.interactable, "Passive earnings preserve the same confirmable prestige review");
                            Check(!router.driver.TapGameplaySurface(), "Prestige browsing blocks manual gameplay taps");
                            Click(router.prestige.confirm);
                            var state = router.Session.Engine.State;
                            Check(state.completedPrestiges.Value == 1 && state.fame.Value == 1 && state.freeCredits.Value == 3 && state.products.All(p => p == 0) && state.cash.CompareTo(new Amount(30)) == 0, "Native confirmation atomically resets the run and retains a manual first-purchase grant");
                            Check(router.toast.text.Contains("First Prestige") && router.toast.text.Contains("3 free draws") && router.toast.text.Contains("products rebuild"), "First Prestige feedback states its saved gains and reset distinction");
                            router.prestige.confirm.onClick.Invoke(); Check(router.Session.Engine.State.completedPrestiges.Value == 1, "Late duplicate confirm listener cannot repeat the prestige");
                            Check(router.CurrentRoute == "talent" && router.talent.freeDraw.interactable && !router.talent.paidDraw.interactable, "First prestige opens unlocked Talent with separate balance guards");
                            Click(router.talent.freeDraw);
                            state = router.Session.Engine.State;
                            Check(router.CurrentRoute == "drawreveal" && state.freeCredits.Value == 2 && state.reveal.pending && state.cardLevels.Sum(n => (int)n.Value) == 1, "One native free draw saves one card before showing its reveal");
                            Check(router.reveal.cardGroup.alpha == 0 && !router.reveal.Settled, "Normal-motion reveal starts hidden on its saved animation host");
                            SessionState.SetString(Key + ".operation", state.reveal.operationId);
                            var loaded = Store().Load().state;
                            Check(loaded.reveal.operationId == state.reveal.operationId && loaded.freeCredits.Value == 2, "On-disk result and debit exist before the reveal is acknowledged");
                            Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Talent, card and prestige routes preserve actual saved object identities");
                            Check(errors.Count == 0, "No gameplay/runtime errors in first Play Mode session: " + string.Join("; ", errors.Take(3)));
                            checks.Add("NOTE Known MCP project-path advisory count in first session: " + tooling.Count);
                            SessionState.SetString(Key + ".checks", JsonUtility.ToJson(new SavedChecks { lines = checks.ToArray() }));
                            SessionState.SetBool(Key + ".restart", true); EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); break;
                    }
                }
                else
                {
                    switch (step++)
                    {
                        case 0:
                            Bind();
                            Check(router.CurrentRoute == "drawreveal" && router.reveal.OperationId == SessionState.GetString(Key + ".operation", "") && router.Session.Engine.State.freeCredits.Value == 2, "New Play Mode session recovers the identical saved reveal without another debit");
                            Click(router.reveal.skip); Check(router.reveal.Settled && router.reveal.cardGroup.alpha == 1 && router.reveal.cardFace.localScale == Vector3.one, "Skip settles only the saved animation properties");
                            Click(router.reveal.details); Check(router.CurrentRoute == "carddetails", "Saved reveal opens its actual card details");
                            Click(router.cardDetails.back); Check(router.CurrentRoute == "drawreveal" && router.reveal.OperationId == SessionState.GetString(Key + ".operation", ""), "Returning before acknowledgement reopens the same result");
                            router.reveal.Settle(); Click(router.reveal.done);
                            Check(router.toastRoot.activeSelf && router.toast.text.Contains("Level 0 → 1"), "Acknowledging the first saved card retains readable level-change feedback");
                            for (int i = 0; i < 2; i++) { Click(router.talent.freeDraw); router.reveal.Settle(); Click(router.reveal.done); }
                            Check(router.Session.Engine.State.freeCredits.IsZero && !router.talent.freeDraw.interactable && router.talent.freeLabel.text.Contains("0 available"), "Last free draw stays a disabled free action and never becomes paid");
                            string purchase = null;
                            Check(router.Session.Run(e => e.EnableDevelopmentShop(DevelopmentShopState.Namespace)), "Simulated wallet is explicitly marked as a development profile");
                            Check(router.Session.Run(e => { purchase = e.BeginDevelopmentPurchase(e.Data.Packs[0].id); return purchase != null; }) && router.Session.Run(e => e.ResolveDevelopmentPurchase(purchase, DevelopmentPurchaseOutcome.Delivered)), "QA funds the wallet only through the isolated test-pack command");
                            router.talent.Refresh(); var before = router.Session.Engine.State.cardLevels.Select(n => n).ToArray();
                            Click(router.talent.paidDraw); var paid = router.Session.Engine.State; paid.Valid(router.Session.Engine.Data);
                            router.Session.Engine.Data.TryCard(paid.reveal.cardId, out int index);
                            Check(paid.kajuKatli.Value == 900 && paid.freeCredits.IsZero && paid.cardLevels[index].Value == before[index].Value + 1 && paid.reveal.payment == "kk", "Native paid draw spends exactly 100 simulated KK and increments exactly its chosen card");
                            router.talent.paidDraw.onClick.Invoke(); Check(router.Session.Engine.State.kajuKatli.Value == 900, "Late paid-click callback cannot spend while a result is pending");
                            deadline = EditorApplication.timeSinceStartup + .6; break;
                        case 1:
                            Check(router.reveal.Settled && router.reveal.cardGroup.alpha == 1, "Normal-motion reveal reaches its final appearance without granting again");
                            Click(router.reveal.done);
                            Check(Store().Load().state.kajuKatli.Value == 900 && !Store().Load().state.reveal.pending, "Final acknowledgement and remaining KK survive another save read");
                            Check(new HashSet<Transform>(instances).SetEquals(router.GetComponentsInChildren<Transform>(true)), "Repeated draws and reveal navigation create no UI objects");
                            Check(errors.Count == 0, "No gameplay/runtime errors in recovered Play Mode session: " + string.Join("; ", errors.Take(3)));
                            checks.Add("NOTE Known MCP project-path advisory count in recovered session: " + tooling.Count);
                            File.WriteAllLines(Report, new[] { "Scope: two actual Play Mode sessions with saved native preview and one disposable profile. Simulated KK only; Main/device acceptance remains open." }.Concat(checks));
                            Debug.Log("REDESIGN_TALENT_PLAY_OK " + checks.Count); Finish(); break;
                    }
                }
            }
            catch (Exception error) { File.WriteAllLines(Report, checks.Concat(new[] { "FAIL " + error })); SessionState.SetBool(Key + ".restart", false); Debug.LogError("REDESIGN_TALENT_PLAY_FAIL " + error); Finish(); }
        }
        static void Finish() { EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); }
    }
}
