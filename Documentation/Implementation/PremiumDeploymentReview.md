# Premium server deployment review

Prepared 1 October 2026. **Undeployed and unconfigured.** The existing project and phone have not been changed. This is the reviewable hosting implementation for RR-09-04; it is not store or release acceptance.

## Proposed service

`Backend/Premium/index.mjs` exports Firebase second-generation HTTPS function `premiumApi` and private Pub/Sub function `premiumPlayNotifications`, on Node 22. Required parameters select region, dedicated runtime service account, Android package, Firebase Android app IDs, four store product IDs and the Play notifications topic. Configuration is validated before initialization, and grant amounts are fixed at 1,000/6,000/13,000/28,000 KK in ascending pack order.

The service accepts POST `/activate`, `/wallet`, `/fulfill`, `/draw`. Every route verifies a non-revoked Firebase Google ID token and an allowlisted App Check token. UID comes from the verified token; a request body cannot supply UID or reward amounts. New paid draws require the active cloud ownership lease and the exact pending operation already stored in cloud progress. Store receipts are verified through the Google Play Developer API using server application-default credentials.

Proposed resource settings: 256 MiB memory, 30-second request timeout, 0 minimum / 2 maximum instances, concurrency 40. These limit prototype resources but are not a guarantee against charges. A shared Firestore limiter allows per account per minute: wallet 60, activation 10, fulfillment 20, draw 60. Limits persist across restarts; storage failure blocks requests. Fixed windows may permit twice the per-minute limit across a window boundary. Clients receive a retriable 429; no wallet mutation occurs. Hosting/throttle defaults can be adjusted during configuration review without changing the economy.

Refund notification processing uses 256 MiB, a 300-second timeout, 0 minimum / 1 maximum instance and concurrency 1, with retry enabled. Only the configured IAM-protected topic invokes it; there is no client refund route. A full one-time-product refund is independently checked against Google's Voided Purchases API before revoking the original grant. A pre-delivery verified revocation stores a token digest so a racing stale purchase verification cannot grant later. Duplicate delivery changes the wallet only once. Configure Google Play publisher permissions and operational alerting before enabling this worker.

`firebase.premium.review.json` is separate from the current Firestore configuration. It declares only the premium codebase and runs synthetic server tests before deployment. Existing commands using `firebase.json` will not publish this function. Source import/discovery makes no Firebase or Play calls. Dependencies are pinned; the latest audit reports zero vulnerabilities.

## Required configuration to finish

| Parameter / setup | Required value or verification |
|---|---|
| Deployment project | Explicit sandbox Firebase project; do not infer a disposable target from the existing production alias |
| `PREMIUM_REGION` | Selected function region |
| `PREMIUM_SERVICE_ACCOUNT` | Dedicated runtime service account with required Firestore/Auth and Play API access; use platform credentials, never a private key in Unity |
| `PREMIUM_ANDROID_PACKAGE` | Actual Play application package matching the tested build |
| `PREMIUM_FIREBASE_APP_IDS_JSON` | JSON array of Firebase Android app IDs registered for App Check |
| `PREMIUM_PACK_IDS_JSON` | JSON array of four real consumable IDs in ascending pack-size order; verify INR price points and test availability |
| `PREMIUM_PLAY_NOTIFICATIONS_TOPIC` | Dedicated Play real-time notifications topic, configured in Play Console; restrict publisher IAM to the intended Google service and administrators |
| Firebase/Play setup | Play App Signing certificate, Google provider, Play Integrity registration/provider initialization, licensed testers and test-track build |
| Firestore rules | Explicitly review/deploy protected `redesignUser` access plus denied client ledger/profile/rate access; local rules remain undeployed |
| Unity runtime | Reviewed HTTPS base URL, existing App Check provider, account/transport/purchasing binding, approved shop/UI |
| Refund acceptance | Configure/test the private notification worker, retry/dead-letter monitoring, missed-event reconciliation and support disclosure |

The implemented worker ignores pending-purchase cancellations, test notices, other packages and subscriptions. Unsupported multi-quantity refunds require review without a guessed debit; purchases already enforce quantity one. A missing API match is retried, never treated as proof. Google exposes a 30-day voided-purchase window; the prototype verifier scans at most ten pages of 1,000 entries and fails for operational reconciliation beyond that. Monitoring, dead-letter handling and recovery after prolonged outages must be configured and validated before release. No live topic or worker is deployed.

After configuration is resolved, the final action is a concrete deployment review identifying project, region, identity, mapping, rules and costs. No deployment command has run. The intended CLI scope is the `premium` functions codebase using the separate review config and an explicitly named sandbox project; production publication remains a later user-directed action.

## Acceptance after deployment

Exercise every sandbox pack, pending/deferred/canceled/invalid outcomes, wrong UID, retries before/after grant and acknowledgment, kill/relaunch, consumed-purchase restore, gift promotion, device handoff and refund/revocation. Compare revisions and immutable operation IDs. Keep tokens/credentials out of logs. Repeat affected core/rules/UI checks and record actual signing/distribution configuration. Synthetic receipts and emulator transactions do not replace these tests.

Official references: [Firebase HTTP functions](https://firebase.google.com/docs/functions/http-events), [Node runtime and resource configuration](https://firebase.google.com/docs/functions/manage-functions), [App Check backend verification](https://firebase.google.com/docs/app-check/custom-resource-backend), [Google notification types](https://developer.android.com/google/play/billing/rtdn-reference), [Voided Purchases API](https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.voidedpurchases).
