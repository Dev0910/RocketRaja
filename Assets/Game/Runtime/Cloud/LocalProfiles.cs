using System;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Cloud
{
    public sealed class GuestSession : IPlayerSession
    {
        readonly SaveStore store;
        public bool IsAccount => false;
        public bool CanPlay => !NeedsRecovery;
        public bool NeedsRecovery { get; private set; }
        public string Warning { get; private set; }
        public string Status => NeedsRecovery ? Warning : "Saved on this device";
        public GameState State { get; }
        public GuestSession(string root)
        {
            store = new SaveStore(Path.Combine(root, "diwali-v1.save"), JsonUtility.ToJson, JsonUtility.FromJson<GameState>);
            State = store.Load(); Warning = store.Warning;
            NeedsRecovery = State == null && Warning != null;
            if (State == null) State = new GameState { now = GameController.Utc, savedAt = GameController.Utc };
        }
        public void Save(bool immediate = false)
        {
            if (NeedsRecovery) return;
            State.savedAt = Math.Max(State.savedAt, State.now);
            store.Save(State); Warning = null;
        }
        public void StartFresh()
        {
            // SaveStore archives unreadable originals before replacing them.
            NeedsRecovery = false;
            try { Save(); } catch { NeedsRecovery = true; throw; }
        }
    }
    public static class LocalProfiles
    {
        public static string Root
        {
            get
            {
                if (RedesignEnvironment.Isolated) return RedesignEnvironment.ProfileRoot;
                string current = Application.persistentDataPath;
#if UNITY_EDITOR || UNITY_STANDALONE
                string parent = Path.GetDirectoryName(current);
                if (!string.IsNullOrEmpty(parent))
                {
                    string legacy = Path.Combine(parent, "Diwali Fireworks Tycoon");
                    if (!File.Exists(Path.Combine(current, "diwali-v1.save")) && !File.Exists(Path.Combine(current, "diwali-v1.save.bak")) &&
                        (File.Exists(Path.Combine(legacy, "diwali-v1.save")) || File.Exists(Path.Combine(legacy, "diwali-v1.save.bak")))) return legacy;
                }
#endif
                return current;
            }
        }
        public static string DeviceId
        {
            get
            {
                string key = RedesignEnvironment.PreferenceKey("RocketRaja.CloudDevice");
                string id = PlayerPrefs.GetString(key, "");
                if (id.Length != 32) { id = Guid.NewGuid().ToString("N"); PlayerPrefs.SetString(key, id); PlayerPrefs.Save(); }
                return id;
            }
        }
        public static string Mode => PlayerPrefs.GetString(RedesignEnvironment.PreferenceKey("RocketRaja.IdentityMode"), "");
        public static void SetMode(string value) { PlayerPrefs.SetString(RedesignEnvironment.PreferenceKey("RocketRaja.IdentityMode"), value); PlayerPrefs.Save(); }
        public static bool HasProgress(GameState s) => s != null && (s.manualTaps > 0 || s.lifetimeEarnings > Amount.Zero || s.prestiges > 0 || s.OwnedProducts > 0 || s.upgrades.Count > 0 || s.legacy.Any(x => x.level > 0));
        public static void Backup(GameState state, string uid, string label)
        {
            string path = Path.Combine(Root, "accounts", SavePayload.Digest(uid), label + "-" + Guid.NewGuid().ToString("N") + ".save");
            new SaveStore(path, JsonUtility.ToJson, JsonUtility.FromJson<GameState>).Save(state);
        }
    }
}
