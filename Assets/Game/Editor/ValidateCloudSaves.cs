using System;
using System.Collections.Generic;
using System.IO;
using RocketRaja.Cloud;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class ValidateCloudSaves
    {
        [MenuItem("Rocket Raja/Validate Cloud Save Codecs")]
        public static void Run()
        {
            var results = new List<string>();
            void Check(bool value, string name) { if (!value) throw new InvalidOperationException("FAIL: " + name); results.Add("PASS: " + name); }
            string root = Path.Combine(Application.temporaryCachePath, "RocketRajaCloudQA-" + Guid.NewGuid().ToString("N"));
            var guest = new GuestSession(root);
            Check(guest.CanPlay && !guest.IsAccount && !guest.NeedsRecovery, "New guest initializes without Firebase services");
            guest.State.cash = new Amount(1.25, 450); guest.State.manualTaps = 5; guest.Save();
            var restored = new GuestSession(root);
            Check(restored.State.cash.exponent == 450 && restored.State.manualTaps == 5, "Guest survives closing and reopening through Unity codec");
            var payload = SavePayload.Create(JsonUtility.ToJson(restored.State));
            var c = new AccountCheckpoint { uid = "codec-test", owner = "test-device", generation = 1, baseRevision = 3, save = payload, pending = true, commitId = "upload-receipt", followup = payload };
            var store = new CheckpointStore(root, c.uid, JsonUtility.ToJson, JsonUtility.FromJson<AccountCheckpoint>);
            store.Save(c); var loaded = store.Load(c.uid);
            Check(loaded.Valid(c.uid) && loaded.baseRevision == 3 && loaded.generation == 1 && loaded.followup.Valid(), "Unity checkpoint preserves upload metadata and follow-up snapshot atomically");
            Check(JsonUtility.FromJson<GameState>(loaded.save.json).cash.exponent == 450, "Account snapshot retains large-number precision");
            Check(store.Load("another-user") == null, "Wrong account cannot load a checkpoint");
            c.followup = null; store.Save(c);
            var encodedAbsent = JsonUtility.FromJson<AccountCheckpoint>(JsonUtility.ToJson(c));
            Check(encodedAbsent.followup != null && !encodedAbsent.followup.Valid(), "Reproduces Unity's empty-object representation of a null follow-up");
            loaded = store.Load(c.uid);
            Check(loaded != null && loaded.pending && loaded.followup == null && loaded.save.checksum == payload.checksum,
                "Pending checkpoint reload normalizes absent follow-up without losing the primary receipt");
            c.pending = false; store.Save(c); loaded = store.Load(c.uid);
            Check(loaded != null && !loaded.pending && loaded.followup == null, "Acknowledged checkpoint reload also normalizes absent follow-up");
            c.followup = new SavePayload { json = "damaged", checksum = "wrong" }; store.Save(c); loaded = store.Load(c.uid);
            Check(loaded.followup != null && loaded.followup.json == "damaged" && !loaded.followup.Valid(),
                "Populated corrupt follow-up is preserved, never silently treated as absent");
            c.followup = payload; c.pending = true; store.Save(c); loaded = store.Load(c.uid);
            Check(loaded.followup != null && loaded.followup.Valid(), "Real follow-up survives normalization and remains recoverable");
            File.WriteAllText(Path.Combine(root, "diwali-v1.save"), "damaged");
            File.WriteAllText(Path.Combine(root, "diwali-v1.save.bak"), "damaged backup");
            var damaged = new GuestSession(root); damaged.Save();
            Check(damaged.NeedsRecovery && File.ReadAllText(Path.Combine(root, "diwali-v1.save")) == "damaged", "Damaged guest data is not silently reset");
            damaged.StartFresh();
            Check(!damaged.NeedsRecovery && File.Exists(Path.Combine(root, "diwali-v1.save.damaged")), "Explicit fresh start preserves damaged evidence");
            Directory.CreateDirectory("Documentation/CloudSaveValidation");
            File.WriteAllLines("Documentation/CloudSaveValidation/UnityCodecs.txt", results);
            Debug.Log("CLOUD_UNITY_CODECS_OK: " + results.Count);
        }
    }
}
