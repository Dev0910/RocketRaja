using RocketRaja.Core;
using System.Text.Json;

static partial class Program
{
    sealed class TestWalletTransport : IPremiumAuthorityTransport
    {
        public bool Configured { get; set; } = true;
        public Func<PremiumSessionFence, Task<PremiumWalletSnapshot>> activate;
        public Func<PremiumIdentity, Task<PremiumWalletSnapshot>> wallet;
        public Func<PremiumSessionFence, CurrencyDrawRequest, Task<CurrencyDrawOutcome>> draw;
        public Task<PremiumWalletSnapshot> ActivateAsync(PremiumSessionFence session) => activate(session);
        public Task<PremiumWalletSnapshot> WalletAsync(PremiumIdentity owner) => wallet(owner);
        public Task<CurrencyDrawOutcome> DrawAsync(PremiumSessionFence session, CurrencyDrawRequest request) => draw(session, request);
        public Task<VerifiedPackGrant> FulfillAsync(PremiumIdentity owner, string sku, string token) => throw new NotSupportedException();
    }
    static async Task PremiumAccountChecks(RedesignCatalog data)
    {
        string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);
        T Decode<T>(string value) => JsonSerializer.Deserialize<T>(value, Json);
        var root = Path.Combine(Path.GetTempPath(), "PremiumAccount-" + Guid.NewGuid().ToString("N"));
        const string uid = "premium-cloud-fixture";
        var cloud = new FakeCloud(); var state = RedesignState.Fresh(cloud.Time);
        state.completedPrestiges = state.fame = 1; state.target = ProgressionNumbers.RunTarget(1); state.freeCredits = 3;
        await cloud.CreateAsync(new AccountProfile { uid = uid, displayName = "Synthetic" }, SavePayload.Create(Encode(state)));
        var local = CheckpointStore.ForRedesign(root, uid, Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>);
        using var account = new RedesignAccountSession(cloud, local, uid, "device-a", data, Encode, Decode<RedesignState>, () => cloud.Time);
        await account.InitializeAsync(await cloud.AcquireAsync(uid, "device-a"));
        var engine = account.BindEngine(new SequenceRandom(0, 0));
        var service = new TestWalletTransport();
        var wallet = new PremiumWalletSnapshot { uid = uid, profileId = state.guestWallet.profileId, revision = "1", balance = "1000", deficit = "0", cards = Enumerable.Repeat("0", 28).ToArray(), giftRegistered = false, importedGiftCardId = "", importedGiftOperationId = "" };
        service.activate = fence => {
            Check(fence.Identity.Uid == uid && fence.Generation == cloud.Record.generation && Decode<RedesignState>(cloud.Record.save.json).guestWallet.profileId == wallet.profileId, "Wallet activation uses confirmed guest provenance and current account lease");
            return Task.FromResult(Clone(wallet));
        };
        service.wallet = _ => Task.FromResult(Clone(wallet));
        using var client = new PremiumAccountClient(account, engine, service);
        Check(!client.Configured && await client.ActivateAsync() && client.Configured, "Purchasing remains gated until owned account activation reconciles a durable wallet");
        int sent = 0; CurrencyDrawOutcome saved = null; bool loseResponse = true;
        service.draw = (fence, request) => {
            sent++;
            var confirmed = Decode<RedesignState>(cloud.Record.save.json);
            Check(confirmed.schema == 3 && confirmed.premium.pendingOperationId == request.OperationId && confirmed.premium.pendingExpectedRevision == request.ExpectedRevision, "Paid request is recoverable from cloud before the debit is sent");
            if (saved == null)
            {
                wallet.revision = "2"; wallet.balance = "900"; wallet.cards[0] = "1";
                saved = new CurrencyDrawOutcome { operationId = request.OperationId, cardId = "C01", before = "0", after = "1", price = "100", revision = "2", wallet = Clone(wallet) };
            }
            if (loseResponse) { loseResponse = false; throw new IOException("response lost after server commit"); }
            return Task.FromResult(Clone(saved));
        };
        bool failed = false; try { await client.DrawAsync(); } catch (IOException) { failed = true; }
        string pending = engine.State.premium.pendingOperationId;
        Check(failed && pending != "" && !engine.State.reveal.pending && engine.State.cardLevels[0].IsZero, "Uncertain server result leaves the same cloud-backed operation pending without a local reroll");
        await account.InitializeAsync(await cloud.AcquireAsync(uid, "device-a"));
        var reopened = account.BindEngine(new SequenceRandom(0, 0));
        using var retry = new PremiumAccountClient(account, reopened, service);
        Check(client.Identity == null && reopened.State.premium.pendingOperationId == pending && await retry.DrawAsync(), "Reopened account retries the pending operation while the old client is fenced");
        Check(sent == 2 && reopened.State.cardLevels[0].Value == 1 && reopened.State.kajuKatli.Value == 900 && reopened.State.reveal.operationId == pending, "Recovered paid draw preserves one debit, original card and persisted reveal");
        reopened.DismissReveal(pending);
        wallet.revision = "3"; wallet.balance = "1900";
        service.draw = (_, _) => throw new PremiumServiceException("stale-wallet");
        Check(!await retry.DrawAsync() && reopened.State.premium.pendingOperationId == "" && reopened.State.kajuKatli.Value == 1900, "Definitive stale-wallet rejection clears the failed request and reconciles the newer balance");
        service.draw = (_, _) => { sent++; throw new Exception("must not be contacted"); };
        cloud.LoseAck = true; int before = sent;
        Check(!await retry.DrawAsync() && sent == before && reopened.State.premium.pendingOperationId != "" && !account.CanPlay, "Unconfirmed cloud write pauses paid spending before contacting the currency server");
        await account.InitializeAsync(await cloud.AcquireAsync(uid, "device-a"));
        reopened = account.BindEngine(new SequenceRandom(0, 0));
        using var afterRecovery = new PremiumAccountClient(account, reopened, service);
        var held = new TaskCompletionSource<CurrencyDrawOutcome>(); service.draw = (_, _) => held.Task;
        var delayed = afterRecovery.DrawAsync();
        string delayedOperation = reopened.State.premium.pendingOperationId;
        Check(!await afterRecovery.DrawAsync(), "Concurrent paid taps share one in-flight request");
        cloud.Time += 31; await cloud.AcquireAsync(uid, "device-b");
        saved.operationId = delayedOperation; saved.before = "1"; saved.after = "2"; saved.revision = "4"; saved.wallet = Clone(wallet); saved.wallet.revision = "4"; saved.wallet.balance = "1800"; saved.wallet.cards[0] = "2";
        held.SetResult(saved);
        failed = false; try { await delayed; } catch (PremiumServiceException ex) { failed = ex.Code == "ownership-lost"; }
        Check(failed && reopened.State.cardLevels[0].Value == 1 && reopened.State.premium.pendingOperationId == delayedOperation, "Device takeover fences late paid results while preserving their recoverable operation");
        var wrong = Clone(reopened.State); wrong.premium.uid = "another-player";
        cloud.Record.save = SavePayload.Create(Encode(wrong));
        using var other = new RedesignAccountSession(cloud, CheckpointStore.ForRedesign(root + "-other", uid, Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>), uid, "device-b", data, Encode, Decode<RedesignState>, () => cloud.Time);
        failed = false; try { await other.InitializeAsync(Clone(cloud.Record)); } catch (InvalidDataException) { failed = true; }
        Check(failed && !other.CanPlay, "Cloud account refuses a structurally valid wallet belonging to another UID");
    }
}
