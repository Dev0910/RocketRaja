# Gouache Bazaar UI unification

Status: HTML qualified at **95.9/100** in two consecutive complete audits; every screen is at least **94.5**. Native implementation is next. This is a documented self-assessment, separate from test counts.

Checkpoint `2cda0d5120bf47303736d72e46869e95aee4d819` was pushed and verified on `codex/ui-unification` before redesign edits.

Approved decisions: Gouache Bazaar; broader UX improvements within existing destinations; full stall retained on all main tabs; selective replacement of mismatched art permitted; automatic Unity transfer after the fixed HTML gate. Gameplay, economy, persistence and service authority remain unchanged.

The immutable scoring contract is [rubric.json](rubric.json). Scores and visual deductions are recorded separately from functional checks. Existing production routes are being revised; no new gameplay destinations are introduced.

The source of presentation values is `UI Builder/HTML/Classic/system/tokens.json`. `artwork.json` records the reused source assets. The component gallery, full route gallery, scored passes and native evidence are added as verification progresses.

## HTML qualification

See [scores and deductions](scores.json), [fixed rubric](rubric.json), [gate](quality-gate.json), [component contract](COMPONENTS.md), [first full gallery](gate-a/index.html), [final full gallery](gate-b/index.html) and [repeatability comparison](repeatability.json). The two full matrices each cover 34 routes at four viewports (272 route/viewport states total), including scroll endpoints. All reported clipping, overflow, asset, font-family and target-size failures are zero. The original baseline scored 68.5. Iterations scored 72.4, 85.6 and 88.0 before the two final 95.9 audits.

Separate functional coverage: 28 interaction checks, 100 fixture/failure-state checks, 103 tutorial checks, 37 feedback checks and 45 device/safe-area checks. Seven text/background pairs exceed 4.5:1. Loading, tutorial and active Classic handoffs pass the package inspector; archived prototypes are excluded from the import and remain untouched. The stage directory is recorded in handoff.json.

The full stall is retained; short Home scrolls rather than shrinking essential UI. Minor remaining deductions concern this extra gesture, dense long reviews and historic illustration texture differences. No major/critical defect or missing interaction is being carried into Unity.
