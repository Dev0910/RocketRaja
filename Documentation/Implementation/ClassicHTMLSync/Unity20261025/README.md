# Classic HTML revisions transferred to Unity — 25 October 2026

The user requested “Apply the changes to unity”. The remaining reviewed Classic revisions are saved in `Assets/Scenes/Main.unity`, on the existing `ClassicProductionCanvas`. This native stage supersedes the earlier HTML-only choice for the Settings switch presentation. The previously completed AFK screen and existing notification rules are retained.

## Changes

- Square 48×48 Settings hit target.
- Twelve selected upgrade-effect sprites bound by `CashUpgrade.effect`, covering all 1,000 upgrades. Thirty saved Player/catalogue row instances have separate artwork, copy and purchase columns, including disabled states.
- Product-detail hit regions and purchase buttons stay in their own columns; prices, level quotes and first-stock behavior still come from the core.
- Shared 23-unit cobalt circle/i with 48-unit entry targets. Player, Products and ProductDetail open their existing topics, including after a topic has been learned. Legacy retains card-draw information. Remaining guide rows use the same glyph.
- Player/Products/ProductDetail guidance moves to compact info controls. Legacy, Shop and DrawReveal banners remain saved inactive. Achievements shortcut is label-only, with its existing red-dot logic retained.
- The user-selected Kaju Katli token is copied byte-for-byte. Balances, paid draw costs, packs, achievements, purchase artwork and prose use it. Legacy uses a dedicated saved Image beside its numeric balance; other currency-bearing text uses a shared TMP sprite asset. Only 25 text targets receive additional Editor-authored material slots. No runtime submesh/UI allocation is required.
- Standard popups retain one painted frame, clipped at the window edge with a four-unit bleed to remove the pale exterior margin. Skills and SkillDetail retain one fine gold edge. Rarity cards retain their own frames.
- Settings has three saved pill tracks, labels and thumbs. Existing `Setting` commands remain the sole state owner. Thumb motion lasts 180 ms with smooth easing; reduced motion settles immediately. Settings still persist normally.
- Help describes the existing ₹0 start, ₹1 tap, ₹30 first product and ₹10 initial tap training.

## Owners and saved layout

`ClassicRevision20261025Authoring.ApplyOctober25Revision` is the explicit Editor transfer. `RefineOctober25Revision` handles repeatable refinements. All objects, fonts, sprite references, anchors, hit regions, masks, switches and TMP slots exist before Play Mode. The original inactive legacy canvases/popups and removed guide roots are preserved. Byte backups capture the prior uncommitted scene and source outside the repository.

Runtime edits are limited to binding effect sprites, formatting currency, animating saved thumbs, opening existing guidance and the revised shortcut/draw presentation. No economy, claim, draw, prestige, account or ad-provider rules were changed. There is no live ad SDK/ad unit; the existing native unavailable state remains in effect.

The production CanvasScaler and safe-area owner are unchanged. Header info controls are anchored beside the collapse buttons; the product popup reserves its existing close/title band. Upgrade rows reserve 36 units for artwork and a 96×54 purchase target with a ten-unit right gutter. Product purchase targets are 90×56. Existing popup body scrolling handles short screens.

## Verification

- `Core-checks.txt`: **839** passing RedesignLab checks.
- `Validation.txt`: **135** passing native production-route checks with a disposable local profile. Includes all saved UI components before startup, twelve effect assets / 1,000 mappings, current row artwork, non-overlapping columns, compact info and learned-topic reentry, currency and live claim updates, popup/rareness states, switch intermediate/settled/reduced-motion states and persistence, four phone sizes, simulated notch insets, a zero-cash reset, earning the first ₹30 and purchasing Phuljhari, and unchanged component identities throughout.
- `Saved-scene-audit.txt`: saved Main reload verifies all serialized revision references, 25 currency pools, three switches, inactive popup entry roots and no missing scripts.
- `Scope-checks.json`: all thirteen imported art files match their HTML sources byte-for-byte; protected font files and Bootstrap are unchanged.
- `HTML-inventory.json`: the live package passes resource/asset/JSON inventory. The inspection excludes preserved historical `parity/before` snapshots and separate review galleries. The source baseline archive is unchanged.

Screenshots in this folder are actual Unity Game View captures. Useful examples: `02-Player.png`, `02-Products.png`, `02-Legacy.png`, `02-Shop.png`, `03-achievements-token.png`, `04-Upgrades.png`, `04-ProductDetail-0.png`, `04-Skills.png`, `05-settings-switches.png`, `08-notched-settings.png`, and `09-fresh-products.png`. The `07-` captures show short/tall layouts.

No real user save or live service was used. Android/device builds were not requested or run for this transfer. Existing Unity scene serialization whitespace is retained; hand-edited source checks pass.
