# Third-party and generated content

- Baloo 2 and Nunito: SIL Open Font License 1.1. Sources: https://github.com/google/fonts/tree/main/ofl/baloo2 and https://github.com/google/fonts/tree/main/ofl/nunito. Licenses are included as `Assets/Resources/Fonts/Baloo2-OFL.txt` and `Nunito-OFL.txt`. These are the approved game fonts as of 2026-09-28.
- `Tools/instantiate_game_fonts.py` uses fontTools 4.59.0 to generate static Baloo 2 weight 700 and Nunito weights 500/800 from the original variable sources in `SourceArt/Fonts`. Both families contain the rupee glyph directly. Saved TMP font assets and real bold-face mappings are authored by `Assets/Game/Editor/GameTypography.cs`; no runtime font construction is required.

- Outfit: copyright 2021 The Outfit Project Authors, SIL Open Font License 1.1. Source: https://github.com/google/fonts/tree/main/ofl/outfit. License included in `Assets/Resources/Fonts/OFL.txt`.
- Noto Sans: SIL Open Font License 1.1. Source: https://github.com/google/fonts/tree/main/ofl/notosans. License included in `Assets/Resources/Fonts/NotoSans-OFL.txt`.
- Both variable fonts were instantiated at weight 500 for predictable Unity rendering using fontTools 4.59.0. The original source fonts remain in `SourceArt/Fonts`; the reproducible script is `Tools/instantiate_fonts.py`. Noto Sans supplies the rupee glyph missing from Outfit.
- TextMeshPro essentials and Liberation Sans fallback are supplied by Unity's UGUI package. Their notices remain alongside imported resources in `Assets/TextMesh Pro`.
- Google Mobile Ads Unity 11.5.0 and External Dependency Manager are installed through OpenUPM. Their own license and notice files remain in the packages. Android dependencies are resolved by the SDK's build preprocessor.
- Stall, product, shopkeeper and app-icon illustrations were generated with the built-in image generation tool for this project. Product prompts and source paths are recorded in `ArtSources.json`; shopkeeper and app-icon prompts are separate text files. Generated originals remain in the Codex generated-images directory.
- Audio cues and background motif are synthesized by the project's own code; no sampled commercial music is included.
- Tap Tycoon screenshots and research informed interaction structure only. No reference game's artwork, UI assets, icons or source code are shipped.
