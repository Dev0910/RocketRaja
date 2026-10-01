# Rocket Raja: 1,000 priced cash unlockables — cash-only v1

Updated: 1 October 2026. Planning only; no Unity data written.

All 1,000 existing IDs and names are retained. This edition replaces the previous unpriced effect placeholders with numerical base prices, effects, and concrete level requirements. [Economy Specification v1](Economy%20Specification%20v1.md) defines stacking, geometric level costs, skill behavior, prestige, and rounding. [Economy Simulation Report v1](Economy%20Simulation%20Report%20v1.md) records the executed tests and their limits.

**Cash-only progression is selected.** All 1,000 rows below now use its steeper late-game prices and have no completed-prestige requirements. The previous milestone-gated option is superseded. The [price table](Cash-only%20Price%20Table%20v1.md) mirrors these values; the catalogue and specification are the current source of truth. Numerical values remain a planning baseline, not shipping validation.

All purchases are run-local; buy again after prestige. Every ID counts once toward lifetime U, including the three base skill unlocks. Cards and product/tap levels are outside this 1,000 count. Visibility alone and rebuys never increment U. Purchasing remains manual with scoped Buy All.

## Numerical conventions

- Base cash prices are rounded to six significant digits at authoring. Apply current run/card named-cash discounts afterward, without refunding prior purchases.
- A cumulative discount or duration/cooldown factor replaces that chain's previous factor. Ordinary profit multipliers stack across purchased entries.
- PP signatures have one complete effect, not a supporting multiplier plus a second signature reward.
- An XL source gate applies to the first family in its ID; the second benefits. PF level requirements apply to every listed family.
- TD/TL effects use the exact formulas in the specification. Displayed percentages and duration/cooldown factors rounded in a row are explanatory, not replacements for the exact formula.
- Meet each row's named ownership, level, and prior-upgrade prerequisites and afford its discounted price. No prestige-count, calendar, or premium-purchase requirement applies to catalogue access.
- Key B03-007 identifies band 3, position 7. The price formula and all 20 opening values are in the specification.

## Inventory

| Type | Count |
|---|---:|
| Product profit | 480 |
| Product level cost | 128 |
| Tap profit (60 TD + 60 TL) | 120 |
| Tap level cost | 40 |
| Global profit | 80 |
| Named-cash cost | 32 |
| Skills | 72 |
| Product links | 32 |
| Product groups | 16 |
| Total | 1,000 |

## Band 01

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-01 | Aerial shots Signature Breakthrough I | PP(AER,1): multiply family normal profit by x2; complete signature effect | Own Aerial shots | B01-001 | 100 |
| PP-ANA-01 | Anar: Gift Packaging I | PP(ANA,1): multiply family normal profit by x1.2 | Own Anar | B01-002 | 103.619 |
| PP-BOX-01 | Celebration boxes: Gift Packaging I | PP(BOX,1): multiply family normal profit by x1.2 | Own Celebration boxes | B01-003 | 107.368 |
| PP-CHA-01 | Chakri: Gift Packaging I | PP(CHA,1): multiply family normal profit by x1.2 | Own Chakri | B01-004 | 111.253 |
| PP-PHU-01 | Phuljhari Signature Breakthrough I | PP(PHU,1): multiply family normal profit by x2; complete signature effect | Own Phuljhari | B01-005 | 115.279 |
| PP-ROC-01 | Rockets: Gift Packaging I | PP(ROC,1): multiply family normal profit by x1.2 | Own Rockets | B01-006 | 119.45 |
| PP-SMC-01 | Small crackers: Gift Packaging I | PP(SMC,1): multiply family normal profit by x1.2 | Own Small crackers | B01-007 | 123.773 |
| PP-STR-01 | Cracker strings: Gift Packaging I | PP(STR,1): multiply family normal profit by x1.2 | Own Cracker strings | B01-008 | 128.251 |
| PP-AER-02 | Aerial shots: Festival Display I | PP(AER,2): multiply family normal profit by x1.2 | Own Aerial shots; PP-AER-01 | B01-009 | 132.892 |
| PP-ANA-02 | Anar: Festival Display I | PP(ANA,2): multiply family normal profit by x1.2 | Own Anar; PP-ANA-01 | B01-010 | 137.701 |
| PP-BOX-02 | Celebration boxes: Festival Display I | PP(BOX,2): multiply family normal profit by x1.2 | Own Celebration boxes; PP-BOX-01 | B01-011 | 142.684 |
| PP-CHA-02 | Chakri Signature Breakthrough I | PP(CHA,2): multiply family normal profit by x2; complete signature effect | Own Chakri; PP-CHA-01 | B01-012 | 147.847 |
| PP-PHU-02 | Phuljhari: Festival Display I | PP(PHU,2): multiply family normal profit by x1.2 | Own Phuljhari; PP-PHU-01 | B01-013 | 153.197 |
| PP-ROC-02 | Rockets: Festival Display I | PP(ROC,2): multiply family normal profit by x1.2 | Own Rockets; PP-ROC-01 | B01-014 | 158.74 |
| PP-SMC-02 | Small crackers: Festival Display I | PP(SMC,2): multiply family normal profit by x1.2 | Own Small crackers; PP-SMC-01 | B01-015 | 164.484 |
| PP-STR-02 | Cracker strings: Festival Display I | PP(STR,2): multiply family normal profit by x1.2 | Own Cracker strings; PP-STR-01 | B01-016 | 170.436 |
| PP-AER-03 | Aerial shots: Regular Customers I | PP(AER,3): multiply family normal profit by x1.2 | Own Aerial shots; PP-AER-02 | B01-017 | 176.603 |
| PP-ANA-03 | Anar: Regular Customers I | PP(ANA,3): multiply family normal profit by x1.2 | Own Anar; PP-ANA-02 | B01-018 | 182.994 |
| PP-BOX-03 | Celebration boxes: Regular Customers I | PP(BOX,3): multiply family normal profit by x1.2 | Own Celebration boxes; PP-BOX-02 | B01-019 | 189.616 |
| PP-CHA-03 | Chakri: Regular Customers I | PP(CHA,3): multiply family normal profit by x1.2 | Own Chakri; PP-CHA-02 | B01-020 | 196.477 |
| PP-PHU-03 | Phuljhari: Regular Customers I | PP(PHU,3): multiply family normal profit by x1.2 | Own Phuljhari; PP-PHU-02 | B01-021 | 203.586 |
| PP-ROC-03 | Rockets: Regular Customers I | PP(ROC,3): multiply family normal profit by x1.2 | Own Rockets; PP-ROC-02 | B01-022 | 210.953 |
| PP-SMC-03 | Small crackers Signature Breakthrough I | PP(SMC,3): multiply family normal profit by x2; complete signature effect | Own Small crackers; PP-SMC-02 | B01-023 | 218.587 |
| PP-STR-03 | Cracker strings: Regular Customers I | PP(STR,3): multiply family normal profit by x1.2 | Own Cracker strings; PP-STR-02 | B01-024 | 226.496 |
| TP-001 | Sales Pitch I | TD(1): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families | B01-025 | 234.692 |
| TP-002 | Counter Display I | TD(2): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-001 | B01-026 | 243.185 |
| TP-003 | Shop Showmanship I | TL(1): linked share A becomes 12% | Own 2+ families; TP-002 | B01-027 | 251.984 |
| TP-004 | Crowd Appeal I | TL(2): linked share A becomes 12.30508475% | Own 2+ families; TP-003 | B01-028 | 261.102 |
| TP-005 | Festival Calls I | TD(3): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-004 | B01-029 | 270.55 |
| TP-006 | Shop Reputation I | TL(3): linked share A becomes 12.61016949% | Own 2+ families; TP-005 | B01-030 | 280.34 |
| TC-01 | Practice Routine I | TC(1): total run tap-level discount becomes 0.875% | Own 1+ families | B01-031 | 290.485 |
| TC-02 | Counter Training I | TC(2): total run tap-level discount becomes 1.75% | Own 1+ families; TC-01 | B01-032 | 300.996 |
| GP-01 | Festival Footfall I | GP(1): normal products and independent tap B x1.10, once | Own 1+ families | B01-033 | 311.887 |
| GP-02 | Community Trust I | GP(2): normal products and independent tap B x1.10, once | Own 1+ families; GP-01 | B01-034 | 323.173 |
| GP-03 | Local Advertising I | GP(3): normal products and independent tap B x1.10, once | Own 1+ families; GP-02 | B01-035 | 334.867 |
| GP-04 | Shopfront Glow I | GP(4): normal products and independent tap B x1.10, once | Own 1+ families; GP-03 | B01-036 | 346.985 |
| UC-01 | Supplier Network I | UC(1): total run named-cash discount becomes 1.09375% | Own 1+ families | B01-037 | 359.54 |
| SK-BO-P-01 | Bulk Order: Unlock | SP(BO,1): 120 seconds of normal P; unlock skill | Own 3+ families | B01-038 | 372.55 |
| SK-FR-P-01 | Festival Rush: Unlock | SP(FR,1): Rush product multiplier x2; unlock skill | Own 1+ families | B01-039 | 386.031 |
| SK-HH-P-01 | Helping Hands: Unlock | SP(HH,1): 3 auto taps/s; unlock skill | Own 2+ families | B01-040 | 400 |

## Band 02

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-04 | Aerial shots: Signature Collection I | PP(AER,4): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-03 | B02-001 | 3000000 |
| PP-ANA-04 | Anar Signature Breakthrough I | PP(ANA,4): multiply family normal profit by x2; complete signature effect | Own Anar; PP-ANA-03 | B02-002 | 3084340 |
| PP-BOX-04 | Celebration boxes Signature Breakthrough I | PP(BOX,4): multiply family normal profit by x2; complete signature effect | Own Celebration boxes; PP-BOX-03 | B02-003 | 3171050 |
| PP-CHA-04 | Chakri: Signature Collection I | PP(CHA,4): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-03 | B02-004 | 3260200 |
| PP-PHU-04 | Phuljhari: Signature Collection I | PP(PHU,4): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-03 | B02-005 | 3351860 |
| PP-ROC-04 | Rockets: Signature Collection I | PP(ROC,4): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-03 | B02-006 | 3446100 |
| PP-SMC-04 | Small crackers: Signature Collection I | PP(SMC,4): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-03 | B02-007 | 3542980 |
| PP-STR-04 | Cracker strings: Signature Collection I | PP(STR,4): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-03 | B02-008 | 3642580 |
| PP-AER-05 | Aerial shots: Celebration Specialist I | PP(AER,5): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-04 | B02-009 | 3744990 |
| PP-ANA-05 | Anar: Celebration Specialist I | PP(ANA,5): multiply family normal profit by x1.6 | Own Anar; PP-ANA-04 | B02-010 | 3850280 |
| PP-BOX-05 | Celebration boxes: Celebration Specialist I | PP(BOX,5): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-04 | B02-011 | 3958520 |
| PP-CHA-05 | Chakri: Celebration Specialist I | PP(CHA,5): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-04 | B02-012 | 4069810 |
| PP-PHU-05 | Phuljhari: Celebration Specialist I | PP(PHU,5): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-04 | B02-013 | 4184230 |
| PP-ROC-05 | Rockets Signature Breakthrough I | PP(ROC,5): multiply family normal profit by x2; complete signature effect | Own Rockets; PP-ROC-04 | B02-014 | 4301870 |
| PP-SMC-05 | Small crackers: Celebration Specialist I | PP(SMC,5): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-04 | B02-015 | 4422810 |
| PP-STR-05 | Cracker strings: Celebration Specialist I | PP(STR,5): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-04 | B02-016 | 4547150 |
| PP-AER-06 | Aerial shots: Premium Range I | PP(AER,6): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-05 | B02-017 | 4674990 |
| PP-ANA-06 | Anar: Premium Range I | PP(ANA,6): multiply family normal profit by x1.6 | Own Anar; PP-ANA-05 | B02-018 | 4806420 |
| PP-BOX-06 | Celebration boxes: Premium Range I | PP(BOX,6): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-05 | B02-019 | 4941550 |
| PP-CHA-06 | Chakri: Premium Range I | PP(CHA,6): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-05 | B02-020 | 5080470 |
| PP-PHU-06 | Phuljhari: Premium Range I | PP(PHU,6): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-05 | B02-021 | 5223300 |
| PP-ROC-06 | Rockets: Premium Range I | PP(ROC,6): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-05 | B02-022 | 5370150 |
| PP-SMC-06 | Small crackers: Premium Range I | PP(SMC,6): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-05 | B02-023 | 5521130 |
| PP-STR-06 | Cracker strings Signature Breakthrough I | PP(STR,6): multiply family normal profit by x2; complete signature effect | Own Cracker strings; PP-STR-05 | B02-024 | 5676350 |
| PC-AER-01 | Aerial shots: Wholesale Deal I | PC(AER,1): total run family-level discount becomes 2.1875% | Own Aerial shots | B02-025 | 5835930 |
| PC-ANA-01 | Anar: Wholesale Deal I | PC(ANA,1): total run family-level discount becomes 2.1875% | Own Anar | B02-026 | 6000000 |
| PC-BOX-01 | Celebration boxes: Wholesale Deal I | PC(BOX,1): total run family-level discount becomes 2.1875% | Own Celebration boxes | B02-027 | 6168680 |
| PC-CHA-01 | Chakri: Wholesale Deal I | PC(CHA,1): total run family-level discount becomes 2.1875% | Own Chakri | B02-028 | 6342110 |
| PC-PHU-01 | Phuljhari: Wholesale Deal I | PC(PHU,1): total run family-level discount becomes 2.1875% | Own Phuljhari | B02-029 | 6520410 |
| PC-ROC-01 | Rockets: Wholesale Deal I | PC(ROC,1): total run family-level discount becomes 2.1875% | Own Rockets | B02-030 | 6703720 |
| PC-SMC-01 | Small crackers: Wholesale Deal I | PC(SMC,1): total run family-level discount becomes 2.1875% | Own Small crackers | B02-031 | 6892190 |
| PC-STR-01 | Cracker strings: Wholesale Deal I | PC(STR,1): total run family-level discount becomes 2.1875% | Own Cracker strings | B02-032 | 7085960 |
| TP-007 | Sales Pitch II | TD(4): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-006 | B02-033 | 7285170 |
| TP-008 | Counter Display II | TD(5): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-007 | B02-034 | 7489980 |
| TP-009 | Shop Showmanship II | TL(4): linked share A becomes 12.91525424% | Own 2+ families; TP-008 | B02-035 | 7700560 |
| TP-010 | Crowd Appeal II | TL(5): linked share A becomes 13.22033898% | Own 2+ families; TP-009 | B02-036 | 7917050 |
| TP-011 | Festival Calls II | TD(6): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-010 | B02-037 | 8139630 |
| TP-012 | Shop Reputation II | TL(6): linked share A becomes 13.52542373% | Own 2+ families; TP-011 | B02-038 | 8368460 |
| TC-03 | Sales Coaching I | TC(3): total run tap-level discount becomes 2.625% | Own 1+ families; TC-02 | B02-039 | 8603730 |
| TC-04 | Team Handbook I | TC(4): total run tap-level discount becomes 3.5% | Own 1+ families; TC-03 | B02-040 | 8845620 |
| GP-05 | Delivery Reach I | GP(5): normal products and independent tap B x1.10, once | Own 1+ families; GP-04 | B02-041 | 9094300 |
| GP-06 | Celebration Demand I | GP(6): normal products and independent tap B x1.10, once | Own 1+ families; GP-05 | B02-042 | 9349970 |
| GP-07 | Brand Recognition I | GP(7): normal products and independent tap B x1.10, once | Own 1+ families; GP-06 | B02-043 | 9612840 |
| GP-08 | Customer Loyalty I | GP(8): normal products and independent tap B x1.10, once | Own 1+ families; GP-07 | B02-044 | 9883090 |
| UC-02 | Trade Association I | UC(2): total run named-cash discount becomes 2.1875% | Own 1+ families; UC-01 | B02-045 | 10160900 |
| UC-03 | Procurement Office I | UC(3): total run named-cash discount becomes 3.28125% | Own 1+ families; UC-02 | B02-046 | 10446600 |
| SK-FR-D-01 | Festival Rush: Longer Window I | SD(FR,1): cumulative run duration factor 1.0625 | SK-FR-P-01 | B02-047 | 10740300 |
| SK-HH-D-01 | Helping Hands: Longer Window I | SD(HH,1): cumulative run duration factor 1.0625 | SK-HH-P-01 | B02-048 | 11042300 |
| SK-BO-C-01 | Bulk Order: Faster Recovery I | SC(BO,1): cumulative run cooldown factor 0.97916667 | SK-BO-P-01 | B02-049 | 11352700 |
| SK-FR-C-01 | Festival Rush: Faster Recovery I | SC(FR,1): cumulative run cooldown factor 0.96875 | SK-FR-P-01 | B02-050 | 11671900 |
| SK-HH-C-01 | Helping Hands: Faster Recovery I | SC(HH,1): cumulative run cooldown factor 0.96875 | SK-HH-P-01 | B02-051 | 12000000 |

## Band 03

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-07 | Aerial shots Signature Breakthrough II | PP(AER,7): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-06 | B03-001 | 72000000 |
| PP-ANA-07 | Anar: Gift Packaging II | PP(ANA,7): multiply family normal profit by x1.6 | Own Anar; PP-ANA-06 | B03-002 | 73908100 |
| PP-BOX-07 | Celebration boxes: Gift Packaging II | PP(BOX,7): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-06 | B03-003 | 75866800 |
| PP-CHA-07 | Chakri: Gift Packaging II | PP(CHA,7): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-06 | B03-004 | 77877400 |
| PP-PHU-07 | Phuljhari Signature Breakthrough II | PP(PHU,7): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-06 | B03-005 | 79941300 |
| PP-ROC-07 | Rockets: Gift Packaging II | PP(ROC,7): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-06 | B03-006 | 82059800 |
| PP-SMC-07 | Small crackers: Gift Packaging II | PP(SMC,7): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-06 | B03-007 | 84234500 |
| PP-STR-07 | Cracker strings: Gift Packaging II | PP(STR,7): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-06 | B03-008 | 86466900 |
| PP-AER-08 | Aerial shots: Festival Display II | PP(AER,8): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-07 | B03-009 | 88758400 |
| PP-ANA-08 | Anar: Festival Display II | PP(ANA,8): multiply family normal profit by x1.6 | Own Anar; PP-ANA-07 | B03-010 | 91110600 |
| PP-BOX-08 | Celebration boxes: Festival Display II | PP(BOX,8): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-07 | B03-011 | 93525200 |
| PP-CHA-08 | Chakri Signature Breakthrough II | PP(CHA,8): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-07 | B03-012 | 96003800 |
| PP-PHU-08 | Phuljhari: Festival Display II | PP(PHU,8): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-07 | B03-013 | 98548000 |
| PP-ROC-08 | Rockets: Festival Display II | PP(ROC,8): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-07 | B03-014 | 101160000 |
| PP-SMC-08 | Small crackers: Festival Display II | PP(SMC,8): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-07 | B03-015 | 103841000 |
| PP-STR-08 | Cracker strings: Festival Display II | PP(STR,8): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-07 | B03-016 | 106593000 |
| PP-AER-09 | Aerial shots: Regular Customers II | PP(AER,9): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-08 | B03-017 | 109417000 |
| PP-ANA-09 | Anar: Regular Customers II | PP(ANA,9): multiply family normal profit by x1.6 | Own Anar; PP-ANA-08 | B03-018 | 112317000 |
| PP-BOX-09 | Celebration boxes: Regular Customers II | PP(BOX,9): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-08 | B03-019 | 115294000 |
| PP-CHA-09 | Chakri: Regular Customers II | PP(CHA,9): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-08 | B03-020 | 118349000 |
| PP-PHU-09 | Phuljhari: Regular Customers II | PP(PHU,9): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-08 | B03-021 | 121486000 |
| PP-ROC-09 | Rockets: Regular Customers II | PP(ROC,9): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-08 | B03-022 | 124705000 |
| PP-SMC-09 | Small crackers Signature Breakthrough II | PP(SMC,9): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-08 | B03-023 | 128010000 |
| PP-STR-09 | Cracker strings: Regular Customers II | PP(STR,9): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-08 | B03-024 | 131403000 |
| PC-AER-02 | Aerial shots: Trusted Supplier I | PC(AER,2): total run family-level discount becomes 4.375% | Own Aerial shots; PC-AER-01 | B03-025 | 134885000 |
| PC-ANA-02 | Anar: Trusted Supplier I | PC(ANA,2): total run family-level discount becomes 4.375% | Own Anar; PC-ANA-01 | B03-026 | 138460000 |
| PC-BOX-02 | Celebration boxes: Trusted Supplier I | PC(BOX,2): total run family-level discount becomes 4.375% | Own Celebration boxes; PC-BOX-01 | B03-027 | 142129000 |
| PC-CHA-02 | Chakri: Trusted Supplier I | PC(CHA,2): total run family-level discount becomes 4.375% | Own Chakri; PC-CHA-01 | B03-028 | 145896000 |
| PC-PHU-02 | Phuljhari: Trusted Supplier I | PC(PHU,2): total run family-level discount becomes 4.375% | Own Phuljhari; PC-PHU-01 | B03-029 | 149762000 |
| PC-ROC-02 | Rockets: Trusted Supplier I | PC(ROC,2): total run family-level discount becomes 4.375% | Own Rockets; PC-ROC-01 | B03-030 | 153731000 |
| PC-SMC-02 | Small crackers: Trusted Supplier I | PC(SMC,2): total run family-level discount becomes 4.375% | Own Small crackers; PC-SMC-01 | B03-031 | 157805000 |
| PC-STR-02 | Cracker strings: Trusted Supplier I | PC(STR,2): total run family-level discount becomes 4.375% | Own Cracker strings; PC-STR-01 | B03-032 | 161987000 |
| TP-013 | Sales Pitch III | TD(7): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-012 | B03-033 | 166280000 |
| TP-014 | Counter Display III | TD(8): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-013 | B03-034 | 170687000 |
| TP-015 | Shop Showmanship III | TL(7): linked share A becomes 13.83050847% | Own 2+ families; TP-014 | B03-035 | 175210000 |
| TP-016 | Crowd Appeal III | TL(8): linked share A becomes 14.13559322% | Own 2+ families; TP-015 | B03-036 | 179854000 |
| TP-017 | Festival Calls III | TD(9): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-016 | B03-037 | 184620000 |
| TP-018 | Shop Reputation III | TL(9): linked share A becomes 14.44067797% | Own 2+ families; TP-017 | B03-038 | 189513000 |
| TC-05 | Customer Insight I | TC(5): total run tap-level discount becomes 4.375% | Own 1+ families; TC-04 | B03-039 | 194535000 |
| TC-06 | Practice Routine II | TC(6): total run tap-level discount becomes 5.25% | Own 1+ families; TC-05 | B03-040 | 199691000 |
| GP-09 | Festival Footfall II | GP(9): normal products and independent tap B x1.10, once | Own 1+ families; GP-08 | B03-041 | 204983000 |
| GP-10 | Community Trust II | GP(10): normal products and independent tap B x1.10, once | Own 1+ families; GP-09 | B03-042 | 210415000 |
| GP-11 | Local Advertising II | GP(11): normal products and independent tap B x1.10, once | Own 1+ families; GP-10 | B03-043 | 215991000 |
| GP-12 | Shopfront Glow II | GP(12): normal products and independent tap B x1.10, once | Own 1+ families; GP-11 | B03-044 | 221716000 |
| UC-04 | Distributor Alliance I | UC(4): total run named-cash discount becomes 4.375% | Own 1+ families; UC-03 | B03-045 | 227591000 |
| SK-BO-P-02 | Larger Bulk Orders II | SP(BO,2): 135 seconds of normal P | Own 3+ families; SK-BO-P-01 | B03-046 | 233623000 |
| XL-AER-BOX-01 | Aerial shots / Celebration boxes: Display Partnership I | XL(AER,BOX,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-047 | 239814000 |
| XL-ANA-CHA-01 | Anar / Chakri: Display Partnership I | XL(ANA,CHA,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-048 | 246170000 |
| XL-BOX-PHU-01 | Celebration boxes / Phuljhari: Display Partnership I | XL(BOX,PHU,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-049 | 252694000 |
| XL-CHA-ROC-01 | Chakri / Rockets: Display Partnership I | XL(CHA,ROC,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-050 | 259390000 |
| XL-PHU-ANA-01 | Phuljhari / Anar: Display Partnership I | XL(PHU,ANA,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-051 | 266265000 |
| XL-ROC-SMC-01 | Rockets / Small crackers: Display Partnership I | XL(ROC,SMC,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-052 | 273321000 |
| XL-SMC-STR-01 | Small crackers / Cracker strings: Display Partnership I | XL(SMC,STR,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-053 | 280565000 |
| XL-STR-AER-01 | Cracker strings / Aerial shots: Display Partnership I | XL(STR,AER,1): beneficiary normal product profit x1.50 | Own both families; Source level >= 25 | B03-054 | 288000000 |

## Band 04

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-10 | Aerial shots: Signature Collection II | PP(AER,10): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-09 | B04-001 | 2232000000 |
| PP-ANA-10 | Anar Signature Breakthrough II | PP(ANA,10): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-09 | B04-002 | 2291150000 |
| PP-BOX-10 | Celebration boxes Signature Breakthrough II | PP(BOX,10): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-09 | B04-003 | 2351870000 |
| PP-CHA-10 | Chakri: Signature Collection II | PP(CHA,10): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-09 | B04-004 | 2414200000 |
| PP-PHU-10 | Phuljhari: Signature Collection II | PP(PHU,10): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-09 | B04-005 | 2478180000 |
| PP-ROC-10 | Rockets: Signature Collection II | PP(ROC,10): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-09 | B04-006 | 2543850000 |
| PP-SMC-10 | Small crackers: Signature Collection II | PP(SMC,10): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-09 | B04-007 | 2611270000 |
| PP-STR-10 | Cracker strings: Signature Collection II | PP(STR,10): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-09 | B04-008 | 2680470000 |
| PP-AER-11 | Aerial shots: Celebration Specialist II | PP(AER,11): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-10 | B04-009 | 2751510000 |
| PP-ANA-11 | Anar: Celebration Specialist II | PP(ANA,11): multiply family normal profit by x1.6 | Own Anar; PP-ANA-10 | B04-010 | 2824430000 |
| PP-BOX-11 | Celebration boxes: Celebration Specialist II | PP(BOX,11): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-10 | B04-011 | 2899280000 |
| PP-CHA-11 | Chakri: Celebration Specialist II | PP(CHA,11): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-10 | B04-012 | 2976120000 |
| PP-PHU-11 | Phuljhari: Celebration Specialist II | PP(PHU,11): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-10 | B04-013 | 3054990000 |
| PP-ROC-11 | Rockets Signature Breakthrough II | PP(ROC,11): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-10 | B04-014 | 3135950000 |
| PP-SMC-11 | Small crackers: Celebration Specialist II | PP(SMC,11): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-10 | B04-015 | 3219060000 |
| PP-STR-11 | Cracker strings: Celebration Specialist II | PP(STR,11): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-10 | B04-016 | 3304370000 |
| PP-AER-12 | Aerial shots: Premium Range II | PP(AER,12): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-11 | B04-017 | 3391940000 |
| PP-ANA-12 | Anar: Premium Range II | PP(ANA,12): multiply family normal profit by x1.6 | Own Anar; PP-ANA-11 | B04-018 | 3481830000 |
| PP-BOX-12 | Celebration boxes: Premium Range II | PP(BOX,12): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-11 | B04-019 | 3574110000 |
| PP-CHA-12 | Chakri: Premium Range II | PP(CHA,12): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-11 | B04-020 | 3668830000 |
| PP-PHU-12 | Phuljhari: Premium Range II | PP(PHU,12): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-11 | B04-021 | 3766050000 |
| PP-ROC-12 | Rockets: Premium Range II | PP(ROC,12): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-11 | B04-022 | 3865860000 |
| PP-SMC-12 | Small crackers: Premium Range II | PP(SMC,12): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-11 | B04-023 | 3968310000 |
| PP-STR-12 | Cracker strings Signature Breakthrough II | PP(STR,12): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-11 | B04-024 | 4073480000 |
| PC-AER-03 | Aerial shots: Bulk Procurement I | PC(AER,3): total run family-level discount becomes 6.5625% | Own Aerial shots; PC-AER-02 | B04-025 | 4181430000 |
| PC-ANA-03 | Anar: Bulk Procurement I | PC(ANA,3): total run family-level discount becomes 6.5625% | Own Anar; PC-ANA-02 | B04-026 | 4292250000 |
| PC-BOX-03 | Celebration boxes: Bulk Procurement I | PC(BOX,3): total run family-level discount becomes 6.5625% | Own Celebration boxes; PC-BOX-02 | B04-027 | 4406000000 |
| PC-CHA-03 | Chakri: Bulk Procurement I | PC(CHA,3): total run family-level discount becomes 6.5625% | Own Chakri; PC-CHA-02 | B04-028 | 4522760000 |
| PC-PHU-03 | Phuljhari: Bulk Procurement I | PC(PHU,3): total run family-level discount becomes 6.5625% | Own Phuljhari; PC-PHU-02 | B04-029 | 4642630000 |
| PC-ROC-03 | Rockets: Bulk Procurement I | PC(ROC,3): total run family-level discount becomes 6.5625% | Own Rockets; PC-ROC-02 | B04-030 | 4765660000 |
| PC-SMC-03 | Small crackers: Bulk Procurement I | PC(SMC,3): total run family-level discount becomes 6.5625% | Own Small crackers; PC-SMC-02 | B04-031 | 4891960000 |
| PC-STR-03 | Cracker strings: Bulk Procurement I | PC(STR,3): total run family-level discount becomes 6.5625% | Own Cracker strings; PC-STR-02 | B04-032 | 5021600000 |
| TP-019 | Sales Pitch IV | TD(10): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-018 | B04-033 | 5154680000 |
| TP-020 | Counter Display IV | TD(11): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-019 | B04-034 | 5291290000 |
| TP-021 | Shop Showmanship IV | TL(10): linked share A becomes 14.74576271% | Own 2+ families; TP-020 | B04-035 | 5431520000 |
| TP-022 | Crowd Appeal IV | TL(11): linked share A becomes 15.05084746% | Own 2+ families; TP-021 | B04-036 | 5575460000 |
| TP-023 | Festival Calls IV | TD(12): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-022 | B04-037 | 5723220000 |
| TP-024 | Shop Reputation IV | TL(12): linked share A becomes 15.3559322% | Own 2+ families; TP-023 | B04-038 | 5874900000 |
| TC-07 | Counter Training II | TC(7): total run tap-level discount becomes 6.125% | Own 1+ families; TC-06 | B04-039 | 6030590000 |
| TC-08 | Sales Coaching II | TC(8): total run tap-level discount becomes 7% | Own 1+ families; TC-07 | B04-040 | 6190410000 |
| GP-13 | Delivery Reach II | GP(13): normal products and independent tap B x1.10, once | Own 1+ families; GP-12 | B04-041 | 6354470000 |
| GP-14 | Celebration Demand II | GP(14): normal products and independent tap B x1.10, once | Own 1+ families; GP-13 | B04-042 | 6522870000 |
| GP-15 | Brand Recognition II | GP(15): normal products and independent tap B x1.10, once | Own 1+ families; GP-14 | B04-043 | 6695740000 |
| GP-16 | Customer Loyalty II | GP(16): normal products and independent tap B x1.10, once | Own 1+ families; GP-15 | B04-044 | 6873180000 |
| UC-05 | Supplier Network II | UC(5): total run named-cash discount becomes 5.46875% | Own 1+ families; UC-04 | B04-045 | 7055330000 |
| UC-06 | Trade Association II | UC(6): total run named-cash discount becomes 6.5625% | Own 1+ families; UC-05 | B04-046 | 7242310000 |
| SK-FR-P-02 | Stronger Rush II | SP(FR,2): Rush product multiplier x2.25 | Own 1+ families; SK-FR-P-01 | B04-047 | 7434240000 |
| SK-HH-P-02 | More Helping Hands II | SP(HH,2): 4 auto taps/s | Own 2+ families; SK-HH-P-01 | B04-048 | 7631260000 |
| SK-BO-P-03 | Larger Bulk Orders III | SP(BO,3): 150 seconds of normal P | Own 3+ families; SK-BO-P-02 | B04-049 | 7833500000 |
| SK-BO-C-02 | Bulk Order: Faster Recovery II | SC(BO,2): cumulative run cooldown factor 0.95833333 | SK-BO-P-01; SK-BO-C-01 | B04-050 | 8041100000 |
| PF-MIX-01 | Celebration Mix: Portfolio Milestone I | PF(MIX,1): each listed family's normal profit x1.30 | BOX+PHU levels >= 25 (every listed family) | B04-051 | 8254210000 |
| PF-RHYTHM-01 | Sound and Rhythm: Portfolio Milestone I | PF(RHYTHM,1): each listed family's normal profit x1.30 | SMC+STR levels >= 25 (every listed family) | B04-052 | 8472960000 |
| PF-SKY-01 | Skyline Collection: Portfolio Milestone I | PF(SKY,1): each listed family's normal profit x1.30 | ROC+AER levels >= 25 (every listed family) | B04-053 | 8697500000 |
| PF-STREET-01 | Street Favourites: Portfolio Milestone I | PF(STREET,1): each listed family's normal profit x1.30 | PHU+ANA+CHA levels >= 25 (every listed family) | B04-054 | 8928000000 |

## Band 05

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-13 | Aerial shots Signature Breakthrough III | PP(AER,13): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-12 | B05-001 | 69192000000 |
| PP-ANA-13 | Anar: Gift Packaging III | PP(ANA,13): multiply family normal profit by x1.6 | Own Anar; PP-ANA-12 | B05-002 | 71137300000 |
| PP-BOX-13 | Celebration boxes: Gift Packaging III | PP(BOX,13): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-12 | B05-003 | 73137200000 |
| PP-CHA-13 | Chakri: Gift Packaging III | PP(CHA,13): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-12 | B05-004 | 75193400000 |
| PP-PHU-13 | Phuljhari Signature Breakthrough III | PP(PHU,13): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-12 | B05-005 | 77307300000 |
| PP-ROC-13 | Rockets: Gift Packaging III | PP(ROC,13): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-12 | B05-006 | 79480700000 |
| PP-SMC-13 | Small crackers: Gift Packaging III | PP(SMC,13): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-12 | B05-007 | 81715200000 |
| PP-STR-13 | Cracker strings: Gift Packaging III | PP(STR,13): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-12 | B05-008 | 84012600000 |
| PP-AER-14 | Aerial shots: Festival Display III | PP(AER,14): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-13 | B05-009 | 86374500000 |
| PP-ANA-14 | Anar: Festival Display III | PP(ANA,14): multiply family normal profit by x1.6 | Own Anar; PP-ANA-13 | B05-010 | 88802800000 |
| PP-BOX-14 | Celebration boxes: Festival Display III | PP(BOX,14): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-13 | B05-011 | 91299400000 |
| PP-CHA-14 | Chakri Signature Breakthrough III | PP(CHA,14): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-13 | B05-012 | 93866200000 |
| PP-PHU-14 | Phuljhari: Festival Display III | PP(PHU,14): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-13 | B05-013 | 96505100000 |
| PP-ROC-14 | Rockets: Festival Display III | PP(ROC,14): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-13 | B05-014 | 99218200000 |
| PP-SMC-14 | Small crackers: Festival Display III | PP(SMC,14): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-13 | B05-015 | 102008000000 |
| PP-STR-14 | Cracker strings: Festival Display III | PP(STR,14): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-13 | B05-016 | 104875000000 |
| PP-AER-15 | Aerial shots: Regular Customers III | PP(AER,15): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-14 | B05-017 | 107824000000 |
| PP-ANA-15 | Anar: Regular Customers III | PP(ANA,15): multiply family normal profit by x1.6 | Own Anar; PP-ANA-14 | B05-018 | 110855000000 |
| PP-BOX-15 | Celebration boxes: Regular Customers III | PP(BOX,15): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-14 | B05-019 | 113972000000 |
| PP-CHA-15 | Chakri: Regular Customers III | PP(CHA,15): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-14 | B05-020 | 117176000000 |
| PP-PHU-15 | Phuljhari: Regular Customers III | PP(PHU,15): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-14 | B05-021 | 120470000000 |
| PP-ROC-15 | Rockets: Regular Customers III | PP(ROC,15): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-14 | B05-022 | 123857000000 |
| PP-SMC-15 | Small crackers Signature Breakthrough III | PP(SMC,15): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-14 | B05-023 | 127339000000 |
| PP-STR-15 | Cracker strings: Regular Customers III | PP(STR,15): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-14 | B05-024 | 130919000000 |
| PC-AER-04 | Aerial shots: Factory Partnership I | PC(AER,4): total run family-level discount becomes 8.75% | Own Aerial shots; PC-AER-03 | B05-025 | 134600000000 |
| PC-ANA-04 | Anar: Factory Partnership I | PC(ANA,4): total run family-level discount becomes 8.75% | Own Anar; PC-ANA-03 | B05-026 | 138384000000 |
| PC-BOX-04 | Celebration boxes: Factory Partnership I | PC(BOX,4): total run family-level discount becomes 8.75% | Own Celebration boxes; PC-BOX-03 | B05-027 | 142275000000 |
| PC-CHA-04 | Chakri: Factory Partnership I | PC(CHA,4): total run family-level discount becomes 8.75% | Own Chakri; PC-CHA-03 | B05-028 | 146274000000 |
| PC-PHU-04 | Phuljhari: Factory Partnership I | PC(PHU,4): total run family-level discount becomes 8.75% | Own Phuljhari; PC-PHU-03 | B05-029 | 150387000000 |
| PC-ROC-04 | Rockets: Factory Partnership I | PC(ROC,4): total run family-level discount becomes 8.75% | Own Rockets; PC-ROC-03 | B05-030 | 154615000000 |
| PC-SMC-04 | Small crackers: Factory Partnership I | PC(SMC,4): total run family-level discount becomes 8.75% | Own Small crackers; PC-SMC-03 | B05-031 | 158961000000 |
| PC-STR-04 | Cracker strings: Factory Partnership I | PC(STR,4): total run family-level discount becomes 8.75% | Own Cracker strings; PC-STR-03 | B05-032 | 163430000000 |
| TP-025 | Sales Pitch V | TD(13): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-024 | B05-033 | 168025000000 |
| TP-026 | Counter Display V | TD(14): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-025 | B05-034 | 172749000000 |
| TP-027 | Shop Showmanship V | TL(13): linked share A becomes 15.66101695% | Own 2+ families; TP-026 | B05-035 | 177606000000 |
| TP-028 | Crowd Appeal V | TL(14): linked share A becomes 15.96610169% | Own 2+ families; TP-027 | B05-036 | 182599000000 |
| TP-029 | Festival Calls V | TD(15): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-028 | B05-037 | 187732000000 |
| TP-030 | Shop Reputation V | TL(15): linked share A becomes 16.27118644% | Own 2+ families; TP-029 | B05-038 | 193010000000 |
| TC-09 | Team Handbook II | TC(9): total run tap-level discount becomes 7.875% | Own 1+ families; TC-08 | B05-039 | 198436000000 |
| TC-10 | Customer Insight II | TC(10): total run tap-level discount becomes 8.75% | Own 1+ families; TC-09 | B05-040 | 204015000000 |
| GP-17 | Festival Footfall III | GP(17): normal products and independent tap B x1.10, once | Own 1+ families; GP-16 | B05-041 | 209751000000 |
| GP-18 | Community Trust III | GP(18): normal products and independent tap B x1.10, once | Own 1+ families; GP-17 | B05-042 | 215648000000 |
| GP-19 | Local Advertising III | GP(19): normal products and independent tap B x1.10, once | Own 1+ families; GP-18 | B05-043 | 221711000000 |
| GP-20 | Shopfront Glow III | GP(20): normal products and independent tap B x1.10, once | Own 1+ families; GP-19 | B05-044 | 227944000000 |
| UC-07 | Procurement Office II | UC(7): total run named-cash discount becomes 7.65625% | Own 1+ families; UC-06 | B05-045 | 234352000000 |
| UC-08 | Distributor Alliance II | UC(8): total run named-cash discount becomes 8.75% | Own 1+ families; UC-07 | B05-046 | 240941000000 |
| SK-FR-D-02 | Festival Rush: Longer Window II | SD(FR,2): cumulative run duration factor 1.125 | SK-FR-P-01; SK-FR-D-01 | B05-047 | 247714000000 |
| SK-HH-D-02 | Helping Hands: Longer Window II | SD(HH,2): cumulative run duration factor 1.125 | SK-HH-P-01; SK-HH-D-01 | B05-048 | 254678000000 |
| SK-FR-C-02 | Festival Rush: Faster Recovery II | SC(FR,2): cumulative run cooldown factor 0.9375 | SK-FR-P-01; SK-FR-C-01 | B05-049 | 261838000000 |
| SK-HH-C-02 | Helping Hands: Faster Recovery II | SC(HH,2): cumulative run cooldown factor 0.9375 | SK-HH-P-01; SK-HH-C-01 | B05-050 | 269200000000 |
| SK-BO-C-03 | Bulk Order: Faster Recovery III | SC(BO,3): cumulative run cooldown factor 0.9375 | SK-BO-P-01; SK-BO-C-02 | B05-051 | 276768000000 |

## Band 06

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-16 | Aerial shots: Signature Collection III | PP(AER,16): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-15 | B06-001 | 2144950000000 |
| PP-ANA-16 | Anar Signature Breakthrough III | PP(ANA,16): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-15 | B06-002 | 2222570000000 |
| PP-BOX-16 | Celebration boxes Signature Breakthrough III | PP(BOX,16): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-15 | B06-003 | 2302990000000 |
| PP-CHA-16 | Chakri: Signature Collection III | PP(CHA,16): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-15 | B06-004 | 2386330000000 |
| PP-PHU-16 | Phuljhari: Signature Collection III | PP(PHU,16): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-15 | B06-005 | 2472680000000 |
| PP-ROC-16 | Rockets: Signature Collection III | PP(ROC,16): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-15 | B06-006 | 2562150000000 |
| PP-SMC-16 | Small crackers: Signature Collection III | PP(SMC,16): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-15 | B06-007 | 2654860000000 |
| PP-STR-16 | Cracker strings: Signature Collection III | PP(STR,16): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-15 | B06-008 | 2750930000000 |
| PP-AER-17 | Aerial shots: Celebration Specialist III | PP(AER,17): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-16 | B06-009 | 2850470000000 |
| PP-ANA-17 | Anar: Celebration Specialist III | PP(ANA,17): multiply family normal profit by x1.6 | Own Anar; PP-ANA-16 | B06-010 | 2953620000000 |
| PP-BOX-17 | Celebration boxes: Celebration Specialist III | PP(BOX,17): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-16 | B06-011 | 3060500000000 |
| PP-CHA-17 | Chakri: Celebration Specialist III | PP(CHA,17): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-16 | B06-012 | 3171240000000 |
| PP-PHU-17 | Phuljhari: Celebration Specialist III | PP(PHU,17): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-16 | B06-013 | 3285990000000 |
| PP-ROC-17 | Rockets Signature Breakthrough III | PP(ROC,17): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-16 | B06-014 | 3404900000000 |
| PP-SMC-17 | Small crackers: Celebration Specialist III | PP(SMC,17): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-16 | B06-015 | 3528110000000 |
| PP-STR-17 | Cracker strings: Celebration Specialist III | PP(STR,17): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-16 | B06-016 | 3655770000000 |
| PP-AER-18 | Aerial shots: Premium Range III | PP(AER,18): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-17 | B06-017 | 3788060000000 |
| PP-ANA-18 | Anar: Premium Range III | PP(ANA,18): multiply family normal profit by x1.6 | Own Anar; PP-ANA-17 | B06-018 | 3925130000000 |
| PP-BOX-18 | Celebration boxes: Premium Range III | PP(BOX,18): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-17 | B06-019 | 4067160000000 |
| PP-CHA-18 | Chakri: Premium Range III | PP(CHA,18): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-17 | B06-020 | 4214330000000 |
| PP-PHU-18 | Phuljhari: Premium Range III | PP(PHU,18): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-17 | B06-021 | 4366830000000 |
| PP-ROC-18 | Rockets: Premium Range III | PP(ROC,18): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-17 | B06-022 | 4524850000000 |
| PP-SMC-18 | Small crackers: Premium Range III | PP(SMC,18): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-17 | B06-023 | 4688580000000 |
| PP-STR-18 | Cracker strings Signature Breakthrough III | PP(STR,18): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-17 | B06-024 | 4858240000000 |
| TP-031 | Sales Pitch VI | TD(16): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-030 | B06-025 | 5034030000000 |
| TP-032 | Counter Display VI | TD(17): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-031 | B06-026 | 5216190000000 |
| TP-033 | Shop Showmanship VI | TL(16): linked share A becomes 16.57627119% | Own 2+ families; TP-032 | B06-027 | 5404940000000 |
| TP-034 | Crowd Appeal VI | TL(17): linked share A becomes 16.88135593% | Own 2+ families; TP-033 | B06-028 | 5600520000000 |
| TP-035 | Festival Calls VI | TD(18): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-034 | B06-029 | 5803180000000 |
| TP-036 | Shop Reputation VI | TL(18): linked share A becomes 17.18644068% | Own 2+ families; TP-035 | B06-030 | 6013170000000 |
| TC-11 | Practice Routine III | TC(11): total run tap-level discount becomes 9.625% | Own 1+ families; TC-10 | B06-031 | 6230750000000 |
| TC-12 | Counter Training III | TC(12): total run tap-level discount becomes 10.5% | Own 1+ families; TC-11 | B06-032 | 6456220000000 |
| GP-21 | Delivery Reach III | GP(21): normal products and independent tap B x1.10, once | Own 1+ families; GP-20 | B06-033 | 6689840000000 |
| GP-22 | Celebration Demand III | GP(22): normal products and independent tap B x1.10, once | Own 1+ families; GP-21 | B06-034 | 6931910000000 |
| GP-23 | Brand Recognition III | GP(23): normal products and independent tap B x1.10, once | Own 1+ families; GP-22 | B06-035 | 7182740000000 |
| GP-24 | Customer Loyalty III | GP(24): normal products and independent tap B x1.10, once | Own 1+ families; GP-23 | B06-036 | 7442650000000 |
| UC-09 | Supplier Network III | UC(9): total run named-cash discount becomes 9.84375% | Own 1+ families; UC-08 | B06-037 | 7711970000000 |
| SK-FR-P-03 | Stronger Rush III | SP(FR,3): Rush product multiplier x2.5 | Own 1+ families; SK-FR-P-02 | B06-038 | 7991030000000 |
| SK-HH-P-03 | More Helping Hands III | SP(HH,3): 5 auto taps/s | Own 2+ families; SK-HH-P-02 | B06-039 | 8280190000000 |
| SK-BO-P-04 | Larger Bulk Orders IV | SP(BO,4): 165 seconds of normal P | Own 3+ families; SK-BO-P-03 | B06-040 | 8579810000000 |

## Band 07

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-19 | Aerial shots Signature Breakthrough IV | PP(AER,19): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-18 | B07-001 | 66493500000000 |
| PP-ANA-19 | Anar: Gift Packaging IV | PP(ANA,19): multiply family normal profit by x1.6 | Own Anar; PP-ANA-18 | B07-002 | 68362900000000 |
| PP-BOX-19 | Celebration boxes: Gift Packaging IV | PP(BOX,19): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-18 | B07-003 | 70284800000000 |
| PP-CHA-19 | Chakri: Gift Packaging IV | PP(CHA,19): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-18 | B07-004 | 72260800000000 |
| PP-PHU-19 | Phuljhari Signature Breakthrough IV | PP(PHU,19): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-18 | B07-005 | 74292300000000 |
| PP-ROC-19 | Rockets: Gift Packaging IV | PP(ROC,19): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-18 | B07-006 | 76381000000000 |
| PP-SMC-19 | Small crackers: Gift Packaging IV | PP(SMC,19): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-18 | B07-007 | 78528300000000 |
| PP-STR-19 | Cracker strings: Gift Packaging IV | PP(STR,19): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-18 | B07-008 | 80736100000000 |
| PP-AER-20 | Aerial shots: Festival Display IV | PP(AER,20): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-19 | B07-009 | 83005900000000 |
| PP-ANA-20 | Anar: Festival Display IV | PP(ANA,20): multiply family normal profit by x1.6 | Own Anar; PP-ANA-19 | B07-010 | 85339500000000 |
| PP-BOX-20 | Celebration boxes: Festival Display IV | PP(BOX,20): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-19 | B07-011 | 87738700000000 |
| PP-CHA-20 | Chakri Signature Breakthrough IV | PP(CHA,20): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-19 | B07-012 | 90205400000000 |
| PP-PHU-20 | Phuljhari: Festival Display IV | PP(PHU,20): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-19 | B07-013 | 92741400000000 |
| PP-ROC-20 | Rockets: Festival Display IV | PP(ROC,20): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-19 | B07-014 | 95348700000000 |
| PP-SMC-20 | Small crackers: Festival Display IV | PP(SMC,20): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-19 | B07-015 | 98029300000000 |
| PP-STR-20 | Cracker strings: Festival Display IV | PP(STR,20): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-19 | B07-016 | 100785000000000 |
| PP-AER-21 | Aerial shots: Regular Customers IV | PP(AER,21): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-20 | B07-017 | 103619000000000 |
| PP-ANA-21 | Anar: Regular Customers IV | PP(ANA,21): multiply family normal profit by x1.6 | Own Anar; PP-ANA-20 | B07-018 | 106532000000000 |
| PP-BOX-21 | Celebration boxes: Regular Customers IV | PP(BOX,21): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-20 | B07-019 | 109527000000000 |
| PP-CHA-21 | Chakri: Regular Customers IV | PP(CHA,21): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-20 | B07-020 | 112606000000000 |
| PP-PHU-21 | Phuljhari: Regular Customers IV | PP(PHU,21): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-20 | B07-021 | 115772000000000 |
| PP-ROC-21 | Rockets: Regular Customers IV | PP(ROC,21): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-20 | B07-022 | 119027000000000 |
| PP-SMC-21 | Small crackers Signature Breakthrough IV | PP(SMC,21): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-20 | B07-023 | 122373000000000 |
| PP-STR-21 | Cracker strings: Regular Customers IV | PP(STR,21): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-20 | B07-024 | 125813000000000 |
| PC-AER-05 | Aerial shots: Wholesale Deal II | PC(AER,5): total run family-level discount becomes 10.9375% | Own Aerial shots; PC-AER-04 | B07-025 | 129350000000000 |
| PC-ANA-05 | Anar: Wholesale Deal II | PC(ANA,5): total run family-level discount becomes 10.9375% | Own Anar; PC-ANA-04 | B07-026 | 132987000000000 |
| PC-BOX-05 | Celebration boxes: Wholesale Deal II | PC(BOX,5): total run family-level discount becomes 10.9375% | Own Celebration boxes; PC-BOX-04 | B07-027 | 136726000000000 |
| PC-CHA-05 | Chakri: Wholesale Deal II | PC(CHA,5): total run family-level discount becomes 10.9375% | Own Chakri; PC-CHA-04 | B07-028 | 140570000000000 |
| PC-PHU-05 | Phuljhari: Wholesale Deal II | PC(PHU,5): total run family-level discount becomes 10.9375% | Own Phuljhari; PC-PHU-04 | B07-029 | 144522000000000 |
| PC-ROC-05 | Rockets: Wholesale Deal II | PC(ROC,5): total run family-level discount becomes 10.9375% | Own Rockets; PC-ROC-04 | B07-030 | 148585000000000 |
| PC-SMC-05 | Small crackers: Wholesale Deal II | PC(SMC,5): total run family-level discount becomes 10.9375% | Own Small crackers; PC-SMC-04 | B07-031 | 152762000000000 |
| PC-STR-05 | Cracker strings: Wholesale Deal II | PC(STR,5): total run family-level discount becomes 10.9375% | Own Cracker strings; PC-STR-04 | B07-032 | 157057000000000 |
| TP-037 | Sales Pitch VII | TD(19): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-036 | B07-033 | 161472000000000 |
| TP-038 | Counter Display VII | TD(20): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-037 | B07-034 | 166012000000000 |
| TP-039 | Shop Showmanship VII | TL(19): linked share A becomes 17.49152542% | Own 2+ families; TP-038 | B07-035 | 170679000000000 |
| TP-040 | Crowd Appeal VII | TL(20): linked share A becomes 17.79661017% | Own 2+ families; TP-039 | B07-036 | 175477000000000 |
| TP-041 | Festival Calls VII | TD(21): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-040 | B07-037 | 180411000000000 |
| TP-042 | Shop Reputation VII | TL(21): linked share A becomes 18.10169492% | Own 2+ families; TP-041 | B07-038 | 185483000000000 |
| TC-13 | Sales Coaching III | TC(13): total run tap-level discount becomes 11.375% | Own 1+ families; TC-12 | B07-039 | 190697000000000 |
| TC-14 | Team Handbook III | TC(14): total run tap-level discount becomes 12.25% | Own 1+ families; TC-13 | B07-040 | 196059000000000 |
| GP-25 | Festival Footfall IV | GP(25): normal products and independent tap B x1.10, once | Own 1+ families; GP-24 | B07-041 | 201571000000000 |
| GP-26 | Community Trust IV | GP(26): normal products and independent tap B x1.10, once | Own 1+ families; GP-25 | B07-042 | 207238000000000 |
| GP-27 | Local Advertising IV | GP(27): normal products and independent tap B x1.10, once | Own 1+ families; GP-26 | B07-043 | 213064000000000 |
| GP-28 | Shopfront Glow IV | GP(28): normal products and independent tap B x1.10, once | Own 1+ families; GP-27 | B07-044 | 219054000000000 |
| UC-10 | Trade Association III | UC(10): total run named-cash discount becomes 10.9375% | Own 1+ families; UC-09 | B07-045 | 225212000000000 |
| UC-11 | Procurement Office III | UC(11): total run named-cash discount becomes 12.03125% | Own 1+ families; UC-10 | B07-046 | 231544000000000 |
| SK-FR-D-03 | Festival Rush: Longer Window III | SD(FR,3): cumulative run duration factor 1.1875 | SK-FR-P-01; SK-FR-D-02 | B07-047 | 238053000000000 |
| SK-HH-D-03 | Helping Hands: Longer Window III | SD(HH,3): cumulative run duration factor 1.1875 | SK-HH-P-01; SK-HH-D-02 | B07-048 | 244746000000000 |
| SK-FR-C-03 | Festival Rush: Faster Recovery III | SC(FR,3): cumulative run cooldown factor 0.90625 | SK-FR-P-01; SK-FR-C-02 | B07-049 | 251627000000000 |
| SK-HH-C-03 | Helping Hands: Faster Recovery III | SC(HH,3): cumulative run cooldown factor 0.90625 | SK-HH-P-01; SK-HH-C-02 | B07-050 | 258701000000000 |
| SK-BO-C-04 | Bulk Order: Faster Recovery IV | SC(BO,4): cumulative run cooldown factor 0.91666667 | SK-BO-P-01; SK-BO-C-03 | B07-051 | 265974000000000 |

## Band 08

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-22 | Aerial shots: Signature Collection IV | PP(AER,22): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-21 | B08-001 | 2061300000000000 |
| PP-ANA-22 | Anar Signature Breakthrough IV | PP(ANA,22): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-21 | B08-002 | 2115930000000000 |
| PP-BOX-22 | Celebration boxes Signature Breakthrough IV | PP(BOX,22): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-21 | B08-003 | 2172000000000000 |
| PP-CHA-22 | Chakri: Signature Collection IV | PP(CHA,22): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-21 | B08-004 | 2229560000000000 |
| PP-PHU-22 | Phuljhari: Signature Collection IV | PP(PHU,22): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-21 | B08-005 | 2288650000000000 |
| PP-ROC-22 | Rockets: Signature Collection IV | PP(ROC,22): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-21 | B08-006 | 2349300000000000 |
| PP-SMC-22 | Small crackers: Signature Collection IV | PP(SMC,22): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-21 | B08-007 | 2411560000000000 |
| PP-STR-22 | Cracker strings: Signature Collection IV | PP(STR,22): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-21 | B08-008 | 2475470000000000 |
| PP-AER-23 | Aerial shots: Celebration Specialist IV | PP(AER,23): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-22 | B08-009 | 2541080000000000 |
| PP-ANA-23 | Anar: Celebration Specialist IV | PP(ANA,23): multiply family normal profit by x1.6 | Own Anar; PP-ANA-22 | B08-010 | 2608420000000000 |
| PP-BOX-23 | Celebration boxes: Celebration Specialist IV | PP(BOX,23): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-22 | B08-011 | 2677550000000000 |
| PP-CHA-23 | Chakri: Celebration Specialist IV | PP(CHA,23): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-22 | B08-012 | 2748510000000000 |
| PP-PHU-23 | Phuljhari: Celebration Specialist IV | PP(PHU,23): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-22 | B08-013 | 2821350000000000 |
| PP-ROC-23 | Rockets Signature Breakthrough IV | PP(ROC,23): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-22 | B08-014 | 2896120000000000 |
| PP-SMC-23 | Small crackers: Celebration Specialist IV | PP(SMC,23): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-22 | B08-015 | 2972870000000000 |
| PP-STR-23 | Cracker strings: Celebration Specialist IV | PP(STR,23): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-22 | B08-016 | 3051650000000000 |
| PP-AER-24 | Aerial shots: Premium Range IV | PP(AER,24): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-23 | B08-017 | 3132530000000000 |
| PP-ANA-24 | Anar: Premium Range IV | PP(ANA,24): multiply family normal profit by x1.6 | Own Anar; PP-ANA-23 | B08-018 | 3215540000000000 |
| PP-BOX-24 | Celebration boxes: Premium Range IV | PP(BOX,24): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-23 | B08-019 | 3300760000000000 |
| PP-CHA-24 | Chakri: Premium Range IV | PP(CHA,24): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-23 | B08-020 | 3388240000000000 |
| PP-PHU-24 | Phuljhari: Premium Range IV | PP(PHU,24): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-23 | B08-021 | 3478030000000000 |
| PP-ROC-24 | Rockets: Premium Range IV | PP(ROC,24): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-23 | B08-022 | 3570200000000000 |
| PP-SMC-24 | Small crackers: Premium Range IV | PP(SMC,24): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-23 | B08-023 | 3664820000000000 |
| PP-STR-24 | Cracker strings Signature Breakthrough IV | PP(STR,24): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-23 | B08-024 | 3761940000000000 |
| PC-AER-06 | Aerial shots: Trusted Supplier II | PC(AER,6): total run family-level discount becomes 13.125% | Own Aerial shots; PC-AER-05 | B08-025 | 3861640000000000 |
| PC-ANA-06 | Anar: Trusted Supplier II | PC(ANA,6): total run family-level discount becomes 13.125% | Own Anar; PC-ANA-05 | B08-026 | 3963980000000000 |
| PC-BOX-06 | Celebration boxes: Trusted Supplier II | PC(BOX,6): total run family-level discount becomes 13.125% | Own Celebration boxes; PC-BOX-05 | B08-027 | 4069030000000000 |
| PC-CHA-06 | Chakri: Trusted Supplier II | PC(CHA,6): total run family-level discount becomes 13.125% | Own Chakri; PC-CHA-05 | B08-028 | 4176870000000000 |
| PC-PHU-06 | Phuljhari: Trusted Supplier II | PC(PHU,6): total run family-level discount becomes 13.125% | Own Phuljhari; PC-PHU-05 | B08-029 | 4287560000000000 |
| PC-ROC-06 | Rockets: Trusted Supplier II | PC(ROC,6): total run family-level discount becomes 13.125% | Own Rockets; PC-ROC-05 | B08-030 | 4401190000000000 |
| PC-SMC-06 | Small crackers: Trusted Supplier II | PC(SMC,6): total run family-level discount becomes 13.125% | Own Small crackers; PC-SMC-05 | B08-031 | 4517830000000000 |
| PC-STR-06 | Cracker strings: Trusted Supplier II | PC(STR,6): total run family-level discount becomes 13.125% | Own Cracker strings; PC-STR-05 | B08-032 | 4637560000000000 |
| TP-043 | Sales Pitch VIII | TD(22): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-042 | B08-033 | 4760460000000000 |
| TP-044 | Counter Display VIII | TD(23): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-043 | B08-034 | 4886620000000000 |
| TP-045 | Shop Showmanship VIII | TL(22): linked share A becomes 18.40677966% | Own 2+ families; TP-044 | B08-035 | 5016120000000000 |
| TP-046 | Crowd Appeal VIII | TL(23): linked share A becomes 18.71186441% | Own 2+ families; TP-045 | B08-036 | 5149060000000000 |
| TP-047 | Festival Calls VIII | TD(24): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-046 | B08-037 | 5285520000000000 |
| TP-048 | Shop Reputation VIII | TL(24): linked share A becomes 19.01694915% | Own 2+ families; TP-047 | B08-038 | 5425590000000000 |
| TC-15 | Customer Insight III | TC(15): total run tap-level discount becomes 13.125% | Own 1+ families; TC-14 | B08-039 | 5569380000000000 |
| TC-16 | Practice Routine IV | TC(16): total run tap-level discount becomes 14% | Own 1+ families; TC-15 | B08-040 | 5716970000000000 |
| GP-29 | Delivery Reach IV | GP(29): normal products and independent tap B x1.10, once | Own 1+ families; GP-28 | B08-041 | 5868480000000000 |
| GP-30 | Celebration Demand IV | GP(30): normal products and independent tap B x1.10, once | Own 1+ families; GP-29 | B08-042 | 6024010000000000 |
| GP-31 | Brand Recognition IV | GP(31): normal products and independent tap B x1.10, once | Own 1+ families; GP-30 | B08-043 | 6183650000000000 |
| GP-32 | Customer Loyalty IV | GP(32): normal products and independent tap B x1.10, once | Own 1+ families; GP-31 | B08-044 | 6347530000000000 |
| UC-12 | Distributor Alliance III | UC(12): total run named-cash discount becomes 13.125% | Own 1+ families; UC-11 | B08-045 | 6515750000000000 |
| SK-BO-P-05 | Larger Bulk Orders V | SP(BO,5): 180 seconds of normal P | Own 3+ families; SK-BO-P-04 | B08-046 | 6688430000000000 |
| XL-AER-BOX-02 | Aerial shots / Celebration boxes: Display Partnership II | XL(AER,BOX,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-AER-BOX-01 | B08-047 | 6865680000000000 |
| XL-ANA-CHA-02 | Anar / Chakri: Display Partnership II | XL(ANA,CHA,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-ANA-CHA-01 | B08-048 | 7047630000000000 |
| XL-BOX-PHU-02 | Celebration boxes / Phuljhari: Display Partnership II | XL(BOX,PHU,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-BOX-PHU-01 | B08-049 | 7234410000000000 |
| XL-CHA-ROC-02 | Chakri / Rockets: Display Partnership II | XL(CHA,ROC,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-CHA-ROC-01 | B08-050 | 7426130000000000 |
| XL-PHU-ANA-02 | Phuljhari / Anar: Display Partnership II | XL(PHU,ANA,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-PHU-ANA-01 | B08-051 | 7622930000000000 |
| XL-ROC-SMC-02 | Rockets / Small crackers: Display Partnership II | XL(ROC,SMC,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-ROC-SMC-01 | B08-052 | 7824950000000000 |
| XL-SMC-STR-02 | Small crackers / Cracker strings: Display Partnership II | XL(SMC,STR,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-SMC-STR-01 | B08-053 | 8032330000000000 |
| XL-STR-AER-02 | Cracker strings / Aerial shots: Display Partnership II | XL(STR,AER,2): beneficiary normal product profit x1.50 | Own both families; Source level >= 100; XL-STR-AER-01 | B08-054 | 8245200000000000 |

## Band 09

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-25 | Aerial shots Signature Breakthrough V | PP(AER,25): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-24 | B09-001 | 63900300000000000 |
| PP-ANA-25 | Anar: Gift Packaging V | PP(ANA,25): multiply family normal profit by x1.6 | Own Anar; PP-ANA-24 | B09-002 | 65626700000000000 |
| PP-BOX-25 | Celebration boxes: Gift Packaging V | PP(BOX,25): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-24 | B09-003 | 67399800000000000 |
| PP-CHA-25 | Chakri: Gift Packaging V | PP(CHA,25): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-24 | B09-004 | 69220800000000000 |
| PP-PHU-25 | Phuljhari Signature Breakthrough V | PP(PHU,25): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-24 | B09-005 | 71091100000000000 |
| PP-ROC-25 | Rockets: Gift Packaging V | PP(ROC,25): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-24 | B09-006 | 73011800000000000 |
| PP-SMC-25 | Small crackers: Gift Packaging V | PP(SMC,25): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-24 | B09-007 | 74984400000000000 |
| PP-STR-25 | Cracker strings: Gift Packaging V | PP(STR,25): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-24 | B09-008 | 77010400000000000 |
| PP-AER-26 | Aerial shots: Festival Display V | PP(AER,26): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-25 | B09-009 | 79091000000000000 |
| PP-ANA-26 | Anar: Festival Display V | PP(ANA,26): multiply family normal profit by x1.6 | Own Anar; PP-ANA-25 | B09-010 | 81227900000000000 |
| PP-BOX-26 | Celebration boxes: Festival Display V | PP(BOX,26): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-25 | B09-011 | 83422500000000000 |
| PP-CHA-26 | Chakri Signature Breakthrough V | PP(CHA,26): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-25 | B09-012 | 85676500000000000 |
| PP-PHU-26 | Phuljhari: Festival Display V | PP(PHU,26): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-25 | B09-013 | 87991300000000000 |
| PP-ROC-26 | Rockets: Festival Display V | PP(ROC,26): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-25 | B09-014 | 90368600000000000 |
| PP-SMC-26 | Small crackers: Festival Display V | PP(SMC,26): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-25 | B09-015 | 92810200000000000 |
| PP-STR-26 | Cracker strings: Festival Display V | PP(STR,26): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-25 | B09-016 | 95317800000000000 |
| PP-AER-27 | Aerial shots: Regular Customers V | PP(AER,27): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-26 | B09-017 | 97893100000000000 |
| PP-ANA-27 | Anar: Regular Customers V | PP(ANA,27): multiply family normal profit by x1.6 | Own Anar; PP-ANA-26 | B09-018 | 100538000000000000 |
| PP-BOX-27 | Celebration boxes: Regular Customers V | PP(BOX,27): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-26 | B09-019 | 103254000000000000 |
| PP-CHA-27 | Chakri: Regular Customers V | PP(CHA,27): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-26 | B09-020 | 106044000000000000 |
| PP-PHU-27 | Phuljhari: Regular Customers V | PP(PHU,27): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-26 | B09-021 | 108909000000000000 |
| PP-ROC-27 | Rockets: Regular Customers V | PP(ROC,27): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-26 | B09-022 | 111852000000000000 |
| PP-SMC-27 | Small crackers Signature Breakthrough V | PP(SMC,27): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-26 | B09-023 | 114874000000000000 |
| PP-STR-27 | Cracker strings: Regular Customers V | PP(STR,27): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-26 | B09-024 | 117977000000000000 |
| PC-AER-07 | Aerial shots: Bulk Procurement II | PC(AER,7): total run family-level discount becomes 15.3125% | Own Aerial shots; PC-AER-06 | B09-025 | 121165000000000000 |
| PC-ANA-07 | Anar: Bulk Procurement II | PC(ANA,7): total run family-level discount becomes 15.3125% | Own Anar; PC-ANA-06 | B09-026 | 124438000000000000 |
| PC-BOX-07 | Celebration boxes: Bulk Procurement II | PC(BOX,7): total run family-level discount becomes 15.3125% | Own Celebration boxes; PC-BOX-06 | B09-027 | 127801000000000000 |
| PC-CHA-07 | Chakri: Bulk Procurement II | PC(CHA,7): total run family-level discount becomes 15.3125% | Own Chakri; PC-CHA-06 | B09-028 | 131253000000000000 |
| PC-PHU-07 | Phuljhari: Bulk Procurement II | PC(PHU,7): total run family-level discount becomes 15.3125% | Own Phuljhari; PC-PHU-06 | B09-029 | 134800000000000000 |
| PC-ROC-07 | Rockets: Bulk Procurement II | PC(ROC,7): total run family-level discount becomes 15.3125% | Own Rockets; PC-ROC-06 | B09-030 | 138442000000000000 |
| PC-SMC-07 | Small crackers: Bulk Procurement II | PC(SMC,7): total run family-level discount becomes 15.3125% | Own Small crackers; PC-SMC-06 | B09-031 | 142182000000000000 |
| PC-STR-07 | Cracker strings: Bulk Procurement II | PC(STR,7): total run family-level discount becomes 15.3125% | Own Cracker strings; PC-STR-06 | B09-032 | 146024000000000000 |
| TP-049 | Sales Pitch IX | TD(25): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-048 | B09-033 | 149969000000000000 |
| TP-050 | Counter Display IX | TD(26): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-049 | B09-034 | 154021000000000000 |
| TP-051 | Shop Showmanship IX | TL(25): linked share A becomes 19.3220339% | Own 2+ families; TP-050 | B09-035 | 158182000000000000 |
| TP-052 | Crowd Appeal IX | TL(26): linked share A becomes 19.62711864% | Own 2+ families; TP-051 | B09-036 | 162456000000000000 |
| TP-053 | Festival Calls IX | TD(27): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-052 | B09-037 | 166845000000000000 |
| TP-054 | Shop Reputation IX | TL(27): linked share A becomes 19.93220339% | Own 2+ families; TP-053 | B09-038 | 171353000000000000 |
| TC-17 | Counter Training IV | TC(17): total run tap-level discount becomes 14.875% | Own 1+ families; TC-16 | B09-039 | 175983000000000000 |
| TC-18 | Sales Coaching IV | TC(18): total run tap-level discount becomes 15.75% | Own 1+ families; TC-17 | B09-040 | 180737000000000000 |
| GP-33 | Festival Footfall V | GP(33): normal products and independent tap B x1.10, once | Own 1+ families; GP-32 | B09-041 | 185620000000000000 |
| GP-34 | Community Trust V | GP(34): normal products and independent tap B x1.10, once | Own 1+ families; GP-33 | B09-042 | 190636000000000000 |
| GP-35 | Local Advertising V | GP(35): normal products and independent tap B x1.10, once | Own 1+ families; GP-34 | B09-043 | 195786000000000000 |
| GP-36 | Shopfront Glow V | GP(36): normal products and independent tap B x1.10, once | Own 1+ families; GP-35 | B09-044 | 201076000000000000 |
| UC-13 | Supplier Network IV | UC(13): total run named-cash discount becomes 14.21875% | Own 1+ families; UC-12 | B09-045 | 206509000000000000 |
| UC-14 | Trade Association IV | UC(14): total run named-cash discount becomes 15.3125% | Own 1+ families; UC-13 | B09-046 | 212088000000000000 |
| SK-FR-P-04 | Stronger Rush IV | SP(FR,4): Rush product multiplier x2.75 | Own 1+ families; SK-FR-P-03 | B09-047 | 217818000000000000 |
| SK-HH-P-04 | More Helping Hands IV | SP(HH,4): 6 auto taps/s | Own 2+ families; SK-HH-P-03 | B09-048 | 223703000000000000 |
| SK-BO-C-05 | Bulk Order: Faster Recovery V | SC(BO,5): cumulative run cooldown factor 0.89583333 | SK-BO-P-01; SK-BO-C-04 | B09-049 | 229747000000000000 |
| PF-MIX-02 | Celebration Mix: Portfolio Milestone II | PF(MIX,2): each listed family's normal profit x1.30 | BOX+ANA levels >= 100; PF-MIX-01 (every listed family) | B09-050 | 235955000000000000 |
| PF-RHYTHM-02 | Sound and Rhythm: Portfolio Milestone II | PF(RHYTHM,2): each listed family's normal profit x1.30 | SMC+STR levels >= 100; PF-RHYTHM-01 (every listed family) | B09-051 | 242330000000000000 |
| PF-SKY-02 | Skyline Collection: Portfolio Milestone II | PF(SKY,2): each listed family's normal profit x1.30 | ROC+AER levels >= 100; PF-SKY-01 (every listed family) | B09-052 | 248877000000000000 |
| PF-STREET-02 | Street Favourites: Portfolio Milestone II | PF(STREET,2): each listed family's normal profit x1.30 | PHU+ANA+CHA levels >= 100; PF-STREET-01 (every listed family) | B09-053 | 255601000000000000 |

## Band 10

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-28 | Aerial shots: Signature Collection V | PP(AER,28): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-27 | B10-001 | 1980910000000000000 |
| PP-ANA-28 | Anar Signature Breakthrough V | PP(ANA,28): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-27 | B10-002 | 2035490000000000000 |
| PP-BOX-28 | Celebration boxes Signature Breakthrough V | PP(BOX,28): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-27 | B10-003 | 2091580000000000000 |
| PP-CHA-28 | Chakri: Signature Collection V | PP(CHA,28): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-27 | B10-004 | 2149210000000000000 |
| PP-PHU-28 | Phuljhari: Signature Collection V | PP(PHU,28): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-27 | B10-005 | 2208440000000000000 |
| PP-ROC-28 | Rockets: Signature Collection V | PP(ROC,28): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-27 | B10-006 | 2269290000000000000 |
| PP-SMC-28 | Small crackers: Signature Collection V | PP(SMC,28): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-27 | B10-007 | 2331820000000000000 |
| PP-STR-28 | Cracker strings: Signature Collection V | PP(STR,28): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-27 | B10-008 | 2396070000000000000 |
| PP-AER-29 | Aerial shots: Celebration Specialist V | PP(AER,29): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-28 | B10-009 | 2462100000000000000 |
| PP-ANA-29 | Anar: Celebration Specialist V | PP(ANA,29): multiply family normal profit by x1.6 | Own Anar; PP-ANA-28 | B10-010 | 2529940000000000000 |
| PP-BOX-29 | Celebration boxes: Celebration Specialist V | PP(BOX,29): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-28 | B10-011 | 2599650000000000000 |
| PP-CHA-29 | Chakri: Celebration Specialist V | PP(CHA,29): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-28 | B10-012 | 2671290000000000000 |
| PP-PHU-29 | Phuljhari: Celebration Specialist V | PP(PHU,29): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-28 | B10-013 | 2744890000000000000 |
| PP-ROC-29 | Rockets Signature Breakthrough V | PP(ROC,29): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-28 | B10-014 | 2820530000000000000 |
| PP-SMC-29 | Small crackers: Celebration Specialist V | PP(SMC,29): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-28 | B10-015 | 2898250000000000000 |
| PP-STR-29 | Cracker strings: Celebration Specialist V | PP(STR,29): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-28 | B10-016 | 2978110000000000000 |
| PP-AER-30 | Aerial shots: Premium Range V | PP(AER,30): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-29 | B10-017 | 3060170000000000000 |
| PP-ANA-30 | Anar: Premium Range V | PP(ANA,30): multiply family normal profit by x1.6 | Own Anar; PP-ANA-29 | B10-018 | 3144500000000000000 |
| PP-BOX-30 | Celebration boxes: Premium Range V | PP(BOX,30): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-29 | B10-019 | 3231140000000000000 |
| PP-CHA-30 | Chakri: Premium Range V | PP(CHA,30): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-29 | B10-020 | 3320180000000000000 |
| PP-PHU-30 | Phuljhari: Premium Range V | PP(PHU,30): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-29 | B10-021 | 3411660000000000000 |
| PP-ROC-30 | Rockets: Premium Range V | PP(ROC,30): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-29 | B10-022 | 3505670000000000000 |
| PP-SMC-30 | Small crackers: Premium Range V | PP(SMC,30): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-29 | B10-023 | 3602270000000000000 |
| PP-STR-30 | Cracker strings Signature Breakthrough V | PP(STR,30): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-29 | B10-024 | 3701530000000000000 |
| PC-AER-08 | Aerial shots: Factory Partnership II | PC(AER,8): total run family-level discount becomes 17.5% | Own Aerial shots; PC-AER-07 | B10-025 | 3803530000000000000 |
| PC-ANA-08 | Anar: Factory Partnership II | PC(ANA,8): total run family-level discount becomes 17.5% | Own Anar; PC-ANA-07 | B10-026 | 3908340000000000000 |
| PC-BOX-08 | Celebration boxes: Factory Partnership II | PC(BOX,8): total run family-level discount becomes 17.5% | Own Celebration boxes; PC-BOX-07 | B10-027 | 4016030000000000000 |
| PC-CHA-08 | Chakri: Factory Partnership II | PC(CHA,8): total run family-level discount becomes 17.5% | Own Chakri; PC-CHA-07 | B10-028 | 4126690000000000000 |
| PC-PHU-08 | Phuljhari: Factory Partnership II | PC(PHU,8): total run family-level discount becomes 17.5% | Own Phuljhari; PC-PHU-07 | B10-029 | 4240400000000000000 |
| PC-ROC-08 | Rockets: Factory Partnership II | PC(ROC,8): total run family-level discount becomes 17.5% | Own Rockets; PC-ROC-07 | B10-030 | 4357250000000000000 |
| PC-SMC-08 | Small crackers: Factory Partnership II | PC(SMC,8): total run family-level discount becomes 17.5% | Own Small crackers; PC-SMC-07 | B10-031 | 4477310000000000000 |
| PC-STR-08 | Cracker strings: Factory Partnership II | PC(STR,8): total run family-level discount becomes 17.5% | Own Cracker strings; PC-STR-07 | B10-032 | 4600680000000000000 |
| TP-055 | Sales Pitch X | TD(28): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-054 | B10-033 | 4727460000000000000 |
| TP-056 | Counter Display X | TD(29): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-055 | B10-034 | 4857720000000000000 |
| TP-057 | Shop Showmanship X | TL(28): linked share A becomes 20.23728814% | Own 2+ families; TP-056 | B10-035 | 4991580000000000000 |
| TP-058 | Crowd Appeal X | TL(29): linked share A becomes 20.54237288% | Own 2+ families; TP-057 | B10-036 | 5129120000000000000 |
| TP-059 | Festival Calls X | TD(30): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-058 | B10-037 | 5270450000000000000 |
| TP-060 | Shop Reputation X | TL(30): linked share A becomes 20.84745763% | Own 2+ families; TP-059 | B10-038 | 5415680000000000000 |
| TC-19 | Team Handbook IV | TC(19): total run tap-level discount becomes 16.625% | Own 1+ families; TC-18 | B10-039 | 5564910000000000000 |
| TC-20 | Customer Insight IV | TC(20): total run tap-level discount becomes 17.5% | Own 1+ families; TC-19 | B10-040 | 5718250000000000000 |
| GP-37 | Delivery Reach V | GP(37): normal products and independent tap B x1.10, once | Own 1+ families; GP-36 | B10-041 | 5875820000000000000 |
| GP-38 | Celebration Demand V | GP(38): normal products and independent tap B x1.10, once | Own 1+ families; GP-37 | B10-042 | 6037730000000000000 |
| GP-39 | Brand Recognition V | GP(39): normal products and independent tap B x1.10, once | Own 1+ families; GP-38 | B10-043 | 6204100000000000000 |
| GP-40 | Customer Loyalty V | GP(40): normal products and independent tap B x1.10, once | Own 1+ families; GP-39 | B10-044 | 6375050000000000000 |
| UC-15 | Procurement Office IV | UC(15): total run named-cash discount becomes 16.40625% | Own 1+ families; UC-14 | B10-045 | 6550710000000000000 |
| UC-16 | Distributor Alliance IV | UC(16): total run named-cash discount becomes 17.5% | Own 1+ families; UC-15 | B10-046 | 6731220000000000000 |
| SK-BO-P-06 | Larger Bulk Orders VI | SP(BO,6): 195 seconds of normal P | Own 3+ families; SK-BO-P-05 | B10-047 | 6916700000000000000 |
| SK-FR-D-04 | Festival Rush: Longer Window IV | SD(FR,4): cumulative run duration factor 1.25 | SK-FR-P-01; SK-FR-D-03 | B10-048 | 7107290000000000000 |
| SK-HH-D-04 | Helping Hands: Longer Window IV | SD(HH,4): cumulative run duration factor 1.25 | SK-HH-P-01; SK-HH-D-03 | B10-049 | 7303130000000000000 |
| SK-FR-C-04 | Festival Rush: Faster Recovery IV | SC(FR,4): cumulative run cooldown factor 0.875 | SK-FR-P-01; SK-FR-C-03 | B10-050 | 7504370000000000000 |
| SK-HH-C-04 | Helping Hands: Faster Recovery IV | SC(HH,4): cumulative run cooldown factor 0.875 | SK-HH-P-01; SK-HH-C-03 | B10-051 | 7711150000000000000 |
| SK-BO-C-06 | Bulk Order: Faster Recovery VI | SC(BO,6): cumulative run cooldown factor 0.875 | SK-BO-P-01; SK-BO-C-05 | B10-052 | 7923630000000000000 |

## Band 11

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-31 | Aerial shots Signature Breakthrough VI | PP(AER,31): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-30 | B11-001 | 61408200000000000000 |
| PP-ANA-31 | Anar: Gift Packaging VI | PP(ANA,31): multiply family normal profit by x1.6 | Own Anar; PP-ANA-30 | B11-002 | 63752600000000000000 |
| PP-BOX-31 | Celebration boxes: Gift Packaging VI | PP(BOX,31): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-30 | B11-003 | 66186600000000000000 |
| PP-CHA-31 | Chakri: Gift Packaging VI | PP(CHA,31): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-30 | B11-004 | 68713400000000000000 |
| PP-PHU-31 | Phuljhari Signature Breakthrough VI | PP(PHU,31): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-30 | B11-005 | 71336800000000000000 |
| PP-ROC-31 | Rockets: Gift Packaging VI | PP(ROC,31): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-30 | B11-006 | 74060300000000000000 |
| PP-SMC-31 | Small crackers: Gift Packaging VI | PP(SMC,31): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-30 | B11-007 | 76887800000000000000 |
| PP-STR-31 | Cracker strings: Gift Packaging VI | PP(STR,31): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-30 | B11-008 | 79823200000000000000 |
| PP-AER-32 | Aerial shots: Festival Display VI | PP(AER,32): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-31 | B11-009 | 82870700000000000000 |
| PP-ANA-32 | Anar: Festival Display VI | PP(ANA,32): multiply family normal profit by x1.6 | Own Anar; PP-ANA-31 | B11-010 | 86034600000000000000 |
| PP-BOX-32 | Celebration boxes: Festival Display VI | PP(BOX,32): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-31 | B11-011 | 89319200000000000000 |
| PP-CHA-32 | Chakri Signature Breakthrough VI | PP(CHA,32): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-31 | B11-012 | 92729300000000000000 |
| PP-PHU-32 | Phuljhari: Festival Display VI | PP(PHU,32): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-31 | B11-013 | 96269500000000000000 |
| PP-ROC-32 | Rockets: Festival Display VI | PP(ROC,32): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-31 | B11-014 | 99944900000000000000 |
| PP-SMC-32 | Small crackers: Festival Display VI | PP(SMC,32): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-31 | B11-015 | 103761000000000000000 |
| PP-STR-32 | Cracker strings: Festival Display VI | PP(STR,32): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-31 | B11-016 | 107722000000000000000 |
| PP-AER-33 | Aerial shots: Regular Customers VI | PP(AER,33): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-32 | B11-017 | 111835000000000000000 |
| PP-ANA-33 | Anar: Regular Customers VI | PP(ANA,33): multiply family normal profit by x1.6 | Own Anar; PP-ANA-32 | B11-018 | 116104000000000000000 |
| PP-BOX-33 | Celebration boxes: Regular Customers VI | PP(BOX,33): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-32 | B11-019 | 120537000000000000000 |
| PP-CHA-33 | Chakri: Regular Customers VI | PP(CHA,33): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-32 | B11-020 | 125139000000000000000 |
| PP-PHU-33 | Phuljhari: Regular Customers VI | PP(PHU,33): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-32 | B11-021 | 129916000000000000000 |
| PP-ROC-33 | Rockets: Regular Customers VI | PP(ROC,33): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-32 | B11-022 | 134876000000000000000 |
| PP-SMC-33 | Small crackers Signature Breakthrough VI | PP(SMC,33): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-32 | B11-023 | 140026000000000000000 |
| PP-STR-33 | Cracker strings: Regular Customers VI | PP(STR,33): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-32 | B11-024 | 145372000000000000000 |
| TP-061 | Sales Pitch XI | TD(31): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-060 | B11-025 | 150922000000000000000 |
| TP-062 | Counter Display XI | TD(32): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-061 | B11-026 | 156684000000000000000 |
| TP-063 | Shop Showmanship XI | TL(31): linked share A becomes 21.15254237% | Own 2+ families; TP-062 | B11-027 | 162665000000000000000 |
| TP-064 | Crowd Appeal XI | TL(32): linked share A becomes 21.45762712% | Own 2+ families; TP-063 | B11-028 | 168876000000000000000 |
| TP-065 | Festival Calls XI | TD(33): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-064 | B11-029 | 175323000000000000000 |
| TP-066 | Shop Reputation XI | TL(33): linked share A becomes 21.76271186% | Own 2+ families; TP-065 | B11-030 | 182017000000000000000 |
| TC-21 | Practice Routine V | TC(21): total run tap-level discount becomes 18.375% | Own 1+ families; TC-20 | B11-031 | 188966000000000000000 |
| TC-22 | Counter Training V | TC(22): total run tap-level discount becomes 19.25% | Own 1+ families; TC-21 | B11-032 | 196180000000000000000 |
| GP-41 | Festival Footfall VI | GP(41): normal products and independent tap B x1.10, once | Own 1+ families; GP-40 | B11-033 | 203670000000000000000 |
| GP-42 | Community Trust VI | GP(42): normal products and independent tap B x1.10, once | Own 1+ families; GP-41 | B11-034 | 211446000000000000000 |
| GP-43 | Local Advertising VI | GP(43): normal products and independent tap B x1.10, once | Own 1+ families; GP-42 | B11-035 | 219518000000000000000 |
| GP-44 | Shopfront Glow VI | GP(44): normal products and independent tap B x1.10, once | Own 1+ families; GP-43 | B11-036 | 227899000000000000000 |
| UC-17 | Supplier Network V | UC(17): total run named-cash discount becomes 18.59375% | Own 1+ families; UC-16 | B11-037 | 236600000000000000000 |
| SK-BO-P-07 | Larger Bulk Orders VII | SP(BO,7): 210 seconds of normal P | Own 3+ families; SK-BO-P-06 | B11-038 | 245633000000000000000 |

## Band 12

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-34 | Aerial shots: Signature Collection VI | PP(AER,34): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-33 | B12-001 | 1.90365e+21 |
| PP-ANA-34 | Anar Signature Breakthrough VI | PP(ANA,34): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-33 | B12-002 | 1.95509e+21 |
| PP-BOX-34 | Celebration boxes Signature Breakthrough VI | PP(BOX,34): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-33 | B12-003 | 2.00791e+21 |
| PP-CHA-34 | Chakri: Signature Collection VI | PP(CHA,34): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-33 | B12-004 | 2.06216e+21 |
| PP-PHU-34 | Phuljhari: Signature Collection VI | PP(PHU,34): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-33 | B12-005 | 2.11787e+21 |
| PP-ROC-34 | Rockets: Signature Collection VI | PP(ROC,34): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-33 | B12-006 | 2.17509e+21 |
| PP-SMC-34 | Small crackers: Signature Collection VI | PP(SMC,34): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-33 | B12-007 | 2.23386e+21 |
| PP-STR-34 | Cracker strings: Signature Collection VI | PP(STR,34): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-33 | B12-008 | 2.29422e+21 |
| PP-AER-35 | Aerial shots: Celebration Specialist VI | PP(AER,35): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-34 | B12-009 | 2.3562e+21 |
| PP-ANA-35 | Anar: Celebration Specialist VI | PP(ANA,35): multiply family normal profit by x1.6 | Own Anar; PP-ANA-34 | B12-010 | 2.41986e+21 |
| PP-BOX-35 | Celebration boxes: Celebration Specialist VI | PP(BOX,35): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-34 | B12-011 | 2.48524e+21 |
| PP-CHA-35 | Chakri: Celebration Specialist VI | PP(CHA,35): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-34 | B12-012 | 2.55239e+21 |
| PP-PHU-35 | Phuljhari: Celebration Specialist VI | PP(PHU,35): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-34 | B12-013 | 2.62135e+21 |
| PP-ROC-35 | Rockets Signature Breakthrough VI | PP(ROC,35): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-34 | B12-014 | 2.69217e+21 |
| PP-SMC-35 | Small crackers: Celebration Specialist VI | PP(SMC,35): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-34 | B12-015 | 2.76491e+21 |
| PP-STR-35 | Cracker strings: Celebration Specialist VI | PP(STR,35): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-34 | B12-016 | 2.83961e+21 |
| PP-AER-36 | Aerial shots: Premium Range VI | PP(AER,36): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-35 | B12-017 | 2.91633e+21 |
| PP-ANA-36 | Anar: Premium Range VI | PP(ANA,36): multiply family normal profit by x1.6 | Own Anar; PP-ANA-35 | B12-018 | 2.99513e+21 |
| PP-BOX-36 | Celebration boxes: Premium Range VI | PP(BOX,36): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-35 | B12-019 | 3.07605e+21 |
| PP-CHA-36 | Chakri: Premium Range VI | PP(CHA,36): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-35 | B12-020 | 3.15916e+21 |
| PP-PHU-36 | Phuljhari: Premium Range VI | PP(PHU,36): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-35 | B12-021 | 3.24451e+21 |
| PP-ROC-36 | Rockets: Premium Range VI | PP(ROC,36): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-35 | B12-022 | 3.33217e+21 |
| PP-SMC-36 | Small crackers: Premium Range VI | PP(SMC,36): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-35 | B12-023 | 3.4222e+21 |
| PP-STR-36 | Cracker strings Signature Breakthrough VI | PP(STR,36): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-35 | B12-024 | 3.51466e+21 |
| PC-AER-09 | Aerial shots: Wholesale Deal III | PC(AER,9): total run family-level discount becomes 19.6875% | Own Aerial shots; PC-AER-08 | B12-025 | 3.60962e+21 |
| PC-ANA-09 | Anar: Wholesale Deal III | PC(ANA,9): total run family-level discount becomes 19.6875% | Own Anar; PC-ANA-08 | B12-026 | 3.70715e+21 |
| PC-BOX-09 | Celebration boxes: Wholesale Deal III | PC(BOX,9): total run family-level discount becomes 19.6875% | Own Celebration boxes; PC-BOX-08 | B12-027 | 3.80731e+21 |
| PC-CHA-09 | Chakri: Wholesale Deal III | PC(CHA,9): total run family-level discount becomes 19.6875% | Own Chakri; PC-CHA-08 | B12-028 | 3.91017e+21 |
| PC-PHU-09 | Phuljhari: Wholesale Deal III | PC(PHU,9): total run family-level discount becomes 19.6875% | Own Phuljhari; PC-PHU-08 | B12-029 | 4.01582e+21 |
| PC-ROC-09 | Rockets: Wholesale Deal III | PC(ROC,9): total run family-level discount becomes 19.6875% | Own Rockets; PC-ROC-08 | B12-030 | 4.12432e+21 |
| PC-SMC-09 | Small crackers: Wholesale Deal III | PC(SMC,9): total run family-level discount becomes 19.6875% | Own Small crackers; PC-SMC-08 | B12-031 | 4.23575e+21 |
| PC-STR-09 | Cracker strings: Wholesale Deal III | PC(STR,9): total run family-level discount becomes 19.6875% | Own Cracker strings; PC-STR-08 | B12-032 | 4.35019e+21 |
| TP-067 | Sales Pitch XII | TD(34): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-066 | B12-033 | 4.46772e+21 |
| TP-068 | Counter Display XII | TD(35): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-067 | B12-034 | 4.58843e+21 |
| TP-069 | Shop Showmanship XII | TL(34): linked share A becomes 22.06779661% | Own 2+ families; TP-068 | B12-035 | 4.7124e+21 |
| TP-070 | Crowd Appeal XII | TL(35): linked share A becomes 22.37288136% | Own 2+ families; TP-069 | B12-036 | 4.83972e+21 |
| TP-071 | Festival Calls XII | TD(36): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-070 | B12-037 | 4.97048e+21 |
| TP-072 | Shop Reputation XII | TL(36): linked share A becomes 22.6779661% | Own 2+ families; TP-071 | B12-038 | 5.10477e+21 |
| TC-23 | Sales Coaching V | TC(23): total run tap-level discount becomes 20.125% | Own 1+ families; TC-22 | B12-039 | 5.2427e+21 |
| TC-24 | Team Handbook V | TC(24): total run tap-level discount becomes 21% | Own 1+ families; TC-23 | B12-040 | 5.38434e+21 |
| GP-45 | Delivery Reach VI | GP(45): normal products and independent tap B x1.10, once | Own 1+ families; GP-44 | B12-041 | 5.52982e+21 |
| GP-46 | Celebration Demand VI | GP(46): normal products and independent tap B x1.10, once | Own 1+ families; GP-45 | B12-042 | 5.67922e+21 |
| GP-47 | Brand Recognition VI | GP(47): normal products and independent tap B x1.10, once | Own 1+ families; GP-46 | B12-043 | 5.83266e+21 |
| GP-48 | Customer Loyalty VI | GP(48): normal products and independent tap B x1.10, once | Own 1+ families; GP-47 | B12-044 | 5.99025e+21 |
| UC-18 | Trade Association V | UC(18): total run named-cash discount becomes 19.6875% | Own 1+ families; UC-17 | B12-045 | 6.1521e+21 |
| UC-19 | Procurement Office V | UC(19): total run named-cash discount becomes 20.78125% | Own 1+ families; UC-18 | B12-046 | 6.31831e+21 |
| SK-FR-P-05 | Stronger Rush V | SP(FR,5): Rush product multiplier x3 | Own 1+ families; SK-FR-P-04 | B12-047 | 6.48902e+21 |
| SK-HH-P-05 | More Helping Hands V | SP(HH,5): 8 auto taps/s | Own 2+ families; SK-HH-P-04 | B12-048 | 6.66434e+21 |
| SK-FR-D-05 | Festival Rush: Longer Window V | SD(FR,5): cumulative run duration factor 1.3125 | SK-FR-P-01; SK-FR-D-04 | B12-049 | 6.8444e+21 |
| SK-HH-D-05 | Helping Hands: Longer Window V | SD(HH,5): cumulative run duration factor 1.3125 | SK-HH-P-01; SK-HH-D-04 | B12-050 | 7.02932e+21 |
| SK-FR-C-05 | Festival Rush: Faster Recovery V | SC(FR,5): cumulative run cooldown factor 0.84375 | SK-FR-P-01; SK-FR-C-04 | B12-051 | 7.21924e+21 |
| SK-HH-C-05 | Helping Hands: Faster Recovery V | SC(HH,5): cumulative run cooldown factor 0.84375 | SK-HH-P-01; SK-HH-C-04 | B12-052 | 7.41429e+21 |
| SK-BO-C-07 | Bulk Order: Faster Recovery VII | SC(BO,7): cumulative run cooldown factor 0.85416667 | SK-BO-P-01; SK-BO-C-06 | B12-053 | 7.61461e+21 |

## Band 13

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-37 | Aerial shots Signature Breakthrough VII | PP(AER,37): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-36 | B13-001 | 5.90132e+22 |
| PP-ANA-37 | Anar: Gift Packaging VII | PP(ANA,37): multiply family normal profit by x1.6 | Own Anar; PP-ANA-36 | B13-002 | 6.05478e+22 |
| PP-BOX-37 | Celebration boxes: Gift Packaging VII | PP(BOX,37): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-36 | B13-003 | 6.21224e+22 |
| PP-CHA-37 | Chakri: Gift Packaging VII | PP(CHA,37): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-36 | B13-004 | 6.37378e+22 |
| PP-PHU-37 | Phuljhari Signature Breakthrough VII | PP(PHU,37): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-36 | B13-005 | 6.53953e+22 |
| PP-ROC-37 | Rockets: Gift Packaging VII | PP(ROC,37): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-36 | B13-006 | 6.70959e+22 |
| PP-SMC-37 | Small crackers: Gift Packaging VII | PP(SMC,37): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-36 | B13-007 | 6.88407e+22 |
| PP-STR-37 | Cracker strings: Gift Packaging VII | PP(STR,37): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-36 | B13-008 | 7.06308e+22 |
| PP-AER-38 | Aerial shots: Festival Display VII | PP(AER,38): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-37 | B13-009 | 7.24675e+22 |
| PP-ANA-38 | Anar: Festival Display VII | PP(ANA,38): multiply family normal profit by x1.6 | Own Anar; PP-ANA-37 | B13-010 | 7.4352e+22 |
| PP-BOX-38 | Celebration boxes: Festival Display VII | PP(BOX,38): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-37 | B13-011 | 7.62855e+22 |
| PP-CHA-38 | Chakri Signature Breakthrough VII | PP(CHA,38): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-37 | B13-012 | 7.82693e+22 |
| PP-PHU-38 | Phuljhari: Festival Display VII | PP(PHU,38): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-37 | B13-013 | 8.03046e+22 |
| PP-ROC-38 | Rockets: Festival Display VII | PP(ROC,38): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-37 | B13-014 | 8.23929e+22 |
| PP-SMC-38 | Small crackers: Festival Display VII | PP(SMC,38): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-37 | B13-015 | 8.45355e+22 |
| PP-STR-38 | Cracker strings: Festival Display VII | PP(STR,38): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-37 | B13-016 | 8.67338e+22 |
| PP-AER-39 | Aerial shots: Regular Customers VII | PP(AER,39): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-38 | B13-017 | 8.89893e+22 |
| PP-ANA-39 | Anar: Regular Customers VII | PP(ANA,39): multiply family normal profit by x1.6 | Own Anar; PP-ANA-38 | B13-018 | 9.13034e+22 |
| PP-BOX-39 | Celebration boxes: Regular Customers VII | PP(BOX,39): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-38 | B13-019 | 9.36777e+22 |
| PP-CHA-39 | Chakri: Regular Customers VII | PP(CHA,39): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-38 | B13-020 | 9.61137e+22 |
| PP-PHU-39 | Phuljhari: Regular Customers VII | PP(PHU,39): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-38 | B13-021 | 9.86131e+22 |
| PP-ROC-39 | Rockets: Regular Customers VII | PP(ROC,39): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-38 | B13-022 | 1.01177e+23 |
| PP-SMC-39 | Small crackers Signature Breakthrough VII | PP(SMC,39): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-38 | B13-023 | 1.03809e+23 |
| PP-STR-39 | Cracker strings: Regular Customers VII | PP(STR,39): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-38 | B13-024 | 1.06508e+23 |
| PC-AER-10 | Aerial shots: Trusted Supplier III | PC(AER,10): total run family-level discount becomes 21.875% | Own Aerial shots; PC-AER-09 | B13-025 | 1.09278e+23 |
| PC-ANA-10 | Anar: Trusted Supplier III | PC(ANA,10): total run family-level discount becomes 21.875% | Own Anar; PC-ANA-09 | B13-026 | 1.12119e+23 |
| PC-BOX-10 | Celebration boxes: Trusted Supplier III | PC(BOX,10): total run family-level discount becomes 21.875% | Own Celebration boxes; PC-BOX-09 | B13-027 | 1.15035e+23 |
| PC-CHA-10 | Chakri: Trusted Supplier III | PC(CHA,10): total run family-level discount becomes 21.875% | Own Chakri; PC-CHA-09 | B13-028 | 1.18026e+23 |
| PC-PHU-10 | Phuljhari: Trusted Supplier III | PC(PHU,10): total run family-level discount becomes 21.875% | Own Phuljhari; PC-PHU-09 | B13-029 | 1.21096e+23 |
| PC-ROC-10 | Rockets: Trusted Supplier III | PC(ROC,10): total run family-level discount becomes 21.875% | Own Rockets; PC-ROC-09 | B13-030 | 1.24245e+23 |
| PC-SMC-10 | Small crackers: Trusted Supplier III | PC(SMC,10): total run family-level discount becomes 21.875% | Own Small crackers; PC-SMC-09 | B13-031 | 1.27476e+23 |
| PC-STR-10 | Cracker strings: Trusted Supplier III | PC(STR,10): total run family-level discount becomes 21.875% | Own Cracker strings; PC-STR-09 | B13-032 | 1.30791e+23 |
| TP-073 | Sales Pitch XIII | TD(37): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-072 | B13-033 | 1.34192e+23 |
| TP-074 | Counter Display XIII | TD(38): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-073 | B13-034 | 1.37681e+23 |
| TP-075 | Shop Showmanship XIII | TL(37): linked share A becomes 22.98305085% | Own 2+ families; TP-074 | B13-035 | 1.41262e+23 |
| TP-076 | Crowd Appeal XIII | TL(38): linked share A becomes 23.28813559% | Own 2+ families; TP-075 | B13-036 | 1.44935e+23 |
| TP-077 | Festival Calls XIII | TD(39): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-076 | B13-037 | 1.48704e+23 |
| TP-078 | Shop Reputation XIII | TL(39): linked share A becomes 23.59322034% | Own 2+ families; TP-077 | B13-038 | 1.52571e+23 |
| TC-25 | Customer Insight V | TC(25): total run tap-level discount becomes 21.875% | Own 1+ families; TC-24 | B13-039 | 1.56539e+23 |
| TC-26 | Practice Routine VI | TC(26): total run tap-level discount becomes 22.75% | Own 1+ families; TC-25 | B13-040 | 1.60609e+23 |
| GP-49 | Festival Footfall VII | GP(49): normal products and independent tap B x1.10, once | Own 1+ families; GP-48 | B13-041 | 1.64786e+23 |
| GP-50 | Community Trust VII | GP(50): normal products and independent tap B x1.10, once | Own 1+ families; GP-49 | B13-042 | 1.69071e+23 |
| GP-51 | Local Advertising VII | GP(51): normal products and independent tap B x1.10, once | Own 1+ families; GP-50 | B13-043 | 1.73468e+23 |
| GP-52 | Shopfront Glow VII | GP(52): normal products and independent tap B x1.10, once | Own 1+ families; GP-51 | B13-044 | 1.77979e+23 |
| UC-20 | Distributor Alliance V | UC(20): total run named-cash discount becomes 21.875% | Own 1+ families; UC-19 | B13-045 | 1.82607e+23 |
| SK-BO-P-08 | Larger Bulk Orders VIII | SP(BO,8): 225 seconds of normal P | Own 3+ families; SK-BO-P-07 | B13-046 | 1.87355e+23 |
| SK-BO-C-08 | Bulk Order: Faster Recovery VIII | SC(BO,8): cumulative run cooldown factor 0.83333333 | SK-BO-P-01; SK-BO-C-07 | B13-047 | 1.92227e+23 |
| XL-AER-BOX-03 | Aerial shots / Celebration boxes: Display Partnership III | XL(AER,BOX,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-AER-BOX-02 | B13-048 | 1.97226e+23 |
| XL-ANA-CHA-03 | Anar / Chakri: Display Partnership III | XL(ANA,CHA,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-ANA-CHA-02 | B13-049 | 2.02355e+23 |
| XL-BOX-PHU-03 | Celebration boxes / Phuljhari: Display Partnership III | XL(BOX,PHU,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-BOX-PHU-02 | B13-050 | 2.07617e+23 |
| XL-CHA-ROC-03 | Chakri / Rockets: Display Partnership III | XL(CHA,ROC,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-CHA-ROC-02 | B13-051 | 2.13016e+23 |
| XL-PHU-ANA-03 | Phuljhari / Anar: Display Partnership III | XL(PHU,ANA,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-PHU-ANA-02 | B13-052 | 2.18555e+23 |
| XL-ROC-SMC-03 | Rockets / Small crackers: Display Partnership III | XL(ROC,SMC,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-ROC-SMC-02 | B13-053 | 2.24239e+23 |
| XL-SMC-STR-03 | Small crackers / Cracker strings: Display Partnership III | XL(SMC,STR,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-SMC-STR-02 | B13-054 | 2.3007e+23 |
| XL-STR-AER-03 | Cracker strings / Aerial shots: Display Partnership III | XL(STR,AER,3): beneficiary normal product profit x1.50 | Own both families; Source level >= 200; XL-STR-AER-02 | B13-055 | 2.36053e+23 |

## Band 14

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-40 | Aerial shots: Signature Collection VII | PP(AER,40): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-39 | B14-001 | 1.82941e+24 |
| PP-ANA-40 | Anar Signature Breakthrough VII | PP(ANA,40): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-39 | B14-002 | 1.88191e+24 |
| PP-BOX-40 | Celebration boxes Signature Breakthrough VII | PP(BOX,40): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-39 | B14-003 | 1.93591e+24 |
| PP-CHA-40 | Chakri: Signature Collection VII | PP(CHA,40): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-39 | B14-004 | 1.99146e+24 |
| PP-PHU-40 | Phuljhari: Signature Collection VII | PP(PHU,40): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-39 | B14-005 | 2.04861e+24 |
| PP-ROC-40 | Rockets: Signature Collection VII | PP(ROC,40): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-39 | B14-006 | 2.10739e+24 |
| PP-SMC-40 | Small crackers: Signature Collection VII | PP(SMC,40): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-39 | B14-007 | 2.16787e+24 |
| PP-STR-40 | Cracker strings: Signature Collection VII | PP(STR,40): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-39 | B14-008 | 2.23008e+24 |
| PP-AER-41 | Aerial shots: Celebration Specialist VII | PP(AER,41): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-40 | B14-009 | 2.29407e+24 |
| PP-ANA-41 | Anar: Celebration Specialist VII | PP(ANA,41): multiply family normal profit by x1.6 | Own Anar; PP-ANA-40 | B14-010 | 2.3599e+24 |
| PP-BOX-41 | Celebration boxes: Celebration Specialist VII | PP(BOX,41): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-40 | B14-011 | 2.42762e+24 |
| PP-CHA-41 | Chakri: Celebration Specialist VII | PP(CHA,41): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-40 | B14-012 | 2.49728e+24 |
| PP-PHU-41 | Phuljhari: Celebration Specialist VII | PP(PHU,41): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-40 | B14-013 | 2.56894e+24 |
| PP-ROC-41 | Rockets Signature Breakthrough VII | PP(ROC,41): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-40 | B14-014 | 2.64266e+24 |
| PP-SMC-41 | Small crackers: Celebration Specialist VII | PP(SMC,41): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-40 | B14-015 | 2.71849e+24 |
| PP-STR-41 | Cracker strings: Celebration Specialist VII | PP(STR,41): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-40 | B14-016 | 2.7965e+24 |
| PP-AER-42 | Aerial shots: Premium Range VII | PP(AER,42): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-41 | B14-017 | 2.87675e+24 |
| PP-ANA-42 | Anar: Premium Range VII | PP(ANA,42): multiply family normal profit by x1.6 | Own Anar; PP-ANA-41 | B14-018 | 2.9593e+24 |
| PP-BOX-42 | Celebration boxes: Premium Range VII | PP(BOX,42): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-41 | B14-019 | 3.04422e+24 |
| PP-CHA-42 | Chakri: Premium Range VII | PP(CHA,42): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-41 | B14-020 | 3.13158e+24 |
| PP-PHU-42 | Phuljhari: Premium Range VII | PP(PHU,42): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-41 | B14-021 | 3.22144e+24 |
| PP-ROC-42 | Rockets: Premium Range VII | PP(ROC,42): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-41 | B14-022 | 3.31388e+24 |
| PP-SMC-42 | Small crackers: Premium Range VII | PP(SMC,42): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-41 | B14-023 | 3.40897e+24 |
| PP-STR-42 | Cracker strings Signature Breakthrough VII | PP(STR,42): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-41 | B14-024 | 3.5068e+24 |
| PC-AER-11 | Aerial shots: Bulk Procurement III | PC(AER,11): total run family-level discount becomes 24.0625% | Own Aerial shots; PC-AER-10 | B14-025 | 3.60743e+24 |
| PC-ANA-11 | Anar: Bulk Procurement III | PC(ANA,11): total run family-level discount becomes 24.0625% | Own Anar; PC-ANA-10 | B14-026 | 3.71095e+24 |
| PC-BOX-11 | Celebration boxes: Bulk Procurement III | PC(BOX,11): total run family-level discount becomes 24.0625% | Own Celebration boxes; PC-BOX-10 | B14-027 | 3.81743e+24 |
| PC-CHA-11 | Chakri: Bulk Procurement III | PC(CHA,11): total run family-level discount becomes 24.0625% | Own Chakri; PC-CHA-10 | B14-028 | 3.92698e+24 |
| PC-PHU-11 | Phuljhari: Bulk Procurement III | PC(PHU,11): total run family-level discount becomes 24.0625% | Own Phuljhari; PC-PHU-10 | B14-029 | 4.03967e+24 |
| PC-ROC-11 | Rockets: Bulk Procurement III | PC(ROC,11): total run family-level discount becomes 24.0625% | Own Rockets; PC-ROC-10 | B14-030 | 4.15559e+24 |
| PC-SMC-11 | Small crackers: Bulk Procurement III | PC(SMC,11): total run family-level discount becomes 24.0625% | Own Small crackers; PC-SMC-10 | B14-031 | 4.27483e+24 |
| PC-STR-11 | Cracker strings: Bulk Procurement III | PC(STR,11): total run family-level discount becomes 24.0625% | Own Cracker strings; PC-STR-10 | B14-032 | 4.3975e+24 |
| TP-079 | Sales Pitch XIV | TD(40): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-078 | B14-033 | 4.52369e+24 |
| TP-080 | Counter Display XIV | TD(41): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-079 | B14-034 | 4.6535e+24 |
| TP-081 | Shop Showmanship XIV | TL(40): linked share A becomes 23.89830508% | Own 2+ families; TP-080 | B14-035 | 4.78704e+24 |
| TP-082 | Crowd Appeal XIV | TL(41): linked share A becomes 24.20338983% | Own 2+ families; TP-081 | B14-036 | 4.92441e+24 |
| TP-083 | Festival Calls XIV | TD(42): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-082 | B14-037 | 5.06572e+24 |
| TP-084 | Shop Reputation XIV | TL(42): linked share A becomes 24.50847458% | Own 2+ families; TP-083 | B14-038 | 5.21108e+24 |
| TC-27 | Counter Training VI | TC(27): total run tap-level discount becomes 23.625% | Own 1+ families; TC-26 | B14-039 | 5.36062e+24 |
| TC-28 | Sales Coaching VI | TC(28): total run tap-level discount becomes 24.5% | Own 1+ families; TC-27 | B14-040 | 5.51444e+24 |
| GP-53 | Delivery Reach VII | GP(53): normal products and independent tap B x1.10, once | Own 1+ families; GP-52 | B14-041 | 5.67269e+24 |
| GP-54 | Celebration Demand VII | GP(54): normal products and independent tap B x1.10, once | Own 1+ families; GP-53 | B14-042 | 5.83547e+24 |
| GP-55 | Brand Recognition VII | GP(55): normal products and independent tap B x1.10, once | Own 1+ families; GP-54 | B14-043 | 6.00292e+24 |
| GP-56 | Customer Loyalty VII | GP(56): normal products and independent tap B x1.10, once | Own 1+ families; GP-55 | B14-044 | 6.17518e+24 |
| UC-21 | Supplier Network VI | UC(21): total run named-cash discount becomes 22.96875% | Own 1+ families; UC-20 | B14-045 | 6.35238e+24 |
| UC-22 | Trade Association VI | UC(22): total run named-cash discount becomes 24.0625% | Own 1+ families; UC-21 | B14-046 | 6.53467e+24 |
| PF-MIX-03 | Celebration Mix: Portfolio Milestone III | PF(MIX,3): each listed family's normal profit x1.30 | BOX+CHA levels >= 200; PF-MIX-02 (every listed family) | B14-047 | 6.72218e+24 |
| PF-RHYTHM-03 | Sound and Rhythm: Portfolio Milestone III | PF(RHYTHM,3): each listed family's normal profit x1.30 | SMC+STR levels >= 200; PF-RHYTHM-02 (every listed family) | B14-048 | 6.91508e+24 |
| PF-SKY-03 | Skyline Collection: Portfolio Milestone III | PF(SKY,3): each listed family's normal profit x1.30 | ROC+AER levels >= 200; PF-SKY-02 (every listed family) | B14-049 | 7.11351e+24 |
| PF-STREET-03 | Street Favourites: Portfolio Milestone III | PF(STREET,3): each listed family's normal profit x1.30 | PHU+ANA+CHA levels >= 200; PF-STREET-02 (every listed family) | B14-050 | 7.31764e+24 |

## Band 15

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-43 | Aerial shots Signature Breakthrough VIII | PP(AER,43): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-42 | B15-001 | 5.67117e+25 |
| PP-ANA-43 | Anar: Gift Packaging VIII | PP(ANA,43): multiply family normal profit by x1.6 | Own Anar; PP-ANA-42 | B15-002 | 5.82147e+25 |
| PP-BOX-43 | Celebration boxes: Gift Packaging VIII | PP(BOX,43): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-42 | B15-003 | 5.97575e+25 |
| PP-CHA-43 | Chakri: Gift Packaging VIII | PP(CHA,43): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-42 | B15-004 | 6.13411e+25 |
| PP-PHU-43 | Phuljhari Signature Breakthrough VIII | PP(PHU,43): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-42 | B15-005 | 6.29668e+25 |
| PP-ROC-43 | Rockets: Gift Packaging VIII | PP(ROC,43): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-42 | B15-006 | 6.46355e+25 |
| PP-SMC-43 | Small crackers: Gift Packaging VIII | PP(SMC,43): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-42 | B15-007 | 6.63484e+25 |
| PP-STR-43 | Cracker strings: Gift Packaging VIII | PP(STR,43): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-42 | B15-008 | 6.81068e+25 |
| PP-AER-44 | Aerial shots: Festival Display VIII | PP(AER,44): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-43 | B15-009 | 6.99117e+25 |
| PP-ANA-44 | Anar: Festival Display VIII | PP(ANA,44): multiply family normal profit by x1.6 | Own Anar; PP-ANA-43 | B15-010 | 7.17645e+25 |
| PP-BOX-44 | Celebration boxes: Festival Display VIII | PP(BOX,44): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-43 | B15-011 | 7.36663e+25 |
| PP-CHA-44 | Chakri Signature Breakthrough VIII | PP(CHA,44): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-43 | B15-012 | 7.56186e+25 |
| PP-PHU-44 | Phuljhari: Festival Display VIII | PP(PHU,44): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-43 | B15-013 | 7.76226e+25 |
| PP-ROC-44 | Rockets: Festival Display VIII | PP(ROC,44): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-43 | B15-014 | 7.96797e+25 |
| PP-SMC-44 | Small crackers: Festival Display VIII | PP(SMC,44): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-43 | B15-015 | 8.17914e+25 |
| PP-STR-44 | Cracker strings: Festival Display VIII | PP(STR,44): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-43 | B15-016 | 8.3959e+25 |
| PP-AER-45 | Aerial shots: Regular Customers VIII | PP(AER,45): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-44 | B15-017 | 8.6184e+25 |
| PP-ANA-45 | Anar: Regular Customers VIII | PP(ANA,45): multiply family normal profit by x1.6 | Own Anar; PP-ANA-44 | B15-018 | 8.8468e+25 |
| PP-BOX-45 | Celebration boxes: Regular Customers VIII | PP(BOX,45): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-44 | B15-019 | 9.08126e+25 |
| PP-CHA-45 | Chakri: Regular Customers VIII | PP(CHA,45): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-44 | B15-020 | 9.32193e+25 |
| PP-PHU-45 | Phuljhari: Regular Customers VIII | PP(PHU,45): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-44 | B15-021 | 9.56897e+25 |
| PP-ROC-45 | Rockets: Regular Customers VIII | PP(ROC,45): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-44 | B15-022 | 9.82256e+25 |
| PP-SMC-45 | Small crackers Signature Breakthrough VIII | PP(SMC,45): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-44 | B15-023 | 1.00829e+26 |
| PP-STR-45 | Cracker strings: Regular Customers VIII | PP(STR,45): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-44 | B15-024 | 1.03501e+26 |
| PC-AER-12 | Aerial shots: Factory Partnership III | PC(AER,12): total run family-level discount becomes 26.25% | Own Aerial shots; PC-AER-11 | B15-025 | 1.06244e+26 |
| PC-ANA-12 | Anar: Factory Partnership III | PC(ANA,12): total run family-level discount becomes 26.25% | Own Anar; PC-ANA-11 | B15-026 | 1.09059e+26 |
| PC-BOX-12 | Celebration boxes: Factory Partnership III | PC(BOX,12): total run family-level discount becomes 26.25% | Own Celebration boxes; PC-BOX-11 | B15-027 | 1.1195e+26 |
| PC-CHA-12 | Chakri: Factory Partnership III | PC(CHA,12): total run family-level discount becomes 26.25% | Own Chakri; PC-CHA-11 | B15-028 | 1.14917e+26 |
| PC-PHU-12 | Phuljhari: Factory Partnership III | PC(PHU,12): total run family-level discount becomes 26.25% | Own Phuljhari; PC-PHU-11 | B15-029 | 1.17962e+26 |
| PC-ROC-12 | Rockets: Factory Partnership III | PC(ROC,12): total run family-level discount becomes 26.25% | Own Rockets; PC-ROC-11 | B15-030 | 1.21088e+26 |
| PC-SMC-12 | Small crackers: Factory Partnership III | PC(SMC,12): total run family-level discount becomes 26.25% | Own Small crackers; PC-SMC-11 | B15-031 | 1.24297e+26 |
| PC-STR-12 | Cracker strings: Factory Partnership III | PC(STR,12): total run family-level discount becomes 26.25% | Own Cracker strings; PC-STR-11 | B15-032 | 1.27591e+26 |
| TP-085 | Sales Pitch XV | TD(43): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-084 | B15-033 | 1.30973e+26 |
| TP-086 | Counter Display XV | TD(44): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-085 | B15-034 | 1.34444e+26 |
| TP-087 | Shop Showmanship XV | TL(43): linked share A becomes 24.81355932% | Own 2+ families; TP-086 | B15-035 | 1.38007e+26 |
| TP-088 | Crowd Appeal XV | TL(44): linked share A becomes 25.11864407% | Own 2+ families; TP-087 | B15-036 | 1.41664e+26 |
| TP-089 | Festival Calls XV | TD(45): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-088 | B15-037 | 1.45418e+26 |
| TP-090 | Shop Reputation XV | TL(45): linked share A becomes 25.42372881% | Own 2+ families; TP-089 | B15-038 | 1.49272e+26 |
| TC-29 | Team Handbook VI | TC(29): total run tap-level discount becomes 25.375% | Own 1+ families; TC-28 | B15-039 | 1.53228e+26 |
| TC-30 | Customer Insight VI | TC(30): total run tap-level discount becomes 26.25% | Own 1+ families; TC-29 | B15-040 | 1.57289e+26 |
| GP-57 | Festival Footfall VIII | GP(57): normal products and independent tap B x1.10, once | Own 1+ families; GP-56 | B15-041 | 1.61457e+26 |
| GP-58 | Community Trust VIII | GP(58): normal products and independent tap B x1.10, once | Own 1+ families; GP-57 | B15-042 | 1.65736e+26 |
| GP-59 | Local Advertising VIII | GP(59): normal products and independent tap B x1.10, once | Own 1+ families; GP-58 | B15-043 | 1.70128e+26 |
| GP-60 | Shopfront Glow VIII | GP(60): normal products and independent tap B x1.10, once | Own 1+ families; GP-59 | B15-044 | 1.74637e+26 |
| UC-23 | Procurement Office VI | UC(23): total run named-cash discount becomes 25.15625% | Own 1+ families; UC-22 | B15-045 | 1.79265e+26 |
| UC-24 | Distributor Alliance VI | UC(24): total run named-cash discount becomes 26.25% | Own 1+ families; UC-23 | B15-046 | 1.84016e+26 |
| SK-FR-P-06 | Stronger Rush VI | SP(FR,6): Rush product multiplier x3.25 | Own 1+ families; SK-FR-P-05 | B15-047 | 1.88893e+26 |
| SK-HH-P-06 | More Helping Hands VI | SP(HH,6): 10 auto taps/s | Own 2+ families; SK-HH-P-05 | B15-048 | 1.93899e+26 |
| SK-BO-P-09 | Larger Bulk Orders IX | SP(BO,9): 240 seconds of normal P | Own 3+ families; SK-BO-P-08 | B15-049 | 1.99037e+26 |
| SK-FR-D-06 | Festival Rush: Longer Window VI | SD(FR,6): cumulative run duration factor 1.375 | SK-FR-P-01; SK-FR-D-05 | B15-050 | 2.04312e+26 |
| SK-HH-D-06 | Helping Hands: Longer Window VI | SD(HH,6): cumulative run duration factor 1.375 | SK-HH-P-01; SK-HH-D-05 | B15-051 | 2.09727e+26 |
| SK-FR-C-06 | Festival Rush: Faster Recovery VI | SC(FR,6): cumulative run cooldown factor 0.8125 | SK-FR-P-01; SK-FR-C-05 | B15-052 | 2.15285e+26 |
| SK-HH-C-06 | Helping Hands: Faster Recovery VI | SC(HH,6): cumulative run cooldown factor 0.8125 | SK-HH-P-01; SK-HH-C-05 | B15-053 | 2.2099e+26 |
| SK-BO-C-09 | Bulk Order: Faster Recovery IX | SC(BO,9): cumulative run cooldown factor 0.8125 | SK-BO-P-01; SK-BO-C-08 | B15-054 | 2.26847e+26 |

## Band 16

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-46 | Aerial shots: Signature Collection VIII | PP(AER,46): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-45 | B16-001 | 1.75806e+27 |
| PP-ANA-46 | Anar Signature Breakthrough VIII | PP(ANA,46): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-45 | B16-002 | 1.82708e+27 |
| PP-BOX-46 | Celebration boxes Signature Breakthrough VIII | PP(BOX,46): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-45 | B16-003 | 1.89881e+27 |
| PP-CHA-46 | Chakri: Signature Collection VIII | PP(CHA,46): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-45 | B16-004 | 1.97336e+27 |
| PP-PHU-46 | Phuljhari: Signature Collection VIII | PP(PHU,46): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-45 | B16-005 | 2.05083e+27 |
| PP-ROC-46 | Rockets: Signature Collection VIII | PP(ROC,46): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-45 | B16-006 | 2.13135e+27 |
| PP-SMC-46 | Small crackers: Signature Collection VIII | PP(SMC,46): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-45 | B16-007 | 2.21502e+27 |
| PP-STR-46 | Cracker strings: Signature Collection VIII | PP(STR,46): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-45 | B16-008 | 2.30198e+27 |
| PP-AER-47 | Aerial shots: Celebration Specialist VIII | PP(AER,47): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-46 | B16-009 | 2.39236e+27 |
| PP-ANA-47 | Anar: Celebration Specialist VIII | PP(ANA,47): multiply family normal profit by x1.6 | Own Anar; PP-ANA-46 | B16-010 | 2.48628e+27 |
| PP-BOX-47 | Celebration boxes: Celebration Specialist VIII | PP(BOX,47): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-46 | B16-011 | 2.58389e+27 |
| PP-CHA-47 | Chakri: Celebration Specialist VIII | PP(CHA,47): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-46 | B16-012 | 2.68533e+27 |
| PP-PHU-47 | Phuljhari: Celebration Specialist VIII | PP(PHU,47): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-46 | B16-013 | 2.79075e+27 |
| PP-ROC-47 | Rockets Signature Breakthrough VIII | PP(ROC,47): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-46 | B16-014 | 2.90031e+27 |
| PP-SMC-47 | Small crackers: Celebration Specialist VIII | PP(SMC,47): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-46 | B16-015 | 3.01418e+27 |
| PP-STR-47 | Cracker strings: Celebration Specialist VIII | PP(STR,47): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-46 | B16-016 | 3.13251e+27 |
| PP-AER-48 | Aerial shots: Premium Range VIII | PP(AER,48): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-47 | B16-017 | 3.25549e+27 |
| PP-ANA-48 | Anar: Premium Range VIII | PP(ANA,48): multiply family normal profit by x1.6 | Own Anar; PP-ANA-47 | B16-018 | 3.3833e+27 |
| PP-BOX-48 | Celebration boxes: Premium Range VIII | PP(BOX,48): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-47 | B16-019 | 3.51613e+27 |
| PP-CHA-48 | Chakri: Premium Range VIII | PP(CHA,48): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-47 | B16-020 | 3.65417e+27 |
| PP-PHU-48 | Phuljhari: Premium Range VIII | PP(PHU,48): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-47 | B16-021 | 3.79763e+27 |
| PP-ROC-48 | Rockets: Premium Range VIII | PP(ROC,48): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-47 | B16-022 | 3.94672e+27 |
| PP-SMC-48 | Small crackers: Premium Range VIII | PP(SMC,48): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-47 | B16-023 | 4.10166e+27 |
| PP-STR-48 | Cracker strings Signature Breakthrough VIII | PP(STR,48): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-47 | B16-024 | 4.26269e+27 |
| TP-091 | Sales Pitch XVI | TD(46): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-090 | B16-025 | 4.43004e+27 |
| TP-092 | Counter Display XVI | TD(47): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-091 | B16-026 | 4.60396e+27 |
| TP-093 | Shop Showmanship XVI | TL(46): linked share A becomes 25.72881356% | Own 2+ families; TP-092 | B16-027 | 4.78471e+27 |
| TP-094 | Crowd Appeal XVI | TL(47): linked share A becomes 26.03389831% | Own 2+ families; TP-093 | B16-028 | 4.97255e+27 |
| TP-095 | Festival Calls XVI | TD(48): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-094 | B16-029 | 5.16777e+27 |
| TP-096 | Shop Reputation XVI | TL(48): linked share A becomes 26.33898305% | Own 2+ families; TP-095 | B16-030 | 5.37066e+27 |
| TC-31 | Practice Routine VII | TC(31): total run tap-level discount becomes 27.125% | Own 1+ families; TC-30 | B16-031 | 5.5815e+27 |
| TC-32 | Counter Training VII | TC(32): total run tap-level discount becomes 28% | Own 1+ families; TC-31 | B16-032 | 5.80063e+27 |
| GP-61 | Delivery Reach VIII | GP(61): normal products and independent tap B x1.10, once | Own 1+ families; GP-60 | B16-033 | 6.02836e+27 |
| GP-62 | Celebration Demand VIII | GP(62): normal products and independent tap B x1.10, once | Own 1+ families; GP-61 | B16-034 | 6.26503e+27 |
| GP-63 | Brand Recognition VIII | GP(63): normal products and independent tap B x1.10, once | Own 1+ families; GP-62 | B16-035 | 6.51099e+27 |
| GP-64 | Customer Loyalty VIII | GP(64): normal products and independent tap B x1.10, once | Own 1+ families; GP-63 | B16-036 | 6.7666e+27 |
| UC-25 | Supplier Network VII | UC(25): total run named-cash discount becomes 27.34375% | Own 1+ families; UC-24 | B16-037 | 7.03225e+27 |

## Band 17

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-49 | Aerial shots Signature Breakthrough IX | PP(AER,49): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-48 | B17-001 | 5.45e+28 |
| PP-ANA-49 | Anar: Gift Packaging IX | PP(ANA,49): multiply family normal profit by x1.6 | Own Anar; PP-ANA-48 | B17-002 | 5.59443e+28 |
| PP-BOX-49 | Celebration boxes: Gift Packaging IX | PP(BOX,49): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-48 | B17-003 | 5.74269e+28 |
| PP-CHA-49 | Chakri: Gift Packaging IX | PP(CHA,49): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-48 | B17-004 | 5.89488e+28 |
| PP-PHU-49 | Phuljhari Signature Breakthrough IX | PP(PHU,49): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-48 | B17-005 | 6.05111e+28 |
| PP-ROC-49 | Rockets: Gift Packaging IX | PP(ROC,49): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-48 | B17-006 | 6.21147e+28 |
| PP-SMC-49 | Small crackers: Gift Packaging IX | PP(SMC,49): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-48 | B17-007 | 6.37608e+28 |
| PP-STR-49 | Cracker strings: Gift Packaging IX | PP(STR,49): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-48 | B17-008 | 6.54506e+28 |
| PP-AER-50 | Aerial shots: Festival Display IX | PP(AER,50): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-49 | B17-009 | 6.71851e+28 |
| PP-ANA-50 | Anar: Festival Display IX | PP(ANA,50): multiply family normal profit by x1.6 | Own Anar; PP-ANA-49 | B17-010 | 6.89656e+28 |
| PP-BOX-50 | Celebration boxes: Festival Display IX | PP(BOX,50): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-49 | B17-011 | 7.07933e+28 |
| PP-CHA-50 | Chakri Signature Breakthrough IX | PP(CHA,50): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-49 | B17-012 | 7.26695e+28 |
| PP-PHU-50 | Phuljhari: Festival Display IX | PP(PHU,50): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-49 | B17-013 | 7.45953e+28 |
| PP-ROC-50 | Rockets: Festival Display IX | PP(ROC,50): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-49 | B17-014 | 7.65722e+28 |
| PP-SMC-50 | Small crackers: Festival Display IX | PP(SMC,50): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-49 | B17-015 | 7.86015e+28 |
| PP-STR-50 | Cracker strings: Festival Display IX | PP(STR,50): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-49 | B17-016 | 8.06846e+28 |
| PP-AER-51 | Aerial shots: Regular Customers IX | PP(AER,51): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-50 | B17-017 | 8.28228e+28 |
| PP-ANA-51 | Anar: Regular Customers IX | PP(ANA,51): multiply family normal profit by x1.6 | Own Anar; PP-ANA-50 | B17-018 | 8.50178e+28 |
| PP-BOX-51 | Celebration boxes: Regular Customers IX | PP(BOX,51): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-50 | B17-019 | 8.72709e+28 |
| PP-CHA-51 | Chakri: Regular Customers IX | PP(CHA,51): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-50 | B17-020 | 8.95837e+28 |
| PP-PHU-51 | Phuljhari: Regular Customers IX | PP(PHU,51): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-50 | B17-021 | 9.19578e+28 |
| PP-ROC-51 | Rockets: Regular Customers IX | PP(ROC,51): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-50 | B17-022 | 9.43948e+28 |
| PP-SMC-51 | Small crackers Signature Breakthrough IX | PP(SMC,51): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-50 | B17-023 | 9.68965e+28 |
| PP-STR-51 | Cracker strings: Regular Customers IX | PP(STR,51): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-50 | B17-024 | 9.94644e+28 |
| PC-AER-13 | Aerial shots: Wholesale Deal IV | PC(AER,13): total run family-level discount becomes 28.4375% | Own Aerial shots; PC-AER-12 | B17-025 | 1.021e+29 |
| PC-ANA-13 | Anar: Wholesale Deal IV | PC(ANA,13): total run family-level discount becomes 28.4375% | Own Anar; PC-ANA-12 | B17-026 | 1.04806e+29 |
| PC-BOX-13 | Celebration boxes: Wholesale Deal IV | PC(BOX,13): total run family-level discount becomes 28.4375% | Own Celebration boxes; PC-BOX-12 | B17-027 | 1.07584e+29 |
| PC-CHA-13 | Chakri: Wholesale Deal IV | PC(CHA,13): total run family-level discount becomes 28.4375% | Own Chakri; PC-CHA-12 | B17-028 | 1.10435e+29 |
| PC-PHU-13 | Phuljhari: Wholesale Deal IV | PC(PHU,13): total run family-level discount becomes 28.4375% | Own Phuljhari; PC-PHU-12 | B17-029 | 1.13362e+29 |
| PC-ROC-13 | Rockets: Wholesale Deal IV | PC(ROC,13): total run family-level discount becomes 28.4375% | Own Rockets; PC-ROC-12 | B17-030 | 1.16366e+29 |
| PC-SMC-13 | Small crackers: Wholesale Deal IV | PC(SMC,13): total run family-level discount becomes 28.4375% | Own Small crackers; PC-SMC-12 | B17-031 | 1.1945e+29 |
| PC-STR-13 | Cracker strings: Wholesale Deal IV | PC(STR,13): total run family-level discount becomes 28.4375% | Own Cracker strings; PC-STR-12 | B17-032 | 1.22615e+29 |
| TP-097 | Sales Pitch XVII | TD(49): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-096 | B17-033 | 1.25865e+29 |
| TP-098 | Counter Display XVII | TD(50): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-097 | B17-034 | 1.292e+29 |
| TP-099 | Shop Showmanship XVII | TL(49): linked share A becomes 26.6440678% | Own 2+ families; TP-098 | B17-035 | 1.32624e+29 |
| TP-100 | Crowd Appeal XVII | TL(50): linked share A becomes 26.94915254% | Own 2+ families; TP-099 | B17-036 | 1.36139e+29 |
| TP-101 | Festival Calls XVII | TD(51): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-100 | B17-037 | 1.39747e+29 |
| TP-102 | Shop Reputation XVII | TL(51): linked share A becomes 27.25423729% | Own 2+ families; TP-101 | B17-038 | 1.43451e+29 |
| TC-33 | Sales Coaching VII | TC(33): total run tap-level discount becomes 28.875% | Own 1+ families; TC-32 | B17-039 | 1.47252e+29 |
| TC-34 | Team Handbook VII | TC(34): total run tap-level discount becomes 29.75% | Own 1+ families; TC-33 | B17-040 | 1.51155e+29 |
| GP-65 | Festival Footfall IX | GP(65): normal products and independent tap B x1.10, once | Own 1+ families; GP-64 | B17-041 | 1.5516e+29 |
| GP-66 | Community Trust IX | GP(66): normal products and independent tap B x1.10, once | Own 1+ families; GP-65 | B17-042 | 1.59272e+29 |
| GP-67 | Local Advertising IX | GP(67): normal products and independent tap B x1.10, once | Own 1+ families; GP-66 | B17-043 | 1.63493e+29 |
| GP-68 | Shopfront Glow IX | GP(68): normal products and independent tap B x1.10, once | Own 1+ families; GP-67 | B17-044 | 1.67826e+29 |
| UC-26 | Trade Association VII | UC(26): total run named-cash discount becomes 28.4375% | Own 1+ families; UC-25 | B17-045 | 1.72274e+29 |
| UC-27 | Procurement Office VII | UC(27): total run named-cash discount becomes 29.53125% | Own 1+ families; UC-26 | B17-046 | 1.76839e+29 |
| SK-FR-P-07 | Stronger Rush VII | SP(FR,7): Rush product multiplier x3.5 | Own 1+ families; SK-FR-P-06 | B17-047 | 1.81526e+29 |
| SK-HH-P-07 | More Helping Hands VII | SP(HH,7): 12 auto taps/s | Own 2+ families; SK-HH-P-06 | B17-048 | 1.86337e+29 |
| SK-BO-P-10 | Larger Bulk Orders X | SP(BO,10): 260 seconds of normal P | Own 3+ families; SK-BO-P-09 | B17-049 | 1.91275e+29 |
| SK-FR-D-07 | Festival Rush: Longer Window VII | SD(FR,7): cumulative run duration factor 1.4375 | SK-FR-P-01; SK-FR-D-06 | B17-050 | 1.96344e+29 |
| SK-HH-D-07 | Helping Hands: Longer Window VII | SD(HH,7): cumulative run duration factor 1.4375 | SK-HH-P-01; SK-HH-D-06 | B17-051 | 2.01547e+29 |
| SK-FR-C-07 | Festival Rush: Faster Recovery VII | SC(FR,7): cumulative run cooldown factor 0.78125 | SK-FR-P-01; SK-FR-C-06 | B17-052 | 2.06889e+29 |
| SK-HH-C-07 | Helping Hands: Faster Recovery VII | SC(HH,7): cumulative run cooldown factor 0.78125 | SK-HH-P-01; SK-HH-C-06 | B17-053 | 2.12372e+29 |
| SK-BO-C-10 | Bulk Order: Faster Recovery X | SC(BO,10): cumulative run cooldown factor 0.79166667 | SK-BO-P-01; SK-BO-C-09 | B17-054 | 2.18e+29 |

## Band 18

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-52 | Aerial shots: Signature Collection IX | PP(AER,52): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-51 | B18-001 | 1.6895e+30 |
| PP-ANA-52 | Anar Signature Breakthrough IX | PP(ANA,52): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-51 | B18-002 | 1.73343e+30 |
| PP-BOX-52 | Celebration boxes Signature Breakthrough IX | PP(BOX,52): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-51 | B18-003 | 1.77851e+30 |
| PP-CHA-52 | Chakri: Signature Collection IX | PP(CHA,52): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-51 | B18-004 | 1.82476e+30 |
| PP-PHU-52 | Phuljhari: Signature Collection IX | PP(PHU,52): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-51 | B18-005 | 1.87221e+30 |
| PP-ROC-52 | Rockets: Signature Collection IX | PP(ROC,52): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-51 | B18-006 | 1.9209e+30 |
| PP-SMC-52 | Small crackers: Signature Collection IX | PP(SMC,52): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-51 | B18-007 | 1.97085e+30 |
| PP-STR-52 | Cracker strings: Signature Collection IX | PP(STR,52): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-51 | B18-008 | 2.0221e+30 |
| PP-AER-53 | Aerial shots: Celebration Specialist IX | PP(AER,53): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-52 | B18-009 | 2.07468e+30 |
| PP-ANA-53 | Anar: Celebration Specialist IX | PP(ANA,53): multiply family normal profit by x1.6 | Own Anar; PP-ANA-52 | B18-010 | 2.12864e+30 |
| PP-BOX-53 | Celebration boxes: Celebration Specialist IX | PP(BOX,53): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-52 | B18-011 | 2.18399e+30 |
| PP-CHA-53 | Chakri: Celebration Specialist IX | PP(CHA,53): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-52 | B18-012 | 2.24078e+30 |
| PP-PHU-53 | Phuljhari: Celebration Specialist IX | PP(PHU,53): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-52 | B18-013 | 2.29905e+30 |
| PP-ROC-53 | Rockets Signature Breakthrough IX | PP(ROC,53): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-52 | B18-014 | 2.35884e+30 |
| PP-SMC-53 | Small crackers: Celebration Specialist IX | PP(SMC,53): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-52 | B18-015 | 2.42018e+30 |
| PP-STR-53 | Cracker strings: Celebration Specialist IX | PP(STR,53): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-52 | B18-016 | 2.48311e+30 |
| PP-AER-54 | Aerial shots: Premium Range IX | PP(AER,54): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-53 | B18-017 | 2.54769e+30 |
| PP-ANA-54 | Anar: Premium Range IX | PP(ANA,54): multiply family normal profit by x1.6 | Own Anar; PP-ANA-53 | B18-018 | 2.61394e+30 |
| PP-BOX-54 | Celebration boxes: Premium Range IX | PP(BOX,54): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-53 | B18-019 | 2.68191e+30 |
| PP-CHA-54 | Chakri: Premium Range IX | PP(CHA,54): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-53 | B18-020 | 2.75165e+30 |
| PP-PHU-54 | Phuljhari: Premium Range IX | PP(PHU,54): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-53 | B18-021 | 2.82321e+30 |
| PP-ROC-54 | Rockets: Premium Range IX | PP(ROC,54): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-53 | B18-022 | 2.89663e+30 |
| PP-SMC-54 | Small crackers: Premium Range IX | PP(SMC,54): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-53 | B18-023 | 2.97195e+30 |
| PP-STR-54 | Cracker strings Signature Breakthrough IX | PP(STR,54): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-53 | B18-024 | 3.04923e+30 |
| PC-AER-14 | Aerial shots: Trusted Supplier IV | PC(AER,14): total run family-level discount becomes 30.625% | Own Aerial shots; PC-AER-13 | B18-025 | 3.12853e+30 |
| PC-ANA-14 | Anar: Trusted Supplier IV | PC(ANA,14): total run family-level discount becomes 30.625% | Own Anar; PC-ANA-13 | B18-026 | 3.20988e+30 |
| PC-BOX-14 | Celebration boxes: Trusted Supplier IV | PC(BOX,14): total run family-level discount becomes 30.625% | Own Celebration boxes; PC-BOX-13 | B18-027 | 3.29336e+30 |
| PC-CHA-14 | Chakri: Trusted Supplier IV | PC(CHA,14): total run family-level discount becomes 30.625% | Own Chakri; PC-CHA-13 | B18-028 | 3.379e+30 |
| PC-PHU-14 | Phuljhari: Trusted Supplier IV | PC(PHU,14): total run family-level discount becomes 30.625% | Own Phuljhari; PC-PHU-13 | B18-029 | 3.46687e+30 |
| PC-ROC-14 | Rockets: Trusted Supplier IV | PC(ROC,14): total run family-level discount becomes 30.625% | Own Rockets; PC-ROC-13 | B18-030 | 3.55702e+30 |
| PC-SMC-14 | Small crackers: Trusted Supplier IV | PC(SMC,14): total run family-level discount becomes 30.625% | Own Small crackers; PC-SMC-13 | B18-031 | 3.64952e+30 |
| PC-STR-14 | Cracker strings: Trusted Supplier IV | PC(STR,14): total run family-level discount becomes 30.625% | Own Cracker strings; PC-STR-13 | B18-032 | 3.74442e+30 |
| TP-103 | Sales Pitch XVIII | TD(52): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-102 | B18-033 | 3.8418e+30 |
| TP-104 | Counter Display XVIII | TD(53): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-103 | B18-034 | 3.9417e+30 |
| TP-105 | Shop Showmanship XVIII | TL(52): linked share A becomes 27.55932203% | Own 2+ families; TP-104 | B18-035 | 4.0442e+30 |
| TP-106 | Crowd Appeal XVIII | TL(53): linked share A becomes 27.86440678% | Own 2+ families; TP-105 | B18-036 | 4.14937e+30 |
| TP-107 | Festival Calls XVIII | TD(54): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-106 | B18-037 | 4.25727e+30 |
| TP-108 | Shop Reputation XVIII | TL(54): linked share A becomes 28.16949153% | Own 2+ families; TP-107 | B18-038 | 4.36798e+30 |
| TC-35 | Customer Insight VII | TC(35): total run tap-level discount becomes 30.625% | Own 1+ families; TC-34 | B18-039 | 4.48157e+30 |
| TC-36 | Practice Routine VIII | TC(36): total run tap-level discount becomes 31.5% | Own 1+ families; TC-35 | B18-040 | 4.59811e+30 |
| GP-69 | Delivery Reach IX | GP(69): normal products and independent tap B x1.10, once | Own 1+ families; GP-68 | B18-041 | 4.71768e+30 |
| GP-70 | Celebration Demand IX | GP(70): normal products and independent tap B x1.10, once | Own 1+ families; GP-69 | B18-042 | 4.84036e+30 |
| GP-71 | Brand Recognition IX | GP(71): normal products and independent tap B x1.10, once | Own 1+ families; GP-70 | B18-043 | 4.96623e+30 |
| GP-72 | Customer Loyalty IX | GP(72): normal products and independent tap B x1.10, once | Own 1+ families; GP-71 | B18-044 | 5.09537e+30 |
| UC-28 | Distributor Alliance VII | UC(28): total run named-cash discount becomes 30.625% | Own 1+ families; UC-27 | B18-045 | 5.22788e+30 |
| SK-BO-P-11 | Larger Bulk Orders XI | SP(BO,11): 280 seconds of normal P | Own 3+ families; SK-BO-P-10 | B18-046 | 5.36382e+30 |
| SK-BO-C-11 | Bulk Order: Faster Recovery XI | SC(BO,11): cumulative run cooldown factor 0.77083333 | SK-BO-P-01; SK-BO-C-10 | B18-047 | 5.50331e+30 |
| XL-AER-BOX-04 | Aerial shots / Celebration boxes: Display Partnership IV | XL(AER,BOX,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-AER-BOX-03 | B18-048 | 5.64642e+30 |
| XL-ANA-CHA-04 | Anar / Chakri: Display Partnership IV | XL(ANA,CHA,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-ANA-CHA-03 | B18-049 | 5.79325e+30 |
| XL-BOX-PHU-04 | Celebration boxes / Phuljhari: Display Partnership IV | XL(BOX,PHU,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-BOX-PHU-03 | B18-050 | 5.9439e+30 |
| XL-CHA-ROC-04 | Chakri / Rockets: Display Partnership IV | XL(CHA,ROC,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-CHA-ROC-03 | B18-051 | 6.09847e+30 |
| XL-PHU-ANA-04 | Phuljhari / Anar: Display Partnership IV | XL(PHU,ANA,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-PHU-ANA-03 | B18-052 | 6.25706e+30 |
| XL-ROC-SMC-04 | Rockets / Small crackers: Display Partnership IV | XL(ROC,SMC,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-ROC-SMC-03 | B18-053 | 6.41977e+30 |
| XL-SMC-STR-04 | Small crackers / Cracker strings: Display Partnership IV | XL(SMC,STR,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-SMC-STR-03 | B18-054 | 6.58671e+30 |
| XL-STR-AER-04 | Cracker strings / Aerial shots: Display Partnership IV | XL(STR,AER,4): beneficiary normal product profit x1.50 | Own both families; Source level >= 300; XL-STR-AER-03 | B18-055 | 6.758e+30 |

## Band 19

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-55 | Aerial shots Signature Breakthrough X | PP(AER,55): multiply family normal profit by x8; complete signature effect | Own Aerial shots; PP-AER-54 | B19-001 | 5.23745e+31 |
| PP-ANA-55 | Anar: Gift Packaging X | PP(ANA,55): multiply family normal profit by x1.6 | Own Anar; PP-ANA-54 | B19-002 | 5.40131e+31 |
| PP-BOX-55 | Celebration boxes: Gift Packaging X | PP(BOX,55): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-54 | B19-003 | 5.57029e+31 |
| PP-CHA-55 | Chakri: Gift Packaging X | PP(CHA,55): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-54 | B19-004 | 5.74456e+31 |
| PP-PHU-55 | Phuljhari Signature Breakthrough X | PP(PHU,55): multiply family normal profit by x8; complete signature effect | Own Phuljhari; PP-PHU-54 | B19-005 | 5.92429e+31 |
| PP-ROC-55 | Rockets: Gift Packaging X | PP(ROC,55): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-54 | B19-006 | 6.10963e+31 |
| PP-SMC-55 | Small crackers: Gift Packaging X | PP(SMC,55): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-54 | B19-007 | 6.30078e+31 |
| PP-STR-55 | Cracker strings: Gift Packaging X | PP(STR,55): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-54 | B19-008 | 6.49791e+31 |
| PP-AER-56 | Aerial shots: Festival Display X | PP(AER,56): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-55 | B19-009 | 6.7012e+31 |
| PP-ANA-56 | Anar: Festival Display X | PP(ANA,56): multiply family normal profit by x1.6 | Own Anar; PP-ANA-55 | B19-010 | 6.91085e+31 |
| PP-BOX-56 | Celebration boxes: Festival Display X | PP(BOX,56): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-55 | B19-011 | 7.12706e+31 |
| PP-CHA-56 | Chakri Signature Breakthrough X | PP(CHA,56): multiply family normal profit by x8; complete signature effect | Own Chakri; PP-CHA-55 | B19-012 | 7.35004e+31 |
| PP-PHU-56 | Phuljhari: Festival Display X | PP(PHU,56): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-55 | B19-013 | 7.58e+31 |
| PP-ROC-56 | Rockets: Festival Display X | PP(ROC,56): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-55 | B19-014 | 7.81714e+31 |
| PP-SMC-56 | Small crackers: Festival Display X | PP(SMC,56): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-55 | B19-015 | 8.06171e+31 |
| PP-STR-56 | Cracker strings: Festival Display X | PP(STR,56): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-55 | B19-016 | 8.31393e+31 |
| PP-AER-57 | Aerial shots: Regular Customers X | PP(AER,57): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-56 | B19-017 | 8.57404e+31 |
| PP-ANA-57 | Anar: Regular Customers X | PP(ANA,57): multiply family normal profit by x1.6 | Own Anar; PP-ANA-56 | B19-018 | 8.84228e+31 |
| PP-BOX-57 | Celebration boxes: Regular Customers X | PP(BOX,57): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-56 | B19-019 | 9.11892e+31 |
| PP-CHA-57 | Chakri: Regular Customers X | PP(CHA,57): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-56 | B19-020 | 9.40422e+31 |
| PP-PHU-57 | Phuljhari: Regular Customers X | PP(PHU,57): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-56 | B19-021 | 9.69844e+31 |
| PP-ROC-57 | Rockets: Regular Customers X | PP(ROC,57): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-56 | B19-022 | 1.00019e+32 |
| PP-SMC-57 | Small crackers Signature Breakthrough X | PP(SMC,57): multiply family normal profit by x8; complete signature effect | Own Small crackers; PP-SMC-56 | B19-023 | 1.03148e+32 |
| PP-STR-57 | Cracker strings: Regular Customers X | PP(STR,57): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-56 | B19-024 | 1.06375e+32 |
| PC-AER-15 | Aerial shots: Bulk Procurement IV | PC(AER,15): total run family-level discount becomes 32.8125% | Own Aerial shots; PC-AER-14 | B19-025 | 1.09703e+32 |
| PC-ANA-15 | Anar: Bulk Procurement IV | PC(ANA,15): total run family-level discount becomes 32.8125% | Own Anar; PC-ANA-14 | B19-026 | 1.13135e+32 |
| PC-BOX-15 | Celebration boxes: Bulk Procurement IV | PC(BOX,15): total run family-level discount becomes 32.8125% | Own Celebration boxes; PC-BOX-14 | B19-027 | 1.16675e+32 |
| PC-CHA-15 | Chakri: Bulk Procurement IV | PC(CHA,15): total run family-level discount becomes 32.8125% | Own Chakri; PC-CHA-14 | B19-028 | 1.20325e+32 |
| PC-PHU-15 | Phuljhari: Bulk Procurement IV | PC(PHU,15): total run family-level discount becomes 32.8125% | Own Phuljhari; PC-PHU-14 | B19-029 | 1.24089e+32 |
| PC-ROC-15 | Rockets: Bulk Procurement IV | PC(ROC,15): total run family-level discount becomes 32.8125% | Own Rockets; PC-ROC-14 | B19-030 | 1.27972e+32 |
| PC-SMC-15 | Small crackers: Bulk Procurement IV | PC(SMC,15): total run family-level discount becomes 32.8125% | Own Small crackers; PC-SMC-14 | B19-031 | 1.31975e+32 |
| PC-STR-15 | Cracker strings: Bulk Procurement IV | PC(STR,15): total run family-level discount becomes 32.8125% | Own Cracker strings; PC-STR-14 | B19-032 | 1.36104e+32 |
| TP-109 | Sales Pitch XIX | TD(55): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-108 | B19-033 | 1.40363e+32 |
| TP-110 | Counter Display XIX | TD(56): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-109 | B19-034 | 1.44754e+32 |
| TP-111 | Shop Showmanship XIX | TL(55): linked share A becomes 28.47457627% | Own 2+ families; TP-110 | B19-035 | 1.49283e+32 |
| TP-112 | Crowd Appeal XIX | TL(56): linked share A becomes 28.77966102% | Own 2+ families; TP-111 | B19-036 | 1.53953e+32 |
| TP-113 | Festival Calls XIX | TD(57): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-112 | B19-037 | 1.5877e+32 |
| TP-114 | Shop Reputation XIX | TL(57): linked share A becomes 29.08474576% | Own 2+ families; TP-113 | B19-038 | 1.63737e+32 |
| TC-37 | Counter Training VIII | TC(37): total run tap-level discount becomes 32.375% | Own 1+ families; TC-36 | B19-039 | 1.6886e+32 |
| TC-38 | Sales Coaching VIII | TC(38): total run tap-level discount becomes 33.25% | Own 1+ families; TC-37 | B19-040 | 1.74143e+32 |
| GP-73 | Festival Footfall X | GP(73): normal products and independent tap B x1.10, once | Own 1+ families; GP-72 | B19-041 | 1.79591e+32 |
| GP-74 | Community Trust X | GP(74): normal products and independent tap B x1.10, once | Own 1+ families; GP-73 | B19-042 | 1.85209e+32 |
| GP-75 | Local Advertising X | GP(75): normal products and independent tap B x1.10, once | Own 1+ families; GP-74 | B19-043 | 1.91004e+32 |
| GP-76 | Shopfront Glow X | GP(76): normal products and independent tap B x1.10, once | Own 1+ families; GP-75 | B19-044 | 1.9698e+32 |
| UC-29 | Supplier Network VIII | UC(29): total run named-cash discount becomes 31.71875% | Own 1+ families; UC-28 | B19-045 | 2.03142e+32 |
| UC-30 | Trade Association VIII | UC(30): total run named-cash discount becomes 32.8125% | Own 1+ families; UC-29 | B19-046 | 2.09498e+32 |

## Band 20

Access: cash, ownership, level, and prior-upgrade requirements only. No completed-prestige requirement.

| ID | Name | Effect | Requirements | Price key | Base cash price |
|---|---|---|---|---|---:|
| PP-AER-58 | Aerial shots: Signature Collection X | PP(AER,58): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-57 | B20-001 | 1.62361e+33 |
| PP-ANA-58 | Anar Signature Breakthrough X | PP(ANA,58): multiply family normal profit by x8; complete signature effect | Own Anar; PP-ANA-57 | B20-002 | 1.66358e+33 |
| PP-BOX-58 | Celebration boxes Signature Breakthrough X | PP(BOX,58): multiply family normal profit by x8; complete signature effect | Own Celebration boxes; PP-BOX-57 | B20-003 | 1.70454e+33 |
| PP-CHA-58 | Chakri: Signature Collection X | PP(CHA,58): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-57 | B20-004 | 1.7465e+33 |
| PP-PHU-58 | Phuljhari: Signature Collection X | PP(PHU,58): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-57 | B20-005 | 1.7895e+33 |
| PP-ROC-58 | Rockets: Signature Collection X | PP(ROC,58): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-57 | B20-006 | 1.83355e+33 |
| PP-SMC-58 | Small crackers: Signature Collection X | PP(SMC,58): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-57 | B20-007 | 1.87869e+33 |
| PP-STR-58 | Cracker strings: Signature Collection X | PP(STR,58): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-57 | B20-008 | 1.92495e+33 |
| PP-AER-59 | Aerial shots: Celebration Specialist X | PP(AER,59): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-58 | B20-009 | 1.97234e+33 |
| PP-ANA-59 | Anar: Celebration Specialist X | PP(ANA,59): multiply family normal profit by x1.6 | Own Anar; PP-ANA-58 | B20-010 | 2.02089e+33 |
| PP-BOX-59 | Celebration boxes: Celebration Specialist X | PP(BOX,59): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-58 | B20-011 | 2.07065e+33 |
| PP-CHA-59 | Chakri: Celebration Specialist X | PP(CHA,59): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-58 | B20-012 | 2.12162e+33 |
| PP-PHU-59 | Phuljhari: Celebration Specialist X | PP(PHU,59): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-58 | B20-013 | 2.17386e+33 |
| PP-ROC-59 | Rockets Signature Breakthrough X | PP(ROC,59): multiply family normal profit by x8; complete signature effect | Own Rockets; PP-ROC-58 | B20-014 | 2.22737e+33 |
| PP-SMC-59 | Small crackers: Celebration Specialist X | PP(SMC,59): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-58 | B20-015 | 2.28221e+33 |
| PP-STR-59 | Cracker strings: Celebration Specialist X | PP(STR,59): multiply family normal profit by x1.6 | Own Cracker strings; PP-STR-58 | B20-016 | 2.3384e+33 |
| PP-AER-60 | Aerial shots: Premium Range X | PP(AER,60): multiply family normal profit by x1.6 | Own Aerial shots; PP-AER-59 | B20-017 | 2.39597e+33 |
| PP-ANA-60 | Anar: Premium Range X | PP(ANA,60): multiply family normal profit by x1.6 | Own Anar; PP-ANA-59 | B20-018 | 2.45495e+33 |
| PP-BOX-60 | Celebration boxes: Premium Range X | PP(BOX,60): multiply family normal profit by x1.6 | Own Celebration boxes; PP-BOX-59 | B20-019 | 2.51539e+33 |
| PP-CHA-60 | Chakri: Premium Range X | PP(CHA,60): multiply family normal profit by x1.6 | Own Chakri; PP-CHA-59 | B20-020 | 2.57732e+33 |
| PP-PHU-60 | Phuljhari: Premium Range X | PP(PHU,60): multiply family normal profit by x1.6 | Own Phuljhari; PP-PHU-59 | B20-021 | 2.64077e+33 |
| PP-ROC-60 | Rockets: Premium Range X | PP(ROC,60): multiply family normal profit by x1.6 | Own Rockets; PP-ROC-59 | B20-022 | 2.70578e+33 |
| PP-SMC-60 | Small crackers: Premium Range X | PP(SMC,60): multiply family normal profit by x1.6 | Own Small crackers; PP-SMC-59 | B20-023 | 2.7724e+33 |
| PP-STR-60 | Cracker strings Signature Breakthrough X | PP(STR,60): multiply family normal profit by x8; complete signature effect | Own Cracker strings; PP-STR-59 | B20-024 | 2.84065e+33 |
| PC-AER-16 | Aerial shots: Factory Partnership IV | PC(AER,16): total run family-level discount becomes 35% | Own Aerial shots; PC-AER-15 | B20-025 | 2.91058e+33 |
| PC-ANA-16 | Anar: Factory Partnership IV | PC(ANA,16): total run family-level discount becomes 35% | Own Anar; PC-ANA-15 | B20-026 | 2.98224e+33 |
| PC-BOX-16 | Celebration boxes: Factory Partnership IV | PC(BOX,16): total run family-level discount becomes 35% | Own Celebration boxes; PC-BOX-15 | B20-027 | 3.05566e+33 |
| PC-CHA-16 | Chakri: Factory Partnership IV | PC(CHA,16): total run family-level discount becomes 35% | Own Chakri; PC-CHA-15 | B20-028 | 3.13089e+33 |
| PC-PHU-16 | Phuljhari: Factory Partnership IV | PC(PHU,16): total run family-level discount becomes 35% | Own Phuljhari; PC-PHU-15 | B20-029 | 3.20797e+33 |
| PC-ROC-16 | Rockets: Factory Partnership IV | PC(ROC,16): total run family-level discount becomes 35% | Own Rockets; PC-ROC-15 | B20-030 | 3.28695e+33 |
| PC-SMC-16 | Small crackers: Factory Partnership IV | PC(SMC,16): total run family-level discount becomes 35% | Own Small crackers; PC-SMC-15 | B20-031 | 3.36787e+33 |
| PC-STR-16 | Cracker strings: Factory Partnership IV | PC(STR,16): total run family-level discount becomes 35% | Own Cracker strings; PC-STR-15 | B20-032 | 3.45078e+33 |
| TP-115 | Sales Pitch XX | TD(58): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-114 | B20-033 | 3.53574e+33 |
| TP-116 | Counter Display XX | TD(59): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-115 | B20-034 | 3.62278e+33 |
| TP-117 | Shop Showmanship XX | TL(58): linked share A becomes 29.38983051% | Own 2+ families; TP-116 | B20-035 | 3.71197e+33 |
| TP-118 | Crowd Appeal XX | TL(59): linked share A becomes 29.69491525% | Own 2+ families; TP-117 | B20-036 | 3.80336e+33 |
| TP-119 | Festival Calls XX | TD(60): independent tap B x1.20; add one owned TD step to mastery M | Own 1+ families; TP-118 | B20-037 | 3.89699e+33 |
| TP-120 | Shop Reputation XX | TL(60): linked share A becomes 30% | Own 2+ families; TP-119 | B20-038 | 3.99293e+33 |
| TC-39 | Team Handbook VIII | TC(39): total run tap-level discount becomes 34.125% | Own 1+ families; TC-38 | B20-039 | 4.09124e+33 |
| TC-40 | Customer Insight VIII | TC(40): total run tap-level discount becomes 35% | Own 1+ families; TC-39 | B20-040 | 4.19196e+33 |
| GP-77 | Delivery Reach X | GP(77): normal products and independent tap B x1.10, once | Own 1+ families; GP-76 | B20-041 | 4.29516e+33 |
| GP-78 | Celebration Demand X | GP(78): normal products and independent tap B x1.10, once | Own 1+ families; GP-77 | B20-042 | 4.4009e+33 |
| GP-79 | Brand Recognition X | GP(79): normal products and independent tap B x1.10, once | Own 1+ families; GP-78 | B20-043 | 4.50925e+33 |
| GP-80 | Customer Loyalty X | GP(80): normal products and independent tap B x1.10, once | Own 1+ families; GP-79 | B20-044 | 4.62027e+33 |
| UC-31 | Procurement Office VIII | UC(31): total run named-cash discount becomes 33.90625% | Own 1+ families; UC-30 | B20-045 | 4.73401e+33 |
| UC-32 | Distributor Alliance VIII | UC(32): total run named-cash discount becomes 35% | Own 1+ families; UC-31 | B20-046 | 4.85056e+33 |
| SK-FR-P-08 | Stronger Rush VIII | SP(FR,8): Rush product multiplier x4 | Own 1+ families; SK-FR-P-07 | B20-047 | 4.96998e+33 |
| SK-HH-P-08 | More Helping Hands VIII | SP(HH,8): 16 auto taps/s | Own 2+ families; SK-HH-P-07 | B20-048 | 5.09233e+33 |
| SK-BO-P-12 | Larger Bulk Orders XII | SP(BO,12): 300 seconds of normal P | Own 3+ families; SK-BO-P-11 | B20-049 | 5.2177e+33 |
| SK-FR-D-08 | Festival Rush: Longer Window VIII | SD(FR,8): cumulative run duration factor 1.5 | SK-FR-P-01; SK-FR-D-07 | B20-050 | 5.34616e+33 |
| SK-HH-D-08 | Helping Hands: Longer Window VIII | SD(HH,8): cumulative run duration factor 1.5 | SK-HH-P-01; SK-HH-D-07 | B20-051 | 5.47777e+33 |
| SK-FR-C-08 | Festival Rush: Faster Recovery VIII | SC(FR,8): cumulative run cooldown factor 0.75 | SK-FR-P-01; SK-FR-C-07 | B20-052 | 5.61263e+33 |
| SK-HH-C-08 | Helping Hands: Faster Recovery VIII | SC(HH,8): cumulative run cooldown factor 0.75 | SK-HH-P-01; SK-HH-C-07 | B20-053 | 5.75081e+33 |
| SK-BO-C-12 | Bulk Order: Faster Recovery XII | SC(BO,12): cumulative run cooldown factor 0.75 | SK-BO-P-01; SK-BO-C-11 | B20-054 | 5.89239e+33 |
| PF-MIX-04 | Celebration Mix: Portfolio Milestone IV | PF(MIX,4): each listed family's normal profit x1.30 | BOX+ROC levels >= 300; PF-MIX-03 (every listed family) | B20-055 | 6.03746e+33 |
| PF-RHYTHM-04 | Sound and Rhythm: Portfolio Milestone IV | PF(RHYTHM,4): each listed family's normal profit x1.30 | SMC+STR levels >= 300; PF-RHYTHM-03 (every listed family) | B20-056 | 6.18609e+33 |
| PF-SKY-04 | Skyline Collection: Portfolio Milestone IV | PF(SKY,4): each listed family's normal profit x1.30 | ROC+AER levels >= 300; PF-SKY-03 (every listed family) | B20-057 | 6.33839e+33 |
| PF-STREET-04 | Street Favourites: Portfolio Milestone IV | PF(STREET,4): each listed family's normal profit x1.30 | PHU+ANA+CHA levels >= 300; PF-STREET-03 (every listed family) | B20-058 | 6.49443e+33 |
