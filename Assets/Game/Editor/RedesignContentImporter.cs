using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RocketRaja.Core;

namespace RocketRaja.Editor
{
    // Pure authoring code shared with RedesignLab. Runtime reads only the validated serialized asset.
    public static class RedesignContentImporter
    {
        public static RedesignContent Import(string catalogue, string roster)
        {
            var content = new RedesignContent { sourceHash = SavePayload.Digest(catalogue.Replace("\r\n", "\n") + "\n" + roster.Replace("\r\n", "\n")) };
            // Preserve the eight established family names/presentation metadata without
            // constructing the retired 96-upgrade/Legacy economy as an intermediate step.
            string[] names = { "Phuljhari", "Anar", "Chakri", "Rockets", "Small crackers", "Cracker strings", "Aerial shots", "Celebration boxes" };
            string[] descriptions = { "A little spark of celebration", "Fountains of festive colour", "A whirl of bright possibilities", "Send your ambitions skyward", "Little packs. Big excitement.", "A string of happy moments", "Paint the evening sky", "A whole festival in a box" };
            string[] colors = { "F3B544", "E66E45", "59AFA1", "789ADB", "D97491", "C594E3", "65B7CD", "E6BE6E" };
            content.products = Enumerable.Range(0, 8).Select(i => new ProductDef { id = Catalog.ProductIds[i], name = names[i], subtitle = descriptions[i], color = colors[i], cost = 30 * Math.Pow(2, i), income = .85 * Math.Pow(1.6, i) }).ToArray();
            var rows = new List<CashUpgrade>();
            foreach (string line in catalogue.Split('\n').Where(x => Regex.IsMatch(x, @"^\| (PP|PC|TP|TC|GP|UC|SK|XL|PF)-")))
            {
                var cols = line.Split('|').Skip(1).Take(6).Select(x => x.Trim()).ToArray();
                if (cols.Length != 6) throw new InvalidDataException("Incomplete cash row");
                var match = Regex.Match(cols[2], @"^(\w+)\(([^)]+)\):");
                if (!match.Success) throw new InvalidDataException("Unknown effect " + cols[0]);
                string type = match.Groups[1].Value;
                string[] args = match.Groups[2].Value.Split(',');
                int rank = int.Parse(args.Last(), CultureInfo.InvariantCulture);
                var row = new CashUpgrade { id = cols[0], name = cols[1], effectText = cols[2], requirementText = cols[3], priceKey = cols[4], basePrice = cols[5], price = new Amount(double.Parse(cols[5], CultureInfo.InvariantCulture)), band = int.Parse(cols[4].Substring(1, 2)), position = int.Parse(cols[4].Substring(4)), rank = rank, signature = cols[1].Contains("Signature Breakthrough") };
                var reqs = new List<ProductRequirement>();
                void Product(int product, int level) { reqs.Add(new ProductRequirement { product = product, level = level }); }
                switch (type)
                {
                    case "PP": row.effect = CashEffect.ProductProfit; row.target = ProductIndex(args[0]); row.factor = row.signature ? rank <= 6 ? 2 : 8 : row.band == 1 ? 1.2 : 1.6; Product(row.target, 1); break;
                    case "PC": row.effect = CashEffect.ProductCost; row.target = ProductIndex(args[0]); row.factor = 1 - .35 * rank / 16; Product(row.target, 1); break;
                    case "TD": row.effect = CashEffect.TapDirect; row.factor = 1.2; break;
                    case "TL": row.effect = CashEffect.TapLinked; row.factor = .12 + .18 * (rank - 1) / 59; break;
                    case "TC": row.effect = CashEffect.TapCost; row.factor = 1 - .35 * rank / 40; break;
                    case "GP": row.effect = CashEffect.GlobalProfit; row.factor = 1.1; break;
                    case "UC": row.effect = CashEffect.UpgradeCost; row.factor = 1 - .35 * rank / 32; break;
                    case "SP": case "SD": case "SC":
                        row.target = Array.IndexOf(RedesignCatalog.SkillKeys, args[0]);
                        if (row.target < 0) throw new InvalidDataException("Unknown skill " + row.id);
                        row.effect = type == "SP" ? CashEffect.SkillPower : type == "SD" ? CashEffect.SkillDuration : CashEffect.SkillCooldown;
                        row.factor = type == "SP" ? RedesignCatalog.SkillPower[row.target][rank - 1] : type == "SD" ? 1 + rank / 16d : 1 - rank / (row.target == 2 ? 48d : 32d); break;
                    case "XL": row.effect = CashEffect.Link; row.target = ProductIndex(args[0]); row.beneficiary = ProductIndex(args[1]); row.factor = 1.5; Product(row.target, new[] { 25, 100, 200, 300 }[rank - 1]); Product(row.beneficiary, 1); break;
                    case "PF": row.effect = CashEffect.Group; row.factor = 1.3; row.group = cols[3].Split(' ')[0].Split('+').Select(ProductIndex).ToArray(); foreach (int p in row.group) Product(p, new[] { 25, 100, 200, 300 }[rank - 1]); break;
                    default: throw new InvalidDataException("Unsupported effect " + type);
                }
                var family = Regex.Match(cols[3], @"Own (\d)\+ families");
                if (family.Success) row.requiredFamilies = int.Parse(family.Groups[1].Value);
                if ((type == "PP" || type == "PC") && !cols[3].Contains("Own " + content.products[row.target].name)) throw new InvalidDataException("Missing named family requirement " + row.id);
                for (int i = 0; i < content.products.Length; i++)
                    if (cols[3].Contains("Own " + content.products[i].name) && !reqs.Any(r => r.product == i)) Product(i, 1);
                if (type == "XL" || type == "PF")
                {
                    var level = Regex.Match(cols[3], @"level(?:s)? >= (\d+)");
                    if (!level.Success) throw new InvalidDataException("Missing explicit level gate " + row.id);
                    int required = int.Parse(level.Groups[1].Value, CultureInfo.InvariantCulture);
                    foreach (var requirement in reqs) if (type == "PF" || requirement.product == row.target) requirement.level = required;
                }
                row.prerequisites = Regex.Matches(cols[3], @"\b(?:PP|PC|TP|TC|GP|UC|SK|XL|PF)-[A-Z0-9-]+\b").Cast<Match>().Select(x => x.Value).ToArray();
                row.productRequirements = reqs.ToArray();
                // Reject newly authored conditions until their semantics are deliberately supported.
                string unexplained = cols[3];
                foreach (string dep in row.prerequisites) unexplained = unexplained.Replace(dep, "");
                unexplained = Regex.Replace(unexplained.Replace(";", " "), @"\s+", " ").Trim();
                unexplained = Regex.Replace(unexplained, @"Own [1-8]\+ families|Own both families|Source level >= (25|100|200|300)|[A-Z+]+ levels >= (25|100|200|300) \(every listed family\)", "");
                foreach (var p in content.products) unexplained = unexplained.Replace("Own " + p.name, "");
                if (unexplained.Replace(";", "").Trim().Length != 0) throw new InvalidDataException("Unparsed requirement " + row.id + ": " + unexplained);
                rows.Add(row);
            }
            content.upgrades = rows.ToArray();
            var cards = new List<TalentCard>();
            foreach (string line in roster.Split('\n').Where(x => Regex.IsMatch(x, @"^\| [CRL]\d\d \|")))
            {
                string[] col = line.Split('|').Skip(1).Select(x => x.Trim()).ToArray();
                int index = cards.Count;
                var card = new TalentCard { id = col[0], name = col[2], tier = (CardTier)Enum.Parse(typeof(CardTier), col[1]) };
                if (index < 8) { card.effect = CardEffect.ProductProfit; card.target = index; }
                else if (index < 16) { card.effect = CardEffect.ProductCost; card.target = index - 8; }
                else switch (index)
                {
                    case 16: card.effect = CardEffect.TapProfit; break;
                    case 17: card.effect = CardEffect.TapCost; break;
                    case 18: case 19: case 20: card.effect = CardEffect.SkillCooldown; card.target = index - 18; break;
                    case 21: case 22: card.effect = CardEffect.SkillDuration; card.target = index - 21; break;
                    case 23: card.effect = CardEffect.OrderPayout; card.target = 2; break;
                    case 24: card.effect = CardEffect.OfflineEfficiency; break;
                    case 25: card.effect = CardEffect.OfflineHours; break;
                    case 26: card.effect = CardEffect.GlobalProfit; break;
                    case 27: card.effect = CardEffect.UpgradeCost; break;
                    default: throw new InvalidDataException("Unexpected card " + card.id);
                }
                string expectedId = index < 16 ? "C" + (index + 1).ToString("00") : index < 26 ? "R" + (index - 15).ToString("00") : "L" + (index - 25).ToString("00");
                if (card.id != expectedId) throw new InvalidDataException("Card roster order/ID changed " + card.id);
                cards.Add(card);
            }
            content.cards = cards.ToArray();
            content.packs = new[] {
                new PremiumPack { id = "rr.kk.small.1000", name = "Small box", kajuKatli = 1000, designInr = 99 },
                new PremiumPack { id = "rr.kk.festival.6000", name = "Festival box", kajuKatli = 6000, designInr = 499 },
                new PremiumPack { id = "rr.kk.celebration.13000", name = "Celebration box", kajuKatli = 13000, designInr = 999 },
                new PremiumPack { id = "rr.kk.grand.28000", name = "Grand celebration box", kajuKatli = 28000, designInr = 1999 }
            };
            RedesignCatalog.Validate(content);
            return content;
        }
        static int ProductIndex(string key)
        {
            int index = Array.IndexOf(RedesignCatalog.ProductKeys, key);
            if (index < 0) throw new InvalidDataException("Unknown product key " + key);
            return index;
        }
    }
}
