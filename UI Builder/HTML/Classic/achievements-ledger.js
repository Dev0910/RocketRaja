/* Disposable presentation fixtures only. Native RedesignEngine owns real rewards and saves. */
(()=>{'use strict';
const definitions=[
 ['taps-100','First sparks','Tap the stall 100 times.','Taps',100,10],
 ['taps-1000','Busy counter','Tap the stall 1,000 times.','Taps',1000,25],
 ['taps-10000','Raja of the counter','Tap the stall 10,000 times.','Taps',10000,50],
 ['earnings-1000','A promising start','Earn ₹1,000 across all runs.','Earnings',1000,10],
 ['earnings-1000000','Festival favourite','Earn ₹1 million across all runs.','Earnings',1000000,25],
 ['earnings-1000000000','A billion bright lights','Earn ₹1 billion across all runs.','Earnings',1000000000,50],
 ['upgrades-10','Better every day','Discover 10 different upgrades.','Upgrades',10,10],
 ['upgrades-100','An eye for upgrades','Discover 100 different upgrades.','Upgrades',100,50],
 ['upgrades-1000','The complete collection','Discover all 1,000 upgrades.','Upgrades',1000,150],
 ['products-1','Open for business','Stock your first product.','Products',1,5],
 ['products-4','Something for everyone','Stock 4 different products together.','Products',4,15],
 ['products-8','A fully lit stall','Stock all 8 products together.','Products',8,25],
 ['prestiges-1','A brighter beginning','Prestige for the first time.','Prestiges',1,25],
 ['prestiges-5','Back and brighter','Complete 5 prestiges.','Prestiges',5,75],
 ['prestiges-10','Rocket Raja','Complete 10 prestiges.','Prestiges',10,150]
].map(([id,title,description,metric,target,reward])=>({id,title,description,metric,target,reward}));
const rects={trophy:[0,80,455,315],kaju:[460,90,350,295],stall:[820,50,410,350],four:[0,415,410,355],eight:[425,420,390,350],prestige:[825,420,410,355],taps:[0,790,410,400],earnings:[420,790,400,400],upgrades:[830,790,410,400]};
const sprite=(name,cls='')=>{const [x,y,w,h]=rects[name];return `<span class="ledger-art ${cls}" aria-hidden="true"><svg viewBox="${x} ${y} ${w} ${h}" overflow="hidden"><image href="assets/achievements/ledger-atlas.png" width="1254" height="1254" style="clip-path:inset(${y}px ${1254-x-w}px ${1254-y-h}px ${x}px)"/></svg></span>`;};
let claims=new Set();
const reference=state=>state.condition==='ledger-reference';
function metrics(state,fixture,data){return reference(state)?{Taps:64,Earnings:1000,Upgrades:4,Products:8,Prestiges:1}:{
 Taps:Number(fixture.taps),Earnings:fixture.lifetime.value||0,
 Upgrades:data.upgrades.filter(u=>fixture.upgrades[u.id].ever||state.bought.has(u.id)).length,
 Products:state.levels.filter(n=>n>0).length,Prestiges:Number(fixture.prestiges)
};}
const complete=(a,m)=>m[a.metric]>=a.target;
const readyCount=(state,fixture,data)=>{const m=metrics(state,fixture,data);return definitions.filter(a=>complete(a,m)&&!claims.has(a.id)).length;};
const compact=n=>n>=1e9?(n/1e9)+'B':n>=1e6?(n/1e6)+'M':n>=1e3?(n/1e3)+'K':n.toLocaleString('en-US');
const illustration=a=>a.metric==='Products'?(a.target===1?'stall':a.target===4?'four':'eight'):a.metric==='Taps'?'taps':a.metric==='Earnings'?'earnings':a.metric==='Upgrades'?'upgrades':'prestige';
function render(state,fixture,data,button,escape,blocked){
 const m=metrics(state,fixture,data),ready=readyCount(state,fixture,data);
 const ordered=[...definitions].sort((a,b)=>(claims.has(a.id)?2:complete(a,m)?0:1)-(claims.has(b.id)?2:complete(b,m)?0:1));
 return `<div id="achievement-ledger" class="achievement-ledger" data-unity-name="AchievementRewardLedger">
 <div class="ledger-hero">${sprite('trophy')}</div>
 <div id="achievement-ledger-summary" class="ledger-summary" aria-live="polite">
 <div><strong>${claims.size} / ${definitions.length}</strong><span>claimed</span></div>
 <div><strong>${ready}</strong><span>rewards ready</span></div>
 <div><strong>${state.kk.toLocaleString('en-US')}</strong><span>Kaju Katli</span></div></div>
 <div class="ledger-rows">${ordered.map(a=>{
  const claimed=claims.has(a.id),done=complete(a,m),phase=claimed?'claimed':done?'ready':'progress';
  const current=Math.min(a.target,m[a.metric]),progress=a.metric==='Earnings'?'₹'+compact(current)+' / ₹'+compact(a.target):current.toLocaleString('en-US')+' / '+a.target.toLocaleString('en-US');
  return `<article id="achievement-${a.id}" class="ledger-row" data-phase="${phase}" data-unity-collection-id="achievements" data-unity-item-key="${a.id}" data-unity-template-id="achievement-ledger-row">
  ${sprite(illustration(a),'ledger-illustration')}<h3>${escape(a.title)}</h3><p class="ledger-description">${escape(a.description)}</p>
  <span class="ledger-track" role="progressbar" aria-label="${escape(a.title)}" aria-valuemin="0" aria-valuemax="${a.target}" aria-valuenow="${current}"><span style="width:${Math.min(100,current/a.target*100)}%"></span></span>
  <strong class="ledger-progress">${progress}</strong><div class="ledger-reward">${sprite('kaju')}<strong>+${a.reward}<small>Kaju Katli</small></strong></div>
  ${done&&!claimed?'<span class="ledger-ready">READY</span>':''}
  ${button('ClaimAchievement',a.id,claimed?'Claimed':done?'Claim':'In progress','action ledger-claim',blocked||claimed||!done)}
  </article>`;
 }).join('')}</div></div>`;
}
function claim(id,state,fixture,data){
 const a=definitions.find(a=>a.id===id);
 if(!a||claims.has(id)||!complete(a,metrics(state,fixture,data))||['save-error','loading','unavailable'].includes(state.condition))return false;
 claims.add(id);state.kk+=a.reward;return true;
}
function reset(state){claims=new Set(reference(state)?['earnings-1000']:[]);if(reference(state))state.kk=410;}
function syncContract(m){
 const ledger=document.getElementById('achievement-ledger');if(!ledger)return;
 m.bindings.achievementRewardLedger={targetIds:['achievement-ledger-summary',...definitions.map(a=>'achievement-'+a.id)],valueType:'Claim count, ready count, Kaju Katli balance, progress and claim state',format:'Selected Reward ledger; independent illustrations and editable text',sourceOwner:'RocketRaja.Classic.ClassicUI / RocketRaja.Core.RedesignAchievements',sourceSymbol:'RefreshAchievements / ReadyCount / AchievementDefinition.Progress',updateTrigger:'Open, command, save/session state and 100ms refresh',visibilityRule:'Achievements popup',interactabilityRule:'Completed, unclaimed and session may mutate',fallback:'In progress, Claimed or save recovery; no currency issued by presentation',runtimeStatus:'existing-verified',evidence:['Assets/Game/Runtime/Classic/ClassicUI.Achievements.cs','Assets/Game/Core/RedesignAchievements.cs','Documentation/AchievementValidation/RewardLedger/native-checks.txt'],reason:'Saved native ledger and actual production Player route verified on 21 October 2026 using temporary test profiles.'};
 m.modules.collections.achievements={count:15,key:'RedesignAchievements.All[].Id',template:'achievement-ledger-row',sort:'Ready first, in progress next, claimed last; stable within each group'};
}
window.classicAchievements={render,claim,reset,readyCount,syncContract,definitions};
})();
