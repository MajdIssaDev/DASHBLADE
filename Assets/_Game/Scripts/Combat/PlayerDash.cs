using System;
using System.Collections.Generic;
using Dashblade.Common;
using Dashblade.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dashblade.Combat
{
    /// <summary>
    /// Thin scene wrapper for the dash attack. A left-mouse press while ready decides the straight path at once: the
    /// body is cast against walls to find where it stops. The player then moves along that path at constant speed and
    /// stops exactly on the end point. Each frame the wider hit circle is swept from where it was to where it is now,
    /// and every <see cref="DashTarget"/> it touches is destroyed, so targets that moved onto the path after the click
    /// are caught too. Things that move themselves (a charging Charger, a projectile) also check their own movement
    /// against the hit circle and call <see cref="TryStrike"/>. Collision callbacks are not used, so nothing can be
    /// skipped at any speed or frame rate. The rules live in <see cref="DashRules"/>. For the config's Start Lockout
    /// after the first frame the run is shown, a left-click is dropped (<see cref="DashStartLockout"/>, timed by a
    /// <see cref="RunClock"/>); the dash still counts as ready meanwhile.
    /// </summary>
    /// <remarks>
    /// During the dash the Rigidbody2D is Kinematic and is placed on the path each frame: a kinematic body gets no
    /// contacts with static or kinematic colliders, so living dummies and enemies (solid to walking) do not stop it
    /// and nothing pushes it off the path. Walking and aim are locked meanwhile. Runs before
    /// <see cref="PlayerMovement"/> and <see cref="PlayerAim"/>, so on the frame the dash ends both run unlocked in
    /// the same frame, and before enemies and projectiles, which see this frame's hit circle.
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerAim))]
    public sealed class PlayerDash : MonoBehaviour
    {
        // Physics tolerances, not gameplay tuning values. Wall hits this close to the start are surfaces the body
        // already touches (about twice Physics2D's default contact offset of 0.01). The cast reports a hit about
        // 0.005 inside the wall; stopping 0.01 short leaves the body just touching it, where walking into it rests.
        private const float StartContactDistance = 0.02f;
        private const float WallSkin = 0.01f;

        [SerializeField] private DashConfig config;

        [Tooltip("On from the frame a dash starts to the frame it ends. Shown for checking; set by the dash, not editable.")]
        [ReadOnlyInInspector]
        [SerializeField] private bool invulnerable;

        private readonly DashRules rules = new DashRules();
        private readonly RunClock runClock = new RunClock();
        private readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();
        private readonly List<Collider2D> overlapHits = new List<Collider2D>();

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private PlayerMovement movement;
        private PlayerAim aim;
        private DashStartLockout startLockout;
        private DashPath path;
        private Vector2 lastSweepPoint;

        public DashState State => rules.State;

        /// <summary>True when a left-click would start a dash.</summary>
        public bool IsReady => rules.IsReady;

        /// <summary>True from the frame a dash starts to the frame it ends (F03 reads this).</summary>
        public bool IsInvulnerable => rules.IsInvulnerable;

        /// <summary>True from the frame a dash starts to the frame it ends: the hit circle is live.</summary>
        public bool IsDashing => rules.State == DashState.Dashing;

        /// <summary>
        /// How far the dash has recharged, 0 to 1: 1 while ready, 0 during the dash, rising over the miss cooldown to
        /// exactly 1 on the frame <see cref="IsReady"/> turns true (see <see cref="DashRules.RechargeProgress"/>).
        /// Read-only; the HUD's dash bar reads it.
        /// </summary>
        public float RechargeProgress => rules.RechargeProgress;

        /// <summary>Centre of the hit circle this frame (the body's centre).</summary>
        public Vector2 HitCentre => body != null ? body.position : (Vector2)transform.position;

        /// <summary>Radius of the hit circle (half the config's Hit Width).</summary>
        public float HitRadius => config != null ? config.HitRadius : 0f;

        /// <summary>
        /// While true (the F11 pause), Update does nothing at all: a left-click is neither acted on nor remembered, and
        /// a dash in progress stays where it is (still Kinematic, still invulnerable). When it turns false the dash
        /// carries on along the same path. Unlike switching this component off, it never cancels a dash. Separate
        /// from the walking and aim locks the dash itself sets.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>Raised with the new value whenever <see cref="IsReady"/> changes.</summary>
        public event Action<bool> ReadinessChanged
        {
            add => rules.ReadinessChanged += value;
            remove => rules.ReadinessChanged -= value;
        }

        /// <summary>
        /// Starts the Start Lockout again from now, counted in game time (F11: after the pause panel's Resume button).
        /// Left-clicks are dropped meanwhile; readiness, walking and aim are not affected.
        /// </summary>
        public void RestartStartLockout()
        {
            startLockout?.Restart();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<CircleCollider2D>();
            movement = GetComponent<PlayerMovement>();
            aim = GetComponent<PlayerAim>();

            if (config == null)
            {
                Debug.LogError($"{nameof(PlayerDash)} on '{name}' has no {nameof(DashConfig)} assigned.", this);
                enabled = false;
                return;
            }

            startLockout = new DashStartLockout(config.StartLockout);
        }

        private void Update()
        {
            if (IsPaused)
            {
                // Game time does not move while paused, so skipping the clocks loses nothing.
                return;
            }

            Mouse mouse = Mouse.current;
            bool clicked = mouse != null && mouse.leftButton.wasPressedThisFrame;
            bool wasDashing = rules.State == DashState.Dashing;

            // The Start Lockout counts from the first frame the run is shown, as ArenaRun's first Wave Pause does.
            startLockout.Tick(runClock.Step(Time.timeAsDouble));
            rules.Tick(Time.deltaTime);

            if (wasDashing)
            {
                // A click during the dash is ignored and not remembered, even if this frame ends the dash.
                StepDash();
            }
            else if (clicked && rules.IsReady && !startLockout.IsLocked)
            {
                // A click during the Start Lockout is dropped, not remembered: only a new press after it dashes.
                BeginDash();
            }

            invulnerable = rules.IsInvulnerable;
        }

        private void OnDisable()
        {
            // Turned off mid-dash (F03 turns the dash off on defeat): stop where the body is and hand it back to
            // physics and walking, so nothing is left Kinematic or locked.
            if (rules.State == DashState.Dashing)
            {
                CancelDash();
            }
        }

        private void BeginDash()
        {
            // Use this frame's cursor: the facing at the moment of the click.
            aim.RefreshFacing();
            Vector2 direction = aim.AimDirection;
            Vector2 start = body.position;

            float length = FindWallStop(start, direction, config.Distance);
            path = new DashPath(start, direction, length);
            lastSweepPoint = path.Start;

            rules.TryStartDash(path.Length, DashRules.DashTimeFor(path.Length, config.Distance, config.Duration));

            movement.IsLocked = true;
            aim.IsLocked = true;
            body.linearVelocity = Vector2.zero;
            body.bodyType = RigidbodyType2D.Kinematic;

            // Frame zero of the dash: targets touching the hit area at the start die now; a zero-length dash ends now.
            StepDash();
        }

        private void StepDash()
        {
            bool complete = rules.IsDashComplete;
            float travelled = complete ? path.Length : rules.Travelled;
            Vector2 point = path.PointAt(travelled);
            MoveBodyTo(point);

            SweepHitArea(lastSweepPoint, point, config.HitRadius);
            lastSweepPoint = point;

            if (complete)
            {
                EndDash();
            }
        }

        /// <summary>
        /// Destroys <paramref name="target"/> because something moving brought it into the hit circle during this
        /// dash. Only works while dashing; a kill counts only if the target <see cref="DashTarget.CountsAsKill"/>.
        /// Returns true if the target was destroyed. Call from Update (this component runs first).
        /// </summary>
        public bool TryStrike(DashTarget target)
        {
            if (!IsDashing || target == null || !target.Kill())
            {
                return false;
            }

            if (target.CountsAsKill)
            {
                rules.RegisterKill();
            }

            return true;
        }

        private void EndDash()
        {
            ReleaseBody();

            // Last, so readiness listeners see the player already back on foot.
            rules.EndDash(config.MissCooldown);
        }

        private void CancelDash()
        {
            ReleaseBody();
            rules.CancelDash(config.MissCooldown);
            invulnerable = rules.IsInvulnerable;
        }

        private void ReleaseBody()
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.linearVelocity = Vector2.zero;
            movement.IsLocked = false;
            aim.IsLocked = false;
        }

        private void MoveBodyTo(Vector2 point)
        {
            // Both, so physics queries and the Transform agree on this very frame.
            body.position = point;
            transform.position = new Vector3(point.x, point.y, transform.position.z);
        }

        /// <summary>
        /// Distance the 1-unit body can travel before it first touches a wall. What counts as a wall is
        /// <see cref="WallQuery.IsWall"/>: anything solid that is not a <see cref="DashTarget"/> or the player (the
        /// four walls and the door marker; enemies, projectiles and dummies never stop the dash).
        /// </summary>
        private float FindWallStop(Vector2 start, Vector2 direction, float maxDistance)
        {
            ContactFilter2D filter = ContactFilter2D.noFilter;
            filter.useTriggers = false;

            float stop = maxDistance;
            int count = bodyCollider.Cast(direction, filter, castHits, maxDistance, true);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = castHits[i];
                Collider2D other = hit.collider;
                if (!WallQuery.IsWall(other))
                {
                    continue;
                }

                // For a surface touched at the start: does the dash head into it, or along or away from it?
                Vector2 towardSurface = other.ClosestPoint(start) - start;
                if (DashWallRules.TryGetStop(hit.distance, StartContactDistance, WallSkin, direction, towardSurface, out float wallStop)
                    && wallStop < stop)
                {
                    stop = wallStop;
                }
            }

            castHits.Clear();
            return Mathf.Max(0f, stop);
        }

        /// <summary>
        /// Destroys every living target the hit circle touches anywhere from <paramref name="from"/> to
        /// <paramref name="to"/>, both ends included, where the targets are now. Triggers count (projectiles are
        /// triggers).
        /// </summary>
        private void SweepHitArea(Vector2 from, Vector2 to, float hitRadius)
        {
            ContactFilter2D filter = ContactFilter2D.noFilter;
            filter.useTriggers = true;

            int overlaps = Physics2D.OverlapCircle(from, hitRadius, filter, overlapHits);
            for (int i = 0; i < overlaps; i++)
            {
                StrikeCollider(overlapHits[i]);
            }

            overlapHits.Clear();

            Vector2 step = to - from;
            float length = step.magnitude;
            if (length > 0f)
            {
                int hits = Physics2D.CircleCast(from, hitRadius, step / length, filter, castHits, length);
                for (int i = 0; i < hits; i++)
                {
                    StrikeCollider(castHits[i].collider);
                }

                castHits.Clear();
            }
        }

        private void StrikeCollider(Collider2D other)
        {
            if (other != null && other.TryGetComponent(out DashTarget target))
            {
                TryStrike(target);
            }
        }
    }
}
