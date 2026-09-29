# HTML typography audition — 2026-09-28

Default: Baloo 2 (700) headings, Nunito interface and figures. Alternatives: Fraunces (700), Fredoka (600). Review at `../../font-review.html`. Unity changes require a separate user direction after review.

Variable TTF files downloaded from the Google Fonts repository, `https://github.com/google/fonts/tree/main/ofl/`, from directories `baloo2`, `nunito`, `fraunces`, and `fredoka`. Original filenames: `Baloo2[wght].ttf`, `Nunito[wght].ttf`, `Fraunces[SOFT,WONK,opsz,wght].ttf`, and `Fredoka[wdth,wght].ttf`. Each accompanying `*-OFL.txt` contains its font's license. Local files avoid a live font CDN dependency.

`../typography.css` defines shared roles; `../../Home/home.css` imports it for every game entry. The review page changes CSS variables within same-origin frames, without persisting a choice or modifying game state outside preview fixtures.
