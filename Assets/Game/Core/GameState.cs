using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketRaja.Core
{
    [Serializable] public class OwnedLevel { public string id; public int level; public OwnedLevel() {} public OwnedLevel(string key, int value = 0) { id = key; level = value; } }
    [Serializable] public class SkillState { public string id; public double activeUntil, readyAt; }
    [Serializable] public class GameState
    {
        public int schema = 1;
        public string economy = Catalog.EconomyVersion;
        public long revision;
        public string runId = Guid.NewGuid().ToString("N");
        public Amount cash, runEarnings, eligibleEarnings, lifetimeEarnings;
        public int tapLevel, prestiges;
        public long reputation, lifetimeReputation, manualTaps;
        public List<OwnedLevel> products = Catalog.ProductIds.Select(x => new OwnedLevel(x)).ToList();
        public List<string> upgrades = new List<string>();
        public List<OwnedLevel> legacy = Enumerable.Range(0, 12).Select(i => new OwnedLevel(i < 8 ? "legacy-" + Catalog.ProductIds[i] : new[] { "legacy-tap", "legacy-all", "legacy-offline", "legacy-cooldown" }[i - 8])).ToList();
        public List<SkillState> skills = Enumerable.Range(0, 3).Select(i => new SkillState { id = "skill-" + i }).ToList();
        public double now, savedAt, sponsorUntil, runSeconds;
        public int onboarding;
        public bool music = true, sound = true, reducedMotion;
        public string pendingOfflineId;
        public Amount pendingOfflineReward;
        public List<string> claimedRewards = new List<string>();
        public int Level(int i) => products.Find(x => x.id == Catalog.ProductIds[i])?.level ?? 0;
        public int Legacy(int i) => legacy[i].level;
        public int OwnedProducts => products.Count(x => x.level > 0);
        public bool Valid()
        {
            bool Time(double v) => !double.IsNaN(v) && !double.IsInfinity(v) && v >= 0 && v < 1e12;
            return schema == 1 && !string.IsNullOrEmpty(runId) && revision >= 0 && reputation >= 0 && lifetimeReputation >= reputation && prestiges >= 0 && manualTaps >= 0 && tapLevel >= 0 && tapLevel <= Catalog.MaxLevel &&
                cash.Valid && runEarnings.Valid && eligibleEarnings.Valid && lifetimeEarnings.Valid && pendingOfflineReward.Valid &&
                Time(now) && Time(savedAt) && Time(sponsorUntil) && Time(runSeconds) &&
                products != null && products.Count == 8 && products.Select(x => x.id).Distinct().Count() == 8 && products.All(x => Catalog.ProductIds.Contains(x.id) && x.level >= 0 && x.level <= Catalog.MaxLevel) &&
                legacy != null && legacy.Count == 12 && legacy.All(x => x != null && x.level >= 0 && x.level <= 5) && legacy.Select(x => x.id).SequenceEqual(new GameState().legacy.Select(x => x.id)) &&
                upgrades != null && upgrades.Distinct().Count() == upgrades.Count && upgrades.All(id => Catalog.Create().upgrades.Any(u => u.id == id)) &&
                skills != null && skills.Count == 3 && skills.Select(x => x.id).SequenceEqual(new[] { "skill-0", "skill-1", "skill-2" }) && skills.All(x => Time(x.activeUntil) && Time(x.readyAt)) && claimedRewards != null && claimedRewards.Count <= 128 && onboarding >= 0 && onboarding <= 4;
        }
    }
    public struct Quote { public int quantity; public Amount cost; public bool affordable; }
    public struct PrestigeQuote { public string runId; public long revision, reward; }
}
