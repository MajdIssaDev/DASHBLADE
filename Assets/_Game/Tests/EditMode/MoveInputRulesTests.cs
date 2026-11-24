using Dashblade.Player;
using NUnit.Framework;
using UnityEngine;

namespace Dashblade.Tests.EditMode
{
    public sealed class MoveInputRulesTests
    {
        private const float Tolerance = 0.0001f;

        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance), "y");
        }

        [Test]
        public void NoKeys_GivesZero()
        {
            AssertVector(Vector2.zero, MoveInputRules.Direction(false, false, false, false));
        }

        [Test]
        public void SingleKeys_GiveTheFourScreenDirections()
        {
            AssertVector(Vector2.up, MoveInputRules.Direction(up: true, down: false, left: false, right: false));
            AssertVector(Vector2.down, MoveInputRules.Direction(up: false, down: true, left: false, right: false));
            AssertVector(Vector2.left, MoveInputRules.Direction(up: false, down: false, left: true, right: false));
            AssertVector(Vector2.right, MoveInputRules.Direction(up: false, down: false, left: false, right: true));
        }

        [Test]
        public void Diagonal_IsNormalised()
        {
            Vector2 direction = MoveInputRules.Direction(up: true, down: false, left: false, right: true);
            Assert.That(direction.magnitude, Is.EqualTo(1f).Within(Tolerance));
            // At 6 units/s this is about (4.24, 4.24).
            AssertVector(new Vector2(4.2426f, 4.2426f), direction * 6f);
        }

        [Test]
        public void OppositeKeys_CancelOnTheirAxis()
        {
            AssertVector(Vector2.zero, MoveInputRules.Direction(up: false, down: false, left: true, right: true));
            AssertVector(Vector2.zero, MoveInputRules.Direction(up: true, down: true, left: false, right: false));
        }

        [Test]
        public void LeftRightAndUp_MovesStraightUpAtFullSpeed()
        {
            Vector2 direction = MoveInputRules.Direction(up: true, down: false, left: true, right: true);
            AssertVector(Vector2.up, direction);
            Assert.That((direction * 6f).magnitude, Is.EqualTo(6f).Within(Tolerance));
        }

        [Test]
        public void AllFourKeys_StandStill()
        {
            AssertVector(Vector2.zero, MoveInputRules.Direction(true, true, true, true));
        }
    }
}
