"""Stage only active entry points for native handoff validation; keep history untouched."""
from pathlib import Path
import tempfile,uuid,shutil,subprocess,json,hashlib
ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/'UI Builder/HTML'
DEST=Path(tempfile.gettempdir())/('BazaarHandoff-'+uuid.uuid4().hex)/'UI Builder/HTML'
DOC=ROOT/'Documentation/Implementation/UiUnification'
def ignore(folder,names):
    return [n for n in names if n in ['before','reviews','renders','renders-r2','revision-1','__pycache__']]
for name in ['Classic','Home','Shared']:
    shutil.copytree(SRC/name,DEST/name,ignore=ignore)
(DEST/'Classic/reviews').mkdir()
shutil.copy2(SRC/'Classic/reviews/device-1080x2200.html',DEST/'Classic/reviews/device-1080x2200.html')
for name,files in [('Loading',['index.html','assets']),('Tutorial',['index.html','classic-tutorial.css','tutorial.js','assets'])]:
    target=DEST/name;target.mkdir()
    for name2 in files:
        source=SRC/name/name2
        if source.is_dir():shutil.copytree(source,target/name2)
        else:shutil.copy2(source,target/name2)
reports={}
for name in ['Classic','Loading','Tutorial']:
    command=[__import__('sys').executable,str(ROOT/'.agents/skills/html-to-ugui/scripts/inspect_html_package.py'),str(DEST/name),'--project-root',str(ROOT),'--resource-root',str(DEST.parent),'--json']
    result=subprocess.run(command,capture_output=True,text=True,encoding='utf-8')
    report=json.loads(result.stdout);reports[name]=report
    (DOC/('inspection-'+name+'.json')).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(name,report['ok'],report['errors'])
    if not report['ok']:print(report['missingLocalResources'][:8])
manifest={'stagedRoot':str(DEST),'entries':['Classic/index.html','Loading/index.html','Tutorial/index.html'],'reviewOnly':['Classic/system/gallery.html','Classic/card-art-gallery.html','Classic/reviews/device-1080x2200.html'],'history':'Excluded from handoff; original archives unchanged','passed':all(r['ok'] for r in reports.values())}
(DOC/'handoff.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
assert manifest['passed'],'Active handoff package validation failed'
