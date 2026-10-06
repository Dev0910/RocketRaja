# Compact milestone feedback

RR-08-04 is implemented. Successful cash purchases now distinguish new unique purchases from rebuys. A meaningful earlier-product income/share increase can add one comeback line. Buy All reports aggregate counts and at most the final crossed shop stage; it never queues hundreds of celebrations. Skill results explain Rush/Hands overlap and Bulk Order's normal-income basis. First Prestige explains the free draws/reset/keep result, and acknowledged card reveals report their actual level change.

The saved notice has a 48px-minimum Dismiss action at tested widths, bounded copy and no required sound or animation. Dismissal changes presentation only. Covered touches cannot hit controls beneath the notice. Successful-result text is composed only after the durable command succeeds; an injected save failure produces no false success or comeback.

Workflow: [wireframe](../../UI%20Builder/Wireframes/Redesign/feedback-v1.svg), [component reuse](../../UI%20Builder/Reference%20Images/Redesign/feedback-reuse.md), [editable HTML](../../UI%20Builder/HTML/Redesign/feedback.html), saved native prefab. UI approval pauses were waived by the user.

Evidence on 2 October 2026:

- [HTML](Feedback-HTML.txt): 4 checks, seven cases at 390×844 and 360×540.
- [Native](Unity-Feedback-UI.txt): 28 checks, actual purchase-derived copy, seven native messages at both sizes, muted/reduced motion, failed saving, exact UI identity preservation.
- [Catalogue](Unity-Catalogue-Play.txt): 69 checks including physical dismissal without a tap or economic command. The final run and an additional repeat both passed. An earlier attempt failed the batch assertion without enough diagnostics to establish a cause; the assertion now includes route, missing IDs, notice and saving diagnostics. No product logic was changed to bypass it.
- [Skills](Unity-Return-Skills-Play.txt): 45 checks including overlap and Bulk Order feedback.
- [Talent](Unity-Talent-Play.txt): 54 checks across actual restart, first Prestige and reveal acknowledgement.
- [Visual progression](Unity-Visual-Play.txt): 36 checks across two sessions; a real 1,000-upgrade batch reports aggregate totals and only its final milestone.

These are Editor/browser checks. Full game device acceptance remains separate and open.
