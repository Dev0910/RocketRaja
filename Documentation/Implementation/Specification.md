# Redesign implementation contract

Started 1 October 2026. User authorized implementing the full TODO, final commit, Android build and device update. Public deployment/store publication is outside that request.

## Source and boundaries

Use `Game Design/Planning Decisions and Review v1.md`, `Economy Specification v1.md`, `1000 Unlockables Catalogue.md`, and `Premium Economy v1.md`. Economy `2.0.0`, content `cash-only-v1`, local-only gameplay schema 2 and authoritative-premium schema 3. The former economy `1.0.0` / gameplay schema 1 remains recognizable as legacy evidence, never interpreted as the new economy.

Confirmed: 1,000 manual run purchases, unique lifetime U, no prestige gates; eight products; x6 minimum opening Fame; 28 automatic permanent cards (16/10/2, 75/22/3 odds); every duplicate +1 level; three first-prestige credits; fixed 100-KK draw; one 100-KK gift on second prestige; no rewarded income; 50%/8h base offline income; manually buy the first product using the matching discounted seed grant. Skills reset. Decorations persist. No new currency sources, automatic purchasing, equipment, pity, achievements or daily/weekly rewards.

Numerical formulae, presentation thresholds and detailed UX are prototype defaults. Tuning requires recorded evidence, never silently changes a confirmed mechanic. Catalogue prices preserve the authored six significant digits and x31 late-band ratio.

## Development boundary

Branch `codex/redesign-implementation`; same checkout is required by the attached Unity editor. Keep the pre-existing FirebaseRepositoryUriFix edit. Runtime profile root is `persistentDataPath/redesign-v2-disposable`; preference keys are separately namespaced. Live Firebase initialization is rejected before authentication or writes. Disposable account tests use fake services and the demo emulator, not production. Smoke tests use their own random temporary directories. Production builds fail while this boundary is enabled. Do not delete/reset existing installed data. Android update must retain the package/signature and use install-replace, never uninstall as a shortcut.

## Module boundaries

- Core content: typed definitions, stable indices, prerequisite graph, content digest; no Unity dependency.
- Core economy: Amount for approximate growing cash/rates; exact decimal-string backed BigInteger for unbounded counts/rewards. No floating-to-integer truncation of growing rewards.
- State/save: explicit schema/economy/content checks; run and persistent fields; immutable durable commands with rollback on failed local commit; UID/generation fences remain authoritative.
- Runtime: adapts the tested core to existing session and lifecycle wiring; separates menu input blocking from suspension.
- Editor: repeatable authored asset/export generation and Unity codec validation; no silent fallback to the old catalogue.
- Billing: verified server ledger is authoritative for paid currency/draws, never client-authored Firestore JSON alone. Fake grants cannot enter release builds.
- UI: native editable prefabs and authoring scripts only after the recorded per-screen review stages in `UI Builder/AGENTS.md`.

## Evidence and acceptance

`Documentation/Implementation/` contains baseline, transition, generated-content validation, core/Unity/device results, and remaining blockers. Add exact task evidence links to the TODO only after the relevant deliverable and checks pass. Human uncoached playtests, visual approvals, storefront sandbox results, and two-device handoff cannot be claimed from a simulator or one connected device. Preserve older validation as history.
