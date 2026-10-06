# Save transition contract

Updated 6 October 2026. No deployed progress conversion is enabled or authorized by this policy. The current [Classic retention, locations and recovery policy](Classic-Save-Account-Integration.md) records the user's approved separate fresh-profile choice, stable account checkpoint location, stricter interrupted/future-file protection and incomplete remote entitlement inventory. The fixture conversion below remains development-only.

## Inventory

Legacy gameplay uses schema 1, economy 1.0.0, eight named product levels, 96 upgrade IDs, 12 ordered five-rank Legacy perks, integer reputation/prestiges, settings/name/onboarding, three timed skills, sponsor expiry, offline reward ID/amount, and a bounded ad-receipt list. `SaveStore` wraps JSON with SHA256 and rotates `.bak`; corrupt input is preserved as `.damaged`. Guest `diwali-v1.save` lives beside separate hashed-UID account checkpoints. A checkpoint includes an immutable upload, optional follow-up, UID, owner, generation, base revision and commit ID; lost acknowledgments are reconciled by identity rather than replaying gameplay. Guest/account replacement makes recovery copies.

No IAP entitlement or authoritative premium ledger is implemented in the baseline. This is a code observation, not proof that all deployed players have no external entitlement. Never infer payment ownership from old ad receipts or from reputation. The connected installed app and local progress must be treated as real until separately identified as disposable.

Sanitized fixtures under `Fixtures/Legacy/` cover a guest, backup, confirmed checkpoint, pending upload and follow-up. All player names, UIDs, session IDs and receipts in fixtures are synthetic. The inventory report contains counts/versions only; original player JSON is not committed.

## Accepted versions and exact policy

| Input | Action |
|---|---|
| Missing files in disposable v2 namespace | Create new schema-2 run, zero Fame/cards/KK/U/prestiges, manual first-product grant |
| Valid schema-2 / economy-2.0.0 / accepted content | Validate all fields, then load |
| Valid schema-3 / economy-2.0.0 / accepted content | Validate authoritative UID/profile and wallet component; account load additionally requires matching UID |
| Valid schema-1 / economy-1.0.0 | Return explicit legacy-recovery/transition-required result, preserve bytes and stop mutation; never treat it as corrupt or silently reset |
| Missing version, malformed values, invalid checksum | Try compatible valid backup; otherwise preserve originals and require recovery |
| Future schema/economy/content | Reject without writing or falling back to older progress; requires newer app |
| Explicit disposable fixture conversion | Back up bytes before creating a separate v2 file; copy music/sound/reduced-motion, normalized name and prompt-handled flag only; zero all old economy/progression and expire all sponsor/ad state |

No legacy upgrade ID maps to a new ID. Reputation, Legacy ranks, old prestiges, old earnings, product/tap levels, skills, ad receipts and onboarding steps do not map to Fame/cards/KK/U or taught v2 topics. This is only the synthetic-fixture reset policy. A real-player compensation/reset proposal requires a concrete reviewed mapping before rollout; existing saves are blocked/preserved until then. Old settings/name can be retained after that decision. External entitlements always require verified ledger recovery, never a reset.

Schema-2/3 saves use `rocket-raja-v2.save`, separate from `diwali-v1.save`. Schema 2 is local-only; schema 3 activates the authoritative premium component and prevents an older schema-2 reader from spending a cached paid balance locally. Empty inline objects produced by Unity JsonUtility do not imply an active wallet. Checkpoint paths/content versions separate economies. Envelope schema stays independently versioned; unsupported future envelopes are never acknowledged. Validate an entire snapshot before replacing anything. Write and flush temporary data, retain a validated backup, atomically replace; failed commit restores prior in-memory state. Keep one-time rewards, debit, selected draw and pending reveal together.

Reject schema-3→2 overwrites, changed wallet UID/profile and decreasing wallet revision even when gameplay revision is higher. Before a paid request leaves the device, persist and confirm its operation UUID in cloud progress. Recovery can replay that identity on another owned device; server debit/card outcome is immutable. Restore the authoritative component by replacement, retaining local free copies. Guest promotion imports its one-time gift or original spent draw once; an ambiguous old gift history requires recovery instead of a replacement grant.

## Rollback and recovery

The old app sees its untouched schema-1 path. It cannot load or overwrite schema-2 data. Its progress cannot later overwrite newer v2 progress; no reverse migration. Restoring a stale v2 backup must reconcile paid balance/draw ledger before enabling premium mutations. Schema migration has an immutable source digest and pre-migration archive; interruption must yield either source or complete destination, never a partly converted file. Device install updates preserve application data and signing. Rollout remains gated until reviewed real-player transition and premium recovery are available.
