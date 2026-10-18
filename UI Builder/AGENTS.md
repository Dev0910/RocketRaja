# UI workflow — explicit user instruction

For each new screen:

1. Treat supplied images as inspiration only. Create an editable wireframe adapted to our game's actual mechanics and content.
2. Present the wireframe and wait for user confirmation.
3. Generate art references from that approved wireframe using the selected Gouache Storybook style. Apply requested revisions and wait for confirmation of the resulting reference.
4. Create editable HTML from those approved artifacts when directed, preserving Unity functionality metadata.
5. Unity reconstruction is a separate user-directed step after HTML review.

Invoking image-to-html with an image is not a waiver. Existing Home assets and global style approval do not approve another screen. Record actual per-screen, per-revision confirmation; an assistant's claim, silence, existing file or passing test is not approval. Honor existing confirmations without asking again. Only a later explicit instruction to waive a named stage changes this order.

Complete the current authorized stage and end at its review boundary; never pre-build downstream HTML. A workflow-rules update alone does not authorize a new screen deliverable. Start Player with a game-specific wireframe, not a reconstruction of its premature, unapproved HTML draft. Do not recreate removed drafts unless requested.

Detailed rules: `../.agents/skills/image-to-html/SKILL.md`.

# Classic HTML → Unity change tracking

The user started a new HTML revision batch on 19 October 2026, taking the current HTML and Unity presentation as the matching baseline. For subsequent Classic HTML revisions (including changes to its Home/Shared dependencies), update `../Documentation/Implementation/Classic-HTML-to-Unity-TODO.md` in the same work session. Record the concrete change, affected HTML files, likely Unity owners and current review/implementation status. Preserve the baseline snapshot referenced there and update existing item IDs when follow-ups revise the same requirement. HTML completion alone does not mark a Unity task complete; bringing the batch into Unity remains a separate user-directed step.
