using System;
using System.Linq;
using Dashblade.Waves;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class LayoutSafetyTests
    {
        private static LayoutCheckResult Check(ArenaPoint[] pillars) =>
            LayoutSafety.Check(pillars, SpecArena.PlayerStart, SpecArena.AllEnemyPositions(), SpecArena.Limits());

        /// <summary>Layout A with one pillar moved.</summary>
        private static ArenaPoint[] LayoutAWith(int index, float x, float y)
        {
            ArenaPoint[] pillars = SpecArena.Layout(ArenaLayout.A);
            pillars[index] = new ArenaPoint(x, y);
            return pillars;
        }

        // ---------- The spec's layouts ----------

        [Test]
        public void TheSpecHasTenEnemyPositions()
        {
            Assert.That(SpecArena.AllEnemyPositions().Length, Is.EqualTo(10));
        }

        [TestCase(ArenaLayout.A)]
        [TestCase(ArenaLayout.B)]
        [TestCase(ArenaLayout.C)]
        public void SpecLayout_PassesEveryRule_AgainstAllTenEnemyPositions(ArenaLayout layout)
        {
            LayoutCheckResult result = Check(SpecArena.Layout(layout));
            Assert.That(result.IsSafe, Is.True, result.ToString());
            Assert.That(result.Violations, Is.Empty);
        }

        // ---------- Rule: 6 pillars ----------

        [Test]
        public void FivePillars_BreaksPillarCountOnly()
        {
            LayoutCheckResult result = Check(SpecArena.Layout(ArenaLayout.A).Take(5).ToArray());
            Assert.That(result.IsSafe, Is.False);
            Assert.That(result.Violations.Select(v => v.Rule), Is.EqualTo(new[] { LayoutRule.PillarCount }));
            Assert.That(result.Violations[0].PillarIndex, Is.EqualTo(-1));
        }

        [Test]
        public void SevenPillars_BreaksPillarCount()
        {
            ArenaPoint[] pillars = SpecArena.Layout(ArenaLayout.A).Concat(new[] { new ArenaPoint(-3f, 8f) }).ToArray();
            Assert.That(Check(pillars).Breaks(LayoutRule.PillarCount), Is.True);
        }

        // ---------- Rule: at least 4 units from the player start ----------

        [Test]
        public void PillarThreeAndAHalfUnitsFromThePlayerStart_BreaksDistanceOnly()
        {
            LayoutCheckResult result = Check(LayoutAWith(0, -11f, 0f));
            Assert.That(result.Violations.Select(v => v.Rule), Is.EqualTo(new[] { LayoutRule.DistanceFromPlayerStart }), result.ToString());
            Assert.That(result.Violations[0].PillarIndex, Is.EqualTo(0));
        }

        [Test]
        public void DistanceFromThePlayerStart_IsAStraightLine()
        {
            // 2.5 in x and 3 in y: each under 4, but the straight-line distance is 3.9, still under 4.
            Assert.That(Check(LayoutAWith(0, -12f, 3f)).Breaks(LayoutRule.DistanceFromPlayerStart), Is.True);

            // 3 in x and 3 in y: 4.24 in a straight line, so it passes although neither axis reaches 4.
            Assert.That(Check(LayoutAWith(0, -11.5f, 3f)).Breaks(LayoutRule.DistanceFromPlayerStart), Is.False);
        }

        [Test]
        public void ExactlyFourUnitsFromThePlayerStart_Passes()
        {
            LayoutCheckResult result = Check(LayoutAWith(0, -10.5f, 0f));
            Assert.That(result.IsSafe, Is.True, result.ToString());
        }

        // ---------- Rule: at least 3 units in x or in y from every enemy position ----------

        [Test]
        public void PillarTwoUnitsFromAWave1Charger_BreaksEnemyClearanceOnly()
        {
            // Layout A's (9, 0) moved to (11, 0): 2 units left of the Charger at (13, 0).
            LayoutCheckResult result = Check(LayoutAWith(5, 11f, 0f));
            Assert.That(result.Violations.Select(v => v.Rule), Is.EqualTo(new[] { LayoutRule.ClearOfEnemyPositions }), result.ToString());
            LayoutViolation violation = result.Violations[0];
            Assert.That(violation.PillarIndex, Is.EqualTo(5));
            ArenaPoint enemy = SpecArena.AllEnemyPositions()[violation.OtherIndex];
            Assert.That(enemy.X, Is.EqualTo(13f));
            Assert.That(enemy.Y, Is.EqualTo(0f));
        }

        [Test]
        public void EnemyPositionsOfLaterWaves_AreCheckedToo()
        {
            // Layout A's (−1, 0) moved to (0, −5): 2 units above wave 2's Turret at (0, −7).
            LayoutCheckResult result = Check(LayoutAWith(2, 0f, -5f));
            Assert.That(result.Violations.Select(v => v.Rule), Is.EqualTo(new[] { LayoutRule.ClearOfEnemyPositions }), result.ToString());
        }

        [Test]
        public void EnemyClearance_IsInXOrInY()
        {
            ArenaPoint[] enemy = { new ArenaPoint(0f, 0f) };
            LayoutSafetyLimits limits = SpecArena.Limits();
            ArenaPoint start = SpecArena.PlayerStart;

            // Far in x only, far in y only, exactly 3 in one axis: all pass.
            Assert.That(LayoutSafety.Check(new[] { new ArenaPoint(3f, 0f) }, start, enemy, limits).Breaks(LayoutRule.ClearOfEnemyPositions), Is.False);
            Assert.That(LayoutSafety.Check(new[] { new ArenaPoint(0f, -3f) }, start, enemy, limits).Breaks(LayoutRule.ClearOfEnemyPositions), Is.False);

            // 2.9 in both axes: 4.1 in a straight line, but under 3 in each axis, so it fails.
            Assert.That(LayoutSafety.Check(new[] { new ArenaPoint(2.9f, 2.9f) }, start, enemy, limits).Breaks(LayoutRule.ClearOfEnemyPositions), Is.True);
        }

        // ---------- Rule: centre between x = −13 and 13 and y = −6 and 6 ----------

        [Test]
        public void PillarLeftOfXMinus13_BreaksBoundsOnly()
        {
            LayoutCheckResult result = Check(LayoutAWith(0, -13.5f, 6f));
            Assert.That(result.Violations.Select(v => v.Rule), Is.EqualTo(new[] { LayoutRule.InsideBounds }), result.ToString());
            Assert.That(result.Violations[0].PillarIndex, Is.EqualTo(0));
        }

        [Test]
        public void PillarAboveY6_BreaksBoundsOnly()
        {
            LayoutCheckResult result = Check(LayoutAWith(2, -4f, 6.5f));
            Assert.That(result.Violations.Select(v => v.Rule), Is.EqualTo(new[] { LayoutRule.InsideBounds }), result.ToString());
        }

        [TestCase(13.5f, 0f)]
        [TestCase(-13.5f, 0f)]
        [TestCase(0f, 6.5f)]
        [TestCase(0f, -6.5f)]
        public void PillarOutsideAnyEdge_BreaksBounds(float x, float y)
        {
            LayoutCheckResult result = LayoutSafety.Check(
                new[] { new ArenaPoint(x, y) }, new ArenaPoint(-100f, -100f), new ArenaPoint[0], SpecArena.Limits());
            Assert.That(result.Breaks(LayoutRule.InsideBounds), Is.True);
        }

        [TestCase(13f, 6f)]
        [TestCase(-13f, -6f)]
        [TestCase(13f, -6f)]
        [TestCase(-13f, 6f)]
        public void PillarOnTheEdge_IsInsideBounds(float x, float y)
        {
            LayoutCheckResult result = LayoutSafety.Check(
                new[] { new ArenaPoint(x, y) }, new ArenaPoint(-100f, -100f), new ArenaPoint[0], SpecArena.Limits());
            Assert.That(result.Breaks(LayoutRule.InsideBounds), Is.False);
        }

        // ---------- Rule: any two pillar centres at least 4 units apart in x or in y ----------

        [Test]
        public void TwoPillarsThreeUnitsApartInBothAxes_BreaksSpacingOnly()
        {
            // Layout A's (−1, 0) moved to (1, 1): 3 and 3 from (4, 4). Straight line 4.24, still too close.
            LayoutCheckResult result = Check(LayoutAWith(2, 1f, 1f));
            Assert.That(result.Violations.Select(v => v.Rule), Is.EqualTo(new[] { LayoutRule.ApartFromOtherPillars }), result.ToString());
            Assert.That(result.Violations[0].PillarIndex, Is.EqualTo(2));
            Assert.That(result.Violations[0].OtherIndex, Is.EqualTo(3));
        }

        [Test]
        public void TwoPillarsExactlyFourApartInOneAxis_Pass()
        {
            ArenaPoint[] pillars = { new ArenaPoint(0f, 0f), new ArenaPoint(4f, 0f), new ArenaPoint(0f, -4f) };
            LayoutCheckResult result = LayoutSafety.Check(pillars, new ArenaPoint(-100f, -100f), new ArenaPoint[0], SpecArena.Limits());
            Assert.That(result.Breaks(LayoutRule.ApartFromOtherPillars), Is.False);
        }

        [Test]
        public void TwoPillarsOnTheSameSpot_BreakSpacing()
        {
            ArenaPoint[] pillars = SpecArena.Layout(ArenaLayout.C);
            pillars[5] = pillars[0];
            Assert.That(Check(pillars).Breaks(LayoutRule.ApartFromOtherPillars), Is.True);
        }

        // ---------- Reporting ----------

        [Test]
        public void EveryBrokenRuleIsReported()
        {
            // Five pillars, one of them right on the player start, outside the bounds and on top of another.
            ArenaPoint[] pillars = SpecArena.Points(-14.5f, 0f, -14.5f, 0f, 4f, 4f, 4f, -4f, 9f, 0f);
            LayoutCheckResult result = Check(pillars);

            Assert.That(result.Breaks(LayoutRule.PillarCount), Is.True);
            Assert.That(result.Breaks(LayoutRule.DistanceFromPlayerStart), Is.True);
            Assert.That(result.Breaks(LayoutRule.InsideBounds), Is.True);
            Assert.That(result.Breaks(LayoutRule.ApartFromOtherPillars), Is.True);
            Assert.That(result.Breaks(LayoutRule.ClearOfEnemyPositions), Is.False);
            Assert.That(result.Violations.All(v => !string.IsNullOrEmpty(v.Description)), Is.True);
        }

        [Test]
        public void RejectsMissingArguments()
        {
            ArenaPoint[] pillars = SpecArena.Layout(ArenaLayout.A);
            ArenaPoint[] enemies = SpecArena.AllEnemyPositions();
            Assert.Throws<ArgumentNullException>(() => LayoutSafety.Check(null, SpecArena.PlayerStart, enemies, SpecArena.Limits()));
            Assert.Throws<ArgumentNullException>(() => LayoutSafety.Check(pillars, SpecArena.PlayerStart, null, SpecArena.Limits()));
            Assert.Throws<ArgumentNullException>(() => LayoutSafety.Check(pillars, SpecArena.PlayerStart, enemies, null));
        }

        [Test]
        public void LimitsRejectInvertedRanges()
        {
            Assert.Throws<ArgumentException>(() => new LayoutSafetyLimits(6, 4f, 3f, 13f, -13f, -6f, 6f, 4f));
            Assert.Throws<ArgumentException>(() => new LayoutSafetyLimits(6, 4f, 3f, -13f, 13f, 6f, -6f, 4f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LayoutSafetyLimits(-1, 4f, 3f, -13f, 13f, -6f, 6f, 4f));
        }
    }
}
