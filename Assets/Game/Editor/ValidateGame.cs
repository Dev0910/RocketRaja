using System;
using System.Collections.Generic;
using System.IO;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class ValidateGame
    {
        [MenuItem("Rocket Raja/Validate Unity Save Codec")]
        public static void Run()
        {
            RedesignCoreValidation.Run();
        }
        [MenuItem("Rocket Raja/Validate Legacy Save Fixtures")]
        public static void RunLegacy()
        {
            var checks = new List<string>();
            void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL " + name); checks.Add("PASS " + name); }
            string folder = Path.Combine(Application.temporaryCachePath, "DiwaliSaveQA-" + Guid.NewGuid().ToString("N"));
            string file = Path.Combine(folder, "save");
            var store = new SaveStore(file, JsonUtility.ToJson, JsonUtility.FromJson<GameState>);
            var e = new GameEngine(Catalog.Create(), new GameState()); e.CriticalSave += () => store.Save(e.State);
            e.State.cash = new Amount(3.45678901234567, 450); e.State.reputation = e.State.lifetimeReputation = 100;
            Check(e.BuyLegacy(9) && store.Load().Legacy(9) == 1, "Legacy command saves immediately through Unity JsonUtility");
            var loaded = store.Load(); Check(loaded.Valid() && Math.Abs(Amount.Ratio(loaded.cash, e.State.cash) - 1) < 1e-12, "Large numbers roundtrip through Unity codec");
            for (int i = 0; i < 8; i++) e.BuyProduct(i, 300); e.State.tapLevel = 100; e.BuyAll(); store.Save(e.State);
            Check(store.Load().upgrades.Count == 96, "All stable upgrade IDs survive Unity serialization");
            e.State.now = e.State.savedAt = 1000; e.ActivateSkill(0);
            Check(store.Load().skills[0].readyAt == e.State.skills[0].readyAt, "Active skill cooldown saved immediately");
            e.ResolveOffline(4600); loaded = store.Load(); var before = loaded.cash;
            var resumed = new GameEngine(e.Data, loaded); resumed.ResolveOffline(4600);
            Check(Math.Abs(Amount.Ratio(before, resumed.State.cash) - 1) < 1e-12, "Reload and repeated offline reconciliation never duplicate income");
            e.State.eligibleEarnings = Catalog.PrestigeThreshold; var preview = e.PreviewPrestige(); e.Prestige(preview); loaded = store.Load();
            Check(loaded.OwnedProducts == 0 && loaded.Legacy(9) == 1 && loaded.reputation == e.State.reputation && loaded.prestiges == 1, "Prestige atomically persists reset and retained Legacy");
            store.Save(e.State); File.WriteAllText(file, "invalid main save");
            Check(store.Load() != null && store.Warning != null, "Unity save loads validated backup after main corruption");
            File.WriteAllLines("Documentation/UnitySaveTests.txt", checks); Debug.Log("DIWALI_UNITY_SAVE_TESTS_OK");
        }
    }
}
