// Publish reproducibility metadata only. Never marks implementation or release acceptance automatically.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve('Documentation/Implementation'),target=path.join(root,'EvidenceManifest.json');
const normalize=s=>s.replace(/^\uFEFF/,'').replace(/\r\n/g,'\n');
const binary=p=>/\.(png|jpg|jpeg|ttf|otf|woff2?)$/i.test(p);
const digest=p=>crypto.createHash('sha256').update(binary(p)?fs.readFileSync(p):normalize(fs.readFileSync(p,'utf8'))).digest('hex').toUpperCase();
if(process.argv.includes('--verify')){
 const manifest=JSON.parse(fs.readFileSync(target,'utf8'));
 for(const entry of [...manifest.inputs,...manifest.outputs])if(!fs.existsSync(entry.path)||digest(entry.path)!==entry.sha256)throw Error('Evidence changed: '+entry.path);
 console.log(`Verified ${manifest.inputs.length} source inputs and ${manifest.outputs.length} evidence files (normalized UTF-8 text; image files use exact bytes).`);
 process.exit(0);
}
const relative=p=>path.relative(process.cwd(),p).replace(/\\/g,'/');
const entries=files=>[...new Set(files)].sort().map(p=>({path:relative(p),sha256:digest(p),...(binary(p)?{hashFormat:'SHA256 of exact binary bytes'}:{})}));
const direct=(dir,pattern)=>fs.readdirSync(dir,{withFileTypes:true}).filter(e=>e.isFile()&&pattern.test(e.name)).map(e=>path.join(dir,e.name));
const walk=dir=>fs.readdirSync(dir,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(dir,e.name)):[path.join(dir,e.name)]);
const catalogue=JSON.parse(fs.readFileSync('Documentation/RedesignContentCatalog.json','utf8'));
const source=[...direct('Assets/Game/Core',/\.cs$/),...direct('Assets/Game/Editor',/^(Redesign.*|BuildRedesign.*|BuildGame|ValidateGame|BuildImported(Home|Offline|Settings|Privacy|Prestige|Sponsor)|HomePageAuthoring)\.cs$/),
 ...direct('Assets/Game/Runtime',/^(Redesign.*|UnityKajuPurchasing|GameController|GameView|RewardedAds|ProductionConfig|Imported(Home|Offline|Sponsor|Privacy|Settings)Screen|SmokeHarness.*)\.cs$/), ...direct('Assets/Game/Runtime/Cloud',/\.cs$/),
 ...direct('Assets/Game/ImportedUI',/^(Home|Offline|Settings|Privacy|Prestige) Screen\.prefab$/),
 ...direct('Backend/Premium',/\.(mjs|json)$/),...direct('Tools/RedesignLab',/\.(cs|cjs|csproj)$/),...direct('Tools/RedesignDeviceQA',/\.(cs|ps1|md)$/),
 ...direct('Tools/CloudSaveTests',/\.(cjs|ps1|json)$/), '.gitattributes','firestore.rules','firestore.indexes.json','firebase.premium.review.json',
 'Documentation/RedesignContentCatalog.json','Assets/Resources/RedesignContentManifest.json','Assets/Resources/GameContent.asset','Assets/Resources/RedesignCloudSettings.asset','Assets/Resources/RedesignCloudSettings.asset.meta',
 'Packages/manifest.json','Packages/packages-lock.json','Assets/Plugins/Android/mainTemplate.gradle','Assets/Plugins/Android/settingsTemplate.gradle',
 'ProjectSettings/AndroidResolverDependencies.xml','Assets/Scenes/Main.unity','Assets/Scenes/HTML Importer.unity','Tools/EconomyLab/Program.cs','Documentation/FirebaseSDK.md','Documentation/ReleaseRunbook.md','Documentation/PrivacyPolicy-DRAFT.md',
 'Game Design/Economy Specification v1.md','Game Design/Economy Model v1.md','Game Design/Economy Simulation Report v1.md','Game Design/1000 Unlockables Catalogue.md','Game Design/Talent and Prestige Draft.md'];
source.push(...walk('Assets/Game/RedesignUI'), ...walk('Assets/Game/Runtime/NativeText'), 'Documentation/Architecture.md', 'Assets/Scenes/Redesign UI Preview.unity',
 ...JSON.parse(fs.readFileSync('Documentation/Implementation/NativeUI-Baseline.json', 'utf8').replace(/^\uFEFF/, '')).files.map(f=>f.path),
 ...direct('Assets/Game/ImportedUI/Art',/^(Product .*|Burst|Helping Hands|Bulk Order|Stall Background|Icon Lock)\.png(\.meta)?$/),
 ...direct('Assets/Resources/Art',/^(phuljhari|anar|chakri|rockets|small-crackers|cracker-strings|aerial-shots|gift-boxes|stalls)\.png(\.meta)?$/),
 ...direct('UI Builder/HTML/Redesign',/\.(html|css|js|cjs|md)$/), 'UI Builder/HTML/Redesign/product-appearances.json', 'UI Builder/HTML/Redesign/shop-stages.json', 'UI Builder/HTML/Redesign/guidance-topics.json',
 ...walk('UI Builder/Reference Images/Redesign'),...walk('UI Builder/Wireframes/Redesign'),
 'UI Builder/HTML/Home/assets/gouache-paper.svg','UI Builder/HTML/Home/assets/gouache-stall.png','UI Builder/HTML/Home/assets/gouache-products.png',
 'UI Builder/HTML/Shared/fonts/nunito.ttf','UI Builder/HTML/Shared/fonts/fraunces.ttf');
const outputs=walk(root).filter(p=>p!==target&&(binary(p)||/\.(md|txt|json|save|bak|checkpoint)$/.test(p)||/\.save\.pre-v2-[A-F0-9]+$/i.test(p)));
outputs.push(...walk('UI Builder/HTML/Redesign/renders'),'UI Builder/HTML/Redesign/inspection.json');
const manifest={generatedAt:new Date().toISOString(),hashFormat:'SHA256 of UTF-8 text, leading BOM removed and CRLF normalized to LF',
 economy:catalogue.economyVersion,content:catalogue.contentVersion,authoredSourceHash:catalogue.sourceHash,
 status:'Reproducible implementation evidence. Ordered visual handoff complete with approval waiver; native integration and prototype device/player acceptance incomplete. Actual IAP linking deferred by user.',
 inputs:entries(source),outputs:entries(outputs)};
fs.writeFileSync(target,JSON.stringify(manifest,null,2)+'\n');
console.log(`Published ${manifest.inputs.length} source inputs and ${manifest.outputs.length} evidence files.`);
