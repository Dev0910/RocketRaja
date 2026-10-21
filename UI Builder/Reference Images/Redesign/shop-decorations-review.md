# Shop decoration reference — 2 October 2026

The user waived UI approval pauses. This records assistant inspection, not human approval.

Ordered artifacts: [editable fixed-target wireframe](../../Wireframes/Redesign/shop-stages-v1.svg), [generated RGBA atlas](shop-decorations-v1.png), [exact prompt](shop-decorations-prompt-v1.txt), [editable six-stage HTML](../../HTML/Redesign/shop-stages.html), then native Editor authoring.

The built-in imagegen tool generated one new transparent sprite sheet for this project. It contains marigold garland, hanging lantern, embroidered apron, pennants and an ornamental peacock/diya crest in the Gouache Storybook palette. The five motifs are inspected and reused cumulatively. The original tool output remains at its generated-images location; the project copy is `Assets/Game/RedesignUI/Art/Shop Decorations.png`. The reference and native source are byte-identical. No original shop or product texture was edited.

The generator did not follow the requested exact cell boundaries, so source-alpha analysis uses inspected partitions and stores five explicit regions in `UI Builder/HTML/Redesign/shop-stages.json`. This selects sprite rectangles without editing pixels. The file is RGBA8, 1536×1024, with 1,204,104 fully transparent pixels and zero alpha at all four corners. Colored hidden RGB around the motifs is not an opaque background. The Unity importer preserves non-power-of-two dimensions and disables mipmaps to retain the reviewed crop mapping.

The first HTML review made the aspect-preserved garland too small. Its wireframe placement was revised to span the sky just above the canopy, clear of the shop name and all controls. The six-stage HTML and every native stage were then inspected again. Lanterns frame the outside corners, the apron sits below the counter, and pennants/crest occupy the overhead sky. The progress panel follows gameplay actions, so it does not push the first purchase farther down. All six noninteractive Images and five layer roots are saved before runtime.

Final native evidence: [35 render/binding checks](../../../Documentation/Implementation/Unity-Shop-Art.txt) and [35 checks across two real Play Mode sessions](../../../Documentation/Implementation/Unity-Visual-Play.txt). These do not establish Android performance or human understanding.
