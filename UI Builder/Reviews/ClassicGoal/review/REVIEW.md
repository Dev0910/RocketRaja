# Independent Classic UI review

**Verdict: PASS. All 13 recorded findings are resolved. No remaining HTML visual or interaction blocker was found in the requested coverage.**

This is an independent review of the current HTML candidate, using a separate headless Edge/Playwright browser and actual rendered screenshots. It does not record user visual approval or claim Unity, authentication, payment, save, or device-runtime integration.

## Final evidence and scope

| Evidence | Final result |
| --- | --- |
| `r5-sweep.json` | 276 route/variant/viewport checks; all five source hashes unchanged; zero browser/resource errors, missing images, duplicate IDs, document overflow, modal header/footer overflow, or visible non-inert controls below 48px. Measurement includes the shared world/header/stall/navigation, active content, buttons, inputs, selects, role buttons and links. |
| `final-states.json` | 82 representative state captures at 360×540 and 390×844; all five final source hashes unchanged; zero browser errors. |
| `r5-deep.json` | 219 independent meaningful interaction assertions passed on the final R5 source. |
| `r5-acceptance.json` | 78 accessibility, feedback, purchase/recovery and visible review-tool records; zero browser errors. All review-screen options reached their declared destination; the artwork-gallery link returned HTTP 200. |
| `final-name-scroll.json` | 60 records: ordinary and wide 24-character names across Home/expanded/Settings at all four sizes, plus main-panel and modal scroll ends. Names stay within the phone; full editable values remain intact; continuation cues disappear at the end. |
| `final-shared-targets.json` | 20 whole-interface geometry sets plus 32 actual shelf clicks at all four sizes. No sub-48px visible controls, shelf/shelf overlap, or shelf/stall overlap. Every shelf opened its matching detail and produced zero Tap events. Final source hashes unchanged. |
| `r5-headers.json` | 48 independent final compact-header captures: four tabs × three names × four sizes. Personally inspected all four contact sheets. Text sits inside the complete painted plaque, with separate cash, settings and Tap controls. Final source hashes unchanged. |

The four viewports are **360×540, 360×640, 390×844, and 450×916**. Normal coverage comprises 33 surfaces, all 28 CardDetail cards and three rarity treatments, all eight ProductDetail families, and all three SkillDetail views: 69 variants × four viewports = 276 captures.

Covered surfaces: Home, Player, Products, Legacy, Shop, Upgrades, BuyAll, ProductDetail, TapTraining, Skills, SkillDetail, CardDetail, DrawReveal, Prestige, Offline, Statistics, Settings, Naming, Privacy, Account, Welcome, AccountCompare, Promote, SignOut, Handoff, SaveRecovery, RecoveryConfirm, RecoveryExit, Purchase, Appearance, Help, Odds, and the hidden Exact sheet reached through the actual Show exact quote action. Legacy Talent and removed UpgradeDetail routing are reconciled by the current prototype rather than treated as new screens.

## Finding verdicts

| Finding | Verdict | Independent verification |
| --- | --- | --- |
| A1 — Phone-local feedback and name validation | PASS | Empty naming has a visible associated error, aria-invalid, and input focus. Account/recovery failures and purchase outcomes are readable inside the phone at all four sizes. Notices do not cover navigation or fixed modal actions. |
| A2 — Expanded content and continuation | PASS | At 360×540, Products exposes multiple rows, Legacy exposes the first collection row, and Player exposes upgrade content. Home retains its full stall composition. Panel and modal continuation cues are truthful and disappear at the bottom; last content/actions remain reachable. Player Fame and prestige remain reachable at the scroll end. |
| A3 — Card dismiss control | PASS | All 28 cards have a visible, unclipped 48px circular close at all four sizes. Actual close returns to the invoking collection control. Card title, level, art, benefit and rarity remain readable. |
| A4 — Current tab | PASS | The restrained gold marker identifies the current blue tab. aria-pressed remains correct; selecting the current tab returns Home without changing layout. |
| A5 — Scoped skill upgrades | PASS | Actual SkillDetail links show only effects 7/8/9 for their own skill. Frozen native Buy All scope and IDs agree. Broad Skills scope clears the target; catalogue paging/search remain usable. |
| A6 — First stock route | PASS | Both new and rebuilt Home CTAs reach Products and its enabled first Stock action. Stocking returns to the populated Home presentation. Rebuilt gains remain represented; detail popups remain read-only. |
| A7 — Stale quote explanation | PASS | The warning is immediately visible at the top of the review, while Confirm stays disabled. Frozen IDs/prices remain unchanged until explicit refresh. |
| A8 — 24-character names | PASS | Ordinary and W×24 values remain editable in full. Home signs truncate within the painted sign; Settings wraps within its control. No horizontal overflow or displacement of settings/Tap controls. |
| R1 — Focus and switch names | PASS | Settings refresh keeps focus on the equivalent switch. Successful rename returns to its invoking control; nested Privacy dismissal restores its opener. Music, Sound effects and Reduced motion have distinct accessible names, switch semantics and correct aria-checked. |
| R2 — Review screen selector | PASS | Every current screen option was driven through the visible mobile review tools and opened its declared route. Fixture selection also applies. |
| R3 — Broken comparison link | PASS | The stale deleted-file link is replaced by the honestly labelled available artwork gallery; HTTP 200 verified and desktop tooling visually inspected. |
| R4 — Home shelf hit targets | PASS | All eight targets meet 48px at all four sizes, do not overlap each other or the stall target, and each actual click opens the matching product without a gameplay tap. Painted merchandise stays aligned with its shelf. |
| R5 — Compact sign alignment | PASS | The final 48-header review puts default, ordinary-24 and W×24 text inside the painted plaque on every expanded tab at every requested size. The previous tall-screen blank-plaque regression is resolved. |

## Pixel and behavior review

All 276 normal variants were visually inspected through the 36 `final-contact-*` sheets, and all 82 state captures through the 12 `finalstate-contact-*` sheets. That review discovered R5 despite passing DOM checks. After the scoped final CSS correction, all 276 variants and 82 states were recaptured on the final source; all affected headers were independently inspected in `r5-header-contact-*` and the four `r5-contact-*-0.png` sheets. Unaffected foregrounds retained their earlier inspected appearance. Individual full-resolution PNGs remain beside the contact sheets.

State coverage includes new/opened/growing/prestige/rebuilt shops; ready/active/cooldown/locked skills; insufficient cash and save failure; empty catalogue; stale/save-blocked Buy All; locked and zero-resource Talent; purchased product and bulk-upgrade feedback; eligible and below-target prestige; all six store conditions; and five actual settled draw results covering new/duplicate Common, Rare and Legendary cards. Account comparison, promotion, sign-out, handoff, recovery retry/confirmation/exit and naming were exercised through actual controls. Full catalogue pagination independently reached all 1,000 IDs; stable-ID search and empty search were checked.

OS and in-game reduced motion stop running animations and make reveal Continue immediately usable. Modal keyboard trapping, nested focus restoration, recovery non-dismissal, and scroll retention were checked. Capture helpers wait for image decode and finite animations, avoiding transient-opacity or unloaded-image false findings.

## Final source stamp

| File | SHA-256 |
| --- | --- |
| index.html | `aab89c102bbec9afd5ddbbd34ff3fb25a09950da19998d221a39979efa9973e1` |
| classic.css | `8a25cd2daf27b8df4499ac7e229372f33e5556e654acda16f02c889d83261ead` |
| classic.js | `c48a74444650521a23ba1878f6fa37622479a44974738380d612611c9ca7575d` |
| motion.css | `329a06bf16c495068913e5f38535ffbb5350765d0f4253775abd155b9723dcbd` |
| motion.js | `51fc804dd237038a3ba192159a20ba7aca0595af85c75162ef8e832fd74980c5` |

**No requested HTML coverage remains open.** Presentation-state coverage is representative rather than an irrelevant Cartesian product of every condition and route. The default unavailable-store fixture remains unavailable; purchase outcomes were reached through explicit preview routes/conditions and actual preview buttons. No real store/account operation was performed. Native reconstruction, backend correctness, real-device safe-area/runtime acceptance and final user approval are outside this HTML review.

`baseline-*`, `flows.json`, `final-sweep.json`, and the earlier `final-*` captures preserve discovery/history. **Use `r5-sweep.json` and the R5 evidence above as the final normal-layout authority.** Only evidence/helpers under this review directory were written by UI Review; implementation and test sources were not edited.
