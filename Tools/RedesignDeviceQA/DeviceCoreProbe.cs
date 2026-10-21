using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RocketRaja.Core;
using UnityEngine;

// Copied only to an isolated temporary Unity project by build.ps1. Never part of the game APK.
public sealed class DeviceCoreProbe : MonoBehaviour
{
    [Serializable] public sealed class Report
    {
        public string phase, result, failure, unity, model, processor, graphics, operatingSystem, contentHash;
        public string[] checks, measurements;
        public bool development;
        public int pointerBytes, systemMemoryMiB;
    }
    readonly List<string> checks = new List<string>(), measurements = new List<string>();
    void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException(name); checks.Add(name); }
    IEnumerator Start()
    {
        Application.targetFrameRate = 30;
        yield return null;
        yield return null;
        string root = Application.persistentDataPath;
        var report = new Report { unity = Application.unityVersion, model = SystemInfo.deviceModel, processor = SystemInfo.processorType,
            graphics = SystemInfo.graphicsDeviceName, operatingSystem = SystemInfo.operatingSystem, development = UnityEngine.Debug.isDebugBuild,
            pointerBytes = IntPtr.Size, systemMemoryMiB = SystemInfo.systemMemorySize };
        try
        {
            var source = JsonUtility.FromJson<RedesignContent>(Resources.Load<TextAsset>("RedesignContentCatalog").text);
            var data = new RedesignCatalog(source); report.contentHash = data.SourceHash;
            var store = new RedesignSaveStore(Path.Combine(root, "pending-paid.save"), data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            var previous = store.Load();
            if (previous.state == null)
            {
                Check(previous.CanCreateFresh, "Diagnostic profile starts without damaged or incompatible input");
                report.phase = "before-force-stop";
                FirstLaunch(data, root, store);
            }
            else
            {
                report.phase = "after-force-stop";
                SecondLaunch(data, store, previous.state);
            }
            report.result = "PASS";
        }
        catch (Exception e) { report.result = "FAIL"; report.failure = e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace; }
        report.checks = checks.ToArray(); report.measurements = measurements.ToArray();
        string json = JsonUtility.ToJson(report, true);
        File.WriteAllText(Path.Combine(root, report.phase == "after-force-stop" ? "report-after.json" : "report-before.json"), json);
        UnityEngine.Debug.Log("REDESIGN_DEVICE_QA " + report.phase + " " + report.result + " checks=" + checks.Count + " root=" + root);
        // Stay alive so the host can force-stop the process between a persisted pending
        // currency operation and its synthetic response. No store or network SDK is present.
    }
    void Measure(string label, int count, Action action)
    {
        action(); var timer = Stopwatch.StartNew();
        for (int i = 0; i < count; i++) action();
        timer.Stop(); measurements.Add(label + ": count=" + count + "; mean_ms=" + (timer.Elapsed.TotalMilliseconds / count).ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
    }
    void FirstLaunch(RedesignCatalog data, string root, RedesignSaveStore pendingStore)
    {
        var disk = new RedesignSaveStore(Path.Combine(root, "core.save"), data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
        var e = new RedesignEngine(data, RedesignState.Fresh(1000), disk.Save);
        Check(e.BuyLevels(0, 1) && disk.Load().state.products[0] == 1, "Manual first purchase roundtrips through Android file I/O and JsonUtility");
        var state = e.State.Copy(); state.cash = new Amount(1, 450); state.products = Enumerable.Repeat(300, 8).ToArray();
        var full = new RedesignEngine(data, state, _ => { });
        Measure("BuyAll_quote_1000", 10, () => { if (full.QuoteBuyAll().Ids.Count != 1000) throw new Exception("Full quote incomplete"); });
        Check(full.CommitBuyAll(full.QuoteBuyAll()) && full.State.UniqueCount == 1000, "Full catalogue purchase reaches all 1000 authored IDs on IL2CPP");
        Check(full.State.Valid(data), "Full catalogue state validates on ARM64");
        Measure("NormalIncome_cached", 2000, () => full.NormalIncome());
        Measure("State_validation_1000", 20, () => { if (!full.State.Valid(data)) throw new Exception("Invalid state"); });
        string json = JsonUtility.ToJson(full.State);
        measurements.Add("full_run_save_utf8_bytes=" + Encoding.UTF8.GetByteCount(json));
        Measure("Json_encode_1000", 20, () => JsonUtility.ToJson(full.State));
        Measure("Json_decode_1000", 20, () => JsonUtility.FromJson<RedesignState>(json));
        Check(Encoding.UTF8.GetByteCount(json) < 750000, "Full collection fits the gameplay cloud payload limit");
        Measure("Atomic_save_1000", 5, () => disk.Save(full.State));
        Check(disk.Load().state.upgrades.Count == 1000, "Full collection survives real Android atomic replacement");
        long memoryBefore = GC.GetTotalMemory(true);
        Measure("Prestige_three_draws_rebuild", 20, () => {
            var s = full.State.Copy(); s.eligibleEarnings = s.target;
            var cycle = new RedesignEngine(data, s, _ => { });
            if (!cycle.Prestige(cycle.PreviewPrestige())) throw new Exception("Reset failed");
            for (int i = 0; i < 3; i++) { if (!cycle.Draw(cycle.QuoteDraw(true))) throw new Exception("Draw failed"); cycle.DismissReveal(cycle.State.reveal.operationId); }
            if (!cycle.BuyLevels(0, 1)) throw new Exception("Manual rebuild failed");
        });
        measurements.Add("managed_retained_byte_delta_after_GC=" + (GC.GetTotalMemory(true) - memoryBefore));
        Check(full.State.UniqueCount == 1000, "Repeated isolated reset/draw cycles preserve their source snapshot");
        var prestigeState = full.State.Copy(); prestigeState.eligibleEarnings = prestigeState.target;
        e = new RedesignEngine(data, prestigeState, disk.Save);
        Check(e.Prestige(e.PreviewPrestige()) && e.State.fame.Value == 1 && e.State.freeCredits.Value == 3, "First prestige delivers exactly one Fame and three free credits");
        Check(e.Draw(e.QuoteDraw(true)), "Secure live random source produces a valid free draw on IL2CPP");
        string first = e.State.reveal.cardId;
        e = new RedesignEngine(data, disk.Load().state, disk.Save);
        Check(e.State.reveal.cardId == first && e.State.reveal.pending && e.DismissReveal(e.State.reveal.operationId), "Reopened draw replays persisted card and dismisses without spending again");
        Check(e.BuyLevels(0, 1) && e.Save() && e.ResolveOffline(e.State.now + 40000), "Offline cap settlement persists on Android");
        var offline = e.State.offline.amount; double now = e.State.now;
        e = new RedesignEngine(data, disk.Load().state, disk.Save);
        Check(!e.ResolveOffline(now) && e.State.offline.amount.CompareTo(offline) == 0, "Repeated offline settlement cannot deliver again");
        var wallet = new PremiumWalletSnapshot { uid = "isolated-device-fixture", profileId = e.State.guestWallet.profileId, revision = "1", balance = "1000", deficit = "0",
            cards = Enumerable.Repeat("0", 28).ToArray(), giftRegistered = false, importedGiftCardId = "", importedGiftOperationId = "" };
        e = new RedesignEngine(data, e.State, pendingStore.Save);
        Check(e.ReconcilePremiumWallet(wallet.uid, wallet), "Synthetic authoritative component activates schema 3 through Android codec");
        var request = e.BeginCurrencyDraw();
        Check(request != null && pendingStore.Load().state.premium.pendingOperationId == request.OperationId, "Pending currency operation is durable before forced process termination");
    }
    void SecondLaunch(RedesignCatalog data, RedesignSaveStore store, RedesignState state)
    {
        var e = new RedesignEngine(data, state, store.Save);
        var request = e.BeginCurrencyDraw();
        Check(state.HasPremium && request != null && request.OperationId == state.premium.pendingOperationId, "Force-stop/relaunch reuses the exact persisted currency operation");
        var wallet = new PremiumWalletSnapshot { uid = state.premium.uid, profileId = state.guestWallet.profileId, revision = "2", balance = "900", deficit = "0",
            cards = Enumerable.Repeat("0", 28).ToArray(), giftRegistered = false, importedGiftCardId = "", importedGiftOperationId = "" };
        wallet.cards[0] = "1";
        var result = new CurrencyDrawOutcome { operationId = request.OperationId, cardId = "C01", before = "0", after = "1", revision = "2", price = "100", wallet = wallet };
        result = JsonUtility.FromJson<CurrencyDrawOutcome>(JsonUtility.ToJson(result));
        var before = state.cardLevels[0];
        Check(e.ApplyCurrencyDraw(state.premium.uid, result) && store.Load().state.reveal.pending && e.State.cardLevels[0].Value == before.Value + 1 && e.State.kajuKatli.Value == 900, "Synthetic response persists one debit and one copy before reveal after relaunch");
        Check(e.DismissReveal(result.operationId) && !e.ApplyCurrencyDraw(state.premium.uid, result) && e.State.kajuKatli.Value == 900, "Repeated response cannot debit, grant or reveal again");
        wallet.revision = "1"; wallet.balance = "1000";
        Check(!e.ReconcilePremiumWallet(state.premium.uid, wallet) && e.State.kajuKatli.Value == 900, "Stale Android wallet snapshot cannot restore spent currency");
    }
}
