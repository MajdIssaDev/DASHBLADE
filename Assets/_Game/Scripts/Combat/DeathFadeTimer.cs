using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Tracks a destroyed enemy's fade: it can start only once, then runs on game time until the fade time is used up.
    /// </summary>
    public sealed class DeathFadeTimer
    {
        public bool IsStarted { get; private set; }

        /// <summary>Seconds of game time since the fade started.</summary>
        public float Elapsed { get; private set; }

        /// <summary>Starts the fade. Returns false if it had already started (the enemy is already dead).</summary>
        public bool TryStart()
        {
            if (IsStarted)
            {
                return false;
            }

            IsStarted = true;
            Elapsed = 0f;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (IsStarted && deltaTime > 0f)
            {
                Elapsed += deltaTime;
            }
        }

        /// <summary>Opacity multiplier: 1 before the fade, falling linearly to 0 at <paramref name="fadeTime"/>.</summary>
        public float Alpha(float fadeTime)
        {
            if (!IsStarted)
            {
                return 1f;
            }

            return fadeTime <= 0f ? 0f : Mathf.Clamp01(1f - Elapsed / fadeTime);
        }

        public bool IsFinished(float fadeTime)
        {
            // The small allowance absorbs float rounding in summed frame times, so the fade never runs a frame long.
            return IsStarted && Elapsed >= fadeTime - 1e-5f;
        }
    }
}
