# Cloud-save infrastructure — currently inactive

## Current status — 28 October 2026

Google login is removed from the playable game. Bootstrap automatically selects the existing local profile and loads Main; Settings and recovery expose no account actions. Local backups, version checks, offline settlement and protected recovery remain authoritative. There is no anonymous-cloud replacement or migration of account/cloud saves.

Retained infrastructure includes `IAccountAuth`, cloud contracts/stores, checkpoints, coordinators, account-session logic, premium authority code and isolated tests. Production hosts cannot create account sessions; Editor-only fixture injection remains available. Firebase SDK modules, real configuration, console settings and cloud records are unchanged.

To restore login later, introduce a reviewed authentication provider/adapter, reconnect session-host construction, review local/account save selection and purchase ownership, then reconstruct account UI through the normal review stages. Restore native login dependencies only when the selected provider requires them. Use the local SDK in `FirebaseSDK.md` if imports are necessary. Re-enabling `RedesignCloudSettings` alone no longer enables production account flow.

Current implementation, art approvals, tests and Android build: [LocalOnly/README.md](Implementation/LocalOnly/README.md).

## Historical Google-login delivery record

The sections below describe the previous integration. Their available-UI and runtime-login descriptions are superseded by the local-only status above.

## Current delivery status

The approved Bootstrap HTML is now reconstructed as editable native uGUI in `Assets/Scenes/Bootstrap.unity`, first in Build Settings before Main. All initialization, save-choice, handoff, failure and recovery controls are serialized in the scene. Settings has an account row; privacy copy describes optional Google sync. Direct Main play routes through Bootstrap unless an isolated smoke session is already prepared. See `Documentation/BootstrapValidation/README.md` for native validation and remaining device checks.

The existing default Standard Firestore database in `asia-south1` (Mumbai) is being reused. The `user.save` indexing exemption is deployed. The tested Firestore rules were published at 20:42 IST on 2026-09-28; the console's full published rule text matches `firestore.rules` after normalizing line endings. Google sign-in is enabled, and both development signing fingerprints below are registered. The existing `Assets/google-services.json` has been updated with the real Android and Web OAuth client IDs observed in Google Cloud Console; Unity's generated `default_web_client_id` matches the registered Web client. The in-app browser did not return a config-file download, so this was a verified metadata update to the existing config, **not a successful new download**. Existing project, app and API configuration was preserved, with a backup under `Temp/CloudOAuthConfigBackup-20260928`. No OAuth client secret was read or stored.

Real-device validation was explicitly deferred by the user on 2026-09-28: “Complete everything testable here; device testing later”. No Android device was attached. This is a remaining release gate, not a successful end-to-end sign-in test.

## Runtime design

`BootstrapController` owns initialization and state/choice callbacks for an editor-authored canvas. `PlayerSessionHost` owns the persistent identity/session lifetime. Firebase is constructed lazily only on the account path. `GuestSession` uses the existing checked, atomic SaveStore. Account files are separate under `accounts/<SHA256(uid)>/checkpoint.save`, with validated backup and preserved recovery copies.

`GoogleSignInBridge` calls Android Credential Manager and returns a Google ID token in memory. `FirebaseAccountAuth` exchanges it for Firebase authentication. Tokens never enter Firestore, JSON saves or logs. Android uses `default_web_client_id` from the real Firebase configuration; there is no invented client ID. The Java entry point is annotated `@Keep`, and the Unity callback has linker preservation.

All users are stored in the singular collection `user`, keyed by Firebase UID. Each document contains:

| Field | Purpose |
| --- | --- |
| profile | UID, display name, email and profile image URL |
| save | Payload schema, complete GameState JSON and SHA-256 checksum |
| revision / commitId | Conditional update version and idempotent upload receipt |
| owner / generation | Installation session identifier and ownership epoch |
| handoff | Device requesting a transfer |
| createdAt / updatedAt / heartbeatAt | Firestore server timestamps |

`save` is exempt from indexing. Normal account saving runs every five seconds; critical commands request a save immediately. Pending saves are immutable. Changes occurring during an upload are stored alongside it as a follow-up snapshot in the same atomic envelope. After an uncertain response, retry the receipt only against its original revision and ownership generation. An acknowledged upload is not applied again. Superseded data remains a local recovery copy and cannot silently overwrite the cloud.

Session leases last 30 seconds, enforced by Firestore rules with `request.time`. Every successful account save renews the lease; the five-second save interval is also the heartbeat. Gameplay gates close when connectivity is lost, when a save has not acknowledged within ten seconds, or when the last acknowledged lease is twelve seconds old. No cached snapshot or offline Firestore write enables gameplay. Writes use transactions and startup reads use Source.Server; disk persistence in Firestore is disabled in favor of the explicit account checkpoint.

A device takeover requests a handoff. The old device pauses and flushes before releasing; an unreachable owner must expire before replacement. Pending ad callbacks are tied to UID, ownership generation and run ID. After an ad, revalidate the same owner and exclude ad duration from offline earnings before accepting a reward. Account offline payouts are committed with their time anchor before gameplay starts.

The economy remains client-authored. UID authorization and version fencing protect normal clients from cross-account access and stale saves; this is not server-authoritative anti-cheat. Rules cannot validate the economic meaning of opaque JSON or establish a physical device identity from a caller-supplied session ID.

## Configuration required for real Android sign-in

Project: **rocket-raja-4729c**. Current Android application ID: **com.diwali.fireworkstycoon.dev**. Reuse Firebase Unity SDK **13.17.0** and the embedded EDM4U installation documented in FirebaseSDK.md.

1. Google is enabled using the project's existing support email and public name Rocket Raja.
2. This machine's development debug certificate is registered for the exact Android app:
   - SHA-1: `77:E7:13:20:74:02:05:13:68:DD:B3:14:3A:F3:A9:54:31:1D:21:8D`
   - SHA-256: `CA:2A:39:CA:B8:EF:D3:A3:60:71:02:D0:2C:D0:1B:07:8A:F6:E0:1A:C4:03:2D:15:24:AA:95:EE:2A:D9:00:7F`
3. The local config now includes the verified Android and Web OAuth clients (see `CloudSaveValidation/OAuthClients.json`). Unity-generated Android resources include the matching `default_web_client_id`. A normal Firebase Console download may replace the equivalent manually refreshed file later. Register upload/Play App Signing certificates separately for a production application ID.
4. Reuse the existing default Firestore database. If none exists, create Standard edition in `asia-south1` (Mumbai). Do not replace or relocate an existing database.
5. Rules and the snapshot index exemption are deployed and verified in the console. For future deployments, authenticate Firebase CLI and run `firebase deploy --only firestore --project rocket-raja-4729c` from the project root. The local CLI is available at `Tools/CloudSaveTests/node_modules/.bin/firebase.cmd`; it currently has no authorized account.
6. Bootstrap and the Settings account controls are authored and wired. Validate real Google sign-in and two-device transfer on Android before release.

The `user` collection appears when the first signed-in player document is created. Do not create placeholder or guest documents. Automatic Firebase Analytics collection is deactivated and Crashlytics collection disabled in the Android manifest; this does not change Google Mobile Ads' separate consent flow.

## Repeatable validation

```powershell
dotnet run --project Tools/EconomyLab
dotnet run --project Tools/CloudSaveLab
npm.cmd ci --prefix Tools/CloudSaveTests --no-audit --no-fund
./Tools/CloudSaveTests/run.ps1 -JavaHome '<JDK or JRE 21+ directory>'
```

The emulator runner uses **demo-rocket-raja**, never the live project. A checksum-verified Adoptium JRE 21 was downloaded into Temp/CloudTestRuntime for local testing only; no system Java installation was changed. Unity menu **Rocket Raja > Validate Cloud Save Codecs** covers Unity JSON and guest recovery. The normal UI smoke harness uses isolated temporary guest saves and waits for Main when Bootstrap is present.

Device acceptance must cover first launch and restored login, sign-in cancellation, guest upgrade and both conflict choices, two-device takeover, airplane mode during saving, process termination, token expiry, account switching, interrupted ad callbacks, unsupported/corrupt cloud payloads, background/resume, and retained offline caps. Verify real documents remain under `user/<uid>` and guests create none.

## Sources

- https://firebase.google.com/docs/auth/unity/google-signin
- https://developer.android.com/identity/sign-in/credential-manager-siwg-implementation
- https://firebase.google.com/docs/firestore/manage-data/transactions
- https://firebase.google.com/docs/firestore/security/test-rules-emulator
