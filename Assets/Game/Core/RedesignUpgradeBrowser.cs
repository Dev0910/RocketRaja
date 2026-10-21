using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketRaja.Core
{
    public enum UpgradeCategory { All, Products, Tapping, Skills, Shop }
    public enum UpgradeView { Available, Upcoming, Owned, Full }

    /// <summary>Complete catalogue projection. Cash ticks do not rebuild membership.</summary>
    public sealed class RedesignUpgradeBrowser
    {
        readonly RedesignEngine engine;
        readonly CashUpgrade[] ordered;
        readonly CashUpgrade[] matches;
        readonly HashSet<string> owned = new HashSet<string>(StringComparer.Ordinal);
        readonly HashSet<string> ever = new HashSet<string>(StringComparer.Ordinal);
        UpgradeScope previous;
        UpgradeView previousView;
        string previousRevision;
        public UpgradeScope Scope { get; } = new UpgradeScope();
        public UpgradeView View { get; set; }
        public int Count { get; private set; }
        public int RebuildCount { get; private set; }
        public CashUpgrade this[int index] => index >= 0 && index < Count ? matches[index] : throw new ArgumentOutOfRangeException(nameof(index));
        public RedesignUpgradeBrowser(RedesignEngine engine)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
            ordered = engine.Data.Upgrades.OrderBy(x => x.price).ThenBy(x => x.id, StringComparer.Ordinal).ToArray();
            matches = new CashUpgrade[ordered.Length];
        }
        public bool Refresh()
        {
            if (previous != null && previousRevision == engine.State.commandRevision.digits && previousView == View &&
                previous.category == Scope.category && previous.effect == Scope.effect && previous.product == Scope.product &&
                previous.skill == Scope.skill && previous.search == Scope.search) return false;
            owned.Clear(); ever.Clear();
            owned.UnionWith(engine.State.upgrades); ever.UnionWith(engine.State.everPurchased);
            int oldCount = Count; Count = 0;
            foreach (var item in ordered)
            {
                if (!Scope.Matches(item)) continue;
                bool isOwned = owned.Contains(item.id);
                if (View == UpgradeView.Owned && !isOwned || View == UpgradeView.Available && (isOwned || !engine.Eligible(item.id)) ||
                    View == UpgradeView.Upcoming && (isOwned || engine.Eligible(item.id))) continue;
                matches[Count++] = item;
            }
            if (oldCount > Count) Array.Clear(matches, Count, oldCount - Count);
            previous = Scope.Copy(); previousView = View; previousRevision = engine.State.commandRevision.digits;
            RebuildCount++;
            return true;
        }
        public bool Owned(string id) => owned.Contains(id);
        public string PurchaseKind(string id) => owned.Contains(id) ? "Owned" : ever.Contains(id) ? "Rebuy" : "First purchase";
        public CashBatchQuote ReviewBatch() => engine.QuoteBuyAll(Scope);
    }
}
