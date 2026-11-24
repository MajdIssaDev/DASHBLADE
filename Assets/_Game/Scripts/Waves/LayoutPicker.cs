using System;

namespace Dashblade.Waves
{
    /// <summary>
    /// Picks the run's pillar layout. Call once when the run starts and keep the result for all
    /// three waves. With Force Layout = None each of A, B and C is equally likely (given a fair
    /// random source) and nothing stops the same layout coming up twice in a row.
    /// </summary>
    public static class LayoutPicker
    {
        /// <summary>How many layouts there are (A, B, C).</summary>
        public const int LayoutCount = 3;

        /// <param name="force">Force Layout from <c>ArenaConfig</c>.</param>
        /// <param name="randomIndexBelow">
        /// Returns a uniformly random whole number from 0 up to, but not including, its argument.
        /// In the game: <c>n =&gt; UnityEngine.Random.Range(0, n)</c>. Not called when a layout is forced.
        /// </param>
        public static ArenaLayout Pick(ForceLayout force, Func<int, int> randomIndexBelow)
        {
            if (randomIndexBelow == null)
            {
                throw new ArgumentNullException(nameof(randomIndexBelow));
            }

            switch (force)
            {
                case ForceLayout.A:
                    return ArenaLayout.A;
                case ForceLayout.B:
                    return ArenaLayout.B;
                case ForceLayout.C:
                    return ArenaLayout.C;
                case ForceLayout.None:
                    int index = randomIndexBelow(LayoutCount);
                    if (index < 0 || index >= LayoutCount)
                    {
                        throw new InvalidOperationException($"The random source returned {index}; it must return 0 to {LayoutCount - 1}.");
                    }

                    return (ArenaLayout)index;
                default:
                    throw new ArgumentOutOfRangeException(nameof(force), force, "Unknown Force Layout.");
            }
        }
    }
}
