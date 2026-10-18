# Classic Goal — Audit coverage and findings

Read-only implementation audit. Evidence generated only in this audit directory. Screenshots were captured before and during Fix changes, so this report records discovery coverage, not independent final acceptance. Review owns the final stable-build verdict.

## Screen inventory and actual rendered inspection

The review selector contains 32 surfaces; Exact is additionally reachable from its quote action (33 total). Talent aliases Legacy; UpgradeDetail aliases Player, so neither is a separate current surface. Baseline captures use `Screen-360x540.png` and `Screen-390x844.png`; all listed short-screen baseline pixels and the tall-screen baseline pixels for the ordinary sheets were inspected with view_image. Home/Player/Products additionally have all four requested viewport captures and inspection: 360x540, 360x640, 390x844, 450x916. Paired baseline capture names below omit the size suffix for brevity.

| Surface | Actual evidence and variants inspected | Verdict / finding |
|---|---|---|
| Home | Home at four sizes; state-Home-{new,opened,growing,progressed,prestige,rebuilt}-normal-360x540; active/cooldown | Shared stall/art/skills coherent. A1 invisible feedback; A6 first-stock CTA destination. |
| Player | Player four sizes; Player-bottom-360x540 | Toolbar, training, prestige, purchased counter, direct bulk and ten upgrade rows. A2 list viewport; A4 current nav marker. |
| Products | Products four sizes; new/insufficient/save-error state captures; all eight details and merchandise | A2 list viewport; row/frame/level/button alignment coherent. |
| Legacy | Legacy paired baseline; state-Legacy-new-normal; rebuilt/insufficient; collection-after-five-pull | Locked/unlocked, free/currency pull labels, disabled cards, rarity grouping. A2 viewport; A3 detail dismissal. |
| Shop | Shop paired baseline; utility-Shop-{loading,unavailable,pending,cancelled,failed,delivered} | All status copy, disabled/enabled pack actions, retry/status checks and collection route inspected. A2 viewport; shared A1 feedback. No bespoke pack art replacement needed. |
| Upgrades | Upgrades paired baseline; catalogue-{Owned,Upcoming,Full}; catalogue-search-{empty,id}; empty/insufficient state captures; skill-scope.json | Search/filter/pagination and row styles coherent. A5 target scope ignored. |
| BuyAll | BuyAll paired baseline; state-BuyAll-progressed-{stale,save-error} | Reviewed names/prices, budget, first purchases and persistent actions inspected. A7 stale warning below full list. |
| ProductDetail | state-ProductDetail-0-new-normal and 1–7-progressed-normal; ProductDetail-0/7 paired baseline, bottom captures | All eight unique names/icons and milestone fills inspected; detail layout/close/earnings coherent. A6 is incoming Home route, not a purchase mechanic change. |
| TapTraining | TapTraining paired baseline plus bottom; new state | Quantity choices, current/after quote, milestone, exact route, catalogue route and footer coherent. Shared A1 feedback. |
| Skills | Skills paired baseline | All three icons, states and detail actions coherent. |
| SkillDetail | All three ready baseline captures; new locked and active/cooldown states at360x540 | Locked/ready/active/cooldown content and action disablement inspected. A5 scoped catalogue destination. |
| CardDetail | card-detail-all-0 through27; card-details-contact-0 through6; baseline Common/Rare/Legendary | All28 full details read at native360x540. No art/text clipping defect; A3 missing explicit close. |
| DrawReveal | reveal-result-0 through4-360x540; baseline DrawReveal pair; reveal-results.json | Five-pull sequence includes Common duplicate/new, Rare duplicate, Legendary new and final Continue. Artwork, rarity faces, progress and outcomes coherent. |
| Prestige | Prestige paired baseline; state-Prestige-{new,progressed,prestige}-normal | Below-target/eligible previews, reset/keep, rewards, target and disabled/enabled CTA coherent. |
| Offline | Offline paired baseline; state-Offline-{new,progressed}-normal | Zero and nonzero reward, storage/efficiency summary and actions coherent. |
| Statistics | Statistics paired baseline | Both run/lifetime tables inspected. No additional visual defect. |
| Settings | Settings paired baseline; Settings-max-wide-name-360x540 | All three switch labels/states and four utility links inspected. A8 accepted name width escaped button. |
| Naming | Naming paired baseline; Naming-empty-submit; max-wide-name captures | A1 empty submission invisible; A8 accepted24-wide-glyph name overflow in consumers. |
| Privacy | Privacy paired baseline | Three information sections and Back/close coherent. No additional issue. |
| Account | Account paired baseline; utility-Account-save-error/failed | Guest/account paths, comparison, takeover, recovery, warning and footer inspected. Shared A1 failure feedback. |
| Welcome | Welcome paired baseline | Portrait, introduction, Google/guest choices coherent. |
| AccountCompare | AccountCompare paired baseline | Both save choices, nonmerge explanation and actions coherent. |
| Handoff | Handoff paired baseline | Takeover consequence and primary/back actions coherent. |
| SaveRecovery | SaveRecovery paired baseline; utility-SaveRecovery-save-error/failed | Blocking explanation, retry/account/new/exit routes coherent. Shared A1 failure feedback. |
| Purchase | Purchase paired baseline; Purchase-pack0 through3-360x540 | All four amounts/prices/draw labels, preview outcomes and Back inspected. No additional visual issue. |
| Appearance | Appearance paired baseline; state-Appearance-progressed-normal | Product signature stages and six permanent thresholds legible; no additional visual defect. |
| Help | Help paired baseline | Three gameplay guidance sections coherent. |
| Odds | Odds paired baseline | Three rarity percentages/per-card rates and duplicate/no-guarantee wording coherent. |
| Promote | Promote paired baseline | Copy/fresh/back choices and guest separation text coherent. |
| SignOut | SignOut paired baseline | Save requirement and maroon confirmation/back coherent. |
| RecoveryConfirm | RecoveryConfirm paired baseline | Separate new-local confirmation, keep-file text and back coherent. |
| RecoveryExit | RecoveryExit paired baseline | Blocking exit explanation and return to welcome coherent. |
| Exact | Exact-level-360x540 | Full magnitude value, explanation, header/close/Back coherent. |

## Meaningful conditions and applicability

- Normal: baseline every surface.
- Insufficient: Products and catalogue purchase controls/zero cash. This fixture condition concerns cash, so it is not treated as a separate KK-insufficiency mechanic.
- Active/cooldown: Home and all three skill details; Bulk Order correctly demonstrates cooldown rather than a timed active effect.
- Stale: BuyAll review and disabled confirmation (A7); Prestige stale shares its existing guarded confirmation style.
- Save failure: BuyAll, Products, Account and blocking recovery; modal warning visible, purchases guarded. Shared invisible transient failure feedback was A1.
- Loading/unavailable/pending/cancelled/failed/delivered: Shop screenshots explicitly inspected, with four pack review variants. These are preview outcomes; no network or payment acceptance is claimed.
- Empty: catalogue forced-empty, no-match search and an empty Upcoming view; disabled pagination and explanatory content inspected.
- Six game fixtures: Home screenshots for all six; new/opened products, growing pre-prestige, established, eligible prestige and rebuilt permanent-gain distinctions were inspected.
- Draw states: free draw, five-pull mixed credit/KK, disabled five-pull, new/duplicate outcomes, rarity faces, first/next/final progress; result artwork inspected.
- Forms: empty shop-name submission and maximum24wide-glyph entry, as well as default name/search/filter values.
- Scroll: initial/full/partial row and bottom capture evidence exists for long tabs/sheets. Fix adds shared cues and compact expanded panels; independent stable final Review must validate these changed layouts.

## Artwork and component inspection

| Family | Inspected evidence | Audit verdict |
|---|---|---|
| Layered stall/character/header | Home four sizes plus six fixtures | Coherent existing illustrated festive baseline; no broad art replacement needed. |
| Eight merchandise cutouts | icons-merchandise-displayed-sizes.png at37/54px; Home8slots; ProductDetail8variants | All eight recognizable, no broken crops/transparent halos requiring regeneration. |
| All28 dedicated card arts | card-art-inventory.png, native full detail screenshots0–27/contact0–6 | All28 distinct recognizable subjects; consistent palette and border context; no artwork replacement required. |
| Three full-card rarity outer/inner borders | Full detail set plus baseline CardDetail0/18/27 | Common navy, Rare plum and Legendary saffron/raspberry differ visibly; names/benefits fit. |
| Three reveal faces | reveal-result0–4 | Common/Rare/Legendary faces and level labels coherent at132x170 short-screen presentation. |
| Shared card back | Direct view_image of assets/cards/shared-card-back.png plus baseline reveal route | Centered symmetric motif, consistent palette/grain; no defect identified. |
| All19 symbol icons | icons-merchandise-displayed-sizes.png at30/36/48px | Each silhouette/crop inspected: player/shelf/legacy/gear/burst/hands/order/coins/tap/stats/skills/prestige/clock/cooldown/lock/play/chevron/close/add. No broken or wrong atlas symbol. |
| Shared ticket buttons/forms | All33 surfaces and state captures | Consistent painted blue/cream/maroon/disabled families. A1 feedback, A3 dismiss, A4 selected nav, A5 target scope, A8 long text. |
| Lists/rows/progress | Product8details, Player10rows, catalogue views, collection and purchase panels | Prices/names/action hierarchy coherent; A2 space/scroll and A7 warning visibility. |

No Unity, native gameplay, backend, economy or external account/payment behavior was edited or accepted by this audit. No user visual approval is inferred. Findings A1–A8 were sent with evidence and acceptance conditions to the parent; Fix and independent final Review are separate roles.

## Final quantitative scope correction — R4

The parent required verifier identified Home shelf touch targets at approximately69.97x42.05CSSpx at360x540, below48px. The raw initial-dom.json independently contains the same measurements for all eight display buttons; this audit captured them but did not promote them into its early findings. R4 is a real issue and must be included in the final ledger. No minimum-target pass is claimed by this audit before the fix.

Initial full-app button measurement covers settings, stall tap, all eight shelf display controls, skills, navigation, panel buttons and sheet controls. Final quantitative checks must additionally include input/select/textarea and nonbutton [role=button]/focusable wallet controls, only when visible and not inert/hidden. Measure Home and expanded-panel states separately because expanded panels intentionally hide/inert shelves. Preserve original painted positions, prevent overlapping active shelf targets, and require>=48x48 actual bounding boxes at all four specified viewports. Independent final Review owns the R4 acceptance and full-app control verdict.

R4 visual follow-up: inspected ../fix/Home-targets-360x540.png with view_image against this audit’s original Home-360x540.png. The eight merchandise images remain centered in their painted compartments, all eight nameplates remain readable, and the stall/counter/shopkeeper/header/skill/nav composition shows no visible overlap or art drift. No additional visual defect identified. The parent reports home-targets.cjs now verifies visible full-app role controls>=48px at all four sizes and pairwise-disjoint eight shelf targets; that quantitative pass and independent final Review are recorded by their respective owners, not inferred from pixels here.
