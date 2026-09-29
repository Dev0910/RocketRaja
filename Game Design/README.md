# Game design research: Tap Tycoon and Rocket Raja

Research expanded 30 September 2026. These documents study Game Hive's *Tap Tycoon: Country vs Country*, review the full checked-in Rocket Raja design, compare other incremental games and primary research papers, and define testable responses to the identified problems. This is a design review; the playable economy was not changed.

Open [the interactive findings dashboard](index.html) in a browser for a visual overview. It is a self-contained HTML file that works from a local checkout; its source links lead back to the Markdown reports.

## Reading order

1. [Comprehensive Design Audit](Comprehensive%20Design%20Audit.md) — ranked problem register across pacing, purchases, products, tapping, skills, prestige, UI, return play, and evidence gaps; includes a fresh instrumented trace.
2. [Design Recovery Roadmap](Design%20Recovery%20Roadmap.md) — measurement, UX repairs, alternative economy experiments, playtests, and save/content constraints.
3. [Research Paper Review](Research%20Paper%20Review.md) — nine primary academic sources, methods, findings, transfer limits, and falsifiable Rocket Raja hypotheses.
4. [Incremental Game Benchmarks](Incremental%20Game%20Benchmarks.md) — developer-documented solutions and limitations across six other incremental games.
5. [Comparison and Recommendations](Comparison%20and%20Recommendations.md) — direct Tap Tycoon versus Rocket Raja comparison and suggested changes.
6. [Tap Tycoon Design Breakdown](Tap%20Tycoon%20Design%20Breakdown.md) — competitor loop, business upgrades, tapping, skills, prestige, Tech Cards, social play, monetization, and reported shortcomings.
7. [Rocket Raja Economy Audit](Rocket%20Raja%20Economy%20Audit.md) — source-derived formulas, content, quantitative examples, and pacing discrepancy.
8. [Balance Experiment Plan](Balance%20Experiment%20Plan.md) — original prototype values, measures, guardrails, and later audit amendments.
9. [Research Sources](Research%20Sources.md) — annotated official, player, wiki, spreadsheet, review, and video/source-code search records.

## Important evidence limits

- Game Hive's official listings establish the intended tap → business → weekly country loop and release history. Detailed numerical mechanics come mainly from 2015–2017 community play records and may differ from the present app. No official source code or raw balance tables were found in the searched public material. Video descriptions were located, but playback was unavailable, so the analysis contains no invented video observations or timestamps.
- The checked-in [Balance.md](../Documentation/Balance.md) and [EconomySimulation.json](../Documentation/EconomySimulation.json) describe an earlier deterministic run. On 30 September 2026, the current simulator failed its first-run pacing assertion. A diagnostic run of the current exported catalog measured 46:26 to first active prestige, 12:36 to the second, and 9:10 to the third. These are simulated policy results, not human playtest times.
- The deeper UI audit corrected an earlier claim that product art and the stall visually change at tracked tiers. The current imported Home pulses the same sprites at product milestones and uses one stall background. [Validation.md](../Documentation/Validation.md) and the relevant design docs now state that limitation accurately.
