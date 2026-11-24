using System;
using Dashblade.Combat;
using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// Thin scene wrapper for the Turret (F04). The attack cycle lives in <see cref="TurretAttackCycle"/>; this
    /// component draws the closing ring and, the moment it closes, creates one <see cref="Projectile"/> at its centre
    /// aimed at the player's centre. It never moves or rotates.
    /// </summary>
    /// <remarks>
    /// Start: a Turret placed in a scene uses its Player field and starts its cycle on its first frame. A spawner
    /// calls <see cref="Initialise"/> right after instantiating, then <see cref="StartCycle"/> when it should begin;
    /// until then it is dormant: not solid, not destroyable, harmless, no ring. Runs after <see cref="PlayerDash"/>
    /// and <see cref="PlayerHealth"/>, so a shot aims at this frame's player position.
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(PlayerHealth.ExecutionOrder + 1)]
    [RequireComponent(typeof(EnemyBody))]
    public sealed class Turret : MonoBehaviour
    {
        [SerializeField] private TurretConfig config;

        [Tooltip("The player to attack, for a Turret placed in a scene. A spawner calls Initialise instead.")]
        [SerializeField] private PlayerHealth player;

        [Tooltip("Created at run time, one per shot.")]
        [SerializeField] private Projectile projectilePrefab;

        [SerializeField] private TurretRingView ring;

        private EnemyBody enemy;
        private DashTarget dashTarget;
        private PlayerLink target;
        private TurretAttackCycle cycle;
        private bool initialisedByCode;

        /// <summary>Where the Turret is in its cycle; null before <see cref="StartCycle"/>.</summary>
        public TurretPhase? Phase => cycle?.Phase;

        /// <summary>The dash target on this Turret (its <see cref="DashTarget.Killed"/> event marks its death).</summary>
        public DashTarget Target => dashTarget;

        /// <summary>The ring's outer width now, in units; 0 when no ring is shown.</summary>
        public float RingWidth => cycle != null ? cycle.RingWidth : 0f;

        /// <summary>Raised with each projectile the moment it is fired.</summary>
        public event Action<Projectile> Fired;

        private void Awake()
        {
            enemy = GetComponent<EnemyBody>();
            dashTarget = GetComponent<DashTarget>();

            if (config == null || projectilePrefab == null || ring == null)
            {
                Debug.LogError($"{nameof(Turret)} on '{name}' needs its {nameof(TurretConfig)}, projectile prefab and ring assigned.", this);
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
                Debug.LogError($"{nameof(Turret)} on '{name}' has no player assigned.", this);
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
        /// Gives a spawned Turret its player. Call right after instantiating; the Turret then waits, dormant, for
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
        /// The one entry point of the attack cycle: from now the Turret is solid, destroyable and attacking, and its
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
                Debug.LogError($"{nameof(Turret)} on '{name}' cannot start without a player (Initialise it first).", this);
                return;
            }

            try
            {
                // The ring closes to the Turret's own width (its body circle).
                cycle = new TurretAttackCycle(
                    config.FirstAttackDelay, config.WarningTime, config.RestTime, config.RingStartWidth, enemy.Radius * 2f);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Debug.LogError($"{nameof(Turret)} on '{name}': invalid {nameof(TurretConfig)} ({exception.Message}).", this);
                return;
            }

            cycle.ShotFired += Fire;
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

            // Shots are created by the ShotFired handler, inside Tick.
            cycle.Tick(Time.deltaTime);
            ShowRing();
        }

        private void Fire()
        {
            Vector2 origin = enemy.Centre;
            Vector2 toPlayer = target.Centre - origin;
            Vector2 direction = toPlayer.sqrMagnitude > 1e-12f ? toPlayer.normalized : Vector2.right;

            Projectile shot = Instantiate(projectilePrefab, new Vector3(origin.x, origin.y, transform.position.z), Quaternion.identity);

            // The ring closed TimeInPhase ago (part of this frame's step): the shot has already flown that long.
            shot.Launch(target, direction, config.ProjectileSpeed, config.ProjectileWidth, cycle.TimeInPhase);
            Fired?.Invoke(shot);
        }

        private void ShowRing()
        {
            if (cycle != null && cycle.IsRingShown)
            {
                ring.Show(cycle.RingWidth);
            }
            else
            {
                ring.Hide();
            }
        }

        private void OnKilled(DashTarget killed)
        {
            cycle?.Destroy();
            ring.Hide();
        }

        private void OnPlayerDefeated()
        {
            cycle?.OnPlayerDefeated();
            ShowRing();
        }
    }
}
