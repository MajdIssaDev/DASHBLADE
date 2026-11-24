using System.Globalization;

namespace Dashblade.Waves
{
    /// <summary>
    /// A position in the arena, in world units. A plain value so the wave and layout rules
    /// need no UnityEngine types; the wrapper converts to and from <c>Vector2</c>.
    /// </summary>
    public readonly struct ArenaPoint
    {
        public ArenaPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }

        public float Y { get; }

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0}, {1})", X, Y);
    }
}
