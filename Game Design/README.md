# Game design research: Tap Tycoon and Rocket Raja

Research completed 30 September 2026. These documents study Game Hive's *Tap Tycoon: Country vs Country*, compare its documented systems with the economy implemented in Rocket Raja, and define experiments for the three reported problems: starter products losing relevance, tapping fading, and prestige feeling unrewarding.

## Reading order

1. [Comparison and Recommendations](Comparison%20and%20Recommendations.md) — the direct design comparison, diagnosis, priorities, and suggested changes.
2. [Tap Tycoon Design Breakdown](Tap%20Tycoon%20Design%20Breakdown.md) — detailed competitor loop, business upgrades, tapping, skills, prestige, Tech Cards, social play, monetization, and reported shortcomings.
3. [Rocket Raja Economy Audit](Rocket%20Raja%20Economy%20Audit.md) — current source-derived formulas, content, quantitative examples, and pacing discrepancy.
4. [Balance Experiment Plan](Balance%20Experiment%20Plan.md) — prototype values, measurements, guardrails, and validation sequence. These are proposals; the live economy was not changed for this research.
5. [Research Sources](Research%20Sources.md) — annotated official, player, wiki, spreadsheet, review, and video/source-code search records.

## Important evidence limits

- Game Hive's official listings establish the intended tap → business → weekly country loop and release history. Detailed numerical mechanics come mainly from 2015–2017 community play records and may differ from the present app. No official source code or raw balance tables were found in the searched public material. Video descriptions were located, but playback was unavailable, so the analysis contains no invented video observations or timestamps.
- The checked-in [Balance.md](../Documentation/Balance.md) and [EconomySimulation.json](../Documentation/EconomySimulation.json) describe an earlier deterministic run. On 30 September 2026, the current simulator failed its first-run pacing assertion. A diagnostic run of the current exported catalog measured 46:26 to first active prestige, 12:36 to the second, and 9:10 to the third. These are simulated policy results, not human playtest times.
