using System.Collections.Generic;
using UnityEngine;

namespace Dashblade.Combat
{
    /// <summary>
    /// What counts as a wall, for everything that stops at walls: the dash, a Charger's lane and charge, and
    /// projectiles. A wall is any solid (non-trigger) collider that is neither a <see cref="DashTarget"/> (enemies,
    /// projectiles, dummies) nor the player: the four walls, the door marker and, from F05, pillars.
    /// </summary>
    public static class WallQuery
    {
        /// <summary>True if <paramref name="other"/> is a wall.</summary>
        public static bool IsWall(Collider2D other)
        {
            return other != null
                && other.enabled
                && !other.isTrigger
                && !other.TryGetComponent(out DashTarget _)
                && !other.TryGetComponent(out PlayerHealth _);
        }

        /// <summary>A filter that leaves out triggers; <see cref="IsWall"/> still has to be checked on each hit.</summary>
        public static ContactFilter2D SolidFilter
        {
            get
            {
                ContactFilter2D filter = ContactFilter2D.noFilter;
                filter.useTriggers = false;
                return filter;
            }
        }

        /// <summary>
        /// Distance a circle moving from <paramref name="origin"/> along <paramref name="direction"/> can travel
        /// before it first touches a wall, at most <paramref name="maxDistance"/>, with the dash's rules
        /// (<see cref="DashWallRules.TryGetStop"/>): it stops <paramref name="skin"/> short of the reported hit, and a
        /// wall it already touches stops it at once only if it heads into that wall.
        /// </summary>
        public static float StopDistance(
            Vector2 origin,
            float radius,
            Vector2 direction,
            float maxDistance,
            float startContactDistance,
            float skin,
            List<RaycastHit2D> buffer)
        {
            float stop = maxDistance;
            int count = Physics2D.CircleCast(origin, radius, direction, SolidFilter, buffer, maxDistance);
            for (int i = 0; i < count; i++)
            {
                Collider2D other = buffer[i].collider;
                if (!IsWall(other))
                {
                    continue;
                }

                Vector2 towardSurface = other.ClosestPoint(origin) - origin;
                if (DashWallRules.TryGetStop(buffer[i].distance, startContactDistance, skin, direction, towardSurface, out float wallStop)
                    && wallStop < stop)
                {
                    stop = wallStop;
                }
            }

            buffer.Clear();
            return Mathf.Max(0f, stop);
        }

        /// <summary>
        /// Distance at which a circle moving from <paramref name="origin"/> along <paramref name="direction"/> first
        /// touches a wall, within <paramref name="maxDistance"/>. 0 if it already touches one. False if it touches none.
        /// </summary>
        public static bool TryFirstContact(
            Vector2 origin,
            float radius,
            Vector2 direction,
            float maxDistance,
            List<RaycastHit2D> buffer,
            out float distance)
        {
            distance = float.PositiveInfinity;
            int count = Physics2D.CircleCast(origin, radius, direction, SolidFilter, buffer, Mathf.Max(0f, maxDistance));
            for (int i = 0; i < count; i++)
            {
                if (IsWall(buffer[i].collider) && buffer[i].distance < distance)
                {
                    distance = Mathf.Max(0f, buffer[i].distance);
                }
            }

            buffer.Clear();
            return !float.IsPositiveInfinity(distance);
        }
    }
}
