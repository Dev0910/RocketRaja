using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RocketRaja.Core
{
    public enum CashEffect { ProductProfit, ProductCost, TapDirect, TapLinked, TapCost, GlobalProfit, UpgradeCost, SkillPower, SkillDuration, SkillCooldown, Link, Group }
    public enum CardTier { Common, Rare, Legendary }
    public enum CardEffect { ProductProfit, ProductCost, TapProfit, TapCost, SkillCooldown, SkillDuration, OrderPayout, OfflineEfficiency, OfflineHours, GlobalProfit, UpgradeCost }
    [Serializable] public sealed class ProductRequirement { public int product, level; }
    [Serializable] public sealed class CashUpgrade
    {
        public string id, name, priceKey, basePrice, effectText, requirementText;
        public CashEffect effect;
        public int band, position, rank, target = -1, beneficiary = -1, requiredFamilies;
        public bool signature;
        public double factor;
        public int[] group = Array.Empty<int>();
        public string[] prerequisites = Array.Empty<string>();
        public ProductRequirement[] productRequirements = Array.Empty<ProductRequirement>();
        public Amount price;
    }
    [Serializable] public sealed class TalentCard
    {
        public string id, name;
        public CardTier tier;
        public CardEffect effect;
        public int target = -1;
    }
    [Serializable] public sealed class PremiumPack
    {
        public string id, name;
        public int kajuKatli, designInr;
    }
    [Serializable] public sealed class RedesignContent
    {
        public string economyVersion = RedesignCatalog.EconomyVersion;
        public string contentVersion = RedesignCatalog.ContentVersion;
        public string sourceHash;
        public ProductDef[] products;
        public CashUpgrade[] upgrades;
        public TalentCard[] cards;
        public PremiumPack[] packs;
    }

    /// <summary>Validated and indexed once per loaded content asset; no runtime Markdown or fallback catalogue.</summary>
    public sealed class RedesignCatalog
    {
        public const string EconomyVersion = "3.0.0", ContentVersion = "faster-start-v1";
        public static readonly string[] ProductKeys = { "PHU", "ANA", "CHA", "ROC", "SMC", "STR", "AER", "BOX" };
        public static readonly string[] SkillKeys = { "FR", "HH", "BO" };
        public static readonly double[][] SkillPower = {
            new[] { 2d, 2.25, 2.5, 2.75, 3, 3.25, 3.5, 4 },
            new[] { 3d, 4, 5, 6, 8, 10, 12, 16 },
            new[] { 120d, 135, 150, 165, 180, 195, 210, 225, 240, 260, 280, 300 }
        };
        public readonly ReadOnlyCollection<ProductDef> Products;
        public readonly ReadOnlyCollection<CashUpgrade> Upgrades;
        public readonly ReadOnlyCollection<TalentCard> Cards;
        public readonly ReadOnlyCollection<PremiumPack> Packs;
        public readonly string SourceHash;
        readonly Dictionary<string, int> upgradeIndex, cardIndex;
        readonly int[][] tiers;
        readonly string[][] appearanceSignatures;
        public RedesignCatalog(RedesignContent content)
        {
            Validate(content);
            Products = Array.AsReadOnly(content.products);
            Upgrades = Array.AsReadOnly(content.upgrades);
            Cards = Array.AsReadOnly(content.cards);
            Packs = Array.AsReadOnly(content.packs);
            SourceHash = content.sourceHash;
            upgradeIndex = content.upgrades.Select((u, i) => new { u.id, i }).ToDictionary(x => x.id, x => x.i, StringComparer.Ordinal);
            cardIndex = content.cards.Select((u, i) => new { u.id, i }).ToDictionary(x => x.id, x => x.i, StringComparer.Ordinal);
            tiers = Enumerable.Range(0, 3).Select(t => content.cards.Select((c, i) => new { c, i }).Where(x => (int)x.c.tier == t).Select(x => x.i).ToArray()).ToArray();
            appearanceSignatures = Enumerable.Range(0, 8).Select(p => content.upgrades.Where(u => u.signature && u.target == p).OrderBy(u => u.rank).Select(u => u.id).ToArray()).ToArray();
        }
        public string AppearanceUpgrade(int product, int ownedTier)
        {
            if (product < 0 || product >= 8 || ownedTier < 1 || ownedTier > 3) throw new ArgumentOutOfRangeException();
            return appearanceSignatures[product][ownedTier == 1 ? 0 : ownedTier == 2 ? 3 : 9];
        }
        public bool TryUpgrade(string id, out int index) { index = -1; return id != null && upgradeIndex.TryGetValue(id, out index); }
        public bool TryCard(string id, out int index) { index = -1; return id != null && cardIndex.TryGetValue(id, out index); }
        public int TierCount(CardTier tier) => tiers[(int)tier].Length;
        public int CardInTier(CardTier tier, int index) => tiers[(int)tier][index];
        public static void Validate(RedesignContent c)
        {
            void Require(bool ok, string detail) { if (!ok) throw new InvalidDataException("Invalid redesign content: " + detail); }
            Require(c != null, "missing asset");
            Require(c.economyVersion == EconomyVersion && c.contentVersion == ContentVersion, "unsupported economy/content version");
            Require(c.sourceHash != null && c.sourceHash.Length == 64 && c.sourceHash.All(Uri.IsHexDigit), "source digest");
            Require(c.products != null && c.products.Length == 8, "eight products");
            for (int i = 0; i < 8; i++) Require(c.products[i] != null && c.products[i].id == Catalog.ProductIds[i] && c.products[i].cost == EconomyBalance.ProductCost(i) && c.products[i].income == EconomyBalance.ProductIncome(i), "product mapping/formula " + i);
            Require(c.upgrades != null && c.upgrades.Length == 1000 && c.upgrades.All(x => x != null && !string.IsNullOrWhiteSpace(x.id) && !string.IsNullOrWhiteSpace(x.name)), "1,000 named cash entries");
            Require(c.upgrades.Select(x => x.id).Distinct(StringComparer.Ordinal).Count() == 1000, "unique IDs");
            int[] expected = { 528, 128, 120, 0, 40, 80, 32, 28, 16, 28, 0, 0 };
            for (int i = 0; i < expected.Length; i++) Require(c.upgrades.Count(x => (int)x.effect == i) == expected[i], "effect count " + (CashEffect)i);
            Require(c.upgrades.Count(x => x.signature) == 80 && c.upgrades.All(x => !x.signature || x.effect == CashEffect.ProductProfit), "80 product signatures");
            for (int p = 0; p < 8; p++) Require(c.upgrades.Count(x => x.signature && x.target == p) == 10, "ten signatures per product " + p);
            Require(c.upgrades.Count(x => x.effect == CashEffect.SkillPower && x.rank == 1) == 3, "three base skill unlocks");
            var ids = c.upgrades.ToDictionary(x => x.id, StringComparer.Ordinal);
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var u in c.upgrades)
            {
                Require(u.price.Valid && u.price > Amount.Zero && double.TryParse(u.basePrice, NumberStyles.Float, CultureInfo.InvariantCulture, out var p) && !double.IsInfinity(p) && p > 0 && Math.Abs(Amount.Ratio(u.price, new Amount(p)) - 1) < 1e-14, "positive authored price " + u.id);
                Require(u.band >= 1 && u.band <= 20 && u.position >= 1 && keys.Add(u.priceKey) && u.priceKey == $"B{u.band:00}-{u.position:000}", "price key " + u.id);
                Require(u.rank > 0 && u.requiredFamilies >= 0 && u.requiredFamilies <= 8 && !double.IsNaN(u.factor) && !double.IsInfinity(u.factor) && u.factor > 0, "rank/effect " + u.id);
                Require(u.prerequisites != null && u.prerequisites.Distinct().Count() == u.prerequisites.Length && u.prerequisites.All(x => x != u.id && ids.ContainsKey(x)), "prerequisite " + u.id);
                Require(u.productRequirements != null && u.productRequirements.All(x => x != null && x.product >= 0 && x.product < 8 && x.level > 0) && u.productRequirements.Select(x => x.product).Distinct().Count() == u.productRequirements.Length, "product requirements " + u.id);
                Require(u.group != null && u.group.Distinct().Count() == u.group.Length && u.group.All(x => x >= 0 && x < 8), "group " + u.id);
                if (u.effect == CashEffect.ProductProfit || u.effect == CashEffect.ProductCost || u.effect == CashEffect.Link) Require(u.target >= 0 && u.target < 8, "product target " + u.id);
                if (u.effect == CashEffect.Link) Require(u.beneficiary == (u.target + 1) % 8, "link beneficiary " + u.id);
                if (u.effect == CashEffect.Group) Require(u.group.Length >= 2 && u.productRequirements.Length == u.group.Length, "group membership " + u.id);
                if (u.effect >= CashEffect.SkillPower && u.effect <= CashEffect.SkillCooldown)
                    {
                    Require(u.target >= 0 && u.target < 3 && (u.effect != CashEffect.SkillDuration || u.target != 2), "skill target " + u.id);
                    int maximum = u.effect == CashEffect.SkillDuration ? 8 : SkillPower[u.target].Length;
                    Require(u.rank <= maximum, "skill rank " + u.id);
                    double factor = u.effect == CashEffect.SkillPower ? SkillPower[u.target][u.rank - 1] : u.effect == CashEffect.SkillDuration ? 1 + u.rank / 16d : 1 - u.rank / (u.target == 2 ? 48d : 32d);
                    Require(Math.Abs(u.factor - factor) < 1e-12, "authored skill effect matches timing " + u.id);
                }
            }
            var visiting = new HashSet<string>(); var visited = new HashSet<string>();
            void Visit(string id)
            {
                if (visited.Contains(id)) return;
                Require(visiting.Add(id), "cyclic prerequisite " + id);
                foreach (string dependency in ids[id].prerequisites) Visit(dependency);
                visiting.Remove(id); visited.Add(id);
            }
            foreach (string id in ids.Keys) Visit(id);
            var priceOrder = c.upgrades.OrderBy(x => x.band).ThenBy(x => x.position).ToArray();
            for (int i = 0; i < priceOrder.Length; i++)
                Require(priceOrder[i].band == i / 50 + 1 && priceOrder[i].position == i % 50 + 1 && Math.Abs(Amount.Ratio(priceOrder[i].price, EconomyBalance.UpgradePrice(i)) - 1) < 1e-12, "continuous exponential price " + priceOrder[i].id);
            for (int p = 0; p < 8; p++) Require(c.upgrades.Count(x => x.effect == CashEffect.ProductProfit && x.target == p) == 66, "66 profit upgrades per family");
            Require(c.cards != null && c.cards.Length == 28 && c.cards.All(x => x != null && !string.IsNullOrWhiteSpace(x.id) && !string.IsNullOrWhiteSpace(x.name)) && c.cards.Select(x => x.id).Distinct().Count() == 28, "28 distinct cards");
            Require(c.cards.Count(x => x.tier == CardTier.Common) == 16 && c.cards.Count(x => x.tier == CardTier.Rare) == 10 && c.cards.Count(x => x.tier == CardTier.Legendary) == 2, "16/10/2 tiers");
            foreach (var card in c.cards)
            {
                Require(Enum.IsDefined(typeof(CardEffect), card.effect), "card effect " + card.id);
                if (card.effect == CardEffect.ProductProfit || card.effect == CardEffect.ProductCost) Require(card.target >= 0 && card.target < 8, "card product " + card.id);
                if (card.effect == CardEffect.SkillCooldown || card.effect == CardEffect.SkillDuration) Require(card.target >= 0 && card.target < (card.effect == CardEffect.SkillDuration ? 2 : 3), "card skill " + card.id);
            }
            Require(c.packs != null && c.packs.Length == 4 && c.packs.All(x => x != null && !string.IsNullOrEmpty(x.id)) && c.packs.Select(x => x.id).Distinct().Count() == 4, "premium pack IDs");
            Require(c.packs.Select(x => x.kajuKatli).SequenceEqual(new[] { 1000, 6000, 13000, 28000 }) && c.packs.Select(x => x.designInr).SequenceEqual(new[] { 99, 499, 999, 1999 }), "premium amounts/prices");
        }
    }
}
