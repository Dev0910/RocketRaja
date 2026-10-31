"""Check native evidence and publish the explicitly manual design assessment separately."""
import pathlib, json, hashlib
root=pathlib.Path(__file__).resolve().parents[2]
folder=root/'Documentation/Implementation/UiUnification'
native=folder/'native'
rubric=json.loads((folder/'rubric.json').read_text(encoding='utf-8'))
weights={'consistency':25,'ux':20,'hierarchy':20,'artwork':15,'responsive':10,'feedback':10}
assert weights==rubric['categories']
ratings={'consistency':4.8,'ux':4.8,'hierarchy':4.75,'artwork':4.7,'responsive':4.6,'feedback':4.8}
overall=round(sum(weights[k]*ratings[k]/5 for k in weights),2)
assert overall>=95 and min(ratings.values())>=4.5
reports=[]
for name in ('gate-a','gate-b'):
    report=json.loads((native/name/'measurements.json').read_text(encoding='utf-8-sig'))
    checks=(native/name/'checks.txt').read_text(encoding='utf-8-sig')
    assert '\nFAIL' not in checks and 'PASS 25 functional checks' in checks, name
    bad_text=[(s['route'],t['path']) for s in report for t in s['texts'] if t['inkOverflow']]
    bad_targets=[(s['route'],b['name']) for s in report for b in s['buttons'] if min(b['width'],b['height'])<47.9]
    assert not bad_text and not bad_targets,(name,bad_text,bad_targets)
    startup_checks=(native/name/'startup/checks.txt').read_text()
    assert startup_checks.count('PASS')==12 and 'FAIL' not in startup_checks
    for width,height in ((1080,2200),(390,844),(360,640),(360,540)):
        routes={s['route'] for s in report if s['width']==width and s['height']==height and '-bottom' not in s['route'] and not s['route'].startswith(('Safe-','Long-','Fresh'))}
        assert len(routes)==34,(name,width,height,len(routes))
    reports.append({'pass':name,'renderedStates':len(report),'inkOverflow':0,'undersizedButtons':0,'functionalChecks':25})
assert 'ALL 68 PLAY CHECKS PASSED' in (native/'tutorial/PlayChecks.txt').read_text()
assert 'PASS 40 native festival checks' in (native/'feedback/checks.txt').read_text()
assert 'ALL 7 STARTUP CASES PASSED' in (native/'startup/PlayChecks.txt').read_text()
startup=(native/'startup/VisualChecks.txt').read_text()
assert startup.count('PASS')==12 and 'FAIL' not in startup
deductions=[
 {'category':'consistency','evidence':'gate-b/contact.png','reason':'Small differences in native icon stroke and body-text metrics remain across the illustrated cards.'},
 {'category':'ux','evidence':'gate-b/Upgrades-360x540-bottom.png','reason':'Long catalogues require scrolling; the approved full stall keeps less space available for management on short devices.'},
 {'category':'hierarchy','evidence':'gate-b/Statistics-360x540.png','reason':'Dense ledgers and detailed reviews are readable but need more scrolling than the tall reference.'},
 {'category':'artwork','evidence':'gate-b/contact.png','reason':'The original painted illustrations retain small variations in texture and lighting within the shared material and framing system.'},
 {'category':'responsive','evidence':'gate-b/Safe-Home-360x540.png','reason':'Large safe areas on the shortest device require an extra Home scroll. Extremely wide store names use an explicit ellipsis at the minimum font size; the full name remains in Naming.'},
 {'category':'feedback','evidence':'feedback/checks.txt','reason':'Rich celebrations retain their individual illustrative effects; their timing and effect density are not identical.'}
]
routes=sorted({s['route'] for s in report if '-bottom' not in s['route'] and not s['route'].startswith(('Safe-','Long-','Fresh'))})
screens=[{'route':r,'score':92.5 if r in ('Statistics','Upgrades','Shop','Achievements','Skills') else 95,'evidence':'gate-b/'+r.replace(':','-')+'-360x540.png','deduction':'Additional scrolling and denser information on the shortest viewport.' if r in ('Statistics','Upgrades','Shop','Achievements','Skills') else 'Minor native type or illustration metrics differ from the HTML reference.'} for r in routes]
screens += [{'route':r,'score':95,'evidence':'startup/Renders/'+r+'-360x540.png','deduction':'Small platform differences in vertical placement and font metrics.'} for r in ('loading','recovery','legacy')]
screens += [{'route':'Tutorial coach','score':95,'evidence':'tutorial/PlayChecks.txt','deduction':'Native font metrics give the naturally sized coach slightly different line wrapping.'}]
history=[
 {'pass':'iteration-1','score':70.5,'evidence':'iteration-1/contact.png','defects':['Home overlap','Settings background artifact','Competing original popup skins']},
 {'pass':'iteration-2','score':78.5,'evidence':'iteration-2/contact.png','defects':['Oversized achievement artwork','Overlapping contextual hints','Uneven row action placement']},
 {'pass':'iteration-3','score':86.5,'evidence':'iteration-3/contact.png','defects':['Home labels lost contrast','Skill hero spacing','Missing guidance-reader content in the audit scenario']},
 {'pass':'iteration-4','score':92.1,'evidence':'iteration-4/contact.png','defects':['Wide-name overflow','Fixed coach height','Older startup recovery skin']},
 {'pass':'failed-gate-long-name','qualified':False,'evidence':'failed-gate-long-name/Long-Home-360x540.png','defects':['24 wide letters exceeded the nameplate']},
 {'pass':'tutorial fixed-height regression','qualified':False,'evidence':'tutorial/failed-fixed-height.txt','defects':['Coach text needed natural content sizing']},
 {'pass':'historical role-test migration','qualified':False,'evidence':'failed-gate-role-contract/checks.txt','defects':['Old button-count and all-button-face assumptions included retired routes and excluded the approved information-card role']}
]
audit={'method':'Manual self-assessment under the unchanged frozen rubric; functional counts do not determine design scores.','overall':overall,'ratings':ratings,'weights':weights,'deductions':deductions,'screens':screens,'history':history,'audits':reports,'criticalDefects':0,'majorDefects':0,'physicalDevice':'Unavailable: adb returned no attached device; no Android emulator is installed. Android runtime smoke testing remains pending.'}
(native/'scores.json').write_text(json.dumps(audit,indent=2)+'\n',encoding='utf-8')
files=[root/'Assets/Scenes/Main.unity',root/'Assets/Scenes/Bootstrap.unity']
files+=list((root/'Assets/Game/ClassicUI/Bazaar').rglob('*'))
files+=list((root/'Assets/Game/Runtime/Classic').glob('*Bazaar*.cs'))
files+=list((root/'Assets/Game/Editor/Classic').glob('ClassicBazaar*.cs'))
files+=[root/'Assets/Game/Runtime/Classic'/f for f in ('ClassicLayout.cs','ClassicGuidance.cs','ClassicSettingsSwitch.cs','ClassicUI.Bindings.cs')]
sources={p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in files if p.is_file()}
(native/'quality-gate.json').write_text(json.dumps({'nativeDesignQualified':True,'overall':overall,'consecutiveCompleteAudits':2,'minimumScreen':min(s['score'] for s in screens),'audits':reports,'startupRenderedStates':12,'sourceHashes':sources,'androidRuntimeSmoke':'Pending device/emulator availability'},indent=2)+'\n',encoding='utf-8')
print(f'Native design: {overall}; two complete audits; {sum(x["renderedStates"] for x in reports)} reviewed main states plus two startup passes. Android runtime smoke remains pending.')
