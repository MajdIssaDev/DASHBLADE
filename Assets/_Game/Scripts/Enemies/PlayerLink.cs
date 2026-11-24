using Dashblade.Combat;
using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// What an enemy or projectile needs to know about the player, read from its components: where its centre is,
    /// how big its body is, whether it is dashing (and where the dash's hit circle is), and the F03 damage path.
    /// Built from the player's <see cref="PlayerHealth"/>; the other components are on the same GameObject.
    /// </summary>
    public sealed class PlayerLink
    {
        private readonly Rigidbody2D body;
        private readonly CircleCollider2D bodyCollider;

        private PlayerLink(PlayerHealth health, PlayerDash dash, Rigidbody2D body, CircleCollider2D bodyCollider)
        {
            Health = health;
            Dash = dash;
            this.body = body;
            this.bodyCollider = bodyCollider;
        }

        public PlayerHealth Health { get; }

        public PlayerDash Dash { get; }

        /// <summary>The player's body collider (a circle).</summary>
        public Collider2D Collider => bodyCollider;

        /// <summary>The player's centre this frame.</summary>
        public Vector2 Centre => body.position;

        /// <summary>Radius of the player's body circle in world units.</summary>
        public float BodyRadius
        {
            get
            {
                Vector3 scale = bodyCollider.transform.lossyScale;
                return bodyCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            }
        }

        public bool IsDefeated => Health.IsDefeated;

        /// <summary>False once the player is defeated (its collision is off): nothing can touch it any more.</summary>
        public bool CanBeTouched => !Health.IsDefeated && bodyCollider.enabled;

        /// <summary>True while a dash is running, so its hit circle destroys what it touches.</summary>
        public bool IsDashing => Dash.enabled && Dash.IsDashing;

        public Vector2 HitCentre => Dash.HitCentre;

        public float HitRadius => Dash.HitRadius;

        /// <summary>
        /// Builds the link, or returns null (and logs why) if the player is missing a component it needs.
        /// </summary>
        public static PlayerLink From(PlayerHealth health, Object context)
        {
            if (health == null)
            {
                return null;
            }

            if (!health.TryGetComponent(out PlayerDash dash)
                || !health.TryGetComponent(out Rigidbody2D body)
                || !health.TryGetComponent(out CircleCollider2D bodyCollider))
            {
                Debug.LogError(
                    $"'{health.name}' needs {nameof(PlayerDash)}, {nameof(Rigidbody2D)} and {nameof(CircleCollider2D)} to be attacked.",
                    context);
                return null;
            }

            return new PlayerLink(health, dash, body, bodyCollider);
        }

        /// <summary>F03's damage path: costs 1 heart unless the player is dashing, protected or defeated.</summary>
        public bool TryHit() => Health.TryHit();

        /// <summary>The dash destroys <paramref name="target"/> if it is running (see <see cref="PlayerDash.TryStrike"/>).</summary>
        public bool TryStrike(DashTarget target) => Dash.TryStrike(target);
    }
}
