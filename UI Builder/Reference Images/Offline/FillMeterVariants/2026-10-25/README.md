# Offline fill-meter variants — 25 October 2026

User requested three generated variations of the interactive fill-meter concept. All use the existing values: 10 h away, 8 h storage cap, 8 h credited, 2 h beyond cap, 50% efficiency, ₹147.45M base earnings and ₹294.90M optional ad total.

1. `01-segmented-storage.png`: eight hourly segments, full at the eight-hour cap, with compact stats.
2. `02-capped-timeline.png`: gold credited-time section ending at the eight-hour cap, then a gray two-hour overflow tail through ten hours away.
3. `03-vertical-vault.png`: full vertical storage meter, with reward and efficiency alongside it.

Generated with built-in image_gen.imagegen, one reference-guided call per variant. Exact prompts are in `UI Builder/HTML/Classic/reviews/offline-fill-variants-20261025/PROVENANCE.json`. Matching copies are served by that review gallery. All three outputs were visually inspected for the required amounts, time values, cap, efficiency, info/close controls and two footer actions.

User selection pending. The gallery displays raster proposals; no replacement screen implementation or Unity changes were made. Generated background details remain illustrative, and any selected implementation must reuse the project-owned background and authoritative gameplay bindings.
