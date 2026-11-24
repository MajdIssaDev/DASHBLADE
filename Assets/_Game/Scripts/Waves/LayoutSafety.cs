using System;
using System.Collections.Generic;

namespace Dashblade.Waves
{
    /// <summary>A layout safety rule.</summary>
    public enum LayoutRule
    {
        /// <summary>The layout has exactly the required number of pillars.</summary>
        PillarCount,

        /// <summary>Every pillar centre is far enough (straight line) from the player start.</summary>
        DistanceFromPlayerStart,

        /// <summary>Every pillar centre is far enough in x or in y from every enemy position of every wave.</summary>
        ClearOfEnemyPositions,

        /// <summary>Every pillar centre lies inside the allowed x and y range (edges included).</summary>
        InsideBounds,

        /// <summary>Any two pillar centres are far enough apart in x or in y.</summary>
        ApartFromOtherPillars,
    }

    /// <summary>One broken rule, with the pillar (and enemy position or other pillar) involved.</summary>
    public readonly struct LayoutViolation
    {
        public LayoutViolation(LayoutRule rule, int pillarIndex, int otherIndex, string description)
        {
            Rule = rule;
            PillarIndex = pillarIndex;
            OtherIndex = otherIndex;
            Description = description;
        }

        public LayoutRule Rule { get; }

        /// <summary>The 0-based index of the offending pillar; −1 for <see cref="LayoutRule.PillarCount"/>.</summary>
        public int PillarIndex { get; }

        /// <summary>
        /// The 0-based index of the enemy position (<see cref="LayoutRule.ClearOfEnemyPositions"/>)
        /// or of the second pillar (<see cref="LayoutRule.ApartFromOtherPillars"/>); −1 otherwise.
        /// </summary>
        public int OtherIndex { get; }

        /// <summary>Human-readable explanation, for a test message or a config warning.</summary>
        public string Description { get; }

        public override string ToString() => $"{Rule}: {Description}";
    }

    /// <summary>The outcome of <see cref="LayoutSafety.Check"/>: every broken rule, empty if the layout is safe.</summary>
    public sealed class LayoutCheckResult
    {
        private readonly LayoutViolation[] violations;

        public LayoutCheckResult(IEnumerable<LayoutViolation> violations)
        {
            this.violations = new List<LayoutViolation>(violations).ToArray();
        }

        public bool IsSafe => violations.Length == 0;

        public IReadOnlyList<LayoutViolation> Violations => violations;

        /// <summary>True if at least one violation is of the given rule.</summary>
        public bool Breaks(LayoutRule rule)
        {
            foreach (LayoutViolation violation in violations)
            {
                if (violation.Rule == rule)
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString() =>
            IsSafe ? "Safe" : string.Join("; ", Array.ConvertAll(violations, v => v.ToString()));
    }

    /// <summary>
    /// The layout safety rules as a pure function over plain numbers. True for layouts A, B and
    /// C, and meant for any layout added later. Every limit is inclusive: a value exactly on a
    /// limit passes.
    /// </summary>
    public static class LayoutSafety
    {
        // Absorbs float rounding so a value exactly on a limit passes.
        private const float Tolerance = 0.0001f;

        /// <param name="pillarCentres">The layout's pillar centres.</param>
        /// <param name="playerStart">Where the player starts (spec: (−14.5, 0)).</param>
        /// <param name="enemyPositions">Every enemy position of every wave.</param>
        /// <param name="limits">The rules' numbers.</param>
        public static LayoutCheckResult Check(
            IReadOnlyList<ArenaPoint> pillarCentres,
            ArenaPoint playerStart,
            IReadOnlyList<ArenaPoint> enemyPositions,
            LayoutSafetyLimits limits)
        {
            if (pillarCentres == null)
            {
                throw new ArgumentNullException(nameof(pillarCentres));
            }

            if (enemyPositions == null)
            {
                throw new ArgumentNullException(nameof(enemyPositions));
            }

            if (limits == null)
            {
                throw new ArgumentNullException(nameof(limits));
            }

            var violations = new List<LayoutViolation>();

            if (pillarCentres.Count != limits.PillarCount)
            {
                violations.Add(new LayoutViolation(
                    LayoutRule.PillarCount, -1, -1,
                    $"{pillarCentres.Count} pillars; the layout needs exactly {limits.PillarCount}."));
            }

            for (int i = 0; i < pillarCentres.Count; i++)
            {
                ArenaPoint pillar = pillarCentres[i];

                float dx = pillar.X - playerStart.X;
                float dy = pillar.Y - playerStart.Y;
                float distance = (float)Math.Sqrt((dx * dx) + (dy * dy));
                if (distance < limits.MinDistanceFromPlayerStart - Tolerance)
                {
                    violations.Add(new LayoutViolation(
                        LayoutRule.DistanceFromPlayerStart, i, -1,
                        $"Pillar {i} at {pillar} is {distance:0.###} units from the player start {playerStart}; it must be at least {limits.MinDistanceFromPlayerStart}."));
                }

                for (int e = 0; e < enemyPositions.Count; e++)
                {
                    ArenaPoint enemy = enemyPositions[e];
                    if (!FarEnoughInXOrY(pillar, enemy, limits.MinAxisOffsetFromEnemies))
                    {
                        violations.Add(new LayoutViolation(
                            LayoutRule.ClearOfEnemyPositions, i, e,
                            $"Pillar {i} at {pillar} is less than {limits.MinAxisOffsetFromEnemies} units in both x and y from the enemy position {enemy}."));
                    }
                }

                if (pillar.X < limits.MinX - Tolerance || pillar.X > limits.MaxX + Tolerance ||
                    pillar.Y < limits.MinY - Tolerance || pillar.Y > limits.MaxY + Tolerance)
                {
                    violations.Add(new LayoutViolation(
                        LayoutRule.InsideBounds, i, -1,
                        $"Pillar {i} at {pillar} is outside x {limits.MinX} to {limits.MaxX}, y {limits.MinY} to {limits.MaxY}."));
                }

                for (int j = i + 1; j < pillarCentres.Count; j++)
                {
                    ArenaPoint other = pillarCentres[j];
                    if (!FarEnoughInXOrY(pillar, other, limits.MinAxisOffsetBetweenPillars))
                    {
                        violations.Add(new LayoutViolation(
                            LayoutRule.ApartFromOtherPillars, i, j,
                            $"Pillars {i} at {pillar} and {j} at {other} are less than {limits.MinAxisOffsetBetweenPillars} units apart in both x and y."));
                    }
                }
            }

            return new LayoutCheckResult(violations);
        }

        private static bool FarEnoughInXOrY(ArenaPoint a, ArenaPoint b, float minOffset) =>
            Math.Abs(a.X - b.X) >= minOffset - Tolerance || Math.Abs(a.Y - b.Y) >= minOffset - Tolerance;
    }
}
