using Dashblade.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Dashblade.Tests.EditMode
{
    public sealed class DashPathTests
    {
        private const float Tolerance = 0.0001f;
        private const float StartContact = 0.02f;
        private const float Skin = 0.01f;

        [Test]
        public void End_IsStartPlusLengthAlongDirection()
        {
            var path = new DashPath(new Vector2(-10.5f, 0f), Vector2.right, 5f);
            Assert.That(path.End.x, Is.EqualTo(-5.5f).Within(Tolerance));
            Assert.That(path.End.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Direction_IsNormalised()
        {
            var path = new DashPath(Vector2.zero, new Vector2(3f, 4f), 5f);
            Assert.That(path.Direction.magnitude, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(path.End.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(path.End.y, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void PointAt_IsOnThePath_AndNeverOvershoots()
        {
            var path = new DashPath(new Vector2(-0.5f, 1f), Vector2.right, 5f);
            Assert.That(path.PointAt(0f), Is.EqualTo(path.Start));
            Assert.That(path.PointAt(-1f), Is.EqualTo(path.Start));
            Assert.That(path.PointAt(2f).x, Is.EqualTo(1.5f).Within(Tolerance));
            Assert.That(path.PointAt(2f).y, Is.EqualTo(1f), "y does not change on a horizontal dash");
            Assert.That(path.PointAt(5f), Is.EqualTo(path.End));
            Assert.That(path.PointAt(7f), Is.EqualTo(path.End));
        }

        [Test]
        public void ZeroLength_EndIsStart()
        {
            var path = new DashPath(new Vector2(11.5f, 0f), Vector2.right, 0f);
            Assert.That(path.End, Is.EqualTo(path.Start));
            Assert.That(path.PointAt(1f), Is.EqualTo(path.Start));
        }

        [Test]
        public void WallAhead_StopsJustShortOfTheReportedHit()
        {
            Assert.That(DashWallRules.TryGetStop(2.005f, StartContact, Skin, Vector2.right, new Vector2(2.5f, 0f), out float stop), Is.True);
            Assert.That(stop, Is.EqualTo(1.995f).Within(Tolerance));
        }

        [Test]
        public void WallAhead_NeverGivesANegativeStop()
        {
            Assert.That(DashWallRules.TryGetStop(0.025f, StartContact, 0.05f, Vector2.right, new Vector2(0.6f, 0f), out float stop), Is.True);
            Assert.That(stop, Is.EqualTo(0f));
        }

        [Test]
        public void TouchedWall_DashStraightIntoIt_ZeroLength()
        {
            // Touching the right wall, dashing right.
            Assert.That(DashWallRules.TryGetStop(0.01f, StartContact, Skin, Vector2.right, new Vector2(0.505f, 0f), out float stop), Is.True);
            Assert.That(stop, Is.EqualTo(0f));
        }

        [Test]
        public void TouchedWall_DashAlongIt_DoesNotBlock()
        {
            // Touching the top wall, dashing right.
            Assert.That(DashWallRules.TryGetStop(0f, StartContact, Skin, Vector2.right, new Vector2(0f, 0.5f), out _), Is.False);
        }

        [Test]
        public void TouchedWall_DashAwayFromIt_DoesNotBlock()
        {
            Vector2 awayAndRight = new Vector2(1f, -1f).normalized;
            Assert.That(DashWallRules.TryGetStop(0.01f, StartContact, Skin, awayAndRight, new Vector2(0f, 0.5f), out _), Is.False);
        }

        [Test]
        public void TouchedWall_DashIntoItAtAnAngle_ZeroLength()
        {
            Vector2 intoAndRight = new Vector2(1f, 0.2f).normalized;
            Assert.That(DashWallRules.TryGetStop(0f, StartContact, Skin, intoAndRight, new Vector2(0f, 0.5f), out float stop), Is.True);
            Assert.That(stop, Is.EqualTo(0f));
        }

        [Test]
        public void CentreInsideWall_Blocks()
        {
            Assert.That(DashWallRules.StartContactBlocks(Vector2.right, Vector2.zero), Is.True);
        }
    }
}
