#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RocketRaja.Core;
using UnityEngine;
using UnityEngine.Profiling;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
        [Serializable] sealed class RedesignCycleSample
        {
            public int cycle, fullRunBytes, postResetBytes;
            public double quoteMs, batchMs, incomeMeanMs, encodeMs, decodeMs, saveMs, prestigeMs, threeRevealsMs;
            public long managedAfterGc, unityAllocatedBytes;
        }

        IEnumerator RedesignRepeatedCycles(RedesignScreenRouter router, RedesignSamples samples)
        {
            var engine = router.Session.Engine;
            samples.cycles = new RedesignCycleSample[8];
            for (int run = 0; run < samples.cycles.Length; run++)
            {
                // Only an explicit disposable smoke profile reaches this path. Stage resources to measure
                // repeated full runs in bounded time; this is not an earned-economy/pacing simulation.
                if (string.IsNullOrEmpty(RedesignSmokeProfile.Root) || !GameController.SmokeMode)
                    throw new InvalidOperationException("Disposable performance fixture required");
                engine.State.cash = Amount.Pow10(100);
                engine.State.products = Enumerable.Repeat(300, 8).ToArray();
                engine.State.eligibleEarnings = engine.State.target * 4;
                Check(router.Session.Run(e => e.Save()), "Cycle " + (run + 1) + " staged fixture persists");
                var result = new RedesignCycleSample { cycle = run + 1 };
                var timer = Stopwatch.StartNew(); var quote = engine.QuoteBuyAll(); timer.Stop(); result.quoteMs = timer.Elapsed.TotalMilliseconds;
                Check(quote.Ids.Count == 1000, "Cycle " + result.cycle + " reviews 1,000 rebuy entries");
                router.OpenBatch(quote); yield return null;
                timer.Restart(); RedesignClick(router.batch.confirm); timer.Stop(); result.batchMs = timer.Elapsed.TotalMilliseconds;
                Check(engine.State.upgrades.Count == 1000, "Cycle " + result.cycle + " native batch finishes");
                timer.Restart(); Amount income = 0;
                for (int i = 0; i < 1000; i++) income = engine.NormalIncome();
                timer.Stop(); result.incomeMeanMs = timer.Elapsed.TotalMilliseconds / 1000;
                Check(income > Amount.Zero, "Cycle " + result.cycle + " cached full-run income remains positive");
                timer.Restart(); string json = JsonUtility.ToJson(engine.State); timer.Stop(); result.encodeMs = timer.Elapsed.TotalMilliseconds;
                result.fullRunBytes = Encoding.UTF8.GetByteCount(json);
                timer.Restart(); var decoded = JsonUtility.FromJson<RedesignState>(json); timer.Stop(); result.decodeMs = timer.Elapsed.TotalMilliseconds;
                Check(result.fullRunBytes < 750000 && decoded.Valid(engine.Data), "Cycle " + result.cycle + " full-run payload roundtrips within cloud guard");
                router.DismissNotice(); router.Navigate("prestige", ""); yield return null;
                var before = engine.State.completedPrestiges;
                timer.Restart(); RedesignClick(router.prestige.confirm); timer.Stop(); result.prestigeMs = timer.Elapsed.TotalMilliseconds;
                Check(engine.State.completedPrestiges.CompareTo(before + (Natural)1) == 0 && engine.State.upgrades.Count == 0 && engine.State.UniqueCount == 1000, "Cycle " + result.cycle + " resets run and retains lifetime collection");
                double revealTime = 0;
                for (int draw = 0; draw < 3; draw++)
                {
                    router.DismissNotice(); router.Navigate("talent", ""); yield return null;
                    timer.Restart(); RedesignClick(router.talent.freeDraw); timer.Stop(); revealTime += timer.Elapsed.TotalMilliseconds;
                    yield return null;
                    timer.Restart(); RedesignClick(router.reveal.done); timer.Stop(); revealTime += timer.Elapsed.TotalMilliseconds;
                    Check(!engine.State.reveal.pending, "Cycle " + result.cycle + " reveal " + (draw + 1) + " acknowledges");
                }
                result.threeRevealsMs = revealTime;
                router.DismissNotice(); router.Navigate("home", ""); yield return null;
                RedesignClick(router.home.firstBuy);
                timer.Restart(); Check(router.Session.Run(e => e.Save()), "Cycle " + result.cycle + " rebuilt run saves"); timer.Stop(); result.saveMs = timer.Elapsed.TotalMilliseconds;
                result.postResetBytes = checked((int)new FileInfo(Path.Combine(RedesignSmokeProfile.Root, "rocket-raja-v2.save")).Length);
                yield return null;
                result.managedAfterGc = GC.GetTotalMemory(true);
                result.unityAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong();
                samples.cycles[run] = result;
            }
        }
    }
}
#endif
