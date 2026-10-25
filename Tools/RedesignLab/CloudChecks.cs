using System.Text.Json;
using RocketRaja.Core;

static partial class Program
{
    static async Task CloudChecks(RedesignCatalog data)
    {
        string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);
        T Decode<T>(string value) => JsonSerializer.Deserialize<T>(value, Json);
        async Task Rejected(Func<Task> action, string message) { bool failed = false; try { await action(); } catch { failed = true; } Check(failed, message); }
        var root = Path.Combine(Path.GetTempPath(), "RedesignCloud-" + Guid.NewGuid().ToString("N"));
        var cloud = new FakeCloud();
        var funded = RedesignState.Fresh(cloud.Time); funded.cash = 30;
        await cloud.CreateAsync(new AccountProfile { uid = "fixture-player", displayName = "Fixture", email = "", photoUrl = "" }, SavePayload.Create(Encode(funded)));
        var owned = await cloud.AcquireAsync("fixture-player", "device-a");
        var local = CheckpointStore.ForRedesign(root, "fixture-player", Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>);
        using var account = new RedesignAccountSession(cloud, local, "fixture-player", "device-a", data, Encode, Decode<RedesignState>, () => cloud.Time);
        await account.InitializeAsync(owned);
        Check(account.CanPlay && account.State.Valid(data), "New cloud state is validated before account play begins");
        RedesignEngine Bind() => account.BindEngine(new SequenceRandom(0, 0));
        var engine = Bind();
        Check(engine.BuyLevels(0, 1), "Account accepts durable manual purchase");
        account.Tick(true); await account.WaitForPendingAsync();
        Check(Decode<RedesignState>(cloud.Record.save.json).products[0] == 1, "New product state reaches immutable cloud payload");
        cloud.Hold = new TaskCompletionSource<bool>();
        engine.Tap(); engine.Save(); account.Tick(true);
        engine.Advance(1); engine.Tap(); engine.Save();
        cloud.Hold.SetResult(true); await account.WaitForPendingAsync(); cloud.Hold = null;
        account.Tick(true); await account.WaitForPendingAsync();
        Check(Decode<RedesignState>(cloud.Record.save.json).manualTaps.Value == 2, "New changes during upload persist as a separate follow-up snapshot");
        cloud.LoseAck = true;
        engine.Advance(1); engine.Tap(); engine.Save(); account.Tick(true); await account.WaitForPendingAsync();
        Check(!account.CanPlay && Decode<RedesignState>(cloud.Record.save.json).manualTaps.Value == 3, "Lost acknowledgment pauses redesigned account without replaying mutation");
        await account.ReconnectOwnedAsync();
        Check(account.CanPlay && account.State.manualTaps.Value == 3 && !local.Load("fixture-player").pending, "Pending receipt recovery acknowledges accepted state exactly once");
        var oldLifecycle = new RedesignLifecycle(engine);
        Check(!engine.Tap() && !engine.Save() && !oldLifecycle.CanMutate, "Reinitializing an account invalidates old engines even when ownership generation is reused");
        var advanced = account.State.Copy(); advanced.eligibleEarnings = advanced.target; advanced.revision += (Natural)1;
        account.CommitState(advanced);
        engine = Bind(); Check(engine.Prestige(engine.PreviewPrestige()), "Prestige uses account's durable checkpoint boundary");
        var draw = engine.QuoteDraw(true); Check(engine.Draw(draw), "Account free draw commits before reveal");
        cloud.LoseAck = true; account.Tick(true); await account.WaitForPendingAsync();
        await account.InitializeAsync(await cloud.AcquireAsync("fixture-player", "device-a"));
        Check(account.State.reveal.pending && account.State.reveal.cardId == "C01" && account.State.freeCredits.Value == 2 && account.State.cardLevels[0].Value == 1, "Interrupted account draw restores the same card, debit and reveal");
        long revisionBefore = cloud.Record.revision;
        await Rejected(() => account.InitializeAsync(awaitOwned(), RedesignState.Fresh()), "Recovery or guest copy cannot overwrite progressed account and premium state");
        Check(cloud.Record.revision == revisionBefore, "Rejected account replacement makes no cloud write");
        await account.InitializeAsync(await cloud.AcquireAsync("fixture-player", "device-a"));
        engine = Bind(); var stale = engine.QuoteBuyAll();
        cloud.Time += 31; await cloud.AcquireAsync("fixture-player", "device-b");
        Check(!account.CanPlay && !engine.CommitBuyAll(stale) && !engine.Tap(), "Device takeover blocks new mutations and stale quote commits");
        long takeoverGeneration = cloud.Record.generation, takeoverRevision = cloud.Record.revision;
        cloud.Time += 60;
        await Rejected(account.ReconnectOwnedAsync, "Retry cannot silently reacquire another device's expired lease");
        Check(cloud.Record.owner == "device-b" && cloud.Record.generation == takeoverGeneration && cloud.Record.revision == takeoverRevision,
            "Rejected retry preserves the other device's owner, generation and progress");
        var futureCloud = new FakeCloud(); var future = RedesignState.Fresh(); future.schema = 9;
        await futureCloud.CreateAsync(new AccountProfile { uid = "future", displayName = "Fixture" }, SavePayload.Create(Encode(future)));
        var futureOwned = await futureCloud.AcquireAsync("future", "future-device");
        using var futureSession = new RedesignAccountSession(futureCloud, new CheckpointStore(root, "future", Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>), "future", "future-device", data, Encode, Decode<RedesignState>, () => futureCloud.Time);
        await Rejected(() => futureSession.InitializeAsync(futureOwned), "Future gameplay payload is rejected before initial cloud commit");
        Check(futureCloud.Record.revision == 1 && !futureSession.CanPlay, "Unsupported version never rewrites cloud progress");
        var emptyCloud = new FakeCloud();
        await emptyCloud.CreateAsync(new AccountProfile { uid = "guest-promotion", displayName = "Fixture" }, SavePayload.Create(Encode(RedesignState.Fresh(emptyCloud.Time))));
        var promotionOwned = await emptyCloud.AcquireAsync("guest-promotion", "promotion-device");
        using var promotion = new RedesignAccountSession(emptyCloud, CheckpointStore.ForRedesign(root, "guest-promotion", Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>), "guest-promotion", "promotion-device", data, Encode, Decode<RedesignState>, () => emptyCloud.Time);
        var guest = RedesignState.Fresh(20000); guest.products[0] = 1; guest.storeName = "Fixture Guest"; guest.music = false;
        guest.upgrades.Add("SK-FR-P-01"); guest.everPurchased.Add("SK-FR-P-01"); guest.activeUntil[0] = 20020; guest.readyAt[0] = 20300;
        await promotion.InitializeAsync(promotionOwned, guest);
        Check(promotion.State.products[0] == 1 && promotion.State.storeName == "Fixture Guest" && !promotion.State.music && promotion.State.activeUntil[0] == 1020 && promotion.State.readyAt[0] == 1300 && guest.now == 20000, "Guest promotion preserves progress/preferences and rebases timers without modifying source guest");
        var lifecycle = new RedesignLifecycle(promotion.BindEngine(new SequenceRandom(0, 0)));
        Check(lifecycle.TapGameplaySurface() && lifecycle.Run(e => e.Save()), "Account lifecycle preserves durable command routing");
        promotion.Block("Synthetic ownership interruption");
        Check(!lifecycle.CanMutate && !lifecycle.TapGameplaySurface(), "Lifecycle composition cannot override an account engine's ownership fence");
        emptyCloud.Time += 60;
        await promotion.ReconnectOwnedAsync();
        Check(promotion.CanPlay && promotion.State.manualTaps.Value == 1 && !lifecycle.CanMutate,
            "Owned retry recovers the durable pending command after lease expiry and permanently fences the old binding");
        var resumed = promotion.BindEngine();
        string offlineId = resumed.State.offline.id; var resumedCash = resumed.State.cash;
        await promotion.ReconnectOwnedAsync();
        Check(promotion.State.offline.id == offlineId && promotion.State.cash.CompareTo(resumedCash) == 0,
            "Repeated account retry does not credit the same absence twice");
        await emptyCloud.RequestHandoffAsync("guest-promotion", "second-device");
        long beforeHandoffRetry = emptyCloud.Record.revision;
        await Rejected(promotion.ReconnectOwnedAsync, "Retry cannot cancel a pending device handoff");
        Check(emptyCloud.Record.handoff == "second-device" && emptyCloud.Record.revision == beforeHandoffRetry,
            "Retry leaves the pending handoff and cloud progress intact");
        promotion.Dispose();
        await Rejected(promotion.ReconnectOwnedAsync, "Disposed account cannot be reconnected");

        var purchaseCloud = new FakeCloud();
        var purchased = RedesignState.Fresh(purchaseCloud.Time);
        purchased.schema = 3; purchased.kajuKatli = 500;
        purchased.premium = new PremiumWalletCache {
            uid = "purchase-player", profileId = purchased.guestWallet.profileId, revision = 1, deficit = 0,
            currencyCards = Enumerable.Repeat(Natural.Zero, 28).ToArray(), pendingOperationId = "", pendingExpectedRevision = "", lastAppliedOperationId = ""
        };
        Check(purchased.Valid(data), "Purchase replacement fixture contains a valid account wallet");
        await purchaseCloud.CreateAsync(new AccountProfile { uid = "purchase-player" }, SavePayload.Create(Encode(purchased)));
        var purchaseOwned = await purchaseCloud.AcquireAsync("purchase-player", "purchase-device");
        using var purchaseSession = new RedesignAccountSession(purchaseCloud, CheckpointStore.ForRedesign(root, "purchase-player", Encode, Decode<AccountCheckpoint>, Decode<SaveVersionHeader>), "purchase-player", "purchase-device", data, Encode, Decode<RedesignState>, () => purchaseCloud.Time);
        await Rejected(() => purchaseSession.InitializeAsync(purchaseOwned, RedesignState.Fresh(), replaceExistingProgress: true), "Explicit device replacement cannot discard an authoritative purchase wallet");
        Check(purchaseCloud.Record.revision == 1 && Decode<RedesignState>(purchaseCloud.Record.save.json).kajuKatli.Value == 500,
            "Purchase protection preserves the original Google wallet without a cloud write");
        CloudRecord awaitOwned() => Clone(cloud.Record);
    }
}
