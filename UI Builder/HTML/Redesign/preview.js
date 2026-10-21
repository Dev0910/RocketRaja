// Local presentation fixtures only: no fetch, credentials, store API or browser persistence.
(()=>{'use strict';
const $=id=>document.getElementById(id),data=window.redesignCatalogue;
const state={screen:'home',fixture:'new',cash:30,kk:100,credits:3,taps:0,nextTap:0,category:'All',view:'Available',target:-1,search:'',offset:0,selectedUpgrade:'PP-PHU-01',pack:null,pending:false,events:[],owned:new Set(),quantities:{product:'x1',tap:'x1'}};
let toastTimer;
const money=n=>'₹'+Number(n).toLocaleString('en-IN',{maximumFractionDigits:3});
function updateBalances(){
 $('shop-balance').textContent=state.kk.toLocaleString('en-IN')+' KK';
 $('talent-balances').textContent=state.credits+' free draws · '+state.kk+' Kaju Katli';
 $('free-draw').textContent='Use 1 free draw · '+state.credits+' available';
 $('free-draw').disabled=state.fixture==='locked'||state.credits===0;
 $('paid-draw').disabled=state.fixture==='locked'||state.kk<100;
}
function selectedProduct(index){
 const p=data.products[index];$('screen-title').textContent=p.name;
 $('product-income').textContent='Normal income '+money(p.income)+'/s · Next milestone level 25 → ×2';
 $('product-cost').textContent='Next level costs '+money(p.cost*1.12);
 $('buy-levels').textContent='Buy 1 level · '+money(p.cost*1.12);
 $('product-upgrades').textContent='View '+p.name+' upgrades';$('product-upgrades').dataset.param=index;
 const art=document.querySelector('#screen-productdetails .product-art');art.style.setProperty('--column',index%4);art.style.setProperty('--row',Math.floor(index/4));
}
function selectedSkill(index){
 state.skill=index;const names=['Festival Rush','Helping Hands','Bulk Order'];
 const base=data.upgrades.find(u=>u.effect===7&&u.target===index&&u.rank===1);
 $('screen-title').textContent=names[index];$('skill-heading').textContent=names[index];
 $('skill-effect').textContent=['Products produce ×2 income for 30s. Manual and Helping Hands taps receive ×1.25. Tap links and Bulk Order use normal sales.','Automatically performs 3 taps per second for 30s. Uses the same tap payout; Festival Rush can improve it.','Pays 120 seconds of normal product income immediately. Festival Rush does not multiply this payout.'][index];
 $('skill-cooldown').textContent='Cooldown: '+(index===2?'10':'5')+' minutes';
 $('skill-requirement').textContent=base.requirementText+' · '+base.name+' · '+money(base.basePrice)+'. Permanent cards improve an owned skill.';
 const mode=['ready','active','cooldown'].includes(state.fixture)?state.fixture:'locked';
 $('skill-status').textContent=mode==='locked'?'Buy the skill this run':mode==='ready'?'Ready':mode==='active'?'Active · 20s remaining':'Cooldown · 2 minutes remaining';
 $('skill-activate').disabled=mode!=='ready';$('skill-activate').textContent=mode==='locked'?'Buy the base skill first':mode==='ready'?'Activate '+names[index]:mode==='active'?'Skill active':'Skill cooling down';
 for(const id of ['skill-activate','skill-unlock','skill-upgrades'])$(id).dataset.param=index;
}
function selectedCard(index){
 const c=data.cards[index],level=index===0?1:0;
 $('card-status').textContent=['Common','Rare','Legendary'][c.tier]+' · '+(level?'Level 1':'Unowned');
 $('card-next').textContent=level?'One more copy → level 2':'First copy → level 1';
 $('card-applies').textContent=[0,1].includes(c.effect)?'Applies after you own '+data.products[c.target].name+'.':[4,5,6].includes(c.effect)?'Applies after you buy the relevant skill this run.':'Applies automatically to its income, cost or offline calculation.';
 const art=document.querySelector('#screen-carddetails .product-art');art.style.setProperty('--column',(c.target<0?0:c.target)%4);art.style.setProperty('--row',Math.floor((c.target<0?0:c.target)/4));
}
function selectedUpgrade(id){
 const u=data.upgrades.find(x=>x.id===id);state.selectedUpgrade=id;
 $('detail-upgrade-name').textContent=u.name;$('detail-upgrade-price').textContent=money(u.basePrice);
 const readable=u.effectText.replace(/^[^:]+:\s*/,'');
 $('detail-effect').textContent=readable;$('detail-requirements').textContent=u.requirementText;
 $('detail-prerequisites').textContent=u.prerequisites.length?'Earlier upgrades: '+u.prerequisites.map(id=>data.upgrades.find(x=>x.id===id).name).join(', '):'No earlier cash upgrade required.';
 $('detail-projection').textContent=u.effect===0?data.products[u.target].name+' normal income per level: '+money(data.products[u.target].income)+' → '+money(data.products[u.target].income*u.factor)+'/s':readable;
 $('show-requirement').dataset.param=u.target>=0&&[0,1,10].includes(u.effect)?'productdetails:'+u.target:'upgrades';
 $('show-requirement').textContent=u.target>=0&&[0,1,10].includes(u.effect)?'View required product':'View requirements in catalogue';
}
function toast(text){$('toast').textContent=text;$('toast').hidden=false;clearTimeout(toastTimer);toastTimer=setTimeout(()=>$('toast').hidden=true,2300);}
function modal(title,copy,test=false){$('modal-title').textContent=title;$('modal-copy').textContent=copy;$('test-outcomes').hidden=!test;$('modal').hidden=false;}
function show(route){const [screen,param]=route.split(':');if(!data.titles[screen])return;state.screen=screen;document.querySelectorAll('.screen').forEach(e=>e.hidden=e.id!=='screen-'+screen);$('screen-title').textContent=data.titles[screen];$('header-summary').textContent=screen==='home'?`₹${state.cash.toFixed(2)} cash · Normal sales ${state.owned.has('first')?'₹0.85':'₹0'}/s`:screen==='upgrades'?'₹2,000 cash · First purchases 249 / 1,000':screen==='prestige'?'Earned ₹10B / ₹10B this run':'Your fireworks business';
 const main=['upgradedetails','buyall'].includes(screen)?'upgrades':['carddetails','drawreveal','kajushop'].includes(screen)?'talent':['productdetails','skilldetails','taptraining','offline'].includes(screen)?'home':screen;
 document.querySelectorAll('nav button').forEach(b=>b.classList.toggle('selected',b.dataset.param===main));
 if(screen==='upgrades')renderRows();
 if(screen==='productdetails')selectedProduct(+(param||0));
 if(screen==='skilldetails')selectedSkill(+(param||0));
 if(screen==='carddetails'&&param!==undefined){const card=data.cards[+param];$('screen-title').textContent=card.name;$('card-name').textContent=card.name;$('card-odds').textContent=['Common: 75% ÷ 16 = 4.6875%','Rare: 22% ÷ 10 = 2.2%','Legendary: 3% ÷ 2 = 1.5%'][card.tier];const descriptions=['product income','product level cost','tap profit','tap level cost','skill cooldown','skill duration','Bulk Order payout','offline efficiency','offline storage','all product income','cash upgrade cost'];const first=['×1 → ×1.20','×1 → ×0.92','×1 → ×1.166667','×1 → ×0.92','×1 → ×0.95','×1 → ×1.125','×1 → ×1.166667','50% → 58.333333%','8 → 10 hours','×1 → ×1.25','×1 → ×0.90'];$('card-effect').textContent=(+param===0?'Level 1 → 2: ×1.20 → ×1.40':'First copy: '+first[card.effect])+' '+descriptions[card.effect];}
 if(screen==='carddetails')selectedCard(+(param||0));
 updateBalances();
}
function matches(u){const e=u.effect;return (state.category==='All'||state.category==='Products'&&[0,1,10,11].includes(e)||state.category==='Tapping'&&[2,3,4].includes(e)||state.category==='Skills'&&[7,8,9].includes(e)||state.category==='Shop'&&[5,6].includes(e))&&(state.target<0||u.target===state.target)&&(u.name+' '+u.id+' '+u.effectText).toLowerCase().includes(state.search.toLowerCase())&&(state.view==='Full'||state.view==='Owned'&&state.owned.has(u.id)||state.view==='Available'&&!state.owned.has(u.id)&&u.band===1||state.view==='Upcoming'&&!state.owned.has(u.id)&&u.band>1);}
function renderRows(){const all=data.upgrades.filter(matches);let items=all.slice(state.offset,state.offset+6);if(state.offset>=all.length){state.offset=0;items=all.slice(0,6);}for(let i=0;i<6;i++){const row=$('upgrade-row-'+i),u=items[i];row.hidden=!u;if(!u)continue;row.dataset.unityItemKey=u.id;$('upgrade-name-'+i).textContent=u.name;$('upgrade-effect-'+i).textContent=(state.owned.has(u.id)?'Owned':i===1?'Rebuy':'First purchase')+' · '+u.effectText;$('upgrade-price-'+i).textContent='₹'+u.basePrice;$('upgrade-details-'+i).dataset.param='upgradedetails';$('upgrade-details-'+i).dataset.upgrade=u.id;$('upgrade-buy-'+i).dataset.param=u.id;$('upgrade-buy-'+i).disabled=state.owned.has(u.id);}$('list-empty').hidden=all.length!==0;$('review-buyall').textContent='Review Buy All · '+state.category;$('upgrade-list').dataset.matchCount=all.length;}
function fixture(name){state.fixture=name;$('talent-locked').hidden=name!=='locked';$('free-draw').disabled=name==='locked'||name==='zero';$('paid-draw').disabled=name==='locked'||name==='zero';$('talent-cards').hidden=name==='locked';$('prestige-confirm').disabled=name==='below-target';document.querySelectorAll('.product-art.unowned').forEach(e=>e.style.opacity=name==='progressed'?'1':'');$('first-buy').hidden=name==='progressed';$('goal-title').textContent=name==='progressed'?'Your pinned goal':'Start your shop';$('goal-copy').textContent=name==='progressed'?'Phuljhari Signature I · View in Upgrades':'Buy your first Phuljhari. Your ₹30 grant is ready.';if(name==='empty'){state.search='no-matching-upgrade';$('upgrade-search').value=state.search;renderRows();}if(name==='loading'||name==='unavailable'){document.querySelectorAll('[data-unity-action="BeginTestPurchase"]').forEach(b=>{b.disabled=true;b.textContent=name==='loading'?'Loading…':'Unavailable';});}else document.querySelectorAll('[data-unity-action="BeginTestPurchase"]').forEach(b=>{b.disabled=false;b.textContent='Test purchase';});if(name==='save-error'||name==='recovery')modal('Progress needs attention','Your progress stays protected. Retry saving or return after recovery. No purchase or draw has been applied.');if(name==='pending'){state.pending=true;state.pack=data.packs[0];modal('Test purchase pending','This simulated purchase has not granted any KK. Resume the same operation or cancel it.',true);}if(name==='stale-quote')modal('Review changed','This purchase review is no longer valid. Return and review the current list.');}
function applyFixture(name){
 fixture(name);
 if(name==='zero'||name==='locked'){state.kk=0;state.credits=0;}
 updateBalances();
 if(state.screen==='skilldetails')selectedSkill(state.skill||0);
 if(name==='below-target'){
  $('header-summary').textContent='Earned ₹5B / ₹10B this run';
  $('prestige-fame').textContent='Not yet eligible · Fame 0 → 0 · Income multiplier stays ×1';
  $('prestige-credits').textContent='0 draws now · Reach ₹10B for the first 3 credits';
 }
 if(name==='save-error'||name==='recovery'){
  $('modal-close').textContent='Retry recovery';
  $('modal-close').dataset.param='retry';
 }
}
function handle(e){const b=e.target.closest('[data-unity-action]');if(!b||b.disabled||b.closest('[hidden]'))return;const action=b.dataset.unityAction,param=b.dataset.param||'';if(!$('modal').hidden&&!b.closest('#modal'))return;state.events.push({action,param});
 switch(action){
 case'Navigate':if(b.dataset.upgrade)selectedUpgrade(b.dataset.upgrade);show(param);break;
 case'Tap':if(state.screen==='home'&&performance.now()>=state.nextTap){state.nextTap=performance.now()+1000/3;state.taps++;toast('Tap accepted · gameplay fixture');}break;
 case'BuyFirst':state.owned.add('first');$('first-buy').hidden=true;$('goal-title').textContent='Your shop is open';$('goal-copy').textContent='Tap the counter or inspect Phuljhari to buy more levels.';document.querySelector('#product-0 .product-art').classList.remove('unowned');show('home');break;
 case'FilterCategory':state.category=param;state.offset=0;document.querySelectorAll('.categories button').forEach(x=>x.classList.toggle('selected',x===b));renderRows();break;
 case'FilterView':state.view=param;state.offset=0;for(let i=0;i<4;i++)$('view-'+i).classList.toggle('selected',$('view-'+i)===b);renderRows();break;
 case'CycleTarget':state.target=(state.target+2)%9-1;$('target-filter').textContent='Product: '+(state.target<0?'All':data.products[state.target].name);state.offset=0;renderRows();break;
 case'FilterProduct':state.category='Products';state.target=+param;show('upgrades');break;
 case'FilterSkill':state.category='Skills';state.target=-1;show('upgrades');break;
 case'FilterTapping':state.category='Tapping';state.target=-1;show('upgrades');break;
 case'Quantity':{const [kind,q]=param.split(':');state.quantities[kind]=q;b.parentElement.querySelectorAll('button').forEach(x=>x.classList.toggle('selected',x===b));toast(q+' selected · native quote will come from the engine');break;}
 case'ReviewBatch':show('buyall');break;
 case'ConfirmBatch':toast('Reviewed batch completed · visual fixture');show('upgrades');break;
 case'BuyUpgrade':state.owned.add(param||state.selectedUpgrade);renderRows();toast('Upgrade purchased · visual fixture');break;
 case'PinGoal':toast('Goal pinned');break;
 case'ConfirmPrestige':state.credits=3;state.owned.clear();$('first-buy').hidden=false;show('home');toast('Rebuild with your permanent gains · visual fixture');break;
 case'FreeDraw':case'PaidDraw':if(action==='FreeDraw'&&state.credits>0){state.credits--;show('drawreveal');}else if(action==='PaidDraw'&&state.kk>=100){state.kk-=100;show('drawreveal');}else toast('Not enough '+(action==='FreeDraw'?'free draws':'KK'));break;
 case'CloseReveal':show('talent');break;
 case'SkipReveal':document.querySelector('.reveal-art').style.animation='none';toast('Result ready; no extra draw');break;
 case'CloseOffline':show(param==='goal'?'upgradedetails':'home');break;
 case'BeginTestPurchase':state.pack=state.pending?state.pack:data.packs.find(p=>p.id===param);state.pending=true;modal('Test purchase · '+state.pack.name,'Choose a simulated outcome. No money will be charged. Pending grants nothing until you simulate success.',true);break;
 case'ResolveTestPurchase':if(!state.pending){toast('No pending test purchase');break;}if(param==='Delivered'){state.kk+=state.pack.kajuKatli;state.pending=false;toast('Test KK delivered');}else if(param!=='Pending'){state.pending=false;toast('Test purchase '+param.toLowerCase()+' · no grant');}else toast('Left pending · no grant');$('modal').hidden=true;show('kajushop');break;
 case'OpenTestStates':if(state.pending)modal('Resume pending test purchase','This fixture has not delivered yet. Choose the next outcome.',true);else modal('Test purchase states','Choose a pack to simulate success, pending, cancellation or failure. A pending operation resumes instead of starting another. Loading and unavailable fixtures are in the inspection tools.');break;
 case'ShowOdds':modal('Exact draw odds','Common 75% ÷16 =4.6875% each. Rare22% ÷10 =2.2% each. Legendary3% ÷2 =1.5% each. Free and KK draws use this same pool.');break;
 case'ExpandNumbers':modal('Exact values','Full values come from the runtime quote. Fixture: ₹115.279, normal income ₹0.85 → ₹1.70. At large card levels, show the exact fractional improvement.');break;
 case'Settings':modal('Settings','Reuse the existing music, sound, reduced motion, store name, privacy and save recovery routes. Live Google access is unavailable in the isolated prototype.');break;
 case'CloseModal':$('modal').hidden=true;if(param==='retry')toast('Recovery retry requested · visual fixture');break;
 case'BuyLevels':case'BuyTapLevels':case'ActivateSkill':toast('Outcome shown in native engine integration; this page is a presentation fixture.');break;
 }
}
document.addEventListener('click',handle);$('upgrade-search').addEventListener('input',e=>{state.search=e.target.value;state.offset=0;renderRows();});
// Six HTML example rows are not a claim of native virtual scrolling. Expose
// every authored choice through explicit inspection paging in this preview.
$('upgrade-list').addEventListener('wheel',e=>{if(e.deltaY>0&&$('upgrade-list').scrollTop+$('upgrade-list').clientHeight>=$('upgrade-list').scrollHeight-2&&+$('upgrade-list').dataset.matchCount>state.offset+6){state.offset+=6;renderRows();$('upgrade-list').scrollTop=0;}}, {passive:true});
document.addEventListener('keydown',e=>{if(e.key==='Escape'){if(!$('modal').hidden)$('modal').hidden=true;else show('home');}});
let down=null;$('stall-tap').addEventListener('pointerdown',e=>down={x:e.clientX,y:e.clientY});$('stall-tap').addEventListener('pointerup',e=>{if(down&&Math.hypot(e.clientX-down.x,e.clientY-down.y)>10){e.preventDefault();e.stopImmediatePropagation();const block=e=>{e.stopImmediatePropagation();};$('stall-tap').addEventListener('click',block,{once:true,capture:true});}down=null;});
window.rrPreview={show,fixture:applyFixture,state,renderRows};const params=new URLSearchParams(location.search);show(params.get('screen')||'home');applyFixture(params.get('state')||'new');
})();
