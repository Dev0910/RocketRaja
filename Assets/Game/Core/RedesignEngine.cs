using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;

namespace RocketRaja.Core
{
    public interface IDrawRandom { int Next(int exclusiveMaximum); }
    public static class TalentDrawPool
    {
        public static int Select(RedesignCatalog data, IDrawRandom random)
        {
            int roll = random.Next(100);
            if (roll < 0 || roll >= 100) throw new InvalidOperationException("Random source returned an out-of-range tier");
            CardTier tier = roll < 75 ? CardTier.Common : roll < 97 ? CardTier.Rare : CardTier.Legendary;
            int identity = random.Next(data.TierCount(tier));
            if (identity < 0 || identity >= data.TierCount(tier)) throw new InvalidOperationException("Random source returned an out-of-range identity");
            return data.CardInTier(tier, identity);
        }
    }
    public sealed class SecureDrawRandom : IDrawRandom
    {
        public int Next(int exclusiveMaximum)
        {
            if (exclusiveMaximum <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMaximum));
            uint bound = (uint)exclusiveMaximum, threshold = unchecked(0u - bound) % bound;
            using (var random = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4]; uint value;
                do { random.GetBytes(bytes); value = BitConverter.ToUInt32(bytes, 0); } while (value < threshold);
                return (int)(value % bound);
            }
        }
    }
    public sealed class UpgradeScope
    {
        public UpgradeCategory category;
        public CashEffect? effect;
        public int product = -1, skill = -1;
        public string search = "";
        public bool Matches(CashUpgrade u) => MatchesCategory(u, category) && (!effect.HasValue || effect == u.effect) &&
            (product < 0 || (u.effect == CashEffect.ProductProfit || u.effect == CashEffect.ProductCost || u.effect == CashEffect.Link) && u.target == product || u.beneficiary == product || u.group.Contains(product)) &&
            (skill < 0 || u.effect >= CashEffect.SkillPower && u.effect <= CashEffect.SkillCooldown && u.target == skill) &&
            (string.IsNullOrEmpty(search) || u.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || u.id.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || u.effectText.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
        public static bool MatchesCategory(CashUpgrade u, UpgradeCategory category)
        {
            switch (category)
            {
                case UpgradeCategory.All: return true;
                case UpgradeCategory.Products: return u.effect == CashEffect.ProductProfit || u.effect == CashEffect.ProductCost || u.effect == CashEffect.Link || u.effect == CashEffect.Group;
                case UpgradeCategory.Tapping: return u.effect == CashEffect.TapDirect || u.effect == CashEffect.TapLinked || u.effect == CashEffect.TapCost;
                case UpgradeCategory.Skills: return u.effect >= CashEffect.SkillPower && u.effect <= CashEffect.SkillCooldown;
                case UpgradeCategory.Shop: return u.effect == CashEffect.GlobalProfit || u.effect == CashEffect.UpgradeCost;
                default: return false;
            }
        }
        public UpgradeScope Copy() => (UpgradeScope)MemberwiseClone();
    }
    public sealed class CashBatchQuote
    {
        internal string owner, session, run, command;
        internal UpgradeScope scope;
        public ReadOnlyCollection<string> Ids { get; internal set; }
        public ReadOnlyCollection<Amount> Prices { get; internal set; }
        public Amount Budget { get; internal set; }
        public Amount Cost { get; internal set; }
        public int FirstPurchases { get; internal set; }
        public UpgradeScope Selection => scope.Copy();
    }
    public sealed class FameQuote
    {
        internal string owner, session, run, command;
        public Natural Fame { get; internal set; }
        public Natural Credits { get; internal set; }
        public Amount CurrentTarget { get; internal set; }
        public Amount NextTarget { get; internal set; }
        public Amount ResultingIncomeMultiplier { get; internal set; }
        public Amount ResultingTapIncomeMultiplier { get; internal set; }
        public Amount NextFameEarnings { get; internal set; }
        public Amount NextDrawEarnings { get; internal set; }
        public Natural RunCredits { get; internal set; }
        public int CatalogueCredits { get; internal set; }
        public int NextUniqueMilestone { get; internal set; }
        public bool Eligible => !Fame.IsZero;
    }
    public enum AffectedCost { None, ProductLevels, TapLevels, CashUpgrades }
    public sealed class UpgradeProjection
    {
        public string id;
        public bool requirementsMet;
        public Amount price, normalBefore, normalAfter, manualTapBefore, manualTapAfter, costBefore, costAfter;
        public Amount[] productsBefore, productsAfter;
        public AffectedCost costCategory;
        public double skillPowerBefore, skillPowerAfter, durationBefore, durationAfter, cooldownBefore, cooldownAfter;
    }
    public sealed class TalentDrawQuote
    {
        internal string owner, session, run, command;
        internal int selectedCard = -1;
        public string OperationId { get; internal set; }
        public bool UseCredit { get; internal set; }
    }
    sealed class RunEffects
    {
        public readonly Amount[] profit = Enumerable.Repeat(new Amount(1), 8).ToArray();
        public readonly double[] productCost = Enumerable.Repeat(1d, 8).ToArray();
        public readonly int[] skillPower = new int[3], skillDuration = new int[3], skillCooldown = new int[3];
        public Amount global = 1, tapDirect = 1;
        public int directCount, linkedCount;
        public double tapCost = 1, upgradeCost = 1;
        public readonly HashSet<string> owned, ever;
        public RunEffects(RedesignCatalog catalog, RedesignState state)
        {
            owned = new HashSet<string>(StringComparer.Ordinal); ever = new HashSet<string>(state.everPurchased, StringComparer.Ordinal);
            foreach (string id in state.upgrades) { catalog.TryUpgrade(id, out int i); Add(catalog.Upgrades[i]); }
        }
        public void Add(CashUpgrade u)
        {
            owned.Add(u.id);
            switch (u.effect)
            {
                case CashEffect.ProductProfit: profit[u.target] *= u.factor; break;
                case CashEffect.ProductCost: productCost[u.target] = Math.Min(productCost[u.target], u.factor); break;
                case CashEffect.TapDirect: tapDirect *= u.factor; directCount++; break;
                case CashEffect.TapLinked: linkedCount = Math.Max(linkedCount, u.rank); break;
                case CashEffect.TapCost: tapCost = Math.Min(tapCost, u.factor); break;
                case CashEffect.GlobalProfit: global *= u.factor; break;
                case CashEffect.UpgradeCost: upgradeCost = Math.Min(upgradeCost, u.factor); break;
                case CashEffect.SkillPower: skillPower[u.target] = Math.Max(skillPower[u.target], u.rank); break;
                case CashEffect.SkillDuration: skillDuration[u.target] = Math.Max(skillDuration[u.target], u.rank); break;
                case CashEffect.SkillCooldown: skillCooldown[u.target] = Math.Max(skillCooldown[u.target], u.rank); break;
                case CashEffect.Link: profit[u.beneficiary] *= u.factor; break;
                case CashEffect.Group: foreach (int p in u.group) profit[p] *= u.factor; break;
            }
        }
    }

    /// <summary>Economy-2 commands stage a complete state, persist it, then publish it to the runtime.</summary>
    public sealed partial class RedesignEngine
    {
        public RedesignCatalog Data { get; }
        public RedesignState State { get; private set; }
        public Func<bool> MutationAllowed = () => true;
        public Func<bool> PremiumSpendAllowed = () => true;
        public string SessionKey { get; set; } = "disposable-guest";
        readonly Action<RedesignState> persist;
        readonly IDrawRandom random;
        readonly string owner = Guid.NewGuid().ToString("N");
        readonly CashUpgrade[] cheapest;
        RunEffects cached;
        RunEffects Effects => cached ?? (cached = new RunEffects(Data, State));
        public RedesignEngine(RedesignCatalog data, RedesignState state, Action<RedesignState> persist, IDrawRandom random = null)
        {
            Data = data; if (!state.Valid(data)) throw new ArgumentException("Invalid redesign state");
            State = state.Copy(); this.persist = persist ?? throw new ArgumentNullException(nameof(persist));
            this.random = random ?? new SecureDrawRandom();
            cheapest = data.Upgrades.OrderBy(x => x.price).ThenBy(x => x.id, StringComparer.Ordinal).ToArray();
        }
        bool Commit(Func<RedesignState, bool> change, bool structural = true)
        {
            if (!MutationAllowed()) return false;
            string session = SessionKey;
            var next = State.Copy();
            if (!change(next)) return false;
            next.mostProductsStocked = Math.Max(next.mostProductsStocked, Math.Max(State.products.Count(n => n > 0), next.products.Count(n => n > 0)));
            next.revision += (Natural)1;
            if (structural) next.commandRevision += (Natural)1;
            next.savedAt = next.now;
            var effects = new RunEffects(Data, next);
            next.departureRate = NormalIncome(next, effects);
            if (!next.Valid(Data)) throw new InvalidOperationException("Command produced invalid state");
            if (!MutationAllowed() || session != SessionKey) return false;
            persist(next); // Exceptions intentionally leave State and its caches unchanged.
            State = next; cached = effects;
            return true;
        }
        bool Same(string qOwner, string qSession, string qRun, string qCommand) => MutationAllowed() && qOwner == owner && qSession == SessionKey && qRun == State.runId && qCommand == State.commandRevision.digits;
        static Amount Global(RedesignState s, RunEffects f) => f.global * ProgressionNumbers.FameMultiplier(s.fame) * ProgressionNumbers.CardMultiplier(CardEffect.GlobalProfit, s.cardLevels[26]);
        Amount ProductIncome(int p, RedesignState s, RunEffects f) => new Amount(Data.Products[p].income) * s.products[p] * EconomyBalance.MilestoneMultiplier(s.products[p]) * EconomyBalance.SynergyMultiplier(s.products.Min()) * f.profit[p] * ProgressionNumbers.CardMultiplier(CardEffect.ProductProfit, s.cardLevels[p]) * Global(s, f);
        Amount NormalIncome(RedesignState s, RunEffects f) { Amount sum = 0; for (int p = 0; p < 8; p++) sum += ProductIncome(p, s, f); return sum; }
        public Amount ProductIncome(int product) => ProductIncome(product, State, Effects);
        public Amount NormalIncome() => NormalIncome(State, Effects);
        static Amount Geometric(double baseCost, int level, int count) => new Amount(baseCost) * Amount.Pow10(level * Math.Log10(EconomyBalance.LevelGrowth)) * (count == 1 ? new Amount(1) : (Amount.Pow10(count * Math.Log10(EconomyBalance.LevelGrowth)) - new Amount(1)) / (EconomyBalance.LevelGrowth - 1));
        public bool ProductAvailable(int p) => p >= 0 && p < 8 && (p == 0 || State.products[p - 1] > 0);
        Amount ProductCost(int p, int q, RedesignState s, RunEffects f) => Geometric(Data.Products[p].cost, s.products[p], q) * f.productCost[p] * ProgressionNumbers.CardFactor(CardEffect.ProductCost, s.cardLevels[8 + p]);
        Amount TapCost(int q, RedesignState s, RunEffects f) => Geometric(10, s.tapLevel, q) * f.tapCost * ProgressionNumbers.CardFactor(CardEffect.TapCost, s.cardLevels[17]);
        public Quote LevelQuote(int product, int quantity)
        {
            bool tap = product == -1;
            if (quantity <= 0 || !tap && !ProductAvailable(product)) return default;
            int remaining = Catalog.MaxLevel - (tap ? State.tapLevel : State.products[product]);
            if (remaining <= 0) return default;
            Amount Price(int q) => tap ? TapCost(q, State, Effects) : ProductCost(product, q, State, Effects);
            int count;
            if (quantity == int.MaxValue)
            {
                int lo = 0, hi = remaining;
                while (lo < hi) { int mid = lo + (hi - lo + 1) / 2; if (Price(mid) <= State.cash) lo = mid; else hi = mid - 1; }
                count = lo;
            }
            else count = Math.Min(remaining, quantity);
            return new Quote { quantity = count, cost = Price(Math.Max(1, count)), affordable = count > 0 && Price(count) <= State.cash };
        }
        public bool BuyLevels(int product, int quantity)
        {
            if (!MutationAllowed()) return false;
            var quote = LevelQuote(product, quantity);
            if (!quote.affordable) return false;
            return Commit(s => { s.cash -= quote.cost; if (product == -1) s.tapLevel += quote.quantity; else s.products[product] += quote.quantity; return true; });
        }
        bool Eligible(CashUpgrade u, RedesignState s, RunEffects f) => !f.owned.Contains(u.id);
        Amount UpgradeCost(CashUpgrade u, RedesignState s, RunEffects f) => u.price * f.upgradeCost * ProgressionNumbers.CardFactor(CardEffect.UpgradeCost, s.cardLevels[27]);
        public bool Eligible(string id) => Data.TryUpgrade(id, out int i) && Eligible(Data.Upgrades[i], State, Effects);
        public Amount UpgradeCost(string id) => Data.TryUpgrade(id, out int i) ? UpgradeCost(Data.Upgrades[i], State, Effects) : throw new ArgumentException("Unknown upgrade");
        public UpgradeProjection PreviewUpgrade(string id)
        {
            if (!Data.TryUpgrade(id, out int index) || Effects.owned.Contains(id)) return null;
            var u = Data.Upgrades[index]; var after = State.Copy(); var effectsAfter = new RunEffects(Data, after);
            effectsAfter.Add(u); // Hypothetical effect only; never spend, persist or change the real state.
            var projection = new UpgradeProjection { id = id, requirementsMet = Eligible(u, State, Effects), price = UpgradeCost(id),
                normalBefore = NormalIncome(), normalAfter = NormalIncome(after, effectsAfter),
                productsBefore = Enumerable.Range(0, 8).Select(p => ProductIncome(p, State, Effects)).ToArray(),
                productsAfter = Enumerable.Range(0, 8).Select(p => ProductIncome(p, after, effectsAfter)).ToArray(),
                manualTapBefore = BaseTapPayout(false, State, Effects), manualTapAfter = BaseTapPayout(false, after, effectsAfter) };
            if (u.effect == CashEffect.ProductCost)
            { projection.costCategory = AffectedCost.ProductLevels; projection.costBefore = ProductCost(u.target, 1, State, Effects); projection.costAfter = ProductCost(u.target, 1, after, effectsAfter); }
            else if (u.effect == CashEffect.TapCost)
            { projection.costCategory = AffectedCost.TapLevels; projection.costBefore = TapCost(1, State, Effects); projection.costAfter = TapCost(1, after, effectsAfter); }
            else if (u.effect == CashEffect.UpgradeCost)
            { projection.costCategory = AffectedCost.CashUpgrades; projection.costBefore = 100 * Effects.upgradeCost * ProgressionNumbers.CardFactor(CardEffect.UpgradeCost, State.cardLevels[27]); projection.costAfter = 100 * effectsAfter.upgradeCost * ProgressionNumbers.CardFactor(CardEffect.UpgradeCost, after.cardLevels[27]); }
            if (u.effect >= CashEffect.SkillPower && u.effect <= CashEffect.SkillCooldown)
            {
                int skill = u.target;
                projection.skillPowerBefore = Effects.skillPower[skill] == 0 ? 0 : RedesignCatalog.SkillPower[skill][Effects.skillPower[skill] - 1];
                projection.skillPowerAfter = effectsAfter.skillPower[skill] == 0 ? 0 : RedesignCatalog.SkillPower[skill][effectsAfter.skillPower[skill] - 1];
                projection.durationBefore = Duration(skill, State, Effects); projection.durationAfter = Duration(skill, after, effectsAfter);
                projection.cooldownBefore = Cooldown(skill, State, Effects); projection.cooldownAfter = Cooldown(skill, after, effectsAfter);
            }
            return projection;
        }
        static void ApplyPurchase(CashUpgrade u, Amount price, RedesignState s, RunEffects f)
        {
            s.cash -= price; s.upgrades.Add(u.id);
            if (s.pinnedUpgradeId == u.id) s.pinnedUpgradeId = "";
            if (f.ever.Add(u.id)) s.everPurchased.Add(u.id);
            bool unlocksSkill = u.effect == CashEffect.SkillPower && f.skillPower[u.target] == 0;
            f.Add(u);
            if (unlocksSkill) { s.readyAt[u.target] = s.now; s.activeUntil[u.target] = 0; s.nextHandsAt[u.target] = 0; }
            s.decorationStage = Math.Max(s.decorationStage, RedesignShopProgress.StageForUnique(s.UniqueCount));
        }
        public bool BuyUpgrade(string id)
        {
            if (!Data.TryUpgrade(id, out int i) || !Eligible(id)) return false;
            var u = Data.Upgrades[i]; var cost = UpgradeCost(u, State, Effects);
            if (cost > State.cash) return false;
            return Commit(s => { ApplyPurchase(u, cost, s, new RunEffects(Data, s)); return true; });
        }
        public CashBatchQuote QuoteBuyAll(UpgradeScope scope = null)
        {
            var filter = (scope ?? new UpgradeScope()).Copy();
            var work = State.Copy(); var effects = new RunEffects(Data, work);
            var ids = new List<string>(); var prices = new List<Amount>(); Amount cost = 0; int before = work.UniqueCount;
            foreach (var chosen in cheapest)
            {
                if (effects.owned.Contains(chosen.id) || !filter.Matches(chosen)) continue;
                // Cash-upgrade discounts are uniform, so if the cheapest eligible row is
                // unaffordable no more expensive row can be bought to change that discount.
                if (UpgradeCost(chosen, work, effects) > work.cash) break;
                var price = UpgradeCost(chosen, work, effects); ids.Add(chosen.id); prices.Add(price); cost += price;
                ApplyPurchase(chosen, price, work, effects);
            }
            return new CashBatchQuote { owner = owner, session = SessionKey, run = State.runId, command = State.commandRevision.digits, scope = filter, Ids = ids.AsReadOnly(), Prices = prices.AsReadOnly(), Budget = State.cash, Cost = cost, FirstPurchases = work.UniqueCount - before };
        }
        public bool CanCommitBuyAll(CashBatchQuote quote) => quote != null && quote.Ids.Count > 0 &&
            Same(quote.owner, quote.session, quote.run, quote.command) && State.cash >= quote.Cost;
        public bool CommitBuyAll(CashBatchQuote quote)
        {
            if (!CanCommitBuyAll(quote)) return false;
            return Commit(s => {
                var f = new RunEffects(Data, s);
                for (int n = 0; n < quote.Ids.Count; n++)
                {
                    if (!Data.TryUpgrade(quote.Ids[n], out int i)) return false;
                    var u = Data.Upgrades[i]; var cost = UpgradeCost(u, s, f);
                    if (!quote.scope.Matches(u) || !Eligible(u, s, f) || cost.CompareTo(quote.Prices[n]) != 0 || cost > s.cash) return false;
                    ApplyPurchase(u, cost, s, f);
                }
                return true;
            });
        }
        Amount BaseTapPayout(bool hands, RedesignState s, RunEffects f)
        {
            Amount b = new Amount(EconomyBalance.TapBase) * (1 + 1.4 * s.tapLevel) * EconomyBalance.MilestoneMultiplier(s.tapLevel) * f.tapDirect * Global(s, f);
            Amount value = b * EconomyBalance.TapFameScale(s.fame) * ProgressionNumbers.CardFactor(CardEffect.TapProfit, s.cardLevels[16]);
#if REDESIGN_LAB
            if (hands) value *= EconomyBalance.HandsPayoutScale;
#endif
            return value;
        }
        public Amount TapPayout(bool hands = false) => BaseTapPayout(hands, State, Effects) * (State.activeUntil[0] > State.now ? 1.25 : 1);
        static void Earn(RedesignState s, Amount amount) { s.cash += amount; s.eligibleEarnings += amount; s.lifetimeEarnings += amount; }
        public bool Tap()
        {
            if (!MutationAllowed()) return false;
            Earn(State, TapPayout()); State.manualTaps += (Natural)1;
            State.revision += (Natural)1;
            return true;
        }
        public bool SkillOwned(int skill) => skill >= 0 && skill < 3 && Effects.skillPower[skill] > 0;
        static double Duration(int skill, RedesignState s, RunEffects f) => f.skillPower[skill] == 0 || skill == 2 ? 0 : 30 * (1 + f.skillDuration[skill] / 16d) * ProgressionNumbers.CardFactor(CardEffect.SkillDuration, s.cardLevels[21 + skill]);
        static double Cooldown(int skill, RedesignState s, RunEffects f) => f.skillPower[skill] == 0 ? 0 : (skill == 2 ? 600 : 300) * (1 - f.skillCooldown[skill] / (skill == 2 ? 48d : 32d)) * ProgressionNumbers.CardFactor(CardEffect.SkillCooldown, s.cardLevels[18 + skill]);
        public double SkillDuration(int skill) => !SkillOwned(skill) ? 0 : Duration(skill, State, Effects);
        public double SkillCooldown(int skill) => !SkillOwned(skill) ? 0 : Cooldown(skill, State, Effects);
        public bool ActivateSkill(int skill)
        {
            if (!SkillOwned(skill) || State.now < State.readyAt[skill]) return false;
            double duration = SkillDuration(skill), cooldown = SkillCooldown(skill);
            return Commit(s => {
                s.readyAt[skill] = s.now + cooldown;
                if (skill == 2) Earn(s, BulkOrderPayout(s, Effects));
                else { s.activeUntil[skill] = s.now + duration; if (skill == 1) s.nextHandsAt[1] = s.now + 1 / RedesignCatalog.SkillPower[1][Effects.skillPower[1] - 1]; }
                return true;
            });
        }
        public void Advance(double seconds)
        {
            if (!MutationAllowed() || seconds <= 0) return;
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || State.now + seconds >= 1e12) throw new ArgumentOutOfRangeException(nameof(seconds));
            double end = State.now + seconds;
            // Ownership, levels and cards cannot change within this accrual call.
            // Reuse their rates across the independent Helping Hands events;
            // temporary Rush multipliers still change at their exact boundaries.
            Amount normalRate = NormalIncome(), handsBase = BaseTapPayout(true, State, Effects);
            // Split exactly at temporary effect expiries and independent Helping Hands events.
            while (State.now < end)
            {
                double next = end;
                for (int i = 0; i < 2; i++) if (State.activeUntil[i] > State.now) next = Math.Min(next, State.activeUntil[i]);
                bool hands = SkillOwned(1) && State.activeUntil[1] > State.now;
                if (hands) next = Math.Min(next, Math.Max(State.now, State.nextHandsAt[1]));
                double dt = next - State.now;
                double rush = State.activeUntil[0] > State.now && SkillOwned(0) ? RedesignCatalog.SkillPower[0][Effects.skillPower[0] - 1] : 1;
                Earn(State, normalRate * rush * dt); State.now = next; State.runSeconds += dt;
                double epsilon = Math.Max(1e-9, Math.Abs(State.now) * 1e-15);
                if (hands && State.nextHandsAt[1] <= State.now + epsilon && State.now <= State.activeUntil[1] + epsilon)
                {
                    Earn(State, handsBase * (State.activeUntil[0] > State.now ? 1.25 : 1));
                    State.nextHandsAt[1] = State.now + 1 / RedesignCatalog.SkillPower[1][Effects.skillPower[1] - 1];
                }
            }
            State.revision += (Natural)1;
        }
        public bool Save() => Commit(_ => true, false);
        public bool ResolveOffline(double utc)
        {
            if (double.IsNaN(utc) || double.IsInfinity(utc) || utc < 0 || utc >= 1e12) throw new ArgumentOutOfRangeException(nameof(utc));
            if (utc <= Math.Max(State.now, State.savedAt)) return false;
            return Commit(s => {
                double absence = utc - Math.Max(s.now, s.savedAt);
                double efficiency = ProgressionNumbers.CardFactor(CardEffect.OfflineEfficiency, s.cardLevels[24]);
                double credited = Math.Min(absence, 3600 * ProgressionNumbers.CardFactor(CardEffect.OfflineHours, s.cardLevels[25]));
                Amount amount = s.departureRate * efficiency * credited;
                Earn(s, amount); s.now = utc; s.savedAt = utc;
                s.activeUntil = new double[3]; s.nextHandsAt = new double[3];
                s.offline = new OfflineReceipt { id = Guid.NewGuid().ToString("N"), absence = absence, creditedSeconds = credited, efficiency = efficiency, amount = amount, pending = amount > Amount.Zero };
                return true;
            }, false);
        }
        public bool DismissOffline() => State.offline.pending && Commit(s => { s.offline.pending = false; return true; }, false);
        public FameQuote PreviewPrestige()
        {
            ProgressionNumbers.PrestigeRewards(State.eligibleEarnings, State.target, State.completedPrestiges, State.UniqueCount, out var fame, out var credits);
            int catalogue = !State.completedPrestiges.IsZero && State.eligibleEarnings >= State.target * 4 ? State.UniqueCount / 250 : 0;
            Natural nextFame = fame + (Natural)1;
            Amount nextDraw = State.target;
            if (State.eligibleEarnings >= State.target)
            {
                int exponent = (int)(credits.Value - catalogue - 1);
                if (!State.completedPrestiges.IsZero) nextDraw = State.target * Amount.Pow10((exponent + 1) * Math.Log10(4));
            }
            if (State.completedPrestiges.IsZero) nextDraw = Amount.Zero; // First reward stays exactly three.
            return new FameQuote { owner = owner, session = SessionKey, run = State.runId, command = State.commandRevision.digits, Fame = fame, Credits = credits, CurrentTarget = State.target, NextTarget = ProgressionNumbers.RunTarget(State.completedPrestiges + (Natural)1, State.fame + fame), ResultingIncomeMultiplier = ProgressionNumbers.FameMultiplier(State.fame + fame), ResultingTapIncomeMultiplier = ProgressionNumbers.TapFameMultiplier(State.fame + fame),
                NextFameEarnings = State.target * nextFame.ToAmount() * nextFame.ToAmount(), NextDrawEarnings = nextDraw, RunCredits = credits - (Natural)catalogue, CatalogueCredits = catalogue, NextUniqueMilestone = State.UniqueCount >= 1000 ? 0 : (State.UniqueCount / 250 + 1) * 250 };
        }
        public bool CanCommitPrestige(FameQuote quote)
        {
            if (quote == null || !quote.Eligible || !Same(quote.owner, quote.session, quote.run, quote.command)) return false;
            ProgressionNumbers.PrestigeRewards(State.eligibleEarnings, State.target, State.completedPrestiges, State.UniqueCount, out var fame, out var credits);
            return fame.CompareTo(quote.Fame) >= 0 && credits.CompareTo(quote.Credits) >= 0;
        }
        public bool Prestige(FameQuote quote)
        {
            if (!CanCommitPrestige(quote)) return false;
            // Passive earnings can improve a reviewed reward. Never award less than the preview.
            ProgressionNumbers.PrestigeRewards(State.eligibleEarnings, State.target, State.completedPrestiges, State.UniqueCount, out var fame, out var credits);
            return Commit(s => {
                s.fame += fame; s.freeCredits += credits; s.completedPrestiges += (Natural)1;
                if (s.completedPrestiges.Value == 2 && !s.secondPrestigeGift) { if (!s.HasPremium) s.kajuKatli += (Natural)100; s.secondPrestigeGift = true; }
                s.runId = Guid.NewGuid().ToString("N"); s.products = new int[8]; s.tapLevel = 0; s.upgrades.Clear();
                s.activeUntil = new double[3]; s.readyAt = new double[3]; s.nextHandsAt = new double[3]; s.nextManualTapAt = 0;
                s.eligibleEarnings = 0; s.runSeconds = 0; s.departureRate = 0; s.target = ProgressionNumbers.RunTarget(s.completedPrestiges, s.fame);
                s.cash = new Amount(30) * ProgressionNumbers.CardFactor(CardEffect.ProductCost, s.cardLevels[8]);
                s.offline = OfflineReceipt.Empty();
                return true;
            });
        }
        public bool CanDraw(bool useCredit)
        {
            return MutationAllowed() && !State.requiresDrawReconciliation && !State.reveal.pending && !(State.HasPremium && State.premium.pendingOperationId != "") &&
                (useCredit ? !State.freeCredits.IsZero : !State.HasPremium && PremiumSpendAllowed() && State.kajuKatli.Value >= 100);
        }
        public TalentDrawQuote QuoteDraw(bool useCredit)
        {
            if (!CanDraw(useCredit)) return null;
            return new TalentDrawQuote { owner = owner, session = SessionKey, run = State.runId, command = State.commandRevision.digits, OperationId = Guid.NewGuid().ToString("N"), UseCredit = useCredit };
        }
        public bool CanCommitDraw(TalentDrawQuote quote) => quote != null && Same(quote.owner, quote.session, quote.run, quote.command) && CanDraw(quote.UseCredit);
        public bool Draw(TalentDrawQuote quote)
        {
            if (!CanCommitDraw(quote)) return false;
            if (quote.selectedCard < 0)
            {
                quote.selectedCard = TalentDrawPool.Select(Data, random);
            }
            int card = quote.selectedCard;
            return Commit(s => {
                if (quote.UseCredit) s.freeCredits -= (Natural)1; else s.kajuKatli -= (Natural)100;
                Natural before = s.cardLevels[card]; s.cardLevels[card] += (Natural)1;
                if (!quote.UseCredit && s.secondPrestigeGift && s.HasGuestProvenance && !s.HasDevelopmentShop) { s.guestWallet.giftDrawCardId = Data.Cards[card].id; s.guestWallet.giftDrawOperationId = quote.OperationId; }
                s.reveal = new DrawReveal { operationId = quote.OperationId, cardId = Data.Cards[card].id, payment = quote.UseCredit ? "credit" : "kk", before = before, after = s.cardLevels[card], pending = true };
                return true;
            });
        }
        public bool DismissReveal(string operationId) => State.reveal.pending && State.reveal.operationId == operationId && Commit(s => { s.reveal.pending = false; return true; });
        public bool LearnTopic(string id) => !string.IsNullOrWhiteSpace(id) && id.Length <= 80 && State.learnedTopics.Count < 256 && !State.learnedTopics.Contains(id) && Commit(s => { s.learnedTopics.Add(id); return true; }, false);
    }
}
