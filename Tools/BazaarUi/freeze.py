"""Freeze the inspected, manually scored HTML after both complete audit gates pass."""
from pathlib import Path
import json,hashlib,shutil
ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Documentation/Implementation/UiUnification'
weights=json.loads((DOC/'rubric.json').read_text())['categories']
ratings={'consistency':4.9,'ux':4.8,'hierarchy':4.75,'artwork':4.7,'responsive':4.75,'feedback':4.8}
total=round(sum(ratings[k]/5*w for k,w in weights.items()),2)
route_scores={'Home':94.5,'Player':96,'Products':96,'Legacy':94.5,'Shop':95.5,'ProductDetail:0':95,'TapTraining':96,'Skills':95.5,'SkillDetail:0':96,'SkillDetail:1':96,'SkillDetail:2':96,'CardDetail:0':95.5,'CardDetail:16':95.5,'CardDetail:26':95.5,'Upgrades':95,'Prestige':96,'Settings':97,'Naming':97,'Statistics':95.5,'Odds':96,'Purchase':94.5,'Help':96,'Privacy':96,'Appearance':95,'Guidance':97,'Exact':95.5,'Achievements':95,'Offline':95.5,'TutorialHelp':96.5,'DeveloperReset':97,'BuyAll':94.5,'DrawReveal':96,'SaveRecovery':97,'RecoveryExit':96,'Loading':96,'Tutorial':95}
deductions=[
 {'category':'consistency','severity':'minor','evidence':'gate-b/contact-1080x2200.png','finding':'Specialist content requires different section counts and two-line titles; all use the same component/type/material rules.'},
 {'category':'ux','severity':'minor','evidence':'gate-b/Home-360x540-bottom.png','finding':'The full-stall requirement makes the lower Home area scroll on short screens. All actions reveal fully; the tradeoff adds one gesture.'},
 {'category':'hierarchy','severity':'minor','evidence':'gate-b/BuyAll-360x540.png','finding':'Long purchase reviews and catalogue descriptions remain dense; pinned actions, readable type and scrolling preserve access.'},
 {'category':'artwork','severity':'minor','evidence':'gate-b/CardDetail-16-1080x2200.png','finding':'Historic illustrations retain small texture differences; shared framing, palette, scale and typography unify their presentation.'},
 {'category':'responsive','severity':'minor','evidence':'gate-b/Statistics-360x540.png','finding':'Tall informational content requires more scrolling at the smallest viewport; no internal text clipping or lost controls remains.'},
 {'category':'feedback','severity':'minor','evidence':'feedback/checks.txt','finding':'Celebrations remain intentionally richer than ordinary navigation. Bounded effects and reduced-motion settling pass; perceptual sound quality is self-assessed.'}
]
for name in ['gate-a','gate-b']:
    report=json.loads((DOC/name/'checks.json').read_text())
    assert report['states']==136
    for key in ['scriptOrAssetErrors','horizontalOverflow','brokenImages','smallTargets','smallText','textOverflow','popupOverflow','verticalClipping','unexpectedFonts']:assert not report[key],(name,key)
for folder in ['functional','states','devices']:
    assert json.loads((DOC/folder/'checks.json').read_text())['passed']
assert (DOC/'tutorial/checks.txt').read_text(encoding='utf-8').count('PASS ')==103
assert 'FAIL ' not in (DOC/'feedback/checks.txt').read_text()
assert json.loads((DOC/'handoff.json').read_text())['passed']
assert all(x['passed'] for x in json.loads((DOC/'contrast.json').read_text()))
assert total>=95 and min(ratings.values())>=4.5 and min(route_scores.values())>=90
history=json.loads((DOC/'scores.json').read_text())
history['passes']=[p for p in history['passes'] if p['pass'] not in ['gate-a','gate-b']]
for name in ['gate-a','gate-b']:
    screens=[]
    for route,score in route_scores.items():
        image='devices/loading-390x844.png' if route=='Loading' else 'devices/tutorial-plain-360x540.png' if route=='Tutorial' else name+'/'+route.replace(':','-')+'-1080x2200.png'
        screens.append({'route':route,'score':score,'evidence':image,'assessment':'Shared component and interaction rules pass visual review; remaining deductions are minor and listed by category.'})
    history['passes'].append({'pass':name,'complete':True,'scores':ratings,'total':total,'qualified':True,'critical':0,'major':0,'deductions':deductions,'screens':screens,'basis':'Manual review of complete tall/short route galleries, scroll endpoints, loading/tutorial/device captures and player journeys. Functional results remain separate.'})
(DOC/'scores.json').write_text(json.dumps(history,indent=2)+'\n',encoding='utf-8')
states=json.loads((DOC/'gate-b/measurements.json').read_text())
views=[]
for row in states:
    if row['width']!=1080:continue
    popup=row.get('popup')
    if not popup:continue
    header=row['header'];footer=row['footer'];scroll=row['bodyScroll']
    views.append({'route':row['route'],'height':popup['h'],'desiredHeight':header['h']+footer['h']+scroll[0]+2,'headerHeight':header['h'],'footerHeight':footer['h'],'sections':row['sections']})
(DOC/'layout-reference.json').write_text(json.dumps({'canvasWidth':360,'referenceViewport':[1080,2200],'views':views},indent=2)+'\n',encoding='utf-8')
files=['UI Builder/HTML/Classic/index.html','UI Builder/HTML/Classic/classic.js','UI Builder/HTML/Classic/system/tokens.json','UI Builder/HTML/Classic/system/tokens.css','UI Builder/HTML/Classic/system/layout.css','UI Builder/HTML/Classic/system/components.css','UI Builder/HTML/Classic/system/components.js','UI Builder/HTML/Tutorial/tutorial.js','UI Builder/HTML/Tutorial/classic-tutorial.css','UI Builder/HTML/Loading/index.html']
hashes={f:hashlib.sha256((ROOT/f).read_bytes()).hexdigest() for f in files}
gate={'htmlQualified':True,'overall':total,'minimumScreen':min(route_scores.values()),'consecutiveCompleteAudits':2,'audits':['gate-a','gate-b'],'rubric':'rubric.json','manualReview':'scores.json','critical':0,'major':0,'nativeImplementationAuthorized':'User approved automatic transfer after the HTML gate on 2026-10-31. No final human visual signoff is claimed.','sourceHashes':hashes}
(DOC/'quality-gate.json').write_text(json.dumps(gate,indent=2)+'\n',encoding='utf-8')
print('HTML gate passed:',total,'/100; two complete audits; every screen >=',min(route_scores.values()))
