const fs=require('fs'),path=require('path'),assert=require('assert/strict'),{pathToFileURL}=require('url');
const {chromium}=require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root=path.resolve(__dirname,'..'),out=path.resolve(root,'Shared/renders/coverage');fs.mkdirSync(out,{recursive:true});
(async()=>{const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'}),p=await browser.newPage({viewport:{width:450,height:916}}),errors=[],checks=[];p.on('pageerror',e=>errors.push(e.message));p.on('requestfailed',r=>errors.push(r.url()));
try{
await p.goto(pathToFileURL(path.join(root,'Player/player.html')).href+'?screen=Home');
await p.evaluate(()=>document.fonts.ready);
const states=await p.evaluate(()=>coveragePreview.extraStates);
const cards=[];
for(const [state,label] of Object.entries(states)){
 await p.evaluate(s=>screenPreview.setFixture(s),state);
 await p.waitForTimeout(220); await p.screenshot({path:path.join(out,state+'.png')});
 const report=await p.evaluate(()=>({overflow:document.documentElement.scrollWidth>innerWidth,broken:[...document.images].filter(i=>!i.complete||!i.naturalWidth).map(i=>i.src)}));
 assert.equal(report.overflow,false,state);assert.deepEqual(report.broken,[],state);
 checks.push({state,result:'pass'});cards.push('<a href="Player/player.html?screen=Home&state='+state+'&review=1"><img loading="lazy" src="Shared/renders/coverage/'+state+'.png" alt=""><strong>'+label+'</strong><small>'+state+'</small></a>');
}
for(const [state,label] of [['save-failed','Progress could not be saved'],['backup-recovered','Your last backup was recovered']]){
await p.evaluate(s=>screenPreview.setFixture(s),state);assert.match(await p.locator('#preview-toast').innerText(),new RegExp(label));
}
await p.evaluate(()=>screenPreview.setFixture('sponsor-expiring'));
assert.equal(await p.locator('#home-sponsor').isDisabled(),true);
await p.evaluate(()=>screenPreview.advanceSponsor(5));assert.equal(await p.locator('#home-sponsor').isDisabled(),false);
assert.match(await p.locator('#preview-toast').innerText(),/ended/);
await p.evaluate(()=>screenPreview.setFixture('all-displays'));assert.equal(await p.locator('#home-milestone-label').innerText(),'PRESTIGE');
await p.evaluate(()=>screenPreview.setFixture('partial-shop'));assert.match(await p.locator('#home-next').innerText(),/Rockets/);
await p.evaluate(()=>screenPreview.setFixture('post-prestige'));assert.equal(await p.locator('#reputation').textContent(),'17');assert.equal(await p.locator('#rank-legacy-phuljhari').textContent(),'Rank 1 / 5');
await p.evaluate(()=>screenPreview.setFixture('display-100'));await p.click('#nav-Products');await p.waitForTimeout(350); await p.screenshot({path:path.join(out,'products-100.png')});await p.click('#detail-phuljhari');
assert.match(await p.locator('#detail-art image').getAttribute('href'),/products-100/);
await p.keyboard.press('Escape');await p.click('#nav-Products');const cash=await p.locator('#cash-value').textContent();await p.click('#stall-tap',{position:{x:100,y:100}});
assert.equal(await p.locator('#cash-value').textContent(),cash);assert.match(await p.locator('#tap-feedback').textContent(),/\+₹/);
await p.emulateMedia({reducedMotion:'reduce'});await p.evaluate(()=>screenPreview.setFixture('ambient'));
assert.equal(await p.locator('.visitor-layer').isVisible(),false);
await p.evaluate(()=>coveragePreview.celebrate());assert.equal(await p.locator('.celebration-spark').evaluateAll(es=>es.flatMap(e=>e.getAnimations()).length),0);
assert.deepEqual(errors,[]);
fs.writeFileSync(path.join(out,'results.json'),JSON.stringify({checks,errors,verified:['Sponsor expires and re-enables','Milestones follow ownership and display completion','Legacy retained after prestige','Tier artwork reaches detail dialog','Tap feedback leaves cash unchanged','Reduced motion suppresses ambient and celebrations']},null,2));
fs.writeFileSync(path.join(root,'state-gallery.html'),'<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>UI state review</title><style>body{margin:0;padding:32px;background:#fff6df;color:#174a7c;font:17px system-ui}h1{font-family:Georgia}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(210px,1fr));gap:20px}a{color:inherit;background:#fffcf1;border:1px solid #d9b76b;padding:12px;text-decoration:none;border-radius:8px}img{width:100%;aspect-ratio:450/916;object-fit:contain}strong,small{display:block;margin-top:8px}p{max-width:850px;line-height:1.5}</style><h1>Missing UI states — review gallery</h1><p>Open a card to inspect the interactive fixture. Use the Screen selector to view the same state in Products, Player or a popup. These are illustrative HTML states; no game saves, advertising or economy are connected.</p><main>'+cards.join('')+'</main></html>');
console.log('PASS: '+checks.length+' new states, transitions, artwork, feedback and reduced motion. Gallery generated.');
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1});

