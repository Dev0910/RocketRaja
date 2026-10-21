using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignCoreValidation
    {
        [MenuItem("Rocket Raja/Redesign/Validate Core And Save Codec")]
        public static void Run()
        {
            var results = new List<string>();
            void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); results.Add("PASS " + message); }
            var data = new RedesignCatalog(AssetDatabase.LoadAssetAtPath<GameContent>("Assets/Resources/GameContent.asset").redesign);
            string root = Path.Combine(Application.temporaryCachePath, "RedesignQA-" + Guid.NewGuid().ToString("N"));
            var store = new RedesignSaveStore(Path.Combine(root, "rocket-raja-v2.save"), data, JsonUtility.ToJson, JsonUtility.FromJson<RedesignState>, JsonUtility.FromJson<SaveVersionHeader>);
            var engine = new RedesignEngine(data, RedesignState.Fresh(1000), store.Save);
            Check(engine.BuyLevels(0, 1) && store.Load().state.products[0] == 1, "First purchase commits with Unity codec");
            var state = engine.State.Copy(); state.cash = new Amount(1, 450); state.products = Enumerable.Repeat(300, 8).ToArray();
            engine = new RedesignEngine(data, state, store.Save);
            var batch = engine.QuoteBuyAll();
            Check(batch.Ids.Count == 1000 && engine.CommitBuyAll(batch), "Unity kernel purchases full authored catalogue");
            Check(store.Load().state.upgrades.Count == 1000 && store.Load().state.UniqueCount == 1000, "All new IDs roundtrip through Unity");
            state = engine.State.Copy(); state.eligibleEarnings = state.target;
            engine = new RedesignEngine(data, state, store.Save);
            Check(engine.Prestige(engine.PreviewPrestige()), "Prestige reward/reset persist in one Unity write");
            var loaded = store.Load().state;
            Check(loaded.completedPrestiges.Value == 1 && loaded.fame.Value == 1 && loaded.freeCredits.Value == 3 && loaded.upgrades.Count == 0 && loaded.UniqueCount == 1000, "Reset retention and exact rewards survive Unity reload");
            engine = new RedesignEngine(data, loaded, store.Save);
            var draw = engine.QuoteDraw(true);
            Check(engine.Draw(draw) && store.Load().state.reveal.pending, "Draw result is saved before reveal");
            Check(!engine.Draw(draw), "Repeated draw quote cannot consume another credit");
            string revealed = engine.State.reveal.cardId;
            engine = new RedesignEngine(data, store.Load().state, store.Save);
            Check(engine.State.reveal.cardId == revealed && engine.DismissReveal(engine.State.reveal.operationId), "Reopen replays and skips existing reveal");
            Check(engine.BuyLevels(0, 1) && engine.Save() && engine.ResolveOffline(41000), "Unity offline settlement commits");
            var reward = engine.State.offline.amount;
            engine = new RedesignEngine(data, store.Load().state, store.Save);
            Check(!engine.ResolveOffline(41000) && engine.State.offline.amount.CompareTo(reward) == 0, "Repeated Unity resume cannot duplicate credit");
            var wallet = new PremiumWalletSnapshot {
                uid = "unity-wallet-fixture", profileId = engine.State.guestWallet.profileId, revision = "1", balance = "100", deficit = "0",
                cards = Enumerable.Repeat("0", 28).ToArray(), giftRegistered = false, importedGiftCardId = "", importedGiftOperationId = ""
            };
            wallet = JsonUtility.FromJson<PremiumWalletSnapshot>(JsonUtility.ToJson(wallet));
            Check(engine.ReconcilePremiumWallet(wallet.uid, wallet) && store.Load().state.schema == 3 && store.Load().state.premium.uid == wallet.uid, "Unity schema 3 preserves an authoritative wallet and decimal-string wire counters");
            var paid = engine.BeginCurrencyDraw();
            Check(paid != null && store.Load().state.premium.pendingOperationId == paid.OperationId, "Unity codec persists the paid operation before network transmission");
            var beforePaid = engine.State.cardLevels[0]; wallet.revision = "2"; wallet.balance = "0"; wallet.cards[0] = "1";
            var outcome = new CurrencyDrawOutcome { operationId = paid.OperationId, cardId = "C01", before = "0", after = "1", price = "100", revision = "2", wallet = wallet };
            outcome = JsonUtility.FromJson<CurrencyDrawOutcome>(JsonUtility.ToJson(outcome));
            Check(engine.ApplyCurrencyDraw(wallet.uid, outcome) && store.Load().state.reveal.pending && store.Load().state.cardLevels[0].Value == beforePaid.Value + 1, "Unity codec roundtrips server draw result and preserves free card copies");
            var packWire = JsonUtility.FromJson<PremiumPackReply>("{\"uid\":\"fixture\",\"productId\":\"rr.kk.small.1000\",\"fulfilled\":true,\"revision\":\"100000000000000000001\",\"granted\":\"1000\"}");
            Check(packWire.ToGrant().revision.digits == "100000000000000000001" && packWire.ToGrant().granted.Value == 1000, "Unity receipt wire codec keeps revisions beyond double integer precision exact");
            engine = new RedesignEngine(data, store.Load().state, store.Save);
            Check(engine.State.HasPremium && engine.State.premium.currencyCards[0].Value == 1 && engine.State.kajuKatli.IsZero && engine.QuoteDraw(false) == null, "Reopened authoritative Unity wallet cannot use local paid spending");
            Check(System.Text.Encoding.UTF8.GetByteCount(JsonUtility.ToJson(engine.State)) < 750000, "Full lifetime collection stays within cloud payload limit");
            Check(!EditorUtility.scriptCompilationFailed, "No Unity compilation failure");
            Check(ReferenceEquals(RedesignRuntimeContent.Load(), RedesignRuntimeContent.Load()), "Runtime catalogue validates once and reuses stable cache");
            var runtimeObject = new GameObject("Temporary Redesign Lifecycle Validation") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var driver = runtimeObject.AddComponent<RedesignRuntimeDriver>();
                Check(driver.InitializeGuest(Path.Combine(root, "runtime")), "Runtime driver opens isolated new-economy guest through Unity save codec");
                Check(driver.Command(e => e.BuyLevels(0, 1)), "Runtime commands bind the redesigned manual purchase");
                driver.Session.GameplayInputEnabled = false;
                var before = driver.Session.Engine.State.eligibleEarnings;
                driver.Session.Tick(10);
                Check(!driver.TapGameplaySurface() && driver.Session.Engine.State.eligibleEarnings > before, "Runtime browsing accrues production while blocking the gameplay input surface");
                Check(driver.Session.Suspend() && !driver.Command(e => e.BuyLevels(0, 1)), "Runtime suspension saves departure and gates spending");
                double resume = driver.Session.Engine.State.now + 100;
                Check(driver.Session.Resume(resume) && driver.Session.Engine.State.offline.pending, "Runtime resume durably settles a return before commands");
            }
            finally { UnityEngine.Object.DestroyImmediate(runtimeObject); }
            File.WriteAllLines("Documentation/Implementation/Unity-Core.txt", results);
            Debug.Log("REDESIGN_UNITY_CORE_OK " + results.Count);
        }
    }
}
