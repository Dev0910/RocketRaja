# Rocket Raja: selected cash-only progression — decision record

1 October 2026. The user selected **cash-only progression**. This file retains its original filename so existing links keep working. No Unity changes.

## Selected rules

- All 1,000 cash upgrades can be bought when their cash, product, level, and prior-upgrade requirements are met.
- There are no completed-prestige requirements for catalogue access.
- Later price-band openings grow by x31 after band 3.
- Permanent income is x1 at zero Fame; otherwise **6 + 3 × (sqrt(F) − 1)**. The opening minimum remains x6; Fame 1/4/9/16 gives x6/x9/x12/x15.
- Prestige continues to award Fame and calculated free draws. T=min(1e36,1e10×1.5^N) still uses prior completed prestiges for the next-run earnings requirement. N does not gate cash upgrades.
- The main catalogue, specification, and simulator defaults now contain this same baseline. No alternative overrides or manual removal of row clauses is needed.

The direction is confirmed. Other numerical coefficients remain a tunable planning baseline. Opening/visual retention, the initial premium economy, and exclusion of rewarded ads are now also confirmed; see the decision register.

## Current sources

- [Economy Specification v1](Economy%20Specification%20v1.md): all shared and cash-only numerical rules.
- [1000 Unlockables Catalogue](1000%20Unlockables%20Catalogue.md): complete current entries and prices.
- [Cash-only Price Table v1](Cash-only%20Price%20Table%20v1.md): matching ID/price reference.
- [Economy Simulation Report v1](Economy%20Simulation%20Report%20v1.md): current cash-only results, policy/luck/paid comparisons, and limitations.
- [Economy Model v1](Economy%20Model%20v1.md): executable source; the default reference runs 150 days.

## Why this option was selected

Cash-only preserves the choice between pushing a run deeper and resetting for permanent power/cards. Steeper late prices and stronger later Fame growth support the three-month reference without making repeated resets a separate content-access requirement.

The rerun of 32 free-player histories completes on days **75 / 86 / 98** at the sample 10th / median / 90th percentiles. Deep-run strategies finish sooner: seed 1009 completes on day 66 at 4T and day 57 at 16T. Never prestiging reaches 896 entries on day 90 and remains incomplete by day 150. These are player-policy simulations, not observed retention or universal completion promises.

## Superseded option, retained for comparison only

| Historical option | Cash price growth after band 3 | Later Fame coefficient | Catalogue access | Simulated completion days: 10th / median / 90th |
|---|---:|---:|---|---|
| Prestige milestones — not selected | x22 | g=1 | First 145 entries before prestige; final band required 140 completed resets | 83 / 88 / 97 |
| Cash-only — selected | x31 | g=3 | Cash and individual prerequisites | 75 / 86 / 98 |

The earlier six-day no-prestige completion came from an older, shallower cash-only experiment. It does not describe the selected curve. Do not restore the historical gate schedule or mix its lower prices/weaker Fame with the selected cash-only baseline.

The x6 opening, three first-prestige free draws, later draw calculation using run earnings plus U, 28-card roster, manual Buy All, and normal offline rules are unchanged.
