using RocketRaja.Core;
using System.Text.Json;

static partial class Program
{
    static void GuidanceChecks(RedesignCatalog data)
    {
        var state = RedesignState.Fresh(); int writes = 0;
        RedesignEngine Engine(RedesignState s) => new(data, s, _ => writes++);
        void Next(RedesignState s, string route, string argument, string expected)
        {
            var e = Engine(s); string before = JsonSerializer.Serialize(e.State, Json);
            Check(RedesignGuidance.Next(e, route, argument) == expected && writes == 0 && before == JsonSerializer.Serialize(e.State, Json), "Read-only contextual topic: " + (expected ?? "none") + " / " + route);
        }
        Next(state, "home", "", "buy-first"); Next(state, "productdetails", "0", "buy-first");
        Next(state, "upgrades", "", null); Next(state, "settings", "", null);
        state.products[0] = 25; state.products[1] = 1;
        Next(state, "home", "", "tap-counter"); Next(state, "productdetails", "0", "product-levels");
        Next(state, "taptraining", "", "product-levels"); Next(state, "upgrades", "", "catalogue-filters");
        Next(state, "buyall", "", "buy-all"); Next(state, "skilldetails", "0", "skill-basics");
        Next(state, "upgradedetails", "PP-PHU-01", "comebacks"); Next(state, "upgradedetails", "PP-ANA-01", null);
        Next(state, "upgradedetails", "missing", null); Next(state, "prestige", "", "prestige");
        Next(state, "talent", "", "cash-and-talent");
        state.upgrades.Add("PP-PHU-01"); state.everPurchased.Add("PP-PHU-01"); Next(state, "upgradedetails", "PP-PHU-01", null);
        state = RedesignState.Fresh(); state.completedPrestiges = 1; state.fame = 1; state.target = ProgressionNumbers.RunTarget(1); state.freeCredits = 3;
        Next(state, "home", "", "rebuild"); Next(state, "talent", "", "talent-free");
        state.cardLevels[0] = 2; state.reveal = new DrawReveal { operationId = "guide-fixture", cardId = data.Cards[0].id, before = 1, after = 2, payment = "credit", pending = true };
        Next(state, "drawreveal", "", "talent-duplicates"); Next(state, "talent", "", null);
        state.reveal.before = 0; state.reveal.after = 1; Next(state, "drawreveal", "", null);
        state.reveal.pending = false; state.completedPrestiges = 2; state.fame = 2; state.target = ProgressionNumbers.RunTarget(2); state.secondPrestigeGift = true;
        Next(state, "talent", "", "second-gift"); Next(state, "kajushop", "", "second-gift");
        state.learnedTopics.Add("second-gift"); Next(state, "talent", "", "talent-free");
        state.learnedTopics.AddRange(RedesignGuidance.Topics.Where(id => !state.learnedTopics.Contains(id))); Next(state, "talent", "", null);
        state.learnedTopics = Enumerable.Range(0, 256).Select(i => "historical-" + i).ToList(); Next(state, "talent", "", null);

        state = RedesignState.Fresh(); state.products[0] = 25; state.cash = 10000; state.eligibleEarnings = state.target;
        bool fail = false; RedesignState disk = null;
        var engine = new RedesignEngine(data, state, s => { if (fail) throw new IOException("tip save failure"); disk = Clone(s); }); var session = new RedesignLifecycle(engine);
        var batch = engine.QuoteBuyAll(); var prestige = engine.PreviewPrestige(); var command = engine.State.commandRevision;
        Check(session.Run(e => e.LearnTopic("buy-first")) && disk.learnedTopics.Contains("buy-first") && engine.State.commandRevision.CompareTo(command) == 0, "Tip acknowledgement persists without an economic command revision");
        Check(engine.CanCommitBuyAll(batch) && engine.CanCommitPrestige(prestige), "Acknowledging a tip preserves frozen Buy All and Prestige reviews");
        var revision = engine.State.revision;
        Check(!engine.LearnTopic("buy-first") && engine.State.revision.CompareTo(revision) == 0, "Repeated acknowledgement never writes twice");
        fail = true; Check(!session.Run(e => e.LearnTopic("tap-counter")) && !engine.State.learnedTopics.Contains("tap-counter") && session.LastSaveException is IOException, "Failed acknowledgement keeps the unlearned topic and reports saving failure");
        fail = false; Check(session.RetrySave() && engine.LearnTopic("tap-counter"), "Recovery allows the same topic to be acknowledged once");
        Check(engine.Prestige(prestige) && engine.State.learnedTopics.SequenceEqual(new[] { "buy-first", "tap-counter" }), "Prestige preserves acknowledged and skipped topic IDs");
        var store = new RedesignSaveStore(Path.GetFullPath("Temp/GuidanceCoreQA-" + Guid.NewGuid().ToString("N") + "/state.save"), data, s => JsonSerializer.Serialize(s, Json), s => JsonSerializer.Deserialize<RedesignState>(s, Json), s => JsonSerializer.Deserialize<SaveVersionHeader>(s, Json));
        store.Save(disk);
        Check(store.Load().state.learnedTopics.SequenceEqual(engine.State.learnedTopics), "Learned topics survive the actual save store and codec");
        session.OwnsSession = () => false; Check(!engine.LearnTopic("rebuild") && !engine.State.learnedTopics.Contains("rebuild"), "Lost ownership prevents topic acknowledgements");
        state.learnedTopics = Enumerable.Range(0, 256).Select(i => "historical-" + i).ToList(); engine = Engine(state);
        Check(!engine.LearnTopic("new-topic") && engine.State.learnedTopics.Count == 256 && writes == 0, "Full historical topic capacity rejects a new acknowledgement without throwing or writing");
    }
}
