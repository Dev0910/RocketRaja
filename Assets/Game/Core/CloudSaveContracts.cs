using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RocketRaja.Core
{
    [Serializable] public sealed class AccountProfile
    {
        public string uid, displayName, email, photoUrl;
    }

    [Serializable] public sealed class SavePayload
    {
        public int schema = 1;
        public string json, checksum;
        public static SavePayload Create(string json)
        {
            if (string.IsNullOrEmpty(json) || Encoding.UTF8.GetByteCount(json) > 750000)
                throw new InvalidDataException("Save snapshot is empty or too large.");
            return new SavePayload { json = json, checksum = Digest(json) };
        }
        public bool Valid() => schema == 1 && !string.IsNullOrEmpty(json) &&
            Encoding.UTF8.GetByteCount(json) <= 750000 && checksum == Digest(json);
        public static string Digest(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "");
        }
    }

    [Serializable] public sealed class CloudRecord
    {
        public AccountProfile profile;
        public SavePayload save;
        public long revision, generation;
        public string owner = "", handoff = "", commitId = "";
        public double heartbeatAt, updatedAt, createdAt;
        public bool OwnedBy(string session, long epoch) => owner == session && generation == epoch;
    }

    [Serializable] public sealed class AccountCheckpoint
    {
        public int schema = 1;
        public string uid, owner, commitId;
        public long baseRevision, generation;
        public SavePayload save;
        public SavePayload followup;
        public bool pending;
        public bool Valid(string expectedUid) => schema == 1 && uid == expectedUid &&
            !string.IsNullOrEmpty(owner) && generation > 0 && baseRevision > 0 &&
            !string.IsNullOrEmpty(commitId) && save != null && save.Valid();
        public bool CanRetry(CloudRecord remote) => pending && remote != null &&
            remote.profile.uid == uid && remote.OwnedBy(owner, generation) && remote.revision == baseRevision;
        public bool IsAcknowledged(CloudRecord remote) => remote != null && remote.profile.uid == uid &&
            remote.commitId == commitId && remote.revision == baseRevision + 1 && remote.save.checksum == save.checksum;
    }

    public sealed class CloudConflictException : Exception
    { public CloudConflictException(string message) : base(message) { } }

    public interface IGoogleSignInProvider
    {
        bool Supported { get; }
        Task<string> SignInAsync();
        Task SignOutAsync();
    }
    public interface IAccountAuth : IDisposable
    {
        AccountProfile Current { get; }
        event Action Changed;
        Task InitializeAsync();
        Task<AccountProfile> SignInAsync();
        Task SignOutAsync();
    }
    public interface ICloudSaveStore : IDisposable
    {
        Task<CloudRecord> ReadAsync(string uid);
        Task<CloudRecord> CreateAsync(AccountProfile profile, SavePayload save);
        Task<CloudRecord> AcquireAsync(string uid, string owner);
        Task<CloudRecord> HeartbeatAsync(string uid, string owner, long generation);
        Task<CloudRecord> CommitAsync(AccountCheckpoint checkpoint, bool release = false);
        Task RequestHandoffAsync(string uid, string target);
        Task ReleaseAsync(string uid, string owner, long generation);
        IDisposable Listen(string uid, Action<CloudRecord> changed, Action<Exception> failed);
    }
    public interface IPlayerSession
    {
        bool IsAccount { get; }
        bool CanPlay { get; }
        string Status { get; }
        GameState State { get; }
        void Save(bool immediate = false);
    }

    /// <summary>Atomic, checksummed envelope; a checkpoint and its base revision cannot tear apart.</summary>
    public sealed class CheckpointStore
    {
        readonly string file;
        readonly Func<AccountCheckpoint, string> encode;
        readonly Func<string, AccountCheckpoint> decode;
        public CheckpointStore(string root, string uid, Func<AccountCheckpoint, string> encode, Func<string, AccountCheckpoint> decode)
        {
            file = Path.Combine(root, "accounts", SavePayload.Digest(uid), "checkpoint.save");
            this.encode = encode; this.decode = decode;
        }
        public AccountCheckpoint Load(string uid)
        {
            foreach (var path in new[] { file, file + ".bak" })
            {
                try
                {
                    string raw = File.ReadAllText(path); int n = raw.IndexOf('\n');
                    if (n < 1 || raw.Substring(0, n) != SavePayload.Digest(raw.Substring(n + 1))) continue;
                    var value = decode(raw.Substring(n + 1));
                    if (value != null && value.Valid(uid))
                    {
                        // Unity serializes null inline classes as empty objects. A schema-1
                        // checkpoint can therefore reload an absent follow-up as {json:"",
                        // checksum:""}. It is not a second upload. Normalize only that exact
                        // empty representation; never discard a populated/damaged payload.
                        var followup = value.followup;
                        if (followup != null && (followup.schema == 0 || followup.schema == 1) &&
                            string.IsNullOrEmpty(followup.json) && string.IsNullOrEmpty(followup.checksum))
                            value.followup = null;
                        return value;
                    }
                }
                catch (Exception) { /* Preserve unreadable evidence; try the last validated backup. */ }
            }
            return null;
        }
        public void Save(AccountCheckpoint value)
        {
            if (!value.Valid(value.uid)) throw new InvalidDataException("Invalid account checkpoint.");
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            string json = encode(value), raw = SavePayload.Digest(json) + "\n" + json;
            using (var stream = new FileStream(file + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            { var bytes = Encoding.UTF8.GetBytes(raw); stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(file))
            {
                // Never rotate an invalid main file over a valid backup.
                string existing = File.ReadAllText(file); int n = existing.IndexOf('\n');
                bool valid = n > 0 && existing.Substring(0, n) == SavePayload.Digest(existing.Substring(n + 1));
                if (valid) File.Copy(file, file + ".bak", true);
                else File.Copy(file, file + ".damaged", true);
                File.Replace(file + ".tmp", file, null);
            }
            else File.Move(file + ".tmp", file);
        }
        public void Archive(string reason)
        {
            if (File.Exists(file)) File.Copy(file, file + "." + reason + "." + Guid.NewGuid().ToString("N"), false);
        }
    }
}
