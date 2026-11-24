using Dashblade.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Dashblade.Tests.EditMode
{
    public sealed class SweptCircleTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void HeadOn_FirstContactAtCentreDistanceMinusRadius()
        {
            bool hit = SweptCircle.TryFirstContact(Vector2.zero, Vector2.right, 10f, new Vector2(5f, 0f), 1f, out float distance);
            Assert.That(hit, Is.True);
            Assert.That(distance, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void StartingInside_ContactAtZero()
        {
            bool hit = SweptCircle.TryFirstContact(new Vector2(4.5f, 0f), Vector2.right, 10f, new Vector2(5f, 0f), 1f, out float distance);
            Assert.That(hit, Is.True);
            Assert.That(distance, Is.EqualTo(0f));
        }

        [Test]
        public void StartingInside_ZeroLength_StillContact()
        {
            Assert.That(SweptCircle.TryFirstContact(new Vector2(4.5f, 0f), Vector2.right, 0f, new Vector2(5f, 0f), 1f, out _), Is.True);
        }

        [Test]
        public void TooShort_NoContact()
        {
            Assert.That(SweptCircle.TryFirstContact(Vector2.zero, Vector2.right, 3.99f, new Vector2(5f, 0f), 1f, out _), Is.False);
        }

        [Test]
        public void ExactlyReachingTheEdge_CountsAsContact()
        {
            Assert.That(SweptCircle.TryFirstContact(Vector2.zero, Vector2.right, 4f, new Vector2(5f, 0f), 1f, out float distance), Is.True);
            Assert.That(distance, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void MovingAway_NoContact()
        {
            Assert.That(SweptCircle.TryFirstContact(Vector2.zero, Vector2.left, 10f, new Vector2(5f, 0f), 1f, out _), Is.False);
        }

        [Test]
        public void PassingBeside_NoContact()
        {
            Assert.That(SweptCircle.TryFirstContact(Vector2.zero, Vector2.right, 10f, new Vector2(5f, 1.01f), 1f, out _), Is.False);
        }

        [Test]
        public void GrazingTheSide_ContactAtTheClosestPoint()
        {
            bool hit = SweptCircle.TryFirstContact(Vector2.zero, Vector2.right, 10f, new Vector2(5f, 1f), 1f, out float distance);
            Assert.That(hit, Is.True);
            Assert.That(distance, Is.EqualTo(5f).Within(0.01f));
        }

        [Test]
        public void LongStepAtLowFrameRate_DoesNotSkipTheCircle()
        {
            // A whole 0.5 s frame of a 15 u/s charge passes straight over a player 3 units ahead.
            bool hit = SweptCircle.TryFirstContact(Vector2.zero, Vector2.left, 7.5f, new Vector2(-3f, 0.2f), 1f, out float distance);
            Assert.That(hit, Is.True);
            Assert.That(distance, Is.EqualTo(3f - Mathf.Sqrt(1f - 0.04f)).Within(Tolerance));
        }

        [Test]
        public void DiagonalDirection_UsesTheGivenUnitVector()
        {
            Vector2 direction = new Vector2(1f, 1f).normalized;
            bool hit = SweptCircle.TryFirstContact(Vector2.zero, direction, 10f, new Vector2(3f, 3f), 0.5f, out float distance);
            Assert.That(hit, Is.True);
            Assert.That(distance, Is.EqualTo(Mathf.Sqrt(18f) - 0.5f).Within(Tolerance));
        }

        [Test]
        public void NegativeRadius_NoContact()
        {
            Assert.That(SweptCircle.TryFirstContact(Vector2.zero, Vector2.right, 10f, Vector2.zero, -1f, out _), Is.False);
        }
    }
}
