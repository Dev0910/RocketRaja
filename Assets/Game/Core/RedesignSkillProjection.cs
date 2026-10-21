using System;

namespace RocketRaja.Core
{
    public sealed class SkillProjection
    {
        public int skill;
        public string unlockId;
        public bool owned;
        public double power, duration, cooldown;
        public Amount orderPayout;
    }
    public sealed partial class RedesignEngine
    {
        Amount BulkOrderPayout(RedesignState s, RunEffects f) => f.skillPower[2] == 0 ? Amount.Zero :
            NormalIncome(s, f) * RedesignCatalog.SkillPower[2][f.skillPower[2] - 1] * ProgressionNumbers.CardFactor(CardEffect.OrderPayout, s.cardLevels[23]);
        public SkillProjection PreviewSkill(int skill)
        {
            if (skill < 0 || skill >= 3) throw new ArgumentOutOfRangeException(nameof(skill));
            string unlock = "SK-" + RedesignCatalog.SkillKeys[skill] + "-P-01";
            bool owned = SkillOwned(skill); var effects = Effects;
            if (!owned)
            {
                Data.TryUpgrade(unlock, out int index); effects = new RunEffects(Data, State); effects.Add(Data.Upgrades[index]);
            }
            return new SkillProjection { skill = skill, unlockId = unlock, owned = owned,
                power = RedesignCatalog.SkillPower[skill][effects.skillPower[skill] - 1],
                duration = Duration(skill, State, effects), cooldown = Cooldown(skill, State, effects),
                orderPayout = skill == 2 ? BulkOrderPayout(State, effects) : Amount.Zero };
        }
    }
}
