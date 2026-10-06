using System;

namespace RocketRaja.Core
{
    public sealed partial class RedesignEngine
    {
        // -1 means unowned; tiers 0..3 are ownership and current-run signatures I, IV, X.
        // Three indexed lookups reuse the same owned-ID cache as economic effects.
        public int ProductAppearance(int product)
        {
            if (product < 0 || product >= 8) throw new ArgumentOutOfRangeException(nameof(product));
            if (State.products[product] == 0) return -1;
            for (int tier = 3; tier >= 1; tier--)
                if (Effects.owned.Contains(Data.AppearanceUpgrade(product, tier))) return tier;
            return 0;
        }
    }
}
