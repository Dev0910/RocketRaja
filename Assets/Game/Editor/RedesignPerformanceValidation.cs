using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignPerformanceValidation
    {
        [MenuItem("Rocket Raja/Redesign/Profile Core In Editor")]
        public static void Run()
        {
            var data = RedesignRuntimeContent.Load();
            var state = RedesignState.Fresh(1000); state.cash = new Amount(1, 450); state.products = Enumerable.Repeat(300, 8).ToArray();
            var engine = new RedesignEngine(data, state, _ => { });
            var rows = new List<string> {
                "Unity editor CPU diagnostic; NOT phone performance or native list-scrolling acceptance.",
                "Unity=" + Application.unityVersion + "; processor=" + SystemInfo.processorType + "; content=" + data.SourceHash
            };
            void Measure(string label, int count, Action action)
            {
                action(); var timer = Stopwatch.StartNew();
                for (int i = 0; i < count; i++) action();
                timer.Stop(); rows.Add(label + ": count=" + count + "; total_ms=" + timer.Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "; mean_ms=" + (timer.Elapsed.TotalMilliseconds / count).ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
            }
            Measure("QuoteBuyAll_1000", 5, () => { if (engine.QuoteBuyAll().Ids.Count != 1000) throw new InvalidOperationException("Incomplete full-catalogue quote"); });
            engine.CommitBuyAll(engine.QuoteBuyAll());
            Measure("NormalIncome_cached", 1000, () => engine.NormalIncome());
            Measure("State_validation_1000", 20, () => { if (!engine.State.Valid(data)) throw new InvalidOperationException("Invalid full state"); });
            string json = JsonUtility.ToJson(engine.State);
            rows.Add("Full_run_save_utf8_bytes=" + Encoding.UTF8.GetByteCount(json) + "; supported_limit=750000");
            Measure("Json_encode_1000", 20, () => JsonUtility.ToJson(engine.State));
            Measure("Json_decode_1000", 20, () => JsonUtility.FromJson<RedesignState>(json));
            long before = GC.GetTotalMemory(true);
            Measure("Prestige_three_draws_rebuild", 10, () => {
                var s = engine.State.Copy(); s.eligibleEarnings = s.target;
                var cycle = new RedesignEngine(data, s, _ => { }); cycle.Prestige(cycle.PreviewPrestige());
                for (int draw = 0; draw < 3; draw++) { cycle.Draw(cycle.QuoteDraw(true)); cycle.DismissReveal(cycle.State.reveal.operationId); }
                if (!cycle.BuyLevels(0, 1)) throw new InvalidOperationException("Manual rebuild failed");
            });
            rows.Add("Editor_managed_retained_byte_delta_after_GC=" + (GC.GetTotalMemory(true) - before) + "; includes editor activity and is not a leak verdict");
            File.WriteAllLines("Documentation/Implementation/Unity-Core-Performance.txt", rows);
            UnityEngine.Debug.Log("REDESIGN_EDITOR_PERFORMANCE_CAPTURED");
        }
    }
}
