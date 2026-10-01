# Rocket Raja: recommendation coverage and remaining work

Updated: 1 October 2026. Planning only; no Unity changes.

This maps [Comparison and Recommendations](Comparison%20and%20Recommendations.md) and [Design Recovery Roadmap](Design%20Recovery%20Roadmap.md) to the redesign. Their implementation findings describe the earlier game. Current user decisions and the v1 redesign take precedence.

## Current status

The numerical authoring pass and paper flow review are complete. All 1,000 cash entries have IDs, names, numerical effects, prerequisites, and individual prices. Cash-only progression is now selected and is the active price/access/Fame baseline; the rejected milestone option remains historical comparison. Product growth, ordinary tapping, skill combinations, card levels, offline earnings, prestige rewards, free supply, and INR-first premium packs are specified as reviewable candidates.

The user selected x6 opening income, cash-only catalogue access, retained shop decorations with rebuilt product displays, and starting cash for the first manual Phuljhari purchase. The initial Kaju Katli draw cost, odds, gift, and INR packs are confirmed. Rewarded ads are excluded from the initial redesign. No mandatory discussion items remain from the latest review; other balance/UX details remain prototype defaults.

No UI prototype, artwork, store products, runtime integration, save migration, or observed player testing has been performed. Simulations do not establish retention or purchase appeal.

## Coverage of the recommendations

| Recommendation | Completed planning work | Remaining validation |
|---|---|---|
| Keep starting products useful | 480 product-profit purchases include 80 signature milestones; 32 links and 16 group purchases have effects and level gates. Contribution changes are modeled. | Observe whether comebacks create worthwhile choices rather than indistinguishable Buy All bursts. |
| Preserve ordinary tapping and tap investment | Direct growth, bounded production link/mastery, 160 tap-profit/cost purchases, manual quantity controls, and input-cap proposal are specified. No-tap policies are compared. | Actual input/accessibility behavior and marginal purchase value across player strategies. |
| Connect the three skills | All 72 skill purchases are priced; normal-income references, Rush tap bonus, Helping Hands overlap, and duration/cooldown combinations are explicit. | Comprehension and whether skill timing fits short visits. |
| Expose the whole upgrade set | One filtered Upgrades screen, all-entry inspection, first-purchase/rebuy states, comparisons, and scoped manual Buy All. | Prototype browsing, purchase effort, and prevention of accidental scope changes. |
| Show useful goals and contributions | Product shares, current→next effects, next requirements, pinned goals, and alternatives are specified. | Can players find a useful next action without constant prompts? |
| Make prestige worthwhile and understandable | Fame, fixed run target, next target, calculated draws, reset/keep summary, reveals, and x6 rebuild are specified and simulated. | Observe reset comprehension and the confirmed manual opening/rebuild flow. |
| Fill the stretch after product discovery | All 1,000 entries priced; first-day paths and later bands modeled. | Eight families appear by 6m 20s in the reference; longest early named-upgrade gap is about 13.7 active minutes. These are explicit prototype risks. |
| Make visual growth visible | Same-shop evolution confirmed; six permanent shop stages and four owned looks per product proposed. | Art production and motion/readability review under the confirmed retention rules. |
| Give repeat runs changing priorities | Card effects, income growth, deeper milestones, product comebacks, and lifetime discovery all persist or reshape purchases. | No assumption that 1,000 entries alone guarantee meaningful variety. |
| Establish an inspectable balance baseline | Executable model in Markdown, exact prices, structural checks, first/rebuild tests, 32 current free histories, paid/luck/behavior stress comparisons. | Runtime parity, numeric boundaries, human behavior, and wider strategy optimization. |

## Current artifact index

| Artifact | Purpose |
|---|---|
| [Redesign Working Plan](Redesign%20Working%20Plan.md) | Confirmed decisions and plan entry point. |
| [Economy Specification v1](Economy%20Specification%20v1.md) | Current cash-only numerical baseline. |
| [1000 Unlockables Catalogue](1000%20Unlockables%20Catalogue.md) | Every individual entry, effect, requirement, and selected cash-only price. |
| [Cash-only Economy Alternative v1](Cash-only%20Economy%20Alternative%20v1.md) | Cash-only decision record and historical comparison. |
| [Cash-only Price Table v1](Cash-only%20Price%20Table%20v1.md) | All 1,000 current prices, matched by ID; mirrors the catalogue. |
| [Premium Economy v1](Premium%20Economy%20v1.md) | Fixed draw price, INR pack draft, gift, and currency/draw flows. |
| [Economy Simulation Report v1](Economy%20Simulation%20Report%20v1.md) | Executed results, stress cases, checks, and limitations. |
| [Economy Model v1](Economy%20Model%20v1.md) | Reproduction source and policy inputs outside Unity. |
| [Planning Decisions and Review v1](Planning%20Decisions%20and%20Review%20v1.md) | Confirmed choices, prototype defaults, and review risks. |
| [Upgrade Browser and Goals Draft](Upgrade%20Browser%20and%20Goals%20Draft.md) | Navigation, purchase comparison, Buy All, and goals. |
| [Prestige and Talent Flow Draft](Prestige%20and%20Talent%20Flow%20Draft.md) | Reward previews, reset, draws, and rebuilding. |
| [Visual Progression Draft](Visual%20Progression%20Draft.md) | Shop/product visuals, milestones, motion, and asset scope. |
| [Onboarding and Return Flow Draft](Onboarding%20and%20Return%20Flow%20Draft.md) | Teaching, offline settlement, and future observation cases. |

The blueprint, product, active-play, and Talent drafts preserve the rationale and card roster. V1 numerical rules take precedence. The earlier 100,000-history collection study remains a fixed-draw discovery experiment; the later coupled models measure elapsed progression and reward supply.

## Numerical results in context

At the twenty-minute, three-visit daily midpoint, selected cash-only completion is day 75 / 86 / 98 at the sample 10th / median / 90th percentiles. Typical reset averages are 1.23 / 1.38 / 1.53 per day. The eight-hour offline cap makes less frequent attendance slower; deep runs and paid cards can finish earlier.

The opening reference reaches first prestige at eight elapsed mixed-play hours or about 2h 6m sustained. The cash-only guaranteed-rebuild diagnostic is about 3.92x faster with matched visits and 5.89x faster sustained, consistent with the selected x6 tradeoff. These are policy-dependent modeled outcomes, not promises for every player.

## Superseded or deferred recommendations

| Older suggestion | Current treatment |
|---|---|
| Preserve the twelve-perk Legacy tree and add a respec specialization | Replaced by permanent random cards and separate guaranteed income. |
| Prefer deterministic rewards over random cards | User chose random draws, immediate duplicate levels, and no pity; unlucky histories are tested within that choice. |
| Make all-eight level milestones the sole product repair | Replaced by signature, link, and group opportunities; no all-eight prestige ownership gate. |
| First prestige after 20–30 minutes | Replaced by 6–10 mixed elapsed hours / 2–3 sustained hours. |
| Add a reputation sink after the old 434-point tree | Specific to the retiring tree; new Fame and card progression are separately defined. |
| Add recurring objectives and premium rewards | Achievements and daily/weekly objectives remain later scope. |
| Country wars or multiplayer | No multiplayer commitment. |
| Sponsor/ad income | Excluded from the initial redesign by the user's decision; any later introduction is separate scope. |

## What remains

All mandatory discussion items from this review are resolved. The numerical baseline and experience drafts are ready for a separately authorized prototype. Remaining coefficients, exact visual thresholds, and detailed UX can be tested and tuned during that phase.

The later phase must test actual screens, short-session interaction time, old-product decisions, duplicate value, input behavior, large-number and purchase boundaries, source/runtime parity, and transaction/save behavior. Observe uncoached first, rebuild, and return sessions before treating the balance as ready to ship. Preserve existing save schemas and entitlements until a migration design is based on actual deployed usage.

This closes the previously missing authoring/modeling work. It does not authorize implementation or mark future prototype tests as already passed.
