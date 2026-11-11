# DASHBLADE · One-page HLD

**One-line pitch:** A masked swordsman destroys ancient machines by dashing through them, chaining kills to recharge the next dash while a miss leaves time to dodge.

**Genre · view · platform:** Single-player 2D top-down combat arena · Unity · Windows. Planned semester project.

## Target experience

Feel fast, precise and powerful while chaining kills. Readable attacks should make mistakes understandable and retries inviting.

## Core loop

Read → position → dash → chain. A kill readies the next dash; a miss requires dodging until it recharges.

## Win and lose conditions

- **Win:** Clear all three waves to reach victory.
- **Lose:** Lose all three hearts to reach defeat. Either ending offers retry from wave 1, return to menu or quit.

## Controls

WASD: move. Mouse: aim. Left-click: dash and slash. Mouse clicks: operate menus.

## Must-have features

- **F01 Movement and aim:** Move freely and aim toward the mouse.
- **F02 Dash attack and kill recharge:** One successful dash kills an ordinary enemy. A kill readies the next dash when the current dash finishes. A miss causes a short cooldown with normal movement available. Dashing grants invulnerability; walls stop it.
- **F03 Hearts and damage protection:** Three hearts, restored between waves; brief protection after damage prevents overlapping hits.
- **F04 Charger and Turret:** Charger warns along a straight lane, then charges. Stationary Turret warns, then fires slow, visible projectiles. Orange danger warnings use shapes and motion too.
- **F05 Arena and three fixed waves:** One arena: chargers, then turrets, then both. Clearing each wave advances the run. Target 3–5 minutes, to be tested.
- **F06 Menus and endings:** Main menu with Play, control instructions and Quit; victory and defeat screens with Retry, Menu and Quit.
- **F07 Basic HUD and Windows build:** Show hearts, wave and dash readiness; provide a playable Windows build.
- **F08 AI-made visuals, music, SFX and UI:** Integrate a minimal set by Lesson 12; use placeholder shapes for the prototype.

## Look and sound

Dark stone, warm lanterns, masked hero with red scarf, red machines and cyan dash trails. Orange marks danger with clear shapes and motion. AI-made slash/impact sounds, one combat music loop and readable UI; extra polish stays optional.

## Out of scope this semester

**F11 Excluded expansions:** Multiplayer, open world, procedural levels, bosses, extra weapons, upgrades, branching story, saved progression, extra arenas, enemy types or waves.
