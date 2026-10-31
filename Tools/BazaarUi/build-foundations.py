"""One-time migration of existing native geometry; presentation uses shared tokens/components.
Re-run explicitly after editing the migration source list, never during game startup.
"""
from pathlib import Path
import re, json, hashlib, shutil

ROOT=Path(__file__).resolve().parents[2]
HTML=ROOT/'UI Builder/HTML/Classic'
OUT=HTML/'system'
DOC=ROOT/'Documentation/Implementation/UiUnification'
SOURCES=['../Home/home.css','../Home/gouache.css','../Shared/screens.css',
 '../Shared/folk-buttons.css','../Shared/player-folk.css','../Shared/ivory-bazaar.css',
 '../Shared/floating-navigation.css','../Shared/icons.css','classic.css','parity/native.css',
 'skills-popup.css','skill-hero.css','popup-refresh.css','popup-clearance.css',
 'achievements-ledger.css','currency.css','information.css','offline-timeline.css',
 'imported-buttons.css','unity-parity.css']

def split_top(text, delimiter):
    result=[]; start=0; depth=0; quote=''; escaped=False
    for i,c in enumerate(text):
        if escaped: escaped=False; continue
        if c=='\\': escaped=True; continue
        if quote:
            if c==quote: quote=''
        elif c in "\"'": quote=c
        elif c in '([': depth+=1
        elif c in ')]': depth-=1
        elif c==delimiter and depth==0: result.append(text[start:i]);start=i+1
    result.append(text[start:]);return result

def rules(text):
    text=re.sub(r'/\*.*?\*/','',text,flags=re.S)
    start=0;depth=0;quote='';escaped=False;head='';body_start=0
    for i,c in enumerate(text):
        if escaped: escaped=False;continue
        if c=='\\': escaped=True;continue
        if quote:
            if c==quote:quote=''
            continue
        if c in "\"'":quote=c;continue
        if c=='{':
            if depth==0:head=text[start:i].strip();body_start=i+1
            depth+=1
        elif c=='}':
            depth-=1
            if depth==0:
                yield head,text[body_start:i];start=i+1

def local_urls(text,source):
    def replace(m):
        value=m[1].strip().strip("\"'")
        if value.startswith(('data:','http:','https:','#')):return m[0]
        import os
        return "url('"+os.path.relpath((source.parent/value).resolve(),OUT).replace('\\','/')+"')"
    return re.sub(r'url\(([^)]+)\)',replace,text)

VISUAL={'white-space','text-overflow','color','background','background-color','background-image','background-size','background-position',
 'background-repeat','background-clip','background-origin','border','border-top','border-right','border-bottom','border-left',
 'border-width','border-color','border-style','border-radius','box-shadow','text-shadow','filter','text-decoration',
 'text-decoration-color','text-decoration-thickness','text-underline-offset','outline','outline-color','outline-offset',
 'outline-width','outline-style','-webkit-text-stroke','letter-spacing','font','font-size','font-family','font-weight',
 'font-style','font-variant','line-height','text-transform'}
POPUPS=re.compile(r'\.product-overview|\.product-progress|\.classic-income|\.income-|\.automatic-income|\.tap-income|\.total-income|\.classic-skill|\.skill-|\.skills-|\.modal|\.popup|\.info-card|\.full-card|\.full-talent|\.flip-|\.skill-hero|\.skill-effect|\.skills-quick|\.skills-reset|\.ledger-|\.afk-|\.appearance-|\.setting-switch|\.settings-row|\.settings-symbol|\.settings-link|\.exact-values|\.odds-row')

def migrate(text,source):
    result=[]
    for selector,body in rules(text):
        if selector.startswith(('@media','@supports','@container')):
            inner=migrate(body,source)
            if inner:result.append(selector+'{'+inner+'}')
            continue
        if selector.startswith('@'):continue
        selectors=[s for s in split_top(selector,',') if not POPUPS.search(s)]
        if not selectors:continue
        selector=','.join(selectors)
        declarations=[]
        for d in split_top(body,';'):
            if ':' not in d:continue
            name,value=d.split(':',1);name=name.strip();value=value.strip()
            if not name or not value:continue
            if name.startswith('--'):
                if name not in ['--native-world-height','--world-height','--world-expanded-height','--expanded-art-height','--native-card-height','--s','--scale','--viewport-scale','--logical-height']:continue
            if name in VISUAL or name.startswith(('border-', 'border-image')):
                if not ('.world' in selector and name in ['background','background-image','background-size','background-position','filter']):continue
            declarations.append(name+':'+local_urls(value,source))
        if declarations:result.append(selector+'{'+(';'.join(declarations))+'}')
    return '\n'.join(result)

def main():
    OUT.mkdir(exist_ok=True);DOC.mkdir(exist_ok=True)
    output=['/* Existing 360-unit production geometry, stripped of historical visual skins. */']
    for path in SOURCES:
        f=HTML/path
        output.append('/* Geometry source: '+path+' */\n'+migrate(f.read_text(encoding='utf-8'),f))
    (OUT/'layout.css').write_text('\n'.join(output)+'\n',encoding='utf-8')
    tokens=json.loads((OUT/'tokens.json').read_text(encoding='utf-8'))
    css=['/* Generated from tokens.json; presentation source shared with native authoring. */',':root{']
    for group,values in tokens.items():
        if not isinstance(values,dict):continue
        for key,value in values.items():
            if isinstance(value,(str,int,float)):css.append(' --bazaar-'+group+'-'+key+':'+str(value)+';')
    css.append('}')
    (OUT/'tokens.css').write_text('\n'.join(css)+'\n',encoding='utf-8')
    assets=[]
    for role,path in {'primary':'assets/buttons/folk-blue.png','secondary':'assets/buttons/folk-ivory.png','disabled':'assets/buttons/folk-sand.png','destructive':'assets/buttons/folk-red.png','paper':'parity/assets/unity/product-paper.png','heading':'parity/assets/fonts/heading.ttf','body':'parity/assets/fonts/body.ttf','bodyBold':'parity/assets/fonts/bold.ttf'}.items():
        f=HTML/path;assets.append({'role':role,'previewSource':path,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'treatment':'existing authored source; consistent shared role','slice':{'top':105,'right':92,'bottom':105,'left':92} if role in ['primary','secondary','disabled','destructive'] else None})
    (OUT/'artwork.json').write_text(json.dumps({'version':1,'direction':'Gouache Bazaar','assets':assets,'illustrations':'Existing stall, shopkeeper, products and Talent art keep recognizable identities. Shared framing, color roles and scale replace competing UI materials. Selective replacements require the same palette, painted finish, light direction and transparent padding.'},indent=2)+'\n',encoding='utf-8')
    print('Built geometry, token CSS and artwork manifest.')

if __name__=='__main__':main()
