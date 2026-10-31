# Shopkeeper tutorial — HTML revision 1

29 October 2026. **HTML candidate; visual approval pending.** Wireframe revision 1 approved explicitly, followed by the Gouache Storybook references. The latest user direction is: “I aprove it star making the html”. This authorizes the present HTML stage.

Canonical entry: `index.html`. Preview: <http://127.0.0.1:8772/Tutorial/>. Clean phone view: `?capture=1`. Choose any approved moment with `?state=welcome|saving|training|result|product|upgrade|achievement|prestige|talent`. The review sidebar provides the same selector, replay, existing-user and saving-unavailable fixtures, and explicit achievement/prestige events. It is excluded from captures and the native action inventory.

## Sources and assets

- [Approved editable wireframes](../../Wireframes/Tutorial/README.md) control geometry and behavior, including the compact phone cases.
- [Approved art-reference revision 1](../../Reference%20Images/Tutorial/ART-DIRECTION.md) controls Gouache Storybook materials, merchant identity, navy/gold actions, cream panels and warm festival scene.
- Existing authorized separable layers are reused byte-for-byte through relative URLs: `../Home/assets/gouache-stall.png`, `gouache-shopkeeper.png`, `gouache-products.png` and `gouache-paper.svg`. Product atlas display uses clipped SVG viewBoxes; images are not programmatically painted or modified. See the Home GOUACHE-PROVENANCE.md for generation provenance.
- Local typography comes from `../Shared/typography.css` and its bundled OFL fonts. Labels, dialogue, prices, balances and controls remain editable text. No reference sheet is used as a flattened screen background.
- This package is served from the common `UI Builder/HTML` parent so its shared local dependencies resolve; it requires the existing Home and Shared folders. It uses no CDN or external service.

## Intent / native / HTML evidence

| Element | Approved intent | Native evidence | HTML | Adaptation |
| --- | --- | --- | --- | --- |
| Opening and waiting | Bubble from merchant; collapse while earning; current action in next-step card | ClassicGuidance optional tips and Home goal text | Independent bubble, scene, shelves, merchant, live objective and replay control | New sequential coordinator required; avoid duplicate tips and keep objective visible |
| Training/product/upgrade | Highlight a useful purchase; explain actual changes | RedesignEngine LevelQuote/PreviewLevels/BuyLevels/PreviewUpgrade/BuyUpgrade | Guarded fixture purchases and before/after explanations | Native values must come from actual quotes/results, not fixed fixture numbers |
| Free navigation | All four tabs usable; collapse to Home | ClassicUI.SetTab/Open/Close | Four semantic tab buttons, independent lower panels; scrolling where needed | No forced route changes; highlight follows actual player state |
| Achievement | First earned claim; no prestige prerequisite | RedesignAchievements and ClassicUI achievement bindings | Ready example, incomplete/claimed guards and exact +5 fixture receipt | Native reward owner remains engine; first actual eligible claim may differ |
| Prestige/free pull | Show reset/keep preview; first prestige earns three credits; teach one free pull | RedesignEngine Prestige/CanDraw/QuoteDraw/Draw | Confirm/cancel preview and free credit reveal | No automatic prestige or paid fallback; native tutorial waits for durable success |
| Save failures/existing users | Suspend mutation; only enroll fresh saves | Session mutation fences and current state validation | Error fixture disables economic actions; old-user fixture suppresses tutorial | New optional state fields need safe old-save defaults and durable step ownership |
| Accessibility/layout | Minimum 48px controls, compact layout, no tap leakage | Existing gesture guards and UI input separation | Real buttons, visible keyboard focus, drag rejection, modal focus, short-screen scrolling | Browser captures do not prove native safe areas or persistence |

## Preview limits

The JavaScript is explicitly preview-only. It keeps deterministic fixture state in memory, has no browser storage and writes no Unity saves. No passive earnings timer is introduced. Tap/purchase examples exercise the focused opening; later achievements and prestige are reached with explicit review events. Full economy tuning, arbitrary higher-level products/upgrades, shop purchases, card randomness and all downstream screens remain owned by existing Unity systems. Informational dialogs stand in for destinations outside this focused tutorial package.

The embedded `unity-metadata` block is the sole handoff behavior contract: 18 actions, six binding groups, guarded transitions, tutorial/persistence/event modules and positive/negative QA. It distinguishes verified owners needing presentation adaptation from the approved new tutor coordinator. No native implementation or release readiness is claimed.

## Review and verification

All **61 browser checks pass**. `verify.cjs` covers all nine states at 390×844, 360×540, 360×640, 390×700 and 450×916. It checks game fit, no horizontal document overflow, four reachable tabs, 48px controls, real pointer tap separation, training debit/result, product purchase, exact claim/duplicate rejection, prestige cancellation, three free credits without auto-pull, free-credit spending, save-unavailable guards, skip/free navigation, old-user exclusion, valid metadata IDs and asset/browser errors. Clean captures for all nine states at normal and short sizes are in `renders/`; results are in `browser-checks.txt`.

The panel scrolls on short screens and brings the suggested purchase into view; Home stays fixed with counter and navigation exposed. Native persistence/failure testing belongs to the later Unity stage.

To serve: start a loopback static HTTP server in `UI Builder/HTML`. To rerun checks: `node verify.cjs <bundled-playwright-module-path>` while port 8772 serves that directory. Record runtime dependency paths with `load_workspace_dependencies` on another machine.

Native transfer is tracked as H2U-024 in `Documentation/Implementation/Classic-HTML-to-Unity-TODO.md`. HTML remains a candidate until the user reviews it; Unity reconstruction requires separate user direction under the project workflow.
