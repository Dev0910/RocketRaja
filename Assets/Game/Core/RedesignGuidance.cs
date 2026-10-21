using System;
using System.Collections.ObjectModel;

namespace RocketRaja.Core
{
    /// <summary>Read-only, contextual suggestions. Acknowledgement is a separate durable command.</summary>
    public static class RedesignGuidance
    {
        public static readonly ReadOnlyCollection<string> Topics = Array.AsReadOnly(new[] {
            "buy-first", "tap-counter", "product-levels", "catalogue-filters", "buy-all", "skill-basics", "comebacks",
            "prestige", "rebuild", "cash-and-talent", "talent-free", "talent-duplicates", "second-gift"
        });
        public static string Next(RedesignEngine engine, string route, string argument)
        {
            if (engine == null || engine.State.learnedTopics.Count >= 256) return null;
            var s = engine.State;
            string Unread(string id) => s.learnedTopics.Contains(id) ? null : id;
            switch (route)
            {
                case "home": return Unread(s.products[0] > 0 ? "tap-counter" : s.completedPrestiges.IsZero ? "buy-first" : "rebuild");
                case "productdetails":
                    if (!int.TryParse(argument, out int product) || product < 0 || product >= 8) return null;
                    if (product == 0 && s.products[0] == 0 && s.completedPrestiges.IsZero) return Unread("buy-first");
                    return Unread("product-levels");
                case "taptraining": return Unread("product-levels");
                case "upgrades": return s.products[0] > 0 ? Unread("catalogue-filters") : null;
                case "buyall": return Unread("buy-all");
                case "skilldetails": return int.TryParse(argument, out int skill) && skill >= 0 && skill < 3 ? Unread("skill-basics") : null;
                case "upgradedetails":
                    if (!engine.Data.TryUpgrade(argument ?? "", out int index)) return null;
                    var upgrade = engine.Data.Upgrades[index];
                    if (!upgrade.signature || s.upgrades.Contains(upgrade.id) || s.products[upgrade.target] == 0) return null;
                    for (int p = upgrade.target + 1; p < 8; p++) if (s.products[p] > 0) return Unread("comebacks");
                    return null;
                case "prestige": return Unread("prestige");
                case "talent":
                    if (s.reveal.pending) return null;
                    if (s.completedPrestiges.IsZero) return Unread("cash-and-talent");
                    if (s.secondPrestigeGift && Unread("second-gift") != null) return "second-gift";
                    return !s.freeCredits.IsZero ? Unread("talent-free") : null;
                case "drawreveal": return s.reveal.pending && !s.reveal.before.IsZero ? Unread("talent-duplicates") : null;
                case "kajushop": return s.secondPrestigeGift ? Unread("second-gift") : null;
                default: return null;
            }
        }
    }
}
