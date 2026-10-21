// Editable grayscale review artifacts only. Does not generate art, HTML, or Unity layouts.
const fs = require('fs'), path = require('path');
const out = path.resolve('UI Builder/Wireframes/Redesign');
fs.mkdirSync(out, {recursive:true});
const esc = s => String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;');
let p=[];
const text=(x,y,s,size=14,weight=400,fill='#333')=>p.push(`<text x="${x}" y="${y}" font-family="Segoe UI, Arial, sans-serif" font-size="${size}" font-weight="${weight}" fill="${fill}">${esc(s)}</text>`);
const box=(x,y,w,h,fill='#fafafa',radius=12)=>p.push(`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${radius}" fill="${fill}" stroke="#555" stroke-width="1"/>`);
function lines(x,y,items,size=14,step=22){items.forEach((s,i)=>text(x,y+i*step,s,size));}
function panel(y,h,title,items=[]){box(20,y,350,h);text(36,y+27,title,17,600);lines(36,y+52,items);}
function button(x,y,w,label,primary=false){box(x,y,w,48,primary?'#333':'#fff',10);text(x+12,y+30,label,14,600,primary?'#fff':'#222');}
function nav(active){box(0,772,390,72,'#eee',0);['Home','Upgrades','Talent','Prestige'].forEach((s,i)=>{if(s===active)box(i*97.5+5,782,87,48,'#333',8);text(i*97.5+12,812,s,13,600,s===active?'#fff':'#333');});}
function header(title,subtitle){text(20,39,title,25,700);if(subtitle)text(20,65,subtitle,13);}
const screens=[];
function screen(name,title,subtitle,draw){p=[];box(0,0,390,844,'#fff',0);header(title,subtitle);draw();const svg=`<svg xmlns="http://www.w3.org/2000/svg" width="390" height="844" viewBox="0 0 390 844"><title>${esc(title)} — redesign wireframe v1</title>${p.join('')}</svg>`;const stem=name.toLowerCase();fs.writeFileSync(path.join(out,`${stem}-v1.svg`),svg);screens.push({name,stem,title});}
screen('Home','Rocket Raja','₹30 cash       Normal sales ₹0/s',()=>{
 button(286,82,84,'Settings');
 box(20,146,350,282,'#eee');text(38,176,'YOUR FIREWORKS STALL',16,600);
 ['Phuljhari','Anar','Chakri','Rockets','Small crackers','Strings','Aerial shots','Boxes'].forEach((s,i)=>{const x=30+(i%4)*88,y=194+Math.floor(i/4)*102;box(x,y,78,88,'#fff',8);text(x+5,y+29,String(i+1),22,600);text(x+5,y+53,s.length>10?s.slice(0,10):s,10);text(x+5,y+73,i===0?'Tap for details':'Locked',10);});
 panel(442,96,'Start your shop',['Buy your first Phuljhari for ₹30.','Your first cash grant is ready to use.']);
 button(20,550,350,'Buy Phuljhari · ₹30',true);
 button(20,610,350,'Tap power · ₹0.85/tap · Train');
 ['Festival Rush','Helping Hands','Bulk Order'].forEach((s,i)=>{box(20+i*120,676,110,76);text(28+i*120,700,s,12,600);text(28+i*120,722,'Buy in Upgrades',11);text(28+i*120,742,'View skill',12);});nav('Home');
});
screen('Upgrades','Upgrades','₹2,000 cash        First purchases 249 / 1,000',()=>{
 ['All','Products','Tapping','Skills','Shop'].forEach((s,i)=>button(20+i*70,88,66,s,s==='All'));
 button(20,146,170,'Product: All ▾');button(200,146,170,'Search upgrades');
 ['Available','Upcoming','Owned','Full'].forEach((s,i)=>button(20+i*88,204,82,s,s==='Available'));
 panel(268,146,'Phuljhari Signature I',['First purchase · x2 Phuljhari profit','₹115.279  ·  Shop ₹0.85 → ₹1.70/s']);button(36,356,140,'Details');button(202,356,150,'Buy',true);
 panel(428,146,'Sales Pitch I',['Rebuy · x1.20 direct tap earnings','₹234.692']);button(36,514,140,'Details');button(202,514,150,'Buy',true);
 panel(590,70,'Festival Rush: Unlock',['₹386.031 · View skill details']);
 text(20,687,'Scroll to see every matching entry',13);
 button(20,704,350,'Review Buy All · All upgrades',true);nav('Upgrades');
});
screen('UpgradeDetails','Upgrade details','Back to Upgrades · filter and scroll retained',()=>{
 panel(90,130,'Phuljhari Signature I',['First purchase · cash upgrade','Doubles Phuljhari normal income.','Complete signature effect.']);
 panel(236,122,'Before → after',['Phuljhari: ₹0.85 → ₹1.70/s','Normal shop: ₹0.85 → ₹1.70/s','Contribution: 100% → 100%']);
 panel(374,112,'Requirements',['Own Phuljhari ✓','No earlier upgrade required.']);
 panel(502,96,'Cash price: ₹115.279',['Expand numbers for full precision.','Counts once toward lifetime purchases.']);
 button(20,618,170,'Pin as goal');button(200,618,170,'Buy upgrade',true);
 button(20,682,350,'Back to filtered catalogue');nav('Upgrades');
});
screen('BuyAll','Review Buy All','Scope: Products / Phuljhari / Available',()=>{
 panel(92,122,'Your reviewed purchase list',['Cash budget at review: ₹2,000','3 upgrades · 3 first purchases','Total: ₹472.062']);
 panel(230,230,'Included upgrades',['1  Phuljhari Signature I   ₹115.279','2  Festival Display I       ₹153.197','3  Regular Customers I  ₹203.586','','Later rows may unlock during this batch.','Each listed price includes prior discounts.']);
 panel(478,116,'What this buys',['Only the three cash upgrades above.','Product / tap levels stay as they are.','Your free credits and KK remain available.']);
 text(20,622,'Normal sales continue while you review.',13);
 text(20,647,'New earnings do not add to this list.',13);
 button(20,688,165,'Cancel');button(200,688,170,'Buy 3 · ₹472.062',true);nav('Upgrades');
});
screen('Talent','Talent','Permanent cards · unlocks after first prestige',()=>{
 panel(92,90,'3 free draws       0 Kaju Katli',['Cards apply automatically when relevant.']);
 text(20,211,'Common 75% · Rare 22% · Legendary 3%',13,600);
 text(20,235,'Equal odds for each card in its tier.  [Odds]',12);
 [['Phuljhari Showcase','Common · Lv 1'],['Anar Showcase','Common · Unowned'],['Night Sales','Rare · Lv 2'],['Festival Rhythm','Rare · Unowned'],['Festival Brand','Legendary · Unowned'],['Master Negotiator','Legendary · Unowned']].forEach((c,i)=>{const x=20+(i%2)*180,y=258+Math.floor(i/2)*104;box(x,y,170,92);text(x+10,y+29,c[0],13,600);text(x+10,y+52,c[1],12);text(x+10,y+76,'View card',12);});
 text(20,596,'Collection: 28 cards · scroll for all',13);
 button(20,618,350,'Use 1 free draw · 3 available',true);
 button(20,678,230,'Draw · 100 Kaju Katli');button(262,678,108,'KK shop');nav('Talent');
});
screen('CardDetails','Phuljhari Showcase','Common · 4.6875% chance per draw',()=>{
 panel(92,188,'LEVEL 1',['[Card illustration area]','','Automatically boosts Phuljhari profit.','All copies remain after prestige.']);
 panel(296,124,'One more copy → level 2',['Current: x1.20 product income','Next: x1.40 product income','Every duplicate adds exactly one level.']);
 panel(436,128,'How this card works',['Applies after you own Phuljhari.','No equipment slot or merge cost.','No maximum card level.']);
 panel(582,78,'Draw odds',['Common 75% ÷ 16 cards = 4.6875%']);
 button(20,690,350,'Back to Talent');nav('Talent');
});
screen('DrawReveal','Your Talent card','Saved to your collection',()=>{
 panel(96,256,'PHULJHARI SHOWCASE',['Common','','[Card illustration area]','','Duplicate · level 1 → level 2']);
 panel(374,132,'Phuljhari income improves',['x1.20 → x1.40','The new level is already applied.','You can close and return safely.']);
 panel(526,88,'2 free draws remain',['Kaju Katli balance: 0']);
 button(20,640,350,'Continue to collection',true);button(20,700,350,'Skip animation');nav('Talent');
});
screen('Prestige','Prestige','Ready · earned ₹10B / ₹10B this run',()=>{
 panel(90,104,'Guaranteed permanent income',['Fame: 0 → 1','Income multiplier: x1 → x6']);
 panel(210,112,'3 free Talent draws',['First prestige always awards 3 credits.','Card results are random; credits persist.']);
 panel(338,174,'Reset / keep',['RESET   Cash, product and tap levels,','              cash upgrades, skills and effects.','','KEEP     Fame, cards, KK, unused credits,','              lifetime purchases, decorations.']);
 panel(528,114,'Rebuild with your permanent gains',['Start with ₹30 for a manual Phuljhari buy.','Next run target: ₹15B','Stay longer: +2 Fame at ₹40B earned.']);
 button(20,687,165,'Continue run');button(200,687,170,'Confirm prestige',true);nav('Prestige');
});
screen('KajuShop','Kaju Katli shop','Balance: 100 KK       One draw costs 100 KK',()=>{
 [['Small box','1,000 KK · 10 draws','₹99'],['Festival box','6,000 KK · 60 draws','₹499'],['Celebration box','13,000 KK · 130 draws','₹999'],['Grand celebration','28,000 KK · 280 draws','₹1,999']].forEach((r,i)=>{const y=96+i*119;panel(y,105,r[0],[r[1]]);button(242,y+40,111,r[2],true);});
 panel(584,86,'Same draw pool for everyone',['Common 75% · Rare 22% · Legendary 3%','No purchase guarantees a rarity.']);
 button(20,692,165,'Purchase help');button(200,692,170,'Back to Talent');nav('Talent');
});
screen('Offline','Welcome back','Your offline earnings have been added',()=>{
 panel(94,118,'+₹144M cash',['Away for 10 hours','Normal sales at departure: ₹10,000/s']);
 panel(230,152,'How this was earned',['Credited time: 8 hours / 8-hour storage','Offline efficiency: 50%','2 hours were beyond your storage.','Skills and tapping are excluded.']);
 panel(400,122,'Your next step',['A pinned upgrade is now affordable.','Your skills are ready to inspect.']);
 button(20,550,350,'View pinned upgrade',true);button(20,614,350,'Continue to shop');
 text(20,697,'Closing this screen keeps the credited cash.',13);nav('Home');
});
screen('ProductDetails','Phuljhari','Owned · level 1',()=>{
 panel(92,154,'Normal income: ₹0.85/s',['Shop contribution: 100%','Next milestone: level 25 → x2','[Run-specific product display]']);
 panel(266,112,'Buy product levels',['Next level costs ₹33.60','Cash upgrades and Talent affect this quote.']);
 ['x1','x10','x100','Max'].forEach((s,i)=>button(20+i*90,396,80,s,s==='x1'));
 button(20,462,350,'Buy 1 level · ₹33.60',true);
 panel(534,94,'Cash upgrades for Phuljhari',['Inspect profits, discounts and links.']);
 button(20,648,350,'Open filtered Upgrades');button(20,708,350,'Back to shop');
});
screen('SkillDetails','Festival Rush','Locked this run · buy the base skill to activate',()=>{
 panel(92,156,'A burst of festival trade',['Products produce x2 income for 30s.','Manual / Helping Hands taps receive x1.25.','Tap links and Bulk Order use normal sales.']);
 panel(266,116,'Cooldown: 5 minutes',['Starts when you activate Rush.','Repurchased base skills start ready.']);
 panel(400,138,'Buy the skill this run',['Own at least one product family.','Festival Rush: Unlock · ₹386.031','Permanent cards improve an owned skill.']);
 button(20,558,350,'View base skill in Upgrades',true);button(20,624,350,'View skill power / timing upgrades');button(20,690,350,'Back to shop');
});
screen('TapTraining','Tap training','Level 0 · current payout ₹0.85 per tap',()=>{
 panel(92,136,'A more valuable tap',['Next level: ₹0.85 → ₹2.04 per tap','Linked product income: ₹0 for now','Only taps on the open stall earn cash.']);
 panel(248,98,'Tap level price: ₹40',['Run discounts and Talent reduce this cost.']);
 ['x1','x10','x100','Max'].forEach((s,i)=>button(20+i*90,366,80,s,s==='x1'));
 button(20,436,350,'Buy 1 tap level · ₹40',true);
 panel(512,112,'Tapping cash upgrades',['Direct tap profits and shop-linked payouts','are listed together in Upgrades.']);
 button(20,648,350,'Open Tapping upgrades');button(20,710,350,'Back to shop');
});
fs.writeFileSync(path.join(out,'review-index.json'),JSON.stringify({revision:'v1',stage:'wireframe',approval:'waived',waiver:'User explicitly instructed skip UI Approval on 2026-10-02; no claim of visual acceptance',handoff:'../../HTML/Redesign/ART-DIRECTION.md',screens},null,2)+'\n');
async function render(){const sharp=require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');for(const s of screens)await sharp(path.join(out,s.stem+'-v1.svg'),{density:144}).png().toFile(path.join(out,s.stem+'-v1.png'));const tiles=[];for(let i=0;i<screens.length;i++){const input=await sharp(path.join(out,screens[i].stem+'-v1.png')).resize(390,844).png().toBuffer();tiles.push({input,left:20+(i%4)*410,top:50+Math.floor(i/4)*880});}await sharp({create:{width:1660,height:50+Math.ceil(screens.length/4)*880,channels:4,background:'#ddd'}}).composite(tiles).png().toFile(path.join(out,'review-board-v1.png'));console.log(`Rendered ${screens.length} editable wireframes and review board.`);}
render().catch(e=>{console.error(e.message);process.exitCode=1});
