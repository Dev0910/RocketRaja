# Classic scene and asset cleanup

2026-10-06. The user's explicit cleanup request supersedes REL-01's temporary retention of inactive compatibility UI.

Bootstrap and Main are the only project scenes and the only enabled build scenes. Main contains the camera, event system and saved ClassicProductionCanvas. Fifteen inactive legacy roots were removed. The obsolete HTML Importer and Redesign UI Preview scenes, retired screen prefabs, unused art/fonts/material and standalone Classic preview APK builder were removed with their metadata.

Classic authoring now targets Main directly and binds ClassicApplication. Production integration requires that saved hierarchy instead of cloning a preview scene. The build guard rejects legacy and preview session owners. Bootstrap authoring no longer edits retired settings/privacy prefabs. Obsolete UI authoring menu commands were removed; compatibility code remains where tests and shared presentation helpers still depend on it.

Removal candidates were checked against the transitive dependencies of every remaining project asset. Shared fonts, product progression artwork, shop decorations and button art remain in their existing folders to preserve references. Files were copied to ignored Temp/ClassicCleanup-20261006 before deletion; Git retains the previously committed versions. No player saves or signing/configuration files were removed.

## Verification

- Saved Bootstrap/Main: no missing scripts or serialized object references.
- Remaining serialized project assets: no references to deleted asset GUIDs.
- Core economy/save/account suite: 702 checks passed. Atomic bundle/persistence suite: 66 checks passed.
- Saved Classic startup/recovery/account flow: 79 checks passed. Native presentation/progression journey: 82 checks passed.
- Connected Android development APK: build succeeded with 0 errors and 30 warnings (deprecated compatibility APIs, optional plugin assemblies, ProBuilder shaders and app icon compression). Package and signing identity preserved; APK size 118,976,236 bytes.
- See References.txt, ../Classic-Production-Validation.txt and ../ClassicPresentation/Validation.txt for evidence.

## Deleted assets
- `Assets/Game/ImportedUI/Art/Folk Blue.svg`
- `Assets/Game/ImportedUI/Art/Folk Ivory.svg`
- `Assets/Game/ImportedUI/Art/Folk Red.svg`
- `Assets/Game/ImportedUI/Art/Folk Sand.svg`
- `Assets/Game/ImportedUI/Art/Icon Skills.png`
- `Assets/Game/ImportedUI/Art/Icon Stats.png`
- `Assets/Game/ImportedUI/Art/Icon Tap.png`
- `Assets/Game/ImportedUI/Art/Ivory Row Frame.svg`
- `Assets/Game/ImportedUI/Art/Ivory Sprig Gold.png`
- `Assets/Game/ImportedUI/Art/Ivory Sprig Gold.svg`
- `Assets/Game/ImportedUI/Art/Product aerial-shots.png`
- `Assets/Game/ImportedUI/Art/Product anar.png`
- `Assets/Game/ImportedUI/Art/Product chakri.png`
- `Assets/Game/ImportedUI/Art/Product cracker-strings.png`
- `Assets/Game/ImportedUI/Art/Product gift-boxes.png`
- `Assets/Game/ImportedUI/Art/Product phuljhari.png`
- `Assets/Game/ImportedUI/Art/Product rockets.png`
- `Assets/Game/ImportedUI/Art/Product small-crackers.png`
- `Assets/Game/ImportedUI/Baloo 2 Bold SDF.asset`
- `Assets/Game/ImportedUI/Home Screen.prefab`
- `Assets/Game/ImportedUI/Legacy Screen.prefab`
- `Assets/Game/ImportedUI/Nunito ExtraBold SDF.asset`
- `Assets/Game/ImportedUI/Nunito Medium SDF.asset`
- `Assets/Game/ImportedUI/Offline Screen.prefab`
- `Assets/Game/ImportedUI/Outfit SDF.asset`
- `Assets/Game/ImportedUI/Player Screen.prefab`
- `Assets/Game/ImportedUI/Prestige Screen.prefab`
- `Assets/Game/ImportedUI/Privacy Screen.prefab`
- `Assets/Game/ImportedUI/Product Detail Screen.prefab`
- `Assets/Game/ImportedUI/Products Screen.prefab`
- `Assets/Game/ImportedUI/Save Recovery Screen.prefab`
- `Assets/Game/ImportedUI/Settings Screen.prefab`
- `Assets/Game/ImportedUI/Skills Screen.prefab`
- `Assets/Game/ImportedUI/Statistics Screen.prefab`
- `Assets/Game/ImportedUI/Store Name Screen.prefab`
- `Assets/Game/ImportedUI/Tap Reward Material.mat`
- `Assets/Game/RedesignUI/Art/Paper.png`
- `Assets/Game/RedesignUI/Redesign Canvas.prefab`
- `Assets/Resources/Art/shopkeeper.png`
- `Assets/Resources/Art/stalls.png`
- `Assets/Scenes/HTML Importer.unity`
- `Assets/Scenes/Redesign UI Preview.unity`
- `Assets/Game/Editor/Classic/BuildClassicAndroid.cs`

## Retained shared assets

- `Assets/Game/ImportedUI/Art/Icon Products.png`
- `Assets/Game/RedesignUI/Product Sprites/rockets-0.asset`
- `Assets/Game/ImportedUI/Art/Icon Settings.png`
- `Assets/Game/ImportedUI/Art/Burst.png`
- `Assets/Resources/Art/cracker-strings.png`
- `Assets/Game/RedesignUI/Product Sprites/aerial-shots-2.asset`
- `Assets/Game/ImportedUI/Art/Icon Player.png`
- `Assets/Game/RedesignUI/Product Sprites/aerial-shots-0.asset`
- `Assets/Game/RedesignUI/Product Sprites/cracker-strings-2.asset`
- `Assets/Game/ImportedUI/Art/Bulk Order.png`
- `Assets/Game/RedesignUI/Product Sprites/phuljhari-0.asset`
- `Assets/Game/RedesignUI/Product Sprites/cracker-strings-0.asset`
- `Assets/Game/RedesignUI/Product Sprites/rockets-2.asset`
- `Assets/Game/RedesignUI/Art/Decoration crest.asset`
- `Assets/Game/RedesignUI/Product Sprites/phuljhari-1.asset`
- `Assets/Game/RedesignUI/Product Appearances.asset`
- `Assets/Game/RedesignUI/Product Sprites/anar-3.asset`
- `Assets/Game/RedesignUI/Product Sprites/chakri-3.asset`
- `Assets/Game/RedesignUI/Product Sprites/anar-2.asset`
- `Assets/Game/ImportedUI/Art/Stall Background.png`
- `Assets/Game/ImportedUI/Art/Icon Legacy.png`
- `Assets/Game/ImportedUI/Art/Folk Blue.png`
- `Assets/Game/RedesignUI/Product Sprites/rockets-1.asset`
- `Assets/Game/ImportedUI/Art/Icon Prestige.png`
- `Assets/Resources/Art/rockets.png`
- `Assets/Game/RedesignUI/Product Sprites/chakri-0.asset`
- `Assets/Game/RedesignUI/Product Sprites/small-crackers-3.asset`
- `Assets/Game/RedesignUI/Product Sprites/cracker-strings-1.asset`
- `Assets/Game/RedesignUI/Product Sprites/anar-0.asset`
- `Assets/Game/RedesignUI/Art/Decoration apron.asset`
- `Assets/Resources/Art/aerial-shots.png`
- `Assets/Game/ImportedUI/Art/Folk Ivory.png`
- `Assets/Game/ImportedUI/Art/Icon Lock.png`
- `Assets/Game/RedesignUI/Product Sprites/small-crackers-0.asset`
- `Assets/Game/RedesignUI/Art/Decoration bunting.asset`
- `Assets/Resources/Art/small-crackers.png`
- `Assets/Game/RedesignUI/Art/Decoration garland.asset`
- `Assets/Game/ImportedUI/Noto Sans Rupee SDF.asset`
- `Assets/Game/ImportedUI/Art/Coins.png`
- `Assets/Game/RedesignUI/Product Sprites/phuljhari-2.asset`
- `Assets/Resources/Art/anar.png`
- `Assets/Game/RedesignUI/Product Sprites/anar-1.asset`
- `Assets/Game/RedesignUI/Art/Shop Decorations.png`
- `Assets/Game/RedesignUI/Product Sprites/gift-boxes-2.asset`
- `Assets/Game/RedesignUI/Product Sprites/gift-boxes-3.asset`
- `Assets/Game/RedesignUI/Art/Decoration lantern.asset`
- `Assets/Game/RedesignUI/Product Sprites/aerial-shots-3.asset`
- `Assets/Game/RedesignUI/Product Sprites/small-crackers-2.asset`
- `Assets/Game/RedesignUI/Product Sprites/cracker-strings-3.asset`
- `Assets/Resources/Art/chakri.png`
- `Assets/Game/ImportedUI/Art/Ivory Row Frame.png`
- `Assets/Game/RedesignUI/Product Sprites/chakri-2.asset`
- `Assets/Game/RedesignUI/Product Sprites/chakri-1.asset`
- `Assets/Resources/Art/gift-boxes.png`
- `Assets/Game/ImportedUI/Art/Shopkeeper.png`
- `Assets/Game/ImportedUI/Art/Folk Sand.png`
- `Assets/Game/ImportedUI/Art/Helping Hands.png`
- `Assets/Game/RedesignUI/Product Sprites/phuljhari-3.asset`
- `Assets/Game/RedesignUI/Product Sprites/aerial-shots-1.asset`
- `Assets/Game/RedesignUI/Product Sprites/rockets-3.asset`
- `Assets/Game/RedesignUI/Product Sprites/small-crackers-1.asset`
- `Assets/Game/RedesignUI/Product Sprites/gift-boxes-1.asset`
- `Assets/Game/RedesignUI/Product Sprites/gift-boxes-0.asset`
- `Assets/Game/ImportedUI/Art/Folk Red.png`
- `Assets/Resources/Art/phuljhari.png`
