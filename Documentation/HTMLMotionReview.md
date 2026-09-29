# HTML animation review — 28 September 2026

The game now uses native, Editor-authored animation components matching the shared HTML preview. This pass was implemented and checked in sequence: transitions, controls/messages, progress/cooldowns, tap income, celebrations, then ambient motion. The final combined review also covers interruptions and real Editor viewport changes.

## Reference and interpretation

The animation authority is `UI Builder/HTML/Shared/motion.css`, `motion.js`, `coverage-states.css`, `coverage-states.js`, `floating-navigation.css`, `folk-buttons.css`, and the Home cooldown CSS. Browser review controls and gallery interactions are excluded. The shared preview's straight tap-number animation is used; the standalone Home file also contains an older bouncy variant.

An inspected local browser preview was rendered with external HTTP requests blocked. Its animations were paused at explicit timestamps for start, intermediate, and end captures. Those images and computed browser animation samples are in `C:/Users/Dev/.codex/visualizations/2026/09/28/html-motion-review`. The initial browser lifecycle regression also passed for panels, nested modals, rapid navigation, scroll retention, focus, resize and game/system Reduced Motion.

Unity matches CSS easing by solving the cubic Bezier's time axis. Pixel travel is converted through the root Canvas scale factor, so the stated movement stays in rendered pixels. Authored typography and larger artwork retain their existing Canvas scaling.

## Sequential review

| Order | Trigger and affected layer | Reference motion | Native result and evidence |
|---|---|---|---|
| 1 | Home/lower panel navigation | 230ms entrance; 16px upward travel from below, or ±18px horizontal between tabs; 160ms exit by 14px; cubic-bezier(.2,.75,.25,1); no scale | Dedicated saved hosts animate around the fitted panels. Older exits settle on subsequent navigation. Start/end samples, rapid navigation, retained scroll and layout checks passed. |
| 2 | All nine modal routes | Backdrop fade 190ms; card entrance 250ms, 16px upward and .98→1 scale; exit 160ms, 10px downward and 1→.985 | Saved CanvasGroups and hosts retain modal input blocking until exit finishes. The first review found four older dialog structures; those were repaired before advancing. All nine close buttons were tested through pointer raycasts. Reopen and Settings/Privacy handoff passed. |
| 3 | Quantity menus, toast, inline messages | Quantity 170ms, +7px x and .97→1 x-scale about the right edge; message 180ms ease-out, 5px upward/fade; immediate hide | Both quantity menus and all message hosts are preauthored. Inline message motion lives inside the layout row. Toast never intercepts input. Leave/reopen, opacity, completed pose and immediate hide checks passed. |
| 4 | Button press/release, skill press/hover, selected navigation, active skill state | Buttons 120ms ease-out, .98 scale and 1px down; skill disc 140ms to .93, mouse hover 2px up; nav size 180ms; filter 160ms; skill shadow color 180ms | Button and icon channels restore authored transforms. Keyboard submit uses unscaled time. Nav icons animate to 40/34 width and 43/36 height ratios. Skill state rings use native geometry and a color transition. Press/release, repeated feedback, submit, pointer exit, selection and Reduced Motion checks are included. Native filter/shadow differences are listed below. |
| 5 | Product level changes, detail opening, skill timer | Bar width 360ms with the standard curve; cooldown cover clears clockwise from 12 o'clock | Eight saved product bars and the detail fill follow the authoritative level. Retargeting begins from the current width and repeated refreshes do not restart it. Radial coverage reads engine cooldown state, including the Legacy modifier, and clears when ready. The first review found a missing overlay renderer; it was repaired and retested. |
| 6 | Stall tap | Shared preview: 900ms ease-out, straight 38px rise, opacity 1→0, no scale; reduced-motion text stays static for 650ms | Six of the existing twelve labels are reused in the HTML's four-position sequence. Rapid taps cancel/reuse their slots. Lifetime, straight trajectory, opacity, pool bounds and mid-flight cancellation passed. Capture: `08-tap-mid.png`. |
| 7 | Display upgrade, skill activation, rewarded success, prestige | Twelve gold/orange stars; 850ms ease-out; 45×32px elliptical spread; opacity peak at eased offset .18; .4→.6 scale | One saved mesh host draws twelve four-point stars. Repeated bursts restart the bounded group. The former random particles, extra large burst image and five-wave prestige effect are removed from the active presentation. Lifetime, restart, hierarchy reuse and Reduced Motion passed. Capture: `09-celebration-mid.png`. |
| 8 | Normal shop idle | Keeper −.7°↔+.7° over 4s, ease-in-out about bottom center; visitors move from −15% to 110% in 16s and 21s, second offset by 9s | A saved clipped native graphic draws both visitors and controls the keeper clock. Pivot changes preserve settled geometry. Both loops freeze under modals and resume after release. Reduced Motion hides visitors and restores the keeper. Period, offsets, modal pause/resume and reset checks passed. Final visual review caught a pivot-compensation error; the original zero offsets were restored, with an additional lower-counter placement regression. Capture: `10-ambient.png`. |

## Interruption and ownership review

- `UiMotion` cancels an earlier transition and its completion callback before reopening. It restores saved scale/position after close, disable, reduced motion, window resize or application focus loss.
- Closing modals keep their raycast blocker and modal ownership until the close completes. Purchases and rewards still commit through the original game engine. Animation code cannot grant a reward or advance a cooldown.
- Quantity menus close with their owning panel. Inline notices use independent child hosts, so layout groups retain control of row placement and height.
- Button transforms, selection, and accent pulses have separate channels. The skill's press and accent use the same component to avoid competing writes to its icon scale.
- Progress changes retarget from the displayed width. Disabled progress components settle to their last target. Reduced Motion completes in-flight presentation without changing the underlying value.
- Rapid taps reuse six labels. TMP may create glyph submesh renderers internally; hierarchy checks exclude those rendering internals and still require a fixed authored effect pool.
- Stars are bounded to twelve. Visitors are always two silhouettes. Effects never instantiate prefabs or add UI components at runtime. All hosts, CanvasRenderers, CanvasGroups, masks, meter fills and references are saved in the thirteen screen prefabs.
- Settings retains its existing 180ms knob movement. Existing purchase, wallet, reputation and unlockable accent pulses remain as Unity feedback; they are not claimed as HTML keyframes.

## Visual approximations and limits

The timing and trajectories are source-matched and exercised in Play Mode. This is not a claim of identical browser and Unity rasterization:

- The browser's typographic star and blurred glow are approximated by a four-point native mesh with two translucent glow layers.
- Visitors use a native silhouette made from a circle and body polygons. The SVG's curved shoulder and 0.4px CSS blur are approximated by the simpler geometry; the timing, tint, clipping and paths are preserved.
- CSS button brightness is mapped to CanvasRenderer tint on the control's native face. Browser filter compositing across child text and CSS drop shadows is not reproduced exactly; the existing button artwork is retained. Skill shadow color is represented by a thin native ring.
- The shared HTML's overlapping star animations are intentionally represented by a restart of one bounded group, so repeated reward events cannot grow an effect queue.
- The browser has both system and in-game Reduced Motion. Unity follows the game's saved Reduced Motion setting; a separate operating-system preference bridge was not added.

## Evidence and saved state

Reports are under `Documentation/AnimationValidation/HtmlMotion`. Stage-specific Play Mode reports preserve the successful intermediate reviews. `Transitions.txt` contains 160 checks over freshly loaded saved prefabs, including every screen profile, renderer, control target, trajectory, duration and restored geometry. `Documentation/AnimationValidation/Editor/SmokeResults.txt` is the most recent combined runtime report; it includes actual 540×900 and 540×1200 Game-view assertions rather than relying on `Screen.SetResolution` inside the Editor.

The test harness uses an isolated temporary save and deterministic test ads. Main and HTML Importer scene files retain their baseline hashes; the pre-existing dirty Main scene remains open. Production UI changes are saved through the connected prefabs. The task backup and a copy of the pre-existing unsaved scene are retained in `Temp/HtmlMotion-20260928` for recovery. The approved Baloo 2/Nunito font assets and unrelated artwork are outside this animation change.

No new tween package, WebView, runtime HTML renderer, or gameplay economy logic was introduced. This pass is validated in the Unity Editor; the older executable under `Builds/AnimationValidation` has not been rebuilt for it.

Final result: **143 Play Mode checks, 160 saved-prefab/trajectory checks and four authored-component reuse checks passed**, with no failures or runtime errors in the final run. The two portrait viewports were confirmed active. Unity was left outside Play Mode with the smoke flag cleared. A TMP material-cache update changed two Baloo shader ratio values during testing; the exact protected font asset was restored. All five backed-up font assets and both scene files match their baseline hashes (see ProtectedAssets.json).
