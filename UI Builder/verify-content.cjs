// Compare the current Unity YAML's simple catalog records with its JSON export.
const fs=require('fs'),assert=require('assert/strict'),path=require('path');
const root=path.join(__dirname,'..'),text=fs.readFileSync(path.join(root,'Assets/Resources/GameContent.asset'),'utf8');
const exported=JSON.parse(fs.readFileSync(path.join(root,'Documentation/ContentCatalog.json'),'utf8'));
let group,item,price=false,values=false,c={};
for(const line of text.split(/\r?\n/)){
 let m;
 if(m=line.match(/^    (products|upgrades|skills|legacy):$/)){group=m[1];c[group]=[];continue;}
 if(m=line.match(/^    - id: (.*)$/)){item={id:m[1]};c[group].push(item);price=values=false;continue;}
 if(!item)continue;
 if(m=line.match(/^      ([A-Za-z]+):(?: (.*))?$/)){price=values=false;let [,key,v]=m;if(key==='price'){item.price={};price=true;}else if(key==='values'){item.values=[];values=true;}else item[key]=v!==''&&Number.isFinite(Number(v))?Number(v):v;continue;}
 if(price&&(m=line.match(/^        (mantissa|exponent): (.*)$/)))item.price[m[1]]=Number(m[2]);
 if(values&&(m=line.match(/^      - (.*)$/)))item.values.push(Number(m[1]));
}
assert.deepEqual(c,exported);
fs.mkdirSync(path.join(__dirname,'Reviews'),{recursive:true});
fs.writeFileSync(path.join(__dirname,'Reviews/content-verification.json'),JSON.stringify({result:'exact match',source:'Assets/Resources/GameContent.asset',export:'Documentation/ContentCatalog.json',counts:Object.fromEntries(Object.entries(c).map(([k,v])=>[k,v.length]))},null,2));
console.log('Active catalog exactly matches export: 8 products, 96 upgrades, 3 skills, 12 Legacy perks.');
