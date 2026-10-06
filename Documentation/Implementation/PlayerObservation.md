# Player observation — pending

RR-11-01/02/03 require player observations; automated passes do not close them. The separate native playtest has been built, but Android has rejected installation. Keep the user's original installed game and its saves intact. Do not replace actual observations with inferred understanding from clicking Got it.

Opening protocol: let the player explore the fresh game without explaining which controls to use. After a short opening session, ask what they tried first, what they believe the main balances/upgrade screens do, and where they felt stuck or unsure. Continue an active session long enough to encounter the early product expansion and an upgrade gap; record actual duration and interruptions. Keep their answers verbatim and distinguish observations from suggestions.

First-rebuild protocol: use earned progression where practical. If using a disposable staged save to reach the reset quickly, disclose that it is a fixture, preserve the opening playtest save, and do not call its elapsed time an earned first-run measurement. Let the player inspect/choose the reset and rebuild without coaching. Ask what they expected to keep or lose, what the free-draw/KK balances mean to them, and how they interpret a repeated Common card. Record the real interaction and uncertainty rather than whether a button was pressed.

Known model risks to compare with these observations:

| Risk | Existing deterministic evidence | Observation still needed |
|---|---|---|
| Products arrive early | Sustained seed 1009 introduces the eighth family at 380 seconds (6m20s); source `ReferenceModel/sustained.json`, `firstFamTimes`. | Does this feel rushed, and can the player identify what comes next? |
| Long gap between named upgrades | Same recorded opening has `maxGapHours=0.227777…` (13m40s). Levels remain available. | Does the player find useful actions or wait without understanding the choices? |
| Earlier-product comeback | Native command test doubles earlier Phuljhari normal income from ₹42.5 to ₹85/s and reports the actual share change; full signature/category checks also pass. | Can the player notice and explain why investing in an earlier shelf can help? |
| Deep runs | Published seed-1009 4T/16T policies complete on days 66/57; the reference seed completes on day 90. These are nonoptimal simulated policies, not player-day predictions. | Can the player understand the extra Fame/credit review and choose whether to reset now or later? |
| Common duplicates | All card curves/boundaries and native duplicate/reveal copies are tested; Common-only model completes on day 123, reaching U=844 at day 90. | Is a duplicate's immediate level/effect gain understandable and worthwhile to the player? |

No economy coefficient or progression threshold was changed based on these pending observations. Current UX changes have recorded layout/interaction evidence in Guidance.md and Feedback.md. Structural changes require an explicit design discussion; an observed frustration does not automatically justify changing confirmed mechanics.

Results: none recorded yet. Do not mark player acceptance complete until actual notes are added here.
