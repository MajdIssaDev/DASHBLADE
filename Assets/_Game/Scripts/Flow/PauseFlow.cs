using System;

namespace Dashblade.Flow
{
    /// <summary>
    /// The pause rules (F11), without any scene objects. Three states: <see cref="PauseState.Playing"/>, paused on
    /// the buttons view (<see cref="PauseState.Buttons"/>) and paused on the Controls view
    /// (<see cref="PauseState.Controls"/>).
    /// <list type="bullet">
    /// <item>Esc pauses while playing (always opening on the buttons view) and resumes from either view.</item>
    /// <item>Resume (buttons view) resumes; the dash then ignores left-clicks for its Start Lockout
    /// (<see cref="LocksDashClicks"/>). Resuming with Esc has no lockout.</item>
    /// <item>Controls (buttons view) opens the Controls view; Back (Controls view) returns to the buttons view.</item>
    /// <item>Menu and Quit (buttons view): only the first one is accepted; after it every input is ignored.</item>
    /// <item>Once the run has a result (<see cref="ReportRunOver"/>) Esc never pauses again, and a pause still
    /// open at that moment ends at once: the ending wins.</item>
    /// </list>
    /// Every call that is not allowed in the current state returns false and changes nothing. Freezing the game
    /// and showing the panel are left to the scene wrapper and the UI, which listen to the events.
    /// </summary>
    public sealed class PauseFlow
    {
        private readonly ChoiceLatch latch = new ChoiceLatch(MenuChoice.Menu, MenuChoice.Quit);

        public PauseState State { get; private set; } = PauseState.Playing;

        /// <summary>True on either paused view.</summary>
        public bool IsPaused => State != PauseState.Playing;

        /// <summary>True once the run has a result; from then on the game is never paused.</summary>
        public bool RunIsOver { get; private set; }

        /// <summary>True once Menu or Quit has been accepted.</summary>
        public bool HasChosen => latch.HasChosen;

        /// <summary>The accepted choice; meaningful only when <see cref="HasChosen"/> is true.</summary>
        public MenuChoice Choice => latch.Choice;

        /// <summary>Raised when the game pauses (before <see cref="StateChanged"/>).</summary>
        public event Action Paused;

        /// <summary>Raised with the reason when the game runs again (before <see cref="StateChanged"/>).</summary>
        public event Action<ResumeCause> Resumed;

        /// <summary>Raised with the new state on every change: pausing, switching view and resuming.</summary>
        public event Action<PauseState> StateChanged;

        /// <summary>True if a resume for <paramref name="cause"/> makes the dash ignore left-clicks for its Start Lockout.</summary>
        public static bool LocksDashClicks(ResumeCause cause) => cause == ResumeCause.ResumeButton;

        /// <summary>
        /// Esc was pressed (once per press; holding it is not a new press). Pauses while playing, resumes from either
        /// paused view. Returns false and changes nothing once the run is over or Menu or Quit has been accepted.
        /// </summary>
        public bool PressEscape()
        {
            if (RunIsOver || HasChosen)
            {
                return false;
            }

            if (IsPaused)
            {
                Resume(ResumeCause.Escape);
            }
            else
            {
                State = PauseState.Buttons;
                Paused?.Invoke();
                StateChanged?.Invoke(State);
            }

            return true;
        }

        /// <summary>The Resume button: resumes from the buttons view only.</summary>
        public bool PressResume()
        {
            if (State != PauseState.Buttons || HasChosen)
            {
                return false;
            }

            Resume(ResumeCause.ResumeButton);
            return true;
        }

        /// <summary>The Controls button: buttons view to the Controls view. The game stays paused.</summary>
        public bool PressControls()
        {
            return SwitchView(PauseState.Buttons, PauseState.Controls);
        }

        /// <summary>The Back button: Controls view to the buttons view. The game stays paused.</summary>
        public bool PressBack()
        {
            return SwitchView(PauseState.Controls, PauseState.Buttons);
        }

        /// <summary>
        /// The Menu or Quit button, on the buttons view. Returns true only for the first of them; the caller then
        /// carries the choice out (unfreezing the game first). Every later call, and every other choice, returns false.
        /// </summary>
        public bool TryChoose(MenuChoice choice)
        {
            return State == PauseState.Buttons && latch.TryChoose(choice);
        }

        /// <summary>
        /// The run has a result (victory or defeat). From now on Esc does nothing. A pause still open ends at once
        /// with <see cref="ResumeCause.RunOver"/>, unless Menu or Quit is already leaving the scene. Returns false if
        /// the run was already over.
        /// </summary>
        public bool ReportRunOver()
        {
            if (RunIsOver)
            {
                return false;
            }

            RunIsOver = true;
            if (IsPaused && !HasChosen)
            {
                Resume(ResumeCause.RunOver);
            }

            return true;
        }

        private bool SwitchView(PauseState from, PauseState to)
        {
            if (State != from || HasChosen)
            {
                return false;
            }

            State = to;
            StateChanged?.Invoke(State);
            return true;
        }

        private void Resume(ResumeCause cause)
        {
            State = PauseState.Playing;
            Resumed?.Invoke(cause);
            StateChanged?.Invoke(State);
        }
    }
}
