# HTML UI state completion

The missing custom presentation states from UI-COVERAGE-AUDIT.md are now implemented in the shared HTML preview. Existing screen structure and the accepted Ivory Bazaar controls are preserved.

Open `http://127.0.0.1:4310/state-gallery.html`. Each card opens an explicit fixture; the review controls can switch the same fixture between screens and replay feedback.

| Audit gap | Added HTML coverage |
| --- | --- |
| Save failure and recovered backup | Persistent review notices with alert/status semantics; normal runtime-style messages auto-dismiss |
| Sponsor active and expiry | Active countdown, disabled sponsor entry, normal income restored and entry enabled at expiry |
| Milestone branches | First shelf, three stocked families, six stocked families, nearest display milestone, all displays complete / prestige |
| First run and post-prestige | First-stock guidance, reset run values, retained reputation, Legacy ranks and lifetime statistics |
| Gameplay feedback | Tap income burst, tap purchase, unlockable purchase, buy-all result, permanent perk, display upgrade, skill activation, reward and prestige notices; short celebration particles |
| Shop and product visuals | Starter, established, showroom and flagship shop appearances; product displays at levels 1, 25, 100 and 250 |
| Ambient motion | Optional shopkeeper idle and visitor silhouettes; pauses behind modal and respects reduced motion |
| Documentation drift | Player next-ten behavior and Products Profit / compact controls recorded in screen-specs.json |

The shared fixture layer is `HTML/Shared/coverage-states.js` and its CSS. No Unity files, saves, ad callbacks or economy were changed. Success feedback is an explicit visual fixture, not a claim that an operation was performed. Google consent, provider video and external privacy pages remain provider-owned, as identified in the audit.

Validation: 22 new fixture checks, sponsor expiry interaction, progression artwork in Product Detail, retained Legacy, tap feedback without wallet mutation, reduced motion, and existing 79 viewport/state plus 17 interaction checks. Requested layout/removal regressions and the motion and Ivory Bazaar suites also passed. Captures and results are under `HTML/Shared/renders/coverage/`.

Artwork was generated using the built-in image generation tool; prompts and original output paths are recorded in `HTML/Home/assets/progression/PROVENANCE.md`. A separate visual review checked shelf geometry, baked text and atlas spacing. The retained original level-one atlas and showroom scene remain available.

Native runtime comparison remains source-backed; this completion does not claim a live Unity Game View or production ad/save test.
