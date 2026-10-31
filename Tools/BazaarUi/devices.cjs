const {chromium}=require(process.argv[2]),fs=require('fs'),path=require('path'),assert=require('node:assert/strict');
const out=path.resolve(__dirname,'../../Documentation/Implementation/UiUnification/devices');fs.mkdirSync(out,{recursive:true});
(async()=>{const b=await chromium.launch(),p=await b.newPage({viewport:{width:1080,height:2200},reducedMotion:'reduce'}),checks=[],errors=[];const check=(v,m)=>{assert.ok(v,m);checks.push(m);};p.on('pageerror',e=>errors.push(e.message));
try{
await p.goto('http://127.0.0.1:8772/Classic/reviews/device-1080x2200.html');await p.waitForTimeout(200);const frame=p.frames().find(f=>f.url().includes('state=progressed'));check(!!frame,'Device wrapper loads the live game');
check(await frame.evaluate(()=>innerWidth===1080&&innerHeight===2200&&document.querySelector('#app').getBoundingClientRect().width===1080),'Review iframe and canvas are exactly 1080 by 2200');
for(const [w,h]of [[1080,2200],[390,844],[360,540]])for(const safe of ['', '44,0,34,0']){
 await p.setViewportSize({width:w,height:h});await p.goto(`http://127.0.0.1:8772/Classic/?capture=1&viewport=${w}x${h}&safe=${safe}&state=progressed`);await p.evaluate(()=>document.fonts.ready);
 const inset=await p.locator('#bazaar-safe').boundingBox();check(inset.y===(safe?44:0)&&Math.abs(inset.height-(h-(safe?78:0)))<1,`${w} safe region follows physical insets ${safe||'none'}`);
 await p.evaluate(()=>classicPreview.navigate('Settings'));
 check(await p.locator('[data-screen=Settings] .modal-card').evaluate((el,safe)=>{const r=el.getBoundingClientRect(),a=document.querySelector('#bazaar-safe').getBoundingClientRect();return r.top>=a.top&&r.bottom<=a.bottom+.5;},safe),`${w} Settings stays within safe region`);
 for(const step of ['welcome','training','product']){
  await p.goto(`http://127.0.0.1:8772/Tutorial/?capture=1&viewport=${w}x${h}&safe=${safe}&step=${step}`);await p.evaluate(()=>document.fonts.ready);await p.waitForTimeout(550);
  const v=await p.evaluate(()=>{const hand=document.querySelector('#tutorial-pointer'),target=document.getElementById(hand.dataset.target),visual=hand.dataset.target==='stall-tap'?document.querySelector('.tap-hint'):target,r=visual?.getBoundingClientRect(),a=hand.getBoundingClientRect();return {hidden:hand.hidden,dx:r?a.left-r.left-r.width/2:null,dy:r?a.top-r.top-r.height/2:null};});
  check(!v.hidden&&Math.abs(v.dx)<1.5&&Math.abs(v.dy)<1.5,`${w} ${step} fingertip stays on CTA with insets ${safe||'none'}`);
  if(w===360&&step==='welcome')await p.screenshot({path:path.join(out,'tutorial-'+(safe?'safe':'plain')+'-360x540.png')});
 }
 await p.goto('http://127.0.0.1:8772/Loading/?capture=1');await p.evaluate(()=>document.fonts.ready);
 check(await p.locator('.brand').evaluate(e=>getComputedStyle(e).fontFamily.includes('Native Heading')),`${w} Loading uses shared heading family`);
 check(await p.locator('.label').evaluate(e=>getComputedStyle(e).fontFamily.includes('Native UI')),`${w} Loading uses shared body family`);
 await p.screenshot({path:path.join(out,`loading-${w}x${h}.png`)});
}
check(!errors.length,'No device or tutorial exceptions');fs.writeFileSync(path.join(out,'checks.json'),JSON.stringify({passed:true,count:checks.length,checks,errors},null,2));console.log('PASS '+checks.length+' device/safe-area checks.');
}finally{await b.close();}})().catch(e=>{console.error(e);process.exitCode=1});
