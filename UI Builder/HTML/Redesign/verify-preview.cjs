// Local, offline presentation inspection only; no Unity or live saves/services.
const {chromium}=require(process.env.PLAYWRIGHT_MODULE||'C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('node:fs'),path=require('node:path'),{pathToFileURL}=require('node:url'),assert=require('node:assert/strict');
(async()=>{
 const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
 const page=await browser.newPage({viewport:{width:390,height:844}}),errors=[],checks=[];
 page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
 await page.route(/^https?:/,r=>r.abort());
 const url=pathToFileURL(path.join(__dirname,'redesign.html')).href;
 fs.mkdirSync(path.join(__dirname,'renders'),{recursive:true});
 try{
  await page.goto(url);await page.evaluate(()=>document.fonts.ready);
  const inventory=await page.evaluate(()=>{const m=JSON.parse(document.getElementById('unity-metadata').textContent),ids=[...document.querySelectorAll('[id]')].map(x=>x.id);return {blocks:document.querySelectorAll('#unity-metadata').length,duplicates:ids.filter((x,i)=>ids.indexOf(x)!==i),missing:[...Object.values(m.actions).flatMap(x=>x.triggerIds),...Object.values(m.bindings).flatMap(x=>x.targetIds)].filter(x=>!document.getElementById(x)),screens:Object.keys(redesignCatalogue.titles),unmapped:[...document.querySelectorAll('[data-unity-action]')].filter(x=>!m.actions[x.dataset.unityAction]).map(x=>x.id)};});
  assert.equal(inventory.blocks,1);assert.deepEqual(inventory.duplicates,[]);assert.deepEqual(inventory.missing,[]);assert.deepEqual(inventory.unmapped,[]);
  checks.push({check:'Static metadata/IDs/action coverage',status:'PASS'});
  for(const [width,height] of [[390,844],[360,640],[360,540],[450,916]]){
   await page.setViewportSize({width,height});
   for(const screen of inventory.screens){
    await page.evaluate(s=>{rrPreview.show(s);document.getElementById('screen-'+s).scrollTop=0;},screen);
    await page.waitForTimeout(screen==='drawreveal'?450:180);
    const layout=await page.evaluate(()=>{
     const visible=[...document.querySelectorAll('button,input')].filter(e=>!e.closest('[hidden]'));
     return {overflow:document.documentElement.scrollWidth>innerWidth,tiny:visible.filter(e=>{const r=e.getBoundingClientRect();return r.width<47.9||r.height<47.9;}).map(e=>({id:e.id,width:e.getBoundingClientRect().width,height:e.getBoundingClientRect().height})),homeFits:rrPreview.state.screen!=='home'||[...document.querySelectorAll('#screen-home button')].every(e=>{const r=e.getBoundingClientRect(),p=document.querySelector('.screens').getBoundingClientRect();return r.top>=p.top-.5&&r.bottom<=p.bottom+.5;})};
    });
    assert.equal(layout.overflow,false,screen+' horizontal overflow '+width+'x'+height);
    assert.deepEqual(layout.tiny,[],screen+' undersized controls '+width+'x'+height);
    assert.equal(layout.homeFits,true,'Home controls clipped '+width+'x'+height);
    if(width===390||height===540)await page.screenshot({path:path.join(__dirname,'renders',screen+'-'+width+'x'+height+'.png')});
    checks.push({screen,viewport:[width,height],status:'PASS'});
   }
  }
  await page.setViewportSize({width:390,height:844});await page.goto(url);
  await page.locator('#nav-upgrades').click();assert.equal(await page.evaluate(()=>rrPreview.state.taps),0);
  await page.locator('#view-3').click();
  const searched=await page.evaluate(()=>redesignCatalogue.upgrades.find(x=>x.id==='PP-PHU-01'));
  await page.locator('#upgrade-search').fill(searched.id);
  assert.equal(await page.locator('#upgrade-name-0').textContent(),searched.name);
  await page.locator('#upgrade-search').fill('nothing-matches-this');assert.equal(await page.locator('#list-empty').isVisible(),true);
  await page.locator('#upgrade-search').fill('');await page.locator('#upgrade-details-0').click();await page.locator('#back-upgrades').click();assert.equal(await page.evaluate(()=>rrPreview.state.view),'Full');
  await page.evaluate(()=>rrPreview.show('productdetails'));await page.locator('#product-quantity-3').click();assert.equal(await page.evaluate(()=>rrPreview.state.quantities.product),'Max');
  await page.evaluate(()=>rrPreview.show('home'));await page.locator('#settings').click();await page.locator('#stall-tap').dispatchEvent('click');assert.equal(await page.evaluate(()=>rrPreview.state.taps),0);await page.keyboard.press('Escape');
  await page.locator('#stall-tap').click();assert.equal(await page.evaluate(()=>rrPreview.state.taps),1);
  await page.locator('#stall-tap').dispatchEvent('click');assert.equal(await page.evaluate(()=>rrPreview.state.taps),1);
  await page.evaluate(()=>rrPreview.show('kajushop'));const initial=await page.evaluate(()=>rrPreview.state.kk);
  await page.locator('#pack-0').click();await page.locator('#test-pending').click();assert.equal(await page.evaluate(()=>rrPreview.state.kk),initial);
  await page.locator('#test-shop-states').click();await page.locator('#test-delivered').click();assert.equal(await page.evaluate(()=>rrPreview.state.kk),initial+1000);
  await page.locator('#test-delivered').dispatchEvent('click');assert.equal(await page.evaluate(()=>rrPreview.state.kk),initial+1000);
  await page.locator('#pack-1').click();await page.locator('#test-cancelled').click();assert.equal(await page.evaluate(()=>rrPreview.state.kk),initial+1000);
  await page.locator('#pack-2').click();await page.locator('#test-failed').click();assert.equal(await page.evaluate(()=>rrPreview.state.kk),initial+1000);
  await page.evaluate(()=>rrPreview.show('talent'));await page.locator('#paid-draw').click();assert.equal(await page.evaluate(()=>rrPreview.state.kk),initial+900);await page.locator('#reveal-continue').click();
  checks.push({check:'Search/empty/retention, quantity, input isolation/ceiling, simulated pending/success/duplicate/cancel/failure and draw',status:'PASS'});
  await page.evaluate(()=>rrPreview.show('productdetails:7'));
  assert.match(await page.locator('#product-upgrades').textContent(),/Celebration boxes/);
  await page.evaluate(()=>rrPreview.show('carddetails:27'));
  assert.equal(await page.locator('#card-status').textContent(),'Legendary · Unowned');
  assert.match(await page.locator('#card-effect').textContent(),/cash upgrade cost/);
  await page.evaluate(()=>rrPreview.show('skilldetails:2'));
  assert.match(await page.locator('#skill-effect').textContent(),/120 seconds/);
  assert.equal(await page.locator('#skill-activate').isDisabled(),true);
  await page.evaluate(()=>rrPreview.fixture('ready'));
  assert.equal(await page.locator('#skill-activate').isDisabled(),false);
  checks.push({check:'Selected product/card/skill identity and locked/ready skill action',status:'PASS'});
  for(const [screen,state] of [['talent','locked'],['talent','zero'],['upgrades','empty'],['kajushop','loading'],['kajushop','unavailable'],['kajushop','pending'],['home','save-error'],['buyall','stale-quote'],['prestige','below-target'],['home','progressed']]){
   await page.goto(url+'?screen='+screen+'&state='+state);await page.waitForTimeout(180);await page.screenshot({path:path.join(__dirname,'renders',screen+'-'+state+'.png')});
   if(state==='zero'){assert.match(await page.locator('#talent-balances').textContent(),/^0 free draws · 0/);assert.equal(await page.locator('#free-draw').isDisabled(),true);}
   if(state==='below-target'){assert.match(await page.locator('#header-summary').textContent(),/₹5B/);assert.equal(await page.locator('#prestige-confirm').isDisabled(),true);}
   checks.push({screen,state,status:'CAPTURED'});
  }
  await page.emulateMedia({reducedMotion:'reduce'});await page.goto(url+'?screen=drawreveal');assert.equal(await page.locator('.reveal-art').evaluate(e=>getComputedStyle(e).animationName),'none');
  assert.deepEqual(errors,[]);checks.push({check:'Reduced motion and browser errors',status:'PASS'});
  fs.writeFileSync(path.join(__dirname,'renders/verification.json'),JSON.stringify({status:'PASS',scope:'Offline browser fixtures; not native, persistence or billing acceptance',browser:await browser.version(),checks,errors},null,2)+'\n');
  console.log(JSON.stringify({status:'PASS',checks:checks.length,captures:36,browserErrors:errors.length}));
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
