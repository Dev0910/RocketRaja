# Rocket Raja: historical upgrade browser and next-goal proposal

**Superseded by the accepted Classic implementation — 7 October 2026.** See [Current Implementation Design](Current%20Implementation%20Design.md). Current behavior: next ten price-sorted upgrades plus a paged full catalogue; search/category/view controls only; immediate global Buy All; no product/skill selector, named-upgrade details or pin/goal-selection flow. The detailed filtered-review and pinned-goal proposal below is retained as history, not required work.

Version 0.1, 1 October 2026. Planning only; no Unity implementation.

Cash-only progression is selected. [Economy Specification v1](Economy%20Specification%20v1.md) and the main catalogue contain the current prices and Fame formula, with no completed-prestige requirements for cash upgrades. The x6 opening bonus is retained. Numerical balance remains subject to prototype tuning; the [decision record](Cash-only%20Economy%20Alternative%20v1.md) preserves the superseded comparison.

This is a concrete first design pass addressing the visibility and decision-feedback gaps in [Redesign Coverage Review](Redesign%20Coverage%20Review.md). Confirmed constraints are 1,000 cash unlockables, individual purchases and manual Buy All throughout, meaningful ordinary taps, older-product comebacks, and lifetime progress counting different upgrades purchased at least once. New controls, navigation, and ranking rules below are proposals.

Confirmed browser direction: one Upgrades destination with category and product filters. Product details link into the corresponding filtered view. The same entry has one ownership state wherever it is shown. Specific category labels, default filters, navigation placement, and quote behavior below remain proposals.

## Navigation and Home

Use four destinations as a first-pass information architecture: Home, Upgrades, Talent, and Prestige. Talent displays its first-prestige requirement until unlocked. Keep cash and normal production visible on Home and Upgrades; show Kaju Katli and free draw credits in Talent. The Kaju Katli balance leads to its shop after Talent is available. This draft introduces no premium spending use beyond draws.

Home emphasizes the shop, a useful next action, and the three skills. Product detail is a sheet reached by tapping its display or product row. A persistent Prestige entry shows progress or readiness; it does not automatically reset the run. Returning from a detail view preserves its parent scroll position.

```text
HOME
Cash: <wallet>                Normal production: <rate>/s

<Shop scene and eight product displays>

NEXT GOAL: <one pinned or suggested milestone>
<progress / missing requirement>                 [View]

[Festival Rush]     [Helping Hands]     [Bulk Order]
<locked requirement / ready / active / cooldown>

Home           Upgrades           Talent           Prestige
```

During a skill, distinguish normal production from the temporary increase. Use normal production for product contribution and purchase comparisons, so the reference does not fluctuate merely because Rush is active.

## Upgrade organization

| Category | Entries included | Product filter behavior |
|---|---|---|
| All | All named cash purchases | Match entries affecting the selected family. |
| Products | Family profit and family level-cost discounts | Select one family or all eight. |
| Tapping | Tap profit and tap-level cost | Product filter is not applicable. |
| Skills | Base skill purchases and cash skill improvements | Replace product filter with skill selector. |
| Shop | Global profit, named-unlockable discounts, links, and group purchases | Match links/groups involving the selected family and global effects that apply to it. |

Categories are views of the existing catalogue; they do not add purchases or duplicate lifetime credit. Player-facing names can change without changing identity. Offer text search by upgrade, product, and skill name in the catalogue view.

Default view: unbought entries whose requirements are met, sorted by current cash price. This includes unaffordable entries with a clearly shown price; an Affordable filter is optional. Do not silently truncate the choice set to the cheapest ten. Follow the available list with a compact Upcoming section showing the next blocked entry in each relevant prerequisite chain. Full Catalogue exposes all 1,000 entries, with filtering and their requirements.

Alternative state views are Owned this run and Full Catalogue. Remember the player's category and product choices during the session. Collapse distant prerequisites into a readable summary with a path to details; names being visible does not count as a lifetime purchase.

```text
UPGRADES                          First-time purchases: 249 / 1,000
[All] [Products] [Tapping] [Skills] [Shop]
Product: [Phuljhari v]             [Search]
[Available] [Upcoming] [Owned] [Full Catalogue]

Phuljhari Signature Breakthrough II           FIRST PURCHASE
<cash price>             <product effect and shop-income gain>
[Details]                                      [Buy]

<Additional entries; no arbitrary ten-entry limit>

[Review Buy All: Products / Phuljhari]
```

The count above is an illustrative UI state, not a planned price-band cutoff or progression target.

## Entry states and lifetime progress

| State | Presentation and action |
|---|---|
| Requirements unmet | State the next missing requirement; View requirement navigates to it. Buying is unavailable. |
| Available, insufficient cash | Show full cash price and amount missing. Pin goal remains available. |
| Available, affordable, never purchased before | First purchase marker; Buy adds current ownership and one lifetime first-purchase record. |
| Available, affordable, purchased in an earlier run | Rebuy marker; Buy restores the run effect without increasing the lifetime count. |
| Owned this run | Owned marker and applied effect; no second purchase of that same entry in the run. |

The lifetime count is about purchases, not revealed names, product levels, card levels, or total transactions. Base skill purchases count because their three entries are already in the 1,000. Prestige clears current-run purchase state under the working reset model and preserves the lifetime record. Filter labels must make these distinctions legible.

## Purchase details and comparison

Each entry shows its name, affected system, cash cost, requirements, and effect. Its detail sheet adds Before -> After, the contribution to normal shop income where applicable, and the next related milestone. Product detail shows the family's own rate and percentage of normal product income. It also provides separate manual product-level controls; those controls are outside named-upgrade Buy All.

Worked display example using invented numbers only: a family earns 100/s out of a 1,000/s shop. A x3 family upgrade raises it to 300/s and the shop to 1,200/s. The preview reads 'Family: 100 -> 300/s; shop: +200/s (+20%)'. It should not label the whole-shop gain as x3. The contribution changes from 10% to 25%.

For discounts, show the affected purchase category and an example next quote, not an invented immediate-income gain. For skill timing, show duration/cooldown changes and whether the skill is currently owned. For links and groups, show the affected families, missing level requirements, and resulting shop effect. Explain tap improvements per tap and at the stated comparison cadence; do not assume constant tapping for an entire session.

These are estimates from a defined current state. Do not call one purchase universally 'best' when a discount, distant milestone, or play style may change its value. The first pass offers comparisons rather than an opaque optimal-purchase score.

## Manual Buy All

Buy All is a two-step cash action: Review, then Buy the quoted batch. Its scope is the category, product/skill filter, and search currently selected, explicitly repeated in the review title. Owned-only views disable the action. The Affordable display filter does not stop the quote from finding purchases made affordable by an earlier discount in the same batch.

The proposal follows the existing blueprint: snapshot the current cash budget, select the cheapest currently eligible entry within scope, apply its effect to the quote, recalculate requirements and prices, and repeat until that budget cannot afford another eligible entry in scope. Use a stable catalogue order for ties. Newly eligible entries can join the quote if they match the stated scope. It spends only cash on named cash upgrades; product/tap levels, Kaju Katli, and free draws are excluded.

The review shows:

- Scope, entry count, total cash spent, and cash remaining.
- Number of first-time purchases versus rebuys and the resulting lifetime count.
- Normal product-rate change plus separate tap/skill/discount summaries where affected.
- A collapsed list that can be expanded to inspect all included purchases.

Freeze the quoted purchase list while the review is open; passive income can continue without silently adding more purchases. If eligibility or prices materially change before confirmation, refresh the quote for another explicit confirmation. Buy only the reviewed list once. On completion, show a compact result and preserve the current category. Closing the review spends nothing. This keeps the action predictable without adding a confirmation to every ordinary cash purchase.

## Next-goal guidance

One Home goal can be pinned by the player. A View goals sheet offers up to three alternatives with full routes into product, upgrade, skill, or prestige details. Pinning never spends money, changes prices, reserves a currency, or claims a separate reward. This is guidance for existing progression; daily/weekly objectives remain deferred.

Use these initial suggestion rules:

1. Honor a valid player pin until completed, cleared, or invalidated by reset.
2. During initial teaching, suggest the next useful tutorial action that the player has not learned; it remains skippable.
3. Otherwise offer a nearby new family or signature milestone, a meaningful available purchase, and a prestige/depth milestone when relevant. Avoid suggesting an action solely because it is the cheapest entry.
4. After a purchase or milestone, refresh the alternatives. Do not reshuffle them continuously on every income tick.
5. If a prerequisite blocks a goal, show the missing requirement and a navigation action. If cash blocks it, show cash needed and current progress; only show a time estimate if the model and reference rate are clear.

Goals can be first-time discovery or current-run rebuilding goals. A first-purchase goal completes once on the account; a rebuild goal relates to the current run. After prestige, clear completed/invalid run pins and offer the next useful rebuild purchase. An uncompleted lifetime catalogue goal may stay pinned, but must show its next actionable purchase.

The first pass does not reserve cash for a pin or exclude pinned purchases from Buy All. A pin is information. If the player wants to save, individual purchases remain available and the Buy All review states its spending clearly.

## Skill and milestone feedback

Skill controls show a requirement when locked, an explicit activation action when ready, remaining duration when active, and the cooldown. The draft Rush/Hands combo gets a shared highlighted state and a short description of the tap benefit while both are active. Display Bulk Order's expected payout before use, based on normal production. Do not count temporary Rush income twice in any preview.

Use a brief on-shop effect for a product comeback, with the product and shop-rate change available in the purchase result. First-time discoveries can have a larger reveal once; repeated rebuild purchases use compact feedback. Both input and numeric feedback remain readable with motion reduced or sound disabled.

## Design checks for the next review

- A player can find an affordable alternative outside the first ten entries and explain its benefit.
- Buying and rebuying the same entry changes the lifetime count exactly once.
- A Buy All preview states its scope and matches the eventual cash, ownership, and count changes.
- A discount purchase can unlock a cheaper subsequent batch entry without spending premium resources.
- A player can distinguish a product-level purchase, named cash upgrade, and permanent Talent card.
- A pinned goal survives ordinary navigation, never makes purchases, and behaves clearly after prestige.
- No production forecast or reward quantity is presented as a proven balance result.

The unified filtered browser is confirmed. Detailed layout and new UX rules remain draft pending discussion and eventual observation. See [Prestige and Talent Flow Draft](Prestige%20and%20Talent%20Flow%20Draft.md) for reset and collection behavior.
