using System.Text.Json;
using RocketRaja.Core;

static partial class Program
{
    sealed class TestPremiumHttp : IPremiumHttpConnection
    {
        public bool Configured { get; set; } = true;
        public string Route, Body;
        public PremiumHttpResponse Response;
        public Task<PremiumHttpResponse> PostAsync(PremiumIdentity owner, string route, string json)
        { Route = route; Body = json; return Task.FromResult(Response); }
    }
    static async Task PremiumHttpChecks(RedesignCatalog data)
    {
        var http = new TestPremiumHttp();
        var transport = new PremiumHttpTransport(http, o => JsonSerializer.Serialize(o, o.GetType(), Json), (s, t) => JsonSerializer.Deserialize(s, t, Json));
        var owner = new PremiumIdentity("http-fixture", "session:1"); var session = new PremiumSessionFence(owner, "device-a", 7);
        string huge = "10000000000000000000000000000000001";
        http.Response = new PremiumHttpResponse { Status = 200, Json = JsonSerializer.Serialize(new PremiumPackReply { uid = owner.Uid, productId = data.Packs[0].id, fulfilled = true, revision = huge, granted = "1000" }, Json) };
        var grant = await transport.FulfillAsync(owner, data.Packs[0].id, "synthetic-only-token");
        using (var request = JsonDocument.Parse(http.Body))
        Check(http.Route == "fulfill" && grant.revision.digits == huge && grant.granted.Value == 1000 && request.RootElement.EnumerateObject().Count() == 2 && !request.RootElement.TryGetProperty("uid", out _), "Purchase wire protocol preserves exact decimal-string revisions and derives ownership only from authenticated headers");
        var wallet = new PremiumWalletSnapshot { uid = owner.Uid, profileId = new string('a', 32), revision = "1", balance = "100", deficit = "0", cards = Enumerable.Repeat("0", 28).ToArray(), importedGiftCardId = "", importedGiftOperationId = "" };
        http.Response.Json = JsonSerializer.Serialize(wallet, Json);
        Check((await transport.ActivateAsync(session)).balance == "100" && http.Route == "activate" && JsonDocument.Parse(http.Body).RootElement.GetProperty("generation").GetInt64() == 7, "Activation sends the owned lease and parses the wallet snapshot");
        await transport.WalletAsync(owner);
        Check(http.Route == "wallet" && JsonDocument.Parse(http.Body).RootElement.EnumerateObject().Count() == 0, "Wallet reads cannot nominate a different player's UID in their body");
        foreach (var pair in new[] { (409, "stale-wallet", "stale-wallet"), (409, "insufficient-currency", "insufficient-currency"), (503, "stale-wallet", "service-unavailable"), (302, "insufficient-currency", "service-unavailable"), (401, "detail", "authentication-required"), (429, "detail", "retry-later"), (409, "untrusted server detail", "service-unavailable") })
        {
            http.Response.Status = pair.Item1; http.Response.Json = JsonSerializer.Serialize(new { error = pair.Item2 });
            string code = ""; try { await transport.WalletAsync(owner); } catch (PremiumServiceException ex) { code = ex.Code; }
            Check(code == pair.Item3, $"HTTP {pair.Item1}/{pair.Item2} maps to {pair.Item3} without converting uncertainty to a safe-to-reroll rejection");
        }
        foreach (string json in new[] { "not json", "null", new string('x', 262145) })
        {
            http.Response.Status = 200; http.Response.Json = json;
            bool rejected = false; try { await transport.WalletAsync(owner); } catch (PremiumServiceException ex) { rejected = ex.Code == "invalid-response"; }
            Check(rejected, "Malformed or oversized success response cannot populate the wallet");
        }
    }
}
