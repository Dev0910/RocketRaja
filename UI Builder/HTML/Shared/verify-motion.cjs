const assert=require('assert/strict'),fs=require('fs'),path=require('path'),{pathToFileURL}=require('url');
const {chromium}=require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root=path.resolve(__dirname,'..');
(async()=>{
 const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
 const p=await browser.newPage(),errors=[],checks=[];p.on('pageerror',e=>errors.push(e.message));
 const settle=()=>p.waitForFunction(()=>document.getAnimations().every(a=>a.playState!=='running')&&!document.querySelector('[data-motion-exit]'));
 const goto=async(query='screen=Home')=>{await p.goto(pathToFileURL(path.join(root,'Player/player.html')).href+'?'+query);await p.evaluate(()=>document.fonts.ready);};
 try{
  for(const [width,height] of [[360,540],[450,916]]){
   await p.setViewportSize({width,height});await goto();const world=await p.locator('.world').boundingBox();
   await p.evaluate(()=>document.getElementById('nav-Player').click());
   assert.ok(await p.evaluate(()=>document.getAnimations().some(a=>a.effect.target.id==='panel-Player')),'opening animation');
   assert.equal(await p.locator('#panel-Home').getAttribute('inert'),'');
   assert.deepEqual(await p.locator('.world').boundingBox(),world);
   await settle();await p.locator('#scroll-Player').evaluate(e=>e.scrollTop=170);const scroll=await p.locator('#scroll-Player').evaluate(e=>e.scrollTop);
   await p.evaluate(()=>{for(const n of ['Products','Legacy','Player','Player','Products','Player'])document.getElementById('nav-'+n).click();});await settle();
   assert.equal(await p.evaluate(()=>screenPreview.getState().tab),'Player');
   assert.equal(await p.locator('#scroll-Player').evaluate(e=>e.scrollTop),scroll);
   assert.equal(await p.locator('#panel-Player').getAttribute('inert'),null);
   assert.deepEqual(await p.locator('.world').boundingBox(),world);
   await p.locator('#scroll-Player').evaluate(e=>e.scrollTop=0);
   await p.click('#open-statistics');await settle();
   await p.keyboard.press('Escape');
   assert.equal(await p.evaluate(()=>screenPreview.getState().modal),null);
   assert.equal(await p.locator('#modal-Statistics').getAttribute('data-motion-exit'),'modal');
   assert.equal(await p.locator('#modal-Statistics').getAttribute('inert'),'');
   assert.equal(await p.evaluate(()=>document.activeElement.id),'open-statistics');await settle();
   assert.equal(await p.locator('#modal-Statistics').isVisible(),false);
   await p.evaluate(()=>screenPreview.navigate('Settings'));await settle();
   await p.click('#settings-privacy');await settle();await p.click('#back-Privacy');await settle();
   assert.equal(await p.evaluate(()=>screenPreview.getState().modal),'Settings');assert.equal(await p.locator('#modal-Settings').getAttribute('inert'),null);
   await p.click('#back-Settings');await settle();
   await p.evaluate(()=>{document.getElementById('nav-Products').click();dispatchEvent(new Event('resize'));});
   assert.equal(await p.locator('[data-motion-exit]').count(),0);assert.deepEqual(await p.locator('.world').boundingBox(),world);
   assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth>innerWidth||document.documentElement.scrollHeight>innerHeight),false);
   assert.equal((await p.evaluate(()=>screenPreview.getState().events)).filter(x=>x.action==='TapStall').length,0);
   checks.push({viewport:[width,height],panels:'open/close/rapid switching',popups:'nested close/focus/inert',geometry:'stable stall; no overflow',scroll:'retained'});
  }
  // Reduced motion cancels a closing layer immediately and suppresses future effects.
  await goto('screen=Settings');await p.evaluate(()=>{document.getElementById('back-Settings').click();document.getElementById('app').dataset.reducedMotion='true';});
  await settle();assert.equal(await p.locator('[data-motion-exit]').count(),0);
  await p.evaluate(()=>document.getElementById('nav-Products').click());assert.equal(await p.evaluate(()=>document.getAnimations().length),0);
  await p.emulateMedia({reducedMotion:'reduce'});await goto();await p.evaluate(()=>{document.getElementById('nav-Player').click();document.getElementById('open-skills').click();});
  assert.equal(await p.evaluate(()=>document.getAnimations().length),0);await p.keyboard.press('Escape');assert.equal(await p.locator('[data-motion-exit]').count(),0);
  checks.push({reducedMotion:'system and game preference; active exits cancelled'});
  await p.emulateMedia({reducedMotion:'no-preference'});await goto('screen=SaveRecovery');await p.keyboard.press('Escape');
  assert.equal(await p.evaluate(()=>screenPreview.getState().modal),'SaveRecovery');assert.equal(await p.locator('#modal-SaveRecovery').getAttribute('data-motion-exit'),null);
  await goto();await p.evaluate(()=>{document.getElementById('nav-Player').click();document.getElementById('open-statistics').click();});await settle();
  await p.screenshot({path:path.join(root,'Player/renders/motion-popup-450x916.png')});
  await p.goto(pathToFileURL(path.join(root,'Home/home.html')).href);await p.click('#settings-button');await settle();await p.click('#review-route-close');await settle();assert.equal(await p.locator('#review-route').isVisible(),false);
  checks.push({recovery:'still blocking',standaloneHome:'popup opens and closes'});
  assert.deepEqual(errors,[]);fs.writeFileSync(path.join(root,'motion-verification.json'),JSON.stringify({status:'pass',checks,errors},null,2));console.log('PASS: animated panel/modal lifecycle, interruptions, focus, scroll, reduced motion and standalone Home.');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1});
