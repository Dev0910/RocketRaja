/* Preview-only fixture transitions. RedesignEngine remains gameplay/save authority. */
(()=>{'use strict';
const $=id=>document.getElementById(id),names=['Phuljhari','Anar','Chakri','Rockets','Crackers','Strings','Aerial shots','Gift boxes'];
const moments=['welcome','saving','training','result','product','upgrade','achievement','prestige','talent'];
let state,modalAction=null,dragStart=null;
function seed(step){return {step,view:({training:'Player',product:'Products',upgrade:'Player',talent:'Legacy'})[step]||'Home',cash:({saving:6,training:10,product:30,upgrade:20,achievement:18000,prestige:1260000,talent:30})[step]||0,tap:['result','product','upgrade','achievement'].includes(step)?2.4:step==='prestige'?1650:step==='talent'?31.8:1,trained:['result','product','upgrade','achievement','prestige'].includes(step),products:step==='upgrade'?1:step==='achievement'?4:step==='prestige'?8:0,income:step==='upgrade'?.85:step==='achievement'?22.42:step==='prestige'?11650:0,credits:step==='talent'?3:0,prestiges:step==='talent'?1:0,kk:0,claimed:false,upgrade:false,card:false,bubble:['welcome','result','achievement','prestige'].includes(step),skipped:false,existing:false,error:false};}
const number=n=>n>=1e6?(n/1e6).toFixed(2).replace(/0+$/,'').replace(/\.$/,'')+'M':n>=1e3?(n/1e3).toFixed(2).replace(/0+$/,'').replace(/\.$/,'')+'K':n.toFixed(2).replace(/\.00$/,'');
const cash=n=>'₹'+number(n);
const say=s=>{$('review-status').textContent=s;};
function productArt(i){const [x,y,w,h]=[[0,175,314,430],[314,175,314,430],[628,175,314,430],[942,175,312,430],[0,720,314,410],[314,720,314,410],[628,720,314,410],[942,720,312,410]][i];return `<span class="product-art" aria-hidden="true"><svg viewBox="${x} ${y} ${w} ${h}"><defs><clipPath id="product-clip-${i}"><rect x="${x}" y="${y}" width="${w}" height="${h}"/></clipPath></defs><image clip-path="url(#product-clip-${i})" href="../Home/assets/gouache-products.png" width="1254" height="1254"/></svg></span>`;}
function reset(step='welcome'){state=seed(moments.includes(step)?step:'welcome');$('modal').close();render();}
function show(title,copy,label='Got it',callback=null){$('modal-title').textContent=title;$('modal-copy').innerHTML=copy;$('modal-ok').textContent=label;modalAction=callback;$('modal').showModal();}
function setView(view){state.view=state.view===view?'Home':view;render();}
function render(){
 const s=state,late=['achievement','prestige','talent'].includes(s.step);
 $('game').dataset.step=s.step;$('game').dataset.view=s.view;$('state-picker').value=s.step;
 $('cash').textContent=cash(s.cash);$('tap-income').textContent=cash(s.tap);$('automatic-income').textContent=cash(s.income)+'/sec';
 $('shelves').innerHTML=names.map((n,i)=>`<div class="shelf" data-unity-collection-id="products" data-unity-item-key="${['PHU','ANA','CHA','ROC','SMA','STR','AER','GIF'][i]}">${i<s.products?productArt(i):''}<label>${n}</label></div>`).join('');
 const text=s.step==='product'?['Your first automatic sales','Stock Phuljhari for ₹30. It earns ₹0.85 every second.','Got it']:s.step==='upgrade'?['Improve your stall','A useful upgrade doubles Phuljhari’s income from ₹0.85 to ₹1.70 per second.','Got it']:s.step==='talent'?['A little Talent goes a long way','Your first prestige earned three free pulls. Try one to discover a permanent benefit.','Got it']:s.step==='result'?['Your taps just got better!',`You now earn ${cash(s.tap)} per tap, up from ₹1. Next, stock Phuljhari.`,'Got it']:s.step==='achievement'?['Your first achievement!','You stocked 4 different products. Collect your +5 Kaju Katli reward.','Show reward']:s.step==='prestige'?['Ready for a brighter start?','Prestige earns Fame and free pulls. Review what resets and what stays.','View prestige']:['Let’s open your stall!','Tap the counter to earn rupees. Then we’ll improve your sales pitch.','Let’s start'];
 $('coach-title').textContent=text[0];$('coach-copy').textContent=text[1];$('coach-continue').textContent=text[2];$('coach').hidden=!(s.bubble&&s.view==='Home'&&!s.skipped&&!s.existing);
 $('home').hidden=s.view!=='Home';$('panel').hidden=s.view==='Home';$('panel-title').textContent=s.view;
 for(const v of ['Player','Products','Legacy','Shop']){ $(v.toLowerCase()+'-content').hidden=s.view!==v;$('nav-'+v).setAttribute('aria-pressed',s.view===v);}
 let objective=s.trained?['NEXT STEP','Stock Phuljhari','Save ₹30 · earn automatically']:['NEXT STEP','Train Salesmanship',`${cash(s.cash)} / ₹10 · ${s.cash>=10?'ready to train':'tap the counter'}`];
 if(s.products) objective=['NEXT STEP','Improve Phuljhari','₹20 · double its automatic income'];
 if(s.upgrade||s.skipped||s.existing) objective=['NEXT DISPLAY','Grow your stall','Explore products and useful upgrades'];
 if(s.step==='achievement'&&!s.claimed) objective=['REWARD READY','Claim Open for business','+5 Kaju Katli · reward ready'];
 if(s.step==='prestige') objective=['PRESTIGE READY','View your first prestige','Review resets, Fame and 3 free pulls'];
 if(s.step==='talent'&&s.credits>0&&!s.card) objective=['NEXT STEP','Use a free Talent pull',`${s.credits} free pulls · permanent benefits`];
 ['objective-kicker','objective-title','objective-detail'].forEach((id,i)=>$(id).textContent=objective[i]);
 $('objective-help').hidden=s.skipped||s.existing;
 $('training-title').textContent='Salesmanship · Lvl '+(s.trained?1:0);$('training-effect').textContent=s.trained?cash(s.tap)+' per tap':'₹1 → ₹2.40 per tap';$('training-cost').textContent=s.trained?'Training completed':'First level · ₹10';
 $('buy-training').disabled=s.error||s.trained||s.cash<10;$('buy-training').textContent=s.trained?'Trained':'Train · ₹10';
 $('buy-product').disabled=s.error||s.products>0||s.cash<30;$('buy-product').textContent=s.products?'Stocked':'Stock · ₹30';$('product-title').textContent='Phuljhari · '+(s.products?'Lvl 1':'not stocked');$('product-total').textContent='Automatic sales · '+cash(s.income)+'/sec';
 $('buy-upgrade').disabled=s.error||s.cash<20||s.upgrade;$('buy-upgrade').textContent=s.upgrade?'Purchased':'Buy · ₹20';$('upgrade-row').hidden=s.step==='training';$('upgrade-hint').hidden=s.step!=='upgrade';$('training-hint').hidden=s.step!=='training';document.querySelector('.toolbar').hidden=s.step==='upgrade';
 $('free-credits').textContent=s.credits+' free pulls available';$('free-pull').disabled=s.error||s.credits<1||s.prestiges<1;$('collection-count').textContent='Talent collection · '+(s.card?1:0)+' / 28';$('talent-hint').hidden=s.prestiges<1||s.card;
 $('coach-continue').disabled=s.error;$('coach-skip').disabled=s.error;
 $('status').textContent=s.error?'Saving unavailable. Your tutorial and progress will wait.':'';
 document.querySelectorAll('.focus-target').forEach(n=>n.classList.remove('focus-target'));
 const focus=!s.skipped&&!s.existing?({saving:'tap-counter',training:'buy-training',product:'buy-product',upgrade:'buy-upgrade',talent:'free-pull'})[s.step]:null;
 if(focus&&(!$(focus).disabled)){$(focus).classList.add('focus-target');if(s.view!=='Home')requestAnimationFrame(()=>{const target=$(focus),scroller=$('panel-scroll');const r=target.getBoundingClientRect(),v=scroller.getBoundingClientRect();if(r.bottom>v.bottom-7)scroller.scrollTop+=r.bottom-v.bottom+10;});}
 $('review').dataset.fixture=late?'milestone':'opening';
}
function achievements(){show('Open for business',`<p>Stock 4 different products together.</p><p class="modal-number">+5 Kaju Katli</p><p>${state.claimed?'Already claimed':state.products>=4?'4 / 4 products · reward ready':'Stock four products to earn this reward.'}</p>`,state.claimed?'Claimed':'Claim reward',()=>{if(state.error||state.claimed||state.products<4)return say('Claim blocked: unavailable, incomplete or already claimed.');state.claimed=true;state.kk+=5;state.bubble=false;render();say('Preview reward claimed once. Fixture wallet: '+state.kk+' KK.');});$('modal-ok').disabled=state.error||state.claimed||state.products<4;}
function prestige(){show('A brighter beginning','<p>Gain permanent Fame and <strong>3 free Talent pulls</strong>.</p><p class="modal-detail"><strong>Resets:</strong> cash, product levels, Salesmanship, run upgrades and skills.<br><strong>Keeps:</strong> Fame, Talent cards, Kaju Katli and achievement claims.</p>','Prestige',()=>{if(state.error||state.step!=='prestige')return say('Prestige unavailable.');const oldKK=state.kk,claimed=state.claimed;state=seed('talent');state.kk=oldKK;state.claimed=claimed;state.bubble=false;render();say('Fixture prestige complete. Three free credits; no pull made automatically.');});$('modal-ok').disabled=state.error||state.step!=='prestige';}
function route(){const s=state;if(s.step==='achievement'&&!s.claimed)return achievements();if(s.step==='prestige')return prestige();if(s.step==='talent'){s.view='Legacy';render();return;}s.view=s.skipped||s.existing?'Products':!s.trained?'Player':!s.products?'Products':'Player';if(!s.trained&&s.cash>=10)s.step='training';if(s.trained&&!s.products)s.step='product';if(s.products&&!s.upgrade)s.step='upgrade';s.bubble=false;render();}
function action(id,param){const s=state;
 if(id==='Tap'){if(s.error)return;s.cash+=s.tap;if(s.step==='welcome'){s.step='saving';s.bubble=false;}render();return;}
 if(id==='Tab')return setView(param);if(id==='Home'){s.view='Home';render();return;}if(id==='ReplayTip'){s.bubble=true;render();return;}
 if(id==='SkipTutorial'){s.skipped=true;s.bubble=false;render();say('Tutorial skipped in this preview; all navigation remains available.');return;}
 if(id==='ContinueTutorial'){s.bubble=false;if(s.step==='welcome')s.step='saving';else if(s.step==='result'){s.step='product';s.view='Home';}else if(s.step==='achievement')return achievements();else if(s.step==='prestige')return prestige();render();return;}
 if(id==='OpenObjective')return route();if(id==='Achievements')return achievements();
 if(id==='BuyTraining'){if(s.error||s.trained||s.cash<10)return;s.cash-=10;s.trained=true;s.tap=2.4;s.step='result';s.view='Home';s.bubble=true;render();say('Fixture purchase: ₹10. Tap income ₹1 → ₹2.40.');return;}
 if(id==='BuyProduct'){if(s.error||s.products||s.cash<30)return;s.cash-=30;s.products=1;s.income=.85;s.view='Home';s.step='upgrade';render();show('Your stall is earning!','<p>Phuljhari now earns <strong>₹0.85 every second</strong>. Keep tapping to grow faster.</p>','View upgrade',()=>{s.view='Player';render();});return;}
 if(id==='BuyUpgrade'){if(s.error||s.upgrade||s.cash<20)return;s.cash-=20;s.upgrade=true;s.income=s.products?1.7:0;render();show('A brighter display',s.products?'<p>Phuljhari’s income increased from <strong>₹0.85 to ₹1.70 per second</strong>.</p>':'<p>This upgrade boosts Phuljhari once you stock it.</p>');return;}
 if(id==='FreePull'){if(s.error||!s.credits||!s.prestiges)return;s.credits--;s.card=true;s.cardLevel=(s.cardLevel||0)+1;render();show('Phuljhari Talent · Level '+s.cardLevel,'<p>'+ (s.cardLevel===1?'A new permanent card!':'Duplicate card: one immediate level.')+'</p><p>Phuljhari income multiplier: ×'+(1+s.cardLevel*.2).toFixed(2)+'. This Talent stays after prestige.</p>');say('One fixture free credit used. No KK spent.');return;}
 if(id==='Stats')return show('Your stall',`<p>Automatic sales: ${cash(s.income)}/sec<br>Per tap: ${cash(s.tap)}<br>Prestiges: ${s.prestiges}<br>Kaju Katli: ${s.kk}</p>`);
 if(id==='Settings')return show('Tutorial help','<p>Guidance highlights useful controls while leaving navigation available. Use ? on the next-step card to repeat an instruction.</p>');
 if(id==='InspectSkill')return show('Skills','<p>Festival Rush, Helping Hands and Bulk Order unlock through cash upgrades. The tutorial introduces a skill when it is owned and ready.</p>');
 if(id==='InspectProduct')return show('Anar','<p>Stock Phuljhari to open the next product display. This focused preview demonstrates the first product purchase.</p>');
}
document.addEventListener('click',e=>{const b=e.target.closest('[data-action]');if(!b||b.disabled)return;if(b.id==='tap-counter')return;action(b.dataset.action,b.dataset.param);});
$('tap-counter').addEventListener('pointerdown',e=>{dragStart={x:e.clientX,y:e.clientY};});$('tap-counter').addEventListener('pointerup',e=>{if(dragStart&&Math.hypot(e.clientX-dragStart.x,e.clientY-dragStart.y)<8)action('Tap');dragStart=null;});$('tap-counter').addEventListener('pointercancel',()=>dragStart=null);$('tap-counter').addEventListener('click',e=>{if(e.detail===0)action('Tap');});
$('modal-ok').addEventListener('click',()=>{if($('modal-ok').disabled)return;const fn=modalAction;modalAction=null;$('modal').close();fn?.();});$('modal-cancel').addEventListener('click',()=>{$('modal').close();modalAction=null;});$('modal').addEventListener('close',()=>{$('modal-ok').disabled=false;});
$('state-picker').addEventListener('change',e=>reset(e.target.value));$('review-replay').onclick=()=>reset();$('review-achievement').onclick=()=>reset('achievement');$('review-prestige').onclick=()=>reset('prestige');$('review-save-error').onclick=()=>{state.error=!state.error;render();};$('review-existing').onclick=()=>{reset('upgrade');state.existing=true;render();say('Existing user fixture: no automatic tutorial.');};
document.querySelectorAll('.skill-disc').forEach(n=>n.innerHTML='<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="5" y="10" width="14" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/><circle cx="12" cy="15" r="1"/></svg>');
const q=new URLSearchParams(location.search);if(q.get('capture')==='1')document.body.classList.add('capture');reset(q.get('state')||'welcome');
window.tutorialPreview={get state(){return {...state};},select:reset};
})();
