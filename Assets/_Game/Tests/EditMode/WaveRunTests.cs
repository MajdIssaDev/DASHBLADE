using System;
using System.Collections.Generic;
using System.Linq;
using Dashblade.Waves;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class WaveRunTests
    {
        private const float Tolerance = 0.0001f;
        private const double TimelineTolerance = 0.001;

        // Uneven frame steps (seconds), repeated: a mix of fast, slow and hitchy frames.
        private static readonly double[] UnevenSteps = { 0.016, 0.033, 0.007, 0.05, 0.021, 0.1, 0.0125 };

        private static WaveRun NewRun() => new WaveRun(SpecArena.WavePause, SpecArena.SpawnFadeTime, SpecArena.Waves());

        // ---------- Construction ----------

        [Test]
        public void StartsInTheFirstPause_Wave1_NothingLeft_Playing()
        {
            var run = NewRun();
            Assert.That(run.CurrentWave, Is.EqualTo(1));
            Assert.That(run.EnemiesLeft, Is.EqualTo(0));
            Assert.That(run.RunState, Is.EqualTo(RunState.Playing));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Pause));
            Assert.That(run.IsFadingIn, Is.False);
            Assert.That(run.FadeInProgress, Is.EqualTo(0f));
            Assert.That(run.WaveCount, Is.EqualTo(3));
        }

        [Test]
        public void BuiltFromWaveDefinitions_UsesTheirEnemyCounts()
        {
            var run = NewRun();
            var spawned = new List<int>();
            run.WaveSpawning += _ => spawned.Add(run.EnemiesLeft);

            PlayWave(run);
            PlayWave(run);
            PlayWave(run);

            Assert.That(spawned, Is.EqualTo(new[] { 3, 3, 4 }));
        }

        [TestCase(-0.1f, 0.5f)]
        [TestCase(float.NaN, 0.5f)]
        [TestCase(2f, -0.1f)]
        [TestCase(2f, float.NaN)]
        public void RejectsNegativeOrNaNTimes(float wavePause, float spawnFadeTime)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveRun(wavePause, spawnFadeTime, new[] { 3, 3, 4 }));
        }

        [Test]
        public void RejectsMissingOrEmptyWaveLists()
        {
            Assert.Throws<ArgumentNullException>(() => new WaveRun(2f, 0.5f, (IReadOnlyList<int>)null));
            Assert.Throws<ArgumentNullException>(() => new WaveRun(2f, 0.5f, (IReadOnlyList<WaveDefinition>)null));
            Assert.Throws<ArgumentException>(() => new WaveRun(2f, 0.5f, new int[0]));
            Assert.Throws<ArgumentException>(() => new WaveRun(2f, 0.5f, new WaveDefinition[] { null }));
        }

        [Test]
        public void RejectsAWaveWithNoEnemies()
        {
            Assert.Throws<ArgumentException>(() => new WaveRun(2f, 0.5f, new[] { 3, 0, 4 }));
        }

        // ---------- Wave Pause (criteria 5 and 14; edge case "every wave is preceded by the pause") ----------

        [Test]
        public void EmptyArenaForTheFirstTwoSeconds_WithCurrentWave1()
        {
            var run = NewRun();
            var log = new Recorder(run);

            run.Tick(1f);
            run.Tick(0.99f);

            Assert.That(log.Count("WaveSpawning"), Is.EqualTo(0));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Pause));
            Assert.That(run.CurrentWave, Is.EqualTo(1));
            Assert.That(run.EnemiesLeft, Is.EqualTo(0));
        }

        [Test]
        public void Wave1SpawnsAtTwoSeconds_ThreeEnemiesLeft()
        {
            var run = NewRun();
            var log = new Recorder(run);

            run.Tick(2f);

            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 1" }));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.FadingIn));
            Assert.That(run.IsFadingIn, Is.True);
            Assert.That(run.EnemiesLeft, Is.EqualTo(3));
            Assert.That(run.CurrentWave, Is.EqualTo(1));
        }

        [Test]
        public void SixtyFramesPerSecond_SpawnsOnTheFrameThatReachesTwoSeconds()
        {
            var run = NewRun();
            var log = new Recorder(run);

            for (int i = 0; i < 119; i++)
            {
                run.Tick(1f / 60f);
            }

            Assert.That(log.Count("WaveSpawning"), Is.EqualTo(0));
            run.Tick(1f / 60f);
            Assert.That(log.Count("WaveSpawning"), Is.EqualTo(1));
        }

        [Test]
        public void WavePauseSix_KeepsTheArenaEmptyForSixSeconds()
        {
            var run = new WaveRun(6f, SpecArena.SpawnFadeTime, SpecArena.Waves());
            var log = new Recorder(run);

            run.Tick(5.99f);
            Assert.That(log.Count("WaveSpawning"), Is.EqualTo(0));
            run.Tick(0.01f);
            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 1" }));
        }

        // ---------- Fade-in (criteria 5 and 6; edge case "fading-in enemies") ----------

        [Test]
        public void Wave1FadesInFromTwoToTwoAndAHalfSeconds()
        {
            var run = NewRun();
            var log = new Recorder(run);

            run.Tick(2f);
            Assert.That(run.FadeInProgress, Is.EqualTo(0f).Within(Tolerance));
            run.Tick(0.25f);
            Assert.That(run.FadeInProgress, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(run.IsFadingIn, Is.True);
            run.Tick(0.24f);
            Assert.That(run.FadeInProgress, Is.EqualTo(0.98f).Within(Tolerance));
            Assert.That(log.Count("FadeInFinished"), Is.EqualTo(0));

            run.Tick(0.01f);
            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 1", "FadeInFinished 1" }));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Fighting));
            Assert.That(run.IsFadingIn, Is.False);
            Assert.That(run.FadeInProgress, Is.EqualTo(1f));
            Assert.That(run.EnemiesLeft, Is.EqualTo(3));
        }

        [Test]
        public void SpawnFadeTimeSix_EnemiesFadeInFromTwoToEightSeconds()
        {
            var run = new WaveRun(SpecArena.WavePause, 6f, SpecArena.Waves());
            var log = new Recorder(run);

            run.Tick(2f);
            run.Tick(5.99f);
            Assert.That(run.IsFadingIn, Is.True);
            Assert.That(run.FadeInProgress, Is.EqualTo(5.99f / 6f).Within(Tolerance));
            Assert.That(log.Count("FadeInFinished"), Is.EqualTo(0));

            run.Tick(0.01f);
            Assert.That(run.IsFadingIn, Is.False);
            Assert.That(log.Count("FadeInFinished"), Is.EqualTo(1));
        }

        [Test]
        public void FadingEnemies_CannotBeDestroyed()
        {
            var run = NewRun();
            var log = new Recorder(run);
            run.Tick(2.2f);

            Assert.That(run.ReportEnemyDestroyed(), Is.False);
            Assert.That(run.EnemiesLeft, Is.EqualTo(3));
            Assert.That(log.Count("WaveCleared"), Is.EqualTo(0));
        }

        [Test]
        public void KillsDuringAPause_AreIgnored()
        {
            var run = NewRun();
            run.Tick(1f);
            Assert.That(run.ReportEnemyDestroyed(), Is.False);
            Assert.That(run.EnemiesLeft, Is.EqualTo(0));
        }

        [Test]
        public void FadeInFinished_LeavesTheOverrunInTimeInPhase()
        {
            // The enemies' first-attack delay starts when the fade-in ends; a long frame that
            // overshoots the end leaves the overshoot in TimeInPhase for the wrapper to pass on.
            var run = NewRun();
            float timeInPhaseAtFadeEnd = -1f;
            run.FadeInFinished += _ => timeInPhaseAtFadeEnd = run.TimeInPhase;

            run.Tick(2f);
            run.Tick(0.4f);
            run.Tick(0.3f);

            Assert.That(timeInPhaseAtFadeEnd, Is.EqualTo(0.2f).Within(Tolerance));
        }

        [Test]
        public void ZeroFadeTime_EnemiesAppearFullyInTheSameStep()
        {
            var run = new WaveRun(SpecArena.WavePause, 0f, SpecArena.Waves());
            var log = new Recorder(run);

            run.Tick(2f);

            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 1", "FadeInFinished 1" }));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Fighting));
        }

        // ---------- Time steps ----------

        [Test]
        public void OneStepCanEndThePauseAndTheFadeIn_InOrder()
        {
            var run = NewRun();
            var log = new Recorder(run);

            run.Tick(3f);

            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 1", "FadeInFinished 1" }));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Fighting));
            Assert.That(run.TimeInPhase, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(run.EnemiesLeft, Is.EqualTo(3));
        }

        [Test]
        public void OneHugeStepAfterAClear_SpawnsOnlyTheNextWave()
        {
            var run = NewRun();
            PlayWave(run);
            var log = new Recorder(run);

            run.Tick(100f);

            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 2", "FadeInFinished 2" }));
            Assert.That(run.CurrentWave, Is.EqualTo(2));
            Assert.That(run.EnemiesLeft, Is.EqualTo(3));
            Assert.That(run.TimeInPhase, Is.EqualTo(97.5f).Within(0.001f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void ZeroNegativeOrNaNSteps_AreIgnored(float step)
        {
            var run = NewRun();
            run.Tick(1.5f);
            run.Tick(step);
            Assert.That(run.TimeInPhase, Is.EqualTo(1.5f).Within(Tolerance));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Pause));
        }

        [Test]
        public void UnevenSteps_Wave1SpawnsAtTwoAndFinishesFadingAtTwoAndAHalf()
        {
            var run = NewRun();
            var clock = new Clock(run);
            var log = new Recorder(run, clock);

            clock.AdvanceTo(5.0);

            Assert.That(log.TimeOf("WaveSpawning 1"), Is.EqualTo(2.0).Within(TimelineTolerance));
            Assert.That(log.TimeOf("FadeInFinished 1"), Is.EqualTo(2.5).Within(TimelineTolerance));
        }

        // ---------- Clearing a wave (criteria 7, 9, 10; edge cases on clearing) ----------

        [Test]
        public void EachKill_LowersEnemiesLeft()
        {
            var run = NewRun();
            run.Tick(2.5f);

            Assert.That(run.ReportEnemyDestroyed(), Is.True);
            Assert.That(run.EnemiesLeft, Is.EqualTo(2));
            Assert.That(run.ReportEnemyDestroyed(), Is.True);
            Assert.That(run.EnemiesLeft, Is.EqualTo(1));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Fighting));
        }

        [Test]
        public void LastKill_ClearsTheWaveAtOnce_ProjectilesThenHearts()
        {
            var run = NewRun();
            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            var log = new Recorder(run);

            run.ReportEnemyDestroyed();

            Assert.That(log.Names, Is.EqualTo(new[]
            {
                "ClearProjectilesRequested", "RestoreHeartsRequested", "WaveCleared 1", "CurrentWaveChanged 2",
            }));
            Assert.That(run.EnemiesLeft, Is.EqualTo(0));
            Assert.That(run.CurrentWave, Is.EqualTo(2));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Pause));
            Assert.That(run.RunState, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void Wave2Appears2SecondsAfterWave1sLastKill()
        {
            var run = NewRun();
            PlayWave(run);
            var log = new Recorder(run);

            run.Tick(1.99f);
            Assert.That(log.Count("WaveSpawning"), Is.EqualTo(0));
            Assert.That(run.EnemiesLeft, Is.EqualTo(0));

            run.Tick(0.01f);
            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 2" }));
            Assert.That(run.CurrentWave, Is.EqualTo(2));
            Assert.That(run.EnemiesLeft, Is.EqualTo(3));

            run.Tick(0.5f);
            Assert.That(log.Names.Last(), Is.EqualTo("FadeInFinished 2"));
        }

        [Test]
        public void TwoEnemiesDestroyedByOneDash_ClearTheWaveOnce()
        {
            var run = NewRun();
            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            var log = new Recorder(run);

            // The last two die in the same dash: two reports in a row.
            Assert.That(run.ReportEnemyDestroyed(), Is.True);
            Assert.That(run.ReportEnemyDestroyed(), Is.True);

            Assert.That(log.Count("WaveCleared"), Is.EqualTo(1));
            Assert.That(log.Count("ClearProjectilesRequested"), Is.EqualTo(1));
            Assert.That(log.Count("RestoreHeartsRequested"), Is.EqualTo(1));
            Assert.That(run.CurrentWave, Is.EqualTo(2));
        }

        [Test]
        public void ExtraKillReportsAfterAClear_AreIgnored_AndSkipNoWave()
        {
            var run = NewRun();
            PlayWave(run);
            var log = new Recorder(run);

            Assert.That(run.ReportEnemyDestroyed(), Is.False);
            Assert.That(run.ReportEnemyDestroyed(), Is.False);

            Assert.That(log.Names, Is.Empty);
            Assert.That(run.CurrentWave, Is.EqualTo(2));
            Assert.That(run.EnemiesLeft, Is.EqualTo(0));
        }

        [Test]
        public void ClearingWave2_ClearsProjectilesThenRestoresHearts()
        {
            var run = NewRun();
            PlayWave(run);
            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            var log = new Recorder(run);

            run.ReportEnemyDestroyed();

            Assert.That(log.Names, Is.EqualTo(new[]
            {
                "ClearProjectilesRequested", "RestoreHeartsRequested", "WaveCleared 2", "CurrentWaveChanged 3",
            }));
        }

        [Test]
        public void Wave3_AppearsTwoSecondsAfterWave2_WithFourEnemies()
        {
            var run = NewRun();
            PlayWave(run);
            PlayWave(run);
            var log = new Recorder(run);

            run.Tick(1.99f);
            Assert.That(log.Count("WaveSpawning"), Is.EqualTo(0));
            run.Tick(0.01f);

            Assert.That(log.Names, Is.EqualTo(new[] { "WaveSpawning 3" }));
            Assert.That(run.CurrentWave, Is.EqualTo(3));
            Assert.That(run.EnemiesLeft, Is.EqualTo(4));
        }

        [Test]
        public void CurrentWaveChanges_OnlyWhenAWaveIsCleared()
        {
            var run = NewRun();
            var changes = new List<int>();
            run.CurrentWaveChanged += changes.Add;

            run.Tick(2.5f);
            Assert.That(changes, Is.Empty);

            KillAll(run);
            Assert.That(changes, Is.EqualTo(new[] { 2 }));

            run.Tick(2.5f);
            Assert.That(changes, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void SubscribersSeeTheNewState()
        {
            var run = NewRun();
            var seen = new List<string>();
            run.WaveSpawning += w => seen.Add($"spawn {w}: left {run.EnemiesLeft}, fading {run.IsFadingIn}");
            run.FadeInFinished += w => seen.Add($"faded {w}: {run.Phase}");
            run.ClearProjectilesRequested += () => seen.Add($"clear: left {run.EnemiesLeft}, wave {run.CurrentWave}, {run.Phase}");
            run.VictoryReached += () => seen.Add($"victory: {run.RunState}, {run.Phase}");

            PlayWave(run);
            PlayWave(run);
            PlayWave(run);

            Assert.That(seen, Is.EqualTo(new[]
            {
                "spawn 1: left 3, fading True", "faded 1: Fighting", "clear: left 0, wave 2, Pause",
                "spawn 2: left 3, fading True", "faded 2: Fighting", "clear: left 0, wave 3, Pause",
                "spawn 3: left 4, fading True", "faded 3: Fighting", "clear: left 0, wave 3, Over",
                "victory: Victory, Over",
            }));
        }

        // ---------- Victory (criterion 11; edge case "after the final wave") ----------

        [Test]
        public void ClearingTheFinalWave_ClearsProjectiles_RestoresNoHearts_Victory()
        {
            var run = NewRun();
            PlayWave(run);
            PlayWave(run);
            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            var log = new Recorder(run);

            run.ReportEnemyDestroyed();

            Assert.That(log.Names, Is.EqualTo(new[] { "ClearProjectilesRequested", "WaveCleared 3", "VictoryReached" }));
            Assert.That(run.RunState, Is.EqualTo(RunState.Victory));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Over));
            Assert.That(run.CurrentWave, Is.EqualTo(3));
            Assert.That(run.EnemiesLeft, Is.EqualTo(0));
        }

        [Test]
        public void AfterVictory_NothingEverSpawns()
        {
            var run = NewRun();
            PlayWave(run);
            PlayWave(run);
            PlayWave(run);
            var log = new Recorder(run);

            run.Tick(10f);
            for (int i = 0; i < 600; i++)
            {
                run.Tick(1f / 60f);
            }

            run.Tick(1000f);

            Assert.That(log.Names, Is.Empty);
            Assert.That(run.RunState, Is.EqualTo(RunState.Victory));
        }

        [Test]
        public void AfterVictory_KillAndDefeatReportsAreIgnored()
        {
            var run = NewRun();
            PlayWave(run);
            PlayWave(run);
            PlayWave(run);
            var log = new Recorder(run);

            Assert.That(run.ReportEnemyDestroyed(), Is.False);
            Assert.That(run.ReportPlayerDefeated(), Is.False);

            Assert.That(log.Names, Is.Empty);
            Assert.That(run.RunState, Is.EqualTo(RunState.Victory));
        }

        // ---------- Defeat (criterion 12; edge case "defeat ends the run") ----------

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void DefeatDuringAPause_NoWaveEverAppears(int wave)
        {
            var run = NewRun();
            for (int i = 1; i < wave; i++)
            {
                PlayWave(run);
            }

            run.Tick(1f);
            var log = new Recorder(run);

            Assert.That(run.ReportPlayerDefeated(), Is.True);
            run.Tick(10f);
            run.Tick(1000f);

            Assert.That(log.Names, Is.EqualTo(new[] { "DefeatReached" }));
            Assert.That(run.RunState, Is.EqualTo(RunState.Defeat));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Over));
            Assert.That(run.CurrentWave, Is.EqualTo(wave));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void DefeatWhileFighting_NoFurtherWave_KillsNoLongerCount(int wave)
        {
            var run = NewRun();
            for (int i = 1; i < wave; i++)
            {
                PlayWave(run);
            }

            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            int left = run.EnemiesLeft;
            var log = new Recorder(run);

            run.ReportPlayerDefeated();
            run.Tick(10f);
            for (int i = 0; i < 4; i++)
            {
                Assert.That(run.ReportEnemyDestroyed(), Is.False);
            }

            run.Tick(10f);

            Assert.That(log.Names, Is.EqualTo(new[] { "DefeatReached" }));
            Assert.That(run.RunState, Is.EqualTo(RunState.Defeat));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Over));
            Assert.That(run.EnemiesLeft, Is.EqualTo(left));
        }

        [Test]
        public void DefeatWhileFadingIn_TheFadeFinishes_ThenNothingMore()
        {
            var run = NewRun();
            run.Tick(2.2f);
            var log = new Recorder(run);

            run.ReportPlayerDefeated();
            Assert.That(run.IsFadingIn, Is.True);
            Assert.That(run.RunState, Is.EqualTo(RunState.Defeat));

            run.Tick(0.3f);
            Assert.That(log.Names, Is.EqualTo(new[] { "DefeatReached", "FadeInFinished 1" }));
            Assert.That(run.Phase, Is.EqualTo(WavePhase.Over));
            Assert.That(run.FadeInProgress, Is.EqualTo(1f));

            Assert.That(run.ReportEnemyDestroyed(), Is.False);
            run.Tick(100f);
            Assert.That(log.Names.Count, Is.EqualTo(2));
        }

        [Test]
        public void DefeatReportedTwice_AnnouncedOnce()
        {
            var run = NewRun();
            var log = new Recorder(run);
            Assert.That(run.ReportPlayerDefeated(), Is.True);
            Assert.That(run.ReportPlayerDefeated(), Is.False);
            Assert.That(log.Count("DefeatReached"), Is.EqualTo(1));
        }

        // ---------- Victory and Defeat on the same frame ----------

        [Test]
        public void SameFrame_VictoryReportedFirst_StaysVictory()
        {
            var run = NewRun();
            PlayWave(run);
            PlayWave(run);
            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            var log = new Recorder(run);

            run.ReportEnemyDestroyed();
            Assert.That(run.ReportPlayerDefeated(), Is.False);

            Assert.That(run.RunState, Is.EqualTo(RunState.Victory));
            Assert.That(log.Count("VictoryReached"), Is.EqualTo(1));
            Assert.That(log.Count("DefeatReached"), Is.EqualTo(0));
        }

        [Test]
        public void SameFrame_DefeatReportedFirst_StaysDefeat_NoClear()
        {
            var run = NewRun();
            PlayWave(run);
            PlayWave(run);
            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            var log = new Recorder(run);

            run.ReportPlayerDefeated();
            Assert.That(run.ReportEnemyDestroyed(), Is.False);

            Assert.That(run.RunState, Is.EqualTo(RunState.Defeat));
            Assert.That(log.Names, Is.EqualTo(new[] { "DefeatReached" }));
        }

        // ---------- The spec's examples ----------

        [Test]
        public void ExampleRun_NoMistakes_UnevenFrames()
        {
            // Empty 0-2 s; wave 1 fades in 2-2.5 s; third Charger destroyed at 20 s; wave 2 fades in
            // at 22 s; last Turret destroyed at 40 s; wave 3 fades in at 42 s; last enemy at 70 s.
            var run = NewRun();
            var clock = new Clock(run);
            var log = new Recorder(run, clock);

            clock.AdvanceTo(12.0);
            run.ReportEnemyDestroyed();
            clock.AdvanceTo(16.3);
            run.ReportEnemyDestroyed();
            clock.AdvanceTo(20.0);
            run.ReportEnemyDestroyed();

            clock.AdvanceTo(30.0);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            clock.AdvanceTo(40.0);
            run.ReportEnemyDestroyed();

            clock.AdvanceTo(50.0);
            run.ReportEnemyDestroyed();
            clock.AdvanceTo(55.0);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            clock.AdvanceTo(70.0);
            run.ReportEnemyDestroyed();

            clock.AdvanceTo(80.0);

            var expected = new (string name, double time)[]
            {
                ("WaveSpawning 1", 2.0), ("FadeInFinished 1", 2.5),
                ("ClearProjectilesRequested", 20.0), ("RestoreHeartsRequested", 20.0), ("WaveCleared 1", 20.0), ("CurrentWaveChanged 2", 20.0),
                ("WaveSpawning 2", 22.0), ("FadeInFinished 2", 22.5),
                ("ClearProjectilesRequested", 40.0), ("RestoreHeartsRequested", 40.0), ("WaveCleared 2", 40.0), ("CurrentWaveChanged 3", 40.0),
                ("WaveSpawning 3", 42.0), ("FadeInFinished 3", 42.5),
                ("ClearProjectilesRequested", 70.0), ("WaveCleared 3", 70.0), ("VictoryReached", 70.0),
            };

            Assert.That(log.Names, Is.EqualTo(expected.Select(e => e.name).ToArray()));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(log.Times[i], Is.EqualTo(expected[i].time).Within(TimelineTolerance), expected[i].name);
            }

            Assert.That(run.RunState, Is.EqualTo(RunState.Victory));
        }

        [Test]
        public void Example_Wave2HeartsOne_LastTurret_ShotsCleared_HeartsRestored_Wave3TwoSecondsLater()
        {
            var run = NewRun();
            PlayWave(run);
            run.Tick(2.5f);
            run.ReportEnemyDestroyed();
            run.ReportEnemyDestroyed();
            var log = new Recorder(run);

            run.ReportEnemyDestroyed();
            run.Tick(2f);

            Assert.That(log.Names.Take(2), Is.EqualTo(new[] { "ClearProjectilesRequested", "RestoreHeartsRequested" }));
            Assert.That(log.Names.Last(), Is.EqualTo("WaveSpawning 3"));
        }

        [Test]
        public void Example_Wave3HeartsOne_HitInALockedLane_DefeatAndNothingElse()
        {
            var run = NewRun();
            PlayWave(run);
            PlayWave(run);
            run.Tick(5f);
            var log = new Recorder(run);

            run.ReportPlayerDefeated();
            run.Tick(10f);

            Assert.That(run.RunState, Is.EqualTo(RunState.Defeat));
            Assert.That(log.Names, Is.EqualTo(new[] { "DefeatReached" }));
        }

        // ---------- Helpers ----------

        /// <summary>From the start of a pause: wait for the wave to appear, then destroy all its enemies.</summary>
        private static void PlayWave(WaveRun run)
        {
            run.Tick(SpecArena.WavePause + SpecArena.SpawnFadeTime);
            KillAll(run);
        }

        private static void KillAll(WaveRun run)
        {
            while (run.EnemiesLeft > 0)
            {
                Assert.That(run.ReportEnemyDestroyed(), Is.True);
            }
        }

        /// <summary>Game time driven in uneven frame steps, landing exactly on each target time.</summary>
        private sealed class Clock
        {
            private readonly WaveRun run;
            private int stepIndex;

            public Clock(WaveRun run)
            {
                this.run = run;
            }

            /// <summary>Game time at the end of the last step.</summary>
            public double Now { get; private set; }

            public void AdvanceTo(double target)
            {
                while (Now < target - 1e-9)
                {
                    double step = Math.Min(UnevenSteps[stepIndex++ % UnevenSteps.Length], target - Now);
                    Now += step;
                    run.Tick((float)step);
                }

                Now = target;
            }
        }

        /// <summary>
        /// Records every event as "Name wave" with the game time it happened at. For events raised
        /// inside Tick that time is the step's end minus the leftover already carried into the new
        /// phase, so a late frame does not hide drift.
        /// </summary>
        private sealed class Recorder
        {
            private readonly WaveRun run;
            private readonly Clock clock;

            public Recorder(WaveRun run, Clock clock = null)
            {
                this.run = run;
                this.clock = clock;
                run.WaveSpawning += w => Add($"WaveSpawning {w}");
                run.FadeInFinished += w => Add($"FadeInFinished {w}");
                run.ClearProjectilesRequested += () => Add("ClearProjectilesRequested");
                run.RestoreHeartsRequested += () => Add("RestoreHeartsRequested");
                run.WaveCleared += w => Add($"WaveCleared {w}");
                run.CurrentWaveChanged += w => Add($"CurrentWaveChanged {w}");
                run.VictoryReached += () => Add("VictoryReached");
                run.DefeatReached += () => Add("DefeatReached");
            }

            public List<string> Names { get; } = new List<string>();

            public List<double> Times { get; } = new List<double>();

            public int Count(string prefix) => Names.Count(n => n.StartsWith(prefix, StringComparison.Ordinal));

            public double TimeOf(string name) => Times[Names.IndexOf(name)];

            private void Add(string name)
            {
                Names.Add(name);
                Times.Add(clock == null ? double.NaN : clock.Now - run.TimeInPhase);
            }
        }
    }
}
