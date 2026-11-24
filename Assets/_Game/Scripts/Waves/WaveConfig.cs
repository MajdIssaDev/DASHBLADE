using System;
using UnityEngine;

namespace Dashblade.Waves
{
    /// <summary>
    /// Tuning numbers and contents of the run's waves (F05). The asset lives at Assets/_Game/Config/WaveConfig.asset.
    /// The wave rules themselves live in <see cref="WaveRun"/>; this asset only feeds them.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "Dashblade/Wave Config")]
    public sealed class WaveConfig : ScriptableObject
    {
        [Tooltip("Seconds of empty arena before every wave, wave 1 included.")]
        [Min(0f)]
        [SerializeField] private float wavePause = 2f;

        [Tooltip("Seconds a wave's enemies take to fade in. Meanwhile they are harmless, not solid and cannot be destroyed.")]
        [Min(0f)]
        [SerializeField] private float spawnFadeTime = 0.5f;

        [Tooltip("The waves in order. Each enemy appears with its centre at its Position.")]
        [SerializeField] private Wave[] waves =
        {
            new Wave(
                new WaveEnemyEntry(EnemyKind.Charger, 10f, 6f),
                new WaveEnemyEntry(EnemyKind.Charger, 10f, -6f),
                new WaveEnemyEntry(EnemyKind.Charger, 13f, 0f)),
            new Wave(
                new WaveEnemyEntry(EnemyKind.Turret, 12f, 5f),
                new WaveEnemyEntry(EnemyKind.Turret, 12f, -5f),
                new WaveEnemyEntry(EnemyKind.Turret, 0f, -7f)),
            new Wave(
                new WaveEnemyEntry(EnemyKind.Charger, 8f, 6f),
                new WaveEnemyEntry(EnemyKind.Charger, 8f, -6f),
                new WaveEnemyEntry(EnemyKind.Turret, 13f, 3f),
                new WaveEnemyEntry(EnemyKind.Turret, 13f, -3f)),
        };

        public float WavePause => wavePause;

        public float SpawnFadeTime => spawnFadeTime;

        /// <summary>The waves as plain data for <see cref="WaveRun"/>, a fresh copy on every call.</summary>
        public WaveDefinition[] BuildWaves()
        {
            Wave[] source = waves ?? Array.Empty<Wave>();
            var result = new WaveDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                WaveEnemyEntry[] entries = source[i]?.Enemies ?? Array.Empty<WaveEnemyEntry>();
                var enemies = new WaveEnemy[entries.Length];
                for (int e = 0; e < entries.Length; e++)
                {
                    enemies[e] = new WaveEnemy(entries[e].Kind, entries[e].Position.x, entries[e].Position.y);
                }

                result[i] = new WaveDefinition(enemies);
            }

            return result;
        }

        /// <summary>One wave in the Inspector: its enemies.</summary>
        [Serializable]
        public sealed class Wave
        {
            [SerializeField] private WaveEnemyEntry[] enemies = Array.Empty<WaveEnemyEntry>();

            public Wave()
            {
            }

            public Wave(params WaveEnemyEntry[] enemies)
            {
                this.enemies = enemies;
            }

            public WaveEnemyEntry[] Enemies => enemies;
        }

        /// <summary>One enemy of a wave in the Inspector: what it is and where its centre appears.</summary>
        [Serializable]
        public struct WaveEnemyEntry
        {
            [SerializeField] private EnemyKind kind;
            [SerializeField] private Vector2 position;

            public WaveEnemyEntry(EnemyKind kind, float x, float y)
            {
                this.kind = kind;
                position = new Vector2(x, y);
            }

            public EnemyKind Kind => kind;

            public Vector2 Position => position;
        }
    }
}
