using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using RocketRaja.Cloud;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    // Explicit local emulator only. Never uses the default Firebase app or real accounts.
    public static class CloudAcknowledgementValidation
    {
        const string Report = "Documentation/CloudSaveValidation/AcknowledgementRegression.txt";
        public static void RunOnce(string request)
        {
            if (SessionState.GetString("RocketRaja.AckRegression", "") == request) return;
            SessionState.SetString("RocketRaja.AckRegression", request);
            Run();
        }
        static async void Run()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Report));
            File.WriteAllText(Report, "Running isolated Firebase SDK acknowledgement regression…\n");
            FirebaseApp app = null;
            FirebaseFirestore db = null;
            ListenerRegistration probeListener = null;
            AccountSession session = null;
            try
            {
                await FirebaseApp.CheckAndFixDependenciesAsync();
                app = FirebaseApp.Create(new AppOptions { ProjectId = "demo-rocket-ack", AppId = "1:123:android:ack-test", ApiKey = "emulator-only" }, "ack-" + Guid.NewGuid().ToString("N"));
                db = FirebaseFirestore.GetInstance(app);
                db.Settings.Host = "127.0.0.1:8085";
                db.Settings.SslEnabled = false;
                db.Settings.PersistenceEnabled = false;
                var doc = db.Collection("probe").Document(Guid.NewGuid().ToString("N"));
                await doc.SetAsync(new Dictionary<string, object> { ["revision"] = 0L });
                var listening = new TaskCompletionSource<bool>();
                probeListener = doc.Listen(MetadataChanges.Include, s => { if (!s.Metadata.IsFromCache) listening.TrySetResult(true); });
                if (await Task.WhenAny(listening.Task, Task.Delay(10000)) != listening.Task) throw new TimeoutException("Emulator listener did not start.");
                int stale = 0;
                for (long revision = 1; revision <= 100; revision++)
                {
                    long expected = revision;
                    await db.RunTransactionAsync(async tx =>
                    {
                        await tx.GetSnapshotAsync(doc);
                        tx.Update(doc, new Dictionary<string, object> { ["revision"] = expected });
                    });
                    var oldRead = await doc.GetSnapshotAsync(Source.Server);
                    if (oldRead.GetValue<long>("revision") != expected) stale++;
                    long fresh = await db.RunTransactionAsync(async tx => (await tx.GetSnapshotAsync(doc)).GetValue<long>("revision"));
                    if (fresh != expected) throw new Exception("Transaction confirmation returned the wrong revision.");
                }
                File.AppendAllText(Report, "Legacy Source.Server stale reads: " + stale + "/100\nPASS 100 transaction reads confirmed the committed revision with an active listener\n");
                var store = new FirestoreSaveStore(db);
                string uid = "test-" + Guid.NewGuid().ToString("N");
                var state = new GameState { now = 1000, savedAt = 1000 };
                await store.CreateAsync(new AccountProfile { uid = uid }, SavePayload.Create(JsonUtility.ToJson(state)));
                var owned = await store.AcquireAsync(uid, "test-device");
                var local = new CheckpointStore(Path.Combine(Application.temporaryCachePath, "AckRegression", uid), uid, JsonUtility.ToJson, JsonUtility.FromJson<AccountCheckpoint>);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                session = new AccountSession(store, local, uid, "test-device", Catalog.Create(), JsonUtility.ToJson, JsonUtility.FromJson<GameState>, () => clock.Elapsed.TotalSeconds);
                await session.InitializeAsync(owned);
                var engine = new GameEngine(Catalog.Create(), session.State) { MutationAllowed = () => session.CanPlay };
                for (int i = 0; i < 100; i++)
                {
                    engine.Tap(); session.Save(true); session.Tick(true);
                    // Include a mutation while the preceding immutable receipt is uploading.
                    engine.Tap(); session.Save(true);
                    await session.WaitForPendingAsync();
                    session.Tick(true); await session.WaitForPendingAsync();
                    if (!session.CanPlay) throw new Exception("Session paused at iteration " + i + ": " + session.Status);
                }
                var final = await store.ReadAsync(uid);
                if (JsonUtility.FromJson<GameState>(final.save.json).manualTaps != 200 || local.Load(uid).pending)
                    throw new Exception("Final cloud state/checkpoint did not acknowledge all mutations.");
                File.AppendAllText(Report, "PASS 200 confirmed account uploads, including in-flight follow-ups, without false pauses\nPASS final cloud snapshot contains all 200 taps; local checkpoint acknowledged\n");
            }
            catch (Exception ex) { File.AppendAllText(Report, "FAIL " + ex + "\n"); Debug.LogException(ex); }
            finally
            {
                session?.Dispose(); probeListener?.Stop();
                if (db != null) await db.TerminateAsync();
                app?.Dispose();
                File.AppendAllText(Report, "Finished\n");
            }
        }
    }
}
