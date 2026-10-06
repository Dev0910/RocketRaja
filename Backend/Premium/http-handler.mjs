import { PremiumError } from './authority.mjs';

// Bind to an HTTPS server only after deployment review. Every dependency is a trusted server service.
export function createPremiumHandler({ authority, verifyIdToken, verifyAppCheck, checkRate, allowedAppIds }) {
  if (!authority || typeof verifyIdToken !== 'function' || typeof verifyAppCheck !== 'function' || typeof checkRate !== 'function' || !Array.isArray(allowedAppIds) || !allowedAppIds.length)
    throw new Error('Premium authority authentication is not configured');
  const fields = {
    wallet: [],
    activate: ['owner', 'generation'],
    fulfill: ['productId', 'purchaseToken'],
    draw: ['operationId', 'expectedRevision', 'owner', 'generation']
  };
  return async (request, response) => {
    response.set('Cache-Control','no-store');
    if(request.method !== 'POST') return response.status(405).json({error:'method-not-allowed'});
    const route = request.path.split('/').filter(Boolean).at(-1);
    if(!Object.hasOwn(fields,route)) return response.status(404).json({error:'not-found'});
    const body=request.body;
    if(!body || Array.isArray(body) || Object.getPrototypeOf(body)!==Object.prototype ||
       (request.rawBody?.length ?? Buffer.byteLength(JSON.stringify(body)))>16384 ||
       Object.keys(body).some(k=>!fields[route].includes(k)) || fields[route].some(k=>!Object.hasOwn(body,k)))
      return response.status(400).json({error:'invalid-request'});
    let auth;
    try {
      const bearer=request.get('Authorization')?.match(/^Bearer (\S+)$/)?.[1];
      const appToken=request.get('X-Firebase-AppCheck');
      if(!bearer || !appToken) throw Error('missing authentication');
      auth=await verifyIdToken(bearer, true);
      const app=await verifyAppCheck(appToken);
      if(auth.firebase?.sign_in_provider!=='google.com' || !allowedAppIds.includes(app.appId)) throw Error('unsupported identity');
    } catch { return response.status(401).json({error:'authentication-required'}); }
    try {
      if(!await checkRate(auth.uid,route)) return response.status(429).json({error:'retry-later'});
      const actor={uid:auth.uid,owner:body.owner,generation:body.generation};
      const result=route==='wallet' ? await authority.wallet(actor) : route==='activate' ? await authority.activate(actor) : route==='fulfill' ? await authority.fulfill(actor,body) : await authority.draw(actor,body);
      return response.status(200).json(result);
    } catch(error) {
      const code=error instanceof PremiumError ? error.code : 'service-unavailable';
      const conflict=['ownership-lost','stale-wallet','insufficient-currency','talent-locked','purchase-owner-mismatch','payment-pending','profile-owner-mismatch','wallet-not-active','progress-recovery-required','gift-provenance-required','operation-not-confirmed'];
      const invalid=['invalid-operation','invalid-purchase','purchase-invalid','purchase-mismatch'];
      return response.status(conflict.includes(code)?409:invalid.includes(code)?400:503).json({error:code});
    }
  };
}
