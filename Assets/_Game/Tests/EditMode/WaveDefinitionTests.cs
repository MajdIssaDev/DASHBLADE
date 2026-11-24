using System;
using System.Linq;
using Dashblade.Waves;
using NUnit.Framework;

namespace Dashblade.Tests.EditMode
{
    public sealed class WaveDefinitionTests
    {
        [Test]
        public void Wave1_IsThreeChargers()
        {
            WaveDefinition wave = SpecArena.Waves()[0];
            Assert.That(wave.Count, Is.EqualTo(3));
            Assert.That(wave.CountOf(EnemyKind.Charger), Is.EqualTo(3));
            Assert.That(wave.CountOf(EnemyKind.Turret), Is.EqualTo(0));
            Assert.That(wave.Enemies.Select(e => e.ToString()), Is.EqualTo(new[] { "Charger (10, 6)", "Charger (10, -6)", "Charger (13, 0)" }));
        }

        [Test]
        public void Wave2_IsThreeTurrets()
        {
            WaveDefinition wave = SpecArena.Waves()[1];
            Assert.That(wave.Count, Is.EqualTo(3));
            Assert.That(wave.CountOf(EnemyKind.Turret), Is.EqualTo(3));
            Assert.That(wave.Enemies.Select(e => e.ToString()), Is.EqualTo(new[] { "Turret (12, 5)", "Turret (12, -5)", "Turret (0, -7)" }));
        }

        [Test]
        public void Wave3_IsTwoChargersAndTwoTurrets()
        {
            WaveDefinition wave = SpecArena.Waves()[2];
            Assert.That(wave.Count, Is.EqualTo(4));
            Assert.That(wave.CountOf(EnemyKind.Charger), Is.EqualTo(2));
            Assert.That(wave.CountOf(EnemyKind.Turret), Is.EqualTo(2));
            Assert.That(wave.Enemies.Select(e => e.ToString()), Is.EqualTo(new[] { "Charger (8, 6)", "Charger (8, -6)", "Turret (13, 3)", "Turret (13, -3)" }));
        }

        [Test]
        public void KeepsItsOwnCopyOfTheEnemies()
        {
            var source = new[] { new WaveEnemy(EnemyKind.Charger, 1f, 2f) };
            var wave = new WaveDefinition(source);
            source[0] = new WaveEnemy(EnemyKind.Turret, 5f, 5f);

            Assert.That(wave.Enemies[0].Kind, Is.EqualTo(EnemyKind.Charger));
            Assert.That(wave.Enemies[0].Position.X, Is.EqualTo(1f));
            Assert.That(wave.Enemies[0].Position.Y, Is.EqualTo(2f));
        }

        [Test]
        public void RejectsAMissingEnemyList()
        {
            Assert.Throws<ArgumentNullException>(() => new WaveDefinition(null));
        }
    }
}
