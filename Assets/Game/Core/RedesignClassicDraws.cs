using System;

namespace RocketRaja.Core
{
    public sealed class ClassicDrawQuote
    {
        internal string owner, session, run, command;
        internal int[] cards;
        public string OperationId { get; internal set; }
        public bool UseCredits { get; internal set; }
    }
    public sealed partial class RedesignEngine
    {
        public bool CanDrawFive(bool credits) => CanDraw(credits) &&
            (credits ? State.freeCredits.Value >= 4 : State.kajuKatli.Value >= 400);

        public ClassicDrawQuote QuoteDrawFive()
        {
            bool credits = State.freeCredits.Value >= 4;
            if (!CanDrawFive(credits)) return null;
            return new ClassicDrawQuote { owner = owner, session = SessionKey, run = State.runId,
                command = State.commandRevision.digits, OperationId = Guid.NewGuid().ToString("N"), UseCredits = credits };
        }
        public bool CanCommitDrawFive(ClassicDrawQuote quote) => quote != null &&
            Same(quote.owner, quote.session, quote.run, quote.command) && CanDrawFive(quote.UseCredits);
        public bool DrawFive(ClassicDrawQuote quote)
        {
            if (!CanCommitDrawFive(quote)) return false;
            // Hold the selected results on the quote so a failed durable save cannot reroll.
            if (quote.cards == null) { quote.cards = new int[5]; for (int i = 0; i < 5; i++) quote.cards[i] = TalentDrawPool.Select(Data, random); }
            return Commit(s => {
                if (quote.UseCredits) s.freeCredits -= (Natural)4; else s.kajuKatli -= (Natural)400;
                var results = new ClassicDrawResult[5];
                for (int i = 0; i < results.Length; i++)
                {
                    int index = quote.cards[i]; var before = s.cardLevels[index]; s.cardLevels[index] += (Natural)1;
                    results[i] = new ClassicDrawResult { cardId = Data.Cards[index].id, before = before, after = s.cardLevels[index] };
                }
                s.reveal = new DrawReveal { operationId = quote.OperationId, payment = quote.UseCredits ? "credit" : "kk", pending = true,
                    batch = results, batchIndex = 0, cardId = results[0].cardId, before = results[0].before, after = results[0].after };
                return true;
            });
        }
        public bool AdvanceClassicReveal(string operationId)
        {
            if (!State.reveal.pending || State.reveal.operationId != operationId || State.reveal.batch == null ||
                State.reveal.batchIndex + 1 >= State.reveal.batch.Length) return false;
            return Commit(s => { var r = s.reveal; var next = r.batch[++r.batchIndex]; r.cardId = next.cardId; r.before = next.before; r.after = next.after; return true; });
        }
    }
}
