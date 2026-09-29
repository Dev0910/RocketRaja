# Bootstrap HTML v1

Visual status: **approved**. On 2026-09-29 the user said “yes I like it make it in unity”. Native reconstruction is authorized and saved at `Assets/Scenes/Bootstrap.unity`; see `Documentation/BootstrapValidation/README.md`.

## Approval evidence

- Wireframe: `UI Builder/Wireframes/Bootstrap/index.html`, v1. After presentation, the user replied “lets complete the pending work” on 2026-09-28; the previous stage recorded this go-ahead.
- Artwork: `UI Builder/Reference Images/Bootstrap/bootstrap-reference-v1.png`, v1. On 2026-09-29 the user explicitly said **“I aprove the art work cretae the html”**. This approves artwork v1 and authorizes this HTML stage.
- HTML v1 was explicitly approved on 2026-09-29 with “yes I like it make it in unity”.

## Open and edit

Open `bootstrap.html` directly, or the local preview at `http://127.0.0.1:8767/Bootstrap/bootstrap.html`. It works offline with bundled fonts and artwork. The review selector is outside the game canvas.

Use `?state=conflict` to open a specific fixture. Add `&capture=1` for a clean viewport-sized screen. States: `welcome`, `loading`, `syncing`, `conflict`, `replace`, `device`, `handoff`, `offline`, `error`, `recovery`, `fresh`, `account`.

- `bootstrap.html`: semantic live text, panel, buttons, two save cards and the single embedded `unity-metadata` contract.
- `bootstrap.css`: palette, local type, panel styling, responsive layout, focus and reduced-motion treatment.
- `preview.js`: explicitly marked preview-only fixture transitions. No network, Firebase, browser storage or economy operations.
- `assets/`: text-free Gouache illustration and local Fraunces/Nunito font copies from the project's Shared fonts.
- `renders/`: 12 states at 1080×2200, 390×844, 360×640 and 360×540; layout check results.

The preview uses the approved Android sign-in presentation. Unity Editor/Windows remain guest/test-only; previewing a Google button does not demonstrate an actual authentication flow. Main is an external destination, not a fabricated game screen. Buttons which would enter Main stop on an initialization fixture and describe the destination in the review toolbar. The selector exposes success/failure outcomes without pretending a service has responded.

## Visual treatment and asset provenance

Gouache Storybook / Ivory Bazaar: maroon title, teal supporting copy, warm gold ornament, ivory live status panel, white Google action and outlined guest action. All visible labels and controls remain editable. The generated raster contains no text or buttons.

`assets/bootstrap-text-free-v1.png` was created with the built-in imagegen tool on 2026-09-29 as a faithful text-removal edit of the approved reference, then copied into this package. Exact prompt: `assets/prompt-v1.txt`. Original tool output: `C:/Users/Dev/.codex/generated_images/01a0e861-0d0b-7ad1-9287-e18555760563/exec-919c7511-3636-4510-87db-0e8148ba98db.png`.

The stall, eight illustrated product positions and shopkeeper are a static decorative startup illustration; they do not react to saved product ownership, earn cash, or animate. A single illustration layer is used for this screen, separate from all live UI. Future character animation would require separate artwork layers during a separately scoped art task.

The comparison values (₹1.24M / ₹8.60M, 6 / 8 products, 1 / 3 prestiges, sample save times) and account display name Raja are explicit fixtures, not observed account data. Preview labels are editable, not generated economy values.

## Evidence and handoff

| Element/state | Approved intent + source | Unity evidence | HTML evidence | Difference | Treatment |
| --- | --- | --- | --- | --- | --- |
| First launch | Approved wireframe and reference; Google or guest | BootstrapController.Start/Welcome; PlayerSessionHost | Welcome state, live buttons | Native scene absent | Bind serialized controller references in later Unity stage |
| Local/cloud initialization | User plan: prepared session before Main | BootstrapController.Run; PrepareGoogleAsync; EnterAccountAsync | Loading/syncing, indeterminate indicator | HTML distinguishes presentation states more clearly | Adapt copy and indicator bindings, preserve existing session ownership |
| Save selection | Never merge; show summaries and choose | ChooseProgress/Summary/ConfirmReplacement | Two live cards, cloud/device actions, explicit replacement confirmation | Runtime has one comparison TMP_Text | Split presentation into serialized card fields; keep state data owned by host |
| Device handoff | One active device; final save or expiry | ContinueAccount, Enter(true), host lease acquisition | Device/handoff states | Pending heading is more specific | Presentation-only adaptation |
| Connection/sign-in failure | Pause account; retry or separate guest | Error, Run catch handlers, AccountSession.Block | Offline/error states | Runtime uses a generic paused title | Presentation-only adaptation; preserve failure guards |
| Save recovery | Preserve unreadable files; confirm fresh guest | Recovery, StartFreshGuest | Recovery/fresh states | No new save semantics | Bind existing actions |
| Account controls | Sign out to separate guest; return to shop | AccountMenuRequested, SignOut, EnterMain | Account state | Fixture name/status; native Settings wiring pending | Bind profile/status, do not infer real login |
| Context hint | Guest stays local; account sync behavior | Current description text and host behavior | Live hint field | No dedicated hint TMP field yet | Add serialized hint presentation in later Unity work |
| Illustration | Approved festival stall/shopkeeper reference | No runtime behavior | Separate text-free raster layer | Static decoration | Never treat product art as save state |

The original HTML-stage runtime evidence below was code-only; subsequent native integration is documented in Documentation/BootstrapValidation/README.md. No Unity scenes, scripts, prefabs, saves, Firebase configuration or import settings were modified. The embedded contract has three state-dispatched action slots, six bindings, state/async/persistence/accessibility modules and negative QA scenarios. Native adaptation is explicitly marked `existing-needs-adaptation`.

## Validation

Browser layout checks cover 48 combinations (12 states × 4 viewports): no viewport overflow, no overlap between branding and status panel, all visible controls fully inside the viewport and at least 48 CSS pixels tall, and artwork loaded. Short screens prioritize the status panel and controls, reducing visible scene area; dense states omit the repeated footer/tagline there.

Inspected full-size welcome, conflict and short-phone recovery captures. Tested preview button transitions for Google sign-in, guest route, both save choices, replacement Back, handoff, retry, fresh-game confirmation/Back and sign-out. Captures demonstrate presentation only; real Google login and device handoff testing remain separate.

The workflow ends here for HTML review. Per `.agents/skills/image-to-html/SKILL.md`: “Unity is separate, explicitly directed work after HTML review.”
