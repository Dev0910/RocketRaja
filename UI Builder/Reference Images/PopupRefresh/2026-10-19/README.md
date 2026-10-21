# All popup refresh — 19 October 2026

The user directed ChatGPT image generation followed by HTML implementation for every existing game popup, using the chatgpt-ui-to-html skill. This revises existing routes; no new screen, mechanic or native integration was requested. The user then explicitly removed the bottom Back button from Settings.

**29 routes covered.** Generated 26 distinct new references using the built-in ChatGPT image_gen tool only. Account shares Welcome. Skills and SkillDetail reuse their already-selected Quick action list and Hero card references; the earlier removal of View skill upgrades remains in effect. Six references received targeted corrections for close-X omissions or the Settings Back removal. Initial images remain as history.

Generated references and resulting HTML await visual acceptance. The broad end-to-end instruction authorizes this implementation; it is not a claim that the user has accepted each generated image. Unity transfer remains separate under H2U-013.

[Open the visual comparison gallery](review.html). [Exact generation prompts, source paths and revisions](generation.json). [Verified starting content and controls](inventory.json). [Layout checks](checks-final.json). [Control identity comparison](control-regression.json). [Focused behavior results](behavior-checks.json).

## Implementation and fidelity

Editable content and existing actions remain in the main Classic entry point. New scoped popup-refresh.css and popup-refresh.js apply the visual treatment; one existing render hook invokes presentation decoration. Settings Back is removed at its source. No flattened screenshot is used as a control surface. The shared transparent painted frame and seven Settings symbols were separately generated. The icon atlas was cropped into individual sprites without redrawing. Remaining illustrations use the game’s existing painted assets; decorative mockup flourishes are simplified for responsive sizes. Collectible cards retain their rarity-specific frames and proportions, with the common card receiving the shared gold/cobalt outer treatment and a visible close X.

Reference typography and artwork hierarchy carry into responsive layouts. Long catalogues and policy content use a scrolling body with accessible fixed header/footer controls. Generated sample numbers, decorative labels and omitted controls never replace actual mechanics or required actions. Welcome and Account share a compact modal using existing stall art.

## Verification

All 29 routes checked at 390×844 and 360×540 (58 cases): frames fit, no horizontal overflow, no broken images or duplicate IDs, one metadata block, and all visible actions at least 48 px high. Every existing action/parameter identity remains except the explicitly removed Settings Back. Checked Settings toggle/rerender, empty-name validation and input focus, nested close restoring the Settings opener, outside dismissal without a stall tap, stale-review and save-error disabled states, all four disposable purchase preview outcomes, and skill activation returning to the list with its live clock. Browser error log empty; both JavaScript syntax checks pass.

## Popup coverage

| Route | Generated/selected reference | HTML at 390×844 | Short phone |
| --- | --- | --- | --- |
| Guidance | [Reference](<references/Guidance.png>) | [HTML](<after/Guidance-390x844.png>) | [360×540](<after/Guidance-360x540.png>) |
| Welcome | [Reference](<references/Welcome.png>) | [HTML](<after/Welcome-390x844.png>) | [360×540](<after/Welcome-360x540.png>) |
| Account | [Reference](<references/Welcome.png>) | [HTML](<after/Account-390x844.png>) | [360×540](<after/Account-360x540.png>) |
| Upgrades | [Reference](<references/Upgrades.png>) | [HTML](<after/Upgrades-390x844.png>) | [360×540](<after/Upgrades-360x540.png>) |
| ProductDetail | [Reference](<references/ProductDetail.png>) | [HTML](<after/ProductDetail-390x844.png>) | [360×540](<after/ProductDetail-360x540.png>) |
| TapTraining | [Reference](<references/TapTraining.png>) | [HTML](<after/TapTraining-390x844.png>) | [360×540](<after/TapTraining-360x540.png>) |
| Skills | [Reference](<../../Skills/Popup Variants/2026-10-19/01-quick-action-list.png>) | [HTML](<after/Skills-390x844.png>) | [360×540](<after/Skills-360x540.png>) |
| SkillDetail | [Reference](<../../SkillDetail/Popup Variants/2026-10-19/01-hero-skill-card.png>) | [HTML](<after/SkillDetail-390x844.png>) | [360×540](<after/SkillDetail-360x540.png>) |
| CardDetail | [Reference](<references/CardDetail.png>) | [HTML](<after/CardDetail-390x844.png>) | [360×540](<after/CardDetail-360x540.png>) |
| DrawReveal | [Reference](<references/DrawReveal.png>) | [HTML](<after/DrawReveal-390x844.png>) | [360×540](<after/DrawReveal-360x540.png>) |
| Prestige | [Reference](<references/Prestige.png>) | [HTML](<after/Prestige-390x844.png>) | [360×540](<after/Prestige-360x540.png>) |
| Offline | [Reference](<references/Offline.png>) | [HTML](<after/Offline-390x844.png>) | [360×540](<after/Offline-360x540.png>) |
| Statistics | [Reference](<references/Statistics.png>) | [HTML](<after/Statistics-390x844.png>) | [360×540](<after/Statistics-360x540.png>) |
| Settings | [Reference](<references/Settings.png>) | [HTML](<after/Settings-390x844.png>) | [360×540](<after/Settings-360x540.png>) |
| Naming | [Reference](<references/Naming.png>) | [HTML](<after/Naming-390x844.png>) | [360×540](<after/Naming-360x540.png>) |
| Privacy | [Reference](<references/Privacy.png>) | [HTML](<after/Privacy-390x844.png>) | [360×540](<after/Privacy-360x540.png>) |
| AccountCompare | [Reference](<references/AccountCompare.png>) | [HTML](<after/AccountCompare-390x844.png>) | [360×540](<after/AccountCompare-360x540.png>) |
| Promote | [Reference](<references/Promote.png>) | [HTML](<after/Promote-390x844.png>) | [360×540](<after/Promote-360x540.png>) |
| SignOut | [Reference](<references/SignOut.png>) | [HTML](<after/SignOut-390x844.png>) | [360×540](<after/SignOut-360x540.png>) |
| Handoff | [Reference](<references/Handoff.png>) | [HTML](<after/Handoff-390x844.png>) | [360×540](<after/Handoff-360x540.png>) |
| SaveRecovery | [Reference](<references/SaveRecovery.png>) | [HTML](<after/SaveRecovery-390x844.png>) | [360×540](<after/SaveRecovery-360x540.png>) |
| RecoveryConfirm | [Reference](<references/RecoveryConfirm.png>) | [HTML](<after/RecoveryConfirm-390x844.png>) | [360×540](<after/RecoveryConfirm-360x540.png>) |
| RecoveryExit | [Reference](<references/RecoveryExit.png>) | [HTML](<after/RecoveryExit-390x844.png>) | [360×540](<after/RecoveryExit-360x540.png>) |
| Purchase | [Reference](<references/Purchase.png>) | [HTML](<after/Purchase-390x844.png>) | [360×540](<after/Purchase-360x540.png>) |
| BuyAll | [Reference](<references/BuyAll.png>) | [HTML](<after/BuyAll-390x844.png>) | [360×540](<after/BuyAll-360x540.png>) |
| Appearance | [Reference](<references/Appearance.png>) | [HTML](<after/Appearance-390x844.png>) | [360×540](<after/Appearance-360x540.png>) |
| Help | [Reference](<references/Help.png>) | [HTML](<after/Help-390x844.png>) | [360×540](<after/Help-360x540.png>) |
| Odds | [Reference](<references/Odds.png>) | [HTML](<after/Odds-390x844.png>) | [360×540](<after/Odds-360x540.png>) |
| Exact | [Reference](<references/Exact.png>) | [HTML](<after/Exact-390x844.png>) | [360×540](<after/Exact-360x540.png>) |
