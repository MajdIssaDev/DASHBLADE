using Dashblade.Common;
using Dashblade.Waves;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    /// <summary>F06 "Loading time is not game time": run time counts from the first frame the run is shown.</summary>
    public sealed class RunClockTests
    {
        private const double Tolerance = 1e-9;

        [Test]
        public void BeforeTheFirstFrame_NotStarted_ZeroElapsed()
        {
            var clock = new RunClock();
            Assert.That(clock.HasStarted, Is.False);
            Assert.That(clock.Elapsed, Is.EqualTo(0d));
        }

        [Test]
        public void FirstFrame_StepsZero_WhateverTheClockReads()
        {
            // The game clock already reads 92.6 s (menu time plus loading); none of it belongs to the run.
            var clock = new RunClock();
            Assert.That(clock.Step(92.6259), Is.EqualTo(0f));
            Assert.That(clock.HasStarted, Is.True);
            Assert.That(clock.Elapsed, Is.EqualTo(0d));
        }

        [Test]
        public void LaterFrames_StepByHowFarTheClockMoved()
        {
            var clock = new RunClock();
            clock.Step(10.0);
            Assert.That(clock.Step(10.016), Is.EqualTo(0.016f).Within(1e-6f));
            Assert.That(clock.Step(10.05), Is.EqualTo(0.034f).Within(1e-6f));
            Assert.That(clock.Elapsed, Is.EqualTo(0.05).Within(Tolerance));
        }

        [Test]
        public void ClockHeldStill_StepsZero()
        {
            // The frame after a scene load (Unity holds the clock still) and frozen frames (Time.timeScale = 0).
            var clock = new RunClock();
            clock.Step(5.0);
            Assert.That(clock.Step(5.0), Is.EqualTo(0f));
            Assert.That(clock.Step(5.0), Is.EqualTo(0f));
            Assert.That(clock.Elapsed, Is.EqualTo(0d));
        }

        [Test]
        public void ClockGoingBack_StepsZero_AndIsNotCountedTwice()
        {
            var clock = new RunClock();
            clock.Step(5.0);
            clock.Step(5.5);
            Assert.That(clock.Step(5.2), Is.EqualTo(0f));
            Assert.That(clock.Step(5.6), Is.EqualTo(0.1f).Within(1e-6f));
            Assert.That(clock.Elapsed, Is.EqualTo(0.6).Within(Tolerance));
        }

        [Test]
        public void StepsAddUpToTheClockDifference_OverManyFrames()
        {
            // 2 seconds at 240 fps starting late in a session: the run's total is the clock difference.
            var clock = new RunClock();
            double start = 1234.5678;
            clock.Step(start);
            float total = 0f;
            for (int i = 1; i <= 480; i++)
            {
                total += clock.Step(start + i / 240.0);
            }

            Assert.That(total, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(clock.Elapsed, Is.EqualTo(2.0).Within(1e-9));
        }

        [Test]
        public void LoadHitchBeforeTheFirstFrame_IsNotCounted()
        {
            // As measured after Play: the menu's last frame at 92.5163 s, the first Arena frame at 92.6259 s (0.11 s of
            // loading), the next frame with the clock held still, then normal frames.
            var clock = new RunClock();
            float total = clock.Step(92.6259);
            total += clock.Step(92.6259);
            total += clock.Step(92.6459);
            total += clock.Step(92.6482);
            Assert.That(total, Is.EqualTo(0.0223f).Within(1e-5f));
        }

        [Test]
        public void WaveOne_SpawnsTwoSecondsAfterTheFirstFrame_DespiteTheLoadHitch()
        {
            // The Wave Pause driven by the clock, with the frames measured after Play, then 240 fps.
            var clock = new RunClock();
            var run = new WaveRun(2f, 0.5f, new[] { 3, 3, 4 });
            double spawnedAt = -1;
            run.WaveSpawning += _ => spawnedAt = clock.Elapsed;

            var frames = new System.Collections.Generic.List<double> { 92.6259, 92.6259, 92.6459 };
            for (int i = 1; i <= 600; i++)
            {
                frames.Add(92.6459 + i / 240.0);
            }

            foreach (double time in frames)
            {
                run.Tick(clock.Step(time));
                if (spawnedAt >= 0)
                {
                    break;
                }
            }

            Assert.That(spawnedAt, Is.EqualTo(2.0).Within(1.0 / 240.0 + 1e-6));
            Assert.That(spawnedAt, Is.GreaterThanOrEqualTo(2.0 - 1e-5));
        }
    }
}
