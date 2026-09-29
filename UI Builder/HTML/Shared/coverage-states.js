/* UI review fixtures, not an economy, save system or advertising integration. */
(()=>{'use strict';
 const $=id=>document.getElementById(id),app=$('app'),catalog=JSON.parse($('fixture-content').textContent);
 const extraStates={
  'first-shelf':'First shelf stocked','partial-shop':'Three families · next shelf','showroom':'Six families · showroom',
  'display-25':'Level 25 displays','display-100':'Level 100 displays','all-displays':'All displays complete',
  'post-prestige':'Fresh start with Legacy','sponsor-active':'Sponsor active','sponsor-expiring':'Sponsor about to expire','sponsor-expired':'Sponsor expired',
  'save-failed':'Saving failed notice','backup-recovered':'Backup recovered notice','purchase-success':'Tap upgrade success',
  'unlockable-success':'Unlockable purchased','buy-all-success':'Buy-all result','perk-success':'Legacy perk improved',
  'display-upgraded':'Display upgrade celebration','skill-activated':'Skill activation feedback','reward-success':'Reward success (illustrative)',
  'prestige-success':'Prestige completion feedback','tap-feedback':'Tap +₹ feedback','ambient':'Shopkeeper and visitors'
 };
 const levelsByState={
  'first-shelf':[1,0,0,0,0,0,0,0],'partial-shop':[25,10,1,0,0,0,0,0],showroom:[100,25,25,10,5,1,0,0],
  'display-25':Array(8).fill(25),'display-100':Array(8).fill(100),'all-displays':Array(8).fill(250),
  'post-prestige':Array(8).fill(0),'prestige-success':Array(8).fill(0),'display-upgraded':[25,8,1,1,1,1,1,1]
 };
 const notices={
  'save-failed':['warning','Progress could not be saved','Please check device storage.'],
  'backup-recovered':['info','Your last backup was recovered','You can continue from the recovered progress.'],
  'purchase-success':['success','Your salesmanship improved.','Your taps now earn more.'],
  'unlockable-success':['success','Gift Packaging purchased.','The product income upgrade is ready.'],
  'buy-all-success':['success','Bought 3 unlockables.','All affordable, available upgrades were selected.'],
  'perk-success':['success','Permanent perk improved.','This benefit stays with you after prestige.'],
  'display-upgraded':['success','Phuljhari display upgraded!','Level 25 · a brighter shelf.'],
  'skill-activated':['success','Festival Rush activated!','Automatic sales are boosted for 30 seconds.'],
  'reward-success':['success','Your bonus is ready!','Your festival sponsor is active.'],
  'prestige-success':['success','A brighter beginning!','Spend your reputation on a permanent perk.']
 };
 let toastTimer,sponsorTimer,sponsorRemaining=0,current='progressed',tapIndex=0;
 const effects=new Set();
 const fmt=v=>'₹'+(v>=1e9?(v/1e9).toFixed(2)+'B':v>=1e6?(v/1e6).toFixed(2)+'M':v>=1000?(v/1000).toFixed(2)+'K':Number(v.toFixed(1)).toLocaleString('en-IN'));
 const baseRates=[2.8,22.4,179.2,1433.6,11468.8,91750.4,734003.2,5872025.6];
 const rate=(i,level)=>baseRates[i]*level;
 const text=(id,value)=>{if($(id))$(id).textContent=value;};
 const reduced=()=>window.uiMotion.reduced();
 const mmss=s=>String(Math.floor(s/60)).padStart(2,'0')+':'+String(s%60).padStart(2,'0');
 function run(el,frames,ms){if(reduced())return;const a=el.animate(frames,{duration:ms,easing:'ease-out'});effects.add(a);a.onfinish=()=>effects.delete(a);a.oncancel=()=>effects.delete(a);}
 function clearFeedback(){clearTimeout(toastTimer);$('preview-toast').hidden=true;$('preview-toast').replaceChildren();for(const a of effects)a.cancel();effects.clear();document.querySelectorAll('.cash-burst').forEach(el=>el.textContent='');}
 function message(title,kind='info',body='',persistent=false){
  clearTimeout(toastTimer);const notice=$('preview-toast');notice.replaceChildren();notice.dataset.kind=kind;
  notice.setAttribute('role',kind==='warning'?'alert':'status');
  const heading=document.createElement('strong');heading.textContent=title;notice.append(heading);
  if(body){const copy=document.createElement('span');copy.textContent=body;notice.append(copy);}
  notice.hidden=false;if(!persistent)toastTimer=setTimeout(()=>notice.hidden=true,kind==='warning'?7000:3800);
 }
 function celebrate(){
  if(reduced())return;
  document.querySelectorAll('.celebration-spark').forEach((el,i)=>{
   const angle=i*Math.PI/6,dx=Math.cos(angle)*45,dy=Math.sin(angle)*32;
   run(el,[{opacity:0,transform:'translate(0,0) scale(.4)'},{opacity:1,offset:.18},{opacity:0,transform:`translate(${dx}px,${dy}px) scale(.6)`}],850);
  });
 }
 function tap(amount,point){
  const pool=document.querySelectorAll('.cash-burst'),el=pool[tapIndex++%pool.length];
  for(const a of el.getAnimations())a.cancel();el.textContent='+'+amount;
  const bounds=el.parentElement.getBoundingClientRect();
  el.style.left=(point?point.clientX-bounds.left:bounds.width*.5)+'px';
  el.style.top=(point?point.clientY-bounds.top:bounds.height*.5)+'px';
  text('tap-feedback','+'+amount);
  if(reduced()){el.style.opacity='1';setTimeout(()=>el.style.opacity='0',650);}
  else run(el,[{opacity:1,transform:'translateY(0)'},{opacity:0,transform:'translateY(-38px)'}],900);
 }
 function feedback(name,persistent=false){
  if(name==='tap-feedback'){tap($('home-tap').textContent);return;}
  const entry=notices[name];if(!entry)return;message(entry[1],entry[0],entry[2],persistent);
  if(['display-upgraded','skill-activated','reward-success','prestige-success'].includes(name))celebrate();
 }
 function paintProduct(i,level){
  const tier=level>=250?4:level>=100?3:level>=25?2:level>0?1:0,id=catalog.products[i].id;
  const source=tier>=2?'../Home/assets/progression/products-'+[0,0,25,100,250][tier]+'.png':'../Home/assets/gouache-products.png';
  for(const container of [$('display-'+id),$('detail-'+id)]){
   container.dataset.displayTier=String(tier);container.querySelectorAll('.row-art image').forEach(image=>{image.setAttribute('href',source);const svg=image.parentElement;if(!svg.dataset.originalBox){svg.dataset.originalBox=svg.getAttribute('viewBox');image.dataset.originalClip=image.style.clipPath;}if(tier>=2){const x=(i%4)*313.5+22,w=269.5,y=i<4?150:710,h=i<4?550:500;svg.setAttribute('viewBox',`${x} ${y} ${w} ${h}`);image.style.clipPath=`inset(${y}px ${1254-x-w}px ${1254-y-h}px ${x}px)`;}else{svg.setAttribute('viewBox',svg.dataset.originalBox);image.style.clipPath=image.dataset.originalClip;}});
  }
 }
 function updateSponsor(){
  const active=sponsorRemaining>0,button=$('home-sponsor');button.disabled=active;
  text('home-sponsor-title',active?'Sponsor active · '+mmss(sponsorRemaining):'Festival Sponsor');
  text('home-sponsor-copy',active?'×2 sales · available after expiry':'Optional video · ×2 sales for 5 minutes');
  const normal=Number(app.dataset.fixtureIncome||0);
  const multiplier=(active?2:1)*(current==='skill-activated'?2:1);
  text('home-income',fmt(normal*multiplier));text('product-total','Profit · '+fmt(normal*multiplier)+' / sec');
  text('home-normal','Normal sales '+fmt(normal)+' / sec'+(active?' · Sponsor ×2':''));
  if(['sponsor-active','sponsor-expiring','sponsor-expired','active','reward-success','progressed'].includes(current)){
   $('sponsor-video').disabled=active;$('sponsor-message').hidden=!active;
   text('sponsor-message',active?'Sponsor active · '+mmss(sponsorRemaining)+' remaining. Come back after it expires.':'');
  }
 }
 function advanceSponsor(seconds){const before=sponsorRemaining;sponsorRemaining=Math.max(0,sponsorRemaining-seconds);updateSponsor();if(before>0&&!sponsorRemaining){clearInterval(sponsorTimer);message('Festival sponsor has ended.','info','Normal sales continue.');}}
 function render(state,levels){
  current=state;clearInterval(sponsorTimer);const owned=levels.filter(n=>n>0).length;
  app.dataset.stallTier=String(owned>=8?4:owned>=6?3:owned>=3?2:owned>0?1:0);
  const scene=owned>=8?'flagship':owned>=6?'showroom':owned>=3?'established':'starter';
  app.querySelector('.stall-art').src=scene==='showroom'?'../Home/assets/gouache-stall.png':'../Home/assets/progression/stall-'+scene+'.png';
  app.dataset.ambient=String(state==='ambient');
  levels.forEach((level,i)=>{paintProduct(i,level);text('income-'+catalog.products[i].id,fmt(rate(i,level))+' / sec');});
  app.dataset.fixtureIncome=String(levels.reduce((sum,level,i)=>sum+rate(i,level),0));
  if(owned<8){const index=levels.findIndex(n=>n===0);text('home-milestone-label','NEXT SHELF');text('home-next',catalog.products[index].name+' · '+fmt(catalog.products[index].cost)+' to stock');}
  else{
   const candidates=levels.flatMap((level,index)=>[1,25,100,250].filter(n=>n>level).map(n=>({index,remaining:n-level}))).sort((a,b)=>a.remaining-b.remaining);
   if(candidates.length){text('home-milestone-label','NEXT DISPLAY');const next=candidates[0];text('home-next',catalog.products[next.index].name+' · '+next.remaining+' levels to upgrade');}
   else{text('home-milestone-label','PRESTIGE');text('home-next','5 reputation available');}
  }
  const fresh=['post-prestige','prestige-success'].includes(state);
  app.querySelector('.tap-hint').textContent=fresh?'FRESH START · YOUR LEGACY REMAINS':owned===0?'TAP TO EARN · THEN OPEN PRODUCTS':'TAP THE STALL TO EARN';
  text('home-guidance',fresh?'Your Legacy makes every fresh start brighter.':owned===0?'Tap the stall, then open Products to stock your first shelf.':'Upgrade displays. Build reputation. Start again stronger.');
  if(fresh){text('reputation','17');text('lifetime-total','₹24.6M');text('manual-taps','12,480');text('prestiges','4');text('lifetime-reputation','29');}
  const ready=['premium','all-displays','max-level'].includes(state);
  if(ready){$('prestige-locked').hidden=true;$('prestige-reset').hidden=false;$('confirm-prestige').disabled=false;text('confirm-prestige','Prestige · +5 reputation');text('prestige-reward','Earn +5 reputation');text('prestige-eligible','Current eligible income · ₹170Q');text('run-eligible','₹170Q');text('run-total','₹180Q');}
  sponsorRemaining=state==='sponsor-expiring'?5:['progressed','active','sponsor-active','reward-success'].includes(state)?222:0;
  updateSponsor();if(sponsorRemaining>0)sponsorTimer=setInterval(()=>advanceSponsor(1),1000);
  if(state==='skill-activated'){
   const skill=$('home-skill-0');skill.disabled=true;skill.dataset.skillState='active';skill.querySelector('.skill-status').textContent='ACTIVE · 00:30';
   text('guide-status-0','ACTIVE · 00:30');$('guide-status-wrap-0').dataset.skillState='active';
  }
 }
 function onFixture(state){clearFeedback();feedback(state,true);}
 const selector=$('review-state');for(const [key,label] of Object.entries(extraStates)){const option=document.createElement('option');option.value=key;option.textContent=label;selector.append(option);}
 const feedbackSelect=$('review-feedback');for(const [key,entry] of Object.entries(notices)){const option=document.createElement('option');option.value=key;option.textContent=entry[1];feedbackSelect.append(option);}
 feedbackSelect.addEventListener('change',()=>feedback(feedbackSelect.value,true));
 $('review-replay-feedback').addEventListener('click',()=>feedback(feedbackSelect.value,true));
 const world=$('world'),pool=document.createElement('div');pool.className='feedback-layer';pool.setAttribute('aria-hidden','true');
 pool.innerHTML=Array.from({length:6},()=>'<span class="cash-burst"></span>').join('')+Array.from({length:12},(_,i)=>`<span class="celebration-spark" style="--spark:${i}">✦</span>`).join('');world.append(pool);
 const live=document.createElement('output');live.id='tap-feedback';live.className='visually-hidden';live.setAttribute('aria-live','polite');world.append(live);
 const visitors=document.createElement('div');visitors.className='visitor-layer';visitors.setAttribute('aria-hidden','true');
 const silhouette='<svg viewBox="0 0 32 64" width="28" height="56" fill="currentColor"><circle cx="16" cy="9" r="7"/><path d="M10 18Q16 15 22 18L29 38 24 40 20 28 21 45 23 64H17L15 47 13 64H7L10 43 11 28 7 41 2 39Z"/></svg>';
 visitors.innerHTML='<span class="visitor visitor-one">'+silhouette+'</span><span class="visitor visitor-two">'+silhouette+'</span>';world.append(visitors);
 const stop=()=>{if(reduced()){for(const a of effects)a.cancel();effects.clear();}};
 new MutationObserver(stop).observe(app,{attributes:true,attributeFilter:['data-reduced-motion']});matchMedia('(prefers-reduced-motion: reduce)').addEventListener('change',stop);
 window.coveragePreview={extraStates,levelsByState,render,onFixture,feedback,message,tap,celebrate,clearFeedback,advanceSponsor,rate,fmt,getSponsorRemaining:()=>sponsorRemaining};
})();

