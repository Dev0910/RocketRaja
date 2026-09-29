# Rocket Raja: implemented economy audit

**Scope.** This is a read-only account of the economy implemented in the Unity project at economy version `1.0.0`. It describes the authored catalog, the engine rules, and both the saved and freshly rerun deterministic simulation. It is a baseline for design decisions, not a claim that a human player experiences the simulator's pacing. Runtime loads the authored [`GameContent.asset`](../Assets/Resources/GameContent.asset) through [`PlayerSessionHost`](../Assets/Game/Runtime/Cloud/PlayerSessionHost.cs#L45); [`Catalog.Create()`](../Assets/Game/Core/Catalog.cs#L61) defines the defaults used to create that asset. The exported [`ContentCatalog.json`](../Documentation/ContentCatalog.json) is the simulator's catalog snapshot.

## 1. The run loop

1. A new run begins with ₹0, no owned product families, tap level 0, no run upgrades, and no active skills. Tapping grants ₹1 before modifiers. [`GameState`](../Assets/Game/Core/GameState.cs#L14), [`GameEngine.TapIncome`](../Assets/Game/Core/GameEngine.cs#L41)
2. The player buys tap levels and eight automatic income products. A product unlocks when the immediately preceding family has at least one level; there is no separate cash or player-level unlock check. [`GameEngine.ProductQuote`](../Assets/Game/Core/GameEngine.cs#L77)
3. Players buy repeatable levels, one-time run upgrades, and three active skills. Normal product income accrues with elapsed play time. Manual and skill-generated taps add discrete or automatic tap income. [`GameEngine.Advance`](../Assets/Game/Core/GameEngine.cs#L51)
4. After owning all eight families and earning at least ₹170 quadrillion in eligible income during the run, the player can prestige for reputation. Prestige resets the run economy and retains reputation, Legacy perks, and lifetime statistics. [`GameEngine.PreviewPrestige`](../Assets/Game/Core/GameEngine.cs#L163), [`GameEngine.Prestige`](../Assets/Game/Core/GameEngine.cs#L169)
5. Reputation buys permanent Legacy ranks. Each rank changes future runs through a multiplier, offline cap, or skill cooldown. [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L73), [`GameEngine.BuyLegacy`](../Assets/Game/Core/GameEngine.cs#L183)

The engine allows a maximum of 100,000 levels per product and 100,000 tap levels. Its numeric `Amount` type supports values beyond ordinary floating point magnitude; that cap does not mean content is authored across the whole range. [`Catalog`](../Assets/Game/Core/Catalog.cs#L50), [`Architecture.md`](../Documentation/Architecture.md)

## 2. Products, unlocks, and costs

Every successive product has **8× the base cost and 8× the base income** of its predecessor. Thus every family's first unmodified level has the same theoretical purchase payback, `cost / income = 42.86 seconds`. These values are the catalog bases, before milestones and upgrades. [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L67), [`GameContent.asset`](../Assets/Resources/GameContent.asset)

| Unlock order | Product | First-level cost | First-level income per second |
|---:|---|---:|---:|
| 1 | Phuljhari | ₹120 | ₹2.8 |
| 2 | Anar | ₹960 | ₹22.4 |
| 3 | Chakri | ₹7,680 | ₹179.2 |
| 4 | Rockets | ₹61,440 | ₹1,433.6 |
| 5 | Small crackers | ₹491,520 | ₹11,468.8 |
| 6 | Cracker strings | ₹3,932,160 | ₹91,750.4 |
| 7 | Aerial shots | ₹31,457,280 | ₹734,003.2 |
| 8 | Celebration boxes | ₹251,658,240 | ₹5,872,025.6 |

For a family with base cost `C`, current level `n`, and `q` levels purchased together, the engine charges the geometric sum

```text
cost(n, q) = C × product-discount-multiplier × 1.12^n
             × (1.12^q − 1) / 0.12
```

The next single level therefore costs `C × discount × 1.12^n`. Each of the two product discounts multiplies future level prices by `0.8`; both together yield `0.64` of the undiscounted price. Buy ×1, ×10, ×100, and Buy MAX use the same quote calculation. [`GameEngine.ProductCost`](../Assets/Game/Core/GameEngine.cs#L72), [`GameEngine.ProductQuote`](../Assets/Game/Core/GameEngine.cs#L77), [`ImportedProductsScreen`](../Assets/Game/Runtime/ImportedProductsScreen.cs#L90)

## 3. Production and milestones

For product `i` at level `n`, normal production is:

```text
product income/s = base income × n × 2^(milestone count)
                   × owned product-run multipliers
                   × owned all-product-run multipliers
                   × 2^(that product's Legacy rank)
                   × 2^(global product Legacy rank)
```

The milestone count rises at levels **25, 50, 100, 200, 300, ...**. As a result, the automatic milestone multiplier is ×2 at level 25, ×4 at 50, ×8 at 100, and ×16 at 200. `VisualTier` tracks thresholds **1, 25, 50, and 100**, but the current imported Home uses one sprite per product and one stall background: crossing a tier pulses the existing sprite rather than swapping artwork. [`GameEngine.ProductIncome`](../Assets/Game/Core/GameEngine.cs#L35), [`ProductMilestones`](../Assets/Game/Core/ProductMilestones.cs#L8), [`ImportedHomeScreen`](../Assets/Game/Runtime/ImportedHomeScreen.cs#L195), [`BuildImportedHome`](../Assets/Game/Editor/BuildImportedHome.cs#L52)

For example, Phuljhari at level 100, with no purchased run or Legacy upgrades, earns `₹2.8 × 100 × 8 = ₹2,240/s`. If its first four profit upgrades are purchased, their combined ×24 multiplier brings it to `₹53,760/s`. [`Catalog` profit tiers](../Assets/Game/Core/Catalog.cs#L79)

`NormalIncome` sums all eight product incomes. Festival Rush multiplies this total while active. The optional sponsor ad adds a separate equal amount for five minutes. The sponsor bonus is excluded from prestige-eligible earnings; ordinary product output and Festival Rush earnings are eligible. [`GameEngine.NormalIncome` and `LiveIncome`](../Assets/Game/Core/GameEngine.cs#L40), [`GameEngine.Advance`](../Assets/Game/Core/GameEngine.cs#L52), [`GameEngine.RewardAd`](../Assets/Game/Core/GameEngine.cs#L203)

## 4. Run upgrades and active skills

There are **96 one-time run upgrades**: 64 product upgrades, 12 tap upgrades, eight all-product upgrades, and 12 skill ranks. They reset at prestige. The engine buys an individually selected upgrade or all affordable available upgrades in ascending price order. [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L77), [`GameEngine.BuyAll`](../Assets/Game/Core/GameEngine.cs#L145), [`GameEngine.Prestige`](../Assets/Game/Core/GameEngine.cs#L177)

| Type | Count | Contents and price rule |
|---|---:|---|
| Product profit | 48: six per product | Gift Packaging, Festival Display, Regular Customers: ×2 each. Signature Collection, Neighbourhood Favourite, Celebration Specialist: ×3 each. Their catalog level landmarks are 10, 25, 50, 100, 175, 250; price is `5 × base product cost × 1.12^landmark`. All six yield ×216 for one product. |
| Product discounts | 16: two per product | Wholesale Deal and Trusted Supplier each multiply that family's level prices by 0.8. Catalog level landmarks are 40 and 150; prices are `base cost × 10^2.8` and `base cost × 10^7.8`. |
| Tap profit | 12 | Salesmanship 1–12 each doubles earnings per tap, up to ×4,096. Tier `t` costs `₹900 × 10^(0.82t)`; Salesmanship 1 is ₹900. |
| All-product profit | 8 | Festival Footfall 1–8 each doubles **product** income, up to ×256. Its price is 30 times the corresponding family's base cost; the first costs ₹3,600. |
| Active skill ranks | 12: four per skill | Each skill's first rank costs ₹2,500, ₹7,500, or ₹22,500 respectively; successive ranks cost ×100 each. Ranks require the previous rank, and the first rank requires at least one, two, or three owned families respectively. |

**The catalog's `requiredLevel` values are price/content landmarks, not enforced purchase gates.** [`GameEngine.Unlocked`](../Assets/Game/Core/GameEngine.cs#L133) checks only skill rank and product-count requirements. Every non-skill upgrade can be bought when affordable, even before its named level or product ownership. The economy test explicitly asserts this current rule in [`Program.cs`](../Tools/EconomyLab/Program.cs#L65). This matters when interpreting the upgrade names as milestone promises.

| Skill | Rank values | Duration / cooldown | Engine effect |
|---|---|---|---|
| Festival Rush | ×2, ×2.5, ×3, ×4 | 30 seconds / 300 seconds | Multiplies automatic product income while active. |
| Helping Hands | 5, 8, 12, 16 auto taps per second | 30 seconds / 300 seconds | Adds `tap income × auto taps/s` while active. |
| Bulk Order | 120, 180, 240, 300 seconds of sales | Instant / 600 seconds | Grants that many seconds of current normal product income at activation. |

Skill data is authored in [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L63); activation is implemented in [`GameEngine.ActivateSkill`](../Assets/Game/Core/GameEngine.cs#L153). A max-rank cooldown Legacy perk reduces the listed cooldowns by 25%.

## 5. Tapping and attention value

Manual tapping gives the following amount per tap:

```text
tap income = (1 + 1.4 × tap level)
             × 2^(Salesmanship upgrades bought)
             × 2^(tap Legacy rank)
```

The next tap level costs `₹40 × 1.17^(current tap level)`. Tap upgrades are multiplicative, but purchased tap levels increase the base **linearly** while their cost grows geometrically. At level 0, three taps per second produce ₹3/s. The first tap-level purchase costs ₹40 and adds ₹1.4 per tap, or ₹4.2/s at three taps per second. [`GameEngine.TapIncome`](../Assets/Game/Core/GameEngine.cs#L41), [`GameEngine.TapCost`](../Assets/Game/Core/GameEngine.cs#L96)

Taps do **not** receive product milestones, Festival Footfall, Festival Rush, the product Legacy multipliers, or the sponsor product-income bonus. Helping Hands produces automatic taps, but uses the same per-tap value. Offline income is based on products only. [`GameEngine.ProductIncome`](../Assets/Game/Core/GameEngine.cs#L35), [`GameEngine.Advance`](../Assets/Game/Core/GameEngine.cs#L52), [`GameEngine.ResolveOffline`](../Assets/Game/Core/GameEngine.cs#L190)

An illustrative later-run state shows the scale difference. With tap level 60 and all 12 Salesmanship upgrades but no tap Legacy, each tap gives `(1 + 60 × 1.4) × 4,096 = ₹348,160`; three taps per second yield **₹1,044,480/s**. A *single* level-one Celebration box with all eight Festival Footfall upgrades gives `₹5,872,025.6 × 256 = ₹1,503,238,553.6/s`. Tapping is then **0.0695% of that one product's output**. This is a formula comparison, not an observed player build; adding levels, product upgrades, or more owned products makes the product side larger still.

## 6. Prestige, Legacy, and offline progress

Prestige requires two conditions together: ownership of every product family and at least **₹1.7 × 10^17** in *eligible earnings accumulated this run*. Spending cash does not reduce eligible earnings. The reward is:

```text
reputation = min(1,000,000, floor(5 × sqrt(eligible run earnings / 1.7e17)))
```

Below the threshold or before all eight products are owned, the reward is zero. The threshold gives five reputation. At 4×, 16×, and 100× the threshold the reward is respectively 10, 20, and 50. The preview is bound to a run ID, state revision, and reward; a stale preview cannot be redeemed. [`GameEngine.PreviewPrestige`](../Assets/Game/Core/GameEngine.cs#L163), [`GameEngine.Prestige`](../Assets/Game/Core/GameEngine.cs#L169)

| Resets | Persists |
|---|---|
| Cash; current run and eligible earnings; product and tap levels; run upgrades; active skills and cooldowns; sponsor time; pending offline offer; run timer; the product and stall appearance generated from ownership/levels | Unspent and lifetime reputation; Legacy ranks; prestige count; lifetime earnings and manual tap count; store name; settings |

The reset and persisted fields come from [`GameEngine.Prestige`](../Assets/Game/Core/GameEngine.cs#L175) and [`GameState`](../Assets/Game/Core/GameState.cs#L14). The prestige screen also discloses reset and keep categories; its dynamic preview shows the reputation amount and eligibility, without a numerical projection of the next run's speed. [`BuildImportedPrestige.cs`](../Assets/Game/Editor/BuildImportedPrestige.cs#L97), [`ImportedPrestigeScreen`](../Assets/Game/Runtime/ImportedPrestigeScreen.cs#L68)

| Permanent Legacy perk | Ranks | Per-rank effect | Rank costs, reputation |
|---|---:|---|---|
| Eight named product expertise perks, one per family | 0–5 each | ×2 income for that product per rank, up to ×32 | 1, 2, 4, 8, 16 each |
| Natural salesperson | 0–5 | ×2 tap income per rank, up to ×32 | 1, 2, 4, 8, 16 |
| Trusted festival brand | 0–5 | ×2 income for **all products** per rank, up to ×32 | 3, 6, 12, 24, 48 |
| After-hours business | 0–5 | +2 hours offline cap per rank, from 8 to 18 hours | 1, 2, 4, 8, 16 |
| Experienced team | 0–5 | 5% shorter skill cooldowns per rank, up to 25% | 1, 2, 4, 8, 16 |

All 60 permanent ranks require **434 reputation total**: 11 ordinary perks at 31 each, plus the 93-point global product perk. No other reputation spending path appears in the current game code; after all ranks are maxed, additional prestige rewards increase balances and statistics without buying another permanent effect. [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L73), [`GameEngine.LegacyCost`](../Assets/Game/Core/GameEngine.cs#L183), [`ImportedLegacyScreen`](../Assets/Game/Runtime/ImportedLegacyScreen.cs#L65)

Offline return income is `normal product income × 50% × min(elapsed time, cap)`. The base cap is eight hours, increased by two hours per After-hours business rank. It excludes manual taps, Helping Hands, Festival Rush, and sponsor production. A rewarded offline ad can grant one additional matching offline payout; that ad bonus is not prestige-eligible. [`GameEngine.ResolveOffline`](../Assets/Game/Core/GameEngine.cs#L190), [`GameEngine.RewardAd`](../Assets/Game/Core/GameEngine.cs#L203)

## 7. What the existing simulator shows, and the current pacing failure

The repository contains a deterministic economy simulator and its exported results. The active policy taps **three times per second**, buys all affordable run upgrades, buys tap/product levels by marginal income payback, stops investing in tap levels at 60, and activates ready skills. After prestige, it buys global product Legacy first, then tap Legacy, then product Legacy from the last family backward. Its occasional and offline policies play for two minutes every 30 minutes; the offline policy closes the game between sessions and receives the implemented 50% return income. [`EconomyLab Program.cs`](../Tools/EconomyLab/Program.cs#L124), [`Balance.md`](../Documentation/Balance.md)

The repository's saved [EconomySimulation.json](../Documentation/EconomySimulation.json) reports:

| Saved policy | First prestige | Second run | Third run | First-run reputation |
|---|---:|---:|---:|---:|
| Active | 24:35 | 9:18 | 5:55 | 5 |
| Occasional | 1:30:01 wall time | 1:00:00 | 0:30:00 | 6 |
| Offline | 1:30:01 wall time | 1:00:00 | 0:30:48 | 5 |

**Fresh verification on 30 September 2026 did not reproduce the saved values.** Running `dotnet run --project Tools/EconomyLab -- --catalog Documentation/ContentCatalog.json --simulate` against the current code stops at [`Program.cs:165`](../Tools/EconomyLab/Program.cs#L165) with `First-run pacing target failed`. The no-catalog default run stops at the same assertion. In a temporary diagnostic copy of the simulator with only its pacing assertions replaced by logging, the current exported catalog produced the following **active-policy** trace; that copy was removed after the run:

| Fresh active trace | First prestige | Second run | Third run | First-run reputation |
|---|---:|---:|---:|---:|
| Current code + exported catalog | **46:26** | **12:36** | **9:10** | 5 |

In the fresh trace the first product appears at **0:48**, the first level-25 milestone at **4:12**, and all eight families at **7:31**. The current scripted first run exceeds the simulator's 20–30 minute assertion, and the second run exceeds its 5–10 minute assertion. The original `--simulate` command therefore does **not** pass today. The saved occasional/offline figures above were not freshly rerun and should be treated as historical artifacts. None of these traces establish average human times, attention, understanding, enjoyment, or retention. Three full uncoached human prestige cycles remain an explicit validation need in [`Validation.md`](../Documentation/Validation.md#L30).

## 8. Design gaps visible from the rules

These are observations and hypotheses for player testing, not changes already made to the game.

1. **Earlier families lose contribution at comparable levels.** With one unupgraded level in each family, total production is ₹6,710,886/s. Celebration boxes provide **87.5%**, while Phuljhari provides **0.0000417%**. At equal levels and equal applicable upgrade ranks, each product remains eight times the preceding one's output. Milestones temporarily change their relative positions, but there is no rule linking an early family's output to later family ownership or creating an early-family catch-up effect. [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L67), [`GameEngine.ProductIncome`](../Assets/Game/Core/GameEngine.cs#L35)
2. **Tapping and automatic production diverge.** Product income gets levels, milestones, product upgrades, eight global run upgrades, its own product Legacy rank, the global product Legacy rank, Festival Rush, sponsor boosts, and offline accrual. Tapping gets linear tap levels, Salesmanship, and one permanent tap perk. Its separate formula explains why a large tap popup can still represent a negligible fraction of production. [`GameEngine`](../Assets/Game/Core/GameEngine.cs#L35)
3. **Prestige primarily repeats existing numerical bonuses.** The first reward is actionable: five reputation can buy the three-point global product perk and two one-point ranks. Later ranks repeat the same effects with doubled costs. The game currently has no prestige-only activity, new product behavior, card discovery, or permanent visual transformation. The imported Home shows owned product sprites, but tier changes do not swap their artwork and `StallTier` is unused by the runtime UI. [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L73), [`ImportedHomeScreen`](../Assets/Game/Runtime/ImportedHomeScreen.cs#L195), [`GameEngine.Prestige`](../Assets/Game/Core/GameEngine.cs#L177)
4. **Permanent advancement has a finite endpoint.** Every Legacy perk caps at rank five. The total permanent catalog costs 434 reputation, yet a single prestige can award up to one million. Once all ranks are purchased, a player can continue resetting but cannot invest that currency further. [`LegacyDef`](../Assets/Game/Core/Catalog.cs#L14), [`GameEngine.PreviewPrestige`](../Assets/Game/Core/GameEngine.cs#L163)
5. **Upgrade landmarks do not govern unlock timing.** Profit, discount, tap, and global upgrade entries carry `requiredLevel` values, but the actual unlock rule does not read that field for non-skills. This offers freedom to spend cash early, while weakening the connection between reaching a named product level and unlocking a new choice. The project test confirms that this is the presently expected behavior. [`GameEngine.Unlocked`](../Assets/Game/Core/GameEngine.cs#L133), [`EconomyLab Program.cs`](../Tools/EconomyLab/Program.cs#L65)
6. **Authored content ends long before the numeric cap.** Product upgrades stop at the level-250 landmark, tracked visual-tier thresholds at 100, and permanent perks at rank five. Milestone doubling continues every 100 levels until the level cap, so late runs can keep growing numerically without a corresponding stream of authored decisions. The current imported artwork does not change at any visual-tier threshold. [`Catalog.Create`](../Assets/Game/Core/Catalog.cs#L59), [`ProductMilestones`](../Assets/Game/Core/ProductMilestones.cs#L8), [`ImportedHomeScreen`](../Assets/Game/Runtime/ImportedHomeScreen.cs#L195)
7. **The game shows amounts more readily than tradeoffs.** The Products panel shows each family's income per second and next milestone, but not its share of total income or purchase payback. The Player panel shows tap value and purchase price, but not tap contribution relative to automatic income. Prestige shows reward and reset terms, but not projected rebuilding speed. Those omissions could make otherwise functioning upgrades feel smaller than they are; that is a testable interface hypothesis. [`ImportedProductsScreen`](../Assets/Game/Runtime/ImportedProductsScreen.cs#L90), [`ImportedPlayerScreen`](../Assets/Game/Runtime/ImportedPlayerScreen.cs#L117), [`ImportedPrestigeScreen`](../Assets/Game/Runtime/ImportedPrestigeScreen.cs#L68)

The next balancing document should separate proposed changes from these implemented facts and compare several active, passive, short-session, and prestige-spending policies before altering authored values.
