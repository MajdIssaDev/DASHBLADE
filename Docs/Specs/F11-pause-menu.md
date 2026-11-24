# Spec: F11 Pause menu

## Goal

The HLD's target experience wants a run the player can retry and replay without friction. Until now the only ways out of a run were to finish it or to close the window. F11 adds a simple pause: Esc freezes the run and shows a panel from which the player can resume, look up the controls, leave to the main menu or quit the game. Decided on 2026-11-24, after the prototype was playable; it replaces F06's rule that Esc does nothing during a run. It is not in the backlog (F09 and F10 are taken there), so it is numbered F11.

## Acceptance criteria

Everything from F01–F07 still holds, except F06 criterion 9 for a run in progress. The pause panel is a plain placeholder like the ending panels: dark panel, white text, grey buttons, mouse only. "Paused" below means the pause panel or its Controls view is showing.

1. **Esc pauses.** Starting state: `Arena.unity` in Play Mode at 1920 × 1080, wave 1 fighting, a Charger moving. Action: press Esc once. I see: on that frame everything in the arena stops; the arena and the HUD darken under the same 70% black layer as the ending panels; a centred panel shows the title "PAUSED" and exactly four buttons, top to bottom: "Resume", "Controls", "Menu", "Quit".
2. **Paused means frozen.** Starting state: paused, a Charger frozen mid-charge and a projectile frozen in flight. Action: wait 5 seconds, press WASD, move the mouse, left-click on the dark area. I see: nothing in the arena moves or changes; the player does not walk, turn or dash; the HUD shows the values from the moment of the pause.
3. **Esc resumes.** Starting state: paused. Action: press Esc once. I see: the panel and the black layer disappear on that frame and the game continues from exactly where it stopped: the Charger finishes its charge, the projectile flies on, timers continue (a wave that was 1.2 seconds into its 2-second pause appears 0.8 seconds after resuming).
4. **Resume button.** Starting state: paused. Action: click Resume once. I see: the same as criterion 3, and the player does not dash because of that click.
5. **Controls view.** Starting state: paused, the four buttons showing. Action: click Controls. I see: the panel now shows the title "CONTROLS", the three lines "WASD or arrow keys: move", "Mouse: aim", "Left-click: dash and slash", and exactly one button, "Back"; the game stays frozen.
6. **Back.** Starting state: the Controls view. Action: click Back. I see: the panel shows "PAUSED" and the four buttons again; the game stays frozen.
7. **Menu.** Starting state: paused, wave 2, Hearts = 2. Action: click Menu once, then click Play. I see: the main menu; then a fresh run as in F06 criterion 3 (Current Wave = 1, Hearts = 3, player at (−14.5, 0)), running and not frozen.
8. **Quit.** Starting state: paused, in the Editor. Action: click Quit once. I see: Play Mode stops, with no confirmation. (In the Windows build the same button closes the game.)
9. **No pause once the run has ended.** Starting state: wave 3, one enemy left. Action: destroy it, press Esc during the next second, and press Esc again when the VICTORY panel is showing. I see: Esc does nothing either time; the VICTORY panel appears 1 second after the kill as in F06. The same holds after the last heart is lost and on the DEFEAT panel.
10. **No pause on the main menu.** Starting state: `MainMenu.unity` in Play Mode. Action: press Esc. I see: nothing changes.
11. **Pausing again.** Starting state: a run that was paused and resumed once; the Controls view was open when it was resumed. Action: press Esc. I see: the pause panel opens on "PAUSED" with the four buttons, not on the Controls view.
12. **Letterboxed.** Starting state: Game view at 1024 × 768, a run in progress. Action: press Esc. I see: the black layer and the panel sit inside the 16:9 picture, laid out as at 1920 × 1080 but smaller; the buttons respond where they are drawn.
13. **In the Windows build.** Starting state: a run in `Builds/Windows/DASHBLADE.exe`. Action: press Esc; click Controls, Back, Resume; press Esc again and click Quit. I see: each step behaves as above and Quit closes the game.

## Edge cases

- **When Esc pauses:** only in the Arena while the run is still being played (Run State = Playing), including the 2-second pauses before a wave and while enemies fade in. Never on the main menu, never once the run has a result, never on an ending panel.
- **Esc pressed on the same frame the run ends:** the ending wins; the game does not pause.
- **Esc on the Controls view:** resumes the game directly, the same as on the "PAUSED" view.
- **Holding Esc:** only the press counts; holding it does not pause and resume repeatedly.
- **A dash in progress when Esc is pressed:** it freezes part-way and finishes after resuming, with the same path and the same result. A cooldown or damage protection in progress continues from where it was.
- **Keys held while resuming:** if a movement key is down when the game resumes, the player walks at once. The facing snaps to the cursor on the first frame after resuming.
- **The click that presses a button never dashes:** not while paused and not on the first frame after Resume.
- **Double-click on Resume:** after the Resume button resumes the game, left-clicks are ignored by the dash for the same Start Lockout as at the start of a run (0.5 seconds, `DashConfig.asset`), so a second press does not dash. Walking and aiming work at once. Resuming with Esc has no lockout: a left-click on the next frame dashes.
- **Clicking twice quickly on Menu or Quit, or two of them in a row:** only the first click counts, as in F06.
- **Clicking the panel or the dark area outside the buttons:** nothing happens.
- **Keyboard:** apart from Esc, no key activates, selects or moves between buttons (not Enter, Space, Tab or the arrow keys).
- **Leaving always unfreezes:** after Menu, the main menu and the next run are at normal speed. A run started after a pause must never begin frozen or paused.
- **Window loses focus while paused:** it stays paused. Losing focus during a run does not pause the game (F01's focus rule still applies).
- **Pause and ending panels never show together.**
- **The HUD is not needed for pausing, and the pause panel is not needed for the game:** with the pause panel's object switched off the game still plays and ends normally.
- **Test scenes:** `Test_DashAndHearts` and `Test_Enemies` have no pause menu; Esc does nothing there.
- **Black layer:** the same Dim Strength (0.7, `EndingConfig.asset`) as the ending panels. No new tuning value is added by this feature.

## Examples

- Wave 2, a Turret's ring half closed → Esc → everything frozen, "PAUSED" → Controls → the three lines → Back → Resume → the ring goes on closing from where it was.
- Paused → Esc → running again; a left-click on the next frame dashes.
- Paused → Menu → main menu → Play → wave 1 with 3 hearts, running.
- Last enemy of wave 3 destroyed → Esc during the next second: nothing → VICTORY panel after 1 second → Esc: nothing.
- Paused for 10 minutes → Resume: the miss cooldown that had 0.4 seconds left still has 0.4 seconds left.

## Out of scope

- Options, volume, key rebinding, a restart button on the pause panel, confirmation dialogs.
- Pausing automatically when the window loses focus.
- A pause key other than Esc; gamepad or keyboard control of the buttons.
- Pausing on the main menu or on the ending panels.
- Pause art, sounds, music changes, transitions or animations (F08).
- Updating `Docs/HLD.md` and `Docs/Backlog.md`, which still say Esc does nothing and do not list this feature.

## Context for the agent (optional)

- Wording and layout follow the F06 panels (legacy uGUI Text, the same button look and hover colour, Screen Space - Camera canvas on the letterboxed camera).
- Pause rules belong in plain C# in `Dashblade.Flow` with Edit Mode tests; the panel is a listener in `Dashblade.UI`. Game code never references the panel.
- `RunEnding` freezes the game by switching the player's components off. That cancels a dash in progress (`PlayerDash.OnDisable`), which is right for an ending but wrong for a pause: a paused dash must finish after resuming.
