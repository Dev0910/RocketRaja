# Native festival presentation — 31 October 2026

Implemented the reviewed HTML effects in `Assets/Scenes/Main.unity` using saved native uGUI and the existing command flow.

- Tap sparks, three coins, pooled payout text and rings; stocking pop/dust and milestone receipt/shopkeeper nod at 10, 25, 50 and 100 (highest new milestone per purchase).
- Skill-driven warm light and working hands; one Bulk Order parcel on successful activation. Appearance stops when the real active interval expires.
- Achievement token flights from the actual claim position to the visible wallet (Shop shortcut fallback). Duplicate claims cannot replay the reward.
- Prestige dim/bloom after the existing command commits; cancellation has no transition and animation never performs a second reset. Existing reveal gets a rarity ring and sparks.
- Quiet 11.52-second synthesized loop, tap/purchase/skill/reward/reveal/UI cues, independent Music/Sound switches and the existing Reduced motion option. Native music uses the existing session lifecycle; the browser-only gesture gate is not needed in the player.

Authoring: `ClassicFestivalFeelAuthoring.Apply` (menu: **Rocket Raja → Classic → Apply reviewed festival animation and audio**). Runtime: `ClassicFestivalFeel`, `ClassicMotion`/presentation, `ClassicUI`, `RedesignAudio`.

Fixed saved pools: original 64 particles, 20 idle-firework sparks, 6 rings, 4 payout labels, 3 KK tokens, two hand images, one parcel and a milestone/prestige presentation. Images never intercept raycasts. Font/material slots are cloned in the Editor. Original particles and feedback use saved child canvases above dialogs. New sprite assets are small procedural UI ring/glow/bloom textures; existing approved hands, parcel and currency art is reused. Exactly two authored AudioSources remain. No runtime UI/effect-source creation.

`ClassicFestivalFeelValidation.Start` runs production Bootstrap → Main with an isolated temporary save and restores the previous profile override afterward. Forty checks passed; see [checks](checks.txt). It tests real opening commands, then explicitly seeds an advanced fixture only in that temporary profile to exercise milestone/skill/reward/prestige branches. Prestige eligibility derives from the live target. It verifies cleanup, settings, retained popup parents, 360×540 / 390×844 / 450×916 layouts, reveal completion, unchanged runtime component identities and no logged runtime errors.

Captures: [tap](tap-390x844.png), [stock](stock-390x844.png), [milestone](milestone-390x844.png), [skills](skills-390x844.png), [reward](reward-390x844.png), [prestige](prestige-450x916.png).

Scope: Editor Play Mode verification. No Android build or physical-device audio mix/performance measurement was performed. Existing save/economy rules and SDK/package configuration are unchanged.
