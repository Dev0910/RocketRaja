import json, sys, pathlib, html
from PIL import Image, ImageDraw
root=pathlib.Path(__file__).resolve().parents[2]
folder=root/'Documentation/Implementation/UiUnification/native'/sys.argv[1]
files=sorted(f for f in folder.glob('*1080x2200.png') if '-bottom-' not in f.name and not f.name.startswith(('Safe-','Long-')))
w,h,cols=180,392,6
canvas=Image.new('RGB',(w*cols,h*((len(files)+cols-1)//cols)), '#203541')
draw=ImageDraw.Draw(canvas)
for i,file in enumerate(files):
    im=Image.open(file).convert('RGB'); im.thumbnail((w,366))
    x=i%cols*w;y=i//cols*h;canvas.paste(im,(x,y+22));draw.text((x+3,y+4),file.name.split('-1080')[0],fill='#fff4db')
canvas.save(folder/'contact.png')
body=''.join(f'<figure><a href="{f.name}"><img loading="lazy" src="{f.name}"></a><figcaption>{html.escape(f.stem)}</figcaption></figure>' for f in sorted(folder.glob('*.png')) if f.name!='contact.png')
(folder/'index.html').write_text('<!doctype html><meta charset="utf-8"><title>Native Bazaar review</title><style>body{background:#203541;color:#fff4db;font:16px system-ui}main{display:grid;grid-template-columns:repeat(auto-fill,minmax(180px,1fr));gap:16px}figure{margin:0}img{width:100%;max-height:470px;object-fit:contain}figcaption{padding:8px}</style><h1>Native Gouache Bazaar review</h1><main>'+body+'</main>',encoding='utf-8')
if (folder/'measurements.json').exists():
    data=json.loads((folder/'measurements.json').read_text(encoding='utf-8-sig'))
    issues=[]
    for screen in data:
        for t in screen['texts']:
            if t['preferred']>t['height']+2 and t['height']>0:
                issues.append({'route':screen['route'],'viewport':[screen['width'],screen['height']],**t})
    (folder/'text-fit-review.json').write_text(json.dumps(issues,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps({'screens':len(data),'textFitCandidates':len(issues),'routes':sorted(set(x['route'] for x in issues))}))
