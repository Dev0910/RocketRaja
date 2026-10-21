const fs=require('fs'),assert=require('node:assert/strict');
const root='Documentation/Implementation/ReferenceModel/';
const m=JSON.parse(fs.readFileSync(root+'manifest.json','utf8'));
const q=a=>{a=a.slice().sort((x,y)=>x-y);return [.1,.5,.9].map(p=>a[Math.ceil(p*a.length)-1]);};
let checks=0;function check(a,b,label){assert.deepEqual(a,b,label);checks++;console.log('PASS '+label);}
const group=p=>m.results.filter(x=>x.id.startsWith(p));
check(m.results.length,125,'125 required reference scenarios recorded');
check(q(group('free-').map(x=>x.completion)),[75,86,98],'Free completion nearest-rank 10th/median/90th matches report');
check(group('free-').filter(x=>x.completion<=90).length,23,'23 of 32 complete by day 90');
check(q(group('free-').map(x=>Number(x.resets31to90.toFixed(2)))) ,[1.23,1.38,1.53],'Late resets/day matches report');
check(q(group('free-').slice(0,16).map(x=>x.completion)),[74,90,101],'Paired free control matches report');
check(q(group('noLegendary-').map(x=>x.completion)),[100,113,127],'No-Legendary cohort matches report');
for(const [draws,expected,saved] of [[10,[72,82,95],3],[60,[71,79,86],10],[130,[65,69,83],21],[280,[52,56,63],34]]){
 const paid=group('pack-'+draws+'-');check(q(paid.map(x=>x.completion)),expected,draws+' paid-draw cohort matches report');
 check(q(paid.map((x,i)=>group('free-')[i].completion-x.completion))[1],saved,draws+' paired median days saved matches report');
}
for(const [id,u,day,resets] of [['minutes15',856,118,1.20],['minutes25',1000,71,1.55],['oneVisit',789,null,1],['twoVisits',848,119,1.18],['noTap',1000,79,1.5],['levelsFirst',1000,75,1.38],['depth4',1000,66,1.65],['depth16',1000,57,1.97],['commonOnly',844,123,1.13],['noCardEffects',720,null,.82],['neverPrestige',896,null,0]]){
 const x=m.results.find(x=>x.id===id);check([x.day90.U,x.completion,Number(x.resets31to90.toFixed(2))],[u,day,resets],id+' policy matches report');
}
const sustained=m.results.find(x=>x.id==='sustained'),mixed=m.results.find(x=>x.id==='matchedMixedRebuild');
check(sustained.first.t,7540,'Sustained first prestige 2h 5m 40s');
check(mixed.first.t,28800,'Mixed first prestige 8h');
check([Number(sustained.rebuild.firstToRebuildRatio.toFixed(2)),Number(mixed.rebuild.firstToRebuildRatio.toFixed(2))],[5.89,3.92],'Matched rebuilding ratios match report');
m.percentileMethod='nearest-rank ceil(p*n)-1, as specified in published report';
fs.writeFileSync(root+'manifest.json',JSON.stringify(m,null,2)+'\n');
let readme=fs.readFileSync(root+'README.md','utf8').replace('sample floor-index','observed nearest-rank').replace('Differences from the published report must be investigated before quoting these as reproduction.','All published cohort/policy figures were reproduced by `node Tools/RedesignLab/CheckReferenceReport.cjs`; see ../ReferenceModel-Checks.txt.');
fs.writeFileSync(root+'README.md',readme);
console.log(`All ${checks} published-result checks passed. These remain planning-model results, not device or human-play evidence.`);
