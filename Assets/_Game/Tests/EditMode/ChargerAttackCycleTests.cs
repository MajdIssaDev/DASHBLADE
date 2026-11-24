using System;
using System.Collections.Generic;
using Dashblade.Enemies;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class ChargerAttackCycleTests
    {
        // ChargerConfig values from the spec (criterion 17).
        private const float FirstAttackDelay = 1f;
        private const float WarningTime = 1f;
        private const float LockTime = 0.4f;
        private const float RestTime = 1.5f;

        private const float Tolerance = 0.0001f;
        private const double TimelineTolerance = 0.001;

        // Uneven frame steps (seconds), repeated: a mix of fast, slow and hitchy frames.
        private static readonly float[] UnevenSteps = { 0.016f, 0.033f, 0.007f, 0.05f, 0.021f, 0.1f, 0.0125f };

        private static ChargerAttackCycle NewCycle() =>
            new ChargerAttackCycle(FirstAttackDelay, WarningTime, LockTime, RestTime);

        // ---------- First attack delay ----------

        [Test]
        public void StartsWaiting_NothingShown_Harmless()
        {
            var cycle = NewCycle();
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.WaitingForFirstAttack));
            Assert.That(cycle.IsLaneShown, Is.False);
            Assert.That(cycle.IsLaneTracking, Is.False);
            Assert.That(cycle.IsLaneLocked, Is.False);
            Assert.That(cycle.WarningProgress, Is.EqualTo(0f));
            Assert.That(cycle.HurtsOnContact, Is.False);
        }

        [Test]
        public void NothingOrangeForTheFirstSecond()
        {
            var cycle = NewCycle();
            cycle.Tick(0.5f);
            cycle.Tick(0.49f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.WaitingForFirstAttack));
            Assert.That(cycle.IsLaneShown, Is.False);
        }

        [Test]
        public void StripAppearsAtOneSecond_Tracking_BarEmpty()
        {
            var cycle = NewCycle();
            var phases = new List<ChargerPhase>();
            cycle.PhaseChanged += phases.Add;

            cycle.Tick(1f);

            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));
            Assert.That(cycle.IsLaneShown, Is.True);
            Assert.That(cycle.IsLaneTracking, Is.True);
            Assert.That(cycle.IsLaneLocked, Is.False);
            Assert.That(cycle.WarningProgress, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(cycle.HurtsOnContact, Is.False, "a warning Charger never hurts");
            Assert.That(phases, Is.EqualTo(new[] { ChargerPhase.Warning }));
        }

        // ---------- Warning: track, then lock ----------

        [Test]
        public void LaneTracksForFirst0_6Seconds_LocksAt1_6Seconds()
        {
            var cycle = NewCycle();
            int locks = 0;
            cycle.LaneLocked += () => locks++;

            cycle.Tick(1f);
            cycle.Tick(0.59f);
            Assert.That(cycle.IsLaneTracking, Is.True, "still tracking at 1.59 s");
            Assert.That(locks, Is.EqualTo(0));

            cycle.Tick(0.02f);
            Assert.That(cycle.IsLaneTracking, Is.False, "locked at 1.61 s");
            Assert.That(cycle.IsLaneLocked, Is.True);
            Assert.That(cycle.IsLaneShown, Is.True, "the strip stays while locked");
            Assert.That(locks, Is.EqualTo(1));

            cycle.Tick(0.2f);
            Assert.That(cycle.IsLaneLocked, Is.True);
            Assert.That(locks, Is.EqualTo(1), "locks once per warning");
        }

        [Test]
        public void ExactStepOntoLockBoundary_Locks()
        {
            var cycle = NewCycle();
            cycle.Tick(1f);
            cycle.Tick(0.6f);
            Assert.That(cycle.IsLaneLocked, Is.True);
        }

        [Test]
        public void BarGrowsLinearlyOverTheWarning()
        {
            var cycle = NewCycle();
            cycle.Tick(1f);

            cycle.Tick(0.25f);
            Assert.That(cycle.WarningProgress, Is.EqualTo(0.25f).Within(Tolerance));
            cycle.Tick(0.25f);
            Assert.That(cycle.WarningProgress, Is.EqualTo(0.5f).Within(Tolerance));
            cycle.Tick(0.25f);
            Assert.That(cycle.WarningProgress, Is.EqualTo(0.75f).Within(Tolerance));
        }

        [Test]
        public void ChargeStartsAtTwoSeconds_WhenTheBarIsFull()
        {
            var cycle = NewCycle();
            cycle.Tick(1f);
            cycle.Tick(0.999f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));
            Assert.That(cycle.WarningProgress, Is.EqualTo(0.999f).Within(Tolerance), "bar almost at the wall");

            cycle.Tick(0.001f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Charging));
            Assert.That(cycle.IsLaneShown, Is.False, "the strip vanishes as the charge starts");
            Assert.That(cycle.WarningProgress, Is.EqualTo(0f));
            Assert.That(cycle.HurtsOnContact, Is.True);
        }

        [Test]
        public void LockTimeEqualToWarningTime_LockedFromTheStart()
        {
            var cycle = new ChargerAttackCycle(FirstAttackDelay, WarningTime, WarningTime, RestTime);
            cycle.Tick(1f);
            Assert.That(cycle.IsLaneShown, Is.True);
            Assert.That(cycle.IsLaneLocked, Is.True);
        }

        [Test]
        public void LockTimeZero_TracksTheWholeWarning()
        {
            var cycle = new ChargerAttackCycle(FirstAttackDelay, WarningTime, 0f, RestTime);
            cycle.Tick(1f);
            cycle.Tick(0.99f);
            Assert.That(cycle.IsLaneTracking, Is.True);
        }

        // ---------- Charging ----------

        [Test]
        public void ChargeLastsUntilTheWrapperReportsTheWall()
        {
            var cycle = NewCycle();
            cycle.Tick(2f);
            cycle.Tick(10f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Charging));
            Assert.That(cycle.HurtsOnContact, Is.True);
            Assert.That(cycle.TimeInPhase, Is.EqualTo(10f).Within(Tolerance), "time charging is available to the wrapper");
        }

        [Test]
        public void ChargeTimeIncludesLeftoverOfTheStepThatStartedIt()
        {
            var cycle = NewCycle();
            cycle.Tick(1f);
            cycle.Tick(1.03f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Charging));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.03f).Within(Tolerance));
        }

        // ---------- Resting ----------

        [Test]
        public void RestIsHarmless_Lasts1_5Seconds_ThenNewStrip()
        {
            var cycle = NewCycle();
            cycle.Tick(2f);
            cycle.ReportChargeEnded();

            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Resting));
            Assert.That(cycle.HurtsOnContact, Is.False, "a resting Charger never hurts");
            Assert.That(cycle.IsLaneShown, Is.False);

            cycle.Tick(1.49f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Resting));

            cycle.Tick(0.02f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));
            Assert.That(cycle.IsLaneTracking, Is.True, "a new warning tracks again");
            Assert.That(cycle.WarningProgress, Is.EqualTo(0.01f).Within(Tolerance));
        }

        [Test]
        public void ReportedOutsideTick_RestStartsFromZero()
        {
            var cycle = NewCycle();
            cycle.Tick(2.5f);
            cycle.ReportChargeEnded();
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0f));
        }

        [Test]
        public void ReportedOutsideTick_TimePastArrivalCountsTowardsTheRest()
        {
            // The body reached the wall 0.03 s before the end of the step that took it there.
            var cycle = NewCycle();
            cycle.Tick(2f);
            cycle.Tick(1.2f);
            cycle.ReportChargeEnded(0.03f);

            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Resting));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.03f).Within(Tolerance));

            cycle.Tick(1.47f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning), "1.5 s after the arrival, not after the report");
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ReportedOutsideTick_TimePastArrivalIsClampedToTheChargeTime()
        {
            var cycle = NewCycle();
            cycle.Tick(2f);
            cycle.Tick(0.1f);
            cycle.ReportChargeEnded(5f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Resting));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.1f).Within(Tolerance));

            var other = NewCycle();
            other.Tick(2.1f);
            other.ReportChargeEnded(-1f);
            Assert.That(other.TimeInPhase, Is.EqualTo(0f));
        }

        [Test]
        public void ReportedOutsideTick_LargeTimePastArrival_CanStartTheNextWarning()
        {
            var cycle = NewCycle();
            cycle.Tick(2f);
            cycle.Tick(3f);
            cycle.ReportChargeEnded(1.7f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.2f).Within(Tolerance));
        }

        [Test]
        public void ReportChargeEnded_OutsideTheCharge_IsIgnored()
        {
            var cycle = NewCycle();
            cycle.ReportChargeEnded();
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.WaitingForFirstAttack));

            cycle.Tick(1.2f);
            cycle.ReportChargeEnded();
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));

            cycle.Tick(0.8f);
            cycle.ReportChargeEnded();
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Resting));
            cycle.Tick(0.5f);
            cycle.ReportChargeEnded();
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Resting));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.5f).Within(Tolerance), "a second report does not restart the rest");
        }

        [Test]
        public void CycleRepeatsForever()
        {
            var cycle = NewCycle();
            int warnings = 0;
            int charges = 0;
            cycle.PhaseChanged += phase =>
            {
                if (phase == ChargerPhase.Warning)
                {
                    warnings++;
                }
                else if (phase == ChargerPhase.Charging)
                {
                    charges++;
                }
            };

            cycle.Tick(2f);
            for (int i = 0; i < 9; i++)
            {
                cycle.Tick(1.2f); // the run along the lane
                cycle.ReportChargeEnded();
                cycle.Tick(RestTime);
                Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));
                cycle.Tick(WarningTime);
                Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Charging));
            }

            Assert.That(warnings, Is.EqualTo(10));
            Assert.That(charges, Is.EqualTo(10));
        }

        [Test]
        public void Example_StripAt1_LockedAt1_6_ChargeAt2_StopAt3_2_NextStripAt4_7()
        {
            var cycle = NewCycle();
            var clock = new Timeline(cycle);
            bool reported = false;

            for (int i = 0; i < 60; i++)
            {
                clock.Tick(0.1f);
                // The wrapper's body reaches the wall about 1.2 s into the charge.
                if (!reported && cycle.Phase == ChargerPhase.Charging && cycle.TimeInPhase >= 1.2f - 0.00001f)
                {
                    cycle.ReportChargeEnded();
                    clock.Record("stopped");
                    reported = true;
                }
            }

            Assert.That(clock.FirstTime("Warning"), Is.EqualTo(1.0).Within(TimelineTolerance));
            Assert.That(clock.FirstTime("Locked"), Is.EqualTo(1.6).Within(TimelineTolerance));
            Assert.That(clock.FirstTime("Charging"), Is.EqualTo(2.0).Within(TimelineTolerance));
            Assert.That(clock.FirstTime("stopped"), Is.EqualTo(3.2).Within(TimelineTolerance));
            Assert.That(clock.Times("Warning")[1], Is.EqualTo(4.7).Within(TimelineTolerance));
        }

        // ---------- Charge that cannot move ----------

        [Test]
        public void ChargeThatCannotMove_EndsAtOnce_RestBegins()
        {
            var cycle = NewCycle();
            var phases = new List<ChargerPhase>();
            cycle.PhaseChanged += phase =>
            {
                phases.Add(phase);
                if (phase == ChargerPhase.Charging)
                {
                    cycle.ReportChargeEnded(); // the wrapper sees it is already against the wall
                }
            };

            cycle.Tick(1f);
            cycle.Tick(1.25f); // crosses the charge start at 2.0 s with 0.25 s to spare

            Assert.That(phases, Is.EqualTo(new[] { ChargerPhase.Warning, ChargerPhase.Charging, ChargerPhase.Resting }));
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Resting));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.25f).Within(Tolerance), "the leftover goes to the rest");

            cycle.Tick(1.25f); // 3.5 s = 2.0 + 1.5
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0f).Within(Tolerance));
        }

        // ---------- Destroyed ----------

        [TestCase(ChargerPhase.WaitingForFirstAttack)]
        [TestCase(ChargerPhase.Warning)]
        [TestCase(ChargerPhase.Charging)]
        [TestCase(ChargerPhase.Resting)]
        public void DestroyedInAnyPhase_EverythingStops_HarmlessAtOnce(ChargerPhase phase)
        {
            var cycle = CycleIn(phase);
            cycle.Destroy();
            AssertDestroyed(cycle);

            var later = new List<string>();
            cycle.PhaseChanged += p => later.Add(p.ToString());
            cycle.LaneLocked += () => later.Add("Locked");

            cycle.Tick(100f);
            cycle.ReportChargeEnded();
            cycle.OnPlayerDefeated();
            AssertDestroyed(cycle);
            Assert.That(later, Is.Empty, "nothing happens after destruction");
        }

        [Test]
        public void DestroyedDuringLockedWarning_StripAndBarVanish()
        {
            var cycle = NewCycle();
            cycle.Tick(1.8f);
            Assert.That(cycle.IsLaneLocked, Is.True);
            cycle.Destroy();
            AssertDestroyed(cycle);
        }

        [Test]
        public void DestroyedTwice_AnnouncedOnce()
        {
            var cycle = NewCycle();
            int destroyed = 0;
            cycle.PhaseChanged += phase =>
            {
                if (phase == ChargerPhase.Destroyed)
                {
                    destroyed++;
                }
            };

            cycle.Destroy();
            cycle.Destroy();
            Assert.That(destroyed, Is.EqualTo(1));
        }

        [Test]
        public void DestroyedAfterPlayerDefeated_IsDestroyed()
        {
            var cycle = NewCycle();
            cycle.OnPlayerDefeated();
            cycle.Destroy();
            AssertDestroyed(cycle);
        }

        // ---------- Player defeated ----------

        [Test]
        public void PlayerDefeatedWhileWaiting_NoWarningEverStarts()
        {
            var cycle = NewCycle();
            cycle.Tick(0.5f);
            cycle.OnPlayerDefeated();
            cycle.Tick(100f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Idle));
            Assert.That(cycle.IsLaneShown, Is.False);
            Assert.That(cycle.HurtsOnContact, Is.False);
        }

        [Test]
        public void PlayerDefeatedDuringWarning_LaneVanishes_NoCharge()
        {
            var cycle = NewCycle();
            cycle.Tick(1.3f);
            cycle.OnPlayerDefeated();
            Assert.That(cycle.IsLaneShown, Is.False, "the showing lane vanishes at once");
            Assert.That(cycle.WarningProgress, Is.EqualTo(0f));

            cycle.Tick(100f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Idle));
            Assert.That(cycle.HurtsOnContact, Is.False);
        }

        [Test]
        public void PlayerDefeatedDuringCharge_ChargeFinishes_ThenNothingMore()
        {
            var cycle = NewCycle();
            cycle.Tick(2.3f);
            cycle.OnPlayerDefeated();
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Charging), "a charge already running finishes");
            Assert.That(cycle.HurtsOnContact, Is.True);

            cycle.Tick(0.5f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Charging));

            cycle.ReportChargeEnded();
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Idle));
            cycle.Tick(100f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Idle));
            Assert.That(cycle.IsLaneShown, Is.False);
            Assert.That(cycle.HurtsOnContact, Is.False);
        }

        [Test]
        public void PlayerDefeatedWhileResting_NoNewWarning()
        {
            var cycle = CycleIn(ChargerPhase.Resting);
            cycle.OnPlayerDefeated();
            cycle.Tick(100f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Idle));
            Assert.That(cycle.IsLaneShown, Is.False);
        }

        // ---------- Time steps ----------

        [Test]
        public void ZeroNegativeAndNaNSteps_AreIgnored()
        {
            var cycle = NewCycle();
            cycle.Tick(0.5f);
            cycle.Tick(0f);
            cycle.Tick(-5f);
            cycle.Tick(float.NaN);
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.WaitingForFirstAttack));
        }

        [Test]
        public void OneStepAcrossStripAndLock_CarriesLeftover()
        {
            var cycle = NewCycle();
            var events = new List<string>();
            cycle.PhaseChanged += p => events.Add(p.ToString());
            cycle.LaneLocked += () => events.Add("Locked");

            cycle.Tick(1.7f);

            Assert.That(events, Is.EqualTo(new[] { "Warning", "Locked" }));
            Assert.That(cycle.IsLaneLocked, Is.True);
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.7f).Within(Tolerance));
            Assert.That(cycle.WarningProgress, Is.EqualTo(0.7f).Within(Tolerance));
        }

        [Test]
        public void OneStepAcrossTheWholeWarning_StopsInCharging()
        {
            var cycle = NewCycle();
            var events = new List<string>();
            cycle.PhaseChanged += p => events.Add(p.ToString());
            cycle.LaneLocked += () => events.Add("Locked");

            cycle.Tick(2.3f);

            Assert.That(events, Is.EqualTo(new[] { "Warning", "Locked", "Charging" }));
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Charging));
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.3f).Within(Tolerance));
        }

        [Test]
        public void OneStepAcrossRestAndLock_LocksTheNewLane()
        {
            var cycle = NewCycle();
            cycle.Tick(2f);
            cycle.ReportChargeEnded();

            cycle.Tick(2.2f); // 1.5 rest + 0.7 into the warning

            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning));
            Assert.That(cycle.IsLaneLocked, Is.True);
            Assert.That(cycle.TimeInPhase, Is.EqualTo(0.7f).Within(Tolerance));
        }

        [Test]
        public void UnevenFrames_StripLockAndChargeOnTime()
        {
            var cycle = NewCycle();
            var clock = new Timeline(cycle);
            clock.Run(UnevenSteps, 2.5);

            Assert.That(clock.FirstTime("Warning"), Is.EqualTo(1.0).Within(Tolerance));
            Assert.That(clock.FirstTime("Locked"), Is.EqualTo(1.6).Within(Tolerance));
            Assert.That(clock.FirstTime("Charging"), Is.EqualTo(2.0).Within(Tolerance));
        }

        [Test]
        public void SixtyFramesPerSecond_StripAfterExactlySixtyFrames()
        {
            var cycle = NewCycle();
            for (int frame = 1; frame <= 59; frame++)
            {
                cycle.Tick(1f / 60f);
            }

            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.WaitingForFirstAttack));
            cycle.Tick(1f / 60f);
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Warning), "rounding does not delay the strip by a frame");
        }

        [Test]
        public void UnevenFrames_ManyCycles_DoNotDrift()
        {
            // Every charge cannot move, so the period is Warning + Rest = 2.5 s and nothing
            // depends on the wrapper: warnings must start at 1.0, 3.5, 6.0, ... for a minute.
            var cycle = NewCycle();
            cycle.PhaseChanged += phase =>
            {
                if (phase == ChargerPhase.Charging)
                {
                    cycle.ReportChargeEnded();
                }
            };

            var clock = new Timeline(cycle);
            clock.Run(UnevenSteps, 60.0);

            List<double> warnings = clock.Times("Warning");
            Assert.That(warnings.Count, Is.EqualTo(24));
            for (int i = 0; i < warnings.Count; i++)
            {
                Assert.That(warnings[i], Is.EqualTo(1.0 + 2.5 * i).Within(TimelineTolerance), $"warning {i + 1}");
            }

            List<double> locks = clock.Times("Locked");
            Assert.That(locks.Count, Is.EqualTo(24));
            for (int i = 0; i < locks.Count; i++)
            {
                Assert.That(locks[i], Is.EqualTo(1.6 + 2.5 * i).Within(TimelineTolerance), $"lock {i + 1}");
            }
        }

        // ---------- Constructor ----------

        [TestCase(-0.1f, 1f, 0.4f, 1.5f)]
        [TestCase(1f, 0f, 0f, 1.5f)]
        [TestCase(1f, -1f, 0f, 1.5f)]
        [TestCase(1f, 1f, -0.1f, 1.5f)]
        [TestCase(1f, 1f, 1.1f, 1.5f)]
        [TestCase(1f, 1f, 0.4f, -0.1f)]
        [TestCase(float.NaN, 1f, 0.4f, 1.5f)]
        public void InvalidTuning_Throws(float firstAttackDelay, float warningTime, float lockTime, float restTime)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ChargerAttackCycle(firstAttackDelay, warningTime, lockTime, restTime));
        }

        // ---------- Helpers ----------

        private static ChargerAttackCycle CycleIn(ChargerPhase phase)
        {
            var cycle = NewCycle();
            switch (phase)
            {
                case ChargerPhase.WaitingForFirstAttack:
                    cycle.Tick(0.5f);
                    break;
                case ChargerPhase.Warning:
                    cycle.Tick(1.3f);
                    break;
                case ChargerPhase.Charging:
                    cycle.Tick(2.5f);
                    break;
                case ChargerPhase.Resting:
                    cycle.Tick(2.5f);
                    cycle.ReportChargeEnded();
                    cycle.Tick(0.5f);
                    break;
            }

            Assert.That(cycle.Phase, Is.EqualTo(phase), "test setup");
            return cycle;
        }

        private static void AssertDestroyed(ChargerAttackCycle cycle)
        {
            Assert.That(cycle.Phase, Is.EqualTo(ChargerPhase.Destroyed));
            Assert.That(cycle.IsDestroyed, Is.True);
            Assert.That(cycle.IsLaneShown, Is.False);
            Assert.That(cycle.IsLaneTracking, Is.False);
            Assert.That(cycle.IsLaneLocked, Is.False);
            Assert.That(cycle.WarningProgress, Is.EqualTo(0f));
            Assert.That(cycle.HurtsOnContact, Is.False);
        }

        /// <summary>
        /// Records when each event happened in exact game time: the end of the step minus the
        /// time already spent in the new phase (the leftover the cycle carried over).
        /// </summary>
        private sealed class Timeline
        {
            private readonly ChargerAttackCycle cycle;
            private readonly List<KeyValuePair<string, double>> events = new List<KeyValuePair<string, double>>();
            private double now;

            public Timeline(ChargerAttackCycle cycle)
            {
                this.cycle = cycle;
                cycle.PhaseChanged += phase => Record(phase.ToString());
                cycle.LaneLocked += () => Record("Locked");
            }

            public void Tick(float step)
            {
                now += step;
                cycle.Tick(step);
            }

            public void Run(float[] steps, double until)
            {
                for (int i = 0; now < until; i++)
                {
                    Tick(steps[i % steps.Length]);
                }
            }

            public void Record(string name)
            {
                // A lock happens (Warning Time - Lock Time) into the warning.
                double intoPhase = name == "Locked"
                    ? cycle.TimeInPhase - (WarningTime - LockTime)
                    : cycle.TimeInPhase;
                events.Add(new KeyValuePair<string, double>(name, now - intoPhase));
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

            public double FirstTime(string name)
            {
                List<double> times = Times(name);
                Assert.That(times, Is.Not.Empty, $"no '{name}' event was recorded");
                return times[0];
            }
        }
    }
}
