# Offline calibration experiments

The 16 October `CardRework-*` files test the user's selected card/Helping Hands redesign. They are experimental and have not changed Unity balance. See [the experiment notes](../CardReworkExperiments.md) for the chosen approximately ×2 level-100 card target, rejected weaker benchmark, full-multiplier checks and validation requirements. Empty output from an interrupted search is not a result; progress files contain only the stages evaluated so far.

These files record candidate searches from 15 October 2026. They are not runtime content and are not all passing configurations. `TapBalanceBeforeStress.json` is the pre-patch synthetic-card stress measurement; `TapCalibrationAccrualParity.json` records the accrual optimization before balance changes. The other files explore tap scales, prestige targets and rejected Fame-income changes.

The selected constants are in [TapBalanceCalibration.json](../TapBalanceCalibration.json) and `EconomyBalance.cs`. The final measured acceptance matrix is [TapBalanceMatrix.json](../TapBalanceMatrix.json). See [the report](../TapBalanceReport.md) for coverage, rejected long-wait candidates and the explicitly shorter catalogue-completion run.

Experimental `--income-offset` candidates were not applied: the product Fame income curve remains unchanged. Cache fixtures under `Temp` contain simulator-generated run starts, not player saves.
