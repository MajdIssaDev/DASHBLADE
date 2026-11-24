# DASHBLADE: notes for the agent

## Project
- Unity 6.3 LTS (6000.3.29f1), Universal 2D template (URP 17.3.0), Input System only (`activeInputHandler: 1`, `com.unity.inputsystem 1.20.1`, legacy Input Manager is off).
- Solo student project. The agent writes all the code; I review it, test it in Play Mode, and commit it.
- Design: `Docs/HLD.md`. Backlog: `Docs/Backlog.md`. Feature specs: `Docs/Specs/`.

## Architecture
- Game code lives in `Assets/_Game/Scripts/<Area>/` with namespace `Dashblade.<Area>`.
- Gameplay tuning numbers live in ScriptableObject configs in `Assets/_Game/Config/` (e.g., `PlayerConfig`, `DashConfig`, `HealthConfig`). MonoBehaviours reference their config instead of storing tuning numbers in `[SerializeField]` fields.
- Core game rules (health/damage, dash recharge/cooldown, wave state) live in plain C# classes so Edit Mode tests can run fast; MonoBehaviours are thin scene wrappers.
- Game code never references UI. UI listens to C# events.

## Conventions
- Identify the player and enemies by component (`TryGetComponent<...>()`), never by tag or GameObject name.
- No singletons and no `Find*` calls at runtime. Wire references via Inspector or collision.

## Verified API notes (Unity 6.3 LTS, 6000.3.29f1, checked 2026-11-24)
- `Rigidbody2D.linearVelocity`, not `velocity`. `linearDamping`, not `drag`.
- Input: `UnityEngine.InputSystem` (`Keyboard.current`, `Mouse.current`). Legacy `UnityEngine.Input` (e.g., `Input.GetAxis`) throws `InvalidOperationException` in this project.
- `OnTriggerEnter2D` requires a `Rigidbody2D` on at least one of the two colliding objects.

## Never do
- Never edit `.unity`, `.prefab`, or `.asset` files as text. Scene and asset changes go through Unity CLI commands (`unity command ...`). No Editor connection? List the steps for me instead.
- Never add features that aren't in the spec or my request. Suggest them at the end instead.
- Never commit or push. I do that.

## When you finish a task
Report: the files you changed, the scene objects and assets you created, how each acceptance criterion is met, and what I should test in Play Mode.
