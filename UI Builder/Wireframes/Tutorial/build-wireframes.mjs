import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const moduleAt = process.argv.indexOf('--sharp');
if (moduleAt < 0) throw new Error('Pass --sharp <path to the installed sharp package>');
const sharp = require(process.argv[moduleAt + 1]);
const out = path.dirname(fileURLToPath(import.meta.url));
const C = { ink:'#26313a', sub:'#626a70', line:'#b6bdc1', paper:'#ffffff', base:'#f3f4f4', scene:'#e7e9ea', pale:'#f7f8f8', focus:'#1c657d', tint:'#e9f3f6' };
const esc = s => String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;');
let nodes=[], labels=[], controls=[], zones=[];
const add=s=>nodes.push(s);
function group(id, fn) { add(`<g id="${id}">`); fn(); add('</g>'); }
function box(x,y,w,h,fill=C.paper,stroke=C.line,r=5,extra='') { add(`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${r}" fill="${fill}" stroke="${stroke}" ${extra}/>`); }
function line(x,y,xx,yy,stroke=C.line,extra='') { add(`<path d="M${x} ${y}L${xx} ${yy}" stroke="${stroke}" fill="none" ${extra}/>`); }
function circle(x,y,r,fill=C.paper,stroke=C.line) { add(`<circle cx="${x}" cy="${y}" r="${r}" fill="${fill}" stroke="${stroke}"/>`); }
function text(x,y,s,size=14,weight=400,fill=C.ink,anchor='start',maxWidth=null) {
  labels.push({x,y,text:s,size,weight,anchor,maxWidth});
  add(`<text x="${x}" y="${y}" font-size="${size}" font-weight="${weight}" fill="${fill}" text-anchor="${anchor}">${esc(s)}</text>`);
}
function lines(x,y,ss,size=13,leading=19,fill=C.ink,maxWidth=330) { ss.forEach((s,i)=>text(x,y+i*leading,s,size,400,fill,'start',maxWidth)); }
function button(id,x,y,w,label,primary=false,h=48,disabled=false) {
  controls.push({id,x,y,w,h});
  group(id,()=>{box(x,y,w,h,disabled?'#eceeef':primary?C.focus:C.paper,primary?C.focus:C.line,6);text(x+w/2,y+h/2+5,label,13,600,disabled?C.sub:primary?'#ffffff':C.ink,'middle',w-16);});
}
function focus(x,y,w,h,id) { zones.push({id,x,y,w,h});box(x-4,y-4,w+8,h+8,'none',C.focus,9,'stroke-width="2" stroke-dasharray="6 4"'); }
function svg(w,h,title,body) { return `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}" role="img"><title>${esc(title)}</title><desc>Editable structural wireframe, revision 1. Blue marks proposed tutorial elements and target highlights. All values are review fixtures. No production artwork or gameplay code.</desc><g font-family="Segoe UI, Arial, sans-serif">${body}</g></svg>`; }

const states=[
 {id:'01-welcome',title:'Meet the shopkeeper',caption:['A short speech bubble starts the opening.','The counter and navigation remain usable.'],kind:'welcome',cash:'0',tap:'1',income:'0',products:0},
 {id:'02-saving',title:'Bubble collapsed while earning',caption:['The next-step card carries progress and price.','Tap ? to reopen the current instruction.'],kind:'saving',cash:'6',tap:'1',income:'0',products:0},
 {id:'03-salesmanship',title:'Make the first training purchase',caption:['The card routes to Player → Salesmanship.','Highlight the existing control; let users browse.'],kind:'training',cash:'10',tap:'1',income:'0',products:0,tab:'Player'},
 {id:'04-training-result',title:'Explain the actual improvement',caption:['Acknowledge the successful purchase once.','Then move to saving for the first product.'],kind:'result',cash:'0',tap:'2.40',income:'0',products:0},
 {id:'05-first-product',title:'Stock the first product',caption:['Teach automatic income through Phuljhari.','Its first stock purchase costs ₹30.'],kind:'product',cash:'30',tap:'2.40',income:'0',products:0,tab:'Products'},
 {id:'06-first-upgrade',title:'Improve something already owned',caption:['Recommend a useful product-profit upgrade.','Show its real before / after income.'],kind:'upgrade',cash:'20',tap:'2.40',income:'.85',products:1,tab:'Player'},
 {id:'07-achievement',title:'Claim the first earned achievement',caption:['Respond to whichever achievement is earned.','This example recognizes four stocked products.'],kind:'achievement',cash:'18K',tap:'2.40',income:'22.42',products:4},
 {id:'08-prestige',title:'Introduce the first prestige',caption:['Open the existing reset / keep preview.','Prestige remains the player’s decision.'],kind:'prestige',cash:'1.26M',tap:'1.65K',income:'11.65K',products:8},
 {id:'09-free-pull',title:'Use a free pull after prestige',caption:['First prestige supplies three free pulls.','Teach a free 1× pull and permanent card benefits.'],kind:'talent',cash:'30',tap:'31.80',income:'0',products:0,tab:'Legacy'}
];

function merchant(w,bottom,compact) {
 const x=w-37, y=bottom-57, r=compact?11:15;
 group('existing-shopkeeper-placeholder',()=>{
  add(`<path d="M${x-24} ${bottom-4}Q${x-26} ${y+15} ${x-10} ${y+12}L${x+10} ${y+12}Q${x+26} ${y+15} ${x+24} ${bottom-4}Z" fill="#c1c6c9" stroke="#697279"/>`);
  circle(x,y,r,'#f9f9f9','#697279');
  add(`<path d="M${x-r} ${y-2}Q${x-r-3} ${y-r-10} ${x+5} ${y-r-4}Q${x+r+4} ${y-r-2} ${x+r} ${y+2}L${x+7} ${y-r+5}L${x-2} ${y-r+4}Z" fill="#757e84"/>`);
  circle(x-4,y+1,.9,C.ink,C.ink);circle(x+4,y+1,.9,C.ink,C.ink);
  add(`<path d="M${x-5} ${y+7}Q${x} ${y+11} ${x+5} ${y+7}" fill="none" stroke="#697279"/>`);
  line(x-7,y+16,x-11,bottom-6,'#697279');line(x+7,y+16,x+11,bottom-6,'#697279');
  add(`<path d="M${x-27} ${bottom-11}Q${x} ${bottom-20} ${x+27} ${bottom-11}" fill="none" stroke="#697279" stroke-width="5"/>`);
 });
}

function stall(s,w,top,bottom,compact) {
 group('existing-stall-layout',()=>{
  box(10,top,w-20,bottom-top,C.scene,C.line,4);
  box(20,top+8,w-40,compact?19:30,'#d2d6d8','#a4adb2',2);
  for(let i=0;i<10;i++)line(20+(w-40)/10*i,top+8,20+(w-40)/10*i,top+(compact?27:38),'#b8bec1');
  text(w/2,top+(compact?22:29),'DIWALI DUKAAN',compact?11:14,700,C.ink,'middle',w-72);
  const sy=top+(compact?34:58), counter=bottom-(compact?24:87), sh=(counter-sy-8)/2, cw=(w-56)/4;
  const names=compact?['Phuljhari','Anar','Chakri','Rockets','Crackers','Strings','Aerial','Gift boxes']:['Phuljhari','Anar','Chakri','Rockets','Crackers','Strings','Aerial shots','Gift boxes'];
  for(let i=0;i<8;i++) {
   const x=25+(i%4)*cw, y=sy+Math.floor(i/4)*sh;
   box(x,y,cw-4,sh-4,s.products>i?'#cbd1d4':'#f5f6f6','#aeb6bb',1);
   if(!compact){box(x+cw*.29,y+9,cw*.35,sh-30,s.products>i?'#a6b0b6':'none','#c0c6ca',1,s.products>i?'':'stroke-dasharray="3 3"');}
   text(x+(cw-4)/2,y+sh-9,names[i],compact?7.5:9,400,C.sub,'middle',cw-8);
  }
  box(14,counter,w-28,compact?25:45,'#b8c0c5','#89959d',3);
  text(35,counter+(compact?17:28),'TAP COUNTER',compact?10:13,700,C.ink,'start',w-110);
  if(!compact)text(35,bottom-19,'Tap to earn · no stock depletion',10,400,C.sub,'start',w-95);
  merchant(w,bottom,compact);
 });
}

function navigation(w,h,tab,compact) {
 group('existing-navigation',()=>{
  const y=h-64;box(0,y,w,64,'#fafafa','#b7bfc3',0);
  ['Player','Products','Legacy','Shop'].forEach((name,i)=>{
   const x=8+i*(w-16)/4, ww=(w-16)/4-6;
   controls.push({id:'navigation-'+name,x,y:y+5,w:ww,h:50});
   box(x,y+5,ww,50,name===tab?'#dce2e5':'#ffffff',name===tab?'#6d7e89':'#c5ccd0',4);
   circle(x+ww/2,y+20,6,'none','#74838d');
   text(x+ww/2,y+44,name,11,600,C.ink,'middle',ww-6);
  });
  box(0,h-5,w,5,'#e4e7e9','none',0);
 });
}

function objective(s,w,y,compact) {
 let title='Train Salesmanship',detail='₹'+s.cash+' / ₹10 · tap the counter',tag='NEXT STEP',cta='›';
 if(s.kind==='result'){title='Stock Phuljhari';detail='Save ₹30 · earn automatically';}
 if(s.kind==='achievement'){title='Claim Open for business';detail='+5 Kaju Katli · reward ready';tag='REWARD READY';}
 if(s.kind==='prestige'){title='View your first prestige';detail='Review resets, Fame and 3 free pulls';tag='PRESTIGE READY';}
 group('tutorial-persistent-objective',()=>{
  box(12,y,w-24,68,C.tint,C.focus,5);
  controls.push({id:'next-step-route',x:12,y,w:w-80,h:68});
  text(25,y+17,tag,9,700,C.focus,'start',w-105);
  text(25,y+36,title,13,700,C.ink,'start',w-105);
  text(25,y+54,detail,10.5,400,C.sub,'start',w-105);
  text(w-82,y+40,'›',20,600,C.focus,'middle',16);
  button('reopen-current-tip',w-67,y+10,48,'?',false);
 });
}

function income(s,w,y,compact) {
 group('existing-income-and-skills',()=>{
  box(12,y,w-24,compact?78:100,C.paper,C.line,4);
  const split=Math.round(w*.55);
  text(25,y+18,'AUTOMATIC SALES',9,700,C.sub,'start',split-34);
  text(25,y+44,'₹'+s.income+'/sec',compact?19:22,700,C.ink,'start',split-30);
  line(split,y+13,split,y+(compact?65:86));
  text(split+14,y+18,'PER TAP',9,700,C.sub,'start',w-split-36);
  text(split+14,y+44,'₹'+s.tap,compact?20:23,700,C.ink,'start',w-split-36);
  if(!compact){text(25,y+79,'Products earn automatically',10,400,C.sub,'start',split-34);text(split+14,y+79,'Tap the counter',10,400,C.sub,'start',w-split-30);}
  const sy=y+(compact?103:133);
  ['Festival Rush','Helping Hands','Bulk Order'].forEach((name,i)=>{
   const x=w*(i+.5)/3;
   controls.push({id:'skill-'+i,x:x-25,y:sy-22,w:50,h:52});
   circle(x,sy,compact?18:23,'#e2e6e8','#a9b2b9');
   text(x,sy+4,'–',15,700,C.sub,'middle',24);
   text(x,sy+(compact?33:38),name,compact?9:10,600,C.sub,'middle',w/3-7);
  });
 });
}

function bubble(s,w,bottom,compact) {
 const x=15, y=compact?75:156, ww=w-(compact?90:42), hh=compact?126:165, bx=compact?26:28;
 let title='Let’s open your stall!',copy=['Tap the counter to earn rupees.','Then we’ll improve your sales pitch.'],action='Let’s start';
 if(s.kind==='result'){title='Your taps just got better!';copy=['You now earn ₹2.40 per tap,','up from ₹1. Next, stock Phuljhari.'];action='Got it';}
 if(s.kind==='achievement'){title='Your first achievement!';copy=['You stocked 4 different products.','Collect your +5 Kaju Katli reward.'];action='Show reward';}
 if(s.kind==='prestige'){title='Ready for a brighter start?';copy=['Prestige earns Fame and free pulls.','Review what resets and what stays.'];action='View prestige';}
 group('tutorial-shopkeeper-bubble',()=>{
  add(compact
   ? `<path d="M${x+ww-1} ${y+75}L${w-53} ${bottom-57}L${x+ww-1} ${y+96}" fill="${C.paper}" stroke="${C.focus}" stroke-width="1.5"/>`
   : `<path d="M${x+ww-57} ${y+hh-1}L${w-42} ${bottom-72}L${x+ww-30} ${y+hh-1}" fill="${C.paper}" stroke="${C.focus}" stroke-width="1.5"/>`);
  box(x,y,ww,hh,C.paper,C.focus,9,'stroke-width="1.5"');
  text(bx,y+(compact?22:28),title,compact?14:16,700,C.ink,'start',ww-26);
  lines(bx,y+(compact?40:53),copy,compact?11.5:13,compact?16:20,C.ink,ww-26);
  button('tutorial-continue',bx,y+hh-57,compact?142:154,action,true);
  button('skip-tutorial',bx+(compact?150:162),y+hh-57,ww-(compact?164:176),'Skip tutorial',false);
 });
}

function panelHeader(w,y,title) {
 text(18,y+26,title,20,700,C.ink,'start',w-90);
 button('collapse-panel',w-66,y,48,'⌄');
}
function hint(x,y,w,copy,id='tutorial-control-hint') {
 group(id,()=>{box(x,y,w,57,C.tint,C.focus,5);lines(x+12,y+23,copy,12,18,C.ink,w-24);});
}
function row(x,y,w,title,desc,price,label,id,highlight=false) {
 group(id+'-row',()=>{
  box(x,y,w,74,C.paper,C.line,5);
  text(x+12,y+24,title,12,700,C.ink,'start',w-132);
  text(x+12,y+45,desc,11,400,C.sub,'start',w-132);
  text(x+12,y+62,price,10,600,C.sub,'start',w-132);
  button(id,x+w-110,y+13,98,label,highlight);
  if(highlight)focus(x+w-110,y+13,98,48,id+'-focus');
 });
}

function lowerPanel(s,w,y,h) {
 panelHeader(w,y,s.tab);
 const x=14, ww=w-28;
 if(s.kind==='training'){
  ['Stats','Skills','Achievements'].forEach((n,i)=>button('toolbar-'+n,x+i*(ww/3+1),y+53,ww/3-4,n));
  hint(x,y+115,ww,['Train once to earn more per tap.','You can still explore every screen.']);
  row(x,y+185,ww,'Salesmanship · Lvl 0','₹1 → ₹2.40 per tap','First level · ₹10','Train · ₹10','buy-training',true);
  text(x+4,y+289,'Upgrades',16,700,C.ink,'start',ww-10);
  text(x+4,y+310,'More improvements become useful as you grow.',11,400,C.sub,'start',ww-10);
 }
 if(s.kind==='product'){
  text(x+4,y+76,'Automatic sales · ₹0/sec',12,600,C.sub,'start',ww-10);
  hint(x,y+96,ww,['Stock Phuljhari to earn automatically.','It earns ₹0.85 every second.']);
  row(x,y+166,ww,'Phuljhari · not stocked','Automatic sales · ₹0.85/sec','First product · ₹30','Stock · ₹30','stock-product',true);
  row(x,y+254,ww,'Anar · locked','Stock Phuljhari first','Next display','View','next-product');
 }
 if(s.kind==='upgrade'){
  row(x,y+61,ww,'Salesmanship · Lvl 1','₹2.40 per tap','You already trained once','Train','training-row');
  text(x+4,y+163,'Upgrades',16,700,C.ink,'start',ww-10);
  hint(x,y+178,ww,['Improve a product you already own.','Phuljhari: ₹0.85 → ₹1.70/sec.']);
  row(x,y+249,ww,'Phuljhari Signature I','Phuljhari profit ×2','One-time upgrade this run · ₹20','Buy · ₹20','buy-upgrade',true);
 }
 if(s.kind==='talent'){
  text(x+4,y+78,'3 free pulls available',17,700,C.ink,'start',ww-10);
  text(x+4,y+99,'Earned from your first prestige',11,400,C.sub,'start',ww-10);
  hint(x,y+119,ww,['Use one free pull to discover a Talent card.','Its benefit stays with you after prestige.']);
  button('free-one-pull',x+8,y+194,ww*.56,'1× pull · FREE',true,56);
  focus(x+8,y+194,ww*.56,56,'free-pull-focus');
  button('five-pull',x+ww*.62,y+194,ww*.36-8,'5× · 400 KK',false,56,true);
  text(x+4,y+282,'Talent collection · 0 / 28',14,700,C.ink,'start',ww-10);
  text(x+4,y+305,'After the reveal: explain this card’s actual benefit.',10.5,400,C.sub,'start',ww-10);
 }
}

function screen(s,w=390,h=844) {
 nodes=[];labels=[];controls=[];zones=[];
 const compact=h<650, top=compact?71:91, bottom=compact?228:414;
 box(0,0,w,h,C.base,'none',0);
 group('existing-cash-and-settings',()=>{
  box(0,0,w,compact?65:82,'#f7f8f8','none',0);
  button('settings',12,compact?10:17,48,'⚙');
  text(w/2+12,compact?35:44,'₹'+s.cash,compact?24:29,700,C.ink,'middle',w-160);
  text(w/2+12,compact?53:65,'CASH BALANCE',9,600,C.sub,'middle',w-160);
 });
 stall(s,w,top,bottom,compact);
 if(!s.tab){objective(s,w,bottom+(compact?8:16),compact);income(s,w,compact?312:h-250,compact);}
 else lowerPanel(s,w,bottom+16,h);
 navigation(w,h,s.tab,compact);
 if(['welcome','result','achievement','prestige'].includes(s.kind))bubble(s,w,bottom,compact);
 if(s.kind==='saving'){
  focus(24,bottom-(compact?44:91),w-112,compact?48:56,'tap-counter-focus');
  if(!compact)text(37,bottom-43,'+₹1 per tap',11,600,C.focus,'start',w-140);
 }
 return {id:s.id+(compact?'-360x540':''),w,h,title:s.title,svg:svg(w,h,s.title,nodes.join('')),body:nodes.join(''),labels:[...labels],controls:[...controls],zones:[...zones]};
}

const screens=states.map(s=>screen(s));
const compactScreens=[screen(states[0],360,540),screen(states[1],360,540)];
const checks=[];
for(const shot of [...screens,...compactScreens]) {
 for(const b of shot.controls) {
  if(b.w<48||b.h<48||b.x<0||b.y<0||b.x+b.w>shot.w+.1||b.y+b.h>shot.h+.1)throw new Error('Invalid control '+shot.id+': '+JSON.stringify(b));
 }
 for(const l of shot.labels)if(l.y<0||l.y>shot.h||l.x<0||l.x>shot.w)throw new Error('Text anchor outside canvas '+shot.id);
 checks.push({id:shot.id,width:shot.w,height:shot.h,controls:shot.controls.length,minTarget:48,labels:shot.labels,targets:shot.controls});
 fs.writeFileSync(path.join(out,shot.id+'.svg'),shot.svg);
 await sharp(Buffer.from(shot.svg)).resize(shot.w*2,shot.h*2).png().toFile(path.join(out,shot.id+'.png'));
}

async function board(name,indices,columns=3,compact=false) {
 nodes=[];labels=[];controls=[];zones=[];
 const shots=compact?compactScreens:indices.map(i=>screens[i]);
 const colW=438, rowH=compact?706:1012, width=columns*colW+64, height=170+Math.ceil(shots.length/columns)*rowH+110;
 box(0,0,width,height,'#edf0f1','none',0);
 text(40,49,'ROCKET RAJA / SHOPKEEPER TUTORIAL',19,700,C.ink,'start',width-80);
 text(40,82,compact?'Short-screen check · 360 × 540':'Wireframe revision 1 · opening + milestone handoffs',30,700,C.ink,'start',width-80);
 text(40,112,'New users only  /  Free navigation  /  First Talent teaching uses a free pull after prestige',15,400,C.sub,'start',width-80);
 box(40,130,13,13,C.tint,C.focus,2);text(61,141,'Blue = proposed tutor / dashed outline = suggested control',12,400,C.sub,'start',width-100);
 shots.forEach((shot,i)=>{
  const s=compact?states[i]:states[indices[i]],x=40+(i%columns)*colW,y=185+Math.floor(i/columns)*rowH;
  text(x,y-7,(compact?'COMPACT '+(i+1):s.id.slice(0,2))+' / '+s.title,15,700,C.ink,'start',398);
  const scopedBody=shot.body.replaceAll(/id="([^"]+)"/g,(_,id)=>`id="${shot.id}-${id}"`);
  add(`<g transform="translate(${x},${y+12})">${scopedBody}</g>`);
  const cy=y+12+shot.h+29;
  lines(x,cy,compact?['Controls retain 48 px touch targets.','Dialogue clears cash, counter and navigation.']:s.caption,13,21,C.sub,395);
 });
 const foot=height-69;
 line(40,foot-23,width-40,foot-23,'#c3cbd0');
 text(40,foot,'Structure + copy for review. Existing game panels are simplified context; values are examples.',13,400,C.sub,'start',width-80);
 text(40,foot+25,'Editable SVGs included. Wireframe approval pending; art, HTML and Unity follow in separate stages.',13,400,C.sub,'start',width-80);
 const source=svg(width,height,'Shopkeeper tutorial wireframe review board',nodes.join(''));
 fs.writeFileSync(path.join(out,name+'.svg'),source);
 await sharp(Buffer.from(source)).png().toFile(path.join(out,name+'.png'));
 return {name,width,height};
}

const boards=[];
boards.push(await board('tutorial-r1-review-board',[0,1,2,3,4,5,6,7,8]));
boards.push(await board('tutorial-r1-opening-board',[0,1,2,3],2));
boards.push(await board('tutorial-r1-compact-checks',[],2,true));
fs.writeFileSync(path.join(out,'layout-checks.json'),JSON.stringify({revision:1,states:checks,boards},null,2)+'\n');
console.log(JSON.stringify({screens:screens.length,compactScreens:compactScreens.length,boards,checks:'all controls fit and are at least 48 × 48 logical pixels'},null,2));
