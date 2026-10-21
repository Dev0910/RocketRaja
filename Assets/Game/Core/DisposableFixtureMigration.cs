#if UNITY_EDITOR || DEVELOPMENT_BUILD || REDESIGN_LAB
using System;
using System.IO;

namespace RocketRaja.Core
{
    /// <summary>Only the committed synthetic fixture directory may use the test reset policy.</summary>
    public static class DisposableFixtureMigration
    {
        public static RedesignState Convert(string source, string destination, RedesignCatalog catalog,
            Func<string, GameState> decodeLegacy, Func<RedesignState, string> encode,
            Func<string, RedesignState> decode, Func<string, SaveVersionHeader> readVersion, double now)
        {
            string allowed = Path.GetFullPath("Documentation/Implementation/Fixtures/Legacy") + Path.DirectorySeparatorChar;
            string sourceFull = Path.GetFullPath(source), destinationFull = Path.GetFullPath(destination);
            if (!sourceFull.StartsWith(allowed, StringComparison.OrdinalIgnoreCase) || sourceFull == destinationFull)
                throw new InvalidOperationException("Only explicitly identified synthetic fixtures may be reset. Real saves require a reviewed transition.");
            string raw = File.ReadAllText(sourceFull); int newline = raw.IndexOf('\n');
            if (newline <= 0 || raw.Substring(0, newline).Trim() != SavePayload.Digest(raw.Substring(newline + 1))) throw new InvalidDataException("Invalid legacy checksum");
            var old = decodeLegacy(raw.Substring(newline + 1));
            if (old == null || !old.Valid() || old.economy != "1.0.0") throw new InvalidDataException("Invalid legacy fixture");
            string digest = SavePayload.Digest(raw);
            var store = new RedesignSaveStore(destinationFull, catalog, encode, decode, readVersion);
            var existing = store.Load();
            if (existing.status == RedesignLoadStatus.Loaded && existing.state.migrationSourceDigest == digest) return existing.state;
            if (!existing.CanCreateFresh) throw new InvalidDataException("Destination contains progress or recovery evidence");
            string backup = sourceFull + ".pre-v2-" + digest;
            if (!File.Exists(backup)) File.Copy(sourceFull, backup, false);
            if (File.ReadAllText(backup) != raw) throw new InvalidDataException("Pre-migration archive does not match the source");
            var state = RedesignState.Fresh(now);
            state.music = old.music; state.sound = old.sound; state.reducedMotion = old.reducedMotion;
            state.storeName = GameState.TryStoreName(old.storeName, out string name) ? name : "";
            state.storeNamePromptHandled = old.storeNamePromptHandled && state.storeName != "";
            state.migrationSourceDigest = digest;
            // No Legacy rank, old ID, reputation, sponsor, ad receipt, cash or onboarding conversion.
            store.Save(state);
            return state;
        }
    }
}
#endif
