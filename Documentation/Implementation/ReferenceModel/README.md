# Published-model rerun

This runs the JavaScript recorded in Game Design/Economy Model v1.md against the authored catalogue. It is **not Unity runtime or human-play evidence**.

Command: `node Tools/RedesignLab/RunReferenceModel.cjs`. Node v24.14.0. Model SHA256 61E440D738D8F0E5C267538AB1C22D5602FE556C72AD8D102E6EE43F97597DF2; catalogue SHA256 77E1B9A96461E8BDAE1B1C21C7310FC7D4D1CE14AA33380FFD0CEAA8B3083250.

125 scenarios: 32 free histories, 16 per pack (four packs), 11 policy variants, 16 no-Legendary histories, sustained and matched-mixed diagnostics. First 16 free histories are paired controls. Seeds, options, exact active-time totals, events and daily records are retained in JSON. Management/reveal time and actual visit gaps are unchanged.

Free completion days (observed nearest-rank 10th/median/90th): 75 / 86 / 98. Completed by day 90: 23/32. All published cohort/policy figures were reproduced by `node Tools/RedesignLab/CheckReferenceReport.cjs`; see ../ReferenceModel-Checks.txt. Runtime/model parity is a separate check.
