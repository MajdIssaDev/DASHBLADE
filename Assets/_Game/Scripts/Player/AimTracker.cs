using UnityEngine;

namespace Dashblade.Player
{
    /// <summary>
    /// Keeps the player's aim direction. The aim snaps straight to the cursor, except when the
    /// cursor is inside the dead zone around the player's centre or no cursor position is known;
    /// then the last direction is kept. Until a usable aim exists the player faces right (+X).
    /// </summary>
    public sealed class AimTracker
    {
        public Vector2 Direction { get; private set; } = Vector2.right;

        /// <summary>Facing as a Z rotation in degrees (0 = +X, 90 = +Y), in the range (-180, 180].</summary>
        public float AngleDegrees => Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;

        /// <summary>
        /// Recalculates the aim. Call every frame, even when the cursor has not moved,
        /// because the player may have moved.
        /// </summary>
        /// <param name="playerCentre">The player's centre in world units.</param>
        /// <param name="cursorWorld">The cursor in world units, or null when no position is reported.</param>
        /// <param name="deadZone">Distance in world units below which the last facing is kept.</param>
        public void Update(Vector2 playerCentre, Vector2? cursorWorld, float deadZone)
        {
            if (!cursorWorld.HasValue)
            {
                return;
            }

            Vector2 offset = cursorWorld.Value - playerCentre;
            float distance = offset.magnitude;
            if (distance < deadZone || distance <= 0f)
            {
                return;
            }

            Direction = offset / distance;
        }
    }
}
