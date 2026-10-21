# Redesign rollout and recovery plan

Prepared 1 October 2026 for RR-11-06. **The game is not release-ready.** This packet records versions, recovery behavior and the installation sequence; it authorizes no deployment or store publication. Final acceptance remains RR-11-07. The user's requested final commit, build and replacement on the connected phone occur only after all TODO tasks are complete.

## Candidate identity and evidence

| Item | Current value |
|---|---|
| Branch | `codex/redesign-implementation`; final commit not created |
| Starting revision | `579291f352eb39efbda7844ce55c3b4765e739be` |
| Engine | Unity 6000.4.9f1; Android ARM64 IL2CPP |
| Economy / content | `2.0.0` / `cash-only-v1` |
| Authored source SHA256 | `C0904702675993BCF0B3F647A8A1DC71E6A4FBFA5A455B97EFE58F43EFEFA5F7` |
| New local gameplay / authoritative gameplay | Schema 2 / schema 3 |
| Payload/checkpoint envelopes | Schema 1; gameplay versions validated separately |
| Existing device application | `com.diwali.fireworkstycoon.dev`, version 0.1.1; preserve its data and signing identity |
| Final app version, version code, artifact hash and commit | Unassigned until acceptance and final build; do not reuse the isolated diagnostic identity |

The [evidence index](README.md) and [hash inventory](EvidenceManifest.json) identify the current implementation and reports. The synthetic migration fixtures and immutable source archives are under [Fixtures/Legacy](Fixtures/Legacy/README.md); the [core report](Core-Redesign.txt) covers original-byte preservation, retry into the same run, rejection of arbitrary source paths, future-version protection, corrupt backups, failed writes and authoritative wallet downgrade/ownership fences. These are isolated checks, not approved compensation for deployed progress.

## Supported data and transition

| Data | Load/update policy | Recovery requirement |
|---|---|---|
| Legacy schema 1, economy 1.0.0 (`diwali-v1.save`, old account checkpoint and `user` collection) | Recognize and preserve; never treat old IDs as a corrupt new save or reset a real player | Obtain a reviewed real-player mapping before enabling conversion; keep original bytes and backups |
| Local-only schema 2, economy 2.0.0 | Validate content and fields before loading `rocket-raja-v2.save` | A recovered stale backup cannot enable premium spending without reconciliation |
| Authoritative schema 3, economy 2.0.0 | Validate UID/profile, exact wallet revision and free/server card components | Recover from the server ledger; never downgrade to schema 2 to regain spending |
| Missing new save in an identified disposable profile | Create one manual-purchase seed grant and zero progression | Do not confuse corrupt/future/legacy files with missing data |
| Future schema/economy/content or invalid envelope | Stop mutation and retain evidence | Use a compatible app or explicit recovery; no fallback that overwrites newer data |

The exact policy is [SaveTransition.md](SaveTransition.md). The present development namespace is `redesign-v2-disposable`; it is not the selected production save location. New cloud progress uses `redesignUser`; local account checkpoints use the `economy-2` subdirectory. Production profile selection and the deployed-player transition remain acceptance inputs, not an excuse to copy old saves into a new reset path.

## Paid entitlement recovery

Preserve `premiumWallets`, `premiumReceipts`, `premiumProfiles`, `premiumRevocations` and `premiumDraws/{uid}/operations` across app rollback and server updates. A purchase-token digest deduplicates receipts; a pending draw's persisted operation identity recovers the same debit/card result. Never delete these records, replay a paid grant from client cash, edit a wallet to match an old backup, or grant a replacement draw because a reveal was interrupted.

Before acknowledging/consuming a purchase, reconcile its verified server result and durably save the wallet cache. Unknown network outcomes remain pending; only a definitive server rejection clears a rejected draw. Recover an account under its actual UID/session lease and retain immutable checkpoint identity. Refunds can produce a wallet deficit without deleting the already delivered cards. The [premium authority contract](PremiumAuthority.md) and [undeployed hosting review](PremiumDeploymentReview.md) define these boundaries; no production receipt evidence exists yet.

## Build and installation sequence after acceptance

1. Close every TODO with its current user-authorized scope against a tested candidate: native UI, simulated purchase/recovery, phone behavior and uncoached player observations. Actual Google Play linking is explicitly deferred for this development delivery; simulated evidence must not be called storefront acceptance. Record live-account/two-device limitations. The development namespace preserves legacy saves without converting them; a future deployed-player conversion still requires a reviewed mapping. Record exact candidate sources/configuration for comparison with the final commit/build.
2. Review the full diff, including generated content, native assets and dependency changes. Preserve the pre-existing Firebase repository-URI fix. Verify the evidence inventory and run affected final checks, then commit the authorized final work. Record the exact commit, engine/package versions and accepted configuration.
3. Build the intended Android artifact from that commit using the agreed package/version and owned signing key. Use a version code compatible with an update. Do not use `Build Android Test APK` to replace the installed release-signed game: that menu selects debug signing. Inspect actual application ID, version, signing certificate, content manifest, included assemblies and build errors before installation. Compare the certificate with [FirebaseSDK.md](../FirebaseSDK.md) and the actual installed package. Keep secrets outside Git/logs.
4. Retain the final build report, artifact digest and prior installed artifact identity. If signing, versioning, acceptance or installation permission fails, stop the replacement; do not uninstall, clear application data, change package identity or bypass the phone restriction. Use a normal same-package update after those checks pass.
5. Launch the updated game and verify expected progress/transition, normal purchase/rebuild, background/resume and pending premium recovery. Keep the final artifact and device evidence alongside its commit. If behavior is unsafe, stop progression/purchases through the supported recovery path and follow the recovery procedure below.

The existing core diagnostic package `com.diwali.rocketrajaredesign.qa` is separate and cannot stand in for this final artifact. It was installed and tested after a phone-authorized retry; [DeviceCore/README.md](DeviceCore/README.md) records its results and the unchanged original APK hash. The later full native playtest is another separate package, `com.diwali.rocketrajaredesign.playtest`; its installation is currently rejected by the phone and awaits the local prompt. No uninstallation or data clearing is authorized by this plan.

## Rollback procedure

- Before any migration is enabled, preserve immutable source archives and a compatible previous artifact. Capture configuration and ledger schema revisions without exporting credentials or purchase tokens into ordinary logs.
- Prefer a forward repair that understands the current gameplay/wallet schema. An old app cannot safely interpret schema 3 or restore a paid balance from schema 2. Do not force an Android version downgrade or reinstall if it would require data loss.
- If a compatible prior client is used, preserve all newer files and namespaces. Its old progress must never replace newer `redesignUser` progress on a subsequent upgrade. Disable unsupported premium actions rather than supplying a local fallback.
- Restore gameplay only through validated backups/checkpoints and the normal ownership protocol. Reconcile the authoritative wallet and any pending operation before enabling paid spending. A database rollback must not erase receipts, draw operations or revocation tombstones; reconcile these records before reopening paid operations.
- For a server failure, retain pending client operations and original ledger data. Restore a compatible service/rules version, verify the same operation replay and UID fencing in a sandbox, and only then resume. Prolonged refund-notification outages require the operational reconciliation listed in the hosting review.

No reverse migration, blanket account reset or destructive rollback is implemented. A recovery requiring a new mapping, paid compensation or modified retention policy needs a concrete reviewed plan.

## Development-only controls

`RedesignBuildValidation` now applies the isolated-build restriction through Unity's common pre-build callback, including normal Build Profiles and `BuildPipeline` entry points. Current non-development builds fail before content/build work; explicit development diagnostics remain available. This closes the custom-menu bypass. [Unity's callback contract](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Build.IPreprocessBuildWithReport.html) and [scripting symbols](https://docs.unity3d.com/6000.4/Documentation/Manual/scripting-symbol-reference.html) support these entry points.

Fixture reset code compiles only for the Editor, development players or the standalone .NET lab. `RedesignBuildSafety.cs` makes injecting `REDESIGN_LAB` into a non-development Unity player a compiler error. Fake receipt services and seeded probes live outside runtime Assets; the actual runtime defaults to secure draw selection and requires configured authenticated authority for paid fulfillment. SmokeHarness code is Editor/development-only. Final inspection of the real Unity/IL2CPP artifact is still required; a symbol test is not proof of that artifact.

Evidence: [four Unity build-mode checks](Unity-Release-Isolation.txt) and [four compiled symbol/type checks](Release-Symbols.txt). Reproduce using `Rocket Raja / Redesign / Validate Release Isolation` and `node Tools/RedesignLab/CheckReleaseSymbols.cjs`. The latter compiles actual core sources into an ignored temporary .NET project, checks the fixture-reset type is absent from ordinary player symbols, verifies the injected-symbol failure, and retains permitted development/lab behavior. It makes no APK and modifies no build setting or installed app.

## Draft release notes and outstanding limitations

The redesign brings 1,000 cash upgrades, Fame and permanent Talent cards, a separate KK wallet, manual rebuilding, and ad-free offline income. Main now connects these systems to 13 saved native screens, retained utilities, product/shop progression, compact feedback and optional guidance. The local development shop simulates pack outcomes without charging money.

Completed preparation includes generated-content drift checks, durable local/cloud protocols, premium authority implementation, ad retirement, model comparisons, saved native screens and integration regressions. UI approval pauses are waived. Remaining prototype work includes native phone installation/acceptance, repeated-run hardware measurements, simulated purchase recovery, uncoached playtests, final artifact inspection and acceptance. Actual Play configuration/localized prices, hosted authenticated premium services, live account/two-device acceptance, public privacy/support/retention details and any real-player conversion remain explicitly unverified; those limits must accompany delivery. Final replacement version/code, commit and APK hash are unassigned until acceptance. There is no release date promise or store approval claim.
