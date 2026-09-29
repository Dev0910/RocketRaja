/* Review-only fixtures. No Unity bridge, passive economy, storage, network or ad rewards. */
(()=>{
  'use strict';
  const $=id=>document.getElementById(id), query=new URLSearchParams(location.search);
  const productDefs=[['phuljhari','Phuljhari'],['anar','Anar'],['chakri','Chakri'],['rockets','Rockets'],['small-crackers','Small<br>crackers'],['cracker-strings','Cracker<br>strings'],['aerial-shots','Aerial shots'],['gift-boxes','Celebration<br>gift boxes']];
  // Atlas crops are presentation-only viewBoxes, not Unity import metadata.
  const cropBoxes=[[0,175,314,430],[314,175,314,430],[628,175,314,430],[942,175,312,430],[0,720,314,410],[314,720,314,410],[628,720,314,410],[942,720,312,410]];
  productDefs.forEach(([id,name],i)=>{
    const el=document.createElement('div');el.className='product';el.id=`product-${id}`;
    Object.assign(el.dataset,{unityName:`Display-${id}`,unityBinding:'Products',unityCollectionId:'products',unityTemplateId:'ProductDisplay',unityItemKey:id,owned:'true'});
    const [x,y,w,h]=cropBoxes[i];
    const crop=i===3?`<defs><clipPath id="rocket-crop"><polygon points="942,175 1254,175 1254,605 985,605 942,400"/></clipPath></defs>`:'';
    const clip=i===3?' clip-path="url(#rocket-crop)"':'';
    el.innerHTML=`<span class="product-art" aria-hidden="true"><svg viewBox="${x} ${y} ${w} ${h}" preserveAspectRatio="none">${crop}<image href="assets/gouache-products.png" width="1254" height="1254"${clip}/></svg></span><span class="empty-mark" aria-hidden="true"><svg class="game-icon utility-icon" data-icon="add" viewBox="0 0 100 100" aria-hidden="true" focusable="false"><use href="#icon-add"/></svg></span><span class="product-name">${name}</span>`;
    $('product-shelves').append(el);
  });
  const originalIcons=['icon-burst','icon-hands','icon-order'];
  // Frozen review timings. Native cooldown duration includes the Legacy modifier.
  const cooldownSeconds=[300,300,600];
  const mmss=seconds=>`${String(Math.floor(seconds/60)).padStart(2,'0')}:${String(seconds%60).padStart(2,'0')}`;
  const skillButtons=[$('skill-rush'),$('skill-hands'),$('skill-order')];
  const clone=o=>JSON.parse(JSON.stringify(o));
  const base={cash:1240000,run:3800000,tap:250,normal:6200,live:12400,sponsor:'active',skills:['ready','active','cooldown'],cooldownRemaining:[270,270,260],owned:8};
  const fixtures={
    progressed:{...base},
    new:{cash:0,run:0,tap:1,normal:0,live:0,sponsor:'available',skills:['locked','locked','locked'],owned:0},
    ready:{...base,live:6200,sponsor:'available',skills:['ready','ready','ready']},
    cooldown:{...base,live:6200,sponsor:'available',skills:['cooldown','cooldown','cooldown']},
    'sponsor-unavailable':{...base,live:6200,sponsor:'unavailable',skills:['ready','ready','ready']}
  };
  let fixtureName='progressed',state=clone(base),blocked=false,popupTimer;
  const events=[];
  function scale(){
    const w=window.innerWidth<=700?window.innerWidth:Math.min(window.innerWidth-(query.has('review')?300:0),window.innerHeight*1080/2200);
    const s=w/360,root=document.documentElement;
    root.style.setProperty('--scale',s);root.style.setProperty('--frame-width',w+'px');root.style.setProperty('--frame-height',window.innerHeight+'px');
  }
  const fmt=n=>{if(n>=1e6)return (n/1e6).toFixed(2).replace(/\.?0+$/,'')+'M';if(n>=1000)return(n/1000).toFixed(2).replace(/\.?0+$/,'')+'K';return String(n)};
  const rupees=n=>'₹'+fmt(n);
  function announce(action,detail={}){
    const record={action,...detail};events.push(record);window.dispatchEvent(new CustomEvent('unity-preview-action',{detail:record}));
    $('review-status').textContent=`${action}${detail.message?' · '+detail.message:''}`;
  }
  function render(){
    $('home-screen').dataset.state=fixtureName;
    $('cash-value').textContent=rupees(state.cash);$('auto-income').textContent=rupees(state.live);$('tap-income').textContent=rupees(state.tap);
    $('normal-income').textContent=`Normal sales ${rupees(state.normal)} / sec${state.sponsor==='active'?' · Sponsor ×2':''}`;
    $('run-income').textContent=`This run · ${rupees(state.run)} earned`;
    $('milestone-heading').textContent=state.owned?'NEXT DISPLAY':'FIRST PRODUCT';
    $('milestone-copy').textContent=state.owned?'Phuljhari · 12 levels to upgrade':'Phuljhari · ₹120 to stock your first shelf';
    $('tap-hint').textContent=state.owned?'TAP THE STALL TO EARN':'TAP TO EARN · THEN OPEN PRODUCTS';
    productDefs.forEach(([id],i)=>{$('product-'+id).dataset.owned=String(i<state.owned)});
    skillButtons.forEach((b,i)=>{
      const v=state.skills[i];b.dataset.skillState=v;b.disabled=v!=='ready'||blocked;
      const remaining=v==='cooldown'?Math.max(0,Math.min(cooldownSeconds[i],state.cooldownRemaining[i])):0;
      b.style.setProperty('--cooldown-cleared',`${360*(1-remaining/cooldownSeconds[i])}deg`);
      b.querySelector('use').setAttribute('href','#'+(v==='locked'?'icon-lock':originalIcons[i]));
      b.querySelector('.skill-status').textContent=v==='locked'?'LOCKED':v==='ready'?'READY':v==='active'?(i===1?'ACTIVE · 00:18':'ACTIVE · 00:30'):`WAIT · ${mmss(remaining)}`;
      b.setAttribute('aria-label',`${b.querySelector('strong').textContent}: ${b.querySelector('.skill-status').textContent}`);
    });
    $('sponsor-button').disabled=state.sponsor==='active'||blocked;
    $('sponsor-heading').textContent=state.sponsor==='active'?'SPONSOR ACTIVE · 03:42':'FESTIVAL SPONSOR';
    $('sponsor-description').textContent=state.sponsor==='active'?'×2 sales · Available after expiry':'Optional video · ×2 sales for 5 minutes';
    $('stall-tap').disabled=blocked;
  }
  function setFixture(name){window.uiMotion.settle();
    fixtureName=Object.hasOwn(fixtures,name)?name:'progressed';state=clone(fixtures[fixtureName]);events.length=0;blocked=false;
    $('review-route').hidden=true;$('review-state').value=fixtureName;clearTimeout(popupTimer);$('tap-feedback').classList.remove('show');$('tap-feedback').textContent='';render();
  }
  function showRoute(title,copy){
    blocked=true;render();$('review-route-title').textContent=title;$('review-route-copy').textContent=copy;$('review-route').hidden=false;window.uiMotion.enter($('review-route'),'modal');$('review-route-close').focus();
  }
  function closeRoute(){window.uiMotion.exit($('review-route'),'modal');blocked=false;$('review-route').hidden=true;render();$('settings-button').focus()}
  function tap(){
    if(blocked)return;state.cash+=state.tap;state.run+=state.tap;announce('TapStall',{amount:state.tap});render();
    const el=$('tap-feedback');el.textContent='+'+rupees(state.tap);el.classList.remove('show');void el.offsetWidth;el.classList.add('show');clearTimeout(popupTimer);popupTimer=setTimeout(()=>el.classList.remove('show'),900);
  }
  const tapArea=$('stall-tap'),pointers=new Map();
  tapArea.addEventListener('pointerdown',e=>{if(blocked||e.button!==0)return;tapArea.setPointerCapture(e.pointerId);pointers.set(e.pointerId,{x:e.clientX,y:e.clientY,dragged:false})});
  tapArea.addEventListener('pointermove',e=>{const p=pointers.get(e.pointerId);if(p&&Math.hypot(e.clientX-p.x,e.clientY-p.y)>5)p.dragged=true});
  tapArea.addEventListener('pointercancel',e=>pointers.delete(e.pointerId));
  tapArea.addEventListener('pointerup',e=>{const p=pointers.get(e.pointerId);pointers.delete(e.pointerId);const b=tapArea.getBoundingClientRect();if(p&&!p.dragged&&Math.hypot(e.clientX-p.x,e.clientY-p.y)<5&&e.clientX>=b.left&&e.clientX<=b.right&&e.clientY>=b.top&&e.clientY<=b.bottom)tap()});
  tapArea.addEventListener('click',e=>{if(e.detail===0)tap()});
  skillButtons.forEach((b,i)=>b.addEventListener('click',()=>{if(blocked||state.skills[i]!=='ready')return;announce('ActivateSkill',{skillId:`skill-${i}`,message:'Request recorded; fixed preview state only, no income or reward applied.'});state.cooldownRemaining[i]=cooldownSeconds[i];state.skills[i]=i===2?'cooldown':'active';render()}));
  $('settings-button').addEventListener('click',()=>{if(blocked)return;announce('OpenSettings');showRoute('Settings','The existing Unity settings route is mapped. Its redesigned popup belongs to a later screen step; no settings or save data are changed here.')});
  document.querySelectorAll('[data-panel]').forEach(b=>b.addEventListener('click',()=>{if(blocked)return;announce('NavigatePanel',{panel:b.dataset.panel});showRoute(b.dataset.panel,`The ${b.dataset.panel} route is mapped to GameView.Switch. This package previews Home only. Destination panels and their collapse behavior will be designed separately.`)}));
  $('sponsor-button').addEventListener('click',()=>{if(blocked||state.sponsor==='active')return;announce('OpenSponsor',{placement:'sponsor'});showRoute('Festival Sponsor',state.sponsor==='unavailable'?'No video is available. Normal play remains available. This fixture grants no reward.':'In Unity this opens an explicit video confirmation. This HTML does not load ads or simulate successful reward callbacks. No boost or currency has been granted.')});
  $('review-route-close').addEventListener('click',closeRoute);
  window.addEventListener('keydown',e=>{if(e.key==='Escape'&&!$('review-route').hidden)closeRoute();if(e.key==='Tab'&&!$('review-route').hidden){e.preventDefault();$('review-route-close').focus()}});
  $('review-state').addEventListener('change',e=>setFixture(e.target.value));$('review-reset').addEventListener('click',()=>setFixture(fixtureName));
  if(query.has('review'))document.body.classList.add('with-review');
  window.addEventListener('resize',scale);scale();setFixture(query.get('state')||'progressed');
  // Explicit review API; not a persistence or Unity integration API.
  window.uiMotion.ready();
  window.homePreview={setFixture,getState:()=>clone(state),getEvents:()=>clone(events),closeRoute};
})();
