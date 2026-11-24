using Dashblade.Player;
using NUnit.Framework;
using UnityEngine;

namespace Dashblade.Tests.EditMode
{
    public sealed class AimTrackerTests
    {
        private const float DeadZone = 0.5f;
        private const float AngleTolerance = 0.01f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void StartsFacingRight()
        {
            var tracker = new AimTracker();
            Assert.That(tracker.Direction, Is.EqualTo(Vector2.right));
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));
        }

        [Test]
        public void SnapsToCursor_AboveLeftBelow()
        {
            var tracker = new AimTracker();
            var player = new Vector2(-10.5f, 0f);

            tracker.Update(player, player + new Vector2(0f, 3f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(90f).Within(AngleTolerance));

            tracker.Update(player, player + new Vector2(-3f, 0f), DeadZone);
            Assert.That(Mathf.Abs(tracker.AngleDegrees), Is.EqualTo(180f).Within(AngleTolerance));

            tracker.Update(player, player + new Vector2(0f, -3f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(-90f).Within(AngleTolerance));
        }

        [Test]
        public void DirectionIsUnitLength()
        {
            var tracker = new AimTracker();
            tracker.Update(Vector2.zero, new Vector2(3f, 4f), DeadZone);
            Assert.That(tracker.Direction.magnitude, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(tracker.Direction.x, Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(tracker.Direction.y, Is.EqualTo(0.8f).Within(Tolerance));
        }

        [Test]
        public void InsideDeadZone_KeepsLastFacing()
        {
            var tracker = new AimTracker();
            tracker.Update(Vector2.zero, new Vector2(5f, 0f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));

            tracker.Update(Vector2.zero, new Vector2(0f, 0.3f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));

            // (0.3, 0.3) is about 0.42 units away, still inside.
            tracker.Update(Vector2.zero, new Vector2(0.3f, 0.3f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));

            tracker.Update(Vector2.zero, new Vector2(0f, 0.6f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(90f).Within(AngleTolerance));
        }

        [Test]
        public void ExactlyAtDeadZone_Tracks()
        {
            var tracker = new AimTracker();
            tracker.Update(Vector2.zero, new Vector2(0f, 0.5f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(90f).Within(AngleTolerance));
        }

        [Test]
        public void CursorOnPlayerCentre_KeepsLastFacing()
        {
            var tracker = new AimTracker();
            tracker.Update(Vector2.zero, new Vector2(0f, -2f), DeadZone);
            tracker.Update(Vector2.zero, Vector2.zero, DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(-90f).Within(AngleTolerance));
        }

        [Test]
        public void NoCursorAtStart_FacesRight()
        {
            var tracker = new AimTracker();
            tracker.Update(Vector2.zero, null, DeadZone);
            Assert.That(tracker.Direction, Is.EqualTo(Vector2.right));
        }

        [Test]
        public void NoCursorReported_KeepsLastFacing()
        {
            var tracker = new AimTracker();
            tracker.Update(Vector2.zero, new Vector2(-4f, 0f), DeadZone);
            tracker.Update(Vector2.zero, null, DeadZone);
            Assert.That(Mathf.Abs(tracker.AngleDegrees), Is.EqualTo(180f).Within(AngleTolerance));
        }

        [Test]
        public void CursorFarOutsideThePicture_StillTracks()
        {
            var tracker = new AimTracker();
            tracker.Update(Vector2.zero, new Vector2(0f, 40f), DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(90f).Within(AngleTolerance));
        }

        [Test]
        public void StillCursor_PlayerMoving_RecalculatesEveryUpdate()
        {
            var tracker = new AimTracker();
            var cursor = new Vector2(0f, 0f);

            // Player at (-3, 0) with the cursor to its right.
            tracker.Update(new Vector2(-3f, 0f), cursor, DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));

            // Player walks over the cursor: facing holds while within 0.5 units.
            tracker.Update(new Vector2(-0.2f, 0f), cursor, DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));
            tracker.Update(new Vector2(0.4f, 0f), cursor, DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));

            // On the way out it snaps to the new direction (a 180 degree flip in one update).
            tracker.Update(new Vector2(0.6f, 0f), cursor, DeadZone);
            Assert.That(Mathf.Abs(tracker.AngleDegrees), Is.EqualTo(180f).Within(AngleTolerance));
        }

        [Test]
        public void PlayerBacksAway_FacingUnchanged()
        {
            var tracker = new AimTracker();
            var cursor = new Vector2(5f, 0f);
            tracker.Update(Vector2.zero, cursor, DeadZone);
            tracker.Update(new Vector2(-1f, 0f), cursor, DeadZone);
            tracker.Update(new Vector2(-2f, 0f), cursor, DeadZone);
            Assert.That(tracker.AngleDegrees, Is.EqualTo(0f).Within(AngleTolerance));
        }
    }
}
