# Native UI quality review — 30 September 2026

The review covers the current Unity UGUI screens, preserving the approved Gouache
Storybook artwork and Ivory Bazaar surfaces. Existing uncommitted work was saved
under `Logs/UIQuality-2026-09-30/Baseline` before this review.

## Scoring contract (set before repairs)

Each of the following 24 checks earns one point only with evidence. The reported
percentage is passed checks / 24. A commit is permitted only at 24/24 and with no
unresolved defect discovered in the reviewed native screen/state matrix.

| Area | Four required checks |
| --- | --- |
| Readability | Main navigation is named; displayed dynamic values have supported glyphs; essential text fits without truncation; reviewed compact-screen body/status text stays legible. |
| Visual craft | Approved artwork remains coherent; input boundary is clearly visible; placeholder contrast reaches 4.5:1; input focus is visibly distinct. |
| Navigation | Tabs retain the correct selected indicator; Back dismisses the topmost surface; quantity choices remain reachable and explicitly selected; outside dismissal consumes its tap without buying. |
| Feedback and accuracy | Effective skill cooldown matches the engine including Legacy; failed videos explain the outcome; available retry is reachable without reopening; pending/used reward state cannot grant a duplicate reward. |
| Session and motion | Purchase availability respects mutation ownership; Settings reflects session changes while open; reduced motion covers Bootstrap guest preferences; existing transition/tap/milestone checks remain valid. |
| Layout and reachability | Home fits the short viewport; shelves receive the frontmost raycast; scrolling reaches the last row; modal exit/actions remain visible at the reviewed sizes. |

Target viewport matrix: 360×540, 360×640, 390×844, 450×916 and 540×1200.
Review all existing screens, with targeted locked/ready/active/cooldown, affordable/
unaffordable/blocked and video cancellation/retry/consumed states.

This is a bounded native implementation review. A perfect review score does not
establish universal artistic perfection, physical-device accessibility, Android
dp measurements, screen-reader support or uncoached player usability. Those
remain separate validation activities; unexecuted checks earn no point here.

## Research and repair log

Initial source review found seven actionable weaknesses. The first native
short-phone run also recorded seven failures, including layout timing and tap
reward geometry. The commit was withheld throughout the repair iterations.

| Finding | Research and alternatives | Selected repair |
| --- | --- | --- |
| Optional-video cancellation strands the offer | Google specifies that ads are single-use and another becomes usable only after reload. A permanently disabled offer contradicts its copy; automatic retry would remove player choice. | Keep eligibility, show the outcome and restore the button when a video is ready. Retain request/receipt guards and block dismissal during the pending callback. |
| Skill guide ignores permanent cooldown reductions | System-status guidance requires values to describe the real state. Rounding fractional minutes masks the perk's effect; duplicating the formula encourages drift. | Centralize effective cooldown in the engine; show exact minutes/seconds in the guide and use it for activation and the Home ring. |
| Purchase buttons appear usable in a blocked session | Affordability and permission are independent. The engine already rejects blocked commands, but an enabled control promises an action it cannot perform. | Gate the existing price controls with the same mutation authority as their commands. Keep the price visible. |
| Shop-name input and focus are hard to distinguish | The old placeholder was 3.67:1 on white, and white against ivory only 1.09:1. Merely changing white to another pale fill would leave the boundary weak. | Use the established cobalt frame, dark placeholder (7.26:1) and a geometric focus underline. Preserve the input's saved identity and text references. |
| Skills guide uses tiny compact-screen text | Unity scales the entire canvas from the reference width. Increasing type alone inside the old boxes would truncate it; squeezing with autosizing would restore the original problem. | Use 36-unit essential type (12 rendered pixels at 360 width), taller cards, two-line effect boxes and a wide status badge. Scroll the content while retaining fixed exits. |
| Quantity menu requires an explicit choice or Back to dismiss | Unity's own dropdown uses a transparent blocker. Letting the first outside click reach a purchase would cause an accidental action. | Save a full-viewport blocker behind the menu; the outside click closes the menu and is consumed. Retain Back and explicit selection. |
| Account status and Bootstrap motion miss live preferences | A stale save label obscures progress; the Bootstrap spinner already resolves the guest preference, but its button feedback did not. | Subscribe Settings only while visible; use the same session/guest motion resolution for Bootstrap presses and reset a held label when the preference changes. |
| Tap popup touches the hint on short phones | Native geometry showed the saved top-pivot popup was positioned using centre-pivot arithmetic. Moving the entire shop or shrinking its text would hide the underlying error. | Place the popup using its actual pivot inside the available lane above the hint. |
| Privacy explanation clips and overlaps its heading | Five native viewport checks and pixel review exposed the same overlap. Shrinking the policy copy would make it harder to read; omitting it would remove relevant information. | Reflow the full copy into a taller scrolling card with separate heading/body boxes and 36-unit body type. |
| Settings toggle artwork disagrees with its text | Unity Image.overrideSprite controls rendering even after Image.sprite changes. Feedback cached the old face/colors and restored them after the presenter set the new state. | Explicitly synchronize the feedback baseline after the presenter changes appearance; preserve purchase-disabled artwork and animation tint ownership. |
| Product details are discoverable only by chance | The existing design review required a separate Details cue. NN/g research favors visible interaction signifiers over invisible click regions. | Add a named Details cue inside the existing artwork/detail target, preserving a separate purchase action. |
| Browse-panel secondary copy becomes too small on compact phones | The canvas scales 1080 reference units to the active width. A 36-unit floor gives 12 rendered pixels at 360 width; increasing type within old rows would clip prices and effects. | Reflow Products, Player and Legacy into taller scrolling rows with explicit one/two-line boxes and larger prices, preserving artwork and saved object IDs. This is a desktop pixel floor, not an Android accessibility certification. |
| Later skill-upgrade rows still advertise base cooldowns | Independent code review followed the effective-cooldown formula beyond the guide and found the catalog's static description. The same system-status rationale requires every purchase description to match the engine. | Format skill upgrade descriptions from the effective cooldown helper; exercise the later pooled rows at Legacy ranks 1 and 5. |
| Store-name character instruction remains too small | A focused-input capture exposed a 10-pixel essential instruction on the shortest viewport. Existing text space already accommodates two lines at the chosen 36-unit floor. | Increase the saved Validation helper to fixed 36-unit type with wrapping; preserve modal geometry and add a native pixel-floor check. |

The initial resize tests sampled Screen, safe-area and layout updates before all
three had settled in the batch editor. The harness now waits explicit frames
before asserting the same unchanged thresholds; it does not lower them.

Primary research: [Android touch targets](https://developer.android.com/guide/topics/ui/accessibility/views/apps-views),
[W3C text contrast](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum),
[W3C non-text contrast](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast),
[NN/g system status](https://www.nngroup.com/articles/visibility-system-status/),
[Google rewarded ads](https://developers.google.com/admob/unity/rewarded).

Additional implementation references: [Unity Canvas Scaler](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html),
[Unity dropdown source](https://github.com/Unity-Technologies/uGUI/blob/main/com.unity.ugui/Runtime/UGUI/UI/Core/Dropdown.cs),
[Unity TMP input events](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_InputField.html),
[W3C animation guidance](https://www.w3.org/WAI/WCAG21/Understanding/animation-from-interactions).

Additional discovery references: [NN/g clickable-element signifiers](https://www.nngroup.com/articles/clickable-elements/),
[Unity Image override sprite](https://docs.unity.cn/2018.1/Documentation/ScriptReference/UI.Image-overrideSprite.html).

## Iteration evidence

| Pass | Result and next action |
| --- | --- |
| Initial short-phone regression | Seven failures. Corrected top-pivot tap geometry and explicit resize/layout settling. |
| Broad pass 1 | 524 checks passed; Privacy text failed in ten top/bottom observations. Nineteen save errors came from a test fixture whose reputation exceeded lifetime reputation; corrected the fixture and asserted validity. Pixel review also identified stale Settings toggle faces and missing Details cues. |
| Broad pass 2 | 597 checks passed, zero failures, 116 native captures. All quantity choices receive pointer hits; outside dismissal consumes purchase taps. Settings ON/OFF artwork and text remain matched after feedback updates. |
| Existing short-phone regression | Passed with the original thresholds, including named shelves, Settings, tap reward lane and account/privacy scroll reachability. |
| Compact-type pass | 627 checks passed, zero failures, 116 captures. Independent pixel review found no new clipping; requested unobstructed short-screen captures and a focused-input capture before awarding visual points. |
| Expanded final pass | 800 checks passed, zero failures, 128 captures. Added actual input focus, all nine later skill upgrades at two Legacy ranks, and synthetic pending-video guards. Independent pixel review requested one last larger store-name instruction. |
| Instruction pass | Native pixel-floor checks caught four observations where the runtime opener reset the saved helper size to 30. Corrected the runtime assignment to 36 in every naming mode; no thresholds were relaxed. |
| Gameplay and motion regression | 145 checks passed. Onboarding/Home: 192 passed. Detail purchase: 22 passed. Unlockable controls: 12 passed. Saved transition/reference checks: 169 passed; Unity save codec: 7 passed; core economy/save checks: 308 passed. |
| Final repaired matrix | 812 checks passed, zero failures, 128 native captures across all five viewports. Final independent pixel review confirms the larger instruction, actual focus, unobstructed compact panels and later skill-upgrade rows. |

## Final score: 24/24 (100%)

All checks in the original scoring contract have supporting native or source
evidence, with no unresolved observed defect in the reviewed matrix. The independent
art and interaction reviews were completed before awarding the score.

| Area | Score | Evidence |
| --- | --- | --- |
| Readability | 4/4 | [Quality matrix](UIQualityValidation/Quality.txt), [saved glyph regression](UIQualityValidation/StoreGlyphs.txt), independent pixel review. Products/Player/Legacy, Skills statuses and naming instructions meet the chosen 12-pixel floor at compact width. |
| Visual craft | 4/4 | Native solid-color contrast checks, empty/focused input captures at 360 and 450 widths, coherent illustration/ivory palette across the matrix. |
| Navigation | 4/4 | [Gameplay and motion](UIQualityValidation/Gameplay.txt) verifies tab/Back behavior; quantity selection and consuming outside pointer hits are verified in the quality matrix. |
| Feedback and accuracy | 4/4 | Effective cooldown checks cover guide, activation and all nine later upgrades at two Legacy ranks. Cancellation, readiness changes, consumed receipts and synthetic pending-video guards pass. |
| Session and motion | 4/4 | Blocked purchase state, live Settings subscription, guest Bootstrap motion fixture, gameplay transitions and [917 milestone checks](UIQualityValidation/Milestones.txt) pass. |
| Layout and reachability | 4/4 | [107 short-viewport checks](UIQualityValidation/ShortViewports.txt), five-size matrix top/bottom geometry and frontmost pointer checks, independent pixel review. |

Additional regressions: [Home/onboarding](UIQualityValidation/HomeCard.txt),
[detail purchases](UIQualityValidation/DetailPurchase.txt),
[unlockables](UIQualityValidation/Unlockables.txt),
[maximum quantity](UIQualityValidation/MaxPurchase.txt),
[tap animation](UIQualityValidation/Taps.txt),
[core economy/save](UIQualityValidation/CoreTests.txt),
[Unity codec](UIQualityValidation/UnitySaveTests.txt), and
[saved transition references](UIQualityValidation/Transitions.txt).
[Idempotence](UIQualityValidation/Idempotence.txt) confirms that applying repairs
twice leaves seven saved prefabs and the authoring scene byte-identical.

The repository retains 28 representative PNGs and all final result logs under
`Documentation/UIQualityValidation`. The full 128-image final matrix remains in
`Logs/UIQuality-2026-09-30/Final4`. Run **Rocket Raja → UI Quality → Run Native
Review** in the Unity Editor to reproduce the matrix in an isolated smoke session.
The **Apply Reviewed Screen Repairs** command preserves existing object IDs.

The commit contains the reviewed current UI, including related UI work already
present at the start of this request. The unrelated Firebase repository-URI edit
is excluded. The original dirty checkout snapshot remains under the ignored
baseline directory recorded above.

The score applies to this defined desktop-native review. Real rewarded ads and
cloud resync, the actual Bootstrap scene, Android physical-device sizing,
screen readers and uncoached usability remain outside the executed evidence.
