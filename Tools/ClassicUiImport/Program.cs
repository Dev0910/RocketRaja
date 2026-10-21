using System.Text.Json;
using RocketRaja.Core;
var options = new JsonSerializerOptions { IncludeFields = true, IgnoreReadOnlyProperties = true };
var catalog = new RedesignCatalog(JsonSerializer.Deserialize<RedesignContent>(File.ReadAllText("Documentation/RedesignContentCatalog.json"), options));
int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
RedesignState Seed(int credits, int kk) { var s=RedesignState.Fresh(1000); s.completedPrestiges=1; s.fame=1; s.target=ProgressionNumbers.RunTarget(s.completedPrestiges); s.freeCredits=credits; s.kajuKatli=kk; return s; }
foreach(var balance in new[]{(4,0),(3,400),(0,400),(8,400)})
{
    var saved=Seed(balance.Item1,balance.Item2); var e=new RedesignEngine(catalog,saved,s=>saved=s.Copy());
    var quote=e.QuoteDrawFive(); Check(quote!=null,"Bundle affordable"); Check(e.DrawFive(quote),"Bundle commits");
    Check(e.State.freeCredits.Value==balance.Item1-(balance.Item1>=4?4:0),"Exact credit cost; no mixed spending");
    Check(e.State.kajuKatli.Value==balance.Item2-(balance.Item1>=4?0:400),"Exact KK cost");
    Check(e.State.cardLevels.Sum(n=>(int)n.Value)==5,"Exactly five cards granted");
    Check(!e.DrawFive(quote)&&e.QuoteDrawFive()==null,"Pending result and replay blocked");
    var restored=JsonSerializer.Deserialize<RedesignState>(JsonSerializer.Serialize(saved,options),options);
    Check(restored.Valid(catalog),"Saved bundle round-trips"); e=new RedesignEngine(catalog,restored,s=>saved=s.Copy());
    for(int i=1;i<5;i++) Check(e.AdvanceClassicReveal(e.State.reveal.operationId)&&e.State.reveal.batchIndex==i,"Advance saved reveal without grants");
    Check(!e.AdvanceClassicReveal(e.State.reveal.operationId),"Final result cannot overrun");
    Check(e.DismissReveal(e.State.reveal.operationId)&&e.State.cardLevels.Sum(n=>(int)n.Value)==5,"Continue dismisses without a grant");
    Check(!e.DismissReveal(e.State.reveal.operationId),"Repeat dismissal rejected");
}
foreach(var balance in new[]{(3,399),(0,0),(1,100)}) Check(new RedesignEngine(catalog,Seed(balance.Item1,balance.Item2),_=>{}).QuoteDrawFive()==null,"Insufficient bundle rejected");
bool fail=true;var random=new CountingRandom();var engine=new RedesignEngine(catalog,Seed(4,0),_=>{if(fail)throw new IOException("Test save failure");},random);
var retry=engine.QuoteDrawFive();try{engine.DrawFive(retry);throw new Exception("Expected save error");}catch(IOException){}
Check(engine.State.freeCredits.Value==4&&engine.State.cardLevels.All(n=>n.IsZero),"Save failure rolls back all cost and rewards");
int calls=random.Calls;fail=false;Check(engine.DrawFive(retry)&&random.Calls==calls,"Same quote retry cannot reroll");
Check(engine.DismissReveal(engine.State.reveal.operationId)&&!engine.State.reveal.pending,"Close early dismisses whole batch");
var invalid=engine.State.Copy();invalid.reveal.batchIndex=9;Check(!invalid.Valid(catalog),"Invalid persisted index rejected");
var copy=engine.State.Copy();copy.reveal.batch[0].cardId="bad";Check(engine.State.reveal.batch[0].cardId!="bad","Batch result deep copied");
var staleEngine=new RedesignEngine(catalog,Seed(4,0),_=>{});var stale=staleEngine.QuoteDrawFive();staleEngine.ToggleSetting("motion");
// Nonstructural preferences intentionally preserve valid economy quotes.
Check(staleEngine.CanCommitDrawFive(stale),"Presentation preference does not invalidate an economy quote");
staleEngine.SessionKey="another-session";Check(!staleEngine.DrawFive(stale),"Cross-session quote rejected");
Console.WriteLine($"PASS {checks} atomic bundle and persistence checks");
sealed class CountingRandom : IDrawRandom { public int Calls; public int Next(int max){Calls++;return 0;} }
