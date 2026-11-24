using System.Collections.Generic;
using Dashblade.Combat;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class DashHitQueueTests
    {
        private sealed class Target
        {
            public Target(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public override string ToString() => Name;
        }

        [Test]
        public void HandsOutInPathOrder_AsTheBodyGetsThere()
        {
            var near = new Target("near");
            var far = new Target("far");
            var queue = new DashHitQueue<Target>();
            queue.Add(far, 2.875f);
            queue.Add(near, 1.375f);
            var reached = new List<Target>();

            queue.CollectReached(1f, reached);
            Assert.That(reached, Is.Empty);

            queue.CollectReached(1.375f, reached);
            Assert.That(reached, Is.EqualTo(new[] { near }));

            queue.CollectReached(2.5f, reached);
            Assert.That(reached, Is.EqualTo(new[] { near }));

            queue.CollectReached(3f, reached);
            Assert.That(reached, Is.EqualTo(new[] { near, far }));
            Assert.That(queue.PendingCount, Is.EqualTo(0));
        }

        [Test]
        public void TargetAtTheStart_IsReachedOnTheFirstFrame()
        {
            var underneath = new Target("underneath");
            var queue = new DashHitQueue<Target>();
            queue.Add(underneath, 0f);
            var reached = new List<Target>();

            queue.CollectReached(0f, reached);
            Assert.That(reached, Is.EqualTo(new[] { underneath }));
        }

        [Test]
        public void SameTargetTwice_CountsOnce_AtTheNearerDistance()
        {
            var target = new Target("both queries");
            var queue = new DashHitQueue<Target>();
            queue.Add(target, 0.5f);
            queue.Add(target, 0f);
            queue.Add(target, 1f);
            Assert.That(queue.PendingCount, Is.EqualTo(1));

            var reached = new List<Target>();
            queue.CollectReached(0f, reached);
            Assert.That(reached, Is.EqualTo(new[] { target }));
            queue.CollectReached(10f, reached);
            Assert.That(reached, Is.EqualTo(new[] { target }));
        }

        [Test]
        public void EverythingLeft_IsHandedOutAtTheEnd()
        {
            var a = new Target("a");
            var b = new Target("b");
            var queue = new DashHitQueue<Target>();
            queue.Add(a, 4f);
            queue.Add(b, 5f);
            var reached = new List<Target>();

            queue.CollectReached(float.PositiveInfinity, reached);
            Assert.That(reached, Is.EqualTo(new[] { a, b }));
        }

        [Test]
        public void Clear_RemovesPendingHits()
        {
            var queue = new DashHitQueue<Target>();
            queue.Add(new Target("x"), 1f);
            queue.Clear();
            Assert.That(queue.PendingCount, Is.EqualTo(0));

            var reached = new List<Target>();
            queue.CollectReached(10f, reached);
            Assert.That(reached, Is.Empty);
        }
    }
}
