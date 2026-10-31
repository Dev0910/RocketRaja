"""Compose the tutorial with the current Classic preview, keeping that source untouched."""
from pathlib import Path
import re,json
root=Path(__file__).resolve().parent
old=(root/'revision-1/index.html').read_text(encoding='utf-8')
classic=(root.parent/'Classic/index.html').read_text(encoding='utf-8')
review=re.search(r'<aside id="review".*?</aside>',old,re.S)[0].replace('id="review"','id="tutorial-review"').replace('id="review-status"','id="tutorial-review-status"')
review=review.replace('../../../Reference%20Images/Tutorial/ART-DIRECTION.md','system/gallery.html').replace('Art direction notes','Shared component library')
coach=re.search(r'<section id="coach".*?</section>',old,re.S)[0].replace('coach parchment','tutorial-coach')
coach=re.sub(r'<div class="coach-actions">.*?</div>','',coach,flags=re.S)
coach=coach.replace('aria-label="Shopkeeper guidance"','aria-label="Shopkeeper guidance" role="note" tabindex="0" data-action="DismissCoach"')
coach=coach.replace('<h2 id="coach-title">','<img class="tutorial-coach-frame" src="../Tutorial/assets/ivory-gold-frame.svg" alt="" aria-hidden="true"><h2 id="coach-title">',1)
modal=re.search(r'<dialog id="modal".*?</dialog>',old,re.S)[0].replace('class="parchment"','class="tutorial-dialog"')
meta=json.loads(re.search(r'<script id="unity-metadata".*?>(.*?)</script>',classic,re.S)[1])
tmeta=json.loads(re.search(r'<script id="unity-metadata".*?>(.*?)</script>',old,re.S)[1])
meta['screen']['identity']='shopkeeper-tutorial-on-current-classic'
meta['screen']['visualApprovalStatus']='candidate'
meta['screen']['latestUserCorrection']='31 October 2026: Classic HTML cleaned up against the current saved Unity Main, using one 360-unit canvas and native popup measurements/artwork. Composed tutorial shares the same presentation, scaled pointer coordinates, separate goal/help controls and existing earning/purchase transitions.'
meta['screen']['capabilityModules']=list(set(meta['screen'].get('capabilityModules',[])+['tutorialFlow']))
meta.setdefault('modules',{})['tutorialFlow']=tmeta['modules']['tutorialFlow']
meta['modules']['tutorialFlow']['presentation']='Compose existing Classic DOM/styles/art; add only coach/next-step content/highlights. Revision 1 presentation rejected, flow retained.'
meta['modules']['tutorialFlow']['automaticTraining']='On the first eligible cash threshold of ₹10, open Player and show the shopkeeper-anchored Salesmanship dialog matching Step 01. Trigger once during saving; never auto-purchase or re-enroll skipped/existing users.'
meta['modules']['tutorialFlow']['automaticProduct']='On first reaching at least ₹30 while saving for the first Phuljhari after Salesmanship, open Products and show the shopkeeper dialog matching Step 01. Show once; dismissal does not buy or repeat the transition. Preserve skipped/existing-user and save-error guards.'
meta['modules']['tutorialFlow']['automaticUpgrade']='Once Phuljhari is stocked and at least ₹20 is available for the first unowned upgrade, open Player and show the shopkeeper dialog matching Step 01; highlight Phuljhari Signature Breakthrough I. Show once as soon as affordable, replacing purchase feedback without requiring dismissal. Keep buying manual, respect save-error/skip/existing-user guards, and do not repeat after dismissal or purchase.'
meta['modules']['tutorialFlow']['earningResults']='Implemented native behavior: training, stock and upgrade results continue on a new earning tap or readiness for the next offer. Preview AdvanceEarnings applies explicit fixture income only; no independent economy or real save.'
meta['modules']['tutorialFlow']['automaticMilestones']='Use one-time, eligible milestone introductions with the same shopkeeper dialog as Step 01. Achievement readiness opens Player and highlights Achievements; first prestige readiness opens Player and highlights View prestige; completed prestige opens Legacy and highlights one free pull. Claim and prestige confirmations remain explicit player actions; no automatic claim, reset, paid draw or free-credit spending. Remove inline Talent hints. Keep actual card reveal before the one-time shopkeeper explanation of its exported benefit and permanence. Guard saving failures, skipped/existing users and exhausted free credits; never reroute after an introduction is dismissed.'
meta['modules']['tutorialFlow']['highlights']='Replace all tutorial outline highlights with the generated transparent white-glove pointing cursor from the user reference. Display the hand about 20% smaller and anchor its fingertip at the exact center of the active CTA, follow panel scroll and viewport resize, hide offscreen/unavailable targets and confirmations/reveals, and never intercept clicks. Keep normal keyboard focus styling.'
meta['modules']['tutorialFlow']['pointer']='Replace all tutorial outline highlights with the generated transparent white-glove pointing cursor from the user reference. Display the hand about 20% smaller and anchor its fingertip at the exact center of the active CTA, follow panel scroll and viewport resize, hide offscreen/unavailable targets and confirmations/reveals, and never intercept clicks. Keep normal keyboard focus styling.'
meta.setdefault('assets',{})['tutorialPointer']={'previewSource': '../Tutorial/assets/tutorial-pointing-hand.png', 'promptSource': 'assets/tutorial-pointing-hand.prompt.txt', 'generation': 'Built-in image_gen; user-provided classic white-glove cursor reference', 'format': 'PNG RGBA, 1254 x 1254, original alpha preserved', 'nativeStatus': 'Pending separately directed Editor-authored sprite/pointer presentation'}
meta['bindings']['tutorialPointer']={'targetIds': ['tutorial-pointer', 'tutorial-hand'], 'sourceOwner': 'Tutorial presentation / authored Unity hand sprite', 'sourceSymbol': 'Active tutorial CTA', 'valueType': 'Decorative pointer sprite', 'format': 'Generated white glove; smaller hand with fingertip at the CTA center; tracks scrolling and resizing; pointer-events none', 'runtimeStatus': 'new-approved', 'reason': 'User requested generated hand pointer replacing tutorial highlights on 30 October'}
for aid in ['ReplayTip']:
 meta['actions'][aid]=tmeta['actions'][aid]
meta['actions']['ReplayTip']['triggerIds']=['objective-help']
meta['bindings']['tutorialCoach']={**tmeta['bindings']['coach'],'targetIds':['coach-title','coach-copy']}
meta['modules']['tutorialFlow']['dialogue']='Compact button-free note; tap note or press Escape to collapse. Use actual controls or next-step card to act.'
meta['actions']['DismissCoach']={**tmeta['actions']['ReplayTip'],'triggerIds':['coach'],'entrySymbol':'Dismiss tutorial note','reason':'User requested removal of both popup buttons; dismissal never buys or opens a destination.'}
meta['conflicts'].extend(tmeta['conflicts'])
meta['qaScenarios'].extend(tmeta['qaScenarios'])
classic=re.sub(r'(<script id="unity-metadata"[^>]*>).*?(</script>)',lambda m:m[1]+json.dumps(meta,ensure_ascii=False,indent=2)+m[2],classic,flags=re.S)
classic=classic.replace('<head>','<head><base href="../Classic/">',1).replace('</head>','<link rel="stylesheet" href="../Tutorial/classic-tutorial.css?v=unity-sync-20261031"></head>',1)
classic=classic.replace('<body>','<body class="tutorial-preview">'+review,1)
classic=classic.replace('<div id="app-inner" class="app-inner">','<div id="app-inner" class="app-inner">'+coach+modal+'<output id="tutorial-status" aria-live="polite"></output>',1)
classic=classic.replace('</body>','<script src="../Tutorial/tutorial.js?v=unity-sync-20261031" data-handoff-role="preview-only"></script></body>')
classic=classic.replace('Rocket Raja · Classic UI preview','Rocket Raja · Tutorial on current game UI')
(root/'index.html').write_text(classic,encoding='utf-8')
print('Composed existing Classic source with tutorial additions.')
