/* Isolated, disposable browser presentation. Real rewards remain owned by RedesignEngine. */
(()=>{'use strict';
const params=new URLSearchParams(location.search),fixture=window.classicFixtures[params.get('state')||'progressed'];
const state={condition:params.get('condition')||(params.get('state')?'normal':'ledger-reference'),kk:Number(fixture.kk),levels:[...fixture.levels],bought:new Set()};
const escape=text=>String(text).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;');
window.classicAchievements.reset(state);
const body=document.getElementById('achievement-body'),modal=document.getElementById('achievement-modal'),open=document.getElementById('achievement-open');
function matchNativeCanvas(){const scale=Math.min(innerWidth,450)/360,app=document.getElementById('app');app.style.width='360px';app.style.height=(innerHeight/scale)+'px';app.style.transform=`scale(${scale})`;app.style.transformOrigin='top left';app.style.margin='0';app.style.left=Math.max(0,(innerWidth-360*scale)/2)+'px';}
matchNativeCanvas();addEventListener('resize',matchNativeCanvas);
const blocked=()=>['save-error','loading','unavailable'].includes(state.condition);
function button(action,id,label,cls,disabled){return `<button id="achievement-claim-${id}" class="${cls}" data-action="${action}" data-param="${id}" data-unity-name="Claim-${id}" data-unity-action="${action}" data-runtime-status="existing-verified" data-unity-listener-owner="RocketRaja.Classic.ClassicControl" data-unity-interactability="Completed, unclaimed, mutable session" data-unity-collection-id="achievements" data-unity-item-key="${id}" data-unity-template-id="achievement-ledger-row" ${disabled?'disabled':''}>${label}</button>`;}
function render(){const y=body.scrollTop;body.innerHTML=window.classicAchievements.render(state,fixture,window.classicContent,button,escape,blocked());body.scrollTop=y;}
render();
function close(){modal.hidden=true;open.hidden=false;open.focus();}
document.addEventListener('click',event=>{
 const target=event.target.closest('[data-action]');
 if(!target){if(event.target===modal)close();return;}
 if(target.disabled)return;
 if(target.dataset.action==='Close'){close();return;}
 if(target.dataset.action==='Navigate'){modal.hidden=false;open.hidden=true;document.getElementById('achievement-close').focus();return;}
 if(target.dataset.action==='ClaimAchievement'&&window.classicAchievements.claim(target.dataset.param,state,fixture,window.classicContent))render();
});
document.addEventListener('keydown',event=>{if(event.key==='Escape'&&!modal.hidden)close();});
window.achievementPreview={state,render};
})();
