using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// The straight path of one dash, decided in full at the click: where it starts, which way it goes and how far.
    /// </summary>
    public readonly struct DashPath
    {
        public DashPath(Vector2 start, Vector2 direction, float length)
        {
            Start = start;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            Length = Mathf.Max(0f, length);
            End = Start + Direction * Length;
        }

        public Vector2 Start { get; }

        /// <summary>Unit vector along the path.</summary>
        public Vector2 Direction { get; }

        public float Length { get; }

        /// <summary>The exact point where the dash stops.</summary>
        public Vector2 End { get; }

        /// <summary>
        /// The body's centre after covering <paramref name="travelled"/> units. Clamped to the path, and exactly
        /// <see cref="End"/> once the whole length is covered, so the dash never overshoots.
        /// </summary>
        public Vector2 PointAt(float travelled)
        {
            if (travelled >= Length)
            {
                return End;
            }

            return travelled <= 0f ? Start : Start + Direction * travelled;
        }
    }

    /// <summary>
    /// Decides which wall hits found by the dash's body cast actually stop the dash.
    /// </summary>
    public static class DashWallRules
    {
        /// <summary>Below this dot product the dash is treated as running along a touched surface, not into it.</summary>
        public const float IntoSurfaceTolerance = 1e-4f;

        /// <summary>
        /// Decides whether one wall hit stops the dash, and where.
        /// A hit further away than <paramref name="startContactDistance"/> stops the dash <paramref name="skin"/> short
        /// of the reported hit distance (the physics cast reports a point very slightly inside the wall; the skin makes
        /// the body end just touching it, where walking into the wall also rests).
        /// A hit within <paramref name="startContactDistance"/> is a surface the body already touches: it stops the dash
        /// at distance 0 if the dash heads into it, and does not stop it at all if the dash runs along it or away from
        /// it (for a convex wall the body then never gets closer to it).
        /// </summary>
        /// <param name="hitDistance">Distance along the path at which the body cast touched the wall.</param>
        /// <param name="startContactDistance">Hits this close to the start count as already touching.</param>
        /// <param name="skin">How far before the reported hit the body stops.</param>
        /// <param name="dashDirection">Unit vector of the dash.</param>
        /// <param name="towardSurface">From the body's centre to the closest point of the wall, at the dash start.</param>
        /// <param name="stopDistance">Where the dash stops because of this wall, when the method returns true.</param>
        public static bool TryGetStop(
            float hitDistance,
            float startContactDistance,
            float skin,
            Vector2 dashDirection,
            Vector2 towardSurface,
            out float stopDistance)
        {
            if (hitDistance > startContactDistance)
            {
                stopDistance = Mathf.Max(0f, hitDistance - skin);
                return true;
            }

            stopDistance = 0f;
            return StartContactBlocks(dashDirection, towardSurface);
        }

        /// <summary>
        /// True if moving along <paramref name="dashDirection"/> brings the body closer to a surface it touches at the
        /// start. A zero <paramref name="towardSurface"/> (centre inside the wall) always blocks.
        /// </summary>
        public static bool StartContactBlocks(Vector2 dashDirection, Vector2 towardSurface)
        {
            if (towardSurface.sqrMagnitude <= 1e-12f)
            {
                return true;
            }

            return Vector2.Dot(dashDirection, towardSurface.normalized) > IntoSurfaceTolerance;
        }
    }
}
