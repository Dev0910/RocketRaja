import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
const sharp = require('C:/Users/Dev/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const out = path.dirname(fileURLToPath(import.meta.url));
const esc = s => String(s).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;');
let nodes = [], texts = [], targets = [];
const add = s => nodes.push(s);
function rect(x,y,w,h,fill='#fff',stroke='#999',radius=0,extra='') { add(`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${radius}" fill="${fill}" stroke="${stroke}" stroke-width="2" ${extra}/>`); }
function line(x,y,x2,y2,color='#aaa',extra='') { add(`<path d="M${x} ${y} L${x2} ${y2}" fill="none" stroke="${color}" stroke-width="2" ${extra}/>`); }
function text(x,y,value,size=36,weight=400,anchor='start',fill='#333') {
  texts.push({x,y,value,size,anchor});
  add(`<text x="${x}" y="${y}" font-size="${size}" font-weight="${weight}" text-anchor="${anchor}" fill="${fill}">${esc(value)}</text>`);
}
function circle(x,y,r,fill='#fff',stroke='#555',extra='') { add(`<circle cx="${x}" cy="${y}" r="${r}" fill="${fill}" stroke="${stroke}" stroke-width="3" ${extra}/>`); }
function group(id,fn) { add(`<g id="${id}">`); fn(); add('</g>'); }
function target(id,x,y,w,h) { targets.push({id,x,y,w,h}); }
function svg(w,h,title,body) { return `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}" role="img" aria-label="${esc(title)}"><title>${esc(title)}</title><desc>Grayscale structural wireframe. No final artwork. Progressed figures are illustrative. Dashed top and bottom lines mark the safe area.</desc><g font-family="Segoe UI, Arial, sans-serif">${body}</g></svg>`; }

function wireframe(progressed, h=1920) {
  nodes=[]; texts=[]; targets=[];
  const stallBottom=h-866, incomeY=stallBottom+118, skillsY=incomeY+220, sponsorY=skillsY+216, navY=sponsorY+144;
  rect(0,0,1080,h,'#fafafa','none');
  group('safe-area-guides',()=>{
    rect(0,0,1080,24,'#dedede','none'); rect(0,h-24,1080,24,'#dedede','none');
    line(0,24,1080,24,'#777','stroke-dasharray="12 9"');
    line(0,h-24,1080,h-24,'#777','stroke-dasharray="12 9"');
  });
  group('header',()=>{
    // 144 reference px at 360 logical px wide = a 48 px target.
    target('settings',32,32,144,144);
    rect(42,42,124,124,'#f1f1f1','#888',8);
    circle(104,104,26,'none'); circle(104,104,10,'none');
    for(let i=0;i<8;i++){let a=i*Math.PI/4;line(104+30*Math.cos(a),104+30*Math.sin(a),104+42*Math.cos(a),104+42*Math.sin(a),'#555');}
    text(540,82,progressed?'SHOP RANK 8':'SHOP RANK 1',34,700,'middle');
    if(progressed) { text(1028,70,'ILLUSTRATIVE',22,500,'end','#777'); text(1028,99,'VALUES',22,500,'end','#777'); }
    rect(338,106,404,14,'#e4e4e4','#999',7); rect(338,106,progressed?244:12,14,'#777','none',7);
    text(540,213,progressed?'₹1.24M':'₹0',78,700,'middle');
    text(540,253,'CASH BALANCE',27,400,'middle','#666');
  });
  group('stall-tap-surface',()=>{
    const sy=284, sh=stallBottom-sy;
    rect(24,sy,1032,sh,'#efefef','#b2b2b2');
    // Compress only empty vertical scene space on short screens.
    const shopY=sy+Math.max(18,(sh-604)*.42), shopH=Math.min(560,sh-82);
    rect(60,shopY,960,shopH,'#fafafa','#666',4);
    rect(60,shopY,960,72,'#e1e1e1','#666');
    for(let i=0;i<12;i++) line(60+i*80,shopY,60+i*80,shopY+72,'#aaa');
    rect(278,shopY+8,524,56,'#fff','#777',3);
    text(540,shopY+48,'DIWALI DUKAAN',34,700,'middle');
    const cellY=shopY+88, cellH=Math.min(142,(shopH-190)/2), gap=18;
    const names=[['Phuljhari'],['Anar'],['Chakri'],['Rockets'],['Small','crackers'],['Cracker','strings'],['Aerial shots'],['Celebration','gift boxes']];
    for(let i=0;i<8;i++) group(`shelf-${i+1}`,()=>{
      const x=84+(i%4)*230,y=cellY+Math.floor(i/4)*(cellH+gap);
      rect(x,y,222,cellH,'#fff','#aaa',0,progressed?'':'stroke-dasharray="8 6"');
      text(x+12,y+27,String(i+1).padStart(2,'0'),23,600,'start','#777');
      if(progressed){
        const packBottom=y+cellH-(names[i].length===2?67:45);
        for(let j=0;j<4;j++){ const ph=Math.min(25+(j%2)*10,cellH-82);rect(x+50+j*32,packBottom-ph,26,ph,j%2?'#aaa':'#d0d0d0','#777'); }
      } else { const cy=y+(names[i].length===2?23:32); line(x+101,cy,x+121,cy,'#bbb'); line(x+111,cy-10,x+111,cy+10,'#bbb'); }
      names[i].forEach((n,j)=>text(x+111,y+cellH-(names[i].length-1-j)*25-9,n,25,500,'middle','#555'));
    });
    const counterY=cellY+cellH*2+gap+8;
    rect(76,counterY,928,Math.max(64,shopY+shopH-counterY-12),'#d9d9d9','#777');
    text(146,counterY+43,'COUNTER',27,500,'start','#666');
    // A deliberately schematic person, not a character-design proposal.
    circle(875,counterY+19,18,'#eee','#666');
    rect(852,counterY+38,46,26,'#eee','#666',8);
    line(857,counterY+44,846,counterY+60,'#666'); line(893,counterY+44,904,counterY+60,'#666');
    text(540,stallBottom-19,progressed?'TAP THE STALL TO EARN':'TAP TO EARN · THEN OPEN PRODUCTS',29,600,'middle','#444');
  });
  group('milestone-and-feedback',()=>{
    rect(24,stallBottom+12,1032,94,'#e7e7e7','#bbb',5);
    text(52,stallBottom+46,progressed?'NEXT DISPLAY':'FIRST PRODUCT',26,700,'start','#666');
    text(52,stallBottom+84,progressed?'Phuljhari · 12 levels to display upgrade':'Phuljhari · ₹120 to stock your first shelf',34,600);
  });
  group('income-summary',()=>{
    text(48,incomeY+37,'AUTOMATIC SALES',30,600,'start','#666');
    text(48,incomeY+110,progressed?'₹12.4K':'₹0',66,700);
    text(progressed?318:157,incomeY+110,'/ sec',32,400,'start','#666');
    text(1028,incomeY+38,'PER TAP',30,600,'end','#666');
    text(1028,incomeY+101,progressed?'₹250':'₹1',49,700,'end');
    text(48,incomeY+158,progressed?'Normal sales ₹6.2K / sec · Sponsor ×2':'Normal sales ₹0 / sec',32,400,'start','#555');
    text(48,incomeY+202,progressed?'This run · ₹3.8M earned':'This run · ₹0 earned',32,500);
    line(48,incomeY+220,1032,incomeY+220,'#ccc');
  });
  group('home-only-skills',()=>{
    const names=['Festival Rush','Helping Hands','Bulk Order'];
    const labels=progressed?['READY','ACTIVE · 00:18','WAIT · 04:20']:['LOCKED','LOCKED','LOCKED'];
    const fills=progressed?['#fff','#d4d4d4','#eaeaea']:['#eee','#eee','#eee'];
    for(let i=0;i<3;i++) group(`skill-${i}`,()=>{
      const cx=190+i*350,cy=skillsY+65;
      target(`skill-${i}`,cx-156,skillsY+6,312,210);
      circle(cx,cy,48,fills[i],'#666',progressed&&i===2?'stroke-dasharray="8 6"':'');
      if(!progressed){rect(cx-17,cy-2,34,30,'none','#888',4);add(`<path d="M${cx-11} ${cy-2} v-12 a11 11 0 0 1 22 0 v12" fill="none" stroke="#888" stroke-width="3"/>`);}
      else if(i===0) {add(`<path d="M${cx+9} ${cy-30} l-32 37 h21 l-7 25 33-38 h-23 z" fill="none" stroke="#555" stroke-width="3"/>`);}
      else if(i===1) {text(cx,cy+12,'AUTO',27,700,'middle');}
      else {circle(cx,cy,26,'none','#777');line(cx,cy,cx,cy-18,'#555');line(cx,cy,cx+15,cy+8,'#555');}
      text(cx,skillsY+157,names[i],34,600,'middle');
      text(cx,skillsY+190,labels[i],29,500,'middle','#666');
    });
  });
  group('festival-sponsor',()=>{
    target('sponsor',32,sponsorY,1016,144);
    rect(40,sponsorY+16,1000,112,'#e6e6e6','#999',6);
    rect(65,sponsorY+44,61,54,'none','#666',5);
    add(`<path d="M86 ${sponsorY+56} l23 15 -23 15 z" fill="#777"/>`);
    text(154,sponsorY+63,progressed?'SPONSOR ACTIVE · 03:42':'FESTIVAL SPONSOR',32,700);
    text(154,sponsorY+104,progressed?'×2 automatic sales · Available after expiry':'Optional video · ×2 sales for 5 minutes',32,400,'start','#555');
  });
  group('bottom-navigation',()=>{
    rect(0,navY,1080,144,'#ddd','none');line(0,navY,1080,navY,'#888');
    ['Player','Products','Legacy'].forEach((n,i)=>{
      const cx=180+i*360; target(`nav-${n}`,i*360,navY,360,144);
      if(i)line(i*360,navY+18,i*360,navY+128,'#aaa');
      if(i===0){circle(cx,navY+40,16,'none');add(`<path d="M${cx-29} ${navY+78} q0-32 29-32 q29 0 29 32" fill="none" stroke="#555" stroke-width="3"/>`);}
      if(i===1){rect(cx-34,navY+28,68,53,'none','#555');line(cx-34,navY+55,cx+34,navY+55,'#555');line(cx,navY+28,cx,navY+81,'#555');}
      if(i===2){circle(cx,navY+49,23,'none');add(`<path d="M${cx-16} ${navY+67} l-8 22 24-10 24 10-8-22" fill="none" stroke="#555" stroke-width="3"/>`);}
      text(cx,navY+122,n,36,600,'middle');
    });
  });
  for(const t of targets) if(t.w<144||t.h<144||t.y+t.h>h-24) throw Error(`Invalid target: ${t.id}`);
  if(texts.some(t=>t.x<0||t.x>1080||t.y<24||t.y>h-24)) throw Error('Text anchor outside safe area');
  return {source:svg(1080,h,progressed?'Home — progressed shop (illustrative values)':'Home — new shop',nodes.join('\n')),targets,texts};
}

const assets=[];
for(const h of [1920,1620,2340]) for(const p of [false,true]) {
  const name=`home-${p?'progressed-shop':'new-shop'}${h===1920?'':`-${h}`}`;
  const result=wireframe(p,h);
  fs.writeFileSync(path.join(out,`${name}.svg`),result.source);
  await sharp(Buffer.from(result.source)).png().toFile(path.join(out,`${name}.png`));
  assets.push({name,width:1080,height:h,controls:result.targets});
}

// Review board uses vector art throughout; no embedded screenshot or raster artwork.
nodes=[];texts=[];targets=[];
rect(0,0,1840,1790,'#f1f1f1','none');
text(64,66,'HOME / WIREFRAME 01',24,700,'start','#777');
text(64,123,'A small stall. A growing shop.',45,700);
text(64,166,'Layout review only · Grayscale placeholders · 1080 × 1920 reference',25,400,'start','#666');
const boardStart=nodes.join('\n');
let screens='';
for(let i=0;i<2;i++){
  const wf=wireframe(i===1).source;
  const inner=wf.slice(wf.indexOf('<g font-family'),wf.lastIndexOf('</svg>'));
  screens+=`<g transform="translate(${64+i*610},255) scale(.5)">${inner.replaceAll('id="',`id="board-${i}-`)}</g>`;
}
nodes=[];texts=[];targets=[];
text(64,225,'01 / NEW SHOP',26,700);text(674,225,'02 / PROGRESSED SHOP',26,700);
text(674,1254,'All progressed values are illustrative.',24,400,'start','#666');
text(1288,276,'LAYOUT NOTES',25,700);
const notes=[
 ['01  SHARED HEADER','Settings, shop rank and cash.','No new currencies or account UI.'],
 ['02  PERSISTENT STALL','Eight fixed positions, two shelves.','Only this scene surface earns taps.'],
 ['03  PROGRESS STRIP','Next purchase or visual milestone.','Feedback uses the same space.'],
 ['04  INCOME HIERARCHY','Live automatic income first.','Normal income stays clear in boosts.'],
 ['05  THREE SKILLS','Home only. State uses text,','not grayscale intensity alone.'],
 ['06  OPTIONAL SPONSOR','Explicit video action when ready.','Disabled while the boost is active.'],
 ['07  THREE TABS','Player / Products / Legacy.','Tap selected tab again for Home.']
];
notes.forEach((n,i)=>{const y=330+i*121;text(1288,y,n[0],24,700);text(1288,y+35,n[1],23);text(1288,y+65,n[2],23,400,'start','#666');});
line(64,1320,1776,1320,'#bbb');
text(64,1376,'INPUT & LAYOUT CONTRACT',26,700);
text(64,1422,'Controls: minimum 48 × 48 logical pixels. At 360 wide, 144 reference pixels = 48 logical pixels.',27);
text(64,1466,'Dashed top and bottom boundaries mark safe insets. Navigation has a padded 144 px touch area.',27);
text(64,1510,'Short screens compress scene space; the income summary, skills and navigation remain visible.',27);
text(64,1554,'Buttons, overlays and drag gestures never award tap income. Home has no scrolling.',27);
text(64,1635,'THIS STEP',24,700,'start','#777');
text(64,1680,'Wireframe → your review → reference image → HTML → Unity',33,600);
text(64,1730,'No game files changed. Later screens will be designed individually after Home review.',26,400,'start','#666');
const board=svg(1840,1790,'Home wireframe comparison and annotations',boardStart+screens+nodes.join('\n'));
fs.writeFileSync(path.join(out,'home-review-board.svg'),board);
await sharp(Buffer.from(board)).png().toFile(path.join(out,'home-review-board.png'));

// A second overview compares the actual adaptive layouts at the same width.
let ratioBody='<rect width="1500" height="1160" fill="#f1f1f1"/><text x="48" y="54" font-size="30" font-weight="700">PORTRAIT FIT CHECK / SAME CONTROL SIZES</text>';
for(const [i,h] of [1620,1920,2340].entries()) {
  const wf=wireframe(true,h).source;
  const inner=wf.slice(wf.indexOf('<g font-family'),wf.lastIndexOf('</svg>'));
  ratioBody+=`<text x="${48+i*492}" y="98" font-size="23">1080 × ${h} / ${h===1620?'short':h===1920?'reference':'tall'}</text><g transform="translate(${48+i*492},124) scale(.4)">${inner.replaceAll('id="',`id="ratio-${i}-`)}</g>`;
}
ratioBody+='<text x="48" y="1110" font-size="23">Scene height adapts. No Home scroll. Safe inset guides shown. Sample values are illustrative.</text>';
const ratios=svg(1500,1160,'Home portrait ratio checks',ratioBody);
fs.writeFileSync(path.join(out,'home-ratio-checks.svg'),ratios);
await sharp(Buffer.from(ratios)).png().toFile(path.join(out,'home-ratio-checks.png'));
fs.writeFileSync(path.join(out,'layout-checks.json'),JSON.stringify({referenceLogicalWidth:360,minimumTarget:48,safeInsetReference:24,assets},null,2));
console.log(`Created ${assets.length} screen SVG/PNG pairs, comparison board, ratio board, and target checks.`);
