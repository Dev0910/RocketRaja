/* Preview-only fixture transitions. RedesignEngine remains gameplay/save authority. */
(()=>{'use strict';
const $=id=>document.getElementById(id),names=['Phuljhari','Anar','Chakri','Rockets','Crackers','Strings','Aerial shots','Gift boxes'];
const moments=['welcome','saving','training','result','product','upgrade','achievement','prestige','talent'];
let state,modalAction=null,dragStart=null;
let pointerTargetId=null,pointerFrame=0;
const pointer=document.createElement('span');pointer.id='tutorial-pointer';pointer.className='tutorial-pointer';pointer.setAttribute('aria-hidden','true');pointer.hidden=true;pointer.dataset.tipX='0.205630';pointer.dataset.tipY='0.048644';
const hand=document.createElement('img');hand.id='tutorial-hand';hand.src='../Tutorial/assets/tutorial-pointing-hand.png';hand.alt='';hand.draggable=false;hand.onload=schedulePointer;pointer.append(hand);$('app-inner').append(pointer);
function schedulePointer(){if(!pointerFrame)pointerFrame=requestAnimationFrame(updatePointerFrame);}
function updatePointerFrame(){
 pointerFrame=0;positionPointer();
 // Panel and row entrances use Web Animations, which do not emit CSS animationend.
 for(let el=$(pointerTargetId);el&&el!==$('app').parentElement;el=el.parentElement){
  if(el.getAnimations().some(a=>a.playState==='running'&&Number.isFinite(a.effect?.getComputedTiming().endTime))){
   pointerFrame=requestAnimationFrame(updatePointerFrame);break;
  }
 }
}
function positionPointer(){
 pointer.hidden=true;
 const s=state,target=$(pointerTargetId),app=$('app-inner');
 if(!hand.complete||!hand.naturalWidth||!s||s.error||s.skipped||s.existing||!target||target.disabled||target.closest('[hidden],[inert]')||$('modal').open||window.classicPreview?.state.stack.length)return;
 const visual=pointerTargetId==='stall-tap'?document.querySelector('.world .tap-hint'):target;
 const factor=window.classicViewport?.scale()||1;
 const rect=element=>{const b=element.getBoundingClientRect();return {left:b.left/factor,top:b.top/factor,right:b.right/factor,bottom:b.bottom/factor,width:b.width/factor,height:b.height/factor};};
 const r=rect(visual),a=rect(app),scroll=target.closest('.scroll-body'),v=scroll&&rect(scroll);
 if(!r.width||!r.height||v&&(r.bottom<=v.top||r.top>=v.bottom))return;
 const bottom=Math.min(a.bottom,rect(document.querySelector('.navigation')).top)-4;
 const tipX=Number(pointer.dataset.tipX||.18),tipY=Number(pointer.dataset.tipY||.04);
 const x=r.left+r.width*.5,y=r.top+r.height*.5;
 if(y<a.top+4||y>bottom||v&&(y<v.top||y>v.bottom))return;
 let size=Math.min(64,Math.max(44,a.width*.135));
 const above=bottom-y<(1-tipY)*size;
 // Keep the fingertip at the exact center; shrink the artwork if an edge needs clearance.
 size=Math.min(size,(x-a.left-4)/(above?1-tipX:tipX),(a.right-4-x)/(above?tipX:1-tipX),(above?y-a.top-4:bottom-y)/(1-tipY));
 if(size<=0)return;
 pointer.style.left=(x-a.left)+'px';pointer.style.top=(y-a.top)+'px';
 pointer.style.setProperty('--hand-size',size+'px');pointer.style.setProperty('--hand-rotation',above?'180deg':'0deg');
 pointer.dataset.target=pointerTargetId;pointer.dataset.side=above?'above':'below';pointer.hidden=false;
}
window.addEventListener('resize',schedulePointer);
document.addEventListener('scroll',schedulePointer,{capture:true,passive:true});
document.addEventListener('animationend',schedulePointer,true);
document.addEventListener('transitionend',schedulePointer,true);
document.addEventListener('click',schedulePointer);
document.addEventListener('keydown',schedulePointer);

function seed(step){return {step,view:({training:'Player',product:'Products',upgrade:'Player',talent:'Legacy'})[step]||'Home',cash:({saving:6,training:10,product:30,upgrade:20,achievement:18000,prestige:1260000,talent:30})[step]||0,tap:['result','product','upgrade','achievement'].includes(step)?2.4:step==='prestige'?1650:step==='talent'?31.8:1,trained:['result','product','upgrade','achievement','prestige'].includes(step),products:step==='upgrade'?1:step==='achievement'?4:step==='prestige'?8:0,income:step==='upgrade'?.85:step==='achievement'?22.42:step==='prestige'?11650:0,credits:step==='talent'?3:0,prestiges:step==='talent'?1:0,kk:0,claimed:false,upgrade:false,card:false,bubble:['welcome','training','result','product','upgrade','achievement','prestige','talent'].includes(step),achievementPrompted:false,prestigePrompted:false,talentPrompted:false,cardExplained:false,productPrompted:step==='product',upgradePrompted:step==='upgrade',skipped:false,existing:false,error:false};}
const number=n=>n>=1e6?(n/1e6).toFixed(2).replace(/0+$/,'').replace(/\.$/,'')+'M':n>=1e3?(n/1e3).toFixed(2).replace(/0+$/,'').replace(/\.$/,'')+'K':n.toFixed(2).replace(/\.00$/,'');
const cash=n=>'₹'+number(n);
const say=s=>{$('tutorial-review-status').textContent=s;};
function reset(step='welcome'){if(window.classicPreview)window.classicPreview.state.reveal=null;state=seed(moments.includes(step)?step:'welcome');$('modal').close();render();}
function show(title,copy,label='Got it',callback=null){$('modal-title').textContent=title;$('modal-copy').innerHTML=copy;$('modal-ok').textContent=label;modalAction=callback;pointer.hidden=true;$('modal').showModal();}
function setView(view){state.view=state.view===view?'Home':view;render();}
function render(){
 const aliases={};const s=state, cp=window.classicPreview, f=structuredClone(window.classicFixtures.new);
 const canContinue=!s.error&&!s.skipped&&!s.existing&&!$('modal').open&&!cp.state.reveal;
 // Result notes do not hold an affordable next action behind a dismissal.
 if(canContinue&&s.step==='result'&&s.cash>=30){s.step='product';s.bubble=false;}
 if(canContinue&&s.feedback?.earning&&s.step==='upgrade'&&!s.upgrade&&s.cash>=20)s.feedback=null;
 if(canContinue&&s.step==='product'&&s.trained&&!s.products&&s.cash>=30&&!s.productPrompted){s.productPrompted=true;s.view='Products';s.bubble=true;}
 // Offer the first useful upgrade once it is affordable; purchase feedback finishes first.
 if(s.step==='upgrade'&&s.prestiges===0&&s.products>0&&s.cash>=20&&!s.upgrade&&!s.upgradePrompted&&!s.feedback&&!s.error&&!s.skipped&&!s.existing){
  s.upgradePrompted=true;s.view='Player';s.bubble=true;
 }
 // Route each remaining introduction once, keeping confirmations and reveals ahead of coaching.
 if(!s.error&&!s.skipped&&!s.existing&&!s.feedback&&!$('modal').open&&!cp.state.reveal){
  let milestone=null;
  if(s.step==='achievement'&&s.products>=4&&!s.claimed&&!s.achievementPrompted)milestone=['achievementPrompted','Player'];
  else if(s.step==='prestige'&&s.prestiges===0&&!s.prestigePrompted)milestone=['prestigePrompted','Player'];
  else if(s.step==='talent'&&s.prestiges>0&&s.credits>0&&!s.card&&!s.talentPrompted)milestone=['talentPrompted','Legacy'];
  if(milestone){s[milestone[0]]=true;s.view=milestone[1];s.bubble=true;}
 }
 const amount=n=>({value:n,shortText:number(n),exact:String(n)});
 f.cash=amount(s.cash);f.tap=amount(s.tap);f.normal=amount(s.income);f.prestiges=String(s.prestiges);f.credits=String(s.credits);f.kk=String(s.kk);
 f.owned=s.upgrade?['PP-PHU-01']:[];f.levels=Array.from({length:8},(_,i)=>i<s.products?1:0);f.prestige.Eligible=s.step==='prestige';
 window.classicFixtures.tutorial=f;
 const native=cp.state;native.fixture='tutorial';native.levels=[...f.levels];native.credits=s.credits;native.kk=s.kk;
 native.skillStates=['locked','locked','locked'];native.skillTimers=[null,null,null];
 native.cardLevels=Array.from({length:28},(_,i)=>i===0&&s.card?(s.cardLevel||1):0);native.bought=new Set(f.owned);native.learned=new Set(['buy-first','rebuild','tap-counter','product-levels','buy-all','catalogue-filters','comebacks','prestige','cash-and-talent','talent-free','talent-duplicates','second-gift','return-play','skill-basics']);
 cp.condition(s.error?'save-error':'normal');if(native.tab!==s.view||native.stack.length)cp.navigate(s.view);
 $('app').dataset.tutorialReady='true';$('app').dataset.tutorialStep=s.step;$('state-picker').value=s.step;
 $('cash-value').textContent=cash(s.cash);
 const text=s.feedback?[s.feedback.title,s.feedback.copy,'']:s.step==='training'?['You’ve earned ₹10!','Train Salesmanship below for ₹10. Each tap will earn ₹2.40 instead of ₹1.','']:s.step==='result'?['Your taps are stronger!',`You now earn ${cash(s.tap)} per tap, up from ₹1. Next, stock Phuljhari.`,'Got it']:s.step==='product'?['You’ve earned ₹30!','Stock Phuljhari for ₹30. It earns ₹0.85 every second.','Got it']:s.step==='upgrade'?['Your first upgrade!','Spend ₹20 on Phuljhari’s first upgrade below to double its income: ₹0.85 → ₹1.70/sec.','Got it']:s.step==='achievement'?['Your first achievement!','You stocked 4 products! Open Achievements below to collect 5 Kaju Katli.','Show reward']:s.step==='prestige'?['Ready for a brighter start?','Open View prestige below to review your Fame, free pulls and what resets. Confirm when you’re ready.','View prestige']:s.step==='talent'?['A little Talent goes a long way',`Your first prestige earned ${s.credits} free pulls. Follow the hand and use a 1× pull to discover a permanent Talent card.`,'Got it']:['Let’s open your stall!','Tap the counter to earn rupees. Then we’ll improve your sales pitch.','Let’s start'];
 const coachChanged=$('coach-title').textContent!==text[0]||$('coach-copy').textContent!==text[1];$('coach-title').textContent=text[0];$('coach-copy').textContent=text[1];$('coach').hidden=!(s.bubble&&(s.view==='Home'||['training','upgrade','achievement','prestige'].includes(s.step)&&s.view==='Player'||s.step==='product'&&s.view==='Products'||s.step==='talent'&&s.view==='Legacy')&&!s.skipped&&!s.existing);

 if(coachChanged&&!$('coach').hidden)window.classicMotion?.enter($('coach'),6);
 let goal=s.trained?['NEXT STEP','Stock Phuljhari · ₹30']:['NEXT STEP',`Train Salesmanship · ${cash(s.cash)} / ₹10`];
 if(s.products)goal=['NEXT STEP','Improve Phuljhari · ₹20'];if(s.upgrade||s.skipped||s.existing)goal=['NEXT DISPLAY','Grow your stall · explore Products'];
 if(s.step==='achievement'&&!s.claimed)goal=['REWARD READY','Claim Open for business · +5 KK'];
 if(s.step==='prestige')goal=['PRESTIGE READY','View your first prestige'];
 if(s.step==='talent')goal=s.card?['NEXT DISPLAY','Your Talent collection · Legacy']:['NEXT STEP',`Use a free pull · ${s.credits} available`];
 const objective=document.querySelector('.home-next'),next=objective.querySelector('.home-next-action');next.dataset.action='OpenObjective';aliases[next.id]='objective-route';next.id='objective-route';next.querySelector('span').innerHTML='<small>'+goal[0]+'</small><strong>'+goal[1]+'</strong>';objective.classList.add('tutorial-objective');
 const help=objective.querySelector('.home-next-help');help.id='objective-help';help.dataset.action='ReplayTip';help.classList.add('tutorial-help');help.ariaLabel='Repeat current instruction';help.textContent='?';help.hidden=s.skipped||s.existing;
 const bind=(sel,id,action,disabled)=>{const el=document.querySelector(sel);if(!el)return null;aliases[el.id]=id;el.id=id;el.dataset.tutorialAction=action;el.disabled=disabled;return el;};
 bind('#panel-Player [data-action="BuyLevels"][data-param="-1"]','buy-training','BuyTraining',s.error||s.trained||s.cash<10);
 bind('#panel-Products [data-action="BuyLevels"][data-param="0"]','buy-product','BuyProduct',s.error||s.products>0||s.cash<30);
 bind('#panel-Player [data-action="BuyUpgrade"][data-param="PP-PHU-01"]','buy-upgrade','BuyUpgrade',s.error||s.upgrade||s.cash<20);
 bind('#panel-Legacy [data-action="Draw"][data-param="1"]','free-pull','FreePull',s.error||s.credits<1||s.prestiges<1);
 bind('#panel-Player [data-action="Navigate"][data-param="Achievements"]','achievement-entry','Achievements',false);
 bind('#panel-Player [data-action="Navigate"][data-param="Prestige"]','prestige-entry','Prestige',false);
 const training=$('buy-training');if(training){training.innerHTML=s.trained?'Trained':'₹10<br>Buy ×1';const row=training.closest('.sheet-row');if(row){row.querySelector('h2').textContent='Salesmanship · Level '+(s.trained?1:0);row.querySelector('p').textContent=cash(s.tap)+' per tap';}}
 document.querySelectorAll('.tutorial-target').forEach(el=>el.classList.remove('tutorial-target'));
 pointerTargetId=null;pointer.hidden=true;
 const id=({welcome:'stall-tap',saving:'stall-tap',training:'buy-training',product:'buy-product',upgrade:s.upgrade?null:'buy-upgrade',achievement:s.claimed?null:'achievement-entry',prestige:s.prestiges?null:'prestige-entry',talent:s.card||s.credits<1?null:'free-pull'})[s.step];
 if(id&&!s.skipped&&!s.existing){const target=$(id);if(target){target.classList.add('tutorial-target');pointerTargetId=id;if(s.view!=='Home'&&target.closest('#panel-'+s.view)){
 requestAnimationFrame(()=>{const scroll=$('scroll-'+s.view),r=target.getBoundingClientRect(),v=scroll.getBoundingClientRect();if(r.top<v.top+8)scroll.scrollTop-=(v.top-r.top+8)/(window.classicViewport?.scale()||1);else if(r.bottom>v.bottom-8)scroll.scrollTop+=(r.bottom-v.bottom+14)/(window.classicViewport?.scale()||1);schedulePointer();});}}}
 schedulePointer();
 $('tutorial-status').textContent=s.error?'Saving unavailable. Your tutorial will wait.':'';
 const m=JSON.parse($('unity-metadata').textContent);m.screen.identity='shopkeeper-tutorial-on-current-classic';for(const a of Object.values(m.actions))a.triggerIds=(a.triggerIds||[]).map(id=>aliases[id]||id);for(const q of m.qaScenarios)if(q&&typeof q==='object')q.relatedActionIds=(q.relatedActionIds||[]).map(id=>({BuyTraining:'BuyLevels',BuyProduct:'BuyLevels',FreePull:'Draw',ContinueTutorial:'DismissCoach'})[id]||id);for(const [key,id]of [['ModalConfirm','modal-ok'],['ModalDismiss','modal-cancel']])m.actions[key]={...m.actions.ReplayTip,triggerIds:[id],runtimeStatus:'new-approved',approvalStatus:'approved',entrySymbol:'Tutorial confirmation callback; underlying economic engine remains authority',reason:'Focused preview confirmation for the approved tutorial flow'};m.bindings.tutorialPointer={targetIds:['tutorial-pointer','tutorial-hand'],sourceOwner:'Tutorial presentation / authored Unity hand sprite',sourceSymbol:'Active tutorial CTA',valueType:'Decorative pointer sprite',format:'Smaller transparent white glove points at the exact CTA center; follows scrolling and resizing; never intercepts input',visibilityRule:'Relevant visible enabled CTA; hidden during confirmations, reveal, recovery, skipped/existing users',runtimeStatus:'new-approved',reason:'30 October user requested generated pointing hand instead of tutorial highlights'};m.bindings.tutorialCoach={targetIds:['coach-title','coach-copy'],sourceOwner:'New tutorial coordinator; ClassicUI',sourceSymbol:'Durable tutorial step; actual purchase result',valueType:'editable coach copy',format:'Current native fonts/buttons; live quoted values',updateTrigger:'Successful action/route',visibilityRule:'Enrolled new user, relevant safe state',interactabilityRule:'Session may mutate',fallback:'Retain objective during recovery',runtimeStatus:'new-approved',reason:'Flow retained; UI correction reuses actual Classic presentation',evidence:['Game Design/Shopkeeper Tutorial Draft.md']};$('unity-metadata').textContent=JSON.stringify(m,null,2);
}

function coachFeedback(title,copy,earning=false){state.feedback={title,copy,earning};state.view='Home';state.bubble=true;render();}
function achievements(){state.bubble=false;$('coach').hidden=true;show('Open for business',`<p>Stock 4 different products together.</p><p class="modal-number">+5 Kaju Katli</p><p>${state.claimed?'Already claimed':state.products>=4?'4 / 4 products · reward ready':'Stock four products to earn this reward.'}</p>`,state.claimed?'Claimed':'Claim reward',origin=>{if(state.error||state.claimed||state.products<4)return say('Claim blocked: unavailable, incomplete or already claimed.');state.claimed=true;state.kk+=5;coachFeedback('Reward collected!','You received 5 Kaju Katli. Keep growing your stall for more achievements.');say('Preview reward claimed once. Fixture wallet: '+state.kk+' KK.');window.classicProgressFeel?.reward(origin,5);});$('modal-ok').disabled=state.error||state.claimed||state.products<4;}
function prestige(){state.bubble=false;$('coach').hidden=true;show('A brighter beginning','<p>Gain permanent Fame and <strong>3 free Talent pulls</strong>.</p><p class="modal-detail"><strong>Resets:</strong> cash, product levels, Salesmanship, run upgrades and skills.<br><strong>Keeps:</strong> Fame, Talent cards, Kaju Katli and achievement claims.</p>','Prestige',()=>{if(state.error||state.step!=='prestige')return say('Prestige unavailable.');const kept={kk:state.kk,claimed:state.claimed,skipped:state.skipped,existing:state.existing,view:state.skipped||state.existing?'Home':'Legacy'};state=Object.assign(seed('talent'),kept);render();window.classicProgressFeel?.prestige();say('Fixture prestige complete. Three free credits; no pull made automatically.');});$('modal-ok').disabled=state.error||state.step!=='prestige';}
function route(){const s=state;if(s.step==='achievement'&&!s.claimed)return achievements();if(s.step==='prestige')return prestige();if(s.step==='talent'){s.view='Legacy';render();return;}s.view=s.skipped||s.existing?'Products':!s.trained?'Player':!s.products?'Products':'Player';if(!s.trained&&s.cash>=10)s.step='training';if(s.trained&&!s.products)s.step='product';if(s.products&&!s.upgrade)s.step='upgrade';s.bubble=false;render();}
function action(id,param){const s=state;
 if(id==='ReplayTutorial'){reset();return;}
 if(id==='ResumeTutorial'){s.view='Home';s.bubble=true;render();return;}
 if(id==='AdvanceEarnings'){if(s.error)return;const seconds=Number(param);if(!Number.isFinite(seconds)||seconds<0)return;s.cash+=s.income*seconds;render();return;}
 if(id==='Tap'){if(s.error||window.classicPreview.state.stack.length||$('modal').open)return;s.feedback=null;s.bubble=false;if(s.step==='result')s.step='product';s.cash+=s.tap;if(s.step==='welcome'){s.step='saving';s.bubble=false;}if(s.step==='saving'&&s.prestiges===0&&s.cash>=10&&!s.trained&&!s.skipped&&!s.existing){s.step='training';s.view='Player';s.bubble=true;}if(s.step==='product'&&s.prestiges===0&&s.trained&&s.cash>=30&&!s.products&&!s.productPrompted&&!s.skipped&&!s.existing){s.productPrompted=true;s.view='Products';s.bubble=true;}render();const target=param&&typeof param==='object'?{isConnected:true,getBoundingClientRect:()=>({left:param.clientX,top:param.clientY,width:1,height:1})}:document.querySelector('.world .tap-hint');window.classicMotion?.tap(target,cash(s.tap));return;}
 if(id==='Tab')return setView(param);if(id==='Home'){s.view='Home';render();return;}if(id==='ReplayTip'){s.bubble=true;render();return;}
 if(id==='SkipTutorial'){s.skipped=true;s.bubble=false;render();say('Tutorial skipped in this preview; all navigation remains available.');return;}
 if(id==='ContinueTutorial'){s.bubble=false;if(s.step==='welcome')s.step='saving';else if(s.step==='result'){s.step='product';s.view='Home';}else if(s.step==='achievement')return achievements();else if(s.step==='prestige')return prestige();render();return;}
 if(id==='OpenObjective')return route();if(id==='Achievements')return achievements();if(id==='Prestige')return prestige();
 if(id==='BuyTraining'){if(s.error||s.trained||s.cash<10)return;s.cash-=10;s.trained=true;s.tap=2.4;s.step='result';s.view='Home';s.bubble=true;render();say('Fixture purchase: ₹10. Tap income ₹1 → ₹2.40.');window.classicMotion?.success(document.querySelector('.world .tap-hint'));return;}
 if(id==='BuyProduct'){if(s.error||s.products||s.cash<30)return;s.cash-=30;s.products=1;s.income=.85;s.step='upgrade';coachFeedback('Your first shelf is stocked!','Phuljhari earns ₹0.85 every second. Keep tapping while your stall earns automatically.',true);window.classicMotion?.success($('display-phuljhari').querySelector('.product-art'),'celebrate');window.classicProgressFeel?.levels(0,0,1);return;}
 if(id==='BuyUpgrade'){if(s.error||s.upgrade||s.cash<20)return;s.cash-=20;s.upgrade=true;s.income=s.products?1.7:0;coachFeedback('A stronger little stall!',s.products?'Automatic sales improved from ₹0.85 to ₹1.70/sec. Your other tabs are always available.':'This upgrade boosts Phuljhari once you stock it.',true);window.classicMotion?.success($('display-phuljhari').querySelector('.product-art'));return;}
 if(id==='FreePull'){if(s.error||!s.credits||!s.prestiges)return;s.credits--;s.card=true;s.bubble=false;s.cardLevel=(s.cardLevel||0)+1;render();const result={card:0,before:s.cardLevel-1,after:s.cardLevel};window.classicPreview.state.reveal={...result,results:[result],index:0};window.classicPreview.navigate('DrawReveal');say('One fixture free credit used. No KK spent.');return;}
 if(id==='FinishTalent'){
  const cp=window.classicPreview,layer=document.querySelector('.modal-layer[data-screen="DrawReveal"]');
  if(!s.card||!cp.state.reveal||!layer?.classList.contains('flip-settled'))return;
  cp.state.reveal=null;
  if(s.cardExplained){s.view='Legacy';s.bubble=false;render();return;}
  s.cardExplained=true;
  const level=s.cardLevel||1,examples=window.classicFixtures.tutorial.cards[0].examples,after=examples[Math.min(64,level)].shortText;
  const copy=level>1?`Phuljhari income improved from ×${examples[Math.min(64,level-1)].shortText} to ×${after}. This Talent stays after prestige.`:`Phuljhari Showcase boosts Phuljhari income ×${after}. Your Talent card stays after prestige.`;
  coachFeedback(level>1?'Your Talent grew!':'Your first Talent card!',copy);return;
 }
 if(id==='Stats')return show('Your stall',`<p>Automatic sales: ${cash(s.income)}/sec<br>Per tap: ${cash(s.tap)}<br>Prestiges: ${s.prestiges}<br>Kaju Katli: ${s.kk}</p>`);
 if(id==='Settings')return show('Tutorial help','<p>Guidance highlights useful controls while leaving navigation available. Use ? on the next-step card to repeat an instruction.</p>');
 if(id==='InspectSkill')return show('Skills','<p>Festival Rush, Helping Hands and Bulk Order unlock through cash upgrades. The tutorial introduces a skill when it is owned and ready.</p>');
 if(id==='InspectProduct')return show('Anar','<p>Stock Phuljhari to open the next product display. This focused preview demonstrates the first product purchase.</p>');
}
function dismissCoach(){state.feedback=null;state.bubble=false;if(state.step==='welcome')state.step='saving';else if(state.step==='result')state.step='product';render();}
$('coach').addEventListener('click',e=>{e.stopPropagation();dismissCoach();});$('coach').addEventListener('keydown',e=>{if(['Enter',' ','Escape'].includes(e.key)){e.preventDefault();e.stopPropagation();dismissCoach();}});
// Capture fixture economic/tutorial events before Classic's existing preview listener.
document.addEventListener('click',e=>{const b=e.target.closest('button');if(!b||b.disabled)return;let a=b.dataset.tutorialAction;let p=b.dataset.param||'';
if(!a&&b.closest('#tutorial-review'))return;
if(!a&&b.closest('#coach'))a=b.dataset.action;
if(!a&&b.id==='objective-route')a='OpenObjective';if(!a&&b.id==='objective-help')a='ReplayTip';
if(!a&&b.dataset.action==='Reveal'&&p==='continue'&&b.closest('.modal-layer[data-screen="DrawReveal"]'))a='FinishTalent';
if(!a&&b.dataset.action==='Tab')a=p==='Home'?'Home':'Tab';
if(a){e.preventDefault();e.stopImmediatePropagation();action(a,p);}},true);
const tap=$('stall-tap');tap.addEventListener('pointerdown',e=>{e.stopImmediatePropagation();dragStart={x:e.clientX,y:e.clientY};},true);tap.addEventListener('pointerup',e=>{e.stopImmediatePropagation();if(dragStart&&Math.hypot(e.clientX-dragStart.x,e.clientY-dragStart.y)<8*(window.classicViewport?.scale()||1))action('Tap',{clientX:e.clientX,clientY:e.clientY});dragStart=null;},true);tap.addEventListener('pointercancel',()=>dragStart=null);tap.addEventListener('keydown',e=>{if(['Enter',' '].includes(e.key)){e.preventDefault();e.stopImmediatePropagation();action('Tap');}},true);
$('modal-ok').addEventListener('click',()=>{if($('modal-ok').disabled)return;const fn=modalAction,origin=$('modal-ok').getBoundingClientRect();modalAction=null;$('modal').close();fn?.(origin);});$('modal-cancel').addEventListener('click',()=>{$('modal').close();modalAction=null;});$('modal').addEventListener('close',()=>{$('modal-ok').disabled=false;schedulePointer();});
$('state-picker').onchange=e=>reset(e.target.value);$('review-replay').onclick=()=>reset();$('review-achievement').onclick=()=>reset('achievement');$('review-prestige').onclick=()=>reset('prestige');$('review-save-error').onclick=()=>{state.error=!state.error;render();};$('review-existing').onclick=()=>{reset('upgrade');state.existing=true;render();};
window.classicPreview.reset('new',false);const q=new URLSearchParams(location.search);reset(q.get('step')||q.get('state')||'welcome');
window.tutorialPreview={get state(){return {...state};},select:reset,dispatch:action};
})();
