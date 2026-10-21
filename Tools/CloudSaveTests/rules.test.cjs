const {test, before, after, beforeEach} = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const {initializeTestEnvironment, assertSucceeds, assertFails} = require('@firebase/rules-unit-testing');
const {doc, setDoc, getDoc, updateDoc, deleteDoc, getDocs, collection, runTransaction, serverTimestamp, Timestamp, setLogLevel} = require('firebase/firestore');
setLogLevel('silent');
let env;
const A = 'a'.repeat(32), B = 'b'.repeat(32), C = 'c'.repeat(32);
const uid = 'test-player';
const payload = {schema:1, json:'{"test":true}', checksum:'A'.repeat(64)};
const context = id => env.authenticatedContext(id, {firebase:{sign_in_provider:'google.com'}}).firestore();
const playerDoc = (db, id = uid) => doc(db,'user',id);
function initial() { return {profile:{uid,displayName:'Test Player',email:'',photoUrl:''},save:payload,revision:1,generation:0,owner:'',handoff:'',commitId:C,createdAt:serverTimestamp(),updatedAt:serverTimestamp(),heartbeatAt:serverTimestamp()}; }
async function seed(fields = {}) { await env.withSecurityRulesDisabled(async c => { await setDoc(playerDoc(c.firestore()), {...initial(),...fields}); }); }
async function acquire(db, owner) {
  const ref = playerDoc(db);
  return runTransaction(db, async tx => { const old=(await tx.get(ref)).data(); tx.update(ref,{owner,generation:old.generation+1,handoff:'',heartbeatAt:serverTimestamp()}); });
}
async function save(db, extra = {}) { return updateDoc(playerDoc(db), {save:payload, revision:2, commitId:B, updatedAt:serverTimestamp(), heartbeatAt:serverTimestamp(), ...extra}); }
before(async () => {
  if (!process.env.FIRESTORE_EMULATOR_HOST) throw new Error('Run with firebase emulators:exec. Never use a live Firebase project for these tests.');
  const [host, port] = process.env.FIRESTORE_EMULATOR_HOST.split(':');
  env = await initializeTestEnvironment({projectId:'demo-rocket-raja',firestore:{host,port:Number(port),rules:fs.readFileSync(path.resolve(__dirname,'../../firestore.rules'),'utf8')}});
});
beforeEach(async () => { await env.clearFirestore(); });
after(async () => { await env?.cleanup(); });

test('redesign snapshots use a separate namespace while legacy progress remains untouched', async () => {
  const db = context(uid);
  await assertSucceeds(setDoc(playerDoc(db), initial()));
  const redesigned = doc(db, 'redesignUser', uid);
  const newPayload = {...payload, json:'{"schema":2,"economy":"2.0.0","contentVersion":"cash-only-v1"}'};
  await assertSucceeds(setDoc(redesigned, {...initial(), save:newPayload}));
  assert.equal((await getDoc(playerDoc(db))).data().save.json, payload.json);
  assert.equal((await getDoc(redesigned)).data().save.json, newPayload.json);
  await assertFails(getDoc(doc(context('other-user'), 'redesignUser', uid)));
  await assertFails(getDocs(collection(db, 'redesignUser')));
  await assertFails(deleteDoc(redesigned));
});

test('client save permissions never grant access to a premium receipt or wallet ledger', async () => {
  const db = context(uid);
  for (const name of ['premiumLedger', 'purchaseReceipts', 'wallets', 'premiumWallets', 'premiumReceipts', 'premiumDraws', 'premiumProfiles', 'premiumRateLimits', 'premiumRevocations', 'unexpected']) {
    await assertFails(setDoc(doc(db, name, uid), initial()));
    await assertFails(getDoc(doc(db, name, uid)));
  }
});

test('Google player can create and read exactly their own user document', async () => {
  const db=context(uid); await assertSucceeds(setDoc(playerDoc(db),initial()));
  const saved=await assertSucceeds(getDoc(playerDoc(db))); assert.equal(saved.data().profile.uid,uid);
});
test('guests, anonymous accounts and another UID cannot read or create player saves', async () => {
  await seed();
  const guest=env.unauthenticatedContext().firestore();
  const anonymous=env.authenticatedContext(uid,{firebase:{sign_in_provider:'anonymous'}}).firestore();
  await assertFails(getDoc(playerDoc(guest))); await assertFails(getDoc(playerDoc(anonymous)));
  await assertFails(getDoc(playerDoc(context('other-user'))));
  await env.clearFirestore();
  await assertFails(setDoc(playerDoc(guest),initial())); await assertFails(setDoc(playerDoc(anonymous),initial()));
});
test('collection enumeration and deletion are denied', async () => {
  await seed(); const db=context(uid);
  await assertFails(getDocs(collection(db,'user'))); await assertFails(deleteDoc(playerDoc(db)));
});
test('extra fields, wrong UID and unsupported payload schema are denied', async () => {
  const db=context(uid);
  await assertFails(setDoc(playerDoc(db),{...initial(),accessToken:'must-not-be-stored'}));
  await assertFails(setDoc(playerDoc(db),{...initial(),profile:{...initial().profile,uid:'other'}}));
  await assertFails(setDoc(playerDoc(db),{...initial(),save:{...payload,schema:2}}));
});
test('new saves require server timestamps, an unowned session and initial revision', async () => {
  const db=context(uid);
  await assertFails(setDoc(playerDoc(db),{...initial(),revision:8}));
  await assertFails(setDoc(playerDoc(db),{...initial(),owner:A,generation:1}));
  await assertFails(setDoc(playerDoc(db),{...initial(),createdAt:Timestamp.fromMillis(1000)}));
});
test('simultaneous acquisitions produce only one active lease', async () => {
  await seed(); const dbA=context(uid), dbB=context(uid);
  const results=await Promise.allSettled([acquire(dbA,A),acquire(dbB,B)]);
  assert.equal(results.filter(x=>x.status==='fulfilled').length,1);
  const saved=(await getDoc(playerDoc(dbA))).data(); assert.equal(saved.generation,1); assert.ok([A,B].includes(saved.owner));
});
test('valid saves increment one revision; rollback, jumps and same receipt are denied', async () => {
  await seed({owner:A,generation:1}); const db=context(uid);
  await assertFails(save(db,{revision:1})); await assertFails(save(db,{revision:3})); await assertFails(save(db,{commitId:C}));
  await assertSucceeds(save(db));
});
test('server-time lease expiry prevents late commits and permits takeover', async () => {
  await seed({owner:A,generation:1,heartbeatAt:Timestamp.fromMillis(Date.now()-40000)}); const db=context(uid);
  await assertFails(save(db)); await assertSucceeds(acquire(db,B));
  const saved=(await getDoc(playerDoc(db))).data(); assert.equal(saved.owner,B); assert.equal(saved.generation,2);
});
test('stale ownership generation cannot be replayed after takeover', async () => {
  await seed({owner:B,generation:2}); const db=context(uid);
  await assertFails(save(db,{owner:A,generation:1}));
  await assertFails(updateDoc(playerDoc(db),{owner:A,generation:1,heartbeatAt:serverTimestamp(),handoff:''}));
});
test('handoff preserves data, allows a final save and releases before the next owner enters', async () => {
  await seed({owner:A,generation:1}); const db=context(uid);
  await assertSucceeds(updateDoc(playerDoc(db),{handoff:B}));
  await assertFails(updateDoc(playerDoc(db),{heartbeatAt:serverTimestamp()}));
  await assertSucceeds(save(db));
  await assertSucceeds(updateDoc(playerDoc(db),{owner:'',heartbeatAt:serverTimestamp()}));
  await assertSucceeds(acquire(db,B));
  const saved=(await getDoc(playerDoc(db))).data(); assert.equal(saved.revision,2); assert.equal(saved.owner,B); assert.equal(saved.handoff,'');
});
test('lease changes cannot smuggle progress or identity edits', async () => {
  await seed(); const db=context(uid);
  await assertFails(updateDoc(playerDoc(db),{owner:A,generation:1,heartbeatAt:serverTimestamp(),save:{...payload,json:'changed'}}));
  await assertFails(updateDoc(playerDoc(db),{profile:{...initial().profile,email:'changed@example.test'}}));
});
test('a released session cannot write until ownership is acquired again', async () => {
  await seed({owner:A,generation:1}); const db=context(uid);
  await assertSucceeds(save(db,{owner:''}));
  await assertFails(save(db,{revision:3,commitId:A}));
});
