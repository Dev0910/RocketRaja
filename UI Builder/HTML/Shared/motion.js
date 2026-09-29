/* Presentation only: existing routes, focus, quotes and actions stay authoritative. */
(()=>{'use strict';
 const root=document.querySelector('.app,.game'),system=matchMedia('(prefers-reduced-motion: reduce)');
 const effects=new Map(),exits=new Map();let ready=false;
 const reduced=()=>system.matches||root.dataset.reducedMotion==='true';
 function cancel(el){for(const a of effects.get(el)||[])a.cancel();effects.delete(el);exits.get(el)?.();}
 function animate(el,frames,duration=220){
  if(!ready||reduced()||!el)return null;
  const a=el.animate(frames,{duration,easing:'cubic-bezier(.2,.75,.25,1)',fill:'none'});
  const group=effects.get(el)||new Set();group.add(a);effects.set(el,group);
  const done=()=>{group.delete(a);if(!group.size&&effects.get(el)===group)effects.delete(el);};
  a.addEventListener('finish',done,{once:true});a.addEventListener('cancel',done,{once:true});return a;
 }
 function enter(el,kind='panel',direction=0){
  if(!el)return;cancel(el);if(!ready||reduced())return;
  if(kind==='modal'){
   animate(el,[{opacity:0},{opacity:1}],190);
   const card=el.querySelector('.modal-card')||el.firstElementChild;
   if(card){cancel(card);animate(card,[{transform:'translateY(16px) scale(.98)'},{transform:'none'}],250);}
  }else animate(el,[{opacity:0,transform:direction?`translateX(${direction*18}px)`:'translateY(16px)'},{opacity:1,transform:'none'}],230);
 }
 function exit(el,kind='panel',direction=0){
  if(!el)return;cancel(el);if(!ready||reduced()||el.hidden)return;
  // Finish older exits in this region before starting another navigation.
  for(const old of [...exits.keys()])if(old.parentElement===el.parentElement)cancel(old);
  const card=el.querySelector('.modal-card');if(card)cancel(card);
  const rect=el.getBoundingClientRect(),parent=el.offsetParent||el.parentElement,bounds=parent.getBoundingClientRect();
  const savedStyle=el.style.cssText,savedInert=el.inert,aria=el.getAttribute('aria-hidden');
  const display=getComputedStyle(el).display;
  el.dataset.motionExit=kind;el.inert=true;el.setAttribute('aria-hidden','true');
  Object.assign(el.style,{position:'absolute',left:rect.left-bounds.left+'px',top:rect.top-bounds.top+'px',width:rect.width+'px',height:rect.height+'px',margin:'0',flex:'none'});
  el.style.setProperty('--exit-display',display);
  const cleanup=()=>{exits.delete(el);delete el.dataset.motionExit;el.style.cssText=savedStyle;el.inert=savedInert;if(aria===null)el.removeAttribute('aria-hidden');else el.setAttribute('aria-hidden',aria);};
  exits.set(el,cleanup);
  // The original element stays visible briefly but inert; no duplicate DOM or listeners.
  const a=animate(el,kind==='modal'?[{opacity:1},{opacity:0}]:[{opacity:1,transform:'none'},{opacity:0,transform:direction?`translateX(${-direction*14}px)`:'translateY(14px)'}],160);
  if(kind==='modal'&&card)animate(card,[{transform:'none'},{transform:'translateY(10px) scale(.985)'}],160);
  if(a)a.addEventListener('finish',cleanup,{once:true});else cleanup();
 }
 function settle(){for(const el of [...effects.keys()])cancel(el);for(const cleanup of [...exits.values()])cleanup();}
 const check=()=>{if(reduced())settle();};
 system.addEventListener('change',check);new MutationObserver(check).observe(root,{attributes:true,attributeFilter:['data-reduced-motion']});
 addEventListener('resize',settle);document.addEventListener('visibilitychange',()=>{if(document.hidden)settle();});
 window.uiMotion={enter,exit,settle,ready:()=>{ready=true;},reduced};
})();
