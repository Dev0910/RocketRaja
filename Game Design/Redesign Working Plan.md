# Rocket Raja: redesign working plan

Updated: 1 October 2026. Planning only. The numerical authoring and simulation pass is now complete as a reviewable v1 candidate. No Unity changes were made.

## Confirmed direction

The user's answers establish:

- Prioritize longer progression with regular milestones.
- Cash-only catalogue progression is selected: no completed-prestige requirements on upgrades. Cash, product ownership, level, and prior-upgrade prerequisites still apply.
- Design the full 1,000 distinct cash unlockables now, rather than a 500-entry catalog with the remainder deferred. These are distinct purchases, not product levels or repeat purchases after a reset.
- Introduce Kaju Katli as a scarce premium currency, with small free grants and IAP acquisition. Initially it buys random permanent-upgrade card draws only. Temporary boosts, instant cash, cosmetics, cooldown resets, and direct card purchases are outside the initial premium shop. The initial amounts and INR pack prices are confirmed in the premium specification.
- Completely redesign prestige progression, drawing inspiration from the existing Tap Tycoon research.
- Use the Wittle Defender Talent collection as a further reference: random card acquisition, permanent upgrades, and automatic progression from duplicate cards.
- Use the accepted 28-card starting roster: 16 Common product-profit/cost cards, 10 Rare tap/skill/offline cards, and 2 Legendary whole-shop-profit/cash-unlockable-discount cards. The 75% Common / 22% Rare / 3% Legendary probabilities are now confirmed, uniform within tier.
- Use independent random draws without a Legendary guarantee, pity counter, or increasing odds after an unlucky streak. This does not remove the separately confirmed guaranteed prestige income bonus.
- Every duplicate immediately adds exactly one card level. Higher levels must not require multiple copies or another upgrade payment. Long-term growth is controlled through the effects gained per level.
- Keep the single-draw Kaju Katli price fixed; it must not increase with draw count. The confirmed fixed cost is 100 Kaju Katli.
- Prestige grants a guaranteed permanent income bonus as well as free draws.
- First prestige grants three free draws. Later free draws combine current-run progress with lifetime progression measured by different cash upgrades purchased at least once from the 1,000 unlockables. The latest explicit selection supersedes the intervening Fame-based draw metric. Rebuying an entry after prestige does not count again. Exact reward thresholds, current-run metric, minimum, and qualification remain draft. Fame remains a separate proposed basis for guaranteed permanent income.
- Include cash-purchased cost-reduction unlockables alongside permanent cost-reduction cards.
- Keep cash purchasing manual with Buy All throughout progression. Do not unlock automatic purchasing later. Automatic duplicate card level-ups and Helping Hands taps are separate mechanics and remain part of the proposed card/skill design.
- Use one Upgrades screen with category and product filters. Detailed navigation, filter behavior, and Buy All previews remain draft.
- Evolve the same shop through bigger displays, lighting, and decorations. Keep earned shop decorations through prestige; rebuild product displays as products are repurchased. Exact visual stages and thresholds remain prototype defaults; distinct shop locations are not the selected direction.
- Grant enough starting cash for the current discounted first Phuljhari unit, bought manually, on a new account and after prestige. Exclude that grant from earned-cash progress.
- Skill access and all cash-purchased skill improvements reset at prestige. Players buy the base skills again each run; permanent skill Talent cards remain owned and apply once the relevant skill is reacquired.
- Base offline earnings are 50% of normal product income for up to eight hours. Talent cards can improve efficiency and storage; their numerical level curves remain draft.
- Support both short daily check-ins and longer active sessions.
- Use 15–25 total active minutes per day after the opening day as the regular nonpaying player's balancing baseline. The user selected this over 30–45 or 60+ minutes. This budget includes purchases, collection management, skills, and rebuilding as well as tapping; it is not a minimum daily activity requirement.
- Keep ordinary tapping worthwhile throughout the run, with skills creating stronger bursts. The user selected this over concentrating most active income in skill windows. The exact formula, contribution, and credited tap rate remain draft.
- Let older products periodically return as leading earners after significant upgrades. The user selected recurring comebacks over older products mainly supporting later products. Exact signature upgrades, multiplier sizes, and sequencing remain proposals; equal output from every product is not the goal.
- Make all eight current product families obtainable before the first prestige. Do not reserve a family behind a later prestige requirement. The v1 numerical candidate now supplies product prices and measured reference timing; availability does not itself establish an all-eight ownership requirement for prestige.
- Target first prestige at 6–10 elapsed hours with mixed visits, or 2–3 sustained hours, without IAP or ads. v1 models eight hours mixed and about 2h 6m sustained under its stated policies; these are simulated results, not human observations.
- Latest choice: use a stronger x6 opening income bonus, prioritizing roughly 3–4x faster matched check-in rebuilding and accepting faster sustained rebuilding. This replaces the x3.5 candidate and the earlier requirement that sustained recovery also stay inside 3–4x. It does not multiply income by six anew at every reset.
- Target roughly three months of major authored progression for a regular nonpaying player. Continuing to gain card levels may extend beyond that horizon. Coupled model results for the candidate schedules are recorded below; human validation remains future work.
- Target 1–2 prestiges per day for a regular mixed-play user after the opening. This describes intended pacing, not a daily reset limit.
- Unlock Talent and give the first card draw at the first prestige. The earlier proposal for an introductory draw 10–15 minutes into play is superseded.
- Add Kaju Katli rewards through achievements and daily/weekly objectives later. Those systems and their currency supply are deferred from the initial economy baseline. Rewarded ads and ad-funded rewards are excluded from the initial redesign. Prestige free draws remain in the initial design. One 100-Kaju-Katli gift after the second prestige is confirmed.
- Use India / INR first, with other regions localized later. Initial packs are confirmed at 1,000 / 6,000 / 13,000 / 28,000 KK for ₹99 / ₹499 / ₹999 / ₹1,999; platform price availability is checked during implementation.
- Remain in planning. Markdown notes are permitted; implementation is not authorized.

## Current plan and source of truth

| Document | Current role |
|---|---|
| [Implementation TODO](Implementation%20TODO.md) | Ordered implementation checklist with dependencies, code entry points, and acceptance checks; no implementation started by creating it. |
| [Economy Specification v1](Economy%20Specification%20v1.md) | Current cash-only numerical rules, products/taps, effects, stacking, and prestige. |
| [1000 Unlockables Catalogue](1000%20Unlockables%20Catalogue.md) | All 1,000 individual names, prices, effects, level prerequisites, and individual access requirements. |
| [Premium Economy v1](Premium%20Economy%20v1.md) | Confirmed fixed draw price and initial INR packs, one-time free grant, and acquisition/purchase rules. |
| [Economy Simulation Report v1](Economy%20Simulation%20Report%20v1.md) | Executed first/rebuild/150-day/card-history/IAP/stress results and limitations. |
| [Cash-only decision record](Cash-only%20Economy%20Alternative%20v1.md) | Selected direction and historical comparison with the superseded milestone option. |
| [Cash-only Price Table v1](Cash-only%20Price%20Table%20v1.md) | All 1,000 current base prices matched by stable ID; mirrors the main catalogue. |
| [Economy Model v1](Economy%20Model%20v1.md) | Reproducible source and exact parameters, stored in Markdown outside Unity runtime. |
| [Planning Decisions and Review v1](Planning%20Decisions%20and%20Review%20v1.md) | Confirmed choices, prototype defaults, and review risks. |
| [Upgrade Browser and Goals Draft](Upgrade%20Browser%20and%20Goals%20Draft.md) | Unified browser, contribution previews, Buy All review, discovery/rebuy states, and goal guidance. |
| [Prestige and Talent Flow Draft](Prestige%20and%20Talent%20Flow%20Draft.md) | Reward previews, reset/keep rules, reveals, free versus paid actions, and rebuilding. |
| [Visual Progression Draft](Visual%20Progression%20Draft.md) | Same-shop stages, product displays, effects, and asset scope. |
| [Onboarding and Return Flow Draft](Onboarding%20and%20Return%20Flow%20Draft.md) | Contextual teaching, offline credit, session flow, and observation questions. |

V1 specification/report values take precedence over earlier numerical draft passages. The [blueprint](1000%20Unlockables%20Blueprint.md), [product draft](Product%20Progression%20Draft.md), [active-play draft](Active%20Play%20and%20Skills%20Draft.md), and [Talent draft](Talent%20and%20Prestige%20Draft.md) preserve design rationale and the accepted roster. The earlier [collection-only study](Collection%20Balance%20Study.md) remains valid for its independent fixed-draw experiment.

## Selected cash-only progression

**Cash-only progression is confirmed.** Every cash entry is available when its individual cash, ownership, level, and prior-upgrade requirements are met. Completed prestige count never locks catalogue sections.

The main catalogue, specification, and model now use the same selected baseline: later price-band growth x31 and permanent income 6+3×(sqrt(F)−1), with x1 at zero Fame. Prestige remains valuable for Fame and free cards. U still counts different cash entries ever purchased for the lifetime draw bonus; N still advances the next-run earnings target.

The rerun of 32 free histories completes on days **75 / 86 / 98** at the sample 10th / median / 90th percentiles. The 86-day median depends on the stated play policy. Deep runs can finish sooner; actual play remains untested. The former prestige-milestone option is superseded; its comparison remains in the [decision record](Cash-only%20Economy%20Alternative%20v1.md).

Opening and visual retention are now confirmed: keep earned decorations, rebuild run-specific product displays, and grant cash for the first manual Phuljhari purchase. The initial premium economy and exclusion of rewarded ads are also confirmed. No mandatory discussion items remain from this review.

## Confirmed premium values and numerical baseline

- Draws cost 100 Kaju Katli. Confirmed initial packs: 1,000 / 6,000 / 13,000 / 28,000 KK for ₹99 / ₹499 / ₹999 / ₹1,999. These are design prices; no live products have been created or platform tiers verified.
- Grant 100 Kaju Katli once after the second prestige. First prestige remains exactly three free draw credits. No recurring KK, ad reward, or deferred achievement/objective source is assumed.
- Use confirmed 75/22/3 tier odds, uniform within each tier, with no pity. Card effect curves remain the current prototype baseline.
- Candidate run target: T=min(1e36,1e10×1.5^N), fixed during the run; N is prior completed prestiges. Candidate Fame gain: floor(sqrt(E/T)).
- Current permanent multiplier: 1 at zero Fame; otherwise 6+3×(sqrt(F)−1). This is the selected cash-only baseline, subject to prototype tuning; x6 opening remains the confirmed direction.
- Later free draws: 1+floor(log4(E/T)), plus floor(U/250) when E>=4T. Thresholds remain recommendations; the hybrid metric is selected.
- All cash prices/effects and concrete source-level requirements are now supplied. No numeric placeholder is needed to run this candidate.

## Executed results

The free-player reference reaches first prestige at eight elapsed hours, and the sustained benchmark at about 2h 6m. The x6 first-rebuild diagnostic, with cards suppressed, is about 5.89x faster sustained and about 3.92x faster under matched mixed visits. Actual overnight absence can postpone a purchase.

Thirty-two free-player histories complete on days 75 / 86 / 98 at the sample 10th / median / 90th percentiles. Their mean daily reset cadence over days 31–90 is 1.23 / 1.38 / 1.53. These are simplified cash-only policy results. One-visit, shorter-session, deeper-run, no-tap, Common-only, and paid cases are in the report.

The first-day test uses 15/5/5/5/10 minutes at elapsed hours 0/2/4/6/8, totalling forty minutes. The regular twenty-minute reference uses 4/4/12-minute visits separated by eight and six hours; there is no invented overnight login or purchase. The shorter initial 24-minute test schedule has been superseded.

## Review risks and later implementation

Review the early reveal of all eight families, the transition after the introductory price band, and small-pack perceived value. The longest named-upgrade gap in the sustained reference is about 13.7 minutes; product/tap investments remain available, but engagement is untested.

The numerical baseline and experience drafts are ready to review. They are not shipping validation. Unity screens/art, uncoached player observation, runtime large-number/IAP/save behavior, and migration require a separately authorized implementation phase.

## Research used

- [Comparison and Recommendations](Comparison%20and%20Recommendations.md) and [Design Recovery Roadmap](Design%20Recovery%20Roadmap.md): product relevance, tap investment, choices, visible growth, prestige, and onboarding/return clarity.
- [Rocket Raja Economy Audit](Rocket%20Raja%20Economy%20Audit.md) and [Comprehensive Design Audit](Comprehensive%20Design%20Audit.md): historical implementation and simulation baseline. Their short-first-run timings do not describe the redesign.
- [Tap Tycoon Design Breakdown](Tap%20Tycoon%20Design%20Breakdown.md), [Research Sources](Research%20Sources.md), [Research Paper Review](Research%20Paper%20Review.md), and [Incremental Game Benchmarks](Incremental%20Game%20Benchmarks.md): structural inspiration and evaluation principles.
- The supplied Wittle Defender screenshot and the user's explanation: permanent random cards and immediate duplicate levels. The image did not establish draw odds, price growth, or limits.

Earlier deterministic-Legacy, chosen-card, and short-first-prestige suggestions are superseded by the user's choices. Historical research originals remain unchanged.
