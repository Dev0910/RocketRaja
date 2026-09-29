# Incremental Game Progression and Unity Development

The recommended starting point is a compact combat incremental game: responsive tapping, automatic allies, upgrade milestones, boss gates, and a prestige system that makes the next expedition substantially faster. Borrow Cookie Clicker’s readable investment choices, Tap Tycoon’s visible empire growth, Tap Titans’ clear push-and-reset rhythm, and Tap Titans 2’s choice of playstyles. Add the larger collection and social systems only after repeated runs are enjoyable.

This report distinguishes **documented mechanics**, **design interpretation**, and **proposed implementation**. Research was checked on September 13, 2026. Cookie Clicker’s browser implementation is the baseline; mobile and console editions can differ. Tap Tycoon and original Tap Titans have substantial historical documentation, while Tap Titans 2 changes through live updates. The proposed numbers below are starting hypotheses, not measured benchmarks or extracted GameHive balancing data.

## 1. How the four games differ

| Game | Immediate action | Repeatable investment | Long-term progression | Most useful design lesson |
|---|---|---|---|---|
| Cookie Clicker | Click for cookies | Buildings and upgrades increase production | Ascension, heavenly upgrades, additional systems | Make the economy legible and repeatedly expand what players can optimize |
| Tap Tycoon | Tap for cash | Build and improve businesses | Prestige, persistent technology bonuses, historical country competition | Connect personal growth to a visibly expanding enterprise |
| Tap Titans | Tap enemies | Hire and level heroes; use skills | Prestige earns relics for artifacts | Make the current obstacle and next breakthrough obvious |
| Tap Titans 2 | Fight through stages using a chosen build | Heroes, spells, and gold spending | Artifacts, skills, equipment, pets, raids, tournaments, seasons | Offer different attention requirements and several routes to permanent power |

The official GameHive pages establish these broad identities. Cookie Clicker’s published code establishes its production and ascension foundations. Specific legacy details require the dated sources discussed below.[^1][^2][^3][^4]

**Design interpretation.** All four turn an immediate action into an investment loop. The interesting decision eventually shifts from “tap again” to “which purchase, timing choice, or reset produces the most useful progress?” A successful original game should preserve that transition.

## 2. Cookie Clicker: production, opportunity cost, and discovery

### Documented mechanics

Clicking generates the initial resource; buildings generate cookies automatically. The published browser code uses a base building price growth factor of **1.15**. It also calculates total prestige from the cube root of qualifying cumulative cookies divided by one trillion. Ascension awards the newly earned difference, rather than awarding the entire lifetime total again.[^1]

Golden Cookies introduce temporary bonuses and immediate payouts. The community wiki documents production Frenzy and Click Frenzy as different effects, and explains how their overlap can create powerful bursts. This means holding resources or preparing a combination can sometimes compete with immediately buying another upgrade.[^5]

Sugar Lumps unlock additional interactions through building levels. The documented minigames include Garden, Pantheon, Grimoire, and Stock Market. They broaden the decision space beyond buying more production.[^6]

Offline production is a separate rule. In the inspected browser code, normal closed-game production is unlocked through heavenly upgrades such as Twin Gates of Transcendence; its base description gives reduced production for a limited period, followed by a lower rate. “Idle while open” and “earning while closed” should therefore not be treated as identical.[^1]

### Why the progression works

**Design interpretation.** Production is easy to understand: spend a known amount and earn faster afterward. Rising prices create opportunity cost, while milestones and new options periodically make a different investment attractive. The theme makes numerical escalation feel like discovery: the business becomes increasingly absurd as the economy grows.

The long-term strength is the arrival of new decisions. A large number alone becomes familiar. A new interaction that changes how that number is earned can refresh the game. Ascension also gives players a reason to reconsider the early economy with new advantages.

### What to borrow

Show each purchase’s cost, resulting output, and next milestone. Let older producers regain relevance through occasional upgrades or synergies. Introduce one new decision layer at a time, and make temporary bonuses visually distinct from permanent output.

Avoid importing an established endgame’s full complexity. A first version can demonstrate the same principle with six producers, a handful of milestones, and one permanent upgrade tree. Maintain separate balance tests for passive earnings and active burst combinations; an acceptable average can conceal an extreme combination that skips most content.

## 3. Tap Tycoon: the business fantasy

### Documented mechanics and historical evidence

GameHive describes tapping to generate money, investing in a multi-industry business empire, and joining fellow countrymen in global competition for medals and rewards.[^2]

A December 2015 guide describes prestige resetting the business run while improving subsequent performance and contributing soldiers to weekly country wars. It reports a 20-trillion-cash prestige threshold. That is **historical evidence**, not a verified current unlock requirement.[^7]

A February 2016 guide describes Tech Cards as persistent bonuses to businesses or the player, including profit improvements and cost reductions. It also describes several acquisition routes and a rotating card shop.[^8] These sources differ in their descriptions of card purchase pricing; exact prices are omitted because they are not necessary to understand the progression design and may reflect different updates.

### Why the progression works

**Design interpretation.** The business theme makes growth concrete. A player can see an enterprise expanding, even when most of the underlying system is numerical. Individual business milestones provide intermediate goals between major unlocks.

Persistent cards create a second kind of progress. A run can end while the player still feels their account improved. The historical war system also gives a private reset a collective consequence: the same action advances the player and contributes to a larger contest.

### What to borrow

Give progression a visible home: a growing workshop, settlement, fleet, or expedition camp. Let important upgrades alter silhouettes, machinery, characters, or activity. These changes make improvement recognizable before the player reads the multiplier.

Use a small set of persistent discoveries to alter future runs. Guarantee a useful first discovery and provide a deliberate path to acquiring missing ones. For a first release, a personal expedition objective can fulfill the goal-setting role of a country war without requiring matchmaking, reward services, and a large participating population.

## 4. Original Tap Titans: a readable combat loop

### Documented mechanics

GameHive’s official description connects tapping, hired heroes, active skills, and prestige. Prestige provides relics that buy artifacts.[^3]

The historical community beginner guide explains hero investment, difficult progression walls, and the tradeoff between pushing a little farther and starting another run. Its companion artifact page describes random artifact acquisition, upgrading with relics, and increasingly expensive new artifact purchases.[^9][^10] These are legacy accounts; the original game’s thresholds should not be carried into Tap Titans 2.

### Why the progression works

**Design interpretation.** Enemy health turns an economic constraint into a visible challenge. Gold purchases improve damage, damage improves clearing, and clearing provides more gold and access to later rewards. A powerful upgrade is felt immediately when an enemy that was slow becomes easy.

A wall creates several possible responses: buy a damage upgrade, improve income, use a temporary skill, farm, or prestige. This is more interesting when those responses have different time costs. A wall with only one affordable answer is mainly a timer.

Prestige changes the meaning of replaying. Familiar enemies become a demonstration of accumulated power. However, the game still needs a way to compress repeated content; merely increasing damage eventually leaves animation and travel time as the bottleneck.

### What to borrow

Use a clearly labeled boss challenge and let failure return to productive farming. Make a new hero or milestone visibly improve clearing speed. Distinguish pushing into unknown stages from replaying known stages, then unlock faster traversal of previously completed content.

The first reset should be understandable without an external guide. Preview what resets, what remains, and what can be purchased with the reward. Follow the reset with an immediate opportunity to experience the resulting power.

## 5. Tap Titans 2: build choice and layered progression

### The run and the account

The official prestige help page says prestige resets stage progress, Sword Master levels, spell levels, and hero levels, while awarding relics. Relics purchase and improve artifacts that strengthen future progress. It also identifies additional progression rewards, including skill points and Stage Rush.[^11]

That page describes a guided first prestige at stage 60, but contains older information about clan-linked starting stages. It should not be treated as a complete current specification. The sound design distinction is **temporary run state versus persistent account state**.

GameHive’s product page describes pets, equipment, skills, tournaments, and clan raids around the combat loop.[^4] The resource guide linked by GameHive’s help center describes separate sources of persistent improvement, including hero weapons, clan scrolls, crafting shards, pets, and raid cards.[^12][^13]

### Builds change the way players interact

The developer-linked build guide is labeled for version 7.5.0. Its broad categories illustrate how one progression system can serve different preferences; the following is a historical design comparison, not a current tier ranking.[^14]

| Build family | Interaction described by the guide | Design pattern |
|---|---|---|
| Pet | Frequent tapping | Continuous active engagement |
| Clan Ship | Automated attacks with optional active interactions | Low-attention baseline with extra involvement available |
| Shadow Clone | Maintain spells, then allow periods of autonomous damage | Preparation followed by hands-off intervals |
| Heavenly Strike | Repeated spell activation and mana management | Deliberate burst actions |
| Dagger | More directed interaction, including aiming | Precision input within an incremental economy |
| Gold Gun | Charge a powerful mode, then spend it in a burst | Alternating preparation and acceleration |

**Design interpretation.** The transferable idea is that build choice can determine the player's attention pattern. A skill tree becomes more meaningful when it changes how a session feels. Three paths that all require identical tapping but have different colors provide less variety.

### Repeatable power and slower power

Relics connect repeated prestige runs to improvement. Other resources connect progress to collections, milestones, and activities outside the immediate run.[^11][^13] This permits a player to improve even when the current stage wall is difficult.

**Design interpretation.** Several progression rates can keep goals available, but each additional currency increases explanation and balancing costs. A small project needs a clear reason for every resource: what earns it, what spends it, and why that transaction cannot use an existing currency.

### Offline progress and replay compression

The developer-linked Silent March guide is labeled for version 6.9.0 and describes inactive traversal toward a limit relative to the starting and maximum stage, rather than unrestricted pushing into new stages. Its timing paragraph contains inconsistent units and an inconsistent worked example; exact timing should not be adopted from it.[^15]

**Design interpretation.** Offline progression can be designed as recovery toward a known frontier, with active play reserved for new discoveries. This is a different product choice from granting a fixed quantity of passive currency. Choose deliberately; do not quietly switch between the two interpretations in the implementation.

### Seasons and the expanding endgame

GameHive introduced Transcendence in its June 2023 update discussion as seasonal progression beyond the then-current cap. A subsequent update explains Monument bonuses that depend on actions within a prestige or across a season.[^16][^17] A 2026 GameHive archive lists Transcendence Season 12 and further raid-card changes, demonstrating continued evolution beyond those earlier guides.[^18]

**Design interpretation.** Seasonal rules can refresh a mature economy by changing what players optimize. They also create recurring content, reward, support, and fairness work. They are an expansion strategy after the ordinary prestige loop has proven durable.

## 6. The progression principles to use

The following are recommendations derived from the comparison, not claims about proprietary design intent or measured retention effects.

**Keep goals available at several distances.** A player should see an affordable improvement, a session milestone, and a longer-term unlock. Each should be specific: a hero level threshold, a boss, or a new build option. A vague instruction to “get stronger” does not help someone decide what to do next.

**Alternate accumulation and transformation.** Small purchases provide continuity; milestones produce noticeable jumps. Periodically add an interaction, change a visual, or unlock an automation rule. A multiplier can be useful without making every reward another multiplier.

**Preserve meaningful choices.** An income upgrade helps only if it changes the timing of later purchases. A damage upgrade helps only if it affects clearing or progression. Compare both against a concrete target such as defeating the next boss, rather than comparing isolated percentages.

**Make attention optional but valuable.** Support tapping with immediate feedback, then introduce alternatives such as held input, automatic attacks, or timed skills. Active play can improve progress without requiring continuous rapid tapping to make the game functional.

**Teach reset through results.** The first prestige reward should purchase a guaranteed useful improvement. Revisit the previous obstacle quickly enough that the player recognizes the difference. Show the previous best stage alongside the current run.

**Spend complexity carefully.** A new currency or subsystem should create a new decision or solve a demonstrated pacing problem. A large content catalog is not a substitute for a satisfying second and third run.

## 7. Economy mathematics

These equations are proposed modeling tools unless explicitly labeled otherwise. Anthony Pecorella’s Kongregate article supports modeling generator growth, bulk purchasing, milestones, and prestige through simulations; it also cautions that simplified models do not fully balance a complete game.[^19]

### Upgrade costs and bulk purchases

Let `b` be the initial price, `r > 1` the price growth factor, and `n` the number of purchases already made:

```text
nextCost(n) = b × r^n
bulkCost(n, k) = b × r^n × (r^k − 1) / (r − 1)
maxAffordable(n, wallet) = floor(log_r(1 + wallet × (r − 1)/(b × r^n)))
```

These are geometric-series identities. Cookie Clicker’s base factor of 1.15 is a verified example, not a universal recommendation.[^1] After calculating maximum affordable quantity, recompute its cost and adjust around rounding boundaries. Handle zero money, purchase caps, and the `r = 1` case separately.

For an illustrative producer with `b = 10` and `r = 1.15`, ten copies from zero cost approximately 203.04. If each adds one unit of income per second and there are no other bonuses, the purchase adds ten income per second and has a nominal 20.30-second payback. This excludes reinvestment and milestones.

### Output and milestones

```text
producerOutput_i = baseOutput_i × level_i × milestoneMultiplier_i
passiveOutput = sum(producerOutput_i) × globalMultiplier
```

For an initial experiment, use milestone multipliers at levels 10, 25, and 50. Compute the output gained by the entire proposed purchase, including any crossed milestone. Comparing only the next single level can miss the best near-term investment.

```text
paybackSeconds = purchaseCost / incrementalOutput
```

Payback is a diagnostic, not a complete player strategy. It ignores unlocks, saved-up burst windows, future purchases, and the value of reaching the next stage. Simulate multiple purchase policies before deciding a curve is healthy.

### Combat, gold, and the wall

```text
enemyHealth(stage) = H0 × q^(stage − 1)
killGold(stage) = G0 × g^(stage − 1)
timeToKill ≈ enemyHealth / effectiveDPS
bossRequiredDPS ≈ bossHealth / bossTimeLimit
```

Choose `q`, `g`, hero cost growth, and hero power growth together. These quantities use different units: a stage multiplier cannot be directly compared with a per-purchase multiplier without modeling purchases earned per stage.

A boss with 30,000 health and a 30-second timer needs about 1,000 sustained DPS under constant-damage assumptions. Spells, critical hits, spawn delays, and discrete attacks can change the actual result. Use expected values for rough balance and event simulation for borderline outcomes.

Model non-damage time too:

```text
stageTime ≈ sum(enemyKillTimes) + spawnDelays + transitionTime
```

A hundredfold damage increase cannot meaningfully accelerate a section already dominated by fixed delays. That is when replay skipping or compressed transitions becomes a progression feature.

### Multipliers

Define explicit groups such as base damage, hero milestones, permanent bonuses, build bonuses, and temporary effects. Within a group, decide whether bonuses add or multiply and keep that rule consistent.

Two independent `×2` groups give `×4`. Two `+100%` effects in the same additive group give `×3` including the base. Store a breakdown that the UI and debugging tools can display. Audit every dependency to prevent a multiplier from applying twice through both hero damage and a derived attack.

### Two different prestige models

**Cumulative production model.** Cookie Clicker’s published foundation is:

```text
totalPrestige = floor((qualifyingCumulativeCookies / 10^12)^(1/3))
newPrestige = max(0, totalPrestige − previouslyAwardedPrestige)
```

Spending heavenly chips does not reduce the cumulative total used to prevent re-awarding old progress.[^1]

**Repeatable stage-run model, proposed for this game.** Award relics for each eligible completed run:

```text
if highestStageThisRun < unlockStage: reward = 0
else: reward = floor(R0 × a^(highestStageThisRun − unlockStage))
```

Here `R0 > 0` sets the first reward and `a > 1` sets reward growth. This is an original placeholder formula, not Tap Titans 2’s actual formula. Use this repeatable model for the recommended combat prototype; do not combine it accidentally with cumulative production awards.

Consider both run efficiency and improvement:

```text
relicsPerMinute = reward / totalRunMinutes
recoveryRatio = minutesToPreviousBest / totalRunMinutes
```

The second quantity shows whether replaying old content consumes the session. Neither quantity alone proves that a run is enjoyable. A slightly slower run may unlock a valuable new feature.

### Offline earnings

For a simple currency-only prototype:

```text
elapsed = clamp(nowUtc − savedUtc, 0, offlineCapSeconds)
reward = savedEligibleIncomePerSecond × elapsed × offlineEfficiency
```

Exclude expired temporary buffs from the saved eligible rate. An illustrative test configuration is a 12-hour cap and 50% efficiency. These are proposed values, not facts about the reference games.

For combat, choose a reproducible farming rate from a completed checkpoint and grant currency without advancing stages in the first version. Add stage catch-up later if testing shows recovery is tedious. Resolve offline time once per resume transaction; repeated focus and pause callbacks must not duplicate rewards.

## 8. A concrete original game direction

**Proposed concept: an expedition workshop.** A small crew explores a changing world and defeats obstacles to recover materials. Players tap to help the crew, invest in specialists, and return to base with knowledge that improves the next expedition. The base visibly grows as permanent discoveries accumulate.

This is a design direction, not a finished theme commitment. It provides a natural place for combat, business-like investment, visual growth, and prestige without reusing the references’ characters or presentation.

| Layer | Proposed contents | Purpose |
|---|---|---|
| Immediate | Tap response, automatic crew attacks, gold drops | Make actions understandable and satisfying |
| Run | Six crew types, upgrade milestones, one burst skill, bosses | Create investment and timing decisions |
| Permanent | Relics, six initial discoveries, best-stage record | Make repeat expeditions change |
| Later expansion | Three small build paths and workshop transformations | Add playstyle and visual progression |

Use one run currency and one prestige currency initially. For the first prestige, offer a guaranteed discovery and enough reward to buy it. Random collections can wait until the core progression is balanced.

### Initial pacing hypotheses

| Experience | Starting test target | Evidence to collect |
|---|---|---|
| First meaningful purchase | Within 20–40 seconds | Does the player recognize its effect? |
| First automatic ally | Within 1–2 minutes | Does the player understand passive damage? |
| First milestone upgrade | Within 3–5 minutes | Is the power jump visible? |
| First timed boss | Within 5–10 minutes | Can the player explain why it was won or lost? |
| First prestige | Within 20–30 active minutes | Can the player predict the benefit? |
| First return to previous best | Within 5–10 minutes after prestige | Does the reset feel worthwhile? |

These targets are intentionally faster than some legacy experiences and need testing on the intended device. They are not published retention benchmarks. Short-session mobile play and long desktop sessions may require different tuning.

## 9. Practical Unity architecture

The workspace has no established Unity project version to target. The following is a proposed architecture, with Unity 6 documentation used for API context; it does not assume a particular installed editor or claim that GameHive uses this architecture.

Keep the economy in ordinary C# classes so it can run without scenes, input, or animations. Unity components translate player input into commands and display the resulting state. This makes simulations and save recovery much easier to validate.

```text
Content definitions → Simulation + runtime state → UI and combat presentation
                              ↕
                       Save/load + offline time
                              ↓
                       Progression telemetry
```

Use ScriptableObjects for authored definitions such as heroes, stage rules, upgrades, and discovery effects. Unity documents them as shared asset-based data containers.[^20] Keep owned levels and currencies in separate runtime/save objects; do not rely on mutating definition assets to save a deployed game.

| Module | Responsibility |
|---|---|
| EconomyService | Income, costs, affordability, bulk buying |
| CombatSimulation | Damage application, deaths, boss timers, stage transitions |
| ModifierResolver | Ordered calculation and explanation of bonuses |
| PrestigeService | Reward preview and validated reset transaction |
| OfflineProgressService | Resolve elapsed time from a saved snapshot |
| SaveService | Serialization, versions, recovery, durable writes |
| GamePresenter | Display state and dispatch player commands |
| SimulationRunner | Advance virtual time with scripted player policies |

Suggested authored fields include stable IDs, base costs, growth factors, base output, milestone lists, unlock conditions, and effect categories. Stable IDs should survive renaming an asset or rearranging its position in a list.

### Numerical representation

Use ordinary integers for bounded counters and a deliberate large-number type for unbounded economy values. BreakInfinity.cs is an MIT-licensed C# option whose maintainer explicitly prioritizes speed over exact accuracy for values beyond ordinary double range.[^21] Evaluate and pin a revision before adoption; this report has not compiled it in a target Unity build.

Approximate large-number arithmetic is suitable for huge damage and production values, but small exact entitlements should use exact counters. Serialize the mantissa and exponent, or another stable invariant representation, and reject invalid numbers on load. Do not save a rounded display string such as “1.23aa” as the authoritative amount.

### Simulation and presentation

Start with a 10–20 Hz simulation update and smooth presentation between updates; treat this as a profiling hypothesis. For deterministic tests, use fixed simulation steps or an event queue, deterministic input, and seeded randomness. Render frame rate should not change currency earned or boss outcomes.

At high clearing speeds, resolve multiple deaths mathematically while displaying a bounded number of effects. Do not require an animation callback to award gold. Pool frequently created effects and update numeric labels only when their displayed value changes.

### Persistence and recovery

Unity provides `Application.persistentDataPath` for persistent application data, with platform-specific behavior.[^22] Use a versioned save with explicit migration steps. Preserve at least one validated backup and write through a temporary file before replacing the main save using a strategy appropriate to the target platform.

Save after prestige, significant rewards, purchases at a sensible debounce interval, and application suspension. Test actual mobile process termination; an orderly quit callback is not a reliable assumption for every exit.

A prestige operation should validate the preview against the current state, award the relics, reset only the designated fields, and persist the combined result. Never leave a save where the reward was granted but the old run can grant it again. Include a monotonic save revision and transaction identifiers where needed.

For local offline play, clamp negative time differences and cap excessive elapsed time. Device time alone is not trustworthy for competitive rewards. If tournaments or shared rewards are introduced, validate their important transactions and eligibility on a backend.

### Explicit reset contract

| Reset at prestige | Keep across prestige |
|---|---|
| Run currency | Unspent relics and awarded-reward history |
| Current stage and enemy health | Highest-ever stage |
| Crew levels and temporary upgrades | Permanent discoveries and build unlocks |
| Temporary buffs and run timers | Cosmetics, settings, accessibility preferences |
| Current run statistics | Lifetime statistics and save schema version |

This contract belongs in data and tests, as well as the confirmation screen.

## 10. Build sequence and acceptance criteria

These are dependency-ordered work packages, not a promised schedule. Art requirements, platform services, and team experience will determine duration.

| Step | Deliverable | Acceptance criterion |
|---|---|---|
| 1. Economy model | Costs, damage, gold, milestones, scripted simulation | At least three purchase policies can be compared through three prestige cycles |
| 2. Playable combat | Tapping, automatic allies, boss challenge, purchases | Players understand how to improve and can recover after a failed boss |
| 3. Prestige | Preview, reset contract, permanent discovery, save | Second run demonstrates faster recovery and reaches a new milestone |
| 4. Offline and durability | Return reward, backups, migrations, resume handling | Closing and reopening cannot duplicate rewards or lose a completed prestige |
| 5. Playstyle choice | Active, companion, and burst-oriented paths | Each path changes interaction and remains viable under its intended attention level |
| 6. Presentation and testing | Growing workshop, readable UI, onboarding | New testers can explain purchases and reset consequences without coaching |

Limit the first playable build to six crew types, roughly thirty authored upgrades, six permanent discoveries, one active skill, a small set of enemy visuals, and three meaningful prestige cycles. Expand content after those cycles work. Keep store integration, competitive seasons, and guilds outside this initial milestone.

## 11. Validation and balancing

Automated checks should protect the economy and player progress:

1. Bulk purchase totals match repeated single purchases within the intended numeric tolerance.
2. A purchase cannot create negative currency or cross its level cap.
3. The prestige preview equals the committed reward and cannot be claimed twice.
4. Permanent fields survive prestige while temporary fields reset.
5. Saving and loading preserves state; a corrupt main save recovers from backup.
6. Repeated pause/resume events cannot award the same offline interval again.
7. Identical seeded inputs produce equivalent progression regardless of rendering speed.
8. A buff that expires while closed does not receive unlimited offline duration.

Simulate a frequent active player, a mostly passive player, and a player returning in short sessions. Include simple purchase policies such as cheapest-first, shortest payback, and saving for milestones. If one policy encounters an unintended multi-hour stop while another progresses smoothly, inspect whether the game teaches the relevant choice.

Track time to first purchase, automatic ally, boss, prestige, and return to previous best. Also track stage-at-exit, failed bosses, upgrade choices, currency inactivity, run duration, and reward per run. Record the economy version with each event so a tuning change does not silently mix incompatible results.

Human testing answers questions simulations cannot: Was the next goal obvious? Did an upgrade feel useful? Was waiting pleasant? Did the reset feel like progress? Did the tester choose to start another run?

Do not label projected retention or revenue as evidence. None of the inspected public sources establishes an applicable retention target for this new game. Establish an initial baseline through testing and evaluate changes against it.

## 12. Monetization and future operations

The reference products use different commercial approaches. Cookie Clicker has a paid Steam edition, while GameHive operates a Tap Titans 2 store selling account-linked offers and acknowledging VIP progress.[^23][^24] Their designs should not be assumed to require the same business model.

**Recommendation.** Validate enjoyable unpaid progression first. Then choose between a paid release and an optional-purchase model based on platform and audience. Cosmetics, clearly bounded optional boosts, and a transparent supporter purchase are simpler starting points than a large assortment of interdependent paid currencies.

If rewarded ads are added, preserve the clearly stated base reward and model the optional bonus separately. If competition is added, determine how purchases affect eligibility and rankings before designing prizes. Shared progression introduces backend authority, account recovery, reward idempotency, and customer-support obligations.

Seasonal content should answer an observed need for new decisions. A smaller rotating challenge with fixed rules is a manageable experiment before implementing a second seasonal prestige economy.

## 13. Evidence limits and design decisions

The most reliable findings concern the games’ core loops, GameHive’s published feature descriptions, and the inspected Cookie Clicker formulas. Tap Tycoon’s detailed prestige/card descriptions are from 2015–2016. Original Tap Titans guides are legacy community documentation. Tap Titans 2 guides explicitly cover different versions; exact unlock requirements, preferred builds, item values, and offline timing need version-specific verification before reproduction.

GameHive’s fetched homepage/blog listings showed older material while another archive exposed 2026 posts. Therefore this report does not claim to identify the latest Tap Titans 2 patch. The Compendium also displayed an undated shutdown notice; its availability should not be assumed for a future tool dependency.[^18][^25]

There was no hands-on measurement of full reference-game runs, no access to their proprietary economy tables, and no Unity implementation or playtest of the proposed design. Proposed timing, architecture, and progression formulas are explicit development hypotheses. The next practical milestone is a simulated and playable three-prestige loop that can validate them.

## Sources

Sources without an explicit publication date are marked undated. Community sources describe observed mechanics rather than an authoritative current implementation.

[^1]: Orteil / DashNet. [Cookie Clicker published browser game code](https://orteil.dashnet.org/cookieclicker/main.js). Undated live code, inspected September 13, 2026. Building price factor, prestige calculation, offline-production conditions. [Official game](https://orteil.dashnet.org/cookieclicker/).
[^2]: GameHive. [Tap Tycoon](https://gamehive.com/games/tap-tycon/). Undated official product page. Business investment and country competition.
[^3]: GameHive. [Tap Titans](https://gamehive.com/games/tap-titans/). Undated official product page. Tapping, heroes, skills, relics and artifacts.
[^4]: GameHive. [Tap Titans 2](https://gamehive.com/games/tap-titans-2/). Undated official product page. Core loop and surrounding systems; marketing counts are not treated as current caps.
[^5]: Cookie Clicker Wiki contributors. [Golden Cookie](https://cookieclicker.wiki.gg/wiki/Golden_cookie). Undated community reference. Temporary effects and combinations.
[^6]: Cookie Clicker Wiki contributors. [Minigames](https://cookieclicker.wiki.gg/wiki/Minigames). Undated community reference. Sugar Lump unlocks and additional systems.
[^7]: Luciano, Touch, Tap, Play. [Tap Tycoon – Guide to Prestige & How to Get More Tech Cards](https://www.touchtapplay.com/tap-tycoon-guide-to-prestige-how-to-get-more-tech-cards/). December 21, 2015. Historical prestige and war mechanics.
[^8]: Throwgame. [Tap Tycoon – Tech Cards](https://throwgame.wordpress.com/2016/02/25/tap-tycoon-tech-cards/). February 25, 2016, with updates within the post. Historical persistent card bonuses.
[^9]: Tap Titans Headquarter. [Beginners Guide](https://tap-titans.com/beginners-guide/). Undated legacy community guide. Walls, hero investment and reset timing.
[^10]: Tap Titans Headquarter. [Artifacts](https://tap-titans.com/artifacts/). Undated legacy community reference. Artifact acquisition and relic costs.
[^11]: GameHive Help Center. [Should I prestige? When?!](https://gamehive.helpshift.com/hc/en/3-tap-titans-2/faq/75-should-i-prestige-when/?p=android). Undated; displays a relative update age. Prestige reset fields and rewards; some surrounding guidance is older.
[^12]: GameHive Help Center. [Acronyms, Artifacts & Much More!](https://gamehive.helpshift.com/hc/en/3-tap-titans-2/faq/68-acronyms-artifacts-much-more/?s=crashes). Undated. Establishes the developer's links to specialist community guides.
[^13]: lemmingllama. [Resource Guide: How To Push Back the Softcap and Make Your Relics Count](https://www.reddit.com/r/TapTitans2/comments/9nmj6m/resource_guide_how_to_push_back_the_softcap_and/). Living guide originally posted in 2018. Resource roles; precise values omitted.
[^14]: lemmingllama. [How To Build a Build](https://www.reddit.com/r/TapTitans2/comments/7e9iz8/how_to_build_a_build_will_be_updated_with_new/). Living guide labeled up to date for version 7.5.0. Build interactions, not a current balance ranking.
[^15]: lemmingllama. [Silent March Inactive Progression Guide](https://www.reddit.com/r/TapTitans2/comments/tbpfdj/silent_march_inactive_progression_guide/). Living guide labeled up to date for version 6.9.0. Catch-up design; internal timing inconsistencies noted.
[^16]: GameHive / lemmingllama. [DevUpdate #95: If I Had A Million Stages](https://gamehive.com/blog/devupdate-95-if-i-had-a-million-stages/). June 16, 2023. Original Transcendence design.
[^17]: GameHive / lemmingllama. [DevUpdate #96: A Monumental Undertaking](https://gamehive.com/blog/devupdate-96-a-monumental-undertaking/). June 2023. Prestige- and season-scoped Monument bonuses.
[^18]: GameHive / lemmingllama. [GameHive archive, page 5](https://gamehive.com/page/5/). Retrieved archive includes April 2026 posts for Transcendence Season 12 and versions 7.21.x. Evidence of ongoing evolution, not the latest patch.
[^19]: Anthony Pecorella / Kongregate. [Quest for Progress: Numbers Behind Idle Games](https://www.kongregate.com/en/pages/quest-for-progress-the-math-of-idle-games). Undated accessible article. Simulation, milestones, growth and prestige modeling.
[^20]: Unity Technologies. [Unity 6.0 Manual: ScriptableObject](https://docs.unity3d.com/6000.0/Documentation/Manual/class-ScriptableObject.html). Versioned documentation. Authored shared data assets.
[^21]: Razenpok. [BreakInfinity.cs](https://github.com/Razenpok/BreakInfinity.cs). Undated repository README and license. Large-number C# arithmetic and accuracy tradeoff.
[^22]: Unity Technologies. [Unity 6.0 Scripting API: Application.persistentDataPath](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html). Versioned documentation. Save-directory API and platform differences.
[^23]: Orteil / DashNet / Playsaurus. [Cookie Clicker on Steam](https://store.steampowered.com/app/1454400/Cookie_Clicker/). Live store listing. Commercial edition; no regional price assumed.
[^24]: GameHive. [Tap Titans 2 Official Webstore](https://store.gamehive.com/). Live storefront and FAQ. Account-linked purchases and VIP progress.
[^25]: DreamXZE / Compendium contributors. [Tap Titans 2 Compendium](https://www.tt2-compendium.com/en). Undated homepage; shutdown notice visible at retrieval. Availability limitation.
