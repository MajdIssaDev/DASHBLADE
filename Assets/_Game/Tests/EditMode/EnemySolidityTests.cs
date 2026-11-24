using Dashblade.Enemies;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class EnemySolidityTests
    {
        private const float Tolerance = 0.02f;

        [Test]
        public void StartsNotBlocking()
        {
            Assert.That(new EnemySolidity().IsBlocking, Is.False);
        }

        [Test]
        public void SolidAndApart_Blocks()
        {
            var solidity = new EnemySolidity();
            Assert.That(solidity.Update(shouldBeSolid: true, overlapDepth: -2f, Tolerance), Is.True);
        }

        [Test]
        public void SolidButOverlapping_DoesNotBlockUntilFullySeparated()
        {
            var solidity = new EnemySolidity();
            Assert.That(solidity.Update(true, 0.8f, Tolerance), Is.False, "a charge ended on the player");
            Assert.That(solidity.Update(true, 0.3f, Tolerance), Is.False, "walking out");
            Assert.That(solidity.Update(true, 0.001f, Tolerance), Is.False, "still overlapping, even a little");
            Assert.That(solidity.Update(true, 0f, Tolerance), Is.True, "touching but no longer overlapping");
            Assert.That(solidity.Update(true, 0.005f, Tolerance), Is.True, "pressing against it again: blocked like a wall");
        }

        [Test]
        public void NotWantedSolid_NeverBlocks()
        {
            var solidity = new EnemySolidity();
            solidity.Update(true, -1f, Tolerance);
            Assert.That(solidity.Update(false, -1f, Tolerance), Is.False, "charging, apart");
            Assert.That(solidity.Update(false, 0.5f, Tolerance), Is.False, "charging through the player");
        }

        [Test]
        public void BecomingSolidWhileOverlapping_WaitsForSeparation()
        {
            var solidity = new EnemySolidity();
            solidity.Update(false, 0.6f, Tolerance);
            Assert.That(solidity.Update(true, 0.6f, Tolerance), Is.False);
            Assert.That(solidity.Update(true, -0.1f, Tolerance), Is.True);
        }

        [Test]
        public void BlockingEnemy_DeepOverlapForAnyReason_LetsThePlayerOut()
        {
            var solidity = new EnemySolidity();
            solidity.Update(true, -1f, Tolerance);
            Assert.That(solidity.Update(true, 0.019f, Tolerance), Is.True, "solver penetration is not an overlap");
            Assert.That(solidity.Update(true, 0.3f, Tolerance), Is.False);
            Assert.That(solidity.Update(true, 0.019f, Tolerance), Is.False, "not yet fully separated");
            Assert.That(solidity.Update(true, -0.001f, Tolerance), Is.True);
        }
    }
}
