using Dashblade.Combat;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class DeathFadeTimerTests
    {
        private const float FadeTime = 0.3f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void Alive_IsOpaque_AndNotFinished()
        {
            var fade = new DeathFadeTimer();
            Assert.That(fade.IsStarted, Is.False);
            Assert.That(fade.Alpha(FadeTime), Is.EqualTo(1f));
            Assert.That(fade.IsFinished(FadeTime), Is.False);
        }

        [Test]
        public void StartsOnlyOnce()
        {
            var fade = new DeathFadeTimer();
            Assert.That(fade.TryStart(), Is.True);
            fade.Tick(0.1f);
            Assert.That(fade.TryStart(), Is.False);
            Assert.That(fade.Elapsed, Is.EqualTo(0.1f).Within(Tolerance), "a second hit does not restart the fade");
        }

        [Test]
        public void FadesLinearly_FinishedAtFadeTime()
        {
            var fade = new DeathFadeTimer();
            fade.TryStart();
            Assert.That(fade.Alpha(FadeTime), Is.EqualTo(1f));

            fade.Tick(0.15f);
            Assert.That(fade.Alpha(FadeTime), Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(fade.IsFinished(FadeTime), Is.False);

            fade.Tick(0.15f);
            Assert.That(fade.Alpha(FadeTime), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(fade.IsFinished(FadeTime), Is.True);
        }

        [Test]
        public void EighteenFramesOfOneSixtieth_FinishTheFade()
        {
            var fade = new DeathFadeTimer();
            fade.TryStart();
            for (int i = 0; i < 17; i++)
            {
                fade.Tick(1f / 60f);
            }

            Assert.That(fade.IsFinished(FadeTime), Is.False);
            fade.Tick(1f / 60f);
            Assert.That(fade.IsFinished(FadeTime), Is.True);
        }

        [Test]
        public void TickBeforeStart_DoesNothing()
        {
            var fade = new DeathFadeTimer();
            fade.Tick(1f);
            Assert.That(fade.Elapsed, Is.EqualTo(0f));
        }

        [Test]
        public void ZeroFadeTime_GoneAtOnce()
        {
            var fade = new DeathFadeTimer();
            fade.TryStart();
            Assert.That(fade.Alpha(0f), Is.EqualTo(0f));
            Assert.That(fade.IsFinished(0f), Is.True);
        }
    }
}
