# Tap Tycoon: game design and balance breakdown

Research date: 30 September 2026

Game: *Tap Tycoon – Country vs Country*, Game Hive Corporation

Purpose: a source-backed reference for our own incremental-game design work. This document analyzes Tap Tycoon; it does not prescribe changes to our game. See the separate comparison document for that work.

## 1. Evidence, method, and limits

This analysis combines Game Hive's [official game page](https://gamehive.com/games/tap-tycon/), the developer-authored text and release notes on [Google Play](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney) and the [Apple App Store](https://apps.apple.com/ca/app/tap-tycoon-country-vs-country/id1058709520), and detailed contemporary player records. The Game Hive URL really is spelled `tap-tycon`; the `tap-tycoon` URL supplied in the request was not available to the research tool. Player-authored guides and spreadsheets provide much more detail than the official descriptions, but they describe builds from roughly 2015–2017. A Game Hive representative said in [November 2022](https://www.reddit.com/r/TapTitans2/comments/s9dshu/beta_test_gamehives_newest_game/) that no team was actively updating Tap Tycoon at that time. Google Play lists a November 2025 update as a vulnerability fix; the visible iOS feature history ends in 2018. Neither fact proves that historical numbers still match today's game.

The evidence labels used below matter:

| Label | Meaning | Appropriate use |
| --- | --- | --- |
| **Official** | A Game Hive page or developer-supplied store description/version note. | Confirms advertised systems and feature names, usually not their numerical tuning. |
| **Community observation** | A player guide, wiki, or spreadsheet documenting a historical build. | Reconstructs rules and examples from the observed era; not proof of a current formula. |
| **Player report** | An individual review or critique. | Identifies a possible experience or failure mode; not a representative survey. |
| **Analysis** | The design interpretation in this document. | Explains why documented systems may work together; it is an inference. |

No official balance table, retention data, exact production/cost equation, or public Game Hive source code was found in the material reviewed. This research did not instrument a live client. Videos indexed during the search mostly repeated the store description; no precise economy values were inferred from footage. Consequently, claims such as “this is the optimal prestige time,” “every first business remains competitive,” or “the current game uses this formula” would exceed the evidence.

## 2. Design at a glance

The **official** pitch is unusually clear: tapping creates money; money funds a growing portfolio of businesses; the portfolio contributes to a country-versus-country competition that pays weekly medals and rewards. Game Hive presents the game as a whimsical 35th-century conglomerate, with inventions such as an anti-gravity fork, luggage shrink ray, and holographic window. These themes turn otherwise abstract multiplier purchases into discoverable products and technologies. [Game Hive](https://gamehive.com/games/tap-tycon/) · [Google Play](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney)

The documented loops operate at several time scales:

| Time scale | Player action and feedback | What advances |
| --- | --- | --- |
| Seconds | Tap for cash; see money appear; buy a level or collect business revenue. | Immediate agency and visible number growth. |
| Minutes | Compare the next business/character upgrade, reach a level milestone, activate and combine timed skills. | A faster run and new purchase targets. |
| Hours/days | Return to idle income, unlock automated collection, push to a new net-worth band, then prestige. | Permanent bonus and possibly persistent cards. |
| Week | Send soldiers through prestiges, contribute to a country's war, collect placement rewards; later versions also added Investment Groups and a daily group challenge. | Social status, medals, cards, and group income bonuses. |

The seconds-to-weeks structure is an **analysis** of the systems documented in the [official store copy](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney), [official version notes](https://apps.apple.com/ca/app/tap-tycoon-country-vs-country/id1058709520), and [community gameplay account](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/). It is not a claim about measured retention.

## 3. Cash, tapping, and businesses

### Two income channels

The official description starts the player with direct tapping to create money, then moves that money into businesses. Google Play classifies the game as idle and offline. A [2016 player guide](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) describes active tap income and business income that can continue while the app is closed. It describes business revenue as collectable and explains that automation is unlocked rather than granted to every business immediately. A fan [unlockables reference](https://tap-tycoon.fandom.com/wiki/Unlockables) likewise lists one Auto Collect upgrade for each of the ten businesses. The exact offline percentage and cap are **not** established by these sources.

The distinction creates a useful investment choice. Direct tapping funds the first purchases quickly; businesses provide persistent run income and can finance further purchases when the player is not tapping. The income paths interact later through skills and Transforming, rather than remaining entirely separate.

### Ten businesses and visible milestones

A [community business reference](https://tap-tycoon.fandom.com/wiki/Business) records ten businesses in unlock order: Restaurant, Entertainment, Post Office, Power Plant, Hotel, Department Store, Armory, Theme Park, Bank, and Media Center. A [contemporary guide](https://throwgame.wordpress.com/2016/06/21/tap-tycoon-beginner-guide/) says the business panel lets players inspect a business's next bonus. That is important interface design: a level purchase has an immediate return and a visible threshold to chase.

The community records describe **two different milestone layers**:

1. An individual business's level milestones raise its own collection profit or speed. A [2016 review](https://www.thetechgame.com/Archives/t%3D7678418/tap-tycoon-review.html) reported a speed milestone at business level 10. That is a historical example, not a verified universal schedule.
2. Bringing **all** businesses to certain levels gives a portfolio-wide profit or speed bonus. One [2016 player account](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) names thresholds such as 25, 50, 100, and 200 and describes roughly ×2/×3 global bonuses. The exact multiplier attached to each threshold is not verified.

**Analysis:** the second layer gives the cheapest-looking early business a second role: it can be the final level needed to activate a multiplier across the entire portfolio. A later business may still dominate direct income. The documented threshold structure only shows that early businesses have a *strategic reason* to be purchased again; it does not prove every early business has a competitive marginal return at every point in the run.

### Cash-priced unlockables keep returning to old businesses

Beyond level purchases, a player-maintained [Tap Tycoon Unlockables spreadsheet](https://docs.google.com/spreadsheets/d/1uGdqmtUNIKK3iJT1fyPMoQmZakOyXPX0GFkg0UqlMes/edit) says it catalogued 742 unlockables as of 28 May 2017. Its visible rows show business-specific multipliers, automation, tap multipliers, global profit, cost reductions, and skill unlocks interleaved by price. Selected entries are below; suffixes such as `aa` and `ab` are reproduced exactly as recorded, without assuming a conversion or current price.

| Historical cash price | Recorded unlockable | Design role |
| ---: | --- | --- |
| 40,000 | Restaurant profit ×5 | First business receives a named upgrade. |
| 52,500 | Restaurant Auto Collect | Automation follows the first business purchase. |
| 200,000 | Tap profit ×6 | Tapping gets a discrete step-up during business progression. |
| 450,000 | All profit ×3 | Multiple income lines are lifted together. |
| 13.1 million | Restaurant profit ×4 | Early business returns as a purchase target. |
| 4.3 billion | Restaurant profit ×5 | The first business continues to receive upgrades after later businesses appear. |
| 5.4 billion | All profit ×4 | A portfolio-wide upgrade competes with specific investments. |
| 222 billion | Tap profit ×5 | Another active-income step. |
| 8 trillion | All profit ×2 | Global scaling continues near the first-prestige band. |
| 25 trillion | Restaurant profit ×8 | The original business still has an upgrade beyond the historical first-prestige requirement. |
| 247 `aa` | Tap profit ×4 | Tap upgrades continue into letter-suffix wealth. |
| 880 `ab` | Restaurant profit ×5 | Restaurant-specific upgrades continue still later. |
| 700 `ad` | Restaurant profit ×4 | A later visible example of old-business support. |

These entries are **community observations from 2017**, not a full or present-day economy export. The sheet's 742 total is its author's claim; only its visible rows were inspected here. A first business receiving many multipliers can still have weak late-game income if its base output, level cost, or competing businesses scale faster. The [US App Store contains a 2016 player complaint](https://apps.apple.com/us/app/tap-tycoon-country-vs-country/id1058709520?platform=ipad) that Department Store purchases become far more expensive than more profitable alternatives after level 500, illustrating that this game also had perceived dead investments.

### What the published numbers do and do not reveal about cost progression

The spreadsheet gives **cash prices for one-time unlockables**, not a complete table of each business's next-level costs, base payout, collection interval, or offline payout. The business and prestige guides discuss increasing costs and changing purchase efficiency, but no inspected official source publishes their underlying equations. Thus the table above shows a long series of purchasing goals, not a calibrated proof that cost and income curves intersect at the ideal times. To test whether a historical business level was worthwhile, we would need at least its next-level cash cost and the resulting change in income per unit of time, including any immediate milestone or portfolio bonus. Those values cannot be reconstructed reliably from unlockable prices alone. [Unlockables spreadsheet](https://docs.google.com/spreadsheets/d/1uGdqmtUNIKK3iJT1fyPMoQmZakOyXPX0GFkg0UqlMes/edit) · [business milestone account](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/).

## 4. Character upgrades and active skill windows

A [historical gameplay guide](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) separates the Player menu from the Business and Tech Card menus. The Player menu upgrades tap profit, unlocks other run upgrades, exposes stats, and houses skills. The [beginner guide](https://throwgame.wordpress.com/2016/06/21/tap-tycoon-beginner-guide/) says individual business panels expose their next bonuses while Player upgrades cover tap income. This gives the player a recurring decision between growing the active and idle paths.

Five skills are documented by the community guide:

| Skill | Historically reported effect | Income path |
| --- | --- | --- |
| Quick Cash | Immediate cash based on business income. | Business burst. |
| Business Supercharged | Multiplies business earnings for a timed window. | Business acceleration. |
| Auto Tap | Generates automatic taps for a timed window. | Active-income acceleration. |
| Cash Stack Chance | Raises the chance of a larger tap payout. | Active-income variance and bursts. |
| Hand of Midas | Multiplies tap profit for a timed window. | Active-income burst. |

Source: [Throwgame's 2016 account](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/). A [fan unlockables reference](https://tap-tycoon.fandom.com/wiki/Unlockables) also lists these effects and reports 30-second windows for the timed skills, but flags some upgrade values as unconfirmed. The [unlockables spreadsheet](https://docs.google.com/spreadsheets/d/1uGdqmtUNIKK3iJT1fyPMoQmZakOyXPX0GFkg0UqlMes/edit) places Quick Cash's unlock at 1.2 million cash, Business Supercharged at 156 million, Auto Tap at 20.3 billion, Cash Stack Chance at 2.64 trillion, and Hand of Midas at 343 trillion in its 2017 record.

The skills reportedly stack, so a player can line up business bursts or tap bursts instead of treating each button as an isolated multiplier. The [beginner guide](https://throwgame.wordpress.com/2016/06/21/tap-tycoon-beginner-guide/) specifically recommends combining them. That creates a short session with a clear preparation-payoff rhythm: wait for cooldowns, prepare purchases, trigger skills, then use the proceeds to pass the next threshold. It also introduces a cost. The same [2016 review](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) says ordinary tapping can feel insignificant outside skill windows and that there may be little productive active play while skills cool down. This is a documented limitation of Tap Tycoon's design, not a problem it entirely solved.

## 5. Transforming: a late-game bridge from idle to tap

A dedicated [June 2016 Transforming guide](https://throwgame.wordpress.com/2016/06/23/tap-tycoon-transforming/) reports that tap level 5,000 unlocks a state called **Transforming**. The author says the player must reach that level again after each prestige. In that observed build, Transforming added an amount equal to **1% of business profit** to tap profit in addition to the tap build's normal output. The guide further says business and tap skills interact through the converted amount. Its displayed arithmetic has ambiguous grouping, so it should not be copied as a proprietary income formula.

The design effect is clearer than the exact arithmetic: active tapping regains a link to the strongest economy path. When businesses grow, the tap channel can grow with them; upgrades to businesses can also affect a player's active play. This creates a later aspiration for players who prefer tapping. Its late threshold and per-prestige reset also mean it cannot solve a *midgame* tap-power gap by itself. All of these are **analysis of historical community observations**, not a statement about the current client.

## 6. Tech Cards: persistent bonuses and specialization

The [official App Store history](https://apps.apple.com/ca/app/tap-tycoon-country-vs-country/id1058709520) confirms Tech Cards, a Card Shop, and a later marketplace fix. A [2016 community card catalogue](https://throwgame.wordpress.com/2016/02/26/tap-tycoon-tech-cards-list/) describes persistent cards with different targets: a single business, multiple businesses, all profit, all upgrade costs, tap profit, cash stacks, and skill output. Its rows group business cards by the ten business types. Each business row includes a leader card whose effect depends on other cards in that row. The author explicitly marked some card values as predictions, so individual card values are less certain than the existence of the broad categories.

Examples from that historical catalogue show the build-shaping possibilities:

| Card example | Community-recorded effect | Why it matters |
| --- | --- | --- |
| Mental Menu | Restaurant profit ×3 | A persistent reason to favor the first business. |
| Weed Roaster | All profit ×1.5 and tap profit ×2 | Bridges global and active growth. |
| Jell Strong | All upgrade cost ×0.8 and tap profit ×2 | Combines cheaper purchases with active play. |
| Hover-brella | All upgrade cost ×0.4 | Changes the value of every future cash purchase. |
| A business leader card | Increases with cards owned for that business | Makes collection depth meaningful, rather than only individual drops. |

Source: [community Tech Card catalogue](https://throwgame.wordpress.com/2016/02/26/tap-tycoon-tech-cards-list/); values are reported for the recorded era and some are uncertain. A [separate 2016 analysis](https://throwgame.wordpress.com/2016/02/25/tap-tycoon-tech-cards/) says duplicate cards level up, normal-card levels and gold-card levels grow at different rates, and the then-current shop rotated two normal and one gold card daily. It reports 100 diamonds for a chosen normal card, 500 for a chosen gold card, or 500 for five random cards. A [December 2015 guide](https://www.touchtapplay.com/tap-tycoon-guide-to-prestige-how-to-get-more-tech-cards/) instead mentions a 200-diamond purchase, demonstrating that prices and packages changed or that contemporary guides disagreed. Do not treat either as a current price list.

**Analysis:** cards add persistent, partly collectible progress to otherwise repeating prestige runs. A Restaurant card can make an early business more attractive; an all-profit card is broadly useful; a tap card supports a different play style. Duplicate leveling adds a long tail. Random drops also create variance in early progression: the [2016 gameplay account](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) notes that a poor first card draw can make the first prestiges feel slower. The Card Shop later gave players some targeted choice, but did not remove the acquisition cost.

## 7. Prestige: reset, permanent growth, and war contribution

Prestige is where Tap Tycoon combines several motivations. Contemporary [December 2015](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards/) and [2016](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) guides describe three possible outcomes of a reset: a permanent income bonus, soldiers for the country war, and Tech Cards at qualifying progress points. The guides place the first prestige requirement at roughly **20–21 trillion**; one describes the initial requirement as cash while another discusses maximum net worth, so even this threshold should be presented as a historical approximate rather than an exact current rule.

| On prestige | Historical evidence | Confidence |
| --- | --- | --- |
| Cash and purchased businesses reset and a fresh run begins. | [WriterParty guide](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards/); [Touch, Tap, Play guide](https://www.touchtapplay.com/tap-tycoon-guide-to-prestige-how-to-get-more-tech-cards/). | High for the historical core reset. |
| Run-purchased unlockables are lost on an ordinary prestige according to the fan wiki, which notes a diamond-spending exception; Transforming must be regained. | [Fan unlockables reference](https://tap-tycoon.fandom.com/wiki/Unlockables); [Transforming guide](https://throwgame.wordpress.com/2016/06/23/tap-tycoon-transforming/). | Medium, historical. |
| A prestige bonus raises later income, including tap and business earnings. | [WriterParty prestige guide](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards/). | Medium, historical; exact equation not established. |
| Soldiers support the weekly country competition. | [WriterParty prestige guide](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards/); official [country-war pitch](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney). | High for the linkage in the documented era. |
| Tech Cards may be obtained through prestige/progress and persist across runs. | [WriterParty guide, page 2](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards-page-2/); [community card guide](https://throwgame.wordpress.com/2016/02/25/tap-tycoon-tech-cards/). | Medium, historical; payout conditions differ by source. |

The optimal reset point is **not** known. One 2015 guide recommended pushing farther for soldiers/cards; the [June 2016 beginner guide](https://throwgame.wordpress.com/2016/06/21/tap-tycoon-beginner-guide/) corrected an earlier soldier calculation and favored short runs once progress stalls. The correction is evidence that the formula or community understanding changed. Avoid using a fixed “prestige every X hours” recommendation as a claim about Tap Tycoon.

The design principle is stronger than the uncertain numbers: a reset converts progress into **immediate next-run power**, **a persistent collection**, and **a contribution to a time-limited social goal**. It can feel worthwhile for different reasons to different players. The reset also has an opportunity cost: run unlockables and Transforming must be rebuilt, so the player has reason to compare a little more current-run progress with a faster next run.

## 8. Weekly competition and Investment Groups

The official game description says players combine forces with people from their country and that the countries with the most powerful businesses earn medals and rewards each week. [Google Play](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney) · [Game Hive](https://gamehive.com/games/tap-tycon/). The [historical prestige guide](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards/) describes soldiers awarded through prestige and contributed to weekly wars. A [2016 player account](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) records Tech Cards as part of leaderboard rewards, with reward tiers based on placement in the player's country. Exact current ranking, reset, and reward rules are unverified.

Investment Groups are a **separate** cooperative layer. The [official iOS release notes](https://apps.apple.com/ca/app/tap-tycoon-country-vs-country/id1058709520) added groups “like clans” and an Investment Game in April 2016, then added a daily Gold Digging challenge in June 2016 that awarded an income bonus to the group. A [2016 beginner guide](https://throwgame.wordpress.com/2016/06/21/tap-tycoon-beginner-guide/) describes group cooperation and the daily investment decision. This extends the game beyond an isolated multiplier chase: players can have a daily group obligation and a weekly country objective alongside their private prestige loop.

The risk is also social. A [2016 player critique](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) says a new player can feel far behind long-established leaderboard participants and asked for more reward tiers. Country competition may motivate engaged players while giving late entrants a weak sense of agency. The presence of a weekly event does not, on its own, make prestige satisfying.

## 9. Premium economy and optional boosts

The [App Store listing](https://apps.apple.com/ca/app/tap-tycoon-country-vs-country/id1058709520) confirms in-app purchases of Diamond packs; [Google Play](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney) labels the game as containing ads and purchases. Historical [player guides](https://throwgame.wordpress.com/2016/06/21/tap-tycoon-beginner-guide/) describe diamonds spent on Tech Cards or temporary perks, including instant cash, cooldown reset, and a timed profit multiplier. A [2016 gameplay account](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) describes occasional rocket rewards and optional rewarded ads. The [official version notes](https://apps.apple.com/ca/app/tap-tycoon-country-vs-country/id1058709520) independently confirm Fat Cat and double-profit perks, though they do not explain the complete reward model.

**Analysis:** permanent cards and temporary accelerators compete for the same premium currency. Permanent options support long-term identity and prestige recovery; temporary options relieve a near-term wall. This is a meaningful design trade-off, but it can become frustrating if crucial permanent power is scarce or if players feel pressured to buy cash to reach prestige. A [2016 App Store player report](https://apps.apple.com/us/app/tap-tycoon-country-vs-country/id1058709520?platform=ipad) explicitly complained that cards and gems were too rare for their pace of play. Present-day store prices, ad rates, and reward probabilities were not verified.

## 10. Why the design can feel balanced, and where it did not

The following are **design interpretations**, grounded in the documented systems rather than access to Game Hive's balancing process:

1. **There is usually a nearby and a distant goal.** A next business level or unlockable gives an immediate target. A full-portfolio threshold, prestige, card collection, or weekly war gives a longer one. [Community business guide](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) · [Official country loop](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney).
2. **Old businesses get multiple jobs.** Their own level milestones, all-business thresholds, recurring named upgrades, and targeted Tech Cards make them relevant in more than one calculation. That improves perceived usefulness, although actual late-game return on investment remains unverified. [Business reference](https://tap-tycoon.fandom.com/wiki/Business) · [2017 unlockables sheet](https://docs.google.com/spreadsheets/d/1uGdqmtUNIKK3iJT1fyPMoQmZakOyXPX0GFkg0UqlMes/edit) · [card catalogue](https://throwgame.wordpress.com/2016/02/26/tap-tycoon-tech-cards-list/).
3. **Active and idle play are connected.** Tap upgrades recur, skills create coordinated bursts, and historical Transforming ties some business power to tapping. [Unlockables sheet](https://docs.google.com/spreadsheets/d/1uGdqmtUNIKK3iJT1fyPMoQmZakOyXPX0GFkg0UqlMes/edit) · [Transforming guide](https://throwgame.wordpress.com/2016/06/23/tap-tycoon-transforming/).
4. **Prestige pays on more than one axis.** A reset can improve the next run, expand the persistent card set, and support a country war. That gives it a broader meaning than a single percentage gain. [Historical prestige guide](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards/) · [official country reward description](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney).
5. **Flavor gives abstract numbers a setting.** Odd future-business and invention names turn multipliers into discoveries. [Game Hive page](https://gamehive.com/games/tap-tycon/) · [Google Play](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney).

The sources also show meaningful weaknesses:

| Reported friction | Evidence | Design implication |
| --- | --- | --- |
| First prestige feels long, and business Auto Collect arrives too late. | [2024 Google Play player review](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney). | The first reset and automation need careful pacing even in a game with strong later loops. |
| Baseline tapping becomes weak relative to bursts. | [2016 community review](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/). | Timed skill power cannot fully substitute for satisfying ordinary taps. |
| A late Department Store upgrade seems expensive versus more profitable businesses. | [2016 App Store player review](https://apps.apple.com/us/app/tap-tycoon-country-vs-country/id1058709520?platform=ipad). | Global milestones can coexist with poor individual business economics. |
| Rare cards and weak early card luck slow the first prestiges. | [2016 App Store review](https://apps.apple.com/us/app/tap-tycoon-country-vs-country/id1058709520?platform=ipad); [2016 community review](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/). | Persistent rewards need a dependable baseline as well as exciting variance. |
| Established leaderboard players can make new entrants feel ineffective. | [2016 community review](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/). | Social rewards need achievable tiers for new players. |

These are individual reports and historical observations. They are reasons to test a design hypothesis, not proof of how often all players encountered the issue. Modern reviews also mention prestige or save failures; those are reliability concerns and should not be confused with intended game balance.

## 11. What to carry into a comparison with our game

The most transferable *patterns* are the two-layer business milestones, repeated upgrades for earlier businesses, distinct but connected active and idle income, a late active-income bridge, persistent build choices after prestige, and a reset that feeds a goal outside the current run. None requires copying Tap Tycoon's business names, prices, or proprietary formulas. To compare actual balance, our game needs its own measurements: each product's marginal income per cost, time to each unlock and prestige, the share of income from ordinary taps versus skills and idle production, and the value gained per reset. Tap Tycoon's public material does not supply equivalent measured curves, so the comparison should identify design mechanisms and testable targets rather than assert numerical parity.

## Source register

| Source | Type / era | What it supports |
| --- | --- | --- |
| [Game Hive Tap Tycoon page](https://gamehive.com/games/tap-tycon/) | Official, indexed game page | Core pitch, global competition, rewards, theme. |
| [Google Play listing](https://play.google.com/store/apps/details?id=com.gamehivecorp.tapmoney) | Official developer text; currently listed | Tap/business/weekly loop, idle/offline label, current store status, selected player reviews. |
| [Apple App Store listing and version history](https://apps.apple.com/ca/app/tap-tycoon-country-vs-country/id1058709520) | Official developer notes, 2015–2018 | Feature chronology, cards, perks, groups, Gold Digging, IAP. |
| [Throwgame overview](https://throwgame.wordpress.com/2016/02/06/tap-tycoon/) | Community observation, 2016 | Income paths, skills, portfolio milestones, prestige/war/cards, qualitative critique. |
| [Throwgame beginner guide](https://throwgame.wordpress.com/2016/06/21/tap-tycoon-beginner-guide/) | Community observation, 2016 | UI, upgrade previews, shop/perks, skill use, changed soldier understanding. |
| [Throwgame Transforming guide](https://throwgame.wordpress.com/2016/06/23/tap-tycoon-transforming/) | Community observation, 2016 | Late-game business-to-tap bridge; historical 5,000 and 1% examples. |
| [Throwgame card list](https://throwgame.wordpress.com/2016/02/26/tap-tycoon-tech-cards-list/) and [card analysis](https://throwgame.wordpress.com/2016/02/25/tap-tycoon-tech-cards/) | Community catalogues, 2016 | Persistent card categories, examples, duplicate leveling, historical shop prices. |
| [Unlockables spreadsheet](https://docs.google.com/spreadsheets/d/1uGdqmtUNIKK3iJT1fyPMoQmZakOyXPX0GFkg0UqlMes/edit) | Player-maintained data, as of May 2017 | Concrete cost and multiplier examples, recurring early-business/tap upgrades. |
| [WriterParty prestige guide](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards/) and [page 2](https://writerparty.com/party/tap-tycoon-country-vs-country-prestige-guide-how-to-get-prestige-points-soldiers-and-tech-cards-page-2/) | Community guide, December 2015 | Historical first-prestige threshold, three reward paths, weekly soldiers/cards. |
| [Fan wiki Business](https://tap-tycoon.fandom.com/wiki/Business) and [Unlockables](https://tap-tycoon.fandom.com/wiki/Unlockables) | Community wiki, undated | Ten-business order, milestone and Auto Collect categories; pages were indexed but not fully available to the research tool. |
