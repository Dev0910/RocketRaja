using System;
using System.IO;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>Normal economy-2 profile namespace. Preview directories are selected explicitly by development tools.</summary>
    public static class RedesignEnvironment
    {
        // Production rollout remains gated by ProductionConfig.releaseAcceptanceComplete.
        public static bool Isolated => false;
        public const string ProfileNamespace = "rocket-raja-v2";
        // The save store detects incompatible legacy progress here and refuses a fresh fallback.
        // Disposable redesign and Classic playtest directories are never release profile sources.
        public static string DefaultProfileRoot => Application.persistentDataPath;
        public static string SeparateProfileRoot => Path.Combine(DefaultProfileRoot, "classic-v2");
        public static string ProfileRoot => PlayerPrefs.GetInt(ProfileNamespace + ".SeparateGuest", 0) == 1 ? SeparateProfileRoot : DefaultProfileRoot;
        // Called only after the explicit Bootstrap legacy-profile confirmation.
        public static void SelectSeparateGuest()
        {
            PlayerPrefs.SetInt(ProfileNamespace + ".SeparateGuest", 1);
            PlayerPrefs.Save();
        }
        public static string PreferenceKey(string key) => ProfileNamespace + "." + key;
        public static void RequireLiveCloud()
        {
            if (Isolated) throw new NotSupportedException("This redesign test build uses a separate guest save. Live Google saves are unavailable during development.");
        }
    }
}
