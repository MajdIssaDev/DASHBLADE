using Dashblade.Display;
using NUnit.Framework;
using UnityEngine;

namespace Dashblade.Tests.EditMode
{
    public sealed class LetterboxMathTests
    {
        private const float Tolerance = 0.0001f;

        private static void AssertRect(Rect expected, Rect actual)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance), "y");
            Assert.That(actual.width, Is.EqualTo(expected.width).Within(Tolerance), "width");
            Assert.That(actual.height, Is.EqualTo(expected.height).Within(Tolerance), "height");
        }

        [Test]
        public void SixteenByNine_FillsTheScreen()
        {
            AssertRect(new Rect(0f, 0f, 1f, 1f), LetterboxMath.Viewport(1920f, 1080f, LetterboxMath.SixteenByNine));
        }

        [Test]
        public void FourByThree_BarsTopAndBottom()
        {
            // 1024 x 768: picture is 1024 x 576, centred, so 96 px bars top and bottom.
            Rect rect = LetterboxMath.Viewport(1024f, 768f, LetterboxMath.SixteenByNine);
            AssertRect(new Rect(0f, 0.125f, 1f, 0.75f), rect);
            Assert.That(rect.width * 1024f / (rect.height * 768f), Is.EqualTo(16f / 9f).Within(Tolerance));
        }

        [Test]
        public void TwentyOneByNine_BarsLeftAndRight()
        {
            // 2560 x 1080: picture is 1920 x 1080, centred, so 320 px bars left and right.
            Rect rect = LetterboxMath.Viewport(2560f, 1080f, LetterboxMath.SixteenByNine);
            AssertRect(new Rect(0.125f, 0f, 0.75f, 1f), rect);
        }

        [Test]
        public void ResizedShape_GivesNewRect()
        {
            Rect first = LetterboxMath.Viewport(1024f, 768f, LetterboxMath.SixteenByNine);
            Rect second = LetterboxMath.Viewport(2560f, 1080f, LetterboxMath.SixteenByNine);
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [TestCase(0f, 1080f)]
        [TestCase(1920f, 0f)]
        [TestCase(-5f, 100f)]
        public void InvalidSize_ReturnsFullScreen(float width, float height)
        {
            AssertRect(new Rect(0f, 0f, 1f, 1f), LetterboxMath.Viewport(width, height, LetterboxMath.SixteenByNine));
        }

        [Test]
        public void VisibleWorldArea_IsAlways26_67By15_AtSize7_5()
        {
            const float orthographicSize = 7.5f;
            foreach (var size in new[] { new Vector2(1920f, 1080f), new Vector2(1024f, 768f), new Vector2(2560f, 1080f), new Vector2(800f, 800f) })
            {
                Rect rect = LetterboxMath.Viewport(size.x, size.y, LetterboxMath.SixteenByNine);
                float pictureAspect = rect.width * size.x / (rect.height * size.y);
                float visibleHeight = orthographicSize * 2f;
                float visibleWidth = visibleHeight * pictureAspect;
                Assert.That(visibleHeight, Is.EqualTo(15f).Within(Tolerance));
                Assert.That(visibleWidth, Is.EqualTo(26.6667f).Within(0.001f), $"at {size}");
            }
        }
    }
}
