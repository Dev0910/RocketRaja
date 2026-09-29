/* Visual regression evidence for the icon refresh. No game-state mutation. */
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const {pathToFileURL} = require('node:url');
const {chromium} = require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root = path.resolve(__dirname, '..');
const review = path.resolve(root, '../Reviews/IconRefresh');
const baselineMode = process.argv.includes('--baseline');
const names = ['Home', ...Object.keys(require('../../screen-specs.json'))];
const entries = names.map(name => ({name, file: name === 'Home' ? 'Player/player.html' : `${name}/${name.toLowerCase()}.html`, query: name === 'Home' ? '?screen=Home' : ''}));
entries.push({name: 'StandaloneHome', file: 'Home/home.html', query: ''});
fs.mkdirSync(review, {recursive: true});
const hash = data => crypto.createHash('sha256').update(data).digest('hex');
(async () => {
  const browser = await chromium.launch({headless: true, executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
  const page = await browser.newPage();
  const errors = [], checks = [], signatures = {};
  page.on('pageerror', e => errors.push(e.message));
  page.on('requestfailed', r => errors.push(r.url()));
  const baselineFile = path.join(review, 'baseline.json');
  const baseline = baselineMode ? null : JSON.parse(fs.readFileSync(baselineFile, 'utf8'));
  try {
    for (const entry of entries) {
      const html = fs.readFileSync(path.join(root, entry.file), 'utf8');
      const metadata = html.match(/<script id="unity-metadata" type="application\/json">([\s\S]*?)<\/script>/)[1];
      signatures[entry.name] = hash(JSON.stringify(JSON.parse(metadata)));
      if (!baselineMode) assert.equal(signatures[entry.name], baseline.metadata[entry.name], entry.name + ' behavior contract changed');
      for (const [width, height] of [[360,540], [390,844], [450,916], [1080,2200]]) {
        await page.setViewportSize({width, height});
        await page.goto(pathToFileURL(path.join(root, entry.file)).href + entry.query);
        await page.evaluate(() => document.fonts.ready);
        const metrics = await page.evaluate(() => {
          const world = document.querySelector('.world').getBoundingClientRect();
          const nav = document.querySelector('.navigation').getBoundingClientRect();
          const visible = e => !e.closest('[hidden],[inert]') && e.getBoundingClientRect().width > 0;
          const useNodes = [...document.querySelectorAll('use')].filter(visible);
          return {
            world: [world.x, world.y, world.width, world.height],
            navigation: [nav.x, nav.y, nav.width, nav.height],
            missingSymbols: useNodes.map(e => e.getAttribute('href')).filter(id => !document.querySelector(id)),
            icons: useNodes.map(e => ({name: e.getAttribute('href'), width: e.ownerSVGElement.getBoundingClientRect().width, height: e.ownerSVGElement.getBoundingClientRect().height})),
            smallTargets: [...document.querySelectorAll('[data-unity-action]')].filter(visible).map(e => ({id: e.id, width: e.getBoundingClientRect().width, height: e.getBoundingClientRect().height})).filter(e => e.width < 47.9 || e.height < 47.9),
            overflow: document.documentElement.scrollWidth > innerWidth || document.documentElement.scrollHeight > innerHeight,
            navLabels: [...document.querySelectorAll('.navigation button')].map(e => e.getAttribute('aria-label')),
          };
        });
        assert.deepEqual(metrics.missingSymbols, [], entry.name + ' missing icon symbols');
        assert.deepEqual(metrics.smallTargets, [], entry.name + ' small controls');
        assert.equal(metrics.overflow, false, entry.name + ' overflow');
        assert.deepEqual(metrics.navLabels, ['Player', 'Products', 'Legacy']);
        if (!baselineMode) {
          const before = baseline.checks.find(c => c.screen === entry.name && c.viewport[0] === width);
          assert.deepEqual(metrics.world, before.world, entry.name + ' stall geometry');
          assert.deepEqual(metrics.navigation, before.navigation, entry.name + ' navigation geometry');
          assert.ok(metrics.icons.every(i => i.width >= 16 && i.height >= 16), entry.name + ' unreadably small icons');
          const folder = path.join(root, entry.name === 'StandaloneHome' ? 'Home' : entry.name, 'renders');
          await page.screenshot({path: path.join(folder, `icons-${entry.name === 'StandaloneHome' ? 'standalone-' : ''}${width}x${height}.png`)});
        }
        checks.push({screen: entry.name, viewport: [width,height], ...metrics});
      }
    }
    assert.deepEqual(errors, []);
    fs.writeFileSync(path.join(review, baselineMode ? 'baseline.json' : 'verification.json'), JSON.stringify({status: 'pass', metadata: signatures, checks, errors}, null, 2));
    console.log(`PASS: ${checks.length} ${baselineMode ? 'baseline captures' : 'icon, geometry and accessibility checks'}; ${entries.length} behavior contracts ${baselineMode ? 'recorded' : 'unchanged'}.`);
  } finally { await browser.close(); }
})().catch(e => { console.error(e); process.exitCode = 1; });
