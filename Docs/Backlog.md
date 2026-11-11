# Feature backlog and scope plan

## Backlog (MoSCoW)

Planned work for one student and one semester. No implementation or playtest is claimed here.

- **Must:** needed for the complete game or an explicit course deliverable.
- **Should:** useful polish, planned only after the Musts are secure.
- **Could:** optional, only if time remains.
- **Won't (this semester):** excluded from the plan.

Effort is relative, not a time promise: S = small isolated task; M = several connected tasks; L = highest uncertainty or integration effort. Estimates include checking the result. The Won't estimate describes work being avoided.

| Feature | MoSCoW | Why | Effort (S/M/L) |
|---|---|---|---|
| F01 Movement and aim | Must | WASD movement and mouse aim let the player position for a dash. | S |
| F02 Dash attack and kill recharge | Must | Central mechanic: a dash kills ordinary enemies in one hit. A kill readies the next dash only after the current dash ends. A miss starts a short cooldown while movement remains available. Dash invulnerability and wall stopping are required. | L |
| F03 Hearts and damage protection | Must | Three hearts allow mistakes. Restore them between waves; brief protection after damage prevents overlapping hits from draining all hearts immediately. | M |
| F04 Charger and Turret | Must | Charger warns with a straight lane, then charges. Stationary Turret warns, then fires slow, visible projectiles. Orange warnings appear before damage, supported by shapes and motion. | L |
| F05 Arena and three fixed waves | Must | One arena runs chargers, then turrets, then both. Clear all enemies to advance or win. Target approximately 3–5 minutes, to be tested rather than promised. | M |
| F06 Menus and endings | Must | Main menu provides Play, control instructions and Quit. Victory and defeat each provide Retry from wave 1, Menu and Quit, completing the game flow. | M |
| F07 Basic HUD and Windows build | Must | Hearts, wave and dash readiness make the rules visible. A Windows build makes the complete game playable outside the editor. | M |
| F08 AI-made visuals, music, SFX and UI | Must | Lesson 12 requires integration. Keep one small visual set, slash/impact sounds, one combat music loop and readable menu/HUD graphics. Follow the HLD's visual direction and preserve warning clarity. | M |
| F09 Dash impact polish | Should | A brief hit flash or small impact effect can reinforce successful slashes. Core hit feedback must already be understandable without these effects. | S |
| F10 Extra sound variation | Could | Additional slash or impact variations reduce repetition; one sound for each action already works. | S |
| F11 Excluded expansions | Won't | Multiplayer, open world, procedural levels, bosses, extra weapons, upgrades, branching story, saved progression, extra arenas, enemy types or waves exceed the fixed semester scope. | L |

F01–F07 form the complete playable game using placeholders. F08 is the minimum course-required presentation pass, not a reason to add systems or elaborate animation. F09–F10 are optional polish.

## Scope plan

These are future checkpoints. Calendar dates and weekly availability were not supplied; use the course schedule to confirm them.

| Deadline | What must be playable | Backlog items |
|---|---|---|
| Home Assignment 02 · Prototype · Lesson 7 | Complete loop from main menu through all three fixed waves to victory or defeat, with retry, menu and quit. Both enemies, dash recharge/miss cooldown, wall stopping, damage protection and HUD work with placeholder shapes. Produce and check a Windows build. | F01–F07 |
| Home Assignment 03 · Agentic Workflow · Lesson 10 | Stable gameplay: check dash hits, misses and wall stops; overlapping damage; wave transitions; both endings; retry resets. Review actual AI-assisted changes and the student's verification against the real Dev Log. Adjust existing enemy counts and timings toward 3–5 minutes. | F01–F07; ongoing Dev Log |
| Home Assignment 04 · Generative Assets · Lesson 12 | Integrate AI-made visuals, music, SFX and UI into the working game. Check that enemies, warnings, hearts and dash readiness remain clear during mixed combat. Keep the asset set small. | F08 plus F01–F07 regression checks |
| Final Project · 3 days before Lesson 14 | Every Must complete. Check the Windows build from main menu through victory and defeat, then retry, return to menu and quit. Resolve blocking bugs and complete the honest Dev Log. Add polish only if the Musts remain stable. | F01–F08; F09–F10 only if time remains |

**Riskiest Must, and my plan B:** F02 Dash attack and kill recharge. Fast movement can skip enemies or walls, breaking both precision and the chain. First build a small placeholder test with an enemy and a wall. Check a hit, a miss, a wall stop, damage during a dash and consecutive kills. If unreliable, use a straight fixed-length dash, check for enemies and walls along its whole path, and stop at the first wall. Use an open rectangular arena without internal obstacles. Preserve one-hit kills, invulnerability, recharge after the current dash finishes, and the miss cooldown with movement. Keep both enemy types and all three waves.

### Requirements and planning assumptions

- **Requirements:** The assignment requires every HLD section, a complete set of Musts, milestones, a riskiest Must with plan B, and real session logs. The student fixes the game at one arena, two enemy types and three waves, with the combat and menu rules in the HLD. Lesson 12 requires AI-made visuals, music, SFX and UI.
- **Suggested starting assumptions:** Fixed top-down camera; straight dash with direction chosen at click; each unprotected enemy hit removes one heart; retry starts a fresh run at wave 1. Confirm these before implementation.
- **Values still to decide:** Dash distance/duration, miss cooldown, damage-protection duration, warning timing, projectile speed, enemy counts and spawn positions. Tune these in the prototype; do not add systems to fill time.
- **Setup still to confirm:** Lesson 2 lists Unity 6.3.28f1 LTS. Confirm the installed course version, available work time, repository location and actual lesson dates. The 3–5-minute run length remains an untested target.

### Three biggest risks and scope cuts

| Risk | Response and cut |
|---|---|
| F02 collision and recharge fail at dash speed | Test this first and use the plan B above before expanding combat. Avoid custom physics and complex arena geometry. |
| F03–F05 mixed combat feels unreadable or unfair | Limit simultaneous attacks, lengthen warnings and reduce enemy counts before adding effects. Clear lingering attacks between waves before restoring hearts. Keep three waves and both enemy types. |
| Solo workload and F08 asset integration crowd out gameplay | Keep placeholders through Lesson 10, then use a small consistent AI-made set for Lesson 12. Cut F10 first, then F09. Keep the required asset categories and complete menu-to-ending flow. |

These responses are mentor suggestions. They do not change the student's required game rules or claim that testing has happened. After each real work session, the student must record what actually happened and personally write “How I verified it.”
