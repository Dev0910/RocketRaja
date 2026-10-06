import { PremiumError } from './authority.mjs';

export class GoogleVoidedPurchaseVerifier {
  constructor(getAccessToken,request=fetch) {this.getAccessToken=getAccessToken;this.request=request;}
  async isVoided(packageName,purchaseToken) {
    const access=await this.getAccessToken();
    if(typeof access!=='string'||!access)throw new PremiumError('verification-unavailable');
    let next;const seen=new Set();
    // Default API window is the preceding 30 days. A scan cap fails closed and
    // requires operational reconciliation; it never authorizes an unverified debit.
    for(let page=0;page<10;page++){
      const url=new URL(`https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${encodeURIComponent(packageName)}/purchases/voidedpurchases`);
      url.searchParams.set('type','0');url.searchParams.set('maxResults','1000');
      url.searchParams.set('includeQuantityBasedPartialRefund','false');
      if(next)url.searchParams.set('token',next);
      const response=await this.request(url.toString(),{headers:{Authorization:'Bearer '+access,Accept:'application/json'},signal:AbortSignal.timeout(15000)});
      if(!response.ok)throw new PremiumError('verification-unavailable');
      const data=await response.json();
      if(data.voidedPurchases!==undefined&&!Array.isArray(data.voidedPurchases))throw new PremiumError('verification-unavailable');
      if((data.voidedPurchases??[]).some(p=>p.purchaseToken===purchaseToken&&(p.voidedQuantity===undefined||p.voidedQuantity===1)))return true;
      next=data.tokenPagination?.nextPageToken;
      if(!next)return false;
      if(typeof next!=='string'||seen.has(next))throw new PremiumError('verification-unavailable');
      seen.add(next);
    }
    throw new PremiumError('refund-page-limit');
  }
}

export function parsePlayNotification(message) {
  if(typeof message?.data!=='string'||message.data.length>32768)return null;
  try {
    const data=Buffer.from(message.data,'base64');
    if(data.length>16384)return null;
    return JSON.parse(data.toString('utf8'));
  } catch {return null;}
}

// Called exclusively by the IAM-protected Pub/Sub trigger, never the client HTTP API.
// A notification is a prompt to verify; it is not proof that currency should be revoked.
export class PremiumRefundNotifications {
  constructor(authority,verifier,packageName) {this.authority=authority;this.verifier=verifier;this.packageName=packageName;}
  async handle(notification,messageId) {
    const notice=notification?.voidedPurchaseNotification;
    if(notification?.packageName!==this.packageName||!notice||notice.productType!==2)return {ignored:true};
    if(notice.refundType!==1)throw new PremiumError('unsupported-refund-review');
    if(typeof notice.purchaseToken!=='string'||notice.purchaseToken.length<1||notice.purchaseToken.length>8192||
       typeof messageId!=='string'||!/^[A-Za-z0-9._:-]{1,128}$/.test(messageId))return {ignored:true};
    if(!await this.verifier.isVoided(this.packageName,notice.purchaseToken))throw new PremiumError('refund-verification-pending');
    return this.authority.revokeVerified(this.packageName,notice.purchaseToken,'rtdn:'+messageId);
  }
}
