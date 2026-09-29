# Cloud-save validation — 2026-09-28

The cloud backend and live Firebase configuration are implemented. The artwork and HTML are approved, and Bootstrap is now reconstructed as native uGUI with Settings account controls. See ../BootstrapValidation/README.md for the new integration checks. This report does not certify real-device Google sign-in; that check was deferred by the user.

| Check | Result | Evidence |
| --- | --- | --- |
| Existing economy suite | Passed | Economy.txt |
| Cloud protocol suite | 26 passed | CloudProtocol.txt; Tools/CloudSaveLab |
| Firestore emulator security rules | 12 passed, zero failed | RulesEmulator.txt; Tools/CloudSaveTests/rules.test.cjs |
| Unity cloud/guest codecs | 7 passed | UnityCodecs.txt |
| Existing Unity save codec | Passed | ../UnitySaveTests.txt |
| Gameplay/UI smoke regression | 143 passed, SMOKE SUCCEEDED | Smoke/SmokeResults.txt |
| Android native backend build | Passed, zero errors; 15 warnings | AndroidBuild.txt; AndroidBridge.txt; AndroidManifest.txt |
| Final Unity editor state | Compilation succeeded; no build or play session running | Checked after final asset refresh |
| Live snapshot index exemption | Deployed for user.save; all automatic indexes disabled | FirestoreIndexExemption.png |
| Live Firebase configuration | Google enabled; Mumbai database reused; published rules match tested source; debug SHA-1/SHA-256 registered | GoogleProviderEnabled.png; FirestoreRulesPublished.png; AndroidOAuthClient.png |
| Local Android OAuth configuration | Actual console client IDs added to existing config; generated default_web_client_id matches | OAuthClients.json; OAuthResourceCheck.txt; ../CloudSaves.md |
| Packaged Android OAuth and signing | APK contains the registered Web client ID; signing SHA-1/SHA-256 match Firebase registrations | APKOAuthResource.txt; APKSigningCertificate.txt |
| Google OAuth audience | Existing status is External / In production; no audience change made | OAuthAudience.png |
| Two Android devices | Deferred by user on 2026-09-28: “Complete everything testable here; device testing later” | ../CloudSaves.md |
| Generated artwork / native startup screen | Artwork and HTML approved; native Bootstrap scene authored and first in Build Settings | ../../UI Builder/Reference Images/Bootstrap/README.md |

Core tests exercise serialization, large values, immutable in-flight saves, pending follow-up snapshots, lost acknowledgments, reconnect, lease handoff/expiry, stale-owner writes, all economy mutation guards, offline payouts and ad-time exclusion. Regression cases verify passive earnings during handoff, upload timeouts, late acknowledgments remaining paused, and replay of valid interrupted local checkpoints after restart. Rule tests run exclusively against the demo-rocket-raja emulator project; no production user document was created or deleted.

The current integration build includes Bootstrap and Main. A successful build proves packaging, not live Google login or two-device acceptance.
