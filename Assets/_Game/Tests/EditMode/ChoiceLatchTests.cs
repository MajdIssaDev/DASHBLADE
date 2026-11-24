using System;
using Dashblade.Flow;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    /// <summary>One click only (F06): the first offered choice is accepted, everything after it is ignored.</summary>
    public sealed class ChoiceLatchTests
    {
        private static ChoiceLatch MainMenu() => new ChoiceLatch(MenuChoice.Play, MenuChoice.Quit);

        [Test]
        public void Constructor_NeedsAtLeastOneChoice()
        {
            Assert.Throws<ArgumentException>(() => new ChoiceLatch());
            Assert.Throws<ArgumentException>(() => new ChoiceLatch(null));
        }

        [Test]
        public void New_HasNotChosen()
        {
            Assert.That(MainMenu().HasChosen, Is.False);
        }

        [TestCase(MenuChoice.Play)]
        [TestCase(MenuChoice.Quit)]
        public void FirstOfferedChoice_IsAccepted(MenuChoice choice)
        {
            ChoiceLatch latch = MainMenu();
            Assert.That(latch.TryChoose(choice), Is.True);
            Assert.That(latch.HasChosen, Is.True);
            Assert.That(latch.Choice, Is.EqualTo(choice));
        }

        [Test]
        public void DoubleClick_CountsOnce()
        {
            ChoiceLatch latch = MainMenu();
            Assert.That(latch.TryChoose(MenuChoice.Play), Is.True);
            Assert.That(latch.TryChoose(MenuChoice.Play), Is.False);
        }

        [Test]
        public void TwoButtonsInARow_OnlyTheFirstCounts()
        {
            ChoiceLatch latch = MainMenu();
            Assert.That(latch.TryChoose(MenuChoice.Play), Is.True);
            Assert.That(latch.TryChoose(MenuChoice.Quit), Is.False);
            Assert.That(latch.Choice, Is.EqualTo(MenuChoice.Play));
        }

        [TestCase(MenuChoice.Retry)]
        [TestCase(MenuChoice.Menu)]
        public void ChoiceNotOffered_IsRefused_AndDoesNotUseTheLatchUp(MenuChoice notOffered)
        {
            ChoiceLatch latch = MainMenu();
            Assert.That(latch.Offers(notOffered), Is.False);
            Assert.That(latch.TryChoose(notOffered), Is.False);
            Assert.That(latch.HasChosen, Is.False);
            Assert.That(latch.TryChoose(MenuChoice.Quit), Is.True);
        }
    }
}
