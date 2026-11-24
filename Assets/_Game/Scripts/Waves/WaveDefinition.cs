using System;
using System.Collections.Generic;

namespace Dashblade.Waves
{
    /// <summary>
    /// The contents of one wave as plain data: the enemies it creates and where. The wrapper
    /// builds these from <c>WaveConfig</c>; <see cref="WaveRun"/> only needs <see cref="Count"/>.
    /// </summary>
    public sealed class WaveDefinition
    {
        private readonly WaveEnemy[] enemies;

        /// <param name="enemies">The wave's enemies. Copied, so later changes to the source do not change the wave.</param>
        public WaveDefinition(IEnumerable<WaveEnemy> enemies)
        {
            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            this.enemies = new List<WaveEnemy>(enemies).ToArray();
        }

        public IReadOnlyList<WaveEnemy> Enemies => enemies;

        /// <summary>How many enemies the wave creates.</summary>
        public int Count => enemies.Length;

        /// <summary>How many enemies of the given kind the wave creates.</summary>
        public int CountOf(EnemyKind kind)
        {
            int count = 0;
            foreach (WaveEnemy enemy in enemies)
            {
                if (enemy.Kind == kind)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
