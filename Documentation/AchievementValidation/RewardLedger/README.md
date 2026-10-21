# Selected Reward ledger implementation

21 October 2026. User-selected Option 1 is implemented in both the current Classic HTML preview and the saved native Achievements popup.

- Trophy header and illustrated category rows reuse one generated transparent atlas.
- Three live overview counts, progress bars, reward amounts, READY labels and Claim/Claimed states remain editable and data-bound.
- All 15 existing achievements and their atomic Kaju Katli reward behavior are retained.
- The previous native popup is preserved inactive as `Achievements__Legacy_20261021`.

Checks: [browser-checks.txt](browser-checks.txt), [native-checks.txt](native-checks.txt), [core-checks.txt](core-checks.txt), and [package-inventory.json](package-inventory.json).

Reference-sized previews: [Unity](unity-390x844.png), [HTML](html-390x844.png). Additional captures cover 360×540, 360×640 and 450×916. Native checks include simulated notch insets, interrupted transitions and actual production navigation with a temporary guest profile. Real accounts and player saves were not accessed.

READY/Claim correction: [current Unity](controls-unity-390x844.png), [current Classic HTML](controls-html-classic.png). The `controls-` captures record the notched gold plaque and rounded cobalt Claim surface, including native short/tall phone checks. Shared button frames and disabled pseudo-elements are explicitly removed from these controls. Targeted native checks confirmed all 15 state sprites, unclipped labels, separated tag/button bounds, exact +5 payout, duplicate prevention, badge removal after claiming, save-blocked disabling, close/reopen and unchanged UI object identities. See [control package inventory](controls-package-inventory.json) for resource and byte-parity verification.

Editable entry: `UI Builder/HTML/Achievements/index.html`; integrated route: `UI Builder/HTML/Classic/index.html?screen=Achievements`. UI authoring command: **Rocket Raja > Classic > Apply selected achievement reward ledger**.

Source/provenance and implementation details: `UI Builder/HTML/Achievements/ART-DIRECTION.md`. Artwork was produced with built-in ChatGPT image generation; its prompt is saved in `UI Builder/HTML/Achievements/ASSET-PROMPT.md`.
