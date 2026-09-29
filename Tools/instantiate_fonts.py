"""Create redistributable static OFL font instances for Unity's font importer.
Requires fonttools 4.59.0. Source variable fonts are preserved in SourceArt/Fonts.
"""
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from pathlib import Path

root = Path(__file__).resolve().parents[1]
for name in ("Outfit", "NotoSans"):
    font = TTFont(root / "SourceArt" / "Fonts" / f"{name}-variable.ttf")
    axes = {axis.axisTag: (500 if axis.axisTag == "wght" else axis.defaultValue) for axis in font["fvar"].axes}
    result = instantiateVariableFont(font, axes, inplace=False)
    result.save(root / "Assets" / "Resources" / "Fonts" / f"{name}.ttf")
    print(name, axes, "rupee glyph:", 0x20B9 in result.getBestCmap())
