# Rocket Raja: product progression draft

Updated: 1 October 2026. Planning only.

Cash-only progression is selected. [Economy Specification v1](Economy%20Specification%20v1.md) and the main catalogue contain the current prices and Fame formula, with no completed-prestige requirements for cash upgrades. The x6 opening bonus is retained. Numerical balance remains subject to prototype tuning; the [decision record](Cash-only%20Economy%20Alternative%20v1.md) preserves the superseded comparison.

Confirmed player experience: older products periodically become leading earners again after significant upgrades. New products should still feel rewarding. The allocations, milestone positions, example values, and comparison criteria below are proposals. This document does not establish economy balance or change Unity data.

## The investment loop

1. Unlock a new product with a competitive return on its first affordable investments.
2. Grow it through levels and supporting cash upgrades while another visible milestone approaches.
3. Reach an older family's signature upgrade, making renewed investment in that family attractive.
4. Use discounts, product links, and mixed-family goals to choose between concentrating spending and preparing another milestone.
5. Prestige for permanent growth, rebuild known purchases, and reach a deeper part of the same catalogue.

The leader changes through purchases. There is no timed rotation, random demand event, automatic catch-up calculation, or obligation to keep all eight families at the same level. Product-specific cards may change the best order. Signature upgrades remain manual purchases and can be included in Buy All.

## Signature milestones inside the existing catalogue

Keep 60 product-profit purchases per family. As an initial allocation, designate one purchase in each six-rank block as a signature milestone: ten per family, 80 total. The other 400 product-profit purchases provide supporting growth. This changes the role and name of selected entries; it does not create additional purchases beyond the existing 1,000.

The first signature purchase establishes the family's distinctive range and need not be a comeback. The other nine are authored opportunities to restore its earning importance as the frontier advances. That gives 72 later comeback opportunities across the eight families, subject to numerical tuning.

| Family | Signature PP ranks | Current price bands |
|---|---|---|
| Phuljhari | 01, 07, 13, 19, 25, 31, 37, 43, 49, 55 | Odd bands 01–19 |
| Anar | 04, 10, 16, 22, 28, 34, 40, 46, 52, 58 | Even bands 02–20 |
| Chakri | 02, 08, 14, 20, 26, 32, 38, 44, 50, 56 | Odd bands 01–19 |
| Rockets | 05, 11, 17, 23, 29, 35, 41, 47, 53, 59 | Even bands 02–20 |
| Small crackers | 03, 09, 15, 21, 27, 33, 39, 45, 51, 57 | Odd bands 01–19 |
| Cracker strings | 06, 12, 18, 24, 30, 36, 42, 48, 54, 60 | Even bands 02–20 |
| Aerial shots | 01, 07, 13, 19, 25, 31, 37, 43, 49, 55 | Odd bands 01–19 |
| Celebration boxes | 04, 10, 16, 22, 28, 34, 40, 46, 52, 58 | Even bands 02–20 |

Each current band contains four signatures. Entries at different PP ranks are separated by supporting purchases in the catalogue. However, price-key order alone does not establish meaningful time between them. Tune prices and positions so several signatures do not routinely collapse into one indistinguishable Buy All burst at the frontier. Rebuying a familiar cluster quickly after prestige is acceptable.

PP requirements retain affected-family ownership and the preceding purchase in that family's chain. Cash-only progression is selected, so catalogue entries have no completed-prestige requirements. No calendar timer or all-eight prestige ownership rule is added. The mixed opening reaches 148 entries in the reference.

Each signature is named '<Family> Signature Breakthrough I' through 'X'. It has a fixed authored multiplier S(family,edition), applied once when purchased. The first edition and later editions need not share a coefficient. Supporting multipliers also require authoring. Price, prerequisites, and the complete effect are previewed before buying. No multiplier is calculated from another family's live income.

## What counts as a successful comeback

A comeback must create a useful cash-spending decision and a noticeable shop-income gain. Merely showing a large multiplier on an insignificant product fails this goal.

For an arithmetic example only, suppose Phuljhari earns 4% of normal product income and the current leader earns 25%. A fixed x8 Phuljhari upgrade would produce:

```text
New shop income = old shop income x (0.96 + 0.04 x 8) = 1.28 x old income
Phuljhari share = 0.32 / 1.28 = 25%
Old leader share = 0.25 / 1.28 = 19.53%
```

Phuljhari becomes the largest earner and total product income rises 28%. This is an illustrative checkpoint, not a proposed universal x8 effect, a chosen upgrade price, or a simulated progression result. If Phuljhari instead contributes only 0.1%, x8 adds just 0.7% shop income; that would not fulfill the intended signature role at that checkpoint.

Candidate authoring checks:

- In a reference run without Talent bonuses, buying the signature and a defined affordable level bundle should be capable of making that family a leader at selected checkpoints. Compare alternatives with the same cash budget, including buying the newest family and saving for the next unlock.
- Compare immediate income gain, time to recover purchase cost, and time to the next useful purchase. Discount effects need a future spending horizon rather than an immediate-income comparison alone.
- Repeat comparisons with varied card histories. Different leaders and purchase orders are desirable; do not silently compensate for a player's stronger permanent card.
- Preserve a worthwhile first investment in each newly revealed product. Do not make unlocking a new family mainly an administrative prerequisite for an older-family bonus.
- Keep supporting purchases useful between signatures. Avoid a long stretch where saving for one distant signature dominates every available option.
- Distinguish frontier discovery from rebuilding: fast Buy All during a familiar rebuild is useful, while a first-time frontier milestone should have a legible effect.

## Links, discounts, and active play

Keep the current proposal of 32 directed links and 16 mixed-family purchases as secondary incentives. They do not substitute for an older product earning meaningful income itself. Link benefits come from authored ownership/level conditions and fixed multipliers, avoiding recursive income calculations around the eight-family ring.

Product-level discounts can help prepare a comeback by improving the return on additional levels. Talent profit and cost cards make particular families more attractive across runs. Their advantages persist even when another family reaches a signature milestone; the economy does not normalize every product to equal output.

A stronger normal product-income total also raises the linked portion of tapping, Bulk Order's reference payout, and the baseline used for offline earnings under the existing draft rules. Apply a product signature to that product once, then derive these linked benefits from the resulting normal production total. Do not multiply the linked payout by the signature a second time. Temporary Festival Rush income remains excluded from those normal-production references.

## First-run product availability and milestone sequence

Confirmed: all eight families are obtainable before the first prestige. No product is reserved behind a later prestige requirement. This lets the first run introduce the entire roster before Talent opens, including the products affected by all sixteen product-profit and product-cost card types. Players can still choose to delay individual purchases. The current baseline does not require all eight for prestige; availability and compulsory ownership are separate.

Use the existing family order as the reveal sequence. The table below preserves the intended teaching roles, not measured reveal spacing. The v1 reference obtains all eight families by 6m 20s, reaches first prestige at eight elapsed hours with mixed visits, and buys 148 distinct entries in the selected cash-only reference. Later cash milestones carry most first-day discovery; prototype review must check this early clustering.

| Stage | Product reveals | Purchases and experience to place around them |
|---|---|---|
| Opening | Phuljhari, then Anar | Learn levels, individual cash upgrades, and Buy All. Make Festival Rush available and introduce its burst benefit. |
| Early expansion | Chakri, then Rockets | Make the shop-linked tapping path affordable and introduce Helping Hands and Bulk Order through spaced purchase opportunities. |
| Middle expansion | Small crackers, then Cracker strings | Place useful cost reductions and an older-family comeback so investing in previous products matters before the first reset. |
| Final expansion | Aerial shots, then Celebration boxes | Make each new product a worthwhile investment and leave room to benefit from Celebration boxes before prestige. |
| Approach to prestige | All eight obtainable | Offer further affordable purchases, a visible reward preview, and a clear next earnings goal. Avoid a long wait with no useful spending decisions after the final product reveal. |
| First reset and rebuild | Talent opens; products are reacquired under the working reset model | Selected x6 opening income targets roughly 3–4x faster matched check-in recovery; faster sustained recovery is accepted. No individual product family is prestige-locked. |

Skill ownership gates remain the separate draft of one/two/three families for Rush/Hands/Bulk respectively. Being eligible does not mean the skill must be purchased immediately. Prices and tutorials should space these purchases; the stage table does not add hidden ownership gates. All skills are bought again each run as confirmed.

The first run should contain at least one understandable older-product comeback opportunity. Under the current signature placement, Phuljhari's second signature is PP-PHU-07 in band 03. Do not assume a first-run cutoff before that band while also relying on that particular comeback: either reach it, move a suitable signature and its pricing, or deliberately create an earlier comeback through supporting purchases. The first-run cutoff and exact comeback purchase remain unselected. This is a constraint for numerical authoring, not a new approved first-run catalogue size.

The final reveal is followed by a short, useful investment phase rather than an automatic reset trigger. Later progression comes from deeper cash upgrades, recurring product comebacks, card collection, and prestige depth. It does not rely on withholding a ninth or later product; additional families are outside the current roster.

Confirmed first-rebuild direction: x6 opening income, prioritizing matched check-ins and accepting faster sustained recovery. The cash-only model gives about 3.92x and 5.89x respectively with cards suppressed. Reacquire the same cash IDs and all eight families; record product access separately from full frontier recovery. Actual overnight absence can delay the next manual purchase.

## Presentation now drafted

The [Upgrade Browser and Goals Draft](Upgrade%20Browser%20and%20Goals%20Draft.md) specifies product contribution, before/after shop benefit, links to the filtered catalogue, and signature goals. The [Visual Progression Draft](Visual%20Progression%20Draft.md) maps existing signature purchases to four proposed owned looks per family and proposes six permanent stages for the same shop. Only the same-shop direction is confirmed; stage counts, visual gates, and reset/retention rules remain proposals. Appearance changes add no extra income effect or catalogue entry.

The [Onboarding and Return Flow Draft](Onboarding%20and%20Return%20Flow%20Draft.md) introduces comeback opportunities when relevant and supplies a candidate first-day session policy. No UI or art has been implemented or tested.

## Remaining authoring

V1 assigns product prices, level growth, supporting/signature effects, and catalogue reach, and tests first prestige and long-term reset cadence. Cash-only access is selected; cash, ownership, level, and prior-upgrade requirements apply with the stronger late price/Fame curve. There are no prestige-count or calendar requirements.

Confirmed: the regular nonpaying player's daily active-time budget is 15–25 minutes after the opening day. The [working plan](Redesign%20Working%20Plan.md) proposes a twenty-minute middle case with two four-minute visits and one twelve-minute visit, including purchase and rebuild time. The draft schedule accounts for the eight-hour offline cap rather than assuming all time away earns income. Visit spacing and tap/management allocation are model proposals. The selected daily budget does not change the separately agreed first-day timing.

Full coupled simulations exist for the selected baseline. See the report for contribution changes, completion ranges, and policy sensitivities. This does not establish that every milestone is engaging. Catalogue structure, prices, and dependencies are checked; no Unity implementation has occurred.
