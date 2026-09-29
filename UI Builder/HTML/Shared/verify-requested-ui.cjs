/* Regression checks for user-requested removals and compact Products controls. */
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
const {pathToFileURL}=require('url');
const {chromium}=require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root=path.resolve(__dirname,'..'),specs=require('../../screen-specs.json');
(async()=>{
 for(const name of Object.keys(specs)){
  const html=fs.readFileSync(path.join(root,name,name.toLowerCase()+'.html'),'utf8');
  assert.ok(!/Scroll to explore more|class="panel-flower"|class="detail-cue"|id="product-normal"/.test(html),name+' restored removed content');
 }
 const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
 try{
  const p=await browser.newPage();
  for(const width of [360,450]){
   await p.setViewportSize({width,height:916});
   await p.goto(pathToFileURL(path.join(root,'Player/player.html')).href+'?screen=Home');
   await p.evaluate(()=>document.fonts.ready);
   const height=(await p.locator('.world').boundingBox()).height;
   for(const name of ['Player','Products','Legacy']){
    const button=p.getByRole('button',{name,exact:true}),base=await button.boundingBox();
    await button.click();assert.ok((await button.boundingBox()).height>base.height);
    assert.equal((await p.locator('.world').boundingBox()).height,height);
    await button.click();assert.equal((await button.boundingBox()).height,base.height);
   }
   await p.click('#nav-Player');
   assert.equal(await p.locator('#upgrade-list>article:not([hidden])').count(),10);
   assert.equal(await p.evaluate(()=>getComputedStyle(document.querySelector('.player-toolbar'),'::before').backgroundImage),'none');
   await p.click('#nav-Products');
   assert.match(await p.locator('#product-total').innerText(),/^Profit/);
   assert.equal(await p.locator('.product-display-frame').count(),8);
   assert.equal(await p.locator('.product-name-level').count(),8);
   for(const mode of ['10','100','MAX','1']){
    await p.click('#product-quantity-toggle');await p.click('#quantity-'+mode);
    assert.equal(await p.evaluate(()=>screenPreview.getState().quantity),mode);
    assert.equal(await p.locator('#product-quantity-options').isVisible(),false);
   }
  }
  console.log('PASS: all generated pages retain requested removals; Products controls, next ten unlockables and expanding navigation verified at two phone widths.');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1});
