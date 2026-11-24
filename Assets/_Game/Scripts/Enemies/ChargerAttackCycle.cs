using System;

namespace Dashblade.Enemies
{
    /// <summary>
    /// The Charger's attack cycle, advanced by game time:
    /// first-attack delay, then warning (the lane tracks the player, then locks for the last
    /// Lock Time), then charging (until the wrapper reports the wall), then resting, then
    /// warning again, forever. Only a charging Charger hurts on contact.
    /// <para>
    /// Time steps of any size are fine: a step that crosses one or more phase boundaries
    /// carries its leftover time into the next phase, so the cycle does not drift.
    /// </para>
    /// </summary>
    public sealed class ChargerAttackCycle
    {
        // A boundary counts as reached when it is this close (seconds). It absorbs float
        // rounding, so for example sixty 1/60 s steps reach 1 second.
        private const float BoundaryEpsilon = 0.00001f;

        private readonly float firstAttackDelay;
        private readonly float warningTime;
        private readonly float lockTime;
        private readonly float restTime;

        private bool laneLocked;
        private bool isTicking;

        /// <param name="firstAttackDelay">Seconds from spawn to the first warning (spec: 1).</param>
        /// <param name="warningTime">Total seconds the lane is shown before the charge (spec: 1). Must be above 0.</param>
        /// <param name="lockTime">The last seconds of the warning during which the lane no longer tracks (spec: 0.4). 0..Warning Time.</param>
        /// <param name="restTime">Seconds of rest after a charge before the next warning (spec: 1.5).</param>
        public ChargerAttackCycle(float firstAttackDelay, float warningTime, float lockTime, float restTime)
        {
            if (!(firstAttackDelay >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(firstAttackDelay), firstAttackDelay, "Must be 0 or more.");
            }

            if (!(warningTime > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(warningTime), warningTime, "Must be above 0: every attack is announced.");
            }

            if (!(lockTime >= 0f) || lockTime > warningTime)
            {
                throw new ArgumentOutOfRangeException(nameof(lockTime), lockTime, "Must be between 0 and Warning Time.");
            }

            if (!(restTime >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(restTime), restTime, "Must be 0 or more.");
            }

            this.firstAttackDelay = firstAttackDelay;
            this.warningTime = warningTime;
            this.lockTime = lockTime;
            this.restTime = restTime;
        }

        /// <summary>Raised whenever the phase changes, with the new phase.</summary>
        public event Action<ChargerPhase> PhaseChanged;

        /// <summary>Raised once per warning, when the lane stops tracking and locks.</summary>
        public event Action LaneLocked;

        public ChargerPhase Phase { get; private set; } = ChargerPhase.WaitingForFirstAttack;

        /// <summary>Seconds spent in the current phase, including leftover carried over from the step that entered it.</summary>
        public float TimeInPhase { get; private set; }

        public bool IsPlayerDefeated { get; private set; }

        public bool IsDestroyed => Phase == ChargerPhase.Destroyed;

        /// <summary>True while the warning strip is shown.</summary>
        public bool IsLaneShown => Phase == ChargerPhase.Warning;

        /// <summary>True during the first part of the warning, while the lane turns to follow the player.</summary>
        public bool IsLaneTracking => Phase == ChargerPhase.Warning && !laneLocked;

        /// <summary>True during the last Lock Time of the warning, when the lane no longer turns.</summary>
        public bool IsLaneLocked => Phase == ChargerPhase.Warning && laneLocked;

        /// <summary>
        /// How far the solid bar has grown along the strip, 0..1. It is 0 when the strip appears
        /// and reaches 1 exactly when the charge starts. 0 when no lane is shown.
        /// </summary>
        public float WarningProgress =>
            Phase == ChargerPhase.Warning ? Clamp01(TimeInPhase / warningTime) : 0f;

        /// <summary>True only while charging: the body costs the player a heart on contact.</summary>
        public bool HurtsOnContact => Phase == ChargerPhase.Charging;

        /// <summary>Advances the cycle by game time. Call once per frame. Steps of 0 or less are ignored.</summary>
        public void Tick(float deltaTime)
        {
            if (!(deltaTime > 0f) || Phase == ChargerPhase.Destroyed)
            {
                return;
            }

            TimeInPhase += deltaTime;
            Advance();
        }

        /// <summary>
        /// The wrapper calls this when the charging body has reached the wall (its centre is
        /// 0.5 units from it). Ignored outside the charge. The rest begins, or, if the player is
        /// defeated, the Charger goes idle for good.
        /// <para>
        /// Called after <see cref="Tick"/>, pass in <paramref name="timePastArrival"/> the part of
        /// the last step that came after the body reached the wall (time charging minus length /
        /// speed); it counts towards the rest, so the rest is not a frame long.
        /// </para>
        /// <para>
        /// A charge that cannot move should be ended from a <see cref="PhaseChanged"/> handler
        /// for <see cref="ChargerPhase.Charging"/>: called from inside <see cref="Tick"/>, the
        /// leftover time of that step goes to the rest (the argument is then ignored), so the
        /// cycle does not drift.
        /// </para>
        /// </summary>
        public void ReportChargeEnded(float timePastArrival = 0f)
        {
            if (Phase != ChargerPhase.Charging)
            {
                return;
            }

            float carried = isTicking ? TimeInPhase : Math.Max(0f, Math.Min(timePastArrival, TimeInPhase));
            Enter(IsPlayerDefeated ? ChargerPhase.Idle : ChargerPhase.Resting, carried);

            if (!isTicking)
            {
                Advance();
            }
        }

        /// <summary>Killed: everything stops at once, the lane hides and the body is harmless.</summary>
        public void Destroy()
        {
            if (Phase == ChargerPhase.Destroyed)
            {
                return;
            }

            Enter(ChargerPhase.Destroyed, 0f);
        }

        /// <summary>
        /// The player is at 0 hearts: no new warning starts and a showing lane vanishes.
        /// A charge already running finishes, then the Charger stays idle.
        /// </summary>
        public void OnPlayerDefeated()
        {
            IsPlayerDefeated = true;

            switch (Phase)
            {
                case ChargerPhase.WaitingForFirstAttack:
                case ChargerPhase.Warning:
                case ChargerPhase.Resting:
                    Enter(ChargerPhase.Idle, 0f);
                    break;
            }
        }

        private void Advance()
        {
            bool wasTicking = isTicking;
            isTicking = true;
            try
            {
                while (true)
                {
                    switch (Phase)
                    {
                        case ChargerPhase.WaitingForFirstAttack:
                            if (!Reached(firstAttackDelay))
                            {
                                return;
                            }

                            Enter(ChargerPhase.Warning, Leftover(firstAttackDelay));
                            break;

                        case ChargerPhase.Warning:
                            if (!laneLocked && Reached(warningTime - lockTime))
                            {
                                laneLocked = true;
                                LaneLocked?.Invoke();
                                break; // re-check: a handler may have changed the phase
                            }

                            if (!Reached(warningTime))
                            {
                                return;
                            }

                            Enter(ChargerPhase.Charging, Leftover(warningTime));
                            break;

                        case ChargerPhase.Resting:
                            if (!Reached(restTime))
                            {
                                return;
                            }

                            Enter(ChargerPhase.Warning, Leftover(restTime));
                            break;

                        default:
                            // Charging ends only through ReportChargeEnded; Idle and Destroyed never change.
                            return;
                    }
                }
            }
            finally
            {
                isTicking = wasTicking;
            }
        }

        private void Enter(ChargerPhase phase, float timeAlreadySpent)
        {
            Phase = phase;
            TimeInPhase = timeAlreadySpent;
            if (phase == ChargerPhase.Warning)
            {
                laneLocked = false;
            }

            PhaseChanged?.Invoke(phase);
        }

        private bool Reached(float duration) => TimeInPhase >= duration - BoundaryEpsilon;

        private float Leftover(float duration) => Math.Max(0f, TimeInPhase - duration);

        private static float Clamp01(float value) => value < 0f ? 0f : (value > 1f ? 1f : value);
    }
}
