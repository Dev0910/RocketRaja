import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { PremiumAuthority, accountId, publicWallet, receiptKey } from './authority.mjs';
import { GooglePlayVerifier } from './google-play.mjs';
import { createPremiumHandler } from './http-handler.mjs';
import { PremiumRateLimiter } from './rate-limit.mjs';
import { premiumConfiguration } from './hosting-config.mjs';
import { GoogleVoidedPurchaseVerifier, PremiumRefundNotifications, parsePlayNotification } from './refund-notifications.mjs';

// Test-only serializable store with explicit precommit failure, lost response and internal retry injection.
class MemoryStore {
  records = new Map(); queue = Promise.resolve(); failBefore = false; failAfter = false; retryBody = false;
  transaction(action) {
    const work = this.queue.then(async () => {
      const attempt = async () => {
        const staged = new Map([...this.records].map(([k,v])=>[k,structuredClone(v)]));
        const result = await action({get: async k=>staged.get(k) ?? null, set:(k,v)=>staged.set(k,structuredClone(v))});
        return {staged,result};
      };
      if(this.retryBody) {this.retryBody=false;await attempt();}
      const {staged,result}=await attempt();
      if(this.failBefore){this.failBefore=false;throw Error('disk unavailable');}
      this.records=staged;
      if(this.failAfter){this.failAfter=false;throw Error('response lost');}
      return structuredClone(result);
    });
    this.queue=work.catch(()=>{});return work;
  }
}
const packs=JSON.parse(fs.readFileSync(new URL('../../Documentation/RedesignContentCatalog.json',import.meta.url),'utf8')).packs;
const packageName='com.synthetic.rocket.fixture',uid='synthetic-player-a';
const actor={uid,owner:'a'.repeat(32),generation:5};
const request={productId:packs[0].id,purchaseToken:'synthetic-token-1'};
const operationId='1'.repeat(32);
const profileId='c'.repeat(32);
const seededWallet=()=>({schema:1,revision:'0',grants:'0',spent:'0',revoked:'0',cards:Array(28).fill('0'),profileId,giftRegistered:false,importedGiftCardId:'',importedGiftOperationId:''});
function fixture(){
 const store=new MemoryStore();let randomCalls=0,verifyCalls=0;
 const verified={productId:request.productId,purchaseState:0,consumptionState:0,quantity:1,obfuscatedExternalAccountId:accountId(uid)};
 const play={verify:async (_package,product)=>{verifyCalls++;return {...verified,productId:verified.productId===request.productId?product:verified.productId};}};
 const authority=new PremiumAuthority({store,play,packageName,packs,now:()=>100000,random:max=>{randomCalls++;return 0;}});
 store.records.set('redesignUser/'+uid,{owner:actor.owner,generation:5,handoff:'',heartbeatMs:99900,save:{json:JSON.stringify({schema:3,economy:'2.0.0',completedPrestiges:{digits:'1'},guestWallet:{profileId,giftDrawCardId:'',giftDrawOperationId:''},premium:{uid,profileId,pendingOperationId:operationId,pendingExpectedRevision:'1'}})}});
 store.records.set('premiumWallets/'+uid,seededWallet());
 store.records.set('premiumProfiles/'+profileId,{uid,profileId});
 return {store,verified,play,authority,randomCalls:()=>randomCalls,verifyCalls:()=>verifyCalls};
}
test('deployment configuration fixes grants and requires explicit unique store/app identifiers',()=>{
 const source={packageName,appIds:JSON.stringify(['1:123:android:abcdef']),packIds:JSON.stringify(packs.map(p=>p.id))};
 const valid=premiumConfiguration(source);assert.deepEqual(valid.packs.map(p=>p.kajuKatli),[1000,6000,13000,28000]);
 for(const changed of [{appIds:'[]'},{packIds:'[]'},{packageName:''},{appIds:'not json'},{packIds:JSON.stringify(Array(4).fill(packs[0].id))},{packIds:JSON.stringify(['../other',...packs.slice(1).map(p=>p.id)])}])
  assert.throws(()=>premiumConfiguration({...source,...changed}));
});
test('shared rate limiter admits exactly the limit under concurrent requests',async()=>{
 const store=new MemoryStore(),rates=new PremiumRateLimiter(store,()=>120000,{draw:3});
 const results=await Promise.all(Array.from({length:12},()=>rates.allow(uid,'draw')));
 assert.equal(results.filter(Boolean).length,3);assert.equal(store.records.size,1);
 assert.equal(await new PremiumRateLimiter(store,()=>120000,{draw:3}).allow(uid,'draw'),false);
});
test('rate buckets renew forward, reject clock rollback and isolate accounts/routes',async()=>{
 const store=new MemoryStore();let now=120000;
 const rates=new PremiumRateLimiter(store,()=>now,{draw:1,wallet:1});
 assert.equal(await rates.allow(uid,'draw'),true);assert.equal(await rates.allow(uid,'draw'),false);
 assert.equal(await rates.allow(uid,'wallet'),true);assert.equal(await rates.allow('another','draw'),true);
 now=60000;assert.equal(await rates.allow(uid,'draw'),false);
 now=180000;assert.equal(await rates.allow(uid,'draw'),true);assert.equal(await rates.allow(uid,'draw'),false);
 assert.equal(await rates.allow(uid,'unknown'),false);
});
test('rate storage failure denies work instead of bypassing the limiter',async()=>{
 const store=new MemoryStore(),rates=new PremiumRateLimiter(store,()=>120000,{draw:1});store.failBefore=true;
 await assert.rejects(rates.allow(uid,'draw'));assert.equal(store.records.size,0);
});
function progress(f, changes={}){
 const account=f.store.records.get('redesignUser/'+uid);
 const value={...JSON.parse(account.save.json),...changes};account.save.json=JSON.stringify(value);return value;
}
function giftProgress(f, spent=false){
 return progress(f,{schema:2,completedPrestiges:{digits:'2'},secondPrestigeGift:true,kajuKatli:{digits:spent?'0':'100'},
  cardLevels:Array.from({length:28},(_,i)=>({digits:spent&&i===0?'1':'0'})),
  guestWallet:{profileId,giftDrawCardId:spent?'C01':'',giftDrawOperationId:spent?operationId:''}});
}
test('activation binds a fresh profile before any real-money balance exists',async()=>{
 const f=fixture();f.store.records.delete('premiumWallets/'+uid);f.store.records.delete('premiumProfiles/'+profileId);
 const wallet=await f.authority.activate(actor);
 assert.equal(wallet.uid,uid);assert.equal(wallet.profileId,profileId);assert.equal(wallet.balance,'0');assert.equal(wallet.revision,'1');
 assert.deepEqual(await f.authority.activate(actor),wallet);
});
test('unspent guest second-prestige gift promotes exactly once, including a lost response',async()=>{
 const f=fixture();giftProgress(f);f.store.failAfter=true;
 await assert.rejects(f.authority.activate(actor));
 const wallet=await f.authority.activate(actor);
 assert.equal(wallet.balance,'100');assert.equal(wallet.giftRegistered,true);assert.equal(wallet.revision,'1');
 assert.equal(f.store.records.get('premiumWallets/'+uid).grants,'100');assert.equal(f.randomCalls(),0);
});
test('spent guest gift imports its original card without random selection or replacement currency',async()=>{
 const f=fixture();giftProgress(f,true);
 const results=await Promise.all(Array.from({length:5},()=>f.authority.activate(actor)));
 assert.ok(results.every(w=>w.cards[0]==='1'&&w.balance==='0'&&w.revision==='1'));
 assert.equal(results[0].importedGiftCardId,'C01');assert.equal(results[0].importedGiftOperationId,operationId);
 assert.equal(f.store.records.get('premiumWallets/'+uid).grants,'100');assert.equal(f.store.records.get('premiumWallets/'+uid).spent,'100');assert.equal(f.randomCalls(),0);
});
test('account second prestige grants gift from confirmed progress without trusting its cached wallet balance',async()=>{
 const f=fixture();giftProgress(f);progress(f,{schema:3,premium:{uid,profileId},kajuKatli:{digits:'0'}});
 await f.authority.fulfill(actor,request);
 const wallet=await f.authority.activate(actor);assert.equal(wallet.balance,'1100');assert.equal(wallet.revision,'2');
 assert.deepEqual(await f.authority.activate(actor),wallet);
});
test('activation failure atomically preserves profile, wallet and original guest result',async()=>{
 const f=fixture();giftProgress(f,true);f.store.records.delete('premiumProfiles/'+profileId);f.store.records.delete('premiumWallets/'+uid);f.store.failBefore=true;
 await assert.rejects(f.authority.activate(actor));
 assert.ok(!f.store.records.has('premiumProfiles/'+profileId)&&!f.store.records.has('premiumWallets/'+uid));
 const wallet=await f.authority.activate(actor);assert.equal(wallet.cards[0],'1');assert.equal(wallet.balance,'0');
});
test('same guest profile cannot bind to a second account',async()=>{
 const f=fixture();giftProgress(f);await f.authority.activate(actor);
 const other={...actor,uid:'synthetic-player-b'};f.store.records.set('redesignUser/'+other.uid,structuredClone(f.store.records.get('redesignUser/'+uid)));
 await assert.rejects(f.authority.activate(other),{code:'profile-owner-mismatch'});
 assert.equal((await f.authority.wallet(other)).balance,'0');
});
test('another guest profile cannot replace an account wallet',async()=>{
 const f=fixture();progress(f,{schema:2,guestWallet:{profileId:'e'.repeat(32)}});
 await assert.rejects(f.authority.activate(actor),{code:'profile-owner-mismatch'});
});
for(const [label,change,code] of [
 ['ambiguous consumed gift',p=>{p.kajuKatli.digits='0';},'gift-provenance-required'],
 ['missing original card',p=>{p.guestWallet.giftDrawCardId='C01';p.guestWallet.giftDrawOperationId=operationId;p.kajuKatli.digits='0';},'gift-provenance-required'],
 ['unknown gift card',p=>{p.guestWallet.giftDrawCardId='C99';p.guestWallet.giftDrawOperationId=operationId;p.kajuKatli.digits='0';},'gift-provenance-required'],
 ['missing delivery marker',p=>{p.secondPrestigeGift=false;},'invalid-progress'],
 ['recovered ambiguous draw',p=>{p.requiresDrawReconciliation=true;},'progress-recovery-required'],
 ['mismatched paid UID',p=>{p.schema=3;p.premium={uid:'someone-else',profileId};},'purchase-owner-mismatch']])
 test(label+' refuses gift activation without a partial grant',async()=>{
  const f=fixture(),p=giftProgress(f);change(p);progress(f,p);
  await assert.rejects(f.authority.activate(actor),{code});assert.equal((await f.authority.wallet(actor)).balance,'0');
 });
test('profile activation obeys the same ownership lease as paid spending',async()=>{
 const f=fixture();giftProgress(f);f.store.records.get('redesignUser/'+uid).handoff='b'.repeat(32);
 await assert.rejects(f.authority.activate(actor),{code:'ownership-lost'});
});
test('paid draw rejects mismatched wallet identity and ambiguous recovered progress',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);
 progress(f,{schema:3,premium:{uid:'someone-else',profileId}});
 await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'1'}),{code:'purchase-owner-mismatch'});
 progress(f,{schema:3,premium:{uid,profileId},requiresDrawReconciliation:true});
 await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'1'}),{code:'progress-recovery-required'});
 assert.equal(f.randomCalls(),0);
});
test('paid operation must already be in the confirmed cloud save before a new debit',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);
 progress(f,{premium:{uid,profileId,pendingOperationId:'',pendingExpectedRevision:''}});
 await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'1'}),{code:'operation-not-confirmed'});
 assert.equal(f.randomCalls(),0);assert.equal((await f.authority.wallet(actor)).balance,'1000');
});
for(const pack of packs)test(pack.id+' verifies and grants exactly its authored amount',async()=>{
 const f=fixture();const reply=await f.authority.fulfill(actor,{...request,productId:pack.id});
 assert.equal(reply.granted,String(pack.kajuKatli));assert.equal((await f.authority.wallet(actor)).balance,String(pack.kajuKatli));
 assert.equal(f.randomCalls(),0);assert.equal(f.verifyCalls(),1);
});
test('concurrent duplicate callbacks create one durable grant',async()=>{
 const f=fixture();const replies=await Promise.all(Array.from({length:8},()=>f.authority.fulfill(actor,request)));
 assert.ok(replies.every(r=>r.revision==='1'));assert.equal((await f.authority.wallet(actor)).balance,'1000');
 assert.equal([...f.store.records.keys()].filter(x=>x.startsWith('premiumReceipts/')).length,1);
});
test('reinstall replay of consumed receipt uses durable record without another grant or network verification',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);f.play.verify=()=>{throw Error('offline');};
 const restored=new PremiumAuthority({store:f.store,play:f.play,packageName,packs});
 assert.equal((await restored.fulfill(actor,request)).granted,'1000');assert.equal((await restored.wallet(actor)).balance,'1000');
});
test('receipt cannot be transferred to another authenticated player',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);
 await assert.rejects(f.authority.fulfill({...actor,uid:'synthetic-player-b'},request),{code:'purchase-owner-mismatch'});
 assert.equal((await f.authority.wallet({...actor,uid:'synthetic-player-b'})).balance,'0');
});
for(const [label,fields,code] of [
 ['deferred',{purchaseState:2},'payment-pending'],['canceled',{purchaseState:1},'purchase-invalid'],
 ['wrong identity',{obfuscatedExternalAccountId:'wrong'},'purchase-mismatch'],['wrong SKU',{productId:'wrong'},'purchase-mismatch'],
 ['unrecorded consumed',{consumptionState:1},'purchase-mismatch'],['unexpected quantity',{quantity:2},'purchase-mismatch']])
 test(label+' receipt never grants currency',async()=>{const f=fixture();Object.assign(f.verified,fields);await assert.rejects(f.authority.fulfill(actor,request),{code});assert.equal((await f.authority.wallet(actor)).balance,'0');});
test('invalid SKU and anonymous caller reject before verification',async()=>{
 const f=fixture();await assert.rejects(f.authority.fulfill(actor,{...request,productId:'unknown'}),{code:'invalid-purchase'});
 await assert.rejects(f.authority.fulfill({},request),{code:'unauthenticated'});assert.equal(f.verifyCalls(),0);
});
test('verification network failure does not create a partial grant',async()=>{
 const f=fixture();f.play.verify=()=>{throw Error('network lost');};await assert.rejects(f.authority.fulfill(actor,request));assert.equal((await f.authority.wallet(actor)).balance,'0');
});
test('draw commits debit and result together and retries after a lost response without reroll',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);f.store.failAfter=true;
 await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'1'}));
 const result=await f.authority.draw(actor,{operationId,expectedRevision:'1'});
 assert.equal(result.cardId,'C01');assert.equal(result.after,'1');assert.equal(result.wallet.balance,'900');assert.equal(f.randomCalls(),2);
 assert.equal((await f.authority.wallet(actor)).cards[0],'1');
});
test('Firestore transaction retry retains the selected identity',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);f.store.retryBody=true;
 const result=await f.authority.draw(actor,{operationId,expectedRevision:'1'});assert.equal(result.wallet.balance,'900');assert.equal(f.randomCalls(),2);
});
test('failure before draw commit keeps balance and all cards unchanged',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);f.store.failBefore=true;
 await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'1'}));
 const wallet=await f.authority.wallet(actor);assert.equal(wallet.balance,'1000');assert.ok(wallet.cards.every(v=>v==='0'));
});
test('two devices cannot spend the same wallet revision',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);
 const results=await Promise.allSettled(['1','2'].map(c=>f.authority.draw(actor,{operationId:c.repeat(32),expectedRevision:'1'})));
 assert.equal(results.filter(x=>x.status==='fulfilled').length,1);assert.equal(results.find(x=>x.status==='rejected').reason.code,'stale-wallet');
 assert.equal((await f.authority.wallet(actor)).balance,'900');
});
for(const [label,change] of [
 ['generation change',a=>a.generation++],['owner change',a=>a.owner='b'.repeat(32)],['handoff',a=>a.handoff='b'.repeat(32)],
 ['expired lease',a=>a.heartbeatMs=70000],['future timestamp',a=>a.heartbeatMs=106000]])
 test(label+' fences a new paid draw',async()=>{const f=fixture();await f.authority.fulfill(actor,request);change(f.store.records.get('redesignUser/'+uid));await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'1'}),{code:'ownership-lost'});assert.equal(f.randomCalls(),0);});
test('Talent gate and insufficient balance reject before choosing a card',async()=>{
 const f=fixture();await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'0'}),{code:'insufficient-currency'});
 f.store.records.get('redesignUser/'+uid).save.json=JSON.stringify({schema:2,economy:'2.0.0',completedPrestiges:{digits:'0'},guestWallet:{profileId}});
 await assert.rejects(f.authority.draw(actor,{operationId,expectedRevision:'0'}),{code:'talent-locked'});assert.equal(f.randomCalls(),0);
});
test('refund is idempotent, retains cards and records spent-currency deficit',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);await f.authority.draw(actor,{operationId,expectedRevision:'1'});
 await f.authority.revokeVerified(packageName,request.purchaseToken,'verified-refund-1');await f.authority.revokeVerified(packageName,request.purchaseToken,'verified-refund-duplicate');
 let wallet=await f.authority.wallet(actor);assert.equal(wallet.balance,'0');assert.equal(wallet.deficit,'100');assert.equal(wallet.cards[0],'1');
 await f.authority.fulfill(actor,{...request,purchaseToken:'synthetic-token-2'});wallet=await f.authority.wallet(actor);assert.equal(wallet.balance,'900');assert.equal(wallet.deficit,'0');
});
test('verified refund before first delivery blocks an otherwise valid stale purchase response',async()=>{
 const f=fixture();await f.authority.revokeVerified(packageName,request.purchaseToken,'refund-before-grant');
 await assert.rejects(f.authority.fulfill(actor,request),{code:'purchase-invalid'});
 assert.equal(f.verifyCalls(),0);assert.equal((await f.authority.wallet(actor)).balance,'0');
 assert.ok(f.store.records.has('premiumRevocations/'+receiptKey(packageName,request.purchaseToken)));
});
test('refund arriving during receipt verification atomically prevents the later grant',async()=>{
 const f=fixture();f.play.verify=async()=>{await f.authority.revokeVerified(packageName,request.purchaseToken,'racing-refund');return {...f.verified};};
 await assert.rejects(f.authority.fulfill(actor,request),{code:'purchase-invalid'});
 assert.equal((await f.authority.wallet(actor)).balance,'0');assert.ok(!f.store.records.has('premiumReceipts/'+receiptKey(packageName,request.purchaseToken)));
});
test('notification alone cannot revoke a pack; verified retry revokes once and keeps drawn cards',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);await f.authority.draw(actor,{operationId,expectedRevision:'1'});
 let verified=false;const worker=new PremiumRefundNotifications(f.authority,{isVoided:async()=>verified},packageName);
 const notice={packageName,voidedPurchaseNotification:{productType:2,refundType:1,purchaseToken:request.purchaseToken}};
 await assert.rejects(worker.handle(notice,'message-1'),{code:'refund-verification-pending'});
 assert.equal((await f.authority.wallet(actor)).balance,'900');verified=true;
 await worker.handle(notice,'message-1');await worker.handle(notice,'message-1-redelivery');
 const w=await f.authority.wallet(actor);assert.equal(w.balance,'0');assert.equal(w.deficit,'100');assert.equal(w.cards[0],'1');assert.equal(w.revision,'3');
});
test('pending cancellations, other packages and subscriptions cannot revoke currency',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);let calls=0;
 const worker=new PremiumRefundNotifications(f.authority,{isVoided:async()=>{calls++;return true;}},packageName);
 for(const notice of [null,{packageName,oneTimeProductNotification:{notificationType:2,purchaseToken:request.purchaseToken}},
  {packageName:'wrong',voidedPurchaseNotification:{productType:2,refundType:1,purchaseToken:request.purchaseToken}},
  {packageName,voidedPurchaseNotification:{productType:1,refundType:1,purchaseToken:request.purchaseToken}}])
  assert.deepEqual(await worker.handle(notice,'message'),{ignored:true});
 assert.equal(calls,0);assert.equal((await f.authority.wallet(actor)).balance,'1000');
});
test('multi-quantity refund is routed to review without guessing a debit',async()=>{
 const f=fixture();await f.authority.fulfill(actor,request);
 const worker=new PremiumRefundNotifications(f.authority,{isVoided:async()=>true},packageName);
 await assert.rejects(worker.handle({packageName,voidedPurchaseNotification:{productType:2,refundType:2,purchaseToken:request.purchaseToken}},'message'),{code:'unsupported-refund-review'});
 assert.equal((await f.authority.wallet(actor)).balance,'1000');
});
test('Pub/Sub parser bounds data and rejects invalid JSON without trusting embedded commands',()=>{
 assert.equal(parsePlayNotification({data:'not json'}),null);assert.equal(parsePlayNotification({data:'x'.repeat(32769)}),null);
 const value={packageName,testNotification:{version:'1.0'}};
 assert.deepEqual(parsePlayNotification({data:Buffer.from(JSON.stringify(value)).toString('base64')}),value);
});
test('Voided Purchases verifier follows pagination and checks the exact token with server credentials',async()=>{
 const requests=[];const verifier=new GoogleVoidedPurchaseVerifier(async()=>'synthetic-server-token',async(url,options)=>{
  requests.push({url,options});return {ok:true,json:async()=>requests.length===1?{voidedPurchases:[{purchaseToken:'different'}],tokenPagination:{nextPageToken:'page/+?'}}:{voidedPurchases:[{purchaseToken:request.purchaseToken}]}};
 });
 assert.equal(await verifier.isVoided(packageName,request.purchaseToken),true);assert.equal(requests.length,2);
 const url=new URL(requests[1].url);assert.equal(url.searchParams.get('token'),'page/+?');assert.equal(url.searchParams.get('type'),'0');
 assert.equal(url.searchParams.get('includeQuantityBasedPartialRefund'),'false');assert.equal(requests[0].options.headers.Authorization,'Bearer synthetic-server-token');
 assert.ok(!requests[0].url.includes(request.purchaseToken)&&!requests[0].url.includes('synthetic-server-token'));
});
test('Voided Purchases service errors and pagination loops never establish a refund',async()=>{
 let verifier=new GoogleVoidedPurchaseVerifier(async()=>'synthetic',async()=>({ok:false,status:503}));
 await assert.rejects(verifier.isVoided(packageName,request.purchaseToken),{code:'verification-unavailable'});
 verifier=new GoogleVoidedPurchaseVerifier(async()=>'synthetic',async()=>({ok:true,json:async()=>({tokenPagination:{nextPageToken:'same'}})}));
 await assert.rejects(verifier.isVoided(packageName,request.purchaseToken),{code:'verification-unavailable'});
 verifier=new GoogleVoidedPurchaseVerifier(async()=>'synthetic',async()=>({ok:true,json:async()=>({voidedPurchases:[]})}));
 assert.equal(await verifier.isVoided(packageName,request.purchaseToken),false);
});
test('balances above the safe JavaScript integer range stay exact',()=>{
 const n=10n**100n;const w={schema:1,revision:n.toString(),grants:n.toString(),spent:'100',revoked:'0',cards:Array(28).fill('0')};
 assert.equal(publicWallet(w).balance,(n-100n).toString());
});
test('Google verifier encodes token and keeps server credential in authorization header',async()=>{
 let seen;const verifier=new GooglePlayVerifier(async()=>'synthetic-access-token',async(url,options)=>{seen={url,options};return {ok:true,json:async()=>({purchaseState:0})};});
 await verifier.verify(packageName,'sku','token/+?');assert.ok(seen.url.endsWith('token%2F%2B%3F'));assert.equal(seen.options.headers.Authorization,'Bearer synthetic-access-token');assert.ok(!seen.url.includes('synthetic-access-token'));
});
test('Google verifier classifies invalid receipts and unavailable service without exposing response bodies',async()=>{
 for(const [status,code]of [[404,'purchase-invalid'],[403,'verification-unavailable'],[503,'verification-unavailable']]){
  const verifier=new GooglePlayVerifier(async()=>'synthetic',async()=>({ok:false,status}));await assert.rejects(verifier.verify(packageName,'sku','token'),{code});
 }
});
function httpFixture(){
 const f=fixture();const authenticated={uid,firebase:{sign_in_provider:'google.com'}};
 let appId='synthetic-app',limit=true,calls=0;
 const handler=createPremiumHandler({authority:f.authority,verifyIdToken:async(token,revocation)=>{assert.equal(token,'synthetic-auth');assert.equal(revocation,true);calls++;return authenticated;},verifyAppCheck:async token=>{assert.equal(token,'synthetic-app-token');return {appId};},checkRate:async()=>limit,allowedAppIds:['synthetic-app']});
 async function send(route,body={},headers={Authorization:'Bearer synthetic-auth','X-Firebase-AppCheck':'synthetic-app-token'},method='POST'){
  const output={code:0,body:null,headers:{}};const response={set:(k,v)=>output.headers[k]=v,status:code=>{output.code=code;return response;},json:body=>{output.body=body;return output;}};
  await handler({method,path:'/premium/'+route,body,get:k=>headers[k]},response);return output;
 }
 return {...f,send,authenticated,app:id=>appId=id,limit:value=>limit=value,calls:()=>calls};
}
test('HTTPS handler derives purchase ownership from verified identity and protects responses from caching',async()=>{
 const f=httpFixture();const result=await f.send('fulfill',request);assert.equal(result.code,200);assert.equal(result.body.uid,uid);assert.equal(result.headers['Cache-Control'],'no-store');
});
test('HTTPS handler rejects client UID, grant injection, large bodies and unknown routes',async()=>{
 const f=httpFixture();assert.equal((await f.send('fulfill',{...request,uid:'other'})).code,400);
 assert.equal((await f.send('fulfill',{...request,granted:'99999'})).code,400);
 assert.equal((await f.send('fulfill',{...request,purchaseToken:'x'.repeat(17000)})).code,400);
 assert.equal((await f.send('refund',{})).code,404);assert.equal(f.calls(),0);
});
test('HTTPS handler requires both Google identity and an allowlisted App Check app',async()=>{
 const f=httpFixture();assert.equal((await f.send('wallet',{},{})).code,401);
 f.authenticated.firebase.sign_in_provider='anonymous';assert.equal((await f.send('wallet')).code,401);
 f.authenticated.firebase.sign_in_provider='google.com';f.app('other-app');assert.equal((await f.send('wallet')).code,401);
});
test('HTTPS handler enforces rate limit and does not expose verifier internals',async()=>{
 const f=httpFixture();f.limit(false);assert.equal((await f.send('wallet')).code,429);f.limit(true);
 f.play.verify=()=>{throw Error('private verification credentials must never appear in a response');};
 const result=await f.send('fulfill',request);assert.equal(result.code,503);assert.deepEqual(result.body,{error:'service-unavailable'});
});
