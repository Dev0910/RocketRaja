# KK shop offers — 26 October 2026

Two Editor-authored rows in production Main precede the IAP packs:

- **24 hours of cash**, priced at **100 KK**. The row shows the exact grant: current normal automatic income × 86,400 seconds. Temporary Festival Rush and shop profit boosts do not inflate this amount. It credits cash, run earnings and lifetime earnings atomically. A zero-income purchase is disabled.
- **Double your profits**, priced at **150 KK**. Product earnings, manual taps, Helping Hands and Bulk Order use a 2× multiplier until the saved absolute expiry 24 hours later. The row displays time remaining. Repurchase is disabled during an active boost; prestige and restarts preserve the original expiry. Instant shop cash and achievement rewards are not doubled.

Offline settlement retains the existing cap and efficiency, doubling only credited seconds that precede boost expiry. Foreground accrual splits exactly at expiry. Purchases respect session ownership, save failure and premium-spending gates, with no currency or benefit committed if saving fails.

These offers use the existing local KK wallet. Account wallets backed by the premium authority remain disabled for these new purchase actions: the existing authority has no consumable-shop endpoint. No local debit is applied to a cached server wallet. The premium activation source also recognizes a guest's original 100-KK gift spent on shop cash so a future backend rollout will not reissue that gift. No backend deployment was performed.

Owners: `RedesignShopOffers`, `RedesignState`, `RedesignEngine`, `ClassicUI.ShopOffers`, `ClassicShopOffersAuthoring`, `BuildClassicPanels`, and `Main.unity`. Re-author with **Rocket Raja / Classic / Author KK shop offers**; objects, bindings and token material slots are saved in the Editor.

Validation: `dotnet run --project Tools/RedesignLab`, `node --test Backend/Premium/authority.test.mjs`, and `ClassicShopOffersValidation.Start()` in Unity Edit Mode. Native checks run through production Main with a disposable profile under Windows Temp. See `Validation.txt` and the native phone captures in this folder.
