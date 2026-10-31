const {chromium}=require(process.argv[2]),fs=require('fs'),path=require('path'),assert=require('node:assert/strict');
const out=path.resolve(__dirname,'../../Documentation/Implementation/UiUnification/functional');fs.mkdirSync(out,{recursive:true});
const checks=[],errors=[];function check(ok,label){assert.ok(ok,label);checks.push(label);}
(async()=>{const browser=await chromium.launch(),page=await browser.newPage({reducedMotion:'reduce'});page.on('pageerror',e=>errors.push(e.message));page.on('response',r=>{if(r.status()>=400)errors.push(r.status()+' '+r.url());});try{
  // Retained Settings, focus, validation, guarded actions and scrolling.
  await page.setViewportSize({width:360,height:540});
  await page.goto('http://127.0.0.1:8772/Classic/?capture=1&viewport=360x540&state=progressed&screen=Settings&build=development');
  await page.evaluate(()=>document.fonts.ready);
  await page.evaluate(()=>{const e=document.querySelector('[data-screen=Settings]');window.settingsIdentity=e;e.querySelector('.modal-body').scrollTop=90;});
  for(const route of ['Naming','Privacy','Help','TutorialHelp','DeveloperReset']){
   await page.evaluate(route=>classicPreview.navigate(route),route);
   check(await page.evaluate(()=>document.querySelector('[data-screen=Settings]')===window.settingsIdentity),`${route} retains original Settings DOM`);
   check(await page.locator('[data-screen=Settings]').getAttribute('inert')!==null,`${route} leaves Settings noninteractive behind it`);
   await page.keyboard.press('Escape');await page.waitForTimeout(20);
   check(await page.evaluate(()=>classicPreview.getSnapshot().sheets.at(-1)==='Settings'),`${route} back restores Settings`);
   check(await page.evaluate(()=>document.querySelector('[data-screen=Settings] .modal-body').scrollTop===90),`${route} preserves Settings scroll`);
  }
  await page.evaluate(()=>classicPreview.navigate('Naming'));
  await page.locator('#shop-name').fill('');await page.locator('[data-screen=Naming] [data-action=Rename]').click();
  check(await page.locator('#shop-name-error').isVisible(),'Blank name preserves validation feedback');
  await page.locator('#shop-name').fill('Festival Corner');await page.locator('[data-screen=Naming] [data-action=Rename]').click();
  check(await page.evaluate(()=>classicPreview.state.name==='Festival Corner'),'Valid naming action still updates the preview');

  await page.evaluate(()=>{classicPreview.navigate('Home');classicPreview.navigate('Products');});
  await page.locator('#scroll-Products').evaluate(e=>e.scrollTop=e.scrollHeight);
  check(await page.locator('#scroll-Products').evaluate(e=>e.scrollTop>0),'Products scrolls to the last families');
  await page.evaluate(()=>classicPreview.navigate('Upgrades'));
  await page.locator('#catalogue-search').fill('PP-PHU-01');
  check(await page.locator('[data-screen=Upgrades]').innerText().then(t=>/Phuljhari|No upgrades/.test(t)),'Catalogue search still filters its rows');

  // Screen-coordinate effects must remain under the tap at 3x canvas scale.
  await page.setViewportSize({width:1080,height:2200});
  await page.goto('http://127.0.0.1:8772/Classic/?capture=1&viewport=1080x2200&state=progressed');
  await page.emulateMedia({reducedMotion:'no-preference'});
  await page.evaluate(()=>{classicPreview.state.settings.motion=false;classicPreview.navigate('Home');});
  await page.waitForTimeout(350);
  const tap=page.locator('#stall-tap'),bounds=await tap.boundingBox(),point={x:bounds.x+bounds.width*.5,y:bounds.y+bounds.height*.7};
  await page.mouse.click(point.x,point.y);await page.waitForTimeout(35);
  check(await page.evaluate(()=>classicPreview.getSnapshot().events.filter(e=>e.action==='Tap').length===1),'One scaled pointer tap records one gameplay tap');
  const payout=page.locator('.tap-payout').last();
  if(await payout.count()){
   const b=await payout.boundingBox();check(b&&Math.abs(b.x+b.width/2-point.x)<20,'Payout appears at the scaled tap position');
  }
  const count=await page.evaluate(()=>classicPreview.getSnapshot().events.filter(e=>e.action==='Tap').length);
  await page.mouse.move(point.x,point.y);await page.mouse.down();await page.mouse.move(point.x+100,point.y+5);await page.mouse.up();
  check(await page.evaluate(()=>classicPreview.getSnapshot().events.filter(e=>e.action==='Tap').length)===count,'Dragging the scaled tap surface does not earn');
  await page.evaluate(()=>classicPreview.navigate('Settings'));
  check(await page.evaluate(()=>classicPreview.getSnapshot().events.filter(e=>e.action==='Tap').length)===count,'Popup navigation does not earn');
  check(!errors.length,`No browser exceptions or failed resources: ${errors.join('; ')}`);
fs.writeFileSync(path.join(out,'checks.json'),JSON.stringify({passed:true,count:checks.length,checks,errors},null,2));console.log('PASS '+checks.length+' interaction checks.');}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1;});
