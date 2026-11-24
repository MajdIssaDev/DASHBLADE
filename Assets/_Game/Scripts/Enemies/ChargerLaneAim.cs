using System;

namespace Dashblade.Enemies
{
    /// <summary>
    /// Keeps the direction of the Charger's lane (and of the triangle's tip) as a unit vector.
    /// While the lane is tracking it points from the Charger's centre to the player's centre;
    /// if the player is on top of the Charger (centres closer than the on-top distance) the last
    /// direction is kept; while the lane is locked (or no lane is shown) the direction never changes.
    /// <para>Plain floats so the class has no engine dependency; X/Y are world axes.</para>
    /// </summary>
    public sealed class ChargerLaneAim
    {
        // Offsets shorter than this (units) are too short to give a direction at all.
        private const double MinimumUsableLength = 0.000001;

        private readonly double onTopDistance;

        /// <param name="initialDirectionX">Facing before the first warning, X part (for example the spawn rotation).</param>
        /// <param name="initialDirectionY">Facing before the first warning, Y part. A zero vector means +X.</param>
        /// <param name="onTopDistance">
        /// Centre distance (units) below which the player counts as on top of the Charger, so the lane keeps its
        /// last direction. The Charger passes the sum of the two body radii (the bodies overlap). 0 or less means
        /// only an exactly shared centre counts.
        /// </param>
        public ChargerLaneAim(float initialDirectionX, float initialDirectionY, float onTopDistance = 0f)
        {
            DirectionX = 1f;
            DirectionY = 0f;
            this.onTopDistance = onTopDistance > 0f ? onTopDistance : 0.0;
            SetIfLongerThan(initialDirectionX, initialDirectionY, MinimumUsableLength);
        }

        public float DirectionX { get; private set; }

        public float DirectionY { get; private set; }

        /// <summary>Facing as a Z rotation in degrees (0 = +X, 90 = +Y), in the range (-180, 180].</summary>
        public float AngleDegrees => (float)(Math.Atan2(DirectionY, DirectionX) * (180.0 / Math.PI));

        /// <summary>
        /// Recalculates the direction. Call every frame while a lane is shown, and once when a
        /// warning starts (with <paramref name="isTracking"/> true) so a new strip always starts
        /// pointing at the player.
        /// </summary>
        /// <param name="isTracking">True while the lane may still turn; false once it is locked.</param>
        public void Update(float chargerX, float chargerY, float playerX, float playerY, bool isTracking)
        {
            if (!isTracking)
            {
                return;
            }

            SetIfLongerThan(playerX - chargerX, playerY - chargerY, Math.Max(onTopDistance, MinimumUsableLength));
        }

        private void SetIfLongerThan(float x, float y, double minimumLength)
        {
            double length = Math.Sqrt((double)x * x + (double)y * y);
            if (!(length > minimumLength) || double.IsInfinity(length))
            {
                return;
            }

            DirectionX = (float)(x / length);
            DirectionY = (float)(y / length);
        }
    }
}
