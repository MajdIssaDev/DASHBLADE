# Spec: F03 Hearts and damage protection

## Goal

The HLD wants mistakes to be "understandable and retries inviting". Three hearts make a mistake survivable, and 0.75 seconds of protection after each hit means overlapping attacks cannot drain all three at once, so every lost heart can be traced to one mistake. The dash's invulnerability (F02) is what makes dashing through danger "fast, precise and powerful": passing through costs nothing, stopping inside it does. F03 builds the hearts, the protection and the invulnerability check, proven on one placeholder hazard zone before real enemies attack in F04.

## Acceptance criteria

Everything from F01 and F02 still holds (coordinates, tolerance of 0.05 units and 5 degrees, nose red = dash ready). An orange 2 × 2 hazard zone lies flat on the floor, centred at (−8, −3), covering x from −9 to −7 and y from −4 to −2. The player is "in the zone" when any part of its 1-unit body overlaps it. "Hearts" is the Current Hearts value on the Player's health component in the Inspector.

1. **Start state.** Starting state: Play Mode just started, Player selected. Action: none. I see: Hearts = 3, the "Protected" tick is off, the circle is solid white.
2. **One hit.** Starting state: player at (−10.5, −3), 3 hearts, not in the zone. Action: tap D to step into the zone's left edge, then hold A to walk back out within 0.75 seconds. I see: Hearts = 2 the moment the body overlaps the zone (centre at about x = −9.5); the circle blinks between solid and half-transparent every 0.1 seconds, with "Protected" ticked, for 0.75 seconds; then it is solid white again and Hearts is still 2.
3. **Staying in danger.** Starting state: player outside the zone with 3 hearts. Action: walk into the zone and stop inside it. I see: Hearts = 2 at once, 1 after 0.75 seconds, 0 after 1.5 seconds.
4. **Zero hearts.** Starting state: 1 heart left, player in the zone, protection about to run out. Action: wait, then press WASD, move the mouse and left-click. I see: Hearts = 0; the player stops with Velocity (0, 0); it fades out over 0.3 seconds; the "Defeated" tick is on; WASD, mouse and clicks do nothing; Hearts never goes below 0.
5. **Dashing through danger is free.** Starting state: player at (−10.5, −3) facing right, 3 hearts, nose red. Action: left-click once. I see: the player dashes through the whole zone to (−5.5, −3), clear of it; Hearts is still 3 and the circle never blinks.
6. **Slow dash through danger.** Starting state: not in Play Mode; set `DashConfig` Duration to 2; enter Play Mode, player at (−10.5, −3) facing right, 3 hearts. Action: left-click once. I see: the circle takes 2 seconds to cross to (−5.5, −3), visibly passing over the orange zone, and Hearts stays 3 the whole time. (Set Duration back to 0.15.)
7. **Stopping in danger costs a heart.** Starting state: player at (−3, −3) facing left (cursor on the left wall at the player's height), 3 hearts, nose red. Action: left-click once. I see: the dash ends at (−8, −3), in the middle of the zone; Hearts is 3 during the dash and becomes 2 on the frame the dash ends; the circle starts blinking.
8. **Restore.** Starting state: Play Mode, Hearts = 1 or 2, player outside the zone. Action: right-click the health component's header in the Inspector and choose "Restore Hearts". I see: Hearts = 3.
9. **Tuning lives in a config asset.** Starting state: not in Play Mode. `Assets/_Game/Config/HealthConfig.asset` shows Max Hearts = 3, Protection Time = 0.75, Blink Interval = 0.1 and Defeat Fade Time = 0.3. Action: set Protection Time to 3, enter Play Mode, walk into the zone and stop. I see: Hearts = 2 at once and 1 only after 3 seconds, with the circle blinking for those 3 seconds. (Set it back to 0.75.)

## Edge cases

- **Every hit costs exactly 1 heart,** whatever caused it. This is a rule, not a tuning number.
- **Hit while protected:** ignored. It does not restart or extend the 0.75 seconds.
- **Hit while dashing:** ignored, and it starts no protection.
- **Two hits on the same frame:** one heart is lost.
- **Protection ends while still in danger:** the next hit lands at once, so hits arrive 0.75 seconds apart.
- **Dash ends in danger while protection is still running:** no hit until the protection ends; then the hit lands.
- **Dashing while protected:** allowed. The blinking continues for the rest of the 0.75 seconds.
- **The final hit:** no blinking; the player fades out instead.
- **While defeated:** no more hits, no movement, no aim, no dash. The nose fades with the body. The faded player has no collision. Restart Play Mode to try again.
- **Restore at 3 hearts:** nothing changes.
- **Restore while protected:** Hearts goes to 3; the protection keeps running for its remaining time.
- **Restore while defeated:** nothing happens. It does not bring the player back (a new run is F06).
- **Blinking:** only the white circle blinks, between fully solid and half-transparent (alpha 0.5). The nose keeps showing dash readiness. The circle is solid on the frame protection ends.
- **A hit does not move the player:** no knockback, no stun, walking speed unchanged.
- **The hazard zone:** it is drawn under the player and the dummies, does not block walking or dashing, never moves, does not affect dummies, and is not a dash target (dashing through it alone is a miss).
- **Timers:** protection, blinking and the defeat fade follow game time.

## Examples

- 3 hearts, step into the zone and straight out: 2 hearts, 0.75 seconds of blinking, then solid.
- 3 hearts, stand in the zone: 2 at 0 s, 1 at 0.75 s, 0 at 1.5 s, faded out by 1.8 s.
- 3 hearts at (−10.5, −3), dash right through the zone: 3 hearts at (−5.5, −3).
- 3 hearts at (−3, −3), dash left into the zone: 2 hearts at (−8, −3), blinking.
- 2 hearts, protected, 0.3 seconds into the blink, still in the zone: still 2 hearts; the next hit lands 0.45 seconds later.
- 1 heart, "Restore Hearts": 3 hearts.

## Out of scope

- Real enemy attacks, warnings and projectiles (F04). The orange zone is a placeholder and F04 removes it.
- Restoring hearts between waves and clearing leftover attacks first (F05). F03 only provides the restore.
- Defeat screen, retry, return to menu (F06). F03 only announces the defeat.
- Hearts on the HUD (F07). Until then Hearts is read in the Inspector.
- Hit sound, particles, screen shake, knockback, slow motion (F08, F09, F10).
- Health pickups, healing, more than 3 hearts, different damage amounts, a grace period after a dash.

## Context for the agent (optional)

Read first: `CLAUDE.md`, `Docs/Specs/F02-dash-attack-and-kill-recharge.md`, the F01 and F02 code in `Assets/_Game/Scripts/`.

Decisions that came from the interview and are not obvious from the criteria:

- The hearts rules (lose one, protection timer, defeat, restore) are a plain C# class with Edit Mode tests, as `CLAUDE.md` requires for health and damage. The MonoBehaviour is a thin wrapper.
- F04's attacks will hurt the player the same way the zone does, so "something that damages the player on overlap" should be reusable, and it finds the player by component, never by tag or name.
- F07's HUD will show hearts, F06 needs the defeat, and F05 will call the restore, so hearts changes and the defeat are announced with C# events. Game code never references UI.
- The player's dash already reports "Invulnerable" (F02); health reads that, the dash does not know about health.
- `OnTriggerEnter2D` alone is not enough: a player who stays in the zone must be hit again when protection ends.

Placeholders (to be replaced later):

| Placeholder | Stands in for |
|---|---|
| Orange 2 × 2 zone centred at (−8, −3) | Charger and Turret attacks (F04) |
| White circle blinking | Hit feedback (F08) |
| Current Hearts in the Inspector | HUD hearts (F07, `full and empty hearts` sprites in F08) |
| Player fading out at 0 hearts | Defeat presentation (F06, F08) |

Open values: none.
