using System.Text.Json;
using RocketRaja.Core;

static partial class Program
{
    static void SaveDurabilityChecks(RedesignCatalog catalog)
    {
        string root = Path.Combine(Path.GetTempPath(), "ClassicDurability-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);
        T Decode<T>(string value) => JsonSerializer.Deserialize<T>(value, Json);
        void Raw<T>(string path, T value) { var json = Encode(value); File.WriteAllText(path, SavePayload.Digest(json) + "\n" + json); }
        void Fails(Action operation, string label)
        {
            bool failed = false; try { operation(); } catch (Exception ex) when (ex is IOException || ex is InvalidDataException) { failed = true; }
            Check(failed, label);
        }
        string file = Path.Combine(root, "rocket-raja-v2.save");
        RedesignSaveStore Store() => new(file, catalog, Encode, Decode<RedesignState>, Decode<SaveVersionHeader>);
        var state = RedesignState.Fresh(1000);
        Raw(file + ".tmp", state);
        var orphan = File.ReadAllBytes(file + ".tmp");
        Check(!Store().Load().CanCreateFresh && Store().Load().status == RedesignLoadStatus.Corrupt, "Interrupted first write is recovery evidence, never a new profile");
        Fails(() => Store().Save(state), "A restarted writer cannot overwrite an orphan temporary save");
        Check(orphan.SequenceEqual(File.ReadAllBytes(file + ".tmp")) && !File.Exists(file), "Interrupted first-write source bytes remain untouched");
        // All removals below are confined to this newly generated test directory.
        File.Delete(file + ".tmp");
        var store = Store(); store.Save(state);
        state.revision += (Natural)1; state.cash = 31; store.Save(state);
        var main = File.ReadAllBytes(file); var backup = File.ReadAllBytes(file + ".bak");
        var newer = state.Copy(); newer.schema = 99;
        foreach (string suffix in new[] { ".bak", ".tmp" })
        {
            Raw(file + suffix, newer); var future = File.ReadAllBytes(file + suffix);
            Check(Store().Load().status == RedesignLoadStatus.UnsupportedVersion, "Future " + suffix + " blocks fallback to an older main");
            Fails(() => Store().Save(state), "Future " + suffix + " cannot be rotated or replaced");
            Check(main.SequenceEqual(File.ReadAllBytes(file)) && future.SequenceEqual(File.ReadAllBytes(file + suffix)), "Future " + suffix + " rejection preserves all source bytes");
            if (suffix == ".bak") File.WriteAllBytes(file + suffix, backup); else File.Delete(file + suffix);
        }
        Raw(file + ".tmp", state);
        var interrupted = File.ReadAllBytes(file + ".tmp");
        Check(Store().Load().state.cash.CompareTo((Amount)31) == 0, "Interrupted replacement loads the committed main, never the uncommitted temporary candidate");
        state.revision += (Natural)1; store.Save(state);
        Check(Directory.GetFiles(root, "*.interrupted-*").Any(p => interrupted.SequenceEqual(File.ReadAllBytes(p))), "Retry archives interrupted bytes before reusing the temporary filename");
        main = File.ReadAllBytes(file); backup = File.ReadAllBytes(file + ".bak");
        using (new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Fails(() => store.Save(state), "An interrupted atomic replacement reports failure");
            Check(main.SequenceEqual(File.ReadAllBytes(file)), "Failed replace preserves the committed main");
        }
        store.Save(state);
        Check(store.Load().state.Valid(catalog), "Retry after a failed replace commits a valid complete snapshot");
        File.WriteAllText(file, "damaged main");
        var recovered = Store().Load();
        Check(recovered.status == RedesignLoadStatus.RecoveredBackup && recovered.state.requiresDrawReconciliation, "Backup recovery carries a durable draw-reconciliation fence");
        Fails(() => Store().Save(state), "A caller cannot remove the backup reconciliation fence before saving");
        Store().Save(recovered.state);
        Check(Directory.GetFiles(root, "*.damaged-*").Any(p => File.ReadAllText(p) == "damaged main"), "Recovery archives the damaged main without replacing the valid backup with it");

        string retryFile = Path.Combine(root, "first-retry", "rocket-raja-v2.save");
        var retryStore = new RedesignSaveStore(retryFile, catalog, Encode, Decode<RedesignState>, Decode<SaveVersionHeader>);
        Directory.CreateDirectory(retryFile + ".tmp");
        bool denied = false; try { retryStore.Save(RedesignState.Fresh()); } catch (UnauthorizedAccessException) { denied = true; }
        Check(denied, "First-save temporary-file creation failure is reported");
        Directory.Delete(retryFile + ".tmp"); File.WriteAllText(retryFile + ".tmp", "partial write");
        retryStore.Save(RedesignState.Fresh());
        Check(retryStore.Load().status == RedesignLoadStatus.Loaded, "The same live writer can retry its failed first save after storage recovery");

        var local = CheckpointStore.ForRedesign(root, "durability-account", Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>);
        string checkpointFile = Path.Combine(root, "economy-2", "accounts", SavePayload.Digest("durability-account"), "checkpoint.save");
        var checkpoint = new AccountCheckpoint { uid = "durability-account", owner = "device-a", generation = 1, baseRevision = 1, commitId = "commit-a", save = SavePayload.Create(Encode(state)), pending = true };
        local.Save(checkpoint); local.Save(checkpoint);
        var checkpointMain = File.ReadAllBytes(checkpointFile); var checkpointBackup = File.ReadAllBytes(checkpointFile + ".bak");
        foreach (string suffix in new[] { "", ".bak", ".tmp" })
        {
            var future = Clone(checkpoint); future.schema = 2; Raw(checkpointFile + suffix, future);
            var bytes = File.ReadAllBytes(checkpointFile + suffix);
            Fails(() => local.Load(checkpoint.uid), "Future checkpoint " + suffix + " blocks older fallback");
            Fails(() => local.Save(checkpoint), "Future checkpoint " + suffix + " blocks overwrite");
            Check(bytes.SequenceEqual(File.ReadAllBytes(checkpointFile + suffix)), "Future checkpoint " + suffix + " remains byte-identical");
            File.WriteAllBytes(checkpointFile, checkpointMain); File.WriteAllBytes(checkpointFile + ".bak", checkpointBackup);
            if (suffix == ".tmp") File.Delete(checkpointFile + suffix);
        }
        var wrong = Clone(checkpoint); wrong.uid = "another-account";
        foreach (bool followup in new[] { false, true })
        {
            var future = Clone(checkpoint); var payload = state.Copy(); payload.contentVersion = "future-content";
            if (followup) future.followup = SavePayload.Create(Encode(payload)); else future.save = SavePayload.Create(Encode(payload));
            Raw(checkpointFile + ".bak", future); var bytes = File.ReadAllBytes(checkpointFile + ".bak");
            Fails(() => local.Load(checkpoint.uid), "Future gameplay in backup " + (followup ? "follow-up" : "upload") + " blocks an older checkpoint");
            Fails(() => local.Save(checkpoint), "Future gameplay backup cannot be rotated away");
            Check(bytes.SequenceEqual(File.ReadAllBytes(checkpointFile + ".bak")), "Future gameplay checkpoint bytes stay intact");
            File.WriteAllBytes(checkpointFile + ".bak", checkpointBackup);
        }
        Fails(() => local.Save(wrong), "Checkpoint path rejects another account's UID");
        var invalid = Clone(checkpoint); invalid.save.checksum = "invalid"; Raw(checkpointFile, invalid);
        Check(local.Load(checkpoint.uid).commitId == checkpoint.commitId, "Semantically damaged checkpoint recovers its validated backup");
        local.Save(checkpoint);
        Check(checkpointBackup.SequenceEqual(File.ReadAllBytes(checkpointFile + ".bak")), "Checksum-valid but invalid checkpoint never rotates over the valid backup");
        Raw(checkpointFile, new AccountCheckpoint { schema = 1 }); File.Delete(checkpointFile + ".bak");
        Fails(() => local.Load(checkpoint.uid), "Unrecoverable checkpoint blocks account initialization rather than becoming missing");
        Fails(() => local.Save(checkpoint), "Unrecoverable checkpoint cannot be replaced by a fresh cloud snapshot");

        var earning = RedesignState.Fresh(1000); earning.cash = 30;
        var engine = new RedesignEngine(catalog, earning, _ => { });
        engine.BuyLevels(0, 1); engine.Save(); var start = engine.State.cash;
        Check(!engine.ResolveOffline(900) && engine.State.cash.CompareTo(start) == 0, "Clock rollback grants nothing and retains the saved time anchor");
        Check(engine.ResolveOffline(1000000) && engine.State.offline.creditedSeconds == 28800, "Large forward jump retains the designed eight-hour offline cap");
        var cash = engine.State.cash; var receipt = engine.State.offline.id;
        Check(!engine.ResolveOffline(1000000) && !engine.ResolveOffline(999999) && engine.State.cash.CompareTo(cash) == 0 && engine.State.offline.id == receipt, "Repeated and reversed clock events cannot replay an offline receipt");
        RedesignState disk = engine.State.Copy(); bool full = true;
        var lifecycle = new RedesignLifecycle(new RedesignEngine(catalog, disk.Copy(), s => { if (full) throw new IOException("Synthetic ENOSPC"); disk = s.Copy(); }));
        string prior = Encode(lifecycle.Engine.State);
        Check(!lifecycle.Run(e => e.BuyLevels(0, 1)) && !lifecycle.CanMutate && Encode(lifecycle.Engine.State) == prior, "ENOSPC injection rolls back the purchase and closes mutation");
        full = false;
        Check(lifecycle.RetrySave() && lifecycle.CanMutate && Encode(lifecycle.Engine.State) == Encode(disk), "Storage recovery durably commits the retained state before reopening mutation");
        Console.WriteLine("Durability fixtures: " + root);
    }
}
