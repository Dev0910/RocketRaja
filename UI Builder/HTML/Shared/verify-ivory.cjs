/* Additional review for the Ivory Bazaar rollout: real layout, routes and long content. */
const fs=require('fs'),path=require('path'),assert=require('assert/strict'),{pathToFileURL}=require('url');
const {chromium}=require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root=path.resolve(__dirname,'..'),specs=require('../../screen-specs.json');
(async()=>{const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'}),p=await browser.newPage(),errors=[],checks=[];p.on('pageerror',e=>errors.push(e.message));p.on('requestfailed',r=>errors.push(r.url()));
const goto=async(n,extra='')=>{await p.goto(pathToFileURL(path.join(root,n,n.toLowerCase()+'.html')).href+extra);await p.evaluate(()=>document.fonts.ready);};
try{
for(const [width,height] of [[1080,2200],[450,916],[390,844],[360,640],[360,540]]){
await p.setViewportSize({width,height});await goto('Player','?screen=Home');const initial=await p.locator('.world').boundingBox();await p.screenshot({path:path.join(root,'Home/renders',`shared-home-${width}x${height}.png`)});
for(const tab of ['Player','Products','Legacy','Home']){await p.evaluate(n=>screenPreview.navigate(n),tab);const box=await p.locator('.world').boundingBox();assert.equal(box.height,initial.height,tab+' stall height');assert.equal(box.y,initial.y);assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth>innerWidth||document.documentElement.scrollHeight>innerHeight),false);}
checks.push({check:'All tabs preserve Home stall geometry',viewport:[width,height],height:initial.height});
}
await p.setViewportSize({width:360,height:540});
for(const [n,spec] of Object.entries(specs)){
await goto(n);const scroll=spec.kind==='panel'?'#scroll-'+n:'#modal-'+n+' .modal-body';
await p.locator(scroll).evaluate(e=>e.scrollTop=e.scrollHeight);
const metrics=await p.locator(scroll).evaluate(e=>({height:e.clientHeight,overflow:e.scrollHeight-e.clientHeight,scroll:e.scrollTop}));assert.ok(metrics.height>60,n+' readable scrolling area');assert.ok(Math.abs(metrics.overflow-metrics.scroll)<2,n+' cannot reach last content');
const clipped=await p.evaluate(()=>[...document.querySelectorAll('h1,h2,h3,p,dt,dd,button,.rank')].filter(e=>!e.closest('[hidden],[inert]')&&e.clientWidth>0&&e.scrollWidth>e.clientWidth+2).map(e=>e.id||e.textContent.slice(0,70)));assert.deepEqual(clipped,[],n+' horizontal text clipping');
if(spec.kind==='modal'){const footer=await p.locator('#modal-'+n+' .modal-footer').boundingBox();assert.ok(footer.y>=0&&footer.y+footer.height<=540,n+' footer outside phone');}
await p.screenshot({path:path.join(root,n,'renders','ivory-bottom-360x540.png')});checks.push({screen:n,check:'Long content reachable; no clipped labels; modal footer remains visible',...metrics});
}
await goto('Player');for(const mode of ['10','100','MAX','1']){await p.click('#player-quantity-toggle');await p.click('#player-quantity-'+mode);assert.equal(await p.evaluate(()=>screenPreview.getState().playerQuantity),mode);}assert.equal(await p.locator('#upgrade-list>article:not([hidden])').count(),10);checks.push({check:'Player quantity menu and next ten unlockables'});
await goto('Settings','?state=consent-required');await p.locator('#settings-consent').scrollIntoViewIfNeeded();assert.equal(await p.locator('#settings-consent').isVisible(),true);await p.click('#settings-privacy');await p.locator('#privacy-consent').scrollIntoViewIfNeeded();await p.click('#back-Privacy');assert.equal(await p.evaluate(()=>screenPreview.getState().modal),'Settings');checks.push({check:'Conditional privacy controls and nested return'});
await goto('Prestige','?state=ready');await p.locator('#modal-Prestige .modal-body').evaluate(e=>e.scrollTop=e.scrollHeight);assert.equal(await p.locator('#confirm-prestige').isEnabled(),true);checks.push({check:'Ready prestige confirm remains accessible'});
await p.setViewportSize({width:450,height:916});for(const n of Object.keys(specs)){await goto(n);await p.screenshot({path:path.join(root,n,'renders','ivory-bazaar-450x916.png')});}
await p.setViewportSize({width:1280,height:900});await p.goto(pathToFileURL(path.join(root,'index.html')).href);await p.evaluate(()=>document.fonts.ready);await p.locator('footer').scrollIntoViewIfNeeded();await p.evaluate(()=>Promise.all([...document.images].map(i=>i.decode())));assert.equal(await p.locator('.screen-card').count(),13);await p.evaluate(()=>scrollTo(0,0));await p.screenshot({path:path.join(root,'gallery-ivory.png'),fullPage:true});checks.push({check:'Gallery has 13 previews; every image loads'});
assert.deepEqual(errors,[]);fs.writeFileSync(path.join(root,'ivory-review-verification.json'),JSON.stringify({status:'pass',checks,browserErrors:errors},null,2));console.log('PASS: '+checks.length+' additional Ivory Bazaar review checks');
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1;});
