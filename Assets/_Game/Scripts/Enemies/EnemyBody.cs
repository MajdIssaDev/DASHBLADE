using Dashblade.Combat;
using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// The body shared by the Charger and the Turret: a kinematic 1-unit circle that is also a <see cref="DashTarget"/>.
    /// It switches three things together. While not <see cref="IsStarted"/> (before its owner starts its attack cycle,
    /// for example while F05 fades it in) it is not solid, not destroyable and harmless. While active it blocks the
    /// walking player like a wall and cannot be pushed, except while its owner says it should not
    /// (<see cref="SetSolidWanted"/>, a charging Charger) and except while it overlaps the player, until the two have
    /// fully separated (<see cref="EnemySolidity"/>). The dash passes through it either way: a dashing player is
    /// kinematic, and kinematic bodies get no contacts with each other.
    /// </summary>
    /// <remarks>
    /// "Not blocking" is a trigger collider: the physics solver then never pushes the player, while the dash's hit
    /// circle (which includes triggers) still finds the body. Re-checked before every physics step, and at once
    /// whenever the owner's state changes.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(DashTarget))]
    public sealed class EnemyBody : MonoBehaviour
    {
        // More than the solver's resting penetration (Box2D's linear slop is 0.005 units), well under any real overlap.
        private const float OverlapTolerance = 0.02f;

        private readonly EnemySolidity solidity = new EnemySolidity();
        private Rigidbody2D body;
        private CircleCollider2D hitbox;
        private DashTarget target;
        private PlayerLink player;
        private bool solidWanted = true;

        /// <summary>The dash target on this body. F05 can count deaths through its <see cref="DashTarget.Killed"/> event.</summary>
        public DashTarget Target => target;

        /// <summary>True once the owner has started: solid (when wanted), destroyable, and able to attack.</summary>
        public bool IsStarted { get; private set; }

        /// <summary>True while the body blocks the walking player.</summary>
        public bool IsBlocking => solidity.IsBlocking;

        public bool IsDead => target.IsDead;

        /// <summary>The body's centre this frame.</summary>
        public Vector2 Centre => body.position;

        /// <summary>Radius of the body circle in world units.</summary>
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
            target = GetComponent<DashTarget>();

            // Dormant until the owner starts it.
            target.IsDestroyable = false;
            ApplyBlocking();
        }

        /// <summary>Tells the body which player it must block and check for overlaps.</summary>
        public void Bind(PlayerLink link)
        {
            player = link;
            Refresh();
        }

        /// <summary>
        /// Switches solid, destroyable and able-to-attack together. Becoming active never shoves an overlapping player:
        /// the body blocks only once the two are apart.
        /// </summary>
        public void SetStarted(bool started)
        {
            IsStarted = started;
            target.IsDestroyable = started;
            Refresh();
        }

        /// <summary>False while the owner must not block or push the player (a charging Charger).</summary>
        public void SetSolidWanted(bool wanted)
        {
            solidWanted = wanted;
            Refresh();
        }

        /// <summary>Moves the body to <paramref name="point"/> at once, for physics queries and the Transform alike.</summary>
        public void MoveTo(Vector2 point)
        {
            body.position = point;
            transform.position = new Vector3(point.x, point.y, transform.position.z);
        }

        private void FixedUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (target == null || target.IsDead)
            {
                return;
            }

            solidity.Update(IsStarted && solidWanted, OverlapDepth(), OverlapTolerance);
            ApplyBlocking();
        }

        private float OverlapDepth()
        {
            if (player == null || !player.CanBeTouched || !hitbox.enabled)
            {
                return 0f;
            }

            ColliderDistance2D distance = hitbox.Distance(player.Collider);
            return distance.isValid ? -distance.distance : 0f;
        }

        private void ApplyBlocking()
        {
            bool trigger = !solidity.IsBlocking;
            if (hitbox.isTrigger != trigger)
            {
                hitbox.isTrigger = trigger;
            }
        }
    }
}
