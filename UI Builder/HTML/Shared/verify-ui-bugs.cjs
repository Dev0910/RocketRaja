const assert = require('assert/strict');
const path = require('path');
const {pathToFileURL} = require('url');
const {chromium} = require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
(async () => {
  const browser = await chromium.launch({headless:true, executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
  try {
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push(e.message));
    for (const [width, height] of [[360,540],[450,916]]) {
      await page.setViewportSize({width,height});
      await page.goto(pathToFileURL(path.resolve(__dirname,'../Player/player.html')).href + '?screen=Home');
      await page.evaluate(() => document.fonts.ready);
      for (const tab of ['Player','Products']) {
        await page.evaluate(tab => screenPreview.navigate(tab), tab);
        await page.waitForTimeout(300);
        const prefix = tab === 'Player' ? 'player' : 'product';
        await page.locator('#'+prefix+'-quantity-toggle').click();
        await page.waitForTimeout(200);
        const boxes = await page.locator('#'+prefix+'-quantity-options button').evaluateAll(elements => elements.map(el => {const r=el.getBoundingClientRect();return {x:r.x,y:r.y,bottom:r.bottom};}));
        assert.equal(boxes.length,4);
        boxes.slice(1).forEach((r,i) => { assert.ok(Math.abs(r.x-boxes[i].x)<1); assert.ok(r.y>=boxes[i].bottom); });
        assert.ok(boxes.at(-1).bottom<=height);
      }
      await page.evaluate(() => screenPreview.navigate('Home'));
      await page.waitForTimeout(300);
      const rect = await page.locator('#stall-tap').boundingBox();
      const x = rect.x + rect.width*.4, y = rect.y + rect.height*.4;
      await page.mouse.click(x,y);
      const popup = await page.locator('.cash-burst').evaluateAll(elements => {
        const el=elements.find(e=>e.textContent),r=el.getBoundingClientRect();
        return {x:r.x,color:getComputedStyle(el).color,top:parseFloat(el.style.top),parentY:el.parentElement.getBoundingClientRect().y};
      });
      assert.ok(Math.abs(popup.x-x)<1);
      assert.ok(Math.abs(popup.top+popup.parentY-y)<1);
      assert.equal(popup.color,'rgb(22, 75, 128)');
      await page.evaluate(() => screenPreview.navigate('Settings'));
      const style=await page.locator('#back-Settings').evaluate(el=>({face:getComputedStyle(el).getPropertyValue('--folk-face'),color:getComputedStyle(el).color}));
      assert.ok(style.face.includes('blue.svg'));
      assert.equal(style.color,'rgb(255, 255, 255)');
    }
    assert.deepEqual(errors,[]);
    console.log('PASS: vertical menus, pointer-positioned blue feedback and blue Settings button at both phone sizes.');
  } finally { await browser.close(); }
})().catch(error => {console.error(error);process.exitCode=1;});
