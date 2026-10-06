# Regression contract audit — 2 October 2026

RR-10-01/05 are complete for the implemented prototype contracts. The default Unity `ValidateGame.Run` dispatches the new core/save codec (23 checks); the explicit `ValidateGame.RunLegacy` (7 checks) and EconomyLab retain old content assertions to validate old saves and their compatibility behavior. They must not be used as evidence for the redesigned live catalogue.

| Previous expectation | Current gameplay expectation and evidence |
|---|---|
| 96 cash entries | 1,000 authored stable IDs, import validation, runtime/model parity, virtualized native list and actual full Buy All. Core 649 plus catalogue Play Mode 69 and visual progression 36. |
| 12 purchasable Legacy ranks | 28 uncapped Talent cards with free/KK draws, immutable committed results, duplicate level gains, retained effects and Prestige grants. Talent native 44/Play Mode 54; core per-card/failure coverage. |
| Rewarded ads and sponsor bonuses | No ad SDK/control/income; obsolete callbacks reject rewards and saved sponsor timestamps are inert. Prior ad-retirement asset/Play Mode evidence is retained; current legacy compatibility rerun passes 310. |
| Original reset/reward model | Eligible earned cash, deterministic Fame/free-credit review, one-time second gift, U retained, current-run upgrades reset. Core boundary tests, Main Play Mode 60, visual progression 36. |
| Original settings/name/quantity behavior | Durable independent music/sound/reduced-motion, literal store names, x1/x10/x100/Max, recovery and no leaked taps. Main 60, catalogue 69, Home 31, guidance 47. |
| Original cloud ownership | Existing protocol remains covered by 28 legacy-cloud checks. New schema/wallet/session invariants have redesigned core coverage, 61 server tests, 22 localhost emulator rules/Admin transaction tests, 75 native account scene-flow checks and 40 native recovery checks. This does not establish real sign-in or two-device acceptance. |

Latest compatibility outputs: [legacy core](Regression-LegacyCore-Current.txt), [legacy cloud](Regression-LegacyCloud-Current.txt), [premium authority](Premium-Authority-Tests.txt), [demo rules/Admin transactions](Firestore-Rules.txt). The emulator rerun initially failed before starting because its Java cache was missing; restoring the same checksum-verified portable runtime resolved the runner issue. The successful run used only the `demo-rocket-raja` localhost target, with all 22 tests passing and no skips.

`SmokeHarness` now selects redesigned checks when Main owns `RedesignApplication`, before accessing the legacy engine. The actual Bootstrap/Main run passes [176 checks](NativeSmoke/SmokeResults.txt): 1,000 upgrades/28 cards, eight saved rows, final-item scroll, reviewed Buy All, final appearances, Prestige reset/retention, durable reveal, manual rebuild and eight additional staged native cycles. The legacy branch remains available for old fixture scenes. All fixture setup is Editor/development-only and creates a separate temporary profile; ordinary play does not use it. [Reproduction and measurement limits](NativeSmoke/README.md).

The current four Unity build-isolation checks also pass. These integration results close the test-contract tasks, not device, live-account, purchase-provider or human acceptance. Android and repeated-run measurements remain RR-10-06/11-04 work; real billing is deferred by the user.
