// Exact individual artwork only. Shared matches are referenced, never reimported.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const base='UI Builder/HTML',target='Assets/Game/ClassicUI/Art';
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const walk=d=>fs.readdirSync(d,{withFileTypes:true}).flatMap(x=>x.isDirectory()?walk(path.join(d,x.name)):[path.join(d,x.name)]);
const existing=new Map(walk('Assets/Game/ImportedUI/Art').filter(p=>p.endsWith('.png')).map(p=>[hash(p),p.replaceAll('\\','/')]));
const requests=[['stall','Home/assets/gouache-stall.png'],['shopkeeper','Home/assets/gouache-shopkeeper.png']];
for(const n of ['sparklers','anar','chakri','rockets','small-crackers','cracker-strings','aerial','gift-boxes'])requests.push(['product-'+n,'Classic/assets/products/icons/'+n+'.png']);
for(const n of ['folk-common','folk-rare','folk-legendary','shared-card-back','full-outer-border','full-inner-border','full-rare-outer','full-rare-inner','full-legendary-outer','full-legendary-inner'])requests.push([n,'Classic/assets/cards/'+n+'.png']);
for(const p of walk(base+'/Classic/assets/cards/art').filter(p=>p.endsWith('.png')))requests.push([path.basename(p,'.png'),path.relative(base,p)]);
fs.mkdirSync(target,{recursive:true});
const manifest=[];
for(const [key,source] of requests){const from=path.join(base,source),sha256=hash(from);let output=existing.get(sha256),imported=!output;if(imported){output=target+'/'+key+'.png';if(fs.existsSync(output)&&hash(output)!==sha256)throw Error('Unexpected preexisting art: '+output);if(!fs.existsSync(output))fs.copyFileSync(from,output);}manifest.push({key,source:from.replaceAll('\\','/'),sha256,path:output,imported});}
fs.writeFileSync('Assets/Game/ClassicUI/art-manifest.json',JSON.stringify({assets:manifest},null,2));
console.log(`${manifest.length} mapped images; ${manifest.filter(x=>x.imported).length} new exact cutouts, ${manifest.filter(x=>!x.imported).length} shared matches`);
