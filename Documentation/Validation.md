# Validation and delivery status

Validated September 14, 2026, using Unity **6000.4.9f1**, .NET 10 and the exported authored catalog. The requested 6000.3.8f1 editor was unavailable on this machine.

## Passed

| Evidence | Result |
|---|---|
| `CoreTests.txt` | 109 checks: arithmetic, quantities, catalog, all Legacy ranks, discounts, skills, prestige, offline reconciliation and backup handling |
| `UnitySaveTests.txt` | 7 checks using Unity JsonUtility and the actual SaveStore, including large numbers, immediate saves, reset retention and backup recovery |
| `Screenshots/SmokeResults.txt` | 36 runtime checks: art loading, rupee glyph, income visibility, scene taps versus drags/navigation, milestone/tier logic, panel scroll retention, prestige confirmation and test ad outcomes. These checks did not establish distinct sprites for the tiers. |
| `EconomySimulation.json` | Three prestige cycles for active, occasional and offline policies; active acceptance bounds pass without ads |
| `Build-StandaloneWindows64.txt` | Windows development player build succeeds |
| `Build-Android.txt` | Android ARM64 IL2CPP development APK build succeeds |
| `AndroidPackageChecks.txt` | Valid APK v2 signature, API 36 target, API 25 minimum, ARM64, 16 KB ZIP alignment and compatible ELF LOAD segments for every packaged native library |

Actual desktop captures are 540×900, 540×960 and 540×1080. The request for a 1200-pixel-high window was clamped to the desktop display height; the capture report records the actual size. These are desktop layout checks, not physical-device or cutout validation.

The screenshot harness uses a separate temporary save and a deterministic ad service. It ignores desktop focus changes so another build process cannot make hidden-window input tests nondeterministic. Normal gameplay retains the real focus/pause lifecycle handling. The main economy suite and Unity save suite test repeated offline reconciliation across save/reload.

## Delivered behavior

Eight auto-earning products, economic milestones, tracked visual-tier thresholds, three lower tabs, bulk quantities, 96 run unlockables, repeatable tap level, three active skills, exact prestige preview, 12 five-rank Legacy perks, local save recovery, offline earnings, independent sound/music/reduced-motion controls, original generated artwork, local audio, particle celebrations, Google Mobile Ads/UMP integration and an isolated test adapter. The imported Home currently binds one sprite per product and one stall background; tier changes pulse existing art but do not swap to distinct display or stall artwork. The `StallTier` value is not used by the runtime UI.

The imported stall background is a complete illustrated facade. Products, character frames, UI and effects are independent layers. Detailed cutout character-part rigging, visible stall-stage swaps, product-tier sprite swaps, and more decorative animation remain deferred; the current character blinks and sways. This corrects the earlier claim that four stall stages and 32 visibly distinct displays were delivered.

## Still required before public release

- Physical ARM64 Android install and gameplay, low-end sustained 30 FPS profiling, cutouts/safe areas, thermal behavior and small-screen readability.
- Real SDK rewarded playback, failures, consent flows, privacy options and pause/resume while a video is showing, using test units on devices.
- Uncoached onboarding tests and three full human prestige cycles; simulator targets do not prove human pacing or retention.
- Owner's Play Console/AdMob verification, final application ID and signing ownership, published privacy policy and accurate policy/data-safety declarations.
- Required closed testing and production-access approval, title availability, final store materials and store review. Nothing has been uploaded or published.

See `ReleaseRunbook.md` for the operational checklist. The APK is intentionally a development build using sample advertising identifiers. Production builds are gated on signing, real identifiers and an HTTPS privacy-policy URL.
