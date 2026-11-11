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
