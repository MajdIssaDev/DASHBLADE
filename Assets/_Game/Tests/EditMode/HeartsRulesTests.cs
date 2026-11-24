using System.Collections.Generic;
using Dashblade.Combat;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class HeartsRulesTests
    {
        private const int MaxHearts = 3;
        private const float ProtectionTime = 0.75f;
        private const float Frame = 1f / 60f;
        private const float Tolerance = 0.0001f;

        private static HeartsRules NewRules()
        {
            return new HeartsRules(MaxHearts, ProtectionTime);
        }

        /// <summary>
        /// A player standing in danger: every frame Tick, then one hit attempt. The first attempt is at time 0
        /// (before any Tick). Returns the game time of every hit that landed, until defeat or <paramref name="maxSeconds"/>.
        /// </summary>
        private static List<double> StandInDanger(HeartsRules rules, float frame, double maxSeconds)
        {
            var hitTimes = new List<double>();
            double time = 0d;
            if (rules.TryHit(false))
            {
                hitTimes.Add(time);
            }

            while (!rules.IsDefeated && time < maxSeconds)
            {
                rules.Tick(frame);
                time += frame;
                if (rules.TryHit(false))
                {
                    hitTimes.Add(time);
                }
            }

            return hitTimes;
        }

        // ---- Start state ----

        [Test]
        public void StartsAtMaxHearts_NotProtected_NotDefeated()
        {
            HeartsRules rules = NewRules();
            Assert.That(rules.Hearts, Is.EqualTo(3));
            Assert.That(rules.MaxHearts, Is.EqualTo(3));
            Assert.That(rules.IsProtected, Is.False);
            Assert.That(rules.IsDefeated, Is.False);
        }

        [Test]
        public void MaxHearts_ComesFromTheValuePassed()
        {
            var rules = new HeartsRules(5, ProtectionTime);
            Assert.That(rules.Hearts, Is.EqualTo(5));
        }

        [Test]
        public void Constructor_ClampsNonsenseTuning()
        {
            var rules = new HeartsRules(0, -1f);
            Assert.That(rules.MaxHearts, Is.EqualTo(1));
            Assert.That(rules.ProtectionTime, Is.EqualTo(0f));
        }

        // ---- One hit ----

        [Test]
        public void Hit_RemovesExactlyOneHeart_AndStartsProtection()
        {
            HeartsRules rules = NewRules();
            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.Hearts, Is.EqualTo(2));
            Assert.That(rules.IsProtected, Is.True);
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0f));
        }

        [Test]
        public void StepInAndOut_ProtectedFor075_ThenUnprotected_StillTwoHearts()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);

            rules.Tick(0.74f);
            Assert.That(rules.IsProtected, Is.True);
            rules.Tick(0.02f);
            Assert.That(rules.IsProtected, Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(2));
        }

        // ---- Ignored hits ----

        [Test]
        public void HitWhileProtected_Ignored_AndDoesNotExtendProtection()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);

            rules.Tick(0.5f);
            Assert.That(rules.TryHit(false), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(2));
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0.5f).Within(Tolerance), "not restarted");

            rules.Tick(0.24f);
            Assert.That(rules.IsProtected, Is.True);
            rules.Tick(0.02f);
            Assert.That(rules.IsProtected, Is.False, "ends 0.75 s after the hit that landed");
        }

        [Test]
        public void HitWhileDashing_Ignored_AndStartsNoProtection()
        {
            HeartsRules rules = NewRules();
            var protectionEvents = new List<bool>();
            rules.ProtectionChanged += protectionEvents.Add;

            for (int i = 0; i < 120; i++)
            {
                Assert.That(rules.TryHit(true), Is.False);
                rules.Tick(Frame);
            }

            Assert.That(rules.Hearts, Is.EqualTo(3));
            Assert.That(rules.IsProtected, Is.False);
            Assert.That(protectionEvents, Is.Empty);
        }

        [Test]
        public void TwoHitsOnTheSameFrame_CostOneHeart()
        {
            HeartsRules rules = NewRules();
            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.TryHit(false), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(2));
        }

        [Test]
        public void TwoHitsOnTheFrameProtectionEnds_CostOneHeart()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.76f);

            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.TryHit(false), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(1));
        }

        [Test]
        public void ZeroProtectionTime_TwoHitsOnTheSameFrameStillCostOneHeart()
        {
            var rules = new HeartsRules(MaxHearts, 0f);
            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.TryHit(false), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(2));

            rules.Tick(Frame);
            Assert.That(rules.TryHit(false), Is.True, "the next frame's hit lands");
            Assert.That(rules.Hearts, Is.EqualTo(1));
        }

        // ---- Staying in danger ----

        [Test]
        public void StandingInDanger_HitsAt0_075_15()
        {
            List<double> hits = StandInDanger(NewRules(), Frame, 5d);

            Assert.That(hits.Count, Is.EqualTo(3));
            Assert.That(hits[0], Is.EqualTo(0d));
            Assert.That(hits[1], Is.EqualTo(0.75d).Within(Frame + Tolerance));
            Assert.That(hits[2], Is.EqualTo(1.5d).Within(Frame + Tolerance));
        }

        [Test]
        public void StandingInDanger_HitsStayOnThe075Grid_AtAnyFrameStep()
        {
            // 0.07, 0.2 and 0.333 do not divide 0.75 evenly. Hit k must land on the first frame at or after
            // k * 0.75 s, so it is never early and never more than one frame late, and the error never builds up.
            foreach (float frame in new[] { 1f / 30f, 1f / 60f, 1f / 144f, 0.07f, 0.2f, 0.333f })
            {
                var rules = new HeartsRules(40, ProtectionTime);
                List<double> hits = StandInDanger(rules, frame, 60d);

                Assert.That(hits.Count, Is.EqualTo(40), $"frame {frame}");
                for (int k = 0; k < hits.Count; k++)
                {
                    double ideal = k * (double)ProtectionTime;
                    Assert.That(hits[k], Is.GreaterThanOrEqualTo(ideal - 0.001d), $"frame {frame}, hit {k} early");
                    Assert.That(hits[k], Is.LessThan(ideal + frame + 0.001d), $"frame {frame}, hit {k} late");
                }
            }
        }

        [Test]
        public void StandingInDanger_UnevenFrameSteps_NoDrift()
        {
            float[] steps = { 0.016f, 0.021f, 0.009f, 0.033f, 0.05f, 0.012f, 0.1f };
            var rules = new HeartsRules(30, ProtectionTime);
            var hits = new List<double>();
            double time = 0d;
            rules.TryHit(false);
            hits.Add(0d);

            for (int i = 0; !rules.IsDefeated; i++)
            {
                float frame = steps[i % steps.Length];
                rules.Tick(frame);
                time += frame;
                if (rules.TryHit(false))
                {
                    hits.Add(time);
                }
            }

            double lastIdeal = 29 * (double)ProtectionTime;
            Assert.That(hits[29], Is.GreaterThanOrEqualTo(lastIdeal - 0.001d));
            Assert.That(hits[29], Is.LessThan(lastIdeal + 0.1d + 0.001d), "within one (largest) frame of the grid after 29 hits");
        }

        [Test]
        public void ProtectionEndsOnTime_DespiteFloatRoundingOfSummedFrames()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            for (int i = 0; i < 44; i++)
            {
                rules.Tick(Frame);
            }

            Assert.That(rules.IsProtected, Is.True);

            // 45 frames of 1/60 s add up to 0.74999994 in float, not 0.75.
            rules.Tick(Frame);
            Assert.That(rules.IsProtected, Is.False, "ends on the 45th frame, not one frame late");
            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.ProtectionElapsed, Is.GreaterThanOrEqualTo(0f), "a rounding shortfall is never carried as negative time");
        }

        [Test]
        public void LeftoverTime_KeptOnlyForAHitOnTheFrameProtectionEnds()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.8f); // Protection ran out 0.05 s into this frame.

            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0.05f).Within(Tolerance), "leftover counts towards the new protection");
        }

        [Test]
        public void LeftoverTime_DiscardedWhenNoHitLandsOnThatFrame()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.8f); // Ends with 0.05 s left over; the player is out of danger this frame.
            rules.Tick(0.5f);

            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0f), "a later hit gets the full protection time");
            rules.Tick(0.74f);
            Assert.That(rules.IsProtected, Is.True);
        }

        [Test]
        public void LeftoverTime_DiscardedByADashOnThatFrame()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.8f);
            Assert.That(rules.TryHit(true), Is.False);
            rules.Tick(0.1f); // The dash ends in danger.

            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0f));
        }

        [Test]
        public void Example_TwoHearts_03IntoTheBlink_NextHitLands045Later()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.3f);
            Assert.That(rules.TryHit(false), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(2));

            rules.Tick(0.449f);
            Assert.That(rules.TryHit(false), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(2));

            rules.Tick(0.002f);
            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.Hearts, Is.EqualTo(1));
        }

        // ---- Dash and protection ----

        [Test]
        public void DashEndsInDanger_NotProtected_HitLandsOnThatFrame()
        {
            HeartsRules rules = NewRules();
            for (int i = 0; i < 9; i++)
            {
                rules.Tick(Frame);
                Assert.That(rules.TryHit(true), Is.False, "3 hearts during the dash");
            }

            rules.Tick(Frame);
            Assert.That(rules.TryHit(false), Is.True, "2 hearts on the frame the dash ends");
            Assert.That(rules.Hearts, Is.EqualTo(2));
            Assert.That(rules.IsProtected, Is.True);
        }

        [Test]
        public void DashEndsInDanger_WhileProtected_HitLandsWhenProtectionEnds()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.2f);
            Assert.That(rules.TryHit(true), Is.False, "dashing");
            rules.Tick(0.15f);

            // Dash over, standing in danger, 0.35 s into protection.
            Assert.That(rules.TryHit(false), Is.False);
            rules.Tick(0.39f);
            Assert.That(rules.TryHit(false), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(2));

            rules.Tick(0.02f);
            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.Hearts, Is.EqualTo(1));
        }

        [Test]
        public void DashingWhileProtected_ProtectionRunsOnForItsRemainingTime()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.3f);

            // A dash starts 0.3 s into protection; the protection keeps counting through it.
            for (int i = 0; i < 8; i++)
            {
                Assert.That(rules.TryHit(true), Is.False);
                rules.Tick(0.05f);
            }

            Assert.That(rules.IsProtected, Is.True);
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0.7f).Within(Tolerance));

            rules.TryHit(true);
            rules.Tick(0.06f);
            Assert.That(rules.IsProtected, Is.False, "ends 0.75 s after the hit, mid-dash, not extended by the dash");
            Assert.That(rules.Hearts, Is.EqualTo(2));
        }

        // ---- Defeat ----

        [Test]
        public void FinalHit_Defeats_WithoutProtection()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.8f);
            rules.TryHit(false);
            rules.Tick(0.8f);
            Assert.That(rules.Hearts, Is.EqualTo(1));

            Assert.That(rules.TryHit(false), Is.True);
            Assert.That(rules.Hearts, Is.EqualTo(0));
            Assert.That(rules.IsDefeated, Is.True);
            Assert.That(rules.IsProtected, Is.False, "no blinking on the final hit");
        }

        [Test]
        public void WhileDefeated_HitsIgnored_HeartsNeverBelowZero()
        {
            HeartsRules rules = NewRules();
            StandInDanger(rules, Frame, 5d);
            Assert.That(rules.IsDefeated, Is.True);

            for (int i = 0; i < 200; i++)
            {
                rules.Tick(Frame);
                Assert.That(rules.TryHit(false), Is.False);
            }

            Assert.That(rules.Hearts, Is.EqualTo(0));
        }

        [Test]
        public void ProtectionTime_ComesFromTheValuePassed()
        {
            var rules = new HeartsRules(MaxHearts, 3f);
            List<double> hits = StandInDanger(rules, Frame, 10d);

            Assert.That(hits[0], Is.EqualTo(0d));
            Assert.That(hits[1], Is.EqualTo(3d).Within(Frame + Tolerance));
            Assert.That(hits[2], Is.EqualTo(6d).Within(Frame + Tolerance));
        }

        // ---- Restore ----

        [Test]
        public void Restore_FromOneOrTwo_SetsMaxHearts()
        {
            foreach (int hitsTaken in new[] { 1, 2 })
            {
                HeartsRules rules = NewRules();
                for (int i = 0; i < hitsTaken; i++)
                {
                    rules.TryHit(false);
                    rules.Tick(0.8f);
                }

                Assert.That(rules.Hearts, Is.EqualTo(3 - hitsTaken));
                Assert.That(rules.Restore(), Is.True);
                Assert.That(rules.Hearts, Is.EqualTo(3), $"after {hitsTaken} hit(s)");
            }
        }

        [Test]
        public void Restore_AtMax_ChangesNothing()
        {
            HeartsRules rules = NewRules();
            var heartEvents = new List<int>();
            rules.HeartsChanged += heartEvents.Add;

            Assert.That(rules.Restore(), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(3));
            Assert.That(heartEvents, Is.Empty);
        }

        [Test]
        public void Restore_WhileProtected_KeepsTheRemainingProtection()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0.3f);

            rules.Restore();
            Assert.That(rules.Hearts, Is.EqualTo(3));
            Assert.That(rules.IsProtected, Is.True);
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0.3f).Within(Tolerance));

            rules.Tick(0.44f);
            Assert.That(rules.IsProtected, Is.True);
            rules.Tick(0.02f);
            Assert.That(rules.IsProtected, Is.False);
        }

        [Test]
        public void Restore_WhileDefeated_DoesNothing()
        {
            HeartsRules rules = NewRules();
            StandInDanger(rules, Frame, 5d);
            var heartEvents = new List<int>();
            rules.HeartsChanged += heartEvents.Add;

            Assert.That(rules.Restore(), Is.False);
            Assert.That(rules.Hearts, Is.EqualTo(0));
            Assert.That(rules.IsDefeated, Is.True);
            Assert.That(heartEvents, Is.Empty);
        }

        // ---- Events ----

        [Test]
        public void Events_Hit_HeartsChangedThenDamagedThenProtectionStarted()
        {
            HeartsRules rules = NewRules();
            var log = new List<string>();
            rules.HeartsChanged += h => log.Add("hearts " + h);
            rules.Damaged += () => log.Add("damaged");
            rules.ProtectionChanged += p => log.Add("protected " + p);
            rules.Defeated += () => log.Add("defeated");

            rules.TryHit(false);
            rules.TryHit(false);
            Assert.That(log, Is.EqualTo(new[] { "hearts 2", "damaged", "protected True" }));

            log.Clear();
            rules.Tick(0.76f);
            Assert.That(log, Is.EqualTo(new[] { "protected False" }));
        }

        [Test]
        public void Events_FinalHit_DefeatedOnce_NoProtection()
        {
            var rules = new HeartsRules(1, ProtectionTime);
            var log = new List<string>();
            rules.HeartsChanged += h => log.Add("hearts " + h);
            rules.Damaged += () => log.Add("damaged");
            rules.ProtectionChanged += p => log.Add("protected " + p);
            rules.Defeated += () => log.Add("defeated");

            rules.TryHit(false);
            rules.Tick(1f);
            rules.TryHit(false);
            rules.Restore();

            Assert.That(log, Is.EqualTo(new[] { "hearts 0", "damaged", "defeated" }));
        }

        [Test]
        public void Events_Restore_RaisesHeartsChanged()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            var heartEvents = new List<int>();
            rules.HeartsChanged += heartEvents.Add;

            rules.Restore();
            Assert.That(heartEvents, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void Events_StateIsAlreadyUpdatedWhenRaised()
        {
            HeartsRules rules = NewRules();
            bool protectedSeen = false;
            int heartsSeen = -1;
            rules.ProtectionChanged += p => protectedSeen = rules.IsProtected;
            rules.HeartsChanged += h => heartsSeen = rules.Hearts;

            rules.TryHit(false);
            Assert.That(protectedSeen, Is.True);
            Assert.That(heartsSeen, Is.EqualTo(2));
        }

        // ---- Time ----

        [Test]
        public void ZeroOrNegativeDeltaTime_ChangesNothing()
        {
            HeartsRules rules = NewRules();
            rules.TryHit(false);
            rules.Tick(0f);
            rules.Tick(-1f);
            Assert.That(rules.ProtectionElapsed, Is.EqualTo(0f));
            Assert.That(rules.IsProtected, Is.True);
        }
    }
}
