# Redesigned account and guest scene flow

2 October 2026. The native flow is implemented and tested with isolated identity/cloud adapters. **Live Google setup and physical-device acceptance remain pending.** This advances RR-07-07 and RR-11-04 without marking either complete.

## Connected behavior

The existing saved `Bootstrap.unity` canvas is reused. Its controller selects the redesigned branch only when the explicit cloud configuration matches the application, or during an Editor-only fixture. No account UI is created at runtime and the legacy `PlayerSessionHost` is not constructed on this route.

`RedesignSessionHost` owns identity and the selected account across scene changes. `RedesignAccountCoordinator` implements selection, new-account promotion, confirmed sign-out, guest return and explicit handoff. Main binds the chosen `RedesignAccountSession`; account options remain accessible from Settings and blocked-account recovery. A guest returns to the same independent local save, while account checkpoints stay under `economy-2`. Auth changes block the previous account. SDK callbacks are marshalled to the Unity thread.

| Trigger | Result | Durable boundary |
|---|---|---|
| Sign-in cancelled | Welcome explains the unfinished sign-in and retains the guest | No account document is created merely to display the choice |
| New account + eligible guest | Player chooses to copy device progress or begin a fresh Google game | Confirmed choice initializes the account checkpoint and rebases time from the server; source guest bytes remain unchanged |
| Existing Google save | Compare basic progress and choose Google or keep the guest | No guest/recovery overwrite of an existing Google game |
| Guest used the simulated shop | Start a separate Google game or remain as guest | The test-shop namespace cannot be promoted, including previously fulfilled test purchases |
| Settings account action | Saved Bootstrap account menu | Main saves/suspends before leaving; the old gameplay binding cannot continue |
| Sign out | Confirm pending progress, release ownership, return to guest | Checkpoint acknowledgment precedes successful sign-out |
| Connection failure | Native recovery offers Retry and Account options | Offline guest return retains pending account files without claiming upload success |
| Another owner | Explicit Play here confirmation | Request handoff, wait up to 45 seconds for acquire conflicts, validate the returned UID/generation, then initialize Main |

The existing spinner, buttons, loading/error groups, reduced-motion handling and comparison cards are retained. Runtime text bindings shorten the comparison hint, use singular/plural summaries correctly, and show accurate Settings/Privacy/recovery text when cloud is configured. Five text references were serialized through the scoped `BuildRedesignUI.BindAccountCopy` Editor operation; no layout or artwork changed. The existing Bootstrap HTML/native assets are the reused visual authority. UI approval pauses are waived by the user.

## Verified evidence

- [Core-Redesign.txt](Core-Redesign.txt): **649 checks**. Twenty-two added identity/coordinator checks cover cancellation, concurrent preparation, explicit creation, guest-byte preservation, time rebasing, checkpoint namespace, auth changes, release, forbidden replacement, fake-grant isolation and future schemas.
- [Unity-Account-Flow.txt](Unity-Account-Flow.txt): **75 checks** using actual saved Bootstrap/Main scene transitions and physical EventSystem raycasts. Covers promotion, Settings, sign-out, existing-save selection, offline guest return, switching between two fixture UIDs, explicit takeover and obsolete-engine fencing. Configured Settings/Privacy copy is checked. No runtime errors in the accepted run.
- [Unity-Account-Play.txt](Unity-Account-Play.txt): **40 recovery checks** rerun after sharing the fixture cloud across multiple UIDs. The fixture routes callbacks by UID and retains each account separately.
- [Firestore-Rules.txt](Firestore-Rules.txt): **22 localhost emulator checks**, zero failures, rerun against `demo-rocket-raja`. No live Firebase calls.
- Inspected Play Mode captures: [new-account choice](NativeUI/account-flow-promote.png), [comparison](NativeUI/account-flow-compare.png), [account menu](NativeUI/account-flow-menu.png), [handoff confirmation](NativeUI/account-flow-handoff.png). The January 1970 timestamp is the explicitly synthetic cloud clock, not a real account's metadata.

Reproduce via `dotnet run --project Tools/RedesignLab`, **Rocket Raja / Redesign / Test Account Scene Flow**, **Test Account Recovery In Play Mode**, and `Tools/CloudSaveTests/run.ps1`. The Unity suites restore the saved scene setup, keep fixture identity mode out of PlayerPrefs, and use fresh temporary profile directories. Fixture injection and fake services are Editor-only.

## Live boundary

`Assets/Resources/RedesignCloudSettings.asset` is saved with `enabledAfterReview = false`, project `rocket-raja-4729c`, package `com.diwali.fireworkstycoon.dev`. The current separate playtest package also fails the package-match gate. Firebase is initialized lazily after a configured account action, checks the expected project, and uses `FirestoreSaveStore.RedesignCollection` (`redesignUser`) explicitly. The legacy `RequireLiveCloud` guard remains unchanged. Real billing is still disconnected.

The [activation review](GoogleAccountActivation.md) describes the exact external setup needed before enabling this configuration. These fixtures do not verify Credential Manager, OAuth registration, deployed rules, real server connectivity, two physical devices, paid receipt authority or final save migration. Original player saves and the `user` collection remain untouched.
