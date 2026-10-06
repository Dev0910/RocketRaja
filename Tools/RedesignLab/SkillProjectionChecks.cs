using RocketRaja.Core;

static partial class Program
{
    static void SkillProjectionChecks(RedesignCatalog data)
    {
        var state = RedesignState.Fresh(); state.cash = 100000; state.products[0] = state.products[1] = state.products[2] = 1;
        state.fame = state.completedPrestiges = 1; state.target = ProgressionNumbers.RunTarget(state.completedPrestiges);
        state.cardLevels[18] = state.cardLevels[21] = state.cardLevels[23] = 3;
        int saves = 0; var e = new RedesignEngine(data, state, _ => saves++); var before = e.State.Copy();
        var locked = e.PreviewSkill(0);
        Check(!locked.owned && locked.power == 2 && locked.duration > 30 && locked.cooldown < 300 && !e.ActivateSkill(0), "Locked skill preview includes relevant Talent but cannot activate");
        Check(saves == 0 && e.State.cash.CompareTo(before.cash) == 0 && e.State.commandRevision.CompareTo(before.commandRevision) == 0, "Skill preview has no save, cash or revision side effect");
        Check(e.BuyUpgrade(locked.unlockId) && e.SkillDuration(0) == locked.duration && e.SkillCooldown(0) == locked.cooldown, "Projected unlock timings match actual Talent-adjusted purchase");
        Check(e.ActivateSkill(0) && Math.Abs(e.State.activeUntil[0] - e.State.now - locked.duration) < 1e-6 && !e.ActivateSkill(0), "Actual activation uses projected duration and rejects a duplicate cooldown action");
        var hands = e.PreviewSkill(1); Check(!hands.owned && hands.power == 3 && hands.duration == 30 && e.BuyUpgrade(hands.unlockId), "Hands projection identifies its independent base skill and power");
        var order = e.PreviewSkill(2); Check(!order.owned && order.duration == 0 && order.orderPayout > e.NormalIncome() * order.power, "Order projection includes Talent and is an instant payout");
        e.BuyUpgrade(order.unlockId); var cash = e.State.cash; Check(e.ActivateSkill(2) && Math.Abs(Amount.Ratio(e.State.cash - cash, order.orderPayout) - 1) < 1e-12, "Order preview equals actual payout while Rush is active");
        var upgraded = RedesignState.Fresh(); upgraded.cash = Amount.Pow10(100); upgraded.products[0] = 1;
        upgraded.upgrades.AddRange(data.Upgrades.Where(u => u.target == 0 && u.effect >= CashEffect.SkillPower && u.effect <= CashEffect.SkillCooldown).Select(u => u.id)); upgraded.everPurchased.AddRange(upgraded.upgrades);
        var max = new RedesignEngine(data, upgraded, _ => { }).PreviewSkill(0); Check(max.owned && max.power == RedesignCatalog.SkillPower[0].Last() && max.duration > 30 && max.cooldown < 300, "Owned skill projection uses upgraded power and timings");
        Reject(() => e.PreviewSkill(3), "Unknown skill preview rejected");
    }
}
