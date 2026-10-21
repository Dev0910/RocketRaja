using RocketRaja.Core;

static partial class Program
{
    static void PreferenceChecks(RedesignCatalog data)
    {
        var state = RedesignState.Fresh(); state.eligibleEarnings = state.target; bool fail = false; RedesignState disk = null;
        var e = new RedesignEngine(data, state, s => { if (fail) throw new IOException("full disk"); disk = s.Copy(); }); var session = new RedesignLifecycle(e);
        var review = e.PreviewPrestige();
        Check(session.Run(x => x.ToggleSetting("music")) && !disk.music && session.Run(x => x.ToggleSetting("sound")) && !disk.sound && session.Run(x => x.ToggleSetting("motion")) && disk.reducedMotion, "All three settings persist through durable commands");
        Check(e.CanCommitPrestige(review), "Changing settings preserves the reviewed economic quote");
        Check(e.SetStoreName("  Diwali & Co.  ") && e.State.storeName == "Diwali & Co." && disk.storeNamePromptHandled && e.CanCommitPrestige(review), "Naming normalizes and persists without invalidating an economic review");
        Check(!e.SetStoreName("\u2603") && !e.SetStoreName("") && !e.SetStoreName(new string('x', 25)) && e.State.storeName == "Diwali & Co.", "Invalid or unsupported names do not replace the saved name");
        Check(!e.ToggleSetting("unknown"), "Unknown preference cannot mutate state");
        fail = true; Check(!session.Run(x => x.ToggleSetting("music")) && !e.State.music && session.LastSaveException is IOException, "Failed setting save publishes no toggle and records its I/O diagnostic");
        fail = false; Check(session.RetrySave() && session.LastSaveException == null && !disk.music, "Saving recovery preserves preferences and clears the old failure diagnostic");
        session.OwnsSession = () => false; Check(!e.SetStoreName("Other shop") && !e.ToggleSetting("sound") && e.State.storeName == "Diwali & Co.", "Lost session ownership blocks name and preference commands");
    }
}
