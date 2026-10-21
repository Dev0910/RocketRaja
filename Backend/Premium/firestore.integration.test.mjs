import { test, before, beforeEach, after } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { initializeApp, deleteApp } from 'firebase-admin/app';
import { getFirestore, Timestamp } from 'firebase-admin/firestore';
import { PremiumAuthority, accountId } from './authority.mjs';
import { FirestoreLedgerStore } from './firestore-store.mjs';
import { PremiumRateLimiter } from './rate-limit.mjs';

let app,db,authority;
const uid='premium-emulator-fixture',actor={uid,owner:'a'.repeat(32),generation:2};
const packs=JSON.parse(fs.readFileSync(new URL('../../Documentation/RedesignContentCatalog.json',import.meta.url),'utf8')).packs;
const packageName='com.synthetic.rocket.fixture';
const profileId='d'.repeat(32);
const receipt={productId:packs[0].id,purchaseToken:'synthetic-emulator-token'};
const play={verify:async (_package,productId)=>({productId,purchaseState:0,consumptionState:0,quantity:1,obfuscatedExternalAccountId:accountId(uid)})};
before(()=>{
 if(!/^(127\.0\.0\.1|localhost):\d+$/.test(process.env.FIRESTORE_EMULATOR_HOST??'')) throw Error('This destructive fixture requires a local Firestore emulator. Never run against a live project.');
 if(process.env.GCLOUD_PROJECT && process.env.GCLOUD_PROJECT!=='demo-rocket-raja') throw Error('Only the demo project is permitted');
 app=initializeApp({projectId:'demo-rocket-raja'},'premium-authority-emulator');db=getFirestore(app);
 authority=new PremiumAuthority({store:new FirestoreLedgerStore(db),play,packageName,packs,now:()=>100000,random:()=>0});
});
beforeEach(async()=>{
 for(const collection of ['premiumWallets','premiumReceipts','premiumDraws','premiumProfiles','premiumRateLimits','premiumRevocations'])await db.recursiveDelete(db.collection(collection));
 await db.doc('redesignUser/'+uid).set({owner:actor.owner,generation:2,handoff:'',heartbeatAt:Timestamp.fromMillis(99900),save:{json:JSON.stringify({schema:3,economy:'2.0.0',completedPrestiges:{digits:'1'},guestWallet:{profileId,giftDrawCardId:'',giftDrawOperationId:''},premium:{uid,profileId,pendingOperationId:'1'.repeat(32),pendingExpectedRevision:'1'}})}});
 await db.doc('premiumWallets/'+uid).set({schema:1,revision:'0',grants:'0',spent:'0',revoked:'0',cards:Array(28).fill('0'),profileId,giftRegistered:false,importedGiftCardId:'',importedGiftOperationId:''});
 await db.doc('premiumProfiles/'+profileId).set({uid,profileId});
});
after(async()=>{await db?.terminate();if(app)await deleteApp(app);});
test('Admin transaction deduplicates concurrent receipt verification and survives a new authority instance',async()=>{
 const results=await Promise.all(Array.from({length:8},()=>authority.fulfill(actor,receipt)));
 assert.ok(results.every(x=>x.revision==='1'));assert.equal((await authority.wallet(actor)).balance,'1000');
 const reopened=new PremiumAuthority({store:new FirestoreLedgerStore(db),play,packageName,packs,now:()=>100000});
 assert.equal((await reopened.fulfill(actor,receipt)).revision,'1');assert.equal((await reopened.wallet(actor)).balance,'1000');
});
test('Admin transactions atomically fence concurrent wallet spends and replay the committed draw',async()=>{
 await authority.fulfill(actor,receipt);
 const requests=['1','2','3','4'].map(id=>({operationId:id.repeat(32),expectedRevision:'1'}));
 const results=await Promise.allSettled(requests.map(r=>authority.draw(actor,r)));
 assert.equal(results.filter(x=>x.status==='fulfilled').length,1);
 const index=results.findIndex(x=>x.status==='fulfilled');const previous=results[index].value;
 const repeat=await authority.draw(actor,requests[index]);assert.deepEqual(repeat,previous);
 const wallet=await authority.wallet(actor);assert.equal(wallet.balance,'900');assert.equal(wallet.cards[0],'1');
});
test('Admin timestamp conversion preserves account handoff fencing',async()=>{
 await authority.fulfill(actor,receipt);await db.doc('redesignUser/'+uid).update({handoff:'b'.repeat(32)});
 await assert.rejects(authority.draw(actor,{operationId:'1'.repeat(32),expectedRevision:'1'}),{code:'ownership-lost'});
 assert.equal((await authority.wallet(actor)).balance,'1000');
});
test('Admin transactions retain refund deficits exactly and never revoke twice',async()=>{
 await authority.fulfill(actor,receipt);await authority.draw(actor,{operationId:'1'.repeat(32),expectedRevision:'1'});
 await Promise.all([authority.revokeVerified(packageName,receipt.purchaseToken,'verified-event-a'),authority.revokeVerified(packageName,receipt.purchaseToken,'verified-event-b')]);
 const wallet=await authority.wallet(actor);assert.equal(wallet.balance,'0');assert.equal(wallet.deficit,'100');assert.equal(wallet.cards[0],'1');assert.equal(wallet.revision,'3');
});
test('Admin activation imports one spent guest gift and persists its account binding atomically',async()=>{
 await db.doc('premiumProfiles/'+profileId).delete();await db.doc('premiumWallets/'+uid).delete();
 const p={schema:2,economy:'2.0.0',completedPrestiges:{digits:'2'},secondPrestigeGift:true,kajuKatli:{digits:'0'},
  cardLevels:Array.from({length:28},(_,i)=>({digits:i===0?'1':'0'})),guestWallet:{profileId,giftDrawCardId:'C01',giftDrawOperationId:'1'.repeat(32)}};
 await db.doc('redesignUser/'+uid).update({save:{json:JSON.stringify(p)}});
 const results=await Promise.all(Array.from({length:4},()=>authority.activate(actor)));
 assert.ok(results.every(w=>w.balance==='0'&&w.cards[0]==='1'&&w.revision==='1'));
 assert.equal((await db.doc('premiumProfiles/'+profileId).get()).data().uid,uid);
});
test('Admin rejects a new spend until its operation identity is cloud-confirmed',async()=>{
 await authority.fulfill(actor,receipt);
 await assert.rejects(authority.draw(actor,{operationId:'f'.repeat(32),expectedRevision:'1'}),{code:'operation-not-confirmed'});
 assert.equal((await authority.wallet(actor)).balance,'1000');
});
test('Admin rate limit transactions coordinate concurrent function instances',async()=>{
 const rates=new PremiumRateLimiter(new FirestoreLedgerStore(db),()=>120000,{draw:3});
 const results=await Promise.all(Array.from({length:8},()=>rates.allow(uid,'draw')));
 assert.equal(results.filter(Boolean).length,3);
 assert.equal(await new PremiumRateLimiter(new FirestoreLedgerStore(db),()=>120000,{draw:3}).allow(uid,'draw'),false);
});
test('Admin pre-delivery revocation prevents a late receipt from creating a grant',async()=>{
 await authority.revokeVerified(packageName,receipt.purchaseToken,'verified-before-delivery');
 await assert.rejects(authority.fulfill(actor,receipt),{code:'purchase-invalid'});
 assert.equal((await authority.wallet(actor)).balance,'0');
});
