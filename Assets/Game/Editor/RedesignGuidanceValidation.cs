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
    public static class RedesignGuidanceValidation
    {
        internal static RedesignState Fixture(string topic, out string route, out string argument)
        {
            var s = RedesignState.Fresh(); route = "home"; argument = "";
            if (topic == "buy-first") return s;
            s.products[0] = 25; s.products[1] = 1; s.cash = 10000;
            switch (topic)
            {
                case "product-levels": route = "productdetails"; argument = "0"; break;
                case "catalogue-filters": route = "upgrades"; break;
                case "buy-all": route = "buyall"; break;
                case "skill-basics": route = "skilldetails"; argument = "0"; break;
                case "comebacks": route = "upgradedetails"; argument = "PP-PHU-01"; break;
                case "prestige": route = "prestige"; break;
                case "cash-and-talent": route = "talent"; break;
                case "rebuild": case "talent-free": case "talent-duplicates": case "second-gift":
                    s.completedPrestiges = s.fame = topic == "second-gift" ? 2 : 1; s.target = ProgressionNumbers.RunTarget(s.completedPrestiges); s.secondPrestigeGift = topic == "second-gift"; s.freeCredits = 3;
                    s.products = new int[8]; route = topic == "rebuild" ? "home" : "talent";
                    if (topic == "talent-duplicates") { route = "drawreveal"; s.cardLevels[0] = 2; s.reveal = new DrawReveal { before = 1, after = 2, operationId = "guide-fixture", cardId = RedesignRuntimeContent.Load().Cards[0].id, payment = "credit", pending = true }; }
                    break;
            }
            return s;
        }
        public static void Run()
        {
            if (EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed) throw new InvalidOperationException("Require compiled Edit Mode");
            const string report = "Documentation/Implementation/Unity-Guidance-UI.txt"; File.WriteAllText(report, "RUNNING\n");
            var checks = new List<string>(); void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            var scene = EditorSceneManager.NewPreviewScene(); GameObject root = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BuildRedesignUI.PrefabPath), scene);
                var router = root.GetComponent<RedesignScreenRouter>(); var guide = router.guidance; var home = router.home; var data = RedesignRuntimeContent.Load();
                Check(guide && !guide.strip.activeSelf && !guide.ReaderOpen && guide.topics.Select(t => t.id).SequenceEqual(RedesignGuidance.Topics), "All 13 known topics and closed optional controls are saved in the prefab");
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("UI Builder/HTML/Redesign/guidance-topics.json"));
                Check(guide.topics.Select(JsonUtility.ToJson).SequenceEqual(manifest.topics.Select(JsonUtility.ToJson)), "Native guidance text exactly matches inspected editable HTML manifest");
                foreach (Transform page in home.transform.parent) page.gameObject.SetActive(false);
                void Awake(object o) => o.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(o, null);
                Awake(guide); Awake(home); var instances = root.GetComponentsInChildren<Transform>(true);
                Check(new[] { guide.title, guide.brief, guide.heading, guide.copy }.All(t => t is RedesignSavedText st && st.savedMaterialSlots.All(m => m)), "Guidance uses saved TMP material slots");
                foreach (string topic in RedesignGuidance.Topics)
                {
                    var state = Fixture(topic, out string route, out string argument); var engine = new RedesignEngine(data, state, _ => { }); var session = new RedesignLifecycle(engine);
                    var context = new RedesignUiContext(engine, () => session.CanMutate, session.Run, (_, __) => { }, router.Notice);
                    guide.Bind(context); home.Bind(context, session.TapGameplaySurface); home.Open(); guide.Refresh(route, argument, true);
                    Check(guide.CurrentTopic == topic && guide.strip.activeSelf && !guide.ReaderOpen, "Context selects a collapsed optional strip: " + topic);
                    guide.read.onClick.Invoke();
                    foreach (var size in new[] { new Vector2Int(390, 844), new Vector2Int(360, 540) })
                    {
                        RedesignNativeUiValidation.Render(root, scene, size.x, size.y, size.x == 390, "guidance-" + topic);
                        float scale = size.x / 1080f;
                        Check(guide.ReaderOpen && new[] { guide.title, guide.brief, guide.heading, guide.copy }.All(t => !t.isTextTruncated) && new[] { guide.read, guide.gotIt, guide.skip }.All(b => ((RectTransform)b.transform).rect.height * scale >= 47.9f), "Full native topic and 48px actions fit " + size + " / " + topic);
                    }
                    guide.CloseReader(); Check(!guide.ReaderOpen && engine.State.learnedTopics.Count == 0, "Closing a reader never acknowledges or spends: " + topic);
                }
                var second = Fixture("second-gift", out _, out _); bool fail = false; int saves = 0;
                var e = new RedesignEngine(data, second, _ => { if (fail) throw new IOException("Injected guide save failure"); saves++; }); var life = new RedesignLifecycle(e);
                bool blocked = false; guide.Bind(new RedesignUiContext(e, () => life.CanMutate && !blocked, life.Run, (_, __) => { }, router.Notice)); guide.Refresh("talent", "", true); guide.OpenReader();
                guide.skip.onClick.Invoke(); guide.Refresh("talent", "");
                Check(!guide.ReaderOpen && !guide.strip.activeSelf && e.State.learnedTopics.Contains("second-gift") && saves == 1, "Skip persists once and suppresses the next topic in an unchanged context");
                guide.Refresh("talent", "", true); Check(guide.CurrentTopic == "talent-free" && !guide.ReaderOpen, "Navigation may suggest the next relevant topic, always collapsed");
                guide.OpenReader(); fail = true; guide.gotIt.onClick.Invoke();
                Check(!e.State.learnedTopics.Contains("talent-free") && life.LastSaveException is IOException && !guide.ReaderOpen && !guide.strip.activeSelf, "Failed native acknowledgement retains the topic and enters normal save recovery");
                fail = false; life.RetrySave(); guide.Refresh("talent", ""); guide.OpenReader(); blocked = true; guide.Refresh("talent", "");
                Check(!guide.ReaderOpen && !guide.strip.activeSelf && guide.screenBody.offsetMax.y == -guide.topWithoutTip, "Utility/recovery blocking hides help and restores body inset");
                blocked = false; guide.Refresh("talent", ""); guide.OpenReader(); router.Notice("Saved result");
                Check(!guide.ReaderOpen && router.toastRoot.activeSelf, "Gameplay feedback closes the competing help reader");
                guide.OpenReader(); Check(guide.ReaderOpen && !router.toastRoot.activeSelf, "Opening help clears the competing notice without acknowledging it");
                guide.Refresh("offline", "", true); Check(!guide.ReaderOpen && !guide.strip.activeSelf, "Unrelated routes close help without persisting a lesson");
                Check(new HashSet<Transform>(instances).SetEquals(root.GetComponentsInChildren<Transform>(true)), "All topics, actions and recovery preserve saved UI object identities");
                Check(!EditorUtility.scriptCompilationFailed, "Unity compilation succeeds");
                File.WriteAllLines(report, new[] { "Scope: saved native topic text/layout at two sizes, durable command bindings and failure isolation. Full physical interaction/relaunch is separate Play Mode evidence." }.Concat(checks)); Debug.Log("REDESIGN_GUIDANCE_UI_OK " + checks.Count);
            }
            catch (Exception ex) { File.AppendAllText(report, string.Join("\n", checks) + "\nFAIL " + ex); throw; }
            finally { if (root) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(scene); }
        }
        [Serializable] sealed class Manifest { public RedesignGuidanceText[] topics; }
    }
}
