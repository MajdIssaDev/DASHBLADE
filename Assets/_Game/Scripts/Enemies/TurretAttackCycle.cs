using System;

namespace Dashblade.Enemies
{
    /// <summary>
    /// The Turret's attack cycle, advanced by game time:
    /// first-attack delay, then a closing ring (warning), then exactly one shot the moment the
    /// ring closes, then rest, then the ring again, forever. With the spec's numbers that is one
    /// shot every 0.8 + 1.5 = 2.3 seconds.
    /// <para>
    /// Time steps of any size are fine: a step that crosses one or more phase boundaries
    /// carries its leftover time into the next phase, so the cycle does not drift, a ring is
    /// never skipped and no ring fires twice.
    /// </para>
    /// </summary>
    public sealed class TurretAttackCycle
    {
        // A boundary counts as reached when it is this close (seconds). It absorbs float
        // rounding, so for example sixty 1/60 s steps reach 1 second.
        private const float BoundaryEpsilon = 0.00001f;

        private readonly float firstAttackDelay;
        private readonly float warningTime;
        private readonly float restTime;
        private readonly float ringStartWidth;
        private readonly float ringEndWidth;

        /// <param name="firstAttackDelay">Seconds from spawn to the first ring (spec: 1).</param>
        /// <param name="warningTime">Seconds the ring takes to close (spec: 0.8). Must be above 0.</param>
        /// <param name="restTime">Seconds from a shot to the next ring (spec: 1.5).</param>
        /// <param name="ringStartWidth">Ring width in units when it appears (spec: 3).</param>
        /// <param name="ringEndWidth">Ring width in units when it closes: the Turret's own width (spec: 1).</param>
        public TurretAttackCycle(
            float firstAttackDelay, float warningTime, float restTime, float ringStartWidth, float ringEndWidth)
        {
            if (!(firstAttackDelay >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(firstAttackDelay), firstAttackDelay, "Must be 0 or more.");
            }

            if (!(warningTime > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(warningTime), warningTime, "Must be above 0: every shot is announced.");
            }

            if (!(restTime >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(restTime), restTime, "Must be 0 or more.");
            }

            if (!(ringStartWidth >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(ringStartWidth), ringStartWidth, "Must be 0 or more.");
            }

            if (!(ringEndWidth >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(ringEndWidth), ringEndWidth, "Must be 0 or more.");
            }

            this.firstAttackDelay = firstAttackDelay;
            this.warningTime = warningTime;
            this.restTime = restTime;
            this.ringStartWidth = ringStartWidth;
            this.ringEndWidth = ringEndWidth;
        }

        /// <summary>Raised whenever the phase changes, with the new phase.</summary>
        public event Action<TurretPhase> PhaseChanged;

        /// <summary>
        /// Raised once per ring, inside <see cref="Tick"/>, on the step in which the ring closes
        /// (after the phase has become <see cref="TurretPhase.Resting"/>). The wrapper reads the
        /// player's centre now and launches one projectile towards it.
        /// </summary>
        public event Action ShotFired;

        public TurretPhase Phase { get; private set; } = TurretPhase.WaitingForFirstAttack;

        /// <summary>Seconds spent in the current phase, including leftover carried over from the step that entered it.</summary>
        public float TimeInPhase { get; private set; }

        public bool IsPlayerDefeated { get; private set; }

        public bool IsDestroyed => Phase == TurretPhase.Destroyed;

        /// <summary>True while the warning ring is shown.</summary>
        public bool IsRingShown => Phase == TurretPhase.Warning;

        /// <summary>
        /// The ring's width in units now: Ring Start Width when it appears, shrinking linearly to
        /// the end width as it closes. 0 when no ring is shown.
        /// </summary>
        public float RingWidth
        {
            get
            {
                if (Phase != TurretPhase.Warning)
                {
                    return 0f;
                }

                float t = TimeInPhase / warningTime;
                t = t < 0f ? 0f : (t > 1f ? 1f : t);
                return ringStartWidth + (ringEndWidth - ringStartWidth) * t;
            }
        }

        /// <summary>
        /// Advances the cycle by game time. Call once per frame. Steps of 0 or less are ignored.
        /// </summary>
        /// <returns>
        /// How many shots were fired during this step: normally 0 or 1; more only if a single
        /// step spans several whole cycles. <see cref="ShotFired"/> is raised once per shot.
        /// </returns>
        public int Tick(float deltaTime)
        {
            if (!(deltaTime > 0f) || Phase == TurretPhase.Destroyed)
            {
                return 0;
            }

            TimeInPhase += deltaTime;

            int shots = 0;
            while (true)
            {
                switch (Phase)
                {
                    case TurretPhase.WaitingForFirstAttack:
                        if (!Reached(firstAttackDelay))
                        {
                            return shots;
                        }

                        Enter(TurretPhase.Warning, Leftover(firstAttackDelay));
                        break;

                    case TurretPhase.Warning:
                        if (!Reached(warningTime))
                        {
                            return shots;
                        }

                        Enter(TurretPhase.Resting, Leftover(warningTime));
                        shots++;
                        ShotFired?.Invoke();
                        break;

                    case TurretPhase.Resting:
                        if (!Reached(restTime))
                        {
                            return shots;
                        }

                        Enter(TurretPhase.Warning, Leftover(restTime));
                        break;

                    default:
                        // Idle and Destroyed never change.
                        return shots;
                }
            }
        }

        /// <summary>Killed: everything stops at once, the ring vanishes and no shot is fired.</summary>
        public void Destroy()
        {
            if (Phase == TurretPhase.Destroyed)
            {
                return;
            }

            Enter(TurretPhase.Destroyed, 0f);
        }

        /// <summary>The player is at 0 hearts: no new ring starts, a showing ring vanishes and no shot is fired.</summary>
        public void OnPlayerDefeated()
        {
            IsPlayerDefeated = true;

            switch (Phase)
            {
                case TurretPhase.WaitingForFirstAttack:
                case TurretPhase.Warning:
                case TurretPhase.Resting:
                    Enter(TurretPhase.Idle, 0f);
                    break;
            }
        }

        private void Enter(TurretPhase phase, float timeAlreadySpent)
        {
            Phase = phase;
            TimeInPhase = timeAlreadySpent;
            PhaseChanged?.Invoke(phase);
        }

        private bool Reached(float duration) => TimeInPhase >= duration - BoundaryEpsilon;

        private float Leftover(float duration) => Math.Max(0f, TimeInPhase - duration);
    }
}
