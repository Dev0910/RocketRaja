using System;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja
{
    [Serializable] public sealed class RedesignContentManifest
    {
        public string sourceHash, economyVersion, contentVersion;
    }
    public static class RedesignRuntimeContent
    {
        static RedesignCatalog cached;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => cached = null;
        public static RedesignCatalog Load()
        {
            if (cached != null) return cached;
            var asset = Resources.Load<GameContent>("GameContent");
            var manifest = Resources.Load<TextAsset>("RedesignContentManifest");
            if (asset == null || manifest == null) throw new InvalidOperationException("Redesign content is missing. Regenerate the authored content before building.");
            var version = JsonUtility.FromJson<RedesignContentManifest>(manifest.text);
            var candidate = new RedesignCatalog(asset.redesign);
            if (version == null || version.sourceHash != candidate.SourceHash || version.economyVersion != RedesignCatalog.EconomyVersion || version.contentVersion != RedesignCatalog.ContentVersion)
                throw new InvalidOperationException("Redesign content is outdated. Regenerate the authored content before building.");
            return cached = candidate;
        }
    }
}
