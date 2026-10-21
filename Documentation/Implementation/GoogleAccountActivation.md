# Google-save test activation

## Current activation — 21 October 2026

The user explicitly requested Firebase verification, Google login activation, a new APK and a physical-phone test. Console access now works. Google is enabled, and the live Android package, app ID, release signing fingerprints and OAuth Web client match the existing project configuration. The tested repository save rules were published to the existing default database; the only change from the previous deployed rules adds the explicit `user` / `redesignUser` collection allowlist. The `redesignUser.save` index exemption was added alongside the existing unchanged `user.save` exemption. Refreshed published rule text exactly matches `firestore.rules`.

`RedesignCloudSettings.enabledAfterReview` is now enabled through the Unity Editor for the existing package and Firebase project. This supersedes the earlier access/configuration blockers below. Build and physical-device results are recorded in [the activation report](ClassicAndroid/2026-10-21-google-login.md); live sign-in acceptance must be assessed from that report rather than inferred from the enabled flag.

## Current Classic target — 6 October 2026

The user confirmed the existing `com.diwali.fireworkstycoon.dev` package and signing identity. The separate-playtest registration proposal below is historical and is **not** the current activation plan. Use [Classic saves/account integration](Classic-Save-Account-Integration.md) for current profile locations, retained data, configuration access status and acceptance gates.

Local config matches the existing Firebase project/package and the APK certificate matches the historically registered release certificate. Firebase CLI has no authorized account; opening Rocket Raja in the currently signed-in console reports missing project or insufficient permission to list apps. Do not enable the local cloud flag until current provider/OAuth settings, deployed rules and indexes can be inspected. No new Firebase app, OAuth client, credential, live rule deployment or data conversion was performed. Actual Play signing remains unverified because no Play test build is available.

After access is restored: inspect the existing deployment, reconcile any differences, deploy only the reviewed save rules/indexes, then activate the existing app configuration and test Google sign-in through Bootstrap + Classic. Preserve `user`, use `redesignUser` for Classic, and retain all local guest/checkpoint data. Two-device acceptance remains unavailable with only the POCO X2.

## Historical separate-playtest proposal

2 October 2026. This packet is for review before changing the existing live Firebase project's configuration. No rule deployment, new Firebase app, OAuth registration or activation has been performed by this redesign work. Real Google Play/IAP linking remains deferred.

## Exact proposed changes

| Resource | Proposed change | Preserved boundary |
|---|---|---|
| Firebase project | Reuse `rocket-raja-4729c` and its existing default Firestore database | Do not create/replace a project or relocate the database |
| Firestore rules | Publish the current repository `firestore.rules` | The diff widens the existing Google-UID-scoped save rules from `user` to the explicit allowlist `user` and `redesignUser`; transaction/lease constraints are unchanged. Other collections remain denied |
| Firestore indexes | Publish `firestore.indexes.json` | Retain the existing `user.save` exemption and add the same exemption for `redesignUser.save` |
| Separate Android playtest app | Register `com.diwali.rocketrajaredesign.playtest` and its existing local debug signing fingerprints for Google authentication | Do not replace the original `com.diwali.fireworkstycoon.dev` registration or its release/debug certificates |
| Android service configuration | Obtain the real updated Firebase configuration and verify both package identities before updating the existing file | Preserve the existing project's web OAuth client and original app configuration; no sample IDs or secrets |
| Local activation | After external configuration is verified, set the reviewed playtest package/project in `RedesignCloudSettings` and enable it through the Editor | New cloud saves use `redesignUser`; legacy saves remain separate; simulator grants remain guest-only |

The current playtest debug certificate is SHA-1 `77:E7:13:20:74:02:05:13:68:DD:B3:14:3A:F3:A9:54:31:1D:21:8D` and SHA-256 `CA:2A:39:CA:B8:EF:D3:A3:60:71:02:D0:2C:D0:1B:07:8A:F6:E0:1A:C4:03:2D:15:24:AA:95:EE:2A:D9:00:7F`. Actual APK certificate evidence is in [DeviceGame/Certificate.txt](DeviceGame/Certificate.txt). The original app's release certificate is different and is documented in [FirebaseSDK.md](../FirebaseSDK.md).

The Firestore deployment command, after approval and verifying the authenticated project, is `firebase deploy --only firestore --project rocket-raja-4729c` with the repository's existing configuration. This command does not deploy premium functions, Play notifications or billing products. Inspect the currently deployed rules first to detect concurrent owner changes; reconcile those before any publication.

## Evidence ready for review

[AccountFlow.md](AccountFlow.md) links the implementation, 649 core checks, 75 native scene-flow checks, 40 native recovery checks and 22 localhost emulator checks. The rules/index diffs contain only the separate save-collection allowance and its indexing exemption. Guest simulation, production billing and old-save conversion are not part of this activation.

## Acceptance still needed

After the setup is approved and applied, rebuild the correctly configured separate APK and verify its actual package/certificate before installation. The phone currently rejects installation and is locked; the user must unlock it and accept the normal USB installer prompt. Test real Google cancellation/sign-in/restore, guest choice, background/resume, connectivity loss, force-stop/relaunch, account switching and two-device handoff. Preserve the original installed game throughout. The final same-package replacement still requires the original owned signing identity after the checklist and final commit are complete.

No existing cloud document needs deletion or conversion. If validation fails, leave the local activation disabled; do not reset accounts, weaken rules or clear device data to make a test pass.
