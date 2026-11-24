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
