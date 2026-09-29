# Connected Android Google login fix — 29 September 2026

## Reproduced failure

The POCO X2 displayed the Google chooser, then returned to Welcome after account selection. Google Play services logged: "This android application is not registered to use OAuth2.0" and advised matching package name and SHA-1. The APK pulled from the connected phone matched the release certificate documented in `Documentation/FirebaseSDK.md`; Firebase previously contained only the debug fingerprints.

## Correction and observed result

- Added the installed release certificate's SHA-1 and SHA-256 to the existing Firebase Android app. Retained debug fingerprints and existing web OAuth client.
- The user retried login on the same installed APK. Firebase Auth emitted successful authentication and token-listener notifications.
- Verified the phone's Google account screen (`PhoneAccount.png`).
- Verified the real Firestore `user/{uid}` document: created at 20:55:15 IST, cloud revision 7 and server update/heartbeat at 20:55:42 IST. No document was manually created or edited.
- No reinstall, app-data reset, signing-key change, or save deletion was needed.

## Source improvement

`BootstrapController` now explains when Google sign-in did not finish, instead of silently returning to the unchanged welcome screen. A developer warning points to package/signature registration when account selection was completed. No tokens or account details are logged by the new warning. Unity recompiled successfully with no C# errors; this message change is for the next APK build and was not installed on the phone. The server-side login correction is already effective in the existing APK.

The Firebase console configuration download timed out, so the local configuration was preserved; its web client ID is unchanged and verified by successful device login. Two-device handoff was not retested as part of this single-device login fix.
