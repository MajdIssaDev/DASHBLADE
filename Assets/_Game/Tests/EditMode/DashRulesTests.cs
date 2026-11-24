using System.Collections.Generic;
using Dashblade.Combat;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class DashRulesTests
    {
        private const float Distance = 5f;
        private const float Duration = 0.15f;
        private const float MissCooldown = 1f;
        private const float Tolerance = 0.0001f;

        private static DashRules StartedFullDash()
        {
            var rules = new DashRules();
            rules.TryStartDash(Distance, DashRules.DashTimeFor(Distance, Distance, Duration));
            return rules;
        }

        private static void RunToEnd(DashRules rules, float frame = 1f / 60f)
        {
            while (!rules.IsDashComplete)
            {
                rules.Tick(frame);
            }
        }

        [Test]
        public void StartsReady_NotInvulnerable()
        {
            var rules = new DashRules();
            Assert.That(rules.State, Is.EqualTo(DashState.Ready));
            Assert.That(rules.IsReady, Is.True);
            Assert.That(rules.IsInvulnerable, Is.False);
        }

        [Test]
        public void StartDash_NotReady_Invulnerable_RaisesNotReady()
        {
            var rules = new DashRules();
            var events = new List<bool>();
            rules.ReadinessChanged += events.Add;

            Assert.That(rules.TryStartDash(Distance, Duration), Is.True);

            Assert.That(rules.State, Is.EqualTo(DashState.Dashing));
            Assert.That(rules.IsReady, Is.False);
            Assert.That(rules.IsInvulnerable, Is.True);
            Assert.That(events, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void DashTime_KeepsConstantSpeed()
        {
            Assert.That(DashRules.DashTimeFor(5f, Distance, Duration), Is.EqualTo(0.15f).Within(Tolerance));
            Assert.That(DashRules.DashTimeFor(2f, Distance, Duration), Is.EqualTo(0.06f).Within(Tolerance));
            Assert.That(DashRules.DashTimeFor(2.5f, Distance, Duration), Is.EqualTo(0.075f).Within(Tolerance));
            Assert.That(DashRules.DashTimeFor(0f, Distance, Duration), Is.EqualTo(0f));
            Assert.That(DashRules.DashTimeFor(5f, Distance, 2f), Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Travelled_IsLinearInTime_AndClampedToLength()
        {
            DashRules rules = StartedFullDash();
            Assert.That(rules.Travelled, Is.EqualTo(0f));

            rules.Tick(0.075f);
            Assert.That(rules.Travelled, Is.EqualTo(2.5f).Within(Tolerance));
            Assert.That(rules.IsDashComplete, Is.False);

            rules.Tick(0.5f);
            Assert.That(rules.Travelled, Is.EqualTo(5f));
            Assert.That(rules.IsDashComplete, Is.True);
        }

        [Test]
        public void Travelled_ExactLengthAtEnd_AtAnyFrameRate()
        {
            foreach (float frame in new[] { 1f / 30f, 1f / 60f, 1f / 144f, 0.07f, 0.2f })
            {
                DashRules rules = StartedFullDash();
                RunToEnd(rules, frame);
                Assert.That(rules.Travelled, Is.EqualTo(5f), $"frame {frame}");
            }
        }

        [Test]
        public void Kill_ReadyAtDashEnd_NoCooldown()
        {
            DashRules rules = StartedFullDash();
            var events = new List<bool>();
            rules.ReadinessChanged += events.Add;

            rules.Tick(0.05f);
            rules.RegisterKill();
            Assert.That(rules.IsReady, Is.False, "the kill readies the next dash only when this one ends");

            RunToEnd(rules);
            Assert.That(rules.EndDash(MissCooldown), Is.True);

            Assert.That(rules.State, Is.EqualTo(DashState.Ready));
            Assert.That(rules.IsInvulnerable, Is.False);
            Assert.That(events, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void Miss_CooldownFromDashEnd_ThenReady()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0.15f);
            rules.EndDash(MissCooldown);

            Assert.That(rules.State, Is.EqualTo(DashState.Cooldown));
            Assert.That(rules.IsInvulnerable, Is.False, "off during the cooldown");

            rules.Tick(0.6f);
            Assert.That(rules.IsReady, Is.False);
            rules.Tick(0.39f);
            Assert.That(rules.IsReady, Is.False);
            rules.Tick(0.02f);
            Assert.That(rules.IsReady, Is.True);
        }

        [Test]
        public void Miss_FrameOvershootCountsTowardsCooldown()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0.2f); // The dash's time ran out 0.05 s into this frame.
            rules.EndDash(MissCooldown);
            Assert.That(rules.CooldownRemaining, Is.EqualTo(0.95f).Within(Tolerance));
        }

        [Test]
        public void ClicksDuringCooldown_IgnoredAndDoNotExtendIt()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0.15f);
            rules.EndDash(MissCooldown);

            rules.Tick(0.2f);
            Assert.That(rules.TryStartDash(Distance, Duration), Is.False);
            rules.Tick(0.2f);
            Assert.That(rules.TryStartDash(Distance, Duration), Is.False);
            Assert.That(rules.TryStartDash(Distance, Duration), Is.False);

            Assert.That(rules.State, Is.EqualTo(DashState.Cooldown));
            Assert.That(rules.CooldownRemaining, Is.EqualTo(0.6f).Within(Tolerance));
            rules.Tick(0.59f);
            Assert.That(rules.IsReady, Is.False);
            rules.Tick(0.02f);
            Assert.That(rules.IsReady, Is.True);
        }

        [Test]
        public void ClicksDuringDash_Ignored()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0.05f);
            Assert.That(rules.TryStartDash(1f, 0.03f), Is.False);
            Assert.That(rules.PathLength, Is.EqualTo(Distance));
            Assert.That(rules.Elapsed, Is.EqualTo(0.05f).Within(Tolerance));
        }

        [Test]
        public void KillsDoNotStack()
        {
            DashRules rules = StartedFullDash();
            rules.RegisterKill();
            rules.RegisterKill();
            RunToEnd(rules);
            rules.EndDash(MissCooldown);
            Assert.That(rules.IsReady, Is.True);

            // One ready dash; the next one without a kill is a miss.
            Assert.That(rules.TryStartDash(Distance, Duration), Is.True);
            Assert.That(rules.TryStartDash(Distance, Duration), Is.False);
            RunToEnd(rules);
            rules.EndDash(MissCooldown);
            Assert.That(rules.State, Is.EqualTo(DashState.Cooldown));
        }

        [Test]
        public void KillOutsideADash_IsIgnored()
        {
            var rules = new DashRules();
            rules.RegisterKill();
            Assert.That(rules.KilledThisDash, Is.False);

            rules.TryStartDash(Distance, Duration);
            Assert.That(rules.KilledThisDash, Is.False);
        }

        [Test]
        public void ZeroLengthDash_CompleteAtOnce_MissStartsCooldown()
        {
            var rules = new DashRules();
            rules.TryStartDash(0f, DashRules.DashTimeFor(0f, Distance, Duration));
            Assert.That(rules.IsInvulnerable, Is.True);
            Assert.That(rules.IsDashComplete, Is.True);

            rules.EndDash(MissCooldown);
            Assert.That(rules.IsInvulnerable, Is.False);
            Assert.That(rules.State, Is.EqualTo(DashState.Cooldown));
            Assert.That(rules.CooldownRemaining, Is.EqualTo(MissCooldown));
        }

        [Test]
        public void ZeroLengthDash_WithKill_ReadyAtOnce()
        {
            var rules = new DashRules();
            rules.TryStartDash(0f, 0f);
            rules.RegisterKill();
            rules.EndDash(MissCooldown);
            Assert.That(rules.IsReady, Is.True);
        }

        [Test]
        public void EndDash_BeforeTimeIsUp_DoesNothing()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0.1f);
            Assert.That(rules.EndDash(MissCooldown), Is.False);
            Assert.That(rules.State, Is.EqualTo(DashState.Dashing));
        }

        [Test]
        public void CooldownLengthComesFromTheValuePassed()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0.15f);
            rules.EndDash(5f);
            rules.Tick(4.9f);
            Assert.That(rules.IsReady, Is.False);
            rules.Tick(0.2f);
            Assert.That(rules.IsReady, Is.True);
        }

        [Test]
        public void SixtyFramesOfOneSixtieth_EndTheCooldownOnTheSixtiethFrame()
        {
            DashRules rules = StartedFullDash();
            int dashFrames = 0;
            while (!rules.IsDashComplete)
            {
                rules.Tick(1f / 60f);
                dashFrames++;
            }

            Assert.That(dashFrames, Is.EqualTo(9), "0.15 s at 60 fps");
            rules.EndDash(MissCooldown);

            int cooldownFrames = 0;
            while (!rules.IsReady)
            {
                rules.Tick(1f / 60f);
                cooldownFrames++;
            }

            Assert.That(cooldownFrames, Is.EqualTo(60));
        }

        [Test]
        public void CancelDash_WithoutKill_EndsAtOnce_FullMissCooldown()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0.05f);

            Assert.That(rules.CancelDash(MissCooldown), Is.True);
            Assert.That(rules.IsInvulnerable, Is.False);
            Assert.That(rules.State, Is.EqualTo(DashState.Cooldown));
            Assert.That(rules.CooldownRemaining, Is.EqualTo(MissCooldown));
        }

        [Test]
        public void CancelDash_AfterKill_ReadyAtOnce()
        {
            DashRules rules = StartedFullDash();
            var events = new List<bool>();
            rules.ReadinessChanged += events.Add;
            rules.RegisterKill();

            Assert.That(rules.CancelDash(MissCooldown), Is.True);
            Assert.That(rules.IsReady, Is.True);
            Assert.That(events, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void CancelDash_WhenNotDashing_ChangesNothing()
        {
            var rules = new DashRules();
            Assert.That(rules.CancelDash(MissCooldown), Is.False);
            Assert.That(rules.State, Is.EqualTo(DashState.Ready));

            rules.TryStartDash(0f, 0f);
            rules.EndDash(MissCooldown);
            rules.Tick(0.3f);
            Assert.That(rules.CancelDash(MissCooldown), Is.False);
            Assert.That(rules.State, Is.EqualTo(DashState.Cooldown));
            Assert.That(rules.CooldownRemaining, Is.EqualTo(0.7f).Within(Tolerance), "a running cooldown is not reset");
        }

        [Test]
        public void ZeroOrNegativeDeltaTime_ChangesNothing()
        {
            DashRules rules = StartedFullDash();
            rules.Tick(0f);
            rules.Tick(-1f);
            Assert.That(rules.Elapsed, Is.EqualTo(0f));
        }
    }
}
