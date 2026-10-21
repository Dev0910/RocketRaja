# Rocket Raja: reproducible economy model v1

**Historical reference — 7 October 2026:** preserve the original rationale, scenarios and results below. They do not define the current interaction/access rules or establish current pacing. See [Current Implementation Design](Current%20Implementation%20Design.md) for the user-confirmed implementation.

1 October 2026. Planning-only JavaScript recorded inside Markdown. It does not call Unity, edit saves, or use project runtime code.

Input is the complete text of [1000 Unlockables Catalogue](1000%20Unlockables%20Catalogue.md). All v1 default parameters are embedded below. Pass that text to `runReference(catalogueMarkdown)`. The returned object includes purchases, lifetime counts, cards, earnings sources, daily checkpoints, reset events, and rebuild measurements.

To reproduce the free cohort, call `runReference(text,{seed:1009*i})` for i=1..32. For paid comparisons, use i=1..16 and `paidDraws:10`, `60`, `130`, or `280`. Examples of other overrides: `minutes:15`, `visits:1`, `depth:4`, `taps:0`, or `policy:"balanced"`. The default policy uses Buy All before levels; balanced reverses those actions.

For continuous first/rebuild checks use `continuous:true,hours:4,cardEffectsOff:true`. For a matched mixed rebuild use `days:1,stopAtFirst:true,matchedRebuild:true,cardEffectsOff:true`. To change integration resolution while preserving the 20-second decision cadence, pass `params:{step:1}`; build merges overrides with the embedded defaults.

Cash-only progression is selected and embedded in the defaults: later band openings grow by 31, Fame growth uses g=3, and catalogue eligibility has no prestige-count check. The default reference runs 150 days. Use `noReset:true` for the no-prestige policy. For no-Legendary stress, use `noLegendary:true` (Common/Rare renormalized to 75/97 and 22/97), with seeds 1009×i for i=1..16. The former gated candidate is historical comparison data only.

This is a simplified player-policy simulator. Management windows represent deliberate manual actions; it is not gameplay automation. The code uses standard JavaScript only and can be evaluated outside Unity. Keep any extracted executable outside the Unity project unless implementation is separately authorized.

```javascript
function parseCatalogue(markdown) {
  return markdown.split(/\r?\n/).filter(line => /^\| (PP|PC|TP|TC|GP|UC|SK|XL|PF)-/.test(line))
    .map((line, index) => {
      const [id, name, effect, req, key, displayedPrice] = line.split("|").slice(1, -1).map(x => x.trim());
      return {index, id, name, effect, req, key, displayedPrice:Number(displayedPrice),
        band:Number(key.slice(1, 3)), position:Number(key.slice(4)),
        deps:[...req.matchAll(/\b(?:PP|PC|TP|TC|GP|UC|SK|XL|PF)-[A-Z0-9-]+\b/g)].map(m => m[0])};
    });
}
function createModel(rows) {

const keys=["PHU","ANA","CHA","ROC","SMC","STR","AER","BOX"];
const idMap=Object.fromEntries(rows.map((r,i)=>[r.id,i]));
const prepared=rows.map(r=>{
 const match=r.effect.match(/^(\w+)\(([^)]+)\)/),type=match[1],args=match[2].split(",");
 const rank=+args[args.length-1],pi=keys.indexOf(args[0]);
 const prereq=r.deps.map(id=>idMap[id]);
 const fam=+(r.req.match(/Own (\d)\+/)||[])[1]||0;
 const group=type==="PF"?(r.req.split(" levels")[0].split("+").map(s=>keys.indexOf(s))):[];
 return {...r,type,args,rank,pi,prereq,fam,group,signature:r.name.includes("Signature Breakthrough")};
});
const defaults={
  "cost0": 30,
  "costRatio": 2,
  "income0": 1,
  "incomeRatio": 1.6,
  "levelGrowth": 1.12,
  "cash0": 200,
  "bandRatio": 24,
  "within": 4,
  "pp": 1.6,
  "sig": 8,
  "gp": 1.1,
  "td": 1.2,
  "incomeScale": 0.85,
  "T0": 10000000000,
  "targetGrowth": 1.5,
  "fameA": 6,
  "fameG": 3,
  "step": 10,
  "linkLevels": [
    25,
    100,
    200,
    300
  ],
  "groupLevels": [
    25,
    100,
    200,
    300
  ],
  "revealSeconds": 40,
  "productWeight": 4,
  "bandPrices": [
    100,
    3000000,
    72000000,
    2232000000,
    69192000000,
    2144952000000,
    66493512000000,
    2061298872000000,
    63900265032000000,
    1980908215992000000,
    61408154695752000000,
    1.903652795568312e+21,
    5.901323666261767e+22,
    1.829410336541148e+24,
    5.671172043277558e+25,
    1.758063333416043e+27,
    5.449996333589734e+28,
    1.6894988634128172e+30,
    5.237446476579734e+31,
    1.6236084077397176e+33
  ],
  "ppFirst": 1.2,
  "sigFirst": 2,
  "Tcap": 1e+36,
  "depthTargetWeight": 0
};
function build(params={}) {
 const p={...defaults,...params};
 const counts=prepared.reduce((o,r)=>(o[r.band]=(o[r.band]||0)+1,o),{});
 const authored=prepared.map(r=>({...r,price:Number(((p.bandPrices?p.bandPrices[r.band-1]:p.cash0*p.bandRatio**(r.band-1))*(r.band===1?(p.withinFirst||p.within):p.within)**((r.position-1)/(counts[r.band]-1))).toPrecision(6))}));
 return {p,authored};
}
function simulate(options={}) {
 const {p,authored}=build(options.params);
 const seed=options.seed||12345;let rng=seed,paidRng=seed^0xdecafbad;
 function random(){rng^=rng<<13;rng^=rng>>>17;rng^=rng<<5;return(rng>>>0)/4294967296;}
 const cards=Array(28).fill(0),ever=Array(1000).fill(false);
 let targetExponent=0;
 let U=0,F=0,resets=0,draws=0,freeDraws=0,paidConsumed=0,giftConsumed=0,t=0,activeSeconds=0,blackout=0;
 let r,first=null,rebuild=null,firstOwned=null,firstFamTimes=null;
 let daily=[],events=[],runStart=0;
 const policy=options.policy||"balanced",depth=options.depth||1;
 const days=options.days||90;
 let stream={product:0,tap:0,hands:0,rush:0,bulk:0,offline:0},totalActions=0;
 const famcost=keys.map((_,i)=>p.cost0*p.costRatio**i);
 const faminc=keys.map((_,i)=>p.income0*p.incomeRatio**i*p.incomeScale);
 const perm=()=>F===0?1:p.fameA+p.fameG*(Math.sqrt(F)-1);
 function draw(n,paid=false){for(let j=0;j<n;j++){let u;if(paid){paidRng^=paidRng<<13;paidRng^=paidRng>>>17;paidRng^=paidRng<<5;u=(paidRng>>>0)/4294967296;}else u=random();if(options.noLegendary)u*=.97;const id=u<.75?Math.floor(u/.75*16):u<.97?16+Math.floor((u-.75)/.22*10):26+Math.floor((u-.97)/.03*2);if(!options.cardEffectsOff)cards[options.commonOnly?(draws%16):id]++;draws++;}}
 function resetState(){
 r={cash:famcost[0]*(1-.4*cards[8]/(cards[8]+4)),E:0,T:Math.min(p.Tcap||Infinity,p.T0*p.targetGrowth**targetExponent),
 levels:Array(8).fill(0),owned:Array(1000).fill(false),pp:Array(8).fill(1),pc:Array(8).fill(0),global:1,uc:0,
 tapN:0,td:0,tl:0,tc:0,tapMult:1,skills:[[0,0,0],[0,0,0],[0,0,0]],ready:[t,t,t],end:[t,t],familyTimes:Array(8).fill(null),
 lastPurchase:t,maxGap:0,purchases:0,signatureLeads:0,signatureLater:0,all8:null};
 runStart=t;
 }
 function milestone(n){return (n>=25?1:0)+(n>=50?1:0)+Math.floor(n/100);}
 function output() {
 const factor=r.global*perm()*(1+.25*cards[26]);
 const a=r.levels.map((n,i)=>faminc[i]*n*2**milestone(n)*r.pp[i]*(1+.2*cards[i])*factor);
 const P=a.reduce((x,y)=>x+y,0),B=(1+1.4*r.tapN)*r.tapMult*factor*p.incomeScale;
 const M=1+.25*r.tapN/(r.tapN+100)+.25*r.td/60;
 const A=r.tl? .12+.18*(r.tl-1)/59:0,C=1+cards[16]/(cards[16]+5);
 return {P,a,B,manual:C*(B+P*A*M/2),auto:C*(B+.25*P*A*M/2)};
 }
 function earn(amount,kind){r.cash+=amount;r.E+=amount;stream[kind]+=amount;}
 function affordable(cost){return cost<=r.cash*(1+1e-12);}
 function eligible(x){
 if(r.owned[x.index]||x.prereq.some(j=>!r.owned[j]))return false;
 const count=r.levels.filter(n=>n>0).length;
 if(count<x.fam)return false;
 if(x.type==="PP"||x.type==="PC")return r.levels[x.pi]>0;
 if(x.type==="XL")return r.levels[x.pi]>=p.linkLevels[x.rank-1]&&r.levels[keys.indexOf(x.args[1])]>0;
 if(x.type==="PF")return x.group.every(i=>r.levels[i]>=p.groupLevels[x.rank-1]);
 return true;
 }
 function uprice(x){return x.price*(1-.35*r.uc/32)*(1-.5*cards[27]/(cards[27]+4));}
 function buy(x){
 const before=x.signature?output():null,price=uprice(x);
 r.cash=Math.max(0,r.cash-price);r.owned[x.index]=true;r.purchases++;
 r.maxGap=Math.max(r.maxGap,t-r.lastPurchase);r.lastPurchase=t;
 if(!ever[x.index]){ever[x.index]=true;U++;}
 if(x.type==="PP")r.pp[x.pi]*=x.signature?(x.rank<=6?(p.sigFirst||p.sig):p.sig):(x.band===1?(p.ppFirst||p.pp):p.pp);
 if(x.type==="PC")r.pc[x.pi]=x.rank;
 if(x.type==="GP")r.global*=p.gp;
 if(x.type==="UC")r.uc=x.rank;
 if(x.type==="TD"){r.td++;r.tapMult*=p.td;}
 if(x.type==="TL")r.tl=x.rank;
 if(x.type==="TC")r.tc=x.rank;
 if(x.type==="XL")r.pp[keys.indexOf(x.args[1])]*=1.5;
 if(x.type==="PF")x.group.forEach(i=>r.pp[i]*=1.3);
 if(["SP","SD","SC"].includes(x.type)){const s=["FR","HH","BO"].indexOf(x.args[0]);r.skills[s][["SP","SD","SC"].indexOf(x.type)]=x.rank;}
 if(x.signature&&x.rank>6){r.signatureLater++;const after=output();if(after.a[x.pi]===Math.max(...after.a)&&before.a[x.pi]<Math.max(...before.a))r.signatureLeads++;}
 }
 function buyAll(){
 let count=0;
 for(const x of authored){
  if(!eligible(x))continue;
  if(!affordable(uprice(x)))break;
  buy(x);count++;
 }
 if(count)totalActions+=2;
 }
 function levelCost(i,q){return famcost[i]*p.levelGrowth**r.levels[i]*(p.levelGrowth**q-1)/(p.levelGrowth-1)*(1-.35*r.pc[i]/16)*(1-.4*cards[i+8]/(cards[i+8]+4));}
 function buyLevels() {
  for(let action=0;action<2;action++){
   let candidate=null,score=-Infinity;const out=output();
   for(let i=0;i<8;i++){
    if(i>0&&r.levels[i-1]===0)continue;
    const n=r.levels[i],q=n===0?1:affordable(levelCost(i,100))?100:affordable(levelCost(i,10))?10:1,cost=levelCost(i,q);
    if(!affordable(cost))continue;
    const gain=n===0?faminc[i]*q*r.pp[i]*r.global*perm()*(1+.25*cards[26])*(1+.2*cards[i]):
      out.a[i]*((n+q)*2**(milestone(n+q)-milestone(n))/n-1);
    const value=(n===0?1000000:p.productWeight)*gain/cost;
    if(value>score){score=value;candidate={i,q,cost};}
   }
   const tapCost=q=>40*1.12**r.tapN*(1.12**q-1)/.12*(1-.35*r.tc/40)*(1-.4*cards[17]/(cards[17]+4));
   const tq=affordable(tapCost(100))?100:affordable(tapCost(10))?10:1,tc=tapCost(tq);
   const masteryGain=.25*((r.tapN+tq)/(r.tapN+tq+100)-r.tapN/(r.tapN+100));
   const linkShare=r.tl?.12+.18*(r.tl-1)/59:0;
   const tapgain=(1.4*tq*r.tapMult*r.global*perm()*(1+.25*cards[26])*p.incomeScale+out.P*linkShare*masteryGain/2)*(1+cards[16]/(cards[16]+5));
   if(options.taps!==0&&affordable(tc)&&tapgain/tc>score){r.cash-=tc;r.tapN+=tq;totalActions++;continue;}
   if(!candidate)break;
   const {i,q,cost}=candidate;r.cash=Math.max(0,r.cash-cost);r.levels[i]+=q;totalActions++;
   if(r.familyTimes[i]===null)r.familyTimes[i]=t-runStart;
   if(r.levels.every(n=>n>0)&&r.all8===null)r.all8=t-runStart;
  }
 }
 function activate(){
  const out=output();
  for(let s=0;s<3;s++){
   const [power,d,c]=r.skills[s];if(!power||r.ready[s]>t)continue;
   const cooldown=(s===2?600:300)*(1-.25*c/(s===2?12:8))*(1-.3*cards[18+s]/(cards[18+s]+5));
   r.ready[s]=t+cooldown;totalActions++;
   if(s<2)r.end[s]=t+30*(1+.5*d/8)*(1+.75*cards[21+s]/(cards[21+s]+5));
   else earn(out.P*[120,135,150,165,180,195,210,225,240,260,280,300][power-1]*(1+cards[23]/(cards[23]+5)),"bulk");
  }
 }
 function checkReset() {
  if(r.E<r.T*(resets?depth:1)||options.noReset)return false;
  const ratio=r.E/r.T,fg=Math.floor(Math.sqrt(ratio)+1e-12);
  const earned=resets?1+Math.floor(Math.log(ratio)/Math.log(4)+1e-12)+(ratio>=4?Math.floor(U/250):0):3;
  const record={t,day:t/86400,activeMinutes:activeSeconds/60,runHours:(t-runStart)/3600,U,owned:r.purchases,fameGain:fg,draws:earned,ratio,all8:r.all8===null?null:r.all8/60,maxGapHours:r.maxGap/3600,signatureLeads:r.signatureLeads,signatureLater:r.signatureLater};
  if(!first){first=record;firstOwned=r.owned.slice();firstFamTimes=r.familyTimes.slice();}
  if(events.length<5000)events.push(record);
  targetExponent+=1+(p.depthTargetWeight||0)*Math.floor(Math.log(ratio)/Math.log(4)+1e-12);
  F+=fg;resets++;draw(earned);freeDraws+=earned;
  if(resets===1&&options.paidDraws){draw(options.paidDraws,true);paidConsumed+=options.paidDraws;}
  if(resets===2&&options.introGrant){draw(1);giftConsumed++;}
  resetState();blackout=t+p.revealSeconds;return true;
 }
 function updateRebuild(){
  if(resets===1&&!rebuild&&firstOwned.every((v,i)=>!v||r.owned[i])&&r.levels.every(n=>n>0))
   rebuild={t,elapsedHours:(t-runStart)/3600,activeMinutes:activeSeconds/60-first.activeMinutes,firstToRebuildRatio:first.runHours/((t-runStart)/3600)};
 }
 resetState();
 function advanceOffline(to){
  const seconds=to-t,eff=.5+.5*cards[24]/(cards[24]+5),cap=(8+12*cards[25]/(cards[25]+5))*3600;
  earn(output().P*eff*Math.min(seconds,cap),"offline");t=to;
 }
 function visit(start,duration,tapShare=.5){
  if(start>t)advanceOffline(start);
  const end=t+duration,visitStart=t;
  let cycle=0,lastDecision=-1;
  while(t<end-1e-8){
   let dt=Math.min(p.step,end-t);activeSeconds+=dt;
   const manage=(t-visitStart)%20<10,decisionIndex=Math.floor((t-visitStart)/20);
   if(t>=blackout&&manage&&decisionIndex!==lastDecision){
    lastDecision=decisionIndex;
    if(true){
     if(policy==="buyall"){buyAll();buyLevels();}else{buyLevels();buyAll();}
     activate();updateRebuild();if(!(options.rebuildFirst&&resets===1&&!rebuild))checkReset();
    }
   }
   const out=output(),manualRate=options.taps===undefined?2:options.taps;
   const tap=(!manage&&t>=blackout)?manualRate:0;
   const cuts=[t,t+dt,...r.end.filter(v=>v>t&&v<t+dt)].sort((a,b)=>a-b);
   for(let j=1;j<cuts.length;j++){
    const len=cuts[j]-cuts[j-1],mid=(cuts[j]+cuts[j-1])/2,fr=r.end[0]>mid,hh=r.end[1]>mid;
    const fm=fr?[2,2.25,2.5,2.75,3,3.25,3.5,4][r.skills[0][0]-1]:1;
    earn(out.P*len,"product");
    if(fr)earn(out.P*(fm-1)*len,"rush");
    if(tap)earn(out.manual*tap*(fr?1.25:1)*len,"tap");
    if(hh)earn(out.auto*[3,4,5,6,8,10,12,16][r.skills[1][0]-1]*(fr?1.25:1)*len,"hands");
   }
   t+=dt;cycle++;
   if(options.stopAtFirst&&first)break;
  }
 }
 if(options.continuous){
  visit(0,(options.hours||12)*3600);
 }else{
  (options.opening||[[0,480],[7200,240],[18000,240],[28800,480]]).forEach(([s,d])=>{if(!(options.stopAtFirst&&first))visit(s,d);});
  if(options.matchedRebuild&&first){options.stopAtFirst=false;const base=first.t;for(const [offset,duration] of options.opening){if(!rebuild)visit(base+offset,duration);}}
  daily.push({day:1,U,F,resets,draws,unique:cards.filter(n=>n).length});
  const mins=options.minutes||20;
  for(let d=1;d<days;d++){
   const durations=[mins*.2,mins*.2,mins*.6].map(v=>v*60);
   const starts=options.visits===1?[d*86400+14*3600]:options.visits===2?[d*86400,d*86400+14*3600]:[d*86400,d*86400+8*3600,d*86400+14*3600];
   starts.forEach((s,i)=>visit(s,options.visits===1?mins*60:options.visits===2?mins*30:durations[i]));
   daily.push({day:d+1,U,F,resets,draws,unique:cards.filter(n=>n).length,tapRatio:output().P?2*output().manual/output().P:null});
  }
 }
 return {options,first,firstFamTimes,rebuild,U,F,resets,draws,unique:cards.filter(n=>n).length,cards,freeDraws,paidConsumed,giftConsumed,day:t/86400,
 daily,events,stream,activeMinutes:activeSeconds/60,totalActions,final:{E:r.E,T:r.T,P:output().P,cash:r.cash,levels:r.levels,owned:r.purchases},endIncomeMultiplier:perm()*(1+.25*cards[26]),rows:undefined};
}
return {build,simulate,defaults,prepared};

}
const REFERENCE_POLICY = {
  "opening": [
    [
      0,
      900
    ],
    [
      7200,
      300
    ],
    [
      14400,
      300
    ],
    [
      21600,
      300
    ],
    [
      28800,
      600
    ]
  ],
  "policy": "buyall",
  "rebuildFirst": true,
  "introGrant": true
};
function runReference(catalogueMarkdown, overrides = {}) {
 const model = createModel(parseCatalogue(catalogueMarkdown));
 return model.simulate({...REFERENCE_POLICY, seed:1009, days:150, ...overrides});
}
```
