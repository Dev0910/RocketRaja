using System;
using System.IO;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignDevelopmentShopValidation
    {
        public static string Run()
        {
            var report = new System.Collections.Generic.List<string>();
            void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); report.Add("PASS " + message); }
            var data = RedesignRuntimeContent.Load();
            RedesignState saved = null;
            RedesignState Codec(RedesignState state) => JsonUtility.FromJson<RedesignState>(JsonUtility.ToJson(state));
            var fresh = Codec(RedesignState.Fresh(1000));
            Check(fresh.Valid(data) && !fresh.HasDevelopmentShop, "JsonUtility empty inline test-shop object does not mark an ordinary guest save");
            var engine = new RedesignEngine(data, fresh, s => saved = Codec(s));
            Check(engine.EnableDevelopmentShop(DevelopmentShopState.Namespace) && saved.Valid(data), "Unity saves the explicit test marker");
            foreach (var pack in data.Packs)
            {
                var before = engine.State.kajuKatli;
                string operation = engine.BeginDevelopmentPurchase(pack.id);
                engine = new RedesignEngine(data, saved, s => saved = Codec(s));
                Check(engine.BeginDevelopmentPurchase(pack.id) == operation, pack.id + " pending operation survives Unity codec");
                Check(engine.ResolveDevelopmentPurchase(operation, DevelopmentPurchaseOutcome.Delivered) && saved.Valid(data) && saved.kajuKatli.Value == before.Value + pack.kajuKatli &&
                    !engine.ResolveDevelopmentPurchase(operation, DevelopmentPurchaseOutcome.Delivered), pack.id + " grant persists exactly once through Unity codec");
            }
            Check(!engine.PreparePremiumProfile(), "Unity test profile cannot prepare live premium ownership");
            report.Add("Scope: Unity editor/JsonUtility tests of simulated development purchases; no Google Play integration or native shop UI acceptance.");
            string text = string.Join("\n", report) + "\n";
            File.WriteAllText("Documentation/Implementation/Unity-Development-Shop.txt", text);
            return text;
        }
    }
}
