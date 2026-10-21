# Classic design and presentation integration

6 October 2026. REL-06–11, following the production startup connection. The normal Bootstrap/Main route and existing persistent-profile owners remain in use.

## Content and progression

`RedesignAuthoring.Validate` compares the exact `Resources/GameContent.asset` with the approved catalogue and Talent design, including every serialized field, all 1,000 IDs, prices and prerequisite chains, eight products and 28 cards. The runtime manifest must match the content hash and economy/content versions. Numeric tolerance is 1e-14; authored price strings are exact. The content asset and manifest SHA-256 values are recorded in [journey evidence](Classic-Journey-Validation.txt).

Classic quotes, affordability, normal/product income, skill previews, Talent effects, Buy All and prestige use the bound `RedesignEngine`. The native replay compares displayed product prices and the committed quantities/costs, and compares the authoritative batch with the resulting purchases. Cash-upgrade prerequisites remain run ownership/product requirements; no Fame or completed-prestige gate was added.

The unboosted command replay starts with 30 cash and no products or rewards, manually buys Phuljhari, earns cash by advancing the actual engine clock, purchases all eight product families and all three base skills, then performs two prestiges and rebuilds. It checks exact Fame/credits, the first three free credits, the second-prestige 100 KK gift, run resets, retained discoveries/cards/guidance/preferences/decorations, opening cash and signature repurchase without duplicate discovery counting. Saves use temporary directories and the production codec. This is deterministic simulated-time evidence, not a claim of human play duration or balance acceptance.

## Saved Classic presentation

- `ClassicGuidance` adapts the existing learned-topic system to Classic's direct purchase controls, next-ten list, Buy All, skills, Talent, prestige, rebuilding and return play. Contextual strips open a scrollable reader. Only Got it commits `LearnTopic`; completed topics stay learned across reload and prestige. Static Help is supplementary.
- `ClassicProgression` retains the approved stall, shelves, navigation and basic product illustrations. Signatures I, IV and X select the existing three advanced illustrations in the same saved shelf, product-row and detail image slots. Prestige clears run appearances; repurchasing restores them.
- The existing five decoration kits are fitted around Classic's stable shelf/counter positions. Together with the opening stall they form six stages, driven by 0/100/250/500/750/1,000 lifetime discoveries. Decorations persist through prestige. The earned stage and next threshold are shown in Statistics; reaching a new stage gives one concise notice.
- One `RedesignAudio` owner and two `AudioSource` components are saved beneath Classic. Existing synthesized music is reused, with bounded tap, purchase, skill, reveal and prestige cues. Successful commands trigger cues; animation completion never grants or charges anything. Scene return creates no persistent duplicate audio owner. Music/sound preferences are stored through the engine. Backgrounding pauses music and stops effects.
- Classic's fixed 64-particle pool and existing animations remain. Reduced motion settles reveals immediately. Input is blocked behind dialogs and recovery; counter drags and interrupted touches earn nothing. Small controls receive a minimum 48-unit raycast area without changing the approved visible artwork. Affordability updates both button behavior and its visible style as passive income arrives.

All new UI objects, guidance readers, decoration layers and audio sources are authored into Main by `BuildClassicUI.ConnectPresentation` in Editor code. Runtime components bind or animate those saved objects; they do not construct UI or grow an effect pool. Previous compatibility UI stays inactive.

## Evidence and limits

- [Engine journey and exact content hashes](Classic-Journey-Validation.txt): 34 passing assertions, two unboosted prestiges and rebuilds.
- [Native Classic journey/presentation checks](ClassicPresentation/Validation.txt): **82 passing assertions, no runtime errors**, with short-screen guidance, earned-run, prestige/rebuild, return-play and settings captures beside it. Includes exact ×100/Max purchases, all four product-look mappings, all six decoration stages, final Continue without a duplicate reveal cue, modal raycasts, drag rejection, minimum 48-unit touch bounds and audio preference/background behavior. The separate appearance/stage matrix uses explicitly marked in-memory fixtures and never saves them into the earned journey.
- [Core regression](Classic-Presentation-Core.txt): 655 checks.
- [Classic draw/persistence regression](Classic-Presentation-Draws.txt): 66 checks.
- [Startup/recovery regression](Classic-Production-Validation.txt): 79 passing assertions after presentation integration.
- [Artwork source checks](ClassicPresentation/Art-source-hashes.txt): all eight product atlases and the decoration atlas match their recorded source hashes. Classic uses its approved eight basic sprites and the 24 advanced atlas regions.

The [installed device candidate and physical checks](ClassicPresentation/DeviceTest.md) record the APK identity, successful build/signature/install, user-confirmed audio and touch behavior, and a successful cold restart with retained progress and connected artwork. REL-06–11 integration is complete for this development candidate. The unboosted journeys use simulated time; human balance acceptance, wider device coverage and a final non-development candidate remain separate tasks. Store installation and live Google/billing configuration remain the previously documented release gates; this work does not enable or claim those services.
