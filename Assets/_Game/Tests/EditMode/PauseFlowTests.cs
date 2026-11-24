using System.Collections.Generic;
using Dashblade.Flow;
using Dashblade.UI;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    /// <summary>
    /// The F11 pause rules: Esc pauses and resumes, the two views, Resume and its dash lockout, Menu and Quit only
    /// once, and no pause once the run has a result.
    /// </summary>
    public sealed class PauseFlowTests
    {
        private PauseFlow flow;
        private List<string> events;

        [SetUp]
        public void SetUp()
        {
            flow = new PauseFlow();
            events = new List<string>();
            flow.Paused += () => events.Add("Paused");
            flow.Resumed += cause => events.Add("Resumed " + cause);
            flow.StateChanged += state => events.Add("State " + state);
        }

        private void PauseOnButtons()
        {
            Assert.That(flow.PressEscape(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Buttons));
            events.Clear();
        }

        private void PauseOnControls()
        {
            PauseOnButtons();
            Assert.That(flow.PressControls(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Controls));
            events.Clear();
        }

        [Test]
        public void New_IsPlaying()
        {
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
            Assert.That(flow.IsPaused, Is.False);
            Assert.That(flow.RunIsOver, Is.False);
            Assert.That(flow.HasChosen, Is.False);
        }

        // Criterion 1.
        [Test]
        public void Escape_WhilePlaying_PausesOnTheButtonsView()
        {
            Assert.That(flow.PressEscape(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Buttons));
            Assert.That(flow.IsPaused, Is.True);
            Assert.That(events, Is.EqualTo(new[] { "Paused", "State Buttons" }));
        }

        // Criterion 3.
        [Test]
        public void Escape_OnTheButtonsView_Resumes_WithoutDashLockout()
        {
            PauseOnButtons();
            Assert.That(flow.PressEscape(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
            Assert.That(flow.IsPaused, Is.False);
            Assert.That(events, Is.EqualTo(new[] { "Resumed Escape", "State Playing" }));
            Assert.That(PauseFlow.LocksDashClicks(ResumeCause.Escape), Is.False);
        }

        // Edge case: Esc on the Controls view resumes directly.
        [Test]
        public void Escape_OnTheControlsView_Resumes()
        {
            PauseOnControls();
            Assert.That(flow.PressEscape(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
            Assert.That(events, Is.EqualTo(new[] { "Resumed Escape", "State Playing" }));
        }

        // Criterion 4 and the double-click edge case.
        [Test]
        public void Resume_OnTheButtonsView_Resumes_WithDashLockout()
        {
            PauseOnButtons();
            Assert.That(flow.PressResume(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
            Assert.That(events, Is.EqualTo(new[] { "Resumed ResumeButton", "State Playing" }));
            Assert.That(PauseFlow.LocksDashClicks(ResumeCause.ResumeButton), Is.True);
        }

        [Test]
        public void Resume_TwiceQuickly_OnlyTheFirstCounts()
        {
            PauseOnButtons();
            Assert.That(flow.PressResume(), Is.True);
            Assert.That(flow.PressResume(), Is.False);
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
            Assert.That(events, Is.EqualTo(new[] { "Resumed ResumeButton", "State Playing" }));
        }

        [Test]
        public void Resume_WhilePlayingOrOnTheControlsView_DoesNothing()
        {
            Assert.That(flow.PressResume(), Is.False);
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));

            PauseOnControls();
            Assert.That(flow.PressResume(), Is.False);
            Assert.That(flow.State, Is.EqualTo(PauseState.Controls));
            Assert.That(events, Is.Empty);
        }

        // Criterion 5.
        [Test]
        public void Controls_OnTheButtonsView_OpensTheControlsView_StillPaused()
        {
            PauseOnButtons();
            Assert.That(flow.PressControls(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Controls));
            Assert.That(flow.IsPaused, Is.True);
            Assert.That(events, Is.EqualTo(new[] { "State Controls" }));
        }

        [Test]
        public void Controls_WhilePlayingOrAlreadyOnControls_DoesNothing()
        {
            Assert.That(flow.PressControls(), Is.False);
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));

            PauseOnControls();
            Assert.That(flow.PressControls(), Is.False);
            Assert.That(flow.State, Is.EqualTo(PauseState.Controls));
            Assert.That(events, Is.Empty);
        }

        // Criterion 6.
        [Test]
        public void Back_OnTheControlsView_ReturnsToTheButtonsView_StillPaused()
        {
            PauseOnControls();
            Assert.That(flow.PressBack(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Buttons));
            Assert.That(flow.IsPaused, Is.True);
            Assert.That(events, Is.EqualTo(new[] { "State Buttons" }));
        }

        [Test]
        public void Back_WhilePlayingOrOnTheButtonsView_DoesNothing()
        {
            Assert.That(flow.PressBack(), Is.False);
            PauseOnButtons();
            Assert.That(flow.PressBack(), Is.False);
            Assert.That(flow.State, Is.EqualTo(PauseState.Buttons));
            Assert.That(events, Is.Empty);
        }

        // Criterion 11.
        [Test]
        public void PausingAgain_AfterResumingFromTheControlsView_OpensOnTheButtonsView()
        {
            PauseOnControls();
            flow.PressEscape();
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));

            Assert.That(flow.PressEscape(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Buttons));
        }

        [Test]
        public void ManyPausesAndResumes_EachRaiseOnePausedAndOneResumed()
        {
            int paused = 0;
            int resumed = 0;
            flow.Paused += () => paused++;
            flow.Resumed += _ => resumed++;

            for (int i = 0; i < 10; i++)
            {
                flow.PressEscape();
                if (i % 2 == 0)
                {
                    flow.PressEscape();
                }
                else
                {
                    flow.PressResume();
                }
            }

            Assert.That(paused, Is.EqualTo(10));
            Assert.That(resumed, Is.EqualTo(10));
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
        }

        // Criteria 7 and 8.
        [TestCase(MenuChoice.Menu)]
        [TestCase(MenuChoice.Quit)]
        public void MenuOrQuit_OnTheButtonsView_IsAccepted(MenuChoice choice)
        {
            PauseOnButtons();
            Assert.That(flow.TryChoose(choice), Is.True);
            Assert.That(flow.HasChosen, Is.True);
            Assert.That(flow.Choice, Is.EqualTo(choice));
        }

        // Edge case: clicking twice quickly on Menu or Quit, or two of them in a row.
        [TestCase(MenuChoice.Menu, MenuChoice.Menu)]
        [TestCase(MenuChoice.Menu, MenuChoice.Quit)]
        [TestCase(MenuChoice.Quit, MenuChoice.Menu)]
        [TestCase(MenuChoice.Quit, MenuChoice.Quit)]
        public void OnlyTheFirstMenuOrQuitCounts(MenuChoice first, MenuChoice second)
        {
            PauseOnButtons();
            Assert.That(flow.TryChoose(first), Is.True);
            Assert.That(flow.TryChoose(second), Is.False);
            Assert.That(flow.Choice, Is.EqualTo(first));
        }

        [Test]
        public void AfterMenuOrQuit_NothingElseCounts()
        {
            PauseOnButtons();
            flow.TryChoose(MenuChoice.Menu);
            events.Clear();

            Assert.That(flow.PressEscape(), Is.False);
            Assert.That(flow.PressResume(), Is.False);
            Assert.That(flow.PressControls(), Is.False);
            Assert.That(flow.PressBack(), Is.False);
            Assert.That(flow.ReportRunOver(), Is.True, "The result is still recorded.");
            Assert.That(flow.State, Is.EqualTo(PauseState.Buttons), "Leaving the scene; the pause is not ended here.");
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void MenuOrQuit_WhilePlayingOrOnTheControlsView_IsRefused_AndDoesNotUseTheChoiceUp()
        {
            Assert.That(flow.TryChoose(MenuChoice.Menu), Is.False);
            PauseOnControls();
            Assert.That(flow.TryChoose(MenuChoice.Quit), Is.False);
            Assert.That(flow.HasChosen, Is.False);

            flow.PressBack();
            Assert.That(flow.TryChoose(MenuChoice.Quit), Is.True);
        }

        [TestCase(MenuChoice.Play)]
        [TestCase(MenuChoice.Retry)]
        public void ChoicesNotOffered_AreRefused_AndDoNotUseTheChoiceUp(MenuChoice choice)
        {
            PauseOnButtons();
            Assert.That(flow.TryChoose(choice), Is.False);
            Assert.That(flow.HasChosen, Is.False);
            Assert.That(flow.TryChoose(MenuChoice.Menu), Is.True);
        }

        // Criterion 9: no pause once the run has a result (during the ending delay and on the panels).
        [Test]
        public void RunOver_WhilePlaying_EscapeNeverPausesAgain()
        {
            Assert.That(flow.ReportRunOver(), Is.True);
            Assert.That(flow.RunIsOver, Is.True);

            for (int i = 0; i < 3; i++)
            {
                Assert.That(flow.PressEscape(), Is.False);
            }

            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
            Assert.That(events, Is.Empty);
        }

        // Edge case: pause and ending never show together; the ending wins.
        [TestCase(false)]
        [TestCase(true)]
        public void RunOver_WhilePaused_EndsThePauseAtOnce(bool onControls)
        {
            if (onControls)
            {
                PauseOnControls();
            }
            else
            {
                PauseOnButtons();
            }

            Assert.That(flow.ReportRunOver(), Is.True);
            Assert.That(flow.State, Is.EqualTo(PauseState.Playing));
            Assert.That(events, Is.EqualTo(new[] { "Resumed RunOver", "State Playing" }));
            Assert.That(PauseFlow.LocksDashClicks(ResumeCause.RunOver), Is.False);
            Assert.That(flow.PressEscape(), Is.False);
            Assert.That(flow.PressResume(), Is.False);
        }

        [Test]
        public void RunOver_IsReportedOnce()
        {
            Assert.That(flow.ReportRunOver(), Is.True);
            Assert.That(flow.ReportRunOver(), Is.False);
        }

        [Test]
        public void RunOver_WhilePaused_MenuIsNoLongerOffered_BecauseThePauseEnded()
        {
            PauseOnButtons();
            flow.ReportRunOver();
            Assert.That(flow.TryChoose(MenuChoice.Menu), Is.False);
        }

        [Test]
        public void PauseCanvas_SortsAboveTheHud_AndBelowTheEnding()
        {
            Assert.That(UiSorting.PauseOrder, Is.GreaterThan(UiSorting.HudOrder));
            Assert.That(UiSorting.PauseOrder, Is.LessThan(UiSorting.EndingOrder));
        }
    }
}
