using System;
using System.Linq;

namespace RocketRaja.Core
{
    public sealed class GameEngine
    {
        public readonly Catalog Data;
        public GameState State { get; private set; }
        public event Action Changed;
        public event Action CriticalSave;
        // Session authorization lives outside the economy. Every command checks it, including direct UI calls.
        public Func<bool> MutationAllowed { private get; set; }
        public bool CanMutate => MutationAllowed == null || MutationAllowed();
        public GameEngine(Catalog data, GameState state)
        {
            Data = data; State = state;
            // Add newly authored definition groups to older development content snapshots.
            if (Data.skills == null || Data.skills.Length != 3) Data.skills = Catalog.Create().skills;
            if (Data.legacy == null || Data.legacy.Length != 12) Data.legacy = Catalog.Create().legacy;
        }
        void Touch(bool critical = false) { State.revision++; Changed?.Invoke(); if (critical) CriticalSave?.Invoke(); }
        public int SkillRank(int skill) => Data.upgrades.Count(x => x.effect == Effect.Skill && x.target == skill && State.upgrades.Contains(x.id));
        public int VisualTier(int index) => Catalog.VisualLevels.Count(level => State.Level(index) >= level);
        public int StallTier => State.OwnedProducts >= 8 ? 4 : State.OwnedProducts >= 6 ? 3 : State.OwnedProducts >= 3 ? 2 : State.OwnedProducts > 0 ? 1 : 0;
        double RunMultiplier(Effect effect, int target = -1) => Data.upgrades.Where(x => x.effect == effect && (target < 0 || x.target == target) && State.upgrades.Contains(x.id)).Aggregate(1d, (v, u) => v * u.multiplier);
        public Amount ProductIncome(int i, int extra = 0)
        {
            int n = State.Level(i) + extra; if (n == 0) return Amount.Zero;
            return new Amount(Data.products[i].income) * n * ProductMilestones.Multiplier(n) * RunMultiplier(Effect.ProductProfit, i) * RunMultiplier(Effect.AllProfit) * Math.Pow(Data.legacy[i].valuePerRank, State.Legacy(i)) * Math.Pow(Data.legacy[9].valuePerRank, State.Legacy(9));
        }
        public Amount NormalIncome => Enumerable.Range(0, 8).Aggregate(Amount.Zero, (a, i) => a + ProductIncome(i));
        public Amount TapIncome => new Amount(1 + State.tapLevel * 1.4) * RunMultiplier(Effect.TapProfit) * Math.Pow(Data.legacy[8].valuePerRank, State.Legacy(8));
        public double RushMultiplier => State.skills[0].activeUntil > State.now && SkillRank(0) > 0 ? Data.skills[0].values[SkillRank(0) - 1] : 1;
        public Amount LiveIncome => NormalIncome * RushMultiplier * (State.sponsorUntil > State.now ? 2 : 1);
        public double OfflineCap => (8 + Data.legacy[10].valuePerRank * State.Legacy(10)) * 3600;
        public void Grant(Amount amount, bool eligible = true)
        {
            if (!CanMutate) return;
            State.cash += amount; State.runEarnings += amount; State.lifetimeEarnings += amount;
            if (eligible) State.eligibleEarnings += amount;
        }
        public void Tap() { if (!CanMutate) return; Grant(TapIncome); State.manualTaps++; Touch(); }
        public void Advance(double seconds)
        {
            if (!CanMutate) return;
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            double end = State.now + seconds;
            while (State.now < end)
            {
                double stop = end;
                foreach (var skill in State.skills) if (skill.activeUntil > State.now) stop = Math.Min(stop, skill.activeUntil);
                if (State.sponsorUntil > State.now) stop = Math.Min(stop, State.sponsorUntil);
                double duration = stop - State.now;
                Amount earned = NormalIncome * RushMultiplier * duration;
                Grant(earned);
                if (State.sponsorUntil > State.now) Grant(earned, false);
                int rank = SkillRank(1);
                if (rank > 0 && State.skills[1].activeUntil > State.now) Grant(TapIncome * Data.skills[1].values[rank - 1] * duration);
                State.now = stop; State.runSeconds += duration;
            }
            Touch();
        }
        public Amount ProductCost(int i, int quantity)
        {
            if (quantity <= 0) return Amount.Zero;
            return new Amount(Data.products[i].cost) * RunMultiplier(Effect.ProductDiscount, i) * Amount.Pow10(State.Level(i) * Math.Log10(Catalog.Growth)) * (Amount.Pow10(quantity * Math.Log10(Catalog.Growth)) - new Amount(1)) / (Catalog.Growth - 1);
        }
        public Quote ProductQuote(int i, int desired)
        {
            if (i > 0 && State.Level(i - 1) == 0) return new Quote();
            int cap = Catalog.MaxLevel - State.Level(i);
            int q = Math.Min(desired, cap);
            if (desired == int.MaxValue)
            {
                int low = 0, high = cap;
                while (low < high) { int mid = low + (high - low + 1) / 2; if (State.cash >= ProductCost(i, mid)) low = mid; else high = mid - 1; }
                q = low;
            }
            Amount cost = ProductCost(i, q); return new Quote { quantity = q, cost = cost, affordable = q > 0 && State.cash >= cost };
        }
        public bool BuyProduct(int i, int desired)
        {
            if (!CanMutate) return false;
            var q = ProductQuote(i, desired); if (!q.affordable) return false;
            State.cash -= q.cost; State.products.Find(x => x.id == Data.products[i].id).level += q.quantity; Touch(true); return true;
        }
        public Amount TapCost => new Amount(40) * Amount.Pow10(State.tapLevel * Math.Log10(1.17));
        public Amount TapCostFor(int quantity)
        {
            if (quantity <= 0) return Amount.Zero;
            return TapCost * (Amount.Pow10(quantity * Math.Log10(1.17)) - new Amount(1)) / .17;
        }
        public Quote TapQuote(int desired)
        {
            int cap = Catalog.MaxLevel - State.tapLevel;
            int quantity = Math.Min(desired, cap);
            if (desired == int.MaxValue)
            {
                int low = 0, high = cap;
                while (low < high)
                {
                    int mid = low + (high - low + 1) / 2;
                    if (State.cash >= TapCostFor(mid)) low = mid; else high = mid - 1;
                }
                quantity = low;
            }
            Amount cost = TapCostFor(quantity);
            return new Quote { quantity = quantity, cost = cost, affordable = quantity > 0 && State.cash >= cost };
        }
        public bool BuyTap(int desired)
        {
            if (!CanMutate) return false;
            var quote = TapQuote(desired);
            if (!quote.affordable) return false;
            State.cash -= quote.cost;
            State.tapLevel += quote.quantity;
            Touch(true);
            return true;
        }
        public bool BuyTap()
        {
            return BuyTap(1);
        }
        public bool Unlocked(UpgradeDef u)
        {
            if (State.upgrades.Contains(u.id)) return false;
            if (u.effect == Effect.Skill) return SkillRank(u.target) == u.tier && State.OwnedProducts >= u.target + 1;
            if (u.effect == Effect.TapProfit) return State.tapLevel >= u.requiredLevel;
            return State.Level(u.target) >= u.requiredLevel;
        }
        public bool BuyUpgrade(string id)
        {
            if (!CanMutate) return false;
            var u = Data.upgrades.FirstOrDefault(x => x.id == id); if (u == null || !Unlocked(u) || State.cash < u.price) return false;
            State.cash -= u.price; State.upgrades.Add(id); Touch(true); return true;
        }
        public int BuyAll()
        {
            int count = 0; foreach (var u in Data.upgrades) if (BuyUpgrade(u.id)) count++; return count;
        }
        public bool ActivateSkill(int i)
        {
            if (!CanMutate) return false;
            int rank = SkillRank(i); var skill = State.skills[i];
            if (rank == 0 || skill.readyAt > State.now) return false;
            skill.readyAt = State.now + Data.skills[i].cooldown * (1 - Data.legacy[11].valuePerRank * State.Legacy(11));
            if (i == 2) Grant(NormalIncome * Data.skills[i].values[rank - 1]);
            else skill.activeUntil = State.now + Data.skills[i].duration;
            Touch(true); return true;
        }
        public PrestigeQuote PreviewPrestige()
        {
            double value = State.eligibleEarnings.Log10 - Math.Log10(Catalog.PrestigeThreshold);
            long reward = value < 0 || State.OwnedProducts < 8 ? 0 : (long)Math.Min(1000000, Math.Floor(5 * Math.Pow(10, Math.Min(12, value) * .5)));
            return new PrestigeQuote { runId = State.runId, revision = State.revision, reward = reward };
        }
        public bool Prestige(PrestigeQuote quote)
        {
            if (!CanMutate) return false;
            var live = PreviewPrestige();
            if (quote.runId != live.runId || quote.revision != live.revision || quote.reward != live.reward || live.reward <= 0) return false;
            if (State.reputation > long.MaxValue - live.reward || State.lifetimeReputation > long.MaxValue - live.reward) return false;
            State.reputation += live.reward; State.lifetimeReputation += live.reward; State.prestiges++;
            State.cash = State.runEarnings = State.eligibleEarnings = Amount.Zero;
            State.tapLevel = 0; State.runSeconds = 0; State.upgrades.Clear(); State.products.ForEach(x => x.level = 0);
            State.skills.ForEach(x => { x.activeUntil = 0; x.readyAt = 0; }); State.sponsorUntil = 0;
            State.pendingOfflineId = null; State.pendingOfflineReward = Amount.Zero;
            State.runId = Guid.NewGuid().ToString("N"); State.onboarding = 4;
            Touch(true); return true;
        }
        public long LegacyCost(int i) => Data.legacy[i].initialCost * (1L << State.Legacy(i));
        public bool BuyLegacy(int i)
        {
            if (!CanMutate) return false;
            if (State.Legacy(i) >= Data.legacy[i].maxRank || State.reputation < LegacyCost(i)) return false;
            State.reputation -= LegacyCost(i); State.legacy[i].level++; Touch(true); return true;
        }
        public Amount ResolveOffline(double utcNow)
        {
            if (!CanMutate) return Amount.Zero;
            if (double.IsNaN(utcNow) || double.IsInfinity(utcNow) || utcNow < 0) throw new ArgumentOutOfRangeException(nameof(utcNow));
            double anchor = Math.Max(State.savedAt, State.now);
            double elapsed = Math.Max(0, utcNow - anchor);
            Amount reward = NormalIncome * (.5 * Math.Min(OfflineCap, elapsed));
            Grant(reward);
            State.now = State.savedAt = Math.Max(anchor, utcNow);
            // Save the new time together with this payout, so focus callbacks cannot award it twice.
            if (elapsed >= 60 && reward > Amount.Zero) { State.pendingOfflineId = Guid.NewGuid().ToString("N"); State.pendingOfflineReward = reward; }
            Touch(true); return reward;
        }
        public bool RewardAd(string receipt, string placement, string offlineId = null)
        {
            if (!CanMutate) return false;
            if (string.IsNullOrEmpty(receipt) || State.claimedRewards.Contains(receipt)) return false;
            if (placement == "sponsor") { if (State.sponsorUntil > State.now) return false; State.sponsorUntil = State.now + 300; }
            else if (placement == "offline")
            {
                if (string.IsNullOrEmpty(offlineId) || offlineId != State.pendingOfflineId) return false;
                Grant(State.pendingOfflineReward, false); State.pendingOfflineId = null; State.pendingOfflineReward = Amount.Zero;
            }
            else return false;
            State.claimedRewards.Add(receipt); if (State.claimedRewards.Count > 128) State.claimedRewards.RemoveAt(0);
            Touch(true); return true;
        }
        public void DismissOffline() { if (!CanMutate) return; State.pendingOfflineId = null; State.pendingOfflineReward = Amount.Zero; Touch(true); }
    }
}
