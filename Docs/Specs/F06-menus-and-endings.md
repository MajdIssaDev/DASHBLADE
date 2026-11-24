# Spec: F06 Menus and endings

## Goal

The HLD says either ending "offers retry from wave 1, return to menu or quit", and its target experience wants retries to be "inviting". F06 wraps the run in the complete flow the backlog requires for the prototype: a main menu that shows the controls and starts the game, and a victory or defeat panel that appears 1 second after the run ends and puts Retry one click away. After this feature the game is playable from main menu to victory or defeat and round again without touching the Editor.

## Acceptance criteria

Everything from F01–F05 still holds. Menus are operated with the mouse only. "Run" is F05's run component in the Arena scene. All screens are plain placeholders: dark panel, white text, grey buttons.

1. **The main menu.** Starting state: `Assets/_Game/Scenes/MainMenu.unity` open, Game view at 1920 × 1080. Action: enter Play Mode. I see: the title "DASHBLADE"; three lines "WASD or arrow keys: move", "Mouse: aim", "Left-click: dash and slash"; the line "Clear all three waves. You have three hearts."; exactly two buttons, "Play" and "Quit"; no player, enemies or arena.
2. **Buttons react to the mouse.** Starting state: main menu showing. Action: move the cursor onto Play, then off it. I see: the button turns a lighter grey while the cursor is on it and goes back when it leaves. The same is true of every button in the game.
3. **Play starts a fresh run.** Starting state: main menu showing. Action: click Play once. I see: the Arena scene loads; Run shows Current Wave = 1 and Run State = Playing; Hearts = 3; the player stands at (−14.5, 0) and does not dash because of that click; after 2 seconds wave 1 fades in.
4. **Victory panel.** Starting state: wave 3, one enemy left. Action: destroy it and keep walking. I see: the game runs on for 1 second (I can still walk); then the arena darkens under a 70% black layer and a centred panel shows "VICTORY", "All three waves cleared." and three buttons "Retry", "Menu", "Quit"; from that moment nothing in the arena moves and WASD, mouse aim and left-click no longer affect the player.
5. **Defeat panel.** Starting state: any wave, Hearts = 1. Action: take a hit. I see: the player fades out; 1 second after the heart was lost the arena darkens under the same 70% black layer and a centred panel shows "DEFEAT", "You lost all three hearts." and the buttons "Retry", "Menu", "Quit"; enemies and projectiles stop moving where they are.
6. **Retry.** Starting state: the victory or the defeat panel is showing. Action: click Retry once. I see: the Arena scene loads again; Current Wave = 1, Hearts = 3, player at (−14.5, 0), a pillar layout is picked again; the game is running, not frozen: wave 1 fades in after 2 seconds and its first strips appear 1 second after that.
7. **Menu.** Starting state: the victory or the defeat panel is showing. Action: click Menu once, then click Play. I see: the main menu of criterion 1; then a fresh run exactly as in criterion 3, running and not frozen.
8. **Quit.** Starting state: the main menu, the victory panel or the defeat panel is showing in the Editor. Action: click Quit once. I see: Play Mode stops. (In the Windows build the same button closes the game; that is checked in F07.)
9. **Esc does nothing.** Starting state: a run in progress, then the main menu. Action: press Esc. I see: nothing changes in either place.
10. **Scenes are registered in order.** Starting state: not in Play Mode. Action: open File > Build Profiles (the scene list). I see: `MainMenu` at index 0 and `Arena` at index 1, both ticked; `Test_DashAndHearts` and `Test_Enemies` are not in the list.
11. **Menus are letterboxed too.** Starting state: Game view set to 1024 × 768; main menu in Play Mode. Action: look, then click Play. I see: black bars above and below a 16:9 picture that holds the whole menu, laid out as at 1920 × 1080 but smaller; the Play button responds where it is drawn.
12. **Tuning lives in a config asset.** Starting state: not in Play Mode. `Assets/_Game/Config/EndingConfig.asset` shows Ending Delay = 1 and Dim Strength = 0.7. Action: set Ending Delay to 4, start a run from the Arena scene, lose all three hearts. I see: the defeat panel appears 4 seconds after the last heart was lost. (Set it back to 1.)
13. **The Arena scene still starts on its own.** Starting state: `Arena.unity` open (not the menu). Action: enter Play Mode and play to an ending; click Menu. I see: the run starts directly as in F05; the ending panel appears as above; Menu loads the main menu.

## Edge cases

- **Only one ending:** the first result of a run decides the panel; it never changes or stacks.
- **The 1 second before the panel:** the game runs normally. After a victory the player can walk and dash in the empty arena; after a defeat the player has already faded and has no control.
- **Frozen means frozen:** while a panel shows, game time is stopped: no movement, no timers, no fades, no new warnings.
- **Leaving a panel always unfreezes:** after Retry or Menu the next scene runs at normal speed. A run started from the menu after an ending must never begin frozen.
- **The click that presses a button never dashes:** not on the frozen run behind a panel, and not on the first frame of the new run.
- **Clicking twice quickly, or two buttons in a row:** only the first click counts.
- **Clicking the panel or the dark area outside the buttons:** nothing happens.
- **Keyboard:** no key activates, selects or moves between buttons (not Enter, Space, Tab or the arrow keys), including just after a button was clicked. WASD on the main menu does nothing.
- **Window resized or not 16:9:** the menu and panels stay inside the 16:9 picture and scale with it.
- **Window loses focus on a menu or panel:** nothing changes.
- **Test scenes:** `Test_DashAndHearts` and `Test_Enemies` have no menus or panels; they behave as before.
- **Quit in the Windows build:** closes the application without a confirmation.

## Examples

- Main menu → Play → lose three hearts in wave 1 → 1 second → DEFEAT panel → Retry → wave 1 again with 3 hearts, running.
- Main menu → Play → clear three waves → 1 second → VICTORY panel → Menu → main menu → Play → wave 1, running.
- VICTORY panel → Quit (Editor): Play Mode stops.
- DEFEAT panel showing, a Charger frozen mid-charge behind it: it stays exactly where it was until Retry or Menu.
- Ending Delay set to 4: the panel appears 4 seconds after the run ends instead of 1.

## Out of scope

- HUD (hearts, wave, dash indicator) and the Windows build itself (F07).
- Menu art, fonts, logo, button sprites, music, click sounds, transitions or animations (F08).
- Pause, Esc handling, options, volume, key rebinding, credits, level or layout select, confirmation dialogs.
- Keyboard or gamepad control of menus.
- Score, run timer, statistics on the ending panel, saved progress.

## Context for the agent (optional)

Read first: `CLAUDE.md`, `Docs/Specs/F05-arena-and-three-waves.md`, and the code in `Assets/_Game/Scripts/`.

Decisions that came from the interview and are not obvious from the criteria:

- Two scenes: `MainMenu.unity` loads `Arena.unity`; Retry reloads the Arena; Menu loads the main menu. Reloading is what guarantees a clean run.
- The panels listen to F05's victory and defeat events. Game code never references UI.
- The project uses the Input System only, so the UI must take its mouse input through the Input System, not the legacy input module.
- Scene loading needs both scenes in the build scene list; F06 adds them (and only them).
- In the Editor, Quit stops Play Mode; in a build it quits the application.
- F01's letterbox must also frame the menu and the panels.

Placeholders (to be replaced in F08):

| Placeholder | Stands in for |
|---|---|
| Plain dark panel, white default-font text | Readable menu and ending UI art |
| Grey buttons, lighter on hover | Button sprites and click sounds |
| 70% black layer | Final ending presentation |

Open values: none.
