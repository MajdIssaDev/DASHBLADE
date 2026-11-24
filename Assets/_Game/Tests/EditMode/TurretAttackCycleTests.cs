using System;
using System.Collections.Generic;
using Dashblade.Enemies;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class TurretAttackCycleTests
    {
        // TurretConfig values from the spec (criterion 17); the ring closes to the Turret's 1-unit width.
        private const float FirstAttackDelay = 1f;
        private const float WarningTime = 0.8f;
        private const float RestTime = 1.5f;
        private const float RingStartWidth = 3f;
        private const float RingEndWidth = 1f;
        private const double Period = 2.3;

        private const float Tolerance = 0.0001f;
        private const double TimelineTolerance = 0.001;

        // Uneven frame steps (seconds), repeated: a mix of fast, slow and hitchy frames.
        private static readonly float[] UnevenSteps = { 0.016f, 0.033f, 0.007f, 0.05f, 0.021f, 0.1f, 0.0125f };

        private static TurretAttackCycle NewCycle() =>
            new TurretAttackCycle(FirstAttackDelay, WarningTime, RestTime, RingStartWidth, RingEndWidth);

        // ---------- First attack delay ----------

        [Test]
        public void StartsWaiting_NoRing()
        {
            var cycle = NewCycle();
            Assert.That(cycle.Phase, Is.EqualTo(TurretPhase.WaitingForFirstAttack));
            Assert.That(cycle.IsRingShown, Is.False);
            Assert.That(cycle.RingWidth, Is.EqualTo(0f));
        }

        [Test]
        public void NothingOrangeForTheFirstSecond()
        {
            var cycle = NewCycle();
            int shots = cycle.Tick(0.5f) + cycle.Tick(0.49f);
            Assert.That(cycle.Phase, Is.EqualTo(TurretPhase.WaitingForFirstAttack));
            Assert.That(cycle.IsRingShown, Is.False);
            Assert.That(shots, Is.EqualTo(0));
        }

        // ---------- Ring ----------

        [Test]
        public void RingAppearsAtOneSecond_ThreeUnitsWide()
        {
            var cycle = NewCycle();
            cycle.Tick(1f);
            Assert.That(cycle.Phase, Is.EqualTo(TurretPhase.Warning));
            Assert.That(cycle.IsRingShown, Is.True);
            Assert.That(cycle.RingWidth, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void RingShrinksLinearlyToOneUnit()
        {
            var cycle = NewCycle();
            cycle.Tick(1f);

            cycle.Tick(0.2f);
            Assert.That(cycle.RingWidth, Is.EqualTo(2.5f).Within(Tolerance));
            cycle.Tick(0.2f);
            Assert.That(cycle.RingWidth, Is.EqualTo(2.0f).Within(Tolerance));
            cycle.Tick(0.2f);
            Assert.That(cycle.RingWidth, Is.EqualTo(1.5f).Within(Tolerance));
            cycle.Tick(0.19f);
            Assert.That(cycle.RingWidth, Is.EqualTo(1.025f).Within(Tolerance));
        }

        [Test]
        public void RingWidthUsesTheGivenWidths()
        {
            var cycle = new TurretAttackCycle(0f, 2f, RestTime, 4f, 2f);
            cycle.Tick(1f);
            Assert.That(cycle.RingWidth, Is.EqualTo(3f).Within(Tolerance));
        }

        // ---------- Shot ----------

        [Test]
        public void OneShotAt1_8Seconds_AsTheRingCloses()
        {
            var cycle = NewCycle();
            int fired = 0;
            cycle.ShotFired += () => fired++;

            Assert.That(cycle.Tick(1f), Is.EqualTo(0));
            Assert.That(cycle.Tick(0.79f), Is.EqualTo(0));
            Assert.That(cycle.IsRingShown, Is.True);

            Assert.That(cycle.Tick(0.02f), Is.EqualTo(1));
            Assert.That(fired, Is.EqualTo(1));
            Assert.That(cycle.Phase, Is.EqualTo(TurretPhase.Resting));
            Assert.That(cycle.IsRingShown, Is.False, "the ring is gone once it has closed");
        }

        [Test]
        public void ExactStepOntoTheClosingMoment_Fires()
        {
            var cycle = NewCycle();
            cycle.Tick(1f);
            Assert.That(cycle.Tick(0.8f), Is.EqualTo(1));
        }

        [Test]
        public void ShotFired_IsRaisedAfterTheRingHasGone()
        {
            var cycle = NewCycle();
            bool ringShownDuringShot = true;
            TurretPhase phaseDuringShot = TurretPhase.Warning;
            cycle.ShotFired += () =>
            {
                ringShownDuringShot = cycle.IsRingShown;
                phaseDuringShot = cycle.Phase;
            };

            cycle.Tick(1.8f);

            Assert.That(ringShownDuringShot, Is.False);
            Assert.That(phaseDuringShot, Is.EqualTo(TurretPhase.Resting));
        }

        [Test]
        public void NeverTwoShotsForOneRing()
        {
            var cycle = NewCycle();
            cycle.Tick(1.8f);

            int more = 0;
            cycle.ShotFired += () => more++;
            cycle.Tick(0f);
            for (int i = 0; i < 149; i++)
            {
                cycle.Tick(0.01f);
            }

            Assert.That(more, Is.EqualTo(0), "no second shot during the 1.5 s rest");
            Assert.That(cycle.Phase, Is.EqualTo(TurretPhase.Resting));
        }

        [Test]
        public void Example_RingAt1_ShotAt1_8_RingAt3_3_ShotAt4_1()
        {
            var cycle = NewCycle();
            var clock = new Timeline(cycle);
            clock.Run(new[] { 0.1f }, 4.5);

            List<double> rings = clock.Times("Warning");
            List<double> shots = clock.Times("Shot");
            Assert.That(rings.Count, Is.EqualTo(2));
            Assert.That(shots.Count, Is.EqualTo(2));
            Assert.That(rings[0], Is.EqualTo(1.0).Within(TimelineTolerance));
            Assert.That(shots[0], Is.EqualTo(1.8).Within(TimelineTolerance));
            Assert.That(rings[1], Is.EqualTo(3.3).Within(TimelineTolerance));
            Assert.That(shots[1], Is.EqualTo(4.1).Within(TimelineTolerance));
        }

        [Test]
        public void OneStepAcrossSeveralRings_FiresOncePerRing()
        {
            var cycle = NewCycle();
            int fired = 0;
            cycle.ShotFired += () => fired++;

            // From spawn: shots at 1.8, 4.1 and 6.4 s; the step ends at 6.5 s.
            int returned = cycle.Tick(6.5f);

            Assert.That(returned, Is.EqualTo(3));
            Assert.That(fired, Is.EqualTo(3));
            Assert.That(cycle.Phase, Is.EqualTo(TurretPhase.Resting));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.1f).Within(Tolerance), "the leftover is carried");
        }

        [Test]
        public void OneStepAcrossTheRingStart_CarriesLeftover()
        {
            var cycle = NewCycle();
            cycle.Tick(1.4f);
            Assert.That(cycle.Phase, Is.EqualTo(TurretPhase.Warning));
            Assert.That(cycle.RingWidth, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void UnevenFrames_OneShotEvery2_3Seconds_NoDrift()
        {
            var cycle = NewCycle();
            var clock = new Timeline(cycle);
            int mostShotsInOneStep = clock.Run(UnevenSteps, 100.0);

            List<double> shots = clock.Times("Shot");
            List<double> rings = clock.Times("Warning");
            Assert.That(shots.Count, Is.EqualTo(43), "shots at 1.8 + 2.3 k s before 100 s");
            Assert.That(mostShotsInOneStep, Is.EqualTo(1));
            for (int i = 0; i < shots.Count; i++)
            {
                Assert.That(rings[i], Is.EqualTo(1.0 + Period * i).Within(TimelineTolerance), $"ring {i + 1}");
                Assert.That(shots[i], Is.EqualTo(1.8 + Period * i).Within(TimelineTolerance), $"shot {i + 1}");
            }
        }

        [Test]
        public void SixtyFramesPerSecond_ShotAfterExactly108Frames()
        {
            var cycle = NewCycle();
            int shots = 0;
            for (int frame = 1; frame <= 107; frame++)
            {
                shots += cycle.Tick(1f / 60f);
            }

            Assert.That(shots, Is.EqualTo(0));
            Assert.That(cycle.Tick(1f / 60f), Is.EqualTo(1), "1.8 s = 108 frames; rounding does not delay the shot");
        }

        [Test]
        public void ZeroNegativeAndNaNSteps_AreIgnored()
        {
            var cycle = NewCycle();
            cycle.Tick(1.5f);
            Assert.That(cycle.Tick(0f), Is.EqualTo(0));
            Assert.That(cycle.Tick(-5f), Is.EqualTo(0));
            Assert.That(cycle.Tick(float.NaN), Is.EqualTo(0));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.5f).Within(Tolerance));
        }

        // ---------- Destroyed ----------

        [TestCase(TurretPhase.WaitingForFirstAttack)]
        [TestCase(TurretPhase.Warning)]
        [TestCase(TurretPhase.Resting)]
        public void DestroyedInAnyPhase_RingVanishes_NoShotEver(TurretPhase phase)
        {
            var cycle = CycleIn(phase);
            int fired = 0;
            cycle.ShotFired += () => fired++;

            cycle.Destroy();
            AssertStopped(cycle, TurretPhase.Destroyed);
            Assert.That(cycle.IsDestroyed, Is.True);

            Assert.That(cycle.Tick(100f), Is.EqualTo(0));
            cycle.OnPlayerDefeated();
            AssertStopped(cycle, TurretPhase.Destroyed);
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void DestroyedJustBeforeTheRingCloses_NoShot()
        {
            var cycle = NewCycle();
            int fired = 0;
            cycle.ShotFired += () => fired++;

            cycle.Tick(1.79f);
            cycle.Destroy();
            Assert.That(cycle.Tick(0.02f), Is.EqualTo(0));
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void DestroyedTwice_AnnouncedOnce()
        {
            var cycle = NewCycle();
            int destroyed = 0;
            cycle.PhaseChanged += phase =>
            {
                if (phase == TurretPhase.Destroyed)
                {
                    destroyed++;
                }
            };

            cycle.Destroy();
            cycle.Destroy();
            Assert.That(destroyed, Is.EqualTo(1));
        }

        // ---------- Player defeated ----------

        [TestCase(TurretPhase.WaitingForFirstAttack)]
        [TestCase(TurretPhase.Warning)]
        [TestCase(TurretPhase.Resting)]
        public void PlayerDefeatedInAnyPhase_RingVanishes_NoShotEver(TurretPhase phase)
        {
            var cycle = CycleIn(phase);
            int fired = 0;
            cycle.ShotFired += () => fired++;

            cycle.OnPlayerDefeated();
            AssertStopped(cycle, TurretPhase.Idle);
            Assert.That(cycle.IsPlayerDefeated, Is.True);

            Assert.That(cycle.Tick(100f), Is.EqualTo(0));
            AssertStopped(cycle, TurretPhase.Idle);
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void DestroyedAfterPlayerDefeated_IsDestroyed()
        {
            var cycle = NewCycle();
            cycle.OnPlayerDefeated();
            cycle.Destroy();
            AssertStopped(cycle, TurretPhase.Destroyed);
        }

        // ---------- Constructor ----------

        [TestCase(-0.1f, 0.8f, 1.5f, 3f, 1f)]
        [TestCase(1f, 0f, 1.5f, 3f, 1f)]
        [TestCase(1f, -0.8f, 1.5f, 3f, 1f)]
        [TestCase(1f, 0.8f, -0.1f, 3f, 1f)]
        [TestCase(1f, 0.8f, 1.5f, -3f, 1f)]
        [TestCase(1f, 0.8f, 1.5f, 3f, -1f)]
        [TestCase(1f, float.NaN, 1.5f, 3f, 1f)]
        public void InvalidTuning_Throws(
            float firstAttackDelay, float warningTime, float restTime, float ringStartWidth, float ringEndWidth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new TurretAttackCycle(firstAttackDelay, warningTime, restTime, ringStartWidth, ringEndWidth));
        }

        // ---------- Helpers ----------

        private static TurretAttackCycle CycleIn(TurretPhase phase)
        {
            var cycle = NewCycle();
            switch (phase)
            {
                case TurretPhase.WaitingForFirstAttack:
                    cycle.Tick(0.5f);
                    break;
                case TurretPhase.Warning:
                    cycle.Tick(1.4f);
                    break;
                case TurretPhase.Resting:
                    cycle.Tick(2.5f);
                    break;
            }

            Assert.That(cycle.Phase, Is.EqualTo(phase), "test setup");
            return cycle;
        }

        private static void AssertStopped(TurretAttackCycle cycle, TurretPhase expected)
        {
            Assert.That(cycle.Phase, Is.EqualTo(expected));
            Assert.That(cycle.IsRingShown, Is.False);
            Assert.That(cycle.RingWidth, Is.EqualTo(0f));
        }

        /// <summary>
        /// Records when each event happened in exact game time: the end of the step minus the
        /// time already spent in the new phase (the leftover the cycle carried over).
        /// </summary>
        private sealed class Timeline
        {
            private readonly TurretAttackCycle cycle;
            private readonly List<KeyValuePair<string, double>> events = new List<KeyValuePair<string, double>>();
            private double now;

            public Timeline(TurretAttackCycle cycle)
            {
                this.cycle = cycle;
                cycle.PhaseChanged += phase => Record(phase.ToString());
                cycle.ShotFired += () => Record("Shot");
            }

            /// <returns>The most shots fired by a single step.</returns>
            public int Run(float[] steps, double until)
            {
                int most = 0;
                for (int i = 0; now < until; i++)
                {
                    float step = steps[i % steps.Length];
                    now += step;
                    most = Math.Max(most, cycle.Tick(step));
                }

                return most;
            }

            public List<double> Times(string name)
            {
                var times = new List<double>();
                foreach (KeyValuePair<string, double> e in events)
                {
                    if (e.Key == name)
                    {
                        times.Add(e.Value);
                    }
                }

                return times;
            }

            private void Record(string name)
            {
                events.Add(new KeyValuePair<string, double>(name, now - cycle.TimeInPhase));
            }
        }
    }
}
