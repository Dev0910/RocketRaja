using System;
using System.Globalization;
using RocketRaja.Core;

namespace RocketRaja
{
    /// <summary>Session/navigation ports for preplaced native views; owns no visual objects.</summary>
    public sealed class RedesignUiContext
    {
        public RedesignEngine Engine { get; }
        public Func<bool> CanMutate { get; }
        public Func<Func<RedesignEngine, bool>, bool> Execute { get; }
        public Action<string, string> Navigate { get; }
        public Action<string> Notice { get; }
        public RedesignUiContext(RedesignEngine engine, Func<bool> canMutate, Func<Func<RedesignEngine, bool>, bool> execute,
            Action<string, string> navigate, Action<string> notice)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine)); CanMutate = canMutate ?? throw new ArgumentNullException(nameof(canMutate));
            Execute = execute ?? throw new ArgumentNullException(nameof(execute)); Navigate = navigate ?? throw new ArgumentNullException(nameof(navigate)); Notice = notice ?? (_ => { });
        }
        public void OpenGoal(GoalTarget target)
        {
            if (target == null) return;
            Navigate(target.kind == GoalTargetKind.Upgrade ? "upgradedetails" : target.kind == GoalTargetKind.Product ? "productdetails" : "taptraining",
                target.kind == GoalTargetKind.Upgrade ? target.upgradeId : target.product.ToString(CultureInfo.InvariantCulture));
        }
        public static string Number(Amount value, bool full = false) => !full ? value.ToString() : value.mantissa == 0 ? "0" :
            value.mantissa.ToString("G17", CultureInfo.InvariantCulture) + " × 10^" + value.exponent.ToString(CultureInfo.InvariantCulture);
        public static string Cash(Amount value, bool full = false) => "₹" + Number(value, full);
    }
}
