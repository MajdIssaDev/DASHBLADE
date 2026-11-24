using System;
using UnityEngine;

namespace Dashblade.Combat
{
    public enum DashState
    {
        Ready,
        Dashing,
        Cooldown,
    }

    /// <summary>
    /// The dash's rules, without any scene objects:
    /// Ready → (click) → Dashing → (kill during the dash) → Ready, or (no kill) → Cooldown → Ready.
    /// Clicks while not Ready are ignored and never extend the cooldown. Kills do not stack: readiness is yes or no.
    /// All times are game time passed in through <see cref="Tick"/>.
    /// </summary>
    public sealed class DashRules
    {
        // Timers summed from frame times drift by float rounding (60 frames of 1/60 s can add up to just under 1 s).
        // Treating a remainder this small as zero keeps the dash and cooldown from ending a frame late.
        private const float TimeEpsilon = 1e-5f;

        public DashState State { get; private set; } = DashState.Ready;

        public bool IsReady => State == DashState.Ready;

        /// <summary>True from the moment a dash starts to the moment it ends.</summary>
        public bool IsInvulnerable => State == DashState.Dashing;

        /// <summary>Length of the current dash's path, in world units.</summary>
        public float PathLength { get; private set; }

        /// <summary>Seconds the current dash takes to cover <see cref="PathLength"/>.</summary>
        public float DashTime { get; private set; }

        /// <summary>Seconds since the current dash started.</summary>
        public float Elapsed { get; private set; }

        public float CooldownRemaining { get; private set; }

        public bool KilledThisDash { get; private set; }

        /// <summary>Distance covered so far along the current dash's path; exactly <see cref="PathLength"/> once time is up.</summary>
        public float Travelled
        {
            get
            {
                if (State != DashState.Dashing)
                {
                    return 0f;
                }

                return DashTime <= 0f ? PathLength : PathLength * Mathf.Min(1f, Elapsed / DashTime);
            }
        }

        /// <summary>True when the current dash has used up its time and should be ended with <see cref="EndDash"/>.</summary>
        public bool IsDashComplete => State == DashState.Dashing && Elapsed >= DashTime - TimeEpsilon;

        /// <summary>Raised with the new value whenever <see cref="IsReady"/> changes.</summary>
        public event Action<bool> ReadinessChanged;

        /// <summary>
        /// Time a dash of <paramref name="pathLength"/> takes at the constant speed fullDistance / fullDuration.
        /// A dash shortened by a wall is therefore shorter in time too, never slower.
        /// </summary>
        public static float DashTimeFor(float pathLength, float fullDistance, float fullDuration)
        {
            if (pathLength <= 0f || fullDistance <= 0f || fullDuration <= 0f)
            {
                return 0f;
            }

            return fullDuration * (pathLength / fullDistance);
        }

        /// <summary>Starts a dash if one is ready. Returns false (and changes nothing) otherwise.</summary>
        public bool TryStartDash(float pathLength, float dashTime)
        {
            if (State != DashState.Ready)
            {
                return false;
            }

            PathLength = Mathf.Max(0f, pathLength);
            DashTime = Mathf.Max(0f, dashTime);
            Elapsed = 0f;
            KilledThisDash = false;
            SetState(DashState.Dashing);
            return true;
        }

        /// <summary>Advances game time. While dashing this only moves the dash on; it never ends it (see <see cref="EndDash"/>).</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            switch (State)
            {
                case DashState.Dashing:
                    Elapsed += deltaTime;
                    break;
                case DashState.Cooldown:
                    CooldownRemaining -= deltaTime;
                    if (CooldownRemaining <= TimeEpsilon)
                    {
                        CooldownRemaining = 0f;
                        SetState(DashState.Ready);
                    }

                    break;
            }
        }

        /// <summary>Records that the current dash destroyed an enemy. Any number of kills counts the same as one.</summary>
        public void RegisterKill()
        {
            if (State == DashState.Dashing)
            {
                KilledThisDash = true;
            }
        }

        /// <summary>
        /// Ends a dash whose time is up: Ready at once after a kill, otherwise Cooldown for <paramref name="missCooldown"/>
        /// seconds counted from the moment the dash's time ran out (any overshoot of the last frame counts towards it).
        /// Returns false and changes nothing if no dash is complete.
        /// </summary>
        public bool EndDash(float missCooldown)
        {
            if (!IsDashComplete)
            {
                return false;
            }

            float overshoot = Mathf.Max(0f, Elapsed - DashTime);
            if (KilledThisDash)
            {
                CooldownRemaining = 0f;
                SetState(DashState.Ready);
                return true;
            }

            CooldownRemaining = missCooldown - overshoot;
            if (CooldownRemaining <= TimeEpsilon)
            {
                CooldownRemaining = 0f;
                SetState(DashState.Ready);
            }
            else
            {
                SetState(DashState.Cooldown);
            }

            return true;
        }

        /// <summary>
        /// Stops the current dash at once, wherever it is (the dash component was turned off mid-dash). Kills already
        /// made count as usual: Ready after a kill, otherwise a full <paramref name="missCooldown"/>.
        /// Returns false and changes nothing if no dash is running.
        /// </summary>
        public bool CancelDash(float missCooldown)
        {
            if (State != DashState.Dashing)
            {
                return false;
            }

            if (KilledThisDash || missCooldown <= TimeEpsilon)
            {
                CooldownRemaining = 0f;
                SetState(DashState.Ready);
            }
            else
            {
                CooldownRemaining = missCooldown;
                SetState(DashState.Cooldown);
            }

            return true;
        }

        private void SetState(DashState next)
        {
            bool wasReady = IsReady;
            State = next;
            if (IsReady != wasReady)
            {
                ReadinessChanged?.Invoke(IsReady);
            }
        }
    }
}
