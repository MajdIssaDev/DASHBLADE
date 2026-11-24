using System;
using Dashblade.Combat;
using Dashblade.Player;
using Dashblade.Waves;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dashblade.Flow
{
    /// <summary>
    /// Thin scene wrapper for the pause (F11), on the "Run Pause" object in Arena.unity. The rules live in
    /// <see cref="PauseFlow"/>. It reads Esc, freezes game time (<see cref="Time.timeScale"/> = 0) and pauses the
    /// player's walking, aim and dash input while paused, lifts both on resuming, and announces every change through
    /// <see cref="StateChanged"/>. The pause panel (UI) listens to that event and passes its button clicks to
    /// <see cref="Resume"/>, <see cref="ShowControls"/>, <see cref="Back"/> and <see cref="Choose"/>; this component
    /// knows nothing about the UI, so the game runs the same with the panel's object switched off.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The player's components are paused through their <c>IsPaused</c> flags, never switched off: switching the
    /// dash off would cancel a dash in progress, while a paused dash must finish after resuming along the same path.
    /// </para>
    /// <para>
    /// Esc is read in LateUpdate, after every Update of the frame: by then any result of this frame (kills and hits
    /// happen in the dash's, enemies' and damage sources' Updates) has already reached <see cref="RunEnding"/>, so
    /// Esc on the frame the run ends does nothing and the ending wins. The freeze therefore starts from the next
    /// frame, and the panel is drawn on the frame Esc was pressed. Resuming with Esc is also applied in LateUpdate,
    /// so a left-click on the next frame dashes. The Resume button's click arrives in the EventSystem's Update, after
    /// the dash (-10) has already skipped its paused Update for that frame.
    /// </para>
    /// <para>
    /// The pause and the ending never show together: Esc never pauses once the run has a result, the Ending Delay
    /// is game time and cannot pass while paused, and a result that still arrives during a pause ends the pause at
    /// once (<see cref="ResumeCause.RunOver"/>).
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class RunPause : MonoBehaviour
    {
        [Tooltip("The run's ending: once the run has a result, Esc no longer pauses.")]
        [SerializeField] private RunEnding ending;

        [Tooltip("The run whose state decides whether Esc may pause (only while Playing).")]
        [SerializeField] private ArenaRun run;

        [Tooltip("The player whose walking, aim and dash input is paused while the game is paused.")]
        [SerializeField] private PlayerDash player;

        private PauseFlow flow;
        private PlayerMovement movement;
        private PlayerAim aim;
        private bool froze;

        /// <summary>Playing, or which paused view shows.</summary>
        public PauseState State => flow != null ? flow.State : PauseState.Playing;

        /// <summary>True while either paused view shows (game frozen, player input paused).</summary>
        public bool IsPaused => flow != null && flow.IsPaused;

        /// <summary>Raised with the new state on every change: pausing (game already frozen), switching view and resuming.</summary>
        public event Action<PauseState> StateChanged;

        private void Awake()
        {
            if (ending == null || run == null || player == null)
            {
                Debug.LogError($"{nameof(RunPause)} on '{name}' needs its ending, run and player assigned.", this);
                enabled = false;
                return;
            }

            movement = player.GetComponent<PlayerMovement>();
            aim = player.GetComponent<PlayerAim>();

            flow = new PauseFlow();
            flow.Paused += Freeze;
            flow.Resumed += Unfreeze;
            flow.StateChanged += OnStateChanged;
            run.VictoryReached += OnRunOver;
            run.DefeatReached += OnRunOver;
        }

        private void OnDestroy()
        {
            if (run != null)
            {
                run.VictoryReached -= OnRunOver;
                run.DefeatReached -= OnRunOver;
            }

            // However the scene is left (Menu, Quit, stopping Play Mode), never leave the game frozen behind it.
            if (froze)
            {
                Time.timeScale = 1f;
            }
        }

        private void LateUpdate()
        {
            if (!flow.RunIsOver && (ending.HasResult || run.RunState != RunState.Playing))
            {
                flow.ReportRunOver();
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                flow.PressEscape();
            }
        }

        /// <summary>The panel's Resume button. Returns false (and changes nothing) unless the buttons view shows.</summary>
        public bool Resume()
        {
            return flow != null && flow.PressResume();
        }

        /// <summary>The panel's Controls button. Returns false (and changes nothing) unless the buttons view shows.</summary>
        public bool ShowControls()
        {
            return flow != null && flow.PressControls();
        }

        /// <summary>The panel's Back button. Returns false (and changes nothing) unless the Controls view shows.</summary>
        public bool Back()
        {
            return flow != null && flow.PressBack();
        }

        /// <summary>
        /// The panel's Menu or Quit button. Only the first Menu or Quit while the buttons view shows is carried out
        /// (the game is unfrozen and the main menu loaded, or the game quits); returns false for every other call,
        /// which changes nothing.
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

        private void OnRunOver()
        {
            flow.ReportRunOver();
        }

        private void Freeze()
        {
            Time.timeScale = 0f;
            froze = true;
            SetPlayerPaused(true);
        }

        private void Unfreeze(ResumeCause cause)
        {
            SetPlayerPaused(false);

            // An ending that shows at once (Ending Delay 0) has already frozen the game itself; leave it frozen.
            if (!ending.IsShowing)
            {
                Time.timeScale = 1f;
            }

            froze = false;

            if (PauseFlow.LocksDashClicks(cause))
            {
                player.RestartStartLockout();
            }
        }

        private void SetPlayerPaused(bool paused)
        {
            player.IsPaused = paused;
            if (movement != null)
            {
                movement.IsPaused = paused;
            }

            if (aim != null)
            {
                aim.IsPaused = paused;
            }
        }

        private void OnStateChanged(PauseState state)
        {
            StateChanged?.Invoke(state);
        }
    }
}
