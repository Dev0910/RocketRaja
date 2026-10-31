/* Successful-action presentation only; no spending, rewards, clocks or saved progress. */
(()=>{'use strict';
const rect=target=>window.classicViewport.rect(target);
const app=document.getElementById('app'),world=app.querySelector('.world'),active=new Set();
const root=document.createElement('div');root.className='progress-effects';root.setAttribute('aria-hidden','true');app.append(root);
const scene=document.createElement('div');scene.className='skill-atmosphere';scene.setAttribute('aria-hidden','true');
scene.innerHTML='<div class="rush-light"></div><img class="working-hands first" src="parity/assets/icons/hands.png" alt=""><img class="working-hands second" src="parity/assets/icons/hands.png" alt="">';world.append(scene);
let wasEnabled=true;
const enabled=()=>window.classicMotion.enabled();
function animate(el,frames,options={}){
 if(!el?.isConnected||!enabled())return;
 const a=el.animate(frames,{duration:650,easing:'cubic-bezier(.2,.75,.25,1)',...options});active.add(a);
 a.finished.then(()=>active.delete(a),()=>active.delete(a));return a;
}
function clear(){for(const a of active)a.cancel();active.clear();root.replaceChildren();scene.querySelectorAll('.bulk-parcel,.prestige-scene').forEach(e=>e.remove());}
function group(target,cls){
 if(root.childElementCount>=8||!target)return null;
 const r=rect(target),frame=rect(app);
 const el=document.createElement('span');el.className=cls;el.style.left=r.left+r.width/2-frame.left+'px';el.style.top=r.top+r.height/2-frame.top+'px';root.append(el);return el;
}
function finish(a,el){if(a)a.finished.then(()=>el.remove(),()=>el.remove());else el.remove();}
function stock(index){
 const slot=document.getElementById('display-'+window.classicContent.products[index]?.id),art=slot?.querySelector('.product-art');
 if(!art||!enabled())return;
 animate(art,[{opacity:0,transform:'translateY(15px) scale(.35)'},{opacity:1,transform:'translateY(-3px) scale(1.12)',offset:.65},{opacity:1,transform:'translateY(0) scale(1)'}],{duration:700});
 const dust=group(art,'stock-dust');if(!dust)return;
 const jobs=[];for(let i=0;i<10;i++){
  const dot=document.createElement('i');dust.append(dot);const x=(i-4.5)*7;
  const a=animate(dot,[{opacity:0,transform:'translate(0,12px) scale(.3)'},{opacity:.75,offset:.3},{opacity:0,transform:`translate(${x}px,${5-i%3*8}px) scale(1.4)`}],{duration:700,delay:80});if(a)jobs.push(a.finished.catch(()=>{}));
 }Promise.all(jobs).then(()=>dust.remove());
}
function milestone(index,before,after){
 const crossed=[10,25,50,100].filter(n=>before<n&&after>=n);if(!crossed.length)return;
 const level=crossed.at(-1),name=index<0?'Salesmanship':window.classicContent.products[index]?.name;
 const toast=document.createElement('span');toast.className='milestone-ribbon';toast.textContent=name+' · Level '+level+'!';
 root.querySelector('.milestone-ribbon')?.remove();root.append(toast);
 if(enabled()){
  const a=animate(toast,[{opacity:0,transform:'translate(-50%,8px) scale(.94)'},{opacity:1,transform:'translate(-50%,0) scale(1)',offset:.2},{opacity:1,offset:.8},{opacity:0,transform:'translate(-50%,-7px) scale(1)'}],{duration:2200});finish(a,toast);
  animate(world.querySelector('.shopkeeper'),[{transform:'rotate(0)'},{transform:'rotate(-4deg) translateY(-3px)',offset:.4},{transform:'rotate(0)'}],{duration:700});
  const target=index<0?world.querySelector('.tap-hint'):document.getElementById('display-'+window.classicContent.products[index].id)?.querySelector('.product-art');
  window.classicMotion.burst(target,'celebrate');
 }else setTimeout(()=>toast.remove(),2200);
 window.classicAudio?.play('celebrate');
}
function levels(index,before,after){if(index>=0&&before===0&&after>0)stock(index);milestone(index,before,after);}
function skills(views){
 app.dataset.festivalRush=String(views[0]?.phase==='active');app.dataset.helpingHands=String(views[1]?.phase==='active');
}
function skill(index){
 if(index!==2||!enabled())return;
 scene.querySelector('.bulk-parcel')?.remove();const parcel=document.createElement('img');parcel.src='parity/assets/icons/order.png';parcel.alt='';parcel.className='bulk-parcel';scene.append(parcel);
 const a=animate(parcel,[{opacity:0,transform:'translateX(-55px) rotate(-8deg)'},{opacity:1,offset:.15},{opacity:1,transform:`translateX(${world.clientWidth*.5}px) translateY(-6px) rotate(5deg)`,offset:.55},{opacity:0,transform:`translateX(${world.clientWidth+55}px) rotate(0)`}],{duration:1350});finish(a,parcel);
}
function reward(source,amount){
 if(!(amount>0))return;
 const targets=['#achievement-ledger-summary>div:last-child strong','#legacy-kk','#panel-Shop .shop-balance','#nav-Shop'];
 const destination=targets.map(s=>document.querySelector(s)).find(e=>e&&e.getClientRects().length);
 if(!destination)return;
 window.classicAudio?.play('celebrate');
 if(!enabled()){window.classicMotion.pulse(destination);return;}
 const flight=group(source||destination,'kaju-flight');if(!flight)return;
 const start=rect(source||destination),end=rect(destination);
 const dx=end.left+end.width/2-start.left-start.width/2,dy=end.top+end.height/2-start.top-start.height/2;
 const jobs=[];for(let i=0;i<3;i++){
  const token=document.createElement('span');token.innerHTML=window.classicCurrency.icon({decorative:true});flight.append(token);
  const a=animate(token,[{opacity:0,transform:`translate(${i*8}px,0) scale(.65)`},{opacity:1,transform:`translate(${dx*.2}px,${dy*.25-20}px) scale(1)`,offset:.25},{opacity:1,offset:.8},{opacity:0,transform:`translate(${dx}px,${dy}px) scale(.55)`}],{duration:850,delay:i*75,fill:'backwards'});if(a)jobs.push(a.finished.catch(()=>{}));
 }Promise.all(jobs).then(()=>{flight.remove();window.classicMotion.pulse(destination);});
}
function prestige(){
 window.classicAudio?.play('legendary');if(!enabled())return;
 scene.querySelector('.prestige-scene')?.remove();const veil=document.createElement('div');veil.className='prestige-scene';veil.innerHTML='<span class="prestige-bloom"></span>';scene.append(veil);
 const a=animate(veil,[{opacity:0},{opacity:.88,offset:.3},{opacity:.75,offset:.55},{opacity:0}],{duration:1350,easing:'ease-in-out'});
 animate(veil.firstElementChild,[{opacity:0,transform:'translate(-50%,-50%) scale(.1)'},{opacity:1,transform:'translate(-50%,-50%) scale(.7)',offset:.45},{opacity:0,transform:'translate(-50%,-50%) scale(2.3)'}],{duration:1250,delay:100,fill:'backwards'});finish(a,veil);
}
function sync(){const on=enabled();if(!on&&wasEnabled||document.hidden)clear();wasEnabled=on;}
window.classicProgressFeel={levels,stock,milestone,skills,skill,reward,prestige,sync};
})();
