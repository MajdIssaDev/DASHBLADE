using Dashblade.Combat;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    /// <summary>F06 criterion 14: left-clicks do not start a dash for the first Start Lockout seconds of a run.</summary>
    public sealed class DashStartLockoutTests
    {
        private const float Lockout = 0.5f;

        [Test]
        public void LockedFromTheStart()
        {
            var lockout = new DashStartLockout(Lockout);
            Assert.That(lockout.IsLocked, Is.True);
            Assert.That(lockout.Elapsed, Is.EqualTo(0f));
            Assert.That(lockout.Duration, Is.EqualTo(Lockout));
        }

        [Test]
        public void StillLockedAt04Seconds()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(0.4f);
            Assert.That(lockout.IsLocked, Is.True);
        }

        [Test]
        public void OverAt05Seconds_AndStaysOver()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(0.3f);
            lockout.Tick(0.2f);
            Assert.That(lockout.IsLocked, Is.False);

            lockout.Tick(10f);
            Assert.That(lockout.IsLocked, Is.False);
        }

        [Test]
        public void OverAt06Seconds()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(0.6f);
            Assert.That(lockout.IsLocked, Is.False);
        }

        [Test]
        public void ThirtyFramesOfOneSixtieth_EndItOnTheThirtiethFrame()
        {
            var lockout = new DashStartLockout(Lockout);
            for (int frame = 1; frame <= 30; frame++)
            {
                lockout.Tick(1f / 60f);
                Assert.That(lockout.IsLocked, Is.EqualTo(frame < 30), $"frame {frame}");
            }
        }

        [Test]
        public void ZeroMeansNoLockout()
        {
            Assert.That(new DashStartLockout(0f).IsLocked, Is.False);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void NegativeOrNaN_MeansNoLockout(float duration)
        {
            var lockout = new DashStartLockout(duration);
            Assert.That(lockout.Duration, Is.EqualTo(0f));
            Assert.That(lockout.IsLocked, Is.False);
        }

        [Test]
        public void FrozenTime_DoesNotCount()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(0f);
            lockout.Tick(-1f);
            Assert.That(lockout.Elapsed, Is.EqualTo(0f));
            Assert.That(lockout.IsLocked, Is.True);
        }

        [Test]
        public void LongerLockout_FromTheValuePassed()
        {
            var lockout = new DashStartLockout(2f);
            lockout.Tick(1.9f);
            Assert.That(lockout.IsLocked, Is.True);
            lockout.Tick(0.1f);
            Assert.That(lockout.IsLocked, Is.False);
        }

        // F11: the pause panel's Resume button starts the lockout again.
        [Test]
        public void Restart_AfterTheLockoutIsOver_LocksForTheFullDurationAgain()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(3f);
            Assert.That(lockout.IsLocked, Is.False);

            lockout.Restart();
            Assert.That(lockout.Elapsed, Is.EqualTo(0f));
            Assert.That(lockout.IsLocked, Is.True);

            lockout.Tick(0.4f);
            Assert.That(lockout.IsLocked, Is.True, "0.4 s after the restart");
            lockout.Tick(0.1f);
            Assert.That(lockout.IsLocked, Is.False, "0.5 s after the restart");
        }

        [Test]
        public void Restart_DuringTheLockout_CountsTheFullDurationFromTheRestart()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(0.3f);
            lockout.Restart();
            lockout.Tick(0.3f);
            Assert.That(lockout.IsLocked, Is.True, "Only 0.3 s since the restart.");
            lockout.Tick(0.2f);
            Assert.That(lockout.IsLocked, Is.False);
        }

        [Test]
        public void Restart_ThirtyFramesOfOneSixtieth_EndItOnTheThirtiethFrame()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(1f);
            lockout.Restart();
            for (int frame = 1; frame <= 30; frame++)
            {
                lockout.Tick(1f / 60f);
                Assert.That(lockout.IsLocked, Is.EqualTo(frame < 30), $"frame {frame}");
            }
        }

        [Test]
        public void Restart_FrozenTimeDoesNotCount()
        {
            var lockout = new DashStartLockout(Lockout);
            lockout.Tick(1f);
            lockout.Restart();
            for (int frame = 0; frame < 100; frame++)
            {
                lockout.Tick(0f);
            }

            Assert.That(lockout.IsLocked, Is.True);
        }

        [Test]
        public void Restart_WithNoLockout_StaysUnlocked()
        {
            var lockout = new DashStartLockout(0f);
            lockout.Restart();
            Assert.That(lockout.IsLocked, Is.False);
        }

        [Test]
        public void TheLockoutDoesNotChangeTheDashsReadiness()
        {
            // The two are separate: while locked the dash is still Ready with a full bar, and nothing is raised.
            var rules = new DashRules();
            var lockout = new DashStartLockout(Lockout);
            int raised = 0;
            rules.ReadinessChanged += _ => raised++;

            for (int frame = 0; frame < 20; frame++)
            {
                lockout.Tick(1f / 60f);
                rules.Tick(1f / 60f);
                Assert.That(lockout.IsLocked, Is.True);
                Assert.That(rules.IsReady, Is.True);
                Assert.That(rules.RechargeProgress, Is.EqualTo(1f));
            }

            Assert.That(raised, Is.EqualTo(0));
        }
    }
}
