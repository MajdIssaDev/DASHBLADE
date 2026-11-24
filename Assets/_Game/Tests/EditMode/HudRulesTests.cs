using System.Linq;
using Dashblade.UI;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    /// <summary>F07 HUD rules: heart squares, wave text, dash bar fill and the HUD's drawing order.</summary>
    public sealed class HudRulesTests
    {
        private const int Squares = 3;

        private static bool[] Solid(int hearts)
        {
            return Enumerable.Range(0, Squares).Select(slot => HudRules.IsHeartSolid(slot, hearts)).ToArray();
        }

        [Test]
        public void ThreeHearts_ThreeSolidSquares()
        {
            Assert.That(Solid(3), Is.EqualTo(new[] { true, true, true }));
        }

        [Test]
        public void HeartsAreLostFromTheRight()
        {
            Assert.That(Solid(2), Is.EqualTo(new[] { true, true, false }), "right-most square hollow first");
            Assert.That(Solid(1), Is.EqualTo(new[] { true, false, false }));
        }

        [Test]
        public void NoHearts_ThreeHollowSquares()
        {
            Assert.That(Solid(0), Is.EqualTo(new[] { false, false, false }));
        }

        [Test]
        public void RestoredHearts_AllSolidAgainAtOnce()
        {
            Assert.That(Solid(1), Is.EqualTo(new[] { true, false, false }));
            Assert.That(Solid(3), Is.EqualTo(new[] { true, true, true }));
        }

        [Test]
        public void OutOfRangeHearts_NeverChangeTheSquareCount()
        {
            Assert.That(Solid(-1), Is.EqualTo(new[] { false, false, false }));
            Assert.That(Solid(5), Is.EqualTo(new[] { true, true, true }));
            Assert.That(HudRules.IsHeartSolid(-1, 3), Is.False);
        }

        [TestCase(1, 3, "Wave 1 / 3")]
        [TestCase(2, 3, "Wave 2 / 3")]
        [TestCase(3, 3, "Wave 3 / 3")]
        public void WaveText_ReadsCurrentOfCount(int current, int count, string expected)
        {
            Assert.That(HudRules.WaveText(current, count), Is.EqualTo(expected));
        }

        [TestCase(0f, 0f)]
        [TestCase(0.5f, 0.5f)]
        [TestCase(1f, 1f)]
        [TestCase(-0.2f, 0f)]
        [TestCase(1.3f, 1f)]
        public void DashBarFill_IsTheRechargeProgressClamped(float progress, float expected)
        {
            Assert.That(HudRules.DashBarFill(progress), Is.EqualTo(expected));
        }

        [Test]
        public void HudOrder_IsAboveEveryArenaSpriteAndBelowTheEndingCanvas()
        {
            Assert.That(UiSorting.HudOrder, Is.InRange(4, 99));
            Assert.That(UiSorting.HudOrder, Is.LessThan(UiSorting.EndingOrder));
        }
    }
}
