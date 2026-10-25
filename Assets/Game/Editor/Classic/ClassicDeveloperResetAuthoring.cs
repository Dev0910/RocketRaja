using System;
using System.IO;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static class ClassicDeveloperResetAuthoring
    {
        public static string Author()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required.");
            var ui = Object.FindObjectsByType<ClassicUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Single(u => u.name == "ClassicProductionCanvas" && u.gameObject.scene.path == "Assets/Scenes/Main.unity");
            if (ui.developerResetRow) return "Reset UI already authored.";
            var settings = ui.dialogs.Single(v => v.route == "Settings");
            var row = Object.Instantiate(settings.scroll.content.Find("Help").gameObject, settings.scroll.content);
            row.name = "Developer Reset";
            row.GetComponentInChildren<TMP_Text>(true).text = "Reset game (developer)";
            var control = row.GetComponent<ClassicControl>();
            control.owner = ui; control.action = "Navigate"; control.parameter = "DeveloperReset";
            // The copied book icon belongs to Help, so keep only the action label and chevron.
            var icon = row.transform.Find("Painted icon");
            if (icon) Object.DestroyImmediate(icon.gameObject);
            ui.developerResetRow = row;

            var source = ui.dialogs.Single(v => v.route == "RecoveryConfirm");
            var root = Object.Instantiate(source.root, source.root.transform.parent);
            root.name = "DeveloperReset";
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name == "Title") text.text = "Reset game?";
                else if (text.name == "Heading") text.text = "Start a fresh game";
                else if (text.name == "Body") text.text = "Erase all local guest progress, including cash, products, upgrades, cards, Fame and settings. This cannot be undone. Google account saves are unchanged.";
            }
            foreach (var c in root.GetComponentsInChildren<ClassicControl>(true))
            {
                c.owner = ui;
                if (c.action == "Account")
                {
                    c.action = "DeveloperReset"; c.parameter = "";
                    c.GetComponentInChildren<TMP_Text>(true).text = "Reset game";
                }
            }
            Transform Map(Transform t) => root.transform.Find(AnimationUtility.CalculateTransformPath(t, source.root.transform));
            var view = new ClassicView { route = "DeveloperReset", root = root,
                window = (RectTransform)Map(source.window), group = root.GetComponent<CanvasGroup>(),
                scroll = source.scroll ? Map(source.scroll.transform).GetComponent<UnityEngine.UI.ScrollRect>() : null };
            ui.dialogs = ui.dialogs.Concat(new[] { view }).ToArray();
            ui.developerResetDialog = root;
            root.SetActive(false);
            EditorUtility.SetDirty(ui);
            EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            EditorSceneManager.SaveScene(ui.gameObject.scene);
            return "Authored developer reset row and confirmation in Main.";
        }

        public static string Validate()
        {
            string root = Path.Combine(Application.temporaryCachePath, "reset-validation-" + Guid.NewGuid().ToString("N"));
            var go = new GameObject("Reset validation");
            var driver = go.AddComponent<RedesignRuntimeDriver>();
            void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
            try
            {
                Require(driver.InitializeGuest(root), "Initialize guest");
                for (int i = 0; i < 30; i++) Require(driver.Command(e => e.Tap()), "Seed guest progress");
                Require(driver.Command(e => e.BuyLevels(0, 1)), "Seed stocked product");
                Require(driver.Command(e => e.SetStoreName("RESET TEST")), "Seed shop name");
                Require(driver.Command(e => e.ToggleSetting("music")), "Seed settings");
                string oldRun = driver.Session.Engine.State.runId;
                Require(driver.ResetDevelopmentGame(), "Reset guest");
                var state = driver.Session.Engine.State;
                Require(state.runId != oldRun && state.manualTaps.IsZero && state.upgrades.Count == 0 &&
                    state.products.All(n => n == 0) && state.cardLevels.All(n => n.IsZero) && state.fame.IsZero,
                    "Reset did not clear progression");
                Require(state.cash.CompareTo(Amount.Zero) == 0 && state.storeName == "" && state.music && state.sound &&
                    !state.reducedMotion && !state.offline.pending && !state.reveal.pending, "Fresh defaults not restored");
                var catalog = RedesignRuntimeContent.Load();
                RedesignSaveStore Store(string path) => new RedesignSaveStore(path, catalog, JsonUtility.ToJson,
                    JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
                var loaded = Store(Path.Combine(root, "rocket-raja-v2.save")).Load();
                Require(loaded.state.runId == state.runId && loaded.state.manualTaps.IsZero, "Reset not durable");
                var backup = Store(Path.Combine(root, "rocket-raja-v2.save.bak")).Load();
                Require(backup.state.runId == state.runId && backup.state.manualTaps.IsZero, "Backup restored old progress");
                Require(driver.Command(e => e.Tap()), "Cannot play after reset");
                driver.Session.Suspend();
                Require(!driver.ResetDevelopmentGame(), "Suspended session allowed reset");
                return "PASS: fresh progression, durable save, fresh backup, playable after reset, suspended reset rejected. Temporary profile only.";
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
