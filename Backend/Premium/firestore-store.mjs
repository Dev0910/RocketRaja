// Construct only with a trusted Admin Firestore instance. Browser/client Firebase SDKs are not supported.
export class FirestoreLedgerStore {
  constructor(db) { this.db = db; }
  transaction(action) {
    return this.db.runTransaction(async transaction => action({
      get: async path => {
        const snapshot = await transaction.get(this.db.doc(path));
        if (!snapshot.exists) return null;
        const value = snapshot.data();
        if (path.startsWith('redesignUser/')) value.heartbeatMs = value.heartbeatAt?.toMillis();
        return value;
      },
      set: (path, value) => transaction.set(this.db.doc(path), value)
    }));
  }
}
