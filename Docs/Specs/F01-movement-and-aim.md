# Spec: F01 Movement and aim

## Goal

The HLD's target experience is to "feel fast, precise and powerful while chaining kills", and its core loop is Read → position → dash → chain. F01 is the "position" step and the aim that the dash (F02) will later follow. Movement starts and stops instantly and the body snaps to the mouse with no lag, so that where the player stands and where the nose points are always exactly what I asked for. If a later dash misses, it should be my mistake, not the controls'.

## Acceptance criteria

All positions are world units. The test area is centred on (0, 0): its inside is 24 × 12 (x from −12 to 12, y from −6 to 6). Rotation Z = 0 means the nose points right (+X); 90 means up. "Velocity" is the Linear Velocity shown under Info on the player's Rigidbody2D in the Inspector. A reading passes when it is within 0.05 of the stated number for positions and velocities, and within 5 degrees for Rotation Z.

1. **Start state.** Starting state: scene `Assets/_Game/Scenes/Arena.unity` open, cursor resting over the spawn point in the Game view. Action: enter Play Mode (Ctrl+P). I see: a white circle 1 unit across with a red 0.25 × 0.25 square nose on its front edge, centred at (−10.5, 0), Rotation Z = 0, Velocity (0, 0).
2. **Instant start and stop.** Starting state: player still at (−10.5, 0). Action: hold D, then release. I see: Velocity reads (6, 0) from the first frame D is held, and (0, 0) on the frame it is released; the X position stops changing at once, with no slide.
3. **Four directions, two key sets.** Starting state: player still, away from walls. Action: hold one key at a time. I see: W or Up Arrow → Velocity (0, 6); S or Down Arrow → (0, −6); A or Left Arrow → (−6, 0); D or Right Arrow → (6, 0).
4. **Diagonal is not faster.** Starting state: player still at (0, 0). Action: hold W+D. I see: Velocity about (4.24, 4.24), which is 6 units/second along the diagonal (6 ÷ √2 per axis).
5. **Aim snaps to the mouse.** Starting state: player still at (−10.5, 0), no keys held. Action: move the cursor to directly above the player, then directly left, then directly below, each more than 0.5 units away. I see: the nose points at the cursor on the same frame, with Rotation Z = 90, then 180, then −90 (the Inspector may show 270).
6. **Moving and aiming are independent.** Starting state: player at (0, 0), cursor 5 units to its right, so Rotation Z = 0. Action: hold A. I see: Velocity (−6, 0) while the nose keeps pointing right at the cursor; the player backs away from the cursor without turning.
7. **Aim dead zone of 0.5 units.** Starting state: player still at (0, 0), cursor at (5, 0), Rotation Z = 0. Action: move the cursor to (0, 0.3), then on to (0, 0.6). I see: at (0, 0.3) Rotation Z stays 0; at (0, 0.6) it becomes 90.
8. **Walls stop the player.** Starting state: player at (−10.5, 0). Action: hold D until the player reaches the right wall and keep holding. I see: the player stops with its centre at x = 11.5, touching the wall, Velocity (0, 0), no jitter, and it never overlaps or passes through the wall.
9. **Sliding along a wall.** Starting state: player touching the top wall (centre y = 5.5), well away from the corners. Action: hold W+D. I see: y stays 5.5 and Velocity reads about (4.24, 0); the player glides right along the wall.
10. **Solid door marker.** Starting state: player at (−10.5, 0). Action: hold A. I see: a brown rectangle 1 unit wide and 2 units tall covering the left wall's full thickness, centred at (−12.5, 0); the player stops with its centre at x = −11.5 against it, exactly as against the wall.
11. **Fixed camera shows everything.** Starting state: Game view set to 1920 × 1080 (16:9). Action: walk from the left wall to the right wall. I see: the whole 26 × 14 footprint (the 24 × 12 inside plus four grey walls, each 1 unit thick) on screen the entire time with a thin dark margin around it, the Main Camera's orthographic Size reads 7.5 (26.67 × 15 units visible), and its Transform position does not change.
12. **Tuning lives in a config asset.** Starting state: `Assets/_Game/Config/PlayerConfig.asset` shows Move Speed = 6 and Aim Dead Zone = 0.5; not in Play Mode. Action: set Move Speed to 3, enter Play Mode, hold D. I see: Velocity (3, 0). (Set it back to 6 afterwards.)
13. **Letterbox to 16:9.** Starting state: Game view set to a 4:3 size such as 1024 × 768, Play Mode running. Action: look at the Game view, then point the cursor directly above the player. I see: black bars above and below a 16:9 picture that still shows the whole 26 × 14 footprint (the same 26.67 × 15 units as at 1920 × 1080), and Rotation Z = 90, so aim is still correct.

## Edge cases

- **No input:** the player does not move, Velocity (0, 0); it still turns to face the mouse.
- **Diagonal input:** direction is normalised; speed along the diagonal is 6, never about 8.49.
- **Opposite keys (A+D or W+S):** they cancel on that axis. A+D+W moves straight up at 6. All four held: the player stands still.
- **WASD and arrows together:** both sets feed the same four directions. W + Up Arrow is still 6, not 12. W + Down Arrow cancels. D + Up Arrow is a normal diagonal at 6.
- **Mouse on top of the player:** when the cursor is closer than 0.5 units to the player's centre, the facing keeps its last value. At 0.5 units or more it tracks again.
- **Player walks over a still cursor:** the facing holds while the centre is within 0.5 units of the cursor, then snaps to the new direction on the way out (it may flip 180° in one frame).
- **Mouse still, player moving:** the facing is recalculated every frame, not only when the mouse moves, so the nose keeps pointing at the cursor as the player walks past it.
- **No usable aim yet at start:** the player faces right (Rotation Z = 0) until the cursor is 0.5 units or more from its centre.
- **Mouse outside the game window:** aim keeps tracking the cursor's reported position past the window edge. If no position is reported, the last facing is kept.
- **Window loses focus while a key is held (Alt+Tab):** the player stops at once, and stays still after I return until I press a movement key again.
- **Pushing straight into a wall:** the player stays put with Velocity (0, 0); no jitter, no pushing through.
- **Pushing into a corner with a diagonal:** the player stops in the corner (for the top-right corner, centre at (11.5, 5.5)) with Velocity (0, 0).
- **Rubbing along a wall or door:** contact never spins the body; the facing comes only from the mouse.
- **Frame rate:** speed is 6 units/second at any frame rate; it does not rise or fall with FPS.
- **Left mouse button:** does nothing in F01.
- **Game view or window that is not 16:9:** the picture is letterboxed to 16:9 with black bars (top and bottom on narrower shapes such as 4:3, left and right on wider shapes such as 21:9). The visible world area is always 26.67 × 15 units.
- **Cursor over a black bar:** treated like the mouse outside the window; aim keeps tracking the cursor's position.
- **Window resized during play:** the letterbox updates to the new shape.
- **Nose marker:** it is visual only and has no collider; collision is the 1-unit circle alone.

## Examples

- Before: player at (−10.5, 0), no keys. I do: hold D. After: Velocity (6, 0); it covers 22 units and stops at x = 11.5 against the right wall after about 3.7 seconds.
- Before: player at (0, 0). I do: hold W+D for 1 second. After: player is near (4.24, 4.24).
- Before: player at (0, 0), cursor at (0, 5). After: Rotation Z = 90. I do: move the cursor to (−5, 0). After: Rotation Z = 180.
- Before: player at (0, 0), Rotation Z = 0, cursor at (5, 0). I do: move the cursor to (0.3, 0.3) (about 0.42 units away). After: Rotation Z is still 0.
- Before: player at (3, 5.5) against the top wall. I do: hold W+D for 1 second. After: player is near (7.24, 5.5).
- Before: player walking right, D held. I do: also press A. After: Velocity (0, 0) until I release one of the two keys.

## Out of scope

- Dash, slash, cooldown, kill recharge and dash invulnerability (F02). The left mouse button stays unused.
- Hearts, damage and damage protection (F03).
- Charger, Turret, projectiles and warnings (F04).
- The final arena layout and size, waves, enemy spawn points and any working door or room travel (F05). The 24 × 12 area and its door marker are placeholders for testing F01.
- Main menu, victory and defeat screens, pause (F06).
- HUD and the Windows build (F07).
- Final art, animation, dash trail, footstep or any other sound, particles, screen shake (F08, F09, F10).
- Acceleration, deceleration, smoothed turning, sprint, dodge roll.
- Gamepad, touch, key rebinding, cursor lock or a custom cursor.
- Camera follow, zoom or shake.

## Context for the agent (optional)

Read first: `CLAUDE.md` (rules and the verified Unity 6.3 API notes), `Docs/HLD.md`, `Docs/Backlog.md`.

Decisions that came from the interview and are not obvious from the criteria:

- W/A/S/D are relative to the screen, never to the facing.
- Wall sliding at about 4.24 on a diagonal is the natural Unity physics result; do not add code that redirects the player along the wall at full speed.
- Arrow keys are an addition to the HLD's control list ("WASD: move"), requested in the interview.
- F02 will read the player's current aim direction, so keep it available from the player rather than buried in the rotation.

Placeholder assets. F01 is built from Unity's built-in 2D sprites. The sprite sheets in `Assets/_Game/Art/SpriteSheets` (see `Docs/SpriteSheets.md`) are for the F08 pass and are not used or changed in F01.

| Placeholder | Stands in for (F08 sprite) |
|---|---|
| White circle, 1 unit across, with a matching circle collider | The masked hero's body (`hero_masked_swordsman`) |
| Red 0.25 × 0.25 square on the circle's front edge, no collider | The hero's facing (red nods to the scarf) |
| Four grey rectangles, 1 unit thick, around the 24 × 12 inside | Dark stone arena walls (1 × 1 wall tile) |
| Brown rectangle, 1 unit wide × 2 tall, centred at (−12.5, 0), solid | The entrance door next to the spawn point (1 × 2 door sprite) |
| Dark camera background | Dark slate floor (2 × 2 floor tile) |

Exact shades were not specified; any shades that keep the five placeholders clearly distinct are fine.

Open values: none. The five values that were open after the interview were answered on 2026-11-24: camera orthographic size 7.5, letterbox to 16:9, nose 0.25 × 0.25, door 1 unit wide, tolerance 0.05 units and 5 degrees.
