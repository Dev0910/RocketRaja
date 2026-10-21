# Skills popup variants — 19 October 2026

The user requested three ChatGPT image-generation concepts for the existing Your skills popup and explicitly allowed changes to its UX. The images use the game's Gouache Storybook / Ivory Bazaar direction: painted parchment, cobalt, gold and maroon, with the existing three skill identities.

| Option | Image | Proposed interaction |
| --- | --- | --- |
| 1 | [Quick action list](01-quick-action-list.png) | Three stacked skill rows, with effects, timing, direct Activate and Details. |
| 2 | [Skill workshop](02-skill-workshop.png) | Vertical skill selector and one focused detail/activation panel. |
| 3 | [Festival tickets](03-festival-tickets.png) | Three illustrated skill columns, each with its own status, timing and activation. |

Selection: **Option 1 — Quick action list, selected by the user on 19 October 2026.** User instruction: “I like the Quick action list — all skills visible in stacked rows. implement it in the html”. This selects the generated reference and directs an adaptation of the existing popup. Options 2 and 3 remain unselected alternatives.

The selected list has now been implemented as editable HTML/CSS in `UI Builder/HTML/Classic/index.html`, `classic.js` and `skills-popup.css`. The HTML result is a candidate awaiting review; selection of the image is not approval of the resulting HTML. Tracked as H2U-010 in `Documentation/Implementation/Classic-HTML-to-Unity-TODO.md`. Unity remains unchanged.

All visible values are illustrative established-shop fixture data, not new mechanics or balance decisions. Real labels and values must remain live in any eventual HTML/native implementation. Gold/grey rings and direct-activation behavior remain governed by the existing reviewed requirements. Existing timer semantics measure cooldown from activation.

Generator: built-in ChatGPT image_gen; no Gemini, alternate image service, CLI generator, HTML rendering or procedural compositing used to create the concepts. Full prompt set: [PROMPTS.md](PROMPTS.md).

