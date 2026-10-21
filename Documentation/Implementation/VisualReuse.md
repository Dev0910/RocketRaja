# Visual progression reuse review — 2 October 2026

Scope: completed product reuse and permanent shop progression for RR-08-01/02/03 after Main integration. Approval pauses remain waived; editable layout, source/reference review, HTML mapping and native checks still apply. Integrated Android/player acceptance is separate.

All eight existing `Assets/Resources/Art/<product-id>.png` sheets were visually inspected. Each contains four distinct arrangements in a 2×2 grid: basic packages, larger stock, an organized tray, and an ornate illuminated showcase. Family silhouettes and packaging colors remain recognizable across all four. These are suitable reuse candidates for the four run-specific looks. No new artwork is claimed and no original texture was modified.

| Product ID | Inspected identity | Candidate cell order / run trigger |
|---|---|---|
| phuljhari | Long gold sparkler packets, maroon/teal trays | Top-left ownership; top-right Signature I; bottom-left IV; bottom-right X |
| anar | Red fountain cones visible through boxed sets | Same |
| chakri | Teal packets with round radial motifs | Same |
| rockets | Blue upright rocket packages | Same |
| small-crackers | Red compact packets and stacked trays | Same |
| cracker-strings | Magenta packets with visible circular cracker strings | Same |
| aerial-shots | Purple tall cylinders and gold rims | Same |
| gift-boxes | Teal assortment boxes, gold ribbons and ornate hamper | Same |

Completed: [editable wireframe](../../UI%20Builder/Wireframes/Redesign/product-appearances-v1.svg), [HTML gallery](../../UI%20Builder/HTML/Redesign/product-appearances.html) and [source-hashed region manifest](../../UI%20Builder/HTML/Redesign/product-appearances.json). Equal quadrants exposed neighboring artwork, so each of the 32 regions follows the existing transparent gaps and alpha bounds. SVG preview clipping and persistent Unity Sprite regions use the same rectangles; source pixels and old importers are unchanged. The 1100-pixel gallery was inspected, and all 32 native looks were inspected on four phone Home captures. Native product details have one capture per family/tier, plus short-screen top/actions checks. Shared empty/locked states stay distinct.

`RedesignProductArt` shares the 32 persistent sprites between Home and product details. `ProductAppearance` uses current-run ownership and three indexed signature-ID lookups, never product levels or lifetime U. Signature ordinals I/IV/X map to PP ranks 01/19/55, rather than ranks 01/04/10. Details show the current look and exact next signature. Core checks cover all eight families, no extra economics, durable save/reload, prestige clearing, basic rebuild and signature rebuy. Actual native purchase changes the shelf sprite immediately. Loading/retained shop milestones and integrated Android acceptance remain separate work.

`Assets/Resources/Art/stalls.png` was also inspected. It contains four whole-shop illustrations with differing frontage proportions. Four unrelated crops do not by themselves fulfill six cumulative treatments or fixed shelf anchors. The current native painted shop remains the structural reference. Five additive kits now supply cumulative stages at U=100/250/500/750/1000: marigolds, lanterns, embroidered apron, bunting and crest. [Generated sheet, exact prompt and inspection record](../../UI%20Builder/Reference%20Images/Redesign/shop-decorations-review.md). A compact progress panel names the next stage and explains retention. All product/counter anchors are unchanged.

`RedesignShopProgress` centralizes cosmetic thresholds separately from economic draw bonuses. Loading derives the earned stage from lifetime U with the saved `decorationStage` as a floor. `RedesignShopVisuals` directly toggles five saved groups; no milestone queue or animation can replay on load, rebuy or Prestige. One real 1,000-upgrade batch, actual Play Mode restart, native Prestige, manual first purchase and signature rebuy pass. All five permanent layers stay active while run product looks reset and rebuild. RR-08-01/03 are complete as native functionality; integrated device and human acceptance remain open.
