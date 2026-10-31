// Compare the live editable HTML against independently captured native bounds.
// node UnityParityChecks.cjs <playwright-package> [native-evidence-directory]
const {chromium}=require(process.argv[2]);
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const out=path.resolve(__dirname,'../../Documentation/Implementation/HTML-Unity-Parity-20261031');
const evidence=process.argv[3]||path.join(out,'native-bounds');
const routes=['Home','Player','Products','Legacy','Shop','ProductDetail:0','TapTraining','Skills','SkillDetail:0','SkillDetail:1','SkillDetail:2','CardDetail:0','CardDetail:16','CardDetail:26','Upgrades','Prestige','Settings','Naming','Statistics','Odds','Purchase','Help','Privacy','Appearance','Guidance','Exact','Achievements','Offline','TutorialHelp','DeveloperReset','BuyAll','DrawReveal'];
const sizes=[[1080,2200],[390,844],[360,640],[360,540]];
const checks=[],errors=[],measures=[];
fs.mkdirSync(out,{recursive:true});
function check(ok,label){assert.ok(ok,label);checks.push(label);}
function near(actual,expected,label,tolerance=.55){check(Math.abs(actual-expected)<=tolerance,`${label}: ${actual.toFixed(3)} ≈ ${expected.toFixed(3)}`);}
function nativeWindow(route){
 if(!evidence)return null;
 const file=path.join(evidence,route.replace(':','-')+'.json');if(!fs.existsSync(file))return null;
 const data=JSON.parse(fs.readFileSync(file,'utf8'));
 return data.elements?.find(e=>e.path===`Local dialogs/${route.split(':')[0]}/Safe dialog bounds/Window`);
}
(async()=>{
 const browser=await chromium.launch(),page=await browser.newPage({reducedMotion:'reduce'});
 page.on('pageerror',e=>errors.push(e.message));
 page.on('response',r=>{if(r.status()>=400)errors.push(`${r.status()} ${r.url()}`);});
 try{
  for(const [width,height] of sizes){
   await page.setViewportSize({width,height});
   await page.goto(`http://127.0.0.1:8772/Classic/?capture=1&viewport=${width}x${height}&state=progressed&build=development`);
   await page.evaluate(()=>document.fonts.ready);
   await page.evaluate(()=>{classicPreview.state.settings.motion=true;classicPreview.navigate('Home');});
   for(const route of routes){
    await page.evaluate(route=>{
     classicPreview.navigate('Home');
     if(['Naming','Privacy','Help','TutorialHelp','DeveloperReset'].includes(route))classicPreview.navigate('Settings');
     classicPreview.navigate(route);
    },route);
    await page.waitForTimeout(45);
    const m=await page.evaluate(()=>{
     const app=document.getElementById('app'),scale=classicViewport.scale(),box=el=>{const r=el.getBoundingClientRect();return [r.x/scale,r.y/scale,r.width/scale,r.height/scale];};
     const top=[...document.querySelectorAll('.modal-layer:not([data-motion-ghost])')].at(-1);
     return {scale,app:box(app),world:box(document.querySelector('.world')),nav:box(document.querySelector('.navigation')),
      card:top?box(top.querySelector('.modal-card')):null,body:top?box(top.querySelector('.modal-body')):null,
      broken:[...document.images].filter(e=>e.getClientRects().length&&e.complete&&!e.naturalWidth).map(e=>e.src),
      overflow:document.documentElement.scrollWidth>innerWidth+1,
      metadata:document.querySelectorAll('#unity-metadata').length,
      text:top?.innerText||'',dialogCount:document.querySelectorAll('[aria-modal=true]').length};
    });
    const logicalHeight=height/(width/360),world=Math.max(220,Math.min(470,logicalHeight*(logicalHeight<600?.5:.51)));
    near(m.app[2],360,`${route} ${width} canvas width`);near(m.app[3],logicalHeight,`${route} ${width} canvas height`);
    near(m.world[3],world,`${route} ${width} stall height`);near(m.nav[3],64,`${route} ${width} navigation height`);
    check(!m.overflow,`${route} ${width} no horizontal page overflow`);check(!m.broken.length,`${route} ${width} all visible art loads`);
    check(m.metadata===1,`${route} ${width} one preserved functionality contract`);
    check(!/undefined|NaN/.test(m.text),`${route} ${width} no missing display values`);
    if(route==='Home')for(const tab of ['Player','Legacy']){
     check(await page.locator('#nav-'+tab).evaluate(e=>{const dot=getComputedStyle(e,'::after');return e.classList.contains('has-notification')&&dot.display==='block'&&parseFloat(dot.left)>e.offsetWidth/2;}),`${tab} ${width} progressed readiness dot is painted at right`);
    }
    if(m.card){
     check(m.card[0]>=-.1&&m.card[1]>=-.1&&m.card[0]+m.card[2]<=360.1&&m.card[1]+m.card[3]<=logicalHeight+.1,`${route} ${width} popup stays in viewport`);
     check(m.dialogCount===1,`${route} ${width} only top popup is modal`);
     const n=nativeWindow(route);
     if(n&&route.split(':')[0]!=='CardDetail'){
      const inset=route.split(':')[0]==='ProductDetail'?10.5:18;
      near(m.card[0],n.x,`${route} ${width} native popup left`);
      near(m.card[2],n.w,`${route} ${width} native popup width`);
      near(m.card[3],Math.min(n.h,logicalHeight-2*inset),`${route} ${width} native popup height`);
     }
    }
    measures.push({route,width,height,...m});
    if(width===1080&&['Home','Player','Products','Settings','ProductDetail:0','Skills','SkillDetail:0','Naming','Statistics','Help','Achievements','Legacy'].includes(route))
     await page.screenshot({path:path.join(out,`html-${route.replace(':','-')}-${width}x${height}.png`)});
   }
  }

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
  fs.writeFileSync(path.join(out,'checks.json'),JSON.stringify({passed:true,count:checks.length,checks,errors,measures},null,2));
  console.log(`PASS ${checks.length} checks across ${measures.length} route/viewport states.`);
 }catch(error){fs.writeFileSync(path.join(out,'checks.json'),JSON.stringify({passed:false,error:error.stack,checks,errors,measures},null,2));throw error;}
 finally{await browser.close();}
})().catch(e=>{console.error(e);process.exit(1)});
