using System;
using System.Collections.Generic;
using System.IO;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignStartupRecoveryValidation
    {
        public static void Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            var checks = new List<string>();
            void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); checks.Add("PASS " + label); }
            string root = Path.GetFullPath("Temp/RedesignStartupRecovery-" + Guid.NewGuid().ToString("N"));
            string file = Path.Combine(root, "rocket-raja-v2.save");
            var data = RedesignRuntimeContent.Load();
            var store = new RedesignSaveStore(file, data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            var state = RedesignState.Fresh(RedesignRuntimeDriver.Utc - 36000); state.products[0] = 1;
            store.Save(state);
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            // Exercise the current driver directly; the old redesign UI prefab is not the shipped Classic owner.
            var owner = new GameObject("Local save recovery validation", typeof(RedesignRuntimeDriver));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(owner, scene);
            try
            {
                var driver = owner.GetComponent<RedesignRuntimeDriver>();
                using (var locked = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Check(!driver.InitializeGuest(root), "A real locked save prevents initial persistence");
                    Check(!driver.Initialized && !driver.Session.CanMutate && driver.Session.LastSaveException is IOException, "Startup stays closed with the underlying I/O failure recorded");
                    Check(!driver.Command(e => e.BuyLevels(0, 1)) && !driver.TapGameplaySurface(), "No purchasing or tapping before recovery");
                }
                Check(driver.RetrySaving() && driver.Initialized && driver.Session.CanMutate, "Retry resumes initialization after the file lock is released");
                var receipt = driver.Session.Engine.State.offline;
                Check(receipt.pending && receipt.creditedSeconds == 28800 && receipt.amount > Amount.Zero, "Recovery settles the original ten-hour absence with the eight-hour cap");
                var cash = driver.Session.Engine.State.cash; string id = receipt.id;
                var saved = store.Load().state;
                Check(saved.offline.id == id && Math.Abs(Amount.Ratio(saved.cash, cash) - 1) < 1e-12, "Offline receipt and credit are durable before input becomes available");
                Check(driver.RetrySaving() && driver.Session.Engine.State.cash.CompareTo(cash) == 0 && driver.Session.Engine.State.offline.id == id, "Repeated retry cannot settle the absence a second time");
            }
            finally { UnityEngine.Object.DestroyImmediate(owner); UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene); }
            File.WriteAllLines("Documentation/Implementation/Unity-Startup-Recovery.txt", checks);
            Debug.Log("REDESIGN_STARTUP_RECOVERY_OK " + checks.Count);
        }
    }
}
