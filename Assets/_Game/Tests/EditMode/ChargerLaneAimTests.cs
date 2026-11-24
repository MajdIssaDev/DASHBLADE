using System;
using Dashblade.Enemies;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class ChargerLaneAimTests
    {
        private const float Tolerance = 0.0001f;
        private const float AngleTolerance = 0.01f;

        [Test]
        public void StartsWithTheGivenFacing_Normalised()
        {
            var aim = new ChargerLaneAim(0f, -2f);
            Assert.That(aim.DirectionX, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(aim.DirectionY, Is.EqualTo(-1f).Within(Tolerance));
            Assert.That(aim.AngleDegrees, Is.EqualTo(-90f).Within(AngleTolerance));
        }

        [Test]
        public void ZeroInitialFacing_FacesRight()
        {
            var aim = new ChargerLaneAim(0f, 0f);
            Assert.That(aim.DirectionX, Is.EqualTo(1f));
            Assert.That(aim.DirectionY, Is.EqualTo(0f));
        }

        [Test]
        public void Tracking_PointsAtPlayerCentre_SpecPositions()
        {
            // Charger at (6, 3), player at the spawn (-10.5, 0): offset (-16.5, -3), about 16.77 units.
            var aim = new ChargerLaneAim(1f, 0f);
            aim.Update(6f, 3f, -10.5f, 0f, isTracking: true);

            double length = Math.Sqrt(16.5 * 16.5 + 3.0 * 3.0);
            Assert.That(aim.DirectionX, Is.EqualTo((float)(-16.5 / length)).Within(Tolerance));
            Assert.That(aim.DirectionY, Is.EqualTo((float)(-3.0 / length)).Within(Tolerance));
            Assert.That(aim.AngleDegrees, Is.EqualTo(-169.70f).Within(AngleTolerance));
        }

        [Test]
        public void DirectionIsUnitLength()
        {
            var aim = new ChargerLaneAim(1f, 0f);
            aim.Update(0f, 0f, 3f, 4f, isTracking: true);
            Assert.That(aim.DirectionX, Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(aim.DirectionY, Is.EqualTo(0.8f).Within(Tolerance));
            double length = Math.Sqrt(aim.DirectionX * aim.DirectionX + aim.DirectionY * aim.DirectionY);
            Assert.That(length, Is.EqualTo(1.0).Within(Tolerance));
        }

        [Test]
        public void WhileTracking_FollowsAWalkingPlayer()
        {
            var aim = new ChargerLaneAim(1f, 0f);
            aim.Update(0f, 0f, -10f, 0f, isTracking: true);
            Assert.That(Math.Abs(aim.AngleDegrees), Is.EqualTo(180f).Within(AngleTolerance));

            aim.Update(0f, 0f, -10f, 10f, isTracking: true);
            Assert.That(aim.AngleDegrees, Is.EqualTo(135f).Within(AngleTolerance));
        }

        [Test]
        public void Locked_NeverChanges()
        {
            var aim = new ChargerLaneAim(1f, 0f);
            aim.Update(0f, 0f, -10f, 0f, isTracking: true);

            aim.Update(0f, 0f, -10f, 10f, isTracking: false);
            aim.Update(0f, 0f, 5f, -5f, isTracking: false);
            aim.Update(3f, 3f, 0f, 0f, isTracking: false);

            Assert.That(Math.Abs(aim.AngleDegrees), Is.EqualTo(180f).Within(AngleTolerance));
        }

        [Test]
        public void PlayerOnTop_KeepsLastDirection_UntilOffTop()
        {
            var aim = new ChargerLaneAim(1f, 0f);
            aim.Update(2f, 2f, 2f, -4f, isTracking: true);
            Assert.That(aim.AngleDegrees, Is.EqualTo(-90f).Within(AngleTolerance));

            aim.Update(2f, 2f, 2f, 2f, isTracking: true);
            Assert.That(aim.AngleDegrees, Is.EqualTo(-90f).Within(AngleTolerance), "zero offset keeps the last direction");

            aim.Update(2f, 2f, 2f, 3f, isTracking: true);
            Assert.That(aim.AngleDegrees, Is.EqualTo(90f).Within(AngleTolerance), "tracks again once off the top");
        }

        [Test]
        public void PlayerOnTopWhenFirstWarningStarts_KeepsInitialFacing()
        {
            var aim = new ChargerLaneAim(0f, 1f);
            aim.Update(6f, 3f, 6f, 3f, isTracking: true);
            Assert.That(aim.AngleDegrees, Is.EqualTo(90f).Within(AngleTolerance));
        }

        [Test]
        public void OnTopDistance_OverlappingPlayerKeepsLastDirection_UntilApart()
        {
            // The Charger passes the sum of the body radii (0.5 + 0.5): overlapping bodies count as on top.
            var aim = new ChargerLaneAim(-1f, 0f, onTopDistance: 1f);

            aim.Update(0f, 0f, 0.3f, 0.6f, isTracking: true);
            Assert.That(Math.Abs(aim.AngleDegrees), Is.EqualTo(180f).Within(AngleTolerance), "overlapping: keeps the last direction");

            aim.Update(0f, 0f, 0f, 1f, isTracking: true);
            Assert.That(Math.Abs(aim.AngleDegrees), Is.EqualTo(180f).Within(AngleTolerance), "just touching still counts as on top");

            aim.Update(0f, 0f, 0f, 1.01f, isTracking: true);
            Assert.That(aim.AngleDegrees, Is.EqualTo(90f).Within(AngleTolerance), "apart: tracks again");
        }

        [Test]
        public void OnTopDistance_DoesNotAffectTheInitialFacing()
        {
            var aim = new ChargerLaneAim(0f, -0.2f, onTopDistance: 1f);
            Assert.That(aim.AngleDegrees, Is.EqualTo(-90f).Within(AngleTolerance));
        }

        [Test]
        public void NewWarningFromTheNewPosition_TracksAgain()
        {
            var aim = new ChargerLaneAim(1f, 0f);
            aim.Update(6f, 3f, -10.5f, 0f, isTracking: true);
            aim.Update(6f, 3f, -10.5f, 5f, isTracking: false);

            // After the charge it rests near the left wall; the next warning tracks from there.
            aim.Update(-11.5f, -0.2f, 0f, -0.2f, isTracking: true);
            Assert.That(aim.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));
        }

        [Test]
        public void WithTheCycle_LaneFollowsTheWalkingPlayerThenLocksAt1_6Seconds()
        {
            // Criterion 2: the player walks up at 6 units/second through the whole warning.
            const float Step = 0.02f;
            const float WalkSpeed = 6f;
            float chargerX = 6f;
            float chargerY = 3f;
            float playerX = -10.5f;
            float playerY = 0f;

            var cycle = new ChargerAttackCycle(1f, 1f, 0.4f, 1.5f);
            var aim = new ChargerLaneAim(-1f, 0f);
            cycle.PhaseChanged += phase =>
            {
                if (phase == ChargerPhase.Warning)
                {
                    aim.Update(chargerX, chargerY, playerX, playerY, isTracking: true);
                }
            };

            double now = 0;
            float playerYAtLock = float.NaN;
            float angleAtLock = float.NaN;
            float angleAtStripStart = float.NaN;
            for (int frame = 0; frame < 99; frame++) // 99 frames of 0.02 s = 1.98 s
            {
                now += Step;
                if (now > 1.0)
                {
                    playerY += WalkSpeed * Step;
                }

                bool wasTracking = cycle.IsLaneTracking;
                cycle.Tick(Step);
                aim.Update(chargerX, chargerY, playerX, playerY, cycle.IsLaneTracking);

                if (float.IsNaN(angleAtStripStart) && cycle.IsLaneShown)
                {
                    angleAtStripStart = aim.AngleDegrees;
                }

                if (wasTracking && cycle.IsLaneLocked)
                {
                    playerYAtLock = playerY;
                    angleAtLock = aim.AngleDegrees;
                }
            }

            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning), "test setup: still warning just before 2 s");
            Assert.That(playerYAtLock, Is.EqualTo(3.6f).Within(0.13f), "locked about 0.6 s into a 6 u/s walk");
            Assert.That(aim.AngleDegrees, Is.Not.EqualTo(angleAtStripStart).Within(1f), "the lane turned while tracking");
            Assert.That(aim.AngleDegrees, Is.EqualTo(angleAtLock), "no turning after the lock");
        }
    }
}
