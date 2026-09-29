# Game typography

Approved and implemented on 2026-09-28: Baloo 2 + Nunito, selected from `UI Builder/HTML/font-review.html`.

- Baloo 2, weight 700: screen/dialog titles, the shop sign, Unlockables, and Statistics section headings.
- Nunito, weight 500: interface text, descriptions and numbers. Existing bold text uses a real Nunito weight 800 face through TMP's font-weight table.
- Source variable fonts: `SourceArt/Fonts/Baloo2-variable.ttf` and `Nunito-variable.ttf`. Recreate Unity-compatible static instances with `Tools/instantiate_game_fonts.py` and fontTools 4.59.0.
- Saved TMP assets: `Assets/Game/ImportedUI/Baloo 2 Bold SDF.asset`, `Nunito Medium SDF.asset`, and `Nunito ExtraBold SDF.asset`. Their static atlases include the saved UI corpus, ordinary ASCII and the required typography/currency symbols. Both families include ₹ directly.
- Editor authoring: `Assets/Game/Editor/GameTypography.cs`. The shared UI kit and Privacy/Settings builders use its font roles. Run **Tools → Rocket Raja → Apply Baloo 2 and Nunito** to reapply the font roles to saved screen prefabs. Existing static atlases are reused; extend their glyph coverage separately before shipping new characters/localizations.

The migration updates 335 labels across all 13 screen prefabs (16 Baloo headings and 319 Nunito labels). Existing scene instances inherit those references; no runtime font factory or gameplay changes were added. Unlockables' row gains 12 reference units of height and the Prestige KEEP/RESET text bounds gain 8 units for the new vertical font metrics.

Verification: every prefab was reloaded and checked for correct font roles, coverage of its text, and persistent material/atlas subassets. Temporary Editor copies of all 13 scene screens were checked at 1080×1920 and 1080×2400; no new text overflow remained. Home and Settings were visually rendered from isolated Editor copies. This is Editor verification, not an Android/device build or a live economy/ad session.

The pre-existing dirty Main scene was not saved. Main and HTML Importer scene files retained their pre-change hashes. Task backups and detailed audits are under `Temp/Typography-20260928`; rendered evidence is under `C:/Users/Dev/.codex/visualizations/2026/09/28/game-typography`.

