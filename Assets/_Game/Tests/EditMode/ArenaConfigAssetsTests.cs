using System.Collections.Generic;
using System.Linq;
using Dashblade.Waves;
using NUnit.Framework;
using UnityEditor;

namespace Dashblade.Tests.EditMode
{
    /// <summary>
    /// Checks the real WaveConfig and ArenaConfig assets against the F05 spec's tables (criterion 14) and every layout
    /// in ArenaConfig against the layout safety rules with the spec's numbers.
    /// </summary>
    public sealed class ArenaConfigAssetsTests
    {
        private const string WaveConfigPath = "Assets/_Game/Config/WaveConfig.asset";
        private const string ArenaConfigPath = "Assets/_Game/Config/ArenaConfig.asset";

        private static WaveConfig LoadWaveConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<WaveConfig>(WaveConfigPath);
            Assert.That(config, Is.Not.Null, $"No {nameof(WaveConfig)} at {WaveConfigPath}.");
            return config;
        }

        private static ArenaConfig LoadArenaConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<ArenaConfig>(ArenaConfigPath);
            Assert.That(config, Is.Not.Null, $"No {nameof(ArenaConfig)} at {ArenaConfigPath}.");
            return config;
        }

        [Test]
        public void WaveConfig_HasTheSpecTimes()
        {
            WaveConfig config = LoadWaveConfig();
            Assert.That(config.WavePause, Is.EqualTo(SpecArena.WavePause));
            Assert.That(config.SpawnFadeTime, Is.EqualTo(SpecArena.SpawnFadeTime));
        }

        [Test]
        public void WaveConfig_HasTheSpecWaves_InOrder()
        {
            string[] actual = Describe(LoadWaveConfig().BuildWaves());
            string[] expected = Describe(SpecArena.Waves());
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WaveConfig_BuildsAValidRun()
        {
            WaveConfig config = LoadWaveConfig();
            var run = new WaveRun(config.WavePause, config.SpawnFadeTime, config.BuildWaves());
            Assert.That(run.WaveCount, Is.EqualTo(3));
        }

        [Test]
        public void ArenaConfig_ForceLayoutIsNone()
        {
            Assert.That(LoadArenaConfig().ForceLayout, Is.EqualTo(ForceLayout.None));
        }

        [TestCase(ArenaLayout.A)]
        [TestCase(ArenaLayout.B)]
        [TestCase(ArenaLayout.C)]
        public void ArenaConfig_LayoutHasTheSpecPillarCentres(ArenaLayout layout)
        {
            string[] actual = LoadArenaConfig().PillarPoints(layout).Select(p => p.ToString()).ToArray();
            string[] expected = SpecArena.Layout(layout).Select(p => p.ToString()).ToArray();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(ArenaLayout.A)]
        [TestCase(ArenaLayout.B)]
        [TestCase(ArenaLayout.C)]
        public void ArenaConfig_LayoutPassesTheSafetyRules_AgainstEveryEnemyPositionInWaveConfig(ArenaLayout layout)
        {
            ArenaPoint[] enemyPositions = LoadWaveConfig().BuildWaves().SelectMany(w => w.Enemies).Select(e => e.Position).ToArray();
            Assert.That(enemyPositions.Length, Is.EqualTo(10));

            LayoutCheckResult result = LayoutSafety.Check(
                LoadArenaConfig().PillarPoints(layout), SpecArena.PlayerStart, enemyPositions, SpecArena.Limits());

            Assert.That(result.IsSafe, Is.True, result.ToString());
        }

        private static string[] Describe(IEnumerable<WaveDefinition> waves) =>
            waves.Select((w, i) => $"Wave {i + 1}: " + string.Join(", ", w.Enemies.Select(e => e.ToString()))).ToArray();
    }
}
