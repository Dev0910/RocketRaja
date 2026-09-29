# Android release runbook

Working title: **Rocket Raja**. Name availability has not been checked. The game does not depend on the real festival date.

## Owner actions

1. Create/verify the Play Console developer account and AdMob account; choose final application ID and store developer name.
2. Own and securely back up the upload keystore. Configure Android Publishing Settings locally; never commit passwords or keys. Choose Play App Signing during store setup.
3. Recruit the required closed-test cohort. For qualifying new personal accounts, budget at least 12 opted-in testers for 14 continuous days, then apply for production access. Approval timing is not guaranteed. [Play testing requirements](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en-GB)
4. Review and publish an accurate privacy policy at a public HTTPS URL. Fill the draft's owner/contact details; verify SDK data collection and regional consent behavior.
5. Create an Android AdMob app and rewarded unit. Configure the real app ID in Assets > Google Mobile Ads > Settings. Create `ProductionConfig` from the Rocket Raja asset menu and save it as `Assets/Resources/ProductionConfig.asset`; fill application ID, rewarded unit, privacy URL and enable productionAds.

## Device testing before closed-test distribution

- Install the test APK on ARM64 Android phones; test a short 16:9 screen and a tall phone with cutout/gesture navigation.
- Validate actual rewarded ads with test units: complete, dismiss, fail network, background during video, duplicate callback handling, consent required/not required and privacy-options form.
- Verify purchases and scrolling never create accidental scene taps, especially multitouch and rapid navigation.
- Force-stop after offline payout, prestige, Legacy purchase and reward; restart and verify balances/cooldowns. Corrupt main save in a test install and verify backup recovery. Preserve evidence before starting a fresh save.
- Test 8-hour and 18-hour offline caps, backward time, repeated focus/pause events and no internet.
- Play three complete prestige cycles without ads. Observe new testers without coaching. Simulator timings are development targets, not usability evidence.
- Profile a low-end physical phone for at least ten minutes after filling the shop: target sustained 30 FPS, acceptable temperature and memory, no ANRs. Windows frame rate is not evidence of Android performance.

## Production submission

- Confirm the current target API, supported SDK minimum, 64-bit and 16 KB page-size requirements in Play Console. The project selects the installed target API automatically and ARM64 IL2CPP; inspect the actual bundle before upload.
- Remove test identifiers from production configuration; use `Rocket Raja > Build Production Bundle`. Keep test advertising identifiers in development builds.
- Review title availability, store icon, feature graphic and screenshots. Use actual gameplay captures; avoid presenting unfinished features.
- Complete content rating, target audience (teens/adults), Contains ads, Data safety, privacy URL, app access and signing declarations. Audit Google's advertising SDK disclosures for the included version; do not claim that no data is collected merely because saves are local.
- Upload to closed testing first. Inspect pre-launch report, crashes/ANRs and Android vitals. Use a staged rollout when approved.

## Schedule

September 14–20: project/account foundations. September 21–27: full shop and offline saves. September 28–October 4: prestige and closed-test build. October 5–18: closed testing and refinements. October 19–25: production access/submission. October 26–November 1: review buffer, aiming for October 30 availability. Account verification and store approval are external gates.

## Integration references

- [Google Mobile Ads Unity setup](https://developers.google.com/admob/unity/quick-start)
- [Rewarded ads](https://developers.google.com/admob/unity/rewarded)
- [Unity consent workflow](https://developers.google.com/admob/unity/privacy)
- [India Post holiday calendar](https://www.indiapost.gov.in/holidays-list)

No separate analytics SDK, purchases, banners, interstitials, app-open ads, cloud accounts or multiplayer are implemented.
