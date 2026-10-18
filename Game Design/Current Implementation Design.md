# Rocket Raja: current implementation design

Confirmed by the user on **7 October 2026**: the current Unity implementation is the intended design. This document supersedes conflicting rules in the 1 October drafts and earlier checklists. It records the accepted behavior; it does not claim new device, billing, account-service or balance acceptance.

Numerical economy rules are now defined by [Faster-start Economy](Faster-start%20Economy.md), implemented after the initial cross-check below.

## Cash upgrades and product levels

- There are **1,000 distinct cash upgrades**, purchased once per run. Prestige clears run ownership. Each stable ID contributes to lifetime discoveries U only on its first-ever purchase.
- A cash upgrade can be bought whenever it is unowned this run and the player can afford its current discounted price. **No product ownership, product level, family count, previous-upgrade, Fame or prestige-count gate applies.** Normal session/save/recovery restrictions still apply to all commands.
- Catalogue prerequisite fields remain historical authoring metadata. The importer reads family metadata but does not enforce purchase gates. Product links and groups have been removed.
- Any skill power rank can unlock its skill. Buying the first power rank owned for that skill starts it ready. Buying a lower rank later cannot downgrade its power or reset its active duration/cooldown. Duration/cooldown upgrades alone do not unlock a skill.
- Cumulative effects use the strongest purchased rank, not the number of ranks purchased: PC/TC/UC discounts and skill power/duration/cooldown. Ordinary profit multipliers still stack once per purchased entry. Buying a lower cumulative rank after a higher one can add a lifetime discovery without increasing that cumulative effect.
- Product and tap levels remain separate purchases with ×1/×10/×100/Max controls, initially set to Max. Initial stocking/training buys one level; subsequent purchases use the selected quantity. Product families still unlock sequentially when the previous family has at least one level. This product-level rule is separate from unrestricted cash upgrades.
- A fresh account has no products and 30 cash for the first manual Phuljhari purchase. Prestige grants the discounted first Phuljhari cost. Seed cash does not count as earned cash.

## Manual taps and skills

Every accepted manual gameplay tap pays full value, with **no per-second cap or tap cooldown**. The saved `nextManualTapAt` field is retained for compatibility and does not restrict taps. Menu-button touches, drags and blocked/recovery input remain distinct from accepted gameplay taps.

Tapping uses the direct multiplier and training-milestone formula in the faster-start design. Festival Rush, Helping Hands and Bulk Order retain their timing. Helping Hands uses its purchased automatic-tap rate; removing the manual limit does not change its rate. Skills and their cash improvements reset on prestige; permanent skill cards do not unlock skills by themselves.

## Classic UI, browsing and Buy All

Classic is the selected presentation, reached through Bootstrap → Main. Player, Products, Legacy and Shop tabs can collapse back to Home.

- Player shows the next ten unowned cash upgrades sorted by current price and stable ID. The full catalogue remains accessible in pages of 20.
- Catalogue controls are text search, category (All / Products / Tapping / Skills / Shop), and view (Available / Upcoming / Owned / Full). Products includes profit and cost entries; Shop includes global-profit and cash-upgrade discounts. Available includes unaffordable unowned entries; Upcoming has no entries under the current no-prerequisite rule.
- There is no product-family or skill-specific filter control in Classic, no named-upgrade details flow, and no player pin/goal-selection control. The Home prompt asks for the first Phuljhari purchase, then uses the fixed Phuljhari signature-discovery prompt.
- Player's **Buy all Upgrades** button immediately buys a global batch across all cash-upgrade categories. It does not require a review or inherit catalogue search/category/view filters. The command spends the current budget on unowned entries in ascending price order, recalculating later prices after discounts. It buys no product/tap levels and spends no KK or free credits.
- The engine's scoped quote API and the saved review dialog are implementation support, not the active Player purchase flow. Their presence does not make filtered review a current player requirement.
- Purchases and progression update values, artwork, local effects and applicable audio. Transient notification banners are disabled, including purchase and shop-stage notices. Retained decorations still advance at U = 0/100/250/500/750/1000; product looks still follow ownership and signatures I/IV/X. Prestige retains decorations and rebuilds product displays.

## Prestige, cards and offline earnings

The faster-start design defines separate, increasing product and tap Fame bonuses and targets based on total Fame. The 15 October calibration retains full upgrade/card effects and preserves existing 3.0.0 saves and in-progress targets. First prestige still grants exactly three free credits; later credits use run earnings and lifetime discoveries. The second completed prestige grants 100 KK once.

Talent contains 28 permanent cards, with 75% Common / 22% Rare / 3% Legendary odds, uniform within tier, no pity, and one immediate level per copy.

- **1× pull:** one free credit if available, otherwise 100 KK.
- **5× pull:** four free credits if available, otherwise 400 KK. No mixed credit/KK payment. Five results are committed atomically and presented through a persisted reveal queue.
- Free and paid pulls use the same pool. Automatic payment-source selection replaces the older proposal for separate free and paid controls.
- Real storefront purchases and account-funded batch authority remain unfinished release work. Production packs are currently disabled/unavailable; this design confirmation does not activate services.

Offline income remains 50% of departure normal product income for up to eight hours before card modifiers, credited once. Cards, Fame, KK, free credits, lifetime discoveries and earned decorations persist through prestige.

## Upgrade data locations

| File | Purpose |
|---|---|
| [1000 Unlockables Catalogue](1000%20Unlockables%20Catalogue.md) | Human-readable source: all 1,000 IDs, names, effects, continuous exponential prices. Its family-metadata column is not a purchase gate. |
| [Cash-only Price Table v1](Cash-only%20Price%20Table%20v1.md) | Compact price lookup by stable ID. |
| [RedesignContentCatalog.json](../Documentation/RedesignContentCatalog.json) | Structured export; the `upgrades` array contains all 1,000 entries. Also contains products, cards and packs. |
| [GameContent.asset](../Assets/Resources/GameContent.asset) | Unity's serialized content; the active list is `redesign.upgrades`. The older `catalog` section is not the current 1,000-upgrade economy. |
| [RedesignContentManifest.json](../Assets/Resources/RedesignContentManifest.json) | Content version and source hash only; not the upgrade list. |

Catalogue and Talent roster Markdown are hashed together by the importer. Editing either source, including prose, requires synchronizing the export, Unity content asset and manifest. The importer preserves explicit authored factors and validates the continuous price curve.

## Evidence and pacing

The 7 October cross-check passed 713 core checks and 66 Classic draw/persistence checks. The core tests explicitly expect unrestricted cash upgrades and unlimited manual taps. All 1,000 Unity upgrade IDs matched the structured export.

The original model/report preserves the earlier prerequisite and interaction assumptions. Its 75/86/98-day completion cohort and first-prestige/rebuild timings are **historical simulation results, not validated forecasts for this implementation**. Unlimited tapping, out-of-order upgrades, Classic interaction time and discounted five-card pulls are included in the new engine-driven simulation report. The old research, model and test reports remain evidence of their recorded versions.
