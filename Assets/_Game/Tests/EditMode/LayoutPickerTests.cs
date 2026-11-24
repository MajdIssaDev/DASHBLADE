using System;
using System.Collections.Generic;
using System.Linq;
using Dashblade.Waves;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class LayoutPickerTests
    {
        [TestCase(ForceLayout.A, ArenaLayout.A)]
        [TestCase(ForceLayout.B, ArenaLayout.B)]
        [TestCase(ForceLayout.C, ArenaLayout.C)]
        public void ForcedLayout_IsAlwaysPicked_WithoutAskingForARandomNumber(ForceLayout force, ArenaLayout expected)
        {
            int calls = 0;
            Func<int, int> random = n =>
            {
                calls++;
                return 0;
            };

            for (int i = 0; i < 6; i++)
            {
                Assert.That(LayoutPicker.Pick(force, random), Is.EqualTo(expected));
            }

            Assert.That(calls, Is.EqualTo(0));
        }

        [TestCase(0, ArenaLayout.A)]
        [TestCase(1, ArenaLayout.B)]
        [TestCase(2, ArenaLayout.C)]
        public void ForceNone_MapsTheRandomIndexToALayout(int index, ArenaLayout expected)
        {
            Assert.That(LayoutPicker.Pick(ForceLayout.None, _ => index), Is.EqualTo(expected));
        }

        [Test]
        public void ForceNone_AsksForOneOfExactlyThree_Once()
        {
            var asked = new List<int>();
            LayoutPicker.Pick(ForceLayout.None, n =>
            {
                asked.Add(n);
                return 0;
            });

            Assert.That(asked, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void ForceNone_EachLayoutEquallyLikely_GivenAFairSource()
        {
            // Every outcome of a fair 3-way source maps to a different layout, so each is 1 in 3.
            var picks = Enumerable.Range(0, 3).Select(i => LayoutPicker.Pick(ForceLayout.None, _ => i)).ToArray();
            Assert.That(picks, Is.EquivalentTo(new[] { ArenaLayout.A, ArenaLayout.B, ArenaLayout.C }));

            // And with a seeded pseudo-random source, 3000 runs split roughly evenly.
            var rng = new Random(12345);
            var counts = new Dictionary<ArenaLayout, int> { { ArenaLayout.A, 0 }, { ArenaLayout.B, 0 }, { ArenaLayout.C, 0 } };
            for (int i = 0; i < 3000; i++)
            {
                counts[LayoutPicker.Pick(ForceLayout.None, rng.Next)]++;
            }

            foreach (int count in counts.Values)
            {
                Assert.That(count, Is.InRange(850, 1150));
            }
        }

        [Test]
        public void TheSameLayoutCanComeUpTwiceInARow()
        {
            Assert.That(LayoutPicker.Pick(ForceLayout.None, _ => 1), Is.EqualTo(ArenaLayout.B));
            Assert.That(LayoutPicker.Pick(ForceLayout.None, _ => 1), Is.EqualTo(ArenaLayout.B));
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void ARandomSourceOutOfRange_Throws(int index)
        {
            Assert.Throws<InvalidOperationException>(() => LayoutPicker.Pick(ForceLayout.None, _ => index));
        }

        [Test]
        public void AMissingRandomSource_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => LayoutPicker.Pick(ForceLayout.None, null));
        }
    }
}
