using Dashblade.Combat;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class ProtectionBlinkTests
    {
        private const float ProtectionTime = 0.75f;
        private const float BlinkInterval = 0.1f;

        [Test]
        public void Alternates_HalfThenSolid_Every01()
        {
            // Mid-interval samples, so float rounding at the boundaries does not matter.
            float[] times = { 0f, 0.05f, 0.15f, 0.25f, 0.35f, 0.45f, 0.55f, 0.65f, 0.72f };
            float[] expected = { 0.5f, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f };
            for (int i = 0; i < times.Length; i++)
            {
                Assert.That(ProtectionBlink.Alpha(times[i], ProtectionTime, BlinkInterval), Is.EqualTo(expected[i]), $"t = {times[i]}");
            }
        }

        [Test]
        public void SolidOnceProtectionTimeIsUsedUp()
        {
            Assert.That(ProtectionBlink.Alpha(0.75f, ProtectionTime, BlinkInterval), Is.EqualTo(1f));
            Assert.That(ProtectionBlink.Alpha(0.9f, ProtectionTime, BlinkInterval), Is.EqualTo(1f));
        }

        [Test]
        public void Solid_ForNegativeTimeOrNonPositiveInterval()
        {
            Assert.That(ProtectionBlink.Alpha(-0.05f, ProtectionTime, BlinkInterval), Is.EqualTo(1f));
            Assert.That(ProtectionBlink.Alpha(0.05f, ProtectionTime, 0f), Is.EqualTo(1f));
        }

        [Test]
        public void IntervalComesFromTheValuePassed()
        {
            Assert.That(ProtectionBlink.Alpha(0.2f, 3f, 0.25f), Is.EqualTo(0.5f));
            Assert.That(ProtectionBlink.Alpha(0.3f, 3f, 0.25f), Is.EqualTo(1f));
            Assert.That(ProtectionBlink.Alpha(2.95f, 3f, 0.1f), Is.EqualTo(1f), "blinks for the whole 3 s protection");
            Assert.That(ProtectionBlink.Alpha(2.85f, 3f, 0.1f), Is.EqualTo(0.5f));
        }

        [Test]
        public void BodyAlpha_SolidWhenNotProtected()
        {
            var rules = new HeartsRules(3, ProtectionTime);
            Assert.That(rules.BodyAlpha(BlinkInterval), Is.EqualTo(1f));
        }

        [Test]
        public void BodyAlpha_HalfOnTheHitFrame_SolidOnTheFrameProtectionEnds()
        {
            var rules = new HeartsRules(3, ProtectionTime);
            rules.TryHit(false);
            Assert.That(rules.BodyAlpha(BlinkInterval), Is.EqualTo(0.5f), "the hit shows at once");

            rules.Tick(0.15f);
            Assert.That(rules.BodyAlpha(BlinkInterval), Is.EqualTo(1f));

            rules.Tick(0.59f);
            Assert.That(rules.IsProtected, Is.True);
            rules.Tick(0.02f);
            Assert.That(rules.IsProtected, Is.False);
            Assert.That(rules.BodyAlpha(BlinkInterval), Is.EqualTo(1f));
        }

        [Test]
        public void BodyAlpha_FinalHit_NoBlink()
        {
            var rules = new HeartsRules(1, ProtectionTime);
            rules.TryHit(false);
            Assert.That(rules.BodyAlpha(BlinkInterval), Is.EqualTo(1f), "the defeat fade takes over, not the blink");
        }
    }
}
