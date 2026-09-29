/* Local review verification; requires Playwright. Never connects to Unity. */
const {chromium}=require(process.env.PLAYWRIGHT_MODULE || 'C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const {pathToFileURL}=require('node:url'),path=require('node:path'),fs=require('node:fs'),assert=require('node:assert/strict');
(async()=>{
const browser=await chromium.launch({headless:true,executablePath:process.env.BROWSER_EXE || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
const results=[],errors=[],p=await browser.newPage({viewport:{width:360,height:640},deviceScaleFactor:1});
p.on('pageerror',e=>errors.push(e.message));p.on('console',m=>{if(m.type()==='error')errors.push(m.text())});
const url=pathToFileURL(path.join(__dirname,'home.html')).href;
try {
for (const [w,h] of [[1080,2200],[360,640],[390,844],[360,540]]) {
 await p.setViewportSize({width:w,height:h});
 for(const state of ['new','progressed']){
  await p.goto(url+'?state='+state);await p.evaluate(()=>document.fonts.ready);
  const report=await p.evaluate(()=>{
    const c=JSON.parse(document.getElementById('unity-metadata').textContent);
    const nodes=[...document.querySelectorAll('[id]')],ids=nodes.map(e=>e.id);
    const controls=[...document.querySelectorAll('[data-unity-action]')].map(e=>{const r=e.getBoundingClientRect();return {id:e.id,w:r.width,h:r.height,visible:r.top>=0&&r.bottom<=innerHeight&&r.left>=0&&r.right<=innerWidth}});
    const missing=[...Object.values(c.actions).flatMap(x=>x.triggerIds),...Object.values(c.bindings).flatMap(x=>x.targetIds)].filter(id=>!document.getElementById(id));
    return {duplicateIds:ids.filter((x,i)=>ids.indexOf(x)!==i),missing,controls,products:document.querySelectorAll('.product').length,skills:document.querySelectorAll('.skill').length,nav:document.querySelectorAll('.navigation button').length,owned:document.querySelectorAll('.product[data-owned="true"]').length,scroll:document.documentElement.scrollHeight>innerHeight||document.documentElement.scrollWidth>innerWidth,metadataCount:document.querySelectorAll('#unity-metadata').length,brokenImages:[...document.images].filter(i=>!i.complete||!i.naturalWidth).map(i=>i.src),bodyText:document.querySelector('main').innerText};
  });
  assert.equal(report.metadataCount,1);assert.deepEqual(report.duplicateIds,[]);assert.deepEqual(report.missing,[]);assert.equal(report.products,8);assert.equal(report.skills,3);assert.equal(report.nav,3);assert.equal(report.owned,state==='new'?0:8);assert.equal(report.scroll,false);assert.deepEqual(report.brokenImages,[]);assert.ok(!/SHOP RANK/i.test(report.bodyText));
  for(const c of report.controls){assert.ok(c.visible,c.id+' outside frame');assert.ok(c.w>=47.9&&c.h>=47.9,c.id+' touch target');}
  await p.screenshot({path:path.join(__dirname,'renders',state+'-'+w+'x'+h+'.png')});
  results.push({state,viewport:[w,h],layout:'pass',metadata:'pass',controls:'pass'});
 }
}
await p.setViewportSize({width:360,height:640});await p.goto(url+'?state=new');
const cash=()=>p.evaluate(()=>homePreview.getState().cash);
await p.locator('#stall-tap').click();assert.equal(await cash(),1,'single tap');
let r=await p.locator('#stall-tap').boundingBox();
await p.mouse.move(r.x+50,r.y+40);await p.mouse.down();await p.mouse.move(r.x+120,r.y+90,{steps:8});await p.mouse.up();assert.equal(await cash(),1,'drag must not earn');
await p.locator('#settings-button').click();assert.equal(await cash(),1);
assert.equal(await p.locator('#review-route').isVisible(),true);
await p.locator('#stall-tap').dispatchEvent('click',{detail:0});assert.equal(await cash(),1,'blocked background');
await p.keyboard.press('Escape');assert.equal(await p.locator('#review-route').isVisible(),false);
await p.locator('#nav-products').click();assert.equal(await cash(),1);await p.keyboard.press('Escape');
assert.equal(await p.locator('#skill-rush').isDisabled(),true);
await p.locator('#skill-rush').dispatchEvent('click');assert.equal((await p.evaluate(()=>homePreview.getEvents())).filter(x=>x.action==='ActivateSkill').length,0);
await p.evaluate(()=>homePreview.setFixture('ready'));
await p.locator('#skill-order').click();assert.equal(await cash(),1240000,'preview must not pretend to grant Bulk Order');
await p.locator('#skill-order').dispatchEvent('click');assert.equal((await p.evaluate(()=>homePreview.getEvents())).filter(x=>x.action==='ActivateSkill').length,1,'no repeated activation');
await p.evaluate(()=>homePreview.setFixture('progressed'));assert.equal(await p.locator('#sponsor-button').isDisabled(),true);
await p.evaluate(()=>homePreview.setFixture('sponsor-unavailable'));await p.locator('#sponsor-button').click();assert.match(await p.locator('#review-route-copy').innerText(),/No video is available/);assert.equal(await cash(),1240000);await p.keyboard.press('Escape');
for(const state of ['ready','cooldown','sponsor-unavailable']){
 await p.evaluate(s=>homePreview.setFixture(s),state);await p.screenshot({path:path.join(__dirname,'renders',state+'-360x640.png')});
}
await p.emulateMedia({reducedMotion:'reduce'});await p.evaluate(()=>homePreview.setFixture('new'));await p.locator('#stall-tap').click();
assert.equal(await p.locator('#tap-feedback').evaluate(e=>getComputedStyle(e).animationName),'none');
assert.deepEqual(errors,[]);
results.push({interaction:'pass',checks:['single tap','drag rejection','settings isolation','navigation isolation','modal input guard','locked skills','repeat skill rejection','active sponsor disabled','unavailable ad grants nothing','no preview Bulk Order award','reduced motion']});
fs.writeFileSync(path.join(__dirname,'renders/verification.json'),JSON.stringify({status:'pass',browser:await browser.version(),results,errors},null,2));
console.log(JSON.stringify({status:'pass',captures:11,interactionChecks:11,browserErrors:errors.length}));
}finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1});

