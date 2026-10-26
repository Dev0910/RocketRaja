using System;
using System.Globalization;

namespace RocketRaja.Core
{
    /// <summary>Read-only presentation of a settled receipt. Storage measures time, not efficiency.</summary>
    public readonly struct OfflineTimeline
    {
        public readonly double Away, Stored, Cap, Overflow, Efficiency, CapPosition, Fill;
        public readonly bool Capped;
        public OfflineTimeline(OfflineReceipt receipt, Natural capCardLevel)
        {
            Cap = receipt.capSeconds > 0 ? receipt.capSeconds :
                3600 * ProgressionNumbers.CardFactor(CardEffect.OfflineHours, capCardLevel);
            Away = Math.Max(0, receipt.absence);
            Stored = Math.Min(Cap, Math.Max(0, receipt.creditedSeconds));
            Overflow = Math.Max(0, Away - Cap);
            Efficiency = receipt.efficiency;
            Capped = Stored >= Cap;
            double extent = Math.Max(Cap, Away);
            CapPosition = Cap / extent;
            Fill = Stored / extent;
        }
        public static string Duration(double seconds)
        {
            if (seconds > 0 && seconds < 60) return "<1m";
            if (seconds < 3600) return Math.Floor(seconds / 60).ToString("0", CultureInfo.InvariantCulture) + "m";
            return (seconds / 3600).ToString("0.#", CultureInfo.InvariantCulture) + "h";
        }
    }
}
