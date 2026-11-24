using System.Collections.Generic;
using Dashblade.Waves;

namespace Dashblade.Tests.EditMode
{
    /// <summary>The F05 spec's numbers, waves and layouts, shared by the wave and layout tests.</summary>
    public static class SpecArena
    {
        // WaveConfig (criterion 14).
        public const float WavePause = 2f;
        public const float SpawnFadeTime = 0.5f;

        public static readonly ArenaPoint PlayerStart = new ArenaPoint(-14.5f, 0f);

        public static WaveDefinition[] Waves() => new[]
        {
            new WaveDefinition(new[]
            {
                new WaveEnemy(EnemyKind.Charger, 10f, 6f),
                new WaveEnemy(EnemyKind.Charger, 10f, -6f),
                new WaveEnemy(EnemyKind.Charger, 13f, 0f),
            }),
            new WaveDefinition(new[]
            {
                new WaveEnemy(EnemyKind.Turret, 12f, 5f),
                new WaveEnemy(EnemyKind.Turret, 12f, -5f),
                new WaveEnemy(EnemyKind.Turret, 0f, -7f),
            }),
            new WaveDefinition(new[]
            {
                new WaveEnemy(EnemyKind.Charger, 8f, 6f),
                new WaveEnemy(EnemyKind.Charger, 8f, -6f),
                new WaveEnemy(EnemyKind.Turret, 13f, 3f),
                new WaveEnemy(EnemyKind.Turret, 13f, -3f),
            }),
        };

        /// <summary>Every enemy position of every wave: the spec's ten positions.</summary>
        public static ArenaPoint[] AllEnemyPositions()
        {
            var positions = new List<ArenaPoint>();
            foreach (WaveDefinition wave in Waves())
            {
                foreach (WaveEnemy enemy in wave.Enemies)
                {
                    positions.Add(enemy.Position);
                }
            }

            return positions.ToArray();
        }

        // ArenaConfig layouts, as pillar centres.
        public static ArenaPoint[] Layout(ArenaLayout layout)
        {
            switch (layout)
            {
                case ArenaLayout.A:
                    return Points(-8, 4, -8, -4, -1, 0, 4, 4, 4, -4, 9, 0);
                case ArenaLayout.B:
                    return Points(-10, -5, -5, 2, 0, 5, 2, -2, 5, 2, 10, -2);
                default:
                    return Points(-6, 0, -3, 5, -3, -5, 3, 5, 3, -5, 6, 0);
            }
        }

        /// <summary>The layout safety rules' numbers.</summary>
        public static LayoutSafetyLimits Limits() =>
            new LayoutSafetyLimits(
                pillarCount: 6,
                minDistanceFromPlayerStart: 4f,
                minAxisOffsetFromEnemies: 3f,
                minX: -13f,
                maxX: 13f,
                minY: -6f,
                maxY: 6f,
                minAxisOffsetBetweenPillars: 4f);

        public static ArenaPoint[] Points(params float[] xy)
        {
            var points = new ArenaPoint[xy.Length / 2];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new ArenaPoint(xy[2 * i], xy[(2 * i) + 1]);
            }

            return points;
        }
    }
}
