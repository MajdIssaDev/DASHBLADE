using System;
using System.Collections.Generic;
using Dashblade.Combat;
using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// Thin scene wrapper for the Charger (F04). The attack cycle lives in <see cref="ChargerAttackCycle"/> and the
    /// lane's direction in <see cref="ChargerLaneAim"/>; this component reads the player, casts the lane to the wall,
    /// moves the charge, hurts the player on contact while charging, and draws the lane.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Start: a Charger placed in a scene uses its Player field and starts its cycle on its first frame. A spawner
    /// calls <see cref="Initialise"/> right after instantiating, then <see cref="StartCycle"/> when it should begin
    /// (for example after a fade-in); until then it is dormant: not solid, not destroyable, harmless, no warning.
    /// </para>
    /// <para>
    /// The charge is placed exactly, like the dash: its end point is decided by a body cast when the charge starts
    /// (the same cast the lane is drawn with), and each frame the body is put at speed × time along the lane, clamped
    /// to that end, so it neither tunnels nor overshoots at any frame rate. Contact with the player and with the
    /// dash's hit circle is checked along each frame's movement, not only at its end.
    /// </para>
    /// <para>
    /// Runs after <see cref="PlayerDash"/> (-10) and <see cref="PlayerHealth"/> (-5), at the damage sources' order,
    /// so it sees this frame's player position and dash, and its hits use F03's protection timer for this frame.
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(PlayerHealth.ExecutionOrder + 1)]
    [RequireComponent(typeof(EnemyBody))]
    public sealed class Charger : MonoBehaviour
    {
        // Physics tolerances, not tuning values: the same as the dash's (see PlayerDash).
        private const float StartContactDistance = 0.02f;
        private const float WallSkin = 0.01f;

        // Far longer than any arena; a lane that finds no wall in this distance stops here.
        private const float MaxLaneLength = 100f;

        [SerializeField] private ChargerConfig config;

        [Tooltip("The player to attack, for a Charger placed in a scene. A spawner calls Initialise instead.")]
        [SerializeField] private PlayerHealth player;

        [Tooltip("The triangle. It is turned so its tip (the sprite's +Y) points along the lane.")]
        [SerializeField] private Transform tip;

        [SerializeField] private ChargerLaneView lane;

        private readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();

        private EnemyBody enemy;
        private DashTarget dashTarget;
        private PlayerLink target;
        private ChargerAttackCycle cycle;
        private ChargerLaneAim aim;
        private bool initialisedByCode;

        private Vector2 chargeStart;
        private Vector2 chargeDirection;
        private float chargeLength;
        private float chargeTravelled;

        /// <summary>Where the Charger is in its cycle; null before <see cref="StartCycle"/>.</summary>
        public ChargerPhase? Phase => cycle?.Phase;

        /// <summary>The dash target on this Charger (its <see cref="DashTarget.Killed"/> event marks its death).</summary>
        public DashTarget Target => dashTarget;

        /// <summary>The lane's direction (and the tip's), a unit vector.</summary>
        public Vector2 LaneDirection => aim != null ? new Vector2(aim.DirectionX, aim.DirectionY) : TipDirection();

        /// <summary>True while the warning lane is drawn.</summary>
        public bool IsLaneShown => cycle != null && cycle.IsLaneShown;

        private void Awake()
        {
            enemy = GetComponent<EnemyBody>();
            dashTarget = GetComponent<DashTarget>();

            if (config == null || tip == null || lane == null)
            {
                Debug.LogError($"{nameof(Charger)} on '{name}' needs its {nameof(ChargerConfig)}, tip and lane assigned.", this);
                enabled = false;
                return;
            }

            dashTarget.Killed += OnKilled;
        }

        private void Start()
        {
            if (initialisedByCode)
            {
                return;
            }

            if (player == null)
            {
                Debug.LogError($"{nameof(Charger)} on '{name}' has no player assigned.", this);
                return;
            }

            Initialise(player);
            StartCycle();
        }

        private void OnDestroy()
        {
            if (dashTarget != null)
            {
                dashTarget.Killed -= OnKilled;
            }

            if (target != null)
            {
                target.Health.Defeated -= OnPlayerDefeated;
            }
        }

        /// <summary>
        /// Gives a spawned Charger its player. Call right after instantiating; the Charger then waits, dormant, for
        /// <see cref="StartCycle"/>.
        /// </summary>
        public void Initialise(PlayerHealth playerHealth)
        {
            initialisedByCode = true;
            player = playerHealth;
            target = PlayerLink.From(playerHealth, this);
            enemy.Bind(target);
        }

        /// <summary>
        /// The one entry point of the attack cycle: from now the Charger is solid, destroyable and attacking, and its
        /// First Attack Delay starts counting. Does nothing if already started or dead, or without a player.
        /// </summary>
        public void StartCycle()
        {
            if (!enabled || cycle != null || enemy.IsDead)
            {
                return;
            }

            if (target == null)
            {
                Debug.LogError($"{nameof(Charger)} on '{name}' cannot start without a player (Initialise it first).", this);
                return;
            }

            try
            {
                cycle = new ChargerAttackCycle(config.FirstAttackDelay, config.WarningTime, config.LockTime, config.RestTime);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Debug.LogError($"{nameof(Charger)} on '{name}': invalid {nameof(ChargerConfig)} ({exception.Message}).", this);
                return;
            }

            Vector2 facing = TipDirection();
            aim = new ChargerLaneAim(facing.x, facing.y, enemy.Radius + target.BodyRadius);

            cycle.PhaseChanged += OnPhaseChanged;
            target.Health.Defeated += OnPlayerDefeated;

            enemy.SetSolidWanted(true);
            enemy.SetStarted(true);

            if (target.IsDefeated)
            {
                cycle.OnPlayerDefeated();
            }
        }

        private void Update()
        {
            if (cycle == null || cycle.IsDestroyed)
            {
                return;
            }

            cycle.Tick(Time.deltaTime);

            if (cycle.Phase == ChargerPhase.Charging)
            {
                StepCharge();
            }
            else if (cycle.IsLaneShown)
            {
                TrackPlayer();
            }

            ShowLane();
        }

        private void OnPhaseChanged(ChargerPhase phase)
        {
            switch (phase)
            {
                case ChargerPhase.Warning:
                    // A new strip always starts pointing at the player (unless the player is on top of the Charger).
                    TrackPlayer();
                    break;

                case ChargerPhase.Charging:
                    BeginCharge();
                    break;

                case ChargerPhase.Resting:
                case ChargerPhase.Idle:
                    enemy.SetSolidWanted(true);
                    break;
            }

            ShowLane();
        }

        private void TrackPlayer()
        {
            if (!cycle.IsLaneTracking)
            {
                return;
            }

            Vector2 centre = enemy.Centre;
            Vector2 playerCentre = target.Centre;
            aim.Update(centre.x, centre.y, playerCentre.x, playerCentre.y, isTracking: true);
            ShowTip();
        }

        private void BeginCharge()
        {
            chargeStart = enemy.Centre;
            chargeDirection = LaneDirection;
            chargeLength = LaneLength(chargeStart, chargeDirection);
            chargeTravelled = 0f;

            // A charging Charger never blocks or pushes the player.
            enemy.SetSolidWanted(false);

            if (chargeLength <= 0f)
            {
                // Already against the wall it points at: the rest begins at once (this runs inside Tick, so the
                // leftover of this step goes to the rest).
                cycle.ReportChargeEnded();
            }
        }

        private void StepCharge()
        {
            float speed = config.ChargeSpeed;
            float goal = speed > 0f ? speed * cycle.TimeInPhase : chargeLength;
            float next = Mathf.Min(goal, chargeLength);
            float step = Mathf.Max(0f, next - chargeTravelled);
            Vector2 from = PointOnCharge(chargeTravelled);

            // The dash's hit circle comes first: it is wider than the player's body, and a dashing player takes no damage.
            if (target.IsDashing
                && SweptCircle.TryFirstContact(from, chargeDirection, step, target.HitCentre, enemy.Radius + target.HitRadius, out float struck))
            {
                chargeTravelled += struck;
                enemy.MoveTo(PointOnCharge(chargeTravelled));
                if (target.TryStrike(dashTarget))
                {
                    // Killed during the charge: it stops here and is harmless (OnKilled has run).
                    return;
                }
            }

            if (target.CanBeTouched
                && SweptCircle.TryFirstContact(from, chargeDirection, step, target.Centre, enemy.Radius + target.BodyRadius, out _))
            {
                // F03 decides: one heart unless dashing, protected or defeated (one pass costs at most one heart).
                target.TryHit();
            }

            chargeTravelled = next;
            enemy.MoveTo(PointOnCharge(chargeTravelled));

            if (goal >= chargeLength)
            {
                float timePastArrival = speed > 0f ? cycle.TimeInPhase - chargeLength / speed : 0f;
                cycle.ReportChargeEnded(timePastArrival);
            }
        }

        private Vector2 PointOnCharge(float travelled)
        {
            return travelled >= chargeLength ? chargeStart + chargeDirection * chargeLength : chargeStart + chargeDirection * travelled;
        }

        /// <summary>How far the Charger's centre can travel along <paramref name="direction"/> before its body touches a wall.</summary>
        private float LaneLength(Vector2 origin, Vector2 direction)
        {
            return WallQuery.StopDistance(origin, enemy.Radius, direction, MaxLaneLength, StartContactDistance, WallSkin, castHits);
        }

        private void ShowLane()
        {
            if (cycle == null || !cycle.IsLaneShown)
            {
                lane.Hide();
                return;
            }

            Vector2 centre = enemy.Centre;
            Vector2 direction = LaneDirection;

            // To the wall: the body stops its radius short of it.
            float length = LaneLength(centre, direction) + enemy.Radius;
            lane.Show(centre, direction, length, enemy.Radius * 2f, cycle.WarningProgress);
            ShowTip();
        }

        private void ShowTip()
        {
            if (aim != null)
            {
                tip.rotation = Quaternion.Euler(0f, 0f, aim.AngleDegrees - 90f);
            }
        }

        private Vector2 TipDirection()
        {
            return tip != null ? (Vector2)tip.up : Vector2.up;
        }

        private void OnKilled(DashTarget killed)
        {
            cycle?.Destroy();
            lane.Hide();
        }

        private void OnPlayerDefeated()
        {
            cycle?.OnPlayerDefeated();
            ShowLane();
        }
    }
}
