namespace RocketRaja.Core
{
    public sealed partial class RedesignEngine
    {
        public bool ToggleSetting(string setting)
        {
            if (setting != "music" && setting != "sound" && setting != "motion") return false;
            return Commit(s => {
                if (setting == "music") s.music = !s.music;
                else if (setting == "sound") s.sound = !s.sound;
                else s.reducedMotion = !s.reducedMotion;
                return true;
            }, false);
        }
        public bool SetStoreName(string value)
        {
            if (!GameState.TryStoreName(value, out string name)) return false;
            return Commit(s => { s.storeName = name; s.storeNamePromptHandled = true; return true; }, false);
        }
    }
}
