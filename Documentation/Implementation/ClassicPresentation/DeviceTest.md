# Classic presentation candidate — physical device

6 October 2026. POCO X2, package `com.diwali.fireworkstycoon.dev`. Updates retain the existing selected Classic profile and the preserved legacy files; no data clear is part of this test.

The native Editor evidence is [Validation.txt](Validation.txt): 82 passing assertions on the saved production hierarchy, including two unboosted simulated-time runs and retained-progress reload. Actual device observations are recorded below after the candidate installs. Editor audio-source state is not proof of audible speaker output, and injected Editor pointer events are not proof of physical touch behavior.

The candidate remains a development APK. No Google Play test build is available, so it does not establish store-installed acceptance or a final non-development release. Live Google and billing configuration remain disabled/unavailable as documented in the production integration report.

## Installed candidate

- [Build record](Android-Build.txt): succeeded, zero errors, 30 warnings, 2m 37.8s.
- Artifact: `Builds/Android/RocketRaja-classic-connected.apk`, 99,848,724 bytes.
- SHA-256: `00A690498B9A88BEE206A7291FB29281AB6DAB835B62C3DF976D9FC9C8A85B29`.
- APK v2 signature verified with one signer.
- `adb install -r` returned Success; package data was retained.
- Initial activity start occurred while Android reported `mWakefulness=Asleep`, display OFF. Its black screenshot is not evidence of a game rendering failure or successful visible launch. Audible playback and physical touch checks were requested after unlocking.
- [Saved hierarchy audit](Saved-Hierarchy.txt): 2,544 Classic objects, zero missing scripts, one active audio owner, two saved audio sources, one listener, 64 fixed particles, 14 guidance rows, five decoration layers and no Editor Play override.

## Physical checks and cold restart

- The user confirmed **“Audio and touch checks work”** in response to the requested music/sound ON/OFF, counter tap/purchase sound, counter drag and Reduced motion checks. This is user-observed device evidence; it is not an automated measurement of speaker output or touch latency.
- [Visible installed game](AndroidVisible.png) shows Classic running with retained progress and the connected product appearances.
- Backgrounded through Android Home, then force-stopped and relaunched the same package without clearing data. `am start -W` returned `Status: ok`, `LaunchState: COLD`, `TotalTime: 664`, `WaitTime: 670` (milliseconds). These are Android activity launch measurements, not a measurement of full game readiness.
- [After cold restart](AndroidRestart.png): Classic Home with all eight shelf products, advanced appearances, Shop 1/6, cash ₹1.57M, automatic sales ₹27.46K/s, Phuljhari level 52 and Anar level 50. Existing progress loaded; no new-player or recovery error screen was visible. The screenshot alone does not prove exact offline settlement arithmetic; that is covered by the native regression.
- Bounded `Unity`/`AndroidRuntime` log inspection for the restarted process (PID 9210) found no `Exception`, `FATAL`, `Error` or `Failed` matches.

REL-06–11 integration acceptance is recorded against this development candidate, combining the exact content checks, unboosted simulated-time journeys, saved-scene presentation tests and these physical observations. Final non-development/store acceptance, wider device coverage, uncoached balance sessions and live services remain open in the release checklist.
