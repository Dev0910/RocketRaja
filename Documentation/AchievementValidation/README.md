# Player achievements and effect precision — 21 October 2026

Current gameplay balance: [40-stage achievement revision, 29 October 2026](2026-10-29-balance.md). The original implementation and verification record below is historical.

The saved Classic Main scene now has an **Achievements** control next to Skills on the Player page. The existing artwork, fonts and popup surfaces are reused. Completed rewards appear first, the control shows a ready count, and claimed rewards move to the end of the list.

## Rewards

Each achievement is claimed once per save. Kaju Katli and the claim receipt are persisted in the same transaction. Claims and progress survive prestige. Existing saves remain compatible; eligible lifetime progress is recognized when opening the screen. The best number of simultaneously stocked products is now retained across resets.

| Milestone | Targets | Kaju Katli rewards |
| --- | --- | --- |
| Manual taps | 100 / 1,000 / 10,000 | 10 / 25 / 50 |
| Lifetime earnings | ₹1,000 / ₹1 million / ₹1 billion | 10 / 25 / 50 |
| Different upgrades discovered | 10 / 100 / 1,000 | 10 / 50 / 150 |
| Different products stocked together | 1 / 4 / 8 | 5 / 15 / 25 |
| Prestiges completed | 1 / 5 / 10 | 25 / 75 / 150 |

There are 15 achievements worth 675 Kaju Katli in total. Rewards use Classic's existing save-backed wallet. Authoritative premium wallet profiles retain the existing restriction against client-side currency grants.

## Effect displays and spinner

Non-cost effects use whole-number multipliers or bonus percentages. For example, ×1.25 is shown as +25%; skill timings and fractional percentage displays round upward. Product, tap and upgrade price factors round upward to two decimal places, using a decimal point independent of device language. The underlying economy calculations retain their precision.

Both saved Bootstrap spinner RectTransforms now have a centered pivot. Their positions are adjusted to preserve the circle's original location. The Bootstrap authoring code and runtime initialization also use the corrected pivot.

## Verification

- `dotnet run --project Tools/RedesignLab`: all **805 checks pass**, including achievement eligibility, exact rewards, duplicate rejection, persistence failure and retry, receipt validation, existing-save compatibility, prestige retention and effect precision.
- Native Unity checks exercise the actual Player control, claim button, disabled claimed state, duplicate callback, checksummed save reload, scrolling and close button. See [native-checks.txt](native-checks.txt).
- All 1,000 cash upgrade descriptions and all 28 Talent card descriptions at levels 0, 1, 10 and 100 were checked against the precision rules.
- Layout checks pass at 360×540, 360×640, 390×844 and 450×916. Screenshots in this folder use disposable review progress, not a real player's wallet.
- Both saved spinner centers remain stationary when rotated from 0° to 90°.
- Unity reports no compilation errors; the disposable preview scene was removed and Bootstrap restored after validation.

To reapply the saved UI repair, use **Rocket Raja > Classic > Add achievements and fix loading spinner** in Edit Mode.
