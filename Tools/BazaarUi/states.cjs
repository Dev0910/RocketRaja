const {chromium}=require(process.argv[2]),fs=require('fs'),path=require('path'),assert=require('node:assert/strict');
const out=path.resolve(__dirname,'../../Documentation/Implementation/UiUnification/states');fs.mkdirSync(out,{recursive:true});
(async()=>{const browser=await chromium.launch(),page=await browser.newPage({viewport:{width:360,height:540},reducedMotion:'reduce'}),checks=[],errors=[];const check=(v,m)=>{assert.ok(v,m);checks.push(m);};
page.on('pageerror',e=>errors.push(e.message));
try{await page.goto('http://127.0.0.1:8772/Classic/?capture=1&viewport=360x540&build=development');await page.evaluate(()=>document.fonts.ready);
const fixtures=await page.evaluate(()=>Object.keys(classicFixtures));
for(const fixture of fixtures){for(const tab of ['Home','Player','Products','Legacy','Shop']){
 await page.evaluate(({fixture,tab})=>{classicPreview.reset(fixture);classicPreview.state.settings.motion=true;classicPreview.navigate(tab);},{fixture,tab});
 check(await page.evaluate(()=>{const w=document.querySelector('.world').getBoundingClientRect(),a=document.querySelector('#app').getBoundingClientRect();return w.height>=219&&w.width===a.width&&document.documentElement.scrollWidth<=innerWidth;}),fixture+' '+tab+' keeps full stall and fits narrow viewport');
 check(!await page.locator('.panel:not([hidden])').innerText().then(t=>/undefined|NaN/.test(t)),fixture+' '+tab+' has complete live values');
}await page.evaluate(()=>classicPreview.navigate('Home'));await page.screenshot({path:path.join(out,fixture+'-360x540.png')});}
for(const condition of ['insufficient','empty','save-error','unavailable','stale'])for(const route of ['Player','Products','Upgrades','Skills','Offline','Purchase']){
 await page.evaluate(({condition,route})=>{classicPreview.reset('progressed');classicPreview.condition(condition);classicPreview.navigate(route);},{condition,route});
 check(!await page.locator('.modal-layer:not([inert]),.panel:not([hidden])').last().innerText().then(t=>/undefined|NaN/.test(t)),condition+' '+route+' renders complete values');
 if(condition==='save-error')check(await page.locator('[data-action=BuyLevels]:visible,[data-action=BuyUpgrade]:visible,[data-action=ActivateSkill]:visible').evaluateAll(es=>es.every(e=>e.disabled)),route+' save failure blocks purchases and activation');
}
await page.evaluate(()=>{classicPreview.condition('normal');classicPreview.navigate('Home');classicPreview.state.name='A VERY BRIGHT DIWALI SHOP';classicPreview.reset('progressed');});await page.waitForTimeout(100);
check(await page.locator('.shop-title').evaluate(e=>e.scrollWidth<=e.clientWidth+1),'Long shop name fits its painted sign');
check(await page.locator('.world .product').evaluateAll(es=>es.every(e=>e.getBoundingClientRect().height>=47.5)),'Shelf controls keep 48-unit hit regions');
check(await page.locator('#scroll-Home').evaluate(e=>e.scrollHeight>e.clientHeight),'Short Home scrolls instead of compressing controls');
check(!errors.length,'No exceptions across fixtures and failure states');
fs.writeFileSync(path.join(out,'checks.json'),JSON.stringify({passed:true,count:checks.length,checks,fixtures,errors},null,2));console.log('PASS '+checks.length+' fixture/failure-state checks across '+fixtures.length+' fixtures.');
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1});
