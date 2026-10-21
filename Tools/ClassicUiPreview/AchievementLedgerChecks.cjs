const fs=require('fs');
const {chromium}=require(process.argv.includes('--playwright-module')?process.argv[process.argv.indexOf('--playwright-module')+1]:'playwright');
(async()=>{
 const browser=await chromium.launch({headless:true});const page=await browser.newPage({viewport:{width:390,height:844}});
 const issues=[];page.on('pageerror',e=>issues.push(e.message));
 await page.goto('http://127.0.0.1:8769/Achievements/index.html',{waitUntil:'networkidle'});await page.evaluate(()=>document.fonts.ready);
 const report=[];const check=(value,label)=>{if(!value)throw new Error(label);report.push('PASS '+label);};
 check(await page.locator('.ledger-row').count()===15,'All 15 stable achievement rows render');
 check(await page.evaluate(()=>{const m=JSON.parse(document.getElementById('unity-metadata').textContent);return m.actions.ClaimAchievement.triggerIds.length===15&&m.actions.ClaimAchievement.triggerIds.every(id=>document.getElementById(id));}),'All declared achievement action IDs resolve in rendered DOM');
 check(await page.locator('.ledger-ready').count()===4,'Selected reference fixture has 4 ready rewards');
 check((await page.locator('.ledger-summary').innerText()).includes('410'),'Reference wallet is 410 Kaju Katli');
 for(const [width,height]of [[390,844],[360,540],[360,640],[450,916]]){
  await page.setViewportSize({width,height});await page.waitForTimeout(80);
  const fit=await page.evaluate(()=>{
   const card=document.querySelector('.modal-card'),body=document.querySelector('.modal-body'),r=card.getBoundingClientRect();
   return {fits:r.x>=0&&r.right<=innerWidth+.5&&r.y>=0&&r.bottom<=innerHeight+.5,horizontal:body.scrollWidth<=body.clientWidth+1,
    targets:[...document.querySelectorAll('.ledger-claim,.close')].every(b=>b.getBoundingClientRect().height>=47.5&&b.getBoundingClientRect().width>=47.5)};
  });
  check(fit.fits&&fit.horizontal&&fit.targets,`${width}x${height}: popup fits, no horizontal overflow, 48px controls`);
  await page.screenshot({path:`Documentation/AchievementValidation/RewardLedger/html-${width}x${height}.png`});
 }
 await page.setViewportSize({width:390,height:844});
 await page.locator('#achievement-claim-products-1').click();
 check(await page.evaluate(()=>window.achievementPreview.state.kk)===415,'Real preview Claim action adds exact +5 reward');
 check(await page.locator('#achievement-products-1').getAttribute('data-phase')==='claimed','Claim updates state and reorders the row');
 check(await page.locator('#achievement-claim-products-1').isDisabled(),'Claimed action becomes disabled');
 check(await page.evaluate(()=>!window.classicAchievements.claim('products-1',window.achievementPreview.state,window.classicFixtures.progressed,window.classicContent)&&window.achievementPreview.state.kk===415),'Stale preview callback cannot duplicate the reward');
 await page.locator('#achievement-close').click();check(await page.locator('#achievement-modal').isHidden(),'Native-equivalent close action dismisses the ledger');
 await page.locator('#achievement-open').click();check(await page.locator('.ledger-row').count()===15,'Reopen retains exactly 15 rows');
 await page.goto('http://127.0.0.1:8769/Achievements/index.html?state=new',{waitUntil:'networkidle'});
 check(await page.locator('.ledger-ready').count()===0&&await page.locator('.ledger-claim:not(:disabled)').count()===0,'New game has no premature claim actions');
 await page.goto('http://127.0.0.1:8769/Achievements/index.html?condition=save-error',{waitUntil:'networkidle'});
 check(await page.locator('.ledger-claim:not(:disabled)').count()===0,'Unavailable saving disables every claim');
 await page.goto('http://127.0.0.1:8769/Classic/index.html?screen=Achievements&condition=ledger-reference',{waitUntil:'networkidle'});
 check(await page.locator('.modal-layer[data-screen=Achievements] .ledger-row').count()===15,'Current Classic preview routes to the same ledger');
 check(issues.length===0,'Browser reports no JavaScript errors');
 fs.writeFileSync('Documentation/AchievementValidation/RewardLedger/browser-checks.txt',report.join('\n')+'\n');console.log(report.join('\n'));
 await browser.close();
})().catch(e=>{console.error(e);process.exitCode=1;});
