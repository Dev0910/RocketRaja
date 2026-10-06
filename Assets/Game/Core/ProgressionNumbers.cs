using System;
using System.Globalization;
using System.Numerics;

namespace RocketRaja.Core
{
    /// <summary>Exact unbounded nonnegative counters serialized as decimal strings by Unity and JSON.</summary>
    [Serializable] public struct Natural : IComparable<Natural>
    {
        public string digits;
        public Natural(BigInteger value)
        {
            if (value.Sign < 0) throw new ArgumentOutOfRangeException(nameof(value));
            digits = value.ToString(CultureInfo.InvariantCulture);
        }
        public bool Valid
        {
            get
            {
                if (string.IsNullOrEmpty(digits) || digits.Length > 1 && digits[0] == '0') return false;
                foreach (char c in digits) if (c < '0' || c > '9') return false;
                return true;
            }
        }
        public BigInteger Value => Valid ? BigInteger.Parse(digits, CultureInfo.InvariantCulture) : throw new FormatException("Invalid exact progression counter");
        public static Natural Zero => new Natural(BigInteger.Zero);
        public static implicit operator Natural(int value) => new Natural(value);
        public static implicit operator Natural(long value) => new Natural(value);
        public static Natural operator +(Natural a, Natural b) => new Natural(a.Value + b.Value);
        public static Natural operator -(Natural a, Natural b) => new Natural(a.Value - b.Value);
        public int CompareTo(Natural other) => Value.CompareTo(other.Value);
        public bool IsZero => Value.IsZero;
        public override string ToString() => digits;
        public Amount ToAmount()
        {
            if (!Valid) throw new FormatException("Invalid exact progression counter");
            if (IsZero) return Amount.Zero;
            int n = Math.Min(16, digits.Length);
            double head = double.Parse(digits.Substring(0, n), CultureInfo.InvariantCulture);
            return new Amount(head, digits.Length - n);
        }
    }

    public static class ProgressionNumbers
    {
        public static Amount Sqrt(Amount value)
        {
            if (!value.Valid) throw new ArgumentOutOfRangeException(nameof(value));
            if (value.mantissa == 0) return Amount.Zero;
            int even = value.exponent % 2 == 0 ? value.exponent : value.exponent - 1;
            return new Amount(Math.Sqrt(value.mantissa * (value.exponent == even ? 1 : 10)), even / 2);
        }
        public static Amount FameMultiplier(Natural fame) => fame.IsZero ? new Amount(1) : new Amount(3) + Sqrt(fame.ToAmount()) * 3;
        public static Amount RunTarget(Natural completedPrestiges)
        {
            // The authored target cap is reached by N=148; compare exactly before a bounded conversion.
            if (completedPrestiges.Value >= 148) return new Amount(1, 36);
            var target = new Amount(1, 10) * Amount.Pow10((int)completedPrestiges.Value * Math.Log10(1.5));
            return target > new Amount(1, 36) ? new Amount(1, 36) : target;
        }
        public static void PrestigeRewards(Amount earned, Amount target, Natural prestiges, int unique, out Natural fame, out Natural credits)
        {
            fame = credits = Natural.Zero;
            if (!earned.Valid || !target.Valid || target.mantissa == 0 || unique < 0 || unique > 1000) throw new ArgumentOutOfRangeException();
            if (earned < target) return;
            Ratio(earned, target, out var numerator, out var denominator);
            // Amount multiplication can put an authored integer threshold one ULP below
            // its exact decimal ratio. Recognize the very same displayed threshold value,
            // without an epsilon that could reward a genuinely smaller representable amount.
            // Above the exact-integer range of double, retain the arbitrary-precision ratio.
            BigInteger integerRatio = numerator / denominator;
            if (integerRatio > 0 && integerRatio <= 9007199254740991L && earned.CompareTo(target * new Natural(integerRatio).ToAmount()) < 0)
                numerator = denominator * integerRatio - 1;
            else if (integerRatio + 1 <= 9007199254740991L && earned.CompareTo(target * new Natural(integerRatio + 1).ToAmount()) == 0)
                numerator = denominator * (integerRatio + 1);
            fame = new Natural(IntegerSqrt(numerator / denominator));
            if (prestiges.IsZero) { credits = 3; return; }
            BigInteger whole = numerator / denominator;
            byte[] bytes = whole.ToByteArray(); int last = bytes.Length - 1;
            while (last > 0 && bytes[last] == 0) last--;
            int bits = last * 8; byte high = bytes[last];
            while (high != 0) { bits++; high >>= 1; }
            int powers = (bits - 1) / 2;
            credits = new Natural(BigInteger.One + powers + (numerator >= denominator * 4 ? unique / 250 : 0));
        }
        public static BigInteger IntegerSqrt(BigInteger value)
        {
            if (value.Sign < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (value < 2) return value;
            // Newton iteration from a safe power-of-two bound, no conversion to double/long.
            byte[] bytes = value.ToByteArray();
            BigInteger x = BigInteger.One << ((bytes.Length * 8 + 1) / 2);
            while (true) { BigInteger next = (x + value / x) >> 1; if (next >= x) return x; x = next; }
        }
        static void DecimalParts(Amount value, out BigInteger integer, out int scale)
        {
            string raw = value.mantissa.ToString("R", CultureInfo.InvariantCulture);
            int e = raw.IndexOfAny(new[] { 'e', 'E' }); int shift = 0;
            if (e >= 0) { shift = int.Parse(raw.Substring(e + 1), CultureInfo.InvariantCulture); raw = raw.Substring(0, e); }
            int point = raw.IndexOf('.'); int fraction = point < 0 ? 0 : raw.Length - point - 1;
            integer = BigInteger.Parse(raw.Replace(".", ""), CultureInfo.InvariantCulture);
            scale = checked(value.exponent + shift - fraction);
        }
        static void Ratio(Amount a, Amount b, out BigInteger n, out BigInteger d)
        {
            DecimalParts(a, out n, out int ae); DecimalParts(b, out d, out int be);
            int gap = checked(ae - be);
            if (gap >= 0) n *= BigInteger.Pow(10, gap); else d *= BigInteger.Pow(10, -gap);
        }
        public static double BoundedRatio(Natural level, int offset)
        {
            BigInteger n = level.Value;
            // Bounded quotient without overflowing either operand; exact level stays in the save.
            const long precision = 1000000000000000;
            return (double)(n * precision / (n + offset)) / precision;
        }
        public static Amount CardMultiplier(CardEffect effect, Natural level)
        {
            switch (effect)
            {
                case CardEffect.ProductProfit: return new Amount(1) + level.ToAmount() * .2;
                case CardEffect.GlobalProfit: return new Amount(1) + level.ToAmount() * .25;
                default: return new Amount(CardFactor(effect, level));
            }
        }
        public static double CardFactor(CardEffect effect, Natural level)
        {
            switch (effect)
            {
                case CardEffect.ProductCost: case CardEffect.TapCost: return 1 - .4 * BoundedRatio(level, 4);
                case CardEffect.UpgradeCost: return 1 - .5 * BoundedRatio(level, 4);
                case CardEffect.SkillCooldown: return 1 - .3 * BoundedRatio(level, 5);
                case CardEffect.SkillDuration: return 1 + .75 * BoundedRatio(level, 5);
                case CardEffect.TapProfit: case CardEffect.OrderPayout: return 1 + BoundedRatio(level, 5);
                case CardEffect.OfflineEfficiency: return .5 + .5 * BoundedRatio(level, 5);
                case CardEffect.OfflineHours: return 8 + 12 * BoundedRatio(level, 5);
                default: throw new ArgumentException("Use Amount for uncapped income multipliers");
            }
        }
        public static string DuplicateBenefit(CardEffect effect, Natural before)
        {
            if (effect == CardEffect.ProductProfit) return "+0.20 to the product multiplier";
            if (effect == CardEffect.GlobalProfit) return "+0.25 to the global multiplier";
            int offset = effect == CardEffect.ProductCost || effect == CardEffect.TapCost || effect == CardEffect.UpgradeCost ? 4 : 5;
            string numerator = effect == CardEffect.ProductCost || effect == CardEffect.TapCost ? "1.6" : effect == CardEffect.UpgradeCost ? "2" : effect == CardEffect.SkillCooldown ? "1.5" : effect == CardEffect.SkillDuration ? "3.75" : effect == CardEffect.OfflineEfficiency ? "2.5" : effect == CardEffect.OfflineHours ? "60" : "5";
            BigInteger denominator = (before.Value + offset) * (before.Value + offset + 1);
            // An exact fraction remains informative when floating display precision rounds both effects alike.
            return (effect == CardEffect.ProductCost || effect == CardEffect.TapCost || effect == CardEffect.UpgradeCost || effect == CardEffect.SkillCooldown ? "−" : "+") + numerator + "/" + denominator.ToString(CultureInfo.InvariantCulture) + (effect == CardEffect.OfflineHours ? " hours" : " factor");
        }
    }
}
