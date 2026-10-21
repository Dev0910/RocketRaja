# Native account recovery and Retry targets

2 October 2026. Partial evidence for RR-07-07 and RR-11-04; neither task is closed by these checks. The subsequent [account scene flow](AccountFlow.md) connects Bootstrap and Main behind an explicit configuration gate, which remains disabled. No real sign-in, two-device handoff, Firebase deployment, purchase or phone acceptance is claimed.

## Account lifecycle

`RedesignApplication.InitializeAccount` can now bind an already-initialized `RedesignAccountSession` to the saved native Main owner. Normal isolated Bootstrap continues to select its separate local guest profile. Settings reports the supplied account's status, and simulated guest pack grants remain unavailable on an account binding.

The native Retry action awaits `RedesignRuntimeDriver.RetryLoadingAsync`. For an account it renews the exact existing owner/generation through `ReconnectOwnedAsync`, recovers immutable pending checkpoints, and durably settles server-timed offline income before rebinding the screens. Concurrent retries share one operation. It does not call `AcquireAsync` to take another device's lease; a takeover or pending handoff remains blocked even when that lease has expired. Disposal prevents reconnecting an abandoned account.

Backgrounding persists the current session and blocks account play until renewed. Rebinding replaces the lifecycle while retaining all saved UI objects. Old engine and screen-context callbacks remain fenced rather than invoking commands against the new binding. Connection failures display the account's reason through the existing recovery modal.

Evidence:

- [Core-Redesign.txt](Core-Redesign.txt): **649 checks**, including the seven recovery/ownership assertions and subsequent account-coordinator coverage. The fixture heartbeat enforces owner/generation/handoff exactly like the existing Firestore adapter instead of delegating to acquisition.
- [Unity-Account-Play.txt](Unity-Account-Play.txt): **40 checks** in actual Unity Play Mode, with the saved native prefab, EventSystem raycasts, an in-memory cloud and real disposable checkpoint files. Covers offline retry, delayed renewal with repeated taps, one pending command, old callbacks, offline acknowledgment, background/resume, takeover, disabled test grants and unchanged UI identities. Zero runtime errors in the accepted run.
- Reproduce with `dotnet run --project Tools/RedesignLab` and **Rocket Raja / Redesign / Test Account Recovery In Play Mode**. The latter restores the original scene setup and never initializes Firebase. Its simulated timestamps and service outcomes are not hardware or live-account evidence.

## Recovery touch-target correction

The new physical test exposed overlapping Retry and Exit buttons: `FitUtility` had converted both stretched footer targets to a fixed height and incorrectly set both vertical offsets to zero. The Exit target then received taps at Retry's visible position. Prior callback-only retry tests did not detect that overlap.

The scoped Editor revision restores the existing two-row footer from `UI Builder/HTML/SaveRecovery/saverecovery.html` and `Assets/Game/ImportedUI/Save Recovery Screen.prefab`. Retry and Exit keep their existing sprites, labels, listeners, parent and references, with separate 144-unit touch targets at y=82.5 and y=-82. The reusable sizing pass preserves each button's anchor-relative position when fixing its height, so regeneration no longer collapses the rows. No new artwork, screen, runtime layout construction or animation was added; the retained reduced-motion presentation remains unchanged.

The existing HTML package passed the project inspector with no broken resources or metadata errors. Its old economy/copy is historical reference; the redesigned Retry behavior and local-test disclosure remain as implemented. UI approval pauses are waived by the user. Scoped authoring uses `BuildRedesignUI.ReviseRecoveryTargets` through Unity Editor prefab APIs; no prefab/scene YAML was edited.

[Unity-Main-UI.txt](Unity-Main-UI.txt) now includes **13 checks**, including non-overlapping saved targets. The inspected [390×844 notched render](NativeUI/main-recovery-390x844-notched.png) and [360×540 scrolled render](NativeUI/main-recovery-bottom-360x540.png) show separate controls. Recovery content remains scrollable. The physical Retry raycast passes throughout the account test. The Main scene, its meta and the shared fallback font remained byte-identical to the pre-revision backup; only the owning prefab's two target offsets changed.

## Remaining work

The connected POCO has rejected installation and was observed locked. Device tests, human opening/rebuild observations, real Google sign-in and actual two-device acceptance remain unverified. The subsequent account selection scene flow passes 75 isolated checks; these do not replace live acceptance. Actual Google Play linking remains deferred under the user's explicit scope.
