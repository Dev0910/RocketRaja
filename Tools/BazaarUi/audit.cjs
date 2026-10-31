// node Tools/BazaarUi/audit.cjs <playwright-package> <pass-name> [base-url]
const {chromium}=require(process.argv[2]),fs=require('fs'),path=require('path');
const pass=process.argv[3]||'iteration-1',base=process.argv[4]||'http://127.0.0.1:8772';
const out=path.resolve(__dirname,'../../Documentation/Implementation/UiUnification',pass);
const routes=['Home','Player','Products','Legacy','Shop','ProductDetail:0','TapTraining','Skills','SkillDetail:0','SkillDetail:1','SkillDetail:2','CardDetail:0','CardDetail:16','CardDetail:26','Upgrades','Prestige','Settings','Naming','Statistics','Odds','Purchase','Help','Privacy','Appearance','Guidance','Exact','Achievements','Offline','TutorialHelp','DeveloperReset','BuyAll','DrawReveal','SaveRecovery','RecoveryExit'];
const sizes=[[1080,2200],[390,844],[360,640],[360,540]],errors=[],states=[];
fs.mkdirSync(out,{recursive:true});
(async()=>{const browser=await chromium.launch(),page=await browser.newPage({reducedMotion:'reduce'});
page.on('pageerror',e=>errors.push(e.message));page.on('response',r=>{if(r.status()>=400)errors.push(`${r.status()} ${r.url()}`);});
for(const [width,height]of sizes){
 await page.setViewportSize({width,height});await page.goto(`${base}/Classic/?capture=1&viewport=${width}x${height}&state=progressed&build=development`);await page.evaluate(()=>document.fonts.ready);
 await page.evaluate(()=>{classicPreview.state.settings.motion=true;classicPreview.navigate('Home');});
 for(const route of routes){
  await page.evaluate(route=>{classicPreview.navigate('Home');if(['Naming','Privacy','Help','TutorialHelp','DeveloperReset'].includes(route))classicPreview.navigate('Settings');classicPreview.navigate(route);},route);
  await page.waitForTimeout(50);
  const m=await page.evaluate(()=>{
   const app=document.getElementById('app'),factor=app.getBoundingClientRect().width/app.offsetWidth,rect=e=>{const r=e.getBoundingClientRect();return {x:r.x/factor,y:r.y/factor,w:r.width/factor,h:r.height/factor};};
   const top=[...document.querySelectorAll('.modal-layer:not([data-motion-ghost])')].at(-1),scope=top||document.querySelector('.panel:not([hidden])'),body=top?.querySelector('.modal-body');
   const label=e=>e.id||e.className||e.tagName,visible=e=>e.getClientRects().length&&getComputedStyle(e).visibility!=='hidden'&&!e.closest('[hidden],[inert]');
   const buttons=[...scope.querySelectorAll('button,input,select')].filter(visible).map(e=>({id:label(e),text:e.innerText?.slice(0,65),...rect(e),font:parseFloat(getComputedStyle(e).fontSize)}));
   const fonts=[...scope.querySelectorAll('h1,h2,h3,p,small,strong,span,label,button')].filter(e=>visible(e)&&e.childElementCount===0&&e.textContent.trim()).map(e=>({id:label(e),text:e.textContent.slice(0,65),size:parseFloat(getComputedStyle(e).fontSize),family:getComputedStyle(e).fontFamily,overflow:e.scrollWidth>e.clientWidth+2&&e.clientWidth>0,verticalClip:e.clientHeight>0&&e.scrollHeight>e.clientHeight+2&&['hidden','clip'].includes(getComputedStyle(e).overflowY),...rect(e)}));
   return {canvas:rect(app),world:rect(document.querySelector('.world')),navigation:rect(document.querySelector('.navigation')),popup:top?rect(top.querySelector('.modal-card')):null,header:top?rect(top.querySelector('.modal-head')):null,footer:top?rect(top.querySelector('.modal-footer')):null,body:body?rect(body):null,bodyScroll:body?[body.scrollHeight,body.clientHeight]:null,sections:body?[...body.children].map(e=>({className:e.className,...rect(e)})):[],buttons,fonts,broken:[...document.images].filter(e=>visible(e)&&e.complete&&!e.naturalWidth).map(e=>e.src),overflow:document.documentElement.scrollWidth>innerWidth+1,metadata:document.querySelectorAll('#unity-metadata').length,text:scope.innerText.slice(0,16000)};
  });
  states.push({route,width,height,...m});
  if(width===1080||height===540){
   await page.screenshot({path:path.join(out,`${route.replaceAll(':','-')}-${width}x${height}.png`)});
   const scroll=page.locator('.modal-layer:not([inert]) .modal-body,.panel:not([hidden]) .scroll-body').last();
   if(await scroll.count()&&await scroll.evaluate(e=>e.scrollHeight>e.clientHeight+2)){
    await scroll.evaluate(e=>e.scrollTop=e.scrollHeight);await page.waitForTimeout(20);
    await page.screenshot({path:path.join(out,`${route.replaceAll(':','-')}-${width}x${height}-bottom.png`)});
   }
  }
 }
}
const summary={pass,states:states.length,scriptOrAssetErrors:[...new Set(errors)],horizontalOverflow:states.filter(s=>s.overflow).map(s=>`${s.route} ${s.width}`),brokenImages:states.filter(s=>s.broken.length).map(s=>({route:s.route,width:s.width,images:s.broken})),smallTargets:states.flatMap(s=>s.buttons.filter(b=>b.w<47.5||b.h<47.5).map(b=>({route:s.route,width:s.width,...b}))),smallText:states.flatMap(s=>s.fonts.filter(f=>f.size<11.9).map(f=>({route:s.route,width:s.width,...f}))),textOverflow:states.flatMap(s=>s.fonts.filter(f=>f.overflow).map(f=>({route:s.route,width:s.width,...f}))),popupOverflow:states.filter(s=>s.popup&&(s.popup.x<-.5||s.popup.y<-.5||s.popup.x+s.popup.w>360.5||s.popup.y+s.popup.h>s.canvas.h+.5)).map(s=>`${s.route} ${s.width}`)};
summary.verticalClipping=states.flatMap(s=>s.fonts.filter(f=>f.verticalClip).map(f=>({route:s.route,width:s.width,...f})));
summary.unexpectedFonts=states.flatMap(s=>s.fonts.filter(f=>!f.family.includes('Native UI')&&!f.family.includes('Native Heading')).map(f=>({route:s.route,width:s.width,...f})));
fs.writeFileSync(path.join(out,'measurements.json'),JSON.stringify(states,null,2));fs.writeFileSync(path.join(out,'checks.json'),JSON.stringify(summary,null,2));
const items=routes.map(r=>`<article><h2>${r}</h2><a href="${r.replaceAll(':','-')}-1080x2200.png"><img src="${r.replaceAll(':','-')}-1080x2200.png" loading="lazy" alt="${r} at 1080 by 2200"></a><a href="${r.replaceAll(':','-')}-360x540.png">Short viewport</a></article>`).join('');
fs.writeFileSync(path.join(out,'index.html'),`<!doctype html><meta charset="utf-8"><title>Gouache Bazaar ${pass}</title><style>body{margin:24px;background:#193c49;color:#fff4db;font:16px system-ui}main{display:grid;grid-template-columns:repeat(auto-fill,minmax(200px,1fr));gap:24px}h2{font-size:18px}img{width:100%;height:auto}a{color:#efd19b}article{min-width:0}</style><h1>Gouache Bazaar · ${pass}</h1><p>Full route gallery. Functional findings are separate from the manual design score.</p><main>${items}</main>`);
console.log(JSON.stringify({...summary,smallTargets:summary.smallTargets.length,smallText:summary.smallText.length,textOverflow:summary.textOverflow.length,verticalClipping:summary.verticalClipping.length,unexpectedFonts:summary.unexpectedFonts.length},null,2));await browser.close();
})().catch(e=>{console.error(e);process.exitCode=1});
