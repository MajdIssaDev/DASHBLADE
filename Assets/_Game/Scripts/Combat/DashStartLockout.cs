namespace Dashblade.Combat
{
    /// <summary>
    /// The dash's start lockout (F06 criterion 14): for the first <see cref="Duration"/> seconds of a run a left-click
    /// does not start a dash, so the second press of a double-click on Play or Retry that lands in the new run is
    /// dropped. The click is not remembered: once the lockout is over only a new press dashes. The lockout does not
    /// touch the dash's readiness (the dash stays ready and its bar full), walking or aim. Time is game time counted
    /// from the first frame the run is shown, passed in through <see cref="Tick"/> (see
    /// <see cref="Dashblade.Common.RunClock"/>). A duration of 0 means no lockout.
    /// </summary>
    public sealed class DashStartLockout
    {
        // As in DashRules: frame times summed by float drift slightly (30 frames of 1/60 s can add up to just under
        // 0.5 s), so a remainder this small counts as over and the lockout never ends a frame late.
        private const float TimeEpsilon = 1e-5f;

        /// <param name="duration">Seconds of the lockout (spec: Start Lockout = 0.5). 0 or less (or NaN) means none.</param>
        public DashStartLockout(float duration)
        {
            Duration = duration > 0f ? duration : 0f;
        }

        /// <summary>Length of the lockout in seconds.</summary>
        public float Duration { get; }

        /// <summary>Game time of the run counted so far, in seconds (it stops counting once the lockout is over).</summary>
        public float Elapsed { get; private set; }

        /// <summary>True while a left-click must not start a dash.</summary>
        public bool IsLocked => Elapsed < Duration - TimeEpsilon;

        /// <summary>Advances game time. Steps of 0 or less are ignored.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime > 0f && IsLocked)
            {
                Elapsed += deltaTime;
            }
        }
    }
}
