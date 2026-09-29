# Native UI animations

The current implementation follows the shared HTML motion reference. The detailed effect-by-effect review, approximations and evidence are in [HTMLMotionReview.md](HTMLMotionReview.md).

| Owner | Responsibility |
|---|---|
| `UiMotion` | 230ms panel entrances, 190/250ms modal fade/card entrances, 160ms exits, 170ms quantity menus, 180ms notices; interruption and modal input ownership |
| `UiFeedback` | 120ms button presses, 140ms skill presses, mouse hover, 180ms navigation icon selection, native face tint, bounded existing accent pulses |
| `UiProgress` | Retargetable 360ms width interpolation from authoritative product state |
| `UiCooldown` / `UiSkillRing` | Clockwise cooldown clearing and 180ms skill state ring color |
| `ImportedHomeScreen` | Six reused 900ms tap labels; 650ms static Reduced Motion feedback; routing state-driven presentation |
| `FestivalSparkles` | Twelve native stars with an 850ms elliptical burst |
| `ShopAmbient` | Four-second keeper sway, two clipped 16/21-second visitor loops, modal pause and reduced-motion reset |
| `ImportedSettingsScreen` | Existing 180ms toggle knob movement |

All UI hosts, components and references are Editor-authored in the thirteen `Assets/Game/ImportedUI/* Screen.prefab` assets already connected to Main. Runtime code animates those objects and binds game state; it does not create UI. Existing purchase/reward transactions retain their original owners.

## Author and verify

- `Tools > Rocket Raja > Author HTML Motion` updates the saved hosts, overlays, meters and references. It uses `TryGetComponent` to handle Unity's fake-null wrappers safely and refuses to run in Play Mode.
- `ReviewHtmlMotion.Transitions()` reloads all saved screen prefabs and checks motion/renderer/control references and start/end geometry.
- `Rocket Raja > Validate UI Component Creation` checks reuse of authored components; runtime fallback creation has been removed.
- `Rocket Raja > Run UI Smoke Checks in Editor` exercises gameplay, all dialog dismiss controls, transitions, feedback, progress, taps, stars, ambient effects and two real Game-view sizes. It uses a temporary save/test ads, restores the previous Game-view size and exits Play Mode.
- Stage-specific checks are available through `ValidateUiMotion.Stage("taps")`, `Stage("celebration")`, and `Stage("ambient")`.

Latest reports and captures are in `Documentation/AnimationValidation/Editor`; preserved stage reviews are in `Documentation/AnimationValidation/HtmlMotion`. Browser timing samples are outside the project at `C:/Users/Dev/.codex/visualizations/2026/09/28/html-motion-review`.

The earlier September 27 Windows build report describes the superseded spring/particle animation pass. That executable has not been rebuilt for the current HTML-matched implementation.

Final Editor validation (28 September 2026): 143 runtime checks, 160 saved-prefab/trajectory checks and four component reuse checks passed. See the detailed review for native rendering approximations. Unity is outside Play Mode and the pre-existing dirty Main scene is preserved.
