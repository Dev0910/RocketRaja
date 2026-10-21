# Unity presentation comparison

19 October 2026. User-directed HTML-only revision to match the current Unity game. Unity source, scenes, assets, settings, editor mode and open scene were left unchanged.

The comparison uses the current saved Main/Bootstrap scenes, `ClassicLayout`, `ClassicGuidance`, `ClassicProgression`, `ClassicUI.Bindings`, the native UI authoring source and actual native textures/fonts/import settings. Main's earlier native captures were used as geometry evidence; their temporary Gemini artwork is not a source for this revision. Existing original Unity artwork supplies all HTML images.

Fresh native evidence is `reference/current-bootstrap-1080x2400.png`, obtained by reading the existing Game View. Unity remained stopped with Bootstrap open and clean. Main was not opened or run. The [Bootstrap comparison](renders/Bootstrap-comparison.png) shows Unity on the left and HTML on the right. The native status panel is x=49, y=1469, width=982, height=864; browser measurement differs by less than 0.05 pixels at that same canvas. Browser/TextMeshPro font antialiasing differs, so this is not a claim of identical pixel buffers.

Changes include the persistent full stall across tabs; native Canvas scaling and panel spacing; native PNG button/row surfaces and 400-PPU slicing; exact font files; product/card geometry and UI copy; compact native guidance; production Shop availability; and the startup welcome. Product Sprite rectangles use Unity's imported texture dimensions: the Resources atlases have ToNearest NPOT scaling from 1254 to 1024. HTML crops account for that ratio. Local PNGs do not alter any Unity importer or Sprite.

`prepare_assets.py` exports browser copies from read-only native sources. `guide.js` contains saved scene topic copy. `native.css` and `startup.css` hold the visual comparison changes. The existing canonical `index.html` and `classic.js` retain the preview and functionality metadata. Previous source is in `before/`.

`check.cjs` passes **134 viewport/route/fixture checks**, plus guidance acknowledgement, disabled production Shop, image loading, metadata and browser error assertions. It covers 26 screens at 360×540, 360×640, 390×844 and 450×916, plus six snapshots across the five main panels. The existing current-UI script also passes 44 viewport/route checks and its interaction assertions. Results are in `html-checks.json`; screenshots are in `renders/`.

`unity-source-before.json` stores the starting hashes. `verify_readonly.py` compares every file under Assets, Packages and ProjectSettings. `unity-readonly-check.json` confirms **1,385 files unchanged**, with no additions or removals. Editor state was false for Play/Pause/Compile/Update before and after; the only open scene remained clean Bootstrap.

HTML numbers, ownership, progression, learned tips and account choices remain disposable fixture data. They do not mirror or alter a real player save. Read-only source comparison establishes the visual mapping; browser tests are not native gameplay execution or production account/store validation.
