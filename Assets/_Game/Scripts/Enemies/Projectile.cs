using System.Collections.Generic;
using Dashblade.Combat;
using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// A Turret's shot (F04): flies straight at a constant speed and never turns. Each frame its movement is swept,
    /// and the first thing it touches decides what happens:
    /// a dashing player's hit circle destroys it (it fades and is harmless, and it is not a kill for the dash);
    /// the player's body uses it up at once, costing a heart only if the player is neither protected nor dashing;
    /// a wall removes it at once, with no fade. Enemies and other projectiles do not affect it.
    /// Once the player is defeated it hurts nobody and flies on to a wall.
    /// </summary>
    /// <remarks>
    /// Its collider is a trigger, so it never pushes anything and the dash's hit circle can still find it. It is a
    /// <see cref="DashTarget"/> whose Counts As Kill is off. Runs after <see cref="PlayerDash"/> and
    /// <see cref="PlayerHealth"/>, like the other damage sources.
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(PlayerHealth.ExecutionOrder + 1)]
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(DashTarget))]
    public sealed class Projectile : MonoBehaviour
    {
        private readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();

        private Rigidbody2D body;
        private CircleCollider2D hitbox;
        private DashTarget dashTarget;
        private PlayerLink target;
        private Vector2 direction;
        private float speed;
        private bool launched;
        private int launchFrame = -1;

        /// <summary>Unit vector of flight, fixed at launch.</summary>
        public Vector2 Direction => direction;

        public float Speed => speed;

        /// <summary>Radius in world units (half the projectile width).</summary>
        public float Radius
        {
            get
            {
                Vector3 scale = transform.lossyScale;
                return hitbox.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<CircleCollider2D>();
            dashTarget = GetComponent<DashTarget>();
        }

        /// <summary>
        /// Sends the projectile off from where it stands. Call once, right after instantiating it.
        /// </summary>
        /// <param name="player">The player it can hit.</param>
        /// <param name="flightDirection">Unit vector it flies along, for good.</param>
        /// <param name="flightSpeed">Units per second.</param>
        /// <param name="width">Diameter in world units (the sprite and the collider are 1 unit at scale 1).</param>
        /// <param name="headStart">Seconds it has already flown (the part of the frame after the shot was fired).</param>
        public void Launch(PlayerLink player, Vector2 flightDirection, float flightSpeed, float width, float headStart)
        {
            target = player;
            direction = flightDirection.sqrMagnitude > 0f ? flightDirection.normalized : Vector2.right;
            speed = Mathf.Max(0f, flightSpeed);
            transform.localScale = new Vector3(width, width, 1f);
            launched = true;
            launchFrame = Time.frameCount;

            Step(Mathf.Max(0f, headStart));
        }

        private void Update()
        {
            // The frame of the launch is already covered by the head start.
            if (launched && Time.frameCount != launchFrame)
            {
                Step(Time.deltaTime);
            }
        }

        private void Step(float deltaTime)
        {
            if (dashTarget.IsDead)
            {
                // Destroyed by the dash: it stays where it was hit while it fades.
                return;
            }

            Vector2 from = body.position;
            float radius = Radius;
            float distance = speed * deltaTime;

            float wall = WallQuery.TryFirstContact(from, radius, direction, distance, castHits, out float wallDistance)
                ? wallDistance
                : float.PositiveInfinity;
            float reach = Mathf.Min(distance, wall);

            if (target != null && target.IsDashing
                && SweptCircle.TryFirstContact(from, direction, reach, target.HitCentre, radius + target.HitRadius, out float struck))
            {
                MoveTo(from + direction * struck);
                if (target.TryStrike(dashTarget))
                {
                    return;
                }
            }

            if (target != null && target.CanBeTouched
                && SweptCircle.TryFirstContact(from, direction, reach, target.Centre, radius + target.BodyRadius, out float touched))
            {
                // Used up on the player's body; F03 decides whether it costs a heart (not while protected or dashing).
                MoveTo(from + direction * touched);
                target.TryHit();
                Disappear();
                return;
            }

            if (wall <= distance)
            {
                MoveTo(from + direction * wall);
                Disappear();
                return;
            }

            MoveTo(from + direction * distance);
        }

        private void MoveTo(Vector2 point)
        {
            body.position = point;
            transform.position = new Vector3(point.x, point.y, transform.position.z);
        }

        private void Disappear()
        {
            launched = false;
            hitbox.enabled = false;
            Destroy(gameObject);
        }
    }
}
