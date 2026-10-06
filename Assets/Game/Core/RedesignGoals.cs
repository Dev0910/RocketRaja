using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketRaja.Core
{
    public enum GoalTargetKind { Upgrade, Product, TapTraining }
    public sealed class GoalTarget
    {
        public GoalTargetKind kind;
        public string upgradeId;
        public int product = -1;
        public Amount cost;
    }
    public sealed partial class RedesignEngine
    {
        public bool PinUpgrade(string id)
        {
            if (id == null || id != "" && (!Data.TryUpgrade(id, out _) || State.upgrades.Contains(id)) || State.pinnedUpgradeId == id) return false;
            // Presentation preference does not invalidate an economic purchase/prestige quote.
            return Commit(s => { s.pinnedUpgradeId = id; return true; }, false);
        }
        public GoalTarget NextRequirement(string id)
        {
            if (!Data.TryUpgrade(id, out int i) || State.upgrades.Contains(id)) return null;
            var upgrade = Data.Upgrades[i];
            foreach (string prerequisite in upgrade.prerequisites)
                if (!State.upgrades.Contains(prerequisite)) return new GoalTarget { kind = GoalTargetKind.Upgrade, upgradeId = prerequisite, cost = UpgradeCost(prerequisite) };
            foreach (var requirement in upgrade.productRequirements)
                if (State.products[requirement.product] < requirement.level) return new GoalTarget { kind = GoalTargetKind.Product, product = requirement.product };
            if (State.products.Count(x => x > 0) < upgrade.requiredFamilies)
                for (int p = 0; p < 8; p++) if (State.products[p] == 0 && ProductAvailable(p)) return new GoalTarget { kind = GoalTargetKind.Product, product = p };
            return null;
        }
        public GoalTarget[] GoalAlternatives()
        {
            var result = new List<GoalTarget>();
            var upgrade = cheapest.FirstOrDefault(x => x.id != State.pinnedUpgradeId && Eligible(x.id));
            if (upgrade != null) result.Add(new GoalTarget { kind = GoalTargetKind.Upgrade, upgradeId = upgrade.id, cost = UpgradeCost(upgrade.id) });
            int product = -1; Amount lowest = default;
            for (int p = 0; p < 8; p++)
            {
                if (!ProductAvailable(p)) continue;
                var quote = LevelQuote(p, 1);
                if (quote.quantity > 0 && (product < 0 || quote.cost < lowest)) { product = p; lowest = quote.cost; }
            }
            if (product >= 0) result.Add(new GoalTarget { kind = GoalTargetKind.Product, product = product, cost = lowest });
            var tap = LevelQuote(-1, 1);
            if (tap.quantity > 0) result.Add(new GoalTarget { kind = GoalTargetKind.TapTraining, cost = tap.cost });
            return result.ToArray();
        }
    }
}
