# Premium ownership and recovery contract

Implementation contract, 1 October 2026. Payments remain disabled. No store products or backend services have been published. Google Play is the proposed Android storefront, pending the user's existing-console configuration details.

## Ownership

Cash gameplay, offline production, prestige and free credits remain available to offline guests. Real-money purchases require linking the game to a recoverable Google/Firebase account before opening the store dialog. The shop explains this requirement at its purchase action. A device ID or local save is not a recoverable paid owner. An anonymous guest is never charged first and asked to recover an account later.

The Firebase UID owns all purchases, premium balance and currency-funded draw records. Sign-out hides that wallet; it never moves it into the guest save. A returning player signs into the same account on reinstall or another device. Account merge is not inferred from matching email/display names. A wrong-account purchase response stays pending for its original owner and is not applied to the currently displayed game.

Use SHA256(`rocket-raja:` + UID) as the Google obfuscated account ID. Verify the returned mapping on the backend. The installed APK must use the Play-configured package/signing certificate; the current development package is `com.diwali.fireworkstycoon.dev`. Its registration is not proof that Play products or license testers exist.

## Authority and storage

Firestore's existing client-authored JSON save is gameplay progress, not proof of a purchase. Trusted backend code validates Firebase identity, verifies receipts with Google and writes its own protected collections. Clients have no write access to the ledger. Amounts, product grants and card selection come from server configuration, never client-provided balances, prices or outcomes.

| Record | Durable key and contents |
|---|---|
| Receipt | Global SHA256(store + package + purchase token); immutable UID, SKU, quantity, grant, verification status and fulfillment state. Deduplication records have no bounded-list eviction. |
| Wallet | UID; arbitrary-precision decimal-string revision, gross grants, spent KK, revoked KK, gift marker, 28 currency-draw copy totals. Effective spendable balance is max(0, grants − spent − revoked). |
| Profile binding | Stable guest profile UUID; immutable owning UID. One offline gift/provenance cannot be imported into two different accounts. |
| Draw | UID + operation UUID; expected wallet revision, fixed 100-KK debit, selected card, resulting copy count and wallet revision, saved together. |
| Refund | Verified store event identity + receipt key; once-only revocation amount. Raw client refund messages are never accepted. |
| Pre-delivery revocation | Receipt digest with verified event identity, retained even if a refund arrives before its original grant; a stale verification cannot subsequently grant it. |

Currency-funded draws require an online authoritative transaction; cash gameplay and free-credit play do not become online-only. Every new draw checks the current account ownership lease, generation, wallet revision and first-prestige unlock. Its operation UUID and expected revision must already be in confirmed cloud progress, so a different device can recover an interrupted result. Server randomness selects 75/22/3, then uniformly within the tier. Transaction retries use the same sampled candidate; committed operation retries return the stored outcome without selecting again.

The implemented client cache records total levels and the authoritative currency-funded component separately; free copies are their difference. A wallet refresh **replaces** its server-owned component; it never adds a snapshot balance or replays an already-applied paid increment. Critical local cache writes precede revealing an outcome. A late result can complete its matching pending reveal while retaining a newer wallet revision. Refund deficits retain cards and block new currency spending. Ambiguous backup recovery remains draw-blocked until an owned, confirmed account snapshot resolves free-credit history too; a wallet read alone does not clear that flag.

Local-only gameplay is schema 2. The first authoritative snapshot upgrades gameplay to schema 3. Version is the authority discriminator: Unity JsonUtility expands null inline objects, so an empty serialized object must never activate paid spending. Schema 3 requires a valid UID/profile/currency component; local and account stores reject an authority downgrade, wrong UID, changed profile or lower wallet revision. Account reinitialization and every new engine binding fence earlier clients.

The second-prestige 100-KK gift is one permanent grant per owner. Account play records eligibility, confirms progress, then claims through `activate`; it never increments authoritative KK locally. A guest can use the local gift offline. Promotion imports either the unspent 100 or the spent gift's original card/operation exactly once, without another roll or another copy. The client subtracts that imported copy from its local component before adding the server component. Guest promotion cannot replace a progressed account, and a global profile binding prevents using the same profile on another UID. Offline guest gameplay/provenance is client-authored and is not cryptographic proof of honest play; this bounded free gift import never establishes a real-money pack entitlement. Ambiguous older development gift history is refused rather than invented.

## Purchase state machine

1. Connect the installed Unity Purchasing 5.4.3 store controller and fetch configured consumable products. Show the store's localized price. Missing products, unconfigured authority and offline verification disable the purchase action with a clear status.
2. Before purchase, bind UID/session generation and obfuscated account ID. Only one store dialog may be pending. Cancellation and deferred payment grant nothing.
3. On `OnPurchasePending`, send the Google purchase token and SKU to the authenticated authority. Verify package, SKU, PURCHASED state, unconsumed first fulfillment, quantity and account mapping with the Play Developer API. Never trust the receipt's local grant amount.
4. In one server transaction, create the receipt and add the pack's KK to the wallet, or return its existing fulfillment. A token already owned by another UID is rejected. Uncertain network outcomes retry the same token.
5. Persist/cache the returned authoritative revision for the matching session. Only after durable server fulfillment call `ConfirmPurchase` on the original pending order. A confirmation failure does not issue another grant; retry confirmation or let the SDK redeliver it.
6. Restore by reading the ledger and fetching still-pending store purchases. Consumed purchases are recovered from the ledger because the store does not return fulfilled consumables. No local receipt-list reset grants them again.

Refund/revocation processing verifies a store event independently and revokes the receipt's grant once. Unspent currency falls immediately. If already spent, retain the drawn card and record the deficit; new currency first offsets that deficit and paid spending is blocked while it remains. Free-credit rewards and cash progress are retained. This proposed player-facing recovery policy must be disclosed/reviewed with the release/support flow before real payments are enabled.

The undeployed private Pub/Sub worker accepts only the configured package's full one-time-product void notifications, verifies an exact token match against Google's Voided Purchases API, then applies the trusted revocation transaction. Pending-purchase cancellations do not revoke delivered currency. Verified refunds before/during grant verification block the later grant through a permanent token-digest record. Retry/dead-letter monitoring and recovery beyond the API's 30-day window still require deployment acceptance; see `PremiumDeploymentReview.md` for the bounded scan and configuration.

## Products and configuration boundary

| Proposed stable SKU | KK | Design INR price |
|---|---:|---:|
| rr.kk.small.1000 | 1,000 | 99 |
| rr.kk.festival.6000 | 6,000 | 499 |
| rr.kk.celebration.13000 | 13,000 | 999 |
| rr.kk.grand.28000 | 28,000 | 1,999 |

These are authored IDs and design prices, not verified Play Console listings. Existing IDs supplied by the owner take precedence through an explicit server/client mapping. Verify supported INR price points in the actual console; never replace localized store prices with these numbers. No automatic paid draw follows pack fulfillment.

Required external acceptance inputs: actual Play package/product IDs, registered Play signing certificate, license tester/test-track setup, a deployed authenticated verification endpoint with server-side Play API access, and a second device/account for handoff tests. Do not put credentials in chat, Unity assets or source control. Deployment/configuration is a separate concrete review step after the implementation and emulator tests are ready.

Implemented integration path: `FirebasePremiumConnection` → `PremiumHttpTransport` → `PremiumAccountClient` → `RedesignEngine`/`RedesignAccountSession`. The Firebase connection accepts only an explicitly supplied HTTPS endpoint, sends ID/App Check tokens in headers, disables redirects and checks the bound UID/session before and after asynchronous work. It does not install or initialize an App Check provider. Configure Play Integrity using the existing Firebase SDK before using it. No endpoint is currently supplied and no Main-scene purchase flow is enabled. Unknown/network/5xx responses retain pending draw identity; only definite 400/409 stale-wallet or insufficient-currency responses clear a rejected request.

## Official references checked during implementation

- Unity pending/confirmation retry and fulfilled-consumable recovery: [Purchases](https://docs.unity.com/en-us/iap/purchases).
- Remote verification architecture: [Unity receipt validation](https://docs.unity.com/en-us/iap/receipt-validation).
- Token verification, secure account attribution and backend fulfillment: [Google Play billing security](https://developer.android.com/google/play/billing/security).
- Authoritative purchase state, consumption and account fields: [ProductPurchase](https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.products) and [verification request](https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.products/get).
- Actual license tester and device scenarios: [Google Play billing testing](https://developer.android.com/google/play/billing/test).
- Custom backend attestation: [Firebase Unity App Check requests](https://firebase.google.com/docs/app-check/unity/custom-resource) and [Play Integrity provider setup](https://firebase.google.com/docs/app-check/unity/default-providers).

Installed package source was inspected under `Library/PackageCache/com.unity.purchasing@c771cfd27c96`, including `StoreController`, `PendingOrder`, order interfaces and the consumable sample. No package upgrade or SDK import was performed. This contract closes design task RR-09-01; it does not close adapter, deployed fulfillment, store metadata or sandbox acceptance tasks.
