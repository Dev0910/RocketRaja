# Privacy policy draft — owner review required before publication

App: Rocket Raja. Publisher: **[OWNER LEGAL NAME]**. Contact: **[PUBLIC SUPPORT EMAIL]**. Effective date: **[PUBLICATION DATE]**.

Draft for the intended redesigned release, updated 1 October 2026. The current isolated prototype has live Google account access and purchasing disabled; the premium service is undeployed. Verify every statement against the accepted release configuration and complete the owner, support, retention and deletion details before publication.

Guest gameplay progress, preferences and reward transaction records are stored locally on your device. Guests do not receive Firebase accounts or Firestore save documents. Clearing application data or uninstalling the game may erase guest progress.

When Google cloud saving is enabled in the released build and you choose to sign in with Google, Firebase Authentication identifies your account. The game stores your basic Google profile (account ID, display name, email and profile image URL), gameplay progress, preferences, reward receipts, save timestamps and device-session identifiers in your private Firestore account document. These records let you continue your game across devices. Account gameplay requires a connection, and only one device can play at a time. Signing out returns to a separate local guest save; it does not delete your Google account or cloud progress. The publisher must provide its account-data deletion and retention procedure before release.

The redesign has no rewarded advertisements. Google Mobile Ads and its advertising consent SDK have been removed; no ad-derived income or advertising privacy-choice control remains.

When the intended Google Play purchase service is enabled, a purchase request sends its product ID and purchase token to the game's trusted service for verification with Google Play. Authenticated requests include Firebase identity and App Check tokens. The service associates verified delivery with the player's account and retains a purchase-token digest, product/grant information, wallet totals, currency-funded card outcomes, operation identities and verification/revocation timestamps to prevent duplicate delivery and recover interrupted purchases or draws. The current ledger does not store the raw purchase token in its receipt document. Hosting, logging, security, support and retention settings must be reviewed before enabling this service. Google Play handles checkout; the game does not implement a payment-card entry form.

The intended audience is teens and adults. The publisher must verify age-related configuration and store target-audience declarations for each distribution region. This draft does not establish an age-assurance policy.

Firebase Analytics and Crashlytics SDKs are installed, but automatic Analytics collection is deactivated and Crashlytics collection disabled by the game's cloud-login configuration. These collection settings were not changed for the redesign. Google Play may provide crash, performance and testing information through its developer services. Review the processing and retention disclosures for the services actually enabled before release.

Contact the publisher at the address above about privacy concerns. The publisher must complete applicable rights, retention, contact and jurisdiction-specific information before this document is published.
