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
    public sealed class UnsupportedCheckpointException : IOException
    { public UnsupportedCheckpointException() : base("This account checkpoint requires a newer app. All files were preserved.") { } }

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
        readonly string expectedUid;
        bool beganFreshWrite;
        readonly Func<AccountCheckpoint, string> encode;
        readonly Func<string, AccountCheckpoint> decode;
        readonly Func<string, SaveVersionHeader> gameplayVersion;
        public static CheckpointStore ForRedesign(string root, string uid, Func<AccountCheckpoint, string> encode, Func<string, AccountCheckpoint> decode, Func<string, SaveVersionHeader> gameplayVersion) =>
            new CheckpointStore(Path.Combine(root, "economy-2"), uid, encode, decode, gameplayVersion ?? throw new ArgumentNullException(nameof(gameplayVersion)));
        public CheckpointStore(string root, string uid, Func<AccountCheckpoint, string> encode, Func<string, AccountCheckpoint> decode, Func<string, SaveVersionHeader> gameplayVersion = null)
        {
            file = Path.Combine(root, "accounts", SavePayload.Digest(uid), "checkpoint.save");
            expectedUid = uid;
            this.encode = encode; this.decode = decode;
            this.gameplayVersion = gameplayVersion;
        }
        public AccountCheckpoint Load(string uid)
        {
            if (uid != expectedUid) return null;
            var main = Read(file, uid);
            var backup = Read(file + ".bak", uid);
            Read(file + ".tmp", uid); // Version guard only: an uncommitted write is never replayed.
            if (main != null) return main;
            if (backup != null) return backup;
            if (File.Exists(file) || File.Exists(file + ".bak") || File.Exists(file + ".tmp"))
                throw new InvalidDataException("Account checkpoint recovery is required. Existing files were preserved.");
            return null;
        }
        AccountCheckpoint Read(string path, string uid)
        {
            if (!File.Exists(path)) return null;
            try
            {
                    string raw = File.ReadAllText(path); int n = raw.IndexOf('\n');
                    if (n < 1 || raw.Substring(0, n) != SavePayload.Digest(raw.Substring(n + 1))) return null;
                    var value = decode(raw.Substring(n + 1));
                    if (value != null && (value.schema > 1 || value.save?.schema > 1 || value.followup?.schema > 1))
                        throw new UnsupportedCheckpointException();
                    CheckGameplayVersion(value?.save); CheckGameplayVersion(value?.followup);
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
                        if (value.followup != null && !value.followup.Valid()) return null;
                        return value;
                    }
            }
            catch (UnsupportedCheckpointException) { throw; }
            catch (Exception error) when (error is InvalidDataException || error is ArgumentException || error is FormatException || error is InvalidOperationException || error.GetType().Name == "JsonException") { }
            // I/O/access failures propagate: a locked or unreadable file is not a missing checkpoint.
            return null;
        }
        public void Save(AccountCheckpoint value)
        {
            if (value == null || !value.Valid(expectedUid) || value.followup != null && !EmptyFollowup(value.followup) && !value.followup.Valid()) throw new InvalidDataException("Invalid account checkpoint.");
            CheckGameplayVersion(value.save); CheckGameplayVersion(value.followup);
            bool empty = !File.Exists(file) && !File.Exists(file + ".bak") && !File.Exists(file + ".tmp");
            if (beganFreshWrite && !File.Exists(file) && !File.Exists(file + ".bak")) Read(file + ".tmp", expectedUid);
            else Load(expectedUid); // Refuse incompatible or unrecoverable existing data before touching any bytes.
            var previous = Read(file, expectedUid);
            string json = encode(value);
            var roundtrip = decode(json);
            if (roundtrip == null || !roundtrip.Valid(expectedUid) || roundtrip.followup != null && !EmptyFollowup(roundtrip.followup) && !roundtrip.followup.Valid()) throw new InvalidDataException("Invalid checkpoint codec output.");
            if (empty) beganFreshWrite = true;
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            string raw = SavePayload.Digest(json) + "\n" + json;
            RedesignSaveStore.PreserveInterrupted(file);
            using (var stream = new FileStream(file + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            { var bytes = Encoding.UTF8.GetBytes(raw); stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(file))
            {
                // Never rotate an invalid main file over a valid backup.
                if (previous != null) File.Copy(file, file + ".bak", true);
                else File.Copy(file, file + ".damaged-" + Guid.NewGuid().ToString("N"), false);
                File.Replace(file + ".tmp", file, null);
            }
            else File.Move(file + ".tmp", file);
            beganFreshWrite = false;
        }
        static bool EmptyFollowup(SavePayload value) => (value.schema == 0 || value.schema == 1) && string.IsNullOrEmpty(value.json) && string.IsNullOrEmpty(value.checksum);
        void CheckGameplayVersion(SavePayload payload)
        {
            if (gameplayVersion == null || payload == null || !payload.Valid()) return;
            var header = gameplayVersion(payload.json);
            if (header == null || header.schema < 1 || string.IsNullOrEmpty(header.economy) || header.schema >= 2 && string.IsNullOrEmpty(header.contentVersion)) throw new InvalidDataException("Missing checkpoint gameplay version.");
            if (header.schema < 2 || header.schema > 3 || header.economy != RedesignCatalog.EconomyVersion || header.contentVersion != RedesignCatalog.ContentVersion) throw new UnsupportedCheckpointException();
        }
        public void Archive(string reason)
        {
            if (File.Exists(file)) File.Copy(file, file + "." + reason + "." + Guid.NewGuid().ToString("N"), false);
        }
    }
}
