"""Static checks for the authored SVG wireframes, not gameplay verification."""
import json
from pathlib import Path
import xml.etree.ElementTree as ET
from PIL import Image, ImageFont

root = Path(__file__).resolve().parent
manifest = json.loads((root / 'layout-checks.json').read_text(encoding='utf-8'))
issues = []
font_root = Path('C:/Windows/Fonts')
for state in manifest['states']:
    for label in state['labels']:
        font = ImageFont.truetype(str(font_root / ('segoeuib.ttf' if label['weight'] >= 600 else 'segoeui.ttf')), round(label['size'] * 10))
        width = font.getlength(label['text']) / 10
        if label['maxWidth'] and width > label['maxWidth'] + 1:
            issues.append(f"Text overflow {state['id']}: {label['text']}")
    for target in state['targets']:
        if min(target['w'], target['h']) < 48:
            issues.append(f"Small touch target {state['id']}: {target['id']}")
    with Image.open(root / (state['id'] + '.png')) as image:
        if image.size != (state['width'] * 2, state['height'] * 2):
            issues.append(f"PNG dimensions {state['id']}")

for source in root.glob('*.svg'):
    doc = ET.parse(source).getroot()
    ids = [node.attrib['id'] for node in doc.iter() if 'id' in node.attrib]
    if len(ids) != len(set(ids)):
        issues.append(f'Duplicate editable IDs: {source.name}')
for board in manifest['boards']:
    with Image.open(root / (board['name'] + '.png')) as image:
        if image.size != (board['width'], board['height']):
            issues.append(f"Board dimensions {board['name']}")
result = {'screen_variants': len(manifest['states']), 'review_boards': len(manifest['boards']), 'issues': issues}
(root / 'validation-result.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
print(json.dumps(result, indent=2))
if issues:
    raise SystemExit(1)
