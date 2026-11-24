# AI Development Log

One entry per real work session, starting with the Lesson 2 class work, newest at the bottom. From Lesson 3 on, commit it together with the work.

AI may draft an entry from a session, but I must check and correct it before keeping it. Record significant AI contributions and quote key prompts exactly. Include what I kept, changed or rejected, and why. I must personally write “How I verified it” using checks I actually performed. Record the real model and time spent; do not guess missing facts.

The entry below records this planning session. I confirmed that this is Session 1 and that I spent one hour on it. I already had the game idea before this session.

## Template (copy it for each session)

```
## YYYY-MM-DD · Session N · [short title]
- Goal:
- Model:
- Key prompts:
- What the AI produced:
- Accepted:
- Changed (and why):
- Rejected (and why):
- How I verified it:
- Time spent:
```

## Entries

## 2026-11-11 · Session 1 · DASHBLADE game pitch and scope
- Goal: I wanted to turn my existing DASHBLADE idea into a clear game pitch and a practical development plan for one student and one semester.
- Model: GPT 6.1 Sol, High reasoning.
- Prompt used: I provided the lecture materials and the Lesson 2 game-design-kit templates as context. These are selected excerpts from my original prompt:

  > You are a game designer and Unity development mentor helping a student scope a small semester project. Be a practical, fair critic: identify risks and suggest cuts instead of automatically adding features.
  >
  > I am studying AI-driven game development using Unity.
  >
  > My chosen game is DASHBLADE.
  >
  > Game concept:
  > A single-player 2D top-down combat arena game for Windows. A masked swordsman destroys ancient machines by dashing through them. Every kill immediately recharges the dash. Missing an attack leaves a short cooldown during which the player must dodge.
  >
  > Target experience:
  > The player should feel fast, precise and powerful while chaining kills. Enemy attacks should be readable, so mistakes feel understandable and retries feel inviting.
  >
  > Core loop:
  > Read → position → dash → chain.
  >
  > Controls:
  > - WASD: move.
  > - Mouse: aim.
  > - Left-click: dash and slash.
  > - Mouse clicks: operate menus.
  >
  > Combat:
  > - Ordinary enemies die from one successful dash.
  > - A kill makes the next dash available after the current dash finishes.
  > - A missed dash causes a short cooldown; normal movement remains available.
  > - The player is invulnerable during the dash.
  > - Walls stop the dash.
  > - Enemy attacks display orange warnings before dealing damage.
  > - The player has three hearts, restored between waves.
  > - Brief protection after taking damage prevents overlapping hits from removing all hearts immediately.
  >
  > Enemies:
  > 1. Charger: shows a straight attack lane, then charges.
  > 2. Turret: stays in place and fires slow, visible projectiles.
  >
  > Structure:
  > One arena and three fixed waves: chargers, turrets, then both together.
  > Clear all waves to win. Lose all hearts to lose.
  > Include a main menu, control instructions, victory and defeat screens, retry, return to menu and quit.
  > Target run length: approximately 3–5 minutes, to be tested.
  >
  > Visual and audio direction:
  > Dark stone arena, warm lanterns, masked hero with a red scarf, red mechanical enemies and cyan dash trails. Orange communicates enemy danger. Use clear shapes and motion alongside color. Add slash sounds, impact sounds and a combat music loop.
  >
  > Work in small stages:
  >
  > 1. Read the attached assignment and templates. List your assumptions and the three biggest project risks. Distinguish requirements from your suggestions.
  > 2. Create Docs/HLD.md using every section of the supplied template. Keep it concise enough for one page.
  > 3. Create Docs/Backlog.md with a MoSCoW feature table, milestone plan, riskiest Must and practical plan B.
  > 4. Create Docs/DevLog.md using the supplied template, but leave the actual entries empty for now.
  > 5. Check consistency across the documents and report what remains before submission.
  >
  > - Scope the project for one student and one semester.
  > - Every line must be understandable and explainable by me.
  > - Musts must form a complete game from main menu to ending.
  > - Keep one arena, two enemy types and three fixed waves.
  > - Placeholder shapes are acceptable for the prototype.
  > - Plan AI-made visuals, music, SFX and UI for Lesson 12.
  > - Exclude multiplayer, open world, procedural levels, bosses, extra weapons, upgrades, branching story and saved progression.
  > - Optional polish must remain Should or Could.
  > - Do not invent development sessions, prompts, time spent, playtests or completed work.
  > - Treat the game as planned, not already implemented.
  > - Identify missing information honestly. Do not invent calendar deadlines.

- What the AI produced: I received AI drafts of Docs/HLD.md, Docs/Backlog.md and Docs/DevLog.md. The drafts described my game, prioritized its features, set milestones, identified three main risks and suggested a simpler dash plan B. I also received a consistency check covering the required sections, feature IDs and milestones.
- Accepted: I kept the written plan because it matched my DASHBLADE idea: dash combat with recharge on a kill, one arena, two enemy types and three fixed waves.
- Changed (and why): I first asked to leave the Dev Log entries empty. After reading the files, I asked to add a real entry for this planning session so I could record the work accurately.
- Rejected (and why): I did not explicitly reject a new AI suggestion during this session. I had already excluded features such as multiplayer, bosses and upgrades to keep the project manageable.
- How I verified it: I read the files and checked that they matched my game idea.
- Time spent: I spent 1 hour on this planning session. I already had the game idea beforehand.

## 2026-11-24 · Session 2 · Unity 6.3 LTS project setup and CLAUDE.md
- Goal: Review Lesson 1–6 and Home Assignment 02 requirements, install the Lesson 4 context-kit, set up the DASHBLADE Unity 6.3 LTS project connected to the existing HW1 GitHub repository, verify Unity CLI Editor connectivity, and write the initial `CLAUDE.md`.
- Model: Gemini 4 Neon (in Antigravity).
- Key prompts:
  - "Role:
You are a disciplined Unity 6 developer helping me set up my solo semester project (DASHBLADE) for the AI Driven Development course.

Context:
- My Unity 6.3 LTS (6000.3.29f1) Universal 2D project with com.unity.pipeline is at E:\AIForGames\DASHBLADE.
- My existing HW1 GitHub repository is https://github.com/MajdIssaDev/DASHBLADE.git (containing Docs/HLD.md, Docs/Backlog.md, and Docs/DevLog.md).
- My course materials are in the uploaded Class Materials zip (lesson03/unity-kit and lesson04/context-kit).
- I am working in Manual mode and keep this project completely separate from any other folders on my PC (do not touch ROBLOXPROJECT or E:\AIForGames\HW1).

Task:
1. Extract lesson04/context-kit to E:\AIForGames\lesson04\context-kit\.
2. Copy .gitignore and .claude/settings.json from lesson03/unity-kit into E:\AIForGames\DASHBLADE\, add .vscode/ to .gitignore, and copy SPEC-template.md into E:\AIForGames\DASHBLADE\Docs\Specs\.
3. Link E:\AIForGames\DASHBLADE to origin/main (https://github.com/MajdIssaDev/DASHBLADE.git) preserving my HW1 commit history and Docs/ files without overwriting any Unity files.
4. Open E:\AIForGames\DASHBLADE in Unity Editor via the Unity CLI until `unity status` reports `ready`, then inspect (read-only) the Unity version, render pipeline, activeInputHandler setting, open scene hierarchy, and Console errors/warnings.
5. Draft E:\AIForGames\DASHBLADE\CLAUDE.md from CLAUDE-template.md using only verified Unity facts and decisions that cannot be found by reading code.

Constraints:
- Never touch any file outside E:\AIForGames\DASHBLADE and E:\AIForGames\lesson04\ (reason: keep other projects isolated and HW1 untouched as a backup).
- Never commit or push — I own every commit and push in this course.
- Never edit .unity, .prefab, or .asset files as text.
- Keep CLAUDE.md under 60 lines (~1 screen), include verified Unity 6.3 API notes, include zero secrets, and mark every architectural or convention decision I haven't confirmed yet with [?].
- If Unity stops at a dialog (software terms, firewall, or URP upgrade), pause and tell me to click it instead of retrying.

Done means (output format):
1. A short status table showing: Unity CLI status, open scene hierarchy, Console error/warning count, and `git status --short` (confirming Library/, Logs/, UserSettings/, and .vscode/ are ignored).
2. A numbered list of every [?] decision in CLAUDE.md for me to approve, change, or cut before you remove the [?] tags.
3. No code files created yet — stop after the CLAUDE.md draft so I can review it, update Docs/DevLog.md, and make the commit myself."
- What the AI produced: Extracted the Lesson 4 `context-kit` into `E:\AIForGames\lesson04\context-kit\`, installed `.gitignore` and `.claude/settings.json` from `lesson03/unity-kit` and `Docs/Specs/SPEC-template.md`, linked `E:\AIForGames\DASHBLADE` to `MajdIssaDev/DASHBLADE` (`origin/main`) with HW1 `Docs/` intact, checked the live Unity Editor (`6000.3.29f1`, URP 2D, Input System only, `SampleScene`, 0 console errors), and wrote `CLAUDE.md`.
- Accepted: Kept `E:\AIForGames\DASHBLADE` as the single semester repository linked to HW1's GitHub repo, approved the drafted `CLAUDE.md` architecture and convention rules, and added `.vscode/` to `.gitignore`.
- Changed (and why): Stopped the agent from working inside the unrelated workspace (for another personal project) and directed all course work to `E:\AIForGames\DASHBLADE`.
- Rejected (and why): Deferred installing `unity setup antigravity` for now; kept commits, pushes, and Unity terms acceptance manual.
- How I verified it: Reading CLAUDE.md, confirming HW1's Docs/ are in E:\AIForGames\DASHBLADE\Docs\, enabling main branch protection on GitHub, and running git status before committing to confirm Library/, Logs/, UserSettings/, and .vscode/ are ignored
- Time spent: around 1 hour

## 2026-11-24 · Session 3 · Specs for F01–F07 and prototype build with subagents

> AI-drafted during the session, at my request. I still have to check and correct every line, and write "How I verified it" and "Time spent" myself. Lines marked TODO are mine to fill in. The build status list at the end is updated as each feature finishes.

- Goal: Start Home Assignment 02: agree a build order for F01–F07, write one spec per feature by interview before it is built, then have the features built one at a time and reviewed against their specs.
- Model: Claude Mythos 5.2 in Claude Code (desktop app) as the orchestrator that interviewed me, wrote the specs and reviewed the builds. Build and rules-drafting subagents were launched with the `opus` model setting at high effort, as I asked ("Opus 5.5 on high").
- Key prompts:
  - First prompt (spec session for F01):

    > Role:
    > You are a disciplined Unity 6.3 developer and spec writer helping me start Home Assignment 02 (Prototype) for my solo semester project game, DASHBLADE.
    >
    > Context:
    > - CLAUDE.md in the project root holds the project rules. Follow it.
    > - Read these first, before asking me anything:
    >   - Docs/HLD.md (target experience, core loop, features F01-F08)
    >   - Docs/Backlog.md
    >   - Docs/Specs/SPEC-template.md (the spec format to use)
    >   - E:\AIForGames\lesson04\context-kit\example\example-spec-health-pickup.md (an example of a finished spec)
    > - Home Assignment 02 needs F01-F07 playable from main menu to victory or defeat with placeholder shapes, one spec per feature written before it is built, a Windows build, and committed work.
    > - The project is a fresh Universal 2D template: only SampleScene exists and there is no game code yet.
    >
    > Task:
    > 1. After reading, give me a short build order for F01-F07, one line each, with the reason for the order. Respect the backlog: the dash (F02) is the riskiest Must and must be proven early on a small placeholder test.
    > 2. Then interview me about F01 Movement and aim only. Ask one question at a time and wait for my answer. Offer 2-3 options where that helps me decide. Keep going until you could write a spec with a goal, acceptance criteria, edge cases, examples and out of scope.
    > 3. Write the spec to Docs/Specs/F01-movement-and-aim.md from the template.
    >
    > 4. In case any asset you need that I have not provided or located in project folder, make it a place holder and let me know what assets are placeholders so I can provide it to you later on.
    >
    > Constraints:
    > - Write no code and change nothing in Unity in this session. The only file you create is the F01 spec.
    > - Do not edit Docs/DevLog.md, Docs/HLD.md or Docs/Backlog.md.
    > - Every acceptance criterion must be testable in Play Mode and contain a real number: a starting state, one action, what I see.
    > - Tie the goal to the target experience in the HLD.
    > - Edge cases must cover the situations where you would otherwise guess (for example: no input, diagonal input, mouse on top of the player, mouse outside the window, arena walls).
    > - Do not invent a value I have not given you. Ask me, or mark it [?] in the spec.
    > - Anything beyond F01 (dash, enemies, health, HUD, menus) goes under "Out of scope".
    > - Never commit or push.
    >
    > Done means:
    > 1. Docs/Specs/F01-movement-and-aim.md exists and follows every section of the template.
    > 2. A numbered list of every [?] left in the spec for me to answer.
    > 3. A list of every decision I made during the interview, so I can check the spec against it.
    > 4. Stop there. I will review the spec, then build it in a fresh session.

  - Second prompt (changing from "build in a fresh session" to building here with subagents):

    > I approve the plan after reading it. also, i have added a folder with assets to use (made using Chatgpt instructed to make sheets of assets), path : /E:/AIForGames/DASHBLADE/Assets/_Game/Art/SpriteSheets
    >
    > To work on it, I require you to be the orchestrator, use sub agents of Opus 5.5 on high, to work on the features and they report back to you to review and you can interview me on the reports if anything seemed off the plan.

  - Third prompt: "Start interview for all F02 - F07 , once each interview is done, start subagent for it and continue interview me for the next"
  - Fourth prompt (sent mid-interview): "btw run paralel agents when possible on scripts or features that dont interfere"
  - Fifth prompt: "Once the subagent of F02 finish, continue the rest. also make sure to log everything needed in devlog."
- What the AI produced:
  - A build order for F01–F07 (F01, F02, F03, F04, F05, F06, F07), with the dash second so the riskiest Must is proven on a small test before real enemies exist.
  - Seven specs in `Docs/Specs/`, each written from an interview where I picked from 2–3 options or typed my own answer: `F01-movement-and-aim.md`, `F02-dash-attack-and-kill-recharge.md`, `F03-hearts-and-damage-protection.md`, `F04-charger-and-turret.md`, `F05-arena-and-three-waves.md`, `F06-menus-and-endings.md`, `F07-basic-hud-and-windows-build.md`. F01 first had five `[?]` values, which I answered before it was built; none of the seven specs has an open value now.
  - F01 built by a subagent through the Unity CLI: scripts in `Assets/_Game/Scripts/Player/` and `Assets/_Game/Scripts/Display/`, Edit Mode tests in `Assets/_Game/Tests/EditMode/`, `Assets/_Game/Config/PlayerConfig.asset`, `Assets/_Game/Physics/PlayerFrictionless.physicsMaterial2D` and a new scene `Assets/_Game/Scenes/Arena.unity` (placeholder player, walls, door marker, fixed letterboxed camera).
  - Checks the AI ran on F01 (these are the AI's checks, not my verification): 29 Edit Mode tests passed; the subagent injected keyboard and mouse state in Play Mode and measured the acceptance criteria; the orchestrator re-ran the tests, read the code, read the scene values through the CLI and looked at Play Mode captures. The AI reported three things it could not check: the 1920 × 1080 Game view, the 4:3 letterbox, and a real Alt+Tab while holding a key.
  - Side effects reported by the AI: Unity upgraded `Assets/Settings/UniversalRP.asset` (asset version 12 to 13) and created `ProjectSettings/SceneTemplateSettings.json` on its own.
  - Rules code for F03, F04 and F05 (plain C# classes with NUnit tests) drafted by parallel subagents in a temporary staging folder outside the project while the Editor was busy with F02, and compiled and tested outside Unity (39, 83 and 94 tests passing as reported). It is not in the project until each feature's build moves it in.
  - Build status at the time of writing: see the list at the end of this entry.
- Accepted:
  - The build order.
  - F01: instant start and stop, 1-unit player, 6 units/second, normalised diagonals, opposite keys cancel, whole body turns and snaps to the mouse, 0.5-unit aim dead zone, slide along walls at the natural 4.24, fixed camera, 24 × 12 test area with 1-unit walls, new `Arena` scene, white circle with red nose, screen-relative WASD, stop on focus loss.
  - F02: the backlog's plan B path check from the start, 5 units in 0.15 seconds, pass through and kill every enemy on the path, input ignored during the dash, early clicks ignored, constant speed on a shortened dash, a blocked dash counts as a miss, nose colour as the readiness signal, four test dummies in a row, invulnerability flag now and the damage check in F03.
  - F03: 1 heart per hit, body blinks while protected, hearts read in the Inspector until the HUD, orange test hazard zone, hit again as soon as protection ends, hit on the frame a dash ends in danger, controls stop and the body fades at 0 hearts, a right-click "Restore Hearts" for testing.
  - F04: 1-second lane warning, charge runs to the first wall at 15 units/second, 1.5-second rest, Charger stays where it stopped and only hurts while charging, 0.8-second ring warning, Turret aims when it fires, 1.5 seconds between shots, shots already flying carry on after the Turret dies, 1-second first-attack delay, lane that fills up, shrinking ring, triangle / hexagon / small circle placeholders, old test objects moved to a test scene.
  - F05: 3 Chargers, then 3 Turrets, then 2 + 2; 2-second pause before every wave; player not moved between waves; layout chosen once per run; victory leaves the player in control.
  - F06: separate `MainMenu` and `Arena` scenes, controls always visible on the menu, ending panel over the dimmed arena after 1 second, game frozen behind the panel, Quit stops Play Mode in the Editor, Esc does nothing, the proposed wording.
  - F07: HUD along the top wall, filled and hollow squares for hearts, refilling cyan dash bar, nose signal kept as well, "Wave 1 / 3", fullscreen release build in `Builds/Windows`, built by the agent.
- Changed (and why): TODO: check each line and add my reasons.
  - I first planned to build F01 in a fresh session, then had the same session orchestrate subagents instead.
  - F01 start position: I replaced the offered options with a spawn point next to a door (door marker on the left wall, spawn 1.5 units inside it).
  - F01 input: I added the arrow keys as an alternative to WASD (the HLD lists WASD only).
  - F01 aspect ratio: I chose to letterbox to 16:9, which added work to F01.
  - F02 hit width: I set 1.25 units, between the two options offered (1 and 1.5).
  - F02 miss cooldown: I chose 1 second.
  - F02 enemy death: I asked for enemies to fade out as if destroyed (0.3 seconds), not vanish.
  - F03 protection time: I chose 0.75 seconds.
  - F04 lane: I chose a lane that follows the player and then locks for the last 0.4 seconds.
  - F04 projectile speed: I chose 7 units/second, faster than walking.
  - F04 enemy bodies: I asked for solid enemies with a way out when stuck at a wall or corner (an enemy overlapping the player does not block until they separate).
  - F04 dash against shots: I chose that a dash destroys a shot without it counting as a kill.
  - F05 arena: I asked for a bigger arena (32 × 18, camera zoomed out to size 10) with six 2 × 2 pillars per run, picked at random from three hand-made layouts. This differs from the HLD (procedural levels out of scope) and the backlog's dash plan B (open arena without obstacles); the HLD and backlog are not updated yet.
  - F05 arrival: I chose enemies fading in over 0.5 seconds.
  - Working method: I asked for parallel agents where work does not interfere. Because every agent shares the one open Editor, only Editor-free rules code could run in parallel.
- Rejected (and why): TODO: check each line and add my reasons.
  - Using the new sprite sheets in F01: I kept placeholder shapes for the prototype and left the sheets for F08.
  - Enemies the player can walk through (the AI's recommendation): I wanted solid enemies.
  - A lane fixed from the start of the warning (the AI's recommendation).
  - The open 24 × 12 arena with no obstacles (the AI's recommendation).
  - Fully random obstacle positions: I chose random picks from hand-made layouts.
  - An Esc pause panel or Esc-to-menu.
- How I verified it: TODO: I must write this myself from checks I actually performed.
- Time spent: TODO: mine to fill in.
- Build status (updated by the AI as each feature finishes; nothing here is committed by the AI):
  - F01 Movement and aim: built and reviewed by the AI. My own Play Mode test: TODO.
  - F02 Dash attack and kill recharge: built and reviewed by the AI. The dash lives in `Assets/_Game/Scripts/Combat/` with `DashConfig.asset` and `DeathFadeConfig.asset`; four test dummies were added to `Arena.unity`. AI checks: 69 Edit Mode tests passed (29 from F01 plus 40 new), and the subagent measured all 13 criteria in Play Mode with injected input. Reported by the AI as not checked: a click on a letterbox bar, losing focus during a dash, and a real hand on the mouse. Reported differences: a dash into a wall stops at x = 11.495 (the same place walking rests, inside the 0.05 tolerance), and a dash aimed even slightly into a wall the player is touching is cut short by that wall. My own Play Mode test: TODO.
  - F03 Hearts and damage protection: built and reviewed by the AI. `PlayerHealth` and `DamagesPlayerOnOverlap` in `Assets/_Game/Scripts/Combat/`, `HealthConfig.asset`, and an orange hazard zone in `Arena.unity`. The staged rules code was moved in with one fix by the build subagent (protection ended one frame late at exactly 60 fps because of float rounding). The subagent also changed two F02 files so that switching the dash off mid-dash is safe. AI checks: 112 Edit Mode tests passed, and the subagent measured all 9 criteria in Play Mode with injected input. Reported by the AI as not done: the real right-click "Restore Hearts" in the Inspector (the same method was called by script). Reported difference: the first hit lands at about x = −9.42, not −9.5, because walking moves in 0.12-unit physics steps. My own Play Mode test: TODO.
  - F04 Charger and Turret: built and reviewed by the AI. Enemy code in `Assets/_Game/Scripts/Enemies/`, prefabs `Charger`, `Turret`, `Projectile` and a shared `Player` prefab in `Assets/_Game/Prefabs/`, `ChargerConfig.asset` and `TurretConfig.asset`. `Arena.unity` now holds one Charger at (6, 3) and one Turret at (6, −3); the dummies and the hazard zone moved to the new `Test_DashAndHearts.unity`. The build subagent changed two F02 files: the dash now sweeps its hit area every frame so it catches moving enemies, and `DashTarget` gained a "Counts As Kill" flag so a destroyed shot is not a kill. It also changed two details of the staged rules code (rest timing after a charge; "player on top of the Charger" now means the bodies overlap). AI checks: 217 Edit Mode tests passed; the subagent measured all 18 criteria in Play Mode with injected input; the orchestrator re-ran the tests and looked at captures of the lane and the ring. Reported by the AI as not checked live: spawning an enemy from a prefab with a delayed start (the path F05 uses) and the dash against a Charger at uneven real frame times. Reported observation: a player pressed against the Turret when its ring closes is hit by the point-blank shot. Left over: `DashHitQueue.cs` and its tests are no longer used by the dash. My own Play Mode test: TODO.
  - F05 Arena and three fixed waves: spec written; rules code drafted outside the project; build in progress.
  - F06 Menus and endings: spec written; build not started.
  - F07 Basic HUD and Windows build: spec written; build not started.
