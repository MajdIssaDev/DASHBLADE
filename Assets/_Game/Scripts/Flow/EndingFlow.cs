using System;

namespace Dashblade.Flow
{
    /// <summary>
    /// The end-of-run rules (F06), advanced by game time. The first result reported (victory or defeat) decides the
    /// ending and never changes; later reports are ignored. Ending Delay seconds of game time after the result the
    /// ending shows: <see cref="Shown"/> is raised once, and from that moment the game is meant to be frozen with the
    /// player's input off (the scene wrapper does that). While it shows, exactly one of Retry, Menu or Quit is
    /// accepted; Play is not offered here.
    /// </summary>
    /// <remarks>
    /// The caller decides which frame's time counts. <see cref="RunEnding"/> does not tick on the frame of the result,
    /// because that frame's time had already passed when the run ended, so the panel appears on the first frame
    /// whose summed game time since the result frame reaches the delay.
    /// </remarks>
    public sealed class EndingFlow
    {
        // A boundary counts as reached when it is this close (seconds). It absorbs float rounding in summed frame
        // times, so for example 60 steps of 1/60 s reach 1 second.
        private const float BoundaryEpsilon = 0.00001f;

        private readonly ChoiceLatch latch = new ChoiceLatch(MenuChoice.Retry, MenuChoice.Menu, MenuChoice.Quit);

        /// <param name="endingDelay">Seconds of game time from the result to the panel (spec: 1). 0 shows it at once.</param>
        public EndingFlow(float endingDelay)
        {
            if (!(endingDelay >= 0f) || float.IsInfinity(endingDelay))
            {
                throw new ArgumentOutOfRangeException(nameof(endingDelay), endingDelay, "Must be 0 or more.");
            }

            EndingDelay = endingDelay;
        }

        public float EndingDelay { get; }

        /// <summary>True once a result has been reported.</summary>
        public bool HasResult { get; private set; }

        /// <summary>The first result reported; meaningful only when <see cref="HasResult"/> is true.</summary>
        public EndingResult Result { get; private set; }

        /// <summary>Game time counted since the result (stops counting once the ending shows).</summary>
        public float TimeSinceResult { get; private set; }

        /// <summary>True from the moment the ending shows; never turns false again.</summary>
        public bool IsShowing { get; private set; }

        /// <summary>True once a button's choice has been accepted.</summary>
        public bool HasChosen => latch.HasChosen;

        /// <summary>The accepted choice; meaningful only when <see cref="HasChosen"/> is true.</summary>
        public MenuChoice Choice => latch.Choice;

        /// <summary>Raised once, with the result, the moment the ending shows.</summary>
        public event Action<EndingResult> Shown;

        /// <summary>
        /// The run has ended. Returns true if this is the first result (it decides the ending); every later report
        /// returns false and changes nothing. With an Ending Delay of 0 the ending shows at once.
        /// </summary>
        public bool ReportResult(EndingResult result)
        {
            if (HasResult)
            {
                return false;
            }

            HasResult = true;
            Result = result;
            TimeSinceResult = 0f;

            if (EndingDelay <= 0f)
            {
                Show();
            }

            return true;
        }

        /// <summary>Advances game time. Does nothing before a result, once showing, or for steps of 0 or less.</summary>
        public void Tick(float deltaTime)
        {
            if (!HasResult || IsShowing || !(deltaTime > 0f))
            {
                return;
            }

            TimeSinceResult += deltaTime;
            if (TimeSinceResult >= EndingDelay - BoundaryEpsilon)
            {
                Show();
            }
        }

        /// <summary>
        /// A button was clicked. Returns true only for the first Retry, Menu or Quit while the ending shows; the
        /// caller then carries the choice out. Every later click, and any click before the ending shows, returns false.
        /// </summary>
        public bool TryChoose(MenuChoice choice)
        {
            return IsShowing && latch.TryChoose(choice);
        }

        private void Show()
        {
            IsShowing = true;
            Shown?.Invoke(Result);
        }
    }
}
