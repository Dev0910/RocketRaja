# Individual skill detail popup variants — 19 October 2026

The user requested three ChatGPT-only generated alternatives for the existing individual skill popup and allowed UX changes. These are separate from the already selected and implemented three-skill Quick action list. That list supplies the surrounding painted Ivory Bazaar design language.

| Option | Image | UX emphasis |
| --- | --- | --- |
| 1 | [Hero skill card](01-hero-skill-card.png) | Large painted skill medallion, effect tiles, compact timing, activation and upgrades. |
| 2 | [Earnings comparison](02-earnings-comparison.png) | Before/with-Rush earnings comparison, timing and clearly separated actions. |
| 3 | [Power and recharge dial](03-power-and-recharge-dial.png) | Readiness dial and an active/recharge timeline explaining the total cycle. |

Selection: **Option 1 — Hero skill card, selected by the user on 19 October 2026.** The user requested implementation and explicitly removed the “View skill upgrades” button. In the ongoing HTML-first revision batch, this reference has been adapted to all three individual skill popups with one activation button and a close X. Options 2 and 3 remain unselected.

Editable implementation: `UI Builder/HTML/Classic/classic.js`, `skill-hero.css` and `index.html`. The selected image is the art/layout reference; live text, art, effect tiles, timing and actions remain separate elements. HTML acceptance is pending; Unity implementation remains tracked separately as H2U-011. No native or economy changes were made.

All concepts use Festival Rush as the sample: x2 automatic sales and +25% manual/Helping Hands tap payout for 30 seconds. Its established-shop fixture cooldown is 285 seconds measured from activation, leaving 255 seconds after the active duration. Option 2's money figures are illustrative fixture values. The selected Hero implementation also covers Helping Hands, instant Bulk Order and ready/active/cooldown/locked/save-error states using each skill's own exported data.

Generator: built-in ChatGPT image_gen only. [Full prompt set](PROMPTS.md). The original generated files remain in the default Codex generated-images directory; these copies are the persistent project references.

