/* Shared painted icon presentation. Crops are viewports; atlas pixels stay untouched. */
const fs = require('node:fs');
const path = require('node:path');
const manifest = require('./HTML/Shared/icons/manifest.json');
const controls = {
  chevron: '<path d="M24 36 50 62 76 36" fill="none" stroke="currentColor" stroke-width="10" stroke-linecap="round" stroke-linejoin="round"/>',
  close: '<path d="M28 28 72 72M72 28 28 72" fill="none" stroke="currentColor" stroke-width="10" stroke-linecap="round"/>',
  add: '<path d="M50 24V76M24 50H76" fill="none" stroke="currentColor" stroke-width="8" stroke-linecap="round"/>'
};
function icon(name) {
  if (!manifest.icons[name] && !controls[name]) throw new Error(`Unknown icon: ${name}`);
  return `<svg class="game-icon${controls[name] ? ' utility-icon' : ''}" data-icon="${name}" viewBox="0 0 100 100" aria-hidden="true" focusable="false"><use href="#icon-${name}"/></svg>`;
}
function symbols(atlasPath = '../Shared/icons/' + manifest.atlas) {
  const defs = Object.entries(manifest.icons).map(([name, spec]) => {
    const [x,y,w,h] = spec.crop;
    // Preserve each silhouette's proportions, normalize its longest edge, and leave a safe gutter.
    const scale = 92 / Math.max(w,h), dx = (100-w*scale)/2-x*scale, dy = (100-h*scale)/2-y*scale;
    return `<clipPath id="icon-${name}-crop" clipPathUnits="userSpaceOnUse"><rect x="${x}" y="${y}" width="${w}" height="${h}"/></clipPath><symbol id="icon-${name}" viewBox="0 0 100 100"><g transform="translate(${dx} ${dy}) scale(${scale})"><image href="${atlasPath}" width="${manifest.width}" height="${manifest.height}" clip-path="url(#icon-${name}-crop)"/></g></symbol>`;
  }).join('');
  return `<svg class="symbol-library" aria-hidden="true" focusable="false" xmlns="http://www.w3.org/2000/svg"><defs>${defs}${Object.entries(controls).map(([name,body]) => `<symbol id="icon-${name}" viewBox="0 0 100 100">${body}</symbol>`).join('')}</defs></svg>`;
}
function buildCatalogue() {
  const rows = Object.entries(manifest.icons).map(([name,spec]) => `<article><div class="samples"><div class="paper">${icon(name)}</div><div class="blue">${icon(name)}</div><div class="raspberry">${icon(name)}</div><div class="small">${icon(name)}${icon(name)}${icon(name)}</div></div><h2>${spec.label}</h2><p>${spec.use}</p></article>`).join('');
  const html = `<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Diwali Dukaan · Icon collection</title><style>@font-face{font-family:Outfit;src:url('../Home/assets/Outfit.ttf')}*{box-sizing:border-box}body{margin:0;background:#fff4db url('../Home/assets/gouache-paper.svg');color:#164b80;font-family:Outfit,Arial,sans-serif}main{max-width:1120px;margin:auto;padding:36px 28px}header{border-bottom:4px double #164b80;margin-bottom:24px;padding-bottom:20px}h1{font-size:40px;margin:6px 0 12px}header p{max-width:680px;font-size:16px;line-height:1.5}a{color:#164b80}.eyebrow{font-size:12px;letter-spacing:2px;font-weight:700}.grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:18px}article{background:#fff8e8;padding:14px;border:1px solid #b7c3c5;border-radius:3px}h2{font-size:18px;margin:14px 0 5px}p{font-size:12px;line-height:1.45;margin:0;min-height:35px}.samples{display:grid;grid-template-columns:repeat(3,1fr);gap:5px}.samples>div{display:flex;align-items:center;justify-content:center;min-height:64px;border-radius:4px}.paper{background:#f7eacf}.blue{background:#1754a0}.raspberry{background:#a82149}.small{grid-column:1/-1;gap:16px;background:#f3ecd9}.game-icon{width:55px;height:55px;display:block;overflow:hidden}.small svg:nth-child(1){width:24px;height:24px}.small svg:nth-child(2){width:32px;height:32px}.small svg:nth-child(3){width:40px;height:40px}.symbol-library{position:absolute;width:0;height:0;overflow:hidden}.utilities{display:flex;gap:24px;align-items:center;margin-top:24px}.utilities svg{width:24px;height:24px}footer{border-top:3px double #164b80;margin-top:28px;padding-top:16px;font-size:13px}@media(max-width:700px){.grid{grid-template-columns:repeat(2,minmax(0,1fr))}main{padding:24px 14px}h1{font-size:30px}}</style></head><body>${symbols('icons/'+manifest.atlas)}<main><header><div class="eyebrow">DIWALI DUKAAN · IVORY BAZAAR</div><h1>Painted for the festival</h1><p>A shared family of sixteen Gouache Storybook icons, shown on ivory, cobalt and raspberry. Each is also shown at 24, 32 and 40 pixels.</p><a href="../index.html">Explore the game screens →</a></header><section class="grid">${rows}</section><div class="utilities">${icon('chevron')}${icon('close')}${icon('add')}<span>Simple controls inherit the surrounding UI color.</span></div><footer>Product families retain their matching painted retail artwork. All game labels, values and control targets remain live HTML.</footer></main></body></html>`;
  fs.writeFileSync(path.join(__dirname, 'HTML/Shared/icon-catalogue.html'), html);
}
function updateHome() {
  const file = path.join(__dirname, 'HTML/Home/home.html');
  let html = fs.readFileSync(file, 'utf8');
  html = html.replace(/<svg class="symbol-library"[\s\S]*?<\/defs>\s*<\/svg>/, symbols());
  html = html.replace(/<span class="portrait"><img[^>]*><\/span>/, icon('player'));
  html = html.replace(/<svg(?: class="([^"]*)")? aria-hidden="true"><use href="#icon-([a-z]+)"\/><\/svg>/g, (_, classes, name) => icon(name).replace('class="game-icon', 'class="'+(classes ? classes+' ' : '')+'game-icon'));
  if (!html.includes('href="../Shared/icons.css"')) html = html.replace('</head>', '<link rel="stylesheet" href="../Shared/icons.css"></head>');
  fs.writeFileSync(file, html);
}
module.exports = {icon, symbols, manifest, buildCatalogue, updateHome};
if (require.main === module) { updateHome(); buildCatalogue(); }
