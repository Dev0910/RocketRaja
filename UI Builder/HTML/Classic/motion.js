/* Presentation only: effects never spend currency, grant rewards, or delay input. */
(()=>{'use strict';
const rect=target=>window.classicViewport.rect(target);
const app=document.getElementById('app'),world=app.querySelector('.world');
const preference=matchMedia('(prefers-reduced-motion: reduce)'),active=new Set(),channels=new WeakMap();
const effects=document.createElement('div');effects.className='motion-effects';effects.setAttribute('aria-hidden','true');app.append(effects);
const ambient=document.createElement('div');ambient.className='festival-atmosphere';ambient.setAttribute('aria-hidden','true');
ambient.innerHTML='<span class="lantern-glow left"></span><span class="lantern-glow right"></span>'+Array.from({length:12},(_,i)=>`<i class="festival-mote" style="--x:${5+(i*37)%90}%;--y:${8+(i*19)%65}%;--drift:${i%2?12:-12}px;--duration:${5+i%4}s;--delay:${-i*.83}s"></i>`).join('');world.append(ambient);
const enabled=()=>!preference.matches&&app.dataset.reducedMotion!=='true'&&!document.hidden;
function play(el,frames,options={},channel='main'){
 if(!el?.isConnected||!enabled())return;
 let map=channels.get(el);if(!map){map=new Map();channels.set(el,map);}map.get(channel)?.cancel();
 const a=el.animate(frames,{duration:300,easing:'cubic-bezier(.2,.75,.25,1)',...options});map.set(channel,a);active.add(a);
 a.finished.then(()=>{active.delete(a);if(map.get(channel)===a)map.delete(channel);},()=>{active.delete(a);if(map.get(channel)===a)map.delete(channel);});return a;
}
function pulse(el){return play(el,[{transform:'scale(1)'},{transform:'scale(1.07)',offset:.35},{transform:'scale(1)'}],{duration:380});}
function enter(el,distance=12,delay=0){return play(el,[{opacity:0,transform:`translateY(${distance}px)`},{opacity:1,transform:'translateY(0)'}],{duration:320,delay,fill:'backwards'});}
function content(root){
 if(!root||!enabled())return;const bounds=rect(root);
 [...root.querySelectorAll('.sheet-row,.home-next,.classic-income,.classic-skills,.talent-card,.info-card,.draw-actions,.shop-balance')]
 .filter(e=>{const r=rect(e);return r.bottom>bounds.top&&r.top<bounds.bottom;}).slice(0,10).forEach((e,i)=>enter(e,9,Math.min(i*28,160)));
 root.querySelectorAll('.product-milestone-bar>span,.product-overview-progress>span,.progress-fill').forEach(e=>play(e,[{transform:'scaleX(0)'},{transform:'scaleX(1)'}],{duration:580,delay:80,fill:'backwards'}));
}
let lastPanel;
function panel(el,nav){enter(el,14);content(el.querySelector('.scroll-body'));if(nav)pulse(nav.querySelector('svg'));if(lastPanel&&lastPanel!==el)window.classicAudio?.play('navigate');lastPanel=el;}
function sheet(el){
 window.classicAudio?.play('open');
 play(el,[{backgroundColor:'#10172a00'},{backgroundColor:'#10172ac9'}],{duration:220},'backdrop');
 play(el.querySelector('.modal-card'),[{opacity:0,transform:'translateY(20px) scale(.96)'},{opacity:1,transform:'translateY(-1px) scale(1.005)',offset:.76},{opacity:1,transform:'translateY(0) scale(1)'}],{duration:360});
 content(el.querySelector('.modal-body'));
}
function dismiss(el){
 window.classicAudio?.play('close');
 if(!el||!enabled())return;
 // A short-lived inert visual copy lets navigation/focus complete immediately.
 const ghost=el.cloneNode(true);ghost.dataset.motionGhost='true';ghost.inert=true;ghost.setAttribute('aria-hidden','true');
 for(const node of [ghost,...ghost.querySelectorAll('*')]){node.removeAttribute('id');node.removeAttribute('data-action');node.removeAttribute('data-param');node.removeAttribute('role');node.removeAttribute('aria-modal');node.removeAttribute('aria-labelledby');}
 const sourceFlip=el.querySelector('.flip-card');if(sourceFlip)ghost.querySelector('.flip-card').style.setProperty('transform',getComputedStyle(sourceFlip).transform,'important');
 app.querySelectorAll('[data-motion-ghost]').forEach(e=>e.remove());app.append(ghost);
 const a=play(ghost,[{opacity:1},{opacity:0}],{duration:160});
 play(ghost.querySelector('.modal-card'),[{transform:'translateY(0) scale(1)'},{transform:'translateY(7px) scale(.98)'}],{duration:160});
 if(a)a.finished.then(()=>ghost.remove(),()=>ghost.remove());else ghost.remove();
}
function burst(target,kind='purchase'){
 if(!enabled()||!target?.isConnected||effects.childElementCount>=5)return;
 const box=rect(target),frame=rect(app);if(!box.width||!box.height)return;
 const group=document.createElement('span');group.className='motion-burst';group.style.left=(box.left+box.width/2-frame.left)+'px';group.style.top=(box.top+box.height/2-frame.top)+'px';effects.append(group);
 const palette=kind==='rare'?['#df95c4','#ffe8bd','#a977c7']:kind==='legendary'?['#ffdc79','#f49752','#fff1bc']:['#f2c56d','#fff0bd','#e5a359','#7cbab0'];
 const count=kind==='tap'?7:kind==='purchase'?12:22;
 const promises=[];
 for(let i=0;i<count;i++){
  const dot=document.createElement('i');dot.className='festival-particle'+(i%3===0?' star':'');dot.style.background=palette[i%palette.length];group.append(dot);
  const angle=i/count*Math.PI*2,radius=(kind==='tap'?28:kind==='purchase'?42:76)+(i%4)*5;
  const x=Math.cos(angle)*radius,y=Math.sin(angle)*radius;
  const a=play(dot,[{opacity:0,transform:'translate(0,0) scale(.25)'},{opacity:1,transform:`translate(${x*.6}px,${y*.6-10}px) scale(1)`,offset:.25},{opacity:0,transform:`translate(${x}px,${y+25}px) rotate(${i*43}deg) scale(.3)`}],{duration:650+(i%4)*65,easing:'cubic-bezier(.15,.6,.4,1)'});
  if(a)promises.push(a.finished.catch(()=>{}));
 }
 Promise.all(promises).then(()=>group.remove());
}
function coins(target){
 if(!enabled()||effects.childElementCount>=5)return;
 const frame=rect(app),start=rect(target),end=rect(document.getElementById('cash-value'));
 const group=document.createElement('span');group.className='motion-burst';group.style.left=start.left+start.width/2-frame.left+'px';group.style.top=start.top+start.height/2-frame.top+'px';effects.append(group);
 const dx=end.left+end.width/2-start.left-start.width/2,dy=end.top+end.height/2-start.top-start.height/2;
 const promises=[];for(let i=0;i<3;i++){const coin=document.createElement('i');coin.className='festival-coin';group.append(coin);const a=play(coin,[{opacity:0,transform:`translate(${i*9-9}px,0) scale(.6)`},{opacity:.95,transform:`translate(${dx*.15+i*12-12}px,${dy*.18}px) scale(1)`,offset:.2},{opacity:.9,transform:`translate(${dx*.85}px,${dy*.8}px) scale(.75)`,offset:.82},{opacity:0,transform:`translate(${dx}px,${dy}px) scale(.35)`}],{duration:650,delay:i*55,fill:'backwards'});if(a)promises.push(a.finished.catch(()=>{}));}Promise.all(promises).then(()=>group.remove());
}
function ring(target,kind='purchase'){
 if(!enabled()||!target?.isConnected||effects.childElementCount>=7)return;
 const r=rect(target),frame=rect(app),group=document.createElement('span');
 group.className='motion-burst reward-ring';group.dataset.kind=kind;
 group.style.left=r.left+r.width/2-frame.left+'px';group.style.top=r.top+r.height/2-frame.top+'px';
 const circle=document.createElement('i');group.append(circle);effects.append(group);
 const a=play(circle,[{opacity:.7,transform:'translate(-50%,-50%) scale(.2)'},{opacity:0,transform:'translate(-50%,-50%) scale(1.7)'}],{duration:520});
 if(a)a.finished.then(()=>group.remove(),()=>group.remove());else group.remove();
}
function floatingPayout(target,value){
 if(!enabled()||!target?.isConnected||effects.childElementCount>=7||!value)return;
 const r=rect(target),frame=rect(app),label=document.createElement('span');
 label.className='motion-payout';label.textContent='+'+value;
 const x=Math.max(40,Math.min(frame.width-40,r.left+r.width/2-frame.left));
 label.style.left=x+'px';label.style.top=Math.max(60,r.top+r.height/2-frame.top)+'px';effects.append(label);
 const a=play(label,[{opacity:0,transform:'translate(-50%,0) scale(.8)'},{opacity:1,transform:'translate(-50%,-16px) scale(1)',offset:.18},{opacity:1,offset:.65},{opacity:0,transform:'translate(-50%,-54px) scale(.95)'}],{duration:780});
 if(a)a.finished.then(()=>label.remove(),()=>label.remove());else label.remove();
}
function fireworks(celebration=false){
 if(!enabled()||app.dataset.motionPaused==='true'||!celebration&&app.dataset.tab!=='Home'||ambient.querySelector('.festival-firework'))return;
 const show=document.createElement('span');show.className='festival-firework';
 show.style.left=(24+Math.random()*52)+'%';show.style.top=(12+Math.random()*16)+'%';
 const trail=document.createElement('i');trail.className='firework-trail';show.append(trail);ambient.append(show);
 const jobs=[],count=celebration?20:14;
 const launch=play(trail,[{opacity:0,transform:'translateY(65px) scaleY(.2)'},{opacity:.8,offset:.6},{opacity:0,transform:'translateY(0) scaleY(1)'}],{duration:480});
 if(launch)jobs.push(launch.finished.catch(()=>{}));
 for(let i=0;i<count;i++){
  const spark=document.createElement('i');spark.className='firework-spark';spark.style.background=i%3?'#ffd875':'#fff4d7';show.append(spark);
  const angle=i/count*Math.PI*2,radius=celebration?48:30,x=Math.cos(angle)*radius,y=Math.sin(angle)*radius;
  const a=play(spark,[{opacity:0,transform:'translate(0,0) scale(.3)'},{opacity:.85,transform:`translate(${x*.6}px,${y*.6}px) scale(1)`,offset:.25},{opacity:0,transform:`translate(${x}px,${y+18}px) scale(.25)`}],{duration:950,delay:380,fill:'backwards'});
  if(a)jobs.push(a.finished.catch(()=>{}));
 }
 Promise.all(jobs).then(()=>show.remove());
}
function success(target,kind='purchase'){window.classicAudio?.play(kind);pulse(target);ring(target,kind);burst(target,kind);if(kind==='celebrate'||kind==='legendary')fireworks(true);}
function reveal(target,tier='common'){window.classicAudio?.play(tier==='common'?'reveal':tier);ring(target,tier);burst(target,tier);}
function tap(target,value){window.classicAudio?.play('tap');burst(target,'tap');coins(target);ring(target,'tap');floatingPayout(target,value);pulse(document.getElementById('cash-value'));pulse(world.querySelector('.tap-hint'));}
function sync(){
 app.dataset.motionPaused=String(document.hidden||document.getElementById('app-inner').inert);
 window.classicAudio?.configure({sound:app.dataset.sound!=='false',music:app.dataset.music!=='false'});
 window.classicProgressFeel?.sync();
 if(!enabled()){for(const a of active)a.cancel();effects.replaceChildren();ambient.querySelectorAll('.festival-firework').forEach(e=>e.remove());app.querySelectorAll('[data-motion-ghost]').forEach(e=>e.remove());}
}
document.addEventListener('visibilitychange',sync);
preference.addEventListener('change',()=>{sync();document.dispatchEvent(new Event('classic-motion-change'));});
setInterval(()=>fireworks(),6800);
window.classicMotion={panel,sheet,dismiss,enter,pulse,success,tap,burst,reveal,fireworks,sync,enabled};sync();
})();
