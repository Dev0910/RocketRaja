const fs=require('fs'),vm=require('vm'),crypto=require('crypto');
const md=fs.readFileSync('Game Design/Economy Model v1.md','utf8');
const catalogue=fs.readFileSync('Game Design/1000 Unlockables Catalogue.md','utf8');
const input=JSON.parse(fs.readFileSync('Documentation/Implementation/Parity-Inputs.json','utf8'));
let source=md.match(/```javascript\s*\r?\n([\s\S]*?)\r?\n```/)[1].replaceAll('\r\n','\n');
const anchor=' resetState();\n function advanceOffline';
if(!source.includes(anchor))throw new Error('Recorded model structure changed; review parity instrumentation');
const tapCost=source.match(/   const tapCost=q=>[^\n]+/)[0].trim();
// Expose the original model's closure for fixed state inputs. Its formulas remain unedited;
// no snapshot mode is written back to the design source or used by the policy suite.
const probe=` resetState();
 if(options.snapshot){
  const s=options.snapshot;F=Number(s.fame);resets=Number(s.prestiges);targetExponent=resets;
  s.cards.forEach((v,i)=>cards[i]=Number(v));resetState();const computedTarget=r.T;r.T=s.target;r.levels=s.levels.slice();r.tapN=s.tapLevel;r.cash=1e80;
  for(const id of s.owned)buy(authored[idMap[id]]);U=s.unique;r.E=s.earned;
  const o=output();
  ${tapCost}
  const answer={computedTarget,production:o.P,products:o.a,manual:o.manual*(s.rush?1.25:1),hands:o.auto*(s.rush?1.25:1),
   levelCosts:keys.map((_,i)=>[1,10,100].map(q=>i>0&&r.levels[i-1]===0?0:levelCost(i,q))),
   tapCosts:[1,10,100].map(tapCost),prices:s.priceIds.map(id=>uprice(authored[idMap[id]]))};
  activate();answer.durations=[0,1,2].map(i=>r.skills[i][0]&&i<2?r.end[i]-t:0);answer.cooldowns=r.ready.map((v,i)=>r.skills[i][0]?v-t:0);
  // The planning policy opens earned draws automatically; the game opens them on request.
  // Suppress only that voluntary policy action to compare the immediate prestige reset itself.
  draw=()=>{};r.E=s.earned;checkReset();const reward=events[0];answer.fame=String(reward?reward.fameGain:0);answer.credits=String(reward?reward.draws:0);
  answer.reset=reward?{fame:String(F),prestiges:String(resets),cash:r.cash,earned:r.E,target:r.T,unique:U,owned:r.purchases,levels:r.levels,tapLevel:r.tapN,skills:r.skills.map(v=>v[0]?1:0),cards:cards.map(String)}:null;
  return answer;
 }
 function advanceOffline`;
source=source.replace(anchor,probe);
const context={};vm.runInNewContext(source+'\nthis.reference=runReference;',context);
let assertions=0,maxRelative=0;const results=[];
function compare(expected,actual,location){
 if(Array.isArray(expected)){if(expected.length!==actual.length)throw new Error(location+' length mismatch');expected.forEach((v,i)=>compare(v,actual[i],location+'['+i+']'));return;}
 if(expected&&typeof expected==='object'){for(const key of Object.keys(expected))compare(expected[key],actual[key],location+'.'+key);return;}
 assertions++;
 if(typeof expected==='number'){const relative=Math.abs(expected-actual)/Math.max(1,Math.abs(expected));maxRelative=Math.max(maxRelative,relative);if(!Number.isFinite(actual)||relative>1e-9)throw new Error(`${location}: runtime ${expected}, model ${actual}`);}
 else if(expected!==actual)throw new Error(`${location}: runtime ${expected}, model ${actual}`);
}
for(const c of input.cases){const observed=context.reference(catalogue,{snapshot:c});compare(c.expected,observed,c.name);results.push({name:c.name,observed});console.log('PASS '+c.name);}
const report={scope:'C# redesigned domain vs original Markdown model formulas, fixed snapshots; not full runtime-policy or device parity',contentHash:input.contentHash,modelHash:crypto.createHash('sha256').update(md).digest('hex'),tolerance:1e-9,assertions,maxRelative,results};
fs.writeFileSync('Documentation/Implementation/Model-Parity.json',JSON.stringify(report,null,2)+'\n');
console.log(`All ${assertions} formula comparisons passed; maximum relative difference ${maxRelative}`);
