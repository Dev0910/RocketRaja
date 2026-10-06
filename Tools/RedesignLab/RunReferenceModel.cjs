// Reproduces the published planning model. Does not simulate the Unity runtime or make human-play claims.
const fs=require('fs'), path=require('path'), vm=require('vm'), crypto=require('crypto');
const markdown=fs.readFileSync('Game Design/Economy Model v1.md','utf8');
const catalogue=fs.readFileSync('Game Design/1000 Unlockables Catalogue.md','utf8');
const match=markdown.match(/```javascript\s*\r?\n([\s\S]*?)\r?\n```/);
if(!match)throw new Error('Recorded JavaScript model block missing');
const context={};vm.runInNewContext(match[1]+'\nthis.reference=runReference;',context,{timeout:5000});
const root=path.resolve('Documentation/Implementation/ReferenceModel');fs.mkdirSync(root,{recursive:true});
const cases=[];
for(let i=1;i<=32;i++)cases.push({id:`free-${i}`,seed:1009*i});
for(const draws of [10,60,130,280])for(let i=1;i<=16;i++)cases.push({id:`pack-${draws}-${i}`,seed:1009*i,paidDraws:draws});
for(const [id,options] of Object.entries({minutes15:{minutes:15},minutes25:{minutes:25},oneVisit:{visits:1},twoVisits:{visits:2},noTap:{taps:0},levelsFirst:{policy:'balanced'},depth4:{depth:4},depth16:{depth:16},commonOnly:{commonOnly:true},noCardEffects:{cardEffectsOff:true},neverPrestige:{noReset:true}}))cases.push({id,seed:1009,...options});
for(let i=1;i<=16;i++)cases.push({id:`noLegendary-${i}`,seed:1009*i,noLegendary:true});
cases.push({id:'sustained',seed:1009,continuous:true,hours:4,cardEffectsOff:true});
cases.push({id:'matchedMixedRebuild',seed:1009,days:1,stopAtFirst:true,matchedRebuild:true,cardEffectsOff:true});
const digest=s=>crypto.createHash('sha256').update(s).digest('hex').toUpperCase();
const manifest={kind:'published Markdown planning model rerun; not Unity/human play',modelHash:digest(markdown),catalogueHash:digest(catalogue),node:process.version,startedAt:new Date().toISOString(),results:[]};
for(const item of cases){
 const {id,...overrides}=item;const start=performance.now();const result=context.reference(catalogue,overrides);
 const completion=result.daily.find(x=>x.U===1000)?.day??null;
 const day90=result.daily.find(x=>x.day===90)??null;
 const resets31to90=result.events.filter(e=>e.day>=30&&e.day<90).length/60;
 const summary={id,overrides,completion,day90,resets31to90,first:result.first,rebuild:result.rebuild,final:result.final,draws:result.draws,cards:result.cards,activeMinutes:result.activeMinutes,elapsedMs:Math.round(performance.now()-start)};
 manifest.results.push(summary);
 // Preserve exact events/daily points/earnings sources for reproduction without bloating chat output.
 fs.writeFileSync(path.join(root,id+'.json'),JSON.stringify(result)+'\n');
 fs.writeFileSync(path.join(root,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
 console.log(`${manifest.results.length}/${cases.length} ${id}: completion ${completion??'>150'}, first ${result.first?.t??'none'}s`);
}
const quantiles=values=>{values=values.filter(x=>x!==null).sort((a,b)=>a-b);return [.1,.5,.9].map(p=>values[Math.ceil(p*values.length)-1]);};
const free=manifest.results.filter(x=>/^free-/.test(x.id));
manifest.finishedAt=new Date().toISOString();manifest.freeCompletionQuantiles=quantiles(free.map(x=>x.completion));
manifest.freeCompleteBy90=free.filter(x=>x.completion!==null&&x.completion<=90).length;
fs.writeFileSync(path.join(root,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
fs.writeFileSync(path.join(root,'README.md'),`# Published-model rerun\n\nThis runs the JavaScript recorded in Game Design/Economy Model v1.md against the authored catalogue. It is **not Unity runtime or human-play evidence**.\n\nCommand: \`node Tools/RedesignLab/RunReferenceModel.cjs\`. Node ${process.version}. Model SHA256 ${manifest.modelHash}; catalogue SHA256 ${manifest.catalogueHash}.\n\n${cases.length} scenarios: 32 free histories, 16 per pack (four packs), 11 policy variants, 16 no-Legendary histories, sustained and matched-mixed diagnostics. First 16 free histories are paired controls. Seeds, options, exact active-time totals, events and daily records are retained in JSON. Management/reveal time and actual visit gaps are unchanged.\n\nFree completion days (observed nearest-rank 10th/median/90th): ${manifest.freeCompletionQuantiles.join(' / ')}. Completed by day 90: ${manifest.freeCompleteBy90}/32. Differences from the published report must be investigated before quoting these as reproduction. Runtime/model parity is a separate check.\n`);
console.log('Reference model suite complete. '+JSON.stringify({free:manifest.freeCompletionQuantiles,by90:manifest.freeCompleteBy90}));
