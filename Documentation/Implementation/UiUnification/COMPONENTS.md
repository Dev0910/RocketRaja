# Shared component contract

`system/tokens.json` owns the palette, font roles, spacing grid, shapes and motion roles. `system/tokens.css` is generated; `system/components.css` owns active visual rules. `system/layout.css` retains production geometry with historical skins removed. The canonical Classic entry loads five stylesheets instead of 22. Historical files remain available but do not supply active visual skins.

| Family | Shared treatment | Variants |
| --- | --- | --- |
| Buttons | Imported Folk faces, crisp Native UI text, 48-unit target | Primary blue; secondary ivory; destructive red; unavailable sand |
| Navigation | Same blue/ivory faces and labels | Selected blue; readiness marker independent of selection |
| Panels | Warm paper, ink titles, restrained gold separators | Full stall retained above all management tabs |
| Popup | 336-unit maximum width, 16-unit safe edge, flowing scroll body, pinned actions | Header wraps naturally; recovery keeps its required-choice guard |
| Section/row | Raised paper, one muted gold outline, 6-unit corners | Art/copy/action columns; information cards; metrics |
| Portrait | Warm circular medallion, gold outline, stable image scale | Small Settings icon; skill; product; hero illustration |
| Progress | Dark track, dark teal fill, rounded clipping, cream value | Product, achievements, stored earnings; loading uses the gold highlight role |
| Form | Raised paper, readable label, 48-unit field, visible error | Search and naming; keyboard focus independent of pointer focus |
| Talent | Same type hierarchy, frame and illustration region | Rarity changes the accent, not the interface language |
| Feedback | Warm palette, bounded particles, independent audio switches | Press, navigation, purchase, reveal, prestige; reduced motion settles feedback |

Primary body text is 14 units; captions are 12. The original illustrated shelf nameplates retain their scene lettering; readable product names and values appear in the management UI. Shelf buttons have independent 48-unit hit regions. On short screens the lower Home region scrolls, preserving the full stall and readable controls. All lower lists and parent popups retain scroll position.

Safe-area insets use browser environment values. Review-only `?safe=44,0,34,0` simulates top/right/bottom/left device CSS pixels; the logical 360-unit canvas divides these by its scale exactly once. Tutorial pointer coordinates are relative to the safe content parent.

Stable action IDs, parameters and Unity functionality metadata remain the routing contract. `bazaarComponents.decorate()` assigns presentation roles without issuing commands or granting rewards. No economy, save schema, ad callback or purchase authority is introduced by these components.

The asset manifest identifies the reused artwork and byte hashes. No new illustration style was required: existing recognizable art is presented through common materials, icon scales and framing. Minor texture differences in historic illustrations are recorded in the manual artwork score.

Validation separates functional checks from manual design assessment. Vertical line-box overflow with visible glyphs is not classified as clipping; hidden/clip overflow and actual screenshot boundaries are checked. Intentional scroll viewport crops are verified again at the bottom of the content.

## Native component mapping

The qualified system is mapped to `Assets/Game/ClassicUI/Bazaar/Gouache Bazaar.asset`, reusable prefabs and existing Editor-authored controls. See [native implementation and reproduction](native/README.md). The application tool preserves serialized action owners; runtime code binds and animates preplaced nodes. Main uses 360 logical units, while the existing Bootstrap canvas uses three native units per logical unit.
