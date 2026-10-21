"""Read existing Unity artwork and export browser copies. Unity is read-only."""
import json,re,shutil,math
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[4]
OUT=Path(__file__).resolve().parent
def copy(src,dest):
    p=OUT/'assets'/dest;p.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/src,p)
icons={'Bulk Order':'order','Helping Hands':'hands','Burst':'burst','Coins':'coins','Icon Legacy':'legacy','Icon Lock':'lock','Icon Player':'player','Icon Prestige':'prestige','Icon Products':'shelf','Icon Settings':'gear','Icon Skills':'skills','Icon Stats':'stats'}
for name,id in icons.items():copy('Assets/Game/ImportedUI/Art/'+name+'.png','icons/'+id+'.png')
for name,id in {'Folk Blue':'blue','Folk Ivory':'ivory','Folk Red':'red','Folk Sand':'sand','Ivory Row Frame':'row-frame'}.items():copy('Assets/Game/ImportedUI/Art/'+name+'.png','surfaces/'+id+'.png')
copy('Assets/Game/ClassicUI/Fonts/NotoSerif.ttf','fonts/card-serif.ttf')
copy('Assets/Game/Bootstrap/Fonts/Fraunces ExtraBold.ttf','fonts/brand.ttf')
copy('Assets/Resources/Fonts/Nunito-Medium.ttf','fonts/body.ttf')
copy('Assets/Resources/Fonts/Nunito-ExtraBold.ttf','fonts/bold.ttf')
copy('Assets/Resources/Fonts/Baloo2-Bold.ttf','fonts/heading.ttf')
copy('Assets/Game/Bootstrap/Art/Festival Stall.png','bootstrap/stall.png')
copy('Assets/Game/Bootstrap/Art/Google Mark.png','bootstrap/google.png')
for family in ['phuljhari','anar','chakri','rockets','small-crackers','cracker-strings','aerial-shots','gift-boxes']:
    im=Image.open(ROOT/f'Assets/Resources/Art/{family}.png').convert('RGBA')
    # External Sprite rectangles use imported texture coordinates. These source
    # textures use Unity's ToNearest NPOT resize (1254 -> 1024), not raw PNG pixels.
    meta=(ROOT/f'Assets/Resources/Art/{family}.png.meta').read_text()
    npot=int(re.search(r'nPOTScale: (\d+)',meta)[1]);maximum=int(re.search(r'maxTextureSize: (\d+)',meta)[1])
    imported=tuple(min(maximum,2**round(math.log2(n))) if npot==1 else min(maximum,n) for n in im.size)
    sx,sy=im.width/imported[0],im.height/imported[1]
    for look in range(1,4):
        s=(ROOT/f'Assets/Game/RedesignUI/Product Sprites/{family}-{look}.asset').read_text()
        m=re.search(r'm_Rect:\s*serializedVersion: \d+\s*x: ([\d.]+)\s*y: ([\d.]+)\s*width: ([\d.]+)\s*height: ([\d.]+)',s)
        x,y,w,h=map(float,m.groups());p=OUT/f'assets/looks/{family}-{look}.png';p.parent.mkdir(parents=True,exist_ok=True)
        im.crop((round(x*sx),round(im.height-(y+h)*sy),round((x+w)*sx),round(im.height-y*sy))).save(p)
im=Image.open(ROOT/'Assets/Game/RedesignUI/Art/Shop Decorations.png').convert('RGBA')
for name in ['garland','lantern','apron','bunting','crest']:
    s=(ROOT/f'Assets/Game/RedesignUI/Art/Decoration {name}.asset').read_text();m=re.search(r'm_Rect:\s*serializedVersion: \d+\s*x: ([\d.]+)\s*y: ([\d.]+)\s*width: ([\d.]+)\s*height: ([\d.]+)',s)
    x,y,w,h=map(float,m.groups());p=OUT/f'assets/decorations/{name}.png';p.parent.mkdir(parents=True,exist_ok=True);im.crop((round(x),round(im.height-y-h),round(x+w),round(im.height-y))).save(p)
nodes=json.loads((ROOT/'Temp/HtmlUnityParity/Main-nodes.json').read_text(encoding='utf-8'))
guide=next(c for n in nodes for c in n['components'] if c.get('m_EditorClassIdentifier','').endswith('ClassicGuidance'))
(OUT/'guide.js').write_text('// Read from the saved Unity scene; presentation data only.\nwindow.nativeGuideTopics='+json.dumps(guide['topics'],ensure_ascii=False)+';\n',encoding='utf-8')
print('Read-only export: exact native icons, surfaces, 24 progressed product images, 5 decorations, guide copy and card font.')
