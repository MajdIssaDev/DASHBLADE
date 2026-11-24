using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// Exact first contact between a point moving along a straight segment and a still circle. A moving circle
    /// against a still circle is the same question with the two radii added together. Used for things that move
    /// themselves each frame (a charging Charger, a projectile), so nothing is skipped at any speed or frame rate.
    /// </summary>
    public static class SweptCircle
    {
        /// <summary>
        /// Distance along the segment at which the moving point first comes within <paramref name="radius"/> of
        /// <paramref name="centre"/> (touching counts). 0 if it starts within it.
        /// </summary>
        /// <param name="start">Where the moving point starts.</param>
        /// <param name="direction">Unit vector of the movement.</param>
        /// <param name="length">How far it moves; 0 checks the start point only.</param>
        /// <param name="centre">Centre of the still circle.</param>
        /// <param name="radius">Radius of the still circle (for two circles: the sum of their radii).</param>
        /// <param name="distance">Where along the segment the first contact is, when the method returns true.</param>
        /// <returns>True if the point touches the circle anywhere on the segment, both ends included.</returns>
        public static bool TryFirstContact(
            Vector2 start, Vector2 direction, float length, Vector2 centre, float radius, out float distance)
        {
            distance = 0f;
            if (!(radius >= 0f))
            {
                return false;
            }

            Vector2 fromCentre = start - centre;
            float c = fromCentre.sqrMagnitude - radius * radius;
            if (c <= 0f)
            {
                return true;
            }

            float b = Vector2.Dot(fromCentre, direction);
            if (!(length > 0f) || b >= 0f)
            {
                // Not moving, or moving away from (or square past) the circle.
                return false;
            }

            float discriminant = b * b - c;
            if (discriminant < 0f)
            {
                return false;
            }

            float first = -b - Mathf.Sqrt(discriminant);
            if (first > length)
            {
                return false;
            }

            distance = Mathf.Max(0f, first);
            return true;
        }
    }
}
