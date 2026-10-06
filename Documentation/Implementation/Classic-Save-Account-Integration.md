# Classic saves and Google recovery — REL-12–16

6 October 2026. The user confirmed `com.diwali.fireworkstycoon.dev` and its existing signing identity remain the target. Only the POCO X2 is available. This report separates implemented local protection from live Google and two-device acceptance.

## Profile and upgrade policy

`P` below is Unity's existing `Application.persistentDataPath` for the same application/signing identity. Updating the APK must preserve this directory and PlayerPrefs. Do not clear application data or change signing to work around errors.

| Data | Selected location and behavior |
|---|---|
| Existing legacy guest | `P/diwali-v1.save`, backup and archives remain untouched. No gameplay fields convert. |
| Existing/default Classic guest | `P/rocket-raja-v2.save`; load validated schema 2/3, economy 2.0.0 and accepted content. |
| Explicit separate Classic guest | `P/classic-v2/rocket-raja-v2.save`, selected by the durable `rocket-raja-v2.SeparateGuest` preference after Bootstrap confirmation. This retains the user's earlier approved preservation choice. It is a normal fresh game, not a converted or boosted save. |
| Classic Google checkpoint | `P/economy-2/accounts/SHA256(uid)/checkpoint.save`, independent of the selected guest. The immutable pending upload and follow-up remain in the same envelope. |
| Earlier guest-relative Google checkpoint | Preserve in place and block entry before any cloud acquire/create/write. No automatic relocation or omission of pending progress. A reviewed recovery is required. |
| Legacy Google checkpoint/cloud document | Original `P/accounts/...` and Firestore `user/{uid}` are retained. Classic uses `redesignUser/{uid}` only; no copy or conversion from `user`. |
| Preview/simulation/converted fixtures | Their directories are never selected as production sources. Development provenance, simulated-shop history and fixture-conversion digests remain ineligible for account promotion. Backup recovery also blocks promotion and new draws until reconciled. |

The retained mapping is deliberately **no legacy conversion**. Legacy cash, products, reputation, perks, skills, ad receipts and offline offers do not become Classic cash, Fame, cards, KK or learned topics. Name/preferences remain in the source; the explicit separate profile starts normally. No real-player compensation or entitlement conversion has been approved or enabled. This policy must not be mistaken for permission to discard old files or grant replacement currency.

Supported main files, backups and interrupted temporary files are inspected before writing. Any checksum-valid future schema/economy/content candidate blocks fallback and overwrite, including future gameplay in account pending/follow-up payloads. A temporary file alone is recovery evidence, not a fresh-profile signal. Only the same live writer may retry its own failed first write; a restarted process cannot assume the temporary data is disposable. Interrupted bytes are archived by digest before reusing `.tmp`. A corrupt main never rotates over a validated backup. Unrecoverable checkpoints block initialization rather than silently loading a new cloud snapshot over them.

Schema 3 remains bound to its authoritative wallet UID/profile/revision. Local rollback cannot downgrade that wallet. One-time grants, debits, selected cards and pending reveals commit together. Backup recovery retains a draw-reconciliation fence and Classic explains why Talent draws are paused. Account recovery uses the same UID/owner/generation and immutable commit identity; Retry cannot take over another device.

## Inventory and entitlement boundary

[Read-only desktop inventory](ClassicRecovery/Local-Inventory.txt) found two legacy schema-1 files and four current schema-2 files, including backups/archives. These are file counts, not unique players. No schema-3 wallet was observed in that scoped scan. Names, UIDs, balances, credentials and raw saves are excluded from the report.

Prior phone work established an older save plus the explicitly selected separate Classic profile. Android restricted direct access to protected storage; no permission bypass or data clearing was used. The installed game visibly retained current progress. Remote player documents and paid entitlement records have **not** been inventoried. Their absence cannot be inferred from local files or the unfinished billing client. REL-12 remains open for the complete deployed-player/entitlement inventory and any later conversion review.

## Local and fixture evidence

- [Core checks](ClassicRecovery/Core.txt): 702 passing, including 47 additional durability/location checks. Covers orphan first writes, interrupted replacement, future main/backup/temp/checkpoint payloads, stale/wrong-account protection, exact retained bytes, clock rollback/forward cap, storage-failure rollback and account-location conflict before remote mutation.
- [Cloud protocol](ClassicRecovery/Cloud-Protocol.txt): 28 passing. Includes lost acknowledgments, pending/follow-up uploads, late callbacks, ownership fences and Unity's exact empty-inline-follow-up representation.
- [Draw persistence](ClassicRecovery/Draws.txt): 66 passing atomic bundle/persistence checks.
- [Firestore emulator](ClassicRecovery/Firestore-Rules.txt): 22 passing, zero failures, against `demo-rocket-raja`. This is not deployment or live access evidence.
- [Classic saved-scene durability](ClassicRecovery/Native.txt): 26 passing assertions, zero runtime errors. Exercises actual recovery/Retry screens, real file-lock replacement failures, autosave, ENOSPC injection, offline cap/clock rollback, pending reveal restoration/Continue, future files, backup recovery and interrupted first-write recovery. Screenshots are beside the report. First prestige/free draw is earned through accelerated engine time; no cash or reward grant is injected.
- [Startup/account fixture regression](ClassicRecovery/Startup-Account.txt): 79 passing assertions after the final source changes, including selected account binding, blocked offline retry, reconnect, background/resume, sign-out, retained guest and stale-engine fencing. The updated APK/device record is [candidate acceptance](ClassicRecovery/DeviceTest.md).

The storage-full case injects ENOSPC at the persistence boundary. We did not fill the user's phone or claim an operating-system low-storage test. Destructive/corrupt-file tests use disposable Editor directories; no real progress is damaged to test recovery.

The updated signed development APK built with zero errors and installed successfully over the existing package. The phone is asleep; visible retained-progress restart acceptance remains pending, so REL-13 stays open despite passing local/native tests. Exact candidate identity and device observations are recorded above.

## Live configuration and remaining acceptance

[Local configuration checks](ClassicRecovery/Configuration.txt) confirm generated project/Web-client values match the source config. Local `google-services.json` identifies `rocket-raja-4729c`, the confirmed Android package and a Web OAuth client. The installed presentation APK's signature matches the historically registered release certificate: SHA-1 `BB:42:49:74:4A:4F:FA:4C:2A:C0:81:CA:53:B9:E4:EC:B1:AE:FC:28`, SHA-256 `5B:13:3F:14:69:43:59:7B:93:68:87:44:CE:C6:49:13:0D:B2:59:9E:C0:9A:99:BE:07:F5:8D:A9:0E:80:32:86`. SDK 13.17.0 is already imported; no SDK/config replacement is needed merely to activate accounts.

Current access check: Firebase CLI reports no authorized account. The signed-in Firebase Console lists Rocket Raja, but opening it reports that the project does not exist or the current account lacks permission to list its apps. Console access was requested from the user. Therefore deployed rules, current OAuth/provider settings and remote records cannot yet be verified. `RedesignCloudSettings.enabledAfterReview` stays false. No rules, indexes, auth registrations, cloud documents or credentials were changed.

Once access is restored, compare deployed rules/indexes to the prepared local files before publication; preserve concurrent owner changes and the existing `user` collection. Verify Google provider, exact package/certificate and generated Web client, then enable the reviewed configuration and rebuild with the same signing identity. Real sign-in/cancellation, explicit promotion, existing-save selection, switching/sign-out/reconnect and network/expired-authentication recovery must then run through Bootstrap + Classic on hardware. Play App Signing must be checked separately when a test track exists.

REL-14 and REL-15 remain open for those live checks. REL-16 remains open because only one physical device is available; fixture/emulator ownership tests do not establish two-device acceptance. A second device must verify handoff, concurrent ownership, interrupted upload and stale-device rejection without clearing either installation.
