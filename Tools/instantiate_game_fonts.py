"""Instantiate the approved Baloo 2 / Nunito typography for Unity (fonttools 4.59.0)."""
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont

root = Path(__file__).resolve().parents[1]
for family, weight, filename in (
    ("Baloo2", 700, "Baloo2-Bold"),
    ("Nunito", 500, "Nunito-Medium"),
    ("Nunito", 800, "Nunito-ExtraBold"),
):
    font = TTFont(root / "SourceArt/Fonts" / f"{family}-variable.ttf")
    axes = {a.axisTag: weight if a.axisTag == "wght" else a.defaultValue for a in font["fvar"].axes}
    static = instantiateVariableFont(font, axes, inplace=False)
    assert "fvar" not in static, "Unity source must be a static font"
    assert 0x20B9 in static.getBestCmap(), f"{family} needs a rupee glyph"
    target = root / "Assets/Resources/Fonts" / f"{filename}.ttf"
    static.save(target)
    print(f"{filename}: weight {weight}; native rupee glyph; {target.stat().st_size} bytes")
