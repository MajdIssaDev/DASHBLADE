using System;
using Dashblade.Combat;
using Dashblade.Player;
using Dashblade.Waves;
using UnityEngine;

namespace Dashblade.Flow
{
    /// <summary>
    /// Thin scene wrapper for the end of a run (F06), on the "Run Ending" object in Arena.unity. The rules live in
    /// <see cref="EndingFlow"/>. It listens to <see cref="ArenaRun.VictoryReached"/> and
    /// <see cref="ArenaRun.DefeatReached"/>, counts Ending Delay of game time, and when the ending shows it freezes
    /// game time (<see cref="Time.timeScale"/> = 0), switches off the player's walking, aim and dash, and announces
    /// <see cref="EndingShown"/>. The ending panel (UI) listens to that event and passes button clicks to
    /// <see cref="Choose"/>; this component knows nothing about the UI.
    /// </summary>
    /// <remarks>
    /// Runs before the player (<see cref="PlayerDash"/> is -10), so on the frame the ending shows the player's
    /// components are already off: a click on that frame never starts a dash and the body does not turn. The result
    /// itself arrives later in some frame (kills and hits happen in the dash's, enemies' and damage sources' Updates),
    /// so that frame is not counted and the delay counts from the next frame.
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrder)]
    public sealed class RunEnding : MonoBehaviour
    {
        /// <summary>Before <see cref="PlayerDash"/> (-10) and everything else in the run.</summary>
        public const int ExecutionOrder = -20;

        [SerializeField] private EndingConfig config;

        [Tooltip("The run whose victory or defeat ends the game.")]
        [SerializeField] private ArenaRun run;

        [Tooltip("The player whose walking, aim and dash are switched off when the ending panel appears.")]
        [SerializeField] private PlayerDash player;

        private EndingFlow flow;
        private PlayerMovement movement;
        private PlayerAim aim;
        private int resultFrame = -1;
        private bool froze;

        /// <summary>True from the moment the ending shows (game frozen, player input off).</summary>
        public bool IsShowing => flow != null && flow.IsShowing;

        /// <summary>True once the run has a result (it may still be in the Ending Delay).</summary>
        public bool HasResult => flow != null && flow.HasResult;

        /// <summary>The run's first result; meaningful only when <see cref="HasResult"/> is true.</summary>
        public EndingResult Result => flow != null ? flow.Result : EndingResult.Victory;

        /// <summary>Raised once, with the result, the moment the ending shows; game time is already frozen.</summary>
        public event Action<EndingResult> EndingShown;

        private void Awake()
        {
            if (config == null || run == null || player == null)
            {
                Debug.LogError($"{nameof(RunEnding)} on '{name}' needs its {nameof(EndingConfig)}, run and player assigned.", this);
                enabled = false;
                return;
            }

            movement = player.GetComponent<PlayerMovement>();
            aim = player.GetComponent<PlayerAim>();

            try
            {
                flow = new EndingFlow(config.EndingDelay);
            }
            catch (ArgumentException exception)
            {
                Debug.LogError($"{nameof(RunEnding)} on '{name}': invalid {nameof(EndingConfig)} ({exception.Message}).", this);
                enabled = false;
                return;
            }

            flow.Shown += OnShown;
            run.VictoryReached += OnVictoryReached;
            run.DefeatReached += OnDefeatReached;
        }

        private void OnDestroy()
        {
            if (run != null)
            {
                run.VictoryReached -= OnVictoryReached;
                run.DefeatReached -= OnDefeatReached;
            }

            // However the scene is left, never leave the game frozen behind it.
            if (froze)
            {
                Time.timeScale = 1f;
            }
        }

        private void Update()
        {
            if (Time.frameCount != resultFrame)
            {
                flow.Tick(Time.deltaTime);
            }
        }

        /// <summary>
        /// A button on the ending panel was clicked. Only the first Retry, Menu or Quit while the ending shows is
        /// carried out (the game is unfrozen and the scene loaded, or the game quits); returns false for every other
        /// call, which changes nothing.
        /// </summary>
        public bool Choose(MenuChoice choice)
        {
            if (flow == null || !flow.TryChoose(choice))
            {
                return false;
            }

            SceneFlow.Carry(choice);
            return true;
        }

        private void OnVictoryReached()
        {
            Report(EndingResult.Victory);
        }

        private void OnDefeatReached()
        {
            Report(EndingResult.Defeat);
        }

        private void Report(EndingResult result)
        {
            if (flow.ReportResult(result))
            {
                resultFrame = Time.frameCount;
            }
        }

        private void OnShown(EndingResult result)
        {
            Time.timeScale = 0f;
            froze = true;

            // Off rather than locked: nothing turns them on again before the scene is left. A dash cut off here
            // stops where the body is (PlayerDash.OnDisable).
            player.enabled = false;
            if (movement != null)
            {
                movement.enabled = false;
            }

            if (aim != null)
            {
                aim.enabled = false;
            }

            EndingShown?.Invoke(result);
        }
    }
}
