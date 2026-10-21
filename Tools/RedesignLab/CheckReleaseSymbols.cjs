// Compile actual core sources under player/lab symbol combinations in an ignored temporary project.
const fs=require('node:fs'),path=require('node:path'),{spawnSync}=require('node:child_process');
const root=process.cwd(),folder=path.join(root,'Temp','ReleaseSymbols-'+Date.now());
fs.mkdirSync(folder,{recursive:true});
const xml=s=>s.replace(/&/g,'&amp;').replace(/"/g,'&quot;').replace(/</g,'&lt;');
const project=path.join(folder,'ReleaseSymbols.csproj');
fs.writeFileSync(project,`<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems><ImplicitUsings>enable</ImplicitUsings><Nullable>disable</Nullable></PropertyGroup><ItemGroup><Compile Include="${xml(path.join(root,'Assets/Game/Core/*.cs'))}"/><Compile Include="Probe.cs"/></ItemGroup></Project>`);
fs.writeFileSync(path.join(folder,'Probe.cs'),`class Probe { static int Main(string[] args) { bool expected = args[0] == "present"; bool present = typeof(RocketRaja.Core.RedesignState).Assembly.GetType("RocketRaja.Core.DisposableFixtureMigration") != null; bool testShop = typeof(RocketRaja.Core.RedesignEngine).GetMethod("EnableDevelopmentShop") != null; var marker = new RocketRaja.Core.DevelopmentShopState { profileNamespace = RocketRaja.Core.DevelopmentShopState.Namespace, operationId = "", packId = "", status = "ready" }; bool acceptsTestSave = marker.Valid(null); System.Console.WriteLine("Fixture-reset=" + present + "; simulated shop=" + testShop + "; accepts test marker=" + acceptsTestSave); return present == expected && testShop == expected && acceptsTestSave == expected ? 0 : 2; } }`);
const run=args=>{
 const r=spawnSync('dotnet',args,{cwd:root,encoding:'utf8',timeout:120000,windowsHide:true});
 if(r.error)throw r.error;
 return {code:r.status,output:(r.stdout||'')+(r.stderr||'')};
};
let result=run(['restore',project,'--nologo']);if(result.code!==0)throw Error(result.output);
const cases=[
 {name:'ordinary Unity release excludes fixture reset/test grants and rejects simulated-shop saves',symbols:['UNITY_5_3_OR_NEWER'],present:false},
 {name:'Unity release rejects injected lab symbol',symbols:['UNITY_5_3_OR_NEWER','REDESIGN_LAB'],reject:true},
 {name:'Unity development diagnostic retains restricted fixture reset',symbols:['UNITY_5_3_OR_NEWER','DEVELOPMENT_BUILD'],present:true},
 {name:'standalone .NET lab retains restricted fixture reset',symbols:['REDESIGN_LAB'],present:true}
];
const results=[];
for(let i=0;i<cases.length;i++){
 const c=cases[i],output=path.join(folder,'out-'+i);
 result=run(['build',project,'--no-restore','--nologo','--verbosity','minimal','-p:DefineConstants='+c.symbols.join('%3B'),'-o',output]);
 fs.writeFileSync(path.join(folder,'compile-'+i+'.txt'),result.output);
 if(c.reject){if(result.code===0||!result.output.includes('CS1029')||!result.output.includes('REDESIGN_LAB is not permitted'))throw Error(c.name+'\n'+result.output);}
 else{
  if(result.code!==0)throw Error(c.name+'\n'+result.output);
  result=run([path.join(output,'ReleaseSymbols.dll'),c.present?'present':'absent']);
  if(result.code!==0)throw Error(c.name+'\n'+result.output);
 }
 results.push('PASS '+c.name);console.log(results.at(-1));
}
results.push('Scope: actual C# core compilation/type inspection with simulated Unity symbols; not a Unity/IL2CPP release artifact or release acceptance.');
fs.writeFileSync(path.join(root,'Documentation/Implementation/Release-Symbols.txt'),results.join('\n')+'\n');
