# Rocket Raja: onboarding, check-ins, and offline returns

**7 October 2026 — current design:** the user confirmed the Unity implementation as intended. [Current Implementation Design](Current%20Implementation%20Design.md) takes precedence over conflicting passages below. Unrestricted cash upgrades, unlimited manual taps, immediate global Buy All, Classic browsing without product filters/pins, disabled transient notices and 1×/5× pulls are accepted.

Updated: 1 October 2026. Planning only.

Cash-only progression is selected. [Economy Specification v1](Economy%20Specification%20v1.md) and the main catalogue contain the current prices and Fame formula, with no completed-prestige requirements for cash upgrades. The x6 opening bonus is retained. Numerical balance remains subject to prototype tuning; the [decision record](Cash-only%20Economy%20Alternative%20v1.md) preserves the superseded comparison.

This draft completes the first pass of contextual teaching and return flow identified in the [coverage review](Redesign%20Coverage%20Review.md). New presentation and settlement rules below are proposals. Confirmed inputs include meaningful ordinary taps, manual purchases, three skills rebought per run, Talent at first prestige with three draws, 50% base offline income for up to eight hours, and 15–25 active minutes per regular day after the opening.

## First-session principles

Teach one relevant action at the moment it becomes useful. Use compact explanations anchored to the actual control, with Skip and later Help available. Do not require a player to read every economy rule before earning. A dismissed tutorial does not withhold cash, skill eligibility, or card access.

The confirmed first/reset starting grant and manual first purchase are in [Prestige and Talent Flow Draft](Prestige%20and%20Talent%20Flow%20Draft.md). The first meaningful action should establish earning and investment promptly. Treat account/naming prompts as a separate journey: where account requirements permit, offer them after the first useful purchase, with a visible existing-player restore path from the start. This is a UX proposal and does not change current authentication or save behavior.

## Contextual teaching sequence

| Trigger | Teach | Completion and repeat behavior |
|---|---|---|
| First playable Home | Tap earns cash; buy the first Phuljhari level using the available grant. | First product purchase or dismissal completes the prompt. Ordinary controls remain usable. |
| First affordable named cash upgrade | Explain its effect and where Upgrades lives. | Inspect or buy once; do not replay on every prestige. |
| First browser visit | Category/product filters, current-run ownership, First purchase versus Rebuy. | One short example; catalogue counter help stays accessible. |
| First Buy All review | Its cash scope, total cost, first-purchase count, and the explicit confirmation. | Explain once; every batch retains the clear review. |
| First production-linked tap upgrade becomes relevant | Products now help tap earnings; tap investment still matters. | Explain the effect after purchase with the actual before/after figure. |
| First skill becomes purchasable | Show its cash/ownership requirement and what activation does. | Buying and trying it can be suggested, never forced. |
| Rush and Helping Hands both owned | Show the proposed combo when both are ready or overlapping. | One contextual hint, with a persistent effect description in skill details. |
| Bulk Order first owned | Preview the payout as ordinary production time; it is an instant reward. | Explain on inspection; no compulsory activation. |
| An older product gains a signature opportunity | Show that family's contribution and the shop gain from buying it. | Use the purchase preview; no separate compulsory quest. |
| First prestige becomes eligible | Explain guaranteed income, exactly three draws, and reset/keep rules. | Open preview when the player chooses; readiness remains visible. |
| First prestige completes | Introduce Talent and the choice to draw now or later. | First reward summary is enough; no forced paid draw. |
| First duplicate card | One copy gives one immediate level; show the effect change. | Explain once per account, not once per card. |
| First rebuilt session | Buy the first product, reuse familiar upgrades, and rebuy skills. | Show a compact reminder; avoid replaying the initial tutorial. |
| First offline return | Explain efficiency, credited time, storage cap, and the next useful action. | Future summaries remain concise with details available. |

These triggers depend on actual state, not fixed clock times or forced product purchase order. If an action happens through Buy All before its teaching trigger, acknowledge the result rather than demanding that the player repeat it. Mark prompts complete across sessions. Skipped instruction remains reachable from the relevant detail screen.

## First-day reference session for later balance work

The executed v1 mixed-play benchmark uses visits at launch, +2h, +4h, +6h, and +8h with 15/5/5/5/10 active minutes (40 total). About 30 active minutes are used by the first reset at eight elapsed hours. This replaces the earlier 24-minute test proposal. Purchases, management, skills, reveals, and taps share the budget; the schedule is a modeling policy, not a required player routine.

The separate sustained benchmark reaches first prestige at about 2h 6m. The selected cash-only model also tests lighter and differently spaced later play. All eight families arrive early in the reference; later cash milestones must make the rest of the opening engaging. Prices and production now exist, but only uncoached sessions can establish whether teaching and choices work well.

For ordinary days, retain the working plan's proposed 4 + 4 + 12 minute schedule and compare the selected 15–25-minute range. The first day can have extra learning without redefining the regular-day baseline.

## Offline settlement proposal

On returning, calculate and credit the eligible offline amount once, then present a summary. Closing the summary does not lose the amount and does not grant it a second time. The summary is an acknowledgement, not a purchase. No rewarded ad is needed for the baseline amount.

Use the normal product income recorded for the absence, excluding temporary Rush, automatic skill actions, tap income, and sponsor boosts. Apply owned offline efficiency/storage effects according to the numerical draft. There are no purchases while away. The confirmed baseline is 50% of normal product production, capped at eight hours per absence.

```text
WELCOME BACK
Away: <elapsed time>
Earning time: <credited duration> / <offline storage limit>
Offline efficiency: <current percentage>
Earned: <cash amount>                          Added to cash

<A useful product/upgrade/goal is now available>
[View upgrade]                                      [Continue]
```

Illustrative arithmetic: 10 hours away, no offline cards, and a departure rate of 10,000/s yields 8 x 3,600 x 0.50 x 10,000 = 144,000,000 cash. Display ten hours away and eight credited, so the player can explain the result. This rate and amount are examples, not configured economy values.

The time represented by the summary stops at resumption; live income starts then, with no overlapping period. The player may open Prestige after the credit is applied, and the cash-reset preview shows the resulting wallet normally. If offline income contributes to eligible run earnings under the final rules, credit that counter once as well. Pending account synchronization and old-save migration need implementation handling later; this flow does not override them.

## Production, skills, and menus

Propose that production and cooldowns continue while ordinary menus and nonblocking tutorials are open. The user is still online; menus do not create a second offline award. Suppress gameplay taps when interacting with buttons or modal controls so dismissing a card cannot also earn a tap or activate a skill.

During absence, cooldown time can elapse, but skills do not activate themselves or generate special offline payouts. Temporary effects end on their normal timeline; the offline earnings formula continues to use the normal product rate. On a prestige, active effects end and skills must be bought again as confirmed. A newly repurchased skill starting ready is the separate proposal in the prestige-flow draft.

If any later interface pauses the economy, that exception must be explicit and reconciled with cooldown and offline accounting. The initial design favors one understandable rule of ongoing production rather than different hidden behavior for each panel.

## Short check-in and longer session

In a short visit, aim for a useful loop: understand the return amount, inspect the pinned/available goal, make purchases, use an available skill if desired, and leave. Prestige can be postponed without a repeated interruption. The player should not need to open every product or read a full card collection before benefiting from the visit.

In the longer visit, allow comparison between a comeback purchase, a new family/frontier upgrade, active skill windows, and a deeper prestige reward. Show these as alternatives rather than presenting constant tapping as a requirement. A player using the shorter visit may progress more slowly, but should still understand the next useful action.

## Rewarded ads and account prompts

Rewarded ads and ad-funded rewards are excluded from the initial redesign, as confirmed. All reference policies use the no-ad path. If ads are considered later, their payouts, eligibility, failure outcomes, and presentation require a separate design and balance pass; they must not delay or consume the base offline credit. Achievement and daily/weekly Kaju Katli grants remain deferred.

Account prompts, notifications, and reconnect messages should not interrupt an active purchase confirmation or silently reset a partially viewed reward. Their platform behavior remains outside this planning-only artifact; no notifications, authentication, or cloud services are changed here.

## Observation script for the next prototype

Ask uncoached players to explain and demonstrate:

1. Where they would invest next and the benefit they expect.
2. Why an older product became worth revisiting.
3. The difference between a First purchase, a Rebuy, and a Talent card.
4. What they will receive and lose when they prestige.
5. Why the offline amount differs from all elapsed time at full production.
6. How to reach a locked skill, and what happens to that skill after prestige.
7. What changed during the first rebuilt session.

Record observed actions, navigation time, confusion, and purchase/reward outcomes. This is a qualitative test plan; it does not supply a retention percentage or claim that any of these flows has been tested with players.
