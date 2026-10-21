using System; using System.Linq; using RocketRaja; using RocketRaja.Core; using RocketRaja.Classic; using UnityEngine; using UnityEngine.UI;
public class ClassicRedDotValidation { public static string Run() {
var go=new GameObject("Notification checks");go.SetActive(false);
try {
var ui=go.AddComponent<ClassicUI>();ui.driver=go.AddComponent<RedesignRuntimeDriver>();
var data=RedesignRuntimeContent.Load();var s=RedesignState.Fresh(1000);
s.products=new[]{25,18,12,8,5,3,1,1};s.cash=1240000;s.completedPrestiges=1;s.fame=1;s.target=ProgressionNumbers.RunTarget(s.completedPrestiges);s.freeCredits=4;s.kajuKatli=400;
s.upgrades=data.Upgrades.Where(u=>u.effect==CashEffect.SkillPower&&u.rank==1).Select(u=>u.id).ToList();s.everPurchased=s.upgrades.ToList();
var engine=new RedesignEngine(data,s,_=>{});var session=new RedesignLifecycle(engine);
typeof(RedesignRuntimeDriver).GetProperty("Session").SetValue(ui.driver,session);
var dots=go.AddComponent<ClassicRedDots>();dots.ui=ui;
var specs=new[]{("Navigate","Achievements"),("Tab","Player"),("Tab","Legacy"),("Draw","1"),("Draw","5"),("ActivateSkill","0"),("BuyAll",""),("ClaimAchievement","products-1"),("Navigate","Prestige")};
dots.bindings=specs.Select(x=>{var child=new GameObject("Control",typeof(RectTransform));child.SetActive(false);child.transform.SetParent(go.transform);var c=child.AddComponent<ClassicControl>();c.action=x.Item1;c.parameter=x.Item2;var dot=new GameObject("Dot");dot.transform.SetParent(child.transform);return new ClassicRedDotBinding{control=c,dot=dot};}).ToArray();
dots.Refresh();if(dots.bindings.Take(8).Any(b=>!b.dot.activeSelf))throw new Exception("Expected all seeded opportunities");
if(!engine.ClaimAchievement("products-1"))throw new Exception("Claim failed");dots.Refresh();if(dots.bindings[7].dot.activeSelf)throw new Exception("Claim dot stayed on");
if(!engine.ActivateSkill(0))throw new Exception("Skill failed");dots.Refresh();if(dots.bindings[5].dot.activeSelf)throw new Exception("Active skill dot stayed on");
engine.State.activeUntil[0]=engine.State.readyAt[0]=0;dots.Refresh();if(!dots.bindings[5].dot.activeSelf)throw new Exception("Ready skill dot did not return");
var affordable=engine.Data.Upgrades.First(u=>engine.Eligible(u.id)&&engine.UpgradeCost(u.id)<=engine.State.cash);
var upgradeControl=dots.bindings[6].control;upgradeControl.action="BuyUpgrade";upgradeControl.parameter=affordable.id;
dots.Refresh();if(!dots.bindings[6].dot.activeSelf)throw new Exception("Affordable upgrade missing");
if(!engine.BuyUpgrade(affordable.id))throw new Exception("Upgrade purchase failed");
dots.Refresh();if(dots.bindings[6].dot.activeSelf)throw new Exception("Purchased upgrade stayed flagged");
upgradeControl.action="BuyAll";upgradeControl.parameter="";
engine.State.freeCredits=0;engine.State.kajuKatli=100;dots.Refresh();if(!dots.bindings[3].dot.activeSelf||dots.bindings[4].dot.activeSelf)throw new Exception("Paid draw affordability");
engine.State.kajuKatli=0;engine.State.cash=0;dots.Refresh();if(dots.bindings[2].dot.activeSelf||dots.bindings[3].dot.activeSelf||dots.bindings[6].dot.activeSelf)throw new Exception("Unaffordable actions flagged");
engine.State.eligibleEarnings=engine.State.target;dots.Refresh();if(!dots.bindings[8].dot.activeSelf)throw new Exception("Prestige not flagged");
session.OwnsSession=()=>false;dots.Refresh();if(dots.bindings.Any(b=>b.dot.activeSelf))throw new Exception("Blocked session flagged");
var main=UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/Scenes/Main.unity").GetRootGameObjects().Single(g=>g.name=="ClassicProductionCanvas").GetComponent<ClassicUI>();
var saved=main.GetComponent<ClassicRedDots>();if(saved.bindings.Length<80||saved.bindings.Any(b=>!b.control||!b.dot||b.dot.GetComponent<Image>().raycastTarget))throw new Exception("Invalid saved bindings");
return "PASS: "+saved.bindings.Length+" saved dots; seeded opportunities, claim clearing, active skill clearing, readiness return, paid draws, unaffordable actions, prestige, blocked sessions and nonblocking graphics. In-memory engine; no profile written.";
}finally{UnityEngine.Object.DestroyImmediate(go);}
}}
