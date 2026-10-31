// Run with the path to Playwright; serves the existing HTML preview on port 8772.
const {chromium}=require(process.argv[2]);
const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'../../Documentation/Implementation/SettingsFix/HTML');
(async()=>{
 const browser=await chromium.launch();const page=await browser.newPage();
 const checks=[],errors=[];fs.mkdirSync(root,{recursive:true});
 page.on('pageerror',e=>errors.push(e.message));
 page.on('response',r=>{if(r.status()>=400)errors.push(`${r.status()} ${r.url()}`);});
 const check=(ok,label)=>{if(!ok)throw Error(label);checks.push('PASS '+label);};
 const settle=()=>page.waitForTimeout(350);
 try{
  for(const [width,height] of [[360,540],[390,844],[450,916]]){
   await page.setViewportSize({width,height});
   await page.goto('http://127.0.0.1:8772/Classic/?capture=1&screen=Settings&build=development&privacy=required');
   await page.evaluate(()=>document.fonts.ready);await settle();
   for(const route of ['Naming','Privacy','Help','TutorialHelp','DeveloperReset']){
    const opener=page.locator(`.modal-layer[data-screen=Settings] [data-action=Navigate][data-param=${route}]`);
    await opener.scrollIntoViewIfNeeded();await opener.focus();
    await page.evaluate(()=>{window.retainedSettings=document.querySelector('.modal-layer[data-screen=Settings]');window.retainedBody=retainedSettings.querySelector('.modal-body');window.retainedScroll=retainedBody.scrollTop;});
    await opener.click();await settle();
    // Pointer automation may scroll the button again before dispatching the click.
    // Compare against the actual scroll position captured when the child opens.
    await page.evaluate(()=>window.retainedScroll=classicPreview.state.stack[0].scroll);
    check(await page.evaluate(route=>{
     const parent=document.querySelector('.modal-layer[data-screen=Settings]'),top=document.querySelector(`.modal-layer[data-screen=${route}]`);
     return parent===window.retainedSettings&&parent.isConnected&&!parent.hidden&&parent.inert&&parent.getAttribute('aria-hidden')==='true'&&top&&!top.inert&&Number(top.style.zIndex)>Number(parent.style.zIndex);
    },route),`${width}x${height} ${route} retains Settings behind an interactive child`);
    if(route==='Naming')await page.screenshot({path:path.join(root,`settings-naming-${width}x${height}.png`)});
    const back=page.locator(`.modal-layer[data-screen=${route}] [data-action=Close]`).filter({hasText:/Back|Cancel/});
    await back.click();await settle();
    check(await page.evaluate(route=>{
     const el=document.querySelector('.modal-layer[data-screen=Settings]');
     const ok=el===window.retainedSettings&&!el.inert&&el.querySelector('.modal-body')===window.retainedBody&&Math.abs(retainedBody.scrollTop-retainedScroll)<1&&document.activeElement?.dataset.param===route;
     if(!ok)throw Error(JSON.stringify({same:el===window.retainedSettings,inert:el.inert,body:el.querySelector('.modal-body')===retainedBody,scroll:retainedBody.scrollTop,oldScroll:retainedScroll,focus:document.activeElement?.outerHTML}));return ok;
    },route),`${width}x${height} ${route} Back retains DOM, scroll and focus`);
   }
   check(await page.locator('.modal-layer[data-screen=Settings] [data-action=AdPrivacyOptions]').count()===1,'Consent-required preview exposes native privacy entry');
   check(await page.evaluate(()=>!document.querySelectorAll('.modal-layer').length||[...document.querySelectorAll('.modal-layer [class*=action],.modal-layer .close,.modal-layer .settings-link')].filter(x=>x.matches('button')).every(b=>getComputedStyle(b).borderImageSource.includes('/assets/buttons/folk-'))),'All Settings action faces use imported PNG artwork');
   await page.evaluate(()=>classicPreview.navigate('Privacy'));await settle();
   await page.keyboard.press('Escape');await settle();
   check(await page.evaluate(()=>classicPreview.state.stack.at(-1).name==='Settings'),'Escape closes only the child');
   await page.keyboard.press('Escape');await settle();
   for(const route of ['Player','Products','Shop','Upgrades','TapTraining','Achievements','Offline']){
    await page.evaluate(r=>classicPreview.navigate(r),route);await settle();
    check(await page.evaluate(()=>[...document.querySelectorAll('.panel:not([hidden]) .action,.modal-layer:not([inert]) .action,.modal-layer:not([inert]) .close')].every(b=>getComputedStyle(b).borderImageSource.includes('/assets/buttons/folk-'))),`${width}x${height} ${route} shared primary, secondary and disabled button faces`);
    check(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1),`${width}x${height} ${route} has no horizontal overflow`);
   }
   await page.screenshot({path:path.join(root,`offline-buttons-${width}x${height}.png`)});
  }
  await page.setViewportSize({width:390,height:844});
  await page.goto('http://127.0.0.1:8772/Tutorial/?capture=1&step=training');await settle();
  await page.evaluate(()=>tutorialPreview.dispatch('BuyTraining'));await settle();
  check(await page.evaluate(()=>tutorialPreview.state.step==='result'&&tutorialPreview.state.bubble),'Training result remains readable below next price');
  await page.evaluate(()=>tutorialPreview.dispatch('Tap'));await settle();
  check(await page.evaluate(()=>tutorialPreview.state.step==='product'&&!tutorialPreview.state.bubble&&tutorialPreview.state.cash===2.4),'New earning tap continues training result');
  for(let n=0;n<12;n++)await page.evaluate(()=>tutorialPreview.dispatch('Tap'));
  check(await page.evaluate(()=>tutorialPreview.state.view==='Products'&&tutorialPreview.state.productPrompted&&!tutorialPreview.state.products),'Affordable stock introduces the next action without buying it');
  await page.evaluate(()=>tutorialPreview.dispatch('BuyProduct'));await settle();
  check(await page.evaluate(()=>tutorialPreview.state.feedback?.earning),'Product purchase shows its result');
  await page.evaluate(()=>tutorialPreview.dispatch('AdvanceEarnings','30'));await settle();
  check(await page.evaluate(()=>!tutorialPreview.state.feedback&&tutorialPreview.state.view==='Player'&&tutorialPreview.state.upgradePrompted&&!tutorialPreview.state.upgrade),'Passive earnings fixture replaces product result with affordable upgrade');
  await page.evaluate(()=>tutorialPreview.dispatch('BuyUpgrade'));await settle();
  check(await page.evaluate(()=>tutorialPreview.state.feedback?.earning),'Upgrade purchase shows its result');
  await page.evaluate(()=>tutorialPreview.dispatch('Tap'));await settle();
  check(await page.evaluate(()=>!tutorialPreview.state.feedback&&!tutorialPreview.state.bubble),'New earning tap continues upgrade result');
  await page.screenshot({path:path.join(root,'tutorial-after-earning.png')});
  await page.evaluate(()=>{tutorialPreview.select('product');tutorialPreview.dispatch('BuyProduct');tutorialPreview.dispatch('Tap');});await settle();
  check(await page.evaluate(()=>!tutorialPreview.state.feedback&&!tutorialPreview.state.bubble),'New earning tap continues product result');
  await page.evaluate(()=>{tutorialPreview.select('training');for(let i=0;i<40;i++)tutorialPreview.dispatch('Tap');tutorialPreview.dispatch('BuyTraining');});await settle();
  check(await page.evaluate(()=>tutorialPreview.state.step==='product'&&tutorialPreview.state.view==='Products'&&tutorialPreview.state.productPrompted),'Surplus funds replace training result immediately');
  await page.locator('#settings-button').click();await settle();
  await page.locator('.modal-layer[data-screen=Settings] [data-param=TutorialHelp]').click();await settle();
  check(await page.locator('.modal-layer[data-screen=Settings]').evaluate(e=>!e.hidden&&e.inert),'Composed tutorial inherits retained Settings layers');
  await page.locator('[data-action=TutorialSkip]').click();await settle();
  check(await page.evaluate(()=>tutorialPreview.state.skipped&&!classicPreview.state.stack.length),'Tutorial settings Skip preserves stall and closes guidance');
  check(errors.length===0,'No JavaScript errors or missing local assets: '+errors.join('; '));
  checks.push(`PASS ${checks.length} checks`);fs.writeFileSync(path.join(root,'checks.txt'),checks.join('\n'));console.log(checks.at(-1));
 }catch(e){fs.writeFileSync(path.join(root,'checks.txt'),checks.concat('FAIL '+e.stack,errors).join('\n'));throw e;}
 finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
