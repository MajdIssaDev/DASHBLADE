using System;
using System.Collections.Generic;
using Dashblade.Flow;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    /// <summary>The F06 end-of-run rules: first result wins, the delay in game time, showing once, one click only.</summary>
    public sealed class EndingFlowTests
    {
        private const float Frame = 1f / 60f;

        private static EndingFlow ShowingFlow(EndingResult result = EndingResult.Victory)
        {
            var flow = new EndingFlow(1f);
            flow.ReportResult(result);
            flow.Tick(1f);
            Assert.That(flow.IsShowing, Is.True);
            return flow;
        }

        [Test]
        public void New_HasNoResult_AndIsNotShowing()
        {
            var flow = new EndingFlow(1f);
            Assert.That(flow.HasResult, Is.False);
            Assert.That(flow.IsShowing, Is.False);
            Assert.That(flow.HasChosen, Is.False);
        }

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_RejectsInvalidDelay(float delay)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EndingFlow(delay));
        }

        [Test]
        public void WithoutResult_TimeNeverShowsTheEnding()
        {
            var flow = new EndingFlow(1f);
            for (int i = 0; i < 600; i++)
            {
                flow.Tick(Frame);
            }

            Assert.That(flow.IsShowing, Is.False);
            Assert.That(flow.TimeSinceResult, Is.EqualTo(0f));
        }

        [TestCase(EndingResult.Victory)]
        [TestCase(EndingResult.Defeat)]
        public void Result_IsRecorded_ButNotShownAtOnce(EndingResult result)
        {
            var flow = new EndingFlow(1f);
            Assert.That(flow.ReportResult(result), Is.True);
            Assert.That(flow.HasResult, Is.True);
            Assert.That(flow.Result, Is.EqualTo(result));
            Assert.That(flow.IsShowing, Is.False);
        }

        [Test]
        public void Shows_OnTheFrameTheDelayIsReached_NotBefore()
        {
            var flow = new EndingFlow(1f);
            flow.ReportResult(EndingResult.Defeat);

            for (int i = 0; i < 59; i++)
            {
                flow.Tick(Frame);
            }

            Assert.That(flow.IsShowing, Is.False, "59 frames of 1/60 s are less than 1 second.");

            flow.Tick(Frame);
            Assert.That(flow.IsShowing, Is.True, "60 frames of 1/60 s reach 1 second.");
        }

        [Test]
        public void JustBeforeTheDelay_NotShowing()
        {
            var flow = new EndingFlow(1f);
            flow.ReportResult(EndingResult.Victory);
            flow.Tick(0.999f);
            Assert.That(flow.IsShowing, Is.False);
        }

        [Test]
        public void DelayOfFour_ShowsAfterFourSeconds()
        {
            var flow = new EndingFlow(4f);
            flow.ReportResult(EndingResult.Defeat);
            flow.Tick(3.99f);
            Assert.That(flow.IsShowing, Is.False);
            flow.Tick(0.01f);
            Assert.That(flow.IsShowing, Is.True);
        }

        [Test]
        public void DelayOfZero_ShowsAtOnce()
        {
            var flow = new EndingFlow(0f);
            var shown = new List<EndingResult>();
            flow.Shown += shown.Add;

            flow.ReportResult(EndingResult.Victory);

            Assert.That(flow.IsShowing, Is.True);
            Assert.That(shown, Is.EqualTo(new[] { EndingResult.Victory }));
        }

        [Test]
        public void Shown_IsRaisedOnce_WithTheResult()
        {
            var flow = new EndingFlow(1f);
            var shown = new List<EndingResult>();
            flow.Shown += shown.Add;

            flow.ReportResult(EndingResult.Defeat);
            flow.Tick(0.5f);
            Assert.That(shown, Is.Empty);

            flow.Tick(0.6f);
            flow.Tick(1f);
            flow.Tick(10f);
            Assert.That(shown, Is.EqualTo(new[] { EndingResult.Defeat }));
        }

        [Test]
        public void OneLargeStep_ShowsOnce()
        {
            var flow = new EndingFlow(1f);
            int count = 0;
            flow.Shown += _ => count++;

            flow.ReportResult(EndingResult.Victory);
            flow.Tick(5f);

            Assert.That(count, Is.EqualTo(1));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void FrozenOrInvalidSteps_DoNotCount(float step)
        {
            var flow = new EndingFlow(1f);
            flow.ReportResult(EndingResult.Victory);
            flow.Tick(0.5f);
            flow.Tick(step);

            Assert.That(flow.TimeSinceResult, Is.EqualTo(0.5f));
        }

        [Test]
        public void TimeStopsCounting_OnceShowing()
        {
            var flow = ShowingFlow();
            float atShow = flow.TimeSinceResult;
            flow.Tick(3f);
            Assert.That(flow.TimeSinceResult, Is.EqualTo(atShow));
        }

        [Test]
        public void FirstResultWins_ASecondResultIsIgnored()
        {
            var flow = new EndingFlow(1f);
            var shown = new List<EndingResult>();
            flow.Shown += shown.Add;

            Assert.That(flow.ReportResult(EndingResult.Victory), Is.True);
            flow.Tick(0.5f);
            Assert.That(flow.ReportResult(EndingResult.Defeat), Is.False);
            Assert.That(flow.Result, Is.EqualTo(EndingResult.Victory));
            Assert.That(flow.TimeSinceResult, Is.EqualTo(0.5f), "A second result does not restart the delay.");

            flow.Tick(0.5f);
            Assert.That(shown, Is.EqualTo(new[] { EndingResult.Victory }));
        }

        [Test]
        public void AResultAfterShowing_ChangesNothing()
        {
            var flow = ShowingFlow(EndingResult.Defeat);
            int count = 0;
            flow.Shown += _ => count++;

            Assert.That(flow.ReportResult(EndingResult.Victory), Is.False);
            Assert.That(flow.Result, Is.EqualTo(EndingResult.Defeat));
            Assert.That(flow.IsShowing, Is.True);
            Assert.That(count, Is.EqualTo(0));
        }

        [TestCase(MenuChoice.Retry)]
        [TestCase(MenuChoice.Menu)]
        [TestCase(MenuChoice.Quit)]
        public void ChoiceBeforeShowing_IsRefused_AndDoesNotUseTheClickUp(MenuChoice choice)
        {
            var flow = new EndingFlow(1f);
            Assert.That(flow.TryChoose(choice), Is.False, "No result yet.");

            flow.ReportResult(EndingResult.Victory);
            Assert.That(flow.TryChoose(choice), Is.False, "During the Ending Delay.");

            flow.Tick(1f);
            Assert.That(flow.TryChoose(choice), Is.True);
            Assert.That(flow.Choice, Is.EqualTo(choice));
        }

        [TestCase(MenuChoice.Retry)]
        [TestCase(MenuChoice.Menu)]
        [TestCase(MenuChoice.Quit)]
        public void OnlyTheFirstClickCounts(MenuChoice first)
        {
            var flow = ShowingFlow();
            Assert.That(flow.TryChoose(first), Is.True);

            Assert.That(flow.TryChoose(first), Is.False, "A double click counts once.");
            Assert.That(flow.TryChoose(MenuChoice.Retry), Is.False);
            Assert.That(flow.TryChoose(MenuChoice.Menu), Is.False);
            Assert.That(flow.TryChoose(MenuChoice.Quit), Is.False);
            Assert.That(flow.HasChosen, Is.True);
            Assert.That(flow.Choice, Is.EqualTo(first));
        }

        [Test]
        public void Play_IsNotOfferedOnTheEndingPanel()
        {
            var flow = ShowingFlow();
            Assert.That(flow.TryChoose(MenuChoice.Play), Is.False);
            Assert.That(flow.HasChosen, Is.False);
            Assert.That(flow.TryChoose(MenuChoice.Retry), Is.True);
        }
    }
}
