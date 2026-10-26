import { createHash, randomInt } from 'node:crypto';

export class PremiumError extends Error {
  constructor(code) { super(code); this.code = code; }
}
const fail = code => { throw new PremiumError(code); };
const natural = value => typeof value === 'string' && /^(0|[1-9][0-9]*)$/.test(value) ? BigInt(value) : fail('invalid-ledger');
const uidValid = uid => typeof uid === 'string' && /^[A-Za-z0-9_-]{1,128}$/.test(uid);
export const accountId = uid => createHash('sha256').update('rocket-raja:' + uid).digest('hex');
export const receiptKey = (packageName, token) => createHash('sha256').update('GooglePlay\0' + packageName + '\0' + token).digest('hex');
const walletKey = uid => 'premiumWallets/' + uid;
const newWallet = () => ({ schema: 1, revision: '0', grants: '0', spent: '0', revoked: '0', cards: Array(28).fill('0'), profileId:'', giftRegistered:false, importedGiftCardId:'', importedGiftOperationId:'' });
const operationId = value => typeof value === 'string' && /^[a-f0-9]{32}$/.test(value);
const cardIndex = id => /^C(0[1-9]|1[0-6])$/.test(id) ? Number(id.slice(1))-1 : /^R(0[1-9]|10)$/.test(id) ? 15+Number(id.slice(1)) : /^L0[12]$/.test(id) ? 25+Number(id.slice(1)) : -1;
function validateWallet(w) {
  if (!w || w.schema !== 1 || !Array.isArray(w.cards) || w.cards.length !== 28) fail('invalid-ledger');
  for (const field of ['revision', 'grants', 'spent', 'revoked']) natural(w[field]);
  w.cards.forEach(natural);
  w.profileId ??= ''; w.giftRegistered ??= false; w.importedGiftCardId ??= ''; w.importedGiftOperationId ??= '';
  if(w.profileId!==''&&!operationId(w.profileId) || typeof w.giftRegistered!=='boolean' ||
     (w.importedGiftCardId==='' ? w.importedGiftOperationId!=='' : !w.giftRegistered||cardIndex(w.importedGiftCardId)<0||!operationId(w.importedGiftOperationId))) fail('invalid-ledger');
  return w;
}
export function publicWallet(w, uid = '') {
  validateWallet(w);
  const net = natural(w.grants) - natural(w.spent) - natural(w.revoked);
  return { uid, profileId:w.profileId, revision: w.revision, balance: (net > 0n ? net : 0n).toString(), deficit: (net < 0n ? -net : 0n).toString(), cards: w.cards.slice(), giftRegistered:w.giftRegistered, importedGiftCardId:w.importedGiftCardId, importedGiftOperationId:w.importedGiftOperationId };
}
function actorUid(actor) { if (!uidValid(actor?.uid)) fail('unauthenticated'); return actor.uid; }
function chosenCard(random) {
  const tier = random(100);
  if (!Number.isInteger(tier) || tier < 0 || tier >= 100) fail('invalid-random');
  const count = tier < 75 ? 16 : tier < 97 ? 10 : 2, offset = tier < 75 ? 0 : tier < 97 ? 16 : 26;
  const card = random(count);
  if (!Number.isInteger(card) || card < 0 || card >= count) fail('invalid-random');
  return offset + card;
}

// Server-only business logic. An authenticated transport supplies actor.uid; never deserialize it from a request body.
// Store.transaction runs serializably and atomically commits all writes, with no partial side effects on failure.
export class PremiumAuthority {
  constructor({ store, play, packageName, packs, now = () => Date.now(), random = randomInt }) {
    if (!packageName || !Array.isArray(packs) || packs.length !== 4) fail('unconfigured');
    this.store = store; this.play = play; this.packageName = packageName; this.now = now; this.random = random;
    this.packs = new Map(packs.map(p => [p.id, BigInt(p.kajuKatli)]));
    if (this.packs.size !== 4 || [...this.packs.values()].some(v => v <= 0n)) fail('unconfigured');
  }
  async wallet(actor) {
    const uid = actorUid(actor);
    return this.store.transaction(async tx => publicWallet(await tx.get(walletKey(uid)) ?? newWallet(),uid));
  }
  readProgress(account) {
    let p;try {p=JSON.parse(account.save.json);}catch {fail('invalid-progress');}
    if(![2,3].includes(p?.schema)||p.economy!=='2.0.0'||!operationId(p.guestWallet?.profileId))fail('invalid-progress');
    natural(p.completedPrestiges?.digits);
    return p;
  }
  async activate(actor) {
    const uid=actorUid(actor);
    return this.store.transaction(async tx=>{
      const account=await tx.get('redesignUser/'+uid);this.requireOwnership(actor,account);
      const p=this.readProgress(account), profile=p.guestWallet.profileId;
      if(p.requiresDrawReconciliation)fail('progress-recovery-required');
      if(p.schema===3&&(p.premium?.uid!==uid||p.premium?.profileId!==profile))fail('purchase-owner-mismatch');
      const profileKey='premiumProfiles/'+profile;
      const binding=await tx.get(profileKey);
      const w=validateWallet(await tx.get(walletKey(uid))??newWallet());
      if(binding&&binding.uid!==uid||w.profileId!==''&&w.profileId!==profile)fail('profile-owner-mismatch');
      let changed=false;
      if(w.profileId===''){w.profileId=profile;changed=true;}
      if(!w.giftRegistered&&natural(p.completedPrestiges.digits)>=2n){
        if(p.secondPrestigeGift!==true)fail('invalid-progress');
        if(p.schema===2){
          const gift=p.guestWallet,card=gift.giftDrawCardId??'',op=gift.giftDrawOperationId??'';
          if(card===''){
            if(p.shopGiftSpent===true){
              if(op!==''||p.kajuKatli?.digits!=='0')fail('gift-provenance-required');
              w.spent=(natural(w.spent)+100n).toString();
            }else if(op!==''||p.kajuKatli?.digits!=='100')fail('gift-provenance-required');
          }else{
            const index=cardIndex(card);
            if(index<0||!operationId(op)||p.kajuKatli?.digits!=='0'||natural(p.cardLevels?.[index]?.digits)<1n)fail('gift-provenance-required');
            w.spent=(natural(w.spent)+100n).toString();w.cards[index]=(natural(w.cards[index])+1n).toString();
            w.importedGiftCardId=card;w.importedGiftOperationId=op;
          }
        }
        w.grants=(natural(w.grants)+100n).toString();w.giftRegistered=true;changed=true;
      }
      if(!binding)tx.set(profileKey,{uid,profileId:profile,createdAt:this.now()});
      if(changed){w.revision=(natural(w.revision)+1n).toString();tx.set(walletKey(uid),w);}
      return publicWallet(w,uid);
    });
  }
  async fulfill(actor, { productId, purchaseToken }) {
    const uid = actorUid(actor), grant = this.packs.get(productId);
    if (!grant || typeof purchaseToken !== 'string' || purchaseToken.length < 1 || purchaseToken.length > 8192) fail('invalid-purchase');
    const key = 'premiumReceipts/' + receiptKey(this.packageName, purchaseToken);
    const revokedKey = 'premiumRevocations/' + receiptKey(this.packageName, purchaseToken);
    const existing = await this.store.transaction(async tx => ({receipt:await tx.get(key),revoked:await tx.get(revokedKey)}));
    if (existing.receipt) return this.replayReceipt(uid, productId, existing.receipt);
    if (existing.revoked) fail('purchase-invalid');
    // Network verification happens outside a retried transaction. No reward is issued for pending payment.
    const verified = await this.play.verify(this.packageName, productId, purchaseToken);
    if (verified.purchaseState !== 0) fail(verified.purchaseState === 2 ? 'payment-pending' : 'purchase-invalid');
    if (verified.productId !== productId || verified.obfuscatedExternalAccountId !== accountId(uid) || (verified.quantity ?? 1) !== 1 || verified.consumptionState !== 0) fail('purchase-mismatch');
    const receipt = await this.store.transaction(async tx => {
      const duplicate = await tx.get(key);
      if (duplicate) { this.replayReceipt(uid, productId, duplicate); return duplicate; }
      if (await tx.get(revokedKey)) fail('purchase-invalid');
      const w = validateWallet(await tx.get(walletKey(uid)) ?? newWallet());
      w.grants = (natural(w.grants) + grant).toString(); w.revision = (natural(w.revision) + 1n).toString();
      const record = { schema: 1, uid, productId, granted: grant.toString(), revision: w.revision, verifiedAt: this.now(), revoked: false };
      tx.set(key, record); tx.set(walletKey(uid), w); return record;
    });
    return this.replayReceipt(uid, productId, receipt);
  }
  replayReceipt(uid, productId, receipt) {
    if (receipt.uid !== uid || receipt.productId !== productId) fail('purchase-owner-mismatch');
    return { uid, productId, fulfilled: true, revoked: receipt.revoked, revision: receipt.revision, granted: receipt.granted };
  }
  async draw(actor, { operationId, expectedRevision }) {
    const uid = actorUid(actor);
    if (!/^[a-f0-9]{32}$/.test(operationId ?? '')) fail('invalid-operation');
    natural(expectedRevision);
    const key = `premiumDraws/${uid}/operations/${operationId}`;
    let candidate; // Stable across Firestore's internal transaction retry.
    return this.store.transaction(async tx => {
      const prior = await tx.get(key);
      if (prior) return prior;
      const account = await tx.get('redesignUser/' + uid);
      this.requireOwnership(actor, account);
      const progress=this.readProgress(account);
      if(progress.requiresDrawReconciliation)fail('progress-recovery-required');
      if(progress.schema===3&&(progress.premium?.uid!==uid||progress.premium?.profileId!==progress.guestWallet.profileId))fail('purchase-owner-mismatch');
      if (natural(progress.completedPrestiges?.digits) < 1n) fail('talent-locked');
      const w = validateWallet(await tx.get(walletKey(uid)) ?? newWallet());
      if(w.profileId!==progress.guestWallet.profileId)fail('wallet-not-active');
      if (w.revision !== expectedRevision) fail('stale-wallet');
      if (natural(publicWallet(w).balance) < 100n) fail('insufficient-currency');
      if(progress.schema!==3||progress.premium.pendingOperationId!==operationId||progress.premium.pendingExpectedRevision!==expectedRevision)fail('operation-not-confirmed');
      if (candidate === undefined) candidate = chosenCard(this.random);
      const before = w.cards[candidate]; w.cards[candidate] = (natural(before) + 1n).toString();
      w.spent = (natural(w.spent) + 100n).toString(); w.revision = (natural(w.revision) + 1n).toString();
      const cardId = candidate < 16 ? 'C' + String(candidate + 1).padStart(2,'0') : candidate < 26 ? 'R' + String(candidate - 15).padStart(2,'0') : 'L' + String(candidate - 25).padStart(2,'0');
      const result = { operationId, cardId, before, after: w.cards[candidate], revision: w.revision, price: '100', wallet: publicWallet(w,uid) };
      tx.set(key, result); tx.set(walletKey(uid), w); return result;
    });
  }
  requireOwnership(actor, account) {
    if (!account || !actor.owner || !Number.isSafeInteger(actor.generation) || account.owner !== actor.owner || account.generation !== actor.generation || account.handoff !== '' ||
      !Number.isFinite(account.heartbeatMs) || account.heartbeatMs > this.now() + 5000 || this.now() - account.heartbeatMs >= 30000) fail('ownership-lost');
  }
  // Called only by a separately authenticated, verified store-notification worker. Not a client endpoint.
  async revokeVerified(packageName, purchaseToken, eventId) {
    if (packageName !== this.packageName || typeof eventId !== 'string' || eventId.length === 0 || typeof purchaseToken !== 'string' || purchaseToken.length < 1 || purchaseToken.length > 8192) fail('invalid-refund');
    const key = 'premiumReceipts/' + receiptKey(packageName, purchaseToken);
    return this.store.transaction(async tx => {
      const receipt = await tx.get(key);
      // A verified refund can race the initial grant. Retain its token digest even
      // without a receipt, so a stale PURCHASED verification cannot grant afterwards.
      if (!receipt) {
        const revokedKey='premiumRevocations/'+receiptKey(packageName,purchaseToken);
        if(!await tx.get(revokedKey))tx.set(revokedKey,{schema:1,eventId,verifiedAt:this.now()});
        return {revoked:true,unfulfilled:true};
      }
      if (receipt.revoked) return { revoked: true };
      const w = validateWallet(await tx.get(walletKey(receipt.uid)));
      w.revoked = (natural(w.revoked) + natural(receipt.granted)).toString(); w.revision = (natural(w.revision) + 1n).toString();
      receipt.revoked = true; receipt.refundEventId = eventId;
      tx.set(key, receipt); tx.set(walletKey(receipt.uid), w);
      return { revoked: true };
    });
  }
}
