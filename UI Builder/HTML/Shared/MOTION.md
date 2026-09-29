# HTML motion

Added at the user's direction: animations for opening and closing pages and popups, plus suitable interaction feedback. This is presentation in the HTML preview only.

- Lower panels: 230 ms entrance, 160 ms exit. Home/panel changes slide vertically; switching between Player, Products and Legacy uses a small directional slide. The world and wallet retain their exact geometry.
- Popups: 190 ms backdrop fade, 250 ms card lift and subtle scale; 160 ms dismissal. Nested dialogs use the same treatment. Blocking save recovery remains blocking.
- Quantity choices: 170 ms reveal. Notices: 180 ms reveal. Next-display progress: 360 ms fill transition when the value changes.
- Buttons: brief press feedback and desktop hover highlights. Selected navigation icons resize over 180 ms. No perpetual movement or automatic reward effects.

`motion.js` uses the existing DOM; outgoing elements remain briefly visible, hidden from accessibility and inert, then clean up. Routing state and focus ownership remain in the preview scripts. Motion cancels on interruption, resize, hidden browser tabs or a reduced-motion preference change. Both `prefers-reduced-motion` and the in-game setting suppress motion. List scroll positions and gameplay fixture values are preserved.

`build-screens.cjs` loads `motion.css` and `motion.js` for all twelve shared entries. The standalone Home study also loads them for its route popup. Existing UI removals remain covered by `verify-requested-ui.cjs`; the animation lifecycle, rapid switching, focus restoration, nested dialogs, resize, reduced motion and Home route popup are checked by `verify-motion.cjs`.
