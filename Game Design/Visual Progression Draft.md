# Rocket Raja: product and shop visual progression

**Current presentation — 7 October 2026:** see [Current Implementation Design](Current%20Implementation%20Design.md). Six permanent shop stages and product looks at ownership/signatures I/IV/X remain. Classic updates local artwork, values, effects and audio; transient purchase/milestone notices are disabled. No stage toast, batch milestone list or pinned-goal flow is required. The earlier proposed feedback below is historical.

Version 0.1, 1 October 2026. Planning only; no artwork or Unity assets created.

Cash-only progression is selected. [Economy Specification v1](Economy%20Specification%20v1.md) and the main catalogue contain the current prices and Fame formula, with no completed-prestige requirements for cash upgrades. The x6 opening bonus is retained. Numerical balance remains subject to prototype tuning; the [decision record](Cash-only%20Economy%20Alternative%20v1.md) preserves the superseded comparison.

Confirmed direction: evolve the same shop through bigger displays, lighting, and decorations. The shop remains recognizable as the player progresses. Earned decorations persist through prestige, and product displays rebuild with product repurchases, as confirmed. Stage names, milestone thresholds, and appearance counts remain prototype defaults.

The aim is to make the longer progression visible while keeping the shop readable and the asset scope manageable. Visual milestones do not introduce new income effects or extra entries beyond the existing 1,000 cash purchases.

## Two layers of visible progress

Use a permanent shop layer to show the account's development and run-specific product displays to show what is currently operating. A prestige clears the run's product displays under the confirmed visual-reset rule but keeps earned shop improvements. Product ownership and current income remain clearly labeled, so an impressive retained storefront does not imply retained product levels.

An empty product position shows a quiet placeholder and its next unlock requirement. An owned position shows that family's current run appearance. Permanently unlocked shop decorations remain in place around these displays. A compact reset message can explain: 'Your shop's improvements stay. Rebuild your products with stronger income.'

## Permanent shop stages

Use different cash upgrades purchased at least once, U, as the candidate visual progression counter. This reuses a confirmed lifetime record, but choosing these visual gates does not approve the separate draw-bonus thresholds.

| Candidate U threshold | Working stage | Visible change |
|---|---|---|
| 0 | Opening stall | Small counter, simple sign, basic shelving, modest product spaces. |
| 100 | Established shop | Repaired frontage, cleaner shelves, larger signs, organized stock. |
| 250 | Festival shop | Coordinated banners, warm string lights, expanded display surfaces. |
| 500 | Neighborhood landmark | Larger awning, layered display shelving, recognizable shop emblem. |
| 750 | Celebration showroom | Premium display framing, richer lighting, more detailed frontage. |
| 1,000 | Flagship shop | Finished facade and a distinctive celebratory decoration set. |

These six appearances can be built from shared structural layers and five upgrade kits rather than six unrelated environments. Keep the eight product positions and primary tap target stable across stages. Increase detail around those anchors without making later interfaces harder to use. Thresholds do not depend on calendar dates, spending real money, or prestige count.

A newly reached shop stage is previewed with its next milestone before it is earned. On first achievement, show a short transformation and a before/after summary; allow it to be skipped without losing the appearance. Do not repeat the full transformation every prestige or every login. Lifetime card levels and visual stages are separate.

## Product display progression

Propose four owned appearances per family, tied to existing signature purchases. The signature effect remains whatever the numerical catalogue later specifies; the art transition grants no additional multiplier.

| Appearance | Candidate trigger in this run | Visual role |
|---|---|---|
| Basic | Own at least one product level | Identify the family and establish its display. |
| Signature | Buy that family's first Signature Breakthrough | Better packaging, stock arrangement, and family sign. |
| Premium | Buy its fourth Signature Breakthrough | Larger display treatment and more distinctive product detail. |
| Showcase | Buy its tenth Signature Breakthrough | Final premium presentation for this product family. |

Each family already has ten signature entries in the current draft. These gates select existing entries rather than adding purchases. The art schedule is a proposal; prices must leave enough time to notice stages, particularly when the first signature follows ownership quickly. Some signatures remain economic milestones with a brief effect rather than a full art replacement.

| Family | Visual identity to preserve through its appearances |
|---|---|
| Phuljhari | Bundles and gift packaging with a fine sparkle motif. |
| Anar | Fountain-shaped product silhouettes and warm upward accents. |
| Chakri | Circular packaging and a recognizable radial motif. |
| Rockets | Upright bundles, launch-direction shapes, and sky accents. |
| Small crackers | Compact packets and stacked trays. |
| Cracker strings | Strung package silhouettes and rhythmic repeated patterns. |
| Aerial shots | Taller cartons and expanding sky motifs. |
| Celebration boxes | Large assortment boxes and premium hamper framing. |

This calls for 32 owned display states total, including eight basic states that may reuse suitable existing art. Shared empty/locked treatments can reduce additional work. The precise format, art style, and reuse quality require an asset review later. No asset count here is an implementation estimate or a claim that the current assets already meet it.

Product appearances reset with run progress as confirmed and return as their qualifying upgrades are rebought. A lifetime album can record appearances seen, but it grants no bonus and is optional; it is not required for this first pass.

## Historical feedback proposal (superseded where noted above)

| Event | Proposed feedback |
|---|---|
| Ordinary product-level purchase | Brief local increase indicator and rate update. |
| First purchase of a cash unlockable | First purchase marker, effect change, and lifetime counter increment. |
| Rebuy | Compact effect confirmation; no lifetime counter increment. |
| Signature upgrade returning a family to importance | Brief spotlight on that display and the normal shop-rate change. |
| Product appearance transition | A short local transformation with an optional detail view. |
| Permanent shop stage | A single skippable scene transition and retained achievement record. |
| Rush/Hands overlap | Shared active outline and an understandable tap-bonus indicator. |
| Prestige | Show retained shop improvements, then clear and rebuild product activity. |

Use animations to explain the state change, not to delay purchases. Buy All aggregates repeated upgrade feedback into one result, with a route to individual changes. A large batch should not play dozens of mandatory celebrations. If it crosses several permanent visual stages, show the final appearance and list the milestones reached.

Keep cash, product labels, tap targets, and skill states readable during effects. Reduced-motion mode uses static highlights and text; important changes do not rely on sound, color alone, flashes, or fine motion. Do not place an effect over a purchase confirmation or obscure a cooldown.

## Presentation budget and boundaries

Initial authoring scope is one recognizable shop composition with six stage treatments, eight product families with four owned looks each, and a small shared set of purchase, milestone, and skill effects. The actual asset production plan should separate new work from reusable material after inspection.

The draft adds no travel map, new shop locations, decoration shop, cosmetic currency, or premium cosmetic spending. Visual milestones are presentation of the selected progression. The user confirmed same-shop evolution and decoration/display retention rules. Individual visual layers and thresholds remain prototype defaults.

## Review criteria

- A player can identify the same shop across all permanent stages.
- At least one meaningful visual change is reachable before late-game catalogue completion; its actual timing must be checked after pricing.
- Locked, currently owned, and upgraded product displays are distinguishable.
- A player understands which visual changes remain after prestige.
- Later shop detail does not obstruct the eight products or the active tapping area.
- A Buy All rebuild is visually concise and does not replay all prior account celebrations.
- Every advertised appearance change has a defined art change; purely economic milestones are described honestly.

See [Onboarding and Return Flow Draft](Onboarding%20and%20Return%20Flow%20Draft.md) for when these concepts are introduced.
