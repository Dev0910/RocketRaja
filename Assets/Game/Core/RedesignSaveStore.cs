using System;
using System.IO;
using System.Text;

namespace RocketRaja.Core
{
    public enum RedesignLoadStatus { Missing, Loaded, RecoveredBackup, LegacyTransitionRequired, UnsupportedVersion, Corrupt }
    [Serializable] public sealed class SaveVersionHeader { public int schema; public string economy, contentVersion; }
    public sealed class RedesignLoadResult
    {
        public RedesignLoadStatus status;
        public RedesignState state;
        public string notice;
        public bool CanCreateFresh => status == RedesignLoadStatus.Missing;
    }
    public sealed class RedesignSaveStore
    {
        readonly string file;
        readonly RedesignCatalog catalog;
        readonly Func<RedesignState, string> encode;
        readonly Func<string, RedesignState> decode;
        readonly Func<string, SaveVersionHeader> readVersion;
        bool beganFreshWrite;
        public RedesignSaveStore(string path, RedesignCatalog catalog, Func<RedesignState, string> encode, Func<string, RedesignState> decode, Func<string, SaveVersionHeader> readVersion)
        { file = path; this.catalog = catalog; this.encode = encode; this.decode = decode; this.readVersion = readVersion; }
        RedesignLoadResult Read(string path)
        {
            if (!File.Exists(path)) return new RedesignLoadResult { status = RedesignLoadStatus.Missing };
            try
            {
                string raw = File.ReadAllText(path); int n = raw.IndexOf('\n');
                if (n <= 0 || raw.Substring(0, n).Trim() != SavePayload.Digest(raw.Substring(n + 1))) throw new InvalidDataException("Checksum mismatch");
                string json = raw.Substring(n + 1);
                var header = readVersion(json);
                if (header == null) throw new InvalidDataException("Missing version");
                if (header.schema >= 1 && header.schema <= 3)
                    if (string.IsNullOrEmpty(header.economy) || header.schema >= 2 && string.IsNullOrEmpty(header.contentVersion)) throw new InvalidDataException("Missing version fields");
                // This release intentionally starts known older local economies afresh.
                // Read remains side-effect free; the first durable v3 write makes this one-time.
                // Authoritative wallets and unknown/future versions still require recovery.
                if (header.schema == 1 && header.economy == "1.0.0" || header.schema == 2 && header.economy == "2.0.0" && header.contentVersion == "cash-only-v1")
                    return new RedesignLoadResult { status = RedesignLoadStatus.Missing, notice = "Progress reset for the faster-start economy." };
                if (header.schema > 3 || header.schema == 1 || header.schema >= 2 && (header.economy != RedesignCatalog.EconomyVersion || header.contentVersion != RedesignCatalog.ContentVersion)) return new RedesignLoadResult { status = RedesignLoadStatus.UnsupportedVersion, notice = "This save requires a different game version. No files were changed." };
                var s = decode(json);
                if (s == null) throw new InvalidDataException("Missing state");
                if (!s.Valid(catalog)) throw new InvalidDataException("Invalid state");
                return new RedesignLoadResult { status = RedesignLoadStatus.Loaded, state = s };
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is ArgumentException || e is FormatException || e is InvalidOperationException || e.GetType().Name == "JsonException")
            { return new RedesignLoadResult { status = RedesignLoadStatus.Corrupt, notice = "Unreadable save preserved. Recovery is required." }; }
        }
        public RedesignLoadResult Load()
        {
            var main = Read(file);
            var backup = Read(file + ".bak");
            var interrupted = Read(file + ".tmp");
            // A newer app may have written any of these files. Never replace its evidence
            // with an older compatible snapshot, including during recovery.
            foreach (var candidate in new[] { main, backup, interrupted })
                if (candidate.status == RedesignLoadStatus.UnsupportedVersion || candidate.status == RedesignLoadStatus.LegacyTransitionRequired) return candidate;
            if (main.status != RedesignLoadStatus.Missing && main.status != RedesignLoadStatus.Corrupt) return main;
            if (backup.status == RedesignLoadStatus.Loaded) { backup.status = RedesignLoadStatus.RecoveredBackup; backup.state.requiresDrawReconciliation = true; backup.notice = "The last valid backup was recovered. Draw balances/results must reconcile before drawing again."; return backup; }
            if (main.status == RedesignLoadStatus.Missing && backup.status == RedesignLoadStatus.Missing)
            {
                if (interrupted.status != RedesignLoadStatus.Missing)
                    return new RedesignLoadResult { status = RedesignLoadStatus.Corrupt, notice = "An interrupted save is preserved. Recovery is required before starting or replacing this profile." };
                // Known old local sibling profiles are also superseded by the new economy.
                string old = Path.Combine(Path.GetDirectoryName(file), "diwali-v1.save");
                foreach (string path in new[] { old, old + ".bak" })
                    if (File.Exists(path)) { var legacy = Read(path); if (!legacy.CanCreateFresh) return legacy; }
                return main;
            }
            return new RedesignLoadResult { status = RedesignLoadStatus.Corrupt, notice = "No compatible valid save or backup. Original files preserved." };
        }
        public void Save(RedesignState state)
        {
            if (state == null || !state.Valid(catalog)) throw new InvalidDataException("Refusing invalid redesign state");
            var previous = Read(file);
            var existing = Load();
            bool retryFirstWrite = beganFreshWrite && previous.status == RedesignLoadStatus.Missing && !File.Exists(file + ".bak");
            if (existing.status == RedesignLoadStatus.LegacyTransitionRequired || existing.status == RedesignLoadStatus.UnsupportedVersion || existing.status == RedesignLoadStatus.Corrupt && !retryFirstWrite)
                throw new InvalidDataException("Existing profile must be recovered before it can be replaced");
            if (existing.status == RedesignLoadStatus.RecoveredBackup && !state.requiresDrawReconciliation)
                throw new InvalidDataException("Recovered draw state requires reconciliation");
            if (existing.state != null && state.revision.CompareTo(existing.state.revision) < 0) throw new InvalidDataException("Refusing stale state");
            if (existing.state?.HasPremium == true && (!state.HasPremium || state.premium.uid != existing.state.premium.uid || state.premium.profileId != existing.state.premium.profileId || state.premium.revision.CompareTo(existing.state.premium.revision) < 0))
                throw new InvalidDataException("Refusing an authoritative wallet downgrade or account replacement");
            string json = encode(state);
            if (Encoding.UTF8.GetByteCount(json) > 750000) throw new InvalidDataException("Save exceeds supported payload size; previous state retained");
            var roundtrip = decode(json);
            if (roundtrip == null || !roundtrip.Valid(catalog)) throw new InvalidDataException("Save codec produced invalid state");
            if (existing.CanCreateFresh) beganFreshWrite = true;
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            string raw = SavePayload.Digest(json) + "\n" + json;
            PreserveInterrupted(file);
            using (var stream = new FileStream(file + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            { byte[] bytes = Encoding.UTF8.GetBytes(raw); stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(file))
            {
                if (previous.status == RedesignLoadStatus.Loaded) File.Copy(file, file + ".bak", true);
                else File.Copy(file, file + ".damaged-" + Guid.NewGuid().ToString("N"), false);
                File.Replace(file + ".tmp", file, null);
            }
            else File.Move(file + ".tmp", file);
            beganFreshWrite = false;
        }
        internal static void PreserveInterrupted(string path)
        {
            if (!File.Exists(path + ".tmp")) return;
            string digest = SavePayload.Digest(File.ReadAllText(path + ".tmp"));
            string archive = path + ".interrupted-" + digest;
            if (!File.Exists(archive)) File.Copy(path + ".tmp", archive, false);
        }
    }
}
