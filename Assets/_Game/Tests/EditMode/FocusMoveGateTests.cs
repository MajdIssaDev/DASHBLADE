using Dashblade.Player;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class FocusMoveGateTests
    {
        [Test]
        public void StartsUnblocked()
        {
            Assert.That(new FocusMoveGate().IsBlocked, Is.False);
        }

        [Test]
        public void FocusLost_Blocks()
        {
            var gate = new FocusMoveGate();
            gate.OnFocusLost();
            Assert.That(gate.IsBlocked, Is.True);
        }

        [Test]
        public void StaysBlocked_UntilAMovementKeyIsPressed()
        {
            var gate = new FocusMoveGate();
            gate.OnFocusLost();

            gate.Update(anyMovementKeyPressedThisFrame: false);
            gate.Update(anyMovementKeyPressedThisFrame: false);
            Assert.That(gate.IsBlocked, Is.True);

            gate.Update(anyMovementKeyPressedThisFrame: true);
            Assert.That(gate.IsBlocked, Is.False);
        }

        [Test]
        public void SecondFocusLoss_BlocksAgain()
        {
            var gate = new FocusMoveGate();
            gate.OnFocusLost();
            gate.Update(true);
            gate.OnFocusLost();
            Assert.That(gate.IsBlocked, Is.True);
        }
    }
}
