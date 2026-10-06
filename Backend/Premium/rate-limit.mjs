import { createHash } from 'node:crypto';

// One bounded document per account/route, shared by every function instance. This is
// abuse throttling, not an entitlement rule; denied requests have no wallet effects.
export class PremiumRateLimiter {
  constructor(store,now=()=>Date.now(),limits={wallet:60,activate:10,fulfill:20,draw:60}) {
    this.store=store;this.now=now;this.limits={...limits};
    if(Object.values(this.limits).some(n=>!Number.isSafeInteger(n)||n<1))throw Error('Invalid rate limits');
  }
  async allow(uid,route) {
    const limit=this.limits[route];
    if(!limit||typeof uid!=='string'||!uid)return false;
    const window=Math.floor(this.now()/60000);
    if(!Number.isSafeInteger(window)||window<0)return false;
    const path='premiumRateLimits/'+createHash('sha256').update(uid+'\0'+route).digest('hex');
    return this.store.transaction(async tx=>{
      const previous=await tx.get(path);
      if(previous&&(!Number.isSafeInteger(previous.window)||!Number.isSafeInteger(previous.count)||previous.count<0||previous.window>window))return false;
      const count=previous?.window===window?previous.count:0;
      if(count>=limit)return false;
      tx.set(path,{window,count:count+1});return true;
    });
  }
}
