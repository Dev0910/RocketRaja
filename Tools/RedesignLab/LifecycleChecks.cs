using RocketRaja.Core;

static partial class Program
{
    static void LifecycleChecks(RedesignCatalog data)
    {
        RedesignState disk = null; bool fail = false;
        var initial = RedesignState.Fresh(1000); initial.products[0] = 1;
        var session = new RedesignLifecycle(new RedesignEngine(data, initial, s => { if (fail) throw new IOException("disk full"); disk = s.Copy(); }));
        session.GameplayInputEnabled = false;
        Check(!session.TapGameplaySurface(), "Overlay input cannot earn a gameplay tap");
        session.Tick(20);
        Check(Near(session.Engine.State.eligibleEarnings, .85 * 20) && disk.now == 1020, "Browsing overlays retains full production and autosave");
        session.GameplayInputEnabled = true;
        Check(session.TapGameplaySurface() && !session.TapGameplaySurface(), "Gameplay input retains unbanked manual tap ceiling");
        var before = session.Engine.State.cash;
        Check(session.Suspend() && session.Suspend(), "Repeated pause callbacks save departure once");
        session.Tick(100);
        Check(!session.TapGameplaySurface() && Near(session.Engine.State.cash, before), "Suspension gates accrual and manual commands");
        Check(session.Resume(2020) && Near(session.Engine.State.cash - before, .85 * .5 * 1000), "Resume credits elapsed absence using normal departure production");
        before = session.Engine.State.cash;
        Check(session.Resume(3020) && Near(session.Engine.State.cash, before), "Duplicate resume without a new pause cannot mint offline cash");
        fail = true; var oldRun = session.Engine.State.runId;
        Check(!session.Run(e => e.Save()) && !session.CanMutate, "Storage failure blocks later mutations");
        session.Tick(50);
        Check(!session.Run(e => e.BuyLevels(0, 1)) && Near(session.Engine.State.cash, before), "Blocked save session cannot spend or silently accrue");
        fail = false;
        Check(session.RetrySave() && session.CanMutate && disk.runId == oldRun, "Storage recovery saves retained state before reopening commands");
        session.OwnsSession = () => false;
        Check(!session.Run(e => e.Save()) && !session.TapGameplaySurface(), "Lifecycle keeps account ownership above UI actions");
        before = session.Engine.State.cash; session.Tick(30);
        Check(Near(session.Engine.State.cash, before), "Lost ownership blocks lifecycle time as well as spending");
        session.OwnsSession = () => true; fail = true;
        Check(!session.Suspend() && session.Suspended, "Failed departure save still suspends gameplay");
        Check(!session.Resume(3020) && session.Suspended, "Failed resume remains suspended so an explicit retry still settles absence");
        fail = false;
        Check(session.Resume(3020) && session.CanMutate && disk.now == 3020, "Departure save failure recovers before offline settlement");
    }
}
