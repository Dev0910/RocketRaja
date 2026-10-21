# Rocket Raja Classic HTML — UI goal coverage

Status: COMPLETE. All 13 findings pass independent review; no open findings. HTML remains a candidate for user review.

The three reusable agents are UI Audit, UI Fix (sole implementation owner), and UI Review; each used gpt-6.1-sol at high effort. The user explicitly delegated wireframe and art-reference checkpoints. Agent acceptance does not claim user visual approval or Unity integration.

## Result

- Expanded panels retain painted shop context and provide room for products, upgrades and Talent cards, with honest scroll cues.
- In-game feedback, inline name validation, logical keyboard focus, named switches, visible card dismiss controls and current-tab indication are implemented.
- First stocking, contextual upgrade scopes, stale-quote explanations, long shop names, desktop review links and shelf hit areas are corrected.
- Existing Gouache Storybook artwork, game rules, features, exported content and economy fixtures are retained. No Unity/backend changes, commits or pushes were made for this goal.

[Live preview](http://127.0.0.1:8765/Classic/) · [Before: Products 360×540](audit/Products-360x540.png) · [After: Products 360×540](review/r5-Products-360x540.png)

## Coverage

33 current surfaces; 69 route/art variants at four phone sizes (360×540, 360×640, 390×844, 450×916); 82 representative state captures. All 13 condition families are covered where applicable across six progression fixtures. All 28 card artworks, three rarity frame families/card back/reveal faces, eight product icons and 19 interface symbols were visually inspected.

| Surface | Audit | Independent final review |
| --- | --- | --- |
| Home | Complete | PASS independent final visual and behavioral review |
| Player | Complete | PASS independent final visual and behavioral review |
| Products | Complete | PASS independent final visual and behavioral review |
| Legacy | Complete | PASS independent final visual and behavioral review |
| Shop | Complete | PASS independent final visual and behavioral review |
| Upgrades | Complete | PASS independent final visual and behavioral review |
| BuyAll | Complete | PASS independent final visual and behavioral review |
| ProductDetail | Complete | PASS independent final visual and behavioral review |
| TapTraining | Complete | PASS independent final visual and behavioral review |
| Skills | Complete | PASS independent final visual and behavioral review |
| SkillDetail | Complete | PASS independent final visual and behavioral review |
| CardDetail | Complete | PASS independent final visual and behavioral review |
| DrawReveal | Complete | PASS independent final visual and behavioral review |
| Prestige | Complete | PASS independent final visual and behavioral review |
| Offline | Complete | PASS independent final visual and behavioral review |
| Statistics | Complete | PASS independent final visual and behavioral review |
| Settings | Complete | PASS independent final visual and behavioral review |
| Naming | Complete | PASS independent final visual and behavioral review |
| Privacy | Complete | PASS independent final visual and behavioral review |
| Account | Complete | PASS independent final visual and behavioral review |
| Welcome | Complete | PASS independent final visual and behavioral review |
| AccountCompare | Complete | PASS independent final visual and behavioral review |
| Handoff | Complete | PASS independent final visual and behavioral review |
| SaveRecovery | Complete | PASS independent final visual and behavioral review |
| Purchase | Complete | PASS independent final visual and behavioral review |
| Appearance | Complete | PASS independent final visual and behavioral review |
| Help | Complete | PASS independent final visual and behavioral review |
| Odds | Complete | PASS independent final visual and behavioral review |
| Promote | Complete | PASS independent final visual and behavioral review |
| SignOut | Complete | PASS independent final visual and behavioral review |
| RecoveryConfirm | Complete | PASS independent final visual and behavioral review |
| RecoveryExit | Complete | PASS independent final visual and behavioral review |
| Exact | Complete | PASS independent final visual and behavioral review |

## Findings

| ID | Area | Status |
| --- | --- | --- |
| A1 | In-app feedback / Naming validation | PASS |
| A2 | Expanded panel usable content / scroll affordance | PASS |
| R1 | Focus preservation across settings refresh / nested sheet close | PASS |
| R2 | Review screen selector | PASS |
| A3 | CardDetail explicit dismiss | PASS |
| A4 | Current-tab indication | PASS |
| R3 | Desktop review comparison link | PASS |
| A5 | Scoped skill upgrade catalogue / quote consistency | PASS |
| A6 | First-stock Home CTA | PASS |
| A7 | Stale BuyAll explanation placement | PASS |
| A8 | Maximum-length shop name boundaries | PASS |
| R4 | Home shared shelf touch targets | PASS |
| R5 | Expanded scene shop-name alignment at taller widths | PASS |

## Verification

| Check | Result |
| --- | --- |
| Existing layout suite | 136 route/viewport checks pass |
| Current UI regression suite | 44 route/viewport checks plus regression assertions pass |
| State suite | 30 state checks plus desktop capture pass |
| Connected interactions | 62 checks pass |
| Motion / reduced motion | 38 checks pass |
| Focused polish | 243 checks pass |
| Scope / first stock / stale quote / names / scroll follow-up | 187 checks pass |
| Shared Home touch targets | Four viewport sets pass; eight shelf targets disjoint, all visible controls at least 48×48 px |
| R5 compact header | 48 expanded route/name/size cases pass; four Home comparisons show zero changed decoded pixels |
| Independent normal visual/layout sweep | 276 checks pass on final source; all pixels inspected; zero errors |
| Independent state sweep | 82 final-source state captures pass and are visually inspected |
| Independent deep flows | 219 final-source assertions pass |
| Independent accessibility/tooling | 78 final-source records pass |
| Independent names and scroll boundaries | 60 records pass |
| Independent shared controls | 20 geometry sets + 32 shelf clicks pass; no accidental Tap events |

## Evidence and authoring

- [Detailed Audit coverage](audit/COVERAGE.md)
- [Definitive independent Review](review/REVIEW.md)
- [Machine-readable screen/state/component/finding ledger](coverage.json)
- [Final source hashes](final-source-hashes.json) and [unchanged content/fixture proof](fixture-integrity.json)
- Independent review evidence: review/r5-sweep.json, final-states.json, r5-deep.json, r5-acceptance.json, final-name-scroll.json, final-shared-targets.json and r5-headers.json.
- Final responsive-header evidence: fix/R5/compact-header-report.json and review/r5-headers.json; all 48 final independent header views pass.

Current Classic HTML/CSS/JS are the editable source. The old build.cjs references deleted HTML and must not regenerate this version; ART-DIRECTION.md documents this boundary. Historical deleted files and unrelated pre-existing edits were preserved.

Preview auth, payments, saves and rewards are disposable examples; browser verification establishes HTML behavior, not live service or native Unity acceptance.

The definitive normal sweep is `review/r5-sweep.json`; earlier `final-sweep.json` is retained as pre-R5 discovery evidence and is superseded. Final sweep/state/header reports match all five current UI source hashes before and after their runs.
