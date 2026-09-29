using System;
using System.Globalization;

namespace RocketRaja.Core
{
    // Approximate non-negative base-10 arithmetic. Prices and earnings may exceed 1e308.
    // Small exact values (ranks, transaction IDs) never use this type.
    [Serializable]
    public struct Amount : IComparable<Amount>
    {
        public double mantissa;
        public int exponent;
        public Amount(double value) { mantissa = value; exponent = 0; Normalize(); }
        public Amount(double m, int e) { mantissa = m; exponent = e; Normalize(); }
        void Normalize()
        {
            if (double.IsNaN(mantissa) || double.IsInfinity(mantissa) || mantissa < 0)
                throw new ArgumentOutOfRangeException(nameof(mantissa));
            if (mantissa == 0) { exponent = 0; return; }
            if (mantissa < 1e-300) { mantissa *= 1e300; exponent = checked(exponent - 300); }
            int shift = (int)Math.Floor(Math.Log10(mantissa));
            mantissa /= Math.Pow(10, shift); exponent = checked(exponent + shift);
            if (mantissa >= 10) { mantissa /= 10; exponent = checked(exponent + 1); }
        }
        public bool Valid => !double.IsNaN(mantissa) && !double.IsInfinity(mantissa) &&
            ((mantissa == 0 && exponent == 0) || (mantissa >= 0.999999999999 && mantissa < 10.000000000001 && Math.Abs((long)exponent) < 1000000));
        public double Log10 => mantissa == 0 ? double.NegativeInfinity : Math.Log10(mantissa) + exponent;
        public static Amount Pow10(double power)
        {
            if (double.IsNaN(power) || power > 999999) throw new ArgumentOutOfRangeException(nameof(power));
            if (power < -300) return Zero;
            int exp = (int)Math.Floor(power); return new Amount(Math.Pow(10, power - exp), exp);
        }
        public static Amount Zero => new Amount(0);
        public static implicit operator Amount(double value) => new Amount(value);
        public static Amount operator +(Amount a, Amount b)
        {
            if (a.mantissa == 0) return b; if (b.mantissa == 0) return a;
            if (a.exponent < b.exponent) { Amount t = a; a = b; b = t; }
            long gap = (long)a.exponent - b.exponent;
            return gap > 17 ? a : new Amount(a.mantissa + b.mantissa * Math.Pow(10, -gap), a.exponent);
        }
        public static Amount operator -(Amount a, Amount b)
        {
            if (b.mantissa == 0) return a;
            if (a.CompareTo(b) <= 0) return Zero;
            long gap = (long)a.exponent - b.exponent;
            return gap > 17 ? a : new Amount(Math.Max(0, a.mantissa - b.mantissa * Math.Pow(10, -gap)), a.exponent);
        }
        public static Amount operator *(Amount a, double b) => a * new Amount(b);
        public static Amount operator *(Amount a, Amount b) => new Amount(a.mantissa * b.mantissa, checked(a.exponent + b.exponent));
        public static Amount operator /(Amount a, double b) => b <= 0 ? throw new DivideByZeroException() : new Amount(a.mantissa / b, a.exponent);
        public static double Ratio(Amount a, Amount b) => b.mantissa == 0 ? double.PositiveInfinity : Math.Pow(10, Math.Min(308, a.Log10 - b.Log10));
        public int CompareTo(Amount b)
        {
            if (mantissa == 0 || b.mantissa == 0) return mantissa.CompareTo(b.mantissa);
            return exponent == b.exponent ? mantissa.CompareTo(b.mantissa) : exponent.CompareTo(b.exponent);
        }
        public static bool operator >=(Amount a, Amount b) => a.CompareTo(b) >= 0;
        public static bool operator <=(Amount a, Amount b) => a.CompareTo(b) <= 0;
        public static bool operator >(Amount a, Amount b) => a.CompareTo(b) > 0;
        public static bool operator <(Amount a, Amount b) => a.CompareTo(b) < 0;
        public override string ToString()
        {
            if (mantissa == 0) return "0";
            if (exponent < 3) return (mantissa * Math.Pow(10, exponent)).ToString("0.##", CultureInfo.InvariantCulture);
            string[] suffix = { "", "K", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af" };
            int group = exponent / 3;
            if (group >= suffix.Length) return mantissa.ToString("0.##", CultureInfo.InvariantCulture) + "e" + exponent;
            return (mantissa * Math.Pow(10, exponent % 3)).ToString("0.##", CultureInfo.InvariantCulture) + suffix[group];
        }
    }
}
