# Scoped Talent artwork correction

The native smoke reveal exposed a green/blue neighboring-product sliver in the existing `Assets/Game/ImportedUI/Art/Product rockets.png`. The original file remains unchanged. This correction reuses the already reviewed Basic regions in `product-appearances.json` and the persistent `Product Sprites/*-0.asset` references. All 28 card-to-family mappings stay as before (`index % 8`); economic effects, card identities, layout and controls are unchanged.

The original Talent/card/reveal wireframes continue to define layout. The editable `talent-art-reuse.html` gallery records the scoped art reference at card/reveal sizes before the saved prefab bindings are updated. UI approval pauses are waived by the user. Native verification must confirm all three surfaces (collection, details and reveal) use these same persistent sprite references, with no runtime creation.
