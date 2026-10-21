# Android release runbook

Updated 2 October 2026 for the ad-free Rocket Raja redesign. **Release is not accepted.** Earlier ad/Legacy instructions and calendar targets are superseded. There is no promised release date or platform approval. The current user-directed delivery is a development game with a local test shop; actual Play linking is deferred.

The operational source is [Rollout and recovery](Implementation/RolloutRecovery.md): supported versions, retained namespaces, migration evidence, entitlement-preserving rollback, development-only exclusions and the exact final commit/build/device-update sequence. The [implementation TODO](../Game%20Design/Implementation%20TODO.md) remains the acceptance checklist.

## Before the final commit and device replacement

1. Native Main now uses the redesigned core, 13 saved screens, retained utilities, progression art, feedback and contextual tips. Finish the remaining integrated regressions and device/player acceptance using the current candidate.
2. Complete the local test-shop/device recovery scope authorized by the user. Actual [store/backend configuration](Implementation/PremiumDeploymentReview.md), licensed Play sandbox purchases and real fulfillment remain explicitly deferred; the compiled adapter and synthetic server tests do not establish those results.
3. Complete device performance, sign-in/guest, interrupted save/draw, offline/resume, two-device and uncoached player checks using identified test profiles. Preserve source hashes, signing/distribution details and results. Never test destructive recovery on actual player data.
4. Resolve deployed-player transition, privacy/support/retention details and the production package/version. Keep Firebase Analytics/Crashlytics collection settings unchanged unless separately decided. AdMob/UMP have been removed; no ad configuration or ad reward tests are required.
5. Close acceptance using the tested candidate. Review and commit the full authorized diff, build the final artifact from that commit, compare it with the accepted sources/configuration, then update the connected phone without uninstalling or clearing data. Verify its installed package/signature before updating. The debug-signed Android Test menu cannot replace a differently signed installed game.

The isolated development boundary blocks all non-development player builds through the common Unity build callback. Do not remove that boundary merely to obtain an APK; the requested local prototype can be delivered as a development build. The separate core QA APK is not the full game. The [native playtest package](Implementation/DeviceGame/README.md) uses the actual saved Bootstrap/Main game and its own application data; it is still an intermediate test build before the final committed replacement.

## Before any later store publication

RR-11-05/07 require a current review of platform/account testing requirements, target API, 64-bit/page-size compatibility, Play signing, store products/localized prices, odds text, app access, data declarations, privacy URL and support/deletion behavior against the actual configuration and artifact. [Official-source review dated 2 October 2026](Implementation/PlatformReview-2026-10-02.md) records current API, native-page and disclosure requirements. Artifact/device checks and owner-specific production inputs remain open; no account eligibility is assumed. Capture real gameplay screenshots and preserve artifact inspection results.

Public backend deployment and store publication are later directed actions. The current request authorizes implementation and the final local device update. See [Premium deployment review](Implementation/PremiumDeploymentReview.md) for the concrete undeployed service and configuration inputs, and [Firebase SDK/signing notes](FirebaseSDK.md) for the installed game's signing constraints.

The [privacy draft](PrivacyPolicy-DRAFT.md) still requires owner completion and a full review against the accepted release configuration; it must not be published as-is.
