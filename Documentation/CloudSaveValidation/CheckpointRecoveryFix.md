# Invalid account checkpoint on restart

The connected POCO X2 retained a valid pending checkpoint at base revision 232. Its optional `followup` was an inline object with schema 1, empty JSON and empty checksum. Unity's `JsonUtility` was confirmed to produce this representation when serializing and deserializing a null inline class.

Recovery previously checked only `previous.followup != null`, constructed a second checkpoint using the empty payload, and failed its normal payload validation with `Invalid account checkpoint.`. The existing .NET protocol tests used System.Text.Json, which preserves null; the Unity codec test covered only a populated follow-up. The earlier live-save verification did not cover this restart condition.

`CheckpointStore.Load` now normalizes only the empty optional placeholder to null after verifying the envelope checksum and primary receipt. Supported placeholder schemas are 0 and 1. Populated follow-ups, including damaged ones, remain untouched; valid queued progress still replays and invalid populated progress is not uploaded. No primary checkpoint validation was weakened. Existing schema-1 files are recovered without manual edits or deletion.

## Validation

- Reproduced using the copied device file and Unity JsonUtility: main valid=true, follow-up nonnull=true/valid=false, synthesized recovery checkpoint valid=false, null round-trip becomes object=true.
- 12 Unity codec checks pass, including absent follow-ups in pending and acknowledged checkpoints, populated follow-up preservation, corruption preservation and guest recovery.
- 28 protocol checks pass, including restart with the exact empty placeholder before upload and after server acceptance. Both recover the pending mutation exactly once.
- Original device account files retained under `Temp/CheckpointDiagnosis/accounts` before updating the app.
## Android build and installation

- Signed release APK: `Builds/Android/RocketRaja-checkpoint-recovery-fix.apk`; installed successfully over the existing app with `adb install -r`, preserving its data.
- Signing certificate matches the previous installed release.
- Unity reports Succeeded (2m33s), with 16 warnings and 8 errors consisting exclusively of failed writes to four generated Firebase `.meta` files. Gradle completed successfully; no C# or Gradle build failure occurred. The metadata files exist and are not read-only when inspected after the build. These editor messages remain a build-environment caveat.
- Live recovery verification initially blocked by the device being asleep/locked; the app received pause/stop immediately after launch. User asked to unlock/open the game. Do not count this launch as a recovery pass.
