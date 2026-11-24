# Spec: F04 Charger and Turret

## Goal

The HLD's target experience says "readable attacks should make mistakes understandable and retries inviting", and its core loop starts with Read. F04 gives the player something to read: two ancient machines whose every attack is announced in orange, with shape and motion, before it can hurt. The Charger draws a lane that fills up, then runs it; the Turret draws a ring that closes, then fires a visible shot. Each dies to one dash (F02), each hit costs one heart (F03), and each has a clear resting moment that invites the dash, which is what makes chaining kills feel earned.

## Acceptance criteria

Everything from F01–F03 still holds (coordinates, tolerance of 0.05 units and 5 degrees, nose red = dash ready, "Hearts" = Current Hearts in the Inspector). `Arena.unity` holds the walls, door, player, one Charger (red triangle, 1 unit wide) at (6, 3) and one Turret (red hexagon, 1 unit wide) at (6, −3). To test one enemy alone, untick the other in the Hierarchy before entering Play Mode. Times are measured from the moment Play Mode starts unless stated.

**Charger**

1. **First warning after 1 second.** Starting state: Turret off; Play Mode just started; player standing at (−10.5, 0). Action: none. I see: nothing orange for 1 second; then a faint orange strip 1 unit wide appears, running from the Charger through the player's position to the left wall, and the triangle's tip points along it.
2. **The lane follows, then locks.** Starting state: the strip has just appeared. Action: hold W to walk up for the whole warning. I see: for the first 0.6 seconds the strip and the triangle's tip turn to keep pointing at the player; for the last 0.4 seconds the strip stays where it is while I walk on; a solid orange bar grows along the strip from the Charger and reaches the wall exactly 1 second after the strip appeared.
3. **The charge.** Starting state: the bar has reached the wall. Action: none. I see: the strip vanishes and the Charger runs along the locked lane at 15 units/second (the roughly 17.8 units from (6, 3) toward the spawn take about 1.2 seconds); it stops with its centre 0.5 units from the wall; it rests there for 1.5 seconds; then a new strip appears from its new position.
4. **Standing in the lane costs a heart.** Starting state: Turret off; Play Mode just started; player at (−10.5, 0), 3 hearts. Action: none; stand still through the first charge. I see: the Charger passes through the player without moving it (position stays (−10.5, 0)); Hearts = 2 as it touches; the circle blinks for 0.75 seconds.
5. **Stepping out of the lane dodges it.** Starting state: the strip has just locked (it has stopped turning). Action: walk sideways until the whole circle is outside the strip. I see: the Charger runs past; Hearts is unchanged.
6. **A resting Charger is solid and harmless.** Starting state: the Charger is resting against a wall. Action: walk into it and keep holding the key. I see: the player stops against it with Velocity (0, 0); the Charger does not move; Hearts is unchanged.
7. **Dash kills a resting Charger.** Starting state: the Charger is resting; player within 4 units of it, facing it, nose red. Action: left-click once. I see: the dash passes through it; it fades out over 0.3 seconds; the nose is red at once; no strip ever appears again.
8. **Dash kills a charging Charger.** Starting state: 3 hearts; player standing in a locked lane, facing the Charger, nose red. Action: left-click as the Charger runs at me. I see: the Charger fades out; Hearts is still 3; the nose is red at once.
9. **Walking out of an overlap.** Starting state: 3 hearts; player touching the wall inside a locked lane, so the charge will end where the player stands. Action: stand still until the Charger stops, then hold a key to walk away from the wall. I see: Hearts = 2; the stopped Charger overlaps the player without shoving it; the player walks out through it at 6 units/second; once the two no longer overlap, walking back into the Charger is blocked.

**Turret**

10. **Ring, then shot.** Starting state: Charger off; Play Mode just started; player standing at (−10.5, 0). Action: none. I see: nothing orange for 1 second; then an orange ring 3 units wide appears around the Turret and shrinks to the Turret's own 1-unit width over 0.8 seconds; as it closes, one orange circle 0.4 units wide leaves the Turret, flying straight at where the player is at 7 units/second; the next ring appears 1.5 seconds after the shot, so there is one shot every 2.3 seconds.
11. **Standing still costs a heart.** Starting state: as criterion 10, 3 hearts. Action: none; stand still. I see: the first shot covers the roughly 16.8 units in about 2.3 seconds, disappears as it touches the circle, and Hearts = 2 with 0.75 seconds of blinking.
12. **Shots fly straight.** Starting state: a shot has just left the Turret. Action: hold W to walk out of its path. I see: the shot does not turn; it passes where the player was, disappears when it touches the wall, and Hearts is unchanged.
13. **A shot is used up on a protected player.** Starting state: not in Play Mode; set `HealthConfig` Protection Time to 5; Charger off; enter Play Mode and stand at (−10.5, 0) with 3 hearts. Action: none; stand still for two shots. I see: the first shot gives Hearts = 2; the second arrives while the circle is still blinking, disappears on touching it, and Hearts is still 2. (Set Protection Time back to 0.75.)
14. **Dash destroys a shot, but it is not a kill.** Starting state: 3 hearts; a shot is flying at the player; player facing it, nose red. Action: left-click to dash through the shot (and through nothing else). I see: the shot fades out; Hearts is still 3; the nose stays grey for 1 second, as for a miss.
15. **Dash kills the Turret; shots in the air fly on.** Starting state: a shot has just been fired; player within 4 units of the Turret, facing it, nose red. Action: left-click once. I see: the Turret fades out over 0.3 seconds and the nose is red at once; the shot already flying carries on until it hits a wall or the player; no ring ever appears again.
16. **The Turret is solid and harmless.** Starting state: player next to the Turret, 3 hearts. Action: walk into it and keep holding the key. I see: the player stops against it with Velocity (0, 0); the Turret never moves; Hearts is unchanged.

**Both**

17. **Tuning lives in config assets.** Starting state: not in Play Mode. `Assets/_Game/Config/ChargerConfig.asset` shows First Attack Delay = 1, Warning Time = 1, Lock Time = 0.4, Charge Speed = 15, Rest Time = 1.5. `Assets/_Game/Config/TurretConfig.asset` shows First Attack Delay = 1, Warning Time = 0.8, Rest Time = 1.5, Projectile Speed = 7, Projectile Width = 0.4, Ring Start Width = 3. Action: set Charge Speed to 3 and Projectile Speed to 2, enter Play Mode and stand at the spawn. I see: the first charge takes about 6 seconds to reach the wall, and the first shot crawls at 2 units/second. (Set them back to 15 and 7.)
18. **The old test objects live in a test scene.** Starting state: not in Play Mode. Action: open `Assets/_Game/Scenes/Test_DashAndHearts.unity` and enter Play Mode. I see: the walls, door, player, the four red dummies at (−7.5, 0), (−3.5, 0), (2, 0), (3.5, 0) and the orange hazard zone at (−8, −3), with no Charger or Turret; three dashes to the right from the spawn destroy all four dummies (F02) and standing in the zone takes Hearts from 3 to 0 in 1.5 seconds (F03). `Arena.unity` has no dummies and no hazard zone.

## Edge cases

- **Every attack is announced:** nothing damages the player without an orange lane or an orange projectile preceded by a ring. A resting, warning or fading enemy never hurts.
- **Player in the lane:** the charging body is 1 unit wide and so is the lane, so the player is hit when any part of its circle overlaps the charging Charger.
- **One charge, one hit:** F03's 0.75-second protection means a single pass costs at most 1 heart.
- **Charge against a dashing player:** no damage; the Charger is destroyed if the dash's 1.25-unit hit area touches it.
- **Enemy moving into the dash:** an enemy is destroyed if the dash's hit area touches it at any moment of the dash, even if it was not on the path when the click happened.
- **Charger killed during its warning:** the strip and bar vanish at once.
- **Charger killed during its charge:** it stops, is harmless from that moment, and fades.
- **Player on top of the Charger when a warning starts:** the lane keeps the Charger's last direction until the player is no longer on top of it or the lane locks.
- **Charge that cannot move** (the Charger is already against the wall it points at): the charge ends at once and the 1.5-second rest begins.
- **Enemies are solid when not charging:** resting and warning Chargers and Turrets block walking like a wall; the player cannot push them. The dash passes through them and destroys them.
- **A charging Charger never pushes:** it passes through the player.
- **Overlap rule:** if an enemy and the player overlap for any reason, that enemy does not block the player until the two have fully separated; then it is solid again. Boxed into a corner without overlapping, the way out is the dash.
- **Enemy against enemy:** enemies and projectiles pass through each other with no effect. A lane always runs to a wall, never stopping at another enemy.
- **Door marker:** it is a wall for Chargers and projectiles too.
- **Turret aim:** at the player's centre on the frame the ring closes; the shot never turns afterwards. The Turret itself never moves or rotates.
- **Projectile and the player's body:** it disappears on contact, costing a heart only if the player is neither protected nor dashing.
- **Projectile and a wall:** it disappears at once, with no fade.
- **Projectile and the dash:** a projectile inside the dash's hit area fades out over 0.3 seconds and is harmless from that moment. It is not a kill: a dash that destroys only projectiles is a miss; a dash that destroys a projectile and an enemy is a kill.
- **Turret killed during its ring:** the ring vanishes and no shot is fired.
- **Many shots:** there is no limit on projectiles in the air; they pass through each other.
- **Player defeated (0 hearts):** enemies start no new warnings and any lane or ring vanishes. A charge already running finishes; projectiles already flying carry on and hurt nobody.
- **Warnings are drawn** above the floor and under the enemies and the player.
- **Timers:** delays, warnings, rests, charge and projectile movement follow game time.

## Examples

- Charger at (6, 3), player still at (−10.5, 0): strip at 1.0 s, locked at 1.6 s, charge at 2.0 s, hit at about 3.1 s (Hearts 3 → 2), Charger stopped against the left wall near (−11.5, −0.2) at about 3.2 s, next strip at about 4.7 s.
- Lane locked through the player, player walks 1.5 units sideways: the charge misses, Hearts unchanged.
- Turret at (6, −3), player still at (−10.5, 0): ring at 1.0 s, shot at 1.8 s, hit at about 4.1 s; second ring at 3.3 s, second shot at 4.1 s.
- Shot flying, dash through it only: shot fades, nose grey for 1 second.
- Shot flying, dash through it and on through the Turret: both fade, nose red at once.
- Resting Charger, player walks into it: blocked, no damage.

## Out of scope

- Waves, enemy counts, spawn points and timing between waves, heart restore, win and lose (F05). F04 places exactly one Charger and one Turret for testing.
- Menus and endings (F06), HUD and build (F07).
- Real sprites, sounds, impact effects, screen shake (F08, F09, F10).
- A Charger that walks, chases or turns during its charge; fixed-length charges; a Turret that moves, rotates a barrel, fires bursts or homing shots.
- Enemies with more than one hit, enemies hurting each other, new enemy types, difficulty scaling.
- Contact damage from a resting enemy.

## Context for the agent (optional)

Read first: `CLAUDE.md`, the F02 and F03 specs, and the F01–F03 code in `Assets/_Game/Scripts/`.

Decisions that came from the interview and are not obvious from the criteria:

- Both enemies die through the same "destroyed by a dash" component the F02 dummies use. Projectiles are destroyed by the dash too but must not count as a kill.
- Both enemies hurt the player through F03's damage path, and find the player by component.
- The F02 dash decided its targets along the path; with moving Chargers and projectiles it must also catch anything its hit area touches during the dash.
- Each enemy's attack cycle (delay → warning → attack → rest) is a plain C# class with Edit Mode tests; the MonoBehaviours are thin.
- F05 will spawn several of each and count their deaths, so each enemy must work as a self-contained object that can be placed more than once.
- Body collision for both enemies is a 1-unit circle, so a Charger stops with its centre 0.5 units from a wall and its lane is exactly 1 unit wide.
- `Test_DashAndHearts.unity` is a copy of the arena as it stands after F03. Sharing the player between the two scenes (for example as a prefab) to stop them drifting apart is the builder's call; report what was done.

Placeholders (to be replaced in F08):

| Placeholder | Stands in for |
|---|---|
| Red triangle, 1 unit wide, tip along its lane | `enemy_charger` |
| Red hexagon, 1 unit wide | `enemy_turret` |
| Orange circle, 0.4 units wide | Turret projectile sprite |
| Faint orange strip with a growing solid bar | Charger lane warning sprite |
| Orange ring shrinking from 3 units to 1 | Turret ring warning sprite |

Open values: none.
