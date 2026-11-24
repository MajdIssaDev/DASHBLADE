using System;

namespace Dashblade.Combat
{
    /// <summary>
    /// The protection blink: the circle alternates between half-transparent and solid every blink interval,
    /// starting half-transparent the moment protection starts (so the hit shows at once), and is solid again
    /// from the moment protection ends.
    /// </summary>
    public static class ProtectionBlink
    {
        public const float SolidAlpha = 1f;
        public const float FadedAlpha = 0.5f;

        /// <summary>
        /// Opacity <paramref name="timeSinceProtectionStarted"/> seconds into a protection lasting
        /// <paramref name="protectionTime"/> seconds: 0.5 for the first interval, 1 for the next, and so on;
        /// 1 once the protection time is used up, before it starts (negative time), or when the interval is not positive.
        /// </summary>
        public static float Alpha(float timeSinceProtectionStarted, float protectionTime, float blinkInterval)
        {
            if (timeSinceProtectionStarted < 0f || timeSinceProtectionStarted >= protectionTime || blinkInterval <= 0f)
            {
                return SolidAlpha;
            }

            long phase = (long)Math.Floor(timeSinceProtectionStarted / blinkInterval);
            return phase % 2 == 0 ? FadedAlpha : SolidAlpha;
        }
    }
}
