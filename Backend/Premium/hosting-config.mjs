// Explicit deployment parameters. Never infer store metadata from client-authored save content.
export function premiumConfiguration({packageName,appIds,packIds}) {
  let apps,ids;
  try { apps=JSON.parse(appIds);ids=JSON.parse(packIds); } catch { throw Error('Premium configuration requires JSON arrays for app and product IDs'); }
  if(typeof packageName!=='string'||!/^([A-Za-z][A-Za-z0-9_]*\.)+[A-Za-z][A-Za-z0-9_]*$/.test(packageName) ||
     !Array.isArray(apps)||!apps.length||apps.some(x=>typeof x!=='string'||!/^1:[0-9]+:android:[a-f0-9]+$/.test(x))||new Set(apps).size!==apps.length ||
     !Array.isArray(ids)||ids.length!==4||ids.some(x=>typeof x!=='string'||!/[a-z]/.test(x)||!/^[a-z0-9._]{1,150}$/.test(x))||new Set(ids).size!==4)
    throw Error('Premium storefront configuration is incomplete or invalid');
  return {packageName,allowedAppIds:apps,packs:ids.map((id,i)=>({id,kajuKatli:[1000,6000,13000,28000][i]}))};
}
