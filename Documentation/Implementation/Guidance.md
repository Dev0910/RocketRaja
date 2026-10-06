# Contextual guidance

RR-08-05 is complete. Editable wireframe, reused visual-component contract, 13-topic HTML preview and saved native implementation are inspected. Five browser checks, 64 native checks, 35 focused core checks and 47 actual Play Mode checks pass. With the new help inset, Main passes 60 Play Mode and 12 asset/render checks, catalogue passes 69, and Home passes 31 pointer/interaction checks.

The 52-unit optional strip reserves 60 units under the 90-unit header. Existing pages remain scrollable. Read tip opens one 230-unit reader above navigation. Both Got it and Skip tip acknowledge the topic durably; neither executes gameplay. New candidates start collapsed. Topic text is authored in `UI Builder/HTML/Redesign/guidance-topics.json` by `BuildGuidancePreview.cjs` and serialized on the native presenter through Editor APIs. Acknowledgement records a skipped/read topic, not proof that a player understands it.

Selection gates:

| Topic | Relevant actual route/state |
|---|---|
| buy-first | Home or product 0 details, no first product and no completed Prestige |
| tap-counter | Home with an owned first product |
| product-levels | Product details or tap training; manual first purchase takes priority |
| catalogue-filters | Upgrades with gameplay started |
| buy-all | Actual frozen Buy All review |
| skill-basics | Skill details |
| comebacks | Unowned signature details for an earlier family when a later family is owned |
| prestige | Prestige review |
| rebuild | Home with no first product after Prestige |
| cash-and-talent | Locked Talent before the first Prestige |
| talent-free | Talent with unused credits and no pending reveal |
| talent-duplicates | Saved duplicate reveal |
| second-gift | Talent or test shop after the one-time gift; take priority over free-draw guidance |

Learned IDs use the existing persisted field and survive Prestige. `LearnTopic` is a presentation command (`structural=false`), rejects duplicates/full capacity, and preserves reviewed economic quotes. Topic selection does not save, grant, or draw. A failed acknowledgement retains the topic and lets normal save recovery handle the failure. A replacement is suppressed in the same unchanged route/argument/command after dismissal; navigating or making a real command may reveal a different relevant collapsed strip. Readers never queue or force-open.

Native integration: one saved presenter on the router, saved strip and reader under Safe Area, saved TMP material slots/buttons. The presenter adjusts only the screen-state body's top inset while the strip is visible. It closes the reader on navigation, topic/context changes, utilities/recovery or new result feedback. Opening a tip hides the competing result notice. Back closes a reader before navigating. Covered touches cannot leak; uncovered navigation/gameplay remains available. Existing retained utility overlays keep priority.

Evidence: [HTML checks](Guidance-HTML.txt) cover all 13 topics at 390×844 and 360×540. [Native checks](Unity-Guidance-UI.txt) cover the same copy at short/notched sizes, saved materials/actions, no truncation, failure/recovery, feedback coordination and unchanged UI identities. Inspected captures include the [short Prestige tip](NativeUI/guidance-prestige-360x540.png), [first-purchase tip](NativeUI/guidance-buy-first-390x844-notched.png) and [second-gift tip](NativeUI/guidance-second-gift-390x844-notched.png). These isolated renders use a Home background to inspect the reader; actual route selection is checked separately.

[Core checks](Core-Redesign.txt) exercise all selectors and nonmatching states, duplicate/failed/ownership-lost acknowledgements, capacity, actual save-store/codec and Prestige persistence, and unchanged frozen batch/Prestige quotes. [Two actual Play Mode sessions](Unity-Guidance-Play.txt) use topmost UI raycasts to open/acknowledge/skip, close help via Back before leaving the page, purchase manually, preserve/commit a reviewed batch, and reload the learned IDs from disk without opening help or generating taps. The final core suite contains 620 checks; model parity remains 7,744 passing comparisons. Native phone behavior and uncoached comprehension remain later acceptance work.

Reproduce: `BuildGuidancePreview.cjs`, `VerifyGuidancePreview.cjs`, `BuildRedesignUI.AddGuidance`, `RedesignGuidanceValidation.Run`, `RedesignGuidancePlayValidation.Start`. Authoring is idempotent and uses Unity Editor APIs only. The user waived UI approval pauses.
