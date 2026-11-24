using System;

namespace Dashblade.Waves
{
    /// <summary>
    /// The numbers in the layout safety rules. The spec's values: 6 pillars; at least 4 units
    /// from the player start (−14.5, 0); at least 3 units in x or in y from every enemy position;
    /// centre between x = −13 and 13 and y = −6 and 6; any two centres at least 4 units apart in
    /// x or in y.
    /// </summary>
    public sealed class LayoutSafetyLimits
    {
        public LayoutSafetyLimits(
            int pillarCount,
            float minDistanceFromPlayerStart,
            float minAxisOffsetFromEnemies,
            float minX,
            float maxX,
            float minY,
            float maxY,
            float minAxisOffsetBetweenPillars)
        {
            if (pillarCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pillarCount), pillarCount, "Must be 0 or more.");
            }

            if (!(minX <= maxX))
            {
                throw new ArgumentException($"minX ({minX}) must not be above maxX ({maxX}).", nameof(minX));
            }

            if (!(minY <= maxY))
            {
                throw new ArgumentException($"minY ({minY}) must not be above maxY ({maxY}).", nameof(minY));
            }

            PillarCount = pillarCount;
            MinDistanceFromPlayerStart = minDistanceFromPlayerStart;
            MinAxisOffsetFromEnemies = minAxisOffsetFromEnemies;
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            MinAxisOffsetBetweenPillars = minAxisOffsetBetweenPillars;
        }

        /// <summary>Exactly this many pillars (spec: 6).</summary>
        public int PillarCount { get; }

        /// <summary>Straight-line distance from every pillar centre to the player start, at least (spec: 4).</summary>
        public float MinDistanceFromPlayerStart { get; }

        /// <summary>Every pillar centre differs from every enemy position by at least this in x or in y (spec: 3).</summary>
        public float MinAxisOffsetFromEnemies { get; }

        /// <summary>Lowest allowed pillar centre x (spec: −13).</summary>
        public float MinX { get; }

        /// <summary>Highest allowed pillar centre x (spec: 13).</summary>
        public float MaxX { get; }

        /// <summary>Lowest allowed pillar centre y (spec: −6).</summary>
        public float MinY { get; }

        /// <summary>Highest allowed pillar centre y (spec: 6).</summary>
        public float MaxY { get; }

        /// <summary>Any two pillar centres differ by at least this in x or in y (spec: 4).</summary>
        public float MinAxisOffsetBetweenPillars { get; }
    }
}
