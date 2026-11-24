using System.Globalization;

namespace Dashblade.Waves
{
    /// <summary>One enemy of a wave: what it is and where its centre appears.</summary>
    public readonly struct WaveEnemy
    {
        public WaveEnemy(EnemyKind kind, float x, float y)
        {
            Kind = kind;
            X = x;
            Y = y;
        }

        public EnemyKind Kind { get; }

        public float X { get; }

        public float Y { get; }

        public ArenaPoint Position => new ArenaPoint(X, Y);

        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "{0} ({1}, {2})", Kind, X, Y);
    }
}
