using UnityEngine;

namespace Dashblade.Enemies
{
    /// <summary>
    /// Tuning numbers for the Turret and its projectile (F04). The asset lives at Assets/_Game/Config/TurretConfig.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "TurretConfig", menuName = "Dashblade/Turret Config")]
    public sealed class TurretConfig : ScriptableObject
    {
        [Tooltip("Seconds from the Turret starting (Play Mode start for a placed Turret) to its first ring.")]
        [Min(0f)]
        [SerializeField] private float firstAttackDelay = 1f;

        [Tooltip("Seconds the ring takes to close. The shot is fired the moment it closes.")]
        [Min(0.01f)]
        [SerializeField] private float warningTime = 0.8f;

        [Tooltip("Seconds from a shot to the next ring.")]
        [Min(0f)]
        [SerializeField] private float restTime = 1.5f;

        [Tooltip("Units per second of a projectile. It flies straight and never turns.")]
        [Min(0f)]
        [SerializeField] private float projectileSpeed = 7f;

        [Tooltip("Diameter of a projectile in world units.")]
        [Min(0.01f)]
        [SerializeField] private float projectileWidth = 0.4f;

        [Tooltip("Width of the warning ring in world units when it appears. It shrinks to the Turret's own width.")]
        [Min(0f)]
        [SerializeField] private float ringStartWidth = 3f;

        public float FirstAttackDelay => firstAttackDelay;
        public float WarningTime => warningTime;
        public float RestTime => restTime;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileWidth => projectileWidth;
        public float RingStartWidth => ringStartWidth;
    }
}
