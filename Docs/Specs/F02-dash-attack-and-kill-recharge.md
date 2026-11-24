# Spec: F02 Dash attack and kill recharge

## Goal

The HLD's target experience is to "feel fast, precise and powerful while chaining kills", and its core loop ends in dash → chain: "a kill readies the next dash; a miss requires dodging until it recharges". F02 is that mechanic. A left-click throws the player 5 units along the facing in 0.15 seconds, destroying every enemy the body sweeps through. A kill makes the next dash ready the moment the current one ends, so kills can be chained; a miss costs 1 second on foot. The dash works out its whole path at the click, so it can never skip an enemy or a wall however fast it moves. This is the riskiest Must in the backlog and it is proven here on four placeholder dummies and the arena walls before any real enemy exists.

## Acceptance criteria

Everything from F01 still holds (coordinates, Rotation Z, "Velocity", tolerance of 0.05 units and 5 degrees). The dash is "ready" when the player's nose is red and "not ready" when it is grey. Four dummies (red 1-unit squares) stand at (−7.5, 0), (−3.5, 0), (2, 0) and (3.5, 0). "Facing right" means the cursor is on the right wall at the player's height.

1. **A hit.** Starting state: Play Mode just started, player at (−10.5, 0) facing right, nose red. Action: left-click once. I see: the player travels 5 units in 0.15 seconds and stops at (−5.5, 0); the dummy at (−7.5, 0) fades out over 0.3 seconds and is gone; the nose is grey during the dash and red again the moment the dash ends.
2. **A chained kill.** Starting state: player at (−5.5, 0) facing right straight after criterion 1, nose red. Action: left-click once. I see: the player stops at (−0.5, 0), the dummy at (−3.5, 0) fades out, and the nose is red again at once, with no cooldown.
3. **Two kills in one dash.** Starting state: player at (−0.5, 0) facing right, nose red. Action: left-click once. I see: the player stops at (4.5, 0), both dummies at (2, 0) and (3.5, 0) fade out, and the nose is red again at once.
4. **A miss.** Starting state: player at (4.5, 0) facing right, no dummy ahead, nose red. Action: left-click once. I see: the player stops at (9.5, 0); the nose stays grey for 1 second after the dash ends and then turns red.
5. **Clicks during the cooldown are ignored.** Starting state: the nose has just turned grey after a miss. Action: left-click three times within the 1 second. I see: the player does not dash, and the nose still turns red 1 second after the missed dash ended (the clicks do not extend it).
6. **Walking during the cooldown.** Starting state: the nose is grey after a miss. Action: hold A. I see: Velocity (−6, 0); the player walks normally.
7. **A wall stops the dash.** Starting state: player at (9.5, 0) facing right, nose red. Action: left-click once. I see: the player stops with its centre at x = 11.5, touching the right wall after 2 units, never overlapping it; the nose stays grey for 1 second (a miss).
8. **A blocked dash is a miss.** Starting state: player at (11.5, 0) touching the right wall, facing right, nose red. Action: left-click once. I see: the player does not move, and the nose is grey for 1 second.
9. **The dash is straight and ignores WASD.** Starting state: player walking up with W held, somewhere between x = −10 and x = 5 at y above 2, facing right, nose red. Action: left-click once while still holding W. I see: the player moves 5 units to the right with its y unchanged during the dash, then carries on walking up at Velocity (0, 6) without my pressing W again.
10. **The hit area is 1.25 units wide.** Starting state: a fresh Play Mode; player resting on top of the dummy at (−7.5, 0), so its centre is at (−7.5, 1.0), facing right. Action: left-click once. I see: the player stops at (−2.5, 1.0); the dummy underneath it and the dummy at (−3.5, 0) both fade out, although the white circle only skimmed their top edge; the nose is red at once.
11. **The hit area is not wider than that.** Starting state: a fresh Play Mode; player at x = −10.5 with y between 1.4 and 1.6, facing right. Action: left-click once. I see: the player passes above the dummy at (−7.5, 0), which stays; the nose is grey for 1 second.
12. **Slow dash shows the rules.** Starting state: not in Play Mode; in `Assets/_Game/Config/DashConfig.asset` set Duration to 2; enter Play Mode with the Player selected, at (−0.5, 0) facing right (walk there around the dummies, or dash there first). Action: left-click once, then move the mouse and press WASD during the dash. I see: the dash takes 2 seconds to reach (4.5, 0); the dummy at (2, 0) starts fading before the one at (3.5, 0), each as the body reaches it; Rotation Z stays 0 and WASD does nothing until the dash ends; the "Invulnerable" tick on the Player's dash component is on for those 2 seconds and off afterwards. (Set Duration back to 0.15.)
13. **Tuning lives in config assets.** Starting state: not in Play Mode. `DashConfig.asset` shows Distance = 5, Duration = 0.15, Miss Cooldown = 1, Hit Width = 1.25, and the enemy death fade config in `Assets/_Game/Config/` shows Fade Time = 0.3. Action: set Miss Cooldown to 5, enter Play Mode, dash upward from the spawn (a miss). I see: the nose stays grey for 5 seconds. (Set it back to 1.)

## Edge cases

- **Start of play:** the dash is ready (nose red).
- **Kills do not stack:** readiness is yes or no. Two kills in one dash ready one next dash, not two.
- **Kill and wall in the same dash:** it is a kill; the dash is ready when it ends, with no cooldown.
- **Dash direction:** the facing at the moment of the click. If the cursor is inside the 0.5-unit aim dead zone, that is the last facing kept by F01.
- **Speed is constant:** every dash moves at 5 ÷ 0.15 = 33.3 units/second, so a dash cut to 2 units by a wall lasts 0.06 seconds.
- **Exact end point:** an unblocked dash ends exactly 5 units from where it began, at any frame rate. It never overshoots.
- **Walking speed at the click:** it is not added to the dash and does not bend it.
- **WASD held through the dash:** ignored during the dash; walking resumes on the frame the dash ends, without a new key press.
- **Mouse moved during the dash:** ignored; the body keeps pointing along the dash and snaps to the cursor on the frame the dash ends.
- **Click during a dash:** ignored and not remembered, even if the dash gets a kill.
- **Holding the mouse button:** one dash per press. Holding it through the end of a dash or cooldown does not start another.
- **Dash into a wall at an angle:** the dash stops where the body first touches the wall. It does not slide along the wall.
- **Dash along a wall the player is already touching** (for example touching the top wall and dashing right): the wall does not block it; the dash runs its full 5 units.
- **Dash into a corner or the door marker:** treated as walls; the dash stops on first contact.
- **Blocked dash with an enemy within reach:** still a dash. If an enemy is inside the hit area it is a kill; otherwise it is a miss.
- **Enemy next to the player when the dash starts:** the hit area is the 1.25-unit circle swept from the dash's start to its end, both ends included, so an enemy within 0.125 units of the body at the start is destroyed whatever the dash direction.
- **Fading enemy:** it is dead from the moment the body reaches it. During its 0.3-second fade it has no collision, does not block walking or dashes, cannot be hit again and counts as one kill.
- **Walking into a living dummy:** it blocks the player like a wall (stop or slide around it). Walking never destroys a dummy, and the player cannot push it.
- **Dummies:** they never move, never attack and never come back. Restart Play Mode to reset them.
- **Invulnerable:** on from the frame the dash starts to the frame it ends, including a blocked dash's zero length (off again on the same frame); off during the cooldown.
- **Timers:** dash time, cooldown and fade follow game time.
- **Click on a letterbox bar:** it is still a click inside the window, so it dashes.

## Examples

- Before: player at (−10.5, 0) facing right, nose red. I do: three left-clicks, each just after the previous dash ends. After: player at (4.5, 0), all four dummies destroyed, nose red; about 0.45 seconds of dashing in total.
- Before: player at (4.5, 0) facing right, nose red, no dummies left. I do: left-click. After: player at (9.5, 0), nose grey for 1 second.
- Before: player at (9.5, 0) facing right, nose red. I do: left-click. After: player at (11.5, 0) against the right wall after 0.06 seconds, nose grey for 1 second.
- Before: player at (−10.5, 3) facing up. I do: left-click. After: player at (−10.5, 5.5) against the top wall (2.5 units, 0.075 seconds), nose grey for 1 second.
- Before: player at (0, 5.5) touching the top wall, facing right. I do: left-click. After: player at (5, 5.5).
- Before: nose grey, 0.4 seconds into a miss cooldown. I do: left-click. After: nothing; the nose turns red 0.6 seconds later.

## Out of scope

- Hearts, damage, and the check that a hit during a dash removes no heart (F03). F02 only makes the player report "Invulnerable" during a dash.
- Charger, Turret, projectiles, warnings, and any enemy that moves or attacks (F04). The four dummies are placeholders.
- Waves, enemy counts, spawn points, win and lose (F05).
- Menus and endings (F06).
- HUD dash indicator and the Windows build (F07). The nose colour is the placeholder signal until then.
- Cyan dash trail, slash arc, impact effect, sounds, screen shake, hit pause (F08, F09, F10). The dash itself is the slash; there is no separate attack.
- Remembering early clicks, more than one stored dash, dashing in the WASD direction, changing direction mid-dash, sliding along walls during a dash, respawning dummies.

## Context for the agent (optional)

Read first: `CLAUDE.md`, `Docs/Specs/F01-movement-and-aim.md`, the F01 code in `Assets/_Game/Scripts/`, `Docs/Backlog.md` (the "Riskiest Must" paragraph is the reason for the path-check method).

Decisions that came from the interview and are not obvious from the criteria:

- The dash uses the backlog's plan B from the start: decide the full straight path at the click, check walls and enemies along all of it, then move the player along it. Do not rely on collision callbacks at dash speed.
- Walls stop the dash with the body's own 1-unit width; enemies are hit with the wider 1.25-unit area.
- F04's Charger and Turret will be destroyed the same way as the dummies, and F05 will count kills, so "can be destroyed by a dash" belongs on a component that real enemies can reuse, and a kill should be announced with a C# event.
- F07's HUD will show dash readiness, so readiness changes should be announced with a C# event. The nose colour listens like any other display would.
- F03 will read the player's "Invulnerable" state.

Placeholders (to be replaced in F08):

| Placeholder | Stands in for |
|---|---|
| Four red 1-unit squares at (−7.5, 0), (−3.5, 0), (2, 0), (3.5, 0) | Charger and Turret (`enemy_charger`, `enemy_turret`) |
| Nose red when ready, grey when not | HUD dash indicator (F07) and the dash-ready glyphs |
| 0.3-second fade to transparent | The destruction effect (impact sprite, sound) |

Open values: none.
