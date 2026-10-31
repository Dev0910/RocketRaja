# Gouache Bazaar UI unification

Status: HTML qualified at **95.9/100** and native Unity at **95.1/100**, each in two consecutive complete audits. Every HTML screen is at least **94.5**; every native screen is at least **92.5**. These are documented self-assessments, separate from test counts. Android device runtime validation remains pending because no device or emulator is available.

Checkpoint `2cda0d5120bf47303736d72e46869e95aee4d819` was pushed and verified on `codex/ui-unification` before redesign edits.

Approved decisions: Gouache Bazaar; broader UX improvements within existing destinations; full stall retained on all main tabs; selective replacement of mismatched art permitted; automatic Unity transfer after the fixed HTML gate. Gameplay, economy, persistence and service authority remain unchanged.

The immutable scoring contract is [rubric.json](rubric.json). Scores and visual deductions are recorded separately from functional checks. Existing production routes are being revised; no new gameplay destinations are introduced.

The source of presentation values is `UI Builder/HTML/Classic/system/tokens.json`. `artwork.json` records the reused source assets. The component gallery, full route gallery, scored passes and native evidence are added as verification progresses.

## HTML qualification

See [scores and deductions](scores.json), [fixed rubric](rubric.json), [gate](quality-gate.json), [component contract](COMPONENTS.md), [first full gallery](gate-a/index.html), [final full gallery](gate-b/index.html) and [repeatability comparison](repeatability.json). The two full matrices each cover 34 routes at four viewports (272 route/viewport states total), including scroll endpoints. All reported clipping, overflow, asset, font-family and target-size failures are zero. The original baseline scored 68.5. Iterations scored 72.4, 85.6 and 88.0 before the two final 95.9 audits.

Separate functional coverage: 28 interaction checks, 100 fixture/failure-state checks, 103 tutorial checks, 37 feedback checks and 45 device/safe-area checks. Seven text/background pairs exceed 4.5:1. Loading, tutorial and active Classic handoffs pass the package inspector; archived prototypes are excluded from the import and remain untouched. The stage directory is recorded in handoff.json.

The full stall is retained; short Home scrolls rather than shrinking essential UI. Minor remaining deductions concern this extra gesture, dense long reviews and historic illustration texture differences. No major/critical defect or missing interaction is being carried into Unity.

## Native qualification

See the [native implementation](native/README.md), [scores and deductions](native/scores.json), [final gallery](native/gate-b/index.html), [first complete audit](native/gate-a/index.html) and [Android build report](native/android-build.txt). Each native audit covers 255 main states plus 12 startup states at the four required sizes. Actual text-glyph overflow and undersized captured buttons are zero. The native journey separately passed 68 tutorial checks, 40 feedback checks, 250 nested-popup/component checks, 25 matrix/edge checks, 12 startup visual checks, and seven isolated startup cases. Aggregate and repeated checks are reported separately; these numbers are not summed into a design score.

Complete prior Main and Bootstrap roots are retained inactive in rollback scene copies. No gameplay/economy files or save schemas were changed. The qualified HTML commit is `a5a4f6676bd1edf8b016037db9fb27703fd77097`; the Unity implementation is a separate final milestone on the same `codex/ui-unification` branch.
