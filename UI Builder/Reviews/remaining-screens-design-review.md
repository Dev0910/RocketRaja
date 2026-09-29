# Delegated design review — remaining screens

Review date: 2026-09-17. Authority: the user's current instruction explicitly substitutes subagent detailed review and resulting revisions for wireframe/reference input gates. This is delegated design review, **not actual user visual approval**. Unity stays read-only. No artifact acceptance is recorded until the actual revision is visually inspected.

## Evidence and boundaries

Read image-to-html/SKILL.md, UI Builder/AGENTS.md, UI Builder/ART-DIRECTION.md, Architecture.md, GameView.cs, GameController.cs, relevant GameEngine symbols and active GameContent.asset. Gouache Storybook scene-only option 05 is global style authority. Old Player draft is not design authority. Inspiration images do not introduce mechanics.

## Screen requirements / review checklist

| Screen | Required content and behavioral limits |
| --- | --- |
| Player | Salesmanship level, tap income and cost; Prestige entry; Statistics; 96 unlockables with purchased/locked/unaffordable/available states; Buy all available; skill unlock/rank purchases belong here. Upper stall retained. |
| Products | Eight authored families, sequential unlock, level and normal income per second, display milestone, quote-driven Buy 1/10/100/Max; no inventory depletion or fulfillment. Preserve scroll. |
| Legacy | Reputation balance, permanent-perk explanation, 12 authored perks, five ranks, cost and maxed/unaffordable states. Eight product perks, tap, all product, offline cap, cooldown. |
| Statistics | This-run total and prestige-eligible earnings separately; lifetime earnings, manual taps, prestiges, lifetime reputation. Runtime is modal; no achievements or invented charts. |
| Skills | Read-only guide. Festival Rush 30 sec at x2/x2.5/x3/x4, base cooldown 300 sec; Helping Hands 30 sec at 5/8/12/16 automatic taps/sec, base cooldown 300 sec; Bulk Order immediate 120/180/240/300 sec of normal product income, base cooldown 600 sec. Home-only activation, Player-only purchases. Guide route is a presentation adaptation, not existing runtime API. |
| ProductDetail | Read-only product explanation / display tier presentation or same existing quote/buy operation if specified. New detail route is a presentation adaptation absent from current GameView, not new gameplay. |
| Prestige | Own all eight families and meet eligible threshold; sponsor/offline-ad extras excluded. Ready view enumerates reset cash/shelves/levels/tap/run upgrades/skills/appearance; keep reputation/Legacy/lifetime/settings. Exact quote validated on confirm. Success opens Legacy. |
| Settings | Music, Sound, Reduced motion; Privacy information; consent choices conditional on Ads.PrivacyOptionsRequired. No cloud/account reset/language purchases. |
| Offline | Reward already added to wallet; continue without video dismisses offer. Optional video doubles via callback only. Base is 50% normal income, cap 8 + 2 × offline Legacy rank hours. Never show Claim for the credited base. |
| Sponsor | Optional video, x2 automatic sales for five minutes, extra income does not count for Reputation. Active state prevents duplicate request. No purchase/subscription/shop. |
| Privacy | Open configured production privacy URL when present; fallback development disclosure says progress local and Google ads/consent, release config pending. Conditional SDK consent entry; no invented policy or HTML consent imitation. |
| SaveRecovery | Blocking message for unreadable main and backup, preserve damaged files, no cloud recovery. Start a new local game or Keep files and exit. No general close/back to live game. |

## Cross-screen acceptance checks

1. Portrait 1080 × 2200 composition with safe areas. Three bottom tabs only: Player, Products, Legacy. Home is collapsed state, selected tab or collapse arrow restores it. Persistent upper stall on tabs. Lists retain scroll.
2. Cash/settings header has no shop rank heading or progress bar. Skills activation appears on Home only. Long labels wrap without intersecting prices/actions. Minimum controls 48 logical pixels (at 1080 art width, use appropriately scaled hit regions).
3. Gouache shapes/pigment, warm cream/maroon/teal/ochre with indigo dusk; live readable text. Avoid flat vector or glossy plastic icon drift. Paint is artwork rather than baked labels.
4. Normal income remains distinct from temporary boosted income. Input blocking prevents overlays, scrolls and controls earning taps. Modal/ad time excluded from earnings; cooldowns use absolute time.
5. Art-reference output must match reviewed wireframe hierarchy/content. Generated strings/values are illustrative, not authoritative. Revision notes name exact files and honest delegated acceptance status.
6. HTML fixture controls are review-only; no real saves, ad calls, rewards, or competing economy. Runtime routes and unimplemented presentation adaptations must be distinguished in metadata.

## Review status

Awaiting actual wireframe and generated-reference revisions for visual inspection. No stage acceptance yet.

## Wireframe v1 visual review — changes required

Inspected all twelve `Wireframes/<Screen>/wireframe-v1.png` previews alongside screen-specs.json and Player NOTES. Verdict: revise before generated references.

1. **Player, Products, Legacy:** the last row extends beneath the rounded lower panel boundary. Define an actual clipped scrolling viewport and a scrollbar/continuation affordance. Do not compress target height to fit everything.
2. **Player:** one Open action cannot unambiguously route to both Statistics and Skill guide. Give each its own named control. Skill unlock purchase remains distinct from guide navigation.
3. **Products:** render four actual quantity choices with visible selected state. Replace generic Upgrade/Stock shelf actions with quoted cost and quantity. Add a separate Details affordance if ProductDetail is part of the flow. Label total normal automatic sales, with boosted total separate when present. The state called max-level must mean actual engine cap (if any), not merely final display tier.
4. **Offline:** available Double reward and No video available coexist in the same representative screen. Split mutually exclusive states. Disable/hide unavailable video action; continue remains available. Credit wording is correct.
5. **Prestige:** remove Quote bound to run and revision from the player-facing surface; use Your shop pauses while you decide. The implementation fact belongs in metadata. **Sponsor:** replace successful video callback wording with Complete the video to receive your bonus. **Settings/Privacy:** conditionals belong in notes, not generic provider labels on the live screen.
6. **All:** draw clear button boundaries and appropriate 48-logical-pixel hit regions. Bold bottom-right text is currently used identically for interactive actions and passive status. Skills Ready/Active/Wait and purchased upgrades need passive status badges; settings need explicit toggle affordances.
7. **Statistics:** replace tall empty section-heading cards with compact section headings; separate lifetime earnings, manual taps, prestige count and reputation into labeled value rows instead of slash pairs.

Positive checks: persistent two-by-four stall placeholder, three tabs, no rank header, Home collapse, modal shading, no Skills activation, complete Prestige reset/keep groups, read-only product detail, already-credited Offline amount, no invented economy/account/cards/cloud purchases. SaveRecovery correctly omits general close and preserves exit alternative.

Review reflects wireframes only; no generated-reference acceptance or user approval claimed.

## Wireframe v2 visual review

Inspected all twelve `wireframe-v2.png` previews. **Delegated wireframe acceptance** for Skills, ProductDetail, Prestige, Settings, Offline, Sponsor, Privacy and SaveRecovery v2: no blocking composition/behavior defects. This is explicitly not user visual approval. Their generated-reference stage may proceed from these exact revisions.

Player, Products, Legacy and Statistics require small targeted corrections:
- Player utility buttons paint over the Statistics and skill information subtitle. Remove that redundant subtitle or allocate more vertical room.
- Products needs visible Details entry separate from purchasing, plus selected-state outline/fill on the current quantity (x1). Details cannot exist only in NOTES.
- All panel continuation labels intersect partial clipped action labels. Reserve an opaque footer below the scrolling clip, rather than overlaying text onto the last visible row.
- Statistics Back is 144 px high within a roughly 112 px row, protruding below its card. Give action row at least 164 px.

Nonblocking polish: shrink modal surface to content plus padding instead of stretching every modal to the same near-full-screen height; keep rank fractions together in Legacy; remove literal STATUS suffix from final live skill labels (wireframe annotation only). Settings ON/OFF buttons are acceptable toggle affordances if HTML uses appropriate toggle semantics and visible state.

## Revised v2 acceptance — remaining four screens

Re-inspected `Player/wireframe-v2.png`, `Products/wireframe-v2.png`, `Legacy/wireframe-v2.png`, and `Statistics/wireframe-v2.png` after in-place targeted revision. **All four pass delegated wireframe review.** Player subtitle collision removed; Products has distinct dashed details hit region and selected x1 outline/fill; panel continuation footer is opaque and separate; Statistics Back now fits its containing row. No further blockers at wireframe stage.

Per-screen gate status: Player v2 accepted; Products v2 accepted; Legacy v2 accepted; Statistics v2 accepted. Combined with preceding eight acceptances, all twelve screen-specific wireframes may proceed to generated references. This records the user's authorized delegated review substitution, never actual user visual approval. Generated references still require their own detailed review.

## Generated-reference v1 review — Player, Products, Legacy

Visually inspected all three screen-specific `Reference Images/<Screen>/reference-v1.png`. Gouache palette/material/render quality passes; clear cream/teal/maroon panels and strong hierarchy are coherent. **Pending revision, not accepted**:

- Player: restore missing panel-collapse control; restore opaque continuation footer / Scroll for more.
- Products: correct Chakri normal income to fixture ₹179.2/sec (generated ₹523.6 is drift); generated purchase numbers remain illustrative, never economic authority.
- Legacy: Trusted festival brand rank 0 costs 3 reputation, not generated 1.
- Shared persistent scene: Player/Legacy dropped shopkeeper, Products added a differently positioned character. Maintain the same approved Home upper scene and character across tab switches.
- Shared navigation: all three images assign changing icons to the same tabs (diya/silhouette, burst/gift, skyline/trophy/diya). Use consistent Home set: Player shopkeeper portrait, Products shelf, Legacy existing Legacy emblem. Preserve selected state.

Exact image revisions must be reviewed after correction before these screens enter HTML construction. Painted generated text will be recreated as live text, not copied as gameplay authority.

## Generated-reference v1 review — Statistics, Skills, ProductDetail

Visually inspected all three reference-v1.png images. Data/content match intended fixtures; Skills has passive status chips and correctly retains Home-only activation copy; ProductDetail remains read-only and its stages/income are correct. Gouache materials pass.

Required edits before acceptance:
- All three: restore full-background indigo modal scrim, including header controls and bottom tabs. Current background is fully lit and appears available, contrary to the blocking modal wireframe. Use approved Home background/navigation continuity.
- Statistics: merge generated Return to Player and Back into one return button. The wireframe label + action annotation was incorrectly rendered as two controls.
- ProductDetail: merge generated Return to Products and Back into one return button for the same reason.

Pending corrected reference revision. Skills card composition itself passes; no new skills controls or behavior requested.

## Reference gate — revised panels and first settings/prestige candidates

Inspected Player reference-v2.png, Products reference-v2.png, Legacy reference-v2.png, Prestige reference-v1.png and Settings reference-v1.png.

- **Player v2 accepted for lower-panel visual direction.** Collapse and continuation controls restored. The approved wireframe down-chevron direction remains authoritative over generated up-chevron.
- **Products v2 accepted for lower-panel visual direction.** Chakri normal income corrected; independent detail and purchase controls, quantity selection and continuation present.
- **Legacy v2 pending v3 correction:** cost is now correct (3 reputation), but generated effect text became x3 reputation per rank, which invents gameplay. Must read x2 all product income per rank.
- **Prestige v1 accepted for modal-card visual direction.** Exact reset/keep content, paused-review explanation, Confirm and Cancel present and legible.
- **Settings v1 accepted for modal-card visual direction.** Three distinct toggles, privacy information, conditional advertising choices, Back and close present and legible.

Scope of acceptance: generated images establish lower-panel/modal materials, hierarchy and illustration, **not replacement Home layers or navigation**. Existing Home scene layers, cash/settings layout, shopkeeper and Home navigation DOM/icons remain explicit inherited authority. All modals use the reviewed wireframe full-background input-blocking scrim, including header and navigation. Generated slight backdrop repainting/nav-icon drift is recorded as non-authoritative, not a new design decision. Live fixture numbers and runtime conditions remain authoritative over generated decorative values. This avoids repeated image rerenders for elements not being reconstructed from these references.

These are delegated reference gate acceptances under the user's explicit instruction, not actual user visual approval. HTML output remains candidate until user review.

## Reference gate — Offline, Sponsor, Privacy, SaveRecovery v1

Visually inspected each screen-specific reference-v1.png. **All four accepted for modal-card visual direction**, subject to the already-recorded inherited Home background/navigation and full blocking modal scrim boundary.

- Offline: base amount explicitly already credited, 50% normal sales / 8h base cap explanation, optional matching extra amount, Double reward and Continue without video preserved. No unavailable contradiction.
- Sponsor: x2 automatic sales for five minutes, reputation exclusion, explicit video action and exit preserved. No invented shop/payment mechanism.
- Privacy: local progress/no cloud, Google advertising and consent, development disclosure, conditional provider privacy entry and Back preserved. This remains development fallback, not an invented production policy.
- SaveRecovery: no close affordance, unreadable main and backup, preservation promise, fresh local game and Keep files and exit alternatives all legible. HTML must retain blocking behavior and preview-only actions.

No missing card controls, overlapping text or incorrect mechanics found. Generated backdrop and navigation variants remain non-authoritative. Acceptance is delegated review, not actual user visual approval.

## Final remaining reference gate

Visually inspected Statistics reference-v2.png, Skills reference-v2.png, ProductDetail reference-v2.png and Legacy reference-v3.png. **All four accepted for their screen-specific card/panel visual direction.** Statistics/ProductDetail now each have one return button; all three modals show background scrim; Skills retains only passive readiness states; Legacy Trusted festival brand effect is correctly x2 all product income per rank and cost is 3 reputation.

Statistics v2 generated an extra decorative miniature stall attached above its statistics card. This is non-authoritative background decoration, outside the accepted modal card; omit it in HTML and use only inherited Home layers behind the scrim. No extra stall or new scene layer is approved. Other previously recorded inheritance/fixture constraints remain in effect.

All 12 remaining-screen reference gates are now complete through delegated review, not actual user visual approval. Latest accepted references: Player v2, Products v2, Legacy v3, Statistics v2, Skills v2, ProductDetail v2, Prestige v1, Settings v1, Offline v1, Sponsor v1, Privacy v1, SaveRecovery v1.

## HTML candidate review — 2026-09-17

Inspected refreshed 360x540 progressed captures for all 12 screens, Shared/screens.css, Shared/preview.js, Skills embedded metadata and verification report. This is independent code/pixel review, not Unity runtime verification or user approval.

Visual checks pass: legible modal titles, multiline Settings heading, 48px controls, fixed modal footer and scrollable inner bodies; clipped panel lists; three tabs; strong modal dimming; no save recovery dismiss. Full collections are catalog-rendered in scroll bodies, not truncated to reference-visible rows. Embedded contract accurately marks Skills/ProductDetail new presentation routes as existing-needs-adaptation, uses mixed profile, documents candidate status and code-only evidence. No economy/ad/save writes occur in preview.

Material fixes requested:
1. **Direct Privacy return route:** initial Privacy navigation opens it over Home with no Settings stack entry, so Back to Settings actually returns Home. Ensure direct-page footer opens Settings, while nested route preserves existing history.
2. **Coherent frozen fixtures:** all eight products are owned and listed incomes reach ₹5.87M/sec, but total says normal ₹6.2K/sec. Home per-tap fixture ₹250 disagrees with Player ₹18. Static fixtures should agree across views; no simulation required.
3. **Prestige fixture consistency:** progressed Statistics eligible ₹3.6M is far below ₹170Q while Prestige offers +5. Reserve ready quote for a threshold-meeting fixture or make progressed Prestige locked.
4. **Independent Reduced motion state:** SetSetting handler sets app.dataset.reducedMotion to false on every music/sound toggle. Read motion switch state independently instead.
5. **Atlas crop edge:** ProductDetail Phuljhari still shows a narrow Anar sliver at its right edge. CSS overflow fixed the broader leak, but source viewBox needs a tighter boundary.

No broader redesign requested. Final review should confirm these fixes and preserve short-screen scroll/footer behavior.

## Final HTML candidate review outcome

**Accepted through delegated candidate review; no remaining material blockers.** Rechecked changed Shared/preview.js: direct Privacy creates Settings history; Reduced motion reads its own independent switch; ready/stale Prestige fixtures use ₹170Q eligible income while progressed remains locked; Home/Player tap rate agrees at ₹18; normal/live total labels agree with displayed product income rounding at ₹6.71M/₹13.42M.

Re-inspected refreshed ProductDetail 360x540 capture modified 05:01:42: explicit source-image clipping removes the neighbouring Anar wedge while preserving Phuljhari artwork. All five HTML review findings resolved. Final verification report modified 05:01:51 records 17 interaction checks including targeted regressions and no browser errors; main agent reports 79 viewport/state checks passed. No Unity runtime validation claimed.

All twelve deliverables remain **HTML candidates for user review**, not actual user-approved visuals or Unity implementations. Delegated wireframe/reference gates and this independent HTML review are complete under the user's instruction. Native Unity sources/assets/saves remain outside this task.
