# Implementation contracts

`Catalog` is authored as a ScriptableObject snapshot in `GameContent`. Products, ordinary upgrades, skill durations/cooldowns/values and Legacy ranks/costs/values are data; economic behavior is implemented by deterministic runtime rules. Product and upgrade IDs are stable strings. Changing the built-in `Catalog.Create()` defaults does not silently overwrite existing definitions. Regenerate or edit the asset intentionally, export with Prepare Project, then rerun the simulator with `--catalog Documentation/ContentCatalog.json`.

`Amount` stores a nonnegative normalized base-10 mantissa and exponent. Earnings can exceed IEEE double magnitude; precision is approximately 15 significant digits, appropriate for an incremental economy. Ranks, counts, receipt IDs and reputation do not use this approximate type.

`GameEngine` owns commands: Tap, ProductQuote/BuyProduct, BuyTap, BuyUpgrade/BuyAll, ActivateSkill, PreviewPrestige/Prestige, BuyLegacy, ResolveOffline and RewardAd. UI displays these values and does not implement separate price/income formulas. Cosmetic traffic never drives simulation.

Product prices are a geometric series with growth 1.12. Income combines level, milestone, run-product, run-global, Legacy-product, Legacy-global and temporary multipliers. Bulk Order uses NormalIncome. Sponsor extras and offline advertising rewards are recorded separately from prestige-eligible earnings.

Prestige uses `floor(5 * sqrt(eligible / threshold))`, gated by owning eight families and reaching the threshold, capped at one million reputation per reset. A quote binds run ID, revision and reward. Confirmation pauses economic progression so the displayed quote remains exact. Modal time is intentionally excluded from income; absolute cooldown timestamps continue after the dialog closes.

Offline income is 50% of normal product income, capped at 8 + 2 × offline Legacy rank hours. The anchor is the greater of saved time and the current simulation clock; backward device clock changes grant zero income. Reconciliation persists the new anchor and reward together. Local clock advancement can still accelerate a fully offline game; there is no server clock or anti-cheat promise.

Ad callbacks enter a main-thread queue. UMP runs before requesting ads. The game uses sample units in development and explicit player actions for both placements. A successful reward callback produces a persisted receipt; duplicate receipts and consumed offline IDs are rejected. Ad presentation time is excluded from offline reconciliation. The deterministic adapter exists for testing and does not replace required real-device SDK/consent testing.

The thirteen screen prefabs use UGUI, TextMeshPro, a 1080×1920 reference resolution and Screen.safeArea. Main stores their presenter references on GameController; GameView binds the shared Home canvas and routes navigation and dialogs without constructing UI at runtime. All thirteen references are required at startup. The obsolete UI copies and their builder methods have been removed from the prefabs and HTML Importer scene.

The shared Home shop stays visible above the lower Player, Products and Legacy panels, which retain their ScrollRects. ShopTapTarget accepts only short pointer gestures starting on the shop. Lower controls live outside that event hierarchy. The saved screen artwork and product displays are referenced directly by their presenters.

Audio is synthesized locally from original short bell motifs, with independent sound/music settings. Home owns the saved cash popups and celebration image. Reduced motion disables popup movement and celebrations.


Bootstrap.unity is the first build scene. Its saved 1080×2200 uGUI canvas initializes identity and selects a prepared PlayerSessionHost session before Main. Main entered directly redirects through Bootstrap; isolated smoke tests use temporary guest sessions and test ads. All startup states reuse serialized objects. Guests never initialize Firebase auth/cloud services. Google accounts require ownership and a server-confirmed save, use server time for offline reconciliation, and pause when connectivity or ownership is lost; see CloudSaves.md. Settings exposes the serialized account entry.
