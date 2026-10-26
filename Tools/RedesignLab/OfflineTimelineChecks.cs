using RocketRaja.Core;

static partial class Program
{
    static void OfflineTimelineChecks(RedesignCatalog catalog)
    {
        var state = RedesignState.Fresh(1000); state.products[0] = 1;
        RedesignState disk = null;
        bool fail = false;
        var engine = new RedesignEngine(catalog, state, s => { if (fail) throw new IOException("Offline test disk full"); disk = Clone(s); });
        engine.Save(); engine.ResolveOffline(37000);
        var receipt = engine.State.offline;
        var timeline = new OfflineTimeline(receipt, 0);
        Check(timeline.Away == 36000 && timeline.Stored == 28800 && timeline.Cap == 28800 && timeline.Overflow == 7200 && timeline.Capped && timeline.Fill == .8 && timeline.CapPosition == .8,
            "Capped timeline shows eight stored hours and two overflow hours across ten hours away");
        Check(timeline.Efficiency == .5 && engine.State.offline.capSeconds == 28800, "Receipt freezes its actual cap and efficiency independently of the meter fill");
        var old = receipt.Copy(); old.capSeconds = 0; old.absence = old.creditedSeconds = 3600;
        timeline = new OfflineTimeline(old, 0);
        Check(timeline.Cap == 28800 && timeline.Overflow == 0 && !timeline.Capped && timeline.Fill == .125 && timeline.CapPosition == 1, "Legacy and under-cap receipts retain storage space without an overflow tail");
        old.absence = old.creditedSeconds = 0;
        timeline = new OfflineTimeline(old, 0);
        Check(timeline.Fill == 0 && timeline.CapPosition == 1 && OfflineTimeline.Duration(30) == "<1m", "Empty and very short absences have a finite readable timeline");
        timeline = new OfflineTimeline(receipt, 5);
        Check(timeline.Cap == 28800, "Changing card levels after receipt creation cannot rewrite the historical cap");
        Check(!engine.RewardOfflineAd("stale") && !engine.RewardOfflineAd(null), "Wrong or missing ad receipt cannot grant a bonus");
        var before = engine.State.cash;
        fail = true;
        bool threw = false;
        try { engine.RewardOfflineAd(receipt.id); } catch (IOException) { threw = true; }
        Check(threw && !engine.State.offline.adRewarded && Near(before, engine.State.cash), "Failed bonus save leaves both cash and claim flag unchanged");
        fail = false;
        Check(engine.RewardOfflineAd(receipt.id) && Near(engine.State.cash - before, receipt.amount) && engine.State.offline.adRewarded,
            "Completed ad saves exactly one extra base reward atomically");
        var total = engine.State.cash;
        Check(!engine.RewardOfflineAd(receipt.id) && Near(total, engine.State.cash), "Duplicate callbacks cannot double an offline reward again");
        engine = new RedesignEngine(catalog, disk, s => disk = Clone(s));
        Check(!engine.RewardOfflineAd(receipt.id) && engine.DismissOffline() && Near(total, engine.State.cash), "Reload and Continue preserve the claimed reward without another grant");
        engine.ResolveOffline(73000); var next = engine.State.offline;
        Check(!next.adRewarded && engine.DismissOffline() && !engine.RewardOfflineAd(next.id), "A new receipt resets its offer; a dismissed receipt rejects late completion");
        var invalid = engine.State.Copy(); invalid.offline.capSeconds = double.NaN;
        Check(!invalid.Valid(catalog), "Non-finite receipt caps cannot enter a save");
    }
}
