# Spec: F07 Basic HUD and Windows build

## Goal

The backlog says "hearts, wave and dash readiness make the rules visible", and the HLD's target experience needs mistakes to be "understandable": I can only understand a lost run if I could see how many hearts were left, which wave I was in and whether the dash was ready. F07 puts those three things on screen without covering the floor where the fight happens, and then produces the Windows build that makes the complete game, from main menu to victory or defeat, playable outside the Editor.

## Acceptance criteria

Everything from F01–F06 still holds. "Hearts", "Current Wave" and the nose colour are the Inspector and placeholder signals from earlier features; the HUD must always agree with them. The HUD is a row drawn over the top wall strip of the Arena (the 1-unit-thick wall along the top of the 16:9 picture): hearts at the left, wave text in the centre, dash bar at the right.

**HUD**

1. **HUD at the start of a run.** Starting state: `Arena.unity` open, Game view at 1920 × 1080. Action: enter Play Mode. I see: over the top wall, three solid red squares at the left, the white text "Wave 1 / 3" in the centre, and a full cyan bar at the right; no part of the HUD covers the 32 × 18 floor.
2. **Losing a heart.** Starting state: Hearts = 3. Action: take one hit. I see: on the same frame Hearts becomes 2, the right-most square turns into a hollow red outline, leaving 2 solid squares and 1 hollow.
3. **Hearts restored.** Starting state: wave 1 or 2, Hearts = 1 (1 solid square, 2 hollow), one enemy left. Action: destroy it. I see: all 3 squares are solid again at once.
4. **No hearts.** Starting state: Hearts = 1. Action: take a hit. I see: 3 hollow squares, 0 solid.
5. **Dash bar on a miss.** Starting state: dash ready, bar full, no enemy ahead. Action: left-click once. I see: the bar is empty during the dash; when the dash ends it refills from left to right over 1 second and is full at the moment the nose turns red.
6. **Dash bar on a kill.** Starting state: dash ready, bar full, an enemy within 4 units ahead. Action: left-click once. I see: the bar is empty for the 0.15 seconds of the dash and full again the moment the dash ends, with no refill.
7. **Wave text.** Starting state: wave 1, one enemy left, text "Wave 1 / 3". Action: destroy it. I see: the text reads "Wave 2 / 3" at once, during the 2-second pause; after wave 2 it reads "Wave 3 / 3", and it still reads "Wave 3 / 3" after victory.
8. **HUD under the ending panels and on the menu.** Starting state: a run ends. Action: wait for the victory or defeat panel; later click Menu. I see: the HUD stays visible but darkened under the 70% black layer, showing the values from the moment the game froze; the main menu shows no HUD.
9. **HUD is letterboxed.** Starting state: Game view set to 1024 × 768, run in progress. Action: look. I see: the HUD row sits inside the 16:9 picture over the top wall, not in the black bars.
10. **The game does not depend on the HUD.** Starting state: not in Play Mode; untick the HUD object in the Arena Hierarchy. Action: enter Play Mode and play a wave. I see: no HUD and no Console errors; walking, dashing, hearts, waves and endings all work. (Tick it again without saving the change.)

**Windows build**

11. **The build exists.** Starting state: the build step has run. Action: look in the project folder and run `git status`. I see: `Builds/Windows/DASHBLADE.exe` with its data folder beside it; nothing under `Builds/` is listed by git.
12. **It opens on the main menu, fullscreen.** Starting state: the Editor is closed or in the background. Action: double-click `DASHBLADE.exe`. I see: a borderless fullscreen window at the monitor's own resolution showing the main menu in a 16:9 picture (black bars if the monitor is not 16:9); no "Development Build" label in any corner.
13. **The whole loop works in the build.** Starting state: main menu in the build. Action: click Play; lose all 3 hearts; click Retry; clear all 3 waves; click Menu; click Play again. I see: every step behaves as in F06 (DEFEAT panel 1 second after the last heart, a fresh run on Retry, VICTORY panel 1 second after the last enemy, the main menu, a fresh running game), with the HUD of criteria 1–7 visible during each run.
14. **Quit closes the build.** Starting state: main menu, victory panel or defeat panel in the build. Action: click Quit. I see: the game window closes and `DASHBLADE.exe` is no longer running.
15. **Focus loss in the build.** Starting state: a run in the build, holding D so the player walks right. Action: press Alt+Tab, release D, then Alt+Tab back. I see: the player is standing still, and moves again only when I press a movement key (F01's rule, which could not be checked in the Editor).

## Edge cases

- **The HUD only listens:** it reads hearts, wave and dash readiness through the C# events and values the game already announces. No game script refers to the HUD.
- **Hearts are lost from right to left** and all restored together. The HUD never shows more or fewer than 3 squares.
- **Blocked dash (zero length):** the bar empties and refills over 1 second, as for any miss.
- **Miss Cooldown changed in `DashConfig`:** the refill takes that long; the HUD has no timing number of its own.
- **Protection blinking:** it does not change the HUD.
- **While frozen behind an ending panel:** the HUD does not change; a half-refilled dash bar stays half-full.
- **The HUD never takes mouse input:** aiming and left-click work the same when the cursor is over a HUD item, and the HUD never blocks a panel button.
- **Test scenes:** `Test_DashAndHearts` and `Test_Enemies` have no HUD.
- **Scenes in the build:** `MainMenu` (index 0) and `Arena` (index 1) only.
- **Esc and Alt+F4 in the build:** Esc does nothing (F06); Alt+F4 closes the window as Windows normally does.
- **Building again:** a new build replaces the old one in `Builds/Windows/`.
- **If the Unity CLI cannot build while the Editor has the project open:** the agent does not work around it; it prepares the settings and lists the exact manual steps for me.

## Examples

- Hearts 3 → hit → 2 solid, 1 hollow → hit → 1 solid, 2 hollow → wave cleared → 3 solid.
- Dash ready, miss: bar empty for 0.15 s, half full 0.5 s after the dash ends, full at 1 s.
- Dash ready, kill: bar empty for 0.15 s, then full.
- Text: "Wave 1 / 3" from 0 s; "Wave 2 / 3" from the moment wave 1's last enemy is destroyed; "Wave 3 / 3" from the moment wave 2's last enemy is destroyed until the run ends.
- Build on a 2560 × 1440 monitor: fullscreen 16:9 picture, no bars. On a 1920 × 1200 monitor: a 1920 × 1080 picture with black bars of 60 pixels above and below.

## Out of scope

- Heart, dash and warning glyph sprites, fonts, HUD animation, sounds and music (F08). Hit flash and impact polish (F09).
- Enemies-left counter, score, run timer, damage numbers, minimap, cooldown numbers, on-screen controls reminder.
- Options, resolution or window-mode settings, key rebinding.
- Application icon, custom splash screen, installer, zip packaging, uploading the build, other platforms, development builds, automated or CI builds.
- Tuning the run toward the 3–5 minute target (measured and adjusted in Home Assignment 03).

## Context for the agent (optional)

Read first: `CLAUDE.md`, the F02, F03, F05 and F06 specs, and the code in `Assets/_Game/Scripts/`.

Decisions that came from the interview and are not obvious from the criteria:

- The nose keeps its F02 colour signal; the HUD bar is an additional signal, not a replacement.
- The refill needs the cooldown's progress, which F02 may only announce as ready or not ready. Reading progress from the dash is fine; the dash must still know nothing about the HUD.
- The build is a normal (non-development) Windows 64-bit build, product name "DASHBLADE", borderless fullscreen at native resolution, produced by the agent through the Unity CLI at `Builds/Windows/DASHBLADE.exe`. `/[Bb]uilds/` is already in `.gitignore`.
- Exact sizes of the squares, text and bar are the builder's choice as long as the whole row fits inside the 1-unit top wall strip; report them.

Placeholders (to be replaced in F08):

| Placeholder | Stands in for |
|---|---|
| Solid and hollow red squares | Full and empty heart sprites |
| Cyan bar | Dash ready and recharging glyphs |
| White default-font text | Readable HUD typography |

Open values: none.
