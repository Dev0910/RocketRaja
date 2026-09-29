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
