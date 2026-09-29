using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Firestore;
using RocketRaja.Core;

namespace RocketRaja.Cloud
{
    /// <summary>All writes use transactions. No offline write queue or cached read can authorize play.</summary>
    public sealed class FirestoreSaveStore : ICloudSaveStore
    {
        readonly FirebaseFirestore db;
        public FirestoreSaveStore(FirebaseFirestore db) { this.db = db; }
        DocumentReference Doc(string uid)
        {
            if (string.IsNullOrEmpty(uid) || uid.Contains("/")) throw new ArgumentException("Invalid account ID.");
            return db.Collection("user").Document(uid);
        }
        static Dictionary<string, object> Payload(SavePayload p)
        {
            if (p == null || !p.Valid()) throw new InvalidDataException("Invalid save payload.");
            return new Dictionary<string, object> { ["schema"] = p.schema, ["json"] = p.json, ["checksum"] = p.checksum };
        }
        static double Seconds(Timestamp time) => (time.ToDateTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        static CloudRecord Decode(DocumentSnapshot s)
        {
            if (!s.Exists) return null;
            var p = s.GetValue<Dictionary<string, object>>("profile");
            var save = s.GetValue<Dictionary<string, object>>("save");
            var result = new CloudRecord
            {
                profile = new AccountProfile { uid = (string)p["uid"], displayName = (string)p["displayName"], email = (string)p["email"], photoUrl = (string)p["photoUrl"] },
                save = new SavePayload { schema = Convert.ToInt32(save["schema"]), json = (string)save["json"], checksum = (string)save["checksum"] },
                revision = s.GetValue<long>("revision"), generation = s.GetValue<long>("generation"),
                owner = s.GetValue<string>("owner"), handoff = s.GetValue<string>("handoff"), commitId = s.GetValue<string>("commitId"),
                heartbeatAt = Seconds(s.GetValue<Timestamp>("heartbeatAt")),
                updatedAt = Seconds(s.GetValue<Timestamp>("updatedAt")), createdAt = Seconds(s.GetValue<Timestamp>("createdAt"))
            };
            if (result.profile.uid != s.Id || !result.save.Valid() || result.revision < 1 || result.generation < 0)
                throw new InvalidDataException("Cloud save is damaged or uses an unsupported format.");
            return result;
        }
        public async Task<CloudRecord> ReadAsync(string uid)
        {
            // Source.Server gets can reuse an active listener's last server snapshot,
            // before that listener observes our completed transaction. Metadata checks
            // do not detect this. Read in a transaction to verify the current receipt
            // directly against the backend (also fails offline).
            var doc = Doc(uid);
            return await db.RunTransactionAsync(async tx => Decode(await tx.GetSnapshotAsync(doc)));
        }
        public async Task<CloudRecord> CreateAsync(AccountProfile profile, SavePayload save)
        {
            var doc = Doc(profile.uid);
            var values = new Dictionary<string, object>
            {
                ["profile"] = new Dictionary<string, object> { ["uid"] = profile.uid, ["displayName"] = profile.displayName ?? "", ["email"] = profile.email ?? "", ["photoUrl"] = profile.photoUrl ?? "" },
                ["save"] = Payload(save), ["revision"] = 1L, ["generation"] = 0L,
                ["owner"] = "", ["handoff"] = "", ["commitId"] = Guid.NewGuid().ToString("N"),
                ["createdAt"] = FieldValue.ServerTimestamp, ["updatedAt"] = FieldValue.ServerTimestamp, ["heartbeatAt"] = FieldValue.ServerTimestamp
            };
            await db.RunTransactionAsync(async tx =>
            {
                if ((await tx.GetSnapshotAsync(doc)).Exists) throw new CloudConflictException("An account save was created on another device. Reload it before choosing progress.");
                tx.Set(doc, values); return true;
            });
            return await ReadAsync(profile.uid);
        }
        async Task<CloudRecord> Change(string uid, Func<CloudRecord, Dictionary<string, object>> change)
        {
            var doc = Doc(uid);
            await db.RunTransactionAsync(async tx =>
            {
                var old = Decode(await tx.GetSnapshotAsync(doc));
                if (old == null) throw new CloudConflictException("The account save is missing.");
                var fields = change(old);
                if (fields != null) tx.Update(doc, fields);
                return true;
            });
            return await ReadAsync(uid);
        }
        public Task<CloudRecord> AcquireAsync(string uid, string owner) => Change(uid, old =>
        {
            // Expiry is enforced by request.time in rules, never by the device's wall clock.
            if (old.owner == owner && old.handoff == "")
            {
                return new Dictionary<string, object> { ["heartbeatAt"] = FieldValue.ServerTimestamp };
            }
            return new Dictionary<string, object> { ["owner"] = owner, ["generation"] = old.generation + 1,
                ["heartbeatAt"] = FieldValue.ServerTimestamp, ["handoff"] = "" };
        });
        static void RequireOwner(CloudRecord old, string owner, long generation)
        {
            if (!old.OwnedBy(owner, generation)) throw new CloudConflictException("Your game moved to another device.");
        }
        public Task<CloudRecord> HeartbeatAsync(string uid, string owner, long generation) => Change(uid, old =>
        {
            RequireOwner(old, owner, generation);
            if (old.handoff != "") throw new CloudConflictException("Another device requested this game.");
            return new Dictionary<string, object> { ["heartbeatAt"] = FieldValue.ServerTimestamp };
        });
        public Task<CloudRecord> CommitAsync(AccountCheckpoint c, bool release = false) => Change(c.uid, old =>
        {
            if (!c.Valid(c.uid)) throw new InvalidDataException("Invalid checkpoint.");
            // A timeout may hide a successful commit. Repeating the same receipt cannot pay twice.
            if (c.IsAcknowledged(old)) return null;
            RequireOwner(old, c.owner, c.generation);
            if (old.revision != c.baseRevision) throw new CloudConflictException("The cloud save changed. Reload before continuing.");
            var values = new Dictionary<string, object> { ["save"] = Payload(c.save), ["revision"] = old.revision + 1,
                ["commitId"] = c.commitId, ["updatedAt"] = FieldValue.ServerTimestamp, ["heartbeatAt"] = FieldValue.ServerTimestamp };
            if (release) values["owner"] = "";
            return values;
        });
        public async Task RequestHandoffAsync(string uid, string target)
        {
            await Change(uid, old =>
            {
                if (old.owner == target || old.owner == "") return null;
                if (old.handoff != "" && old.handoff != target) throw new CloudConflictException("A device switch is already in progress. Try again shortly.");
                return new Dictionary<string, object> { ["handoff"] = target };
            });
        }
        public async Task ReleaseAsync(string uid, string owner, long generation)
        {
            await Change(uid, old =>
            {
                if (old.owner == "" && old.generation == generation) return null;
                RequireOwner(old, owner, generation);
                return new Dictionary<string, object> { ["owner"] = "", ["heartbeatAt"] = FieldValue.ServerTimestamp };
            });
        }
        public IDisposable Listen(string uid, Action<CloudRecord> changed, Action<Exception> failed)
        {
            var registration = Doc(uid).Listen(MetadataChanges.Include, s =>
            {
                if (s.Metadata.IsFromCache || s.Metadata.HasPendingWrites) return;
                try { changed(Decode(s)); } catch (Exception ex) { failed(ex); }
            });
            return new Listener(registration, failed);
        }
        sealed class Listener : IDisposable
        {
            ListenerRegistration registration;
            public Listener(ListenerRegistration registration, Action<Exception> failed)
            {
                this.registration = registration;
                registration.ListenerTask.ContinueWith(t => { if (t.IsFaulted && this.registration != null) failed(t.Exception); });
            }
            public void Dispose() { var old = registration; registration = null; old?.Stop(); }
        }
        public void Dispose() { }
    }
}
