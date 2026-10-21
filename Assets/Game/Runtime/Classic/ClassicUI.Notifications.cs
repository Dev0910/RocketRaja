namespace RocketRaja.Classic
{
    public sealed partial class ClassicUI
    {
        public int NotificationSkillIndex => skill;
        public bool NotificationBatchReady => batch != null && Engine.CanCommitBuyAll(batch);
    }
}
