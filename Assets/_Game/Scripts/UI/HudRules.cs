using UnityEngine;

namespace Dashblade.UI
{
    /// <summary>
    /// What the in-run HUD (F07) shows for a given game state, without any scene objects. <see cref="HudView"/> only
    /// applies these answers to its squares, text and bar.
    /// </summary>
    public static class HudRules
    {
        /// <summary>
        /// Whether heart square <paramref name="slot"/> (0 = left-most) is solid for <paramref name="hearts"/> hearts
        /// left. Squares fill from the left, so hearts are lost from the right-most square leftwards and all come back
        /// together when the hearts are restored. Out-of-range values show as hollow (below 0) or all solid (above the
        /// square count); the number of squares never changes.
        /// </summary>
        public static bool IsHeartSolid(int slot, int hearts)
        {
            return slot >= 0 && slot < hearts;
        }

        /// <summary>The centre text, for example "Wave 1 / 3".</summary>
        public static string WaveText(int currentWave, int waveCount)
        {
            return $"Wave {currentWave} / {waveCount}";
        }

        /// <summary>
        /// Share of the dash bar that is filled, 0 (empty) to 1 (full), from the dash's own recharge progress. The HUD
        /// keeps no timer of its own: this only clamps the value it is given.
        /// </summary>
        public static float DashBarFill(float rechargeProgress)
        {
            return Mathf.Clamp01(rechargeProgress);
        }
    }
}
