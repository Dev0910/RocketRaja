# Repeated cloud-save pause investigation

The connected Android build repeatedly displayed `Cloud progress changed during saving.` under the generic `Waiting for a connection` heading. The device's pending receipt had base revision 22; Firestore contained revision 23 with the same commit ID and checksum. The upload had reached the server, although the immediate confirmation path had rejected its result.

`FirestoreSaveStore.Change` confirmed transactions using `DocumentReference.GetSnapshotAsync(Source.Server)`. That API uses the SDK's listener path. A previously delivered server snapshot can be reused before a live listener catches up with a committed transaction; `IsFromCache == false` does not establish that a snapshot includes the just-completed write. This matches the reported [Firebase Android SDK issue #6974](https://github.com/firebase/firebase-android-sdk/issues/6974), although the exact timing has not been reproduced in the desktop emulator.

`ReadAsync` now reads within a read-only Firestore transaction, so acquisition, recovery and post-write confirmation use a backend transaction snapshot. Receipt, checksum, revision, ownership, offline and timeout checks remain enforced. The UI heading is now `Your Google game is paused`, leaving the actual reason visible below it.

## Repeatable validation

- `dotnet run --project Tools/CloudSaveLab/CloudSaveLab.csproj`: 26 protocol checks (handoff, stale writes, timeouts, interrupted upload recovery and mutation gates).
- `CloudAcknowledgementValidation.RunOnce(uniqueRequest)`: Firebase Unity SDK test using a separately named app with project `demo-rocket-ack`, Firestore emulator at `127.0.0.1:8085`, SSL and persistence disabled. Requires an isolated emulator with permissive fixture rules; never deploy those fixture rules. Uses no real account or default Firebase app.
- SDK test compares 100 original server reads and transactional reads with an active listener, then exercises 200 account uploads with in-flight follow-ups and checks the complete final state and acknowledged local checkpoint. See `AcknowledgementRegression.txt` for actual results, including whether the original read reproduced the race on this host.
- Android update uses the configured release signing key and preserves the installed app's data. Build result is recorded in `AckFixAndroidBuild.txt`.

## Results — 29 September 2026

All 26 protocol checks, 100 transactional confirmation reads and 200 SDK account uploads passed. The original server-read race occurred 0/100 times on the desktop emulator; device evidence and the SDK issue motivated the changed read path, rather than claiming a desktop reproduction.

Release APK `Builds/Android/RocketRaja-cloud-save-fix.apk` built with 0 errors and 24 warnings in 4m25s. Verified its SHA-1 matches the previously installed release certificate. `adb install -r` succeeded without clearing data. Launched the app on POCO X2 at approximately 21:16:55 IST and observed gameplay and confirmed saves through 21:19:45. Checkpoints advanced from base revision 66 (acknowledged) through 102 (in flight) to 146 (acknowledged), with no recurring pause during this observation. Final device screenshot `AckFixPhone.png` shows Settings reporting `Saved to Google`. See `AckFixDeviceChecks.txt` for sampled receipts. Existing account files were copied to `Temp/AckFixDeviceBackup` before installation; no account or guest saves were deleted.
