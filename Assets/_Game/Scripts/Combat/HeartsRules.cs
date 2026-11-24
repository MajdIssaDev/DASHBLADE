using System;

namespace Dashblade.Combat
{
    /// <summary>
    /// The player's hearts and damage protection, without any scene objects.
    /// Starts at max hearts. A hit that lands removes exactly one heart and starts protection; while protected,
    /// while the caller reports the player as invulnerable (dashing), or once defeated, hits are ignored, and an
    /// ignored hit never starts, restarts or extends protection. The hit that takes the last heart defeats the
    /// player instead of protecting them. All times are game time passed in through <see cref="Tick"/>.
    /// </summary>
    /// <remarks>
    /// Frame order: call <see cref="Tick"/> once per frame first, then <see cref="TryHit"/> for every danger
    /// touching the player that frame. Protection only ever ends inside <see cref="Tick"/>, so any number of hits
    /// between two Ticks (the same frame) costs at most one heart.
    /// <para>
    /// Leftover time: when protection runs out partway through a frame, the part of that frame after the end is
    /// kept. If a hit lands before the next <see cref="Tick"/> (the player is still in danger on the frame
    /// protection ends), the new protection starts with that leftover already elapsed, so for a caller that
    /// attempts a hit every frame, hit k lands on the first frame at or after k × Protection Time from the first
    /// hit, at any frame rate, with no drift. If no hit lands on that frame, the leftover is thrown away and the
    /// next hit gets a full Protection Time.
    /// </para>
    /// </remarks>
    public sealed class HeartsRules
    {
        // Timers summed from frame times drift by float rounding (45 frames of 1/60 s can add up to just under
        // 0.75 s). Treating a remainder this small as used up keeps protection from ending a frame late, as in
        // DashRules.
        private const float TimeEpsilon = 1e-5f;

        private float carriedTime;

        /// <param name="maxHearts">Hearts at the start and after a restore. Values below 1 are treated as 1.</param>
        /// <param name="protectionTime">Seconds of protection after a hit that does not defeat. Negative is treated as 0.</param>
        public HeartsRules(int maxHearts, float protectionTime)
        {
            MaxHearts = Math.Max(1, maxHearts);
            ProtectionTime = Math.Max(0f, protectionTime);
            Hearts = MaxHearts;
        }

        public int MaxHearts { get; }

        public float ProtectionTime { get; }

        /// <summary>Hearts left, from <see cref="MaxHearts"/> down to 0.</summary>
        public int Hearts { get; private set; }

        /// <summary>True once the last heart is lost. Nothing changes the hearts after that.</summary>
        public bool IsDefeated => Hearts == 0;

        /// <summary>True from the hit that started protection until the <see cref="Tick"/> in which it runs out.</summary>
        public bool IsProtected { get; private set; }

        /// <summary>Seconds of game time since the current protection started; 0 when not protected.</summary>
        public float ProtectionElapsed { get; private set; }

        /// <summary>Raised with the new value whenever <see cref="Hearts"/> changes (a hit or a restore).</summary>
        public event Action<int> HeartsChanged;

        /// <summary>Raised once for every hit that lands, including the final one. Raised after <see cref="HeartsChanged"/>.</summary>
        public event Action Damaged;

        /// <summary>Raised once, after <see cref="Damaged"/>, when the final hit lands.</summary>
        public event Action Defeated;

        /// <summary>Raised with the new value whenever <see cref="IsProtected"/> changes.</summary>
        public event Action<bool> ProtectionChanged;

        /// <summary>
        /// Advances game time: runs the protection down and ends it once <see cref="ProtectionTime"/> is used up.
        /// Keeps running while the player dashes. Zero or negative delta time changes nothing.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            carriedTime = 0f;
            if (!IsProtected)
            {
                return;
            }

            ProtectionElapsed += deltaTime;
            if (ProtectionElapsed < ProtectionTime - TimeEpsilon)
            {
                return;
            }

            // Leftover of this frame after protection ran out; used only by a hit before the next Tick.
            carriedTime = Math.Max(0f, Math.Min(ProtectionElapsed - ProtectionTime, ProtectionTime));
            IsProtected = false;
            ProtectionElapsed = 0f;
            ProtectionChanged?.Invoke(false);
        }

        /// <summary>
        /// Something dangerous touches the player. Lands only if the player is not defeated, not invulnerable and
        /// not protected; then removes exactly one heart and either starts protection or, on the last heart, defeats.
        /// Returns true if the hit landed. An ignored hit changes nothing.
        /// </summary>
        /// <param name="isInvulnerable">True while the player is dashing (the dash reports this; F02).</param>
        public bool TryHit(bool isInvulnerable)
        {
            if (IsDefeated || isInvulnerable || IsProtected)
            {
                return false;
            }

            Hearts -= 1;
            bool defeated = Hearts == 0;
            if (!defeated)
            {
                IsProtected = true;
                ProtectionElapsed = carriedTime;
            }

            carriedTime = 0f;

            HeartsChanged?.Invoke(Hearts);
            Damaged?.Invoke();
            if (defeated)
            {
                Defeated?.Invoke();
            }
            else
            {
                ProtectionChanged?.Invoke(true);
            }

            return true;
        }

        /// <summary>
        /// Sets the hearts back to <see cref="MaxHearts"/>. Does nothing at max hearts or once defeated.
        /// A running protection keeps its remaining time. Returns true if the hearts changed.
        /// </summary>
        public bool Restore()
        {
            if (IsDefeated || Hearts == MaxHearts)
            {
                return false;
            }

            Hearts = MaxHearts;
            HeartsChanged?.Invoke(Hearts);
            return true;
        }

        /// <summary>The circle's opacity this frame: blinking while protected (see <see cref="ProtectionBlink"/>), otherwise solid.</summary>
        public float BodyAlpha(float blinkInterval)
        {
            return IsProtected ? ProtectionBlink.Alpha(ProtectionElapsed, ProtectionTime, blinkInterval) : ProtectionBlink.SolidAlpha;
        }
    }
}
