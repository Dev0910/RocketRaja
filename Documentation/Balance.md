# Economy version 1.0.0

> **30 September 2026 verification note:** The table below is the saved historical result in `EconomySimulation.json`, not a passing run of the current code. `dotnet run --project Tools/EconomyLab -- --catalog Documentation/ContentCatalog.json --simulate` currently throws `First-run pacing target failed` at `Program.cs:165`. A temporary diagnostic copy that logged instead of throwing measured active cycles of **46:26**, **12:36**, and **9:10** with the current exported catalog; first level 25 was **4:12** and all families **7:31**. The current first and second cycles exceed the asserted pacing ranges. See [the current economy audit](../Game%20Design/Rocket%20Raja%20Economy%20Audit.md#7-what-the-existing-simulator-shows-and-the-current-pacing-failure). The original simulator's assertions and economy data were not changed by this documentation update.

These results are deterministic simulations of the implemented economy, without advertising. They are not human playtest results.

| Active progression | Result | Target |
|---|---:|---:|
| First product | 48 seconds | 30–60 seconds |
| First level-25 milestone | 3:35 | Before 5:00 |
| All eight families | 5:07 | Before first prestige |
| First prestige | 24:35 | 20:00–30:00 |
| First rebuild to prestige | 9:18 | 5:00–10:00 |
| Third prestige cycle | 5:55 | Faster than the second |

The active policy taps three times per second, buys all affordable unlocked run upgrades in ascending price order, buys levels by marginal income per rupee, caps its tap-level investment at 60, and activates skills when ready. After prestige it prioritizes all-product Legacy, then tap Legacy, then individual products. The initial reward is five reputation: enough for the three-point global perk plus two one-point perk purchases.

An occasional policy plays two minutes in every thirty while the game remains open. The offline policy plays similar sessions but closes between them and receives 50% normal income. Their full three-cycle wall-clock timings are in `EconomySimulation.json`. Skill availability, purchase choices and session boundaries change these timings substantially.

The first-run pacing and first-rebuild bounds are assertions in the simulator. The third cycle must improve on the preceding cycle. Run the simulator again after changing authored content; use its `--catalog` option against the exported content to avoid testing different data from Unity.

The first prestige threshold is 1.7e17 eligible rupees. Formula: floor(5 × sqrt(eligible / threshold)), with ownership of all eight families required. Rupee costs use compact K/M/B/T/aa notation at large magnitudes; there are no real-money purchases.

Known balance work for closed testing: verify that humans can find skill upgrades and Buy All, determine whether the eight-product unlock cadence feels satisfying, and check whether the reset feels worthwhile. The imported Home currently pulses a static product sprite at tier changes rather than swapping to a new display; see the [comprehensive design audit](../Game%20Design/Comprehensive%20Design%20Audit.md). Change balance from observed player behavior, then rerun regression checks.
