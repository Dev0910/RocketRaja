using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketRaja.Core
{
    public enum Effect { ProductProfit, ProductDiscount, TapProfit, AllProfit, Skill }
    [Serializable] public class SkillDef
    {
        public string id, name;
        public double cooldown, duration;
        public double[] values;
    }
    [Serializable] public class LegacyDef
    {
        public string id, name;
        public int maxRank = 5;
        public long initialCost;
        public double valuePerRank;
    }
    [Serializable] public class ProductDef
    {
        public string id, name, subtitle, color;
        public double cost, income;
    }
    [Serializable] public class UpgradeDef
    {
        public string id, name;
        public Effect effect;
        public int target, tier, requiredLevel;
        public double multiplier;
        public Amount price;
        public string Description(Catalog catalog)
        {
            var products = catalog.products;
            switch (effect)
            {
                case Effect.ProductProfit: return "x" + multiplier + " " + products[target].name + " income";
                case Effect.ProductDiscount: return "20% lower " + products[target].name + " level prices";
                case Effect.TapProfit: return "x" + multiplier + " earnings per tap";
                case Effect.AllProfit: return "x" + multiplier + " all product income";
                default:
                    var skill = catalog.skills[target]; double value = skill.values[tier];
                    return (target == 0 ? "x" + value + " sales for " + skill.duration + "s" : target == 1 ? value + " auto taps/s for " + skill.duration + "s" : value / 60 + " minutes normal sales") + "; " + skill.cooldown / 60 + "m cooldown";
            }
        }
    }
    [Serializable] public class Catalog
    {
        public const string EconomyVersion = "1.0.0";
        public const int MaxLevel = 100000;
        public const double Growth = 1.12;
        public const double PrestigeThreshold = 1.7e17;
        public ProductDef[] products;
        public UpgradeDef[] upgrades;
        public SkillDef[] skills;
        public LegacyDef[] legacy;
        public static readonly string[] SkillNames = { "Festival Rush", "Helping Hands", "Bulk Order" };
        public static readonly int[] VisualLevels = { 1, 25, 50, 100 };
        public static readonly string[] ProductIds = { "phuljhari", "anar", "chakri", "rockets", "small-crackers", "cracker-strings", "aerial-shots", "gift-boxes" };
        public static Catalog Create()
        {
            var c = new Catalog();
            c.skills = new[] {
                new SkillDef { id = "skill-0", name = SkillNames[0], cooldown = 300, duration = 30, values = new[] { 2d, 2.5, 3, 4 } },
                new SkillDef { id = "skill-1", name = SkillNames[1], cooldown = 300, duration = 30, values = new[] { 5d, 8, 12, 16 } },
                new SkillDef { id = "skill-2", name = SkillNames[2], cooldown = 600, duration = 0, values = new[] { 120d, 180, 240, 300 } }
            };
            string[] names = { "Phuljhari", "Anar", "Chakri", "Rockets", "Small crackers", "Cracker strings", "Aerial shots", "Celebration boxes" };
            string[] descriptions = { "A little spark of celebration", "Fountains of festive colour", "A whirl of bright possibilities", "Send your ambitions skyward", "Little packs. Big excitement.", "A string of happy moments", "Paint the evening sky", "A whole festival in a box" };
            string[] colors = { "F3B544", "E66E45", "59AFA1", "789ADB", "D97491", "C594E3", "65B7CD", "E6BE6E" };
            c.products = Enumerable.Range(0, 8).Select(i => new ProductDef { id = ProductIds[i], name = names[i], subtitle = descriptions[i], color = colors[i], cost = 120 * Math.Pow(8, i), income = 2.8 * Math.Pow(8, i) }).ToArray();
            c.legacy = Enumerable.Range(0, 12).Select(i => new LegacyDef {
                id = i < 8 ? "legacy-" + ProductIds[i] : new[] { "legacy-tap", "legacy-all", "legacy-offline", "legacy-cooldown" }[i - 8],
                name = i < 8 ? names[i] + " expertise" : new[] { "Natural salesperson", "Trusted festival brand", "After-hours business", "Experienced team" }[i - 8],
                initialCost = i == 9 ? 3 : 1, valuePerRank = i < 10 ? 2 : i == 10 ? 2 : .05
            }).ToArray();
            var list = new List<UpgradeDef>();
            int[] levels = { 10, 25, 50, 100, 175, 250 };
            string[] titles = { "Gift Packaging", "Festival Display", "Regular Customers", "Signature Collection", "Neighbourhood Favourite", "Celebration Specialist" };
            for (int p = 0; p < 8; p++)
            {
                for (int t = 0; t < 6; t++) list.Add(new UpgradeDef { id = "profit-" + ProductIds[p] + "-" + t, name = titles[t], target = p, tier = t, requiredLevel = levels[t], effect = Effect.ProductProfit, multiplier = t < 3 ? 2 : 3, price = new Amount(c.products[p].cost) * Amount.Pow10(Math.Log10(Catalog.Growth) * levels[t]) * 5 });
                for (int t = 0; t < 2; t++) list.Add(new UpgradeDef { id = "discount-" + ProductIds[p] + "-" + t, name = t == 0 ? "Wholesale Deal" : "Trusted Supplier", target = p, tier = t, requiredLevel = t == 0 ? 40 : 150, effect = Effect.ProductDiscount, multiplier = .8, price = new Amount(c.products[p].cost) * Amount.Pow10(t == 0 ? 2.8 : 7.8) });
            }
            for (int t = 0; t < 12; t++) list.Add(new UpgradeDef { id = "tap-" + t, name = "Salesmanship " + (t + 1), effect = Effect.TapProfit, tier = t, requiredLevel = t * 5, multiplier = 2, price = new Amount(900) * Amount.Pow10(t * .82) });
            for (int t = 0; t < 8; t++) list.Add(new UpgradeDef { id = "all-" + t, name = "Festival Footfall " + (t + 1), effect = Effect.AllProfit, target = t, tier = t, requiredLevel = 1, multiplier = 2, price = new Amount(c.products[t].cost) * 30 });
            for (int s = 0; s < 3; s++) for (int t = 0; t < 4; t++) list.Add(new UpgradeDef { id = "skill-" + s + "-" + t, name = SkillNames[s] + (t == 0 ? "" : " " + (t + 1)), effect = Effect.Skill, target = s, tier = t, price = new Amount(2500 * Math.Pow(3, s)) * Amount.Pow10(t * 2) });
            c.upgrades = list.OrderBy(x => x.price).ThenBy(x => x.id, StringComparer.Ordinal).ToArray();
            return c;
        }
    }
}
