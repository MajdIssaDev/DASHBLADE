# Spec: F05 Arena and three fixed waves

## Goal

The HLD's win condition is "clear all three waves", in the order chargers, then turrets, then both, and its target experience wants retries to be "inviting". F05 turns the single test enemies into a run: a bigger arena with pillars for cover, three fixed waves that each teach one thing (lanes, shots, both together), a short pause with hearts restored between them, and a clear end state for victory and defeat. Each run picks one of three hand-made pillar layouts, so a retry looks different while every layout is known to be playable.

This feature changes two things written in the HLD and backlog, by the student's decision in the interview: the arena gets internal obstacles (the backlog's dash plan B assumed none), and their layout is picked at random from fixed layouts (the HLD lists procedural levels as out of scope; hand-made layouts are the smallest step away from that).

## Acceptance criteria

Everything from F01–F04 still holds, except that `Arena.unity` is now bigger: the inside is 32 × 18, centred on (0, 0) (x from −16 to 16, y from −9 to 9), with walls 1 unit thick (footprint 34 × 20). The door marker is at (−16.5, 0) and the player starts at (−14.5, 0). "Run" is the run component in the Arena scene, which shows Current Wave, Enemies Left, Run State and Layout in the Inspector. Pillars are dark-grey 2 × 2 solid squares. Positions for waves and layouts are in the tables under Examples.

1. **The bigger arena.** Starting state: `Arena.unity` open, Game view at 1920 × 1080. Action: enter Play Mode, then hold W. I see: the whole 34 × 20 footprint on screen; the Main Camera's Size reads 10; the player starts at (−14.5, 0) and stops against the top wall with its centre at y = 8.5.
2. **A layout is picked at random.** Starting state: `ArenaConfig` Force Layout = None. Action: enter and leave Play Mode 6 times. I see: each time Run shows Layout = A, B or C and exactly 6 pillars stand at that layout's positions; over the 6 starts I see at least two different layouts.
3. **A layout can be forced for testing.** Starting state: not in Play Mode; set Force Layout to B. Action: enter Play Mode. I see: Layout = B and pillars centred at (−10, −5), (−5, 2), (0, 5), (2, −2), (5, 2) and (10, −2). (Repeat for A and C; set it back to None.)
4. **Pillars are walls.** Starting state: Force Layout = C; Play Mode just started; player at (−14.5, 0) facing right. Action: left-click twice, the second click after the first dash's cooldown. I see: the first dash ends at (−9.5, 0); the second stops at x = −7.5, touching the pillar centred at (−6, 0) after 2 units, and counts as a miss. Walking into the pillar stops the player or slides it around the corner.
5. **Wave 1 arrives after 2 seconds.** Starting state: Play Mode just started. Action: none. I see: an empty arena for 2 seconds with Current Wave = 1; then three red triangles fade in over 0.5 seconds at (10, 6), (10, −6) and (13, 0); Enemies Left = 3; the first orange strips appear 1 second after the fade-in ends.
6. **Enemies that are fading in cannot be hurt or hurt.** Starting state: not in Play Mode; set `WaveConfig` Spawn Fade Time to 6; enter Play Mode and walk to the Charger appearing at (13, 0). Action: dash through it while it is still fading in. I see: the dash passes through it and it is not destroyed (a miss); walking through it is not blocked; no strip appears until it has fully appeared. (Set Spawn Fade Time back to 0.5.)
7. **Clearing a wave starts the next.** Starting state: wave 1, one Charger left. Action: destroy it with a dash. I see: Enemies Left = 0; 2 seconds later three red hexagons fade in over 0.5 seconds at (12, 5), (12, −5) and (0, −7); Current Wave = 2; Enemies Left = 3.
8. **The player is not moved between waves.** Starting state: the last enemy of wave 1 has just been destroyed; player standing anywhere. Action: stand still for 3 seconds. I see: the player's position does not change when wave 2 appears.
9. **Hearts are restored and shots are cleared.** Starting state: wave 2, Hearts = 1 or 2, at least one projectile in the air, one Turret left. Action: destroy the last Turret. I see: every projectile vanishes at once, and Hearts = 3.
10. **Wave 3 mixes both.** Starting state: the last Turret of wave 2 has just been destroyed. Action: wait 2 seconds. I see: two triangles fade in at (8, 6) and (8, −6) and two hexagons at (13, 3) and (13, −3); Current Wave = 3; Enemies Left = 4.
11. **Victory.** Starting state: wave 3, one enemy left. Action: destroy it, then wait 10 seconds, then walk and dash. I see: Run State = Victory; every projectile vanishes at once; no new enemy ever appears; the player still walks at 6 units/second and can dash.
12. **Defeat.** Starting state: any wave, Hearts = 1. Action: take a hit, then wait 10 seconds. I see: Run State = Defeat; the player fades out (F03); no new wave ever appears.
13. **Pillars block lanes, charges and shots.** Starting state: Force Layout = A, wave 1 or 3; player standing so that a pillar is between it and a Charger whose strip has just locked. Action: stand still. I see: the strip ends at the pillar; the Charger stops with its centre 0.5 units from the pillar's face and rests there; Hearts is unchanged. In wave 2 or 3, a shot that reaches a pillar disappears at once.
14. **Tuning lives in config assets.** Starting state: not in Play Mode. `Assets/_Game/Config/WaveConfig.asset` shows Wave Pause = 2, Spawn Fade Time = 0.5 and the three waves with the enemies and positions in the table below; `Assets/_Game/Config/ArenaConfig.asset` shows Force Layout = None and the three layouts in the table below. Action: set Wave Pause to 6, enter Play Mode. I see: the arena stays empty for 6 seconds before wave 1 fades in. (Set it back to 2.)
15. **The F04 checks still have a scene.** Starting state: not in Play Mode. Action: open `Assets/_Game/Scenes/Test_Enemies.unity` and enter Play Mode. I see: the old 24 × 12 area with the player at (−10.5, 0), one Charger at (6, 3) and one Turret at (6, −3), no waves and no pillars, behaving as in the F04 spec. `Test_DashAndHearts.unity` still works as F04 left it.

## Edge cases

- **Wave order and contents never change:** wave 1 is 3 Chargers, wave 2 is 3 Turrets, wave 3 is 2 Chargers and 2 Turrets, always at the same positions whatever the layout.
- **Every wave is preceded by the 2-second pause,** including wave 1.
- **A wave is cleared** the moment its last enemy starts its death fade. Projectiles do not count as enemies.
- **Two enemies destroyed by one dash,** the last two of a wave: the wave is cleared once.
- **At the moment a wave is cleared:** leftover projectiles vanish at once (no fade), then Hearts goes to 3. A running protection blink carries on for its remaining time.
- **After the final wave:** projectiles are cleared, hearts are not restored, and nothing can hurt the player any more.
- **Defeat ends the run:** no further wave appears. Enemies still alive behave as F04 says for a defeated player.
- **Fading-in enemies:** harmless, not solid, not destroyable, show no warning. Their 1-second first-attack delay starts when the fade-in ends.
- **Player standing on an enemy position when a wave appears:** the enemy appears anyway; F04's overlap rule lets the player walk out, and the enemy is solid once they separate.
- **Pillars behave exactly like walls** for everything: walking (stop or slide), the dash (stops at first contact, no sliding), Charger lanes and charges, and projectiles. A dash that kills an enemy and then stops at a pillar is still a kill.
- **Turrets do not check for cover:** a Turret fires at the player even when a pillar is in the way; the shot hits the pillar.
- **An enemy behind a pillar** cannot be reached by a dash straight through the pillar.
- **Layout choice:** made once when the run starts and kept for all three waves. Each of the three layouts is equally likely; the same layout can come up twice in a row.
- **Force Layout** is a testing aid: None means random; A, B or C always picks that layout.
- **Layout safety rules** (true for A, B and C, and for any layout added later): 6 pillars; every pillar centre is at least 4 units from the player start; at least 3 units in x or in y from every enemy position of every wave; between x = −13 and 13 and y = −6 and 6, so at least 2 units of floor separate a pillar from a wall; any two pillar centres differ by at least 4 units in x or in y, so at least 2 units of floor separate two pillars.
- **Not 16:9:** the picture is still letterboxed (F01); the visible area is now 35.56 × 20 units.
- **Run length:** the 3–5 minute target is not promised by this spec; it is measured and tuned through the config assets afterwards.

## Examples

Waves (in `WaveConfig.asset`):

| Wave | Enemies | Positions |
|---|---|---|
| 1 | 3 Chargers | (10, 6), (10, −6), (13, 0) |
| 2 | 3 Turrets | (12, 5), (12, −5), (0, −7) |
| 3 | 2 Chargers, 2 Turrets | Chargers (8, 6), (8, −6); Turrets (13, 3), (13, −3) |

Pillar layouts, as pillar centres (in `ArenaConfig.asset`):

| Layout | Pillar centres |
|---|---|
| A | (−8, 4), (−8, −4), (−1, 0), (4, 4), (4, −4), (9, 0) |
| B | (−10, −5), (−5, 2), (0, 5), (2, −2), (5, 2), (10, −2) |
| C | (−6, 0), (−3, 5), (−3, −5), (3, 5), (3, −5), (6, 0) |

These wave positions and layouts were proposed by Claude under the safety rules above, at the student's request, and are for the student to review and change.

A run with no mistakes: empty arena 0–2 s; wave 1 fades in 2–2.5 s; first strips at 3.5 s; third Charger destroyed at, say, 20 s; wave 2 fades in at 22 s; first rings at 23.5 s; last Turret destroyed at 40 s (Hearts back to 3); wave 3 fades in at 42 s; last enemy destroyed at 70 s; Run State = Victory.

- Before: wave 2, Hearts = 1, two shots in the air. I do: destroy the last Turret. After: no shots, Hearts = 3, wave 3 fades in 2 seconds later.
- Before: wave 3, Hearts = 1. I do: stand in a locked lane. After: Hearts = 0, Run State = Defeat, nothing else appears.

## Out of scope

- Main menu, victory and defeat screens, retry, return to menu, quit (F06). F05 only sets Run State and announces victory and defeat.
- HUD wave counter, hearts and dash indicator; the Windows build (F07). Until then the wave is read on Run in the Inspector.
- Stone wall, floor, door, lantern and prop sprites; music and sound (F08).
- Obstacles placed at fully random positions, more than three layouts, layouts that change between waves, obstacles that move or can be destroyed, obstacle shapes other than the 2 × 2 pillar.
- More waves, endless mode, enemy counts that scale, random enemy positions, new enemy types, bosses, pickups, score, timers, saved progress.
- Moving the player between waves; a countdown or "Wave 2" banner in the world.

## Context for the agent (optional)

Read first: `CLAUDE.md`, the F02–F04 specs, and the F01–F04 code in `Assets/_Game/Scripts/`.

Decisions that came from the interview and are not obvious from the criteria:

- Wave state (which wave, how many left, pause timer, victory and defeat) is a plain C# class with Edit Mode tests, as `CLAUDE.md` requires for wave state. The layout safety rules are worth an Edit Mode test that checks A, B and C against every enemy position.
- Enemies are created at run time for each wave, so the Charger, the Turret and the pillar need to exist as reusable objects that can be placed many times.
- The wave runner learns about deaths from the kill event that F02's "destroyed by a dash" component raises, and about defeat from F03's event. It calls F03's restore.
- F06 will start, restart and end runs, and F07 will show the wave number, so wave changes, victory and defeat are announced with C# events. Game code never references UI.
- `Test_Enemies.unity` is a copy of `Arena.unity` as F04 left it, made before the arena is enlarged, following the earlier decision to keep old test set-ups in test scenes.
- The camera size (10) is scene set-up; F01's letterbox still applies.

Placeholders (to be replaced in F08):

| Placeholder | Stands in for |
|---|---|
| Dark-grey 2 × 2 squares | Stone pillar and arena props (`dashblade-arena-props.png`) |
| Grey walls, brown door, dark background | Stone wall, door and slate floor tiles (`dashblade-arena.png`) |
| Current Wave, Enemies Left, Run State in the Inspector | HUD wave display (F07) and ending screens (F06) |
| 0.5-second fade-in | Enemy arrival effect |

Open values: none. For review by the student: the ten enemy positions and the three pillar layouts above.
