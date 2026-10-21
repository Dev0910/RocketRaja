using System;
using System.Globalization;

namespace RocketRaja.Core
{
    /// <summary>Whole-number effect copy, with hundredths reserved for price reduction factors.</summary>
    public static class UpgradeEffectCopy
    {
        public static string Whole(double value) => value <= 0 ? "0" : Math.Ceiling(value - 1e-9).ToString("0", CultureInfo.InvariantCulture);
        public static string Bonus(double factor) => "+" + Whole(Math.Max(0, (factor - 1) * 100)) + "%";
        public static string Reduction(double factor) => Whole(Math.Max(0, (1 - factor) * 100)) + "%";
        public static string Cost(double factor) => (Math.Ceiling(factor * 100 - 1e-9) / 100).ToString("0.00", CultureInfo.InvariantCulture);
        public static string Multiplier(double factor) => Math.Abs(factor - Math.Round(factor)) < 1e-9
            ? "×" + Whole(factor) : Bonus(factor);
    }
}
