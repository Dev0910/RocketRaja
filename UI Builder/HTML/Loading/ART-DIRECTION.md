# Loading — visual and implementation record

28 October 2026. Wireframe revision 1 explicitly approved ("Approve revision 1"). Generated festival-stall illustration revision 1 explicitly approved with direction to create HTML ("Approve illustration and create HTML preview"). HTML revision 1 explicitly approved and Unity reconstruction authorized ("Approve HTML and implement in Unity"). Native verification is recorded separately.

Approved composition: `../../Wireframes/Loading/loading-r1.svg`. Approved illustration: `../../Reference Images/Loading/festival-stall-r1.png`. Style: `../../ART-DIRECTION.md`, Gouache Storybook. Artwork was created with the built-in image_gen tool, using the approved wireframe for composition and the selected Home Gouache reference for style. It is decorative; title, label and progress are editable HTML.

Prompt: new portrait festival-stall loading background; straight-on view, maroon/cream awning, marigolds, warm lanterns against indigo evening streets, two shelves of four fireworks displays; quiet upper and lower regions for live title/loading UI; visible gouache pigment; no text, labels, panels, buttons or baked progress bar.

The displayed 58% is a fixed visual-review fixture. HTML performs no authentication, persistence, gameplay, ads or automatic navigation. The native loading flow uses real startup phases and scene progress.

| Intent | HTML | Native owner / status |
|---|---|---|
| New festival illustration and live title | Local approved PNG plus live heading | Saved Bootstrap reconstructed after HTML approval and explicit Unity direction |
| Automatic startup and read-only loading bar | Editable label and CSS bar; 58% review fixture | BootstrapController.LoadLocal/SetLoading with saved native geometry and Editor-authored text material slots |
| Local save and recovery | Metadata documents existing behavior | RedesignRuntimeDriver / RedesignSaveStore / ClassicApplication remain authority |

Review captures: 1080×2200, 390×844, 360×640, 360×540. Review for crop/readability and no overflow. Browser evidence does not establish device safe-area or native behavior.

Native verification: 12 loading/recovery/legacy captures and live-text fit checks passed; seven isolated startup scenarios passed with runtime-error detection, and the existing 864 core checks passed. Evidence: `../../../Documentation/Implementation/LocalOnly/`. Native title/bar use the project's existing fonts and sliced UI surfaces; artwork is the approved PNG.
